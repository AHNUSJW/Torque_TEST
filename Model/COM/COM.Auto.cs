using DBHelper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

//Lumi 20240524
//Ricardo 20240820

namespace Model
{
    public class TaskManager
    {
        #region 定义字段

        //用于跟踪ProcessCommands线程的Task对象
        private Task processCommandsTask = null;
        //用于暂停ProcessCommands线程
        private ManualResetEvent pauseEvent = new ManualResetEvent(true);
        //用于通知ProcessCommands线程设备已回复
        private AutoResetEvent receiveSignal = new AutoResetEvent(false);
        //用于保护currentCommand字段
        private readonly object lockCmdObject = new object();

        //用户指令队列
        private ConcurrentQueue<UserCommand> userCommandQueue = new ConcurrentQueue<UserCommand>();
        //自动机队列
        private ConcurrentQueue<AutoCommand> autoCommandQueue = new ConcurrentQueue<AutoCommand>();
        //当前发送待回复指令
        private Command currentCommand = null;

        //重发计数器
        private Dictionary<Command, int> timeoutCounts = new Dictionary<Command, int>();
        //重发次数
        private int retry = 5;
        //超时时间（毫秒）
        private int timeout = 500;
        //最小发送时间间隔（毫秒）
        private int minSendInterval = 5;

        //设备列表(CONNECTED/WORKING)
        private ConcurrentBag<int> validDevices = new ConcurrentBag<int>();
        //选择的设备 0：未选择特定设备 1-255：选择的设备index
        private int selectedDev = 0;

        //自动机模式
        private AutoMode mode = AutoMode.UserOnly;

        //当前工单点位拧紧结果
        private bool isDataValid = false;

        //定时唤醒指令(4分钟)
        private System.Timers.Timer wakeTimer = new System.Timers.Timer(4 * 60 * 1000);

        //连接的设备，递增（用于TCP连接）
        private int connectID = 0;
        //自动连接的取消令牌
        private CancellationTokenSource autoConnectCts = new CancellationTokenSource();

        #endregion

        #region 定义属性
        public Command CurrentCommand
        {
            get
            {
                lock (lockCmdObject)
                {
                    return currentCommand;
                }
            }
            set
            {
                lock (lockCmdObject)
                {
                    currentCommand = value;
                }
            }
        }

        public int SelectedDev
        {
            get
            {
                return selectedDev;
            }
            set
            {
                selectedDev = value;
            }
        }
        public AutoMode Mode { get => mode; set => mode = value; }
        public bool IsDataValid { get => isDataValid; set => isDataValid = value; }

        public TaskManager()
        {
            // 初始化定时器
            wakeTimer.Elapsed += new ElapsedEventHandler(TimedSend);
            wakeTimer.AutoReset = true;
            wakeTimer.Enabled = true;
        }

        #endregion

