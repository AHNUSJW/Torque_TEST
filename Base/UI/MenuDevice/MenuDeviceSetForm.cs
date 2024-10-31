using DBHelper;
using HZH_Controls.Controls;
using Library;
using Model;
using RecXF;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

//Ricardo 20240227
//Lumi 20240507
//Ricardo 2024018

namespace Base.UI.MenuDevice
{
    public partial class MenuDeviceSetForm : Form
    {
        private readonly float x; //定义当前窗体的宽度
        private readonly float y; //定义当前窗体的高度

        private XET actXET;       //当前设备
        private TASKS meTask;     //按键操作指令

        private int unit;         //设备扭矩单位(ui控件操控的单位)
        private int xetUnit;      //指令发送更改设备的单位
        private List<Byte> mutiAddres = new List<Byte>();         //存储已连接设备的地址

        private DataGridViewTextBoxEditingControl CellEdit = null;//单元格

        private string buttonClicked = "";    //记录按下的按钮,区分按下的是预设值还是模式设置
        private int selectNum = 0;            //轮询发送指令的扳手下标
        private byte oldDevAddr = 1;          //改站点之前的旧站点
        private const int ticketCntMax = 32;  //离线工单最大值
        private bool isInputValid = true;     //离线工单相关输入配置是否合格

        public class GridModel
        {
            public string Device { get; set; }
        }

        public MenuDeviceSetForm()
        {
            InitializeComponent();
            x = this.ClientRectangle.Width;
            y = this.ClientRectangle.Height;
            setTag(this);
        }

        #region 页面加载关闭

        //打开窗口
        private void MenuDeviceSetForm_Load(object sender, EventArgs e)
        {
            actXET = MyDevice.actDev;
            actXET.torqueMultiple = (int)Math.Pow(10, actXET.devc.torque_decimal);
            actXET.angleMultiple = (int)Math.Pow(10, actXET.para.angle_decimal);

            #region 窗口初始化

            //根据分辨率初始化窗口大小 (w:1232 h:837)，以（1920 * 1080)作为标准窗口大小
            double initWidth = this.Width;
            double initHeight = this.Height;
            double proportion = initHeight / initWidth;

            this.Width = Convert.ToInt32(initWidth * (Screen.PrimaryScreen.Bounds.Width * 1.0 / 1920));
            this.Height = Convert.ToInt32(this.Width * 1.0 * proportion);

            //窗口在屏幕中居中显示
            this.Location = new Point(Convert.ToInt32(Screen.PrimaryScreen.Bounds.Width / 2 - this.Width / 2),
                                      Convert.ToInt32(Screen.PrimaryScreen.Bounds.Height / 2 - this.Height / 2));

            #endregion

            #region 设备选择框

            this.ucDataGridView1.Width = splitContainer1.Panel1.Width;
            this.ucDataGridView1.Height = splitContainer1.Panel1.Height - label1.Height * 2;
            this.ucDataGridView1.Dock = DockStyle.Bottom;

            #endregion

            #region 表格初始化

            //预设值设置表格初始化
            DataGridView1_Init();

            //离线工单表格初始化
            DataGridView2_Init();

            #endregion

            //参数combox和textbox初始化
            Parameters_Init();

            //权限控制
            RoleBasedUIChange();

            //依据扳手型号调整控件状态
            TypeBasedUIChange();

            #region 接收器和路由器配置提示

            //接收器配置信息
            var test = MyRecXFSettings.ReadDevConfig();
            groupBox12.Location = new Point(label_wifiIp.Location.X, groupBox8.Location.Y + groupBox8.Height + 10);
            groupBox11.Location = new Point(label_wifiIp.Location.X, groupBox12.Location.Y + groupBox12.Height + 10);
            label_recIP.Text += $" 192.168.{test.IpWiFi}.1";
            label_recPort.Text += " " + test.PortWiFi;
            label_recWIFIName.Text += " " + test.SsidWiFi;
            label_recWIFIPwd.Text += " " + test.PswdWiFi;

            //本地ip与wifi名称
            //wifi名称加载时间较长，故使用异步
            //await避免阻塞UI主线程, 使该控件在页面加载后再更新
            Task.Run(async () =>
            {
                string ipAddress = await Task.Run(() => WifiInfo.GetIP());
                string wifiSsid = await Task.Run(() => WifiInfo.GetAccurateWIFISsid());

                if (wifiSsid == null || wifiSsid == "")
                {
                    wifiSsid = "无网络，请检查网络配置";
                }

                //IsHandleCreated判断更新控件是否被分配了语柄
                //创建窗口句柄之前,不能在控件上调用 Invoke 或 BeginInvoke,否则报错
                while (!label_curIP.IsHandleCreated) ;

                if (label_curIP.InvokeRequired)
                {
                    label_curIP.BeginInvoke(new MethodInvoker(() => label_curIP.Text += " " + ipAddress));
                }
                else
                {
                    label_curIP.Text += " " + ipAddress;
                }

                while (!label_curWIFIName.IsHandleCreated) ;

                if (label_curWIFIName.InvokeRequired)
                {
                    label_curWIFIName.BeginInvoke(new MethodInvoker(() => label_curWIFIName.Text += " " + wifiSsid));
                }
                else
                {
                    label_curWIFIName.Text += " " + wifiSsid;
                }

                //配置选择
                Action action = () =>
                {
                    //ucCombox_wirelessSelection.SelectedIndex = 0;
                    if (ucTextBoxEx_wifiIp.InputText == label_recIP.Text.Split(' ')[1] &&
                        ucTextBoxEx_port.InputText == label_recPort.Text.Split(' ')[1] &&
                        ucTextBoxEx_ssid.InputText == label_recWIFIName.Text.Split(' ')[1] &&
                        ucTextBoxEx_pwd.InputText == label_recWIFIPwd.Text.Split(' ')[1]
                        )
                    {
                        ucCombox_wirelessSelection.SelectedIndex = 0;
                    }
                    else if (ucTextBoxEx_wifiIp.InputText == label_curIP.Text.Split(' ')[1] &&
                             ucTextBoxEx_port.InputText == "5678" &&
                             ucTextBoxEx_ssid.InputText == label_curWIFIName.Text.Split(' ')[1]
                    )
                    {
                        ucCombox_wirelessSelection.SelectedIndex = 1;
                    }
                    else
                    {
                        ucCombox_wirelessSelection.SelectedIndex = -1;
                    }
                };

                // 异步调用
                if (this.InvokeRequired)
                {
                    this.Invoke(action);
                }
                else
                {
                    action();
                }
            });


            #endregion

            //界面更新事件
            MyDevice.myTaskManager.UpdateUI += updateUI;

            //切换扳手权限
            if (MyDevice.protocol.type == COMP.UART)
            {
                MyDevice.myTaskManager.AddUserCommand(MyDevice.protocol.addr, ProtocolFunc.Protocol_Write_SendCOM, TASKS.WRITE_MEMABLE, Convert.ToByte(MyDevice.userRole), this.Name);
            }
            else
            {
                selectNum = 0;
                buttonClicked = "bt_UpdateMemable";
                //已连接的多设备均设置权限
                for (byte i = 1; i != 0; i++)
                {
                    if (MyDevice.protocol.type == COMP.XF)
                    {
                        if (MyDevice.mXF[i].sTATE == STATE.WORKING)
                        {
                            //切换扳手权限
                            MyDevice.myTaskManager.AddUserCommand(i, ProtocolFunc.Protocol_Write_SendCOM, TASKS.WRITE_MEMABLE, Convert.ToByte(MyDevice.userRole), this.Name);
                        }
                    }
                    else if (MyDevice.protocol.type == COMP.TCP)
                    {
                        if (MyDevice.mTCP[i].sTATE == STATE.WORKING)
                        {
                            MyDevice.protocol.addr = i;//
                            if (MyDevice.protocol.type == COMP.TCP)
                            {
                                MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
                            }
                            actXET = MyDevice.actDev;
                            //切换扳手权限
                            MyDevice.myTaskManager.AddUserCommand(i, ProtocolFunc.Protocol_Write_SendCOM, TASKS.WRITE_MEMABLE, Convert.ToByte(MyDevice.userRole), this.Name);
                            return;
                        }
                    }
                }
            }

        }

        //初始化参数
        private void Parameters_Init()
        {
            #region 预设值设置

            //单位
            List<KeyValuePair<string, string>> Torque_unitMode = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("0", "N·m"),
                new KeyValuePair<string, string>("1", "lbf·in"),
                new KeyValuePair<string, string>("2", "lbf·ft"),
                new KeyValuePair<string, string>("3", "kgf·cm")
            };
            if ((TYPE)(actXET.devc.type + 1280) == TYPE.TQ_XH_XL01_07 || (TYPE)(actXET.devc.type + 1280) == TYPE.TQ_XH_XL01_09)
            {
                Torque_unitMode.Add(new KeyValuePair<string, string>("4", "kgf·m"));
            }

            ucCombox_torqueUnit.Source = Torque_unitMode;
            ucCombox_torqueUnit.SelectedIndex = (byte)actXET.para.torque_unit;

            //角度小数点
            List<KeyValuePair<string, string>> Angle_decimalMode = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("0", " 无小数位 "),
                new KeyValuePair<string, string>("1", " 保留一位小数 "),
                new KeyValuePair<string, string>("2", " 保留两位小数 "),
                new KeyValuePair<string, string>("3", " 保留三位小数 ")
            };

            ucCombox_point.Source = Angle_decimalMode;
            ucCombox_point.SelectedIndex = actXET.devc.type == TYPE.TQ_XH_XL01_06 - (UInt16)ADDROFFSET.TQ_XH_ADDR ? 1 : actXET.para.angle_decimal;

