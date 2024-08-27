using Library;
using Model;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

//Lumi 20231128

namespace MaciX.UI.MyControl
{
    public partial class MutiDevice : UserControl
    {
        private XET actXET;//需要操作的设备
        private string dataValue = "";//数值
        private string unitValue = "";//单位
        private string outypeValue = "";//输出类型
        private string addressValue = "";//地址
        private Color dataColor = Color.Black;//数值颜色
        private Color[] lampColor = new Color[] { Color.Black, Color.Green };//指示灯颜色

        private AutoFormSize autoFormSize = new AutoFormSize();//自适应屏幕分辨率

        public EventHandler SetZero;//定义归零事件
        public EventHandler SetTare;//定义去皮事件

        [Description("数值"), Category("自定义")]
        public string Data
        {
            get
            {
                return dataValue;
            }
            set
            {
                dataValue = value;
                signalOutput1.Text = dataValue;
            }
        }

        [Description("单位"), Category("自定义")]
        public string Unit
        {
            get
            {
                return unitValue;
            }
            set
            {
                unitValue = value;
                signalUnit1.Text = unitValue;
            }
        }

        [Description("输出类型"), Category("自定义")]
        public string Outype
        {
            get
            {
                return outypeValue;
            }
        }

        [Description("地址"), Category("自定义")]
        public string Address
        {
            get
            {
                return addressValue;
            }
            set
            {
                if (value == "")
                {
                    return;
                }
                addressValue = value;
                address.Text = "ID: " + addressValue;
            }
        }

        [Description("数值颜色"), Category("自定义")]
        public Color DataColor
        {
            get
            {
                return dataColor;
            }
            set
            {
                dataColor = value;
                signalOutput1.ForeColor = dataColor;
            }
        }

        [Description("灯颜色，当需要闪烁时，至少需要2个及以上颜色，不需要闪烁则至少需要1个颜色"), Category("自定义")]
        public Color[] LampColor
        {
            get
            {
                return lampColor;
            }
            set
            {
                if (value == null || value.Length <= 0)
                {
                    return;
                }

                lampColor = value;
                ucSignalLamp1.LampColor = lampColor;
            }
        }

        //构造函数
        public MutiDevice()
        {
            InitializeComponent();
        }

        //初始化
        private void MutiDevice_Load(object sender, EventArgs e)
        {
            //布局随分辨率变化
            autoFormSize.UIComponetForm(this);

            //需要操作的设备
            actXET = MyDevice.actDev;

            //按键可见性
            if(actXET.S_DeviceType == TYPE.T8X420H)
            {
                buttonX2.Visible = false;
            }
            else
            {
                buttonX2.Visible = true;
            }

            //初始化comboBox和address
            comboBox1.Items.Clear();
            if (actXET.S_DeviceType == TYPE.T4X600H || actXET.S_DeviceType == TYPE.T8X420H)
            {
                comboBox1.Items.Add("模拟量");
            }
            if (actXET.S_DeviceType != TYPE.T8X420H)
            {
                comboBox1.Items.Add("毛重");
                comboBox1.Items.Add("净重");
            }
            comboBox1.SelectedIndex = 0;
        }

        //归零
        private void buttonX1_Click(object sender, EventArgs e)
        {
            //指定要归零的设备地址
            if (MyDevice.actDev.S_DeviceType == TYPE.T8X420H)   //该型号没有SCT5
            {
                if (byte.TryParse(addressValue, out byte addr))
                {
                    MyDevice.protocol.addr = addr;
                }
                else
                {
                    MyDevice.protocol.addr = actXET.E_addr;
                }
            }
            else
            {
                MyDevice.protocol.addr = actXET.E_addr;
            }

            SetZero(sender, e);
        }

        //去皮
        private void buttonX2_Click(object sender, EventArgs e)
        {
            //指定需要去皮的设备地址
            MyDevice.protocol.addr = actXET.E_addr;

            SetTare(sender, e);
        }

        //净毛重状态
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //获取输出类型
            outypeValue = comboBox1.Text;

            if (comboBox1.Text == "模拟量")
            {
                ucSignalLamp1.Visible = false;

                if (MyDevice.protocol.trTASK == TASKS.ADC)
                {
                    //先停止连续dacout避免读SCT0零点内码接收字节被淹没
                    MyDevice.mePort_StopDacout(MyDevice.protocol);
                }

                //单位
                switch (actXET.S_OutType)
                {
                    case OUT.UT420:
                        signalUnit1.Text = "mA";
                        break;

                    case OUT.UTP05:
                    case OUT.UTP10:
                    case OUT.UTN05:
                    case OUT.UTN10:
                        signalUnit1.Text = "V";
                        break;

                    default:
                        signalUnit1.Text = "";
                        break;
                }
            }
            else
            {
                ucSignalLamp1.Visible = true;

                //获取单位
                switch ((UNIT)actXET.E_wt_unit)
                {
                    case UNIT.kg: signalUnit1.Text = "kg"; break;
                    case UNIT.lb: signalUnit1.Text = "lb"; break;
                    case UNIT.oz: signalUnit1.Text = "oz"; break;
                    case UNIT.g: signalUnit1.Text = "g"; break;
                    case UNIT.mg: signalUnit1.Text = "mg"; break;
                    case UNIT.t: signalUnit1.Text = "t"; break;
                    case UNIT.ct: signalUnit1.Text = "ct"; break;
                    case UNIT.N: signalUnit1.Text = "N"; break;
                    case UNIT.kN: signalUnit1.Text = "kN"; break;
                    case UNIT.mvdv: signalUnit1.Text = "mV/V"; break;
                    case UNIT.内码: signalUnit1.Text = "内码"; break;
                    default: signalUnit1.Text = ""; break;
                }
            }
        }

        //双击进入单元设备显示窗口
        private void MutiDevice_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //指定设备地址
            if (MyDevice.actDev.S_DeviceType == TYPE.T8X420H)   //该型号没有SCT5
            {
                if (byte.TryParse(addressValue, out byte addr))
                {
                    MyDevice.protocol.addr = addr;
                }
                else
                {
                    MyDevice.protocol.addr = actXET.E_addr;
                }
            }
            else
            {
                MyDevice.protocol.addr = actXET.E_addr;
            }

            //进入单元设备显示窗口
            Main.isMeasure = true;
            Main.callDelegate();
        }

        //双击进入单元设备显示窗口
        private void signalOutput1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //指定设备地址
            if (MyDevice.actDev.S_DeviceType == TYPE.T8X420H)   //该型号没有SCT5
            {
                if (byte.TryParse(addressValue, out byte addr))
                {
                    MyDevice.protocol.addr = addr;
                }
                else
                {
                    MyDevice.protocol.addr = actXET.E_addr;
                }
            }
            else
            {
                MyDevice.protocol.addr = actXET.E_addr;
            }

            //进入单元设备显示窗口
            Main.isMeasure = true;
            Main.callDelegate();
        }

        //分辨率变换
        private void MutiDevice_Resize(object sender, EventArgs e)
        {
            if (Screen.PrimaryScreen.Bounds.Height < 1080 || Screen.PrimaryScreen.Bounds.Width < 1920)
            {
                autoFormSize.UIComponetForm_Resize(this, 2);
                this.comboBox1.Font = new System.Drawing.Font("宋体", buttonX1.Height / 2, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            }
        }
    }
}
