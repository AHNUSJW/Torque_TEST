using DBHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Base.UI.MenuHomework
{

    public partial class SearchWrenchInfoForm : Form
    {
        private List<DSWrenchDevc> wrenchDevcList = new List<DSWrenchDevc>();   //扳手信息表
        private List<DSWrenchWlan> wrenchWlanList = new List<DSWrenchWlan>();   //扳手WLAN方案
        private List<DSWrenchWlan> filteredList = new List<DSWrenchWlan>();     //筛选后的有效扳手表
        private List<DSWrenchWlan> targetList = new List<DSWrenchWlan>();       //传递的目标有效扳手表
        private DSWrenchWlan wrenchWlan = new DSWrenchWlan();
        private DSWrenchDevc wrenchDevc = new DSWrenchDevc();
        private bool isCellDoubleClick = false; //是否双击表格，根据此变量强制客户点击选中才可关闭页面
        private string torUnit;                 //扭矩单位

        // 定义委托类型
        public delegate void DataSelectedEventHandler(DSWrenchWlan wrench);

        // 定义事件
        public event DataSelectedEventHandler DataSelected;

        public SearchWrenchInfoForm()
        {
            InitializeComponent();
        }

        //页面加载
        private void SearchWrenchInfoForm_Load(object sender, EventArgs e)
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

            //清除历史目标纪录
            targetList.Clear();

            //获取扳手总表
            wrenchDevcList = JDBC.GetAllWrenchDevc();
            wrenchWlanList = JDBC.GetAllWrenchWlan();
            foreach (var item in wrenchDevcList)
            {
                DataGridViewRow row = new DataGridViewRow();
                wrenchWlan = JDBC.GetWrenchWlanByWlanId(item.WlanId);
                //单位更新
                switch (item.Unit)
                {
                    case "UNIT_nm": torUnit = "N·m"; break;
                    case "UNIT_lbfin": torUnit = "lbf·in"; break;
                    case "UNIT_lbfft": torUnit = "lbf·ft"; break;
                    case "UNIT_kgcm": torUnit = "kgf·cm"; break;
                    case "UNIT_kgm": torUnit = "kgf·m"; break;
                    default: break;
                }

                row.CreateCells(dataGridView1, wrenchWlan.Addr, (item.Capacity / Math.Pow(10, item.TorqueDecimal)) + " " + torUnit, item.BohrCode);
                dataGridView1.Rows.Add(row);
                targetList.Add(wrenchWlan);
            }
        }

        //页面关闭
        private void SearchWrenchInfoForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isCellDoubleClick)
            {
                DSWrenchWlan dSWrenchWlan = new DSWrenchWlan();
                if (textBoxEx1.Text == "")
                {
                    dSWrenchWlan.Addr = 1;
                }
                else
                {
                    dSWrenchWlan.Addr = Convert.ToByte(textBoxEx1.Text);
                }
                DataSelected?.Invoke(dSWrenchWlan);
            }
        }

        //搜索
        private void btn_Search_Click(object sender, EventArgs e)
        {
            //查找扳手总表中包含搜索字符串的所有项
            filteredList = wrenchWlanList.Where(item => item.Addr.ToString().Contains(textBoxEx1.Text)).ToList();

            //显示查到的所有项
            dataGridView1.Rows.Clear();
            dataGridView1.ClearSelection();
            targetList.Clear();

            foreach (var item1 in filteredList)
            {
                foreach (var item2 in wrenchDevcList)
                {
                    if (item1.WlanId == item2.WlanId) //根据两张表共同属性Wlanid添加
                    {
                        DataGridViewRow row = new DataGridViewRow();
                        //单位更新
                        switch (item2.Unit)
                        {
                            case "UNIT_nm": torUnit = "N·m"; break;
                            case "UNIT_lbfin": torUnit = "lbf·in"; break;
                            case "UNIT_lbfft": torUnit = "lbf·ft"; break;
                            case "UNIT_kgcm": torUnit = "kgf·cm"; break;
                            case "UNIT_kgm": torUnit = "kgf·m"; break;
                            default: break;
                        }

                        row.CreateCells(dataGridView1, item1.Addr, (item2.Capacity / Math.Pow(10, item2.TorqueDecimal)) + " " + torUnit, item2.BohrCode);
                        dataGridView1.Rows.Add(row);
                        targetList.Add(item1);
                    }
                }
            }
        }

        //双击表格选中扳手
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            DataSelected?.Invoke(targetList[dataGridView1.CurrentRow.Index]);

            isCellDoubleClick = true;//选中扳手允许关闭页面

            this.Close();
        }

    }
}
