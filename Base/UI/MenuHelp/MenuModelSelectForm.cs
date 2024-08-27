using Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

//Ricardo 20240807

namespace Base.UI.MenuHelp
{
    public partial class MenuModelSelectForm : Form
    {
        DataView modelData = new DataView();

        public MenuModelSelectForm()
        {
            InitializeComponent();
        }

        private void MenuModelSelectForm_Load(object sender, EventArgs e)
        {
            //导入数据
            string dataPath = "";
            if (MyDevice.languageType == 0)
            {
                dataPath = Application.StartupPath + @"\pic\Modelcn.tmp";
            }
            else
            {
                dataPath = Application.StartupPath + @"\pic\Modelen.tmp";
            }
            string err = "";
            DataTable dt = TxtToDataTable(dataPath, '\t', ref err);

            if (dt == null) return;
            modelData = dt.DefaultView;
            //只取前12列
            DataTable newTable = modelData.ToTable(false, dt.Columns.Cast<DataColumn>().Take(12).Select(c => c.ColumnName).ToArray());
            this.dataGridView1.DataSource = newTable;

            //去除首列
            this.dataGridView1.RowHeadersVisible = false;
            //解决用户点击复选框，表格自动增加一行
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            #region 设置列宽
            this.dataGridView1.Columns[0].FillWeight = this.Width / 20 * 1;
            this.dataGridView1.Columns[1].FillWeight = this.Width / 20 * 2;
            this.dataGridView1.Columns[2].FillWeight = this.Width / 20 * 2;
            this.dataGridView1.Columns[3].FillWeight = this.Width / 20 * 2;
            this.dataGridView1.Columns[4].FillWeight = this.Width / 20 * 2;
            this.dataGridView1.Columns[5].FillWeight = this.Width / 20 * 4;
            this.dataGridView1.Columns[6].FillWeight = this.Width / 20 * 2;
            this.dataGridView1.Columns[7].FillWeight = this.Width / 20 * 2;
            this.dataGridView1.Columns[8].FillWeight = this.Width / 20 * 1;
            this.dataGridView1.Columns[9].FillWeight = this.Width / 20 * 1;
            this.dataGridView1.Columns[10].FillWeight = this.Width / 20 * 1;
            this.dataGridView1.Columns[11].FillWeight = this.Width / 20 * 2;
            #endregion

            for (int i = 0; i < this.dataGridView1.Columns.Count; i++)
            {
                this.dataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;//禁止排序
            }
        }

        //双击单元格打开表格
        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                string cellValue = this.dataGridView1.CurrentCell.Value.ToString();//鼠标双击获取的当前型号值
                DataRowView row = modelData.Cast<DataRowView>().FirstOrDefault(r => r[1].ToString() == cellValue);

                if (row != null)
                {
                    string documentName = row[12].ToString(); // 获取第10列的文档名称
                    string documentPath = Application.StartupPath + "\\pic\\" + documentName;
                    if (File.Exists(documentPath))
                    {
                        Process myProcess = new Process();
                        myProcess.StartInfo.FileName = documentPath;
                        myProcess.StartInfo.Verb = "Open";
                        myProcess.StartInfo.CreateNoWindow = true;
                        myProcess.Start();
                    }
                    else
                    {
                        MessageBox.Show("该型号产品暂无相关产品手册");
                    }
                }
            }
            catch
            {
                MessageBox.Show("该型号产品暂无相关产品手册");
            }
        }

        #region 方法--txt导入dgv
        /// <summary>
        /// 将Txt中数据读入DataTable中
        /// </summary>
        /// <param name="strFileName">文件名称</param>
        /// <param name="isHead">是否包含表头</param>
        /// <param name="strSplit">分隔符</param>
        /// <param name="strErrorMessage">错误信息</param>
        /// <returns>DataTable</returns>
        public static DataTable TxtToDataTable(string strFileName, char strSplit, ref string strErrorMessage)
        {
            DataTable dtReturn = new DataTable();

            try
            {
                string[] strFileTexts = File.ReadAllLines(strFileName, System.Text.Encoding.Default);// System.Text.Encoding.UTF8

                if (strFileTexts.Length == 0) // 如果没有数据
                {
                    strErrorMessage = "文件中没有数据！";
                    return null;
                }

                string[] strLineTexts = strFileTexts[0].Split(strSplit);
                if (strLineTexts.Length == 0)
                {
                    strErrorMessage = "文件中数据格式不正确！";
                    return null;
                }


                for (int i = 0; i < strLineTexts.Length; i++)
                {
                    if (i == 1)
                    {
                        dtReturn.Columns.Add(strLineTexts[i] + (MyDevice.languageType == 0 ? "(双击打开说明书)" : "(Double-click to open the manual)"));
                    }
                    else
                    {
                        dtReturn.Columns.Add(strLineTexts[i]);
                    }
                }

                for (int i = 1; i < strFileTexts.Length; i++)
                {
                    strLineTexts = strFileTexts[i].Split(strSplit);
                    DataRow dr = dtReturn.NewRow();
                    for (int j = 0; j < strLineTexts.Length; j++)
                    {
                        dr[j] = strLineTexts[j].ToString();
                    }
                    dtReturn.Rows.Add(dr);
                }
            }
            catch (Exception ex)
            {
                strErrorMessage = "读入数据出错！" + ex.Message;

                return null;
            }

            return dtReturn;
        }
        #endregion

    }
}
