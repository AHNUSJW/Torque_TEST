using DBHelper;
using HZH_Controls.Controls;
using Library;
using Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Windows.Forms;

//Ricardo 20240709

namespace Base.UI.MenuDevice
{
    //专项用于XH-08系列RS485通讯
    public partial class MenuDeviceSetForm2 : Form
    {
        private readonly float x; //定义当前窗体的宽度
        private readonly float y; //定义当前窗体的高度

        private XET actXET;       //当前设备
        private TASKS meTask;     //按键操作指令

        private int unit;         //设备扭矩单位
        private List<Byte> mutiAddres = new List<Byte>();         //存储已连接设备的地址

        private DataGridViewTextBoxEditingControl CellEdit = null;//单元格

        private string buttonClicked = "";    //记录按下的按钮,区分按下的是预设值还是模式设置
        private int selectNum = 0;            //轮询发送指令的扳手下标
        private byte oldDevAddr = 1;          //改站点之前的旧站点
        private byte oldBaud = 1;             //旧设备波特率
        private byte newBaud = 1;             //新设备波特率

        public class GridModel
        {
            public string Device { get; set; }
        }

        public MenuDeviceSetForm2()
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
            dataGridView1.Rows[0].Cells[0].Value = "扭矩报警值";
            dataGridView1.Rows[0].Cells[1].Value = "最小显示扭矩值";
            dataGridView1.Rows[0].Cells[2].Value = "角度停止扭矩值";
            dataGridView1.Rows[0].Cells[3].Value = "扭矩报警值";

            //模式数据列初始化
            dataGridView1.Rows.Add();

            //行首与列首均禁止编辑
            dataGridView1.Rows[0].ReadOnly = true;
            dataGridView1.Columns[0].ReadOnly = true;

            //设置列宽
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            //设置行高
            int height = 0;
            dataGridView1.ColumnHeadersHeight = dataGridView1.Height / 3;
            dataGridView1.Rows[0].Height = (dataGridView1.Height - dataGridView1.ColumnHeadersHeight) / 2;
            dataGridView1.Rows[1].Height = (dataGridView1.Height - dataGridView1.ColumnHeadersHeight) / 2;

            #endregion

            //参数combox和textbox初始化
            Parameters_Init();

            //权限控制
            RoleBasedUIChange();

