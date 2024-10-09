using DBHelper;
using HZH_Controls;
using Model;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

//Ricardo 20240606

namespace Base.UI.MenuData
{
    public partial class MenuDataManageForm : Form
    {
        private List<DSDataGroup> groups = new List<DSDataGroup>();//数据组
        private List<DSData> targetDataList = new List<DSData>();  //数据集合
        private DataTable dataTable = new DataTable();             //原始数据表
        private DataTable cloneTable = new DataTable();            //克隆表（拷贝原始数据表，避免直接引用数据表）
        private DataTable tempTable1 = new DataTable();            //临时表（查询数据库时将表临时存储，避免返回上级目录再次查询数据库）
        private DataTable tempTable2 = new DataTable();            //临时表
        private int pageSize = 1000;                               //每页的数量
        private int pageIndex = 1;                                 //页码
        private int pageNum = 1;                                   //总页数
        private List<double> torqueList = new List<double>();      //扭矩集合（画曲线）
        private List<double> angleList = new List<double>();       //角度集合（画曲线）
        private VLine verticalLine;                                //纵轴
        private Text textAnnotation = null;                        //数据文本
        private string torUnit = "";                               //扭矩单位
        private bool isPeakShow = false;                           //是否是峰值展示模式
        private List<DSData> peakShowDataList = new List<DSData>();//峰值模式展示的数据集合（未筛选过）
        private List<DSData> peakFilterDataList = new List<DSData>();//筛选后的实际展示数据集合
        private bool isRecentDate = true;                          //是否筛选最近日期
        private bool isDataLoad = false;                           //是否数据加载中（加载中其他UI事件均失效）
        private int peakRecentDays = -1;                           //峰值模式下最近几天
        private DateTime selectDateTime = new DateTime();              //峰值模式下指定日期

        private Dictionary<byte, List<DSData>> DataDic = new Dictionary<byte, List<DSData>>(); //不同站点下的信息汇总

        public MenuDataManageForm()
        {
            InitializeComponent();
        }

