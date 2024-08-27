using DBHelper;
using Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

//Ricardo 20240520

namespace Base.UI.MenuHomework
{
    public partial class MenuManageTicketForm : Form
    {
        private int currentRowIndex = 0;                                         //当前选中行下标
        private CheckBox headerCheckBox = new CheckBox();                        //行首复选框，实现全选/全不选
        private List<DSTicketInfo>  dSTicketInfos = new List<DSTicketInfo> ();   //工单总表
        private List<DSTicketInfo> selectTicketInfos = new List<DSTicketInfo>(); //选中的工单表（用于删除工单）                                                                             
        private List<int> rowsToRemove = new List<int>();                       // 列表存储要删除的行的索引

        public MenuManageTicketForm()
        {
            InitializeComponent();
        }

        private void MenuCreateTicketForm_Load(object sender, EventArgs e)
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
            dSTicketInfos = JDBC.GetAllTickets();
            if (dSTicketInfos == null) return;
            foreach (DSTicketInfo ticketInfo in dSTicketInfos)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dataGridView1, 
                                0,
                                ticketInfo.WorkId, 
                                ticketInfo.Time, 
                                ticketInfo.ImagePath, 
                                ticketInfo.WoArea, 
                                ticketInfo.WoFactory, 
                                ticketInfo.WoLine, 
                                ticketInfo.WoStation, 
                                ticketInfo.WoBat, 
                                ticketInfo.WoNum, 
                                ticketInfo.WoStamp, 
                                ticketInfo.WoName, 
                                ticketInfo.Note, 
                                ticketInfo.Reserve);
                dataGridView1.Rows.Add(row);
            }
        }

        //创建工单
        private void btn_Create_Click(object sender, EventArgs e)
        {
            textBox1.Focus();//重新聚焦，用于扫码枪输入

            TicketInfoForm ticketInfoForm = new TicketInfoForm();
            ticketInfoForm.StartPosition = FormStartPosition.CenterScreen;
            ticketInfoForm.ShowDialog();

            //更新工单
            dataGridView1.Rows.Clear();
            dataGridView1.ClearSelection();

            //查询工单表
            dSTicketInfos = JDBC.GetAllTickets();
            if (dSTicketInfos == null) return;
            foreach (DSTicketInfo ticketInfo in dSTicketInfos)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dataGridView1,
                                0,
                                ticketInfo.WorkId,
                                ticketInfo.Time,
                                ticketInfo.ImagePath,
                                ticketInfo.WoArea,
                                ticketInfo.WoFactory,
                                ticketInfo.WoLine,
                                ticketInfo.WoStation,
                                ticketInfo.WoBat,
                                ticketInfo.WoNum,
                                ticketInfo.WoStamp,
                                ticketInfo.WoName,
                                ticketInfo.Note,
                                ticketInfo.Reserve);
                dataGridView1.Rows.Add(row);
            }
        }

        //删除工单（删除选中的n个工单）
        private void btn_Delete_Click(object sender, EventArgs e)
        {
            textBox1.Focus();//重新聚焦，用于扫码枪输入

            //管理员以上的权限
            if (MyDevice.userRole == "0")
            {
                MessageBox.Show("无权限删除工单，请切换管理员用户", "确认", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //无有效工单
            if (dataGridView1.RowCount < 1) return;

            rowsToRemove.Clear();//清除删除纪录

            //收集被选中的工单汇总
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                if (this.dataGridView1.Rows[i].Cells[0].Value.ToString() == "True")
                {
                    DSTicketInfo selectTicketInfo = new DSTicketInfo();//必须新建局部变量，因为添加的是指针，如果是全局变量，最后添加的工单信息全部一致
                    selectTicketInfo.WorkId = Convert.ToUInt32(this.dataGridView1.Rows[i].Cells[1].Value.ToString());
                    selectTicketInfos.Add(selectTicketInfo);
                    rowsToRemove.Add(i);
                }
            }

            if (selectTicketInfos.Count < 1)
            {
                MessageBox.Show("未选择任何工单，无法删除", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult result = MessageBox.Show("是否删除选中的工单" + "？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No) return;

            //遍历删除工单
            foreach (var item in selectTicketInfos)
            {
                JDBC.DeleteTicketByWorkId(item.WorkId);
                JDBC.DeletePointsByWorkId(item.WorkId);//删除对应工单包含的点位
            }

            //逆序遍历删除 （表格行数递减，正序删除容易造成溢出报错）
            for (int i = rowsToRemove.Count - 1; i >= 0; i--)
            {
                this.dataGridView1.Rows.RemoveAt(rowsToRemove[i]);
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

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex != -1)
            {
                // 选中整行
                this.dataGridView1.Rows[e.RowIndex].Selected = true;

                this.dataGridView1.Rows[e.RowIndex].Cells[0].Value = !Convert.ToBoolean(this.dataGridView1.Rows[e.RowIndex].Cells[0].Value);
            }
        }

        //双击表格打开工单设计
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (JDBC.GetAllScrewAlarms().Count == 0)
            {
                MessageBox.Show("未创建任何螺栓，无法新建工单", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (dataGridView1.CurrentRow != null)
            {
                MenuCreateTicketForm menuCreateTicketForm = new MenuCreateTicketForm();
                menuCreateTicketForm.MeTicketInfo = dSTicketInfos[dataGridView1.CurrentRow.Index];
                menuCreateTicketForm.MdiParent = this.MdiParent;
                menuCreateTicketForm.Show();
                menuCreateTicketForm.WindowState = FormWindowState.Maximized;
                this.Close();
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

        //扫码枪创建工单（工单号锁定）
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 检查是否按下了回车键
            if (e.KeyChar == (char)Keys.Enter)
            {
                // 根据扫码枪内容创建工单
                TicketInfoForm ticketInfoForm = new TicketInfoForm();
                ticketInfoForm.scanWorkNum = textBox1.Text;//扫码枪提供工单号
                ticketInfoForm.StartPosition = FormStartPosition.CenterScreen;
                ticketInfoForm.ShowDialog();

                //更新工单
                dataGridView1.Rows.Clear();
                dataGridView1.ClearSelection();

                //查询工单表
                dSTicketInfos = JDBC.GetAllTickets();
                if (dSTicketInfos == null) return;
                foreach (DSTicketInfo ticketInfo in dSTicketInfos)
                {
                    DataGridViewRow row = new DataGridViewRow();
                    row.CreateCells(dataGridView1,
                                    0,
                                    ticketInfo.WorkId,
                                    ticketInfo.Time,
                                    ticketInfo.ImagePath,
                                    ticketInfo.WoArea,
                                    ticketInfo.WoFactory,
                                    ticketInfo.WoLine,
                                    ticketInfo.WoStation,
                                    ticketInfo.WoBat,
                                    ticketInfo.WoNum,
                                    ticketInfo.WoStamp,
                                    ticketInfo.WoName,
                                    ticketInfo.Note,
                                    ticketInfo.Reserve);
                    dataGridView1.Rows.Add(row);
                }

                //清除扫描抢历史内容
                textBox1.Clear();
            }
        }

        //删除工单（删除选中的单个工单）
        private void DelTicketToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //管理员以上的权限
            if (MyDevice.userRole == "0")
            {
                MessageBox.Show("无权限删除工单，请切换管理员用户", "确认", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            uint delTicketID = Convert.ToUInt32(this.dataGridView1.Rows[currentRowIndex].Cells[1].Value.ToString());
            DialogResult result = MessageBox.Show("是否删除工单" + delTicketID + "？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No) return;

            //确定删除
            try
            {
                JDBC.DeleteTicketByWorkId(delTicketID);//数据库删除指定工单号对应的工单
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            this.dataGridView1.Rows.RemoveAt(currentRowIndex);//表格更新，删除对应的行
        }

        //编辑工单
        private void EditTicketToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("是否修改工单" + Convert.ToUInt32(this.dataGridView1.Rows[currentRowIndex].Cells[1].Value.ToString()) + "？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No) return;

            TicketInfoForm ticketInfoForm = new TicketInfoForm();
            ticketInfoForm.StartPosition = FormStartPosition.CenterScreen;
            ticketInfoForm.LoadTicketInfo(dataGridView1.Rows[currentRowIndex], currentRowIndex);
            ticketInfoForm.ShowDialog();

            //更新工单
            dataGridView1.Rows.Clear();
            dataGridView1.ClearSelection();

            //查询工单表
            dSTicketInfos = JDBC.GetAllTickets();
            if (dSTicketInfos == null) return;
            foreach (DSTicketInfo ticketInfo in dSTicketInfos)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dataGridView1,
                                0,
                                ticketInfo.WorkId,
                                ticketInfo.Time,
                                ticketInfo.ImagePath,
                                ticketInfo.WoArea,
                                ticketInfo.WoFactory,
                                ticketInfo.WoLine,
                                ticketInfo.WoStation,
                                ticketInfo.WoBat,
                                ticketInfo.WoNum,
                                ticketInfo.WoStamp,
                                ticketInfo.WoName,
                                ticketInfo.Note,
                                ticketInfo.Reserve);
                dataGridView1.Rows.Add(row);
            }
        }

        //生成条形码（工单号）
        private void GenerateBarcodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GenerateCode(1);
        }

        //生成二维码（工单号）
        private void GenerateQRcodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GenerateCode(2);
        }

        // 生成条形码
        private Bitmap GetBarcodeBitmap(string barcodeContent, int barcodeWidth, int barcodeHeight)
        {
            BarcodeWriter barcodeWriter = new BarcodeWriter();
            barcodeWriter.Format = BarcodeFormat.CODE_128;            //设置编码格式
            EncodingOptions encodingOptions = new EncodingOptions();
            encodingOptions.Width = barcodeWidth;                     //设置宽度
            encodingOptions.Height = barcodeHeight;                   //设置长度
            encodingOptions.Margin = 2;                               //设置边距
            barcodeWriter.Options = encodingOptions;
            Bitmap bitmap = barcodeWriter.Write(barcodeContent);
            return bitmap;
        }

        // 生成二维码
        public static Bitmap GetQRCodeBitmap(string qrCodeContent, int qrCodeWidth, int qrCodeHeight)
        {
            BarcodeWriter barcodeWriter = new BarcodeWriter();
            barcodeWriter.Format = BarcodeFormat.QR_CODE;
            QrCodeEncodingOptions qrCodeEncodingOptions = new QrCodeEncodingOptions();
            qrCodeEncodingOptions.DisableECI = true;
            qrCodeEncodingOptions.CharacterSet = "UTF-8";             //设置编码
            qrCodeEncodingOptions.Width = qrCodeWidth;                //设置二维码宽度
            qrCodeEncodingOptions.Height = qrCodeHeight;              //设置二维码高度
            qrCodeEncodingOptions.Margin = 1;                         //设置二维码边距

            barcodeWriter.Options = qrCodeEncodingOptions;
            Bitmap bitmap = barcodeWriter.Write(qrCodeContent);       //写入内容
            return bitmap;
        }

        //生成码操作
        private void GenerateCode(int type)
        {
            List<DSTicketInfo> curTickets = JDBC.GetAllTickets();
            if (curTickets == null) return;

            string codeWorkID = curTickets[this.dataGridView1.SelectedRows[0].Index].WoNum.ToString();  //要生成条形码的工单ID

            Bitmap barcodeBitmap = type == 1 ? GetBarcodeBitmap(codeWorkID, 400, 200) : GetQRCodeBitmap(codeWorkID, 200, 200);

            string fileName = $"{codeWorkID}.png";    //文件名

            // 创建保存文件对话框
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PNG files (*.png)|*.png",
                Title = "保存图片文件",
                DefaultExt = "png",
                FileName = fileName,
            };

            // 显示对话框并获取结果
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                // 获取用户选择的文件路径
                string filePath = saveFileDialog.FileName;
                //
                barcodeBitmap.Save(filePath, ImageFormat.Png);
            }
        }

    }
}
