using DBHelper;
using System;
using System.Windows.Forms;

namespace Base.UI.MenuHomework
{
    public partial class WrenchInfoForm : Form
    {
        private uint wid;
        private DSWrenchDevc wrenchDevc = new DSWrenchDevc();
        private DSWrenchPara wrenchPara = new DSWrenchPara();
        private DSWrenchWork wrenchWork = new DSWrenchWork();
        private DSWrenchWlan wrenchWlan = new DSWrenchWlan();

        public uint Wid { get => wid; set => wid = value; }

        public WrenchInfoForm()
        {
            InitializeComponent();
        }

        private void WrenchInfoForm_Load(object sender, EventArgs e)
        {
            //查询工单表
            wrenchDevc = JDBC.GetWrenchDevcByWid(wid);
            wrenchWlan = JDBC.GetWrenchWlanByWlanId(wrenchDevc.WlanId);

            //扭矩值根据小数点需要调整的倍数
            int torqueMultiple = (int)Math.Pow(10, wrenchDevc.TorqueDecimal);

            //更新扳手信息表
            label21.Text = wrenchDevc.Series.ToString();
            label22.Text = wrenchDevc.Type.ToString();
            label23.Text = wrenchDevc.Version.ToString();
            label24.Text = wrenchDevc.BohrCode.ToString();
            label25.Text = wrenchDevc.Unit.ToString();
            label26.Text = wrenchDevc.TorqueDecimal.ToString();
            label27.Text = wrenchDevc.TorqueFdn.ToString();
            label28.Text = (1.0 * wrenchDevc.Capacity / torqueMultiple).ToString();
            label29.Text = (1.0 * wrenchDevc.TorqueDisp / torqueMultiple).ToString();
            label30.Text = (1.0 * wrenchDevc.TorqueMin / torqueMultiple).ToString();
            label31.Text = (1.0 * wrenchDevc.TorqueMax / torqueMultiple).ToString();
            label32.Text = (1.0 * wrenchDevc.TorqueOver / torqueMultiple).ToString();

            //更新扳手参数表
            label_baud.Text    = wrenchWlan.Baud.ToString();
            label_stopbit.Text = wrenchWlan.Stopbit.ToString();
            label_parity.Text  = wrenchWlan.Parity.ToString();
            label_WiFiIP.Text  = wrenchWlan.WFIp;
            label_WIFIPort.Text = wrenchWlan.WFPort.ToString();
            label_WIFISsid.Text = wrenchWlan.WFSsid.ToString();
            label_WIFIPwd.Text = wrenchWlan.WFPwd.ToString();

            //存储数据实际转换成真实数据，例如波特率存储的byte字节,实际展示的值是115200

            //WiFi IP
            string ip = null;
            for (int i = 0; i < wrenchWlan.WFIp.Length; i += 2)
            {
                int num = Convert.ToInt32($"{wrenchWlan.WFIp[i]}{wrenchWlan.WFIp[i + 1]}", 16);
                ip = i == 0 ? num.ToString() : ip + "." + num.ToString();
            }
            label_WiFiIP.Text = ip;

            //
            label_baud.Text = GetBaud(wrenchWlan.Baud);
            label_stopbit.Text = GetStopBit(wrenchWlan.Stopbit).ToString();
            label_parity.Text = GetParity(wrenchWlan.Parity);
        }

        private String GetBaud(byte baud)
        {
            string showBaud = "115200";

            switch (baud)
            {
                case 0:
                    showBaud = "1200";
                    break;
                case 1:
                    showBaud = "2400";
                    break;
                case 2:
                    showBaud = "4800";
                    break;
                case 3:
                    showBaud = "9600";
                    break;
                case 4:
                    showBaud = "14400";
                    break;
                case 5:
                    showBaud = "19200";
                    break;
                case 6:
                    showBaud = "38400";
                    break;
                case 7:
                    showBaud = "57600";
                    break;
                case 8:
                    showBaud = "115200";
                    break;
                case 255:
                    showBaud = "115200";
                    break;
                default:
                    break;
            }
            return showBaud;
        }

        private byte GetStopBit(byte stopbit)
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

        private string GetParity(byte parity)
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
                default:
                    break;
            }

            return showParity;
        }
    }
}
