using Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Model
{
    //定义委托
    public delegate void freshHandler();

    public static class MyDevice
    {
        //User function
        public static Int32 languageType;//语言,0=中文,1=英文
        public static String userName;
        public static String userPassword;
        public static String userRole;//用户角色,0=普通用户,1=管理员,20=工厂超级用户，30=制造商
        public static String userCFG;//软件参数目录
        public static String userDAT;//账户信息目录
        public static String userLOG;//日志数据目录
        public static String userPIC;//图片和说明书目录
        public static String userPDF;//pdf保存位置
        public static String userTXT;//文本数据路径
        public static String userCSV;//日志文件路径
        public static Byte   keyLock;//按键锁,0=解锁,1=加锁,2=蜂鸣器及加锁

        //User PC Copyright
        public static string myMac;
        public static Int64 myVar;
        public static Byte myPC;
        public static Boolean IsMySqlStart;

        //User Event 定义事件
        public static event freshHandler myUpdate;

        //浮点数转换
        public static UIT myUIT = new UIT();

        //设备连接的接口类型
        //单设备的蓝牙和usb用myUART
        //wifi通讯协议用myTCP
        //射频和WiFi转接器用myXFUART
        //RS485通讯用myRS485
        public static IProtocol myTCPUART = new TCPUARTProtocol();
        public static IProtocol myUART = new UARTProtocol();
        public static IProtocol myXFUART = new XFUARTProtocol();
        public static IProtocol myRS485 = new RS485Protocol();

        //设备连接的接口
        public static IProtocol protocol; //接口

        //设备数据
        public static XET[] mBUS = new XET[256];   //USB UART
        public static XET[] mTCP = new XET[256];   //wifi TCP
        public static XET[] mXF = new XET[256];    //射频,wfi,接收器
        public static XET[] mRS = new XET[256];    //RS485

        //多设备地址集合
        public static List<Byte> AddrList = new List<Byte>();

        //socket客户端
        public static Dictionary<string, Socket> clientConnectionItems = new Dictionary<string, Socket> { };// 存储客户端连接Socket

        //addr_ip客户端ip和扳手ID
        public static Dictionary<string, string> addr_ip = new Dictionary<string, string> { };//绑定扳手的地址和ip地址


        //指令管理
        public static TaskManager myTaskManager = new TaskManager();

        //工单信息
        public static String DataType = "ActualData";  //数据类型（工单/非工单）
        public static UInt32 WorkId = 0;               //工单标识符
        public static String WorkNum = "";             //工单号（前缀）
        public static String SequenceId = "";          //工单序列号（后缀）
        public static String PointNum = "";            //点位
        public static String DataResult = "NG";        //数据结果（合格/不合格）
        public static String Vin = "";                 //一段数据集合标识

        //蓝牙接收器
        public static Boolean IsUnbind;                //接收器能否解绑

        //设备连接信息（存在数据库中，扳手无法读取的信息）
        public static String ConnectType;              //扳手历史连接方式
        public static String ConnectAuto;              //扳手允许自动连接
        public static int ConnectDevCnt;               //连接的总设备数
        public static int WorkDevCnt;                  //工作的设备数

        //临时变量（用于程序上未做好时，调用静态变量）
        public static Int32 angleResist;              //复拧角度


        /// <summary>
        /// protocol指向的设备数量
        /// </summary>
        public static Int32 devSum
        {
            get
            {
                int num = 0;
                AddrList.Clear();

                switch (protocol.type)
                {
                    default:
                    case COMP.UART:
                        for (int i = 1; i < 256; i++)
                        {
                            switch (mBUS[i].sTATE)
                            {
                                case STATE.INVALID:
                                case STATE.OFFLINE:
                                    break;

                                case STATE.CONNECTED:
                                case STATE.WORKING:
                                    AddrList.Add(1);
                                    return 1;
                            }
                        }
                        return 0;

                    case COMP.TCP:
                        for (int i = 1; i < 256; i++)
                        {
                            switch (mTCP[i].sTATE)
                            {
                                case STATE.INVALID:
                                case STATE.OFFLINE:
                                    break;

                                case STATE.CONNECTED:
                                case STATE.WORKING:
                                    AddrList.Add((Byte)i);
                                    num++;
                                    break;
                            }
                        }
                        return num;

                    case COMP.XF:
                        for (int i = 1; i < 256; i++)
                        {
                            switch (mXF[i].sTATE)
                            {
                                case STATE.INVALID:
                                case STATE.OFFLINE:
                                    break;

                                case STATE.CONNECTED:
                                case STATE.WORKING:
                                    AddrList.Add((Byte)i);
                                    num++;
                                    break;
                            }
                        }
                        return num;

                    case COMP.RS485:
                        for (int i = 1; i < 256; i++)
                        {
                            switch (mRS[i].sTATE)
                            {
                                case STATE.INVALID:
                                case STATE.OFFLINE:
                                    break;

                                case STATE.CONNECTED:
                                case STATE.WORKING:
                                    AddrList.Add((Byte)i);
                                    num++;
                                    break;
                            }
                        }
                        return num;
                }
            }
        }

        /// <summary>
        /// protocol.addr指向的设备
        /// </summary>
        public static XET actDev
        {
            get
            {
                switch (protocol.type)
                {
                    default:
                    case COMP.UART:
                        return mBUS[protocol.addr];

                    case COMP.TCP:
                        return mTCP[protocol.addr];

                    case COMP.XF:
                        return mXF[protocol.addr];

                    case COMP.RS485:
                        return mRS[protocol.addr];
                }
            }
        }

        /// <summary>
        /// protocol.addr指向的设备数组
        /// </summary>
        public static XET[] mDev
        {
            get
            {
                switch (protocol.type)
                {
                    default:
                    case COMP.UART:
                        return mBUS;

                    case COMP.TCP:
                        return mTCP;

                    case COMP.XF:
                        return mXF;

                    case COMP.RS485:
                        return mRS;
                }
            }
        }

        /// <summary>
        /// protocol指向的状态为目标状态的设备index
        /// </summary>
        public static List<Int32> GetDevState(STATE sTATE)
        {
            switch (protocol.type)
            {
                default:
                case COMP.UART:
                    return GetDevices(mBUS, sTATE);
                case COMP.TCP:
                    return GetDevices(mTCP, sTATE);
                case COMP.XF:
                    return GetDevices(mXF, sTATE);
                case COMP.RS485:
                    return GetDevices(mRS, sTATE);
            }
        }

        /// <summary>
        /// 目标状态的设备index
        /// </summary>
        private static List<Int32> GetDevices(XET[] devices, STATE sTATE)
        {
            List<Int32> mDevices = new List<Int32>();
            for (int i = 1; i < 256; i++)
            {
                if (devices[i].sTATE == sTATE)
                {
                    mDevices.Add(i);
                }
            }
            return mDevices;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        static MyDevice()
        {
            protocol = myUART;
            //
            languageType = 0;
            userRole = "0";
            userCFG = Application.StartupPath + @"\cfg";
            userDAT = Application.StartupPath + @"\dat";
            userLOG = Application.StartupPath + @"\log";
            userPIC = Application.StartupPath + @"\pic";
            userPDF = Application.StartupPath + @"\out";
            //
            myPC = 0;
            myMac = "";
            myVar = 0;
            IsMySqlStart = false;

            //
            for (int i = 0; i < 256; i++)
            {
                mBUS[i] = new XET();
                mBUS[i].sTATE = STATE.INVALID;
            }

            //
            for (int i = 0; i < 256; i++)
            {
                mXF[i] = new XET();
                mXF[i].sTATE = STATE.INVALID;
            }

            //
            for (int i = 0; i < 256; i++)
            {
                mTCP[i] = new XET();
                mTCP[i].sTATE = STATE.INVALID;
            }

            //
            for (int i = 0; i < 256; i++)
            {
                mRS[i] = new XET();
                mRS[i].sTATE = STATE.INVALID;
            }

            //
            IsUnbind = false;

            //
            ConnectType = "有线连接";
            ConnectAuto = "False";
            ConnectDevCnt = 1;
            WorkDevCnt = 0;

            //
            angleResist = 5000;
        }

        /// <summary>
        /// 执行委托
        /// </summary>
        public static void callDelegate()
        {
            //委托
            if (myUpdate != null)
            {
                myUpdate();
            }
        }

        /// <summary>
        /// 设置协议
        /// </summary>
        /// <param name="comp"></param>
        public static void mePort_SetProtocol(COMP comp)
        {
            if (protocol.type != comp)
            {
                //注意
                //如果重新new SelfUARTProtocol()或new RS485Protocol()
                //会因为protocol释放空间导致打开的mePort丢失句柄
                //所以new情况下必须mePort_Close();
                protocol.Protocol_PortClose();

                //
                switch (comp)
                {
                    case COMP.UART:
                        protocol = myUART;
                        break;

                    case COMP.TCP:
                        protocol = myTCPUART;
                        break;

                    case COMP.XF:
                        protocol = myXFUART;
                        break;

                    case COMP.RS485:
                        protocol = myRS485;
                        break;
                }
            }
        }


        /// <summary>
        /// 保存开机自动连接
        /// </summary>
        /// <param name="language"></param>
        public static void SaveConnectType(bool isEnable)
        {
            //空
            if (!Directory.Exists(MyDevice.userDAT))
            {
                Directory.CreateDirectory(MyDevice.userDAT);
            }

            //写入
            try
            {
                string mePath = MyDevice.userDAT + @"\AutoStart.txt";//设置文件路径
                if (File.Exists(mePath))
                {
                    System.IO.File.SetAttributes(mePath, FileAttributes.Normal);
                }
                if (isEnable)
                {
                    File.WriteAllText(mePath, "1");
                }
                else
                {
                    File.WriteAllText(mePath, "0");
                }
                System.IO.File.SetAttributes(mePath, FileAttributes.ReadOnly);
            }
            catch
            {
            }
        }

        /// <summary>
        /// 读取连接方式
        /// </summary>
        /// <param name="filePath"></param>
        public static bool ReadConnetType(String filePath)
        {
            bool IsAuto = false;
            //空
            if (!Directory.Exists(MyDevice.userDAT))
            {
                IsAuto = false;
            }

            //读取
            try
            {
                if (File.Exists(filePath))
                {
                    string connectType = File.ReadAllText(filePath);
                    IsAuto = connectType == "1" ? true : false;
                }
            }
            catch
            {
            }

            return IsAuto;
        }

        //获取波特率（从字节转换成有效展示值）
        public static int GetBaud(byte baud)
        {
            int showBaud = 115200;

            switch (baud)
            {
                case 0:
                    showBaud = 1200;
                    break;
                case 1:
                    showBaud = 2400;
                    break;
                case 2:
                    showBaud = 4800;
                    break;
                case 3:
                    showBaud = 9600;
                    break;
                case 4:
                    showBaud = 14400;
                    break;
                case 5:
                    showBaud = 19200;
                    break;
                case 6:
                    showBaud = 38400;
                    break;
                case 7:
                    showBaud = 57600;
                    break;
                case 8:
                    showBaud = 115200;
                    break;
                case 255:
                    showBaud = 115200;
                    break;
                default:
                    break;
            }
            return showBaud;
        }

        //获取停止位
        public static byte GetStopBit(byte stopbit)
        {
            byte showStopBit = 1;

            switch (stopbit)
            {
                case 1:
                    showStopBit = 1;
                    break;
                case 2:
                    showStopBit = 2;
                    break;
                case 255:
                    showStopBit = 1;
                    break;
                default:
                    break;
            }

            return showStopBit;
        }

        //获取校验位
        public static string GetParity(byte parity)
        {
            string showParity = "None";

            switch (parity)
            {
                case 0:
                    showParity = "None";
                    break;
                case 1:
                    showParity = "Odd(奇校验)";
                    break;
                case 2:
                    showParity = "Even(偶校验)";
                    break;
                case 3:
                    showParity = "Mark";
                    break;
                case 4:
                    showParity = "Space";
                    break;
                case 255:
                    showParity = "None";
                    break;
                default:
                    break;
            }

            return showParity;
        }
    
        //UInt32转换成日期 (仅适用于20240820类似截至到天的值)
        public static DateTime UInt32ToDateTime(UInt32 number)
        {
            int year = (int)(number / 10000);
            int month = (int)((number % 10000) / 100);
            int day = (int)(number % 100);

            DateTime date = new DateTime(year, month, day);
            return date;
        }

        //日期转换成UInt32
        public static UInt32 DateTimeToUInt32(DateTime time)
        {
            uint dateValue = 0;
            if (time.Year > 9999)
            {
                MessageBox.Show("年限超过了正常范畴，数据无效", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0;
            }
            dateValue = (uint)(time.Year * 10000 + time.Month * 100 + time.Day);

            return dateValue;
        }

        //检查电脑的IP
        public static List<string> GetIPList()
        {
            //获取本地的IP地址
            string AddressIP = string.Empty;
            List<string> getIPList = new List<string>();
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    AddressIP = _IPAddress.ToString();
                    if (!getIPList.Contains(AddressIP))
                    {
                        getIPList.Add(AddressIP);
                    }
                }
            }
            return getIPList;
        }
    }
}
