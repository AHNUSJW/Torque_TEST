using DBHelper;
using Library;
using Model;
using System;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

//Ricardo 20240418

namespace Base.UI.MenuDevice
{
    public partial class MenuConnectForm : Form
    {
        //变量初始化
        private String myCOM = "COM1";
        private Boolean isConnecting = false; //是否正在连接中

        private string comPort;               //串口号
        private string ipPort;                //端口号

        private TASKS meTask = TASKS.NULL;    //当前状态机

        private XET actXET;                   //需操作的设备
        private Byte addr = 1;                //扫描站点1-255
        volatile bool isXFScan = false;       //是否点击接收器扫描
        volatile bool isTCPScan = false;      //是否点击wifi扫描
        volatile bool isRS485Scan = false;    //是否点击RS485扫描
        private volatile byte connectID = 0;  //连接的设备，递增（用于TCP连接）
        private int readDevTick = 0;          //读取设备信息指令次数（作为扫描过程中判定是否切换当前设备）

        public MenuConnectForm()
        {
            InitializeComponent();
        }

        private void MenuConnectForm_Load(object sender, EventArgs e)
        {
            //暂停自动任务管理
            MyDevice.myTaskManager.Pause();
            //清除指令序列（防止上次工单指令未结束，又打开新工单新指令，产生多次蜂鸣）
            MyDevice.myTaskManager.ClearCommand();

            //初始化设备数据
            MyDevice.protocol.addr = addr;
            actXET = MyDevice.actDev;
            MyDevice.protocol.trTASK = TASKS.NULL;

            //页面初始化
            bt_connect.Enabled = false;
            bt_scan.Enabled = false;
            btn_unbind.Enabled = false;
        }

        private void MenuConnectForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isConnecting)
            {
                MessageBox.Show("设备正在连接中，请稍等...", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                e.Cancel = true;
            }
            else
            {
                MyDevice.myUpdate -= new freshHandler(receiveData);

                closePort();

                timer_USB.Enabled = false;
                timer_XF.Enabled = false;
                timer_TCP.Enabled = false;
                timer_RS485.Enabled = false;

                //重启任务管理
                MyDevice.myTaskManager.Resume();
            }

        }

        //连接模式切换
        private void treeViewEx1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            //RS485专属
            label_baud.Visible = false;
            label_stopbit.Visible = false;
            label_parity.Visible = false;
            comboBox_baud.Visible = false;
            comboBox_stopbit.Visible = false;
            comboBox_parity.Visible = false;

            switch (e.Node.Text.Trim())
            {
                case "有线连接":
                    label13.Text = "串口:";
                    label17.Visible = false;
                    textBox1.Visible = false;
                    bt_scan.Visible = false;
                    btn_unbind.Visible = false;
                    break;
                case "蓝牙连接":
                    label13.Text = "串口:";
                    label17.Visible = true;
                    textBox1.Visible = true;
                    bt_scan.Visible = true;
                    btn_unbind.Visible = true;
                    break;
                case "RS485连接":
                    label17.Visible = true;
                    textBox1.Visible = true;
                    bt_scan.Visible = true;
                    btn_unbind.Visible = false;

                    //RS485专属
                    label_baud.Visible = true;
                    label_stopbit.Visible = true;
                    label_parity.Visible = true;
                    comboBox_baud.Visible = true;
                    comboBox_stopbit.Visible = true;
                    comboBox_parity.Visible = true;

                    comboBox_baud.SelectedIndex = 3;//当前版本XH08暂时优先波特率9600
                    comboBox_stopbit.SelectedIndex = 0;
                    comboBox_parity.SelectedIndex = 0;
                    break;
                case "接收器连接":
                    label13.Text = "串口:";
                    label17.Visible = true;
                    textBox1.Visible = true;
                    bt_scan.Visible = true;
                    btn_unbind.Visible = false;
                    break;
                case "路由器WiFi连接":
                    label13.Text = "端口:";
                    label17.Visible = true;
                    textBox1.Visible = true;
                    bt_scan.Visible = true;
                    btn_unbind.Visible = false;
                    break;
                default:
                    break;
            }

            //针对蓝牙通讯，timer1的时间需要拉长
            //防止出现上一次发送指令没回，下一次指令发送收到的回复的是上一条的
            if (e.Node.Text.Trim() == "蓝牙连接")
            {
                timer_USB.Interval = 200;
            }
            else
            {
                timer_USB.Interval = 100;
            }

