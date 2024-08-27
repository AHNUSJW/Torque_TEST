using Model;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MaciX.UI.MyControl
{
    public partial class MutiDevice485 : UserControl
    {
        private float dataFontSize = 36;//数值字体大小
        private float unitFontSize = 18;//单位字体大小
        private float addrFontSize = 12.75f;//地址字体大小
        private Color dataColor = Color.Black;//数值颜色
        private Color[] lampColor = new Color[] { Color.Black, Color.Green };//指示灯颜色

        private XET actXET;//需要操作的设备
        private string dataValue = "";//数值
        private string weightValue = "";//毛重净重数值
        private string unitValue = "";//单位
        private string outypeValue = "";//输出类型
        private string s_outTypeValue = "";//模拟量输出，actXET.S_OutType
        private string addressValue = "";//地址

        public EventHandler SetZero;//定义归零事件
        public EventHandler SetTare;//定义去皮事件

        [Description("数值字体大小"), Category("自定义")]
        public float DataFontSize
        {
            get
            {
                return dataFontSize;
            }
            set
            {
                dataFontSize = value;
                signalOutput1.Font = new Font(signalOutput1.Font.FontFamily, dataFontSize, signalOutput1.Font.Style);
            }
        }

        [Description("单位字体大小"), Category("自定义")]
        public float UnitFontSize
        {
            get
            {
                return unitFontSize;
            }
            set
            {
                unitFontSize = value;
                signalUnit1.Font = new Font(signalUnit1.Font.FontFamily, unitFontSize, signalUnit1.Font.Style);
            }
        }

        [Description("地址字体大小"), Category("自定义")]
        public float AddrFontSize
        {
            get
            {
                return addrFontSize;
            }
            set
            {
                addrFontSize = value;
                address.Font = new Font(address.Font.FontFamily, addrFontSize, address.Font.Style);
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

        [Description("毛重净重数值"), Category("自定义")]
        public string Weight
        {
            get
            {
                return weightValue;
            }
            set
            {
                weightValue = value;
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
            set
            {
                outypeValue = value;
            }
        }

        [Description("模拟量输出类型"), Category("自定义")]
        public string S_Outype
        {
            get
            {
                return s_outTypeValue;
            }
            set
            {
                s_outTypeValue = value;
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

        //构造函数
        public MutiDevice485()
        {
            InitializeComponent();
        }

        //初始化
        private void MutiDevice485_Load(object sender, EventArgs e)
        {
            //需要操作的设备
            actXET = MyDevice.actDev;

            //模拟量输出类型
            switch (actXET.S_OutType)
            {
                default:
                case OUT.UT420:
                    s_outTypeValue = "UT420";
                    break;

                case OUT.UTP05:
                    s_outTypeValue = "UTP05";
                    break;

                case OUT.UTP10:
                    s_outTypeValue = "UTP10";
                    break;

                case OUT.UTN05:
                    s_outTypeValue = "UTN05";
                    break;

                case OUT.UTN10:
                    s_outTypeValue = "UTN10";
                    break;

                case OUT.UMASK:
                    s_outTypeValue = "UMASK";
                    break;
            }
        }
    }
}