            //Pt模式
            ucCombox_modePt.Source = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("0", "跟踪模式T"),
                new KeyValuePair<string, string>("1", "峰值模式P")
            };
            ucCombox_modePt.SelectedIndex = actXET.para.mode_pt;

            //Ax模式
            List<KeyValuePair<string, string>> AxMode = new List<KeyValuePair<string, string>>();
            switch (actXET.devc.type)
            {
                default:
                case TYPE.TQ_XH_XL01_06 - (UInt16)ADDROFFSET.TQ_XH_ADDR:
                case TYPE.TQ_XH_XL01_07:
                case TYPE.TQ_XH_XL01_07 - 1280:
                case TYPE.TQ_XH_XL01_09 - 1280:
                    AxMode.Add(new KeyValuePair<string, string>("0", " EN "));
                    AxMode.Add(new KeyValuePair<string, string>("1", " EA "));
                    AxMode.Add(new KeyValuePair<string, string>("2", " SN "));
                    AxMode.Add(new KeyValuePair<string, string>("3", " SA "));
                    AxMode.Add(new KeyValuePair<string, string>("4", " MN "));
                    AxMode.Add(new KeyValuePair<string, string>("5", " MA "));
                    break;
            }

            ucCombox_modeAx.Source = AxMode;
            ucCombox_modeAx.SelectedIndex = actXET.para.mode_ax;

            //Mx模式
            ucCombox_modeMx.Source = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("0", " M0 "),
                new KeyValuePair<string, string>("1", " M1 "),
                new KeyValuePair<string, string>("2", " M2 "),
                new KeyValuePair<string, string>("3", " M3 "),
                new KeyValuePair<string, string>("4", " M4 "),
                new KeyValuePair<string, string>("5", " M5 "),
                new KeyValuePair<string, string>("6", " M6 "),
                new KeyValuePair<string, string>("7", " M7 "),
                new KeyValuePair<string, string>("8", " M8 "),
                new KeyValuePair<string, string>("9", " M9 ")
            };
            ucCombox_modeMx.SelectedIndex = actXET.para.mode_mx;

            //更改选择
            ucCombox_select.Source = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("0", " 不允许手动更改 "),
                new KeyValuePair<string, string>("1", " 允许手动更改"),
            };
            ucCombox_select.SelectedIndex = 0;

            #endregion

            #region 模式设置

            //覆盖模式
            ucCombox_fifomode.Source = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("0", " 不覆盖 "),
                new KeyValuePair<string, string>("1", " 覆盖 ")
            };
            ucCombox_fifomode.SelectedIndex = actXET.para.fifomode;

            //缓存模式
            ucCombox_fiforec.Source = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("0", "不缓存"),
                new KeyValuePair<string, string>("1", "追随缓存RECt"),
                new KeyValuePair<string, string>("2", "峰值缓存RECp"),
            };
            ucCombox_fiforec.SelectedIndex = actXET.para.fiforec;

            //缓存速率
            ucCombox_fifospeed.Source = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("0", "10Hz"),
                new KeyValuePair<string, string>("1", "20Hz"),
                new KeyValuePair<string, string>("2", "40Hz"),
                new KeyValuePair<string, string>("3", "50Hz"),
                new KeyValuePair<string, string>("4", "100Hz"),
                new KeyValuePair<string, string>("5", "125Hz"),
                new KeyValuePair<string, string>("6", "200Hz"),
            };
            ucCombox_fifospeed.SelectedIndex = actXET.para.fifospeed;

            //持续回复
            ucCombox_heart.Source = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("0", "持续"),
                new KeyValuePair<string, string>("1", "不持续"),
            };
            ucCombox_heart.SelectedIndex = actXET.para.heartcount < 0 ? 0 : 1;

            //心跳回复帧数
            ucTextBoxEx_heartcount.InputText = actXET.para.heartcount < 0 ? "0" : actXET.para.heartcount.ToString();
            if (ucCombox_heart.SelectedIndex > 0)
            {
                ucTextBoxEx_heartcount.Enabled = true;
            }
            else
            {
                ucTextBoxEx_heartcount.Enabled = false;
            }

            //心跳间隔
            ucTextBoxEx_heartcycle.InputText = actXET.para.heartcycle < 1 ? "0" : actXET.para.heartcycle.ToString();

            //角度累加
            ucCombox_accmode.Source = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("0", "不累加"),
                new KeyValuePair<string, string>("1", "棘轮扳手累加"),
                new KeyValuePair<string, string>("2", "开口扳手累加")
            };
            ucCombox_accmode.SelectedIndex = actXET.para.accmode;

            //声光报警省电
            ucCombox_alarmode.Source = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("0", "关闭"),
                new KeyValuePair<string, string>("1", "开启")
            };
            ucCombox_alarmode.SelectedIndex = actXET.para.alarmode;

            //自动关机时间
            ucTextBoxEx_timeoff.InputText = actXET.para.timeoff <= 0 ? "1" : actXET.para.timeoff.ToString();

            //自动背光时间
            ucTextBoxEx_timeback.InputText = actXET.para.timeback <= 0 ? "1" : actXET.para.timeback.ToString();

            //自动归零时间
            ucTextBoxEx_timezero.InputText = actXET.para.timezero <= 0 ? "1" : actXET.para.timezero.ToString();

            //屏幕方向
            ucCombox_disptype.Source = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("0", "正竖屏"),
                new KeyValuePair<string, string>("1", "反竖屏"),
                new KeyValuePair<string, string>("2", "正横屏"),
                new KeyValuePair<string, string>("3", "反横屏")
            };
            ucCombox_disptype.SelectedIndex = actXET.para.disptype;

            //主题
            ucCombox_disptheme.Source = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("0", "主题1"),
                new KeyValuePair<string, string>("1", "主题2"),
                new KeyValuePair<string, string>("2", "主题3"),
                new KeyValuePair<string, string>("3", "主题4")
            };
            ucCombox_disptheme.SelectedIndex = actXET.para.disptheme;

            //语言
            ucCombox_displan.Source = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("0", "中文"),
                new KeyValuePair<string, string>("1", "英文")
            };
            ucCombox_displan.SelectedIndex = actXET.para.displan;

            //扭矩修正系数
            ucTextBoxEx_torcorr.InputText = actXET.para.torcorr.ToString();

            //设置重复拧紧角度格式
            switch (actXET.para.angle_decimal)
            {
                case 0:
                    ucTextBoxEx_angleResist.RegexPattern = @"^\d{0,7}$";
                    break;
                case 1:
                    ucTextBoxEx_angleResist.RegexPattern = @"^\d{0,7}(\.\d{0,1})?$";
                    break;
                case 2:
                    ucTextBoxEx_angleResist.RegexPattern = @"^\d{0,7}(\.\d{0,2})?$";
                    break;
                case 3:
                    ucTextBoxEx_angleResist.RegexPattern = @"^\d{0,7}(\.\d{0,3})?$";
                    break;
                default:
                    break;
            }
            ucTextBoxEx_angleResist.InputText = (actXET.para.angle_resist * 1.0 / (int)Math.Pow(10, actXET.para.angle_decimal)).ToString();

            //校准时间
            if (actXET.work.caltime != 0)
            {
                dateTimePicker_caltime.Value = MyDevice.UInt32ToDateTime(actXET.work.caltime);
            }

            //复校时间
            if (actXET.work.calremind != 0)
            {
                dateTimePicker_calremind.Value = MyDevice.UInt32ToDateTime(actXET.work.calremind);
            }

            #endregion

            #region WLAN设置

            //站点地址
            ucTextBoxEx_addr.InputText = actXET.wlan.addr.ToString();

            //通信信道
            ucCombox_RFchan.Source = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("0x00", " 431"),
                new KeyValuePair<string, string>("0x01", " 431.5"),
                new KeyValuePair<string, string>("0x02", " 432"),
                new KeyValuePair<string, string>("0x03", " 432.5"),
                new KeyValuePair<string, string>("0x04", " 433"),
                new KeyValuePair<string, string>("0x05", " 433.5"),
                new KeyValuePair<string, string>("0x06", " 434"),
                new KeyValuePair<string, string>("0x07", " 434.5"),
                new KeyValuePair<string, string>("0x08", " 435"),
                new KeyValuePair<string, string>("0x09", " 435.5"),
                new KeyValuePair<string, string>("0x0A", " 436"),
                new KeyValuePair<string, string>("0x0B", " 436.5"),
                new KeyValuePair<string, string>("0x0C", " 437"),
                new KeyValuePair<string, string>("0x0D", " 437.5"),
                new KeyValuePair<string, string>("0x0E", " 438"),
                new KeyValuePair<string, string>("0x0F", " 438.5"),
                new KeyValuePair<string, string>("0x10", " 439"),
                new KeyValuePair<string, string>("0x11", " 439.5"),
                new KeyValuePair<string, string>("0x12", " 440"),
                new KeyValuePair<string, string>("0x13", " 440.5"),
                new KeyValuePair<string, string>("0x14", " 441"),
                new KeyValuePair<string, string>("0x15", " 441.5"),
                new KeyValuePair<string, string>("0x16", " 442"),
                new KeyValuePair<string, string>("0x17", " 442.5"),
                new KeyValuePair<string, string>("0x18", " 443"),
                new KeyValuePair<string, string>("0x19", " 443.5"),
                new KeyValuePair<string, string>("0x1A", " 444"),
                new KeyValuePair<string, string>("0x1B", " 444.5"),
                new KeyValuePair<string, string>("0x1C", " 445"),
                new KeyValuePair<string, string>("0x1D", " 445.5"),
                new KeyValuePair<string, string>("0x1E", " 446"),
                new KeyValuePair<string, string>("0x1F", " 446.5"),
            };
            ucCombox_RFchan.SelectedIndex = actXET.wlan.rf_chan;

            //WiFi IP
            string ip = null;
            for (int i = 0; i < actXET.wlan.wf_ip.Length; i += 2)
            {
                int num = Convert.ToInt32($"{actXET.wlan.wf_ip[i]}{actXET.wlan.wf_ip[i + 1]}", 16);
                ip = i == 0 ? num.ToString() : ip + "." + num.ToString();
            }
            ucTextBoxEx_wifiIp.InputText = ip;

            //WiFi 端口
            ucTextBoxEx_port.InputText = actXET.wlan.wf_port.ToString();

            //WiFi名称
            ucTextBoxEx_ssid.InputText = actXET.wlan.wf_ssid;

            //WiFi密码
            ucTextBoxEx_pwd.InputText = actXET.wlan.wf_pwd;

            //WiFi/RF无线
            ucCombox_wifimode.Source = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("0", "关闭"),
                new KeyValuePair<string, string>("1", "开启")
            };
            ucCombox_wifimode.SelectedIndex = actXET.wlan.wifimode;

            //配置选择
            ucCombox_wirelessSelection.Source = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("0", "接收器配置"),
                new KeyValuePair<string, string>("1", "路由器配置")
            };
            
            #endregion

            #region 工厂设置

            //标定零点
            tb_adZero.InputText = actXET.devc.ad_zero.ToString();

            //标定内码 (包含正向与反向，各5组)
            tb_adPos1.InputText = actXET.devc.ad_pos_point1.ToString();
            tb_adPos2.InputText = actXET.devc.ad_pos_point2.ToString();
            tb_adPos3.InputText = actXET.devc.ad_pos_point3.ToString();
            tb_adPos4.InputText = actXET.devc.ad_pos_point4.ToString();
            tb_adPos5.InputText = actXET.devc.ad_pos_point5.ToString();

            tb_adNeg1.InputText = actXET.devc.ad_neg_point1.ToString();
            tb_adNeg2.InputText = actXET.devc.ad_neg_point2.ToString();
            tb_adNeg3.InputText = actXET.devc.ad_neg_point3.ToString();
            tb_adNeg4.InputText = actXET.devc.ad_neg_point4.ToString();
            tb_adNeg5.InputText = actXET.devc.ad_neg_point5.ToString();

            //标定单位
            ucCombox_calUnit.Source = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("0", "N·m"),
                new KeyValuePair<string, string>("1", "lbf·in"),
                new KeyValuePair<string, string>("2", "lbf·ft"),
                new KeyValuePair<string, string>("3", "kgf·cm")
            };
            if ((TYPE)(actXET.devc.type + 1280) == TYPE.TQ_XH_XL01_07 || (TYPE)(actXET.devc.type + 1280) == TYPE.TQ_XH_XL01_09)
            {
                ucCombox_calUnit.Source.Add(new KeyValuePair<string, string>("4", "kgf·m"));
            }
            ucCombox_calUnit.SelectedIndex = (byte)actXET.devc.calunit;

            //标定方式
            ucCombox_calType.Source = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("0", "五点标定"),
                new KeyValuePair<string, string>("1", "七点标定"),
                new KeyValuePair<string, string>("2", "九点标定"),
                new KeyValuePair<string, string>("3", "十一点标定")
            };
            ucCombox_calType.SelectedIndex = actXET.devc.caltype;

            //标定量程
            ucCombox_capacity.Source = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("0",  "6"),
                new KeyValuePair<string, string>("1",  "12"),
                new KeyValuePair<string, string>("2",  "20"),
                new KeyValuePair<string, string>("3",  "30"),
                new KeyValuePair<string, string>("4",  "50"),
                new KeyValuePair<string, string>("5",  "85"),
                new KeyValuePair<string, string>("6",  "100"),
                new KeyValuePair<string, string>("7",  "200"),
                new KeyValuePair<string, string>("8",  "300"),
                new KeyValuePair<string, string>("9",  "400"),
                new KeyValuePair<string, string>("10", "650"),
                new KeyValuePair<string, string>("11", "800"),
                new KeyValuePair<string, string>("12", "1000"),
                new KeyValuePair<string, string>("13", "1300"),
                new KeyValuePair<string, string>("14", "1500"),
                new KeyValuePair<string, string>("15", "1800"),
                new KeyValuePair<string, string>("16", "2000"),
                new KeyValuePair<string, string>("17", "2600"),
                new KeyValuePair<string, string>("18", "3000"),
            };
            for (int i = 0; i < ucCombox_capacity.Source.Count; i++)
            {
                if (ucCombox_capacity.Source[i].Value == (actXET.devc.capacity / (int)Math.Pow(10, actXET.devc.torque_decimal)).ToString())
                {
                    ucCombox_capacity.SelectedIndex = i;
                    break;
                }
            }

            //脱钩保持时间
            ucTextBoxEx_unhook.InputText = actXET.para.unhook.ToString();

            //USB通信使能
            ucCombox_usbEN.Source = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("0", "关闭"),
                new KeyValuePair<string, string>("1", "开启")
            };
            ucCombox_usbEN.SelectedIndex = actXET.para.usbEn == 0x55 ? 0 : 1;

            //无线通信使能
            ucCombox_wirelessEn.Source = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("0", "关闭"),
                new KeyValuePair<string, string>("1", "开启")
            };
            ucCombox_wirelessEn.SelectedIndex = actXET.para.wirelessEn == 0x55 ? 0 : 1;

            //扭矩采样速度
            ucCombox_adspeed.Source = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("0", "10Hz"),//0x08
                new KeyValuePair<string, string>("1", "40Hz"),//0x18
                new KeyValuePair<string, string>("2", "640Hz"),//0x28
                new KeyValuePair<string, string>("3", "1280Hz")//0x38
            };
            //ucCombox_adspeed.SelectedIndex = actXET.para.adspeed;

            //归零范围
            ucCombox_autozero.Source = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("0", "不归零"),//0x00
                new KeyValuePair<string, string>("1", "归零范围2%"),//0x02
                new KeyValuePair<string, string>("2", "归零范围4%"),//0x04
                new KeyValuePair<string, string>("3", "归零范围10%"),//0x0A
                new KeyValuePair<string, string>("4", "归零范围20%"),//0x14
                new KeyValuePair<string, string>("5", "归零范围50%")//0x32
            };
            //ucCombox_autozero.SelectedIndex = (int)actXET.para.autozero;

            //零点跟踪
            ucCombox_trackzero.Source = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("0", "无零点跟踪"),//0x00
                new KeyValuePair<string, string>("1", "零点跟踪10%"),//0x01
                new KeyValuePair<string, string>("2", "零点跟踪20%"),//0x02
                new KeyValuePair<string, string>("3", "零点跟踪30%"),//0x03
                new KeyValuePair<string, string>("4", "零点跟踪40%"),//0x04
                new KeyValuePair<string, string>("5", "零点跟踪50%")//0x05
            };
            //ucCombox_trackzero.SelectedIndex = (int)actXET.para.trackzero;

            #endregion

            #region 工单设置

            //离线工单数量
            ucCombox_screwMax.Source = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("0",  "1"),
                new KeyValuePair<string, string>("1",  "2"),
                new KeyValuePair<string, string>("2",  "3"),
                new KeyValuePair<string, string>("3",  "4"),
                new KeyValuePair<string, string>("4",  "5"),
                new KeyValuePair<string, string>("5",  "6"),
                new KeyValuePair<string, string>("6",  "7"),
                new KeyValuePair<string, string>("7",  "8"),
                new KeyValuePair<string, string>("8",  "9"),
                new KeyValuePair<string, string>("9",  "10"),
                new KeyValuePair<string, string>("10", "11"),
                new KeyValuePair<string, string>("11", "12"),
                new KeyValuePair<string, string>("12", "13"),
                new KeyValuePair<string, string>("13", "14"),
                new KeyValuePair<string, string>("14", "15"),
                new KeyValuePair<string, string>("15", "16"),
                new KeyValuePair<string, string>("16", "17"),
                new KeyValuePair<string, string>("17", "18"),
                new KeyValuePair<string, string>("18", "19"),
                new KeyValuePair<string, string>("19", "20"),
                new KeyValuePair<string, string>("20", "21"),
                new KeyValuePair<string, string>("21", "22"),
                new KeyValuePair<string, string>("22", "23"),
                new KeyValuePair<string, string>("23", "24"),
                new KeyValuePair<string, string>("24", "25"),
                new KeyValuePair<string, string>("25", "26"),
                new KeyValuePair<string, string>("26", "27"),
                new KeyValuePair<string, string>("27", "28"),
                new KeyValuePair<string, string>("28", "29"),
                new KeyValuePair<string, string>("29", "30"),
                new KeyValuePair<string, string>("30", "31"),
                new KeyValuePair<string, string>("31", "32"),
            };
            ucCombox_screwMax.SelectedIndex = actXET.para.screwmax - 1;

            //离线工单执行模式
            ucCombox_runMode.Source = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("0",  "不执行"),
                new KeyValuePair<string, string>("1",  "手动执行"),
                new KeyValuePair<string, string>("2",  "自动执行"),
            };
            ucCombox_runMode.SelectedIndex = actXET.para.runmode;

            #endregion

        }

        //依据权限调整控件状态
        private void RoleBasedUIChange()
        {
            //预设值设置
            switch (MyDevice.userRole)
            {
                case "0":
                    label_point.Visible     = false;
                    ucCombox_point.Visible  = false;

                    label_select.Enabled    = false;
                    ucCombox_select.Enabled = false;
                    break;
                case "1":
                    label_point.Visible     = false;
                    ucCombox_point.Visible  = false;

                    label_select.Enabled    = true;
                    ucCombox_select.Enabled = true;
                    break;
                default:
                case "32":
                    label_point.Visible     = true;
                    ucCombox_point.Visible  = true;

                    label_select.Enabled    = true;
                    ucCombox_select.Enabled = true;
                    break;
            }
            //模式设置
            switch (MyDevice.userRole)
            {
                case "0":
                    label_fifomode.Enabled         = false;
                    ucCombox_fifomode.Enabled      = false;
                    label_fiforec.Enabled          = false;
                    ucCombox_fiforec.Enabled       = false;
                    label_fifospeed.Enabled        = false;
                    ucCombox_fifospeed.Enabled     = false;
                    label_heart.Enabled            = false;
                    ucCombox_heart.Enabled         = false;
                    label_heartcount.Enabled       = false;
                    ucTextBoxEx_heartcount.Enabled = false;
                    label_heartcycle.Enabled       = false;
                    ucTextBoxEx_heartcycle.Enabled = false;
                    label_accmode.Enabled          = false;
                    ucCombox_accmode.Enabled       = false;
                    label_alarmode.Enabled         = false;
                    ucCombox_alarmode.Enabled      = false;
                    label_wifimode.Enabled         = false;
                    ucCombox_wifimode.Enabled      = false;
                    label_timeoff.Enabled          = false;
                    ucTextBoxEx_timeoff.Enabled    = false;
                    label_timeback.Enabled         = false;
                    ucTextBoxEx_timeback.Enabled   = false;
                    label_timezero.Enabled         = false;
                    ucTextBoxEx_timezero.Enabled   = false;
                    label_torcorr.Enabled          = false;
                    ucTextBoxEx_torcorr.Enabled    = false;
                    label_angleResist.Enabled      = false;
                    ucTextBoxEx_angleResist .Enabled = false;    
                    label_caltime.Enabled          = false;
                    dateTimePicker_caltime.Enabled = false;
                    label_calremind.Enabled        = false;
                    dateTimePicker_calremind .Enabled = false;

                    groupBox4.Visible              = false;
                    //label_disptype.Enabled         = false;
                    //ucCombox_disptype.Enabled      = false;
                    //label_disptheme.Enabled        = false;
                    //ucCombox_disptheme.Enabled     = false;
                    //label_displan.Enabled          = false;
                    //ucCombox_displan.Enabled       = false;

                    bt_UpdateMode.Enabled          = false;
                    break;
                case "1":
                    label_fifomode.Enabled         = true;
                    ucCombox_fifomode.Enabled      = true;
                    label_fiforec.Enabled          = true;
                    ucCombox_fiforec.Enabled       = true;
                    label_fifospeed.Enabled        = true;
                    ucCombox_fifospeed.Enabled     = true;
                    label_heart.Enabled            = true;
                    ucCombox_heart.Enabled         = true;
                    label_heartcount.Enabled       = true;
                    ucTextBoxEx_heartcount.Enabled = true;
                    label_heartcycle.Enabled       = true;
                    ucTextBoxEx_heartcycle.Enabled = true;
                    label_accmode.Enabled          = true;
                    ucCombox_accmode.Enabled       = true;
                    label_alarmode.Enabled         = true;
                    ucCombox_alarmode.Enabled      = true;
                    label_wifimode.Enabled         = true;
                    ucCombox_wifimode.Enabled      = true;
                    label_timeoff.Enabled          = true;
                    ucTextBoxEx_timeoff.Enabled    = true;
                    label_timeback.Enabled         = true;
                    ucTextBoxEx_timeback.Enabled   = true;
                    label_timezero.Enabled         = true;
                    ucTextBoxEx_timezero.Enabled   = true;       
                    label_torcorr.Enabled          = true;
                    ucTextBoxEx_torcorr.Enabled    = true;
                    label_angleResist.Enabled      = true;
                    ucTextBoxEx_angleResist .Enabled = true;               
                    label_caltime.Enabled          = true;
                    dateTimePicker_caltime.Enabled = true;
                    label_calremind.Enabled        = true;
                    dateTimePicker_calremind .Enabled = true;

                    groupBox4.Visible = false;
                    //label_disptype.Enabled         = true;
                    //ucCombox_disptype.Enabled      = true;
                    //label_disptheme.Enabled        = true;
                    //ucCombox_disptheme.Enabled     = true;
                    //label_displan.Enabled          = true;
                    //ucCombox_displan.Enabled       = true;

                    bt_UpdateMode.Enabled          = true;
                    break;
                default:
                case "32":
                    label_fifomode.Enabled         = true;
                    ucCombox_fifomode.Enabled      = true;
                    label_fiforec.Enabled          = true;
                    ucCombox_fiforec.Enabled       = true;
                    label_fifospeed.Enabled        = true;
                    ucCombox_fifospeed.Enabled     = true;
                    label_heart.Enabled            = true;
                    ucCombox_heart.Enabled         = true;
                    label_heartcount.Enabled       = true;
                    ucTextBoxEx_heartcount.Enabled = true;
                    label_heartcycle.Enabled       = true;
                    ucTextBoxEx_heartcycle.Enabled = true;
                    label_accmode.Enabled          = true;
                    ucCombox_accmode.Enabled       = true;
                    label_alarmode.Enabled         = true;
                    ucCombox_alarmode.Enabled      = true;
                    label_wifimode.Enabled         = true;
                    ucCombox_wifimode.Enabled      = true;
                    label_timeoff.Enabled          = true;
                    ucTextBoxEx_timeoff.Enabled    = true;
                    label_timeback.Enabled         = true;
                    ucTextBoxEx_timeback.Enabled   = true;
                    label_timezero.Enabled         = true;
                    ucTextBoxEx_timezero.Enabled   = true;        
                    label_torcorr.Enabled          = true;
                    ucTextBoxEx_torcorr.Enabled    = true;
                    label_angleResist.Enabled      = true;
                    ucTextBoxEx_angleResist .Enabled = true;
                    label_caltime.Enabled          = true;
                    dateTimePicker_caltime.Enabled = true;
                    label_calremind.Enabled        = true;
                    dateTimePicker_calremind .Enabled = true;

                    groupBox4.Visible              = false;            
                    //label_disptype.Enabled         = true;
                    //ucCombox_disptype.Enabled      = true;
                    //label_disptheme.Enabled        = true;
                    //ucCombox_disptheme.Enabled     = true;
                    //label_displan.Enabled          = true;
                    //ucCombox_displan.Enabled       = true;

                    bt_UpdateMode.Enabled          = true;
                    break;
            }
            //WLAN设置
            switch (MyDevice.userRole)
            {
                case "32":
                case "1":
                    label_addr.Enabled          = true;
                    ucTextBoxEx_addr.Enabled    = true;
                    label_RFchan.Enabled        = true;
                    ucCombox_RFchan.Enabled     = true;
                    label_wifiIp.Enabled        = true;
                    ucTextBoxEx_wifiIp.Enabled  = true;
                    label_port.Enabled          = true;
                    ucTextBoxEx_port.Enabled    = true;
                    label_ssid.Enabled          = true;
                    ucTextBoxEx_ssid.Enabled    = true;
                    label_pwd.Enabled           = true;
                    ucTextBoxEx_pwd.Enabled     = true;

                    bt_UpdateWLAN.Enabled       = true;

                    label_wirelessSelection.Enabled = true;
                    ucCombox_wirelessSelection.Enabled = true;
                    label_tip.Enabled = true;
                    break;
                default:
                case "0":
                    label_addr.Enabled          = false;
                    ucTextBoxEx_addr.Enabled    = true;
                    ucCombox_RFchan.Enabled     = false;
                    label_wifiIp.Enabled        = false;
                    ucTextBoxEx_wifiIp.Enabled  = false;
                    label_port.Enabled          = false;
                    ucTextBoxEx_port.Enabled    = false;
                    label_ssid.Enabled          = false;
                    ucTextBoxEx_ssid.Enabled    = false;
                    label_pwd.Enabled           = false;
                    ucTextBoxEx_pwd.Enabled     = false;

                    bt_UpdateWLAN.Enabled       = false;

                    label_wirelessSelection.Enabled = false;
                    ucCombox_wirelessSelection.Enabled = false;
                    label_tip.Enabled = false;
                    break;
            }
            //工厂设置
            switch (MyDevice.userRole)
            {
                case "0":
                case "1":
                    tb_adZero.Enabled = false;
                    tb_adPos1.Enabled = false;
                    tb_adPos2.Enabled = false;
                    tb_adPos3.Enabled = false;
                    tb_adPos4.Enabled = false;
                    tb_adPos5.Enabled = false;
                    tb_adNeg1.Enabled = false;
                    tb_adNeg2.Enabled = false;
                    tb_adNeg3.Enabled = false;
                    tb_adNeg4.Enabled = false;
                    tb_adNeg5.Enabled = false;

                    tb_adZeroOutput.Enabled = false;
                    tb_adPosOutPut1.Enabled = false;
                    tb_adPosOutPut2.Enabled = false;
                    tb_adPosOutPut3.Enabled = false;
                    tb_adPosOutPut4.Enabled = false;
                    tb_adPosOutPut5.Enabled = false;
                    tb_adNegOutPut1.Enabled = false;
                    tb_adNegOutPut2.Enabled = false;
                    tb_adNegOutPut3.Enabled = false;
                    tb_adNegOutPut4.Enabled = false;
                    tb_adNegOutPut5.Enabled = false;

                    label_calUnit.Enabled    = false;
                    ucCombox_calUnit.Enabled = false;
                    label_calType.Enabled    = false;
                    ucCombox_calType.Enabled = false;
                    label_capacity.Enabled   = false;
                    ucCombox_capacity.Enabled = false;

                    label_unhook.Enabled        = false;
                    ucTextBoxEx_unhook.Enabled  = false;

                    groupBox24.Visible          = false;
                    label_usbEn.Enabled         = false;
                    ucCombox_usbEN.Enabled      = false;
                    label_wirelessEn.Enabled    = false;
                    ucCombox_wirelessEn.Enabled = false;

                    groupBox6.Visible           = false;

                    btn_SuperUpdate.Enabled = false;
                    break;
                case "32":
                    tb_adZero.Enabled = false;
                    tb_adPos1.Enabled = false;
                    tb_adPos2.Enabled = false;
                    tb_adPos3.Enabled = false;
                    tb_adPos4.Enabled = false;
                    tb_adPos5.Enabled = false;
                    tb_adNeg1.Enabled = false;
                    tb_adNeg2.Enabled = false;
                    tb_adNeg3.Enabled = false;
                    tb_adNeg4.Enabled = false;
                    tb_adNeg5.Enabled = false;

                    tb_adZeroOutput.Enabled = true;
                    tb_adPosOutPut1.Enabled = true;
                    tb_adPosOutPut2.Enabled = true;
                    tb_adPosOutPut3.Enabled = true;
                    tb_adPosOutPut4.Enabled = true;
                    tb_adPosOutPut5.Enabled = true;
                    tb_adNegOutPut1.Enabled = true;
                    tb_adNegOutPut2.Enabled = true;
                    tb_adNegOutPut3.Enabled = true;
                    tb_adNegOutPut4.Enabled = true;
                    tb_adNegOutPut5.Enabled = true;
                    
                    label_calUnit.Enabled    = true;
                    ucCombox_calUnit.Enabled = true;
                    label_calType.Enabled    = true;
                    ucCombox_calType.Enabled = true;
                    label_capacity.Enabled   = true;
                    ucCombox_capacity.Enabled = true;

                    label_unhook.Enabled        = true;
                    ucTextBoxEx_unhook.Enabled  = true;
                    
                    groupBox24.Visible           = true;
                    label_usbEn.Enabled          = true;
                    ucCombox_usbEN.Enabled       = true;
                    label_wirelessEn.Enabled     = true;
                    ucCombox_wirelessEn.Enabled  = true;

                    groupBox6.Visible       = true;

                    btn_SuperUpdate.Enabled = true;
                    break;
                default:
                    break;
            }
            //工单设置
            switch (MyDevice.userRole)
            {
                case "0":
                    btn_UpdateTicket.Enabled = false;

                    dataGridView2.Enabled = false;
                    ucCombox_screwMax.Enabled = false;
                    ucCombox_runMode.Enabled = false;

                    break;
                case "1":
                    btn_UpdateTicket.Enabled = true;

                    dataGridView2.Enabled = true;
                    ucCombox_screwMax.Enabled = true;
                    ucCombox_runMode.Enabled = true;
                    break;
                case "32":
                    break;

            }
        }

        //依据扳手型号调整控件状态
        private void TypeBasedUIChange()
        {
            //预设值设置
            switch (actXET.devc.type)
            {
                case TYPE.TQ_XH_XL01_06 - (UInt16)ADDROFFSET.TQ_XH_ADDR:
                    ucCombox_point.Enabled = false;
                    groupBox22.Visible = false;
                    break;

                default:
                case TYPE.TQ_XH_XL01_07:
                case TYPE.TQ_XH_XL01_07 - 1280:
                case TYPE.TQ_XH_XL01_09 - 1280:
                    ucCombox_point.Enabled = true;
                    groupBox22.Visible = true;
                    break;
            }

            //WLAN设置
            switch (actXET.devc.type)
            {
                case TYPE.TQ_XH_XL01_06 - (UInt16)ADDROFFSET.TQ_XH_ADDR:
                case TYPE.TQ_XH_XL01_07:
                case TYPE.TQ_XH_XL01_07 - 1280:
                case TYPE.TQ_XH_XL01_09 - 1280:
                    groupBox9.Visible = false;
                    break;

                default:
                    groupBox9.Visible = true;
                    break;
            }
        }

        //加载设备列表
        private void ucDataGridView1_Load(object sender, EventArgs e)
        {
            //设备列表控件
            List<DataGridViewColumnEntity> lstCulumns = new List<DataGridViewColumnEntity>();

            //新增"设备列表"列
            lstCulumns.Add(new DataGridViewColumnEntity() { DataField = "Device", HeadText = "设备", Width = ucDataGridView1.Width, WidthType = SizeType.Absolute });

            this.ucDataGridView1.Columns = lstCulumns;
            this.ucDataGridView1.IsShowCheckBox = true;
            List<object> lstSource = new List<object>();

            //USB单设备
            if (MyDevice.protocol.type == COMP.UART)
            {
                GridModel model = new GridModel()
                {
                    Device = "设备1"
                };
                lstSource.Add(model);
                mutiAddres.Add(1);
            }
            //XF/TCP设备
            else
            {
                //将已连接设备的地址存入列表
                for (Byte i = 1; i != 0; i++)
                {
                    if (MyDevice.protocol.type == COMP.XF)
                    {
                        //设置参数需要读取设备所有参数，必须设备状态在Working，connected状态下读取的参数不完全
                        if (MyDevice.mXF[i].sTATE == STATE.WORKING)
                        {
                            mutiAddres.Add(i);
                        }
                    }
                    else if (MyDevice.protocol.type == COMP.TCP)
                    {
                        if (MyDevice.mTCP[i].sTATE == STATE.WORKING)
                        {
                            mutiAddres.Add(i);
                        }
                    }
                    else if (MyDevice.protocol.type == COMP.RS485)
                    {
                        if (MyDevice.mRS[i].sTATE == STATE.WORKING)
                        {
                            mutiAddres.Add(i);
                        }
                    }
                }

                //根据已连接设备数新增"设备"行
                for (int i = 0; i < mutiAddres.Count; i++)
                {
                    GridModel model = new GridModel()
                    {
                        Device = "设备" + mutiAddres[i].ToString()
                    };
                    lstSource.Add(model);
                }
            }

            this.ucDataGridView1.DataSource = lstSource;

            if (this.ucDataGridView1.Rows.Count > 0)
            {
                this.ucDataGridView1.Rows[0].IsChecked = true;//默认选中连接的第一个设备
            }
        }

        //关闭窗口
        private void MenuDeviceSetForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //界面更新事件
            MyDevice.myTaskManager.UpdateUI -= updateUI;

            //针对08扳手usb通讯改站点操作
            //标准的写站点操作是，写下去什么站点，回复的就是以该站点为帧头的回复
            //由于USB限制指令回复为01，故任意站点写下去回复的时候是01，导致底层写成功之后更新站点为01，与实际不符合
            if (MyDevice.protocol.type == COMP.UART)
            {
                actXET.wlan.addr = Convert.ToByte(ucTextBoxEx_addr.InputText);
            }
        }

        //预设值设置表格初始化
        private void DataGridView1_Init()
        {
            //表格初始化
            dataGridView1.EnableHeadersVisualStyles = false;//允许自定义行头样式
            dataGridView1.RowHeadersVisible = false; //第一列空白隐藏掉
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.CadetBlue;//行头背景颜色
            dataGridView1.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dataGridView1.AllowUserToAddRows = false;//禁止用户添加行
            dataGridView1.AllowUserToDeleteRows = false;//禁止用户删除行
            dataGridView1.AllowUserToOrderColumns = false;//禁止用户手动重新定位行
            dataGridView1.AllowUserToResizeRows = false;//禁止用户调整行大小
            dataGridView1.AllowUserToResizeColumns = false;//禁止用户调整列大小
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;// 禁止用户改变列头的高度 
            dataGridView1.Font = new Font("Arial", 8, FontStyle.Bold);

            Font font = new Font("Arial", 10, FontStyle.Bold);

            //模式数据行初始化
            dataGridView1.Rows.Add();
            dataGridView1.Rows[0].Cells[0].Value = "模式";
            dataGridView1.Rows[0].Cells[1].Value = "扭矩限制值";
            dataGridView1.Rows[0].Cells[2].Value = "扭矩初始值";
            dataGridView1.Rows[0].Cells[3].Value = "角度限制值";
            dataGridView1.Rows[0].Cells[4].Value = "扭矩下限值";
            dataGridView1.Rows[0].Cells[5].Value = "扭矩上限值";
            dataGridView1.Rows[0].Cells[6].Value = "扭矩初始值";
            dataGridView1.Rows[0].Cells[7].Value = "角度下限值";
            dataGridView1.Rows[0].Cells[8].Value = "角度上限值";

            //模式数据列初始化
            for (int i = 1; i < 11; i++)
            {
                dataGridView1.Rows.Add();
                dataGridView1.Rows[i].Cells[0].Value = "M" + (i - 1).ToString();
                dataGridView1.Rows[i].Cells[0].Style.Font = font;
            }

            //行首与列首均禁止编辑
            dataGridView1.Rows[0].ReadOnly = true;
            dataGridView1.Columns[0].ReadOnly = true;

            //设置列宽
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            //设置行高
            int height = 0;
            dataGridView1.ColumnHeadersHeight = dataGridView1.Height / 25;
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                dataGridView1.Rows[i].Height = (dataGridView1.Height - dataGridView1.ColumnHeadersHeight) / dataGridView1.RowCount;
                height += dataGridView1.Rows[i].Height;
            }
            dataGridView1.ColumnHeadersHeight = dataGridView1.Height - height;
        }

        //离线工单表格初始化
        private void DataGridView2_Init()
        {
            //表格初始化
            dataGridView2.EnableHeadersVisualStyles = false;//允许自定义行头样式
            dataGridView2.RowHeadersVisible = false; //第一列空白隐藏掉
            dataGridView2.ColumnHeadersDefaultCellStyle.BackColor = Color.CadetBlue;//行头背景颜色
            dataGridView2.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView2.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dataGridView2.AllowUserToAddRows = false;//禁止用户添加行
            dataGridView2.AllowUserToDeleteRows = false;//禁止用户删除行
            dataGridView2.AllowUserToOrderColumns = false;//禁止用户手动重新定位行
            dataGridView2.AllowUserToResizeRows = false;//禁止用户调整行大小
            dataGridView2.AllowUserToResizeColumns = false;//禁止用户调整列大小
            dataGridView2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;// 禁止用户改变列头的高度 
            dataGridView2.Font = new Font("Arial", 8, FontStyle.Bold);

            Font font = new Font("Arial", 10, FontStyle.Bold);

            //模式数据列初始化
            for (int i = 0; i < ticketCntMax; i++)
            {
                dataGridView2.Rows.Add();
                dataGridView2.Rows[i].Cells[0].Value = i + 1;
                dataGridView2.Rows[i].Cells[0].Style.Font = font;
            }

            //行首与列首均禁止编辑
            dataGridView2.Columns[0].ReadOnly = true;

            // 禁止所有列的排序
            foreach (DataGridViewColumn column in dataGridView2.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            //设置列宽
            dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView2.ScrollBars = ScrollBars.None;

            //设置行高
            int height = 0;
            dataGridView2.ColumnHeadersHeight = dataGridView2.Height / 25;
            for (int i = 0; i < dataGridView2.RowCount; i++)
            {
                dataGridView2.Rows[i].Height = (dataGridView2.Height - dataGridView2.ColumnHeadersHeight) / dataGridView2.RowCount;
                height += dataGridView2.Rows[i].Height;
            }
            dataGridView2.ColumnHeadersHeight = dataGridView2.Height - height;

            TicketInit();//表格初始化后初始化离线工单
        }

        #endregion

        #region 控件功能

        //选择设备
        private void ucDataGridView1_ItemClick(object sender, DataGridViewEventArgs e)
        {
            //若只选择1个设备则更新此设备的参数
            if (ucDataGridView1.SelectRows.Count == 1)
            {
                //获取已选设备地址
                if (MyDevice.protocol.type != COMP.UART)
                {
                    MyDevice.protocol.addr = mutiAddres[ucDataGridView1.SelectRows[0].RowIndex];
                    actXET = MyDevice.actDev;
                    actXET.torqueMultiple = (int)Math.Pow(10, actXET.devc.torque_decimal);
                    actXET.angleMultiple = (int)Math.Pow(10, actXET.para.angle_decimal);
                }
            }
            else if (ucDataGridView1.SelectRows.Count > 1)
            {
                //获取已选设备地址
                if (MyDevice.protocol.type != COMP.UART)
                {
                    MyDevice.protocol.addr = mutiAddres[ucDataGridView1.SelectRows[e.RowIndex].RowIndex];
                    actXET = MyDevice.actDev;
                    actXET.torqueMultiple = (int)Math.Pow(10, actXET.devc.torque_decimal);
                    actXET.angleMultiple = (int)Math.Pow(10, actXET.para.angle_decimal);
                }
                ucCombox1_SelectedChangedEvent(null, null);

                //参数combox和textbox初始化
                Parameters_Init();

                //权限控制
                RoleBasedUIChange();

                //依据扳手型号调整控件状态
                TypeBasedUIChange();
            }
        }

        //更新参数
        private void bt_UpdatePara_Click(object sender, EventArgs e)
        {
            if (ucDataGridView1.SelectRows.Count == 0)
            {
                MessageBox.Show("未选择设备");
                return;
            }

            //按键状态
            bt_UpdatePara.BackColor = Color.Firebrick;

            //针对06-07扳手参数部分不同必须统一,参数向较少系列的设备靠近
            UnifyDevPara();

            //多设备
            for (int j = 0; j < ucDataGridView1.SelectRows.Count; j++)
            {
                //获取已选设备地址
                if (MyDevice.protocol.type != COMP.UART)
                {
                    MyDevice.protocol.addr = mutiAddres[ucDataGridView1.SelectRows[j].RowIndex];
                    if (MyDevice.protocol.type == COMP.TCP)
                    {
                        MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
                    }
                    actXET = MyDevice.actDev;
                }

                //设备参数
                actXET.para.torque_unit = (UNIT)ucCombox_torqueUnit.SelectedIndex;
                actXET.para.mode_pt = (byte)ucCombox_modePt.SelectedIndex;
                actXET.para.mode_ax = (byte)ucCombox_modeAx.SelectedIndex;
                actXET.para.mode_mx = (byte)ucCombox_modeMx.SelectedIndex;
                actXET.para.angle_decimal = (byte)ucCombox_point.SelectedIndex;
                actXET.torqueMultiple = (int)Math.Pow(10, actXET.devc.torque_decimal);
                actXET.angleMultiple = (int)Math.Pow(10, actXET.para.angle_decimal);
                if (MyDevice.userRole != "0")
                {
                    actXET.para.amenable = (ushort)(ucCombox_select.SelectedIndex == 0 ? 0 : GetAlarmEnable());
                }


                int torqueMax = actXET.devc.torque_max;//量程上限
                int torqueMin = actXET.devc.torque_min;//量程下限

                //单位转换
                switch (actXET.devc.calunit)
                {
                    //根据不同的标定单位获取指定单位的峰值
                    //标定单位：actXET.devc.unit 指设备初次标定时所用的单位，基准是min = 150, max = 3000
                    //指定单位：actXET.para.torque_unit，设备使用时选择的单位
                    case UNIT.UNIT_nm:
                        torqueMax = UnitConvert.Torque_nmTrans(torqueMax, (byte)actXET.para.torque_unit);
                        torqueMin = UnitConvert.Torque_nmTrans(torqueMin, (byte)actXET.para.torque_unit);
                        break;
                    case UNIT.UNIT_lbfin:
                        torqueMax = UnitConvert.Torque_lbfinTrans(torqueMax, (byte)actXET.para.torque_unit);
                        torqueMin = UnitConvert.Torque_lbfinTrans(torqueMin, (byte)actXET.para.torque_unit);
                        break;
                    case UNIT.UNIT_lbfft:
                        torqueMax = UnitConvert.Torque_lbfftTrans(torqueMax, (byte)actXET.para.torque_unit);
                        torqueMin = UnitConvert.Torque_lbfftTrans(torqueMin, (byte)actXET.para.torque_unit);
                        break;
                    case UNIT.UNIT_kgcm:
                        torqueMax = UnitConvert.Torque_kgfcmTrans(torqueMax, (byte)actXET.para.torque_unit);
                        torqueMin = UnitConvert.Torque_kgfcmTrans(torqueMin, (byte)actXET.para.torque_unit);
                        break;
                    case UNIT.UNIT_kgm:
                        torqueMax = UnitConvert.Torque_kgfmTrans(torqueMax, (byte)actXET.para.torque_unit);
                        torqueMin = UnitConvert.Torque_kgfmTrans(torqueMin, (byte)actXET.para.torque_unit);
                        break;
                    default:
                        break;
                }

                //报警值
                for (int i = 0; i < 10; i++)
                {
                    SetTorqueAlarm(i, 1, ref actXET.alam.SN_target, torqueMax, torqueMin);
                    SetTorqueAlarm(i, 2, ref actXET.alam.SA_pre, torqueMax, torqueMin);
                    SetTorqueAlarm(i, 4, ref actXET.alam.MN_low, torqueMax, torqueMin);
                    SetTorqueAlarm(i, 5, ref actXET.alam.MN_high, torqueMax, torqueMin);
                    SetTorqueAlarm(i, 6, ref actXET.alam.MA_pre, torqueMax, torqueMin);

                    actXET.alam.SA_ang[i] = dataGridView1.Rows[i + 1].Cells[3].Value.ToString() == "" ? -1 : (Int32)(Convert.ToDouble(dataGridView1.Rows[i + 1].Cells[3].Value.ToString()) * actXET.angleMultiple + 0.5);
                    actXET.alam.MA_low[i] = dataGridView1.Rows[i + 1].Cells[7].Value.ToString() == "" ? -1 : (Int32)(Convert.ToDouble(dataGridView1.Rows[i + 1].Cells[7].Value.ToString()) * actXET.angleMultiple + 0.5);
                    actXET.alam.MA_high[i] = dataGridView1.Rows[i + 1].Cells[8].Value.ToString() == "" ? -1 : (Int32)(Convert.ToDouble(dataGridView1.Rows[i + 1].Cells[8].Value.ToString()) * actXET.angleMultiple + 0.5);
                }
            }
            selectNum = 0;
            MyDevice.protocol.addr = mutiAddres[ucDataGridView1.SelectRows[selectNum].RowIndex];//从第一个设备开始设置
            if (MyDevice.protocol.type == COMP.TCP)
            {
                MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
            }
            actXET = MyDevice.actDev;
            buttonClicked = "bt_UpdatePara";
            MyDevice.protocol.Protocol_ClearState();//清串口，防止WiFI接收错乱

            List<TASKS> tasks = new List<TASKS>
            {
                TASKS.REG_BLOCK3_PARA,
                TASKS.REG_BLOCK5_AM1,
                TASKS.REG_BLOCK5_AM2,
                TASKS.REG_BLOCK5_AM3
            };

            for (int i = 0; i < ucDataGridView1.SelectRows.Count; i++)
            {
                MyDevice.mDev[mutiAddres[ucDataGridView1.SelectRows[i].RowIndex]].torqueMultiple = (int)Math.Pow(10, MyDevice.mDev[mutiAddres[ucDataGridView1.SelectRows[i].RowIndex]].devc.torque_decimal);
                MyDevice.mDev[mutiAddres[ucDataGridView1.SelectRows[i].RowIndex]].angleMultiple = (int)Math.Pow(10, MyDevice.mDev[mutiAddres[ucDataGridView1.SelectRows[i].RowIndex]].para.angle_decimal);
                MyDevice.myTaskManager.AddUserCommands(mutiAddres[ucDataGridView1.SelectRows[i].RowIndex], ProtocolFunc.Protocol_Sequence_SendCOM, tasks, this.Name);
            }
        }

        //更新模式设置
        private void bt_UpdateMode_Click(object sender, EventArgs e)
        {
            if (ucDataGridView1.SelectRows.Count == 0)
            {
                MessageBox.Show("未选择设备");
                return;
            }

            if ((ucTextBoxEx_heartcount.InputText == ""    && ucTextBoxEx_heartcount.Visible == true)
                || (ucTextBoxEx_heartcycle.InputText == "" && ucTextBoxEx_heartcycle.Visible == true)
                || (ucTextBoxEx_timeoff.InputText == ""    && ucTextBoxEx_timeoff.Visible == true)
                || (ucTextBoxEx_timeback.InputText == ""   && ucTextBoxEx_timeback.Visible == true)
                || (ucTextBoxEx_timezero.InputText == ""   && ucTextBoxEx_timezero.Visible == true)
                || (ucTextBoxEx_torcorr.InputText == "" && ucTextBoxEx_torcorr.Visible == true)
                || (ucTextBoxEx_angleResist.InputText == "" && ucTextBoxEx_angleResist.Visible == true)
                )
            {
                MessageBox.Show("有参数未填写, 请检查所有参数", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //按键状态
            bt_UpdateMode.BackColor = Color.Firebrick;

            //多设备
            for (int j = 0; j < ucDataGridView1.SelectRows.Count; j++)
            {
                //获取已选设备地址
                if (MyDevice.protocol.type != COMP.UART)
                {
                    MyDevice.protocol.addr = mutiAddres[ucDataGridView1.SelectRows[j].RowIndex];
                    if (MyDevice.protocol.type == COMP.TCP)
                    {
                        MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
                    }
                    actXET = MyDevice.actDev;
                }

                //模式设置
                actXET.para.fifomode = (byte)ucCombox_fifomode.SelectedIndex;
                actXET.para.fiforec = (byte)ucCombox_fiforec.SelectedIndex;
                actXET.para.fifospeed = (byte)ucCombox_fifospeed.SelectedIndex;

                //还需要加持续回复的判断
                if (byte.TryParse(ucTextBoxEx_heartcount.InputText, out byte heartcount))
                {
                    actXET.para.heartcount = heartcount;
                }
                if (UInt16.TryParse(ucTextBoxEx_heartcycle.InputText, out UInt16 heartcycle))
                {
                    actXET.para.heartcycle = heartcycle;
                }
                actXET.para.accmode = (byte)ucCombox_accmode.SelectedIndex;
                actXET.para.alarmode = (byte)ucCombox_alarmode.SelectedIndex;
                if (byte.TryParse(ucTextBoxEx_timeoff.InputText, out byte timeoff))
                {
                    actXET.para.timeoff = timeoff;
                }
                if (byte.TryParse(ucTextBoxEx_timeback.InputText, out byte timeback))
                {
                    actXET.para.timeback = timeback;
                }
                if (byte.TryParse(ucTextBoxEx_timezero.InputText, out byte timezero))
                {
                    actXET.para.timezero = timezero;
                }

                //更新校准时间
                actXET.work.caltime = MyDevice.DateTimeToUInt32((DateTime)dateTimePicker_caltime.Value);
                actXET.work.calremind = MyDevice.DateTimeToUInt32((DateTime)dateTimePicker_calremind.Value);

                if (actXET.work.calremind < actXET.work.caltime)
                {
                    MessageBox.Show("复校时间不能早于校准时间，请重新设置", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                //actXET.para.disptype = (byte)ucCombox_disptype.SelectedIndex;
                //actXET.para.disptheme = (byte)ucCombox_disptheme.SelectedIndex;
                //actXET.para.displan = (byte)ucCombox_displan.SelectedIndex;

                //扭矩修正系数
                if (float.TryParse(ucTextBoxEx_torcorr.InputText, out float torcorr))
                {
                    actXET.para.torcorr = torcorr;
                }

                //复拧角度
                if (ucTextBoxEx_angleResist.InputText != "")
                {
                    actXET.para.angle_resist = (int)(Convert.ToDouble(ucTextBoxEx_angleResist.InputText) * (int)Math.Pow(10, actXET.para.angle_decimal) + 0.5);
                }

            }
            selectNum = 0;
            MyDevice.protocol.addr = mutiAddres[ucDataGridView1.SelectRows[selectNum].RowIndex];//从第一个设备开始设置
            if (MyDevice.protocol.type == COMP.TCP)
            {
                MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
            }
            actXET = MyDevice.actDev;
            buttonClicked = "bt_UpdateMode";

            List<TASKS> tasks = new List<TASKS>
            {
                TASKS.REG_BLOCK5_INFO,
                TASKS.REG_BLOCK3_PARA,
            };

            for (int i = 0; i < ucDataGridView1.SelectRows.Count; i++)
            {
                MyDevice.myTaskManager.AddUserCommands(mutiAddres[ucDataGridView1.SelectRows[i].RowIndex], ProtocolFunc.Protocol_Sequence_SendCOM, tasks, this.Name);
            }

        }

        //更新WLAN设置
        private void bt_UpdateWLAN_Click(object sender, EventArgs e)
        {
            if (ucDataGridView1.SelectRows.Count == 0)
            {
                MessageBox.Show("未选择设备");
                return;
            }

            if (ucDataGridView1.SelectRows.Count > 1)
            {
                MessageBox.Show("设备数量超过1，避免地址冲突", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (ucTextBoxEx_addr.InputText == "" ||
                ucTextBoxEx_wifiIp.InputText == "" ||
                ucTextBoxEx_port.InputText == "" ||
                ucTextBoxEx_ssid.InputText == "" ||
                ucTextBoxEx_pwd.InputText == ""
                )
            {
                MessageBox.Show("有参数未填写, 请检查所有参数", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //IP地址格式校验
            string ipPattern = @"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\." +
               @"(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\." +
               @"(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\." +
               @"(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";

            if (!Regex.IsMatch(ucTextBoxEx_wifiIp.InputText, ipPattern))
            {
                MessageBox.Show("IP格式不正确，请重新输入，示例192.168.1.1");
                ucTextBoxEx_wifiIp.InputText = "192.168.1.1";
                return;
            }

            bt_UpdateWLAN.BackColor = Color.Firebrick;
            buttonClicked = "bt_UpdateWLAN";
            selectNum = 0;

            //wlan设置
            byte oldAddr;
            byte newAddr;
            oldAddr = mutiAddres[ucDataGridView1.SelectRows[selectNum].RowIndex];
            oldDevAddr = oldAddr;

            if (byte.TryParse(ucTextBoxEx_addr.InputText, out newAddr))
            {
                actXET.wlan.addr = newAddr;
            }
            actXET.wlan.rf_chan = (byte)ucCombox_RFchan.SelectedIndex;

            /*
             * ip需要做特殊处理，8个字节
             * 以192.168.1.1示例, 发出去的字节分布为 192 00 168 00 01 00 01
             * 转换成字符串为"C0A80101",Model底层解析
             */
            actXET.wlan.wf_ip = "";
            IPAddress ipAddress = IPAddress.Parse(ucTextBoxEx_wifiIp.InputText);
            byte[] ipBytes = ipAddress.GetAddressBytes();
            foreach (var item in ipBytes)
            {
                actXET.wlan.wf_ip += item.ToString("X2");
            }

            actXET.wlan.wf_port = Convert.ToUInt32(ucTextBoxEx_port.InputText);
            actXET.wlan.wf_ssid = ucTextBoxEx_ssid.InputText;
            //wf_pwd密码8-15位默认最后一位补'\0'
            if (ucTextBoxEx_pwd.InputText.Length >= 8 && ucTextBoxEx_pwd.InputText.Length <= 15)
            {
                actXET.wlan.wf_pwd = ucTextBoxEx_pwd.InputText + '\0';
            }
            else
            {
                actXET.wlan.wf_pwd = ucTextBoxEx_pwd.InputText;
            }

            actXET.wlan.wifimode = (byte)ucCombox_wifimode.SelectedIndex;
            actXET.torqueMultiple = (int)Math.Pow(10, actXET.devc.torque_decimal);
            actXET.angleMultiple = (int)Math.Pow(10, actXET.para.angle_decimal);

            if (newAddr != oldAddr)
            {
                //站点更改，对应的参数更新，同时状态更新
                switch (MyDevice.protocol.type)
                {
                    default:
                    case COMP.UART:
                        break;

                    case COMP.TCP:
                        MyDevice.mTCP[oldAddr].sTATE = STATE.INVALID;
                        MyDevice.mTCP[newAddr].sTATE = STATE.WORKING;
                        MyDevice.mTCP[newAddr].opsn = MyDevice.mTCP[oldAddr].opsn;
                        MyDevice.mTCP[newAddr].snBat = MyDevice.mTCP[oldAddr].snBat;
                        MyDevice.mTCP[newAddr].torqueMultiple = MyDevice.mTCP[oldAddr].torqueMultiple;
                        MyDevice.mTCP[newAddr].angleMultiple = MyDevice.mTCP[oldAddr].angleMultiple;
                        MyDevice.mTCP[newAddr].auto = MyDevice.mTCP[oldAddr].auto;
                        MyDevice.mTCP[newAddr].wlan = MyDevice.mTCP[oldAddr].wlan;
                        MyDevice.mTCP[newAddr].devc = MyDevice.mTCP[oldAddr].devc;
                        MyDevice.mTCP[newAddr].para = MyDevice.mTCP[oldAddr].para;
                        MyDevice.mTCP[newAddr].alam = MyDevice.mTCP[oldAddr].alam;
                        MyDevice.mTCP[newAddr].work = MyDevice.mTCP[oldAddr].work;
                        MyDevice.mTCP[newAddr].fifo = MyDevice.mTCP[oldAddr].fifo;
                        break;

                    case COMP.XF:
                        MyDevice.mXF[oldAddr].sTATE = STATE.INVALID;
                        MyDevice.mXF[newAddr].sTATE = STATE.WORKING;
                        MyDevice.mXF[newAddr].opsn = MyDevice.mXF[oldAddr].opsn;
                        MyDevice.mXF[newAddr].snBat = MyDevice.mXF[oldAddr].snBat;
                        MyDevice.mXF[newAddr].torqueMultiple = MyDevice.mXF[oldAddr].torqueMultiple;
                        MyDevice.mXF[newAddr].angleMultiple = MyDevice.mXF[oldAddr].angleMultiple;
                        MyDevice.mXF[newAddr].auto = MyDevice.mXF[oldAddr].auto;
                        MyDevice.mXF[newAddr].wlan = MyDevice.mXF[oldAddr].wlan;
                        MyDevice.mXF[newAddr].devc = MyDevice.mXF[oldAddr].devc;
                        MyDevice.mXF[newAddr].para = MyDevice.mXF[oldAddr].para;
                        MyDevice.mXF[newAddr].alam = MyDevice.mXF[oldAddr].alam;
                        MyDevice.mXF[newAddr].work = MyDevice.mXF[oldAddr].work;
                        MyDevice.mXF[newAddr].fifo = MyDevice.mXF[oldAddr].fifo;
                        break;
                }
            }

            //发送指令
            if (MyDevice.protocol.type == COMP.UART)
            {
                //USB串口通讯固定地址01
                MyDevice.myTaskManager.AddUserCommand(1, ProtocolFunc.Protocol_Sequence_SendCOM, TASKS.REG_BLOCK3_WLAN, this.Name);
            }
            else
            {
                MyDevice.myTaskManager.AddUserCommand(oldAddr, ProtocolFunc.Protocol_Sequence_SendCOM, TASKS.REG_BLOCK3_WLAN, this.Name);
            }
        }

        //更新工厂设置
        private void btn_SuperUpdate_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("开发中...");
            //return;

            //按键状态
            btn_SuperUpdate.BackColor = Color.Firebrick;

            //多设备
            for (int j = 0; j < ucDataGridView1.SelectRows.Count; j++)
            {
                //获取已选设备地址
                if (MyDevice.protocol.type != COMP.UART)
                {
                    MyDevice.protocol.addr = mutiAddres[ucDataGridView1.SelectRows[j].RowIndex];
                    if (MyDevice.protocol.type == COMP.TCP)
                    {
                        MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
                    }
                    actXET = MyDevice.actDev;
                }

                /********更新参数***********/

                //更新内码相关参数(必须工厂权限)
                actXET.devc.calunit     = (UNIT)ucCombox_calUnit.SelectedIndex;
                actXET.devc.caltype  = (byte)ucCombox_calType.SelectedIndex;
                actXET.devc.capacity = Convert.ToInt32(ucCombox_capacity.SelectedText) * (int)Math.Pow(10, actXET.devc.torque_decimal);

                if (tb_adPosOutPut1.InputText != "") actXET.devc.ad_pos_point1 = Convert.ToInt32(tb_adPosOutPut1.InputText);
                if (tb_adPosOutPut2.InputText != "") actXET.devc.ad_pos_point2 = Convert.ToInt32(tb_adPosOutPut2.InputText);
                if (tb_adPosOutPut3.InputText != "") actXET.devc.ad_pos_point3 = Convert.ToInt32(tb_adPosOutPut3.InputText);
                if (tb_adPosOutPut4.InputText != "") actXET.devc.ad_pos_point4 = Convert.ToInt32(tb_adPosOutPut4.InputText);
                if (tb_adPosOutPut5.InputText != "") actXET.devc.ad_pos_point5 = Convert.ToInt32(tb_adPosOutPut5.InputText);
                if (tb_adNegOutPut1.InputText != "") actXET.devc.ad_neg_point1 = Convert.ToInt32(tb_adNegOutPut1.InputText);
                if (tb_adNegOutPut2.InputText != "") actXET.devc.ad_neg_point2 = Convert.ToInt32(tb_adNegOutPut2.InputText);
                if (tb_adNegOutPut3.InputText != "") actXET.devc.ad_neg_point3 = Convert.ToInt32(tb_adNegOutPut3.InputText);
                if (tb_adNegOutPut4.InputText != "") actXET.devc.ad_neg_point4 = Convert.ToInt32(tb_adNegOutPut4.InputText);
                if (tb_adNegOutPut5.InputText != "") actXET.devc.ad_neg_point5 = Convert.ToInt32(tb_adNegOutPut5.InputText);

                actXET.para.usbEn = (byte)((byte)ucCombox_usbEN.SelectedIndex == 0x00 ? 0x55 : 0x00);
                actXET.para.wirelessEn = (byte)((byte)ucCombox_wirelessEn.SelectedIndex == 0x00 ? 0x55 : 0x00);
                if (UInt16.TryParse(ucTextBoxEx_unhook.InputText, out UInt16 unhook))
                {
                    actXET.para.unhook = unhook;
                }
            }

            selectNum = 0;
            MyDevice.protocol.addr = mutiAddres[ucDataGridView1.SelectRows[selectNum].RowIndex];//从第一个设备开始设置
            if (MyDevice.protocol.type == COMP.TCP)
            {
                MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
            }
            actXET = MyDevice.actDev;
            buttonClicked = "bt_SuperUpdate";

            List<TASKS> tasks = new List<TASKS>
            {
                TASKS.REG_BLOCK3_PARA,
                TASKS.REG_BLOCK4_CAL1,
            };

            for (int i = 0; i < ucDataGridView1.SelectRows.Count; i++)
            {
                MyDevice.myTaskManager.AddUserCommands(mutiAddres[ucDataGridView1.SelectRows[i].RowIndex], ProtocolFunc.Protocol_Sequence_SendCOM, tasks, this.Name);
            }
        }

        //更新工单设置
        private void btn_UpdateTicket_Click(object sender, EventArgs e)
        {
            MessageBox.Show("开发中...");
            return;

            if (ucDataGridView1.SelectRows.Count == 0)
            {
                MessageBox.Show("未选择设备");
                return;
            }

            if (ucCombox_screwMax.TextValue == null || ucCombox_runMode.TextValue == null)
            {
                MessageBox.Show("离线工单基础设置未填写, 请检查所有参数", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //按键状态
            btn_UpdateTicket.BackColor = Color.Firebrick;

            //多设备
            for (int j = 0; j < ucDataGridView1.SelectRows.Count; j++)
            {
                //获取已选设备地址
                if (MyDevice.protocol.type != COMP.UART)
                {
                    MyDevice.protocol.addr = mutiAddres[ucDataGridView1.SelectRows[j].RowIndex];
                    if (MyDevice.protocol.type == COMP.TCP)
                    {
                        MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
                    }
                    actXET = MyDevice.actDev;

                }

                if (actXET.devc.type != TYPE.TQ_XH_XL01_07 - (UInt16)ADDROFFSET.TQ_XH_ADDR &&
                    actXET.devc.type != TYPE.TQ_XH_XL01_09 - (UInt16)ADDROFFSET.TQ_XH_ADDR)
                {
                    MessageBox.Show("离线工单只能指定07 / 09型号扳手设置");
                    return;
                }

                //离线工单基础设置
                actXET.para.screwmax = (byte)(ucCombox_screwMax.SelectedIndex + 1);
                actXET.para.runmode = (byte)ucCombox_runMode.SelectedIndex;

                //离线工单数值设置
                for (int i = 0; i < ucCombox_screwMax.SelectedIndex + 1; i++)
                {
                    actXET.screw[i].scw_ticketAxMx = GetTicketAxmx(dataGridView2.Rows[i].Cells[1].Value.ToString(), dataGridView2.Rows[i].Cells[2].Value.ToString());
                    actXET.screw[i].scw_ticketCnt = Convert.ToByte(dataGridView2.Rows[i].Cells[6].Value.ToString());
                    actXET.screw[i].scw_ticketNum = Convert.ToUInt32(dataGridView2.Rows[i].Cells[7].Value.ToString());
                    actXET.screw[i].scw_ticketSerial = Convert.ToUInt64(dataGridView2.Rows[i].Cells[8].Value.ToString()); ;
                }

            }

            selectNum = 0;
            MyDevice.protocol.addr = mutiAddres[ucDataGridView1.SelectRows[selectNum].RowIndex];//从第一个设备开始设置
            if (MyDevice.protocol.type == COMP.TCP)
            {
                MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
            }
            actXET = MyDevice.actDev;
            buttonClicked = "bt_UpdateTicket";

            List<TASKS> tasks = new List<TASKS>
            {
                TASKS.REG_BLOCK3_SCREW1,
                TASKS.REG_BLOCK3_SCREW2,
                TASKS.REG_BLOCK3_SCREW3,
                TASKS.REG_BLOCK3_SCREW4,
                TASKS.REG_BLOCK3_PARA,
            };

            for (int i = 0; i < ucDataGridView1.SelectRows.Count; i++)
            {
                MyDevice.myTaskManager.AddUserCommands(mutiAddres[ucDataGridView1.SelectRows[i].RowIndex], ProtocolFunc.Protocol_Sequence_SendCOM, tasks, this.Name);
            }
        }

        //单位切换更新数据表格——进行单位换算
        private void ucCombox1_SelectedChangedEvent(object sender, EventArgs e)
        {
            unit = ucCombox_torqueUnit.SelectedIndex;  //切换后的单位

            for (int i = 0; i < 10; i++)
            {
                dataGridView1.Rows[i + 1].Cells[1].Value = actXET.alam.SN_target[i, unit] / (float)actXET.torqueMultiple;
                dataGridView1.Rows[i + 1].Cells[2].Value = actXET.alam.SA_pre[i, unit] / (float)actXET.torqueMultiple;
                dataGridView1.Rows[i + 1].Cells[3].Value = actXET.alam.SA_ang[i] / (float)actXET.angleMultiple;
                dataGridView1.Rows[i + 1].Cells[4].Value = actXET.alam.MN_low[i, unit] / (float)actXET.torqueMultiple;
                dataGridView1.Rows[i + 1].Cells[5].Value = actXET.alam.MN_high[i, unit] / (float)actXET.torqueMultiple;
                dataGridView1.Rows[i + 1].Cells[6].Value = actXET.alam.MA_pre[i, unit] / (float)actXET.torqueMultiple;
                dataGridView1.Rows[i + 1].Cells[7].Value = actXET.alam.MA_low[i] / (float)actXET.angleMultiple;
                dataGridView1.Rows[i + 1].Cells[8].Value = actXET.alam.MA_high[i] / (float)actXET.angleMultiple;
            }

            //XH-06限定模式
            for (int i = 1; i < dataGridView1.Columns.Count; i++)
            {
                for (int j = 1; j < dataGridView1.Rows.Count; j++)
                {
                    if (dataGridView1[i, j].Value.ToString().Contains("-") || dataGridView1[i, j].Value.ToString() == "0")
                    {
                        dataGridView1[i, j].Value = "";
                        dataGridView1[i, j].ReadOnly = true;
                    }
                }
            }
        }

        //更改选择——控制表格编辑
        private void ucCombox6_SelectedChangedEvent(object sender, EventArgs e)
        {
            dataGridView1.ReadOnly = false;
            
            if (ucCombox_select.SelectedIndex == 0)
            {
                //不允许修改扳手报警值
                //SetEditableCells(dataGridView1, new List<int> { 1, 2, 3, 4, 5, 6, 7 , 8 }, new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
                ucCheckBox_SN.Visible = false;
                ucCheckBox_SA.Visible = false;
                ucCheckBox_MN.Visible = false;
                ucCheckBox_MA.Visible = false;
            }
            else
            {
                //允许扳手修改报警值，开防修改权限供客户
                ucCheckBox_SN.Visible = true;
                ucCheckBox_SA.Visible = true;
                ucCheckBox_MN.Visible = true;
                ucCheckBox_MA.Visible = true;
            }

            //switch (ucCombox_select.SelectedIndex)
            //{
            //    case 0://不可更改
            //        dataGridView1.ReadOnly = true;
            //        break;
            //    case 1://只允许更改SN
            //        if (actXET.devc.type == TYPE.TQ_XH_XL01_06 - (UInt16)ADDROFFSET.TQ_XH_ADDR)
            //        {
            //            SetEditableCells(dataGridView1, new List<int> { 1 }, new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            //        }
            //        else
            //        {
            //            SetEditableCells(dataGridView1, new List<int> { 1 }, new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            //        }
            //        break;
            //    case 2://只允许更改SA
            //        if (actXET.devc.type == TYPE.TQ_XH_XL01_06 - (UInt16)ADDROFFSET.TQ_XH_ADDR)
            //        {
            //            SetEditableCells(dataGridView1, new List<int> { 2, 3 }, new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            //        }
            //        else
            //        {
            //            SetEditableCells(dataGridView1, new List<int> { 2, 3 }, new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            //        }
            //        break;
            //    case 3://只允许更改MN
            //        if (actXET.devc.type == TYPE.TQ_XH_XL01_06 - (UInt16)ADDROFFSET.TQ_XH_ADDR)
            //        {
            //            SetEditableCells(dataGridView1, new List<int> { 4, 5 }, new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            //        }
            //        else
            //        {
            //            SetEditableCells(dataGridView1, new List<int> { 4, 5 }, new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            //        }
            //        break;
            //    case 4://只允许更改MA
            //        if (actXET.devc.type == TYPE.TQ_XH_XL01_06 - (UInt16)ADDROFFSET.TQ_XH_ADDR)
            //        {
            //            SetEditableCells(dataGridView1, new List<int> { 6, 7, 8 }, new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            //        }
            //        else
            //        {
            //            SetEditableCells(dataGridView1, new List<int> { 6, 7, 8 }, new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            //        }
            //        break;
            //    default:
            //        break;

            //}
        }

        //心跳回复帧数启动关闭
        private void ucCombox_heart_SelectedChangedEvent(object sender, EventArgs e)
        {
            if (ucCombox_heart.SelectedIndex > 0)
            {
                ucTextBoxEx_heartcount.Enabled = true;
            }
            else
            {
                ucTextBoxEx_heartcount.Enabled = false;
            }
        }

        //标定方式切换
        private void ucCombox_calType_SelectedChangedEvent(object sender, EventArgs e)
        {
            switch (ucCombox_calType.SelectedIndex)
            {
                case 0://五点标定
                    tb_adPosOutPut1.Visible = true;
                    tb_adPosOutPut2.Visible = true;
                    tb_adPosOutPut3.Visible = false;
                    tb_adPosOutPut4.Visible = false;
                    tb_adPosOutPut5.Visible = false;

                    tb_adPosOutPut1.Visible = true;
                    tb_adPosOutPut2.Visible = true;
                    tb_adNegOutPut3.Visible = false;
                    tb_adNegOutPut4.Visible = false;
                    tb_adNegOutPut5.Visible = false;
                    break;
                case 1://七点标定
                    tb_adPosOutPut1.Visible = true;
                    tb_adPosOutPut2.Visible = true;
                    tb_adPosOutPut3.Visible = true;
                    tb_adPosOutPut4.Visible = false;
                    tb_adPosOutPut5.Visible = false;

                    tb_adPosOutPut1.Visible = true;
                    tb_adPosOutPut2.Visible = true;
                    tb_adNegOutPut3.Visible = true;
                    tb_adNegOutPut4.Visible = false;
                    tb_adNegOutPut5.Visible = false;
                    break;
                case 2://九点标定
                    tb_adPosOutPut1.Visible = true;
                    tb_adPosOutPut2.Visible = true;
                    tb_adPosOutPut3.Visible = true;
                    tb_adPosOutPut4.Visible = true;
                    tb_adPosOutPut5.Visible = false;

                    tb_adPosOutPut1.Visible = true;
                    tb_adPosOutPut2.Visible = true;
                    tb_adNegOutPut3.Visible = true;
                    tb_adNegOutPut4.Visible = true;
                    tb_adNegOutPut5.Visible = false;
                    break;
                case 3://十一点标定
                    tb_adPosOutPut1.Visible = true;
                    tb_adPosOutPut2.Visible = true;
                    tb_adPosOutPut3.Visible = true;
                    tb_adPosOutPut4.Visible = true;
                    tb_adPosOutPut5.Visible = true;

                    tb_adNegOutPut1.Visible = true;
                    tb_adNegOutPut2.Visible = true;
                    tb_adNegOutPut3.Visible = true;
                    tb_adNegOutPut4.Visible = true;
                    tb_adNegOutPut5.Visible = true;
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region 表格输入限制

        //指定m列n行可编辑
        private void SetEditableCells(DataGridView dataGridView, List<int> editableColumnIndices, List<int> editableRowIndices)
        {
            // 遍历所有行和列
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                foreach (DataGridViewColumn col in dataGridView.Columns)
                {
                    DataGridViewCell cell = row.Cells[col.Index];

                    // 检查单元格是否在指定的可编辑列和行范围内
                    if (editableColumnIndices.Contains(col.Index) && editableRowIndices.Contains(row.Index))
                    {
                        cell.ReadOnly = false; // 设置单元格可编辑
                    }
                    else
                    {
                        cell.ReadOnly = true; // 设置单元格只读
                    }
                }
            }
        }

        //扭矩数据输入限制
        private void torque_KeyPress(object sender, KeyPressEventArgs e)
        {
            //只允许输入数字,小数点和删除键
            if (((e.KeyChar < '0') || (e.KeyChar > '9')) && (e.KeyChar != '.') && (e.KeyChar != 8))
            {
                e.Handled = true;
                return;
            }

            //小数点只能出现1位
            if ((e.KeyChar == '.') && ((DataGridViewTextBoxEditingControl)sender).Text.Contains("."))
            {
                e.Handled = true;
                return;
            }

            //第一位不能为小数点
            if ((e.KeyChar == '.') && (((DataGridViewTextBoxEditingControl)sender).Text.Length == 0))
            {
                e.Handled = true;
                return;
            }

            //扭矩含小数点长度限制5
            if ((e.KeyChar != 8) && ((TextBox)sender).Text.Length >= 5 && ((DataGridViewTextBoxEditingControl)sender).Text.Contains("."))
            {
                e.Handled = true;
                return;
            }

            //角度不含小数点长度限制4
            if ((e.KeyChar != 8) && ((TextBox)sender).Text.Length >= 4 && !((DataGridViewTextBoxEditingControl)sender).Text.Contains("."))
            {
                e.Handled = true;
                return;
            }
        }

        //角度数据输入限制
        private void angle_KeyPress(object sender, KeyPressEventArgs e)
        {
            //只允许输入数字,小数点和删除键
            if (((e.KeyChar < '0') || (e.KeyChar > '9')) && (e.KeyChar != '.') && (e.KeyChar != 8))
            {
                e.Handled = true;
                return;
            }

            //小数点只能出现1位
            if ((e.KeyChar == '.') && ((DataGridViewTextBoxEditingControl)sender).Text.Contains("."))
            {
                e.Handled = true;
                return;
            }

            //第一位不能为小数点
            if ((e.KeyChar == '.') && ((DataGridViewTextBoxEditingControl)sender).Text.Length == 0)
            {
                e.Handled = true;
                return;
            }

            //角度含小数点长度限制4
            if ((e.KeyChar != 8) && ((TextBox)sender).Text.Length >= 4 && ((DataGridViewTextBoxEditingControl)sender).Text.Contains("."))
            {
                e.Handled = true;
                return;
            }

            //角度不含小数点长度限制3
            if ((e.KeyChar != 8) && ((TextBox)sender).Text.Length >= 3 && !((DataGridViewTextBoxEditingControl)sender).Text.Contains("."))
            {
                e.Handled = true;
                return;
            }
        }

        //单元格限制
        private void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            CellEdit = (DataGridViewTextBoxEditingControl)e.Control;
            if (this.dataGridView1.CurrentCellAddress.X == 3 || this.dataGridView1.CurrentCellAddress.X == 7 || this.dataGridView1.CurrentCellAddress.X == 8)//获取当前处于活动状态的单元格索引
            {
                CellEdit.KeyPress += angle_KeyPress;
            }
            else
            {
                CellEdit.KeyPress += torque_KeyPress;
            }
        }

        //扭矩范围限制
        void SetTorqueAlarm(int i, int cellIndex, ref int[,] targetArray, int torqueMax, int torqueMin)
        {
            string cellValue = dataGridView1.Rows[i + 1].Cells[cellIndex].Value.ToString();
            if (cellValue != "")
            {
                int torqueAlarm = (int)(Convert.ToDouble(cellValue) * actXET.torqueMultiple + 0.5);//表格预设值
                int torqueToset = 0;//实际设置预设值

                if (torqueAlarm >= torqueMax)
                {
                    torqueToset = torqueMax;
                }
                else if (torqueAlarm <= torqueMin)
                {
                    torqueToset = torqueMin;
                }
                else
                {
                    torqueToset = torqueAlarm;

                }
                //根据单位调整上下限
                switch (actXET.para.torque_unit)
                {
                    case UNIT.UNIT_nm:
                        targetArray[i, 0] = UnitConvert.Torque_nmTrans(torqueToset, 0);
                        targetArray[i, 1] = UnitConvert.Torque_nmTrans(torqueToset, 1);
                        targetArray[i, 2] = UnitConvert.Torque_nmTrans(torqueToset, 2);
                        targetArray[i, 3] = UnitConvert.Torque_nmTrans(torqueToset, 3);
                        targetArray[i, 4] = UnitConvert.Torque_nmTrans(torqueToset, 4);
                        break;
                    case UNIT.UNIT_lbfin:
                        targetArray[i, 0] = UnitConvert.Torque_lbfinTrans(torqueToset, 0);
                        targetArray[i, 1] = UnitConvert.Torque_lbfinTrans(torqueToset, 1);
                        targetArray[i, 2] = UnitConvert.Torque_lbfinTrans(torqueToset, 2);
                        targetArray[i, 3] = UnitConvert.Torque_lbfinTrans(torqueToset, 3);
                        targetArray[i, 4] = UnitConvert.Torque_lbfinTrans(torqueToset, 4);
                        break;
                    case UNIT.UNIT_lbfft:
                        targetArray[i, 0] = UnitConvert.Torque_lbfftTrans(torqueToset, 0);
                        targetArray[i, 1] = UnitConvert.Torque_lbfftTrans(torqueToset, 1);
                        targetArray[i, 2] = UnitConvert.Torque_lbfftTrans(torqueToset, 2);
                        targetArray[i, 3] = UnitConvert.Torque_lbfftTrans(torqueToset, 3);
                        targetArray[i, 4] = UnitConvert.Torque_lbfftTrans(torqueToset, 4);
                        break;
                    case UNIT.UNIT_kgcm:
                        targetArray[i, 0] = UnitConvert.Torque_kgfcmTrans(torqueToset, 0);
                        targetArray[i, 1] = UnitConvert.Torque_kgfcmTrans(torqueToset, 1);
                        targetArray[i, 2] = UnitConvert.Torque_kgfcmTrans(torqueToset, 2);
                        targetArray[i, 3] = UnitConvert.Torque_kgfcmTrans(torqueToset, 3);
                        targetArray[i, 4] = UnitConvert.Torque_kgfcmTrans(torqueToset, 4);
                        break;
                    case UNIT.UNIT_kgm:
                        targetArray[i, 0] = UnitConvert.Torque_kgfmTrans(torqueToset, 0);
                        targetArray[i, 1] = UnitConvert.Torque_kgfmTrans(torqueToset, 1);
                        targetArray[i, 2] = UnitConvert.Torque_kgfmTrans(torqueToset, 2);
                        targetArray[i, 3] = UnitConvert.Torque_kgfmTrans(torqueToset, 3);
                        targetArray[i, 4] = UnitConvert.Torque_kgfmTrans(torqueToset, 4);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                targetArray[i, (int)actXET.para.torque_unit] = -1;
            }
        }

        //
        void SetTicketAlarm(int i, int cellIndex, ref int[,] targetArray, int torqueMultiple)
        {
            string cellValue = dataGridView2.Rows[i].Cells[cellIndex].Value.ToString();
            if (cellValue != "")
            {
                int ticketAlarm = (int)(Convert.ToDouble(cellValue) * torqueMultiple + 0.5);//表格预设值

                //根据单位调整上下限
                switch (actXET.para.torque_unit)
                {
                    case UNIT.UNIT_nm:
                        targetArray[i, 0] = UnitConvert.Torque_nmTrans(ticketAlarm, 0);
                        targetArray[i, 1] = UnitConvert.Torque_nmTrans(ticketAlarm, 1);
                        targetArray[i, 2] = UnitConvert.Torque_nmTrans(ticketAlarm, 2);
                        targetArray[i, 3] = UnitConvert.Torque_nmTrans(ticketAlarm, 3);
                        targetArray[i, 4] = UnitConvert.Torque_nmTrans(ticketAlarm, 4);
                        break;
                    case UNIT.UNIT_lbfin:
                        targetArray[i, 0] = UnitConvert.Torque_lbfinTrans(ticketAlarm, 0);
                        targetArray[i, 1] = UnitConvert.Torque_lbfinTrans(ticketAlarm, 1);
                        targetArray[i, 2] = UnitConvert.Torque_lbfinTrans(ticketAlarm, 2);
                        targetArray[i, 3] = UnitConvert.Torque_lbfinTrans(ticketAlarm, 3);
                        targetArray[i, 4] = UnitConvert.Torque_lbfinTrans(ticketAlarm, 4);
                        break;
                    case UNIT.UNIT_lbfft:
                        targetArray[i, 0] = UnitConvert.Torque_lbfftTrans(ticketAlarm, 0);
                        targetArray[i, 1] = UnitConvert.Torque_lbfftTrans(ticketAlarm, 1);
                        targetArray[i, 2] = UnitConvert.Torque_lbfftTrans(ticketAlarm, 2);
                        targetArray[i, 3] = UnitConvert.Torque_lbfftTrans(ticketAlarm, 3);
                        targetArray[i, 4] = UnitConvert.Torque_lbfftTrans(ticketAlarm, 4);
                        break;
                    case UNIT.UNIT_kgcm:
                        targetArray[i, 0] = UnitConvert.Torque_kgfcmTrans(ticketAlarm, 0);
                        targetArray[i, 1] = UnitConvert.Torque_kgfcmTrans(ticketAlarm, 1);
                        targetArray[i, 2] = UnitConvert.Torque_kgfcmTrans(ticketAlarm, 2);
                        targetArray[i, 3] = UnitConvert.Torque_kgfcmTrans(ticketAlarm, 3);
                        targetArray[i, 4] = UnitConvert.Torque_kgfcmTrans(ticketAlarm, 4);
                        break;
                    case UNIT.UNIT_kgm:
                        targetArray[i, 0] = UnitConvert.Torque_kgfmTrans(ticketAlarm, 0);
                        targetArray[i, 1] = UnitConvert.Torque_kgfmTrans(ticketAlarm, 1);
                        targetArray[i, 2] = UnitConvert.Torque_kgfmTrans(ticketAlarm, 2);
                        targetArray[i, 3] = UnitConvert.Torque_kgfmTrans(ticketAlarm, 3);
                        targetArray[i, 4] = UnitConvert.Torque_kgfmTrans(ticketAlarm, 4);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                targetArray[i, (int)actXET.para.torque_unit] = -1;
            }
        }

        #endregion

        #region 屏幕自适应

        //
        public static void setTag(Control cons)
        {
            foreach (Control con in cons.Controls)//循环窗体中的控件
            {
                con.Tag = con.Width + ":" + con.Height + ":" + con.Left + ":" + con.Top + ":" + con.Font.Size;
                if (con.Controls.Count > 0)
                    setTag(con);
            }
        }

        public static void ReWinformLayout(Rectangle control, float x, float y, Control cn)
        {
            var newx = control.Width / x;
            var newy = control.Height / y;
            if (newx == 0) { newx = 0.01f; }
            if (newy == 0) { newy = 0.01f; }
            setControls(newx, newy, cn);
        }

        public static void setControls(float newx, float newy, Control cons)
        {
            //遍历窗体中的控件，重新设置控件的值
            foreach (Control con in cons.Controls)
            {
                if (con.Tag == null)
                {
                    continue;
                }
                string[] mytag = con.Tag.ToString().Split(new char[] { ':' });//获取控件的Tag属性值，并分割后存储字符串数组
                float a = System.Convert.ToSingle(mytag[0]) * newx;//根据窗体缩放比例确定控件的值，宽度
                con.Width = (int)a;//宽度
                a = System.Convert.ToSingle(mytag[1]) * newy;//高度
                con.Height = (int)(a);
                a = System.Convert.ToSingle(mytag[2]) * newx;//左边距离
                con.Left = (int)(a);
                a = System.Convert.ToSingle(mytag[3]) * newy;//上边缘距离
                con.Top = (int)(a);
                Single currentSize = System.Convert.ToSingle(mytag[4]) * newy;//字体大小
                con.Font = new Font(con.Font.Name, currentSize, con.Font.Style, con.Font.Unit);
                if (con.Controls.Count > 0)
                {
                    setControls(newx, newy, con);
                }
            }
        }

        private void MenuDeviceSetForm_Resize(object sender, EventArgs e)
        {
            ReWinformLayout(this.ClientRectangle, x, y, this);

            //datagridview单元格特殊性，属于控件独立内容，需要额外设置自适应

            //设置列宽
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            //设置行高
            int height = 0;
            dataGridView1.ColumnHeadersHeight = dataGridView1.Height / 25 <= 4 ? 4 : dataGridView1.Height / 25;//dataGridView1.ColumnHeadersHeight 必须大于4，否则报错
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                dataGridView1.Rows[i].Height = (dataGridView1.Height - dataGridView1.ColumnHeadersHeight) / dataGridView1.RowCount;
                height += dataGridView1.Rows[i].Height;
            }
            dataGridView1.ColumnHeadersHeight = dataGridView1.Height - height <= 4 ? 4 : dataGridView1.Height - height;//dataGridView1.ColumnHeadersHeight 必须大于4，否则报错

            //设置datagridview2
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            //设置行高
            int height2 = 0;
            dataGridView2.ColumnHeadersHeight = dataGridView2.Height / 25 <= 4 ? 4 : dataGridView2.Height / 25;//dataGridView1.ColumnHeadersHeight 必须大于4，否则报错
            for (int i = 0; i < dataGridView2.RowCount; i++)
            {
                dataGridView2.Rows[i].Height = (dataGridView2.Height - dataGridView2.ColumnHeadersHeight) / dataGridView2.RowCount;
                height2 += dataGridView2.Rows[i].Height;
            }
            dataGridView2.ColumnHeadersHeight = dataGridView2.Height - height2 <= 4 ? 4 : dataGridView2.Height - height2;//dataGridView1.ColumnHeadersHeight 必须大于4，否则报错
        }

        #endregion

        //UI更新事件
        private void updateUI(object sender, UpdateUIEventArgs e)
        {
            Command currentCommand = e.Command;
            // 检查source是否与当前页面的name匹配
            if (currentCommand?.Source == this.Name)
            {
                if (buttonClicked == "bt_UpdatePara")
                {
                    switch (currentCommand.TaskState)
                    {
                        case TASKS.REG_BLOCK5_AM3:
                            updateDatabaseWrench();

                            selectNum++;
                            //所有设备接收
                            if (selectNum == ucDataGridView1.SelectRows.Count)
                            {
                                bt_UpdatePara.BackColor = Color.Green;
                                buttonClicked = "";

                                //更新工单对应的索引值（单位更改）
                                if (dataGridView2.InvokeRequired)
                                {
                                    dataGridView2.Invoke(new MethodInvoker(() =>
                                    {
                                        TicketInit();
                                    }));
                                }
                                else
                                {
                                    TicketInit();
                                }

                                //设置重复拧紧角度格式
                                switch (actXET.para.angle_decimal)
                                {
                                    case 0:
                                        ucTextBoxEx_angleResist.RegexPattern = @"^\d{0,7}$";
                                        break;
                                    case 1:
                                        ucTextBoxEx_angleResist.RegexPattern = @"^\d{0,7}(\.\d{0,1})?$";
                                        break;
                                    case 2:
                                        ucTextBoxEx_angleResist.RegexPattern = @"^\d{0,7}(\.\d{0,2})?$";
                                        break;
                                    case 3:
                                        ucTextBoxEx_angleResist.RegexPattern = @"^\d{0,7}(\.\d{0,3})?$";
                                        break;
                                    default:
                                        break;
                                }


                                if (ucTextBoxEx_angleResist.InvokeRequired)
                                {
                                    ucTextBoxEx_angleResist.Invoke(new MethodInvoker(() =>
                                    {
                                        ucTextBoxEx_angleResist.InputText = (actXET.para.angle_resist * 1.0 / (int)Math.Pow(10, actXET.para.angle_decimal)).ToString();
                                    }));
                                }
                                else
                                {
                                    ucTextBoxEx_angleResist.InputText = (actXET.para.angle_resist * 1.0 / (int)Math.Pow(10, actXET.para.angle_decimal)).ToString();
                                }
                                
                            }
                            else
                            {
                                //轮询下一台设备
                                MyDevice.protocol.addr = mutiAddres[ucDataGridView1.SelectRows[selectNum].RowIndex];//
                                if (MyDevice.protocol.type == COMP.TCP)
                                {
                                    MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
                                }
                                actXET = MyDevice.actDev;
                                actXET.torqueMultiple = (int)Math.Pow(10, actXET.devc.torque_decimal);
                                actXET.angleMultiple = (int)Math.Pow(10, actXET.para.angle_decimal);
                            }
                            break;
                        default:
                            break;
                    }
                }
                //模式设置
                else if (buttonClicked == "bt_UpdateMode")
                {
                    switch (currentCommand.TaskState)
                    {
                        case TASKS.REG_BLOCK3_PARA:
                            updateDatabaseWrench();

                            selectNum++;
                            //所有设备接收
                            if (selectNum == ucDataGridView1.SelectRows.Count)
                            {
                                bt_UpdateMode.BackColor = Color.Green;
                                buttonClicked = "";
                            }
                            else
                            {
                                //轮询下一台设备
                                MyDevice.protocol.addr = mutiAddres[ucDataGridView1.SelectRows[selectNum].RowIndex];//
                                if (MyDevice.protocol.type == COMP.TCP)
                                {
                                    MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
                                }
                                actXET = MyDevice.actDev;
                            }

                            break;
                        default:
                            break;
                    }
                }
                //WLAN设置
                else if (buttonClicked == "bt_UpdateWLAN")
                {
                    switch (currentCommand.TaskState)
                    {
                        case TASKS.REG_BLOCK3_WLAN:
                            updateDatabaseWrench();

                            bt_UpdateWLAN.BackColor = Color.Green;
                            mutiAddres[ucDataGridView1.SelectRows[selectNum].RowIndex] = actXET.wlan.addr;
                            actXET.wlan.wf_ip = "";
                            IPAddress ipAddress = IPAddress.Parse(ucTextBoxEx_wifiIp.InputText);
                            byte[] ipBytes = ipAddress.GetAddressBytes();
                            foreach (var item in ipBytes)
                            {
                                actXET.wlan.wf_ip += item.ToString("X2");
                            }

                            if (MyDevice.protocol.type == COMP.TCP)
                            {
                                //TCP改站点需要改变addr与ip的绑定
                                if (MyDevice.addr_ip.ContainsKey(oldDevAddr.ToString()) == true)
                                {
                                    string newKey = actXET.wlan.addr.ToString();
                                    string oldValue = MyDevice.addr_ip[oldDevAddr.ToString()];
                                    MyDevice.addr_ip.Remove(oldDevAddr.ToString());//移除旧key_value
                                    MyDevice.addr_ip.Add(newKey, oldValue);//添加新key_value
                                }
                            }


                            if (this.InvokeRequired)
                            {
                                this.Invoke(new MethodInvoker(() =>
                                {
                                    //修改扳手WLAN参数后，重新连接
                                    MenuDeviceSetForm_FormClosing(null, null);
                                    this.Hide();
                                    MenuConnectForm myConnectForm = new MenuConnectForm();
                                    myConnectForm.StartPosition = FormStartPosition.CenterParent;
                                    myConnectForm.ShowDialog();
                                    this.Close();
                                    myConnectForm.Dispose();
                                }));
                            }
                            else
                            {
                                //修改扳手WLAN参数后，重新连接
                                MenuDeviceSetForm_FormClosing(null, null);
                                this.Hide();
                                MenuConnectForm myConnectForm = new MenuConnectForm();
                                myConnectForm.StartPosition = FormStartPosition.CenterParent;
                                myConnectForm.ShowDialog();
                                this.Close();
                                myConnectForm.Dispose();
                            }

                            break;
                        default:
                            break;
                    }
                }
                //权限设置
                else if (buttonClicked == "bt_UpdateMemable")
                {
                    updateDatabaseWrench();

                    selectNum++;
                    //所有设备接收
                    if (selectNum == ucDataGridView1.Rows.Count)
                    {
                        buttonClicked = "";
                    }
                    else
                    {
                        //轮询下一台设备
                        MyDevice.protocol.addr = mutiAddres[ucDataGridView1.Rows[selectNum].RowIndex];//
                        if (MyDevice.protocol.type == COMP.TCP)
                        {
                            MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
                        }
                        actXET = MyDevice.actDev;
                        MyDevice.myTaskManager.AddUserCommand(MyDevice.protocol.addr, ProtocolFunc.Protocol_Write_SendCOM, TASKS.WRITE_MEMABLE, Convert.ToByte(MyDevice.userRole), this.Name);
                    }
                }
                //工厂设置
                else if (buttonClicked == "bt_SuperUpdate")
                {
                    switch (currentCommand.TaskState)
                    {
                        case TASKS.REG_BLOCK4_CAL1:
                            updateDatabaseWrench();

                            selectNum++;
                            //所有设备接收
                            if (selectNum == ucDataGridView1.SelectRows.Count)
                            {
                                btn_SuperUpdate.BackColor = Color.Green;
                                buttonClicked = "";
                            }
                            else
                            {
                                //轮询下一台设备
                                MyDevice.protocol.addr = mutiAddres[ucDataGridView1.SelectRows[selectNum].RowIndex];//
                                if (MyDevice.protocol.type == COMP.TCP)
                                {
                                    MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
                                }
                                actXET = MyDevice.actDev;
                            }
                            break;
                        default:
                            break;
                    }
                }
                //工单设置
                else if (buttonClicked == "bt_UpdateTicket")
                {
                    switch (currentCommand.TaskState)
                    {
                        case TASKS.REG_BLOCK3_PARA:

                            selectNum++;
                            //所有设备接收
                            if (selectNum == ucDataGridView1.SelectRows.Count)
                            {
                                btn_UpdateTicket.BackColor = Color.Green;
                                buttonClicked = "";
                            }
                            else
                            {
                                //轮询下一台设备
                                MyDevice.protocol.addr = mutiAddres[ucDataGridView1.SelectRows[selectNum].RowIndex];//
                                if (MyDevice.protocol.type == COMP.TCP)
                                {
                                    MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
                                }
                                actXET = MyDevice.actDev;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        //更新数据库
        private void updateDatabaseWrench()
        {
            //判断当前电脑是否启动数据库服务
            if (!GetComPuterInfo.ServiceIsRunning("MySQL", 21))
            {
                return;
            }
            else
            {
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
                    TimeOff = actXET.para.timeoff,
                    TimeBack = actXET.para.timeback,
                    TimeZero = actXET.para.timezero,
                    DispType = actXET.para.disptype,
                    DispTheme = actXET.para.disptheme,
                    DispLan = actXET.para.displan,
                    Unhook = actXET.para.unhook,
                    AdSpeed = actXET.para.adspeed,
                    AutoZero = actXET.para.autozero.ToString(),
                    TrackZero = actXET.para.trackzero.ToString(),
                    Amenable = actXET.para.amenable.ToString(),
                    Screwmax = actXET.para.screwmax,
                    Runmode = actXET.para.runmode,
                    Auploaden = actXET.para.auploaden,
                    AngCorr = actXET.para.torcorr.ToString(),
                    AngleResist = actXET.para.angle_resist,
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
                    WifiMode = actXET.wlan.wifimode,
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

        //统一设备参数
        private void UnifyDevPara()
        {
            if (ucDataGridView1.SelectRows.Count > 1)
            {
                for (int i = 0; i < ucDataGridView1.SelectRows.Count; i++)
                {
                    byte curAddr = mutiAddres[ucDataGridView1.SelectRows[i].RowIndex];
                    if (MyDevice.protocol.type == COMP.XF)
                    {
                        //如果有06扳手，参数必须向06靠拢，单位和小数点下标必须做出限制
                        if (MyDevice.mXF[curAddr].devc.type == TYPE.TQ_XH_XL01_06 - (UInt16)ADDROFFSET.TQ_XH_ADDR)
                        {
                            //06扳手只有1位小数点
                            ucCombox_point.SelectedIndex = 1;

                            //06扳手单位没有kgf·m
                            if (ucCombox_torqueUnit.SelectedIndex == 4)
                            {
                                ucCombox_torqueUnit.SelectedIndex = (byte)MyDevice.mXF[curAddr].para.torque_unit;
                            }
                        }
                    }
                    else
                    {
                        if (MyDevice.mTCP[curAddr].devc.type == TYPE.TQ_XH_XL01_06 - (UInt16)ADDROFFSET.TQ_XH_ADDR)
                        {
                            //06扳手只有1位小数点
                            ucCombox_point.SelectedIndex = 1;

                            //06扳手单位没有kgf·m
                            if (ucCombox_torqueUnit.SelectedIndex == 4)
                            {
                                ucCombox_torqueUnit.SelectedIndex = (byte)MyDevice.mTCP[curAddr].para.torque_unit;
                            }
                        }
                    }
                }
            }
        }
    
        //设置有效使能扳手按键修改报警值
        private ushort GetAlarmEnable()
        {
            ushort alarmEnable = 0;
            if (ucCheckBox_SN.Checked == true)
            {
                alarmEnable += 1;
            }
            if (ucCheckBox_SA.Checked == true)
            {
                alarmEnable += 2;
            }
            if ((ucCheckBox_MN.Checked == true))
            {
                alarmEnable += 4;
            }
            if ((ucCheckBox_MA.Checked == true))
            {
                alarmEnable += 8;
            }
            return alarmEnable;
        }

        //工单数量切换
        private void ucCombox_screwMax_SelectedChangedEvent(object sender, EventArgs e)
        {
            List<int> limitRowList = new List<int>();//限制输入行

            for (int i = 0; i < ucCombox_screwMax.SelectedIndex + 1; i++)
            {
                limitRowList.Add(i);
            }

            //根据用户选中工单数量开放权限
            for (int j = 0; j < ticketCntMax; j++)
            {
                // 获取指定行
                DataGridViewRow row2 = dataGridView2.Rows[j];

                if (j < ucCombox_screwMax.SelectedIndex + 1)
                {
                    // 将行背景颜色和前景颜色设为 DataGridView 的默认样式
                    row2.DefaultCellStyle.BackColor = dataGridView2.DefaultCellStyle.BackColor;
                    row2.DefaultCellStyle.ForeColor = dataGridView2.DefaultCellStyle.ForeColor;

                    // 遍历该行的所有单元格
                    foreach (DataGridViewCell cell in row2.Cells)
                    {
                        // 如果是 ComboBox 类型的单元格
                        if (cell is DataGridViewComboBoxCell comboBoxCell)
                        {
                            // 恢复 ComboBox 单元格的默认样式和显示下拉箭头
                            comboBoxCell.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing; // 隐藏下拉箭头

                            // 取消只读状态
                            comboBoxCell.ReadOnly = false;
                        }
                        else
                        {
                            // 如果是其他单元格，也恢复默认的只读状态（如果适用）
                            cell.ReadOnly = false;
                        }
                    }
                }
                else
                {
                    // 将背景颜色和前景颜色设为灰色
                    row2.DefaultCellStyle.BackColor = Color.LightGray;
                    row2.DefaultCellStyle.ForeColor = Color.DarkGray;

                    // 遍历该行的所有单元格
                    foreach (DataGridViewCell cell in row2.Cells)
                    {
                        // 如果是 ComboBox 类型的单元格
                        if (cell is DataGridViewComboBoxCell comboBoxCell)
                        {
                            // 将 ComboBox 单元格设为只读
                            comboBoxCell.ReadOnly = true;

                            // 设置 ComboBox 单元格的样式（前景和背景）
                            comboBoxCell.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing; // 隐藏下拉箭头
                        }
                        else
                        {
                            // 将其他单元格也设为只读（根据需要）
                            cell.ReadOnly = true;
                        }
                    }
                }
            }


            //报警值可编辑，只能从扳手预设值引用
            SetEditableCells(dataGridView2, new List<int> { 1, 2, 6, 7, 8 }, limitRowList);
        }

        // 订阅 DataGridView2 的 EditingControlShowing 事件
        private void dataGridView2_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            // 判断当前编辑的列是否是 DataGridViewComboBoxColumn
            if ((dataGridView2.CurrentCell.ColumnIndex == 1 || dataGridView2.CurrentCell.ColumnIndex == 2) && e.Control is ComboBox comboBox)
            {
                // 移除之前可能已经订阅的事件，防止事件被多次触发
                comboBox.SelectedIndexChanged -= ComboBox_SelectedIndexChanged;

                // 订阅 ComboBox 的 SelectedIndexChanged 事件
                comboBox.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
            }
        }

        // 离线工单AxMx切换同步更改索引值
        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 获取当前正在编辑的 ComboBox
            ComboBox comboBox = sender as ComboBox;

            // 获取当前选择的索引和内容
            int selectedIndex = comboBox.SelectedIndex;
            string selectedText = comboBox.Text;

            // 获取当前行索引
            int rowIndex = dataGridView2.CurrentCell.RowIndex;

            // 获取 Cell[1] 和 Cell[2] 的值，进行 null 检查，否则null直接赋值会报错
            string strAx = string.Empty;

            string strMx = string.Empty;

            int targetAx = 0;
            int targetMx = 0;
            int targetAxMx = 0;

            if (dataGridView2.CurrentCell.ColumnIndex == 1)
            {
                strAx = comboBox.Text;
                strMx = dataGridView2.Rows[rowIndex].Cells[2].Value != null
                                ? dataGridView2.Rows[rowIndex].Cells[2].Value.ToString()
                                : string.Empty;
            }
            else if (dataGridView2.CurrentCell.ColumnIndex == 2)
            {
                strAx = dataGridView2.Rows[rowIndex].Cells[1].Value != null
                                ? dataGridView2.Rows[rowIndex].Cells[1].Value.ToString()
                                : string.Empty;
                strMx = comboBox.Text;
            }

            switch (strAx)
            {
                case "SN":
                    targetAx = 2;
                    break;
                case "SA":
                    targetAx = 3;
                    break;
                case "MN":
                    targetAx = 4;
                    break;
                case "MA":
                    targetAx = 5;
                    break;
                default:
                    break;
            }
            switch (strMx)
            {
                case "M0":
                    targetMx = 0;
                    break;
                case "M1":
                    targetMx = 1;
                    break;
                case "M2":
                    targetMx = 2;
                    break;
                case "M3":
                    targetMx = 3;
                    break;
                case "M4":
                    targetMx = 4;
                    break;
                case "M5":
                    targetMx = 5;
                    break;
                case "M6":
                    targetMx = 6;
                    break;
                case "M7":
                    targetMx = 7;
                    break;
                case "M8":
                    targetMx = 8;
                    break;
                case "M9":
                    targetMx = 9;
                    break;
                default:
                    break;
            }

            targetAxMx = targetAx * 16 + targetMx;

            if (dataGridView2.CurrentCell.ColumnIndex == 1 || dataGridView2.CurrentCell.ColumnIndex == 2)
            {
                switch (targetAxMx)
                {
                    case 0x20:
                    case 0x21:
                    case 0x22:
                    case 0x23:
                    case 0x24:
                    case 0x25:
                    case 0x26:
                    case 0x27:
                    case 0x28:
                    case 0x29:
                        dataGridView2.Rows[rowIndex].Cells[3].Value = actXET.alam.SN_target[targetMx, (int)actXET.para.torque_unit] / (float)actXET.torqueMultiple;
                        dataGridView2.Rows[rowIndex].Cells[4].Value = "";
                        dataGridView2.Rows[rowIndex].Cells[5].Value = "";
                        break;
                    case 0x30:
                    case 0x31:
                    case 0x32:
                    case 0x33:
                    case 0x34:
                    case 0x35:
                    case 0x36:
                    case 0x37:
                    case 0x38:
                    case 0x39:
                        dataGridView2.Rows[rowIndex].Cells[3].Value = actXET.alam.SA_pre[targetMx, (int)actXET.para.torque_unit] / (float)actXET.torqueMultiple;
                        dataGridView2.Rows[rowIndex].Cells[4].Value = actXET.alam.SA_ang[targetMx] / (float)actXET.angleMultiple;
                        dataGridView2.Rows[rowIndex].Cells[5].Value = "";
                        break;
                    case 0x40:
                    case 0x41:
                    case 0x42:
                    case 0x43:
                    case 0x44:
                    case 0x45:
                    case 0x46:
                    case 0x47:
                    case 0x48:
                    case 0x49:
                        dataGridView2.Rows[rowIndex].Cells[3].Value = actXET.alam.MN_low[targetMx, (int)actXET.para.torque_unit] / (float)actXET.torqueMultiple;
                        dataGridView2.Rows[rowIndex].Cells[4].Value = actXET.alam.MN_high[targetMx, (int)actXET.para.torque_unit] / (float)actXET.torqueMultiple;
                        dataGridView2.Rows[rowIndex].Cells[5].Value = "";
                        break;
                    case 0x50:
                    case 0x51:
                    case 0x52:
                    case 0x53:
                    case 0x54:
                    case 0x55:
                    case 0x56:
                    case 0x57:
                    case 0x58:
                    case 0x59:
                        dataGridView2.Rows[rowIndex].Cells[3].Value = actXET.alam.MA_pre[targetMx, (int)actXET.para.torque_unit] / (float)actXET.torqueMultiple;
                        dataGridView2.Rows[rowIndex].Cells[4].Value = actXET.alam.MA_low[targetMx] / (float)actXET.angleMultiple;
                        dataGridView2.Rows[rowIndex].Cells[5].Value = actXET.alam.MA_high[targetMx] / (float)actXET.angleMultiple;
                        break;
                    default:
                        break;
                }
            }
        }

        //配置选择
        private void ucCombox_wirelessSelection_SelectedChangedEvent(object sender, EventArgs e)
        {
            //IP地址格式校验
            string ipPattern1 = @"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\." +
               @"(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\." +
               @"(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\." +
               @"(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";

            if (ucCombox_wirelessSelection.SelectedIndex == 0)
            {
                label_wifiIp.Text = "接收器 IP:";
                label_ssid.Text = "接收器WiFi名称:";
                label_pwd.Text = "接收器WiFi密码:";

                label_tip.Visible = false;

                try
                {
                    ucTextBoxEx_wifiIp.InputText = label_recIP.Text.Split(' ')[1];
                    ucTextBoxEx_port.InputText = label_recPort.Text.Split(' ')[1];
                    ucTextBoxEx_ssid.InputText = label_recWIFIName.Text.Split(' ')[1];
                    ucTextBoxEx_pwd.InputText = label_recWIFIPwd.Text.Split(' ')[1];

                    if (!Regex.IsMatch(ucTextBoxEx_wifiIp.InputText, ipPattern1))
                    {
                        MessageBox.Show("IP格式不正确，请重新输入，示例192.168.1.1");
                        ucTextBoxEx_wifiIp.InputText = "192.168.1.1";
                        ucTextBoxEx_port.InputText = "";
                        ucTextBoxEx_ssid.InputText = "";
                        ucTextBoxEx_pwd.InputText = "";
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("请重新输入参数");
                    ucTextBoxEx_wifiIp.InputText = "";
                    ucTextBoxEx_port.InputText = "";
                    ucTextBoxEx_ssid.InputText = "";
                    ucTextBoxEx_pwd.InputText = "";
                }

            }
            else if (ucCombox_wirelessSelection.SelectedIndex == 1)
            {
                label_wifiIp.Text = "本地WiFi IP:";
                label_ssid.Text = "本地WiFi名称:";
                label_pwd.Text = "本地WiFi密码:";

                label_tip.Visible = true;

                try
                {
                    ucTextBoxEx_wifiIp.InputText = label_curIP.Text.Split(' ')[1];
                    ucTextBoxEx_port.InputText = "5678";
                    ucTextBoxEx_ssid.InputText = label_curWIFIName.Text.Split(' ')[1];
                    ucTextBoxEx_pwd.InputText = actXET.wlan.wf_pwd != "12345678" ? actXET.wlan.wf_pwd : "";

                    if (!Regex.IsMatch(ucTextBoxEx_wifiIp.InputText, ipPattern1))
                    {
                        MessageBox.Show("IP格式不正确，请重新输入，示例192.168.1.1");
                        ucTextBoxEx_wifiIp.InputText = "192.168.1.1";
                        ucTextBoxEx_port.InputText = "";
                        ucTextBoxEx_ssid.InputText = "";
                        ucTextBoxEx_pwd.InputText = "";
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("请重新输入参数");
                    ucTextBoxEx_wifiIp.InputText = "";
                    ucTextBoxEx_port.InputText = "";
                    ucTextBoxEx_ssid.InputText = "";
                    ucTextBoxEx_pwd.InputText = "";
                }
            }
        }

        //工单数值初始化
        private void TicketInit()
        {
            //获取扳手单位
            byte ticketUnit = (byte)actXET.para.torque_unit;

            //获取工单实际数值
            //工单数值初始化
            for (int i = 0; i < ticketCntMax; i++)
            {
                // 获取特定单元格
                DataGridViewComboBoxCell comboBoxCell_Ax = (DataGridViewComboBoxCell)dataGridView2.Rows[i].Cells[1];
                DataGridViewComboBoxCell comboBoxCell_Mx = (DataGridViewComboBoxCell)dataGridView2.Rows[i].Cells[2];
                int ticketAx = actXET.screw[i].scw_ticketAxMx >> 0x04;
                int ticketMx = actXET.screw[i].scw_ticketAxMx & 0x0F;

                //Ax —— 取一个字节的高4位
                switch (ticketAx)
                {
                    case 0://EN
                        break;
                    case 1://EA
                        break;
                    case 2://SN
                        // 检查值是否在Items列表中
                        if (comboBoxCell_Ax.Items.Contains("SN"))
                        {
                            comboBoxCell_Ax.Value = "SN"; // 赋值
                        }
                        break;
                    case 3://SA
                        // 检查值是否在Items列表中
                        if (comboBoxCell_Ax.Items.Contains("SA"))
                        {
                            comboBoxCell_Ax.Value = "SA"; // 赋值
                        }
                        break;
                    case 4://MN
                        // 检查值是否在Items列表中
                        if (comboBoxCell_Ax.Items.Contains("MN"))
                        {
                            comboBoxCell_Ax.Value = "MN"; // 赋值
                        }
                        break;
                    case 5://MA
                        // 检查值是否在Items列表中
                        if (comboBoxCell_Ax.Items.Contains("MA"))
                        {
                            comboBoxCell_Ax.Value = "MA"; // 赋值
                        }
                        break;
                    case 6://AZ
                        break;
                    default:
                        break;
                }

                //Mx —— 取一个字节的低4位
                switch (ticketMx)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                        // 检查值是否在Items列表中
                        if (comboBoxCell_Mx.Items.Contains($"M{ticketMx}"))
                        {
                            comboBoxCell_Mx.Value = $"M{ticketMx}"; // 赋值
                        }
                        break;
                    default:
                        break;
                }

                //索引扳手的报警值
                switch (actXET.screw[i].scw_ticketAxMx)
                {
                    case 0x20:
                    case 0x21:
                    case 0x22:
                    case 0x23:
                    case 0x24:
                    case 0x25:
                    case 0x26:
                    case 0x27:
                    case 0x28:
                    case 0x29:
                        dataGridView2.Rows[i].Cells[3].Value = actXET.alam.SN_target[ticketMx, ticketUnit] / (float)actXET.torqueMultiple;
                        dataGridView2.Rows[i].Cells[4].Value = "";
                        dataGridView2.Rows[i].Cells[5].Value = "";
                        break;
                    case 0x30:
                    case 0x31:
                    case 0x32:
                    case 0x33:
                    case 0x34:
                    case 0x35:
                    case 0x36:
                    case 0x37:
                    case 0x38:
                    case 0x39:
                        dataGridView2.Rows[i].Cells[3].Value = actXET.alam.SA_pre[ticketMx, ticketUnit] / (float)actXET.torqueMultiple;
                        dataGridView2.Rows[i].Cells[4].Value = actXET.alam.SA_ang[ticketMx] / (float)actXET.angleMultiple;
                        dataGridView2.Rows[i].Cells[5].Value = "";
                        break;
                    case 0x40:
                    case 0x41:
                    case 0x42:
                    case 0x43:
                    case 0x44:
                    case 0x45:
                    case 0x46:
                    case 0x47:
                    case 0x48:
                    case 0x49:
                        dataGridView2.Rows[i].Cells[3].Value = actXET.alam.MN_low[ticketMx, ticketUnit] / (float)actXET.torqueMultiple;
                        dataGridView2.Rows[i].Cells[4].Value = actXET.alam.MN_high[ticketMx, ticketUnit] / (float)actXET.torqueMultiple;
                        dataGridView2.Rows[i].Cells[5].Value = "";
                        break;
                    case 0x50:
                    case 0x51:
                    case 0x52:
                    case 0x53:
                    case 0x54:
                    case 0x55:
                    case 0x56:
                    case 0x57:
                    case 0x58:
                    case 0x59:
                        dataGridView2.Rows[i].Cells[3].Value = actXET.alam.MA_pre[ticketMx, ticketUnit] / (float)actXET.torqueMultiple;
                        dataGridView2.Rows[i].Cells[4].Value = actXET.alam.MA_low[ticketMx] / (float)actXET.angleMultiple;
                        dataGridView2.Rows[i].Cells[5].Value = actXET.alam.MA_high[ticketMx] / (float)actXET.angleMultiple;
                        break;
                    default:
                        break;
                }

                dataGridView2.Rows[i].Cells[6].Value = actXET.screw[i].scw_ticketCnt;
                dataGridView2.Rows[i].Cells[7].Value = actXET.screw[i].scw_ticketNum;
                dataGridView2.Rows[i].Cells[8].Value = actXET.screw[i].scw_ticketSerial;
            }
        }

        //获取工单的AxMx模式(通过Ax 与 Mx 高低位拼接)
        private byte GetTicketAxmx(string Ax, string Mx)
        {
            byte ax = 0;
            byte mx = 0;
            byte AxMx = 0;
            switch (Ax)
            {
                case "SN":
                    ax = 2;
                    break;
                case "SA":
                    ax = 3;
                    break;
                case "MN":
                    ax = 4;
                    break;
                case "MA":
                    ax = 5;
                    break;
                default:
                    break;
            }
            switch (Mx)
            {
                case "M0":
                    mx = 0;
                    break;
                case "M1":
                    mx = 1;
                    break;
                case "M2":
                    mx = 2;
                    break;
                case "M3":
                    mx = 3;
                    break;
                case "M4":
                    mx = 4;
                    break;
                case "M5":
                    mx = 5;
                    break;
                case "M6":
                    mx = 6;
                    break;
                case "M7":
                    mx = 7;
                    break;
                case "M8":
                    mx = 8;
                    break;
                case "M9":
                    mx = 9;
                    break;
                default:
                    break;
            }

            AxMx = (byte)(ax * 16 + mx);

            return AxMx;
        }

        //单元格限制
        private void dataGridView2_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            string headerText = dataGridView2.Columns[e.ColumnIndex].HeaderText;
            string input = e.FormattedValue.ToString();

            if (headerText == "螺栓数量")
            {
                if (!int.TryParse(input, out int value) || value < 1 || value > 255)
                {
                    e.Cancel = true;//强制焦点在此，不合格无法进行其他输入
                    MessageBox.Show("螺栓数量限制在 1 - 255，请规范输入");
                    isInputValid = false;
                }
                else
                {
                    isInputValid = true;
                }
            }
            else if (headerText == "工单号")
            {
                if (!long.TryParse(input, out long _) || input.Length > 9)
                {
                    e.Cancel = true;
                    MessageBox.Show("请输入不超过9位的数字");
                    isInputValid = false;
                }
                else
                {
                    isInputValid = true;
                }
            }
            else if (headerText == "序列号")
            {
                if (!long.TryParse(input, out long _) || input.Length > 13)
                {
                    e.Cancel = true;
                    MessageBox.Show("请输入不超过13位的数字");
                    isInputValid = false;
                }
                else
                {
                    isInputValid = true;
                }
            }
        }

        //单元格回车后不合格内容改成默认值
        private void dataGridView2_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            string headerText = dataGridView2.Columns[e.ColumnIndex].HeaderText;
            string input = dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();

            if (headerText == "螺栓数量")
            {
                if (!isInputValid)
                {
                    dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 10;
                }
            }
            else if (headerText == "工单号")
            {
                if (!isInputValid)
                {
                    if (input.Length > 9 && Regex.IsMatch(input, @"^\d+$"))
                    {
                        dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = input.Substring(0, 9);
                    }
                    else
                    {
                        dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "8888";
                    }
                }
            }
            else if (headerText == "序列号")
            {
                if (!isInputValid)
                {
                    if (input.Length > 13 && Regex.IsMatch(input, @"^\d+$"))
                    {
                        dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = input.Substring(0, 13);
                    }
                    else
                    {
                        dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "88888888";
                    }
                }
            }
        }

    }
}