            //依据扳手型号调整控件状态
            TypeBasedUIChange();        

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
                    else if (MyDevice.protocol.type == COMP.RS485)
                    {
                        if (MyDevice.mRS[i].sTATE == STATE.WORKING)
                        {
                            //切换扳手权限
                            MyDevice.myTaskManager.AddUserCommand(i, ProtocolFunc.Protocol_Write_SendCOM, TASKS.WRITE_MEMABLE, Convert.ToByte(MyDevice.userRole), this.Name);
                        }
                    }
                }
            }

            oldBaud = (byte)ucCombox_baud.SelectedIndex;//刷新旧波特率
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
            ucCombox_point.SelectedIndex = 1;

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
                    break;
                case TYPE.TQ_XH_XL01_08 - (UInt16)ADDROFFSET.TQ_XH_ADDR:
                    AxMode.Add(new KeyValuePair<string, string>("2", " SN "));
                    AxMode.Add(new KeyValuePair<string, string>("6", " AZ "));
                    break;
            }

            ucCombox_modeAx.Source = AxMode;
            if (actXET.para.mode_ax == 2)
            {
                ucCombox_modeAx.SelectedIndex = 0;//SN
            }
            else
            {
                ucCombox_modeAx.SelectedIndex = 1;//AZ
            }

            //Mx模式--只有M0           

            //更改选择
            ucCombox_select.Source = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("0", " 不允许手动更改 "),
                new KeyValuePair<string, string>("1", " 允许手动更改SN "),
                new KeyValuePair<string, string>("2", " 允许手动更改AZ "),
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

            //脱钩保持时间
            ucTextBoxEx_unhook.InputText = actXET.para.unhook.ToString();

            #endregion

            #region WLAN设置

            //站点地址
            ucTextBoxEx_addr.InputText = actXET.wlan.addr.ToString();

            //RS485波特率
            ucCombox_baud.Source = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("0", " 2400 "),
                new KeyValuePair<string, string>("1", " 4800 "),
                new KeyValuePair<string, string>("2", " 9600 "),
                new KeyValuePair<string, string>("3", " 14400 "),
                new KeyValuePair<string, string>("4", " 19200 "),
                new KeyValuePair<string, string>("5", " 38400 "),
                new KeyValuePair<string, string>("6", " 57600 "),
                new KeyValuePair<string, string>("7", " 115200 ")
            };
            ucCombox_baud.SelectedIndex = actXET.wlan.rs485_baud - 1;//-1是由于还存在一个波特率1200，暂时不开放

            //RS485停止位
            ucCombox_stopbit.Source = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("0", " 1 "),
                new KeyValuePair<string, string>("1", " 2 ")
            };
            ucCombox_stopbit.SelectedIndex = actXET.wlan.rs485_stopbit - 1;

            //RS485校验位
            ucCombox_parity.Source = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("0", " None "),
                new KeyValuePair<string, string>("1", " Odd(奇校验) "),
                new KeyValuePair<string, string>("2", " Even(偶校验) "),
                new KeyValuePair<string, string>("3", " Mark "),
                new KeyValuePair<string, string>("4", " Space ")
            };
            ucCombox_parity.SelectedIndex = actXET.wlan.rs485_parity;

            //WiFi/RF无线
            ucCombox_wifimode.Source = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("0", "关闭"),
                new KeyValuePair<string, string>("1", "开启")
            };
            ucCombox_wifimode.SelectedIndex = actXET.wlan.wifimode;

            #endregion
        }

        //依据权限调整控件状态
        private void RoleBasedUIChange()
        {
            //预设值设置
            switch (MyDevice.userRole)
            {
                case "0":
                    label_point.Visible = false;
                    ucCombox_point.Visible = false;

                    label_select.Enabled = false;
                    ucCombox_select.Enabled = false;
                    break;
                case "1":
                    label_point.Visible = false;
                    ucCombox_point.Visible = false;

                    label_select.Enabled = true;
                    ucCombox_select.Enabled = true;
                    break;
                default:
                case "32":
                    label_point.Visible = true;
                    ucCombox_point.Visible = true;

                    label_select.Enabled = true;
                    ucCombox_select.Enabled = true;
                    break;
            }
            //模式设置
            switch (MyDevice.userRole)
            {
                case "0":
                    groupBox6.Visible = false;
                    groupBox6.Visible = false;

                    label_fifomode.Enabled = false;
                    ucCombox_fifomode.Enabled = false;
                    label_fiforec.Enabled = false;
                    ucCombox_fiforec.Enabled = false;
                    label_fifospeed.Enabled = false;
                    ucCombox_fifospeed.Enabled = false;
                    label_heart.Enabled = false;
                    ucCombox_heart.Enabled = false;
                    label_heartcount.Enabled = false;
                    ucTextBoxEx_heartcount.Enabled = false;
                    label_heartcycle.Enabled = false;
                    ucTextBoxEx_heartcycle.Enabled = false;
                    label_accmode.Enabled = false;
                    ucCombox_accmode.Enabled = false;
                    label_alarmode.Enabled = false;
                    ucCombox_alarmode.Enabled = false;
                    label_wifimode.Enabled = false;
                    ucCombox_wifimode.Enabled = false;
                    label_timeoff.Enabled = false;
                    ucTextBoxEx_timeoff.Enabled = false;
                    label_timeback.Enabled = false;
                    ucTextBoxEx_timeback.Enabled = false;
                    label_timezero.Enabled = false;
                    ucTextBoxEx_timezero.Enabled = false;
                    label_disptype.Enabled = false;
                    ucCombox_disptype.Enabled = false;
                    label_disptheme.Enabled = false;
                    ucCombox_disptheme.Enabled = false;
                    label_displan.Enabled = false;
                    ucCombox_displan.Enabled = false;

                    bt_UpdateMode.Enabled = false;
                    break;
                case "1":
                    groupBox6.Visible = false;
                    groupBox6.Visible = false;

                    label_fifomode.Enabled = true;
                    ucCombox_fifomode.Enabled = true;
                    label_fiforec.Enabled = true;
                    ucCombox_fiforec.Enabled = true;
                    label_fifospeed.Enabled = true;
                    ucCombox_fifospeed.Enabled = true;
                    label_heart.Enabled = true;
                    ucCombox_heart.Enabled = true;
                    label_heartcount.Enabled = true;
                    ucTextBoxEx_heartcount.Enabled = true;
                    label_heartcycle.Enabled = true;
                    ucTextBoxEx_heartcycle.Enabled = true;
                    label_accmode.Enabled = true;
                    ucCombox_accmode.Enabled = true;
                    label_alarmode.Enabled = true;
                    ucCombox_alarmode.Enabled = true;
                    label_wifimode.Enabled = true;
                    ucCombox_wifimode.Enabled = true;
                    label_timeoff.Enabled = true;
                    ucTextBoxEx_timeoff.Enabled = true;
                    label_timeback.Enabled = true;
                    ucTextBoxEx_timeback.Enabled = true;
                    label_timezero.Enabled = true;
                    ucTextBoxEx_timezero.Enabled = true;
                    label_disptype.Enabled = true;
                    ucCombox_disptype.Enabled = false;
                    label_disptheme.Enabled = true;
                    ucCombox_disptheme.Enabled = true;
                    label_displan.Enabled = true;
                    ucCombox_displan.Enabled = true;

                    bt_UpdateMode.Enabled = true;
                    break;
                default:
                case "32":
                    groupBox6.Visible = true;
                    groupBox6.Visible = true;

                    label_fifomode.Enabled = true;
                    ucCombox_fifomode.Enabled = true;
                    label_fiforec.Enabled = true;
                    ucCombox_fiforec.Enabled = true;
                    label_fifospeed.Enabled = true;
                    ucCombox_fifospeed.Enabled = true;
                    label_heart.Enabled = true;
                    ucCombox_heart.Enabled = true;
                    label_heartcount.Enabled = true;
                    ucTextBoxEx_heartcount.Enabled = true;
                    label_heartcycle.Enabled = true;
                    ucTextBoxEx_heartcycle.Enabled = true;
                    label_accmode.Enabled = true;
                    ucCombox_accmode.Enabled = true;
                    label_alarmode.Enabled = true;
                    ucCombox_alarmode.Enabled = true;
                    label_wifimode.Enabled = true;
                    ucCombox_wifimode.Enabled = true;
                    label_timeoff.Enabled = true;
                    ucTextBoxEx_timeoff.Enabled = true;
                    label_timeback.Enabled = true;
                    ucTextBoxEx_timeback.Enabled = true;
                    label_timezero.Enabled = true;
                    ucTextBoxEx_timezero.Enabled = true;
                    label_disptype.Enabled = true;
                    ucCombox_disptype.Enabled = false;
                    label_disptheme.Enabled = true;
                    ucCombox_disptheme.Enabled = false;
                    label_displan.Enabled = true;
                    ucCombox_displan.Enabled = false;

                    bt_UpdateMode.Enabled = true;
                    break;
            }
            //WLAN设置
            switch (MyDevice.userRole)
            {
                case "32":
                case "1":
                    label_addr.Enabled = true;
                    ucTextBoxEx_addr.Enabled = true;
                    bt_UpdateWLAN.Enabled = true;

                    label_baud.Enabled = true;
                    label_stopbit.Enabled = true;
                    label_parity.Enabled = true;
                    ucCombox_baud.Enabled = true;
                    ucCombox_stopbit.Enabled = true;
                    ucCombox_parity.Enabled = true;

                    label_wifimode.Visible = false;
                    ucCombox_wifimode.Visible = false;
                    break;
                default:
                case "0":
                    label_addr.Enabled = false;
                    ucTextBoxEx_addr.Enabled = false;
                    bt_UpdateWLAN.Enabled = false;

                    label_baud.Enabled = false;
                    label_stopbit.Enabled = false;
                    label_parity.Enabled = false;
                    ucCombox_baud.Enabled = false;
                    ucCombox_stopbit.Enabled = false;
                    ucCombox_parity.Enabled = false;

                    label_wifimode.Visible = false;
                    ucCombox_wifimode.Visible = false;
                    break;
            }
        }

        //依据扳手型号调整控件状态
        private void TypeBasedUIChange()
        {
            //预设值设置
            switch (actXET.devc.type)
            {
                case TYPE.TQ_XH_XL01_08 - (UInt16)ADDROFFSET.TQ_XH_ADDR:
                    //08单位固定Nm，角度固定一位
                    ucCombox_point.Enabled = false;
                    ucCombox_torqueUnit.Enabled = false;
                    break;
            }

            //WLAN设置
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

                //根据已连接设备数新增"设备"行 MyDevice.devSum
                for (int i = 0; i < MyDevice.devSum; i++)
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

                oldBaud = (byte)ucCombox_baud.SelectedIndex;//刷新旧波特率
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
                actXET.para.mode_ax = (byte)(ucCombox_modeAx.SelectedIndex == 0 ? 2 : 6);
                actXET.para.mode_mx = 0;
                actXET.para.angle_decimal = (byte)ucCombox_point.SelectedIndex;
                actXET.torqueMultiple = (int)Math.Pow(10, actXET.devc.torque_decimal);
                actXET.angleMultiple = (int)Math.Pow(10, actXET.para.angle_decimal);

                int torqueMax = actXET.devc.torque_max;//量程上限
                int torqueMin = actXET.devc.torque_min;//量程下限
                int torqueDisp = actXET.devc.torque_disp;//最小显示扭矩值(AZ模式常用)

                //单位转换
                switch (actXET.devc.calunit)
                {
                    //根据不同的标定单位获取指定单位的峰值
                    //标定单位：actXET.devc.unit 指设备初次标定时所用的单位，基准是min = 150, max = 3000
                    //指定单位：actXET.para.torque_unit，设备使用时选择的单位
                    case UNIT.UNIT_nm:
                        torqueMax = UnitConvert.Torque_nmTrans(torqueMax, (byte)actXET.para.torque_unit);
                        torqueMin = UnitConvert.Torque_nmTrans(torqueMin, (byte)actXET.para.torque_unit);
                        torqueDisp = UnitConvert.Torque_nmTrans(torqueDisp, (byte)actXET.para.torque_unit);
                        break;
                    case UNIT.UNIT_lbfin:
                        torqueMax = UnitConvert.Torque_lbfinTrans(torqueMax, (byte)actXET.para.torque_unit);
                        torqueMin = UnitConvert.Torque_lbfinTrans(torqueMin, (byte)actXET.para.torque_unit);
                        torqueDisp = UnitConvert.Torque_lbfinTrans(torqueDisp, (byte)actXET.para.torque_unit);
                        break;
                    case UNIT.UNIT_lbfft:
                        torqueMax = UnitConvert.Torque_lbfftTrans(torqueMax, (byte)actXET.para.torque_unit);
                        torqueMin = UnitConvert.Torque_lbfftTrans(torqueMin, (byte)actXET.para.torque_unit);
                        torqueDisp = UnitConvert.Torque_lbfftTrans(torqueDisp, (byte)actXET.para.torque_unit);
                        break;
                    case UNIT.UNIT_kgcm:
                        torqueMax = UnitConvert.Torque_kgfcmTrans(torqueMax, (byte)actXET.para.torque_unit);
                        torqueMin = UnitConvert.Torque_kgfcmTrans(torqueMin, (byte)actXET.para.torque_unit);
                        torqueDisp = UnitConvert.Torque_kgfcmTrans(torqueDisp, (byte)actXET.para.torque_unit);
                        break;
                    case UNIT.UNIT_kgm:
                        torqueMax = UnitConvert.Torque_kgfmTrans(torqueMax, (byte)actXET.para.torque_unit);
                        torqueMin = UnitConvert.Torque_kgfmTrans(torqueMin, (byte)actXET.para.torque_unit);
                        torqueDisp = UnitConvert.Torque_kgfmTrans(torqueDisp, (byte)actXET.para.torque_unit);
                        break;
                    default:
                        break;
                }

                //报警值
                SetTorqueAlarm(0, 0, ref actXET.alam.SN_target, torqueMax, torqueMin);
                SetTorqueAlarm(0, 1, ref actXET.alam.AZ_start, torqueMax, torqueDisp);
                SetTorqueAlarm(0, 2, ref actXET.alam.AZ_stop, torqueMax, torqueDisp);
                SetTorqueAlarm(0, 3, ref actXET.alam.AZ_hock, torqueMax, torqueDisp);

                //AZ报警值限制 按照 AZ_start < AZ_stop < AZ_hock
                if (actXET.alam.AZ_hock[0, (int)actXET.devc.calunit] < actXET.alam.AZ_stop[0, (int)actXET.devc.calunit])
                {
                    int temp = actXET.alam.AZ_hock[0, (int)actXET.devc.calunit];
                    actXET.alam.AZ_hock[0, (int)actXET.devc.calunit] = actXET.alam.AZ_stop[0, (int)actXET.devc.calunit];
                    actXET.alam.AZ_stop[0, (int)actXET.devc.calunit] = temp;
                }
                if (actXET.alam.AZ_hock[0, (int)actXET.devc.calunit] < actXET.alam.AZ_start[0, (int)actXET.devc.calunit])
                {
                    int temp = actXET.alam.AZ_hock[0, (int)actXET.devc.calunit];
                    actXET.alam.AZ_hock[0, (int)actXET.devc.calunit] = actXET.alam.AZ_start[0, (int)actXET.devc.calunit];
                    actXET.alam.AZ_start[0, (int)actXET.devc.calunit] = temp;
                }
                if (actXET.alam.AZ_stop[0, (int)actXET.devc.calunit] < actXET.alam.AZ_start[0, (int)actXET.devc.calunit])
                {
                    int temp = actXET.alam.AZ_stop[0, (int)actXET.devc.calunit];
                    actXET.alam.AZ_stop[0, (int)actXET.devc.calunit] = actXET.alam.AZ_start[0, (int)actXET.devc.calunit];
                    actXET.alam.AZ_start[0, (int)actXET.devc.calunit] = temp;
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

            if (ucTextBoxEx_heartcount.InputText == ""
                || ucTextBoxEx_heartcycle.InputText == ""
                || ucTextBoxEx_timeoff.InputText == ""
                || ucTextBoxEx_timeback.InputText == ""
                || ucTextBoxEx_timezero.InputText == ""
                || ucTextBoxEx_unhook.InputText == "")
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
                actXET.para.disptype = (byte)ucCombox_disptype.SelectedIndex;
                actXET.para.disptheme = (byte)ucCombox_disptheme.SelectedIndex;
                actXET.para.displan = (byte)ucCombox_displan.SelectedIndex;
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
            buttonClicked = "bt_UpdateMode";

            List<TASKS> tasks = new List<TASKS>
            {
                TASKS.REG_BLOCK3_PARA
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
            if (ucTextBoxEx_addr.InputText == "")
            {
                MessageBox.Show("有参数未填写, 请检查所有参数", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

            //RS485配置
            actXET.wlan.rs485_baud = (byte)(ucCombox_baud.SelectedIndex + 1);
            actXET.wlan.rs485_stopbit = (byte)(ucCombox_stopbit.SelectedIndex + 1);
            actXET.wlan.rs485_parity = (byte)ucCombox_parity.SelectedIndex;
            actXET.wlan.wifimode = (byte)ucCombox_wifimode.SelectedIndex;

            actXET.torqueMultiple = (int)Math.Pow(10, actXET.devc.torque_decimal);
            actXET.angleMultiple = (int)Math.Pow(10, actXET.para.angle_decimal);

            //新波特率
            newBaud = (byte)ucCombox_baud.SelectedIndex;

            if (newAddr != oldAddr)
            {
                //站点更改，对应的参数更新，同时状态更新
                switch (MyDevice.protocol.type)
                {
                    default:
                    case COMP.UART:
                        break;
                    case COMP.RS485:
                        MyDevice.mRS[oldAddr].sTATE = STATE.INVALID;
                        MyDevice.mRS[newAddr].sTATE = STATE.WORKING;
                        MyDevice.mRS[newAddr].opsn = MyDevice.mRS[oldAddr].opsn;
                        MyDevice.mRS[newAddr].snBat = MyDevice.mRS[oldAddr].snBat;
                        MyDevice.mRS[newAddr].torqueMultiple = MyDevice.mRS[oldAddr].torqueMultiple;
                        MyDevice.mRS[newAddr].angleMultiple = MyDevice.mRS[oldAddr].angleMultiple;
                        MyDevice.mRS[newAddr].auto = MyDevice.mRS[oldAddr].auto;
                        MyDevice.mRS[newAddr].wlan = MyDevice.mRS[oldAddr].wlan;
                        MyDevice.mRS[newAddr].devc = MyDevice.mRS[oldAddr].devc;
                        MyDevice.mRS[newAddr].para = MyDevice.mRS[oldAddr].para;
                        MyDevice.mRS[newAddr].alam = MyDevice.mRS[oldAddr].alam;
                        MyDevice.mRS[newAddr].work = MyDevice.mRS[oldAddr].work;
                        MyDevice.mRS[newAddr].fifo = MyDevice.mRS[oldAddr].fifo;
                        break;
                }
            }

            //发送指令
            if (MyDevice.protocol.type == COMP.UART)
            {
                //USB串口通讯固定地址01
                MyDevice.myTaskManager.AddUserCommand(1, ProtocolFunc.Protocol_Sequence_SendCOM, TASKS.REG_BLOCK3_WLAN, this.Name);
                //修改波特率需要发送重启串口指令
                MyDevice.myTaskManager.AddUserCommand(1, ProtocolFunc.Protocol_Write_SendCOM, TASKS.WRITE_RESET, this.Name);
            }
            else
            {
                MyDevice.myTaskManager.AddUserCommand(actXET.wlan.addr, ProtocolFunc.Protocol_Sequence_SendCOM, TASKS.REG_BLOCK3_WLAN, this.Name);
                //修改波特率需要发送重启串口指令
                MyDevice.myTaskManager.AddUserCommand(actXET.wlan.addr, ProtocolFunc.Protocol_Write_SendCOM, TASKS.WRITE_RESET, this.Name);
            }
        }

        //单位切换更新数据表格——进行单位换算
        private void ucCombox1_SelectedChangedEvent(object sender, EventArgs e)
        {
            unit = ucCombox_torqueUnit.SelectedIndex;  //切换后的单位

            dataGridView1.Rows[1].Cells[0].Value = actXET.alam.SN_target[0, unit] / (float)actXET.torqueMultiple;
            dataGridView1.Rows[1].Cells[1].Value = actXET.alam.AZ_start[0, unit] / (float)actXET.torqueMultiple;
            dataGridView1.Rows[1].Cells[2].Value = actXET.alam.AZ_stop[0, unit] / (float)actXET.torqueMultiple;
            dataGridView1.Rows[1].Cells[3].Value = actXET.alam.AZ_hock[0, unit] / (float)actXET.torqueMultiple;
        }

        //更改选择——控制表格编辑
        private void ucCombox6_SelectedChangedEvent(object sender, EventArgs e)
        {
            dataGridView1.ReadOnly = false;

            switch (ucCombox_select.SelectedIndex)
            {
                case 0://不可更改
                    dataGridView1.ReadOnly = true;
                    break;
                case 1://只允许更改SN
                    SetEditableCells(dataGridView1, new List<int> { 0 }, new List<int> { 1 });
                    break;
                case 2://只允许更改AZ
                    SetEditableCells(dataGridView1, new List<int> { 1, 2, 3 }, new List<int> { 1 });
                    break;             
                default:
                    break;

            }
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
            CellEdit.KeyPress += torque_KeyPress;
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
                }
                //WLAN设置
                else if (buttonClicked == "bt_UpdateWLAN")
                {
                    switch (currentCommand.TaskState)
                    {
                        case TASKS.WRITE_RESET:
                            updateDatabaseWrench();

                            bt_UpdateWLAN.BackColor = Color.Green;
                            mutiAddres[ucDataGridView1.SelectRows[selectNum].RowIndex] = actXET.wlan.addr;

                            if (newBaud != oldBaud)
                            {
                                MessageBox.Show("设备波特率已修改成功，需要重新连接", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                                //波特率修改设备状态重置
                                actXET.sTATE = STATE.INVALID;

                                //关闭当前页面
                                if (this.InvokeRequired)
                                {
                                    this.Invoke(new Action(() => this.Close()));
                                }
                                else
                                {
                                    this.Close();
                                }
                                
                                //重启连接页面
                                MenuConnectForm menuConnectForm = new MenuConnectForm();
                                menuConnectForm.StartPosition = FormStartPosition.CenterScreen;
                                menuConnectForm.ShowDialog();
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
                    AngCorr = actXET.para.angcorr.ToString(),
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
                    Parity = actXET.wlan.rs485_parity,
                    WifiMode = actXET.wlan.wifimode,
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
    
    }
}
