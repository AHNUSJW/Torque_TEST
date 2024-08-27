using DBHelper;
using Library;
using Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

//Ricardo 20240821

namespace Base.UI.MenuDevice
{
    public partial class MenuQuickConnectForm : Form
    {
        private List<byte> quickAddrList = new List<byte>();//快捷连接扳手的站点汇总（不可有重复的站点地址，否则冲突）
        private List<string> quickConnectTypeList = new List<string>();//快捷连接扳手的连接方式（不可重复，只能确定一种方式选择连接）
        private DSWrenchWlan targetWrenchWlan = new DSWrenchWlan();//目标设备wlan相关参数
        private List<DSWrenchDevc> wrenchDevcList = new List<DSWrenchDevc>();//扳手汇总表
        private List<DSWrenchDevc> quickWrenchList = new List<DSWrenchDevc>();//快捷连接扳手表
        private List<Tuple<int, byte, string>> SerialPortInfo = new List<Tuple<int, byte, string>>(); //设备要求串口波特率，停止位，校验位
        private List<Tuple<string, string, string, ushort>> WiFiInfo = new List<Tuple<string, string, string, ushort>>(); //设备WiFi信息
        private int portID = 0;//端口号下标，递增（用于TCP连接）
        private int connectCnt = 0;//实际连接数量
        private int targetConCnt = 0;//目标连接数量
        private const int devMax = 24; //设备最大连接数
        private const int sendMax = 5; //指令最大发送次数，超过代表设备掉线
        private List<Button> buttons = new List<Button>();//站点按钮集合
        private TASKS meTask = TASKS.NULL;//当前状态机
        private CancellationTokenSource _cts = new CancellationTokenSource();//线程取消令牌
        private bool isAloneConnect = false;//单连接
        private bool isConnecting = false;//是否正在连接


        public MenuQuickConnectForm()
        {
            InitializeComponent();
        }

