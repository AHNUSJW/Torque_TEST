using DBHelper;
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

//Ricardo 20240725

namespace Base.UI.MenuData
{
    public partial class MenuDataAnalysisForm : Form
    {
        #region 变量定义

        private BindingSource _bindingSource = new BindingSource();//绑定数据源(BindingSourse性能更好,内置方法更多)
        private DataTable dataTable = new DataTable();             //原始数据表
        private DataTable cloneTable = new DataTable();            //克隆表（拷贝原始数据表，避免直接引用数据表）
        private DataTable copyTable = new DataTable();             //复制表 (备份原始数据)
        private DataTable csvTable = new DataTable();              //另存到csv的数据表
        private AnalysisData targetData = new AnalysisData();      //存储单条获取到的表格数据
        private Dictionary<string, List<AnalysisData>> DataDic = new Dictionary<string, List<AnalysisData>>(); //不同站点下的信息汇总
        private Dictionary<string, List<string>> WrenchDic = new Dictionary<string, List<string>>();//不同站点下的点位汇总(由于是从csv导入的数据，统一用string)
        private ValueTuple<int, string, double> valueTuple = new ValueTuple<int, string, double>();//同时存储站点

        private VLine verticalLine1;                                      //第一光标
        private VLine verticalLine2;                                      //第二光标
        private HLine horizontalLine;                                     //水平轴
        private List<ScatterPlot> slopeLineList = new List<ScatterPlot>();//切线
        private Text textAnnotation1 = null;                              //光标数据文本1
        private Text textAnnotation2 = null;                              //光标数据文本2
        private List<double> targetXVal = new List<double>();             //目标角度数据X
        private List<double> targetYVal = new List<double>();             //目标扭矩数据Y

        private const int BatchSize = 500;                                // 每批加载的数据量
        private Dictionary<string, Queue<(double, double)>> remainingData;//不同设备的数据字典
        private Int32 totalCnt = 0;                                       //总数据量
        private Int32 readCnt = 0;                                        //已读数据量
        private List<string> keyList = new List<string>();                //用于存储当前数据的站点
        private bool isDataDeal = false;                                  //数据是否预处理
        private bool isDrawSlopeLine = false;                             //是否画切线
        private bool isDrawFirstCursor = false;                           //是否画第一光标
        private bool isDrawSecondCursor = false;                          //是否画第二光标
        private double targetSlope = 0;                                   //目标切线的斜率
        private double targetOffset = 0;                                  //目标切线的偏移量
        private double cursorAngle = double.MinValue;                     //求残余扭矩的第一光标所在角度
        private double torqueDiff1 = 0;                                   //求扭矩差值时的第一光标显示的扭矩
        private double torqueDiff2 = 0;                                   //求扭矩差值时的第二光标显示的扭矩

        //数据分析的数据结构
        public class AnalysisData
        {
            public string id;
            public string WorkNum;
            public string SequenceId;
            public string VinId;
            public string Bohrcode;
            public string DevType;
            public string PointNum;
            public string DevAddr;
            public string CreateTime;
            public string DType;
            public string Stamp;
            public string Torque;
            public string TorquePeak;
            public string Angle;
            public string AngleAcc;
            public string DataResult;
        }
        #endregion 

        public MenuDataAnalysisForm()
        {
            InitializeComponent();
        }

        private void MenuDataAnalysisForm_Load(object sender, EventArgs e)
        {
            //页面初始化
            DatagridviewInit(this.dataGridView1);
            ScottPlotInit();
            //控件显示
            ControlsVisible(false);
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_import_Click(object sender, EventArgs e)
        {
            //打开导入操作页面
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CSV Files (*.csv)|*.csv";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                //取消预处理
                if (isDataDeal)
                {
                    ControlsVisible(false);

                    isDataDeal = false;
                    isDrawSlopeLine = false;
                    isDrawFirstCursor = false;
                    isDrawSecondCursor = false;
                    //
                    _bindingSource.DataSource = dataTable;
                    btn_dealData.Text = "数据预处理";

                    this.formsPlot1.Visible = false;

                    targetXVal.Clear();
                    targetYVal.Clear();
                }

                //清空历史数据
                this.dataGridView1.DataSource = null;//必须释放绑定的历史数据源，否则会提示异常 System.ArgumentException: 不能清除此列表
                this.dataGridView1.Rows.Clear();
                this.dataGridView1.Columns.Clear();
                this.formsPlot1.Plot.Clear();
                this.formsPlot1.Refresh();
                this.comboBox_Dev.Items.Clear();
                this.comboBox_Point.Items.Clear();
                DataDic.Clear();
                WrenchDic.Clear();

                //导入 CSV -> DataTable -> DataGridView
                dataTable = ReadCsvFile(ofd.FileName);
                copyTable = dataTable.Copy();

                if (dataTable == null || dataTable.Rows.Count == 0) return;//表格为空或者无数据 终止
                if (IsCsvPass(dataTable) == false) return;//表格不合格 终止

                _bindingSource.DataSource = dataTable;//表格数据来源绑定到数据表
                this.dataGridView1.DataSource = _bindingSource;//绑定数据源，通常绑定模式是 dataGridView -> bindingSource -> dataTable

                cloneTable = dataTable.Clone();// 克隆表结构，直接操作dataTable会影响有效数据引用,例如分页

                // 禁止所有列的排序
                foreach (DataGridViewColumn column in dataGridView1.Columns)
                {
                    column.SortMode = DataGridViewColumnSortMode.NotSortable;
                }

                //解析csv
                int tableRowCnt = dataTable.Rows.Count;
                int tableColCnt = dataTable.Columns.Count;
                int columnIndex = dataTable.Columns["设备站点"].Ordinal;//指定列名所在表的列下标，没有返回-1

                for (int i = 0; i < tableRowCnt; i++)
                {
                    targetData = new AnalysisData()
                    {
                        id         = dataTable.Rows[i][0].ToString(),
                        WorkNum    = dataTable.Rows[i][1].ToString(),
                        SequenceId = dataTable.Rows[i][2].ToString(),
                        VinId      = dataTable.Rows[i][3].ToString(),
                        Bohrcode   = dataTable.Rows[i][4].ToString(),
                        DevType    = dataTable.Rows[i][5].ToString(),
                        PointNum   = dataTable.Rows[i][6].ToString(),
                        DevAddr    = dataTable.Rows[i][7].ToString(),
                        CreateTime = dataTable.Rows[i][8].ToString(),
                        DType      = dataTable.Rows[i][9].ToString(),
                        Stamp      = dataTable.Rows[i][10].ToString(),
                        Torque     = dataTable.Rows[i][11].ToString(),
                        TorquePeak = dataTable.Rows[i][12].ToString(),
                        Angle      = dataTable.Rows[i][13].ToString(),
                        AngleAcc   = dataTable.Rows[i][14].ToString(),
                        DataResult = dataTable.Rows[i][15].ToString(),
                    };
                    //不同站点分配不同的List
                    if (columnIndex != -1 && !DataDic.ContainsKey(dataTable.Rows[i][columnIndex].ToString()))
                    {
                        DataDic.Add(dataTable.Rows[i][columnIndex].ToString(), new List<AnalysisData>());
                        WrenchDic.Add(dataTable.Rows[i][columnIndex].ToString(), new List<string>());
                    }
                    DataDic[dataTable.Rows[i][columnIndex].ToString()].Add(targetData);

                    //获取工单点位汇总
                    if (targetData.PointNum != "" && !WrenchDic[dataTable.Rows[i][columnIndex].ToString()].Contains(targetData.PointNum))
                    {
                        WrenchDic[dataTable.Rows[i][columnIndex].ToString()].Add(targetData.PointNum);
                    }

                }

                //画曲线,数据量较少时一次性画，较多时分批次画
                if (dataTable.Rows.Count <= 70000)
                {
                    //await DrawCurve1();
                }
                else
                {
                    //ucProcessLine1.Visible = true;
                    //DrawCurve2();
                }
            }
        }

