using DBHelper;
using Library;
using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

//Ricardo 20240522

namespace Base.UI.MenuHomework
{
    public partial class TicketScrewForm : Form
    {
        private bool IsCreate = true;//是否是新建（区分新建螺栓和修改螺栓）
        private uint screwId; //螺栓（用于修改螺栓对应的螺栓号）

        public TicketScrewForm()
        {
            InitializeComponent();
        }

        // 加载DataGridView行数据到TextBox
        public void LoadScrewInfo(DataGridViewRow row, int index)
        {
            IsCreate = false;//修改旧螺栓，而非创建
            screwId = JDBC.GetAllScrewAlarms()[index].ScrewId;

            //自动填充旧螺栓内容
            if (row != null)
            {
                tb_name.Text          = row.Cells[1].Value.ToString();
                tb_specs.Text         = row.Cells[2].Value.ToString();
                tb_standard.Text      = row.Cells[3].Value.ToString();
                tb_material.Text      = row.Cells[4].Value.ToString();
                tb_headSize.Text      = row.Cells[5].Value.ToString();
                tb_headStructure.Text = row.Cells[6].Value.ToString();
                comboBox_Unit.Text    = row.Cells[7].Value.ToString();
                comboBox_ptMode.Text  = row.Cells[8].Value.ToString();
                comboBox_mxMode.Text  = row.Cells[9].Value.ToString();
                comboBox_axMode.Text  = row.Cells[10].Value.ToString();
                if (comboBox_axMode.Text == "SN")
                {
                    tb_alarm1.Text = row.Cells[11].Value.ToString();
                }
                else if (comboBox_axMode.Text == "SA" || comboBox_axMode.Text == "MN")
                {
                    tb_alarm1.Text = row.Cells[11].Value.ToString();
                    tb_alarm2.Text = row.Cells[12].Value.ToString();
                }
                else if (comboBox_axMode.Text == "MA")
                {
                    tb_alarm1.Text = row.Cells[11].Value.ToString();
                    tb_alarm2.Text = row.Cells[12].Value.ToString();
                    tb_alarm3.Text = row.Cells[13].Value.ToString();
                }
                tb_description.Text  = row.Cells[14].Value.ToString();
            }
        }

        private void ScrewInfoForm_Load(object sender, EventArgs e)
        {
            //禁止最大最小化
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            if (IsCreate)
            {
                this.comboBox_Unit.SelectedIndex = 0;
                this.comboBox_ptMode.SelectedIndex = 0;
                this.comboBox_mxMode.SelectedIndex = 0;
                this.comboBox_axMode.SelectedIndex = 0;
            }

            tb_alarm1.KeyPress += new KeyPressEventHandler(BoxRestrict.KeyPress_IntegerPositive_len3);
            tb_alarm2.KeyPress += new KeyPressEventHandler(BoxRestrict.KeyPress_IntegerPositive_len3);
            tb_alarm3.KeyPress += new KeyPressEventHandler(BoxRestrict.KeyPress_IntegerPositive_len3);
        }

        //模式切换
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox_axMode.Text)
            {
                case "SN":
                    label6.Text = "扭矩限制值";
                    label7.Visible = false;
                    tb_alarm2.Visible = false;
                    label8.Visible = false;
                    tb_alarm3.Visible = false;
                    break;

                case "SA":
                    label6.Text = "扭矩限制值";
                    label7.Text = "角度限制值";
                    label7.Visible = true;
                    tb_alarm2.Visible = true;
                    label8.Visible = false;
                    tb_alarm3.Visible = false;
                    break;

                case "MN":
                    label6.Text = "扭矩下限值";
                    label7.Text = "扭矩上限值";
                    label7.Visible = true;
                    tb_alarm2.Visible = true;
                    label8.Visible = false;
                    tb_alarm3.Visible = false;
                    break;

                case "MA":
                    label6.Text = "扭矩限制值";
                    label7.Text = "角度下限值";
                    label8.Text = "角度上限值";
                    label7.Visible = true;
                    tb_alarm2.Visible = true;
                    label8.Visible = true;
                    tb_alarm3.Visible = true;
                    break;

                default:
                    break;
            }

            tb_alarm1.Text = "";
            tb_alarm2.Text = "";
            tb_alarm3.Text = "";
        }

        //创建螺栓
        private void bt_create_Click(object sender, EventArgs e)
        {
            if (tb_name.Text == "" 
                || tb_specs.Text == "" 
                || tb_standard.Text == ""
                || tb_alarm1.Text == "" 
                || tb_material.Text == "" 
                || tb_headSize.Text == "" 
                || tb_headStructure.Text == "")
            {
                MessageBox.Show("螺栓信息不得为空");
                return;
            }

            if (tb_alarm2.Visible == true && tb_alarm2.Text == "")
            {
                MessageBox.Show("螺栓信息不得为空");
                return;
            }

            if (tb_alarm3.Visible == true && tb_alarm3.Text == "")
            {
                MessageBox.Show("螺栓信息不得为空");
                return;
            }

            DSTicketScrews myTicketScrews = new DSTicketScrews
            {
                ScrewId       = IsCreate ? 0 : screwId,
                Name          = tb_name.Text,
                Specification = tb_specs.Text,
                Standard      = tb_standard.Text,
                Material      = tb_material.Text,
                Screw_headSize = tb_headSize.Text,
                Screw_headStructure = tb_headStructure.Text,
                Torque_unit   = comboBox_Unit.Text,
                ModePt        = comboBox_ptMode.Text,
                ModeMx        = comboBox_mxMode.Text,
                ModeAx        = comboBox_axMode.Text,
                Alarm0        = tb_alarm1.Text,
                Alarm1        = tb_alarm2.Text,
                Alarm2        = tb_alarm3.Text,
                Description   = tb_description.Text,
            };

            try
            {
                if (IsCreate)
                {
                    //螺栓表增加一条记录
                    JDBC.AddScrewAlarm(myTicketScrews);
                }
                else
                {
                    //螺栓表修改一条纪录
                    JDBC.UpdateScrewAlarmByScrewId(screwId, myTicketScrews);
                }
            }
            catch
            {
                MessageBox.Show("螺栓创建失败，请安装数据库");
                return;
            }

            this.Close();
        }

        //螺栓规格输入限制 (M开头 + 数字)
        private void tb_specs_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 如果输入的字符不是M，并且当前文本框的内容为空，则自动添加M
            if (tb_specs.Text.Length == 0 || tb_specs.Text[0] != 'M')
            {
                tb_specs.Text = "M";
                tb_specs.SelectionStart = tb_specs.Text.Length;
            }

            // 使用正则表达式限制输入
            Regex regex = new Regex(@"^[0-9_]+$");
            if (!regex.IsMatch(e.KeyChar.ToString()) && e.KeyChar != '\b')
            {
                e.Handled = true; // 阻止非法字符输入
            }
        }

        private double ConvertToDouble(string targetValue, string errorText)
        {
            double doubleValue = 0;
            // 尝试将输入转换为double
            if (double.TryParse(targetValue, out double result))
            {
                // 检查是否大于0
                if (result > 0)
                {
                    // 有效值
                    doubleValue = result;
                }
                else
                {
                    // 值小于0，弹出提示框
                    MessageBox.Show($"{errorText}请输入大于0的数字。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return -1;
                }
            }
            else
            {
                // 转换失败，弹出提示框
                MessageBox.Show($"{errorText}输入的格式不正确，请输入一个有效的数字。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }

            return doubleValue;
        }
    }
}