        private void MenuQuickConnectForm_Load(object sender, EventArgs e)
        {
            /* 根据管理员预定的设备连接方式与连接设备数进行快捷连接 */
            MyDevice.myUpdate += new freshHandler(receiveData);

            //判断数据库是否开启
            if (!GetComPuterInfo.ServiceIsRunning("MySQL", 21))
            {
                Enable_Menu(false);
                MessageBox.Show("Mysql数据库未安装，该页面功能无法使用", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //根据自动连接控制控件权限
            Enable_Menu(IsQuickConnect());

            //串口/端口初始化
            InitPort();

            //根据设备数量开放按钮
            InitBtn();
        }

        //页面关闭
        private void MenuQuickConnectForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            MyDevice.myUpdate -= new freshHandler(receiveData);
            _cts.Cancel();//取消通讯线程
            timer1.Enabled = false;
        }

        // 多连接
        private void btn_connect_Click(object sender, EventArgs e)
        {
            foreach (var btn in buttons)
            {
                btn.BackColor = SystemColors.Control;//恢复默认色
            }

            label2.Visible = true;
            label2.Text = "请耐心等待，设备正在进行连接通讯...";
            portID = 0;
            connectCnt = 0;
            isAloneConnect = false;

            InitDev();
        }

        //单设备连接
        private async void BtnX_Click(object sender, EventArgs e)
        {
            if (comboBox0_port.Text == "")
            {
                MessageBox.Show("无有效串口，请检测终端设备");
                return;
            }

            Button buttonX = sender as Button;
            buttonX.BackColor = SystemColors.Control;//恢复默认色
            connectCnt = 0;
            portID = 0;
            label2.Visible = true;
            label2.Text = "请耐心等待，设备正在进行连接通讯...";
            isAloneConnect = true;
            timer1.Enabled = false;

            switch (quickConnectTypeList[0])
            {
                case "有线连接":
                    //切换通讯
                    MyDevice.mePort_SetProtocol(COMP.UART);
                    MyDevice.ConnectType = "有线连接";

                    //打开串口
                    MyDevice.protocol.Protocol_PortOpen(comboBox0_port.Text, 115200, StopBits.One, Parity.None);

                    //串口有效
                    if (MyDevice.myUART.IsOpen)
                    {
                        targetConCnt = 1;
                        //开始连接指令
                        await SendCommandAsync(new List<byte> { Convert.ToByte(buttonX.Text) });
                    }
                    break;
                case "RS485连接":
                    //切换通讯
                    MyDevice.mePort_SetProtocol(COMP.RS485);
                    MyDevice.ConnectType = "RS485连接";

                    //打开串口
                    MyDevice.protocol.Protocol_PortOpen(comboBox0_port.Text, MyDevice.GetBaud(targetWrenchWlan.Baud), (StopBits)targetWrenchWlan.Stopbit, (Parity)targetWrenchWlan.Parity);
                    //串口有效
                    if (MyDevice.myRS485.IsOpen)
                    {
                        targetConCnt = 1;
                        //开始连接指令
                        await SendCommandAsync(new List<byte> { Convert.ToByte(buttonX.Text) });
                    }
                    break;
                case "蓝牙连接":
                case "接收器连接":
                    //切换通讯
                    MyDevice.mePort_SetProtocol(COMP.XF);
                    MyDevice.ConnectType = quickConnectTypeList[0];

                    //打开串口
                    MyDevice.protocol.Protocol_PortOpen(comboBox0_port.Text, 115200, StopBits.One, Parity.None);
                    //串口有效
                    if (MyDevice.myXFUART.IsOpen)
                    {
                        targetConCnt = 1;
                        //开始连接指令
                        await SendCommandAsync(new List<byte> { Convert.ToByte(buttonX.Text) });
                    }
                    break;
                case "路由器WiFi连接":
                    List<string> ipList = new List<string>();
                    ipList = MyDevice.GetIPList();

                    string targetIp = null;//进行连接之前已经统一了ip相关信息,即WiFiInfo[0].Item3
                    for (int i = 0; i < WiFiInfo[0].Item3.Length; i += 2)
                    {
                        int num = Convert.ToInt32($"{WiFiInfo[0].Item3[i]}{WiFiInfo[0].Item3[i + 1]}", 16);
                        targetIp = i == 0 ? num.ToString() : targetIp + "." + num.ToString();
                    }
                    //通常一台电脑联网状态只会分配一个IP地址,不联网的状态下IP为"127.0.0.1"
                    //局限性：特殊情况下多个IP地址
                    if (ipList == null || ipList.Count == 0 || ipList[0] == "127.0.0.1" || !ipList.Contains(targetIp))
                    {
                        MessageBox.Show("快捷连接的WIFI端口未能找到，请重新设置！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    //切换通讯
                    MyDevice.mePort_SetProtocol(COMP.TCP);
                    MyDevice.ConnectType = "路由器WiFi连接";

                    if (targetIp != comboBox0_port.Text)
                    {
                        MessageBox.Show($"请选择正确的端口{targetIp}", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    //打开端口
                    MyDevice.protocol.Protocol_PortOpen(targetIp, 5678, StopBits.One, Parity.None);
                    //串口有效
                    if (MyDevice.myTCPUART.IsOpen)
                    {
                        Thread.Sleep(1000);//服务端给客户端分配ip时需要时间
                        if (MyDevice.clientConnectionItems.Count != 0)
                        {
                            if (MyDevice.clientConnectionItems.Count > quickAddrList.Count)
                            {
                                MessageBox.Show("能分配IP的扳手数量超过快捷连接设备数量，防止通讯混乱，请重新设置！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                            else
                            {
                                //含有绑定过ip的地址纪录，直接调用该端口
                                if (MyDevice.addr_ip.ContainsKey(Convert.ToByte(buttonX.Text).ToString()))
                                {
                                    MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[Convert.ToByte(buttonX.Text).ToString()]];
                                }
                                else
                                {
                                    //没有ip纪录，从头开始匹配
                                    MyDevice.protocol.port = MyDevice.clientConnectionItems.Values.ElementAt(portID);
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("IP分配失败，扳手关机或者故障，请重新设置！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        targetConCnt = 1;

                        await SendCommandAsync(new List<byte> { Convert.ToByte(buttonX.Text) });
                    }
                    break;
                default:
                    break;
            }
        }

        //定时器——用于专项TCP多设备连接
        private void timer1_Tick(object sender, EventArgs e)
        {
            //切换Ip，每台连接的设备分配的ip不一致
            if (MyDevice.clientConnectionItems.Count != 0)
            {
                MyDevice.protocol.port = MyDevice.clientConnectionItems.Values.ElementAt(portID);
            }

            if ((MyDevice.protocol.addr) != 0)
            {
                //清除串口任务
                //扫描-先发送REG_BLOCK1_DEV,
                //没回复直接扫描下一个站点,
                //回复了继续发送剩余的指令,发完读取指令，继续下一个站点
                MyDevice.protocol.Protocol_ClearState();

                switch (meTask)
                {
                    case TASKS.NULL:
                        MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK1_DEV);
                        meTask = TASKS.REG_BLOCK1_DEV;
                        break;

                    case TASKS.REG_BLOCK1_DEV:
                        if (MyDevice.protocol.isEqual)
                        {
                            MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK4_CAL);
                            meTask = TASKS.REG_BLOCK4_CAL;
                        }
                        else
                        {
                            //获取当前读取设备位于快捷连接设备集合的下标
                            int curIndex = quickAddrList.IndexOf(MyDevice.protocol.addr);
                            if (curIndex >= 0 && curIndex <= quickAddrList.Count - 1)
                            {
                                MyDevice.protocol.addr = quickAddrList[(curIndex + 1) % quickAddrList.Count];
                            }

                            //校验未通过补发
                            MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK1_DEV);
                            meTask = TASKS.REG_BLOCK1_DEV;
                        }
                        break;

                    case TASKS.REG_BLOCK4_CAL:
                        if (MyDevice.protocol.isEqual)
                        {
                            MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK5_INFO);
                            meTask = TASKS.REG_BLOCK5_INFO;
                        }
                        else
                        {
                            MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK4_CAL);
                            meTask = TASKS.REG_BLOCK4_CAL;
                        }
                        break;

                    case TASKS.REG_BLOCK5_INFO:
                        if (MyDevice.protocol.isEqual)
                        {
                            MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_WLAN);
                            meTask = TASKS.REG_BLOCK3_WLAN;
                        }
                        else
                        {
                            MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK5_INFO);
                            meTask = TASKS.REG_BLOCK5_INFO;
                        }
                        break;

                    case TASKS.REG_BLOCK3_WLAN:
                        if (MyDevice.protocol.isEqual)
                        {
                            MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK1_ID);
                            meTask = TASKS.REG_BLOCK1_ID;
                        }
                        else
                        {
                            MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_WLAN);
                            meTask = TASKS.REG_BLOCK3_WLAN;
                        }
                        break;

                    case TASKS.REG_BLOCK1_ID:
                        if (MyDevice.protocol.isEqual)
                        {
                            MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK2_PARA);
                            meTask = TASKS.REG_BLOCK2_PARA;
                        }
                        else
                        {
                            MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK1_ID);
                            meTask = TASKS.REG_BLOCK1_ID;
                        }
                        break;

                    case TASKS.REG_BLOCK2_PARA:
                        if (MyDevice.protocol.isEqual)
                        {
                            MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK5_AM1);
                            meTask = TASKS.REG_BLOCK5_AM1;
                        }
                        else
                        {
                            MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK2_PARA);
                            meTask = TASKS.REG_BLOCK2_PARA;
                        }
                        break;

