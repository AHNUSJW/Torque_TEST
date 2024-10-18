using DBHelper;
using Library;
using Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Base.UI.MenuHomework
{
    public partial class TicketInfoForm : Form
    {
        private string ticketImagePath;//工单图片路径
        private bool isCreate = true;//是否是新建（区分新建工单和修改工单）
        private uint workId;//工单号（用于修改工单对应的工单号）
        private List<DSTicketInfo> allTicketInfo = new List<DSTicketInfo>();//工单总表

        public String reportInfoPath;//报告信息文件路径
        public String scanWorkNum = "";//扫码枪扫描提供的工单号
        private int ticketNumLen = 4;//工单号限定长度
        private int sequenceLen = 6;//序列号限定长度
        private int ticketEncodeLen = 10;//工单编码限定长度——工单编码 = 工单号 + 序列号

        public TicketInfoForm()
        {
            InitializeComponent();
        }

        // 加载DataGridView行数据到TextBox
        public void LoadTicketInfo(DataGridViewRow row, int index)
        {
            isCreate = false;//修改旧工单，而非创建
            workId = JDBC.GetAllTickets()[index].WorkId;

            //自动填充旧工单内容
            if (row != null)
            {
                tb_time.Text      = row.Cells[2].Value.ToString();
                tb_ImagePath.Text = row.Cells[3].Value.ToString();
                tb_WoArea.Text    = row.Cells[4].Value.ToString();
                tb_WoFactory.Text = row.Cells[5].Value.ToString();
                tb_WoLine.Text    = row.Cells[6].Value.ToString();
                tb_WoStation.Text = row.Cells[7].Value.ToString();
                tb_WoBat.Text     = row.Cells[8].Value.ToString();
                tb_WoNum.Text     = row.Cells[9].Value.ToString();
                tb_WoStamp.Text   = row.Cells[10].Value.ToString();
                tb_WoName.Text    = row.Cells[11].Value.ToString();
                tb_AngleResist.Text = row.Cells[12].Value.ToString();
                tb_note.Text      = row.Cells[13].Value.ToString();
            }
        }

        private void TicketInfoForm_Load(object sender, EventArgs e)
        {
            //禁止最大最小化
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            //加载历史记录
            reportInfoPath = MyDevice.userCFG + @"\user.ifo";

            //初始化工单信息
            tb_time.Text = UnitConvert.GetTime(UnitConvert.GetTimeStamp()).ToString();

            //获取工单总表
            allTicketInfo = JDBC.GetAllTickets();

            if (scanWorkNum != "")
            {
                tb_WoNum.Text = scanWorkNum.Substring(0, ticketNumLen);
                tb_WoNum.Enabled = false;
            }

        }

        //加载图片路径
        private void bt_picLoad_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                // 设置文件筛选器，限制为图片文件
                openFileDialog.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp; *.png)|*.jpg; *.jpeg; *.gif; *.bmp; *.png";
                openFileDialog.Title = "请选择文件保存路径";

                // 打开文件对话框
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // 获取选择的文件路径
                    ticketImagePath = openFileDialog.FileName;
                }
            }
            tb_ImagePath.Text = ticketImagePath;

        }

        //生成工单
        private void bt_create_Click(object sender, EventArgs e)
        {
            //保存内容
            SaveReprotInfo();

            if (tb_time.Text == "" || tb_ImagePath.Text == "" || tb_WoNum.Text == "")
            {
                MessageBox.Show("工单信息中图片路径和工单编号不得为空", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            double tempAngleResist = 0;//临时复拧角度

            // 尝试将输入转换为double
            if (double.TryParse(tb_AngleResist.Text, out double result))
            {
                // 检查是否大于等于0
                if (result >= 0)
                {
                    // 有效值
                    tempAngleResist = result;
                }
                else
                {
                    // 值小于0，弹出提示框
                    MessageBox.Show("复拧角度请输入大于或等于0的数字。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                if (tb_AngleResist.Text == "")
                {
                    tempAngleResist = 0;
                }
                else
                {
                    // 转换失败，弹出提示框
                    MessageBox.Show("复拧角度输入的格式不正确，请输入一个有效的数字。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            //工单详细信息
            DSTicketInfo myTicketInfo = new DSTicketInfo
            {
                WorkId    = isCreate ? 0 : workId,
                Time      = tb_time.Text,
                ImagePath = tb_ImagePath.Text.Replace("\\", "/"),
                WoArea    = tb_WoArea.Text,
                WoFactory = tb_WoFactory.Text,
                WoLine    = tb_WoLine.Text,
                WoStation = tb_WoStation.Text,
                WoBat     = tb_WoBat.Text,
                WoNum     = tb_WoNum.Text,
                WoStamp   = tb_WoStamp.Text,
                WoName    = tb_WoName.Text,
                AngleResist = tempAngleResist,
                Note      = tb_note.Text,
            };

            try
            {
                if (isCreate)
                {
                    //判读是否是重复工单
                    foreach (var item in allTicketInfo)
                    {
                        if (item.WoNum == myTicketInfo.WoNum)
                        {
                            MessageBox.Show("工单号重复，创建失败", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    //工单表新增一条纪录
                    JDBC.AddTicket(myTicketInfo);
                }
                else
                {
                    //工单表修改一条记录
                    JDBC.UpdateTicketByWorkId(workId, myTicketInfo);
                }
            }
            catch
            {
                MessageBox.Show("工单创建失败，请安装数据库", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (isCreate)
            {
                //工单衍生产品
                DSProductInfo productInfo = new DSProductInfo()
                {
                    ProductId = 0,
                    WorkId = JDBC.GetTicketByWoNum(myTicketInfo.WoNum).First().WorkId,//myTicketInfo.WoId默认是0，需要手动获取最新值
                    WorkNum = myTicketInfo.WoNum,
                    SequenceId = "0".PadLeft(sequenceLen, '0'),
                    ImagePath = tb_ImagePath.Text.Replace("\\", "/"),
                    AngleResist = myTicketInfo.AngleResist
                };

                //产品表新增一条纪录
                JDBC.AddProduct(productInfo);
            }

            this.Close();
        }

        //工单编号只允许输入数字和字母
        private void tb_WoNum_KeyPress(object sender, KeyPressEventArgs e)
        {
            Regex regex = new Regex(@"[a-zA-Z0-9]");
            if ((regex.IsMatch(e.KeyChar.ToString()) == false) && e.KeyChar != 8)
            {
                e.Handled = true;
                return;
            }
        }

        //保存报告记录
        private void SaveReprotInfo()
        {
            if (!Directory.Exists(MyDevice.userCFG))
            {
                Directory.CreateDirectory(MyDevice.userCFG);
            }

            if (!File.Exists(reportInfoPath))
            {
                //创建文件（close的目的是为了关闭进程，防止重新打开软件出现进程被占用的问题）
                File.Create(reportInfoPath).Close();
            }

            // 设置文件属性为正常
            File.SetAttributes(reportInfoPath, FileAttributes.Normal);

            FileStream meFS = new FileStream(reportInfoPath, FileMode.Create, FileAccess.Write);
            TextWriter meWrite = new StreamWriter(meFS);
            //if (tb_fileName.TextLength > 0)
            //{
            //    meWrite.WriteLine("reportFileName=" + tb_fileName.Text);
            //}
            //if (tb_company.TextLength > 0)
            //{
            //    meWrite.WriteLine("reportCompany=" + tb_company.Text);
            //}
            //if (tb_load.TextLength > 0)
            //{
            //    meWrite.WriteLine("reportLoad=" + tb_load.Text);
            //}
            //if (tb_commodity.TextLength > 0)
            //{
            //    meWrite.WriteLine("reportCommodity=" + tb_commodity.Text);
            //}
            //if (tb_standard.TextLength > 0)
            //{
            //    meWrite.WriteLine("reportStandard=" + tb_standard.Text);
            //}
            meWrite.Close();
            meFS.Close();
            File.SetAttributes(reportInfoPath, FileAttributes.ReadOnly);
        }

    }
}