        //启动任务管理器
        public void Start()
        {
            // 如果ProcessCommands线程已经启动，那么就不再启动新的线程
            if (processCommandsTask != null && processCommandsTask.Status == TaskStatus.Running)
            {
                return;
            }

            //委托
            MyDevice.myUpdate += new freshHandler(receiveData);

            //启动指令处理线程
            processCommandsTask = Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        ProcessCommands();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred: {ex.Message}");
                    }
                }
            });
        }

        //暂停任务管理器
        public void Pause()
        {
            MyDevice.myUpdate -= new freshHandler(receiveData);
            pauseEvent.Reset();
            autoCommandQueue = new ConcurrentQueue<AutoCommand>();
        }

        //恢复任务管理器
        public void Resume()
        {
            for (int i = 1; i < 256; i++)
            {
                MyDevice.mDev[i].auto = new AUTO();
            }
            pauseEvent.Set();
            MyDevice.myUpdate += new freshHandler(receiveData);
        }

        #region 主线程

        //主线程
        private void ProcessCommands()
        {
            while (true)
            {
                pauseEvent.WaitOne();
                if (MyDevice.devSum > 0)
                {
                    //取队头指令
                    Command command = null;
                    //优先处理用户指令
                    if (userCommandQueue.TryPeek(out UserCommand userCommand))
                    {
                        command = userCommand;
                    }
                    //处理auto指令
                    else
                    {
                        //auto队列中没有指令，依据扳手状态新添加指令
                        if (autoCommandQueue.IsEmpty)
                        {
                            //实时数据自动机
                            if (mode == AutoMode.UserAndActualData)
                            {
                                EnqueueAutoCommand(mode, CreateDataAutoCommand);
                            }
                            //工单数据自动机
                            else if (mode == AutoMode.UserAndTicketWork)
                            {
                                EnqueueAutoCommand(mode, CreateTicketAutoCommand);
                            }
                        }
                        //auto队列中有指令
                        if (autoCommandQueue.TryPeek(out AutoCommand autoCommand))
                        {
                            command = autoCommand;
                        }
                    }

                    //发送指令
                    if (command != null)
                    {
                        //判断设备在线状态

                        // 发送指令
                        CurrentCommand = command;
                        DateTime startTime = DateTime.Now;
                        SendCommand(CurrentCommand);
                        Console.WriteLine("发送"+ MyDevice.protocol.trTASK + DateTime.Now.ToString("HH:mm:ss:fff"));
                        // 等待设备回复或超时
                        if (receiveSignal.WaitOne(TimeSpan.FromMilliseconds(timeout)))
                        {
                            Console.WriteLine("进入自动机线程处理" + MyDevice.protocol.trTASK + DateTime.Now.ToString("HH:mm:ss:fff"));
                            //回复后处理
                            HandleDeviceResponse();

                            //防止发太快
                            TimeSpan waitTime = DateTime.Now - startTime;
                            if (waitTime.TotalMilliseconds < minSendInterval)
                            {
                                int remainingTime = minSendInterval - (int)waitTime.TotalMilliseconds;
                                Task.Delay(remainingTime).Wait();
                            }
                        }
                        else
                        {
                            MyDevice.protocol.Protocol_ClearState();
                            //超时重发
                            HandleTimeout();
                        }
                    }
                    else
                    {
                        //没有指令可用，稍微暂停一下，避免CPU占用过高
                        Task.Delay(300).Wait();
                    }
                }
                else
                {
                    //没有设备可用，稍微暂停一下，避免CPU占用过高
                    Task.Delay(300).Wait();
                }
            }
        }

        //接收后的处理
        private void HandleDeviceResponse()
        {
            // Console.WriteLine("Receive:" + CurrentCommand.ProtocolFunc + " " + CurrentCommand.TaskState + " " + CurrentCommand.WrenchId);
            if (CurrentCommand.GetType().Name == "UserCommand")
            {
                //触发UI更新事件
                TriggerUpdateUI(CurrentCommand);
            }
            else
            {
                //实时数据
                if (mode == AutoMode.UserAndActualData)
                {
                    //Console.WriteLine(MyDevice.protocol.addr + "========" + CurrentCommand.TaskState);
                    switch (CurrentCommand.TaskState)
                    {
                        case TASKS.REG_BLOCK1_FIFO:
                            MyDevice.actDev.auto.fifoIndex = MyDevice.actDev.fifo.read;
                            MyDevice.actDev.auto.fifoCount = MyDevice.actDev.fifo.count;

                            if (MyDevice.actDev.devc.type == TYPE.TQ_XH_XL01_07 - 1280 || MyDevice.actDev.devc.type == TYPE.TQ_XH_XL01_09 - 1280)
                            {
                                if (MyDevice.actDev.auto.fifoCount != 0)
                                {
                                    MyDevice.actDev.auto.nextTask = TASKS.WRITE_FIFO_INDEX;
                                }
                                else
                                {
                                    MyDevice.actDev.auto.nextTask = TASKS.REG_BLOCK1_FIFO;
                                }
                            }
                            else if (MyDevice.actDev.devc.type == TYPE.TQ_XH_XL01_06 - 1280 || MyDevice.actDev.devc.type == TYPE.TQ_XH_XL01_05 - 1280)
                            {
                                MyDevice.actDev.auto.nextTask = TASKS.WRITE_FIFO_INDEX;
                            }
                            else if (MyDevice.actDev.devc.type == TYPE.TQ_XH_XL01_08 - (UInt16)ADDROFFSET.TQ_XH_ADDR)
                            {
                                if (MyDevice.actDev.auto.fifoCount != 0)
                                {
                                    MyDevice.actDev.auto.nextTask = TASKS.WRITE_FIFO_INDEX;
                                }
                                else
                                {
                                    MyDevice.actDev.auto.nextTask = TASKS.REG_BLOCK1_FIFO;
                                }
                            }

                            break;
                        case TASKS.WRITE_FIFO_INDEX:
                            MyDevice.actDev.auto.nextTask = TASKS.REG_BLOCK2_DAT;
                            break;
                        case TASKS.REG_BLOCK2_DAT:
                            MyDevice.actDev.auto.dataTick++;
                            //Console.WriteLine(MyDevice.actDev.wlan.addr + ": " + MyDevice.actDev.fifo.index + "---正常-----" + MyDevice.actDev.auto.fifoIndex);

                            if (MyDevice.actDev.devc.type == TYPE.TQ_XH_XL01_06 - (UInt16)ADDROFFSET.TQ_XH_ADDR || MyDevice.actDev.devc.type == TYPE.TQ_XH_XL01_05 - (UInt16)ADDROFFSET.TQ_XH_ADDR)
                            {
                                //触发UI更新事件
                                //TriggerUpdateUI(CurrentCommand);

                                //读完了清缓存
                                if (MyDevice.actDev.auto.dataTick * 5 >= MyDevice.actDev.auto.fifoCount && MyDevice.actDev.auto.fifoCount > 0)
                                {
                                    MyDevice.actDev.auto.nextTask = TASKS.WRITE_FIFOCLEAR;
                                    MyDevice.actDev.auto.fifoCount = 0;//清完之后，扳手状态是读一条dat自动清一条，count永久是0
                                }
                                else
                                {
                                    MyDevice.actDev.auto.nextTask = TASKS.REG_BLOCK2_DAT;
                                }
                            }
                            else
                            {
                                if (MyDevice.actDev.fifo.index == MyDevice.actDev.auto.fifoIndex)
                                {
                                    if (MyDevice.actDev.auto.fifoCount > 0)
                                    {
                                        //触发UI更新事件
                                        //TriggerUpdateUI(CurrentCommand);
                                    }

                                    if ((int)MyDevice.actDev.auto.fifoCount - 6000 * 5 > 0)
                                    {
                                        //读6000次dat清一次
                                        if (MyDevice.actDev.auto.dataTick < 6000)
                                        {
                                            MyDevice.actDev.auto.nextTask = TASKS.REG_BLOCK2_DAT;
                                            MyDevice.actDev.auto.fifoIndex += 5 * 28;
                                        }
                                        else
                                        {
                                            MyDevice.actDev.auto.nextTask = TASKS.WRITE_FIFOCLEAR;
                                        }
                                    }
                                    else
                                    {
                                        if ((int)MyDevice.actDev.auto.fifoCount / 5 > 1)
                                        {
                                            MyDevice.actDev.auto.nextTask = TASKS.REG_BLOCK2_DAT;
                                            MyDevice.actDev.auto.fifoIndex += 5 * 28;
                                            MyDevice.actDev.auto.fifoCount -= 5;
                                        }
                                        else
                                        {
                                            for (int i = 0; i < 5; i++)
                                            {
                                                Console.WriteLine(MyDevice.actDev.data[i].dtype);
                                            }
                                            MyDevice.actDev.auto.nextTask = TASKS.WRITE_FIFOCLEAR;
                                        }
                                    }
                                }
                                //1.收到的index不是连续性，说明数据丢失，重新读该条
                                else
                                {
                                    //if (MyDevice.protocol.type == COMP.XF)
                                    //{
                                    //    if (MyDevice.mXF[1].fifo.index == MyDevice.mXF[2].fifo.index)
                                    //    {
                                    //        MessageBox.Show("接收器乱码改了index");
                                    //    }
                                    //    else if (Math.Abs(MyDevice.mXF[1].fifo.index - MyDevice.mXF[2].fifo.index) == 140 ||
                                    //             Math.Abs(MyDevice.mXF[2].fifo.index - MyDevice.mXF[1].fifo.index) == 140
                                    //            )
                                    //    {
                                    //        MessageBox.Show("接收器乱码改了index");
                                    //    }
                                    //}
                                    //else if (MyDevice.protocol.type == COMP.TCP)
                                    //{
                                    //    if (MyDevice.mTCP[2].fifo.index == MyDevice.mTCP[1].fifo.index)
                                    //    {
                                    //        MessageBox.Show("路由器乱码改了index");
                                    //    }
                                    //    else if (Math.Abs(MyDevice.mTCP[1].fifo.index - MyDevice.mTCP[2].fifo.index) % 140 == 0 ||
                                    //             Math.Abs(MyDevice.mTCP[2].fifo.index - MyDevice.mTCP[1].fifo.index) % 140 == 0
                                    //            )
                                    //    {
                                    //        MessageBox.Show("路由器乱码改了index");
                                    //    }
                                    //}

                                    Console.WriteLine(MyDevice.actDev.wlan.addr + ": " + MyDevice.actDev.fifo.index + "---异常--------------" + MyDevice.actDev.auto.fifoIndex);
                                    //MyDevice.actDev.auto.fifoIndex -= 5 * 28;//回退到上一条是为了防止丢失的数据是5包中的其一
                                    MyDevice.actDev.auto.dataTick--;
                                    MyDevice.actDev.auto.nextTask = TASKS.WRITE_FIFO_INDEX;
                                }
                            }

                            break;
                        case TASKS.WRITE_FIFOCLEAR:
                            MyDevice.actDev.auto.dataTick = 0;
                            MyDevice.actDev.auto.nextTask = TASKS.REG_BLOCK1_FIFO;
                            break;
                        default:
                            break;
                    }

                    Console.WriteLine("更新UI");
                    TriggerUpdateUI(CurrentCommand);
                }
                //工单
                else if (mode == AutoMode.UserAndTicketWork)
                {
                    switch (CurrentCommand.TaskState)
                    {
                        case TASKS.REG_BLOCK1_FIFO:
                            MyDevice.actDev.auto.fifoIndex = MyDevice.actDev.fifo.read;
                            MyDevice.actDev.auto.fifoCount = MyDevice.actDev.fifo.count;

                            if (MyDevice.actDev.devc.type == TYPE.TQ_XH_XL01_07 - 1280 || MyDevice.actDev.devc.type == TYPE.TQ_XH_XL01_09 - 1280)
                            {
                                if (MyDevice.actDev.auto.fifoCount != 0)
                                {
                                    MyDevice.actDev.auto.nextTask = TASKS.WRITE_FIFO_INDEX;
                                }
                                else
                                {
                                    MyDevice.actDev.auto.nextTask = TASKS.REG_BLOCK1_FIFO;
                                }
                            }
                            else if (MyDevice.actDev.devc.type == TYPE.TQ_XH_XL01_06 - (UInt16)ADDROFFSET.TQ_XH_ADDR || MyDevice.actDev.devc.type == TYPE.TQ_XH_XL01_05 - (UInt16)ADDROFFSET.TQ_XH_ADDR)
                            {
                                MyDevice.actDev.auto.nextTask = TASKS.WRITE_FIFO_INDEX;
                            }
                            else if (MyDevice.actDev.devc.type == TYPE.TQ_XH_XL01_08 - (UInt16)ADDROFFSET.TQ_XH_ADDR)
                            {
                                if (MyDevice.actDev.auto.fifoCount != 0)
                                {
                                    MyDevice.actDev.auto.nextTask = TASKS.WRITE_FIFO_INDEX;
                                }
                                else
                                {
                                    MyDevice.actDev.auto.nextTask = TASKS.REG_BLOCK1_FIFO;
                                }
                            }

                            break;
                        case TASKS.WRITE_FIFO_INDEX:
                            MyDevice.actDev.auto.nextTask = TASKS.REG_BLOCK2_DAT;
                            break;
                        case TASKS.REG_BLOCK2_DAT:
                            if (MyDevice.actDev.devc.type == TYPE.TQ_XH_XL01_06 - (UInt16)ADDROFFSET.TQ_XH_ADDR || MyDevice.actDev.devc.type == TYPE.TQ_XH_XL01_05 - (UInt16)ADDROFFSET.TQ_XH_ADDR)
                            {
                                MyDevice.actDev.auto.readDataNum = MyDevice.actDev.auto.readDataNum + 5;
                                //判断该数据是否合格
                                TriggerUpdateUI(CurrentCommand);
                                if (IsDataValid)
                                {
                                    MyDevice.actDev.auto.nextTask = TASKS.WRITE_FIFOCLEAR;
                                }
                                else
                                {
                                    //不合格读的包数大于缓存立即清
                                    if (MyDevice.actDev.auto.readDataNum >= MyDevice.actDev.auto.fifoCount && MyDevice.actDev.auto.fifoCount > 0)
                                    {
                                        MyDevice.actDev.auto.nextTask = TASKS.WRITE_FIFOCLEAR;
                                        MyDevice.actDev.auto.fifoCount = 0;//清完之后，扳手状态是读一条dat自动清一条，count永久是0
                                    }
                                    else
                                    {
                                        MyDevice.actDev.auto.nextTask = TASKS.REG_BLOCK2_DAT;
                                    }
                                }
                            }
                            else if (MyDevice.actDev.devc.type == TYPE.TQ_XH_XL01_07 - 1280 || MyDevice.actDev.devc.type == TYPE.TQ_XH_XL01_09 - 1280)
                            {
                                if (MyDevice.actDev.fifo.index == MyDevice.actDev.auto.fifoIndex)
                                {
                                    MyDevice.actDev.auto.readDataNum = MyDevice.actDev.auto.readDataNum + 5;

                                    //判断该数据是否合格
                                    TriggerUpdateUI(CurrentCommand);
                                    if (IsDataValid)
                                    {
                                        MyDevice.actDev.auto.nextTask = TASKS.WRITE_FIFOCLEAR;
                                    }
                                    else
                                    {
                                        MyDevice.actDev.auto.nextTask = TASKS.REG_BLOCK2_DAT;
                                        MyDevice.actDev.auto.fifoIndex += 5 * 28;

                                        if (MyDevice.actDev.auto.readDataNum >= MyDevice.actDev.auto.fifoCount)
                                            MyDevice.actDev.auto.nextTask = TASKS.WRITE_FIFOCLEAR;//读出的包超出缓存数，清缓存
                                    }
                                }
                                //1.收到的index不是连续性，说明数据丢失，重新读该条
                                else
                                {
                                    MyDevice.actDev.auto.fifoIndex -= 5 * 28;
                                    MyDevice.actDev.auto.nextTask = TASKS.WRITE_FIFO_INDEX;
                                }
                            }
                            else if (MyDevice.actDev.devc.type == TYPE.TQ_XH_XL01_08 - (UInt16)ADDROFFSET.TQ_XH_ADDR)
                            {
                                if (MyDevice.actDev.fifo.index == MyDevice.actDev.auto.fifoIndex)
                                {
                                    MyDevice.actDev.auto.readDataNum = MyDevice.actDev.auto.readDataNum + 5;

                                    //判断该数据是否合格
                                    TriggerUpdateUI(CurrentCommand);
                                    if (IsDataValid)
                                    {
                                        MyDevice.actDev.auto.nextTask = TASKS.WRITE_FIFOCLEAR;
                                    }
                                    else
                                    {
                                        MyDevice.actDev.auto.nextTask = TASKS.REG_BLOCK2_DAT;
                                        MyDevice.actDev.auto.fifoIndex += 5 * 28;

                                        if (MyDevice.actDev.auto.readDataNum >= MyDevice.actDev.auto.fifoCount)
                                            MyDevice.actDev.auto.nextTask = TASKS.WRITE_FIFOCLEAR;//读出的包超出缓存数，清缓存
                                    }
                                }
                                //1.收到的index不是连续性，说明数据丢失，重新读该条
                                else
                                {
                                    MyDevice.actDev.auto.fifoIndex -= 5 * 28;
                                    MyDevice.actDev.auto.nextTask = TASKS.WRITE_FIFO_INDEX;
                                }
                            }
                            break;
                        case TASKS.WRITE_FIFOCLEAR:
                            //合格清缓存并且切点
                            TriggerUpdateUI(CurrentCommand);
                            MyDevice.actDev.auto.readDataNum = 0;
                            MyDevice.actDev.auto.nextTask = TASKS.REG_BLOCK1_FIFO;
                            break;
                        default:
                            break;
                    }
                }
            }

            //获取指令类型
            string commandType = CurrentCommand.GetType().Name;
            if (commandType == "UserCommand")
            {
                // 移除已处理的指令
                userCommandQueue.TryDequeue(out _);
            }
            else if (commandType == "AutoCommand")
            {
                // 移除已处理的指令
                autoCommandQueue.TryDequeue(out _);
            }

            // 重置超时计数器
            if (timeoutCounts.ContainsKey(CurrentCommand))
            {
                timeoutCounts[CurrentCommand] = 0;
            }
        }

        // 处理超时
        private void HandleTimeout()
        {
            // 增加超时计数器
            if (!timeoutCounts.ContainsKey(CurrentCommand))
            {
                timeoutCounts[CurrentCommand] = 0;
            }
            timeoutCounts[CurrentCommand]++;

            // 如果一个命令超时重发达到重发次数，从队列中移除它
            if (timeoutCounts[CurrentCommand] >= retry)
            {
                string commandType = CurrentCommand.GetType().Name;
                if (commandType == "UserCommand")
                {
                    userCommandQueue.TryDequeue(out _);
                }
                else if (commandType == "AutoCommand")
                {
                    autoCommandQueue.TryDequeue(out _);
                }
                timeoutCounts.Remove(CurrentCommand);
            }
        }

        #endregion

        #region 添加用户指令

        //为指令队列添加指令
        public void AddUserCommand(UserCommand userCommand)
        {
            userCommandQueue.Enqueue(userCommand);
        }

        //为指令队列添加指令
        public void AddUserCommand(byte addr, ProtocolFunc protocolFunc, TASKS wrenchStatus, string source)
        {
            UserCommand userCommand = new UserCommand(addr.ToString(), protocolFunc, wrenchStatus, source);
            AddUserCommand(userCommand);
        }

        //为指令队列添加指令
        public void AddUserCommand(byte addr, ProtocolFunc protocolFunc, TASKS wrenchStatus, ushort writeData, string source)
        {
            UserCommand userCommand = new UserCommand(addr.ToString(), protocolFunc, wrenchStatus, source);
            userCommand.WriteData = writeData;
            AddUserCommand(userCommand);
        }

        //为指令队列增加多条指令
        public void AddUserCommands(List<UserCommand> userCommands)
        {
            foreach (var userCommand in userCommands)
            {
                AddUserCommand(userCommand);
            }
        }

        //为指令队列增加多条指令
        public void AddUserCommands(byte addr, ProtocolFunc protocolFunc, List<TASKS> autoCommands, string source)
        {
            List<UserCommand> userCommands = new List<UserCommand>();
            foreach (var autoCommand in autoCommands)
            {
                userCommands.Add(new UserCommand(addr.ToString(), protocolFunc, autoCommand, source));
            }

            AddUserCommands(userCommands);
        }

        #endregion

        #region 添加自动机指令

        //自动机队列入队
        private void EnqueueAutoCommand(AutoMode mode, Func<XET, AutoCommand> createCommandFunc)
        {
            XET[] mDev = MyDevice.mDev;
            if (selectedDev == 0)
            {
                //更新设备列表
                UpdateValidDevices();
                //向自动机队列中添加指令
                autoCommandQueue = new ConcurrentQueue<AutoCommand>();
                foreach (int index in validDevices)
                {
                    AutoCommand newAutoCommand = createCommandFunc(mDev[index]);
                    autoCommandQueue.Enqueue(newAutoCommand);
                }
            }
            else //有在实时数据界面选择设备
            {
                if (mDev[selectedDev].sTATE == STATE.WORKING || mDev[selectedDev].sTATE == STATE.CONNECTED)
                {
                    //向自动机队列中添加指令
                    autoCommandQueue = new ConcurrentQueue<AutoCommand>();
                    AutoCommand newAutoCommand = createCommandFunc(mDev[selectedDev]);
                    autoCommandQueue.Enqueue(newAutoCommand);
                }
            }
        }

        //创建实时数据型自动机指令
        private AutoCommand CreateDataAutoCommand(XET mDev)
        {
            AutoCommand newAutoCommand = new AutoCommand()
            {
                WrenchId = mDev.wlan.addr.ToString(),
                TaskState = mDev.auto.nextTask,
                SeqWriteNum = mDev.auto.dataTick * 5,
                SeqWriteIndex = mDev.auto.fifoIndex
            };

            switch (newAutoCommand.TaskState)
            {
                case TASKS.REG_BLOCK1_FIFO:
                    newAutoCommand.ProtocolFunc = ProtocolFunc.Protocol_Read_SendCOM;
                    break;
                case TASKS.WRITE_FIFO_INDEX:
                    newAutoCommand.ProtocolFunc = ProtocolFunc.Protocol_Sequence_FifoIndex;
                    break;
                case TASKS.REG_BLOCK2_DAT:
                    newAutoCommand.ProtocolFunc = ProtocolFunc.Protocol_Read_SendCOM;
                    break;
                case TASKS.WRITE_FIFOCLEAR:
                    newAutoCommand.ProtocolFunc = ProtocolFunc.Protocol_Sequence_FifoClear;
                    break;
                default:
                    break;
            }

            return newAutoCommand;
        }

        //创建工单型自动机指令
        private AutoCommand CreateTicketAutoCommand(XET mDev)
        {
            AutoCommand newAutoCommand = new AutoCommand()
            {
                WrenchId = mDev.wlan.addr.ToString(),
                TaskState = mDev.auto.nextTask,
                SeqWriteNum = 0x1FFFFFFF,
                SeqWriteIndex = mDev.auto.fifoIndex
            };

            switch (newAutoCommand.TaskState)
            {
                case TASKS.REG_BLOCK1_FIFO:
                    newAutoCommand.ProtocolFunc = ProtocolFunc.Protocol_Read_SendCOM;
                    break;
                case TASKS.WRITE_FIFO_INDEX:
                    newAutoCommand.ProtocolFunc = ProtocolFunc.Protocol_Sequence_FifoIndex;
                    break;
                case TASKS.REG_BLOCK2_DAT:
                    newAutoCommand.ProtocolFunc = ProtocolFunc.Protocol_Read_SendCOM;
                    break;
                case TASKS.WRITE_FIFOCLEAR:
                    newAutoCommand.ProtocolFunc = ProtocolFunc.Protocol_Sequence_FifoClear;
                    break;
                default:
                    break;
            }

            return newAutoCommand;
        }

        #endregion

        #region 清除指令

        //清除指令，防止页面重启时上次的指令积累导致冲突
        public void ClearCommand()
        {
            //清空用户指令队列
            if (userCommandQueue.Count > 0)
            {
                while (userCommandQueue.TryDequeue(out UserCommand result))
                {
                    // 继续尝试出列直到队列为空
                }
            }

            //清空自动机指令队列
            if (autoCommandQueue.Count > 0)
            {
                while (autoCommandQueue.TryDequeue(out AutoCommand result))
                {
                    // 继续尝试出列直到队列为空
                }
            }
        }

        #endregion

        #region 定时发指令

        //定时发送指令，防止设备5分钟内无通讯自动关机
        public void TimedSend(Object source, ElapsedEventArgs e)
        {
            // 确保在UI线程中执行任务
            if (Application.OpenForms.Count > 0)
            {
                Form mainForm = Application.OpenForms[0];
                mainForm.Invoke((MethodInvoker)delegate
                {
                    UpdateValidDevices();//确定设备状态

                    //必须存在设备连接
                    if (validDevices.Count > 0)
                    {
                        XET[] mDev = MyDevice.mDev;
                        //向自动机队列中添加指令
                        foreach (int index in validDevices)
                        {
                            //发送唤醒指令
                            AddUserCommand((byte)index, ProtocolFunc.Protocol_Read_SendCOM, TASKS.REG_BLOCK3_JOB, mainForm.Name);
                        }
                    }

                });
            }
        }

        #endregion

        #region 开机自动连接

        //分析当前工位的定制，自动连接扳手
        public void AutoConnect()
        {
            if (!MyDevice.IsMySqlStart)
            {
                MessageBox.Show("软件未安装对应的数据库，无法执行自动连接！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            byte autoAddr = 1;//自动连接扳手地址
            List<byte> autoAddrList = new List<byte>();//自动连接扳手的站点汇总（不可有重复的站点地址，否则冲突）
            List<byte> recAddrList = new List<byte>();//试探指令发出后回复的站点汇总
            List<string> autoConnectTypeList = new List<string>();//自动连接扳手的连接方式（不可重复，只能确定一种方式选择连接）
            DSWrenchWlan targetWrenchWlan = new DSWrenchWlan();//目标设备wlan相关参数
            List<DSWrenchDevc> wrenchDevcList = new List<DSWrenchDevc>();//扳手汇总表
            List<DSWrenchDevc> autoWrenchList = new List<DSWrenchDevc>();//自动连接扳手表
            List<Tuple<int, byte, string>> SerialPortInfo = new List<Tuple<int, byte, string>>(); //设备要求串口波特率，停止位，校验位
            List<Tuple<string, string, string, ushort>> WiFiInfo = new List<Tuple<string, string, string, ushort>>(); //设备WiFi信息

            //利用多线程，避免因为通讯导致页面开启自动连接后无法点击其他页面
            Task.Run(() => {

                //先读取工位表,有几把扳手允许自动连接

                //读取扳手汇总表
                wrenchDevcList = JDBC.GetAllWrenchDevc();
                //获取扳手允许自动连接的表
                if (wrenchDevcList.Count > 0)
                {
                    foreach (var itemWrench in wrenchDevcList)
                    {
                        //站点更新
                        targetWrenchWlan = JDBC.GetWrenchWlanByWlanId(itemWrench.WlanId);
                        autoAddr = targetWrenchWlan.Addr;

                        //确定扳手是否允许自动连接
                        if (itemWrench.ConnectAuto == "True")
                        {
                            autoAddrList.Add(autoAddr);
                            autoConnectTypeList.Add(itemWrench.ConnectType);
                            autoWrenchList.Add(itemWrench);

                            SerialPortInfo.Add(new Tuple<int, byte, string>(
                                MyDevice.GetBaud(targetWrenchWlan.Baud),
                                MyDevice.GetStopBit(targetWrenchWlan.Stopbit),
                                MyDevice.GetParity(targetWrenchWlan.Parity))
                                );
                            WiFiInfo.Add(new Tuple<string, string, string, ushort>(
                                targetWrenchWlan.WFSsid,
                                targetWrenchWlan.WFPwd,
                                targetWrenchWlan.WFIp,
                                targetWrenchWlan.WFPort
                                ));

                            //加入新扳手站点后分析是否存在站点冲突
                            if (autoAddrList.Distinct().Count() != autoAddrList.Count)
                            {
                                MessageBox.Show($"设置自动连接的扳手系列中站点为{autoAddr}有多把，存在冲突，无法执行自动连接！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            //加入新扳手站点后分析是否存在连接方式不统一
                            if (autoConnectTypeList.Distinct().Count() != 1)
                            {
                                MessageBox.Show($"设置自动连接的扳手系列中连接方式有多种，存在冲突，无法执行自动连接！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                            //连接方式统一
                            else
                            {
                                //考虑有线和蓝牙只能 1对1，故多个设备自动连接无法实现
                                if (autoConnectTypeList.Count > 1 && (autoConnectTypeList[0] == "有线连接" || autoConnectTypeList[0] == "蓝牙连接"))
                                {
                                    MessageBox.Show($"{autoConnectTypeList[0]}只能自动连接一台设备，有多台设备设置成{autoConnectTypeList[0]}存在冲突，请重新设置！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }

                                //连接方式统一，需要判断多设备连接的信息是否统一
                                if (autoConnectTypeList.Count > 1 && autoConnectTypeList[0] == "RS485连接")
                                {
                                    //检查多设备的所需串口信息
                                    if (CompareSerialPortInfo(SerialPortInfo) == false) return;
                                }
                                else if (autoConnectTypeList.Count > 1 && (autoConnectTypeList[0] == "接收器连接" || autoConnectTypeList[0] == "路由器WiFi连接"))
                                {
                                    //检查多设备的所需串口信息和WiFi信息
                                    if (CompareSerialPortInfo(SerialPortInfo) == false) return;
                                    if (CompareWiFiInfo(WiFiInfo) == false) return;
                                }
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("软件设置允许扳手自动连接的数量为 0，请重新设置！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                //判断
                if (autoAddrList.Count > 0)
                {
                    //先排除多串口无法识别正确串口的问题
                    if (autoConnectTypeList[0] != "路由器WiFi连接" && SerialPort.GetPortNames().Count() > 1)
                    {
                        MessageBox.Show("请保证有且只有一个串口被打开，否则自动连接无法识别正确的串口进行通讯，请重新设置！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    //轮询发送试探指令
                    switch (autoConnectTypeList[0])
                    {
                        case "有线连接":
                            //切换通讯
                            MyDevice.mePort_SetProtocol(COMP.UART);
                            MyDevice.ConnectType = "有线连接";
                            
                            //打开串口
                            MyDevice.protocol.Protocol_PortOpen(SerialPort.GetPortNames()[0], 115200, StopBits.One, Parity.None);

                            //串口有效
                            if (MyDevice.myUART.IsOpen)
                            {
                                //发送试探指令
                                SendTestCommand(autoAddrList, recAddrList);
                            }
                            break;
                        case "RS485连接":
                            //切换通讯
                            MyDevice.mePort_SetProtocol(COMP.RS485);
                            MyDevice.ConnectType = "RS485连接";

                            //打开串口
                            MyDevice.protocol.Protocol_PortOpen(SerialPort.GetPortNames()[0], MyDevice.GetBaud(targetWrenchWlan.Baud), (StopBits)targetWrenchWlan.Stopbit, (Parity)targetWrenchWlan.Parity);
                            //串口有效
                            if (MyDevice.myRS485.IsOpen)
                            {
                                //发送试探指令
                                SendTestCommand(autoAddrList, recAddrList);
                            }
                            break;
                        case "蓝牙连接":
                        case "接收器连接":
                            //切换通讯
                            MyDevice.mePort_SetProtocol(COMP.XF);
                            MyDevice.ConnectType = autoConnectTypeList[0];

                            //打开串口
                            MyDevice.protocol.Protocol_PortOpen(SerialPort.GetPortNames()[0], 115200, StopBits.One, Parity.None);
                            //串口有效
                            if (MyDevice.myXFUART.IsOpen)
                            {
                                //发送试探指令
                                SendTestCommand(autoAddrList, recAddrList);
                            }
                            break;
                        case "路由器WiFi连接":
                            List<string> ipList = new List<string>();
                            ipList = MyDevice.GetIPList();
                            //通常一台电脑联网状态只会分配一个IP地址
                            //局限性：特殊情况下多个IP地址，无法确定哪一个是准确能进行通讯的IP
                            if (ipList == null || ipList.Count == 0 || ipList[0] == "127.0.0.1")
                            {
                                MessageBox.Show("自动连接的WIFI端口未能找到，请重新设置！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            //切换通讯
                            MyDevice.mePort_SetProtocol(COMP.TCP);
                            MyDevice.ConnectType = "路由器WiFi连接";

                            //打开端口
                            MyDevice.protocol.Protocol_PortOpen(ipList[0], 5678, StopBits.One, Parity.None);
                            //串口有效
                            if (MyDevice.myTCPUART.IsOpen)
                            {
                                Thread.Sleep(2000);//服务端给客户端分配ip时需要时间
                                if (MyDevice.clientConnectionItems.Count != 0)
                                {
                                    if (MyDevice.clientConnectionItems.Count > autoAddrList.Count)
                                    {
                                        MessageBox.Show("能分配IP的扳手数量超过自动连接设备数量，防止通讯混乱，请重新设置！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                        return;
                                    }
                                    else
                                    {
                                        MyDevice.protocol.port = MyDevice.clientConnectionItems.Values.ElementAt(connectID);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("IP分配失败，扳手关机或者故障，请重新设置！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }

                                SendTestCommand(autoAddrList, recAddrList);
                            }
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    MessageBox.Show($"扳手库中没有扳手设置成自动连接模式，无法执行自动连接！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                //取消任务，终止函数
                if (autoConnectCts.Token.IsCancellationRequested)
                {
                    return;
                }

                //试探指令回复结果
                if (autoAddrList.Count == 0 || recAddrList.Count < autoAddrList.Count)
                {
                    List<byte> failAddrList = autoAddrList.Except(recAddrList).ToList();
                    failAddrList.Sort();//升序
                    string outPut = "";
                    foreach (var element in failAddrList)
                    {
                        outPut += element + "，";
                    }
                    //更新连接设备数和工作设备数
                    MyDevice.ConnectDevCnt = autoAddrList.Count;
                    MyDevice.WorkDevCnt = recAddrList.Count;

                    MessageBox.Show($"设备{outPut}连接异常，自动连接失败，请手动连接", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                List<byte> disConnectDevs = new List<byte>();//掉线设备集合
                //回复了开始连接指令
                foreach (var item in recAddrList)
                {
                    //取消任务，终止函数
                    if (autoConnectCts.Token.IsCancellationRequested)
                    {
                        return;
                    }

                    int conSendCnt = 0;//连接指令发送次数
                    MyDevice.protocol.addr = item;
                    MyDevice.protocol.trTASK = TASKS.NULL;

                    if (MyDevice.ConnectType == "路由器WiFi连接")
                    {
                        MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
                    }

                    while (MyDevice.protocol.trTASK != TASKS.REG_BLOCK2_DAT)
                    {
                        if (receiveSignal.WaitOne(TimeSpan.FromMilliseconds(timeout)))
                        {
                            Console.WriteLine("自动连接" + item + "=====" + MyDevice.protocol.trTASK + "=====" + DateTime.Now.ToString("HH:mm:ss:fff"));
                            Thread.Sleep(10);//防止发送太快，刚接收完整回复就立即发送指令
                            MyDevice.protocol.Protocol_mePort_ReadAllTasks();
                        }
                        else
                        {
                            MyDevice.protocol.Protocol_ClearState();
                            //超时重发
                            MyDevice.protocol.Protocol_mePort_ReadAllTasks();

                            conSendCnt++;
                            if (conSendCnt > retry)
                            {
                                MyDevice.protocol.trTASK = TASKS.REG_BLOCK2_DAT;//指令发送超过最大限制次数都不回复，默认设备掉线
                                disConnectDevs.Add(item);
                            }
                        }
                    }
                }

                //检测掉线情况
                if (disConnectDevs.Count > 0)
                {
                    string outDisConnect = "";
                    foreach (var item in disConnectDevs)
                    {
                        outDisConnect += item + "，";
                    }
                    MessageBox.Show($"设备{outDisConnect}连接异常，自动连接失败，请手动连接", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

            });
        }

        //取消自动连接
        public void CancelAutoConnect()
        {
            autoConnectCts?.Cancel();
        }

        //发送试探指令
        private void SendTestCommand(List<byte> targetAutoAddrList, List<byte> targetRecAddrList)
        {
            int sendCntMax = 10;//试探指令发送范围，超过这个数量切换下一台设备

            foreach_start:  // 定义循环开始的标签

            //发送试探指令
            foreach (var item in targetAutoAddrList)
            {
                //取消任务，终止函数
                if (autoConnectCts.Token.IsCancellationRequested)
                {
                    return;
                }

                UserCommand testCommand = new UserCommand(item.ToString(), ProtocolFunc.Protocol_Read_SendCOM, TASKS.REG_BLOCK3_JOB, null);//试探指令
                CurrentCommand = testCommand;
                SendCommand(CurrentCommand);
                bool testReceive = false;//试探指令是否回复
                int sendCnt = 0;//发送次数
                while (!testReceive)
                {
                    // 等待设备回复或超时
                    if (receiveSignal.WaitOne(TimeSpan.FromMilliseconds(timeout)))
                    {
                        Console.WriteLine("接受" + item + "=====" + MyDevice.protocol.trTASK + "=====" + DateTime.Now.ToString("HH:mm:ss:fff"));
                        if (!targetRecAddrList.Contains(item))
                        {
                            targetRecAddrList.Add(item);//回复扳手汇总

                            if (MyDevice.ConnectType == "路由器WiFi连接")
                            {
                                //设备地址绑定Ip
                                if (MyDevice.addr_ip.ContainsKey(item.ToString()) == false)
                                {
                                    MyDevice.addr_ip.Add(item.ToString(), ((IPEndPoint)((Socket)MyDevice.protocol.port).RemoteEndPoint).Address.ToString());
                                }

                                if (connectID < MyDevice.clientConnectionItems.Count - 1)
                                {
                                    connectID++;
                                    MyDevice.protocol.port = MyDevice.clientConnectionItems.Values.ElementAt(connectID);
                                }

                                //读取之后需要从头开始扫描
                                //从头开始的原因：由于一开始给设备分配IP地址时，先后顺序是随机的，不一定就是根据扳手站点排序
                                //示例：设备1和3均被分配Ip，可能一开始读取的ip是设备3的，此时只有设备3能响应，不从头开始的话设备1跳过了
                                if (targetAutoAddrList.Count > 1 && targetRecAddrList.Count < targetAutoAddrList.Count)
                                {
                                    // 从头开始循环
                                    goto foreach_start; // 跳转到循环的开始
                                }
                            }
                        }
                        testReceive = true;
                    }
                    else
                    {
                        MyDevice.protocol.Protocol_ClearState();
                        //超时重发
                        SendCommand(CurrentCommand);
                        Console.WriteLine("重发" + item + "=====" + MyDevice.protocol.trTASK + "=====" + DateTime.Now.ToString("HH:mm:ss:fff"));
                        //超过发送次数默认跳过不回复设备，防止影响后面设备的连接
                        sendCnt++;
                        if (sendCnt > sendCntMax)
                        {
                            testReceive = true;
                        }
                    }
                }
            }
        }

        //比较设备需要串口信息（波特率，校验位，停止位）
        private bool CompareSerialPortInfo(List<Tuple<int, byte, string>> SerialPortInfo)
        {
            // 检查是否所有的 Tuple 中相同位置的元素都是相同的
            for (int i = 0; i < SerialPortInfo.Count - 1; i++)
            {
                for (int j = i + 1; j < SerialPortInfo.Count; j++)
                {
                    if (SerialPortInfo[i].Item1 != SerialPortInfo[j].Item1 ||
                        SerialPortInfo[i].Item2 != SerialPortInfo[j].Item2 ||
                        SerialPortInfo[i].Item3 != SerialPortInfo[j].Item3)
                    {
                        MessageBox.Show($"扳手库中多台设备的波特率，校验位，停止位不统一，无法执行自动连接！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                }
            }
            return true;
        }

        //比较设备的WiFi信息
        private bool CompareWiFiInfo(List<Tuple<string, string, string, ushort>> WiFiInfo)
        {
            // 检查是否所有的 Tuple 中相同位置的元素都是相同的
            for (int i = 0; i < WiFiInfo.Count - 1; i++)
            {
                for (int j = i + 1; j < WiFiInfo.Count; j++)
                {
                    if (WiFiInfo[i].Item1 != WiFiInfo[j].Item1 ||
                        WiFiInfo[i].Item2 != WiFiInfo[j].Item2 ||
                        WiFiInfo[i].Item3 != WiFiInfo[j].Item3 ||
                        WiFiInfo[i].Item4 != WiFiInfo[j].Item4)
                    {
                        MessageBox.Show($"扳手库中多台设备的WiFi相关参数不统一，无法执行自动连接！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                }
            }

            return true;
        }

        #endregion

        #region 发送

        //发送指令
        private void SendCommand(Command command)
        {
            //地址
            byte wrenchId;
            if (string.IsNullOrEmpty(command.WrenchId) || !byte.TryParse(command.WrenchId, out wrenchId))
            {
                return;
            }
            byte newaddr = MyDevice.actDev.wlan.addr;
            MyDevice.protocol.addr = wrenchId;

            //发送
            switch (command.TaskState)
            {
                case TASKS.REG_BLOCK3_WLAN:
                    //发指令
                    MyDevice.actDev.wlan.addr = newaddr;
                    ExecuteCommand(command);
                    MyDevice.protocol.addr = newaddr;
                    break;

                default:
                    //发指令
                    ExecuteCommand(command);
                    break;
            }
        }

        //使用对应的发送方法
        private void ExecuteCommand(Command command)
        {
            ProtocolFunc protocolFunc = command.ProtocolFunc;
            TASKS meTask = command.TaskState;
            //Console.WriteLine("Send: " + command.ProtocolFunc + " " + command.TaskState + " " + command.WrenchId);

            switch (protocolFunc)
            {
                case ProtocolFunc.Protocol_Read_SendCOM:
                    MyDevice.protocol.Protocol_Read_SendCOM(meTask);
                    break;

                case ProtocolFunc.Protocol_Write_SendCOM:
                    if (command is UserCommand userCommand)
                    {
                        UInt16 data = userCommand.WriteData;
                        MyDevice.protocol.Protocol_Write_SendCOM(meTask, data);
                    }
                    else
                    {
                        MyDevice.protocol.Protocol_Write_SendCOM(meTask);
                    }
                    break;

                case ProtocolFunc.Protocol_Sequence_FifoClear:
                    if (command is AutoCommand autoCommand)
                    {
                        int num = autoCommand.SeqWriteNum;
                        MyDevice.protocol.Protocool_Sequence_FifoClear(num);
                    }
                    break;

                case ProtocolFunc.Protocol_Sequence_FifoIndex:
                    if (command is AutoCommand autoCommand2)
                    {
                        uint index = autoCommand2.SeqWriteIndex;
                        MyDevice.protocol.Protocool_Sequence_FifoIndex(index);
                    }
                    break;

                case ProtocolFunc.Protocol_Sequence_SendCOM:
                    MyDevice.protocol.Protocool_Sequence_SendCOM(meTask);
                    break;

                default:
                    Console.WriteLine("Invalid command type");
                    break;
            }
        }

        #endregion

        #region 接收

        //接收处理
        private void receiveData()
        {
            if (CurrentCommand == null) return;

            // 发送信号以表示设备已回复
            receiveSignal.Set();
            Console.WriteLine("信号量回复");
        }

        #endregion

        #region 设备管理

        //更新设备列表
        private void UpdateValidDevices()
        {
            validDevices = new ConcurrentBag<int>();
            foreach (var id in MyDevice.GetDevState(STATE.CONNECTED))
            {
                validDevices.Add(id);
            }
            foreach (var id in MyDevice.GetDevState(STATE.WORKING))
            {
                validDevices.Add(id);
            }
        }

        #endregion

        #region UI更新事件

        public event EventHandler<UpdateUIEventArgs> UpdateUI;    //接收event

        //接收事件
        public void TriggerUpdateUI(Command command)
        {
            //触发UI更新事件
            TriggerUpdateUIEvent(new UpdateUIEventArgs(command));
        }

        private static readonly object lock_onReceive = new object();
        //封装接收事件
        protected virtual void TriggerUpdateUIEvent(UpdateUIEventArgs e)
        {
            Console.WriteLine("进入锁");

            lock (lock_onReceive)
            {
                UpdateUI?.Invoke(this, e);
                MyDevice.StickyPacksHandle.Set();
            }
        }

        #endregion
    }

    #region 指令类

    //指令
    public class Command
    {
        //发到哪儿 WrenchId
        //怎么发 FunctionName 
        //发什么 TaskState
        //什么时候发的 Timestamp
        //从哪儿发 Source

        //能在通讯过程中区别识别扳手的某一XET参数，如addr、ip
        public string WrenchId { get; set; }
        //使用的发送方法名称
        public ProtocolFunc ProtocolFunc { get; set; }
        //指令名称/当前扳手在自动机的指令状态
        public TASKS TaskState { get; set; }
        //时间戳
        public long Timestamp { get; set; }
        //指令来源（UI名称，对应的委托，UI变化方法、数据库操作方法）
        public string Source { get; set; }
    }

    //UI指令队列项
    public class UserCommand : Command
    {
        //发送写命令的data
        public ushort WriteData { get; set; }

        public UserCommand(string wrenchId, ProtocolFunc protocolFunc, TASKS taskStatus, string source)
        {
            WrenchId = wrenchId;
            ProtocolFunc = protocolFunc;
            TaskState = taskStatus;
            WriteData = 0xFFFF;
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            Source = source;
        }
    }

    //自动机项
    public class AutoCommand : Command
    {
        //连续写的num
        public int SeqWriteNum { get; set; }
        //连续写的index
        public uint SeqWriteIndex { get; set; }

        public AutoCommand()
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
        public AutoCommand(string wrenchId, TASKS taskStatus)
        {
            WrenchId = wrenchId;
            TaskState = taskStatus;
            SeqWriteNum = 0;
            SeqWriteNum = 0;
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }

    #endregion

    //自动模式
    public enum AutoMode
    {
        UserOnly = 1,          // 只有用户指令
        UserAndActualData = 2, // 用户指令和实时数据
        UserAndTicketWork = 3  // 用户指令和工单
    }

    //UI更新事件
    public class UpdateUIEventArgs : EventArgs
    {
        public Command Command { get; set; }

        public UpdateUIEventArgs(Command command)
        {
            Command = command;
        }
    }
}