        /// <summary>
        /// 数据预处理 —— 只留角度递增数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_dealData_Click(object sender, EventArgs e)
        {
            if (dataTable.Rows.Count == 0) return;

            if (btn_dealData.Text.Trim() == "数据预处理")
            {
                isDataDeal = true;//预处理启动
                
                foreach (var item in DataDic)
                {
                    comboBox_Dev.Items.Add(item.Key);//确定设备
                }

                btn_dealData.Text = "取消预处理";
                comboBox_Dev.SelectedIndex = 0;//触发设备切换函数
                this.formsPlot1.Visible = true;

            }
            else if (btn_dealData.Text.Trim() == "取消预处理")
            {
                //处理操作全部重置
                isDataDeal = false;
                isDrawSlopeLine = false;
                isDrawFirstCursor = false;
                isDrawSecondCursor = false;
                //
                _bindingSource.DataSource = dataTable;
                btn_dealData.Text = "数据预处理";

                this.comboBox_Point.Visible = false;
                this.formsPlot1.Visible = false;

                targetXVal.Clear();
                targetYVal.Clear();
                this.comboBox_Dev.Items.Clear();
                this.comboBox_Point.Items.Clear();
            }

            //控件显示
            ControlsVisible(isDataDeal);
        }

        /// <summary>
        /// 第一光标 —— 获取分离扭矩
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_firstCursor_Click(object sender, EventArgs e)
        {
            //
            this.btn_firstCursor.BackColor = Color.Green;
            this.btn_secondCursor.BackColor = Color.Red;
            this.btn_slopeLine.BackColor = Color.Red;

            isDrawFirstCursor = true;
            isDrawSecondCursor = false;
            isDrawSlopeLine = false;

            //清除旧数据涂鸦
            ClearCursor1();
            ClearTextAnnotation1();

            //画第一光标
            GetVerticalLine(this.formsPlot1.Plot.GetAxisLimits().XMin + ((this.formsPlot1.Plot.GetAxisLimits().XMax - this.formsPlot1.Plot.GetAxisLimits().XMin) / 50),
                0, targetXVal.ToArray(), targetYVal.ToArray());

        }

        /// <summary>
        /// 第二光标 —— 获取残余扭矩
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_secondCursor_Click(object sender, EventArgs e)
        {
            //
            this.btn_firstCursor.BackColor = Color.Red;
            this.btn_secondCursor.BackColor = Color.Green;
            this.btn_slopeLine.BackColor = Color.Red;

            isDrawFirstCursor = false;
            isDrawSecondCursor = true;
            isDrawSlopeLine = false;

            //清除旧数据涂鸦
            ClearCursor2();
            ClearTextAnnotation2();
            //清除切线
            ClearSlopeLine();

            //画第二光标
            GetVerticalLine(this.formsPlot1.Plot.GetAxisLimits().XMax - ((this.formsPlot1.Plot.GetAxisLimits().XMax - this.formsPlot1.Plot.GetAxisLimits().XMin) / 50),
                0, targetXVal.ToArray(), targetYVal.ToArray());

        }

        /// <summary>
        /// 切线生成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_slopeLine_Click(object sender, EventArgs e)
        {
            //
            this.btn_firstCursor.BackColor = Color.Red;
            this.btn_secondCursor.BackColor = Color.Red;
            this.btn_slopeLine.BackColor = Color.Green;

            isDrawFirstCursor = false;
            isDrawSecondCursor = false;
            isDrawSlopeLine = true;
        }