            bt_refresh_Click(null, null);
        }

        //文本框文本超出自动滚屏
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            //自动滚屏
            textBox2.SelectionStart = textBox2.Text.Length;
            textBox2.ScrollToCaret();
        }

        //串口刷新
        private void bt_refresh_Click(object sender, EventArgs e)
        {
            //TCP
            if (treeViewEx1.SelectedNode.Text.Trim() == "路由器WiFi连接")
            {
                comboBox0_port.Items.Clear();
                //获取本地的ip
                string str = comboBox0_port.Text;
                getIP();
                if (str != comboBox0_port.Text)
                {
                    MyDevice.protocol.Protocol_PortClose();
                    bt_close.Enabled = true;
                }
            }
            else
            {
                if (MyDevice.devSum > 0)
                {
                    string connectedInfo = "";
                    for (int i = 0; i < MyDevice.devSum; i++)
                    {
                        if ((MyDevice.protocol.type == COMP.XF && MyDevice.mXF[MyDevice.AddrList[i]].sTATE == STATE.WORKING) ||
                            (MyDevice.protocol.type == COMP.TCP && MyDevice.mTCP[MyDevice.AddrList[i]].sTATE == STATE.WORKING) ||
                            (MyDevice.protocol.type == COMP.RS485 && MyDevice.mRS[MyDevice.AddrList[i]].sTATE == STATE.WORKING) ||
                            (MyDevice.protocol.type == COMP.UART && MyDevice.mBUS[MyDevice.AddrList[i]].sTATE == STATE.WORKING)
                            )
                        {
                            connectedInfo += MyDevice.AddrList[i] + "，";
                        }
                    }
                    if (connectedInfo != "")
                    {
                        textBox2.Text = MyDevice.languageType == 0 ? $"设备{connectedInfo}已连接" : $"The device{connectedInfo} is connected.";
                    }
                }

                //刷串口
                comboBox0_port.Items.Clear();
                comboBox0_port.Items.AddRange(SerialPort.GetPortNames());

                //无串口
                if (comboBox0_port.Items.Count == 0)
                {
                    comboBox0_port.Text = null;
                    myCOM = null;
                }
                //有可用串口
                else
                {
                    //
                    if (comboBox0_port.SelectedIndex < 0)
                    {
                        comboBox0_port.SelectedIndex = 0;
                    }
                    myCOM = comboBox0_port.Text;
                }
            }
        }

        //串口连接
        private void bt_connect_Click(object sender, EventArgs e)
        {
            if (isXFScan || isTCPScan || isRS485Scan)
            {
                MessageBox.Show("设备扫描中，无法打断，请关闭扫描", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (treeViewEx1.SelectedNode.Text.Trim() == "有线连接")
            {
                //切换自定义通讯
                MyDevice.mePort_SetProtocol(COMP.UART);
                MyDevice.ConnectType = "有线连接";

                //更新设备，有线设备统一地址为1。防止用户扫描后地址改变，需要重新更新
                MyDevice.protocol.addr = 1;
                actXET = MyDevice.actDev;

                //打开串口
                comPort = comboBox0_port.Text;
                if (bt_close.BackColor == Color.Green) MyDevice.protocol.Protocol_PortClose();
                MyDevice.protocol.Protocol_PortOpen(comPort, 115200, StopBits.One, Parity.None);

                if (MyDevice.protocol.IsOpen)
                {
                    textBox2.Text = MyDevice.languageType == 0 ? "适配器已打开\r\n搜索中 ." : "Adapter turned on. \r\nIn the search .";
                    bt_connect.BackColor = Color.OrangeRed;
                    timer_USB.Enabled = true;
                    meTask = TASKS.NULL;
                    MyDevice.protocol.trTASK = TASKS.NULL;
                }
                else
                {
                    textBox2.Text = MyDevice.languageType == 0 ? "适配器打开失败\r\n" : "Adapter opening failed. \r\n";
                    bt_connect.BackColor = Color.Firebrick;
                }
            }
            else
            {
                if (treeViewEx1.SelectedNode.Text.Trim() == "接收器连接" || treeViewEx1.SelectedNode.Text.Trim() == "蓝牙连接")
                {
                    //切换XF通讯
                    MyDevice.mePort_SetProtocol(COMP.XF);
                    MyDevice.ConnectType = treeViewEx1.SelectedNode.Text.Trim();

                    //打开串口
                    comPort = comboBox0_port.Text;
                    MyDevice.protocol.Protocol_PortOpen(comPort, 115200, StopBits.One, Parity.None);
                }
                else if (treeViewEx1.SelectedNode.Text.Trim() == "路由器WiFi连接")
                {
                    //切换TCP通讯
                    MyDevice.mePort_SetProtocol(COMP.TCP);
                    MyDevice.ConnectType = treeViewEx1.SelectedNode.Text.Trim();

                    //打开端口
                    ipPort = comboBox0_port.Text;
                    MyDevice.protocol.Protocol_PortOpen(ipPort, 5678, StopBits.One, Parity.None);
                }
                else if (treeViewEx1.SelectedNode.Text.Trim() == "RS485连接")
                {
                    //根据波特率选择扫描间隔
                    switch (comboBox_baud.Text)
                    {
                        case "2400": timer_RS485.Interval = 800; break; 
                        case "4800": timer_RS485.Interval = 400; break; 
                        case "9600": timer_RS485.Interval = 300; break; 
                        case "14400": timer_RS485.Interval = 200; break;
                        case "19200": timer_RS485.Interval = 200; break;
                        case "38400": timer_RS485.Interval = 200; break;
                        case "57600": timer_RS485.Interval = 200; break;
                        case "115200": timer_RS485.Interval = 100; break;
                        default: break;
                    }

                    //切换RS485通讯
                    MyDevice.mePort_SetProtocol(COMP.RS485);
                    MyDevice.ConnectType = treeViewEx1.SelectedNode.Text.Trim();

                    //打开串口
                    comPort = comboBox0_port.Text;
                    MyDevice.protocol.Protocol_PortOpen(comPort,
                        Convert.ToInt32(comboBox_baud.Text),
                        (StopBits)(comboBox_stopbit.SelectedIndex + 1),
                        (Parity)comboBox_parity.SelectedIndex);
                }

                //站点地址
                if (textBox1.Text == "")
                {
                    MessageBox.Show("设备ID不得为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                if (Convert.ToUInt16(textBox1.Text) > 0 && Convert.ToUInt16(textBox1.Text) < 256)
                {
                    addr = Convert.ToByte(textBox1.Text);
                    MyDevice.protocol.addr = Convert.ToByte(textBox1.Text);
                    actXET = MyDevice.actDev;
                }
                else
                {
                    MessageBox.Show("设备ID不得超出 1 - 255 的范围");
                    return;
                }

                if (MyDevice.protocol.IsOpen)
                {
                    textBox2.Text = MyDevice.languageType == 0 ? "适配器已打开\r\n搜索中 ." : "Adapter turned on. \r\nIn the search .";
                    bt_connect.BackColor = Color.OrangeRed;
                    meTask = TASKS.NULL;
                    MyDevice.protocol.Protocol_ClearState();
                    if (MyDevice.protocol.type == COMP.XF)
                    {
                        timer_USB.Enabled = true;//XF单连接可以走USB定时器，更快
                    }
                    else if (MyDevice.protocol.type == COMP.TCP)
                    {
                        connectID = 0;
                        timer_TCP.Enabled = true;
                    }
                    else if (MyDevice.protocol.type == COMP.RS485)
                    {
                        timer_RS485.Enabled = true;
                    }
                    MyDevice.protocol.trTASK = TASKS.NULL;
                }
                else
                {
                    textBox2.Text = MyDevice.languageType == 0 ? "适配器打开失败\r\n" : "Adapter opening failed. \r\n";
                    bt_connect.BackColor = Color.Firebrick;
                }
            }
        }

        //串口开关
        private void bt_close_Click(object sender, EventArgs e)
        {
            if (treeViewEx1.SelectedNode.Text.Trim() != "路由器WiFi连接")
            {
                //串口不为空
                if (comboBox0_port.Text != "")
                {
                    if (bt_close.BackColor == Color.Green)
                    {
                        //关闭串口
                        MyDevice.protocol.Protocol_PortClose();
                        MyDevice.myUpdate -= new freshHandler(receiveData);
                        MyDevice.protocol.trTASK = TASKS.NULL;

                        closePort();
                    }
                    else
                    {
                        //打开端口
                        MyDevice.myUpdate += new freshHandler(receiveData);
                        openPort();
                    }
                }
                else
                {
                    MessageBox.Show("串口不得为空");
                }
            }
            else
            {
                if (comboBox0_port.Text == "")
                {
                    MessageBox.Show("ip地址不得为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                //检测并关闭端口
                if (bt_close.BackColor == Color.Green)
                {
                    try
                    {
                        //关闭端口
                        MyDevice.protocol.Protocol_PortClose();
                        MyDevice.myUpdate -= new freshHandler(receiveData);
                        MyDevice.protocol.trTASK = TASKS.NULL;
                        MyDevice.clientConnectionItems.Clear();

                        closePort();
                    }
                    catch
                    {
                        textBox1.Text = MyDevice.languageType == 0 ? "未能正确关闭端口" : "Failed to properly close the serial port";
                    }
                }
                else
                {
                    MyDevice.myUpdate += new freshHandler(receiveData);
                    openPort();
                    //切换TCP通讯
                    MyDevice.mePort_SetProtocol(COMP.TCP);
                    MyDevice.ConnectType = treeViewEx1.SelectedNode.Text.Trim();

                    //打开端口
                    ipPort = comboBox0_port.Text;
                    MyDevice.protocol.Protocol_PortOpen(ipPort, 5678, StopBits.One, Parity.None);
                }
            }
        }

        //扫描功能
        private void bt_scan_Click(object sender, EventArgs e)
        {
            if (treeViewEx1.SelectedNode.Text.Trim() == "接收器连接" || treeViewEx1.SelectedNode.Text.Trim() == "蓝牙连接")
            {
                timer_USB.Enabled = false;
                timer_TCP.Enabled = false;
                timer_RS485.Enabled = false;
                XFScan();
            }
            else if (treeViewEx1.SelectedNode.Text.Trim() == "路由器WiFi连接")
            {
                timer_USB.Enabled = false;
                timer_XF.Enabled = false;
                timer_RS485.Enabled = false;
                TCPScan();
            }
            else if (treeViewEx1.SelectedNode.Text.Trim() == "RS485连接")
            {
                timer_USB.Enabled = false;
                timer_XF.Enabled = false;
                timer_TCP.Enabled = false;
                RS485Scan();
            }
        }

        //解除绑定（针对XH-09）
        private void btn_unbind_Click(object sender, EventArgs e)
        {
            MyDevice.mePort_SetProtocol(COMP.XF);
            MyDevice.ConnectType = treeViewEx1.SelectedNode.Text.Trim();
            //打开串口
            comPort = comboBox0_port.Text;
            MyDevice.protocol.Protocol_ClearState();
            MyDevice.protocol.Protocol_PortOpen(comPort, 115200, StopBits.One, Parity.None);
            MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_R_BLUETOOTH_UNBIND);
            meTask = MyDevice.protocol.trTASK;
        }

        //有线连接——定时器1
        private void timer1_Tick(object sender, EventArgs e)
        {
            textBox2.Text += ".";
            MyDevice.protocol.Protocol_mePort_ReadAllTasks();
            meTask = MyDevice.protocol.trTASK;
        }

        //接收器连接——定时器2
        private void timer2_Tick(object sender, EventArgs e)
        {
            //扫描多设备
            if (isXFScan)
            {
                //扫描地址1-255
                if ((addr) != 0)
                {
                    //

                    textBox1.Text = addr.ToString();

                    //清除串口任务
                    //扫描-先发送REG_BLOCK1_DEV,
                    //没回复直接扫描下一个站点,
                    //回复了继续发送剩余的指令,发完读取指令，继续下一个站点
                    MyDevice.protocol.Protocol_ClearState();

                    textBox2.Text += ".";

                    switch (meTask)
                    {
                        case TASKS.NULL:
                            MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK1_DEV);
                            meTask = TASKS.REG_BLOCK1_DEV;
                            break;

                        case TASKS.REG_BLOCK1_DEV:
                            if (MyDevice.protocol.isEqual)
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK4_CAL1);
                                meTask = TASKS.REG_BLOCK4_CAL1;
                            }
                            else
                            {
                                addr++;
                                MyDevice.protocol.addr = addr;
                                actXET = MyDevice.actDev;

                                //校验未通过补发
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK1_DEV);
                                meTask = TASKS.REG_BLOCK1_DEV;
                            }
                            break;

                        case TASKS.REG_BLOCK4_CAL1:
                            if (MyDevice.protocol.isEqual)
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK5_CAL2);
                                meTask = TASKS.REG_BLOCK5_CAL2;
                            }
                            else
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK4_CAL1);
                                meTask = TASKS.REG_BLOCK4_CAL1;
                            }
                            break;

                        case TASKS.REG_BLOCK5_CAL2:
                            if (MyDevice.protocol.isEqual)
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK5_INFO);
                                meTask = TASKS.REG_BLOCK5_INFO;
                            }
                            else
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK5_CAL2);
                                meTask = TASKS.REG_BLOCK5_CAL2;
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
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_PARA);
                                meTask = TASKS.REG_BLOCK3_PARA;
                            }
                            else
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_WLAN);
                                meTask = TASKS.REG_BLOCK3_WLAN;
                            }
                            break;

                        case TASKS.REG_BLOCK3_PARA:
                            if (MyDevice.protocol.isEqual)
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK5_AM1);
                                meTask = TASKS.REG_BLOCK5_AM1;
                            }
                            else
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_PARA);
                                meTask = TASKS.REG_BLOCK3_PARA;
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
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK1_FIFO);
                                meTask = TASKS.REG_BLOCK1_FIFO;
                            }
                            else
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_OP);
                                meTask = TASKS.REG_BLOCK3_OP;
                            }
                            break;

                        case TASKS.REG_BLOCK1_FIFO:
                            if (MyDevice.protocol.isEqual)
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_SCREW1);
                                meTask = TASKS.REG_BLOCK3_SCREW1;
                            }
                            else
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK1_FIFO);
                                meTask = TASKS.REG_BLOCK1_FIFO;
                            }
                            break;

                        case TASKS.REG_BLOCK3_SCREW1:
                            if (MyDevice.protocol.isEqual)
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_SCREW2);
                                meTask = TASKS.REG_BLOCK3_SCREW2;
                            }
                            else
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_SCREW1);
                                meTask = TASKS.REG_BLOCK3_SCREW1;
                            }
                            break;

                        case TASKS.REG_BLOCK3_SCREW2:
                            if (MyDevice.protocol.isEqual)
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_SCREW3);
                                meTask = TASKS.REG_BLOCK3_SCREW3;
                            }
                            else
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_SCREW2);
                                meTask = TASKS.REG_BLOCK3_SCREW2;
                            }
                            break;

                        case TASKS.REG_BLOCK3_SCREW3:
                            if (MyDevice.protocol.isEqual)
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_SCREW4);
                                meTask = TASKS.REG_BLOCK3_SCREW4;
                            }
                            else
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_SCREW3);
                                meTask = TASKS.REG_BLOCK3_SCREW3;
                            }
                            break;

                        case TASKS.REG_BLOCK3_SCREW4:
                            if (MyDevice.protocol.isEqual)
                            {
                                actXET.sTATE = STATE.WORKING;
                                meTask = TASKS.NULL;
                            }
                            else
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_SCREW4);
                                meTask = TASKS.REG_BLOCK3_SCREW4;
                            }
                            break;

                        default:
                            break;
                    }
                }
                //扫描结束
                else
                {
                    timer_XF.Enabled = false;//关闭扫描

                    meTask = TASKS.NULL;
                    bt_scan.Text = "完 成";
                    bt_scan.BackColor = Color.Green;

                }
            }
            else
            {
                //连接单设备
                textBox2.Text += ".";
                MyDevice.protocol.Protocol_mePort_ReadAllTasks();
                meTask = MyDevice.protocol.trTASK;
            }
        }

        //路由器连接——定时器3
        private void timer3_Tick(object sender, EventArgs e)
        {
            //切换Ip，每台连接的设备分配的ip不一致
            if (MyDevice.clientConnectionItems.Count != 0 && treeViewEx1.SelectedNode.Text.Trim() == "路由器WiFi连接")
            {
                MyDevice.protocol.port = MyDevice.clientConnectionItems.Values.ElementAt(connectID);
            }

            //扫描多连接
            if (isTCPScan)
            {
                //扫描地址1-255
                if ((addr) != 0)
                {
                    //由于每次扫描结束之后从头开始，故将working的设备跳过，以防重复回复
                    //从头开始的原因：由于一开始给设备分配IP地址时，先后顺序是随机的，不一定就是根据扳手站点排序
                    //示例：设备1和3均被分配Ip，可能一开始读取的ip是设备3的，此时只有设备3能响应，不从头开始的话设备1跳过了
                    if (actXET.sTATE == STATE.WORKING)
                    {
                        addr++;
                        MyDevice.protocol.addr = addr;
                        actXET = MyDevice.actDev;
                    }

                    //
                    textBox1.Text = addr.ToString();
                    textBox2.Text += ".";

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
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK4_CAL1);
                                meTask = TASKS.REG_BLOCK4_CAL1;
                            }
                            else
                            {
                                addr++;
                                MyDevice.protocol.addr = addr;
                                actXET = MyDevice.actDev;

                                //校验未通过补发
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK1_DEV);
                                meTask = TASKS.REG_BLOCK1_DEV;
                            }
                            break;

                        case TASKS.REG_BLOCK4_CAL1:
                            if (MyDevice.protocol.isEqual)
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK5_CAL2);
                                meTask = TASKS.REG_BLOCK5_CAL2;
                            }
                            else
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK4_CAL1);
                                meTask = TASKS.REG_BLOCK4_CAL1;
                            }
                            break;

                        case TASKS.REG_BLOCK5_CAL2:
                            if (MyDevice.protocol.isEqual)
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK5_INFO);
                                meTask = TASKS.REG_BLOCK5_INFO;
                            }
                            else
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK5_CAL2);
                                meTask = TASKS.REG_BLOCK5_CAL2;
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
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_PARA);
                                meTask = TASKS.REG_BLOCK3_PARA;
                            }
                            else
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_WLAN);
                                meTask = TASKS.REG_BLOCK3_WLAN;
                            }
                            break;

                        case TASKS.REG_BLOCK3_PARA:
                            if (MyDevice.protocol.isEqual)
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK5_AM1);
                                meTask = TASKS.REG_BLOCK5_AM1;
                            }
                            else
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_PARA);
                                meTask = TASKS.REG_BLOCK3_PARA;
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
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK1_FIFO);
                                meTask = TASKS.REG_BLOCK1_FIFO;
                            }
                            else
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_OP);
                                meTask = TASKS.REG_BLOCK3_OP;
                            }
                            break;

                        case TASKS.REG_BLOCK1_FIFO:
                            if (MyDevice.protocol.isEqual)
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_SCREW1);
                                meTask = TASKS.REG_BLOCK3_SCREW1;
                            }
                            else
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK1_FIFO);
                                meTask = TASKS.REG_BLOCK1_FIFO;
                            }
                            break;

                        case TASKS.REG_BLOCK3_SCREW1:
                            if (MyDevice.protocol.isEqual)
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_SCREW2);
                                meTask = TASKS.REG_BLOCK3_SCREW2;
                            }
                            else
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_SCREW1);
                                meTask = TASKS.REG_BLOCK3_SCREW1;
                            }
                            break;

                        case TASKS.REG_BLOCK3_SCREW2:
                            if (MyDevice.protocol.isEqual)
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_SCREW3);
                                meTask = TASKS.REG_BLOCK3_SCREW3;
                            }
                            else
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_SCREW2);
                                meTask = TASKS.REG_BLOCK3_SCREW2;
                            }
                            break;

                        case TASKS.REG_BLOCK3_SCREW3:
                            if (MyDevice.protocol.isEqual)
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_SCREW4);
                                meTask = TASKS.REG_BLOCK3_SCREW4;
                            }
                            else
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_SCREW3);
                                meTask = TASKS.REG_BLOCK3_SCREW3;
                            }
                            break;

                        case TASKS.REG_BLOCK3_SCREW4:
                            if (MyDevice.protocol.isEqual)
                            {
                                meTask = TASKS.NULL;
                            }
                            else
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_SCREW4);
                                meTask = TASKS.REG_BLOCK3_SCREW4;
                            }
                            break;

                        default:
                            break;
                    }
                }
                //扫描结束
                else
                {
                    timer_TCP.Enabled = false;//关闭扫描

                    meTask = TASKS.NULL;
                    bt_scan.Text = "完 成";
                    bt_scan.BackColor = Color.Green;

                }
            }
            else
            {
                //单连接情况
                textBox2.Text += ".";
                MyDevice.protocol.Protocol_mePort_ReadAllTasks();
                meTask = MyDevice.protocol.trTASK;
            }
        }

        //RS485连接——定时器3
        private void timer_RS485_Tick(object sender, EventArgs e)
        {
            //扫描多设备
            if (isRS485Scan)
            {
                //扫描地址1-255
                if ((addr) != 0)
                {
                    //

                    textBox1.Text = addr.ToString();

                    //清除串口任务
                    //扫描-先发送REG_BLOCK1_DEV,
                    //没回复直接扫描下一个站点,
                    //回复了继续发送剩余的指令,发完读取指令，继续下一个站点
                    MyDevice.protocol.Protocol_ClearState();

                    textBox2.Text += ".";

                    switch (meTask)
                    {
                        case TASKS.NULL:
                            MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK1_DEV);
                            meTask = TASKS.REG_BLOCK1_DEV;
                            break;

                        case TASKS.REG_BLOCK1_DEV:
                            if (MyDevice.protocol.isEqual)
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK4_CAL1);
                                meTask = TASKS.REG_BLOCK4_CAL1;
                            }
                            else
                            {
                                addr++;
                                MyDevice.protocol.addr = addr;
                                actXET = MyDevice.actDev;

                                //校验未通过补发
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK1_DEV);
                                meTask = TASKS.REG_BLOCK1_DEV;
                            }
                            break;

                        case TASKS.REG_BLOCK4_CAL1:
                            if (MyDevice.protocol.isEqual)
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK5_CAL2);
                                meTask = TASKS.REG_BLOCK5_CAL2;
                            }
                            else
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK4_CAL1);
                                meTask = TASKS.REG_BLOCK4_CAL1;
                            }
                            break;

                        case TASKS.REG_BLOCK5_CAL2:
                            if (MyDevice.protocol.isEqual)
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK5_INFO);
                                meTask = TASKS.REG_BLOCK5_INFO;
                            }
                            else
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK5_CAL2);
                                meTask = TASKS.REG_BLOCK5_CAL2;
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
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_PARA);
                                meTask = TASKS.REG_BLOCK3_PARA;
                            }
                            else
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_WLAN);
                                meTask = TASKS.REG_BLOCK3_WLAN;
                            }
                            break;

                        case TASKS.REG_BLOCK3_PARA:
                            if (MyDevice.protocol.isEqual)
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK5_AM1);
                                meTask = TASKS.REG_BLOCK5_AM1;
                            }
                            else
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_PARA);
                                meTask = TASKS.REG_BLOCK3_PARA;
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
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK1_FIFO);
                                meTask = TASKS.REG_BLOCK1_FIFO;
                            }
                            else
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_OP);
                                meTask = TASKS.REG_BLOCK3_OP;
                            }
                            break;

                        case TASKS.REG_BLOCK1_FIFO:
                            if (MyDevice.protocol.isEqual)
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_SCREW1);
                                meTask = TASKS.REG_BLOCK3_SCREW1;
                            }
                            else
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK1_FIFO);
                                meTask = TASKS.REG_BLOCK1_FIFO;
                            }
                            break;

                        case TASKS.REG_BLOCK3_SCREW1:
                            if (MyDevice.protocol.isEqual)
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_SCREW2);
                                meTask = TASKS.REG_BLOCK3_SCREW2;
                            }
                            else
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_SCREW1);
                                meTask = TASKS.REG_BLOCK3_SCREW1;
                            }
                            break;

                        case TASKS.REG_BLOCK3_SCREW2:
                            if (MyDevice.protocol.isEqual)
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_SCREW3);
                                meTask = TASKS.REG_BLOCK3_SCREW3;
                            }
                            else
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_SCREW2);
                                meTask = TASKS.REG_BLOCK3_SCREW2;
                            }
                            break;

                        case TASKS.REG_BLOCK3_SCREW3:
                            if (MyDevice.protocol.isEqual)
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_SCREW4);
                                meTask = TASKS.REG_BLOCK3_SCREW4;
                            }
                            else
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_SCREW3);
                                meTask = TASKS.REG_BLOCK3_SCREW3;
                            }
                            break;

                        case TASKS.REG_BLOCK3_SCREW4:
                            if (MyDevice.protocol.isEqual)
                            {
                                actXET.sTATE = STATE.WORKING;
                                meTask = TASKS.NULL;
                            }
                            else
                            {
                                MyDevice.protocol.Protocol_Read_SendCOM(TASKS.REG_BLOCK3_SCREW4);
                                meTask = TASKS.REG_BLOCK3_SCREW4;
                            }
                            break;
                        default:
                            break;
                    }
                }
                //扫描结束
                else
                {
                    timer_RS485.Enabled = false;//关闭扫描

                    meTask = TASKS.NULL;
                    bt_scan.Text = "完 成";
                    bt_scan.BackColor = Color.Green;

                }
            }
            else
            {
                //连接单设备
                textBox2.Text += ".";
                MyDevice.protocol.Protocol_mePort_ReadAllTasks();
                meTask = MyDevice.protocol.trTASK;
            }
        }

        //获取本地的ip
        private void getIP()
        {
            //获取本地的IP地址
            string AddressIP = string.Empty;
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    AddressIP = _IPAddress.ToString();
                    if (!comboBox0_port.Items.Contains(AddressIP))
                    {
                        comboBox0_port.Items.Add(AddressIP);
                    }
                }
            }
            comboBox0_port.SelectedIndex = 0;
        }

        //打开串口/端口控件变化
        private void openPort()
        {
            bt_close.Text = "关 闭";
            bt_close.BackColor = Color.Green;
            bt_connect.Enabled = true;
            bt_scan.Enabled = true;
            btn_unbind.Enabled = true;
        }

        //关闭串口/端口控件变化
        private void closePort()
        {
            timer_USB.Enabled = false;
            timer_XF.Enabled = false;
            timer_TCP.Enabled = false;
            timer_RS485.Enabled = false;
            bt_close.Text = "打 开";
            bt_close.BackColor = Color.Red;
            bt_connect.Enabled = false;
            bt_connect.BackColor = Color.CadetBlue;
            bt_scan.Enabled = false;
            bt_scan.BackColor = Color.CadetBlue;
            btn_unbind.Enabled = false;
            btn_unbind.BackColor = Color.CadetBlue;
            bt_scan.Text = "扫 描";
            isTCPScan = false;
            isXFScan = false;
            isRS485Scan = false;
            isConnecting = false;
        }

        //接收器扫描
        private void XFScan()
        {
            //待扫描
            if (!timer_XF.Enabled)
            {
                if (comboBox0_port.Text != null)
                {
                    //切换自定义通讯
                    MyDevice.mePort_SetProtocol(COMP.XF);
                    MyDevice.ConnectType = "接收器连接";

                    //打开串口
                    comPort = comboBox0_port.Text;
                    MyDevice.protocol.Protocol_PortOpen(comPort, 115200, StopBits.One, Parity.None);

                    //串口有效
                    if (MyDevice.myXFUART.IsOpen)
                    {
                        //初始化设备连接状态
                        for (int i = 0; i < 256; i++)
                        {
                            MyDevice.mXF[i].sTATE = STATE.INVALID;
                        }

                        //扫描初始化
                        addr = 1;
                        MyDevice.protocol.addr = addr;
                        actXET = MyDevice.actDev;
                        meTask = TASKS.NULL;

                        if (MyDevice.protocol.IsOpen)
                        {
                            textBox2.Text = MyDevice.languageType == 0 ? "适配器已打开\r\n搜索中 ." : "Adapter turned on. \r\nIn the search .";
                            bt_scan.Text = "停止";
                            bt_scan.BackColor = Color.OrangeRed;
                            timer_XF.Enabled = true;
                            isXFScan = true;
                        }
                        else
                        {
                            textBox2.Text = MyDevice.languageType == 0 ? "适配器打开失败\r\n" : "Adapter opening failed. \r\n";
                            bt_scan.BackColor = Color.Firebrick;
                        }
                    }
                    else
                    {
                        MessageBox.Show("串口未打开，检查串口是否被占用", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
                else
                {
                    MessageBox.Show("串口不得为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            //扫描中
            else
            {
                timer_XF.Enabled = false;//关闭扫描
                isXFScan = false;

                meTask = TASKS.NULL;
                bt_scan.Text = "扫描";
                bt_scan.BackColor = Color.CadetBlue;
            }
        }

        //路由器扫描
        private void TCPScan()
        {
            //待扫描
            if (!timer_TCP.Enabled)
            {
                if (comboBox0_port.Text != null)
                {
                    //切换TCP通讯
                    MyDevice.mePort_SetProtocol(COMP.TCP);
                    MyDevice.ConnectType = "路由器WiFi连接";

                    //打开串口
                    ipPort = comboBox0_port.Text;
                    MyDevice.protocol.Protocol_PortOpen(ipPort, 5678, StopBits.One, Parity.None);

                    //串口有效
                    if (MyDevice.myTCPUART.IsOpen)
                    {
                        //初始化设备连接状态
                        for (int i = 0; i < 256; i++)
                        {
                            MyDevice.mTCP[i].sTATE = STATE.INVALID;
                        }

                        //扫描初始化
                        addr = 1;
                        connectID = 0;
                        MyDevice.protocol.addr = addr;
                        actXET = MyDevice.actDev;
                        meTask = TASKS.NULL;

                        if (MyDevice.protocol.IsOpen)
                        {
                            textBox2.Text = MyDevice.languageType == 0 ? "适配器已打开\r\n搜索中 ." : "Adapter turned on. \r\nIn the search .";
                            bt_scan.Text = "停止";
                            bt_scan.BackColor = Color.OrangeRed;
                            timer_TCP.Enabled = true;
                            isTCPScan = true;
                        }
                        else
                        {
                            textBox2.Text = MyDevice.languageType == 0 ? "适配器打开失败\r\n" : "Adapter opening failed. \r\n";
                            bt_scan.BackColor = Color.Firebrick;
                        }
                    }
                    else
                    {
                        MessageBox.Show("端口未打开，检查端口是否被占用", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
                else
                {
                    MessageBox.Show("端口不得为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            //扫描中
            else
            {
                timer_TCP.Enabled = false;//关闭扫描
                isTCPScan = false;

                meTask = TASKS.NULL;
                bt_scan.Text = "扫描";
                bt_scan.BackColor = Color.CadetBlue;
            }
        }

        //RS485扫描
        private void RS485Scan()
        {
            //待扫描
            if (!timer_RS485.Enabled)
            {
                if (comboBox0_port.Text != null)
                {
                    //根据波特率选择扫描间隔
                    switch (comboBox_baud.Text)
                    {
                        case "2400": timer_RS485.Interval = 800; break;
                        case "4800": timer_RS485.Interval = 400; break;
                        case "9600": timer_RS485.Interval = 300; break;
                        case "14400": timer_RS485.Interval = 200; break;
                        case "19200": timer_RS485.Interval = 200; break;
                        case "38400": timer_RS485.Interval = 200; break;
                        case "57600": timer_RS485.Interval = 200; break;
                        case "115200": timer_RS485.Interval = 100; break;
                        default: break;
                    }

                    //切换RS485通讯
                    MyDevice.mePort_SetProtocol(COMP.RS485);
                    MyDevice.ConnectType = "RS485连接";

                    //打开串口
                    comPort = comboBox0_port.Text;         
                    MyDevice.protocol.Protocol_PortOpen(comPort, 
                        Convert.ToInt32(comboBox_baud.Text),
                        (StopBits)(comboBox_stopbit.SelectedIndex + 1),
                        (Parity)comboBox_parity.SelectedIndex);

                    //串口有效
                    if (MyDevice.myRS485.IsOpen)
                    {
                        //初始化设备连接状态
                        for (int i = 0; i < 256; i++)
                        {
                            MyDevice.mTCP[i].sTATE = STATE.INVALID;
                        }

                        //扫描初始化
                        addr = 1;
                        MyDevice.protocol.addr = addr;
                        actXET = MyDevice.actDev;
                        meTask = TASKS.NULL;

                        if (MyDevice.protocol.IsOpen)
                        {
                            textBox2.Text = MyDevice.languageType == 0 ? "适配器已打开\r\n搜索中 ." : "Adapter turned on. \r\nIn the search .";
                            bt_scan.Text = "停止";
                            bt_scan.BackColor = Color.OrangeRed;
                            timer_RS485.Enabled = true;
                            isRS485Scan = true;
                        }
                        else
                        {
                            textBox2.Text = MyDevice.languageType == 0 ? "适配器打开失败\r\n" : "Adapter opening failed. \r\n";
                            bt_scan.BackColor = Color.Firebrick;
                        }
                    }
                    else
                    {
                        MessageBox.Show("串口未打开，检查串口是否被占用", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
                else
                {
                    MessageBox.Show("串口不得为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            //扫描中
            else
            {
                timer_RS485.Enabled = false;//关闭扫描
                isRS485Scan = false;

                meTask = TASKS.NULL;
                bt_scan.Text = "扫描";
                bt_scan.BackColor = Color.CadetBlue;
            }
        }

        //委托
        private void receiveData()
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
                    case TASKS.REG_R_BLUETOOTH_UNBIND:
                        textBox2.Clear();
                        if (MyDevice.IsUnbind)
                        {
                            MyDevice.protocol.Protocol_Write_SendCOM(TASKS.REG_W_BLUETOOTH_UNBIND);
                            meTask = MyDevice.protocol.trTASK;
                        }
                        else
                        {
                            textBox2.Text += "\r\n无法解绑，蓝牙被占用，请关闭占用的设备";
                            btn_unbind.BackColor = Color.Firebrick;
                        }
                        break;
                    case TASKS.REG_W_BLUETOOTH_UNBIND:
                        textBox2.Clear();
                        textBox2.Text += "\r\n解绑成功";
                        btn_unbind.BackColor = Color.Green;
                        break;
                    case TASKS.REG_BLOCK1_DEV:
                        textBox2.Text += "\r\n扳手型号读取成功";
                        isConnecting = true;//正在连接，禁止关闭页面
                        break;
                    case TASKS.REG_BLOCK4_CAL1:
                        textBox2.Text += "\r\n扳手CAL读取成功";
                        break;
                    case TASKS.REG_BLOCK5_CAL2:
                        textBox2.Text += "\r\n扳手CAL2读取成功";
                        break;
                    case TASKS.REG_BLOCK5_INFO:
                        textBox2.Text += "\r\n扳手INFO读取成功";
                        break;
                    case TASKS.REG_BLOCK3_WLAN:
                        textBox2.Text += "\r\n扳手WLAN读取成功";
                        break;
                    case TASKS.REG_BLOCK3_PARA:
                        textBox2.Text += "\r\n扳手PARA读取成功";
                        break;
                    case TASKS.REG_BLOCK5_AM1:
                        textBox2.Text += "\r\n扳手AM1读取成功";
                        break;
                    case TASKS.REG_BLOCK5_AM2:
                        textBox2.Text += "\r\n扳手AM2读取成功";
                        break;
                    case TASKS.REG_BLOCK5_AM3:
                        textBox2.Text += "\r\n扳手AM3读取成功";
                        break;
                    case TASKS.REG_BLOCK3_JOB:
                        textBox2.Text += "\r\n扳手JOB读取成功";
                        break;
                    case TASKS.REG_BLOCK3_OP:
                        textBox2.Text += "\r\n扳手OP读取成功";
                        break;
                    case TASKS.REG_BLOCK1_FIFO:
                        textBox2.Text += "\r\n扳手FIFO读取成功";
                        break;
                    case TASKS.REG_BLOCK2_DAT:
                        textBox2.Text += "\r\n扳手DAT读取成功";
                        break;
                    case TASKS.REG_BLOCK3_SCREW1:
                        textBox2.Text += "\r\n扳手SCREW1读取成功";
                        break;
                    case TASKS.REG_BLOCK3_SCREW2:
                        textBox2.Text += "\r\n扳手SCREW2读取成功";
                        break;
                    case TASKS.REG_BLOCK3_SCREW3:
                        textBox2.Text += "\r\n扳手SCREW3读取成功";
                        break;
                    case TASKS.REG_BLOCK3_SCREW4:
                        //成功连接，更新连接扳手信息(先更新数据库，防止多设备连接时当前设备地址成功后还未存入就递增扫描了，存入的是扫描地址)
                        updateDatabaseWrench();

                        //USB
                        if (MyDevice.protocol.type == COMP.UART)
                        {
                            textBox2.Text += "\r\n扳手SCREW4读取成功\r\n成功连接";
                            bt_connect.BackColor = Color.Green;
                            timer_USB.Enabled = false;
                            isConnecting = false;
                            meTask = TASKS.NULL;
                        }
                        //RS485
                        else if (MyDevice.protocol.type == COMP.RS485)
                        {
                            isConnecting = false;
                            meTask = TASKS.NULL;
                            textBox2.Text += "\r\n扳手SCREW4读取成功\r\n成功连接设备" + MyDevice.protocol.addr;
                            if (isRS485Scan)
                            {
                                //连接成功扫描下一个
                                addr++;
                                MyDevice.protocol.addr = addr;
                                actXET = MyDevice.actDev;
                            }
                            else
                            {
                                bt_connect.BackColor = Color.Green;
                                timer_RS485.Enabled = false;//RS485未扫描只单连接时关闭
                            }
                        }
                        //接收器
                        else if (MyDevice.protocol.type == COMP.XF)
                        {
                            isConnecting = false;
                            meTask = TASKS.NULL;
                            textBox2.Text += "\r\n扳手SCREW4读取成功\r\n成功连接设备" + MyDevice.protocol.addr;
                            if (isXFScan)
                            {
                                //连接成功扫描下一个
                                addr++;
                                MyDevice.protocol.addr = addr;
                                actXET = MyDevice.actDev;
                            }
                            else
                            {
                                bt_connect.BackColor = Color.Green;
                                timer_USB.Enabled = false;//接收器未扫描只单连接时关闭(单连接时走USB定时器，读取更快)
                            }
                        }
                        else
                        {
                            isConnecting = false;
                            meTask = TASKS.NULL;
                            textBox2.Text += "\r\n扳手SCREW4读取成功\r\n成功连接设备" + MyDevice.protocol.addr;

                            //设备地址绑定Ip
                            if (MyDevice.addr_ip.ContainsKey(addr.ToString()) == false)
                            {
                                MyDevice.addr_ip.Add(addr.ToString(), ((IPEndPoint)((Socket)MyDevice.protocol.port).RemoteEndPoint).Address.ToString());
                            }

                            if (connectID < MyDevice.clientConnectionItems.Count - 1)
                            {
                                //读取之后需要从头开始扫描（addr = 1）
                                //从头开始的原因：由于一开始给设备分配IP地址时，先后顺序是随机的，不一定就是根据扳手站点排序
                                //示例：设备1和3均被分配Ip，可能一开始读取的ip是设备3的，此时只有设备3能响应，不从头开始的话设备1跳过了
                                connectID++;
                                addr = 1;
                                MyDevice.protocol.addr = addr;
                                actXET = MyDevice.actDev;
                            }

                            if (!isTCPScan)
                            {
                                bt_connect.BackColor = Color.Green;
                                timer_TCP.Enabled = false;//路由器未扫描只单连接时关闭
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        //更新数据库
        private void updateDatabaseWrench()
        {
            //判断当前电脑是否启动数据库服务
            if (!GetComPuterInfo.ServiceIsRunning("MySQL", 21))
            {
                //MessageBox.Show("Mysql数据库未安装，部分功能无法使用");
            }
            else
            {
                if (actXET.devc.bohrcode == 0)
                {
                    MessageBox.Show("读取错误");
                    return;
                }
                //将扳手信息存入数据库
                DSWrenchDevc wrenchDevc = new DSWrenchDevc
                {
                    Series = actXET.devc.series.ToString(),
                    Type = actXET.devc.type.ToString(),
                    Version = actXET.devc.version,
                    BohrCode = actXET.devc.bohrcode,
                    Unit = actXET.devc.calunit.ToString(),
                    TorqueDecimal = actXET.devc.torque_decimal,
                    TorqueFdn = actXET.devc.torque_fdn,
                    CalType = actXET.devc.caltype,
                    Capacity = actXET.devc.capacity,
                    AdZero = actXET.devc.ad_zero,
                    AdPosPoint1 = actXET.devc.ad_pos_point1,
                    AdPosPoint2 = actXET.devc.ad_pos_point2,
                    AdPosPoint3 = actXET.devc.ad_pos_point3,
                    AdPosPoint4 = actXET.devc.ad_pos_point4,
                    AdPosPoint5 = actXET.devc.ad_pos_point5,
                    AdNegPoint1 = actXET.devc.ad_neg_point1,
                    AdNegPoint2 = actXET.devc.ad_neg_point2,
                    AdNegPoint3 = actXET.devc.ad_neg_point3,
                    AdNegPoint4 = actXET.devc.ad_neg_point4,
                    AdNegPoint5 = actXET.devc.ad_neg_point5,
                    TqPosPoint1 = actXET.devc.tq_pos_point1,
                    TqPosPoint2 = actXET.devc.tq_pos_point2,
                    TqPosPoint3 = actXET.devc.tq_pos_point3,
                    TqPosPoint4 = actXET.devc.tq_pos_point4,
                    TqPosPoint5 = actXET.devc.tq_pos_point5,
                    TqNegPoint1 = actXET.devc.tq_neg_point1,
                    TqNegPoint2 = actXET.devc.tq_neg_point2,
                    TqNegPoint3 = actXET.devc.tq_neg_point3,
                    TqNegPoint4 = actXET.devc.tq_neg_point4,
                    TqNegPoint5 = actXET.devc.tq_neg_point5,
                    TorqueDisp = actXET.devc.torque_disp,
                    TorqueMin = actXET.devc.torque_min,
                    TorqueMax = actXET.devc.torque_max,
                    TorqueOver = actXET.devc.torque_over[(int)actXET.para.torque_unit],
                    TorqueErr = actXET.devc.torque_err[(int)actXET.para.torque_unit].ToString(),
                    ConnectType = MyDevice.ConnectType,
                };
                DSWrenchPara wrenchPara = new DSWrenchPara
                {
                    TorqueUnit = actXET.para.torque_unit.ToString(),
                    AngleSpeed = actXET.para.angle_speed,
                    AngleDecimal = actXET.para.angle_decimal,
                    ModePt = actXET.para.mode_pt,
                    ModeAx = actXET.para.mode_ax,
                    ModeMx = actXET.para.mode_mx,
                    FifoMode = actXET.para.fifomode,
                    FifoRec = actXET.para.fiforec,
                    FifoSpeed = actXET.para.fifospeed,
                    HeartCount = actXET.para.heartcount,
                    HeartCycle = actXET.para.heartcycle,
                    AccMode = actXET.para.accmode,
                    AlarmMode = actXET.para.alarmode,
                    WifiMode = actXET.wlan.wifimode,
                    TimeOff = actXET.para.timeoff,
                    TimeBack = actXET.para.timeback,
                    TimeZero = actXET.para.timezero,
                    DispType = actXET.para.disptype,
                    DispTheme = actXET.para.disptheme,
                    DispLan = actXET.para.displan,
                    Unhook = actXET.para.unhook,
                    AngCorr = actXET.para.angcorr.ToString(),
                    AdSpeed = actXET.para.adspeed,
                    AutoZero = actXET.para.autozero.ToString(),
                    TrackZero = actXET.para.trackzero.ToString(),
                };
                DSWrenchWork wrenchWork = new DSWrenchWork
                {
                    SrNo = actXET.work.srno,
                    Number = actXET.work.number,
                    MfgTime = actXET.work.mfgtime,
                    CalTime = actXET.work.caltime,
                    CalRemind = actXET.work.calremind,
                    Name = actXET.work.name,
                    ManageTxt = actXET.work.managetxt,
                    Decription = actXET.work.decription,
                    WoArea = actXET.work.wo_area,
                    WoFactory = actXET.work.wo_factory,
                    WoLine = actXET.work.wo_line,
                    WoStation = actXET.work.wo_station,
                    WoBat = actXET.work.wo_bat,
                    WoNum = actXET.work.wo_num,
                    WoStamp = actXET.work.wo_stamp,
                    WoName = actXET.work.wo_name,
                    UserId = actXET.work.user_ID,
                    UserName = actXET.work.user_name,
                    Screworder = actXET.work.screworder[actXET.para.mode_mx].ToString(),
                };
                DSWrenchWlan wrenchWlan = new DSWrenchWlan
                {
                    Addr = actXET.wlan.addr,
                    RfChan = actXET.wlan.rf_chan,
                    RfOption = actXET.wlan.rf_option,
                    RfPara = actXET.wlan.rf_para,
                    Baud = actXET.wlan.rs485_baud,
                    Stopbit = actXET.wlan.rs485_stopbit,
                    Parity = actXET.wlan.rs485_parity,
                    WFSsid = actXET.wlan.wf_ssid,
                    WFPwd = actXET.wlan.wf_pwd,
                    WFIp = actXET.wlan.wf_ip,
                    WFPort = (ushort)actXET.wlan.wf_port,
                };
                DSWrenchAlam wrenchAlam = new DSWrenchAlam
                {
                    EnTarget = actXET.alam.EN_target[(int)actXET.para.torque_unit].ToString(),
                    EnPre = actXET.alam.EA_pre[(int)actXET.para.torque_unit].ToString(),
                    EaAng = actXET.alam.EA_ang,
                    SnTarget = actXET.alam.SN_target[actXET.para.mode_mx, (int)actXET.para.torque_unit].ToString(),
                    SaPre = actXET.alam.SA_pre[actXET.para.mode_mx, (int)actXET.para.torque_unit].ToString(),
                    SaAng = actXET.alam.SA_ang[actXET.para.mode_mx].ToString(),
                    MnLow = actXET.alam.MN_low[actXET.para.mode_mx, (int)actXET.para.torque_unit].ToString(),
                    MnHigh = actXET.alam.MN_high[actXET.para.mode_mx, (int)actXET.para.torque_unit].ToString(),
                    MaPre = actXET.alam.MA_pre[actXET.para.mode_mx, (int)actXET.para.torque_unit].ToString(),
                    MaLow = actXET.alam.MA_low[actXET.para.mode_mx].ToString(),
                    MaHigh = actXET.alam.MA_high[actXET.para.mode_mx].ToString(),
                };

                //依据bohrcode查找wrench表中是否有该设备
                if (JDBC.GetWidByBohrcode(actXET.devc.bohrcode) is uint wid && wid != 0)
                {
                    //数据库中已有，则更新
                    wrenchDevc.ConnectAuto = JDBC.GetWrenchDevcByWid(wid).ConnectAuto;//自动连接扳手中无法读取，需要手动更新
                    JDBC.UpdateWrench(wid, wrenchDevc, wrenchPara, wrenchWork, wrenchWlan, wrenchAlam);
                }
                else
                {
                    //没有加入
                    JDBC.AddWrench(wrenchDevc, wrenchPara, wrenchWork, wrenchWlan, wrenchAlam);
                }
            }
        }

    }
}
