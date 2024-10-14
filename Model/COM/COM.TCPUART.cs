using Base;
using DBHelper;
using Library;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Model
{
    public class TCPUARTProtocol : IProtocol
    {
        //C# Socket异步通信参考 https://www.cnblogs.com/llllll/archive/2009/05/13/1455703.html

        //
        private Byte sAddress = 1;                            //访问mTCP[256]的指针

        //
        private volatile bool is_serial_listening = false;    //串口正在监听标记
        private volatile bool is_serial_closing = false;      //串口正在关闭标记

        //
        private Socket ListenerSocket;                        //服务端Socket
        private Socket ClientSocket;                          //客户端Socket
        private String clientMessage;                         //客户端信息
        private volatile TASKS trTSK = TASKS.NULL;            //通讯状态机
        private Boolean isConnect = false;                    //判断tcp的连接状态

        //
        private Byte[] meTXD = new Byte[Constants.TxSize];    //发送缓冲区
        private Byte[] meRXD = new Byte[Constants.RxSize];    //接收缓冲区
        private Byte[] meCRC = new Byte[200];                 //解码数组
        private UInt16 txCnt = 0;                             //发送计数
        private UInt16 rxRead = 0;                            //接收读指针
        private UInt16 rxRnt = 0;                             //接收字节数,需要处理的字节数
        private UInt16 rxWrite = 0;                           //接收写指针
        private UInt16 rxWnt = 0;                             //接收计数,统计字节数
        private bool isEQ = false;                            //接收校验结果

        private String rxStr = null;
        private UInt32 tempStamp = 0;                         //程序F39版本及以上F3数据帧的stamp删除了，故需要通过继承上一个F2继承
        private Byte angleDecimal = 0;                        //角度小数点（用于计算复拧角度）
        private int f2Num = 0;
        private object lockObj = new object();

        public String rxString
        {
            get
            {
                return rxStr;
            }
        }

        public COMP type
        {
            get
            {
                return COMP.TCP;
            }
        }

        public Byte addr
        {
            set
            {
                sAddress = value;
                MyDevice.mTCP[sAddress].wlan.addr = value;
            }
            get
            {
                return MyDevice.mTCP[sAddress].wlan.addr;
            }
        }

        public bool Is_serial_listening
        {
            set
            {
                is_serial_listening = value;
            }
            get
            {
                return is_serial_listening;
            }
        }
        public bool Is_serial_closing
        {
            set
            {
                is_serial_closing = value;
            }
            get
            {
                return is_serial_closing;
            }
        }
        public Object port
        {
            set
            {
                ClientSocket = (Socket)value;
            }
            get
            {
                return ClientSocket;
            }
        }
        public String portName
        {
            set
            {
                clientMessage = value;
            }
            get
            {
                return clientMessage;
            }
        }
        public Boolean IsOpen
        {
            get
            {
                return isConnect;
            }
        }
        public TASKS trTASK
        {
            set
            {
                trTSK = value;
            }
            get
            {
                return trTSK;
            }
        }
        public Int32 txCount
        {
            get
            {
                return txCnt;
            }
        }
        public Int32 rxCount
        {
            get
            {
                return rxWnt;
            }
        }
        public Int32 rxReadCnt
        {
            get
            {
                return rxRnt;
            }
        }
        public Boolean isEqual
        {
            get
            {
                return isEQ;
            }
        }

        // 容器的结构类型为：Code
        //容器至少为一个ListenerSocket类型。
        public class StateObject
        {
            // Client ClientSocket.
            public Socket workSocket = null;
            // Size of receive buffer.
            public const int BufferSize = 22000;
            // Receive buffer.
            public byte[] buffer = new byte[BufferSize];
            // Received data string.
            public StringBuilder sb = new StringBuilder();
        }

        // 发送回调
        private static void SendCallback(IAsyncResult ar)
        {
            //发送完毕，bytesSent发送字节数。
            Socket handler = (Socket)ar.AsyncState;
            int bytesSent = handler.EndSend(ar);
        }

        // 连接回调
        public void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                Socket listener = ar.AsyncState as Socket;
                if (listener != null)
                {
                    //完成连接，返回此时的ListenerSocket通道。
                    Socket handler = listener.EndAccept(ar);
                    StateObject state = new StateObject();
                    state.workSocket = handler;
                    IPEndPoint clientipe = (IPEndPoint)handler.RemoteEndPoint;

                    //多设备联网处理
                    if (MyDevice.clientConnectionItems.Count != 0)
                    {
                        List<string> list = new List<string>(MyDevice.clientConnectionItems.Keys);
                        int cnt = MyDevice.clientConnectionItems.Count;
                        for (int i = 0; i < cnt; i++)
                        {
                            //分配的ip地址已有设备使用
                            //出现情况可能：1、设备关机重新打开  2、网络信号异常设备重连

                            //新分配的新客户端Ip 与 之前连接纪录的ip重复(新分配的端口号变了，前四位ip地址不变)
                            if (list[i] == clientipe.Address.ToString())
                            {
                                MyDevice.clientConnectionItems.Remove(list[i]);//删除旧ip
                                MyDevice.clientConnectionItems.Add(clientipe.Address.ToString(), handler);

                                //遍历配对成功的扳手站点和ip集合，进行更新
                                for (int j = 0; j < MyDevice.addr_ip.Count; j++)
                                {
                                    //根据更新后的ip获取当前设备的站点进行更新
                                    if (MyDevice.addr_ip.ElementAt(j).Value == clientipe.Address.ToString())
                                    {
                                        string ipKey = MyDevice.addr_ip.ElementAt(j).Key;
                                        MyDevice.addr_ip.Remove(ipKey);
                                        MyDevice.addr_ip.Add(ipKey, clientipe.Address.ToString());

                                        //用于客户端网络断开后重新连接，需要更新端口号（不加会导致通讯无法回复）
                                        MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[ipKey]];
                                    }
                                }
                            }
                            //分配的客户端ip地址没有设备使用
                            else
                            {
                                if (!MyDevice.clientConnectionItems.Keys.Contains(clientipe.Address.ToString()))
                                {
                                    MyDevice.clientConnectionItems.Add(clientipe.Address.ToString(), handler);
                                }
                            }
                        }
                    }
                    //单设备联网
                    else
                    {
                        MyDevice.clientConnectionItems.Add(clientipe.Address.ToString(), handler);
                    }

                    //接收的字节，0，字节长度，0，接收时调用的回调函数，接收行为的容器。
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(RevCallback), state);
                }
                if (listener != null)
                {
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                }
            }
            catch (Exception ex)
            {
            }
        }

        // 接收回调
        public void RevCallback(IAsyncResult ar)
        {
            // 获取客户请求的ListenerSocket
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            if (handler != null)
            {
                try
                {
                    IPEndPoint clientipe = (IPEndPoint)handler.RemoteEndPoint;
                    // Read data from the client ClientSocket.
                    //完成一次连接。数据存储在state.buffer里，bytesRead为读取的长度。
                    int bytesRead = handler.EndReceive(ar);

                    Console.WriteLine("接收" + bytesRead +"++++" + MyDevice.protocol.trTASK + DateTime.Now.ToString("HH:mm:ss:fff"));

                    if (bytesRead > 0)
                    {
                        byte[] a = new byte[bytesRead];
                        Buffer.BlockCopy(state.buffer, 0, a, 0, bytesRead);
                        for (int i = 0; i < bytesRead; i++)
                        {
                            meRXD[rxWrite] = a[i];

                            //指针和计数器更新
                            rxWrite++;
                            rxWnt++;
                            rxRnt++;
                            if (rxWrite >= Constants.RxSize)
                            {
                                rxWrite = 0;
                            }

                            //接收函数
                            //mePort_DataReceived();
                        }

                        Monitor.Enter(lockObj);
                        try
                        {
                            // 临界区代码

                            //接收函数
                            mePort_DataReceived();
                        }
                        finally
                        {
                            Monitor.Exit(lockObj);
                        }

                        //粘包字节未处理干净(回复的包是上一个不同指令的回复)
                        if (rxRnt != 0)
                        {
                            Thread.Sleep(50);
                            if (rxRnt >= 8)
                            {
                                lock (lockObj)
                                {
                                    while (rxRnt >= 8)
                                    {
                                        Console.WriteLine("处理粘包");
                                        mePort_DataReceived();
                                        Thread.Sleep(50);
                                    }
                                }
                            }
                            mePort_DataRemove(rxRnt);
                            Console.WriteLine("粘包处理成功！！！！！！");
                        }

                        // 发送数据byteData，回调函数SendCallback。容器handler
                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(RevCallback), state);
                    }
                }
                catch (Exception ex)
                {
                    if (ex is ObjectDisposedException)
                    {
                        isConnect = false;
                        if (MyDevice.clientConnectionItems.Count > 0)
                        {
                            MyDevice.clientConnectionItems.Clear();
                        }
                        //关闭连接
                        if (handler.Connected)
                        {
                            handler.Shutdown(SocketShutdown.Both);
                            handler.Close();
                        }
                    }
                }
            }

        }

        //打开端口
        public void Protocol_PortOpen(string ip, Int32 port = 5678, StopBits stb = StopBits.One, Parity pay = Parity.None)
        {
            if (!isConnect)
            {
                //生成一个服务端的socket
                ListenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                ListenerSocket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
                ListenerSocket.Listen(10);//监听
                isConnect = true;//更改tcp的连接状态
            }
            if (MyDevice.clientConnectionItems.Count == 0)
            {
                //一旦连接上后的回调函数为AcceptCallback。当系统调用这个函数时，自动赋予的输入参数为IAsyncResoult类型变量ar。
                ListenerSocket.BeginAccept(new AsyncCallback(AcceptCallback), ListenerSocket);
            }

            //
            trTSK = TASKS.NULL;
            txCnt = 0;
            rxWnt = 0;
            rxRnt = 0;
            isEQ = false;
        }

        //关闭串口
        public bool Protocol_PortClose()
        {
            if (isConnect)
            {
                try
                {
                    foreach (Socket sc in MyDevice.clientConnectionItems.Values)
                    {
                        sc.Shutdown(SocketShutdown.Both);
                        sc.Close();
                    }
                }
                catch { }
                try
                {
                    if (ListenerSocket != null)
                    {
                        ListenerSocket.Close();

                        isConnect = false;
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        //断开异常连接
        private void closeConnection()
        {
            //关闭连接
            if (ClientSocket.Connected)
            {
                ClientSocket.Shutdown(SocketShutdown.Both);
                ClientSocket.Close();
            }
        }


        //清除串口任务
        public void Protocol_ClearState()
        {
            trTASK = TASKS.NULL;

            rxRead = 0;
            rxWrite = 0;
            rxRnt = 0;
            rxWnt = 0;
            txCnt = 0;

            if (isConnect)
            {
                List<Socket> readList = new List<Socket>();
                readList.Add(ListenerSocket);
                Socket.Select(readList, null, null, 0); //检测可读状态
                if (readList.Count > 0) //如果有可读的 ListenerSocket
                {
                    int bytesToRead = ListenerSocket.Available;
                    byte[] buffer = new byte[bytesToRead];
                    ListenerSocket.Receive(buffer, bytesToRead, SocketFlags.None); //读取并清空接收缓冲区
                }
            }

            Array.Clear(meTXD, 0, meTXD.Length);
            Array.Clear(meRXD, 0, meRXD.Length);
        }

        //刷新IsEQ
        public void Protocol_ChangeEQ()
        {
            isEQ = false;
        }

        //发送读指令
        //读指令格式如下：
        // 1Byte       1Bytes      2Bytes      2Bytes       2Bytes
        // ADDR        FCODE       REG         NUM          MODBUS
        // 设备地址    功能码03    寄存器地址  寄存器个数   XL XH
        public void Protocol_Read_SendCOM(TASKS meTask)
        {
            //
            if (ClientSocket == null) return;
            Array.Clear(meTXD, 0, meTXD.Length);

            //读指令不包含校验位的长度固定6
            switch (meTask)
            {
                case TASKS.REG_BLOCK1_DEV:
                    (new Byte[] { sAddress, (byte)CMD.CMD_READ, Constants.REG_BLOCK1_DEV >> 8, Constants.REG_BLOCK1_DEV & 0xFF, 0x00, 0x10 }).CopyTo(meTXD, 0);
                    break;

                case TASKS.REG_BLOCK4_CAL1:
                    (new Byte[] { sAddress, (byte)CMD.CMD_READ, Constants.REG_BLOCK4_CAL1 >> 8, Constants.REG_BLOCK4_CAL1 & 0xFF, 0x00, 0x40 }).CopyTo(meTXD, 0);
                    break;

                case TASKS.REG_BLOCK5_CAL2:
                    (new Byte[] { sAddress, (byte)CMD.CMD_READ, Constants.REG_BLOCK5_CAL2 >> 8, Constants.REG_BLOCK5_CAL2 & 0xFF, 0x00, 0x50 }).CopyTo(meTXD, 0);
                    break;

                case TASKS.REG_BLOCK5_INFO:
                    (new Byte[] { sAddress, (byte)CMD.CMD_READ, Constants.REG_BLOCK5_INFO >> 8, Constants.REG_BLOCK5_INFO & 0xFF, 0x00, 0x50 }).CopyTo(meTXD, 0);
                    break;

                case TASKS.REG_BLOCK3_WLAN:
                    (new Byte[] { sAddress, (byte)CMD.CMD_READ, Constants.REG_BLOCK4_WLAN >> 8, Constants.REG_BLOCK4_WLAN & 0xFF, 0x00, 0x30 }).CopyTo(meTXD, 0);
                    break;

                case TASKS.REG_BLOCK3_PARA:
                    (new Byte[] { sAddress, (byte)CMD.CMD_READ, Constants.REG_BLOCK2_PARA >> 8, Constants.REG_BLOCK2_PARA & 0xFF, 0x00, 0x30 }).CopyTo(meTXD, 0);
                    break;

                case TASKS.REG_BLOCK5_AM1:
                    (new Byte[] { sAddress, (byte)CMD.CMD_READ, Constants.REG_BLOCK5_AM1 >> 8, Constants.REG_BLOCK5_AM1 & 0xFF, 0x00, 0x50 }).CopyTo(meTXD, 0);
                    break;

                case TASKS.REG_BLOCK5_AM2:
                    (new Byte[] { sAddress, (byte)CMD.CMD_READ, Constants.REG_BLOCK5_AM2 >> 8, Constants.REG_BLOCK5_AM2 & 0xFF, 0x00, 0x50 }).CopyTo(meTXD, 0);
                    break;

                case TASKS.REG_BLOCK5_AM3:
                    (new Byte[] { sAddress, (byte)CMD.CMD_READ, Constants.REG_BLOCK5_AM3 >> 8, Constants.REG_BLOCK5_AM3 & 0xFF, 0x00, 0x50 }).CopyTo(meTXD, 0);
                    break;

                case TASKS.REG_BLOCK3_JOB:
                    (new Byte[] { sAddress, (byte)CMD.CMD_READ, Constants.REG_BLOCK3_JOB >> 8, Constants.REG_BLOCK3_JOB & 0xFF, 0x00, 0x30 }).CopyTo(meTXD, 0);
                    break;

                case TASKS.REG_BLOCK3_OP:
                    (new Byte[] { sAddress, (byte)CMD.CMD_READ, Constants.REG_BLOCK3_OP >> 8, Constants.REG_BLOCK3_OP & 0xFF, 0x00, 0x30 }).CopyTo(meTXD, 0);
                    break;

                case TASKS.REG_BLOCK1_FIFO:
                    (new Byte[] { sAddress, (byte)CMD.CMD_READ, Constants.REG_BLOCK1_FIFO >> 8, Constants.REG_BLOCK1_FIFO & 0xFF, 0x00, 0x10 }).CopyTo(meTXD, 0);
                    break;

                case TASKS.REG_BLOCK2_DAT://读dat一次性读5包 —— 0x48
                    if ((MyDevice.mTCP[sAddress].devc.type == TYPE.TQ_XH_XL01_08 - (UInt16)ADDROFFSET.TQ_XH_ADDR && MyDevice.mTCP[sAddress].devc.version >= 11) ||
                        (MyDevice.mTCP[sAddress].devc.type == TYPE.TQ_XH_XL01_07 - (UInt16)ADDROFFSET.TQ_XH_ADDR && MyDevice.mTCP[sAddress].devc.version >= 41))
                    {
                        (new Byte[] { sAddress, (byte)CMD.CMD_READ, Constants.REG_BLOCK2_DAT >> 8, Constants.REG_BLOCK2_DAT & 0xFF, 0x00, 0x52 }).CopyTo(meTXD, 0);
                    }
                    else
                    {
                        (new Byte[] { sAddress, (byte)CMD.CMD_READ, Constants.REG_BLOCK2_DAT >> 8, Constants.REG_BLOCK2_DAT & 0xFF, 0x00, 0x48 }).CopyTo(meTXD, 0);
                    }
                    break;

                case TASKS.REG_BLOCK3_SCREW1:
                    (new Byte[] { sAddress, (byte)CMD.CMD_READ, Constants.REG_BLOCK3_SCREW1 >> 8, Constants.REG_BLOCK3_SCREW1 & 0xFF, 0x00, 0x30 }).CopyTo(meTXD, 0);
                    break;

                case TASKS.REG_BLOCK3_SCREW2:
                    (new Byte[] { sAddress, (byte)CMD.CMD_READ, Constants.REG_BLOCK3_SCREW2 >> 8, Constants.REG_BLOCK3_SCREW2 & 0xFF, 0x00, 0x30 }).CopyTo(meTXD, 0);
                    break;

                case TASKS.REG_BLOCK3_SCREW3:
                    (new Byte[] { sAddress, (byte)CMD.CMD_READ, Constants.REG_BLOCK3_SCREW3 >> 8, Constants.REG_BLOCK3_SCREW3 & 0xFF, 0x00, 0x30 }).CopyTo(meTXD, 0);
                    break;

                case TASKS.REG_BLOCK3_SCREW4:
                    (new Byte[] { sAddress, (byte)CMD.CMD_READ, Constants.REG_BLOCK3_SCREW4 >> 8, Constants.REG_BLOCK3_SCREW4 & 0xFF, 0x00, 0x30 }).CopyTo(meTXD, 0);
                    break;

                default:
                    break;
            }

            //
            MODBUS.AP_CRC16_MODBUS(meTXD, 6).CopyTo(meTXD, 6);
            try
            {
                ClientSocket.BeginSend(meTXD, 0, 8, 0, new AsyncCallback(SendCallback), ClientSocket);
            }
            catch
            {
                closeConnection();
            }
            txCnt += 8;
            trTASK = meTask;
            isEQ = false;
        }

        //发送写命令
        //写指令格式如下：
        // 1Byte       1Bytes      2Bytes      nBytes       2Bytes
        // ADDR        FCODE       REG         DATA         MODBUS
        // 设备地址    功能码06    寄存器地址  数据         XL XH
        public void Protocol_Write_SendCOM(TASKS meTask, UInt16 data = 0xFFFF)
        {
            //
            if (ClientSocket == null) return;
            Array.Clear(meTXD, 0, meTXD.Length);

            //
            switch (meTask)
            {
                //读指令不包含校验位的长度固定6
                case TASKS.WRITE_ZERO:
                    (new Byte[] { sAddress, (byte)CMD.CMD_WRITE, (UInt16)REG.REG_W_ZERO >> 8, (UInt16)REG.REG_W_ZERO & 0xFF, 0x00, 0x01 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.WRITE_POWEROFF:
                    (new Byte[] { sAddress, (byte)CMD.CMD_WRITE, (UInt16)REG.REG_W_POWEROFF >> 8, (UInt16)REG.REG_W_POWEROFF & 0xFF, 0x00, 0x01 }).CopyTo(meTXD, 0);
                    break;
                case TASKS.WRITE_KEYLOCK:
                    (new Byte[] { sAddress, (byte)CMD.CMD_WRITE, (UInt16)REG.REG_W_KEYLOCK >> 8, (UInt16)REG.REG_W_KEYLOCK & 0xFF, (byte)(data >> 8), (byte)(data & 0xFF) }).CopyTo(meTXD, 0);
                    break;
                case TASKS.WRITE_MEMABLE:
                    (new Byte[] { sAddress, (byte)CMD.CMD_WRITE, (UInt16)REG.REG_W_MEMABLE >> 8, (UInt16)REG.REG_W_MEMABLE & 0xFF, (byte)(data >> 8), (byte)(data & 0xFF) }).CopyTo(meTXD, 0);
                    break;

                default:
                    break;
            }

            //
            MODBUS.AP_CRC16_MODBUS(meTXD, 6).CopyTo(meTXD, 6);
            try
            {
                ClientSocket.BeginSend(meTXD, 0, 8, 0, new AsyncCallback(SendCallback), ClientSocket);
            }
            catch
            {
                closeConnection();
            }
            txCnt += 8;
            trTASK = meTask;
            isEQ = false;
        }

        //发送连续写指令
        //连续写指令格式如下：
        // 1Byte       1Bytes      2Bytes             2Bytes       1Bytes      nBytes       2Bytes
        // ADDR        FCODE       REG                NUM          LEN         DATA         MODBUS
        // 设备地址    功能码10    起始寄存器地址     寄存器个数   数据长度    数据         XL XH
        public void Protocool_Sequence_SendCOM(TASKS meTask)
        {
            Byte idx = 0;
            Byte num = 0;
            int strLen = 0;
            Byte[] strArr;

            //
            if (ClientSocket == null) return;
            Array.Clear(meTXD, 0, meTXD.Length);

            //
            switch (meTask)
            {
                case TASKS.REG_BLOCK4_CAL1:
                    num = 0x40;//64个寄存器个数
                    meTXD[idx++] = sAddress;
                    meTXD[idx++] = (byte)CMD.CMD_SEQUENCE;
                    meTXD[idx++] = Constants.REG_BLOCK4_CAL1 >> 8;
                    meTXD[idx++] = Constants.REG_BLOCK4_CAL1 & 0xFF;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = num;
                    meTXD[idx++] = (byte)(num * 2);
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = (byte)MyDevice.mTCP[sAddress].devc.calunit;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = (byte)MyDevice.mTCP[sAddress].devc.caltype;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = (byte)MyDevice.mTCP[sAddress].devc.torque_decimal;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = (byte)MyDevice.mTCP[sAddress].devc.torque_fdn;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = (byte)MyDevice.mTCP[sAddress].devc.calIndex;
                    meTXD[idx++] = 0xFF;
                    meTXD[idx++] = 0xFF;
                    meTXD[idx++] = 0xFF;
                    meTXD[idx++] = 0xFF;
                    meTXD[idx++] = 0xFF;
                    meTXD[idx++] = 0xFF;
                    meTXD[idx++] = 0xFF;
                    meTXD[idx++] = 0xFF;
                    meTXD[idx++] = 0xFF;
                    meTXD[idx++] = 0xFF;
                    meTXD[idx++] = 0xFF;
                    meTXD[idx++] = 0xFF;
                    meTXD[idx++] = 0xFF;
                    meTXD[idx++] = 0xFF;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.torque_disp;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.torque_min;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.torque_max;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.torque_over[(byte)MyDevice.mTCP[sAddress].devc.calunit];
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.capacity;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.ad_zero;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.ad_pos_point1;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.ad_pos_point2;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.ad_pos_point3;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.ad_pos_point4;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.ad_pos_point5;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.ad_neg_point1;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.ad_neg_point2;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.ad_neg_point3;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.ad_neg_point4;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.ad_neg_point5;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.tq_pos_point1;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.tq_pos_point2;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.tq_pos_point3;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.tq_pos_point4;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.tq_pos_point5;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.tq_neg_point1;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.tq_neg_point2;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.tq_neg_point3;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.tq_neg_point4;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.tq_neg_point5;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;

                    while (idx < 128 + 7)
                    {
                        meTXD[idx++] = 0xFF;
                    }
                    break;

                case TASKS.REG_BLOCK5_CAL2:
                    num = 0x50;//80个寄存器个数
                    meTXD[idx++] = sAddress;
                    meTXD[idx++] = (byte)CMD.CMD_SEQUENCE;
                    meTXD[idx++] = Constants.REG_BLOCK5_CAL2 >> 8;
                    meTXD[idx++] = Constants.REG_BLOCK5_CAL2 & 0xFF;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = num;
                    meTXD[idx++] = (byte)(num * 2);
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.cl2_ad_zero;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.cl2_ad_pos_point1;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.cl2_ad_pos_point2;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.cl2_ad_pos_point3;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.cl2_ad_pos_point4;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.cl2_ad_pos_point5;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.cl2_ad_neg_point1;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.cl2_ad_neg_point2;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.cl2_ad_neg_point3;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.cl2_ad_neg_point4;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.cl2_ad_neg_point5;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.cr1_ad_zero;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.cr1_ad_pos_point1;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.cr1_ad_pos_point2;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.cr1_ad_pos_point3;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.cr1_ad_pos_point4;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.cr1_ad_pos_point5;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.cr1_ad_neg_point1;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.cr1_ad_neg_point2;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.cr1_ad_neg_point3;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.cr1_ad_neg_point4;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.cr1_ad_neg_point5;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.cr2_ad_zero;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.cr2_ad_pos_point1;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.cr2_ad_pos_point2;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.cr2_ad_pos_point3;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.cr2_ad_pos_point4;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.cr2_ad_pos_point5;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.cr2_ad_neg_point1;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.cr2_ad_neg_point2;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.cr2_ad_neg_point3;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.cr2_ad_neg_point4;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].devc.cr2_ad_neg_point5;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;

                    while (idx < 160 + 7)
                    {
                        meTXD[idx++] = 0xFF;
                    }
                    break;

                case TASKS.REG_BLOCK5_INFO:
                    num = 0x50;//80个寄存器个数
                    meTXD[idx++] = sAddress;
                    meTXD[idx++] = (byte)CMD.CMD_SEQUENCE;
                    meTXD[idx++] = Constants.REG_BLOCK5_INFO >> 8;
                    meTXD[idx++] = Constants.REG_BLOCK5_INFO & 0xFF;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = num;
                    meTXD[idx++] = (byte)(num * 2);
                    MyDevice.myUIT.UI = MyDevice.mTCP[sAddress].work.srno;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.UI = MyDevice.mTCP[sAddress].work.number;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.UI = MyDevice.mTCP[sAddress].work.mfgtime;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.UI = MyDevice.mTCP[sAddress].work.caltime;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.UI = MyDevice.mTCP[sAddress].work.calremind;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    for (int i = idx; i < 32 + 7; i++)
                    {
                        meTXD[idx++] = 0xFF;
                    }

                    //MyDevice.mTCP[sAddress].work.name
                    strArr = Encoding.ASCII.GetBytes(MyDevice.mTCP[sAddress].work.name);
                    strLen = strArr.Length;

                    for (int i = idx; i < 64 + 7; i++)
                    {
                        if (idx < strLen)
                        {
                            meTXD[idx++] = strArr[i];
                        }
                        else
                        {
                            meTXD[idx++] = 0xFF;
                        }
                    }

                    //MyDevice.mTCP[sAddress].work.managetxt
                    strArr = Encoding.ASCII.GetBytes(MyDevice.mTCP[sAddress].work.managetxt);
                    strLen = strArr.Length;
                    for (int i = 0; idx < 96 + 7; i++)
                    {
                        if (idx < strLen)
                        {
                            meTXD[idx++] = strArr[i];
                        }
                        else
                        {
                            meTXD[idx++] = 0xFF;
                        }
                    }

                    //MyDevice.mTCP[sAddress].work.decription
                    strArr = Encoding.ASCII.GetBytes(MyDevice.mTCP[sAddress].work.decription);
                    strLen = strArr.Length;
                    for (int i = 0; idx < 160 + 7; i++)
                    {
                        if (idx < strLen)
                        {
                            meTXD[idx++] = strArr[i];
                        }
                        else
                        {
                            meTXD[idx++] = 0xFF;
                        }
                    }

                    break;

                case TASKS.REG_BLOCK3_WLAN:
                    num = 0x30;//48个寄存器个数
                    meTXD[idx++] = sAddress;
                    meTXD[idx++] = (byte)CMD.CMD_SEQUENCE;
                    meTXD[idx++] = Constants.REG_BLOCK4_WLAN >> 8;
                    meTXD[idx++] = Constants.REG_BLOCK4_WLAN & 0xFF;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = num;
                    meTXD[idx++] = (byte)(num * 2);
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = MyDevice.mTCP[sAddress].wlan.addr;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = MyDevice.mTCP[sAddress].wlan.rs485_baud;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = MyDevice.mTCP[sAddress].wlan.rs485_stopbit;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = MyDevice.mTCP[sAddress].wlan.rs485_parity;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = MyDevice.mTCP[sAddress].wlan.wifimode;

                    //示例ip地址给的字符串为 C0A80101 ，转成8个字节为 00 192 00 168 00 01 01
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = Convert.ToByte(MyDevice.mTCP[sAddress].wlan.wf_ip.Substring(0, 2), 16);
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = Convert.ToByte(MyDevice.mTCP[sAddress].wlan.wf_ip.Substring(2, 2), 16);
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = Convert.ToByte(MyDevice.mTCP[sAddress].wlan.wf_ip.Substring(4, 2), 16);
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = Convert.ToByte(MyDevice.mTCP[sAddress].wlan.wf_ip.Substring(6, 2), 16);

                    //示例输入的port是 uint32类型 5678 ,最后转换成2个字节 22 46
                    meTXD[idx++] = Convert.ToByte(MyDevice.mTCP[sAddress].wlan.wf_port.ToString("X2").Substring(0, 2), 16);
                    meTXD[idx++] = Convert.ToByte(MyDevice.mTCP[sAddress].wlan.wf_port.ToString("X2").Substring(2, 2), 16);

                    //MyDevice.mTCP[sAddress].wlan.wf_ssid
                    //wf_ssid 32个字节，以BEM46示例， 字节显示为 00 42 00 45 00 4D 00 34 00 36 ... 00
                    strArr = Encoding.ASCII.GetBytes(MyDevice.mTCP[sAddress].wlan.wf_ssid);
                    strLen = strArr.Length;
                    for (int i = 0; i < strLen; i++)
                    {
                        meTXD[idx++] = 0x00;
                        meTXD[idx++] = strArr[i];
                    }
                    while (idx < 52 + 7)
                    {
                        meTXD[idx++] = 0x00;
                    }

                    //MyDevice.mTCP[sAddress].wlan.wf_pwd
                    strArr = Encoding.ASCII.GetBytes(MyDevice.mTCP[sAddress].wlan.wf_pwd);
                    strLen = strArr.Length;
                    for (int i = 0; i < strLen; i++)
                    {
                        meTXD[idx++] = 0x00;
                        meTXD[idx++] = strArr[i];
                    }
                    while (idx < 84 + 7)
                    {
                        meTXD[idx++] = 0x00;
                    }

                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = MyDevice.mTCP[sAddress].wlan.rf_chan;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = MyDevice.mTCP[sAddress].wlan.rf_option;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = MyDevice.mTCP[sAddress].wlan.rf_para;
                    while (idx < 96 + 7)
                    {
                        meTXD[idx++] = 0xFF;
                    }

                    break;

                case TASKS.REG_BLOCK3_PARA:
                    num = 0x30;//48个寄存器个数
                    meTXD[idx++] = sAddress;
                    meTXD[idx++] = (byte)CMD.CMD_SEQUENCE;
                    meTXD[idx++] = Constants.REG_BLOCK2_PARA >> 8;
                    meTXD[idx++] = Constants.REG_BLOCK2_PARA & 0xFF;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = num;
                    meTXD[idx++] = (byte)(num * 2);
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = (byte)MyDevice.mTCP[sAddress].para.torque_unit;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = MyDevice.mTCP[sAddress].para.angle_speed;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = MyDevice.mTCP[sAddress].para.angle_decimal;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = MyDevice.mTCP[sAddress].para.mode_pt;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = MyDevice.mTCP[sAddress].para.mode_ax;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = MyDevice.mTCP[sAddress].para.mode_mx;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = MyDevice.mTCP[sAddress].para.fifomode;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = MyDevice.mTCP[sAddress].para.fiforec;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = MyDevice.mTCP[sAddress].para.fifospeed;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = MyDevice.mTCP[sAddress].para.heartformat;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = MyDevice.mTCP[sAddress].para.heartcount;

                    MyDevice.myUIT.US = MyDevice.mTCP[sAddress].para.heartcycle;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;

                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = MyDevice.mTCP[sAddress].para.accmode;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = MyDevice.mTCP[sAddress].para.alarmode;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = MyDevice.mTCP[sAddress].para.timeoff;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = MyDevice.mTCP[sAddress].para.timeback;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = MyDevice.mTCP[sAddress].para.timezero;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = MyDevice.mTCP[sAddress].para.disptype;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = MyDevice.mTCP[sAddress].para.disptheme;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = MyDevice.mTCP[sAddress].para.displan;

                    MyDevice.myUIT.US = MyDevice.mTCP[sAddress].para.unhook;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;

                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = MyDevice.mTCP[sAddress].para.adspeed;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = (byte)MyDevice.mTCP[sAddress].para.autozero;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = (byte)MyDevice.mTCP[sAddress].para.trackzero;

                    MyDevice.myUIT.US = MyDevice.mTCP[sAddress].para.amenable;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;

                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = MyDevice.mTCP[sAddress].para.screwmax;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = MyDevice.mTCP[sAddress].para.runmode;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = MyDevice.mTCP[sAddress].para.auploaden;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = MyDevice.mTCP[sAddress].para.devrole;

                    //预留6个字节
                    meTXD[idx++] = 0xFF;
                    meTXD[idx++] = 0xFF;
                    meTXD[idx++] = 0xFF;
                    meTXD[idx++] = 0xFF;
                    meTXD[idx++] = 0xFF;
                    meTXD[idx++] = 0xFF;

                    MyDevice.myUIT.F = MyDevice.mTCP[sAddress].para.angcorr;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.I = MyDevice.mTCP[sAddress].para.angle_resist;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;

                    //预留字节
                    for (int i = idx; i < 96 + 7; i++)
                    {
                        meTXD[idx++] = 0xFF;
                    }
                    break;

                case TASKS.REG_BLOCK5_AM1:
                    num = 0x50;//80个寄存器个数
                    meTXD[idx++] = sAddress;
                    meTXD[idx++] = (byte)CMD.CMD_SEQUENCE;
                    meTXD[idx++] = Constants.REG_BLOCK5_AM1 >> 8;
                    meTXD[idx++] = Constants.REG_BLOCK5_AM1 & 0xFF;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = num;
                    meTXD[idx++] = (byte)(num * 2);

                    for (int i = 0; i < 10; i++)
                    {
                        MyDevice.myUIT.I = MyDevice.mTCP[sAddress].alam.SN_target[i, (int)MyDevice.mTCP[sAddress].para.torque_unit];
                        meTXD[idx++] = MyDevice.myUIT.B3;
                        meTXD[idx++] = MyDevice.myUIT.B2;
                        meTXD[idx++] = MyDevice.myUIT.B1;
                        meTXD[idx++] = MyDevice.myUIT.B0;
                    }

                    for (int i = 0; i < 10; i++)
                    {
                        MyDevice.myUIT.I = MyDevice.mTCP[sAddress].alam.SA_pre[i, (int)MyDevice.mTCP[sAddress].para.torque_unit];
                        meTXD[idx++] = MyDevice.myUIT.B3;
                        meTXD[idx++] = MyDevice.myUIT.B2;
                        meTXD[idx++] = MyDevice.myUIT.B1;
                        meTXD[idx++] = MyDevice.myUIT.B0;
                        MyDevice.myUIT.I = MyDevice.mTCP[sAddress].alam.SA_ang[i];
                        meTXD[idx++] = MyDevice.myUIT.B3;
                        meTXD[idx++] = MyDevice.myUIT.B2;
                        meTXD[idx++] = MyDevice.myUIT.B1;
                        meTXD[idx++] = MyDevice.myUIT.B0;
                    }

                    for (int i = 0; i < 5; i++)
                    {
                        MyDevice.myUIT.I = MyDevice.mTCP[sAddress].alam.MN_low[i, (int)MyDevice.mTCP[sAddress].para.torque_unit];
                        meTXD[idx++] = MyDevice.myUIT.B3;
                        meTXD[idx++] = MyDevice.myUIT.B2;
                        meTXD[idx++] = MyDevice.myUIT.B1;
                        meTXD[idx++] = MyDevice.myUIT.B0;
                        MyDevice.myUIT.I = MyDevice.mTCP[sAddress].alam.MN_high[i, (int)MyDevice.mTCP[sAddress].para.torque_unit];
                        meTXD[idx++] = MyDevice.myUIT.B3;
                        meTXD[idx++] = MyDevice.myUIT.B2;
                        meTXD[idx++] = MyDevice.myUIT.B1;
                        meTXD[idx++] = MyDevice.myUIT.B0;
                    }
                    break;

                case TASKS.REG_BLOCK5_AM2:
                    num = 0x50;//80个寄存器个数
                    meTXD[idx++] = sAddress;
                    meTXD[idx++] = (byte)CMD.CMD_SEQUENCE;
                    meTXD[idx++] = Constants.REG_BLOCK5_AM2 >> 8;
                    meTXD[idx++] = Constants.REG_BLOCK5_AM2 & 0xFF;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = num;
                    meTXD[idx++] = (byte)(num * 2);

                    for (int i = 5; i < 10; i++)
                    {
                        MyDevice.myUIT.I = MyDevice.mTCP[sAddress].alam.MN_low[i, (int)MyDevice.mTCP[sAddress].para.torque_unit];
                        meTXD[idx++] = MyDevice.myUIT.B3;
                        meTXD[idx++] = MyDevice.myUIT.B2;
                        meTXD[idx++] = MyDevice.myUIT.B1;
                        meTXD[idx++] = MyDevice.myUIT.B0;
                        MyDevice.myUIT.I = MyDevice.mTCP[sAddress].alam.MN_high[i, (int)MyDevice.mTCP[sAddress].para.torque_unit];
                        meTXD[idx++] = MyDevice.myUIT.B3;
                        meTXD[idx++] = MyDevice.myUIT.B2;
                        meTXD[idx++] = MyDevice.myUIT.B1;
                        meTXD[idx++] = MyDevice.myUIT.B0;
                    }

                    for (int i = 0; i < 10; i++)
                    {
                        MyDevice.myUIT.I = MyDevice.mTCP[sAddress].alam.MA_pre[i, (int)MyDevice.mTCP[sAddress].para.torque_unit];
                        meTXD[idx++] = MyDevice.myUIT.B3;
                        meTXD[idx++] = MyDevice.myUIT.B2;
                        meTXD[idx++] = MyDevice.myUIT.B1;
                        meTXD[idx++] = MyDevice.myUIT.B0;
                        MyDevice.myUIT.I = MyDevice.mTCP[sAddress].alam.MA_low[i];
                        meTXD[idx++] = MyDevice.myUIT.B3;
                        meTXD[idx++] = MyDevice.myUIT.B2;
                        meTXD[idx++] = MyDevice.myUIT.B1;
                        meTXD[idx++] = MyDevice.myUIT.B0;
                        MyDevice.myUIT.I = MyDevice.mTCP[sAddress].alam.MA_high[i];
                        meTXD[idx++] = MyDevice.myUIT.B3;
                        meTXD[idx++] = MyDevice.myUIT.B2;
                        meTXD[idx++] = MyDevice.myUIT.B1;
                        meTXD[idx++] = MyDevice.myUIT.B0;
                    }
                    break;

                case TASKS.REG_BLOCK5_AM3:
                    num = 0x50;//80个寄存器个数
                    meTXD[idx++] = sAddress;
                    meTXD[idx++] = (byte)CMD.CMD_SEQUENCE;
                    meTXD[idx++] = Constants.REG_BLOCK5_AM3 >> 8;
                    meTXD[idx++] = Constants.REG_BLOCK5_AM3 & 0xFF;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = num;
                    meTXD[idx++] = (byte)(num * 2);

                    for (int i = 0; i < 10; i++)
                    {
                        MyDevice.myUIT.I = MyDevice.mTCP[sAddress].alam.AZ_start[i, (int)MyDevice.mTCP[sAddress].para.torque_unit];
                        meTXD[idx++] = MyDevice.myUIT.B3;
                        meTXD[idx++] = MyDevice.myUIT.B2;
                        meTXD[idx++] = MyDevice.myUIT.B1;
                        meTXD[idx++] = MyDevice.myUIT.B0;
                        MyDevice.myUIT.I = MyDevice.mTCP[sAddress].alam.AZ_stop[i, (int)MyDevice.mTCP[sAddress].para.torque_unit];
                        meTXD[idx++] = MyDevice.myUIT.B3;
                        meTXD[idx++] = MyDevice.myUIT.B2;
                        meTXD[idx++] = MyDevice.myUIT.B1;
                        meTXD[idx++] = MyDevice.myUIT.B0;
                        MyDevice.myUIT.I = MyDevice.mTCP[sAddress].alam.AZ_hock[i, (int)MyDevice.mTCP[sAddress].para.torque_unit];
                        meTXD[idx++] = MyDevice.myUIT.B3;
                        meTXD[idx++] = MyDevice.myUIT.B2;
                        meTXD[idx++] = MyDevice.myUIT.B1;
                        meTXD[idx++] = MyDevice.myUIT.B0;
                    }
                    //预留字节
                    for (int i = idx; i < 160 + 7; i++)
                    {
                        meTXD[idx++] = 0xFF;
                    }
                    break;

                case TASKS.REG_BLOCK3_JOB:
                    num = 0x30;//48个寄存器个数
                    meTXD[idx++] = sAddress;
                    meTXD[idx++] = (byte)CMD.CMD_SEQUENCE;
                    meTXD[idx++] = Constants.REG_BLOCK3_JOB >> 8;
                    meTXD[idx++] = Constants.REG_BLOCK3_JOB & 0xFF;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = num;
                    meTXD[idx++] = (byte)(num * 2);
                    MyDevice.myUIT.UI = MyDevice.mTCP[sAddress].work.wo_area;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.UI = MyDevice.mTCP[sAddress].work.wo_factory;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.UI = MyDevice.mTCP[sAddress].work.wo_line;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.UI = MyDevice.mTCP[sAddress].work.wo_station;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;

                    //预留字节
                    for (int i = idx; i < 32 + 7; i++)
                    {
                        meTXD[idx++] = 0xFF;
                    }

                    MyDevice.myUIT.UI = MyDevice.mTCP[sAddress].work.wo_stamp;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.UI = MyDevice.mTCP[sAddress].work.wo_bat;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    MyDevice.myUIT.UI = MyDevice.mTCP[sAddress].work.wo_num;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    //MyDevice.mTCP[sAddress].work.wo_name
                    strArr = Encoding.ASCII.GetBytes(MyDevice.mTCP[sAddress].work.wo_name);
                    strLen = strArr.Length;
                    for (int i = 0; idx < 84 + 7; i++)
                    {
                        if (idx < strLen)
                        {
                            meTXD[idx++] = strArr[i];
                        }
                        else
                        {
                            meTXD[idx++] = 0xFF;
                        }
                    }
                    //预留字节
                    for (int i = idx; i < 96 + 7; i++)
                    {
                        meTXD[idx++] = 0xFF;
                    }

                    break;

                case TASKS.REG_BLOCK3_OP:
                    num = 0x30;//48个寄存器个数
                    meTXD[idx++] = sAddress;
                    meTXD[idx++] = (byte)CMD.CMD_SEQUENCE;
                    meTXD[idx++] = Constants.REG_BLOCK3_OP >> 8;
                    meTXD[idx++] = Constants.REG_BLOCK3_OP & 0xFF;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = num;
                    meTXD[idx++] = (byte)(num * 2);
                    MyDevice.myUIT.UI = MyDevice.mTCP[sAddress].work.user_ID;
                    meTXD[idx++] = MyDevice.myUIT.B3;
                    meTXD[idx++] = MyDevice.myUIT.B2;
                    meTXD[idx++] = MyDevice.myUIT.B1;
                    meTXD[idx++] = MyDevice.myUIT.B0;
                    //MyDevice.mTCP[sAddress].work.user_name
                    strArr = Encoding.ASCII.GetBytes(MyDevice.mTCP[sAddress].work.user_name);
                    strLen = strArr.Length;
                    for (int i = 0; idx < 52 + 7; i++)
                    {
                        if (idx < strLen)
                        {
                            meTXD[idx++] = strArr[i];
                        }
                        else
                        {
                            meTXD[idx++] = 0xFF;
                        }
                    }
                    //预留字节
                    for (int i = idx; i < 96 + 7; i++)
                    {
                        meTXD[idx++] = 0xFF;
                    }

                    break;

                case TASKS.REG_BLOCK3_SCREW1:
                    num = 0x30;//48个寄存器个数
                    meTXD[idx++] = sAddress;
                    meTXD[idx++] = (byte)CMD.CMD_SEQUENCE;
                    meTXD[idx++] = Constants.REG_BLOCK3_SCREW1 >> 8;
                    meTXD[idx++] = Constants.REG_BLOCK3_SCREW1 & 0xFF;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = num;
                    meTXD[idx++] = (byte)(num * 2);
                    for (int i = 0; i < 8; i++)
                    {
                        meTXD[idx++] = MyDevice.mTCP[sAddress].screw[i].scw_ticketAxMx;
                        meTXD[idx++] = MyDevice.mTCP[sAddress].screw[i].scw_ticketCnt;
                        MyDevice.myUIT.UI = MyDevice.mTCP[sAddress].screw[i].scw_ticketNum;
                        meTXD[idx++] = MyDevice.myUIT.B3;
                        meTXD[idx++] = MyDevice.myUIT.B2;
                        meTXD[idx++] = MyDevice.myUIT.B1;
                        meTXD[idx++] = MyDevice.myUIT.B0;
                        MyDevice.myUIT.US = (ushort)(MyDevice.mTCP[sAddress].screw[i].scw_ticketSerial / Math.Pow(10, 9));
                        meTXD[idx++] = MyDevice.myUIT.B1;
                        meTXD[idx++] = MyDevice.myUIT.B0;
                        MyDevice.myUIT.UI = (uint)(MyDevice.mTCP[sAddress].screw[i].scw_ticketSerial % Math.Pow(10, 9));
                        meTXD[idx++] = MyDevice.myUIT.B3;
                        meTXD[idx++] = MyDevice.myUIT.B2;
                        meTXD[idx++] = MyDevice.myUIT.B1;
                        meTXD[idx++] = MyDevice.myUIT.B0;
                    }
                    break;

                case TASKS.REG_BLOCK3_SCREW2:
                    num = 0x30;//48个寄存器个数
                    meTXD[idx++] = sAddress;
                    meTXD[idx++] = (byte)CMD.CMD_SEQUENCE;
                    meTXD[idx++] = Constants.REG_BLOCK3_SCREW2 >> 8;
                    meTXD[idx++] = Constants.REG_BLOCK3_SCREW2 & 0xFF;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = num;
                    meTXD[idx++] = (byte)(num * 2);
                    for (int i = 8; i < 16; i++)
                    {
                        meTXD[idx++] = MyDevice.mTCP[sAddress].screw[i].scw_ticketAxMx;
                        meTXD[idx++] = MyDevice.mTCP[sAddress].screw[i].scw_ticketCnt;
                        MyDevice.myUIT.UI = MyDevice.mTCP[sAddress].screw[i].scw_ticketNum;
                        meTXD[idx++] = MyDevice.myUIT.B3;
                        meTXD[idx++] = MyDevice.myUIT.B2;
                        meTXD[idx++] = MyDevice.myUIT.B1;
                        meTXD[idx++] = MyDevice.myUIT.B0;
                        MyDevice.myUIT.UI = (ushort)(MyDevice.mTCP[sAddress].screw[i].scw_ticketSerial / Math.Pow(10, 9));
                        meTXD[idx++] = MyDevice.myUIT.B1;
                        meTXD[idx++] = MyDevice.myUIT.B0;
                        MyDevice.myUIT.UI = (uint)(MyDevice.mTCP[sAddress].screw[i].scw_ticketSerial % Math.Pow(10, 9));
                        meTXD[idx++] = MyDevice.myUIT.B3;
                        meTXD[idx++] = MyDevice.myUIT.B2;
                        meTXD[idx++] = MyDevice.myUIT.B1;
                        meTXD[idx++] = MyDevice.myUIT.B0;
                    }
                    break;

                case TASKS.REG_BLOCK3_SCREW3:
                    num = 0x30;//48个寄存器个数
                    meTXD[idx++] = sAddress;
                    meTXD[idx++] = (byte)CMD.CMD_SEQUENCE;
                    meTXD[idx++] = Constants.REG_BLOCK3_SCREW3 >> 8;
                    meTXD[idx++] = Constants.REG_BLOCK3_SCREW3 & 0xFF;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = num;
                    meTXD[idx++] = (byte)(num * 2);
                    for (int i = 16; i < 24; i++)
                    {
                        meTXD[idx++] = MyDevice.mTCP[sAddress].screw[i].scw_ticketAxMx;
                        meTXD[idx++] = MyDevice.mTCP[sAddress].screw[i].scw_ticketCnt;
                        MyDevice.myUIT.UI = MyDevice.mTCP[sAddress].screw[i].scw_ticketNum;
                        meTXD[idx++] = MyDevice.myUIT.B3;
                        meTXD[idx++] = MyDevice.myUIT.B2;
                        meTXD[idx++] = MyDevice.myUIT.B1;
                        meTXD[idx++] = MyDevice.myUIT.B0;
                        MyDevice.myUIT.UI = (ushort)(MyDevice.mTCP[sAddress].screw[i].scw_ticketSerial / Math.Pow(10, 9));
                        meTXD[idx++] = MyDevice.myUIT.B1;
                        meTXD[idx++] = MyDevice.myUIT.B0;
                        MyDevice.myUIT.UI = (uint)(MyDevice.mTCP[sAddress].screw[i].scw_ticketSerial % Math.Pow(10, 9));
                        meTXD[idx++] = MyDevice.myUIT.B3;
                        meTXD[idx++] = MyDevice.myUIT.B2;
                        meTXD[idx++] = MyDevice.myUIT.B1;
                        meTXD[idx++] = MyDevice.myUIT.B0;
                    }
                    break;

                case TASKS.REG_BLOCK3_SCREW4:
                    num = 0x30;//48个寄存器个数
                    meTXD[idx++] = sAddress;
                    meTXD[idx++] = (byte)CMD.CMD_SEQUENCE;
                    meTXD[idx++] = Constants.REG_BLOCK3_SCREW4 >> 8;
                    meTXD[idx++] = Constants.REG_BLOCK3_SCREW4 & 0xFF;
                    meTXD[idx++] = 0x00;
                    meTXD[idx++] = num;
                    meTXD[idx++] = (byte)(num * 2);
                    for (int i = 24; i < 32; i++)
                    {
                        meTXD[idx++] = MyDevice.mTCP[sAddress].screw[i].scw_ticketAxMx;
                        meTXD[idx++] = MyDevice.mTCP[sAddress].screw[i].scw_ticketCnt;
                        MyDevice.myUIT.UI = MyDevice.mTCP[sAddress].screw[i].scw_ticketNum;
                        meTXD[idx++] = MyDevice.myUIT.B3;
                        meTXD[idx++] = MyDevice.myUIT.B2;
                        meTXD[idx++] = MyDevice.myUIT.B1;
                        meTXD[idx++] = MyDevice.myUIT.B0;
                        MyDevice.myUIT.UI = (ushort)(MyDevice.mTCP[sAddress].screw[i].scw_ticketSerial / Math.Pow(10, 9));
                        meTXD[idx++] = MyDevice.myUIT.B1;
                        meTXD[idx++] = MyDevice.myUIT.B0;
                        MyDevice.myUIT.UI = (uint)(MyDevice.mTCP[sAddress].screw[i].scw_ticketSerial % Math.Pow(10, 9));
                        meTXD[idx++] = MyDevice.myUIT.B3;
                        meTXD[idx++] = MyDevice.myUIT.B2;
                        meTXD[idx++] = MyDevice.myUIT.B1;
                        meTXD[idx++] = MyDevice.myUIT.B0;
                    }
                    break;

                default:
                    break;
            }

            //
            MODBUS.AP_CRC16_MODBUS(meTXD, idx).CopyTo(meTXD, idx);
            idx += 2;
            try
            {
                ClientSocket.BeginSend(meTXD, 0, idx, 0, new AsyncCallback(SendCallback), ClientSocket);
            }
            catch
            {
                closeConnection();
            }
            txCnt += idx;
            trTASK = meTask;
            isEQ = false;
        }

        //发送连续写指令
        public void Protocool_Sequence_FifoClear(Int32 num)
        {
            //
            if (ClientSocket == null) return;
            Array.Clear(meTXD, 0, meTXD.Length);

            //
            meTXD[0] = sAddress;
            meTXD[1] = (byte)CMD.CMD_SEQUENCE;
            meTXD[2] = (UInt16)REG.REG_W_FIFOCLEAR >> 8;
            meTXD[3] = (UInt16)REG.REG_W_FIFOCLEAR & 0xFF;
            meTXD[4] = 0x00;
            meTXD[5] = 0x02;
            meTXD[6] = 0x04;
            MyDevice.myUIT.I = num;
            meTXD[7] = MyDevice.myUIT.B3;
            meTXD[8] = MyDevice.myUIT.B2;
            meTXD[9] = MyDevice.myUIT.B1;
            meTXD[10] = MyDevice.myUIT.B0;

            //
            MODBUS.AP_CRC16_MODBUS(meTXD, 11).CopyTo(meTXD, 11);
            try
            {
                ClientSocket.BeginSend(meTXD, 0, 13, 0, new AsyncCallback(SendCallback), ClientSocket);
            }
            catch
            {
                closeConnection();
            }
            txCnt += 13;
            trTASK = TASKS.WRITE_FIFOCLEAR;
            isEQ = false;
        }

        //发送连续写指令
        public void Protocool_Sequence_FifoIndex(UInt32 index)
        {
            //
            if (ClientSocket == null) return;
            Array.Clear(meTXD, 0, meTXD.Length);

            //
            meTXD[0] = sAddress;
            meTXD[1] = (byte)CMD.CMD_SEQUENCE;
            meTXD[2] = (UInt16)REG.REG_R_RECDAT >> 8;
            meTXD[3] = (UInt16)REG.REG_R_RECDAT & 0xFF;
            meTXD[4] = 0x00;
            meTXD[5] = 0x02;
            meTXD[6] = 0x04;
            MyDevice.myUIT.UI = index;
            meTXD[7] = MyDevice.myUIT.B3;
            meTXD[8] = MyDevice.myUIT.B2;
            meTXD[9] = MyDevice.myUIT.B1;
            meTXD[10] = MyDevice.myUIT.B0;

            //
            MODBUS.AP_CRC16_MODBUS(meTXD, 11).CopyTo(meTXD, 11);
            try
            {
                ClientSocket.BeginSend(meTXD, 0, 13, 0, new AsyncCallback(SendCallback), ClientSocket);
            }
            catch
            {
                closeConnection();
            }
            txCnt += 13;
            trTASK = TASKS.WRITE_FIFO_INDEX;
            isEQ = false;
        }

        //移除已处理的字节
        private void mePort_DataRemove(UInt16 num)
        {
            rxRead += num;
            rxRnt -= num;

            if (rxRead >= Constants.RxSize)
            {
                rxRead = (UInt16)(rxRead % Constants.RxSize);
            }
        }

        //取一个Byte数
        private Byte mePort_GetByte(UInt16 idx)
        {
            idx += rxRead;

            if (idx >= Constants.RxSize)
            {
                idx -= Constants.RxSize;
            }

            return meRXD[idx];
        }

        //取一个Int16数
        private Int16 mePort_GetInt16(UInt16 idx)
        {
            UIT myUIT = new UIT();
            myUIT.B1 = meCRC[idx++];
            myUIT.B0 = meCRC[idx++];
            return myUIT.S;
        }

        //取一个UInt16数
        private UInt16 mePort_GetUInt16(UInt16 idx)
        {
            UIT myUIT = new UIT();
            myUIT.B1 = meCRC[idx++];
            myUIT.B0 = meCRC[idx++];
            return myUIT.US;
        }

        //取一个Int32数
        private Int32 mePort_GetInt32(UInt16 idx)
        {
            UIT myUIT = new UIT();
            myUIT.B3 = meCRC[idx++];
            myUIT.B2 = meCRC[idx++];
            myUIT.B1 = meCRC[idx++];
            myUIT.B0 = meCRC[idx++];
            return myUIT.I;
        }

        //取一个UInt32数
        private UInt32 mePort_GetUInt32(UInt16 idx)
        {
            UIT myUIT = new UIT();
            myUIT.B3 = meCRC[idx++];
            myUIT.B2 = meCRC[idx++];
            myUIT.B1 = meCRC[idx++];
            myUIT.B0 = meCRC[idx++];
            return myUIT.UI;
        }

        //取一个String数
        private String mePort_GetString(UInt16 idx, UInt16 len)
        {
            return System.Text.Encoding.Default.GetString(meCRC, idx, len);
        }

        //取一个float数
        private float mePort_GetFloat(UInt16 idx)
        {
            UIT myUIT = new UIT();
            myUIT.B3 = meCRC[idx++];
            myUIT.B2 = meCRC[idx++];
            myUIT.B1 = meCRC[idx++];
            myUIT.B0 = meCRC[idx++];
            return myUIT.F;
        }

        //接收读帧
        private void mePort_DataReceiveRead()
        {
            //长度
            UInt16 len = (UInt16)(mePort_GetByte(2) + 5);

            //长度不够返回,等收到更多字节再校验
            //只支持0x10,20,30,40,50个寄存器,读数据至少37字节
            //不支持1个寄存器,不存在接收7字节的读数据
            if (rxRnt < len)
            {
                return;
            }

            //拷贝
            Array.Clear(meCRC, 0, meCRC.Length);
            for (UInt16 idx = 0; idx < len; idx++)
            {
                meCRC[idx] = mePort_GetByte(idx);
            }

            //校验CRC
            if (0 != MODBUS.AP_CRC16_MODBUS(meCRC, len, true))
            {
                string crcErr = "";
                for (int i = 0; i < len; i++)
                {
                    crcErr += meCRC[i].ToString("X2") + " ";
                }
                Console.WriteLine(crcErr);
                mePort_DataRemove(1);
                return;
            }

            //解码
            switch (trTASK)
            {
                case TASKS.REG_BLOCK1_DEV:
                    if (len == 0x25)
                    {
                        sAddress = mePort_GetByte(0);
                        MyDevice.mTCP[sAddress].wlan.addr = sAddress;
                        MyDevice.mTCP[sAddress].devc.series = (SERIES)mePort_GetUInt16(3);
                        MyDevice.mTCP[sAddress].devc.type = (TYPE)mePort_GetUInt16(5);
                        MyDevice.mTCP[sAddress].devc.version = (byte)mePort_GetUInt16(7);
                        MyDevice.mTCP[sAddress].devc.hardware = (byte)mePort_GetUInt16(9);
                        MyDevice.mTCP[sAddress].devc.bohrcode = mePort_GetUInt16(11);
                        MyDevice.mTCP[sAddress].devc.bohrcode = mePort_GetUInt16(13) + (MyDevice.mTCP[sAddress].devc.bohrcode << 8);
                        MyDevice.mTCP[sAddress].devc.bohrcode = mePort_GetUInt16(15) + (MyDevice.mTCP[sAddress].devc.bohrcode << 8);
                        MyDevice.mTCP[sAddress].devc.bohrcode = mePort_GetUInt16(17) + (MyDevice.mTCP[sAddress].devc.bohrcode << 8);
                        MyDevice.mTCP[sAddress].devc.bohrcode = mePort_GetUInt16(19) + (MyDevice.mTCP[sAddress].devc.bohrcode << 8);
                        MyDevice.mTCP[sAddress].devc.bohrcode = mePort_GetUInt16(21) + (MyDevice.mTCP[sAddress].devc.bohrcode << 8);
                        MyDevice.mTCP[sAddress].devc.torque_err[0] = mePort_GetInt32(23);
                        MyDevice.mTCP[sAddress].devc.torque_err[1] = mePort_GetInt32(23);
                        MyDevice.mTCP[sAddress].devc.torque_err[2] = mePort_GetInt32(23);
                        MyDevice.mTCP[sAddress].devc.torque_err[3] = mePort_GetInt32(23);
                        MyDevice.mTCP[sAddress].devc.torque_err[4] = mePort_GetInt32(23);
                        mePort_DataRemove(0x25);
                        isEQ = true;
                        MyDevice.mTCP[sAddress].sTATE = STATE.CONNECTED;//状态已连接

                        Task.Run(() =>
                        {
                            if (MyDevice.IsMySqlStart)
                            {
                                //根据扳手bohrcode确定唯一值
                                if (JDBC.GetDataGroupByBohrCode(MyDevice.mTCP[sAddress].devc.bohrcode.ToString()).Count == 0)
                                {
                                    JDBC.AddDataGroup(new DSDataGroup()
                                    {
                                        GroupId = 1,
                                        VinId = UnitConvert.GetTime(UnitConvert.GetTimeStamp()).ToString("yyyy-MM-dd"),
                                        BohrCode = MyDevice.mTCP[sAddress].devc.bohrcode.ToString(),
                                    });
                                }

                                //添加数据库数据纪录汇总表
                                if (JDBC.GetDataSummaryByDateType(MyDevice.DataType).Count == 0)
                                {
                                    JDBC.AddDataSummary(new DSDataSummary()
                                    {
                                        DataId = 1,
                                        DataType = MyDevice.DataType,
                                        CreateTime = DateTime.Now.ToString(),
                                        WorkId = MyDevice.WorkId,
                                        WorkNum = MyDevice.WorkNum,
                                        SequenceId = MyDevice.SequenceId
                                    });
                                }
                            }
                        });
                    }
                    else
                    {
                        mePort_DataRemove(1);
                        return;
                    }
                    break;

                case TASKS.REG_BLOCK4_CAL1:
                    if (len == 0x85)
                    {
                        MyDevice.mTCP[sAddress].devc.calunit = (UNIT)mePort_GetUInt16(3);
                        MyDevice.mTCP[sAddress].devc.caltype        = (byte)mePort_GetUInt16(5);
                        MyDevice.mTCP[sAddress].devc.torque_decimal = (byte)mePort_GetUInt16(7);
                        MyDevice.mTCP[sAddress].devc.torque_fdn     = (byte)mePort_GetUInt16(9);
                        MyDevice.mTCP[sAddress].devc.calIndex       = (byte)mePort_GetUInt16(11);
                        //14个备用字节
                        MyDevice.mTCP[sAddress].devc.torque_disp = mePort_GetInt32(27);
                        MyDevice.mTCP[sAddress].devc.torque_min = mePort_GetInt32(31);
                        MyDevice.mTCP[sAddress].devc.torque_max = mePort_GetInt32(35);
                        MyDevice.mTCP[sAddress].devc.torque_over[(int)MyDevice.mTCP[sAddress].devc.calunit] = mePort_GetInt32(39);

                        //更新超量程使用扭矩值——超量程以over作为基准，无论标定单位是什么这个基准均是3600
                        switch (MyDevice.mTCP[sAddress].devc.calunit)
                        {
                            case UNIT.UNIT_nm:
                                MyDevice.mTCP[sAddress].devc.torque_over[0] = mePort_GetInt32(39);
                                MyDevice.mTCP[sAddress].devc.torque_over[1] = UnitConvert.TorqueTransLbfin(MyDevice.mTCP[sAddress].devc.torque_over[0], (byte)UNIT.UNIT_nm);
                                MyDevice.mTCP[sAddress].devc.torque_over[2] = UnitConvert.TorqueTransLbfft(MyDevice.mTCP[sAddress].devc.torque_over[0], (byte)UNIT.UNIT_nm);
                                MyDevice.mTCP[sAddress].devc.torque_over[3] = UnitConvert.TorqueTransKgfcm(MyDevice.mTCP[sAddress].devc.torque_over[0], (byte)UNIT.UNIT_nm);
                                MyDevice.mTCP[sAddress].devc.torque_over[4] = UnitConvert.TorqueTransKgfm(MyDevice.mTCP[sAddress].devc.torque_over[0], (byte)UNIT.UNIT_nm);
                                break;
                            case UNIT.UNIT_lbfin:
                                MyDevice.mTCP[sAddress].devc.torque_over[0] = UnitConvert.TorqueTransNm(MyDevice.mTCP[sAddress].devc.torque_over[1], (byte)UNIT.UNIT_lbfin);
                                MyDevice.mTCP[sAddress].devc.torque_over[1] = mePort_GetInt32(39);
                                MyDevice.mTCP[sAddress].devc.torque_over[2] = UnitConvert.TorqueTransLbfft(MyDevice.mTCP[sAddress].devc.torque_over[1], (byte)UNIT.UNIT_lbfin);
                                MyDevice.mTCP[sAddress].devc.torque_over[3] = UnitConvert.TorqueTransKgfcm(MyDevice.mTCP[sAddress].devc.torque_over[1], (byte)UNIT.UNIT_lbfin);
                                MyDevice.mTCP[sAddress].devc.torque_over[4] = UnitConvert.TorqueTransKgfm(MyDevice.mTCP[sAddress].devc.torque_over[1], (byte)UNIT.UNIT_lbfin);
                                break;
                            case UNIT.UNIT_lbfft:
                                MyDevice.mTCP[sAddress].devc.torque_over[0] = UnitConvert.TorqueTransNm(MyDevice.mTCP[sAddress].devc.torque_over[2], (byte)UNIT.UNIT_lbfft);
                                MyDevice.mTCP[sAddress].devc.torque_over[1] = UnitConvert.TorqueTransLbfin(MyDevice.mTCP[sAddress].devc.torque_over[2], (byte)UNIT.UNIT_lbfft);
                                MyDevice.mTCP[sAddress].devc.torque_over[2] = mePort_GetInt32(39);
                                MyDevice.mTCP[sAddress].devc.torque_over[3] = UnitConvert.TorqueTransKgfcm(MyDevice.mTCP[sAddress].devc.torque_over[2], (byte)UNIT.UNIT_lbfft);
                                MyDevice.mTCP[sAddress].devc.torque_over[4] = UnitConvert.TorqueTransKgfm(MyDevice.mTCP[sAddress].devc.torque_over[2], (byte)UNIT.UNIT_lbfft);
                                break;
                            case UNIT.UNIT_kgcm:
                                MyDevice.mTCP[sAddress].devc.torque_over[0] = UnitConvert.TorqueTransNm(MyDevice.mTCP[sAddress].devc.torque_over[3], (byte)UNIT.UNIT_kgcm);
                                MyDevice.mTCP[sAddress].devc.torque_over[1] = UnitConvert.TorqueTransLbfin(MyDevice.mTCP[sAddress].devc.torque_over[3], (byte)UNIT.UNIT_kgcm);
                                MyDevice.mTCP[sAddress].devc.torque_over[2] = UnitConvert.TorqueTransLbfft(MyDevice.mTCP[sAddress].devc.torque_over[3], (byte)UNIT.UNIT_kgcm);
                                MyDevice.mTCP[sAddress].devc.torque_over[3] = mePort_GetInt32(39);
                                MyDevice.mTCP[sAddress].devc.torque_over[4] = UnitConvert.TorqueTransKgfm(MyDevice.mTCP[sAddress].devc.torque_over[3], (byte)UNIT.UNIT_kgcm);
                                break;
                            case UNIT.UNIT_kgm:
                                MyDevice.mTCP[sAddress].devc.torque_over[0] = UnitConvert.TorqueTransNm(MyDevice.mTCP[sAddress].devc.torque_over[4], (byte)UNIT.UNIT_kgm);
                                MyDevice.mTCP[sAddress].devc.torque_over[1] = UnitConvert.TorqueTransLbfin(MyDevice.mTCP[sAddress].devc.torque_over[4], (byte)UNIT.UNIT_kgm);
                                MyDevice.mTCP[sAddress].devc.torque_over[2] = UnitConvert.TorqueTransLbfft(MyDevice.mTCP[sAddress].devc.torque_over[4], (byte)UNIT.UNIT_kgm);
                                MyDevice.mTCP[sAddress].devc.torque_over[3] = UnitConvert.TorqueTransKgfcm(MyDevice.mTCP[sAddress].devc.torque_over[4], (byte)UNIT.UNIT_kgm);
                                MyDevice.mTCP[sAddress].devc.torque_over[4] = mePort_GetInt32(39);
                                break;
                            default:
                                break;
                        }

                        MyDevice.mTCP[sAddress].devc.capacity       = mePort_GetInt32(43);
                        MyDevice.mTCP[sAddress].devc.ad_zero        = mePort_GetInt32(47);
                        MyDevice.mTCP[sAddress].devc.ad_pos_point1  = mePort_GetInt32(51);
                        MyDevice.mTCP[sAddress].devc.ad_pos_point2  = mePort_GetInt32(55);
                        MyDevice.mTCP[sAddress].devc.ad_pos_point3  = mePort_GetInt32(59);
                        MyDevice.mTCP[sAddress].devc.ad_pos_point4  = mePort_GetInt32(63);
                        MyDevice.mTCP[sAddress].devc.ad_pos_point5  = mePort_GetInt32(67);
                        MyDevice.mTCP[sAddress].devc.ad_neg_point1  = mePort_GetInt32(71);
                        MyDevice.mTCP[sAddress].devc.ad_neg_point2  = mePort_GetInt32(75);
                        MyDevice.mTCP[sAddress].devc.ad_neg_point3  = mePort_GetInt32(79);
                        MyDevice.mTCP[sAddress].devc.ad_neg_point4  = mePort_GetInt32(83);
                        MyDevice.mTCP[sAddress].devc.ad_neg_point5  = mePort_GetInt32(87);
                        MyDevice.mTCP[sAddress].devc.tq_pos_point1  = mePort_GetInt32(91);
                        MyDevice.mTCP[sAddress].devc.tq_pos_point2  = mePort_GetInt32(95);
                        MyDevice.mTCP[sAddress].devc.tq_pos_point3  = mePort_GetInt32(99);
                        MyDevice.mTCP[sAddress].devc.tq_pos_point4  = mePort_GetInt32(103);
                        MyDevice.mTCP[sAddress].devc.tq_pos_point5  = mePort_GetInt32(107);
                        MyDevice.mTCP[sAddress].devc.tq_neg_point1  = mePort_GetInt32(111);
                        MyDevice.mTCP[sAddress].devc.tq_neg_point2  = mePort_GetInt32(115);
                        MyDevice.mTCP[sAddress].devc.tq_neg_point3  = mePort_GetInt32(119);
                        MyDevice.mTCP[sAddress].devc.tq_neg_point4  = mePort_GetInt32(123);
                        MyDevice.mTCP[sAddress].devc.tq_neg_point5  = mePort_GetInt32(127);

                        mePort_DataRemove(0x85);
                        isEQ = true;
                    }
                    else
                    {
                        mePort_DataRemove(1);
                        return;
                    }
                    break;

                case TASKS.REG_BLOCK5_CAL2:
                    if (len == 0xA5)
                    {
                        MyDevice.mTCP[sAddress].devc.cl2_ad_zero = mePort_GetInt32(3);
                        MyDevice.mTCP[sAddress].devc.cl2_ad_pos_point1 = mePort_GetInt32(7);
                        MyDevice.mTCP[sAddress].devc.cl2_ad_pos_point2 = mePort_GetInt32(11);
                        MyDevice.mTCP[sAddress].devc.cl2_ad_pos_point3 = mePort_GetInt32(15);
                        MyDevice.mTCP[sAddress].devc.cl2_ad_pos_point4 = mePort_GetInt32(19);
                        MyDevice.mTCP[sAddress].devc.cl2_ad_pos_point5 = mePort_GetInt32(23);
                        MyDevice.mTCP[sAddress].devc.cl2_ad_neg_point1 = mePort_GetInt32(27);
                        MyDevice.mTCP[sAddress].devc.cl2_ad_neg_point2 = mePort_GetInt32(31);
                        MyDevice.mTCP[sAddress].devc.cl2_ad_neg_point3 = mePort_GetInt32(35);
                        MyDevice.mTCP[sAddress].devc.cl2_ad_neg_point4 = mePort_GetInt32(39);
                        MyDevice.mTCP[sAddress].devc.cl2_ad_neg_point5 = mePort_GetInt32(43);
                        MyDevice.mTCP[sAddress].devc.cr1_ad_zero = mePort_GetInt32(47);
                        MyDevice.mTCP[sAddress].devc.cr1_ad_pos_point1 = mePort_GetInt32(51);
                        MyDevice.mTCP[sAddress].devc.cr1_ad_pos_point2 = mePort_GetInt32(55);
                        MyDevice.mTCP[sAddress].devc.cr1_ad_pos_point3 = mePort_GetInt32(59);
                        MyDevice.mTCP[sAddress].devc.cr1_ad_pos_point4 = mePort_GetInt32(63);
                        MyDevice.mTCP[sAddress].devc.cr1_ad_pos_point5 = mePort_GetInt32(67);
                        MyDevice.mTCP[sAddress].devc.cr1_ad_neg_point1 = mePort_GetInt32(71);
                        MyDevice.mTCP[sAddress].devc.cr1_ad_neg_point2 = mePort_GetInt32(75);
                        MyDevice.mTCP[sAddress].devc.cr1_ad_neg_point3 = mePort_GetInt32(79);
                        MyDevice.mTCP[sAddress].devc.cr1_ad_neg_point4 = mePort_GetInt32(83);
                        MyDevice.mTCP[sAddress].devc.cr1_ad_neg_point5 = mePort_GetInt32(87);
                        MyDevice.mTCP[sAddress].devc.cr2_ad_zero = mePort_GetInt32(91);
                        MyDevice.mTCP[sAddress].devc.cr2_ad_pos_point1 = mePort_GetInt32(95);
                        MyDevice.mTCP[sAddress].devc.cr2_ad_pos_point2 = mePort_GetInt32(99);
                        MyDevice.mTCP[sAddress].devc.cr2_ad_pos_point3 = mePort_GetInt32(103);
                        MyDevice.mTCP[sAddress].devc.cr2_ad_pos_point4 = mePort_GetInt32(107);
                        MyDevice.mTCP[sAddress].devc.cr2_ad_pos_point5 = mePort_GetInt32(111);
                        MyDevice.mTCP[sAddress].devc.cr2_ad_neg_point1 = mePort_GetInt32(115);
                        MyDevice.mTCP[sAddress].devc.cr2_ad_neg_point2 = mePort_GetInt32(119);
                        MyDevice.mTCP[sAddress].devc.cr2_ad_neg_point3 = mePort_GetInt32(123);
                        MyDevice.mTCP[sAddress].devc.cr2_ad_neg_point4 = mePort_GetInt32(127);
                        MyDevice.mTCP[sAddress].devc.cr2_ad_neg_point5 = mePort_GetInt32(131);

                        mePort_DataRemove(0xA5);
                        isEQ = true;
                    }
                    else
                    {
                        mePort_DataRemove(1);
                        return;
                    }
                    break;

                case TASKS.REG_BLOCK5_INFO:
                    if (len == 0xA5)
                    {
                        MyDevice.mTCP[sAddress].work.srno = mePort_GetUInt32(3);
                        MyDevice.mTCP[sAddress].work.number = mePort_GetUInt32(7);
                        MyDevice.mTCP[sAddress].work.mfgtime = mePort_GetUInt32(11);
                        MyDevice.mTCP[sAddress].work.caltime = mePort_GetUInt32(15);
                        MyDevice.mTCP[sAddress].work.calremind = mePort_GetUInt32(19);
                        MyDevice.mTCP[sAddress].work.name = mePort_GetString(35, 32);
                        MyDevice.mTCP[sAddress].work.managetxt = mePort_GetString(67, 32);
                        MyDevice.mTCP[sAddress].work.decription = mePort_GetString(99, 64);
                        mePort_DataRemove(0xA5);
                        isEQ = true;
                    }
                    else
                    {
                        mePort_DataRemove(1);
                        return;
                    }
                    break;

                case TASKS.REG_BLOCK3_WLAN:
                    if (len == 0x65)
                    {
                        MyDevice.mTCP[sAddress].wlan.addr          = mePort_GetByte(4);
                        MyDevice.mTCP[sAddress].wlan.rs485_baud    = mePort_GetByte(6);
                        MyDevice.mTCP[sAddress].wlan.rs485_stopbit = mePort_GetByte(8);
                        MyDevice.mTCP[sAddress].wlan.rs485_parity  = mePort_GetByte(10);
                        MyDevice.mTCP[sAddress].wlan.wifimode      = mePort_GetByte(12);
                        MyDevice.mTCP[sAddress].wlan.wf_ip         = $"{mePort_GetByte(14):X2}{mePort_GetByte(16):X2}{mePort_GetByte(18):X2}{mePort_GetByte(20):X2}";
                        MyDevice.mTCP[sAddress].wlan.wf_port       = Convert.ToUInt32(mePort_GetByte(21)) << 8 | Convert.ToUInt32(mePort_GetByte(22)); //两个字节分别位于高低位拼成Uint32
                        MyDevice.mTCP[sAddress].wlan.wf_ssid       = "";
                        MyDevice.mTCP[sAddress].wlan.wf_pwd        = "";
                        //备用两个字节

                        //wf_ssid 32个字节，以BEM46示例， 字节显示为 00 42 00 45 00 4D 00 34 00 36 ... 00
                        for (int i = 0; i < 16; i++)
                        {
                            if (mePort_GetByte((ushort)(23 + i * 2 + 1)) != 0x00)
                            {
                                MyDevice.mTCP[sAddress].wlan.wf_ssid += (char)mePort_GetByte((ushort)(23 + i * 2 + 1));
                            }
                        }
                        //wf_pwd 32个字节
                        for (int i = 0; i < 16; i++)
                        {
                            if (mePort_GetByte((ushort)(55 + i * 2 + 1)) != 0x00)
                            {
                                MyDevice.mTCP[sAddress].wlan.wf_pwd += (char)mePort_GetByte((ushort)(55 + i * 2 + 1));
                            }
                        }

                        MyDevice.mTCP[sAddress].wlan.rf_chan   = mePort_GetByte(88);
                        MyDevice.mTCP[sAddress].wlan.rf_option = mePort_GetByte(90);
                        MyDevice.mTCP[sAddress].wlan.rf_para   = mePort_GetByte(92);

                        mePort_DataRemove(0x65);
                        isEQ = true;
                    }
                    else
                    {
                        mePort_DataRemove(1);
                        return;
                    }
                    break;

                case TASKS.REG_BLOCK3_PARA:
                    if (len == 0x65)
                    {
                        MyDevice.mTCP[sAddress].para.torque_unit     = (UNIT)mePort_GetByte(4);
                        MyDevice.mTCP[sAddress].para.angle_speed     = mePort_GetByte(6);
                        MyDevice.mTCP[sAddress].para.angle_decimal   = mePort_GetByte(8);
                        MyDevice.mTCP[sAddress].para.mode_pt         = mePort_GetByte(10);
                        MyDevice.mTCP[sAddress].para.mode_ax         = mePort_GetByte(12);
                        MyDevice.mTCP[sAddress].para.mode_mx         = mePort_GetByte(14);
                        MyDevice.mTCP[sAddress].para.fifomode        = mePort_GetByte(16);
                        MyDevice.mTCP[sAddress].para.fiforec         = mePort_GetByte(18);
                        MyDevice.mTCP[sAddress].para.fifospeed       = mePort_GetByte(20);
                        MyDevice.mTCP[sAddress].para.heartformat     = mePort_GetByte(22);
                        MyDevice.mTCP[sAddress].para.heartcount      = mePort_GetByte(24);
                        MyDevice.mTCP[sAddress].para.heartcycle      = mePort_GetUInt16(25);
                        MyDevice.mTCP[sAddress].para.accmode         = mePort_GetByte(28);
                        MyDevice.mTCP[sAddress].para.alarmode        = mePort_GetByte(30);
                        MyDevice.mTCP[sAddress].para.timeoff         = mePort_GetByte(32);
                        MyDevice.mTCP[sAddress].para.timeback        = mePort_GetByte(34);
                        MyDevice.mTCP[sAddress].para.timezero        = mePort_GetByte(36);
                        MyDevice.mTCP[sAddress].para.disptype        = mePort_GetByte(38);
                        MyDevice.mTCP[sAddress].para.disptheme       = mePort_GetByte(40);
                        MyDevice.mTCP[sAddress].para.displan         = mePort_GetByte(42);
                        MyDevice.mTCP[sAddress].para.unhook          = mePort_GetUInt16(43);
                        MyDevice.mTCP[sAddress].para.adspeed         = mePort_GetByte(46);
                        MyDevice.mTCP[sAddress].para.autozero        = (AUTOZERO)mePort_GetByte(48);
                        MyDevice.mTCP[sAddress].para.trackzero       = (TRACKZERO)mePort_GetByte(50);
                        MyDevice.mTCP[sAddress].para.amenable        = mePort_GetUInt16(51);
                        MyDevice.mTCP[sAddress].para.screwmax        = mePort_GetByte(54);
                        MyDevice.mTCP[sAddress].para.runmode         = mePort_GetByte(56);
                        MyDevice.mTCP[sAddress].para.auploaden       = mePort_GetByte(58);
                        MyDevice.mTCP[sAddress].para.devrole         = mePort_GetByte(60);

                        MyDevice.mTCP[sAddress].para.angcorr         = mePort_GetFloat(67);
                        MyDevice.mTCP[sAddress].para.angle_resist    = mePort_GetInt32(71);

                        mePort_DataRemove(0x65);
                        isEQ = true;
                    }
                    else
                    {
                        mePort_DataRemove(1);
                        return;
                    }
                    break;

                case TASKS.REG_BLOCK5_AM1:
                    if (len == 0xA5)
                    {
                        switch (MyDevice.mTCP[sAddress].para.torque_unit)
                        {
                            case UNIT.UNIT_nm:
                                for (int i = 0; i < 10; i++)
                                {
                                    MyDevice.mTCP[sAddress].alam.SN_target[i, 0] = mePort_GetInt32((UInt16)(3 + 4 * i));
                                    MyDevice.mTCP[sAddress].alam.SN_target[i, 1] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(3 + 4 * i)), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.SN_target[i, 2] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(3 + 4 * i)), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.SN_target[i, 3] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(3 + 4 * i)), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.SN_target[i, 4] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(3 + 4 * i)), (byte)UNIT.UNIT_kgm);

                                    MyDevice.mTCP[sAddress].alam.SA_pre[i, 0] = mePort_GetInt32((UInt16)(43 + 8 * i));
                                    MyDevice.mTCP[sAddress].alam.SA_pre[i, 1] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(43 + 8 * i)), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.SA_pre[i, 2] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(43 + 8 * i)), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.SA_pre[i, 3] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(43 + 8 * i)), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.SA_pre[i, 4] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(43 + 8 * i)), (byte)UNIT.UNIT_kgm);

                                    MyDevice.mTCP[sAddress].alam.SA_ang[i] = mePort_GetInt32((UInt16)(47 + 8 * i));
                                }

                                for (int i = 0; i < 5; i++)
                                {
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 0] = mePort_GetInt32((UInt16)(123 + 8 * i));
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 1] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(123 + 8 * i)), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 2] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(123 + 8 * i)), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 3] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(123 + 8 * i)), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 4] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(123 + 8 * i)), (byte)UNIT.UNIT_kgm);

                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 0] = mePort_GetInt32((UInt16)(127 + 8 * i));
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 1] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(127 + 8 * i)), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 2] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(127 + 8 * i)), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 3] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(127 + 8 * i)), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 4] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(127 + 8 * i)), (byte)UNIT.UNIT_kgm);
                                }
                                break;

                            case UNIT.UNIT_lbfin:
                                for (int i = 0; i < 10; i++)
                                {
                                    MyDevice.mTCP[sAddress].alam.SN_target[i, 0] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(3 + 4 * i)), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.SN_target[i, 1] = mePort_GetInt32((UInt16)(3 + 4 * i));
                                    MyDevice.mTCP[sAddress].alam.SN_target[i, 2] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(3 + 4 * i)), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.SN_target[i, 3] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(3 + 4 * i)), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.SN_target[i, 4] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(3 + 4 * i)), (byte)UNIT.UNIT_kgm);

                                    MyDevice.mTCP[sAddress].alam.SA_pre[i, 0] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(43 + 8 * i)), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.SA_pre[i, 1] = mePort_GetInt32((UInt16)(43 + 8 * i));
                                    MyDevice.mTCP[sAddress].alam.SA_pre[i, 2] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(43 + 8 * i)), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.SA_pre[i, 3] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(43 + 8 * i)), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.SA_pre[i, 4] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(43 + 8 * i)), (byte)UNIT.UNIT_kgm);

                                    MyDevice.mTCP[sAddress].alam.SA_ang[i] = mePort_GetInt32((UInt16)(47 + 8 * i));
                                }

                                for (int i = 0; i < 5; i++)
                                {
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 0] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(123 + 8 * i)), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 1] = mePort_GetInt32((UInt16)(123 + 8 * i));
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 2] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(123 + 8 * i)), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 3] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(123 + 8 * i)), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 4] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(123 + 8 * i)), (byte)UNIT.UNIT_kgm);

                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 0] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(127 + 8 * i)), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 1] = mePort_GetInt32((UInt16)(127 + 8 * i));
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 2] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(127 + 8 * i)), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 3] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(127 + 8 * i)), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 4] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(127 + 8 * i)), (byte)UNIT.UNIT_kgm);
                                }
                                break;

                            case UNIT.UNIT_lbfft:
                                for (int i = 0; i < 10; i++)
                                {
                                    MyDevice.mTCP[sAddress].alam.SN_target[i, 0] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(3 + 4 * i)), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.SN_target[i, 1] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(3 + 4 * i)), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.SN_target[i, 2] = mePort_GetInt32((UInt16)(3 + 4 * i));
                                    MyDevice.mTCP[sAddress].alam.SN_target[i, 3] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(3 + 4 * i)), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.SN_target[i, 4] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(3 + 4 * i)), (byte)UNIT.UNIT_kgm);

                                    MyDevice.mTCP[sAddress].alam.SA_pre[i, 0] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(43 + 8 * i)), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.SA_pre[i, 1] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(43 + 8 * i)), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.SA_pre[i, 2] = mePort_GetInt32((UInt16)(43 + 8 * i));
                                    MyDevice.mTCP[sAddress].alam.SA_pre[i, 3] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(43 + 8 * i)), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.SA_pre[i, 4] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(43 + 8 * i)), (byte)UNIT.UNIT_kgm);

                                    MyDevice.mTCP[sAddress].alam.SA_ang[i] = mePort_GetInt32((UInt16)(47 + 8 * i));
                                }

                                for (int i = 0; i < 5; i++)
                                {
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 0] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(123 + 8 * i)), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 1] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(123 + 8 * i)), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 2] = mePort_GetInt32((UInt16)(123 + 8 * i));
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 3] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(123 + 8 * i)), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 4] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(123 + 8 * i)), (byte)UNIT.UNIT_kgm);

                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 0] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(127 + 8 * i)), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 1] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(127 + 8 * i)), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 2] = mePort_GetInt32((UInt16)(127 + 8 * i)); ;
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 3] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(127 + 8 * i)), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 4] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(127 + 8 * i)), (byte)UNIT.UNIT_kgm);
                                }
                                break;

                            case UNIT.UNIT_kgcm:
                                for (int i = 0; i < 10; i++)
                                {
                                    MyDevice.mTCP[sAddress].alam.SN_target[i, 0] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(3 + 4 * i)), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.SN_target[i, 1] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(3 + 4 * i)), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.SN_target[i, 2] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(3 + 4 * i)), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.SN_target[i, 3] = mePort_GetInt32((UInt16)(3 + 4 * i));
                                    MyDevice.mTCP[sAddress].alam.SN_target[i, 4] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(3 + 4 * i)), (byte)UNIT.UNIT_kgm);

                                    MyDevice.mTCP[sAddress].alam.SA_pre[i, 0] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(43 + 8 * i)), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.SA_pre[i, 1] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(43 + 8 * i)), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.SA_pre[i, 2] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(43 + 8 * i)), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.SA_pre[i, 3] = mePort_GetInt32((UInt16)(43 + 8 * i));
                                    MyDevice.mTCP[sAddress].alam.SA_pre[i, 4] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(43 + 8 * i)), (byte)UNIT.UNIT_kgm);

                                    MyDevice.mTCP[sAddress].alam.SA_ang[i] = mePort_GetInt32((UInt16)(47 + 8 * i));
                                }

                                for (int i = 0; i < 5; i++)
                                {
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 0] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(123 + 8 * i)), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 1] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(123 + 8 * i)), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 2] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(123 + 8 * i)), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 3] = mePort_GetInt32((UInt16)(123 + 8 * i));
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 4] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(123 + 8 * i)), (byte)UNIT.UNIT_kgm);

                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 0] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(127 + 8 * i)), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 1] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(127 + 8 * i)), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 2] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(127 + 8 * i)), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 3] = mePort_GetInt32((UInt16)(127 + 8 * i));
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 4] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(127 + 8 * i)), (byte)UNIT.UNIT_kgm);
                                }
                                break;

                            case UNIT.UNIT_kgm:
                                for (int i = 0; i < 10; i++)
                                {
                                    MyDevice.mTCP[sAddress].alam.SN_target[i, 0] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(3 + 4 * i)), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.SN_target[i, 1] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(3 + 4 * i)), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.SN_target[i, 2] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(3 + 4 * i)), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.SN_target[i, 3] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(3 + 4 * i)), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.SN_target[i, 4] = mePort_GetInt32((UInt16)(3 + 4 * i));

                                    MyDevice.mTCP[sAddress].alam.SA_pre[i, 0] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(43 + 8 * i)), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.SA_pre[i, 1] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(43 + 8 * i)), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.SA_pre[i, 2] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(43 + 8 * i)), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.SA_pre[i, 3] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(43 + 8 * i)), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.SA_pre[i, 4] = mePort_GetInt32((UInt16)(43 + 8 * i));

                                    MyDevice.mTCP[sAddress].alam.SA_ang[i] = mePort_GetInt32((UInt16)(47 + 8 * i));
                                }

                                for (int i = 0; i < 5; i++)
                                {
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 0] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(123 + 8 * i)), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 1] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(123 + 8 * i)), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 2] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(123 + 8 * i)), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 3] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(123 + 8 * i)), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 4] = mePort_GetInt32((UInt16)(123 + 8 * i));

                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 0] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(127 + 8 * i)), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 1] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(127 + 8 * i)), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 2] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(127 + 8 * i)), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 3] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(127 + 8 * i)), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 4] = mePort_GetInt32((UInt16)(127 + 8 * i));
                                }
                                break;

                            default:
                                break;
                        }

                        mePort_DataRemove(0xA5);
                        isEQ = true;
                    }
                    else
                    {
                        mePort_DataRemove(1);
                        return;
                    }
                    break;

                case TASKS.REG_BLOCK5_AM2:
                    if (len == 0xA5)
                    {
                        switch (MyDevice.mTCP[sAddress].para.torque_unit)
                        {
                            case UNIT.UNIT_nm:
                                for (int i = 5; i < 10; i++)
                                {
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 0] = mePort_GetInt32((UInt16)(3 + 8 * (i - 5)));
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 1] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(3 + 8 * (i - 5))), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 2] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(3 + 8 * (i - 5))), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 3] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(3 + 8 * (i - 5))), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 4] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(3 + 8 * (i - 5))), (byte)UNIT.UNIT_kgm);

                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 0] = mePort_GetInt32((UInt16)(7 + 8 * (i - 5)));
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 1] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(7 + 8 * (i - 5))), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 2] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(7 + 8 * (i - 5))), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 3] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(7 + 8 * (i - 5))), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 4] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(7 + 8 * (i - 5))), (byte)UNIT.UNIT_kgm);
                                }

                                for (int i = 0; i < 10; i++)
                                {
                                    MyDevice.mTCP[sAddress].alam.MA_pre[i, 0] = mePort_GetInt32((UInt16)(43 + 12 * i));
                                    MyDevice.mTCP[sAddress].alam.MA_pre[i, 1] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(43 + 12 * i)), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.MA_pre[i, 2] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(43 + 12 * i)), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.MA_pre[i, 3] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(43 + 12 * i)), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.MA_pre[i, 4] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(43 + 12 * i)), (byte)UNIT.UNIT_kgm);

                                    MyDevice.mTCP[sAddress].alam.MA_low[i] = mePort_GetInt32((UInt16)(47 + 12 * i));

                                    MyDevice.mTCP[sAddress].alam.MA_high[i] = mePort_GetInt32((UInt16)(51 + 12 * i));
                                }
                                break;

                            case UNIT.UNIT_lbfin:
                                for (int i = 5; i < 10; i++)
                                {
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 0] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(3 + 8 * (i - 5))), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 1] = mePort_GetInt32((UInt16)(3 + 8 * (i - 5)));
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 2] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(3 + 8 * (i - 5))), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 3] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(3 + 8 * (i - 5))), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 4] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(3 + 8 * (i - 5))), (byte)UNIT.UNIT_kgm);

                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 0] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(7 + 8 * (i - 5))), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 1] = mePort_GetInt32((UInt16)(7 + 8 * (i - 5)));
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 2] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(7 + 8 * (i - 5))), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 3] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(7 + 8 * (i - 5))), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 4] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(7 + 8 * (i - 5))), (byte)UNIT.UNIT_kgm);
                                }

                                for (int i = 0; i < 10; i++)
                                {
                                    MyDevice.mTCP[sAddress].alam.MA_pre[i, 0] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(43 + 12 * i)), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.MA_pre[i, 1] = mePort_GetInt32((UInt16)(43 + 12 * i));
                                    MyDevice.mTCP[sAddress].alam.MA_pre[i, 2] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(43 + 12 * i)), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.MA_pre[i, 3] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(43 + 12 * i)), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.MA_pre[i, 4] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(43 + 12 * i)), (byte)UNIT.UNIT_kgm);

                                    MyDevice.mTCP[sAddress].alam.MA_low[i] = mePort_GetInt32((UInt16)(47 + 12 * i));

                                    MyDevice.mTCP[sAddress].alam.MA_high[i] = mePort_GetInt32((UInt16)(51 + 12 * i));
                                }
                                break;

                            case UNIT.UNIT_lbfft:
                                for (int i = 5; i < 10; i++)
                                {
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 0] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(3 + 8 * (i - 5))), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 1] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(3 + 8 * (i - 5))), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 2] = mePort_GetInt32((UInt16)(3 + 8 * (i - 5)));
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 3] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(3 + 8 * (i - 5))), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 4] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(3 + 8 * (i - 5))), (byte)UNIT.UNIT_kgm);

                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 0] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(7 + 8 * (i - 5))), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 1] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(7 + 8 * (i - 5))), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 2] = mePort_GetInt32((UInt16)(7 + 8 * (i - 5)));
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 3] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(7 + 8 * (i - 5))), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 4] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(7 + 8 * (i - 5))), (byte)UNIT.UNIT_kgm);
                                }

                                for (int i = 0; i < 10; i++)
                                {
                                    MyDevice.mTCP[sAddress].alam.MA_pre[i, 0] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(43 + 12 * i)), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.MA_pre[i, 1] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(43 + 12 * i)), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.MA_pre[i, 2] = mePort_GetInt32((UInt16)(43 + 12 * i));
                                    MyDevice.mTCP[sAddress].alam.MA_pre[i, 3] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(43 + 12 * i)), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.MA_pre[i, 4] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(43 + 12 * i)), (byte)UNIT.UNIT_kgm);

                                    MyDevice.mTCP[sAddress].alam.MA_low[i] = mePort_GetInt32((UInt16)(47 + 12 * i));

                                    MyDevice.mTCP[sAddress].alam.MA_high[i] = mePort_GetInt32((UInt16)(51 + 12 * i));
                                }
                                break;

                            case UNIT.UNIT_kgcm:
                                for (int i = 5; i < 10; i++)
                                {
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 0] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(3 + 8 * (i - 5))), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 1] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(3 + 8 * (i - 5))), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 2] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(3 + 8 * (i - 5))), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 3] = mePort_GetInt32((UInt16)(3 + 8 * (i - 5)));
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 4] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(3 + 8 * (i - 5))), (byte)UNIT.UNIT_kgm);

                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 0] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(7 + 8 * (i - 5))), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 1] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(7 + 8 * (i - 5))), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 2] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(7 + 8 * (i - 5))), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 3] = mePort_GetInt32((UInt16)(7 + 8 * (i - 5)));
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 4] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(7 + 8 * (i - 5))), (byte)UNIT.UNIT_kgm);
                                }

                                for (int i = 0; i < 10; i++)
                                {
                                    MyDevice.mTCP[sAddress].alam.MA_pre[i, 0] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(43 + 12 * i)), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.MA_pre[i, 1] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(43 + 12 * i)), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.MA_pre[i, 2] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(43 + 12 * i)), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.MA_pre[i, 3] = mePort_GetInt32((UInt16)(43 + 12 * i));
                                    MyDevice.mTCP[sAddress].alam.MA_pre[i, 4] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(43 + 12 * i)), (byte)UNIT.UNIT_kgm);

                                    MyDevice.mTCP[sAddress].alam.MA_low[i] = mePort_GetInt32((UInt16)(47 + 12 * i));

                                    MyDevice.mTCP[sAddress].alam.MA_high[i] = mePort_GetInt32((UInt16)(51 + 12 * i));
                                }
                                break;

                            case UNIT.UNIT_kgm:
                                for (int i = 5; i < 10; i++)
                                {
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 0] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(3 + 8 * (i - 5))), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 1] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(3 + 8 * (i - 5))), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 2] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(3 + 8 * (i - 5))), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 3] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(3 + 8 * (i - 5))), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.MN_low[i, 4] = mePort_GetInt32((UInt16)(3 + 8 * (i - 5)));

                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 0] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(7 + 8 * (i - 5))), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 1] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(7 + 8 * (i - 5))), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 2] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(7 + 8 * (i - 5))), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 3] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(7 + 8 * (i - 5))), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.MN_high[i, 4] = mePort_GetInt32((UInt16)(7 + 8 * (i - 5)));
                                }

                                for (int i = 0; i < 10; i++)
                                {
                                    MyDevice.mTCP[sAddress].alam.MA_pre[i, 0] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(43 + 12 * i)), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.MA_pre[i, 1] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(43 + 12 * i)), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.MA_pre[i, 2] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(43 + 12 * i)), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.MA_pre[i, 3] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(43 + 12 * i)), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.MA_pre[i, 4] = mePort_GetInt32((UInt16)(43 + 12 * i));

                                    MyDevice.mTCP[sAddress].alam.MA_low[i] = mePort_GetInt32((UInt16)(47 + 12 * i));

                                    MyDevice.mTCP[sAddress].alam.MA_high[i] = mePort_GetInt32((UInt16)(51 + 12 * i));
                                }
                                break;

                            default:
                                break;
                        }
                        mePort_DataRemove(0xA5);
                        isEQ = true;
                    }
                    else
                    {
                        mePort_DataRemove(1);
                        return;
                    }
                    break;

                case TASKS.REG_BLOCK5_AM3:
                    if (len == 0xA5)
                    {
                        switch (MyDevice.mTCP[sAddress].para.torque_unit)
                        {
                            case UNIT.UNIT_nm:
                                for (int i = 0; i < 10; i++)
                                {
                                    MyDevice.mTCP[sAddress].alam.AZ_start[i, 0] = mePort_GetInt32((UInt16)(3 + 12 * i));
                                    MyDevice.mTCP[sAddress].alam.AZ_start[i, 1] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(3 + 12 * i)), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.AZ_start[i, 2] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(3 + 12 * i)), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.AZ_start[i, 3] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(3 + 12 * i)), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.AZ_start[i, 4] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(3 + 12 * i)), (byte)UNIT.UNIT_kgm);

                                    MyDevice.mTCP[sAddress].alam.AZ_stop[i, 0] = mePort_GetInt32((UInt16)(7 + 12 * i));
                                    MyDevice.mTCP[sAddress].alam.AZ_stop[i, 1] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(7 + 12 * i)), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.AZ_stop[i, 2] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(7 + 12 * i)), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.AZ_stop[i, 3] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(7 + 12 * i)), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.AZ_stop[i, 4] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(7 + 12 * i)), (byte)UNIT.UNIT_kgm);

                                    MyDevice.mTCP[sAddress].alam.AZ_hock[i, 0] = mePort_GetInt32((UInt16)(11 + 12 * i));
                                    MyDevice.mTCP[sAddress].alam.AZ_hock[i, 1] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(11 + 12 * i)), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.AZ_hock[i, 2] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(11 + 12 * i)), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.AZ_hock[i, 3] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(11 + 12 * i)), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.AZ_hock[i, 4] = UnitConvert.Torque_nmTrans(mePort_GetInt32((UInt16)(11 + 12 * i)), (byte)UNIT.UNIT_kgm);
                                }
                                break;

                            case UNIT.UNIT_lbfin:
                                for (int i = 0; i < 10; i++)
                                {
                                    MyDevice.mTCP[sAddress].alam.AZ_start[i, 0] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(3 + 12 * i)), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.AZ_start[i, 1] = mePort_GetInt32((UInt16)(3 + 12 * i));
                                    MyDevice.mTCP[sAddress].alam.AZ_start[i, 2] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(3 + 12 * i)), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.AZ_start[i, 3] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(3 + 12 * i)), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.AZ_start[i, 4] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(3 + 12 * i)), (byte)UNIT.UNIT_kgm);

                                    MyDevice.mTCP[sAddress].alam.AZ_stop[i, 0] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(7 + 12 * i)), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.AZ_stop[i, 1] = mePort_GetInt32((UInt16)(7 + 12 * i));
                                    MyDevice.mTCP[sAddress].alam.AZ_stop[i, 2] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(7 + 12 * i)), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.AZ_stop[i, 3] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(7 + 12 * i)), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.AZ_stop[i, 4] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(7 + 12 * i)), (byte)UNIT.UNIT_kgm);

                                    MyDevice.mTCP[sAddress].alam.AZ_hock[i, 0] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(11 + 12 * i)), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.AZ_hock[i, 1] = mePort_GetInt32((UInt16)(11 + 12 * i));
                                    MyDevice.mTCP[sAddress].alam.AZ_hock[i, 2] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(11 + 12 * i)), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.AZ_hock[i, 3] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(11 + 12 * i)), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.AZ_hock[i, 4] = UnitConvert.Torque_lbfinTrans(mePort_GetInt32((UInt16)(11 + 12 * i)), (byte)UNIT.UNIT_kgm);
                                }
                                break;

                            case UNIT.UNIT_lbfft:
                                for (int i = 0; i < 10; i++)
                                {
                                    MyDevice.mTCP[sAddress].alam.AZ_start[i, 0] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(3 + 12 * i)), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.AZ_start[i, 1] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(3 + 12 * i)), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.AZ_start[i, 2] = mePort_GetInt32((UInt16)(3 + 12 * i));
                                    MyDevice.mTCP[sAddress].alam.AZ_start[i, 3] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(3 + 12 * i)), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.AZ_start[i, 4] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(3 + 12 * i)), (byte)UNIT.UNIT_kgm);

                                    MyDevice.mTCP[sAddress].alam.AZ_stop[i, 0] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(7 + 12 * i)), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.AZ_stop[i, 1] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(7 + 12 * i)), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.AZ_stop[i, 2] = mePort_GetInt32((UInt16)(7 + 12 * i));
                                    MyDevice.mTCP[sAddress].alam.AZ_stop[i, 3] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(7 + 12 * i)), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.AZ_stop[i, 4] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(7 + 12 * i)), (byte)UNIT.UNIT_kgm);

                                    MyDevice.mTCP[sAddress].alam.AZ_hock[i, 0] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(11 + 12 * i)), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.AZ_hock[i, 1] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(11 + 12 * i)), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.AZ_hock[i, 2] = mePort_GetInt32((UInt16)(11 + 12 * i));
                                    MyDevice.mTCP[sAddress].alam.AZ_hock[i, 3] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(11 + 12 * i)), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.AZ_hock[i, 4] = UnitConvert.Torque_lbfftTrans(mePort_GetInt32((UInt16)(11 + 12 * i)), (byte)UNIT.UNIT_kgm);
                                }
                                break;

                            case UNIT.UNIT_kgcm:
                                for (int i = 0; i < 10; i++)
                                {
                                    MyDevice.mTCP[sAddress].alam.AZ_start[i, 0] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(3 + 12 * i)), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.AZ_start[i, 1] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(3 + 12 * i)), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.AZ_start[i, 2] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(3 + 12 * i)), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.AZ_start[i, 3] = mePort_GetInt32((UInt16)(3 + 12 * i));
                                    MyDevice.mTCP[sAddress].alam.AZ_start[i, 4] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(3 + 12 * i)), (byte)UNIT.UNIT_kgm);

                                    MyDevice.mTCP[sAddress].alam.AZ_stop[i, 0] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(7 + 12 * i)), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.AZ_stop[i, 1] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(7 + 12 * i)), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.AZ_stop[i, 2] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(7 + 12 * i)), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.AZ_stop[i, 3] = mePort_GetInt32((UInt16)(7 + 12 * i));
                                    MyDevice.mTCP[sAddress].alam.AZ_stop[i, 4] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(7 + 12 * i)), (byte)UNIT.UNIT_kgm);

                                    MyDevice.mTCP[sAddress].alam.AZ_hock[i, 0] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(11 + 12 * i)), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.AZ_hock[i, 1] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(11 + 12 * i)), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.AZ_hock[i, 2] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(11 + 12 * i)), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.AZ_hock[i, 3] = mePort_GetInt32((UInt16)(11 + 12 * i));
                                    MyDevice.mTCP[sAddress].alam.AZ_hock[i, 4] = UnitConvert.Torque_kgfcmTrans(mePort_GetInt32((UInt16)(11 + 12 * i)), (byte)UNIT.UNIT_kgm);
                                }
                                break;

                            case UNIT.UNIT_kgm:
                                for (int i = 0; i < 10; i++)
                                {
                                    MyDevice.mTCP[sAddress].alam.AZ_start[i, 0] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(3 + 12 * i)), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.AZ_start[i, 1] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(3 + 12 * i)), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.AZ_start[i, 2] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(3 + 12 * i)), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.AZ_start[i, 3] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(3 + 12 * i)), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.AZ_start[i, 4] = mePort_GetInt32((UInt16)(3 + 12 * i));

                                    MyDevice.mTCP[sAddress].alam.AZ_stop[i, 0] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(7 + 12 * i)), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.AZ_stop[i, 1] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(7 + 12 * i)), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.AZ_stop[i, 2] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(7 + 12 * i)), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.AZ_stop[i, 3] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(7 + 12 * i)), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.AZ_stop[i, 4] = mePort_GetInt32((UInt16)(7 + 12 * i));

                                    MyDevice.mTCP[sAddress].alam.AZ_hock[i, 0] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(11 + 12 * i)), (byte)UNIT.UNIT_nm);
                                    MyDevice.mTCP[sAddress].alam.AZ_hock[i, 1] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(11 + 12 * i)), (byte)UNIT.UNIT_lbfin);
                                    MyDevice.mTCP[sAddress].alam.AZ_hock[i, 2] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(11 + 12 * i)), (byte)UNIT.UNIT_lbfft);
                                    MyDevice.mTCP[sAddress].alam.AZ_hock[i, 3] = UnitConvert.Torque_kgfmTrans(mePort_GetInt32((UInt16)(11 + 12 * i)), (byte)UNIT.UNIT_kgcm);
                                    MyDevice.mTCP[sAddress].alam.AZ_hock[i, 4] = mePort_GetInt32((UInt16)(11 + 12 * i));
                                }
                                break;

                            default:
                                break;
                        }
                        MyDevice.mTCP[sAddress].sTATE = STATE.CONNECTED;//扳手连接状态
                        mePort_DataRemove(0xA5);
                        isEQ = true;
                    }
                    else
                    {
                        mePort_DataRemove(1);
                        return;
                    }
                    break;

                case TASKS.REG_BLOCK3_JOB:
                    if (len == 0x65)
                    {
                        MyDevice.mTCP[sAddress].work.wo_area = mePort_GetUInt32(3);
                        MyDevice.mTCP[sAddress].work.wo_factory = mePort_GetUInt32(7);
                        MyDevice.mTCP[sAddress].work.wo_line = mePort_GetUInt32(11);
                        MyDevice.mTCP[sAddress].work.wo_station = mePort_GetUInt32(15);
                        //预留字节16个
                        MyDevice.mTCP[sAddress].work.wo_stamp = mePort_GetUInt32(35);
                        MyDevice.mTCP[sAddress].work.wo_bat = mePort_GetUInt32(39);
                        MyDevice.mTCP[sAddress].work.wo_num = mePort_GetUInt32(43);
                        MyDevice.mTCP[sAddress].work.wo_name = mePort_GetString(47, 40);
                        mePort_DataRemove(0x65);
                        isEQ = true;
                    }
                    else
                    {
                        mePort_DataRemove(1);
                        return;
                    }
                    break;

                case TASKS.REG_BLOCK3_OP:
                    if (len == 0x65)
                    {
                        MyDevice.mTCP[sAddress].work.user_ID = mePort_GetByte(6);
                        MyDevice.mTCP[sAddress].work.user_name = mePort_GetString(7, 48);
                        mePort_DataRemove(0x65);
                        isEQ = true;
                    }
                    else
                    {
                        mePort_DataRemove(1);
                        return;
                    }
                    break;

                case TASKS.REG_BLOCK1_FIFO:
                    if (len == 0x25)
                    {
                        MyDevice.mTCP[sAddress].fifo.full = Convert.ToBoolean(mePort_GetInt16(3));
                        MyDevice.mTCP[sAddress].fifo.empty = Convert.ToBoolean(mePort_GetInt16(5));
                        MyDevice.mTCP[sAddress].fifo.size = mePort_GetUInt32(7);
                        MyDevice.mTCP[sAddress].fifo.count = mePort_GetUInt32(11);
                        MyDevice.mTCP[sAddress].fifo.read = mePort_GetUInt32(15);
                        MyDevice.mTCP[sAddress].fifo.write = mePort_GetUInt32(19);
                        mePort_DataRemove(0x25);
                        isEQ = true;
                    }
                    else
                    {
                        mePort_DataRemove(1);
                        return;
                    }
                    break;

                case TASKS.REG_BLOCK2_DAT:
                    if ((MyDevice.mTCP[sAddress].devc.type == TYPE.TQ_XH_XL01_08 - (UInt16)ADDROFFSET.TQ_XH_ADDR && MyDevice.mTCP[sAddress].devc.version >= 11) ||
                        (MyDevice.mTCP[sAddress].devc.type == TYPE.TQ_XH_XL01_07 - (UInt16)ADDROFFSET.TQ_XH_ADDR && MyDevice.mTCP[sAddress].devc.version >= 41))
                    {
                        if (len == 0x52 * 2 + 5)
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                MyDevice.mTCP[sAddress].data[i] = new DATA(); //创建新的引用，避免集合添加时引用重复
                            }

                            //第一包
                            MyDevice.mTCP[sAddress].fifo.index = mePort_GetUInt32(3);
                            MyDevice.mTCP[sAddress].data[0].stamp = mePort_GetUInt32(7);
                            MyDevice.mTCP[sAddress].data[0].dtype = mePort_GetByte(11);
                            if (MyDevice.mTCP[sAddress].data[0].dtype == 0xF1)       //01过程帧
                            {
                                MyDevice.mTCP[sAddress].data[0].torque_unit  = (UNIT)mePort_GetByte(12);
                                MyDevice.mTCP[sAddress].data[0].torque       = mePort_GetInt32(13);
                                MyDevice.mTCP[sAddress].data[0].torseries_pk = mePort_GetInt32(17);
                                MyDevice.mTCP[sAddress].data[0].angle        = mePort_GetInt32(21);
                                MyDevice.mTCP[sAddress].data[0].angle_acc    = mePort_GetInt32(25);
                                MyDevice.mTCP[sAddress].data[0].mode_pt      = mePort_GetByte(29);
                                MyDevice.mTCP[sAddress].data[0].mode_ax      = mePort_GetByte(30);
                                MyDevice.mTCP[sAddress].data[0].mode_mx      = mePort_GetByte(31);
                                MyDevice.mTCP[sAddress].data[0].battery      = mePort_GetByte(32);
                            }
                            else if (MyDevice.mTCP[sAddress].data[0].dtype == 0xF2)  //02一次结果帧
                            {
                                if (MyDevice.mTCP[sAddress].devc.version >= 39) tempStamp = MyDevice.mTCP[sAddress].data[0].stamp;

                                MyDevice.mTCP[sAddress].data[0].mark          = mePort_GetByte(12);
                                MyDevice.mTCP[sAddress].data[0].torque_unit   = (UNIT)mePort_GetByte(13);
                                MyDevice.mTCP[sAddress].data[0].angle_decimal = mePort_GetByte(14);
                                MyDevice.mTCP[sAddress].data[0].torseries_pk  = mePort_GetInt32(15);
                                MyDevice.mTCP[sAddress].data[0].angle_acc     = mePort_GetInt32(19);
                                MyDevice.mTCP[sAddress].data[0].begin_series  = mePort_GetUInt32(23);
                                MyDevice.mTCP[sAddress].data[0].begin_group   = mePort_GetUInt32(27);
                                MyDevice.mTCP[sAddress].data[0].len           = mePort_GetUInt16(31);
                            }
                            else if (MyDevice.mTCP[sAddress].data[0].dtype == 0xF3)  //03一组结果帧
                            {
                                if (MyDevice.mTCP[sAddress].devc.version >= 39)
                                {
                                    MyDevice.mTCP[sAddress].data[0].stamp = tempStamp;
                                    MyDevice.mTCP[sAddress].data[0].angle_resist = mePort_GetInt32(7);
                                }

                                MyDevice.mTCP[sAddress].data[0].mark         = mePort_GetByte(12);
                                MyDevice.mTCP[sAddress].data[0].mode_ax      = mePort_GetByte(13);
                                MyDevice.mTCP[sAddress].data[0].mode_mx      = mePort_GetByte(14);
                                MyDevice.mTCP[sAddress].data[0].torgroup_pk  = mePort_GetInt32(15);
                                MyDevice.mTCP[sAddress].data[0].angle_acc    = mePort_GetInt32(19);
                                MyDevice.mTCP[sAddress].data[0].angle_resist = mePort_GetInt32(23);
                                MyDevice.mTCP[sAddress].data[0].alarm[0]     = mePort_GetInt32(27);
                                MyDevice.mTCP[sAddress].data[0].alarm[1]     = mePort_GetInt32(31);
                                MyDevice.mTCP[sAddress].data[0].alarm[2]     = mePort_GetInt32(35);
                            }
                            else if (MyDevice.mTCP[sAddress].data[0].dtype == 0xF4)  //04一组工单结果帧
                            {
                                //MyDevice.mTCP[sAddress].data[0].mark = mePort_GetByte(12);
                                //MyDevice.mTCP[sAddress].data[0].mode = mePort_GetByte(13);
                                //MyDevice.mTCP[sAddress].data[0].screwCnt = mePort_GetByte(14);
                                //MyDevice.mTCP[sAddress].data[0].work_num = mePort_GetUInt32(15);
                                //MyDevice.mTCP[sAddress].data[0].work_psq = (ulong)(mePort_GetUInt16(19) * Math.Pow(10, 9) + mePort_GetUInt32(21));//6位
                                //MyDevice.mTCP[sAddress].data[0].screwSeq = mePort_GetByte(25);
                            }

                            //第二包
                            MyDevice.mTCP[sAddress].data[1].stamp = mePort_GetUInt32(39);
                            MyDevice.mTCP[sAddress].data[1].dtype = mePort_GetByte(43);
                            if (MyDevice.mTCP[sAddress].data[1].dtype == 0xF1)       //01过程帧
                            {
                                MyDevice.mTCP[sAddress].data[1].torque_unit  = (UNIT)mePort_GetByte(44);
                                MyDevice.mTCP[sAddress].data[1].torque       = mePort_GetInt32(45);
                                MyDevice.mTCP[sAddress].data[1].torseries_pk = mePort_GetInt32(49);
                                MyDevice.mTCP[sAddress].data[1].angle        = mePort_GetInt32(53);
                                MyDevice.mTCP[sAddress].data[1].angle_acc    = mePort_GetInt32(57);
                                MyDevice.mTCP[sAddress].data[1].mode_pt      = mePort_GetByte(61);
                                MyDevice.mTCP[sAddress].data[1].mode_ax      = mePort_GetByte(62);
                                MyDevice.mTCP[sAddress].data[1].mode_mx      = mePort_GetByte(63);
                                MyDevice.mTCP[sAddress].data[1].battery      = mePort_GetByte(64);
                            }
                            else if (MyDevice.mTCP[sAddress].data[1].dtype == 0xF2)  //02一次结果帧
                            {
                                if (MyDevice.mTCP[sAddress].devc.version >= 39) tempStamp = MyDevice.mTCP[sAddress].data[1].stamp;

                                MyDevice.mTCP[sAddress].data[1].mark          = mePort_GetByte(44);
                                MyDevice.mTCP[sAddress].data[1].torque_unit   = (UNIT)mePort_GetByte(45);
                                MyDevice.mTCP[sAddress].data[1].angle_decimal = mePort_GetByte(46);
                                MyDevice.mTCP[sAddress].data[1].torseries_pk  = mePort_GetInt32(47);
                                MyDevice.mTCP[sAddress].data[1].angle_acc     = mePort_GetInt32(51);
                                MyDevice.mTCP[sAddress].data[1].begin_series  = mePort_GetUInt32(55);
                                MyDevice.mTCP[sAddress].data[1].begin_group   = mePort_GetUInt32(59);
                                MyDevice.mTCP[sAddress].data[1].len           = mePort_GetUInt16(63);
                            }
                            else if (MyDevice.mTCP[sAddress].data[1].dtype == 0xF3)  //03一组结果帧
                            {
                                if (MyDevice.mTCP[sAddress].devc.version >= 39)
                                {
                                    MyDevice.mTCP[sAddress].data[1].stamp = tempStamp;
                                    MyDevice.mTCP[sAddress].data[1].angle_resist = mePort_GetInt32(35);
                                }

                                MyDevice.mTCP[sAddress].data[1].mark         = mePort_GetByte(44);
                                MyDevice.mTCP[sAddress].data[1].mode_ax      = mePort_GetByte(45);
                                MyDevice.mTCP[sAddress].data[1].mode_mx      = mePort_GetByte(46);
                                MyDevice.mTCP[sAddress].data[1].torgroup_pk  = mePort_GetInt32(47);
                                MyDevice.mTCP[sAddress].data[1].angle_acc    = mePort_GetInt32(51);
                                MyDevice.mTCP[sAddress].data[1].angle_resist = mePort_GetInt32(55);
                                MyDevice.mTCP[sAddress].data[1].alarm[0]     = mePort_GetInt32(59);
                                MyDevice.mTCP[sAddress].data[1].alarm[1]     = mePort_GetInt32(63);
                                MyDevice.mTCP[sAddress].data[1].alarm[2]     = mePort_GetInt32(67);
                            }
                            else if (MyDevice.mTCP[sAddress].data[1].dtype == 0xF4)  //04一组工单结果帧
                            {
                                //MyDevice.mTCP[sAddress].data[1].mark = mePort_GetByte(40);
                                //MyDevice.mTCP[sAddress].data[1].mode = mePort_GetByte(41);
                                //MyDevice.mTCP[sAddress].data[1].screwCnt = mePort_GetByte(42);
                                //MyDevice.mTCP[sAddress].data[1].work_num = mePort_GetUInt32(43);
                                //MyDevice.mTCP[sAddress].data[1].work_psq = (ulong)(mePort_GetUInt16(47) * Math.Pow(10, 9) + mePort_GetUInt32(49));//6位
                                //MyDevice.mTCP[sAddress].data[1].screwSeq = mePort_GetByte(53);
                            }

                            //第三包
                            MyDevice.mTCP[sAddress].data[2].stamp = mePort_GetUInt32(71);
                            MyDevice.mTCP[sAddress].data[2].dtype = mePort_GetByte(75);
                            if (MyDevice.mTCP[sAddress].data[2].dtype == 0xF1)       //01过程帧
                            {
                                MyDevice.mTCP[sAddress].data[2].torque_unit  = (UNIT)mePort_GetByte(76);
                                MyDevice.mTCP[sAddress].data[2].torque       = mePort_GetInt32(77);
                                MyDevice.mTCP[sAddress].data[2].torseries_pk = mePort_GetInt32(81);
                                MyDevice.mTCP[sAddress].data[2].angle        = mePort_GetInt32(85);
                                MyDevice.mTCP[sAddress].data[2].angle_acc    = mePort_GetInt32(89);
                                MyDevice.mTCP[sAddress].data[2].mode_pt      = mePort_GetByte(93);
                                MyDevice.mTCP[sAddress].data[2].mode_ax      = mePort_GetByte(94);
                                MyDevice.mTCP[sAddress].data[2].mode_mx      = mePort_GetByte(95);
                                MyDevice.mTCP[sAddress].data[2].battery      = mePort_GetByte(96);
                            }
                            else if (MyDevice.mTCP[sAddress].data[2].dtype == 0xF2)  //02一次结果帧
                            {
                                if (MyDevice.mTCP[sAddress].devc.version >= 39) tempStamp = MyDevice.mTCP[sAddress].data[2].stamp;

                                MyDevice.mTCP[sAddress].data[2].mark          = mePort_GetByte(76);
                                MyDevice.mTCP[sAddress].data[2].torque_unit   = (UNIT)mePort_GetByte(77);
                                MyDevice.mTCP[sAddress].data[2].angle_decimal = mePort_GetByte(78);
                                MyDevice.mTCP[sAddress].data[2].torseries_pk  = mePort_GetInt32(79);
                                MyDevice.mTCP[sAddress].data[2].angle_acc     = mePort_GetInt32(83);
                                MyDevice.mTCP[sAddress].data[2].begin_series  = mePort_GetUInt32(87);
                                MyDevice.mTCP[sAddress].data[2].begin_group   = mePort_GetUInt32(91);
                                MyDevice.mTCP[sAddress].data[2].len           = mePort_GetUInt16(95);
                            }
                            else if (MyDevice.mTCP[sAddress].data[2].dtype == 0xF3)  //03一组结果帧
                            {
                                if (MyDevice.mTCP[sAddress].devc.version >= 39)
                                {
                                    MyDevice.mTCP[sAddress].data[2].stamp = tempStamp;
                                    MyDevice.mTCP[sAddress].data[2].angle_resist = mePort_GetInt32(63);
                                }

                                MyDevice.mTCP[sAddress].data[2].mark         = mePort_GetByte(76);
                                MyDevice.mTCP[sAddress].data[2].mode_ax      = mePort_GetByte(77);
                                MyDevice.mTCP[sAddress].data[2].mode_mx      = mePort_GetByte(78);
                                MyDevice.mTCP[sAddress].data[2].torgroup_pk  = mePort_GetInt32(79);
                                MyDevice.mTCP[sAddress].data[2].angle_acc    = mePort_GetInt32(83);
                                MyDevice.mTCP[sAddress].data[2].angle_resist = mePort_GetInt32(87);
                                MyDevice.mTCP[sAddress].data[2].alarm[0]     = mePort_GetInt32(91);
                                MyDevice.mTCP[sAddress].data[2].alarm[1]     = mePort_GetInt32(95);
                                MyDevice.mTCP[sAddress].data[2].alarm[2]     = mePort_GetInt32(99);
                            }
                            else if (MyDevice.mTCP[sAddress].data[2].dtype == 0xF4)  //04一组工单结果帧
                            {
                                //MyDevice.mTCP[sAddress].data[2].mark = mePort_GetByte(68);
                                //MyDevice.mTCP[sAddress].data[2].mode = mePort_GetByte(69);
                                //MyDevice.mTCP[sAddress].data[2].screwCnt = mePort_GetByte(70);
                                //MyDevice.mTCP[sAddress].data[2].work_num = mePort_GetUInt32(71);
                                //MyDevice.mTCP[sAddress].data[2].work_psq = (ulong)(mePort_GetUInt16(75) * Math.Pow(10, 9) + mePort_GetUInt32(77));//6位
                                //MyDevice.mTCP[sAddress].data[2].screwSeq = mePort_GetByte(81);
                            }

                            //第四包
                            MyDevice.mTCP[sAddress].data[3].stamp = mePort_GetUInt32(103);
                            MyDevice.mTCP[sAddress].data[3].dtype = mePort_GetByte(107);
                            if (MyDevice.mTCP[sAddress].data[3].dtype == 0xF1)       //01过程帧
                            {
                                MyDevice.mTCP[sAddress].data[3].torque_unit  = (UNIT)mePort_GetByte(108);
                                MyDevice.mTCP[sAddress].data[3].torque       = mePort_GetInt32(109);
                                MyDevice.mTCP[sAddress].data[3].torseries_pk = mePort_GetInt32(113);
                                MyDevice.mTCP[sAddress].data[3].angle        = mePort_GetInt32(117);
                                MyDevice.mTCP[sAddress].data[3].angle_acc    = mePort_GetInt32(121);
                                MyDevice.mTCP[sAddress].data[3].mode_pt      = mePort_GetByte(125);
                                MyDevice.mTCP[sAddress].data[3].mode_ax      = mePort_GetByte(126);
                                MyDevice.mTCP[sAddress].data[3].mode_mx      = mePort_GetByte(127);
                                MyDevice.mTCP[sAddress].data[3].battery      = mePort_GetByte(128);
                            }
                            else if (MyDevice.mTCP[sAddress].data[3].dtype == 0xF2)  //02一次结果帧
                            {
                                if (MyDevice.mTCP[sAddress].devc.version >= 39) tempStamp = MyDevice.mTCP[sAddress].data[3].stamp;

                                MyDevice.mTCP[sAddress].data[3].mark          = mePort_GetByte(108);
                                MyDevice.mTCP[sAddress].data[3].torque_unit   = (UNIT)mePort_GetByte(109);
                                MyDevice.mTCP[sAddress].data[3].angle_decimal = mePort_GetByte(110);
                                MyDevice.mTCP[sAddress].data[3].torseries_pk  = mePort_GetInt32(111);
                                MyDevice.mTCP[sAddress].data[3].angle_acc     = mePort_GetInt32(115);
                                MyDevice.mTCP[sAddress].data[3].begin_series  = mePort_GetUInt32(119);
                                MyDevice.mTCP[sAddress].data[3].begin_group   = mePort_GetUInt32(123);
                                MyDevice.mTCP[sAddress].data[3].len           = mePort_GetUInt16(127);
                            }
                            else if (MyDevice.mTCP[sAddress].data[3].dtype == 0xF3)  //03一组结果帧
                            {
                                if (MyDevice.mTCP[sAddress].devc.version >= 39)
                                {
                                    MyDevice.mTCP[sAddress].data[3].stamp = tempStamp;
                                    MyDevice.mTCP[sAddress].data[3].angle_resist = mePort_GetInt32(91);
                                }

                                MyDevice.mTCP[sAddress].data[3].mark         = mePort_GetByte(108);
                                MyDevice.mTCP[sAddress].data[3].mode_ax      = mePort_GetByte(109);
                                MyDevice.mTCP[sAddress].data[3].mode_mx      = mePort_GetByte(110);
                                MyDevice.mTCP[sAddress].data[3].torgroup_pk  = mePort_GetInt32(111);
                                MyDevice.mTCP[sAddress].data[3].angle_acc    = mePort_GetInt32(115);
                                MyDevice.mTCP[sAddress].data[3].angle_resist = mePort_GetInt32(119);
                                MyDevice.mTCP[sAddress].data[3].alarm[0]     = mePort_GetInt32(123);
                                MyDevice.mTCP[sAddress].data[3].alarm[1]     = mePort_GetInt32(127);
                                MyDevice.mTCP[sAddress].data[3].alarm[2]     = mePort_GetInt32(131);
                            }
                            else if (MyDevice.mTCP[sAddress].data[3].dtype == 0xF4)  //04一组工单结果帧
                            {
                                //MyDevice.mTCP[sAddress].data[3].mark = mePort_GetByte(96);
                                //MyDevice.mTCP[sAddress].data[3].mode = mePort_GetByte(97);
                                //MyDevice.mTCP[sAddress].data[3].screwCnt = mePort_GetByte(98);
                                //MyDevice.mTCP[sAddress].data[3].work_num = mePort_GetUInt32(99);
                                //MyDevice.mTCP[sAddress].data[3].work_psq = (ulong)(mePort_GetUInt16(103) * Math.Pow(10, 9) + mePort_GetUInt32(105));//6位
                                //MyDevice.mTCP[sAddress].data[3].screwSeq = mePort_GetByte(109);
                            }

                            //第五包
                            MyDevice.mTCP[sAddress].data[4].stamp = mePort_GetUInt32(135);
                            MyDevice.mTCP[sAddress].data[4].dtype = mePort_GetByte(139);
                            if (MyDevice.mTCP[sAddress].data[4].dtype == 0xF1)       //01过程帧
                            {
                                MyDevice.mTCP[sAddress].data[4].torque_unit  = (UNIT)mePort_GetByte(140);
                                MyDevice.mTCP[sAddress].data[4].torque       = mePort_GetInt32(141);
                                MyDevice.mTCP[sAddress].data[4].torseries_pk = mePort_GetInt32(145);
                                MyDevice.mTCP[sAddress].data[4].angle        = mePort_GetInt32(149);
                                MyDevice.mTCP[sAddress].data[4].angle_acc    = mePort_GetInt32(153);
                                MyDevice.mTCP[sAddress].data[4].mode_pt      = mePort_GetByte(157);
                                MyDevice.mTCP[sAddress].data[4].mode_ax      = mePort_GetByte(158);
                                MyDevice.mTCP[sAddress].data[4].mode_mx      = mePort_GetByte(159);
                                MyDevice.mTCP[sAddress].data[4].battery      = mePort_GetByte(160);
                            }
                            else if (MyDevice.mTCP[sAddress].data[4].dtype == 0xF2)  //02一次结果帧
                            {
                                if (MyDevice.mTCP[sAddress].devc.version >= 39) tempStamp = MyDevice.mTCP[sAddress].data[4].stamp;

                                MyDevice.mTCP[sAddress].data[4].mark          = mePort_GetByte(140);
                                MyDevice.mTCP[sAddress].data[4].torque_unit   = (UNIT)mePort_GetByte(141);
                                MyDevice.mTCP[sAddress].data[4].angle_decimal = mePort_GetByte(142);
                                MyDevice.mTCP[sAddress].data[4].torseries_pk  = mePort_GetInt32(143);
                                MyDevice.mTCP[sAddress].data[4].angle_acc     = mePort_GetInt32(147);
                                MyDevice.mTCP[sAddress].data[4].begin_series  = mePort_GetUInt32(151);
                                MyDevice.mTCP[sAddress].data[4].begin_group   = mePort_GetUInt32(155);
                                MyDevice.mTCP[sAddress].data[4].len           = mePort_GetUInt16(159);
                            }
                            else if (MyDevice.mTCP[sAddress].data[4].dtype == 0xF3)  //03一组结果帧
                            {
                                if (MyDevice.mTCP[sAddress].devc.version >= 39)
                                {
                                    MyDevice.mTCP[sAddress].data[4].stamp = tempStamp;
                                    MyDevice.mTCP[sAddress].data[4].angle_resist = mePort_GetInt32(119);
                                }

                                MyDevice.mTCP[sAddress].data[4].mark         = mePort_GetByte(140);
                                MyDevice.mTCP[sAddress].data[4].mode_ax      = mePort_GetByte(141);
                                MyDevice.mTCP[sAddress].data[4].mode_mx      = mePort_GetByte(142);
                                MyDevice.mTCP[sAddress].data[4].torgroup_pk  = mePort_GetInt32(143);
                                MyDevice.mTCP[sAddress].data[4].angle_acc    = mePort_GetInt32(147);
                                MyDevice.mTCP[sAddress].data[4].angle_resist = mePort_GetInt32(151);
                                MyDevice.mTCP[sAddress].data[4].alarm[0]     = mePort_GetInt32(155);
                                MyDevice.mTCP[sAddress].data[4].alarm[1]     = mePort_GetInt32(159);
                                MyDevice.mTCP[sAddress].data[4].alarm[2]     = mePort_GetInt32(163);
                            }
                            else if (MyDevice.mTCP[sAddress].data[4].dtype == 0xF4)  //04一组工单结果帧
                            {
                                //MyDevice.mTCP[sAddress].data[4].mark = mePort_GetByte(124);
                                //MyDevice.mTCP[sAddress].data[4].mode = mePort_GetByte(125);
                                //MyDevice.mTCP[sAddress].data[4].screwCnt = mePort_GetByte(126);
                                //MyDevice.mTCP[sAddress].data[4].work_num = mePort_GetUInt32(127);
                                //MyDevice.mTCP[sAddress].data[4].work_psq = (ulong)(mePort_GetUInt16(131) * Math.Pow(10, 9) + mePort_GetUInt32(133));//6位
                                //MyDevice.mTCP[sAddress].data[4].screwSeq = mePort_GetByte(137);
                            }

                            List<DSData> sqlDataList = new List<DSData>();//存入数据库的数据列表

                            foreach (DATA data1 in MyDevice.mTCP[sAddress].data)
                            {
                                if (data1.dtype == 0xF2)
                                {
                                    f2Num++;
                                    Console.WriteLine("F2类型" + data1.dtype + "++++++++++++++" + data1.stamp + "_" + data1.torseries_pk);
                                    Console.WriteLine("F2数量" + f2Num);
                                    for (int i = 0; i < 5; i++)
                                    {
                                        Console.WriteLine($"抓峰值: " + MyDevice.mTCP[sAddress].data[i].dtype + "_" + MyDevice.mTCP[sAddress].data[i].stamp);
                                    }
                                }
                                else
                                {
                                    //Console.WriteLine(data1.dtype + "______" + data1.stamp);
                                }
                            }

                            // 遍历数组并将每个元素的拷贝添加到 List 集合中
                            foreach (DATA data in MyDevice.mTCP[sAddress].data)
                            {
                                if (data.dtype == 0xF1 || data.dtype == 0xF2 || data.dtype == 0xF3)      //添加有效数据
                                {
                                    if (data.dtype == 0xF1 && data.torque == 0 && data.angle == 0) break;
                                    MyDevice.mTCP[sAddress].dataList.Add(data);

                                    MyDevice.DataResult = "NG";
                                    //分析结果
                                    if (data.dtype == 0xF3)
                                    {
                                        //根据模式
                                        switch (data.mode_ax)
                                        {
                                            //EN模式
                                            case 0:
                                            //SN模式
                                            case 2:
                                                //峰值扭矩 >= 预设扭矩 = 合格
                                                if (data.torgroup_pk >= data.alarm[0])
                                                {
                                                    MyDevice.DataResult = "pass";
                                                }
                                                else
                                                {
                                                    MyDevice.DataResult = "NG";
                                                }
                                                break;
                                            //EA模式
                                            case 1:
                                            //SA模式
                                            case 3:
                                                //峰值扭矩 >= 预设扭矩 && 峰值角度 >= 预设角度 = 合格
                                                if (data.torgroup_pk >= data.alarm[0] && data.angle_acc >= data.alarm[1])
                                                {
                                                    MyDevice.DataResult = "pass";
                                                }
                                                else
                                                {
                                                    MyDevice.DataResult = "NG";
                                                }
                                                break;
                                            //MN模式
                                            case 4:
                                                // 扭矩下限 <= 峰值扭矩 <= 扭矩上限  = 合格
                                                if (data.alarm[0] <= data.torgroup_pk && data.torgroup_pk <= data.alarm[1])
                                                {
                                                    MyDevice.DataResult = "pass";
                                                }
                                                else
                                                {
                                                    MyDevice.DataResult = "NG";
                                                }
                                                break;
                                            //MA模式
                                            case 5:
                                                //峰值扭矩 >= 预设扭矩 && 角度下限 <= 峰值角度 <= 角度上限 = 合格
                                                if (data.torgroup_pk >= data.alarm[0]
                                                    && data.alarm[1] <= data.angle_acc && data.angle_acc <= data.alarm[2])
                                                {
                                                    MyDevice.DataResult = "pass";
                                                }
                                                else
                                                {
                                                    MyDevice.DataResult = "NG";
                                                }
                                                break;
                                            //AZ模式
                                            case 6:
                                                //峰值扭矩 >= 预设扭矩
                                                if (data.torgroup_pk >= data.alarm[2])
                                                {
                                                    MyDevice.DataResult = "pass";
                                                }
                                                else
                                                {
                                                    MyDevice.DataResult = "NG";
                                                }
                                                break;
                                            default:
                                                break;
                                        }

                                        //是否超量程(F3没有单位，所以需要继承上一个F2的单位)
                                        if (MyDevice.mTCP[sAddress].dataList.Count > 1 &&
                                            data.torgroup_pk > MyDevice.mTCP[sAddress].devc.torque_over[(int)MyDevice.mTCP[sAddress].dataList[MyDevice.mTCP[sAddress].dataList.Count - 2].torque_unit])
                                        {
                                            MyDevice.DataResult = "error";
                                            data.torque_unit = MyDevice.mTCP[sAddress].dataList[MyDevice.mTCP[sAddress].dataList.Count - 2].torque_unit;
                                        }
                                    }
                                    else if (data.dtype == 0xF2
                                        && (MyDevice.mTCP[sAddress].devc.type == TYPE.TQ_XH_XL01_06 - (UInt16)ADDROFFSET.TQ_XH_ADDR
                                        || MyDevice.mTCP[sAddress].devc.type == TYPE.TQ_XH_XL01_05 - (UInt16)ADDROFFSET.TQ_XH_ADDR))
                                    {
                                        //根据模式
                                        switch (MyDevice.mTCP[sAddress].para.mode_ax)
                                        {
                                            //EN模式
                                            case 0:
                                            //SN模式
                                            case 2:
                                                //峰值扭矩 >= 预设扭矩 = 合格
                                                if (data.torseries_pk >= MyDevice.mTCP[sAddress].alam.SN_target[MyDevice.mTCP[sAddress].para.mode_mx, (int)data.torque_unit])
                                                {
                                                    MyDevice.DataResult = "pass";
                                                }
                                                else
                                                {
                                                    MyDevice.DataResult = "NG";
                                                }
                                                break;
                                            //EA模式
                                            case 1:
                                            //SA模式
                                            case 3:
                                                //峰值扭矩 >= 预设扭矩 && 峰值角度 >= 预设角度 = 合格
                                                if (data.torseries_pk >= MyDevice.mTCP[sAddress].alam.SA_pre[MyDevice.mTCP[sAddress].para.mode_mx, (int)data.torque_unit]
                                                    && data.angle_acc >= MyDevice.mTCP[sAddress].alam.SA_ang[MyDevice.mTCP[sAddress].para.mode_mx])
                                                {
                                                    MyDevice.DataResult = "pass";
                                                }
                                                else
                                                {
                                                    MyDevice.DataResult = "NG";
                                                }
                                                break;
                                            //MN模式
                                            case 4:
                                                // 扭矩下限 <= 峰值扭矩 <= 扭矩上限  = 合格
                                                if (MyDevice.mTCP[sAddress].alam.MN_low[MyDevice.mTCP[sAddress].para.mode_mx, (int)data.torque_unit] <= data.torseries_pk
                                                    && data.torseries_pk <= MyDevice.mTCP[sAddress].alam.MN_high[MyDevice.mTCP[sAddress].para.mode_mx, (int)data.torque_unit])
                                                {
                                                    MyDevice.DataResult = "pass";
                                                }
                                                else
                                                {
                                                    MyDevice.DataResult = "NG";
                                                }
                                                break;
                                            //MA模式
                                            case 5:
                                                //峰值扭矩 >= 预设扭矩 && 角度下限 <= 峰值角度 <= 角度上限 = 合格
                                                if (data.torseries_pk >= MyDevice.mTCP[sAddress].alam.MA_pre[MyDevice.mTCP[sAddress].para.mode_mx, (int)data.torque_unit]
                                                    && MyDevice.mTCP[sAddress].alam.MA_low[MyDevice.mTCP[sAddress].para.mode_mx] <= data.angle_acc
                                                    && data.angle_acc <= MyDevice.mTCP[sAddress].alam.MA_high[MyDevice.mTCP[sAddress].para.mode_mx])
                                                {
                                                    MyDevice.DataResult = "pass";
                                                }
                                                else
                                                {
                                                    MyDevice.DataResult = "NG";
                                                }
                                                break;
                                            //AZ模式
                                            case 6:
                                                break;
                                            default:
                                                break;
                                        }

                                        //是否超量程
                                        if (data.torseries_pk > MyDevice.mTCP[sAddress].devc.torque_over[(int)data.torque_unit])
                                        {
                                            MyDevice.DataResult = "error";
                                        }
                                    }

                                    sqlDataList.Add(new DSData()
                                    {
                                        DataId = 1,
                                        DataType = MyDevice.DataType,
                                        Bohrcode = MyDevice.mTCP[sAddress].devc.bohrcode,
                                        DevType = MyDevice.mTCP[sAddress].devc.series + "-" + MyDevice.mTCP[sAddress].devc.type,
                                        WorkId = MyDevice.WorkId,
                                        WorkNum = MyDevice.WorkNum,
                                        SequenceId = MyDevice.SequenceId,
                                        PointNum = MyDevice.PointNum,
                                        DevAddr = sAddress,
                                        VinId = MyDevice.Vin,
                                        DType = data.dtype,
                                        Stamp = data.stamp,
                                        Torque = data.torque / (double)MyDevice.mTCP[sAddress].torqueMultiple,
                                        TorquePeak = (data.dtype == 0xF2 ? data.torseries_pk : data.torgroup_pk) / (double)MyDevice.mTCP[sAddress].torqueMultiple,
                                        TorqueUnit = data.torque_unit.ToString(),
                                        Angle = data.angle / (double)MyDevice.mTCP[sAddress].angleMultiple,
                                        AngleAcc = data.angle_acc / (double)MyDevice.mTCP[sAddress].angleMultiple,
                                        DataResult = MyDevice.DataResult,
                                        ModePt = data.mode_pt,
                                        ModeAx = data.mode_ax,
                                        ModeMx = data.mode_mx,
                                        Battery = data.battery,
                                        KeyBuf = data.keybuf,
                                        KeyLock = data.keylock.ToString(),
                                        MemAble = data.memable.ToString(),
                                        Update = data.update.ToString(),
                                        Error = "",
                                        Alarm = data.dtype == 0xF3 ? $"{data.alarm[0]},{data.alarm[1]},{data.alarm[2]}" : "",
                                        CreateTime = new DateTime(),
                                    });
                                }
                            }

                            //线程执行，否则会堵塞主线程，数据库插入耗时
                            var taskDataList = new List<DSData>(sqlDataList); // 创建一个本地变量，防止当前 sqlDataList 的引用在任务执行时仍然可能被修改，从而导致数据不一致或冲突
                            Task.Run(() =>
                            {
                                if (MyDevice.IsMySqlStart)
                                {
                                    JDBC.AddDataList(taskDataList);
                                }
                            });

                            mePort_DataRemove(0x52 * 2 + 5);
                            isEQ = true;
                        }
                        else
                        {
                            mePort_DataRemove(1);
                            return;
                        }
                    }
                    else
                    {
                        if (len == 0x48 * 2 + 5)
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                MyDevice.mTCP[sAddress].data[i] = new DATA(); //创建新的引用，避免集合添加时引用重复
                            }

                            //第一包
                            MyDevice.mTCP[sAddress].fifo.index = mePort_GetUInt32(3);
                            MyDevice.mTCP[sAddress].data[0].stamp = mePort_GetUInt32(7);
                            MyDevice.mTCP[sAddress].data[0].dtype = mePort_GetByte(11);
                            if (MyDevice.mTCP[sAddress].data[0].dtype == 0xF1)       //01过程帧
                            {
                                MyDevice.mTCP[sAddress].data[0].torque_unit = (UNIT)mePort_GetByte(12);
                                MyDevice.mTCP[sAddress].data[0].torque = mePort_GetInt32(13);
                                MyDevice.mTCP[sAddress].data[0].torseries_pk = mePort_GetInt32(17);
                                MyDevice.mTCP[sAddress].data[0].angle = mePort_GetInt32(21);
                                MyDevice.mTCP[sAddress].data[0].angle_acc = mePort_GetInt32(25);
                                MyDevice.mTCP[sAddress].data[0].mode_pt = mePort_GetByte(29);
                                MyDevice.mTCP[sAddress].data[0].mode_ax = mePort_GetByte(30);
                                MyDevice.mTCP[sAddress].data[0].mode_mx = mePort_GetByte(31);
                                MyDevice.mTCP[sAddress].data[0].battery = mePort_GetByte(32);
                            }
                            else if (MyDevice.mTCP[sAddress].data[0].dtype == 0xF2)  //02一次结果帧
                            {
                                if (MyDevice.mTCP[sAddress].devc.version >= 39) tempStamp = MyDevice.mTCP[sAddress].data[0].stamp;

                                MyDevice.mTCP[sAddress].data[0].mark = mePort_GetByte(12);
                                MyDevice.mTCP[sAddress].data[0].torque_unit = (UNIT)mePort_GetByte(13);
                                MyDevice.mTCP[sAddress].data[0].angle_decimal = mePort_GetByte(14);
                                MyDevice.mTCP[sAddress].data[0].torseries_pk = mePort_GetInt32(15);
                                MyDevice.mTCP[sAddress].data[0].angle_acc = mePort_GetInt32(19);
                                MyDevice.mTCP[sAddress].data[0].begin_series = mePort_GetUInt32(23);
                                MyDevice.mTCP[sAddress].data[0].begin_group = mePort_GetUInt32(27);
                                MyDevice.mTCP[sAddress].data[0].len = mePort_GetUInt16(31);
                            }
                            else if (MyDevice.mTCP[sAddress].data[0].dtype == 0xF3)  //03一组结果帧
                            {
                                if (MyDevice.mTCP[sAddress].devc.version >= 39)
                                {
                                    MyDevice.mTCP[sAddress].data[0].stamp = tempStamp;
                                    MyDevice.mTCP[sAddress].data[0].angle_resist = mePort_GetInt32(7);
                                }

                                MyDevice.mTCP[sAddress].data[0].mode_pt = mePort_GetByte(12);
                                MyDevice.mTCP[sAddress].data[0].mode_ax = mePort_GetByte(13);
                                MyDevice.mTCP[sAddress].data[0].mode_mx = mePort_GetByte(14);
                                MyDevice.mTCP[sAddress].data[0].torgroup_pk = mePort_GetInt32(15);
                                MyDevice.mTCP[sAddress].data[0].angle_acc = mePort_GetInt32(19);
                                MyDevice.mTCP[sAddress].data[0].alarm[0] = mePort_GetInt32(23);
                                MyDevice.mTCP[sAddress].data[0].alarm[1] = mePort_GetInt32(27);
                                MyDevice.mTCP[sAddress].data[0].alarm[2] = mePort_GetInt32(31);
                            }
                            else if (MyDevice.mTCP[sAddress].data[0].dtype == 0xF4)  //04一组工单结果帧
                            {
                                //MyDevice.mTCP[sAddress].data[0].mark = mePort_GetByte(12);
                                //MyDevice.mTCP[sAddress].data[0].mode = mePort_GetByte(13);
                                //MyDevice.mTCP[sAddress].data[0].screwCnt = mePort_GetByte(14);
                                //MyDevice.mTCP[sAddress].data[0].work_num = mePort_GetUInt32(15);
                                //MyDevice.mTCP[sAddress].data[0].work_psq = (ulong)(mePort_GetUInt16(19) * Math.Pow(10, 9) + mePort_GetUInt32(21));//6位
                                //MyDevice.mTCP[sAddress].data[0].screwSeq = mePort_GetByte(25);
                            }

                            //第二包
                            MyDevice.mTCP[sAddress].data[1].stamp = mePort_GetUInt32(35);
                            MyDevice.mTCP[sAddress].data[1].dtype = mePort_GetByte(39);
                            if (MyDevice.mTCP[sAddress].data[1].dtype == 0xF1)       //01过程帧
                            {
                                MyDevice.mTCP[sAddress].data[1].torque_unit = (UNIT)mePort_GetByte(40);
                                MyDevice.mTCP[sAddress].data[1].torque = mePort_GetInt32(41);
                                MyDevice.mTCP[sAddress].data[1].torseries_pk = mePort_GetInt32(45);
                                MyDevice.mTCP[sAddress].data[1].angle = mePort_GetInt32(49);
                                MyDevice.mTCP[sAddress].data[1].angle_acc = mePort_GetInt32(53);
                                MyDevice.mTCP[sAddress].data[1].mode_pt = mePort_GetByte(57);
                                MyDevice.mTCP[sAddress].data[1].mode_ax = mePort_GetByte(58);
                                MyDevice.mTCP[sAddress].data[1].mode_mx = mePort_GetByte(59);
                                MyDevice.mTCP[sAddress].data[1].battery = mePort_GetByte(60);
                            }
                            else if (MyDevice.mTCP[sAddress].data[1].dtype == 0xF2)  //02一次结果帧
                            {
                                if (MyDevice.mTCP[sAddress].devc.version >= 39) tempStamp = MyDevice.mTCP[sAddress].data[1].stamp;

                                MyDevice.mTCP[sAddress].data[1].mark = mePort_GetByte(40);
                                MyDevice.mTCP[sAddress].data[1].torque_unit = (UNIT)mePort_GetByte(41);
                                MyDevice.mTCP[sAddress].data[1].angle_decimal = mePort_GetByte(42);
                                MyDevice.mTCP[sAddress].data[1].torseries_pk = mePort_GetInt32(43);
                                MyDevice.mTCP[sAddress].data[1].angle_acc = mePort_GetInt32(47);
                                MyDevice.mTCP[sAddress].data[1].begin_series = mePort_GetUInt32(51);
                                MyDevice.mTCP[sAddress].data[1].begin_group = mePort_GetUInt32(55);
                                MyDevice.mTCP[sAddress].data[1].len = mePort_GetUInt16(59);
                            }
                            else if (MyDevice.mTCP[sAddress].data[1].dtype == 0xF3)  //03一组结果帧
                            {
                                if (MyDevice.mTCP[sAddress].devc.version >= 39)
                                {
                                    MyDevice.mTCP[sAddress].data[1].stamp = tempStamp;
                                    MyDevice.mTCP[sAddress].data[1].angle_resist = mePort_GetInt32(35);
                                }

                                MyDevice.mTCP[sAddress].data[1].mode_pt = mePort_GetByte(40);
                                MyDevice.mTCP[sAddress].data[1].mode_ax = mePort_GetByte(41);
                                MyDevice.mTCP[sAddress].data[1].mode_mx = mePort_GetByte(42);
                                MyDevice.mTCP[sAddress].data[1].torgroup_pk = mePort_GetInt32(43);
                                MyDevice.mTCP[sAddress].data[1].angle_acc = mePort_GetInt32(47);
                                MyDevice.mTCP[sAddress].data[1].alarm[0] = mePort_GetInt32(51);
                                MyDevice.mTCP[sAddress].data[1].alarm[1] = mePort_GetInt32(55);
                                MyDevice.mTCP[sAddress].data[1].alarm[2] = mePort_GetInt32(59);
                            }
                            else if (MyDevice.mTCP[sAddress].data[1].dtype == 0xF4)  //04一组工单结果帧
                            {
                                //MyDevice.mTCP[sAddress].data[1].mark = mePort_GetByte(40);
                                //MyDevice.mTCP[sAddress].data[1].mode = mePort_GetByte(41);
                                //MyDevice.mTCP[sAddress].data[1].screwCnt = mePort_GetByte(42);
                                //MyDevice.mTCP[sAddress].data[1].work_num = mePort_GetUInt32(43);
                                //MyDevice.mTCP[sAddress].data[1].work_psq = (ulong)(mePort_GetUInt16(47) * Math.Pow(10, 9) + mePort_GetUInt32(49));//6位
                                //MyDevice.mTCP[sAddress].data[1].screwSeq = mePort_GetByte(53);
                            }

                            //第三包
                            MyDevice.mTCP[sAddress].data[2].stamp = mePort_GetUInt32(63);
                            MyDevice.mTCP[sAddress].data[2].dtype = mePort_GetByte(67);
                            if (MyDevice.mTCP[sAddress].data[2].dtype == 0xF1)       //01过程帧
                            {
                                MyDevice.mTCP[sAddress].data[2].torque_unit = (UNIT)mePort_GetByte(68);
                                MyDevice.mTCP[sAddress].data[2].torque = mePort_GetInt32(69);
                                MyDevice.mTCP[sAddress].data[2].torseries_pk = mePort_GetInt32(73);
                                MyDevice.mTCP[sAddress].data[2].angle = mePort_GetInt32(77);
                                MyDevice.mTCP[sAddress].data[2].angle_acc = mePort_GetInt32(81);
                                MyDevice.mTCP[sAddress].data[2].mode_pt = mePort_GetByte(85);
                                MyDevice.mTCP[sAddress].data[2].mode_ax = mePort_GetByte(86);
                                MyDevice.mTCP[sAddress].data[2].mode_mx = mePort_GetByte(87);
                                MyDevice.mTCP[sAddress].data[2].battery = mePort_GetByte(88);
                            }
                            else if (MyDevice.mTCP[sAddress].data[2].dtype == 0xF2)  //02一次结果帧
                            {
                                if (MyDevice.mTCP[sAddress].devc.version >= 39) tempStamp = MyDevice.mTCP[sAddress].data[2].stamp;

                                MyDevice.mTCP[sAddress].data[2].mark = mePort_GetByte(68);
                                MyDevice.mTCP[sAddress].data[2].torque_unit = (UNIT)mePort_GetByte(69);
                                MyDevice.mTCP[sAddress].data[2].angle_decimal = mePort_GetByte(70);
                                MyDevice.mTCP[sAddress].data[2].torseries_pk = mePort_GetInt32(71);
                                MyDevice.mTCP[sAddress].data[2].angle_acc = mePort_GetInt32(75);
                                MyDevice.mTCP[sAddress].data[2].begin_series = mePort_GetUInt32(79);
                                MyDevice.mTCP[sAddress].data[2].begin_group = mePort_GetUInt32(83);
                                MyDevice.mTCP[sAddress].data[2].len = mePort_GetUInt16(87);
                            }
                            else if (MyDevice.mTCP[sAddress].data[2].dtype == 0xF3)  //03一组结果帧
                            {
                                if (MyDevice.mTCP[sAddress].devc.version >= 39)
                                {
                                    MyDevice.mTCP[sAddress].data[2].stamp = tempStamp;
                                    MyDevice.mTCP[sAddress].data[2].angle_resist = mePort_GetInt32(63);
                                }

                                MyDevice.mTCP[sAddress].data[2].mode_pt = mePort_GetByte(68);
                                MyDevice.mTCP[sAddress].data[2].mode_ax = mePort_GetByte(69);
                                MyDevice.mTCP[sAddress].data[2].mode_mx = mePort_GetByte(70);
                                MyDevice.mTCP[sAddress].data[2].torgroup_pk = mePort_GetInt32(71);
                                MyDevice.mTCP[sAddress].data[2].angle_acc = mePort_GetInt32(75);
                                MyDevice.mTCP[sAddress].data[2].alarm[0] = mePort_GetInt32(79);
                                MyDevice.mTCP[sAddress].data[2].alarm[1] = mePort_GetInt32(83);
                                MyDevice.mTCP[sAddress].data[2].alarm[2] = mePort_GetInt32(87);
                            }
                            else if (MyDevice.mTCP[sAddress].data[2].dtype == 0xF4)  //04一组工单结果帧
                            {
                                //MyDevice.mTCP[sAddress].data[2].mark = mePort_GetByte(68);
                                //MyDevice.mTCP[sAddress].data[2].mode = mePort_GetByte(69);
                                //MyDevice.mTCP[sAddress].data[2].screwCnt = mePort_GetByte(70);
                                //MyDevice.mTCP[sAddress].data[2].work_num = mePort_GetUInt32(71);
                                //MyDevice.mTCP[sAddress].data[2].work_psq = (ulong)(mePort_GetUInt16(75) * Math.Pow(10, 9) + mePort_GetUInt32(77));//6位
                                //MyDevice.mTCP[sAddress].data[2].screwSeq = mePort_GetByte(81);
                            }

                            //第四包
                            MyDevice.mTCP[sAddress].data[3].stamp = mePort_GetUInt32(91);
                            MyDevice.mTCP[sAddress].data[3].dtype = mePort_GetByte(95);
                            if (MyDevice.mTCP[sAddress].data[3].dtype == 0xF1)       //01过程帧
                            {
                                MyDevice.mTCP[sAddress].data[3].torque_unit = (UNIT)mePort_GetByte(96);
                                MyDevice.mTCP[sAddress].data[3].torque = mePort_GetInt32(97);
                                MyDevice.mTCP[sAddress].data[3].torseries_pk = mePort_GetInt32(101);
                                MyDevice.mTCP[sAddress].data[3].angle = mePort_GetInt32(105);
                                MyDevice.mTCP[sAddress].data[3].angle_acc = mePort_GetInt32(109);
                                MyDevice.mTCP[sAddress].data[3].mode_pt = mePort_GetByte(113);
                                MyDevice.mTCP[sAddress].data[3].mode_ax = mePort_GetByte(114);
                                MyDevice.mTCP[sAddress].data[3].mode_mx = mePort_GetByte(115);
                                MyDevice.mTCP[sAddress].data[3].battery = mePort_GetByte(116);
                            }
                            else if (MyDevice.mTCP[sAddress].data[3].dtype == 0xF2)  //02一次结果帧
                            {
                                if (MyDevice.mTCP[sAddress].devc.version >= 39) tempStamp = MyDevice.mTCP[sAddress].data[3].stamp;

                                MyDevice.mTCP[sAddress].data[3].mark = mePort_GetByte(96);
                                MyDevice.mTCP[sAddress].data[3].torque_unit = (UNIT)mePort_GetByte(97);
                                MyDevice.mTCP[sAddress].data[3].angle_decimal = mePort_GetByte(98);
                                MyDevice.mTCP[sAddress].data[3].torseries_pk = mePort_GetInt32(99);
                                MyDevice.mTCP[sAddress].data[3].angle_acc = mePort_GetInt32(103);
                                MyDevice.mTCP[sAddress].data[3].begin_series = mePort_GetUInt32(107);
                                MyDevice.mTCP[sAddress].data[3].begin_group = mePort_GetUInt32(111);
                                MyDevice.mTCP[sAddress].data[3].len = mePort_GetUInt16(115);
                            }
                            else if (MyDevice.mTCP[sAddress].data[3].dtype == 0xF3)  //03一组结果帧
                            {
                                if (MyDevice.mTCP[sAddress].devc.version >= 39)
                                {
                                    MyDevice.mTCP[sAddress].data[3].stamp = tempStamp;
                                    MyDevice.mTCP[sAddress].data[3].angle_resist = mePort_GetInt32(91);
                                }

                                MyDevice.mTCP[sAddress].data[3].mode_pt = mePort_GetByte(96);
                                MyDevice.mTCP[sAddress].data[3].mode_ax = mePort_GetByte(97);
                                MyDevice.mTCP[sAddress].data[3].mode_mx = mePort_GetByte(98);
                                MyDevice.mTCP[sAddress].data[3].torgroup_pk = mePort_GetInt32(99);
                                MyDevice.mTCP[sAddress].data[3].angle_acc = mePort_GetInt32(103);
                                MyDevice.mTCP[sAddress].data[3].alarm[0] = mePort_GetInt32(107);
                                MyDevice.mTCP[sAddress].data[3].alarm[1] = mePort_GetInt32(111);
                                MyDevice.mTCP[sAddress].data[3].alarm[2] = mePort_GetInt32(115);
                            }
                            else if (MyDevice.mTCP[sAddress].data[3].dtype == 0xF4)  //04一组工单结果帧
                            {
                                //MyDevice.mTCP[sAddress].data[3].mark = mePort_GetByte(96);
                                //MyDevice.mTCP[sAddress].data[3].mode = mePort_GetByte(97);
                                //MyDevice.mTCP[sAddress].data[3].screwCnt = mePort_GetByte(98);
                                //MyDevice.mTCP[sAddress].data[3].work_num = mePort_GetUInt32(99);
                                //MyDevice.mTCP[sAddress].data[3].work_psq = (ulong)(mePort_GetUInt16(103) * Math.Pow(10, 9) + mePort_GetUInt32(105));//6位
                                //MyDevice.mTCP[sAddress].data[3].screwSeq = mePort_GetByte(109);
                            }

                            //第五包
                            MyDevice.mTCP[sAddress].data[4].stamp = mePort_GetUInt32(119);
                            MyDevice.mTCP[sAddress].data[4].dtype = mePort_GetByte(123);
                            if (MyDevice.mTCP[sAddress].data[4].dtype == 0xF1)       //01过程帧
                            {
                                MyDevice.mTCP[sAddress].data[4].torque_unit = (UNIT)mePort_GetByte(124);
                                MyDevice.mTCP[sAddress].data[4].torque = mePort_GetInt32(125);
                                MyDevice.mTCP[sAddress].data[4].torseries_pk = mePort_GetInt32(129);
                                MyDevice.mTCP[sAddress].data[4].angle = mePort_GetInt32(133);
                                MyDevice.mTCP[sAddress].data[4].angle_acc = mePort_GetInt32(137);
                                MyDevice.mTCP[sAddress].data[4].mode_pt = mePort_GetByte(141);
                                MyDevice.mTCP[sAddress].data[4].mode_ax = mePort_GetByte(142);
                                MyDevice.mTCP[sAddress].data[4].mode_mx = mePort_GetByte(143);
                                MyDevice.mTCP[sAddress].data[4].battery = mePort_GetByte(144);
                            }
                            else if (MyDevice.mTCP[sAddress].data[4].dtype == 0xF2)  //02一次结果帧
                            {
                                if (MyDevice.mTCP[sAddress].devc.version >= 39) tempStamp = MyDevice.mTCP[sAddress].data[4].stamp;

                                MyDevice.mTCP[sAddress].data[4].mark = mePort_GetByte(124);
                                MyDevice.mTCP[sAddress].data[4].torque_unit = (UNIT)mePort_GetByte(125);
                                MyDevice.mTCP[sAddress].data[4].angle_decimal = mePort_GetByte(126);
                                MyDevice.mTCP[sAddress].data[4].torseries_pk = mePort_GetInt32(127);
                                MyDevice.mTCP[sAddress].data[4].angle_acc = mePort_GetInt32(131);
                                MyDevice.mTCP[sAddress].data[4].begin_series = mePort_GetUInt32(135);
                                MyDevice.mTCP[sAddress].data[4].begin_group = mePort_GetUInt32(139);
                                MyDevice.mTCP[sAddress].data[4].len = mePort_GetUInt16(143);
                            }
                            else if (MyDevice.mTCP[sAddress].data[4].dtype == 0xF3)  //03一组结果帧
                            {
                                if (MyDevice.mTCP[sAddress].devc.version >= 39)
                                {
                                    MyDevice.mTCP[sAddress].data[4].stamp = tempStamp;
                                    MyDevice.mTCP[sAddress].data[4].angle_resist = mePort_GetInt32(119);
                                }

                                MyDevice.mTCP[sAddress].data[4].mode_pt = mePort_GetByte(124);
                                MyDevice.mTCP[sAddress].data[4].mode_ax = mePort_GetByte(125);
                                MyDevice.mTCP[sAddress].data[4].mode_mx = mePort_GetByte(126);
                                MyDevice.mTCP[sAddress].data[4].torgroup_pk = mePort_GetInt32(127);
                                MyDevice.mTCP[sAddress].data[4].angle_acc = mePort_GetInt32(131);
                                MyDevice.mTCP[sAddress].data[4].alarm[0] = mePort_GetInt32(135);
                                MyDevice.mTCP[sAddress].data[4].alarm[1] = mePort_GetInt32(139);
                                MyDevice.mTCP[sAddress].data[4].alarm[2] = mePort_GetInt32(143);
                            }
                            else if (MyDevice.mTCP[sAddress].data[4].dtype == 0xF4)  //04一组工单结果帧
                            {
                                //MyDevice.mTCP[sAddress].data[4].mark = mePort_GetByte(124);
                                //MyDevice.mTCP[sAddress].data[4].mode = mePort_GetByte(125);
                                //MyDevice.mTCP[sAddress].data[4].screwCnt = mePort_GetByte(126);
                                //MyDevice.mTCP[sAddress].data[4].work_num = mePort_GetUInt32(127);
                                //MyDevice.mTCP[sAddress].data[4].work_psq = (ulong)(mePort_GetUInt16(131) * Math.Pow(10, 9) + mePort_GetUInt32(133));//6位
                                //MyDevice.mTCP[sAddress].data[4].screwSeq = mePort_GetByte(137);
                            }

                            List<DSData> sqlDataList = new List<DSData>();//存入数据库的数据列表

                            foreach (DATA data1 in MyDevice.mTCP[sAddress].data)
                            {
                                if (data1.dtype == 0xF2)
                                {
                                    f2Num++;
                                    Console.WriteLine("F2类型" + data1.dtype + "++++++++++++++" + data1.stamp + "_" + data1.torseries_pk);
                                    Console.WriteLine("F2数量" + f2Num);
                                    for (int i = 0; i < 5; i++)
                                    {
                                        Console.WriteLine($"抓峰值: " + MyDevice.mTCP[sAddress].data[i].dtype + "_" + MyDevice.mTCP[sAddress].data[i].stamp);
                                    }
                                }
                                else
                                {
                                    //Console.WriteLine(data1.dtype + "______" + data1.stamp);
                                }
                            }

                            // 遍历数组并将每个元素的拷贝添加到 List 集合中
                            foreach (DATA data in MyDevice.mTCP[sAddress].data)
                            {
                                if (data.dtype == 0xF1 || data.dtype == 0xF2 || data.dtype == 0xF3)      //添加有效数据
                                {
                                    if (data.dtype == 0xF1 && data.torque == 0 && data.angle == 0) break;
                                    MyDevice.mTCP[sAddress].dataList.Add(data);

                                    MyDevice.DataResult = "NG";
                                    //分析结果
                                    if (data.dtype == 0xF3)
                                    {
                                        //根据模式
                                        switch (data.mode_ax)
                                        {
                                            //EN模式
                                            case 0:
                                            //SN模式
                                            case 2:
                                                //峰值扭矩 >= 预设扭矩 = 合格
                                                if (data.torgroup_pk >= data.alarm[0])
                                                {
                                                    MyDevice.DataResult = "pass";
                                                }
                                                else
                                                {
                                                    MyDevice.DataResult = "NG";
                                                }
                                                break;
                                            //EA模式
                                            case 1:
                                            //SA模式
                                            case 3:
                                                //峰值扭矩 >= 预设扭矩 && 峰值角度 >= 预设角度 = 合格
                                                if (data.torgroup_pk >= data.alarm[0] && data.angle_acc >= data.alarm[1])
                                                {
                                                    MyDevice.DataResult = "pass";
                                                }
                                                else
                                                {
                                                    MyDevice.DataResult = "NG";
                                                }
                                                break;
                                            //MN模式
                                            case 4:
                                                // 扭矩下限 <= 峰值扭矩 <= 扭矩上限  = 合格
                                                if (data.alarm[0] <= data.torgroup_pk && data.torgroup_pk <= data.alarm[1])
                                                {
                                                    MyDevice.DataResult = "pass";
                                                }
                                                else
                                                {
                                                    MyDevice.DataResult = "NG";
                                                }
                                                break;
                                            //MA模式
                                            case 5:
                                                //峰值扭矩 >= 预设扭矩 && 角度下限 <= 峰值角度 <= 角度上限 = 合格
                                                if (data.torgroup_pk >= data.alarm[0]
                                                    && data.alarm[1] <= data.angle_acc && data.angle_acc <= data.alarm[2])
                                                {
                                                    MyDevice.DataResult = "pass";
                                                }
                                                else
                                                {
                                                    MyDevice.DataResult = "NG";
                                                }
                                                break;
                                            //AZ模式
                                            case 6:
                                                //峰值扭矩 >= 预设扭矩
                                                if (data.torgroup_pk >= data.alarm[2])
                                                {
                                                    MyDevice.DataResult = "pass";
                                                }
                                                else
                                                {
                                                    MyDevice.DataResult = "NG";
                                                }
                                                break;
                                            default:
                                                break;
                                        }

                                        //是否超量程(F3没有单位，所以需要继承上一个F2的单位)
                                        if (MyDevice.mTCP[sAddress].dataList.Count > 1 &&
                                            data.torgroup_pk > MyDevice.mTCP[sAddress].devc.torque_over[(int)MyDevice.mTCP[sAddress].dataList[MyDevice.mTCP[sAddress].dataList.Count - 2].torque_unit])
                                        {
                                            MyDevice.DataResult = "error";
                                            data.torque_unit = MyDevice.mTCP[sAddress].dataList[MyDevice.mTCP[sAddress].dataList.Count - 2].torque_unit;
                                        }
                                    }
                                    else if (data.dtype == 0xF2
                                        && (MyDevice.mTCP[sAddress].devc.type == TYPE.TQ_XH_XL01_06 - (UInt16)ADDROFFSET.TQ_XH_ADDR
                                        || MyDevice.mTCP[sAddress].devc.type == TYPE.TQ_XH_XL01_05 - (UInt16)ADDROFFSET.TQ_XH_ADDR))
                                    {
                                        //根据模式
                                        switch (MyDevice.mTCP[sAddress].para.mode_ax)
                                        {
                                            //EN模式
                                            case 0:
                                            //SN模式
                                            case 2:
                                                //峰值扭矩 >= 预设扭矩 = 合格
                                                if (data.torseries_pk >= MyDevice.mTCP[sAddress].alam.SN_target[MyDevice.mTCP[sAddress].para.mode_mx, (int)data.torque_unit])
                                                {
                                                    MyDevice.DataResult = "pass";
                                                }
                                                else
                                                {
                                                    MyDevice.DataResult = "NG";
                                                }
                                                break;
                                            //EA模式
                                            case 1:
                                            //SA模式
                                            case 3:
                                                //峰值扭矩 >= 预设扭矩 && 峰值角度 >= 预设角度 = 合格
                                                if (data.torseries_pk >= MyDevice.mTCP[sAddress].alam.SA_pre[MyDevice.mTCP[sAddress].para.mode_mx, (int)data.torque_unit]
                                                    && data.angle_acc >= MyDevice.mTCP[sAddress].alam.SA_ang[MyDevice.mTCP[sAddress].para.mode_mx])
                                                {
                                                    MyDevice.DataResult = "pass";
                                                }
                                                else
                                                {
                                                    MyDevice.DataResult = "NG";
                                                }
                                                break;
                                            //MN模式
                                            case 4:
                                                // 扭矩下限 <= 峰值扭矩 <= 扭矩上限  = 合格
                                                if (MyDevice.mTCP[sAddress].alam.MN_low[MyDevice.mTCP[sAddress].para.mode_mx, (int)data.torque_unit] <= data.torseries_pk
                                                    && data.torseries_pk <= MyDevice.mTCP[sAddress].alam.MN_high[MyDevice.mTCP[sAddress].para.mode_mx, (int)data.torque_unit])
                                                {
                                                    MyDevice.DataResult = "pass";
                                                }
                                                else
                                                {
                                                    MyDevice.DataResult = "NG";
                                                }
                                                break;
                                            //MA模式
                                            case 5:
                                                //峰值扭矩 >= 预设扭矩 && 角度下限 <= 峰值角度 <= 角度上限 = 合格
                                                if (data.torseries_pk >= MyDevice.mTCP[sAddress].alam.MA_pre[MyDevice.mTCP[sAddress].para.mode_mx, (int)data.torque_unit]
                                                    && MyDevice.mTCP[sAddress].alam.MA_low[MyDevice.mTCP[sAddress].para.mode_mx] <= data.angle_acc
                                                    && data.angle_acc <= MyDevice.mTCP[sAddress].alam.MA_high[MyDevice.mTCP[sAddress].para.mode_mx])
                                                {
                                                    MyDevice.DataResult = "pass";
                                                }
                                                else
                                                {
                                                    MyDevice.DataResult = "NG";
                                                }
                                                break;
                                            //AZ模式
                                            case 6:
                                                break;
                                            default:
                                                break;
                                        }

                                        //是否超量程
                                        if (data.torseries_pk > MyDevice.mTCP[sAddress].devc.torque_over[(int)data.torque_unit])
                                        {
                                            MyDevice.DataResult = "error";
                                        }
                                    }

                                    sqlDataList.Add(new DSData()
                                    {
                                        DataId = 1,
                                        DataType = MyDevice.DataType,
                                        Bohrcode = MyDevice.mTCP[sAddress].devc.bohrcode,
                                        DevType = MyDevice.mTCP[sAddress].devc.series + "-" + MyDevice.mTCP[sAddress].devc.type,
                                        WorkId = MyDevice.WorkId,
                                        WorkNum = MyDevice.WorkNum,
                                        SequenceId = MyDevice.SequenceId,
                                        PointNum = MyDevice.PointNum,
                                        DevAddr = sAddress,
                                        VinId = MyDevice.Vin,
                                        DType = data.dtype,
                                        Stamp = data.stamp,
                                        Torque = data.torque / (double)MyDevice.mTCP[sAddress].torqueMultiple,
                                        TorquePeak = (data.dtype == 0xF2 ? data.torseries_pk : data.torgroup_pk) / (double)MyDevice.mTCP[sAddress].torqueMultiple,
                                        TorqueUnit = data.torque_unit.ToString(),
                                        Angle = data.angle / (double)MyDevice.mTCP[sAddress].angleMultiple,
                                        AngleAcc = data.angle_acc / (double)MyDevice.mTCP[sAddress].angleMultiple,
                                        DataResult = MyDevice.DataResult,
                                        ModePt = data.mode_pt,
                                        ModeAx = data.mode_ax,
                                        ModeMx = data.mode_mx,
                                        Battery = data.battery,
                                        KeyBuf = data.keybuf,
                                        KeyLock = data.keylock.ToString(),
                                        MemAble = data.memable.ToString(),
                                        Update = data.update.ToString(),
                                        Error = "",
                                        Alarm = data.dtype == 0xF3 ? $"{data.alarm[0]},{data.alarm[1]},{data.alarm[2]}" : "",
                                        CreateTime = new DateTime(),
                                    });
                                }
                            }

                            //线程执行，否则会堵塞主线程，数据库插入耗时
                            var taskDataList = new List<DSData>(sqlDataList); // 创建一个本地变量，防止当前 sqlDataList 的引用在任务执行时仍然可能被修改，从而导致数据不一致或冲突
                            Task.Run(() =>
                            {
                                if (MyDevice.IsMySqlStart)
                                {
                                    JDBC.AddDataList(taskDataList);
                                }
                            });

                            mePort_DataRemove(0x48 * 2 + 5);
                            isEQ = true;
                        }
                        else
                        {
                            mePort_DataRemove(1);
                            return;
                        }
                    }
                    
                    break;

                case TASKS.REG_BLOCK3_SCREW1:
                    if (len == 0x65)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            MyDevice.mTCP[sAddress].screw[i].scw_ticketAxMx = mePort_GetByte((ushort)(3 + i * 12));
                            MyDevice.mTCP[sAddress].screw[i].scw_ticketCnt = mePort_GetByte((ushort)(4 + i * 12));
                            MyDevice.mTCP[sAddress].screw[i].scw_ticketNum = mePort_GetUInt32((ushort)(5 + i * 12));
                            MyDevice.mTCP[sAddress].screw[i].scw_ticketSerial = (ulong)((UInt64)mePort_GetUInt16((ushort)(9 + i * 12)) * Math.Pow(10, 9) + ((UInt64)mePort_GetUInt32((ushort)(11 + i * 12))));
                        }
                        mePort_DataRemove(0x65);
                        isEQ = true;
                    }
                    else
                    {
                        mePort_DataRemove(1);
                        return;
                    }
                    break;

                case TASKS.REG_BLOCK3_SCREW2:
                    if (len == 0x65)
                    {
                        for (int i = 8; i < 16; i++)
                        {
                            MyDevice.mTCP[sAddress].screw[i].scw_ticketAxMx = mePort_GetByte((ushort)(3 + (i - 8) * 12));
                            MyDevice.mTCP[sAddress].screw[i].scw_ticketCnt = mePort_GetByte((ushort)(4 + (i - 8) * 12));
                            MyDevice.mTCP[sAddress].screw[i].scw_ticketNum = mePort_GetUInt32((ushort)(5 + (i - 8) * 12));
                            MyDevice.mTCP[sAddress].screw[i].scw_ticketSerial = (ulong)((UInt64)mePort_GetUInt16((ushort)(9 + (i - 8) * 12)) * Math.Pow(10, 9) + ((UInt64)mePort_GetUInt32((ushort)(11 + (i - 8) * 12)))); ;
                        }
                        mePort_DataRemove(0x65);
                        isEQ = true;
                    }
                    else
                    {
                        mePort_DataRemove(1);
                        return;
                    }
                    break;

                case TASKS.REG_BLOCK3_SCREW3:
                    if (len == 0x65)
                    {
                        for (int i = 16; i < 24; i++)
                        {
                            MyDevice.mTCP[sAddress].screw[i].scw_ticketAxMx = mePort_GetByte((ushort)(3 + (i - 16) * 12));
                            MyDevice.mTCP[sAddress].screw[i].scw_ticketCnt = mePort_GetByte((ushort)(4 + (i - 16) * 12));
                            MyDevice.mTCP[sAddress].screw[i].scw_ticketNum = mePort_GetUInt32((ushort)(5 + (i - 16) * 12));
                            MyDevice.mTCP[sAddress].screw[i].scw_ticketSerial = (ulong)((UInt64)mePort_GetUInt16((ushort)(9 + (i - 16) * 12)) * Math.Pow(10, 9) + ((UInt64)mePort_GetUInt32((ushort)(11 + (i - 16) * 12))));
                        }
                        mePort_DataRemove(0x65);
                        isEQ = true;
                    }
                    else
                    {
                        mePort_DataRemove(1);
                        return;
                    }
                    break;

                case TASKS.REG_BLOCK3_SCREW4:
                    if (len == 0x65)
                    {
                        for (int i = 24; i < 32; i++)
                        {
                            MyDevice.mTCP[sAddress].screw[i].scw_ticketAxMx = mePort_GetByte((ushort)(3 + (i - 24) * 12));
                            MyDevice.mTCP[sAddress].screw[i].scw_ticketCnt = mePort_GetByte((ushort)(4 + (i - 24) * 12));
                            MyDevice.mTCP[sAddress].screw[i].scw_ticketNum = mePort_GetUInt32((ushort)(5 + (i - 24) * 12));
                            MyDevice.mTCP[sAddress].screw[i].scw_ticketSerial = (ulong)((UInt64)mePort_GetUInt16((ushort)(9 + (i - 24) * 12)) * Math.Pow(10, 9) + ((UInt64)mePort_GetUInt32((ushort)(11 + (i - 24) * 12))));
                        }
                        mePort_DataRemove(0x65);
                        isEQ = true;
                        MyDevice.mTCP[sAddress].sTATE = STATE.WORKING;//状态工作中
                    }
                    else
                    {
                        mePort_DataRemove(1);
                        return;
                    }
                    break;

                default:
                    mePort_DataRemove(1);
                    return;
            }

            //顺利解码
            if (trTASK == TASKS.REG_BLOCK2_DAT)
            {
                Console.WriteLine("读数据解码成功后进入委托");
            }
            MyDevice.callDelegate();
        }

        //接收写帧
        private void mePort_DataReceiveWrite()
        {
            //长度
            UInt16 len = 8;

            //拷贝
            Array.Clear(meCRC, 0, meCRC.Length);
            for (UInt16 idx = 0; idx < len; idx++)
            {
                meCRC[idx] = mePort_GetByte(idx);
            }

            //校验CRC
            if (0 != MODBUS.AP_CRC16_MODBUS(meCRC, len, true))
            {
                mePort_DataRemove(1);
                return;
            }

            //解码
            switch (trTASK)
            {
                case TASKS.WRITE_ZERO:
                    if ((REG)mePort_GetUInt16(2) == REG.REG_WR_AD_ZERO && mePort_GetInt16(4) == 1)
                    {
                        //写命令的参数值是1
                        isEQ = true;
                        mePort_DataRemove(8);
                    }
                    else
                    {
                        mePort_DataRemove(1);
                        return;
                    }
                    break;
                case TASKS.WRITE_POWEROFF:
                    if ((REG)mePort_GetUInt16(2) == REG.REG_W_POWEROFF && mePort_GetInt16(4) == 1)
                    {
                        //写命令的参数值是1
                        isEQ = true;
                        mePort_DataRemove(8);
                    }
                    else
                    {
                        mePort_DataRemove(1);
                        return;
                    }
                    break;
                case TASKS.WRITE_KEYLOCK:
                    if ((REG)mePort_GetUInt16(2) == REG.REG_W_KEYLOCK && mePort_GetUInt16(4) == 0xFFFF)
                    {
                        //写命令的参数值是按键锁
                        isEQ = true;
                        mePort_DataRemove(8);
                    }
                    else
                    {
                        mePort_DataRemove(1);
                        return;
                    }
                    break;
                case TASKS.WRITE_MEMABLE:
                    if ((REG)mePort_GetUInt16(2) == REG.REG_W_MEMABLE && mePort_GetInt16(4) == Convert.ToByte(MyDevice.userRole))
                    {
                        //写命令的参数值是角色权限
                        isEQ = true;
                        mePort_DataRemove(8);
                    }
                    else
                    {
                        mePort_DataRemove(1);
                        return;
                    }
                    break;
                default:
                    mePort_DataRemove(1);
                    return;
            }

            //顺利解码
            MyDevice.callDelegate();
        }

        //接收连续写帧
        private void mePort_DataReceiveSequence()
        {
            //长度
            UInt16 len = 8;

            //拷贝
            Array.Clear(meCRC, 0, meCRC.Length);
            for (UInt16 idx = 0; idx < len; idx++)
            {
                meCRC[idx] = mePort_GetByte(idx);
            }

            //校验CRC
            if (0 != MODBUS.AP_CRC16_MODBUS(meCRC, len, true))
            {
                mePort_DataRemove(1);
                return;
            }

            //解码
            switch (trTASK)
            {
                case TASKS.REG_BLOCK3_PARA:
                    if (mePort_GetUInt16(2) == Constants.REG_BLOCK2_PARA && mePort_GetInt16(4) == 0x30)
                    {
                        //连续写入的寄存器个数是0x20
                        isEQ = true;
                        mePort_DataRemove(0x08);
                    }
                    else
                    {
                        mePort_DataRemove(1);
                        return;
                    }
                    break;

                case TASKS.REG_BLOCK3_WLAN:
                    if (mePort_GetUInt16(2) == Constants.REG_BLOCK4_WLAN && mePort_GetInt16(4) == 0x30)
                    {
                        //连续写入的寄存器个数是0x30
                        isEQ = true;
                        mePort_DataRemove(0x08);
                    }
                    else
                    {
                        mePort_DataRemove(1);
                        return;
                    }
                    break;
                case TASKS.REG_BLOCK3_JOB:
                    if (mePort_GetUInt16(2) == Constants.REG_BLOCK3_JOB && mePort_GetInt16(4) == 0x30)
                    {
                        //连续写入的寄存器个数是0x30
                        isEQ = true;
                        mePort_DataRemove(0x08);
                    }
                    else
                    {
                        mePort_DataRemove(1);
                        return;
                    }
                    break;
                case TASKS.REG_BLOCK3_OP:
                    if (mePort_GetUInt16(2) == Constants.REG_BLOCK3_OP && mePort_GetInt16(4) == 0x30)
                    {
                        //连续写入的寄存器个数是0x30
                        isEQ = true;
                        mePort_DataRemove(0x08);
                    }
                    else
                    {
                        mePort_DataRemove(1);
                        return;
                    }
                    break;

                case TASKS.REG_BLOCK4_CAL1:
                    if (mePort_GetUInt16(2) == Constants.REG_BLOCK4_CAL1 && mePort_GetInt16(4) == 0x40)
                    {
                        //连续写入的寄存器个数是0x40
                        isEQ = true;
                        mePort_DataRemove(0x08);
                    }
                    else
                    {
                        mePort_DataRemove(1);
                        return;
                    }
                    break;

                case TASKS.REG_BLOCK5_CAL2:
                    if (mePort_GetUInt16(2) == Constants.REG_BLOCK5_CAL2 && mePort_GetInt16(4) == 0x50)
                    {
                        //连续写入的寄存器个数是0x50
                        isEQ = true;
                        mePort_DataRemove(0x08);
                    }
                    else
                    {
                        mePort_DataRemove(1);
                        return;
                    }
                    break;

                case TASKS.REG_BLOCK5_INFO:
                    if (mePort_GetUInt16(2) == Constants.REG_BLOCK5_INFO && mePort_GetInt16(4) == 0x50)
                    {
                        //连续写入的寄存器个数是0x50
                        isEQ = true;
                        mePort_DataRemove(0x08);
                    }
                    else
                    {
                        mePort_DataRemove(1);
                        return;
                    }
                    break;
                case TASKS.REG_BLOCK5_AM1:
                    if (mePort_GetUInt16(2) == Constants.REG_BLOCK5_AM1 && mePort_GetInt16(4) == 0x50)
                    {
                        //连续写入的寄存器个数是0x50
                        isEQ = true;
                        mePort_DataRemove(0x08);
                    }
                    else
                    {
                        mePort_DataRemove(1);
                        return;
                    }
                    break;
                case TASKS.REG_BLOCK5_AM2:
                    if (mePort_GetUInt16(2) == Constants.REG_BLOCK5_AM2 && mePort_GetInt16(4) == 0x50)
                    {
                        //连续写入的寄存器个数是0x50
                        isEQ = true;
                        mePort_DataRemove(0x08);
                    }
                    else
                    {
                        mePort_DataRemove(1);
                        return;
                    }
                    break;
                case TASKS.REG_BLOCK5_AM3:
                    if ((mePort_GetUInt16(2) == Constants.REG_BLOCK5_AM3 && mePort_GetInt16(4) == 0x50))
                    {
                        //连续写入的寄存器个数是0x50
                        isEQ = true;
                        mePort_DataRemove(0x08);
                    }
                    else
                    {
                        mePort_DataRemove(1);
                        return;
                    }
                    break;

                case TASKS.WRITE_FIFOCLEAR:
                    if ((REG)mePort_GetUInt16(2) == REG.REG_W_FIFOCLEAR && mePort_GetInt16(4) == 0x02)
                    {
                        //连续写入的寄存器个数是0x02
                        isEQ = true;
                        mePort_DataRemove(0x08);
                    }
                    else
                    {
                        mePort_DataRemove(1);
                        return;
                    }
                    break;

                case TASKS.WRITE_FIFO_INDEX:
                    if ((REG)mePort_GetUInt16(2) == REG.REG_R_RECDAT && mePort_GetInt16(4) == 0x02)
                    {
                        //连续写入的寄存器个数是0x02
                        isEQ = true;
                        mePort_DataRemove(0x08);
                    }
                    else
                    {
                        mePort_DataRemove(1);
                        return;
                    }
                    break;

                case TASKS.REG_BLOCK3_SCREW1:
                    if (mePort_GetUInt16(2) == Constants.REG_BLOCK3_SCREW1 && mePort_GetInt16(4) == 0x30)
                    {
                        //连续写入的寄存器个数是0x30
                        isEQ = true;
                        mePort_DataRemove(0x08);
                    }
                    else
                    {
                        mePort_DataRemove(1);
                        return;
                    }
                    break;

                case TASKS.REG_BLOCK3_SCREW2:
                    if (mePort_GetUInt16(2) == Constants.REG_BLOCK3_SCREW2 && mePort_GetInt16(4) == 0x30)
                    {
                        //连续写入的寄存器个数是0x30
                        isEQ = true;
                        mePort_DataRemove(0x08);
                    }
                    else
                    {
                        mePort_DataRemove(1);
                        return;
                    }
                    break;

                case TASKS.REG_BLOCK3_SCREW3:
                    if (mePort_GetUInt16(2) == Constants.REG_BLOCK3_SCREW3 && mePort_GetInt16(4) == 0x30)
                    {
                        //连续写入的寄存器个数是0x30
                        isEQ = true;
                        mePort_DataRemove(0x08);
                    }
                    else
                    {
                        mePort_DataRemove(1);
                        return;
                    }
                    break;

                case TASKS.REG_BLOCK3_SCREW4:
                    if (mePort_GetUInt16(2) == Constants.REG_BLOCK3_SCREW4 && mePort_GetInt16(4) == 0x30)
                    {
                        //连续写入的寄存器个数是0x30
                        isEQ = true;
                        mePort_DataRemove(0x08);
                    }
                    else
                    {
                        mePort_DataRemove(1);
                        return;
                    }
                    break;

                default:
                    mePort_DataRemove(1);
                    return;
            }

            //顺利解码
            MyDevice.callDelegate();
        }

        //接收触发函数,实际会由串口线程创建
        private void mePort_DataReceived()
        {
            if (is_serial_closing)
            {
                is_serial_listening = false;//准备关闭串口时，reset串口侦听标记
                return;
            }
            try
            {
                if (ListenerSocket == null)
                {
                    return;
                }

                //串口有数据时，接受数据并处理
                //循环校验每个字节
                if (rxRnt >= 8)
                {
                    //匹配地址
                    if (meRXD[rxRead] == sAddress)
                    {
                        //匹配功能码
                        switch ((CMD)mePort_GetByte(1))
                        {
                            case CMD.CMD_READ:
                                mePort_DataReceiveRead();
                                break;

                            case CMD.CMD_WRITE:
                                mePort_DataReceiveWrite();
                                break;

                            case CMD.CMD_SEQUENCE:
                                mePort_DataReceiveSequence();
                                break;

                            default:
                                mePort_DataRemove(1);
                                break;
                        }
                    }
                    else
                    {
                        Console.WriteLine(meRXD[rxRead]);
                        mePort_DataRemove(1);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + ",请检查设备的通讯连接");
                return;
            }
            finally
            {
                is_serial_listening = false;//串口调用完毕后，reset串口侦听标记
            }
        }

        //串口读取所有任务状态机 DEV -> CAL -> INFO -> WLAN -> ID -> PARA -> AM1 -> AM2 -> AM3 -> JOB -> OP -> HEART -> FIFO -> DAT -> SPEC -> SCREW1 -> SCREW2 -> SCREW3 -> SCREW4
        public void Protocol_mePort_ReadAllTasks()
        {
            //启动TASKS -> 根据任务选择指令 -> 根据接口指令装帧发送
            //mePort_DataReceived -> 串口接收字节 -> 字节解析完整帧 -> callDelegate
            //委托回调 -> 根据trTSK和rxDat和rxStr和isEQ进行下一个TASKS

            switch (trTSK)
            {
                case TASKS.NULL:
                    Protocol_Read_SendCOM(TASKS.REG_BLOCK1_DEV);
                    break;

                case TASKS.REG_BLOCK1_DEV:
                    if (isEQ)
                    {
                        Protocol_Read_SendCOM(TASKS.REG_BLOCK4_CAL1);
                    }
                    else
                    {
                        Protocol_Read_SendCOM(TASKS.REG_BLOCK1_DEV);
                    }
                    break;

                case TASKS.REG_BLOCK4_CAL1:
                    if (isEQ)
                    {
                        Protocol_Read_SendCOM(TASKS.REG_BLOCK5_CAL2);
                    }
                    else
                    {
                        Protocol_Read_SendCOM(TASKS.REG_BLOCK4_CAL1);
                    }
                    break;

                case TASKS.REG_BLOCK5_CAL2:
                    if (isEQ)
                    {
                        Protocol_Read_SendCOM(TASKS.REG_BLOCK5_INFO);
                    }
                    else
                    {
                        Protocol_Read_SendCOM(TASKS.REG_BLOCK5_CAL2);
                    }
                    break;

                case TASKS.REG_BLOCK5_INFO:
                    if (isEQ)
                    {
                        Protocol_Read_SendCOM(TASKS.REG_BLOCK3_WLAN);
                    }
                    else
                    {
                        Protocol_Read_SendCOM(TASKS.REG_BLOCK5_INFO);
                    }
                    break;

                case TASKS.REG_BLOCK3_WLAN:
                    if (isEQ)
                    {
                        Protocol_Read_SendCOM(TASKS.REG_BLOCK3_PARA);
                    }
                    else
                    {
                        Protocol_Read_SendCOM(TASKS.REG_BLOCK3_WLAN);
                    }
                    break;

                case TASKS.REG_BLOCK3_PARA:
                    if (isEQ)
                    {
                        Protocol_Read_SendCOM(TASKS.REG_BLOCK5_AM1);
                    }
                    else
                    {
                        Protocol_Read_SendCOM(TASKS.REG_BLOCK3_PARA);
                    }
                    break;

                case TASKS.REG_BLOCK5_AM1:
                    if (isEQ)
                    {
                        Protocol_Read_SendCOM(TASKS.REG_BLOCK5_AM2);
                    }
                    else
                    {
                        Protocol_Read_SendCOM(TASKS.REG_BLOCK5_AM1);
                    }
                    break;

                case TASKS.REG_BLOCK5_AM2:
                    if (isEQ)
                    {
                        Protocol_Read_SendCOM(TASKS.REG_BLOCK5_AM3);
                    }
                    else
                    {
                        Protocol_Read_SendCOM(TASKS.REG_BLOCK5_AM2);
                    }
                    break;

                case TASKS.REG_BLOCK5_AM3:
                    if (isEQ)
                    {
                        Protocol_Read_SendCOM(TASKS.REG_BLOCK3_JOB);
                    }
                    else
                    {
                        Protocol_Read_SendCOM(TASKS.REG_BLOCK5_AM3);
                    }
                    break;

                case TASKS.REG_BLOCK3_JOB:
                    if (isEQ)
                    {
                        Protocol_Read_SendCOM(TASKS.REG_BLOCK3_OP);
                    }
                    else
                    {
                        Protocol_Read_SendCOM(TASKS.REG_BLOCK3_JOB);
                    }
                    break;

                case TASKS.REG_BLOCK3_OP:
                    if (isEQ)
                    {
                        Protocol_Read_SendCOM(TASKS.REG_BLOCK1_FIFO);
                    }
                    else
                    {
                        Protocol_Read_SendCOM(TASKS.REG_BLOCK3_OP);
                    }
                    break;

                case TASKS.REG_BLOCK1_FIFO:
                    if (isEQ)
                    {
                        Protocol_Read_SendCOM(TASKS.REG_BLOCK3_SCREW1);
                    }
                    else
                    {
                        Protocol_Read_SendCOM(TASKS.REG_BLOCK1_FIFO);
                    }
                    break;

                case TASKS.REG_BLOCK3_SCREW1:
                    if (isEQ)
                    {
                        Protocol_Read_SendCOM(TASKS.REG_BLOCK3_SCREW2);
                    }
                    else
                    {
                        Protocol_Read_SendCOM(TASKS.REG_BLOCK3_SCREW1);
                    }
                    break;

                case TASKS.REG_BLOCK3_SCREW2:
                    if (isEQ)
                    {
                        Protocol_Read_SendCOM(TASKS.REG_BLOCK3_SCREW3);
                    }
                    else
                    {
                        Protocol_Read_SendCOM(TASKS.REG_BLOCK3_SCREW2);
                    }
                    break;

                case TASKS.REG_BLOCK3_SCREW3:
                    if (isEQ)
                    {
                        Protocol_Read_SendCOM(TASKS.REG_BLOCK3_SCREW4);
                    }
                    else
                    {
                        Protocol_Read_SendCOM(TASKS.REG_BLOCK3_SCREW3);
                    }
                    break;

                case TASKS.REG_BLOCK3_SCREW4:
                    if (isEQ)
                    {
                        MyDevice.mTCP[sAddress].sTATE = STATE.WORKING;
                        trTASK = TASKS.NULL;

                        if (MyDevice.addr_ip.ContainsKey(addr.ToString()) == false)
                        {
                            MyDevice.addr_ip.Add(addr.ToString(), ((IPEndPoint)ClientSocket.RemoteEndPoint).Address.ToString());
                        }
                    }
                    else
                    {
                        Protocol_Read_SendCOM(TASKS.REG_BLOCK3_SCREW4);
                    }
                    break;

                default:
                    break;
            }
        }

        //串口写入所有任务状态机 INFO -> WLAN -> ID -> PARA -> AM1 -> AM2 -> AM3 -> JOB -> OP
        public void Protocol_mePort_WriteAllTasks()
        {
            //启动TASKS -> 根据任务选择指令 -> 根据接口指令装帧 -> 发送WriteTasks
            //mePort_DataReceived -> 串口接收字节 -> 字节解析完整帧 -> callDelegate
            //委托回调 -> 根据trTSK和rxDat和rxStr和isEQ进行下一个TASKS -> 继续WriteTasks

            switch (trTSK)
            {
                case TASKS.NULL:
                    Protocool_Sequence_SendCOM(TASKS.REG_BLOCK5_INFO);
                    break;

                case TASKS.REG_BLOCK5_INFO:
                    if (isEQ)
                    {
                        Protocool_Sequence_SendCOM(TASKS.REG_BLOCK3_WLAN);
                    }
                    else
                    {
                        Protocool_Sequence_SendCOM(TASKS.REG_BLOCK5_INFO);
                    }
                    break;

                case TASKS.REG_BLOCK3_WLAN:
                    if (isEQ)
                    {
                        Protocool_Sequence_SendCOM(TASKS.REG_BLOCK3_PARA);
                    }
                    else
                    {
                        Protocool_Sequence_SendCOM(TASKS.REG_BLOCK3_WLAN);
                    }
                    break;

                case TASKS.REG_BLOCK3_PARA:
                    if (isEQ)
                    {
                        Protocool_Sequence_SendCOM(TASKS.REG_BLOCK5_AM1);
                    }
                    else
                    {
                        Protocool_Sequence_SendCOM(TASKS.REG_BLOCK3_PARA);
                    }
                    break;

                case TASKS.REG_BLOCK5_AM1:
                    if (isEQ)
                    {
                        Protocool_Sequence_SendCOM(TASKS.REG_BLOCK5_AM2);
                    }
                    else
                    {
                        Protocool_Sequence_SendCOM(TASKS.REG_BLOCK5_AM1);
                    }
                    break;

                case TASKS.REG_BLOCK5_AM2:
                    if (isEQ)
                    {
                        Protocool_Sequence_SendCOM(TASKS.REG_BLOCK5_AM3);
                    }
                    else
                    {
                        Protocool_Sequence_SendCOM(TASKS.REG_BLOCK5_AM2);
                    }
                    break;

                case TASKS.REG_BLOCK5_AM3:
                    if (isEQ)
                    {
                        Protocool_Sequence_SendCOM(TASKS.REG_BLOCK3_JOB);
                    }
                    else
                    {
                        Protocool_Sequence_SendCOM(TASKS.REG_BLOCK5_AM3);
                    }
                    break;

                case TASKS.REG_BLOCK3_JOB:
                    if (isEQ)
                    {
                        Protocool_Sequence_SendCOM(TASKS.REG_BLOCK3_OP);
                    }
                    else
                    {
                        Protocool_Sequence_SendCOM(TASKS.REG_BLOCK3_JOB);
                    }
                    break;

                case TASKS.REG_BLOCK3_OP:
                    if (isEQ)
                    {
                        MyDevice.mTCP[sAddress].sTATE = STATE.WORKING;
                        trTASK = TASKS.NULL;
                    }
                    else
                    {
                        Protocol_Read_SendCOM(TASKS.REG_BLOCK3_OP);
                    }
                    break;

                default:
                    break;
            }
        }
    }
}