        /// <summary>
        /// 曲线还原
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_curveRevert_Click(object sender, EventArgs e)
        {
            //清除旧数据涂鸦
            ClearLastData();

            formsPlot1.Plot.Render(lowQuality: true);//低质量
            this.formsPlot1.Plot.AxisAuto();// 设置zoom to fit
            formsPlot1.Render();
            Application.DoEvents();
        }

        /// <summary>
        /// 数据另存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_save_Click(object sender, EventArgs e)
        {
            if (isDataDeal)
            {
                if (csvTable.Rows.Count < 1)
                {
                    MessageBox.Show("无有效数据，无法转存文件");
                    return;
                }

                //文件名称
                string filename = "";

                //文件名根据作业号同步
                if (csvTable.Rows[0][3].ToString() != "")
                {
                    filename = csvTable.Rows[0][3].ToString();
                }
                else
                {
                    filename = DateTime.Now.ToString("yyyyMMddHHmm");
                }

                // 创建保存文件对话框
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv",
                    Title = "保存csv文件",
                    DefaultExt = "csv",
                    FileName = $"XhTorque数据分析表{filename}.csv"
                };

                // 显示对话框并获取结果
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // 获取用户选择的文件路径
                    string filePath = saveFileDialog.FileName;

                    // 将DataTable数据转换为CSV文件并保存到用户选择的路径
                    DataTableToCsvFile(csvTable, filePath);
                }
            }
        }

        /// <summary>
        /// 单元格双击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (dataGridView1.Rows.Count == 0 || e.RowIndex < 0)
            {
                return;
            }

            //第一光标才触发
            if (isDrawFirstCursor)
            {
                //表格选中
                dataGridView1.Rows[e.RowIndex].Selected = true;

                //清除旧光标与文本
                ClearCursor1();
                ClearTextAnnotation1();

                // 光标位置的坐标
                double cursorX;
                double cursorY;

                cursorX = targetXVal[e.RowIndex];
                cursorY = (formsPlot1.Plot.GetAxisLimits().YMin + formsPlot1.Plot.GetAxisLimits().YMax) / 2;

                //画第一光标
                try
                {
                    GetVerticalLine(cursorX, cursorY, targetXVal.ToArray(), targetYVal.ToArray());
                }
                catch (Exception ex)
                {

                }
            }
        }
 
        /// <summary>
        /// 双击数据画数据分析（此版本的scottPlot的doubleClick事件无效果）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void formsPlot1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && e.Clicks == 2) // 检查是否左键双击
            {
                if (this.dataGridView1.RowCount < 1) return;

                if (isDataDeal == false) return;

                if (isDrawSlopeLine)
                {
                    //清除旧切线
                    ClearSlopeLine();
                    //清除旧光标2
                    ClearCursor2();
                    //清除旧文本2
                    ClearTextAnnotation2();

                    // 获取鼠标点击位置的坐标
                    var (mouseX, mouseY) = formsPlot1.GetMouseCoordinates();

                    try
                    {
                        GetSlopeLine(mouseX, mouseY, targetXVal.ToArray(), targetYVal.ToArray());
                    }
                    catch (Exception ex)
                    {

                    }
                }
                else
                {
                    if (isDrawFirstCursor)
                    {
                        //清除旧光标与文本
                        ClearCursor1();
                        ClearTextAnnotation1();
                    }
                    else
                    {
                        //清除旧光标与文本
                        ClearCursor2();
                        ClearTextAnnotation2();
                    }

                    // 获取鼠标点击位置的坐标
                    var (mouseX, mouseY) = formsPlot1.GetMouseCoordinates();

                    try
                    {
                        //画光标
                        GetVerticalLine(mouseX, mouseY, targetXVal.ToArray(), targetYVal.ToArray());
                    }
                    catch (Exception ex)
                    {

                    }
                }

                //求残余扭矩, 切线与第一光标的交点纵坐标
                if (isDrawFirstCursor || isDrawSlopeLine)
                {
                    if (cursorAngle != double.MinValue)
                    {
                        this.label_torqueResidual.Text = $"残余扭矩：{GetResidualTorque(targetSlope, targetOffset, cursorAngle):F2}";
                    }

                    //防止切线被清除后的残留斜率继续计算残余扭矩
                    if (slopeLineList.Count == 0 || slopeLineList == null || verticalLine1 == null)
                    {
                        this.label_torqueResidual.Text = "残余扭矩：";
                    }
                }

                // 更新并刷新图表
                formsPlot1.Refresh();
            }
        }

        /// <summary>
        /// 导入csv——直接导入到表格
        /// </summary>
        /// <param name="filePath"></param>
        private void ImportCsvToDataGridView(string filePath)
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                string[] headers = sr.ReadLine().Split(',');
                foreach (string header in headers)
                {
                    dataGridView1.Columns.Add(header, header);
                }

                while (!sr.EndOfStream)
                {
                    string[] rows = sr.ReadLine().Split(',');
                    dataGridView1.Rows.Add(rows);
                }
            }
        }

        /// <summary>
        /// 导入csv——导入到DataTable (DataTable更适合数据处理)
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private DataTable ReadCsvFile(string filePath)
        {
            DataTable dt = new DataTable();

            using (StreamReader sr = new StreamReader(filePath))
            {
                string[] headers = sr.ReadLine().Split(',');
                foreach (string header in headers)
                {
                    dt.Columns.Add(header);
                }

                while (!sr.EndOfStream)
                {
                    string[] rows = sr.ReadLine().Split(',');
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < headers.Length; i++)
                    {
                        dr[i] = rows[i];
                    }
                    dt.Rows.Add(dr);
                }
            }

            return dt;
        }

        /// <summary>
        /// 将DataTable类型转换成csv
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="filePath"></param>
        public void DataTableToCsvFile(DataTable dataTable, string filePath)
        {
            // 确保文件路径目录存在
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            //判断是否能打开文件
            try
            {
                // 创建文件并写入数据
                using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
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

        /// <summary>
        /// 判断csv是否合格 (校验csv是否是开发商指定的表格内容)
        /// </summary>
        /// <param name="checkTable"></param>
        /// <returns></returns>
        private bool IsCsvPass(DataTable checkTable)
        {
            if (checkTable == null || checkTable.Rows.Count == 0)
            {
                return false;
            }
            else
            {
                try
                {
                    if (checkTable.Columns["设备站点"].Ordinal == -1 || checkTable.Columns["作业号"].Ordinal == -1)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("导入的csv文件不合格");
                }
            }

            return false;
        }

        /// <summary>
        /// 表格初始化
        /// </summary>
        /// <param name="dataGridView"></param>
        private void DatagridviewInit(DataGridView dataGridView)
        {
            //表格初始化
            dataGridView.EnableHeadersVisualStyles = false;//允许自定义行头样式
            dataGridView.RowHeadersVisible = false; //第一列空白隐藏掉
            dataGridView.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.LightSkyBlue;
            dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;//不允许修改行首高度

            dataGridView.AllowUserToAddRows = false;//禁止用户添加行
            dataGridView.AllowUserToDeleteRows = false;//禁止用户删除行
            dataGridView.AllowUserToOrderColumns = false;//禁止用户手动重新定位行
            dataGridView.AllowUserToResizeRows = false;//禁止用户调整行大小
            dataGridView.AllowUserToResizeColumns = false;//禁止用户调整列大小

            dataGridView.ReadOnly = true; // 如果只需要展示数据，设置为只读可以提高性能
        }

        /// <summary>
        /// 曲线初始化
        /// </summary>
        private void ScottPlotInit()
        {
            this.formsPlot1.Visible = true;

            // 禁用渲染时间提示框(左下角)
            this.formsPlot1.Configuration.DoubleClickBenchmark = false;

            formsPlot1.Plot.Render(lowQuality: true);//低质量

            // 使用自定义的渲染队列来优化频繁更新
            formsPlot1.Configuration.UseRenderQueue = true;
        }

        /// <summary>
        /// 控件权限
        /// </summary>
        /// <param name="visible"></param>
        private void ControlsVisible(bool visible)
        {
            //Visible
            this.btn_firstCursor.Visible = visible;
            this.btn_secondCursor.Visible = visible;
            this.btn_slopeLine.Visible = visible;
            this.btn_curveRevert.Visible = visible;
            this.btn_save.Visible = visible;
            this.comboBox_Dev.Visible = visible;

            //BackColor
            this.btn_firstCursor.BackColor = SystemColors.Control;
            this.btn_secondCursor.BackColor = SystemColors.Control;
            this.btn_slopeLine.BackColor = SystemColors.Control;

            //label
            this.label_torquePeak.Text = "峰值扭矩：";
            this.label_anglePeak.Text = "峰值角度：";
            this.label_torqueSeparate.Text = "分离扭矩：";
            this.label_torqueResidual.Text = "残余扭矩：";
            this.label_slope.Text = "切线斜率：";
            this.label_torqueDiff.Text = "扭矩差值：";
        }

        /// <summary>
        /// 画曲线(一次性加载)
        /// </summary>
        private async Task DrawCurve1()
        {
            await Task.Run(() =>
            {
                foreach (var item in DataDic)
                {
                    string[] dataY = item.Value.ToList().Select(dsData => dsData.Torque).ToArray();
                    string[] dataX = item.Value.ToList().Select(dsData => dsData.Angle).ToArray();

                    double[] targetX = new double[dataX.Length];
                    double[] targetY = new double[dataY.Length];
                    for (int i = 0; i < dataX.Length; i++)
                    {
                        targetX[i] = double.Parse(dataX[i]);
                        targetY[i] = double.Parse(dataY[i].Split(' ')[0]);
                    }

                    List<AnalysisData> selectData = new List<AnalysisData>();
                    selectData = item.Value;
                    int dataLen = selectData.Count;
                    for (int i = 0; i < dataLen; i++)
                    {
                        if (selectData[i].DType == "242" || selectData[i].DType == "243")
                        {
                            targetX[i] = double.Parse(selectData[i].AngleAcc);
                        }
                    }
                    this.formsPlot1.Invoke((Action)(() =>
                    {
                        this.formsPlot1.Plot.AddScatterLines(targetX, targetY, color: this.formsPlot1.Plot.GetNextColor(), label: "扭矩角度曲线 " + item.Key);
                    }));
                }
                this.formsPlot1.Invoke((Action)(() =>
                {
                    this.formsPlot1.Plot.Title("扭矩角度曲线");
                    this.formsPlot1.Plot.XLabel("角度");
                    this.formsPlot1.Plot.YLabel("扭矩");
                    this.formsPlot1.Plot.Legend();
                    this.formsPlot1.Render();
                }));

            });
        }

        /// <summary>
        /// 画曲线(分批次加载)
        /// </summary>
        private void DrawCurve2()
        {
            remainingData = new Dictionary<string, Queue<(double, double)>>();

            foreach (var item in DataDic)
            {
                var dataQueue = new Queue<(double, double)>();
                foreach (var dsData in item.Value)
                {
                    double angle = double.Parse(dsData.Angle);
                    double torque = double.Parse(dsData.Torque.Split(' ')[0]);

                    if (dsData.DType == "242" || dsData.DType == "243")
                    {
                        angle = double.Parse(dsData.AngleAcc);
                    }

                    dataQueue.Enqueue((angle, torque));
                }
                remainingData[item.Key] = dataQueue;
                totalCnt += dataQueue.Count;
            }

            // 
            timer1.Interval = 100; // 设置间隔为100毫秒
            timer1.Tick += LoadDataBatch; // 设置Tick事件处理程序
            timer1.Start(); // 启动定时器

            this.formsPlot1.Plot.Title("扭矩角度曲线");
            this.formsPlot1.Plot.XLabel("角度");
            this.formsPlot1.Plot.YLabel("扭矩");
            this.formsPlot1.Plot.Legend();
        }

        /// <summary>
        /// 画过程数据
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="devAddr"></param>
        private void DrwaProcessCurve(double[] x, double[] y, int devAddr)
        {
            this.formsPlot1.Plot.Clear();
            this.formsPlot1.Refresh();
            this.formsPlot1.Plot.AddScatterLines(x, y, label: "扭矩角度曲线 " + devAddr);
            this.formsPlot1.Plot.Title("扭矩角度曲线");
            this.formsPlot1.Plot.XLabel("角度");
            this.formsPlot1.Plot.YLabel("扭矩");
            this.formsPlot1.Plot.Legend();
            this.formsPlot1.Render();
        }

        /// <summary>
        /// 定时画数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadDataBatch(object sender, EventArgs e)
        {           
            bool allDataLoaded = true;

            foreach (var kvp in remainingData)
            {
                string key = kvp.Key;
                var dataQueue = kvp.Value;

                if (dataQueue.Count > 0)
                {
                    allDataLoaded = false;
                    var batchX = new List<double>();
                    var batchY = new List<double>();

                    int count = Math.Min(BatchSize, dataQueue.Count);
                    for (int i = 0; i < count; i++)
                    {
                        var (x, y) = dataQueue.Dequeue();
                        batchX.Add(x);
                        batchY.Add(y);
                        readCnt++;
                    }

                    if (ucProcessLine1.Value < 100)
                    {
                        ucProcessLine1.Visible = true;
                        ucProcessLine1.Value = (int)((1.0 * readCnt / totalCnt) * 100);
                    }

                    Task.Run(() =>
                    {
                        this.formsPlot1.Invoke(new Action(() =>
                        {
                            if (keyList.IndexOf(key) == -1)
                            {
                                keyList.Add(key);

                                this.formsPlot1.Plot.AddScatterLines(batchX.ToArray(), batchY.ToArray(), color:Color.Blue, label: "扭矩角度曲线 " + key);
                            }
                            else
                            {
                                this.formsPlot1.Plot.AddScatterLines(batchX.ToArray(), batchY.ToArray(), color: Color.Blue);
                            }
                        }));
                    });

                }
            }

            if (allDataLoaded)
            {
                timer1.Stop(); // 所有数据加载完成后，停止定时器
                timer1.Dispose(); // 释放定时器资源
                //恢复 ScottPlot 交互
                this.formsPlot1.Configuration.LockHorizontalAxis = false;
                this.formsPlot1.Configuration.LockVerticalAxis = false;
                this.formsPlot1.Plot.AxisAuto();// 设置zoom to fit
                this.formsPlot1.Plot.Render();
                ucProcessLine1.Value = 100;
                ucProcessLine1.Visible = false;
                ucProcessLine1.Value = 0;
                totalCnt = 0;
                readCnt = 0;
                keyList.Clear();
            }
        }

        /// <summary>
        /// 清除历史数据分析文本
        /// </summary>
        private void ClearLastData()
        {
            //清除旧切线轴
            ClearSlopeLine();

            // 擦除旧文本
            ClearTextAnnotation1();
            ClearTextAnnotation2();

            //清除旧纵轴
            ClearCursor1();
            ClearCursor2();

        }

        /// <summary>
        /// 清除切线
        /// </summary>
        private void ClearSlopeLine()
        {
            //清除旧切线轴
            if (slopeLineList != null && slopeLineList.Count != 0)
            {
                foreach (var line in slopeLineList)
                {
                    formsPlot1.Plot.Remove(line);
                }
                slopeLineList.Clear();

                // 刷新图表
                formsPlot1.Render();
            }
        }

        /// <summary>
        /// 清除第一光标
        /// </summary>
        private void ClearCursor1()
        {
            //清除旧光标
            if (verticalLine1 != null)
            {
                formsPlot1.Plot.Remove(verticalLine1);

                // 刷新图表
                formsPlot1.Render();
            }
        }

        /// <summary>
        /// 清除第二光标
        /// </summary>
        private void ClearCursor2()
        {
            //清除旧光标
            if (verticalLine2 != null)
            {
                formsPlot1.Plot.Remove(verticalLine2);

                // 刷新图表
                formsPlot1.Render();
            }
        }

        /// <summary>
        /// 清除文本1
        /// </summary>
        private void ClearTextAnnotation1()
        {
            // 擦除旧文本
            if (textAnnotation1 != null)
            {
                formsPlot1.Plot.Remove(textAnnotation1);
                textAnnotation1 = null;

                // 刷新图表
                formsPlot1.Render();
            }
        }

        /// <summary>
        /// 清除文本2
        /// </summary>
        private void ClearTextAnnotation2()
        {
            // 擦除旧文本
            if (textAnnotation2 != null)
            {
                formsPlot1.Plot.Remove(textAnnotation2);
                textAnnotation2 = null;

                // 刷新图表
                formsPlot1.Render();
            }
        }

        /// <summary>
        /// 画切线
        /// </summary>
        /// <param name="mouseX"></param>
        /// <param name="mouseY"></param>
        private void GetSlopeLine(double mouseX, double mouseY, double[] XVal, double[] YVal)
        {
            // 查找最接近的点
            var nearestIndex = Enumerable.Range(0, XVal.Length)
                .OrderBy(i => Math.Abs(XVal[i] - mouseX))
                .First();

            //当前鼠标所在X坐标在数据长度内绘制
            if (XVal.Max() >= mouseX && mouseX >= XVal.Min())
            {
                if (nearestIndex >= 1 && nearestIndex < YVal.Length - 1)
                {
                    // 使用两点法近似计算斜率
                    double dy = YVal[nearestIndex + 1] - YVal[nearestIndex - 1];
                    double dx = XVal[nearestIndex + 1] - XVal[nearestIndex - 1];
                    double slope = dy / dx;//斜率

                    // 获取当前图表的 X 和 Y 轴上下限
                    double xMin = formsPlot1.Plot.GetAxisLimits().XMin;
                    double xMax = formsPlot1.Plot.GetAxisLimits().XMax;
                    double yMin = formsPlot1.Plot.GetAxisLimits().YMin;
                    double yMax = formsPlot1.Plot.GetAxisLimits().YMax;

                    // y = slope * x + offset
                    double offset = YVal[nearestIndex] - slope * XVal[nearestIndex];//偏移量

                    double y0 = yMin;
                    double x0 = (y0 - offset) / slope;
                    double y1 = yMax;
                    double x1 = (y1 - offset) / slope;

                    var slopeLine = formsPlot1.Plot.AddLine(x0, y0, x1, y1);
                    slopeLine.Color = System.Drawing.Color.Green;
                    slopeLine.LineWidth = 1;
                    slopeLineList.Add(slopeLine);
                    label_slope.Text = "切线斜率: ";
                    label_slope.Text += slope.ToString("0.00");

                    //更新斜率和偏移量
                    targetSlope = slope;
                    targetOffset = offset;
                }
            }
        }

        /// <summary>
        /// 画垂直线
        /// </summary>
        /// <param name="mouseX"></param>
        /// <param name="mouseY"></param>
        private void GetVerticalLine(double mouseX, double mouseY, double[] XVal, double[] YVal)
        {
            //显示纵轴
            if (isDrawFirstCursor)
            {
                verticalLine1 = formsPlot1.Plot.AddVerticalLine(mouseX, System.Drawing.Color.CadetBlue, 2);
                verticalLine1.X = mouseX;
            }
            else if (isDrawSecondCursor)
            {
                verticalLine2 = formsPlot1.Plot.AddVerticalLine(mouseX, System.Drawing.Color.CadetBlue, 2);
                verticalLine2.X = mouseX;
            }
            else
            {
                return;
            }

            //显示文本的指定内容
            string text = "";

            // 查找最接近的点
            var nearestIndex = Enumerable.Range(0, XVal.Length)
                .OrderBy(i => Math.Abs(XVal[i] - mouseX))
                .First();

            //当前鼠标所在X坐标在数据长度内绘制
            if (XVal.Max() >= mouseX && mouseX >= XVal.Min())
            {
                // 显示数据
                if (isDrawFirstCursor)
                {
                    text += $" 第一光标信息汇总:  \n" +
                            $" 设备: {comboBox_Dev.Text} \n" +
                            $" 角度（X）: {XVal[nearestIndex]:F3}\n" +
                            $" 扭矩（Y）: {YVal[nearestIndex]:F2}\n";

                    this.label_torquePeak.Text =  $"峰值扭矩：{YVal.Max():F2}";
                    this.label_anglePeak.Text =  $"峰值角度：{XVal.Max():F3}";
                    this.label_torqueSeparate.Text = $"分离扭矩：{YVal[nearestIndex]:F2}";
                    torqueDiff1 = YVal[nearestIndex];
                    cursorAngle = XVal[nearestIndex];//仅第一光标才会触发残余扭矩
                }
                else if (isDrawSecondCursor)
                {
                    text += $" 第二光标信息汇总:  \n" +
                            $" 设备: {comboBox_Dev.Text} \n" +
                            $" 角度（X）: {XVal[nearestIndex]:F3}\n" +
                            $" 扭矩（Y）: {YVal[nearestIndex]:F2}\n";

                    this.label_torquePeak.Text = $"峰值扭矩：{YVal.Max():F2}";
                    this.label_anglePeak.Text = $"峰值角度：{XVal.Max():F3}";
                    torqueDiff2 = YVal[nearestIndex];
                }
            }
            else
            {
                if (isDrawFirstCursor) cursorAngle = double.MinValue;//画曲线在有效区域外，异常数据

                for (int i = 0; i < dataGridView1.SelectedRows.Count; i++)
                {
                    dataGridView1.SelectedRows[i].Selected = false;//表格取消选中
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
                if (isDrawFirstCursor)
                {
                    textAnnotation1 = formsPlot1.Plot.AddText(text, mouseX, mouseY, color: System.Drawing.Color.Blue, size: 12);

                    //第一光标可选中对应表格
                    for (int i = 0; i < dataGridView1.SelectedRows.Count; i++)
                    {
                        dataGridView1.SelectedRows[i].Selected = false;//表格取消选中
                    }
                    
                    dataGridView1.Rows[nearestIndex].Selected = true;//表格选中                                                              
                    dataGridView1.FirstDisplayedScrollingRowIndex = nearestIndex > 15 ? nearestIndex - 15 : 0; //索引移到表格
                }
                else if (isDrawSecondCursor)
                {
                    textAnnotation2 = formsPlot1.Plot.AddText(text, mouseX, mouseY, color: System.Drawing.Color.Blue, size: 12);
                }
            }

            //两条光标信息均有效显示差值
            if (textAnnotation1 != null && textAnnotation2 != null)
            {
                this.label_torqueDiff.Text = $"扭矩差值：{torqueDiff2 - torqueDiff1:F2}";//第二光标显示扭矩差值
            }
        }

        /// <summary>
        /// 筛选递增数据
        /// </summary>
        /// <param name="angleArr"></param>
        /// <returns></returns>
        private double[] FilterData(double[] angleArr)
        {
            List<double> filteredData = new List<double>();//筛选后的数据

            int dataLen = angleArr.Length;// 获取数组长度

            if (dataLen <= 1) return angleArr;

            filteredData.Add(angleArr[0]);

            for (int i = 1; i < dataLen; i++)
            {
                //A2 - A1 <= 0 的数据删除，只留递增数据
                if (angleArr[i] - filteredData.Last() > 0)
                {
                    filteredData.Add(angleArr[i]);
                }
            }

            return filteredData.ToArray();
        }

        /// <summary>
        /// 筛选递增数据的下标
        /// </summary>
        /// <param name="angleArr"></param>
        /// <returns></returns>
        private int[] IncIndexAddr(double[] angleArr)
        {
            List<int> filteredData = new List<int>();//筛选后的数据

            int dataLen = angleArr.Length;// 获取数组长度

            if (dataLen <= 1)
            {
                filteredData.Add(0);
                return filteredData.ToArray();
            }

            filteredData.Add(0);

            for (int i = 1; i < dataLen; i++)
            {
                //只留递增数据
                if (angleArr[i] - angleArr[filteredData.Last()] > 0)
                {
                    filteredData.Add(i);
                }
            }

            return filteredData.ToArray();
        }

        /// <summary>
        /// 获取峰值数据——只看F2 || F3
        /// </summary>
        private void GetPeakData()
        {
            if (dataTable.Rows.Count == 0) return;

            if (btn_dealData.Text.Trim() == "数据预处理")
            {
                isDataDeal = true;

                // 创建一个新的 DataTable 用于存放符合条件的行
                DataTable filteredDataTable = new DataTable();
                filteredDataTable = dataTable.Clone(); // 复制结构，包括列的信息

                // 使用 LINQ 查询筛选出符合条件的行
                var query = from row in dataTable.AsEnumerable()
                            where ((row.Field<string>("设备型号").IndexOf("6") == -1 && row.Field<string>("数据类型") == "243")
                                   || (row.Field<string>("设备型号").IndexOf("6") != -1 && row.Field<string>("数据类型") == "242"))
                            select row;

                // 将查询结果复制到新的 DataTable 中
                foreach (DataRow row in query)
                {
                    filteredDataTable.ImportRow(row);
                }

                _bindingSource.DataSource = filteredDataTable;
                btn_dealData.Text = "取消预处理";

                this.formsPlot1.Visible = true;
            }
            else if (btn_dealData.Text.Trim() == "取消预处理")
            {
                isDataDeal = false;
                //
                _bindingSource.DataSource = dataTable;
                btn_dealData.Text = "数据预处理";

                this.formsPlot1.Visible = false;
            }
        }

        /// <summary>
        /// 获取残余扭矩
        /// </summary>
        /// <param name="slope"></param>
        /// <param name="offset"></param>
        /// <param name="cursorX"></param>
        /// <returns></returns>
        private double GetResidualTorque(double slope, double offset, double cursorX)
        {
            //切线与第一光标的交点纵坐标 —— y = kx + △t
            double residulTorque = slope * cursorX + offset;
            return residulTorque;
        }

        //设备切换
        private void comboBox_Dev_SelectedIndexChanged(object sender, EventArgs e)
        {
            //清除历史设备数据
            this.formsPlot1.Plot.Clear();
            targetXVal.Clear();
            targetYVal.Clear();
            comboBox_Point.Items.Clear();

            //根据站点更新对应的工单点位
            if (WrenchDic.ContainsKey(comboBox_Dev.Text))
            {
                //针对工单数据的情况
                if (WrenchDic[comboBox_Dev.Text] != null && WrenchDic[comboBox_Dev.Text].Count > 0)
                {
                    foreach (var pointNum in WrenchDic[comboBox_Dev.Text])
                    {
                        comboBox_Point.Items.Add(pointNum.ToString());//确定点位号
                    }

                    comboBox_Point.SelectedIndex = 0;//触发点位切换函数
                    this.comboBox_Point.Visible = true;
                }
                //针对实时数据的情况
                else
                {
                    // 创建一个新的 DataTable 用于存放符合条件的行
                    DataTable filteredDataTable = new DataTable();
                    filteredDataTable = dataTable.Clone(); // 复制结构，包括列的信息

                    List<double> xVal = new List<double>();//设备的过程数据
                    List<double> yVal = new List<double>();

                    // 使用 LINQ 查询筛选出符合条件的行
                    filteredDataTable = dataTable.AsEnumerable()
                                        .Where(row => row.Field<string>("设备站点") == comboBox_Dev.Text)
                                        .CopyToDataTable();

                    DataTable copyFiltTable = filteredDataTable.Copy();//用于拷贝筛选后的表
                    filteredDataTable.Clear();

                    //获取角度递增的下标
                    int[] targetIndexArr = IncIndexAddr(DataDic[comboBox_Dev.Text].ToList().Select(dsData => double.Parse(dsData.Angle)).ToArray());

                    // 将查询结果复制到新的 DataTable 中
                    for (int i = 0; i < targetIndexArr.Length; i++)
                    {
                        DataRow dataRow = copyFiltTable.Rows[targetIndexArr[i]];
                        filteredDataTable.ImportRow(dataRow);
                        filteredDataTable.Rows[i][0] = i + 1;//序号从头始

                        xVal.Add(double.Parse(DataDic[comboBox_Dev.Text][targetIndexArr[i]].Angle));
                        yVal.Add(double.Parse(DataDic[comboBox_Dev.Text][targetIndexArr[i]].Torque.Split(' ')[0]));

                        targetXVal.Add(double.Parse(DataDic[comboBox_Dev.Text][targetIndexArr[i]].Angle));
                        targetYVal.Add(double.Parse(DataDic[comboBox_Dev.Text][targetIndexArr[i]].Torque.Split(' ')[0]));
                    }

                    csvTable = filteredDataTable.Copy();//csv另存的原始数据表

                    _bindingSource.DataSource = filteredDataTable;
                    this.formsPlot1.Visible = true;

                    //画过程数据曲线
                    DrwaProcessCurve(xVal.ToArray(), yVal.ToArray(), Convert.ToInt16(comboBox_Dev.Text));
                }
            }

            //// 创建一个新的 DataTable 用于存放符合条件的行
            //DataTable filteredDataTable = new DataTable();
            //filteredDataTable = dataTable.Clone(); // 复制结构，包括列的信息

            //List<double> xVal = new List<double>();//设备的过程数据
            //List<double> yVal = new List<double>();

            //// 使用 LINQ 查询筛选出符合条件的行
            //filteredDataTable = dataTable.AsEnumerable()
            //                    .Where(row => row.Field<string>("设备站点") == comboBox_Dev.Text)
            //                    .CopyToDataTable();

            //DataTable copyFiltTable = filteredDataTable.Copy();//用于拷贝筛选后的表
            //filteredDataTable.Clear();

            ////获取角度递增的下标
            //int[] targetIndexArr = IncIndexAddr(DataDic[comboBox_Dev.Text].ToList().Select(dsData => double.Parse(dsData.Angle)).ToArray());

            //// 将查询结果复制到新的 DataTable 中
            //for (int i = 0; i < targetIndexArr.Length; i++)
            //{
            //    DataRow dataRow = copyFiltTable.Rows[targetIndexArr[i]];
            //    filteredDataTable.ImportRow(dataRow);
            //    filteredDataTable.Rows[i][0] = i + 1;//序号从头始

            //    xVal.Add(double.Parse(DataDic[comboBox_Dev.Text][targetIndexArr[i]].Angle));
            //    yVal.Add(double.Parse(DataDic[comboBox_Dev.Text][targetIndexArr[i]].Torque.Split(' ')[0]));

            //    targetXVal.Add(double.Parse(DataDic[comboBox_Dev.Text][targetIndexArr[i]].Angle));
            //    targetYVal.Add(double.Parse(DataDic[comboBox_Dev.Text][targetIndexArr[i]].Torque.Split(' ')[0]));
            //}

            //csvTable = filteredDataTable.Copy();//csv另存的原始数据表

            //_bindingSource.DataSource = filteredDataTable;
            //this.formsPlot1.Visible = true;

            ////画过程数据曲线
            //DrwaProcessCurve(xVal.ToArray(), yVal.ToArray(), Convert.ToInt16(comboBox_Dev.Text));
        }

        //点位切换
        private void comboBox_Point_SelectedIndexChanged(object sender, EventArgs e)
        {
            //清除历史设备数据
            this.formsPlot1.Plot.Clear();
            targetXVal.Clear();
            targetYVal.Clear();

            // 创建一个新的 DataTable 用于存放符合条件的行
            DataTable filteredDataTable = new DataTable();
            filteredDataTable = dataTable.Clone(); // 复制结构，包括列的信息

            List<double> xVal = new List<double>();//设备的过程数据
            List<double> yVal = new List<double>();

            int tempIndex = 0; //临时序号下标

            // 筛选出指定设备站点的数据行
            filteredDataTable = dataTable.AsEnumerable()
                                .Where(row => row.Field<string>("设备站点") == comboBox_Dev.Text)
                                .CopyToDataTable();

            DataTable copyFiltTable = filteredDataTable.Copy();//用于拷贝筛选后的表
            filteredDataTable.Clear();

            //获取角度递增的下标
            int[] targetIndexArr = IncIndexAddr(DataDic[comboBox_Dev.Text].ToList()
                                                                          .Select(dsData => double.Parse(dsData.Angle))
                                                                          .ToArray());

            // 将查询结果复制到新的 DataTable 中
            for (int i = 0; i < targetIndexArr.Length; i++)
            {
                DataRow dataRow = copyFiltTable.Rows[targetIndexArr[i]];

                Console.WriteLine(dataRow[6].ToString() +   ""+ comboBox_Point.Text);
                if (dataRow[6].ToString() == comboBox_Point.Text)
                {
                    filteredDataTable.ImportRow(dataRow);
                    filteredDataTable.Rows[tempIndex][0] = ++tempIndex;//序号从头始

                    xVal.Add(double.Parse(DataDic[comboBox_Dev.Text][targetIndexArr[i]].Angle));
                    yVal.Add(double.Parse(DataDic[comboBox_Dev.Text][targetIndexArr[i]].Torque.Split(' ')[0]));

                    targetXVal.Add(double.Parse(DataDic[comboBox_Dev.Text][targetIndexArr[i]].Angle));
                    targetYVal.Add(double.Parse(DataDic[comboBox_Dev.Text][targetIndexArr[i]].Torque.Split(' ')[0]));
                }
            }

            csvTable = filteredDataTable.Copy();//csv另存的原始数据表

            _bindingSource.DataSource = filteredDataTable;
            this.formsPlot1.Visible = true;

            //画过程数据曲线
            DrwaProcessCurve(xVal.ToArray(), yVal.ToArray(), Convert.ToInt16(comboBox_Dev.Text));
        }
    }
}