        private void MenuDataManageForm_Load(object sender, EventArgs e)
        {
            if (GetShowType(MyDevice.userDAT + @"\DataShowType.txt") == "1")
            {
                btn_toggle.Text = "切换\n全局模式";
                btn_delete.Visible = true;
                isPeakShow = true;

                //峰值模式展示最近一天的峰值
                ShowPeakData();
            }
            else if (GetShowType(MyDevice.userDAT + @"\DataShowType.txt") == "0" || GetShowType(MyDevice.userDAT + @"\DataShowType.txt") == "")
            {
                btn_toggle.Text = "切换\n峰值模式";
                btn_delete.Visible = false;
                isPeakShow = false;

                //显示数据汇总表（根据工单名称）
                ShowSummaryData();
            }

            // 将BindingSource绑定到DataGridView
            dataGridView1.DataSource = bindingSource1;

            //新增切换模式——用于客户选择展示模式，峰值模式/全局模式
            btn_toggle.Visible = true;
            btn_toggle.Location = new System.Drawing.Point(btn_back.Location.X, btn_back.Location.Y);
            btn_back.Visible = false;

            //禁止更新后的表格排序
            for (int i = 0; i < this.dataGridView1.Columns.Count; i++)
            {
                dataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        //表格初始化
        private void datagridviewInit()
        {
            //表格初始化
            dataGridView1.Columns.Clear();
            dataGridView1.ClearSelection();
            dataGridView1.ReadOnly = true;//表格只读
            dataGridView1.EnableHeadersVisualStyles = false;//允许自定义行头样式
            dataGridView1.RowHeadersVisible = false; //第一列空白隐藏掉
            dataGridView1.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.AllowUserToAddRows = false;//禁止用户添加行
            dataGridView1.AllowUserToDeleteRows = false;//禁止用户删除行
            dataGridView1.AllowUserToOrderColumns = false;//禁止用户手动重新定位行
            dataGridView1.AllowUserToResizeRows = false;//禁止用户调整行大小
            dataGridView1.AllowUserToResizeColumns = false;//禁止用户调整列大小
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;//表格自动填充
        }

        //底部导航栏初始化
        private void bindingNavigatorInit()
        {
            this.bindingNavigator1.Visible = true;
            this.bindingNavigatorMoveFirstItem.Enabled = true;
            this.bindingNavigatorMovePreviousItem.Enabled = true;
            this.bindingNavigatorMoveNextItem.Enabled = true;
            this.bindingNavigatorMoveLastItem.Enabled = true;
            this.bindingNavigatorPositionItem.Text = pageIndex.ToString();
        }

        //分页
        private void DisplayPage()
        {
            cloneTable.Clear();

            int startIndex = (pageIndex - 1) * pageSize;//行首下标
            int endIndex = Math.Min(startIndex + pageSize, dataTable.Rows.Count);//行末下标

            for (int i = startIndex; i < endIndex; i++)
            {
                cloneTable.ImportRow(dataTable.Rows[i]);
            }

            bindingSource1.DataSource = cloneTable;
        }

        //上一页
        private void bindingNavigatorMovePreviousItem_Click(object sender, EventArgs e)
        {
            if (pageIndex > 1)
            {
                pageIndex--;
                DisplayPage();
                this.bindingNavigatorPositionItem.Text = pageIndex.ToString();
            }
        }

        //下一页
        private void bindingNavigatorMoveNextItem_Click(object sender, EventArgs e)
        {
            if (pageIndex < pageNum)
            {
                pageIndex++;
                DisplayPage();
                this.bindingNavigatorPositionItem.Text = pageIndex.ToString();
            }
        }

        //置顶页
        private void bindingNavigatorMoveFirstItem_Click(object sender, EventArgs e)
        {
            if (pageIndex > 1)
            {
                pageIndex = 1;
                DisplayPage();
                this.bindingNavigatorPositionItem.Text = pageIndex.ToString();
            }
        }

        //置底页
        private void bindingNavigatorMoveLastItem_Click(object sender, EventArgs e)
        {
            pageIndex = pageNum;
            DisplayPage();
            this.bindingNavigatorPositionItem.Text = pageIndex.ToString();
        }

        //自定义到指定页面
        private void toolStripTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            //只允许输入数字、删除键、回车键
            if (((e.KeyChar < '0') || (e.KeyChar > '9')) && (e.KeyChar != 8) && e.KeyChar != (char)Keys.Enter)
            {
                e.Handled = true;
                return;
            }

            // 如果第一位为0，且输入的不是删除键，则不允许输入
            if ((e.KeyChar != 8) && (((ToolStripTextBox)sender).Text == "0"))
            {
                e.Handled = true;
                return;
            }

            //长度限制3
            if ((e.KeyChar != 8) && ((ToolStripTextBox)sender).Text.Length >= 3)
            {
                e.Handled = true;
                return;
            }

            // 检查是否按下回车键
            if (e.KeyChar == (char)Keys.Enter)
            {
                if (int.TryParse(toolStripTextBox1.Text, out int targetPage))
                {
                    //有效页码范围内更新
                    if (targetPage > 0 && targetPage <= pageNum)
                    {
                        pageIndex = targetPage;
                        DisplayPage();
                        this.bindingNavigatorPositionItem.Text = pageIndex.ToString();
                    }
                }
            }
        }

        //双击显示指定数据
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (isDataLoad) return;

            if (e.RowIndex >= 0)
            {
                btn_filter.Visible = false;
                btn_delete.Visible = false;
            }

            //峰值模式下（双击只有一层过程数据）
            if (isPeakShow)
            {
                string peakVid;//峰值模式下的作业号

                if (e.RowIndex < 0) return;//防止点击列名报错

                if (dataGridView1.Columns[dataGridView1.Columns.Count - 1].HeaderText == "设备识别号" && dataGridView1.Columns[dataGridView1.Columns.Count - 2].HeaderText == "作业时间")
                {
                    peakVid = peakFilterDataList[e.RowIndex].VinId;
                    DateTime targetTime  = peakFilterDataList[e.RowIndex].CreateTime;
                    uint targetStamp     = peakFilterDataList[e.RowIndex].Stamp;
                    ulong targetBohrcode = peakFilterDataList[e.RowIndex].Bohrcode;
                    Byte targetAddr      = peakFilterDataList[e.RowIndex].DevAddr;

                    if (dataTable.Rows.Count > 0) dataTable.Clear();
                    //ShowProcessData(peakVid);
                    ShowProcessData(targetTime,targetStamp, targetAddr, targetBohrcode);
                }

                button2.Visible = true;
                button3.Visible = true;
                comboBox1.Visible = true;
            }
            //全局模式下（多层）
            else
            {
                DateTime targetTime;//当前表格的日期
                string targetWorkNum;//当前表格的工单号
                string targetSeqId; //当前表格的序列号
                string targetVid;//当前表格的作业号

                if (e.RowIndex < 0) return;//防止点击列名报错

                switch (dataGridView1.Columns[dataGridView1.Columns.Count - 1].HeaderText)
                {
                    case "工单名称":
                        targetWorkNum = dataGridView1.Rows[e.RowIndex].Cells[3].Value.ToString();
                        if (dataTable.Rows.Count > 0) dataTable.Clear();
                        ShowSummaryDataByWorkNum(targetWorkNum);

                        btn_toggle.Visible = false;
                        btn_back.Visible = true;
                        break;
                    case "工作日期":
                        targetWorkNum = dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString();
                        targetSeqId = dataGridView1.Rows[e.RowIndex].Cells[3].Value.ToString();
                        targetTime = dataGridView1.Rows[e.RowIndex].Cells[4].Value.ToDate();
                        if (dataTable.Rows.Count > 0) dataTable.Clear();
                        ShowSummaryDataByWorkNumAndSeqAndTime(targetWorkNum, targetSeqId, targetTime);
                        break;
                    case "作业号":
                        targetWorkNum = dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString();
                        targetSeqId = dataGridView1.Rows[e.RowIndex].Cells[3].Value.ToString();
                        targetTime = dataGridView1.Rows[e.RowIndex].Cells[4].Value.ToDate();
                        targetVid = dataGridView1.Rows[e.RowIndex].Cells[5].Value.ToString();
                        if (dataTable.Rows.Count > 0) dataTable.Clear();
                        ShowSummaryDataByDay(targetWorkNum, targetSeqId, targetTime, targetVid);
                        DisplayPage();
                        break;
                    default:
                        break;
                }
            }

            //禁止更新后的表格排序
            for (int i = 0; i < this.dataGridView1.Columns.Count; i++)
            {
                dataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        //数据总表——按日期排列
        private void ShowAllData()
        {
            datagridviewInit();
            bindingNavigator1.Visible = false;

            dataTable = new DataTable();

            // 添加列到DataTable
            dataTable.Columns.Add("序列号", typeof(int));
            dataTable.Columns.Add("日期", typeof(string));
            dataTable.Columns.Add("备注", typeof(int));

            groups = JDBC.GetAllDataGroup();
            int groupCnt = groups.Count;
            for (int i = 0; i < groupCnt; i++)
            {
                dataTable.Rows.Add(new object[] { i + 1, JDBC.GetAllDataGroup()[i].VinId, groups[i].WorkId });
            }
            bindingSource1.DataSource = dataTable;
        }

        //数据表——指定时间所有数据
        private void ShowTargetData(DateTime time)
        {
            //表格初始化
            datagridviewInit();
            bindingNavigatorInit();

            dataTable = new DataTable();

            // 添加列到DataTable
            dataTable.Columns.Add("序列号", typeof(int));
            dataTable.Columns.Add("时间标识", typeof(string));
            dataTable.Columns.Add("扭矩", typeof(int));
            dataTable.Columns.Add("扭矩峰值", typeof(int));
            dataTable.Columns.Add("角度", typeof(string));
            dataTable.Columns.Add("角度累加", typeof(int));

            //添加数据
            targetDataList.Clear();
            targetDataList = JDBC.GetDataByTime(time);
            int dataCnt = targetDataList.Count;
            pageNum = dataCnt % pageSize == 0 ? dataCnt / pageSize : dataCnt / pageSize + 1;//更新总页数
            bindingNavigatorCountItem.Text = pageNum.ToString();

            // 开始批量加载数据（提高批量加载数据的性能）
            dataTable.BeginLoadData();

            for (int i = 0; i < dataCnt; i++)
            {
                dataTable.Rows.Add(new object[] { i + 1, targetDataList[i].Stamp, targetDataList[i].Torque, targetDataList[i].TorquePeak, targetDataList[i].Angle, targetDataList[i].AngleAcc });
            }

            // 结束批量加载数据
            dataTable.EndLoadData();

            // 克隆表结构，直接操作dataTable会影响有效数据引用
            cloneTable = dataTable.Clone();
        }

        #region 全局模式数据展示

        //数据汇总表
        private void ShowSummaryData()
        {
            btn_back.Visible = false;
            btn_toggle.Visible = true;
            btn_filter.Visible = false;
            button2.Visible = false;
            datagridviewInit();
            bindingNavigator1.Visible = false;
            dataTable = new DataTable();

            // 添加列到DataTable
            dataTable.Columns.Add("序号", typeof(int));
            dataTable.Columns.Add("数据类型", typeof(string));
            dataTable.Columns.Add("创建时间", typeof(string));
            dataTable.Columns.Add("工单名称", typeof(string));

            //获取汇总表
            List<DSDataSummary> summaryList = JDBC.GetAllDataSummary();
            if (summaryList != null && summaryList.Count != 0)
            {
                //
                for (int i = 0; i < summaryList.Count; i++)
                {
                    dataTable.Rows.Add(new object[] { i + 1,
                                                  summaryList[i].DataType == "ActualData" ? "实时数据": "工单数据",
                                                  summaryList[i].CreateTime,
                                                  summaryList[i].WorkNum,
                    });
                }
            }

            bindingSource1.DataSource = dataTable;
        }

        //数据汇总表（依据工单）
        private void ShowSummaryDataByWorkNum(string workNum)
        {
            datagridviewInit();
            bindingNavigator1.Visible = false;
            dataTable = new DataTable();

            // 添加列到DataTable
            dataTable.Columns.Add("序号", typeof(int));
            dataTable.Columns.Add("数据类型", typeof(string));
            dataTable.Columns.Add("工单名称", typeof(string));
            dataTable.Columns.Add("序列号", typeof(string));
            dataTable.Columns.Add("工作日期", typeof(string));

            // 获取汇总表
            // 查询特定列的数据
            var dataList = JDBC.GetSpecificDataByWorkNumGroupedBySeqIdAndDate(workNum);

            var filteredList = dataList.AsParallel()
                                       .OrderBy(d => d.SequenceId)
                                       .ThenBy(d => d.CreateTime.Date)
                                       .ToList(); //使用 PLINQ（Parallel LINQ）来并行处理查询，以利用多核 CPU 的优势筛选

            if (filteredList != null && filteredList.Count != 0)
            {
                //
                for (int i = 0; i < filteredList.Count; i++)
                {
                    dataTable.Rows.Add(new object[] { i + 1,
                                                  filteredList[i].DataType == "ActualData" ? "实时数据": "工单数据",
                                                  filteredList[i].WorkNum,
                                                  filteredList[i].SequenceId,
                                                  filteredList[i].CreateTime.ToString("yyyy-MM-dd"),
                    });
                }
            }

            bindingSource1.DataSource = dataTable;

            //将读取数据库的表深拷贝，避免下次返回目录二次查询，减少时间损耗
            tempTable1.Clear();
            tempTable1 = dataTable.Copy();
        }

        //数据汇总表（依据工单, 序列号和时间）
        private void ShowSummaryDataByWorkNumAndSeqAndTime(string workNum, string sequenceId, DateTime time)
        {
            datagridviewInit();
            bindingNavigator1.Visible = false;
            dataTable = new DataTable();

            // 添加列到DataTable
            dataTable.Columns.Add("序号", typeof(int));
            dataTable.Columns.Add("数据类型", typeof(string));
            dataTable.Columns.Add("工单名称", typeof(string));
            dataTable.Columns.Add("序列号", typeof(string));
            dataTable.Columns.Add("工作日期", typeof(string));
            dataTable.Columns.Add("作业号", typeof(string));

            //获取汇总表
            var dataList = JDBC.GetSpecificDataByWorkNumdSeqIdAndDateGroupByVinId(workNum, sequenceId, time);

            var filteredList = dataList.AsParallel()
                           .AsOrdered()
                           .ToList(); //使用 PLINQ（Parallel LINQ）来并行处理查询，以利用多核 CPU 的优势筛选

            if (filteredList != null && filteredList.Count != 0)
            {
                //
                for (int i = 0; i < filteredList.Count; i++)
                {
                    dataTable.Rows.Add(new object[] { i + 1,
                                                  filteredList[i].DataType == "ActualData" ? "实时数据": "工单数据",
                                                  filteredList[i].WorkNum,
                                                  filteredList[i].SequenceId,
                                                  filteredList[i].CreateTime.ToString("yyyy-MM-dd"),
                                                  filteredList[i].VinId.ToString()
                    });
                }
            }

            bindingSource1.DataSource = dataTable;

            //将读取数据库的表深拷贝，避免下次返回目录二次查询，减少时间损耗
            tempTable2.Clear();
            tempTable2 = dataTable.Copy();
        }

        //数据汇总表（依据工单，序列号，日期，作业号）
        private void ShowSummaryDataByDay(string workNum, string sequenceId, DateTime day, string vid)
        {
            int filteredDataCnt = 0;//筛选后的数据数量

            datagridviewInit();
            bindingNavigatorInit();
            pageIndex = 1;
            this.bindingNavigatorPositionItem.Text = pageIndex.ToString();
            dataTable = new DataTable();
            torqueList.Clear();
            angleList.Clear();
            DataDic.Clear();
            button3.Visible = true;
            button2.Visible = true;

            // 添加列到DataTable
            dataTable.Columns.Add("序号", typeof(int));
            dataTable.Columns.Add("工单名称", typeof(string));
            dataTable.Columns.Add("序列号", typeof(string));
            dataTable.Columns.Add("作业号", typeof(string));
            dataTable.Columns.Add("设备唯一编号", typeof(ulong));
            dataTable.Columns.Add("设备型号", typeof(string));
            dataTable.Columns.Add("点位号", typeof(string));
            dataTable.Columns.Add("设备站点", typeof(byte));
            dataTable.Columns.Add("创建时间", typeof(string));
            dataTable.Columns.Add("数据类型", typeof(string));
            dataTable.Columns.Add("时间标识", typeof(string));
            dataTable.Columns.Add("扭矩", typeof(string));
            dataTable.Columns.Add("扭矩峰值", typeof(double));
            dataTable.Columns.Add("角度", typeof(string));
            dataTable.Columns.Add("角度累加", typeof(double));
            dataTable.Columns.Add("拧紧结果", typeof(string));

            //获取汇总表
            var filteredList = JDBC.GetSpecificDataByWoNumAndSeIdAndTimeAndVid(workNum, sequenceId, vid, day);

            if (filteredList != null && filteredList.Count != 0)
            {
                filteredDataCnt = filteredList.Count;

                // 开始批量加载数据（提高批量加载数据的性能）
                dataTable.BeginLoadData();
                //
                for (int i = 0; i < filteredDataCnt; i++)
                {
                    //单位更新
                    switch (filteredList[i].TorqueUnit)
                    {
                        case "UNIT_nm": torUnit = "N·m"; break;
                        case "UNIT_lbfin": torUnit = "lbf·in"; break;
                        case "UNIT_lbfft": torUnit = "lbf·ft"; break;
                        case "UNIT_kgcm": torUnit = "kgf·cm"; break;
                        case "UNIT_kgm": torUnit = "kgf·m"; break;
                        default: break;
                    }

                    dataTable.Rows.Add(new object[] { i + 1,
                                                  filteredList[i].WorkNum,
                                                  filteredList[i].SequenceId,
                                                  filteredList[i].VinId,
                                                  filteredList[i].Bohrcode,
                                                  filteredList[i].DevType,
                                                  filteredList[i].PointNum,
                                                  filteredList[i].DevAddr,
                                                  filteredList[i].CreateTime,
                                                  filteredList[i].DType,
                                                  filteredList[i].Stamp,
                                                  filteredList[i].Torque + " " + torUnit,
                                                  filteredList[i].TorquePeak,
                                                  filteredList[i].Angle,
                                                  filteredList[i].AngleAcc,
                                                  filteredList[i].DataResult,
                    });
                    torqueList.Add(filteredList[i].Torque);
                    angleList.Add(filteredList[i].Angle);

                    //不同站点分配不同的List
                    if (!DataDic.ContainsKey(filteredList[i].DevAddr))
                    {
                        DataDic.Add(filteredList[i].DevAddr, new List<DSData>());
                    }
                    DataDic[filteredList[i].DevAddr].Add(filteredList[i]);
                }

                // 结束批量加载数据
                dataTable.EndLoadData();
            }

            pageNum = filteredDataCnt % pageSize == 0 ? filteredDataCnt / pageSize : filteredDataCnt / pageSize + 1;//更新总页数
            bindingNavigatorCountItem.Text = pageNum.ToString();

            // 克隆表结构，直接操作dataTable会影响有效数据引用
            cloneTable = dataTable.Clone();
        }

        #endregion

        #region 峰值模式数据展示

        //显示峰值数据(默认最近一天)
        private void ShowPeakData()
        {
            btn_filter.Visible = true;
            button2.Visible = true;

            datagridviewInit();
            bindingNavigator1.Visible = false;
            dataTable = new DataTable();

            // 添加列到DataTable
            dataTable.Columns.Add("是否选择", typeof(bool));
            dataTable.Columns.Add("序号", typeof(int));
            dataTable.Columns.Add("作业号", typeof(string));
            dataTable.Columns.Add("工单编号", typeof(string));
            dataTable.Columns.Add("序列号", typeof(string));
            dataTable.Columns.Add("点位号", typeof(string));
            dataTable.Columns.Add("标准扭矩", typeof(string));
            dataTable.Columns.Add("标准角度", typeof(string));
            dataTable.Columns.Add("峰值扭矩", typeof(string));
            dataTable.Columns.Add("峰值角度", typeof(string));
            dataTable.Columns.Add("扭矩结果", typeof(string));
            dataTable.Columns.Add("角度结果", typeof(string));
            dataTable.Columns.Add("作业时间", typeof(string));
            //dataTable.Columns.Add("时间标识", typeof(string));
            //dataTable.Columns.Add("设备编号", typeof(string));
            //dataTable.Columns.Add("设备型号", typeof(string));
            dataTable.Columns.Add("设备识别号", typeof(string));

            // 获取最近一天的数据表
            peakShowDataList = JDBC.GetDataByRecent(1);

            //var filteredList = peakShowDataList.Where(x => !string.IsNullOrEmpty(x.VinId))                   // 作业号不为空，提前筛选减少数据量，减少并发量
            //                                   .AsParallel()                                                 // 并发执行
            //                                   .AsOrdered()                                                  // 使得并发按照顺序执行
            //                                   .GroupBy(x => x.VinId)                                        // 按属性a分组
            //                                   .Select(g => g.OrderByDescending(x => x.TorquePeak).First())  // 取出每组中的最大值
            //                                   .ToList(); //使用 PLINQ（Parallel LINQ）来并行处理查询，以利用多核 CPU 的优势筛选

            var filteredList = peakShowDataList
                                               .Where(x => !string.IsNullOrEmpty(x.VinId))  // 过滤作业号不为空的数据
                                               .Where(x => x.DType == 242 && x.DevType.Contains("5") ||
                                                           x.DType == 242 && x.DevType.Contains("6") ||
                                                           x.DType == 243 && x.DevType.Contains("7") ||
                                                           x.DType == 243 && x.DevType.Contains("8") ||
                                                           x.DType == 243 && x.DevType.Contains("9"))// 只保留满足条件的分组
                                               .AsParallel()
                                               .AsOrdered()  // 保证并发按照顺序执行                       
                                               .ToList();

            if (filteredList != null && filteredList.Count != 0)
            {
                string tempUint = "N·m";
                //
                for (int i = 0; i < filteredList.Count; i++)
                {
                    //单位更新
                    switch (filteredList[i].TorqueUnit)
                    {
                        case "UNIT_nm": tempUint = "N·m"; break;
                        case "UNIT_lbfin": tempUint = "lbf·in"; break;
                        case "UNIT_lbfft": tempUint = "lbf·ft"; break;
                        case "UNIT_kgcm": tempUint = "kgf·cm"; break;
                        case "UNIT_kgm": tempUint = "kgf·m"; break;
                        default: break;
                    }

                    dataTable.Rows.Add(new object[] { false ,i + 1,
                                                  filteredList[i].VinId,
                                                  filteredList[i].DataType == "ActualData" ? "": filteredList[i].WorkNum.ToString(),
                                                  filteredList[i].DataType == "ActualData" ? "": filteredList[i].SequenceId.ToString(),
                                                  filteredList[i].DataType == "ActualData" ? "": filteredList[i].PointNum.ToString(),
                                                  filteredList[i].Torque,
                                                  filteredList[i].Angle,
                                                  filteredList[i].TorquePeak + " " + tempUint,
                                                  filteredList[i].AngleAcc,
                                                  filteredList[i].DataResult,
                                                  filteredList[i].DataResult,
                                                  filteredList[i].CreateTime,
                                                  //filteredList[i].Stamp,
                                                  //filteredList[i].Bohrcode,
                                                  //filteredList[i].DevType,
                                                  filteredList[i].DevAddr,
                    });
                }
            }

            bindingSource1.DataSource = dataTable;

            //深拷贝筛选集合
            peakFilterDataList = new List<DSData>(filteredList.Count);

            foreach (DSData item in filteredList)
            {
                peakFilterDataList.Add(item); 
            }

            //将读取数据库的表深拷贝，避免下次返回目录二次查询，减少时间损耗
            tempTable1.Clear();
            tempTable1 = dataTable.Copy();
        }

        //显示峰值数据(最近几天)
        private void ShowPeakData(int recentDays)
        {
            // 获取最近n天的数据表(耗时操作可以其他线程执行)
            peakShowDataList = JDBC.GetDataByRecent(recentDays);

            // UI操作部分必须在主线程中执行
            this.Invoke(new Action(() => {
                btn_filter.Visible = true;
                button2.Visible = true;

                datagridviewInit();
                bindingNavigator1.Visible = false;
                dataTable = new DataTable();

                // 添加列到DataTable
                dataTable.Columns.Add("是否选择", typeof(bool));
                dataTable.Columns.Add("序号", typeof(int));
                dataTable.Columns.Add("作业号", typeof(string));
                dataTable.Columns.Add("工单编号", typeof(string));
                dataTable.Columns.Add("序列号", typeof(string));
                dataTable.Columns.Add("点位号", typeof(string));
                dataTable.Columns.Add("标准扭矩", typeof(string));
                dataTable.Columns.Add("标准角度", typeof(string));
                dataTable.Columns.Add("峰值扭矩", typeof(string));
                dataTable.Columns.Add("峰值角度", typeof(string));
                dataTable.Columns.Add("扭矩结果", typeof(string));
                dataTable.Columns.Add("角度结果", typeof(string));
                dataTable.Columns.Add("作业时间", typeof(string));
                //dataTable.Columns.Add("时间标识", typeof(string));
                //dataTable.Columns.Add("设备编号", typeof(string));
                //dataTable.Columns.Add("设备型号", typeof(string));
                dataTable.Columns.Add("设备识别号", typeof(string));

                var filteredList = peakShowDataList
                                                   .Where(x => !string.IsNullOrEmpty(x.VinId))  // 过滤作业号不为空的数据
                                                   .Where(x => x.DType == 242 && x.DevType.Contains("5") ||
                                                               x.DType == 242 && x.DevType.Contains("6") ||
                                                               x.DType == 243 && x.DevType.Contains("7") ||
                                                               x.DType == 243 && x.DevType.Contains("8") ||
                                                               x.DType == 243 && x.DevType.Contains("9"))// 只保留满足条件的分组
                                                   .AsParallel()
                                                   .AsOrdered()  // 保证并发按照顺序执行                       
                                                   .ToList();

                if (filteredList != null && filteredList.Count != 0)
                {
                    string tempUint = "N·m";
                    //
                    for (int i = 0; i < filteredList.Count; i++)
                    {
                        //单位更新
                        switch (filteredList[i].TorqueUnit)
                        {
                            case "UNIT_nm": tempUint = "N·m"; break;
                            case "UNIT_lbfin": tempUint = "lbf·in"; break;
                            case "UNIT_lbfft": tempUint = "lbf·ft"; break;
                            case "UNIT_kgcm": tempUint = "kgf·cm"; break;
                            case "UNIT_kgm": tempUint = "kgf·m"; break;
                            default: break;
                        }

                        dataTable.Rows.Add(new object[] { false, i + 1,
                                                  filteredList[i].VinId,
                                                  filteredList[i].DataType == "ActualData" ? "": filteredList[i].WorkNum.ToString(),
                                                  filteredList[i].DataType == "ActualData" ? "": filteredList[i].SequenceId.ToString(),
                                                  filteredList[i].DataType == "ActualData" ? "": filteredList[i].PointNum.ToString(),
                                                  filteredList[i].Torque,
                                                  filteredList[i].Angle,
                                                  filteredList[i].TorquePeak + " " + tempUint,
                                                  filteredList[i].AngleAcc,
                                                  filteredList[i].DataResult,
                                                  filteredList[i].DataResult,
                                                  filteredList[i].CreateTime,
                                                  //filteredList[i].Stamp,
                                                  //filteredList[i].Bohrcode,
                                                  //filteredList[i].DevType,
                                                  filteredList[i].DevAddr,
                        });
                    }
                }

                bindingSource1.DataSource = dataTable;

                //深拷贝筛选集合
                peakFilterDataList = new List<DSData>(filteredList.Count);

                foreach (DSData item in filteredList)
                {
                    peakFilterDataList.Add(item);
                }

                //将读取数据库的表深拷贝，避免下次返回目录二次查询，减少时间损耗
                tempTable1.Clear();
                tempTable1 = dataTable.Copy();
            }));
            
        }

        //显示峰值数据(指定日期)
        private void ShowPeakData(DateTime selectDate)
        {
            // 获取最近n天的数据表
            peakShowDataList = JDBC.GetDataByTime(selectDate);

            this.Invoke(new Action(() => {
                btn_filter.Visible = true;
                button2.Visible = true;

                datagridviewInit();
                bindingNavigator1.Visible = false;
                dataTable = new DataTable();

                // 添加列到DataTable
                dataTable.Columns.Add("是否选择", typeof(bool));
                dataTable.Columns.Add("序号", typeof(int));
                dataTable.Columns.Add("作业号", typeof(string));
                dataTable.Columns.Add("工单编号", typeof(string));
                dataTable.Columns.Add("序列号", typeof(string));
                dataTable.Columns.Add("点位号", typeof(string));
                dataTable.Columns.Add("标准扭矩", typeof(string));
                dataTable.Columns.Add("标准角度", typeof(string));
                dataTable.Columns.Add("峰值扭矩", typeof(string));
                dataTable.Columns.Add("峰值角度", typeof(string));
                dataTable.Columns.Add("扭矩结果", typeof(string));
                dataTable.Columns.Add("角度结果", typeof(string));
                dataTable.Columns.Add("作业时间", typeof(string));
                //dataTable.Columns.Add("时间标识", typeof(string));
                //dataTable.Columns.Add("设备编号", typeof(string));
                //dataTable.Columns.Add("设备型号", typeof(string));
                dataTable.Columns.Add("设备识别号", typeof(string));

                var filteredList = peakShowDataList
                                                   .Where(x => !string.IsNullOrEmpty(x.VinId))  // 过滤作业号不为空的数据
                                                   .Where(x => x.DType == 242 && x.DevType.Contains("5") || 
                                                               x.DType == 242 && x.DevType.Contains("6") ||
                                                               x.DType == 243 && x.DevType.Contains("7") ||
                                                               x.DType == 243 && x.DevType.Contains("8") ||
                                                               x.DType == 243 && x.DevType.Contains("9") )// 只保留满足条件的分组
                                                   .AsParallel()
                                                   .AsOrdered()  // 保证并发按照顺序执行                       
                                                   .ToList();

                if (filteredList != null && filteredList.Count != 0)
                {
                    string tempUint = "N·m";
                    //
                    for (int i = 0; i < filteredList.Count; i++)
                    {
                        //单位更新
                        switch (filteredList[i].TorqueUnit)
                        {
                            case "UNIT_nm": tempUint = "N·m"; break;
                            case "UNIT_lbfin": tempUint = "lbf·in"; break;
                            case "UNIT_lbfft": tempUint = "lbf·ft"; break;
                            case "UNIT_kgcm": tempUint = "kgf·cm"; break;
                            case "UNIT_kgm": tempUint = "kgf·m"; break;
                            default: break;
                        }

                        dataTable.Rows.Add(new object[] { false, i + 1,
                                                  filteredList[i].VinId,
                                                  filteredList[i].DataType == "ActualData" ? "": filteredList[i].WorkNum.ToString(),
                                                  filteredList[i].DataType == "ActualData" ? "": filteredList[i].SequenceId.ToString(),
                                                  filteredList[i].DataType == "ActualData" ? "": filteredList[i].PointNum.ToString(),
                                                  filteredList[i].Torque,
                                                  filteredList[i].Angle,
                                                  filteredList[i].TorquePeak + " " + tempUint,
                                                  filteredList[i].AngleAcc,
                                                  filteredList[i].DataResult,
                                                  filteredList[i].DataResult,
                                                  filteredList[i].CreateTime,
                                                  //filteredList[i].Stamp,
                                                  //filteredList[i].Bohrcode,
                                                  //filteredList[i].DevType,
                                                  filteredList[i].DevAddr,
                        });
                    }
                }

                bindingSource1.DataSource = dataTable;

                //深拷贝筛选集合
                peakFilterDataList = new List<DSData>(filteredList.Count);

                foreach (DSData item in filteredList)
                {
                    peakFilterDataList.Add(item);
                }

                //将读取数据库的表深拷贝，避免下次返回目录二次查询，减少时间损耗
                tempTable1.Clear();
                tempTable1 = dataTable.Copy();
            }));
        }

        //显示峰值模式下过程数据(根据作业号)
        private void ShowProcessData(string peakVid)
        {
            btn_back.Visible = true;
            btn_toggle.Visible = false;

            DataDic.Clear();
            datagridviewInit();
            bindingNavigator1.Visible = false;
            dataTable = new DataTable();

            // 添加列到DataTable
            dataTable.Columns.Add("序号", typeof(int));
            dataTable.Columns.Add("作业号", typeof(string));
            dataTable.Columns.Add("工单编号", typeof(string));
            dataTable.Columns.Add("序列号", typeof(string));
            dataTable.Columns.Add("点位号", typeof(string));
            dataTable.Columns.Add("标准扭矩", typeof(string));
            dataTable.Columns.Add("标准角度", typeof(string));
            dataTable.Columns.Add("作业时间", typeof(string));
            dataTable.Columns.Add("时间标识", typeof(string));
            dataTable.Columns.Add("设备识别号", typeof(string));

            //根据作业号筛选过程数据
            var filteredList = peakShowDataList.AsParallel()
                                               .AsOrdered()
                                               .Where(x => x.VinId == peakVid)
                                               .ToList(); //使用 PLINQ（Parallel LINQ）来并行处理查询，以利用多核 CPU 的优势筛选


            if (filteredList != null && filteredList.Count != 0)
            {
                string processUint = "N·m";
                //
                for (int i = 0; i < filteredList.Count; i++)
                {
                    //单位更新
                    switch (filteredList[i].TorqueUnit)
                    {
                        case "UNIT_nm": processUint = "N·m"; break;
                        case "UNIT_lbfin": processUint = "lbf·in"; break;
                        case "UNIT_lbfft": processUint = "lbf·ft"; break;
                        case "UNIT_kgcm": processUint = "kgf·cm"; break;
                        case "UNIT_kgm": processUint = "kgf·m"; break;
                        default: break;
                    }

                    dataTable.Rows.Add(new object[] { i + 1,
                                                  filteredList[i].VinId,
                                                  filteredList[i].DataType == "ActualData" ? "": filteredList[i].WorkNum.ToString(),
                                                  filteredList[i].DataType == "ActualData" ? "": filteredList[i].SequenceId.ToString(),
                                                  filteredList[i].DataType == "ActualData" ? "": filteredList[i].PointNum.ToString(),
                                                  filteredList[i].Torque + " " + processUint,
                                                  filteredList[i].Angle,
                                                  filteredList[i].CreateTime,
                                                  filteredList[i].Stamp,
                                                  filteredList[i].DevAddr,
                    });

                    //不同站点分配不同的List
                    if (!DataDic.ContainsKey(filteredList[i].DevAddr))
                    {
                        DataDic.Add(filteredList[i].DevAddr, new List<DSData>());
                    }
                    DataDic[filteredList[i].DevAddr].Add(filteredList[i]);
                }
            }

            bindingSource1.DataSource = dataTable;

            //深拷贝筛选集合
            peakFilterDataList = new List<DSData>(filteredList.Count);

            foreach (DSData item in filteredList)
            {
                peakFilterDataList.Add(item);
            }
        }

        //显示峰值模式下过程数据(根据日期，时间标识，地址，bohrcode)
        private void ShowProcessData(DateTime time, uint stamp, byte addr, ulong bohrcode)
        {
            btn_back.Visible = true;
            btn_toggle.Visible = false;

            DataDic.Clear();
            datagridviewInit();
            bindingNavigator1.Visible = false;
            dataTable = new DataTable();

            // 添加列到DataTable
            dataTable.Columns.Add("序号", typeof(int));
            dataTable.Columns.Add("作业号", typeof(string));
            dataTable.Columns.Add("工单编号", typeof(string));
            dataTable.Columns.Add("序列号", typeof(string));
            dataTable.Columns.Add("点位号", typeof(string));
            dataTable.Columns.Add("标准扭矩", typeof(string));
            dataTable.Columns.Add("标准角度", typeof(string));
            dataTable.Columns.Add("作业时间", typeof(string));
            dataTable.Columns.Add("时间标识", typeof(string));
            dataTable.Columns.Add("设备识别号", typeof(string));

            //筛选过程数据
            var filteredList = GroupData(peakShowDataList, time, stamp, addr, bohrcode);

            if (filteredList != null && filteredList.Count != 0)
            {
                string processUint = "N·m";
                //
                for (int i = 0; i < filteredList.Count; i++)
                {
                    //单位更新
                    switch (filteredList[i].TorqueUnit)
                    {
                        case "UNIT_nm": processUint = "N·m"; break;
                        case "UNIT_lbfin": processUint = "lbf·in"; break;
                        case "UNIT_lbfft": processUint = "lbf·ft"; break;
                        case "UNIT_kgcm": processUint = "kgf·cm"; break;
                        case "UNIT_kgm": processUint = "kgf·m"; break;
                        default: break;
                    }

                    dataTable.Rows.Add(new object[] { i + 1,
                                                  filteredList[i].VinId,
                                                  filteredList[i].DataType == "ActualData" ? "": filteredList[i].WorkNum.ToString(),
                                                  filteredList[i].DataType == "ActualData" ? "": filteredList[i].SequenceId.ToString(),
                                                  filteredList[i].DataType == "ActualData" ? "": filteredList[i].PointNum.ToString(),
                                                  filteredList[i].Torque + " " + processUint,
                                                  filteredList[i].Angle,
                                                  filteredList[i].CreateTime,
                                                  filteredList[i].Stamp,
                                                  filteredList[i].DevAddr,
                    });

                    //不同站点分配不同的List
                    if (!DataDic.ContainsKey(filteredList[i].DevAddr))
                    {
                        DataDic.Add(filteredList[i].DevAddr, new List<DSData>());
                    }
                    DataDic[filteredList[i].DevAddr].Add(filteredList[i]);
                }
            }

            bindingSource1.DataSource = dataTable;

            //将读取数据库的表深拷贝，避免下次返回目录二次查询，减少时间损耗
            tempTable2.Clear();
            tempTable2 = dataTable.Copy();
        }

        //获取峰值对应下的过程数据
        public List<DSData> GroupData(List<DSData> dataList, DateTime time, uint stamp,  byte addr, ulong bohrcode)
        {
            var targetDataList = new List<DSData>();

            Int32 foundIndex = (Int32)(dataList
                             .AsParallel()
                             .AsOrdered()
                             .Select((item, idx) => new { item, idx })
                             .FirstOrDefault(x => x.item.CreateTime == time 
                                               && x.item.Stamp == stamp 
                                               && x.item.DevAddr == addr 
                                               && x.item.Bohrcode == bohrcode)
                                               ?.idx ?? -1);

            if (foundIndex != -1)
            {
                int dtype = dataList[foundIndex].DType;//数据类型
                //倒序查找
                for (int i = foundIndex - 1; i > 0; i--)
                {
                    if (dataList[i].DevAddr == addr && dataList[i].Bohrcode == bohrcode && dataList[i].VinId != "")
                    {
                        if (dataList[i].DType == dtype)
                        {
                            break;//上一个03/02退出
                        }

                        //F3峰值下的过程数据抛去F2过程峰值
                        if (dtype == 0xF3)
                        {
                            if (dataList[i].DType != 0xF2)
                            {
                                targetDataList.Add(dataList[i]);
                            }
                        }
                        else
                        {
                            targetDataList.Add(dataList[i]);
                        }
                    }
                    else
                    {
                        break;//不是一个站点或者不是一个bohrcode
                    }
                }
            }

            targetDataList.Reverse();//先前倒序查找，故恢复

            return targetDataList;
        }

        #endregion

        //返回
        private void btn_back_Click(object sender, EventArgs e)
        {
            if (isPeakShow)
            {
                if (dataGridView1.Columns[dataGridView1.Columns.Count - 1].HeaderText == "设备识别号")
                {

                    if (dataTable.Rows.Count > 0) dataTable.Clear();

                    if (formsPlot1.Visible == false)
                    {
                        btn_back.Visible = false;
                        btn_toggle.Visible = true;
                        btn_filter.Visible = true;
                        btn_delete.Visible = true;

                        button2.Visible = true;
                        button3.Visible = false;
                        comboBox1.Visible = false;
                        bindingSource1.DataSource = tempTable1;//返回顶层表

                        dataTable = tempTable1.Copy();

                        AddCheckBoxCol();

                        //获取列的顺序，防止dataGridview展示的内容列的顺序与dataTable不一致
                        for (int i = 0; i < dataTable.Columns.Count; i++)
                        {
                            dataGridView1.Columns[dataTable.Columns[i].ColumnName].DisplayIndex = i;
                        }
                    }
                    else
                    {
                        btn_back.Visible = true;
                        btn_toggle.Visible = false;
                        btn_filter.Visible = false;
                        btn_delete.Visible = false;

                        button2.Visible = true;
                        button3.Visible = true;
                        comboBox1.Visible = true;
                        bindingSource1.DataSource = tempTable2;//返回当前曲线对应的数据表

                        dataTable = tempTable2.Copy();
                    }

                    this.formsPlot1.Plot.Clear();
                    this.formsPlot1.Visible = false;
                    this.comboBox1.Visible = false;

                    //禁止更新后的表格排序
                    for (int i = 0; i < this.dataGridView1.Columns.Count; i++)
                    {
                        dataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                    }
                    return;
                }
            }
            else
            {
                switch (dataGridView1.Columns[dataGridView1.Columns.Count - 1].HeaderText)
                {
                    case "工作日期":
                        if (dataTable.Rows.Count > 0) dataTable.Clear();

                        //显示数据汇总表（根据工单名称）
                        ShowSummaryData();
                        break;
                    case "作业号":
                        button3.Visible = false;

                        if (dataGridView1.RowCount == 0)
                        {
                            //显示数据汇总表（根据工单名称）
                            ShowSummaryData();
                        }
                        else
                        {
                            string targetWorkNum = dataGridView1.Rows[0].Cells[2].Value.ToString();

                            if (dataTable.Rows.Count > 0) dataTable.Clear();

                            //ShowSummaryDataByWorkNum(targetWorkNum);//数据库查找
                            bindingSource1.DataSource = tempTable1;//调用上次数据库查询的表，减少查询损耗
                        }

                        break;
                    case "拧紧结果":
                        if (comboBox1.Visible == false)
                        {
                            button3.Visible = false;
                            string selectWorkNum = dataGridView1.Rows[0].Cells[1].Value.ToString();
                            string selectSeqId = dataGridView1.Rows[0].Cells[2].Value.ToString();
                            DateTime selectTime = dataGridView1.Rows[0].Cells[8].Value.ToDate();
                            if (dataTable.Rows.Count > 0) dataTable.Clear();

                            //ShowSummaryDataByWorkNumAndSeqAndTime(selectWorkNum, selectSeqId, selectTime);
                            bindingSource1.DataSource = tempTable2;//调用上次数据库查询的表，减少查询损耗
                        }
                        else
                        {
                            comboBox1.Visible = false;
                            this.formsPlot1.Visible = false;
                        }
                        break;
                    default:
                        break;
                }
            }

            //禁止更新后的表格排序
            for (int i = 0; i < this.dataGridView1.Columns.Count; i++)
            {
                dataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            this.formsPlot1.Plot.Clear();
            this.formsPlot1.Visible = false;
            this.comboBox1.Visible = false;
            this.button2.Visible = this.button3.Visible;
        }

        //将数据转存为csv
        private void button2_Click(object sender, EventArgs e)
        {
            if (dataTable.Rows.Count < 1)
            {
                MessageBox.Show("无有效数据，无法转存文件");
                return;
            }

            // 获取当前用户的桌面路径
            //string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            //文件名称
            string filename = "";

            if (isPeakShow)
            {
                if (dataTable.Rows[0][1].ToString() != "")
                {
                    filename = dataTable.Rows[0][1].ToString();
                }
                else
                {
                    filename = DateTime.Now.ToString("yyyyMMddHHmm");
                }
            }
            else
            {
                if (dataTable.Rows[0][3].ToString() != "")
                {
                    filename = dataTable.Rows[0][3].ToString();
                }
                else
                {
                    filename = DateTime.Now.ToString("yyyyMMddHHmm");
                }
            }

            // 创建保存文件对话框
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                Title = "保存csv文件",
                DefaultExt = "csv",
                FileName = $"XhTorque数据汇总表{filename}.csv"
            };

            // 显示对话框并获取结果
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                // 获取用户选择的文件路径
                string filePath = saveFileDialog.FileName;

                // 将DataTable数据转换为CSV文件并保存到用户选择的路径
                DataTableToCsvFile(dataTable, filePath);
            }
        }

        //显示曲线
        private async void button3_Click(object sender, EventArgs e)
        {
            comboBox1.Visible = true;
            comboBox1.SelectedIndex = 2;//自动切换到扭矩角度曲线
            formsPlot1.Visible = true;
            formsPlot1.Dock = DockStyle.Fill;
            formsPlot1.Configuration.DoubleClickBenchmark = false;// 禁用渲染时间提示框(左下角)
            await UpdatePlotAsync();//加载大量数据时会出现卡顿现象，使用异步可以解决卡顿
            formsPlot1.MouseUp += FormsPlot1_MouseDoubleClick;
        }

        //异步更新曲线
        private async Task UpdatePlotAsync()
        {
            // 禁用UI元素
            comboBox1.Enabled = false;
            formsPlot1.Enabled = false;

            try
            {
                //await异步 添加启动task线程
                //新开了线程，所以必须添加this.Invoke确保在UI线程更新
                await Task.Run(() => this.Invoke(new Action(() => {

                    formsPlot1.Plot.Clear();
                    double _xMin = 0;//x轴下限
                    double _xMax = 0;//x轴上限
                    if (comboBox1.SelectedIndex == 0)
                    {
                        _xMin = 0;
                        _xMax = 0;
                        foreach (var item in DataDic)
                        {
                            double[] dataY = item.Value.ToList().Select(dsData => dsData.Torque).ToArray();
                            double[] dataX = Enumerable.Range(0, dataY.Length).Select(i => (double)i).ToArray();
                            this.formsPlot1.Plot.AddScatterLines(dataX, dataY, label: "扭矩曲线 " + item.Key);
                            _xMin = Math.Min(_xMin, dataX.Min());
                            _xMax = Math.Max(_xMax, dataX.Max());
                        }
                        this.formsPlot1.Plot.Title("扭矩曲线");
                        this.formsPlot1.Plot.XLabel("");
                        this.formsPlot1.Plot.YLabel("扭矩");
                        this.formsPlot1.Plot.Legend();
                        this.formsPlot1.Plot.SetAxisLimits(xMin: _xMin, xMax: _xMax);//设置X轴的下标范围
                        this.formsPlot1.Render();//必须添加render，否则无法将点渲染到图像
                    }
                    else if (comboBox1.SelectedIndex == 1)
                    {
                        _xMin = 0;
                        _xMax = 0;
                        foreach (var item in DataDic)
                        {
                            double[] dataY = item.Value.ToList().Select(dsData => dsData.Angle).ToArray();
                            double[] dataX = Enumerable.Range(0, dataY.Length).Select(i => (double)i).ToArray();
                            this.formsPlot1.Plot.AddScatterLines(dataX, dataY, label: "角度曲线 " + item.Key);
                            _xMin = Math.Min(_xMin, dataX.Min());
                            _xMax = Math.Max(_xMax, dataX.Max());
                        }
                        this.formsPlot1.Plot.Title("角度曲线");
                        this.formsPlot1.Plot.XLabel("");
                        this.formsPlot1.Plot.YLabel("角度");
                        this.formsPlot1.Plot.Legend();
                        this.formsPlot1.Plot.SetAxisLimits(xMin: _xMin, xMax: _xMax);//设置X轴的下标范围
                        this.formsPlot1.Render();
                    }
                    else if (comboBox1.SelectedIndex == 2)
                    {
                        foreach (var item in DataDic)
                        {
                            List<DSData> targetData = new List<DSData>();
                            targetData = item.Value;
                            double[] dataY = targetData.ToList().Select(dsData => dsData.Torque).ToArray();
                            double[] dataX = targetData.ToList().Select(dsData => dsData.DType == 0xF2 || dsData.DType == 0xF3 ? dsData.AngleAcc : dsData.Angle).ToArray();

                            this.formsPlot1.Plot.AddScatterLines(dataX, dataY, label: "扭矩角度曲线 " + item.Key);
                        }
                        this.formsPlot1.Plot.Title("扭矩角度曲线");
                        this.formsPlot1.Plot.XLabel("角度");
                        this.formsPlot1.Plot.YLabel("扭矩");
                        this.formsPlot1.Plot.Legend();
                        this.formsPlot1.Render();
                    }

                    // 刷新图表
                    formsPlot1.Refresh();
                }))
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                // 重新启用UI元素
                comboBox1.Enabled = true;
                formsPlot1.Enabled = true;
            }
        }

        private void FormsPlot1_MouseMove(object sender, MouseEventArgs e)
        {
            //获取当前坐标
            (double mouseX, double mouseY) = formsPlot1.GetMouseCoordinates();

            //获取最近的x坐标
            int nearestIndex = GetNearestIndex(mouseX);

            //显示数据
            double peakTorque = torqueList.Max();
            double peakAngle = angleList.Max();

            //显示纵轴
            if (verticalLine == null)
            {
                verticalLine = formsPlot1.Plot.AddVerticalLine(mouseX, System.Drawing.Color.CadetBlue);
            }
            else
            {
                verticalLine.X = mouseX;
            }

            // 擦除旧文本
            if (textAnnotation != null)
            {
                formsPlot1.Plot.Remove(textAnnotation);
            }

            // 显示文本
            string text = $"角度（X）: {angleList[nearestIndex]:F3},\n 扭矩（Y）: {torqueList[nearestIndex]:F2},\n 角度峰值: {peakAngle:F2},\n 扭矩: {peakTorque:F2}";
            textAnnotation = formsPlot1.Plot.AddText(text, mouseX, mouseY, color: System.Drawing.Color.Blue, size: 12);

            //刷新
            formsPlot1.Render();
        }

        //获取最近的下标
        private int GetNearestIndex(double mouseX)
        {
            double minDistance = double.MaxValue;
            int nearestIndex = -1;
            for (int i = 0; i < torqueList.Count; i++)
            {
                double distance = Math.Abs(torqueList[i] - mouseX);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestIndex = i;
                }
            }
            return nearestIndex;
        }

        //双击获取信息
        private void FormsPlot1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // 获取鼠标点击位置的坐标
            var (mouseX, mouseY) = formsPlot1.GetMouseCoordinates();

            //显示文本的指定内容
            string text = "";

            // 擦除旧文本
            if (textAnnotation != null)
            {
                formsPlot1.Plot.Remove(textAnnotation);
            }

            //清除旧轴
            if (verticalLine != null)
            {
                formsPlot1.Plot.Remove(verticalLine);
            }

            //显示纵轴
            verticalLine = formsPlot1.Plot.AddVerticalLine(mouseX, System.Drawing.Color.CadetBlue);
            verticalLine.X = mouseX;

            //根据曲线类型区分数据分析
            if (comboBox1.SelectedIndex == 2)
            {
                text = "";
                foreach (var item in DataDic)
                {
                    double[] dataY = item.Value.ToList().Select(dsData => dsData.Torque).ToArray();
                    double[] dataX = item.Value.ToList().Select(dsData => dsData.Angle).ToArray();

                    // 查找最接近的点
                    var nearestIndex = Enumerable.Range(0, dataX.Length)
                        .OrderBy(i => Math.Abs(dataX[i] - mouseX))
                        .First();

                    //当前鼠标所在X坐标在数据长度内绘制
                    if (dataX.Max() >= mouseX && mouseX >= dataX.Min())
                    {
                        // 显示数据
                        text += $" 设备: {item.Key} \n 角度（X）: {dataX[nearestIndex]:F3}\n 扭矩（Y）: {dataY[nearestIndex]:F2}\n 角度峰值 : {dataX.Max():F3}\n 扭矩峰值 : {dataY.Max():F2}\n\n";
                    }
                }

                //防止点击区域过于靠边遮挡文字
                if (mouseX > formsPlot1.Plot.GetAxisLimits().XMax * 0.95)
                {
                    mouseX = formsPlot1.Plot.GetAxisLimits().XMax * 0.95;
                }

                //text = ""情形下，添加描绘会报错 System.InvalidOperationException:“text cannot be null or whitespace”
                if (text != "")
                {
                    textAnnotation = formsPlot1.Plot.AddText(text, mouseX, mouseY, color: System.Drawing.Color.Blue, size: 12);
                }

            }
            else if (comboBox1.SelectedIndex == 1)
            {
                text = "";
                foreach (var item in DataDic)
                {
                    double[] dataY = item.Value.ToList().Select(dsData => dsData.Angle).ToArray();
                    double[] dataX = Enumerable.Range(0, dataY.Length).Select(i => (double)i).ToArray();

                    //当前鼠标所在X坐标在数据长度内绘制
                    if (dataX.Length >= mouseX && mouseX >= 0)
                    {
                        var nearestY = dataY[(int)mouseX];// 查找最接近的点——当前x轴下标数量与数据1：1

                        // 显示数据
                        text += $" 设备: {item.Key} \n 角度（Y）: {nearestY:F3}\n 角度峰值: {dataY.Max():F3}\n\n";
                    }
                }

                //防止点击区域过于靠边遮挡文字
                if (mouseX > formsPlot1.Plot.GetAxisLimits().XMax * 0.95)
                {
                    mouseX = formsPlot1.Plot.GetAxisLimits().XMax * 0.95;
                }

                //text = ""情形下，添加描绘会报错 System.InvalidOperationException:“text cannot be null or whitespace”
                if (text != "")
                {
                    textAnnotation = formsPlot1.Plot.AddText(text, mouseX, mouseY, color: System.Drawing.Color.Blue, size: 12);
                }
            }
            else if (comboBox1.SelectedIndex == 0)
            {
                text = "";
                foreach (var item in DataDic)
                {
                    double[] dataY = item.Value.ToList().Select(dsData => dsData.Torque).ToArray();
                    double[] dataX = Enumerable.Range(0, dataY.Length).Select(i => (double)i).ToArray();

                    //当前鼠标所在X坐标在数据长度内绘制
                    if (dataX.Length >= mouseX && mouseX >= 0)
                    {
                        var nearestY = dataY[(int)mouseX];// 查找最接近的点——当前x轴下标数量与数据1：1

                        // 显示数据
                        text += $" 设备: {item.Key} \n 扭矩（Y）: {nearestY:F2}\n 扭矩峰值: {dataY.Max():F2}\n\n";
                    }
                }

                //防止点击区域过于靠边遮挡文字
                if (mouseX > formsPlot1.Plot.GetAxisLimits().XMax * 0.95)
                {
                    mouseX = formsPlot1.Plot.GetAxisLimits().XMax * 0.95;
                }

                //text = ""情形下，添加描绘会报错 System.InvalidOperationException:“text cannot be null or whitespace”
                if (text != "")
                {
                    textAnnotation = formsPlot1.Plot.AddText(text, mouseX, mouseY, color: System.Drawing.Color.Blue, size: 12);
                }
            }

            formsPlot1.Refresh();
        }

