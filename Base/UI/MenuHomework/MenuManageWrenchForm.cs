using DBHelper;
using Model;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Base.UI.MenuHomework
{
    //Ricardo 20240723

    public partial class MenuManageWrenchForm : Form
    {
        private List<DSWrenchDevc> wrenchDevcList = new List<DSWrenchDevc>();   //扳手信息表
        private List<int> rowsToRemove = new List<int>();                       //列表存储要删除的行的索引
        private string torUnit = "";                                            //扭矩单位
        private byte addr = 1;                                                  //设备站点

        public MenuManageWrenchForm()
        {
            InitializeComponent();
        }

        private void MenuManageWrenchForm_Load(object sender, EventArgs e)
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

            //查询工单表
            wrenchDevcList = JDBC.GetAllWrenchDevc();
            DSWrenchWlan targetWrenchWlan = new DSWrenchWlan();
            bool IsAllowAuto = false;//是否允许自动连接
            if (wrenchDevcList == null) return;
            foreach (var item in wrenchDevcList)
            {
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

                targetWrenchWlan = JDBC.GetWrenchWlanByWlanId(item.WlanId);
                //站点更新
                addr = targetWrenchWlan.Addr;

                //判断是否能自动更新
                IsAllowAuto = item.ConnectAuto != "True" ? false : true;

                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dataGridView1,
                                0,
                                item.Series + "-" + item.Type,
                                addr,
                                item.BohrCode,
                                item.Capacity / Math.Pow(10, item.TorqueDecimal) + " " + torUnit,
                                item.ConnectType,
                                IsAllowAuto,
                                "删除"
                                );
                dataGridView1.Rows.Add(row);
            }

        }

        //单击选行
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //选中行
            if (e.RowIndex != -1)
            {
                // 选中整行
                this.dataGridView1.Rows[e.RowIndex].Selected = true;

                this.dataGridView1.Rows[e.RowIndex].Cells[0].Value = !Convert.ToBoolean(this.dataGridView1.Rows[e.RowIndex].Cells[0].Value);
            }

            //选择是否允许自动连接
            if (e.ColumnIndex == dataGridView1.Columns.Count - 2 && e.RowIndex >= 0)
            {
                DialogResult result;
                bool IsAutoConnect = false;
                wrenchDevcList = JDBC.GetAllWrenchDevc();
                if (dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString() == "False")
                {
                    IsAutoConnect = true;
                    result = MessageBox.Show("是否允许扳手自动连接", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                }
                else
                {
                    IsAutoConnect = false;
                    result = MessageBox.Show("是否禁止扳手自动连接", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                }

                if (result == DialogResult.Yes)
                {
                    dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = IsAutoConnect;
                    DSWrenchDevc targetWrenchDevc = wrenchDevcList[e.RowIndex];
                    targetWrenchDevc.ConnectAuto = IsAutoConnect == true ? "True" : "False";
                    JDBC.UpdateWrenchDevc(targetWrenchDevc.Wid, targetWrenchDevc);
                }
            }

            //删除扳手——管理员权限
            if (e.ColumnIndex == dataGridView1.Columns.Count - 1 && e.RowIndex >= 0)
            {
                switch (MyDevice.userRole)
                {
                    case "0":
                        MessageBox.Show("无权限删除扳手，请切换管理员用户", "确认", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    case "1":
                    case "32":
                        break;
                    default:
                        break;
                }

                rowsToRemove.Clear();

                // 删除扳手
                DialogResult result = MessageBox.Show("是否删除选中的扳手" + "？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                rowsToRemove.Add(e.RowIndex);
                if (result == DialogResult.No) return;
                try
                {
                    wrenchDevcList = JDBC.GetAllWrenchDevc();//查询工单表,每次删除后需要重新查询

                    if (JDBC.DeleteWrench(wrenchDevcList[dataGridView1.CurrentRow.Index].Wid))
                    {
                        //逆序遍历删除 （表格行数递减，正序删除容易造成溢出报错）
                        for (int i = rowsToRemove.Count - 1; i >= 0; i--)
                        {
                            this.dataGridView1.Rows.RemoveAt(rowsToRemove[i]);
                        }

                        MessageBox.Show("删除成功");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        //双击弹窗
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            WrenchInfoForm wrenchInfoForm = new WrenchInfoForm();
            wrenchInfoForm.Wid = wrenchDevcList[dataGridView1.CurrentRow.Index].Wid;
            wrenchInfoForm.StartPosition = FormStartPosition.CenterScreen;
            wrenchInfoForm.ShowDialog();
        }
    }
}