                    case TASKS.REG_BLOCK5_AM1:
                        if (MyDevice.protocol.isEqual)
                        {
                            MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK5_AM2);
                            meTask = TASKS.REG_BLOCK5_AM2;
                        }
                        else
                        {
                            MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK5_AM1);
                            meTask = TASKS.REG_BLOCK5_AM1;
                        }
                        break;

                    case TASKS.REG_BLOCK5_AM2:
                        if (MyDevice.protocol.isEqual)
                        {
                            MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK5_AM3);
                            meTask = TASKS.REG_BLOCK5_AM3;
                        }
                        else
                        {
                            MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK5_AM2);
                            meTask = TASKS.REG_BLOCK5_AM2;
                        }
                        break;

                    case TASKS.REG_BLOCK5_AM3:
                        if (MyDevice.protocol.isEqual)
                        {
                            MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_JOB);
                            meTask = TASKS.REG_BLOCK3_JOB;
                        }
                        else
                        {
                            MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK5_AM3);
                            meTask = TASKS.REG_BLOCK5_AM3;
                        }
                        break;

                    case TASKS.REG_BLOCK3_JOB:
                        if (MyDevice.protocol.isEqual)
                        {
                            MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_OP);
                            meTask = TASKS.REG_BLOCK3_OP;
                        }
                        else
                        {
                            MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_JOB);
                            meTask = TASKS.REG_BLOCK3_JOB;
                        }
                        break;

                    case TASKS.REG_BLOCK3_OP:
                        if (MyDevice.protocol.isEqual)
                        {
                            MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK1_HEART);
                            meTask = TASKS.REG_BLOCK1_HEART;
                        }
                        else
                        {
                            MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_OP);
                            meTask = TASKS.REG_BLOCK3_OP;
                        }
                        break;

                    case TASKS.REG_BLOCK1_HEART:
                        if (MyDevice.protocol.isEqual)
                        {
                            MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK1_FIFO);
                            meTask = TASKS.REG_BLOCK1_FIFO;
                        }
                        else
                        {
                            MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK1_HEART);
                            meTask = TASKS.REG_BLOCK1_HEART;
                        }
                        break;

                    case TASKS.REG_BLOCK1_FIFO:
                        if (MyDevice.protocol.isEqual)
                        {
                            MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK2_DAT);
                            meTask = TASKS.REG_BLOCK2_DAT;
                        }
                        else
                        {
                            MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK1_FIFO);
                            meTask = TASKS.REG_BLOCK1_FIFO;
                        }
                        break;

                    case TASKS.REG_BLOCK2_DAT:
                        if (MyDevice.protocol.isEqual)
                        {
                            meTask = TASKS.NULL;
                        }
                        else
                        {
                            MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK2_DAT);
                            meTask = TASKS.REG_BLOCK2_DAT;
                        }
                        break;

                    default:
                        break;
                }

                Console.WriteLine(MyDevice.protocol.addr + "==" + meTask);
            }
            //扫描结束
            else
            {
                timer1.Enabled = false;//关闭扫描

                meTask = TASKS.NULL;

            }
        }

        //页面控件权限
        private void Enable_Menu(bool enabled)
        {
            foreach (Control control in this.Controls)
            {
                control.Enabled = enabled;
            }
        }

        //是否能执行快捷连接
        private Boolean IsQuickConnect()
        {
            byte quickAddr = 1;//快捷连接扳手地址

            //读取扳手汇总表
            wrenchDevcList = JDBC.GetAllWrenchDevc();
            //获取扳手允许快捷连接的表
            if (wrenchDevcList.Count > 0)
            {
                foreach (var itemWrench in wrenchDevcList)
                {
                    //站点更新
                    targetWrenchWlan = JDBC.GetWrenchWlanByWlanId(itemWrench.WlanId);
                    quickAddr = targetWrenchWlan.Addr;

                    //确定扳手是否允许快捷连接
                    if (itemWrench.ConnectAuto == "True")
                    {
                        quickAddrList.Add(quickAddr);
                        quickConnectTypeList.Add(itemWrench.ConnectType);
                        quickWrenchList.Add(itemWrench);

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
                        if (quickAddrList.Distinct().Count() != quickAddrList.Count)
                        {
                            MessageBox.Show($"设置快捷连接的扳手系列中站点为{quickAddr}有多把，存在冲突，无法执行快捷连接！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return false;
                        }

                        //加入新扳手站点后分析是否存在连接方式不统一
                        if (quickConnectTypeList.Distinct().Count() != 1)
                        {
                            MessageBox.Show($"设置快捷连接的扳手系列中连接方式有多种，存在冲突，无法执行快捷连接！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return false;
                        }
                        //连接方式统一
                        else
                        {
                            MyDevice.ConnectType = quickConnectTypeList[0];

                            //考虑有线和蓝牙只能 1对1，故多个设备快捷连接无法实现
                            if (quickConnectTypeList.Count > 1 && (quickConnectTypeList[0] == "有线连接" || quickConnectTypeList[0] == "蓝牙连接"))
                            {
                                MessageBox.Show($"{quickConnectTypeList[0]}只能快捷连接一台设备，有多台设备设置成{quickConnectTypeList[0]}存在冲突，请重新设置！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return false;
                            }

                            //连接方式统一，需要判断多设备连接的信息是否统一
                            if (quickConnectTypeList.Count > 1 && quickConnectTypeList[0] == "RS485连接")
                            {
                                //检查多设备的所需串口信息
                                if (CompareSerialPortInfo(SerialPortInfo) == false) return false;
                            }
                            else if (quickConnectTypeList.Count > 1 && (quickConnectTypeList[0] == "接收器连接" || quickConnectTypeList[0] == "路由器WiFi连接"))
                            {
                                //检查多设备的所需串口信息和WiFi信息
                                if (CompareSerialPortInfo(SerialPortInfo) == false) return false;
                                if (CompareWiFiInfo(WiFiInfo) == false) return false;
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("软件设置允许扳手快捷连接的数量为 0，请重新设置！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        //串口/端口初始化
        private void InitPort()
        {
            //刷串口
            comboBox0_port.Items.Clear();
            
            if (MyDevice.ConnectType != "路由器WiFi连接")
            {
                comboBox0_port.Items.AddRange(SerialPort.GetPortNames());

                //无串口
                if (comboBox0_port.Items.Count == 0)
                {
                    comboBox0_port.Text = null;
                }
                //有可用串口
                else
                {
                    //
                    if (comboBox0_port.SelectedIndex < 0)
                    {
                        comboBox0_port.SelectedIndex = 0;
                    }
                }
            }
            else
            {
                comboBox0_port.Items.AddRange(MyDevice.GetIPList().ToArray());

                //无串口
                if (comboBox0_port.Items.Count == 0)
                {
                    comboBox0_port.Text = null;
                }
                //有可用串口
                else
                {
                    //
                    if (comboBox0_port.SelectedIndex < 0)
                    {
                        comboBox0_port.SelectedIndex = 0;
                    }
                }
            }

            label1.Text = MyDevice.ConnectType.ToString();
        }

        //设备按钮初始化
        private void InitBtn()
        {
            foreach (Button btn in groupBox2.Controls)
            {
                btn.Text = "0";
                buttons.Add(btn);//遍历groupBox2按钮是倒序的
                btn.Visible = false;
            }

            //按钮系列初始化
            int btnX = 0;
            if (quickAddrList.Count <= devMax && quickAddrList.Count > 0)
            {
                quickAddrList.Sort();
                foreach (byte addr in quickAddrList)
                {
                    buttons[devMax - 1 - btnX].Text = addr.ToString();
                    buttons[devMax - 1 - btnX].Visible = true;

                    buttons[devMax - 1 - btnX].Click += new EventHandler(BtnX_Click);//按钮添加事件
                    btnX++;
                }
            }
            else
            {
                if (quickAddrList.Count == 0)
                {
                    MessageBox.Show($"设置的快捷连接设备数为 0，无法快捷连接，请重新设置！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show($"设置的快捷连接设备数超过了最大连接设备数{devMax}，请重新设置！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return;
            }
        }

        //初始化设备
        private async void InitDev()
        {
            if (comboBox0_port.Text == "")
            {
                MessageBox.Show("无有效串口，请检测终端设备");
            }

            //判断
            if (quickAddrList.Count > 0)
            {
                //先排除多串口无法识别正确串口的问题
                //if (quickConnectTypeList[0] != "路由器WiFi连接" && SerialPort.GetPortNames().Count() > 1)
                //{
                //    MessageBox.Show("请保证有且只有一个串口被打开，否则快捷连接无法识别正确的串口进行通讯，请重新设置！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    return;
                //}

                //label1.Text = quickConnectTypeList[0].ToString();

                //轮询发送试探指令
                switch (quickConnectTypeList[0])
                {
                    case "有线连接":
                        //切换通讯
                        MyDevice.mePort_SetProtocol(COMP.UART);
                        MyDevice.ConnectType = "有线连接";

                        //打开串口
                        MyDevice.protocol.Protocol_PortOpen(comboBox0_port.Text, 115200, StopBits.One, Parity.None);

                        //串口有效
                        if (MyDevice.myUART.IsOpen)
                        {
                            targetConCnt = quickAddrList.Count;
                            //开始连接指令
                            await SendCommandAsync(quickAddrList);
                        }
                        break;
                    case "RS485连接":
                        //切换通讯
                        MyDevice.mePort_SetProtocol(COMP.RS485);
                        MyDevice.ConnectType = "RS485连接";

                        //打开串口
                        MyDevice.protocol.Protocol_PortOpen(comboBox0_port.Text, MyDevice.GetBaud(targetWrenchWlan.Baud), (StopBits)targetWrenchWlan.Stopbit, (Parity)targetWrenchWlan.Parity);
                        //串口有效
                        if (MyDevice.myRS485.IsOpen)
                        {
                            targetConCnt = quickAddrList.Count;
                            //开始连接指令
                            await SendCommandAsync(quickAddrList);
                        }
                        break;
                    case "蓝牙连接":
                    case "接收器连接":
                        //切换通讯
                        MyDevice.mePort_SetProtocol(COMP.XF);
                        MyDevice.ConnectType = quickConnectTypeList[0];

                        //打开串口
                        MyDevice.protocol.Protocol_PortOpen(comboBox0_port.Text, 115200, StopBits.One, Parity.None);
                        //串口有效
                        if (MyDevice.myXFUART.IsOpen)
                        {
                            targetConCnt = quickAddrList.Count;
                            //开始连接指令
                            await SendCommandAsync(quickAddrList);
                        }
                        break;
                    case "路由器WiFi连接":
                        List<string> ipList = new List<string>();
                        ipList = MyDevice.GetIPList();

                        string targetIp = null;//进行连接之前已经统一了ip相关信息,即WiFiInfo[0].Item3
                        for (int i = 0; i < WiFiInfo[0].Item3.Length; i += 2)
                        {
                            int num = Convert.ToInt32($"{WiFiInfo[0].Item3[i]}{WiFiInfo[0].Item3[i + 1]}", 16);
                            targetIp = i == 0 ? num.ToString() : targetIp + "." + num.ToString();
                        }
                        //通常一台电脑联网状态只会分配一个IP地址,不联网的状态下IP为"127.0.0.1"
                        //局限性：特殊情况下多个IP地址
                        if (ipList == null || ipList.Count == 0 || ipList[0] == "127.0.0.1" || !ipList.Contains(targetIp))
                        {
                            MessageBox.Show("快捷连接的WIFI端口未能找到，请重新设置！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        //切换通讯
                        MyDevice.mePort_SetProtocol(COMP.TCP);
                        MyDevice.ConnectType = "路由器WiFi连接";

                        if (targetIp != comboBox0_port.Text)
                        {
                            MessageBox.Show($"请选择正确的端口{targetIp}", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        //打开端口
                        MyDevice.protocol.Protocol_PortOpen(targetIp, 5678, StopBits.One, Parity.None);
                        //串口有效
                        if (MyDevice.myTCPUART.IsOpen)
                        {
                            Thread.Sleep(2000);//服务端给客户端分配ip时需要时间
                            if (MyDevice.clientConnectionItems.Count != 0)
                            {
                                if (MyDevice.clientConnectionItems.Count > quickAddrList.Count)
                                {
                                    MessageBox.Show("能分配IP的扳手数量超过快捷连接设备数量，防止通讯混乱，请重新设置！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }
                                else
                                {
                                    //含有绑定过ip的地址纪录，直接调用该端口
                                    if (MyDevice.addr_ip.ContainsKey(Convert.ToByte(buttons[buttons.Count - 1].Text).ToString()))
                                    {
                                        MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[Convert.ToByte(buttons[buttons.Count - 1].Text).ToString()]];
                                    }
                                    else
                                    {
                                        //没有ip纪录，从头开始匹配
                                        MyDevice.protocol.port = MyDevice.clientConnectionItems.Values.ElementAt(portID);
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show("IP分配失败，扳手关机或者故障，请重新设置！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            targetConCnt = quickAddrList.Count;

                            if (targetConCnt == 1)
                            {
                                await SendCommandAsync(quickAddrList);
                            }
                            else
                            {
                                //TCP多设备连接情况特殊，启动定时器
                                meTask = TASKS.NULL;
                                MyDevice.protocol.addr = quickAddrList[0];
                                timer1.Interval = 300;
                                timer1.Enabled = true;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                MessageBox.Show($"扳手库中没有扳手设置成快捷连接模式，无法执行快捷连接！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
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
                        MessageBox.Show($"扳手库中多台设备的波特率，校验位，停止位不统一，无法执行快捷连接！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                        MessageBox.Show($"扳手库中多台设备的WiFi相关参数不统一，无法执行快捷连接！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                }
            }

            return true;
        }

        //发送指令
        private async Task SendCommandAsync(List<byte> addrList)
        {
            //开始连接指令
            foreach (var item in addrList)
            {
                bool isDisConnect = false;//设备是否掉线
                MyDevice.protocol.addr = (byte)(MyDevice.ConnectType == "有线连接" ? 1 : item);//同步更新发送指令设备的地址
                meTask = TASKS.REG_BLOCK1_DEV;
                Dictionary<TASKS, int> taskDic = new Dictionary<TASKS, int>();//用于纪录指令发送次数

                //利用多线程，防止堵塞主线程,利用await异步，防止task之后的if语句没有等task执行就运行了
                await Task.Run(async () =>
                {
                    while (!_cts.Token.IsCancellationRequested && meTask != TASKS.NULL && !isDisConnect)
                    {
                        if (MyDevice.ConnectType == "有线连接")
                        {
                            //不使用Task.Delay().Wait()是Task.Run 内部的代码仍然是同步防止阻塞线程
                            await Task.Delay(100);//会释放当前线程，允许它执行其他工作
                        }
                        else
                        {
                            await Task.Delay(300);
                        }

                        MyDevice.protocol.Protocol_mePort_ReadAllTasks();
                        meTask = MyDevice.protocol.trTASK;

                        Console.WriteLine(MyDevice.protocol.addr + "==" + meTask);

                        if (!taskDic.ContainsKey(meTask))
                        {
                            taskDic.Add(meTask, 1);//每条指令分配一个纪录发送次数
                        }

                        taskDic[meTask]++;
                        if (taskDic[meTask] > sendMax)
                        {
                            isDisConnect = true;

                            //非主线程调用UI控件，必须使用BeginInvoke/Invoke
                            if (label2.InvokeRequired)
                            {
                                label2.BeginInvoke(new MethodInvoker(() => {
                                    label2.Visible = true;
                                    label2.Text = $"设备{MyDevice.protocol.addr} 连接异常，请检查设备，重新连接";
                                }));
                            }
                            
                            //多设备情况下，用户使用选择指定设备进行单连接
                            if (isAloneConnect)
                            {
                                //如果选择的客户端地址不对，无法回复，切换另外一个ip重新发送连接指令
                                if (portID < MyDevice.clientConnectionItems.Count - 1)
                                {
                                    portID++;
                                    MyDevice.protocol.port = MyDevice.clientConnectionItems.Values.ElementAt(portID);
                                    await SendCommandAsync(addrList);
                                }
                            }

                            break;//发送指令超过上限次数，判断当前设备掉线，退出循环 
                        }
                    }
                });

                //当前设备掉线，切换下一台设备
                if (isDisConnect)
                {
                    continue;
                }
            }
        }
   
        //委托
        private async void receiveData()
        {
            //其它线程的操作请求
            if (this.InvokeRequired)
            {
                try
                {
                    freshHandler meDelegate = new freshHandler(receiveData);
                    this.Invoke(meDelegate, new object[] { });
                }
                catch
                {
                    //MessageBox.Show("MenuConnectForm receiveData err 1");
                }
            }
            //本线程的操作请求
            else
            {
                switch (meTask)
                {
                    case TASKS.REG_BLOCK1_DEV:
                        isConnecting = true;//正在连接，禁止关闭页面
                        break;
                    case TASKS.REG_BLOCK4_CAL:
                        break;
                    case TASKS.REG_BLOCK5_INFO:
                        break;
                    case TASKS.REG_BLOCK3_WLAN:
                        break;
                    case TASKS.REG_BLOCK1_ID:
                        break;
                    case TASKS.REG_BLOCK2_PARA:
                        break;
                    case TASKS.REG_BLOCK5_AM1:
                        break;
                    case TASKS.REG_BLOCK5_AM2:
                        break;
                    case TASKS.REG_BLOCK5_AM3:
                        break;
                    case TASKS.REG_BLOCK3_JOB:
                        break;
                    case TASKS.REG_BLOCK3_OP:
                        break;
                    case TASKS.REG_BLOCK1_HEART:
                        break;
                    case TASKS.REG_BLOCK1_FIFO:
                        break;
                    case TASKS.REG_BLOCK2_DAT:
                        //成功连接，更新连接扳手信息(先更新数据库，防止多设备连接时当前设备地址成功后还未存入就递增扫描了，存入的是扫描地址)
                        //updateDatabaseWrench();

                        isConnecting = false;

                        //USB
                        if (MyDevice.protocol.type == COMP.UART)
                        {
                            //isConnecting = false;
                            meTask = TASKS.NULL;
                            buttons[devMax - 1].BackColor = Color.Green;
                            connectCnt++;
                        }
                        //RS485
                        else if (MyDevice.protocol.type == COMP.RS485)
                        {
                            meTask = TASKS.NULL;

                            foreach (var btn in buttons)
                            {
                                if (btn.Text == MyDevice.protocol.addr.ToString())
                                {
                                    btn.BackColor = Color.Green;
                                    connectCnt++;
                                }
                            }
                        }
                        //接收器
                        else if (MyDevice.protocol.type == COMP.XF)
                        {
                            meTask = TASKS.NULL;

                            foreach (var btn in buttons)
                            {
                                if (btn.Text == MyDevice.protocol.addr.ToString())
                                {
                                    btn.BackColor = Color.Green;
                                    connectCnt++;
                                }
                            }
                        }
                        else
                        {
                            meTask = TASKS.NULL;
                            foreach (var btn in buttons)
                            {
                                if (btn.Text == MyDevice.protocol.addr.ToString())
                                {
                                    btn.BackColor = Color.Green;
                                    connectCnt++;
                                }
                            }

                            //设备地址绑定Ip
                            if (MyDevice.addr_ip.ContainsKey(MyDevice.protocol.addr.ToString()) == false)
                            {
                                MyDevice.addr_ip.Add(MyDevice.protocol.addr.ToString(), ((IPEndPoint)((Socket)MyDevice.protocol.port).RemoteEndPoint).Address.ToString());
                            }

                            //切换客户端ip
                            if (portID < MyDevice.clientConnectionItems.Count - 1)
                            {
                                portID++;
                                MyDevice.protocol.port = MyDevice.clientConnectionItems.Values.ElementAt(portID);
                                if (timer1.Enabled == true) MyDevice.protocol.addr = quickAddrList[0];
                            }
                            else
                            {
                                timer1.Enabled = false;

                                string errMessage = "";//异常连接信息
                                foreach (var btn in buttons)
                                {
                                    if (btn.Visible == true && btn.BackColor != Color.Green)
                                    {
                                        errMessage += btn.Text + "，";//存储异常设备地址
                                    }
                                }
                                if (errMessage != "")
                                {
                                    label2.Text = $"设备{errMessage} 连接异常，请检查设备，重新连接";
                                }
                            }
                        }

                        //比较实际连接设备数量与预取设备数量
                        if (connectCnt == targetConCnt)
                        {
                            label2.Text = "目标设备已连接成功";
                            timer1.Enabled = false;
                        }

                        break;
                    default:
                        break;
                }
            }
        }

    }
}
