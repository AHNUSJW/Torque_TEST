using DBHelper;
using Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Base.UI.MenuHomework
{
    public partial class SearchSrcewInfoForm : Form
    {

        private List<DSTicketScrews> ticketScrewsList = new List<DSTicketScrews>();   //螺栓总表
        private List<DSTicketScrews> filteredList = new List<DSTicketScrews>();       //筛选后的有效螺栓表
        private bool isCellDoubleClick = false; //是否双击表格，根据此变量强制客户点击选中才可关闭页面
        private string screwAlarms = "";        //螺栓报警值汇总

        // 定义委托类型
        public delegate void DataSelectedEventHandler(DSTicketScrews screw);

        // 定义事件
        public event DataSelectedEventHandler DataSelected;

        public SearchSrcewInfoForm()
        {
            InitializeComponent();
        }

        //加载页面
        private void Form1_Load(object sender, EventArgs e)
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

            //获取螺栓总表
            ticketScrewsList = JDBC.GetAllScrewAlarms();
            foreach (var item in ticketScrewsList)
            {
                DataGridViewRow row = new DataGridViewRow();

                //更新报警值
                switch (item.ModeAx)
                {
                    case "SN":
                        screwAlarms = $"{item.Alarm0} {item.Torque_unit}";
                        break;
                    case "SA":
                        screwAlarms = $"{item.Alarm0} {item.Torque_unit}, " +  item.Alarm1 + "°";
                        break;
                    case "MN":
                        screwAlarms = $"[{item.Alarm0}, {item.Alarm1}] {item.Torque_unit}";
                        break;
                    case "MA":
                        screwAlarms = $"{item.Alarm0} {item.Torque_unit}, " + $"[{item.Alarm1}°, {item.Alarm2}°]";
                        break;
                    default:
                        break;
                }

                row.CreateCells(dataGridView1, item.Name, item.Specification, item.Standard, item.Material, item.Screw_headSize, item.Screw_headStructure, screwAlarms);
                dataGridView1.Rows.Add(row);
            }
        }

        //关闭页面
        private void SearchSrcewInfoForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 如果没有发生双击事件，则阻止关闭页面（防止画了点位之后不选择螺栓导致点位信息为null）
            if (!isCellDoubleClick)
            {
                e.Cancel = true;
                MessageBox.Show("请双击选中一个螺栓", "确认", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        //搜索
        private void btn_Search_Click(object sender, EventArgs e)
        {
            //查找螺栓总表中包含搜索字符串的所有项
            filteredList = ticketScrewsList.Where(item => item.Name.Contains(textBoxEx1.Text)).ToList();

            //显示查到的所有项
            dataGridView1.Rows.Clear();
            dataGridView1.ClearSelection();

            foreach (var item in filteredList)
            {
                DataGridViewRow row = new DataGridViewRow();

                //更新报警值
                switch (item.ModeAx)
                {
                    case "SN":
                        screwAlarms = $"{item.Alarm0} {item.Torque_unit}";
                        break;
                    case "SA":
                        screwAlarms = $"{item.Alarm0} {item.Torque_unit}, " + item.Alarm1 + "°";
                        break;
                    case "MN":
                        screwAlarms = $"[{item.Alarm0}, {item.Alarm1}] {item.Torque_unit}";
                        break;
                    case "MA":
                        screwAlarms = $"{item.Alarm0} {item.Torque_unit}, " + $"[{item.Alarm1}°, {item.Alarm2}°]";
                        break;
                    default:
                        break;
                }

                row.CreateCells(dataGridView1, item.Name, item.Specification, item.Standard, item.Material, item.Screw_headSize, item.Screw_headStructure, screwAlarms);
                dataGridView1.Rows.Add(row);
            }
        }

        //双击表格选中螺栓
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            //搜索后，显示对应螺栓表
            if (filteredList.Count > 0)
            {
                // 在按钮点击时触发事件，传递新的值
                DataSelected?.Invoke(filteredList[dataGridView1.CurrentRow.Index]);
            }
            //未搜索，显示整张螺栓表
            else
            {
                DataSelected?.Invoke(ticketScrewsList[dataGridView1.CurrentRow.Index]);
            }

            isCellDoubleClick = true;
            this.Close();
        }
    }
}
