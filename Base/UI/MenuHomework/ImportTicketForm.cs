using DBHelper;
using Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

//Ricardo 20240510

namespace Base.UI.MenuHomework
{
    public partial class ImportTicketForm : Form
    {
        private string targetTicketNum = "";      //目标工单的工单号
        private List<DSTicketInfo> dSTicketInfos = new List<DSTicketInfo>();       //工单总表
        private List<DSTicketPoints> dSTicketPoints = new List<DSTicketPoints>();  //点位总表
        private List<DSTicketInfo> filteredList = new List<DSTicketInfo>();        //筛选后的有效工单表
        private List<Byte> ticketAddrList = new List<Byte>();                      //工单所包含的扳手站点集合

        public DSTicketInfo MeTicketInfo;

        public ImportTicketForm()
        {
            InitializeComponent();
        }

        private void ImportTicketForm_Load(object sender, EventArgs e)
        {
            dataGridView1.AllowUserToAddRows = false;//禁止用户添加行，解决用户点击复选框，表格自动增加一行
            dataGridView1.AllowUserToDeleteRows = false;//禁止用户删除行
            dataGridView1.AllowUserToOrderColumns = false;//禁止用户手动重新定位行
            dataGridView1.AllowUserToResizeRows = false;//禁止用户调整行大小
            dataGridView1.AllowUserToResizeColumns = false;//禁止用户调整列大小

            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;//表格自动填充
            dataGridView1.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.ReadOnly = true;

            // 禁止所有列的排序
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            dSTicketInfos = JDBC.GetAllTickets();
            if (dSTicketInfos == null) return;
            foreach (DSTicketInfo ticketInfo in dSTicketInfos)
            {
                ticketAddrList.Clear();
                dSTicketPoints = JDBC.GetPointsByWorkId(ticketInfo.WorkId);
                foreach (DSTicketPoints ticketPoint in dSTicketPoints)
                {
                    ticketAddrList.Add((byte)JDBC.GetWrenchesByPointId(ticketPoint.PointId)[0].Addr);//待改进，一个点位可能多把扳手拧
                }
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dataGridView1,ticketInfo.WoNum, string.Join(", ", ticketAddrList.OrderBy(n => n).Distinct()));
                dataGridView1.Rows.Add(row);
            }

        }

        //页面关闭
        private void ImportTicketForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //未选择任何工单，默认第一个
            if (MeTicketInfo == null)
            {
                MeTicketInfo = filteredList.Count == 0 ? dSTicketInfos[0] : filteredList[0];
            }
        }

        //限制输入
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 使用正则表达式限制输入
            Regex regex = new Regex(@"^[a-zA-Z0-9_]+$");
            if (!regex.IsMatch(e.KeyChar.ToString()) && e.KeyChar != '\b')
            {
                e.Handled = true; // 阻止非法字符输入
            }

            // 检查是否按下了回车键
            if (e.KeyChar == (char)Keys.Enter)
            {
                // 触发按钮的 Click 事件
                btn_Search.PerformClick();
            }
        }

        //允许复制粘贴
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            BoxRestrict.KeyUp_ControlXCV(sender, e);
        }

        //搜索工单
        private void btn_Search_Click(object sender, EventArgs e)
        {
            //查找扳手总表中包含搜索字符串的所有项
            filteredList = dSTicketInfos.Where(item => item.WoNum.ToString().Contains(textBoxEx1.Text)).ToList();

            //显示查到的所有项
            dataGridView1.Rows.Clear();
            dataGridView1.ClearSelection();

            foreach (var item1 in filteredList)
            {
                ticketAddrList.Clear();
                dSTicketPoints = JDBC.GetPointsByWorkId(item1.WorkId);
                foreach (DSTicketPoints ticketPoint in dSTicketPoints)
                {
                    ticketAddrList.Add((byte)JDBC.GetWrenchesByPointId(ticketPoint.PointId)[0].Addr);//待改进，一个点位可能多把扳手拧
                }
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dataGridView1, item1.WoNum, string.Join(", ", ticketAddrList.OrderBy(n => n).Distinct()));
                dataGridView1.Rows.Add(row);
            }
        }

        //双击打开功能
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            MeTicketInfo = filteredList.Count == 0 ? dSTicketInfos[dataGridView1.CurrentRow.Index] : filteredList[dataGridView1.CurrentRow.Index];
            this.Close();
        }
    }
}
