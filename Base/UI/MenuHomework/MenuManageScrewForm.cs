using DBHelper;
using Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

//Ricardo 20240516

namespace Base.UI.MenuHomework
{
    public partial class MenuManageScrewForm : Form
    {
        private int currentRowIndex = 0;                                             //当前选中行下标
        private CheckBox headerCheckBox = new CheckBox();                            //行首复选框，实现全选/全不选
        private List<DSTicketScrews> ticketScrewsList = new List<DSTicketScrews>();  //螺栓总表
        private List<DSTicketScrews> selectTicketScrews = new List<DSTicketScrews>();//选中的螺栓表（用于删除螺栓）                                                                             
        private List<int> rowsToRemove = new List<int>();                            // 列表存储要删除的行的索引


        public MenuManageScrewForm()
        {
            InitializeComponent();
        }

        private void MenuManageScrewForm_Load(object sender, EventArgs e)
        {
            dataGridView1.AllowUserToAddRows = false;//禁止用户添加行，解决用户点击复选框，表格自动增加一行
            dataGridView1.AllowUserToDeleteRows = false;//禁止用户删除行
            dataGridView1.AllowUserToOrderColumns = false;//禁止用户手动重新定位行
            dataGridView1.AllowUserToResizeRows = false;//禁止用户调整行大小
            dataGridView1.AllowUserToResizeColumns = false;//禁止用户调整列大小

            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;//表格自动填充
            //dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;//点击选中整行
            dataGridView1.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.ReadOnly = true;//只读

            // 禁止所有列的排序
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            //查询螺栓表
            ticketScrewsList = JDBC.GetAllScrewAlarms();
            if (ticketScrewsList == null) return;
            foreach (DSTicketScrews ticketScrew in ticketScrewsList)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dataGridView1,
                                0,
                                ticketScrew.Name,
                                ticketScrew.Specification,
                                ticketScrew.Standard,
                                ticketScrew.Material,
                                ticketScrew.Screw_headSize,
                                ticketScrew.Screw_headStructure,
                                ticketScrew.Torque_unit,
                                ticketScrew.ModePt,
                                ticketScrew.ModeMx,
                                ticketScrew.ModeAx,
                                ticketScrew.Alarm0,
                                ticketScrew.Alarm1,
                                ticketScrew.Alarm2,
                                ticketScrew.Description);
                dataGridView1.Rows.Add(row);
            }
        }

        //创建螺栓方案
        private void btn_Create_Click(object sender, EventArgs e)
        {
            TicketScrewForm screwInfoForm = new TicketScrewForm();
            screwInfoForm.StartPosition = FormStartPosition.CenterScreen;
            screwInfoForm.ShowDialog();

            //更新工单
            dataGridView1.Rows.Clear();
            dataGridView1.ClearSelection();

            //刷新
            ticketScrewsList = JDBC.GetAllScrewAlarms();
            if (ticketScrewsList == null) return;
            foreach (DSTicketScrews ticketScrew in ticketScrewsList)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dataGridView1,
                                0,
                                ticketScrew.Name,
                                ticketScrew.Specification,
                                ticketScrew.Standard,
                                ticketScrew.Material,
                                ticketScrew.Screw_headSize,
                                ticketScrew.Screw_headStructure,
                                ticketScrew.Torque_unit,
                                ticketScrew.ModePt,
                                ticketScrew.ModeMx,
                                ticketScrew.ModeAx,
                                ticketScrew.Alarm0,
                                ticketScrew.Alarm1,
                                ticketScrew.Alarm2,
                                ticketScrew.Description);
                dataGridView1.Rows.Add(row);
            }
        }

        //全选/全不选
        private void HeaderCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                DataGridViewCheckBoxCell checkboxCell = (DataGridViewCheckBoxCell)dataGridView1[0, i];
                checkboxCell.Value = headerCheckBox.Checked;
            }
            dataGridView1.EndEdit(); // 结束编辑状态
        }

        //绘制单元格 (添加行头复选框)
        private void dataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex == -1 && e.ColumnIndex == 0) // 判断是否是行首列的表头
            {
                e.PaintBackground(e.ClipBounds, false);

                // 计算复选框的位置使其位于单元格中心
                int checkBoxSize = 18; // 复选框大小
                int cellHeight = e.CellBounds.Height;
                int yOffset = (cellHeight - checkBoxSize) / 2;
                int xOffset = (e.CellBounds.Width - checkBoxSize) / 2;

                Point point = e.CellBounds.Location; // 获取列头的位置
                point.X += xOffset;
                point.Y += yOffset;
                headerCheckBox.Location = point;
                headerCheckBox.Size = new Size(18, 18);
                headerCheckBox.BackColor = Color.White;
                headerCheckBox.CheckedChanged += new EventHandler(HeaderCheckBox_CheckedChanged);

                dataGridView1.Controls.Add(headerCheckBox);
                e.Handled = true;
            }
        }

        //鼠标点击
        private void dataGridView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (dataGridView1.Rows.Count != 0)
            {
                if (e.Button == MouseButtons.Left && !(Control.ModifierKeys == Keys.Shift))
                {
                    currentRowIndex = this.dataGridView1.CurrentRow.Index;
                }
            }
        }

        //鼠标 + shift 实现多选
        private void dataGridView1_KeyUp(object sender, KeyEventArgs e)
        {
            if (this.dataGridView1.SelectedCells.Count > 0 && e.KeyData == Keys.ShiftKey)
            {
                int endrow = this.dataGridView1.CurrentRow.Index;
                if (currentRowIndex <= endrow)
                {
                    //正序选时
                    for (int i = currentRowIndex; i <= endrow; i++)
                    {
                        this.dataGridView1.Rows[i].Cells[0].Value = true;
                        this.dataGridView1.Rows[i].Selected = true;
                    }

                    for (int j = endrow + 1; j < this.dataGridView1.Rows.Count; j++)
                    {
                        this.dataGridView1.Rows[j].Cells[0].Value = false;
                        this.dataGridView1.Rows[j].Selected = false;
                    }

                    for (int k = 0; k < currentRowIndex; k++)
                    {
                        this.dataGridView1.Rows[k].Cells[0].Value = false;
                        this.dataGridView1.Rows[k].Selected = false;
                    }
                }
                else
                {
                    //倒序选时
                    for (int i = endrow; i <= currentRowIndex; i++)
                    {
                        this.dataGridView1.Rows[i].Cells[0].Value = true;
                        this.dataGridView1.Rows[i].Selected = true;
                    }

                    for (int j = 0; j < endrow; j++)
                    {
                        this.dataGridView1.Rows[j].Cells[0].Value = false;
                        this.dataGridView1.Rows[j].Selected = false;
                    }

                    for (int k = currentRowIndex + 1; k < this.dataGridView1.Rows.Count; k++)
                    {
                        this.dataGridView1.Rows[k].Cells[0].Value = false;
                        this.dataGridView1.Rows[k].Selected = false;
                    }
                }
            }
        }

        //单击单元格
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex != -1)
            {
                // 选中整行
                this.dataGridView1.Rows[e.RowIndex].Selected = true;

                this.dataGridView1.Rows[e.RowIndex].Cells[0].Value = !Convert.ToBoolean(this.dataGridView1.Rows[e.RowIndex].Cells[0].Value);
            }
        }

        //右键菜单
        private void dataGridView1_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                if (e.RowIndex == -1) return;

                currentRowIndex = e.RowIndex;//获取当前行下标
                this.dataGridView1.Rows[e.RowIndex].Selected = true;  //是否选中当前行
                this.dataGridView1.CurrentCell = this.dataGridView1.Rows[e.RowIndex].Cells[2]; //用于只选中鼠标所在位置的行，其他均不选中

                //指定控件（DataGridView），指定位置（鼠标指定位置）
                this.contextMenuStrip1.Show(Cursor.Position);        //锁定右键列表出现的位置
            }
        }

        //删除螺栓（删除选中的单个螺栓）
        private void DelScrewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //管理员以上的权限
            if (MyDevice.userRole == "0")
            {
                MessageBox.Show("无权限删除螺栓，请切换管理员用户", "确认", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            uint delScrewID = JDBC.GetAllScrewAlarms()[currentRowIndex].ScrewId;
            DialogResult result = MessageBox.Show("是否删除螺栓" + delScrewID + "？删除会附带删除使用该螺栓的所有工单", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No) return;

            //确定删除
            try
            {
                if (JDBC.DeleteScrewAlarmByScrewId(delScrewID)) //数据库删除指定螺栓号对应的螺栓表
                {
                    JDBC.DeleteTicketByScrewId(delScrewID);//数据库删除含有指定螺栓号的工单表 （工单中含有n个螺栓）
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            this.dataGridView1.Rows.RemoveAt(currentRowIndex);//表格更新，删除对应的行
        }

        //删除螺栓（删除选中的n个螺栓）
        private void btn_Delete_Click(object sender, EventArgs e)
        {
            //管理员以上的权限
            if (MyDevice.userRole == "0")
            {
                MessageBox.Show("无权限删除螺栓，请切换管理员用户", "确认", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //无有效螺栓
            if (dataGridView1.RowCount < 1) return;

            rowsToRemove.Clear();//清除删除纪录

            //收集被选中的工单汇总
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                if (this.dataGridView1.Rows[i].Cells[0].Value.ToString() == "True")
                {
                    DSTicketScrews selectTicketScrew = new DSTicketScrews();//必须新建局部变量，因为添加的是指针，如果是全局变量，最后添加的工单信息全部一致
                    rowsToRemove.Add(i);
                    selectTicketScrew.ScrewId = JDBC.GetAllScrewAlarms()[i].ScrewId;
                    selectTicketScrews.Add(selectTicketScrew);
                }
            }

            if (selectTicketScrews.Count < 1)
            {
                MessageBox.Show("未选择任何螺栓，无法删除", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult result = MessageBox.Show("是否删除选中的螺栓" + "？删除会附带删除使用该螺栓的所有工单", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No) return;

            //遍历删除螺栓
            foreach (var item in selectTicketScrews)
            {
                if (JDBC.DeleteScrewAlarmByScrewId(item.ScrewId)) //数据库删除指定螺栓号对应的螺栓表
                {
                    JDBC.DeleteTicketByScrewId(item.ScrewId);//数据库删除含有指定螺栓号的工单表 （工单中含有n个螺栓）
                }
            }

            //逆序遍历删除 （表格行数递减，正序删除容易造成溢出报错）
            for (int i = rowsToRemove.Count - 1; i >= 0; i--)
            {
                this.dataGridView1.Rows.RemoveAt(rowsToRemove[i]);
            }
        }

        //修改螺栓
        private void EditScrewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("是否修改螺栓信息" + JDBC.GetAllScrewAlarms()[currentRowIndex].ScrewId + "？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No) return;

            TicketScrewForm screwInfoForm = new TicketScrewForm();
            screwInfoForm.StartPosition = FormStartPosition.CenterScreen;
            screwInfoForm.LoadScrewInfo(dataGridView1.Rows[currentRowIndex], currentRowIndex);
            screwInfoForm.ShowDialog();

            //更新工单
            dataGridView1.Rows.Clear();
            dataGridView1.ClearSelection();

            //查询螺栓表
            ticketScrewsList = JDBC.GetAllScrewAlarms();
            if (ticketScrewsList == null) return;
            foreach (DSTicketScrews ticketScrew in ticketScrewsList)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dataGridView1,
                                0,
                                ticketScrew.Name,
                                ticketScrew.Specification,
                                ticketScrew.Standard,
                                ticketScrew.Material,
                                ticketScrew.Screw_headSize,
                                ticketScrew.Screw_headStructure,
                                ticketScrew.Torque_unit,
                                ticketScrew.ModePt,
                                ticketScrew.ModeMx,
                                ticketScrew.ModeAx,
                                ticketScrew.Alarm0,
                                ticketScrew.Alarm1,
                                ticketScrew.Alarm2,
                                ticketScrew.Description);
                dataGridView1.Rows.Add(row);
            }
        }
    }
}