        //曲线类型切换
        private async void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            await UpdatePlotAsync();
        }

        //将DataTable类型转换成csv
        public void DataTableToCsvFile(DataTable dataTable, string filePath)
        {
            // 确保文件路径目录存在
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            //判断是否能打开文件
            try
            {
                // 创建文件并写入数据,  new UTF8Encoding(true): 这里的 true 参数指示 UTF8Encoding 使用带 BOM 的 UTF-8 编码。 
                using (StreamWriter writer = new StreamWriter(filePath, false, new UTF8Encoding(true)))//效果：Encoding.UTF8 < new UTF8Encoding(true)
                {
                    // 写入表头
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        writer.Write(dataTable.Columns[i]);
                        if (i < dataTable.Columns.Count - 1)
                        {
                            writer.Write(",");
                        }
                    }
                    writer.WriteLine();

                    // 写入数据行
                    foreach (DataRow row in dataTable.Rows)
                    {
                        for (int i = 0; i < dataTable.Columns.Count; i++)
                        {
                            string cellValue = row[i].ToString();
                            //if (decimal.TryParse(cellValue, out decimal number) && cellValue.Length > 10)
                            //{
                            //    cellValue = $"'{cellValue}"; // 加上单引号前缀，防止科学计数法
                            //}
                            writer.Write(cellValue);
                            if (i < dataTable.Columns.Count - 1)
                            {
                                writer.Write(",");
                            }
                        }
                        writer.WriteLine();
                    }
                }
            }
            catch (IOException)
            {
                // 文件被占用
                MessageBox.Show("csv文件被打开，请先关闭");
                return;
            }

        }

        //右键菜单删除
        private void dataGridView1_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (dataGridView1.Columns[dataGridView1.Columns.Count - 1].HeaderText != "工作日期" &&
                dataGridView1.Columns[dataGridView1.Columns.Count - 1].HeaderText != "作业号")
            {
                return;
            }

            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                if (e.RowIndex == -1) return;

                this.dataGridView1.Rows[e.RowIndex].Selected = true;  //是否选中当前行
                this.dataGridView1.CurrentCell = this.dataGridView1.Rows[e.RowIndex].Cells[2]; //用于只选中鼠标所在位置的行，其他均不选中

                //指定控件（DataGridView），指定位置（鼠标指定位置）
                this.contextMenuStrip1.Show(System.Windows.Forms.Cursor.Position);        //锁定右键列表出现的位置
            }
        }

        //删除功能
        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string delWorkNum;
            string delSeqId;
            DateTime delTime;
            string delVinId;
            switch (dataGridView1.Columns[dataGridView1.Columns.Count - 1].HeaderText)
            {
                case "工作日期":
                    DialogResult result1 = MessageBox.Show("是否删除该条数据？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result1 == DialogResult.No) return;
                    //确定删除
                    try
                    {
                        delWorkNum = this.dataGridView1.Rows[this.dataGridView1.CurrentRow.Index].Cells[2].Value.ToString();
                        delSeqId = this.dataGridView1.Rows[this.dataGridView1.CurrentRow.Index].Cells[3].Value.ToString();
                        delTime = this.dataGridView1.Rows[this.dataGridView1.CurrentRow.Index].Cells[4].Value.ToDate();
                        JDBC.DeleteDataByWidAndSeqAndTime(delWorkNum, delSeqId, delTime);

                        this.dataGridView1.Rows.RemoveAt(this.dataGridView1.CurrentRow.Index);//表格更新，删除对应的行
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    break;
                case "作业号":
                    DialogResult result2 = MessageBox.Show("是否删除该条数据？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result2 == DialogResult.No) return;

                    //确定删除
                    try
                    {
                        delWorkNum = this.dataGridView1.Rows[this.dataGridView1.CurrentRow.Index].Cells[2].Value.ToString();
                        delSeqId = this.dataGridView1.Rows[this.dataGridView1.CurrentRow.Index].Cells[3].Value.ToString();
                        delTime = this.dataGridView1.Rows[this.dataGridView1.CurrentRow.Index].Cells[4].Value.ToDate();
                        delVinId = this.dataGridView1.Rows[this.dataGridView1.CurrentRow.Index].Cells[5].Value.ToString();
                        JDBC.DeleteDataByWidAndSeqAndTimeAndVid(delWorkNum, delSeqId, delTime, delVinId);

                        this.dataGridView1.Rows.RemoveAt(this.dataGridView1.CurrentRow.Index);//表格更新，删除对应的行
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    break;
                default:
                    break;
            }
        }

        //切换模式
        private void btn_toggle_Click(object sender, EventArgs e)
        {
            if (isDataLoad) return;

            DialogResult typeResult = MessageBox.Show($"是否{btn_toggle.Text}" + "？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (typeResult == DialogResult.No) return;

            if (panel4.Visible == true) { panel4.Visible = false; } 

            if (btn_toggle.Text == "切换\n峰值模式")
            {
                btn_toggle.Text = "切换\n全局模式";
                btn_delete.Visible = true;
                isPeakShow = true;
                SaveShowType(1);

                //峰值模式展示最近一天的峰值
                if (dataTable.Rows.Count > 0) dataTable.Clear();
                ShowPeakData();

                AddCheckBoxCol();
            }
            else if (btn_toggle.Text == "切换\n全局模式")
            {
                btn_toggle.Text = "切换\n峰值模式";
                btn_delete.Visible = false;
                isPeakShow = false;
                SaveShowType(0);

                //显示数据汇总表（根据工单名称）
                if (dataTable.Rows.Count > 0) dataTable.Clear();
                ShowSummaryData();
            }

            //禁止更新后的表格排序
            for (int i = 0; i < this.dataGridView1.Columns.Count; i++)
            {
                dataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        //判定展示模式
        private string GetShowType(string filePath)
        {
            string showType = "";
            //空
            if (!Directory.Exists(MyDevice.userDAT))
            {
                return showType;
            }

            //读取
            try
            {
                if (File.Exists(filePath))
                {
                    showType = File.ReadAllText(filePath);
                }
            }
            catch
            {
            }

            return showType;
        }

        //存储展示模式
        private void SaveShowType(int showType)
        {
            //空
            if (!Directory.Exists(MyDevice.userDAT))
            {
                Directory.CreateDirectory(MyDevice.userDAT);
            }

            //写入
            try
            {
                string mePath = MyDevice.userDAT + @"\DataShowType.txt";//设置文件路径
                if (File.Exists(mePath))
                {
                    System.IO.File.SetAttributes(mePath, FileAttributes.Normal);
                }
                File.WriteAllText(mePath, showType.ToString());// 0代表全局展示， 1代表峰值展示
                System.IO.File.SetAttributes(mePath, FileAttributes.ReadOnly);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        //筛选
        private void btn_filter_Click(object sender, EventArgs e)
        {
            panel4.Visible = !panel4.Visible;
            panel4.Location = new System.Drawing.Point(btn_filter.PointToScreen(System.Drawing.Point.Empty).X, 0);

            //弹窗
            if (panel4.Visible == true)
            {
                label1.Text = "";
                FilterInit();

                isDataLoad = true;

                if (isRecentDate)
                {
                    checkBox_selectDate.Checked = false;
                    ucCombox_recentDate.Enabled = true;
                }
                else
                {
                    checkBox_selectDate.Checked = true;
                    ucCombox_recentDate.Enabled = false;
                }
            }
            //关闭
            else
            {
                isDataLoad = false;
            }
        }

        //筛选框初始化
        private void FilterInit()
        {
            //最近日期
            ucCombox_recentDate.Source = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("0",  "最近1天"),
                new KeyValuePair<string, string>("1",  "最近3天"),
                new KeyValuePair<string, string>("2",  "最近7天"),
            };

            switch (peakRecentDays)
            {
                case 1:
                    ucCombox_recentDate.TextValue = "最近1天";
                    break;
                case 3:
                    ucCombox_recentDate.TextValue = "最近3天";
                    break;
                case 7:
                    ucCombox_recentDate.TextValue = "最近7天";
                    break;
                default:
                    ucCombox_recentDate.TextValue = "最近1天";
                    break;
            }
        }

        //最近日期
        private async void ucCombox_recentDate_SelectedChangedEvent(object sender, EventArgs e)
        {
            if (checkBox_recentDate.Checked)
            {
                await UpdatePeakRecentDate();
                AddCheckBoxCol();
            }
        }

        //更新最近日期的峰值
        private async Task UpdatePeakRecentDate()
        {
            // 禁用UI元素
            label1.Text = "数据加载中，请耐心等待...";
            btn_filter.Enabled = false;
            ucCombox_recentDate.Enabled = false; // 禁用下拉框，避免多次选择

            try
            {
                // 获取所选的index，防止UI线程中的值变化
                int selectedIndex = ucCombox_recentDate.SelectedIndex;

                // 耗时操作放在Task.Run中，避免阻塞UI线程
                await Task.Run(() => {
                    switch (selectedIndex)
                    {
                        case 0:
                            peakRecentDays = 1;
                            ShowPeakData(1);
                            break;

                        case 1:
                            peakRecentDays = 3;
                            ShowPeakData(3);
                            break;

                        case 2:
                            peakRecentDays = 7;
                            ShowPeakData(7);
                            break;

                        default:
                            break;
                    }
                });

                // 耗时操作完成后更新UI
                this.Invoke(new Action(() =>
                {
                    label1.Text = "数据加载成功";
                }));
            }
            catch (Exception ex)
            {
                this.Invoke(new Action(() =>
                {
                    MessageBox.Show(ex.Message);
                }));
            }
            finally
            {
                // 无论成功与否，最终都重新启用UI元素
                this.Invoke(new Action(() =>
                {
                    btn_filter.Enabled = true;
                    ucCombox_recentDate.Enabled = true;
                }));
            }
        }

        //日期切换
        private async void monthCalendar1_DateChanged(object sender, DateRangeEventArgs e)
        {
            label1.Focus();

            if (!isRecentDate)
            {
                await UpdatePeakSelectDate();
                AddCheckBoxCol();
            }
        }

        //更新选中日期的峰值
        private async Task UpdatePeakSelectDate()
        {
            // 禁用UI元素
            label1.Text = "数据加载中，请耐心等待...";
            btn_filter.Enabled = false;

            try
            {
                // 获取所选的date，防止UI线程中的值变化
                selectDateTime = monthCalendar1.SelectionStart;

                // 耗时操作放在Task.Run中，避免阻塞UI线程
                await Task.Run(() => {
                    ShowPeakData(selectDateTime.Date);
                });

                // 耗时操作完成后更新UI
                this.Invoke(new Action(() =>
                {
                    label1.Text = "数据加载成功";
                }));
            }
            catch (Exception ex)
            {
                this.Invoke(new Action(() =>
                {
                    MessageBox.Show(ex.Message);
                }));
            }
            finally
            {
                // 无论成功与否，最终都重新启用UI元素
                this.Invoke(new Action(() =>
                {
                    btn_filter.Enabled = true;
                }));
            }
        }

        //更新切换后的日期的峰值
        private async void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            if (!isRecentDate)
            {
                await UpdatePeakSelectDate();
            }
        }

        //最近日期的选择切换
        private void checkBox_recentDate_Click(object sender, EventArgs e)
        {
            if (checkBox_recentDate.Checked)
            {
                checkBox_selectDate.Checked = false;
                ucCombox_recentDate.Enabled = true;
                isRecentDate = true;
            }
            else
            {
                checkBox_selectDate.Checked = true;
                ucCombox_recentDate.Enabled = false;
                isRecentDate = false;
            }
        }

        //指定日期的选择切换
        private void checkBox_selectDate_Click(object sender, EventArgs e)
        {
            //不得使用monthCalendar1.Enable控制控件开关，否则会多次触发monthCalendar1切换函数
            if (checkBox_selectDate.Checked)
            {
                checkBox_recentDate.Checked = false;
                ucCombox_recentDate.Enabled = false;
                isRecentDate = false;
            }
            else
            {
                checkBox_selectDate.Checked = true;
                ucCombox_recentDate.Enabled = true;
                isRecentDate = true;
            }
        }

        private void AddCheckBoxCol()
        {
            //行首替换成checkbox
            datagridviewCheckboxHeaderCell ch = new datagridviewCheckboxHeaderCell();
            ch.OnCheckBoxClicked += new datagridviewCheckboxHeaderCell.HeaderEventHander(ch_OnCheckBoxClicked);
            DataGridViewCheckBoxColumn checkboxCol = dataGridView1.Columns["是否选择"] as DataGridViewCheckBoxColumn;
            checkboxCol.HeaderCell = ch;
            checkboxCol.HeaderCell.Value = "";//消除列头checkbox旁出现的文字
        }

        #region 表多选
        void ch_OnCheckBoxClicked(object sender, datagridviewCheckboxHeaderEventArgs e)
        {
            if (dataGridView1.RowCount < 1) return; //防止表格无单元格时，BeginEdit()导致报错，无法执行此操作。
            dataGridView1.SuspendLayout();//暂停布局更新
            dataGridView1.BeginEdit(false);//批量操作开始，避免更改值时触发额外的事件和验证
            foreach (DataGridViewRow dgvRow in dataGridView1.Rows)
            {
                if (e.CheckedState)
                {
                    dgvRow.Cells[0].Value = true;
                }
                else
                {
                    dgvRow.Cells[0].Value = false;
                }
            }
            dataGridView1.EndEdit();//批量操作结束
            dataGridView1.ResumeLayout();//恢复布局更新
        }

        public class datagridviewCheckboxHeaderEventArgs : EventArgs
        {
            private bool checkedState = false;
            public bool CheckedState
            {
                get { return checkedState; }
                set { checkedState = value; }
            }
        }

        //定义继承于DataGridViewColumnHeaderCell的类，用于绘制checkbox，定义checkbox鼠标单击事件
        public class datagridviewCheckboxHeaderCell : DataGridViewColumnHeaderCell
        {
            public delegate void HeaderEventHander(object sender, datagridviewCheckboxHeaderEventArgs e);
            public event HeaderEventHander OnCheckBoxClicked;
            Point checkBoxLocation;
            Size checkBoxSize;
            bool _checked = false;
            Point _cellLocation = new Point();
            System.Windows.Forms.VisualStyles.CheckBoxState _cbState =
                System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal;

            //绘制列头checkbox
            protected override void Paint(System.Drawing.Graphics graphics,
               System.Drawing.Rectangle clipBounds,
               System.Drawing.Rectangle cellBounds,
               int rowIndex,
               DataGridViewElementStates dataGridViewElementState,
               object value,
               object formattedValue,
               string errorText,
               DataGridViewCellStyle cellStyle,
               DataGridViewAdvancedBorderStyle advancedBorderStyle,
               DataGridViewPaintParts paintParts)
            {
                base.Paint(graphics, clipBounds, cellBounds, rowIndex,
                    dataGridViewElementState, value,
                    formattedValue, errorText, cellStyle,
                    advancedBorderStyle, paintParts);
                Point p = new Point();
                Size s = CheckBoxRenderer.GetGlyphSize(graphics,
                System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal);
                p.X = cellBounds.Location.X +
                    (cellBounds.Width / 2) - (s.Width / 2) - 1;//列头checkbox的X坐标
                p.Y = cellBounds.Location.Y +
                    (cellBounds.Height / 2) - (s.Height / 2);//列头checkbox的Y坐标
                _cellLocation = cellBounds.Location;
                checkBoxLocation = p;
                checkBoxSize = s;
                if (_checked)
                    _cbState = System.Windows.Forms.VisualStyles.
                        CheckBoxState.CheckedNormal;
                else
                    _cbState = System.Windows.Forms.VisualStyles.
                        CheckBoxState.UncheckedNormal;
                CheckBoxRenderer.DrawCheckBox
                (graphics, checkBoxLocation, _cbState);
            }

            /// <summary>
            /// 点击列头checkbox单击事件
            /// </summary>
            /// <param name="e"></param>
            protected override void OnMouseClick(DataGridViewCellMouseEventArgs e)
            {
                Point p = new Point(e.X + _cellLocation.X, e.Y + _cellLocation.Y);
                if (p.X >= checkBoxLocation.X && p.X <=
                    checkBoxLocation.X + checkBoxSize.Width
                && p.Y >= checkBoxLocation.Y && p.Y <=
                    checkBoxLocation.Y + checkBoxSize.Height)
                {
                    _checked = !_checked;
                    //获取列头checkbox的选择状态
                    datagridviewCheckboxHeaderEventArgs ex = new datagridviewCheckboxHeaderEventArgs();
                    ex.CheckedState = _checked;

                    object sender = new object();//此处不代表选择的列头checkbox，只是作为参数传递。应该列头checkbox是绘制出来的，无法获得它的实例

                    if (OnCheckBoxClicked != null)
                    {
                        OnCheckBoxClicked(sender, ex);//触发单击事件
                        this.DataGridView.InvalidateCell(this);
                    }
                }
                base.OnMouseClick(e);
            }
        }

        //结束编辑状态
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (isPeakShow && dataGridView1.Columns[0].GetType() == typeof(DataGridViewCheckBoxColumn))
            {
                if (e.RowIndex != -1 && e.ColumnIndex == 0)
                {
                    dataGridView1.Rows[e.RowIndex].Cells[0].Value = !Convert.ToBoolean(dataGridView1.Rows[e.RowIndex].Cells[0].Value);
                }
            }
        }

        #endregion

        //加载稳定后引入checkbox自定义列（Load里面使用会导致可能datagridview未稳定就调用方法无效）
        private void MenuDataManageForm_Shown(object sender, EventArgs e)
        {
            // 确保DataGridView的列已初始化
            if (btn_toggle.Text == "切换\n全局模式")
            {
                //行首替换成checkbox
                AddCheckBoxCol();
            }
        }

        //删除数据（峰值模式下）
        private void btn_delete_Click(object sender, EventArgs e)
        {
            //管理员以上的权限
            if (MyDevice.userRole == "0")
            {
                MessageBox.Show("无权限删除工单，请切换管理员用户", "确认", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //无有效工单
            if (dataGridView1.RowCount < 1) return;

            List<int> rowsToRemove = new List<int>();// 列表存储要删除的行的索引

            //收集被选中的工单汇总
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                if (this.dataGridView1.Rows[i].Cells[0].Value.ToString() == "True")
                {
                    //DSTicketInfo selectTicketInfo = new DSTicketInfo();//必须新建局部变量，因为添加的是指针，如果是全局变量，最后添加的工单信息全部一致
                    //selectTicketInfo.WorkId = Convert.ToUInt32(this.dataGridView1.Rows[i].Cells[1].Value.ToString());
                    //selectTicketInfos.Add(selectTicketInfo);
                    rowsToRemove.Add(i);
                }
            }

            if (rowsToRemove.Count < 1)
            {
                MessageBox.Show("未选择任何有效数据，无法删除", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else
            {
                //全删
                if (rowsToRemove.Count == dataGridView1.RowCount)
                {
                    //最近日期
                    if (isRecentDate)
                    {
                        switch (ucCombox_recentDate.TextValue)
                        {
                            case "最近1天":
                                JDBC.DeleteDataByRecent(1);
                                break;
                            case "最近3天":
                                JDBC.DeleteDataByRecent(3);
                                break;
                            case "最近7天":
                                JDBC.DeleteDataByRecent(7);
                                break;
                            default:
                                JDBC.DeleteDataByRecent(1);
                                break;
                        }
                    }
                    //指定日期
                    else
                    {
                        if (selectDateTime == new DateTime())
                        {
                            MessageBox.Show("指定日期模式下未选择具体的日期，无法删除", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                        JDBC.DeleteDataByTime(selectDateTime.Date);
                    }
                }
                //部分删
                else
                {
                    //最近日期
                    if (isRecentDate)
                    {

                    }
                    //指定日期
                    else
                    {

                    }
                }
            }

            //逆序遍历删除 （表格行数递减，正序删除容易造成溢出报错）
            for (int i = rowsToRemove.Count - 1; i >= 0; i--)
            {
                this.dataGridView1.Rows.RemoveAt(rowsToRemove[i]);
            }

            #region 删指定日期

            //全选删除

            //部分选删除

            //单选删除

            #endregion

            #region 删最近日期

            //全选删除

            //部分选删除

            //单选删除

            #endregion
        }
    }
}
