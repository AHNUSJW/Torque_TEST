using DBHelper;
using HZH_Controls.Controls;
using Library;
using Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

//Ricardo  20240328
//Lumi     20240530
//Ricardo  20240606

namespace Base.UI.MenuDevice
{
    public partial class MenuActualDataForm : Form
    {
        #region 变量定义

        private XET actXET;                   //操作的设备

        private double torque = 0;            //实时扭矩
        private double angle = 0;             //实时角度
        private double torqueOld = 0;         //记录扭矩(用于针对06 -Track模式连续读取0的情况)
        private double angleOld = 0;          //用于针对画曲线时02数据，记录成02之前的01值
        private double torquePeak = 0;        //峰值扭矩
        private double anglePeak = 0;         //峰值角度
        private string torqueUnit = "";       //扭矩单位
        private Int32 lines = 0;              //表格行数

        private List<double> oldTorqueUpper = new List<double>();               //旧扭矩坐标上限
        private List<double> oldTorqueLower = new List<double>();               //旧扭矩坐标下限
        private List<double> newTorqueUpper = new List<double>();               //新扭矩坐标上限
        private List<double> newTorqueLower = new List<double>();               //新扭矩坐标下限
        private List<double> oldAngleUpper = new List<double>();                //旧角度坐标上限
        private List<double> oldAngleLower = new List<double>();                //旧角度坐标下限
        private List<double> newAngleUpper = new List<double>();                //新角度坐标上限
        private List<double> newAngleLower = new List<double>();                //新角度坐标下限

        private List<DrawPicture> myPictures = new List<DrawPicture>();         //绘图
        private List<List<double>> torqueLists = new List<List<double>>();      //扭矩集合，用于画扭矩曲线
        private List<List<double>> angleLists = new List<List<double>>();       //角度集合，用于画角度曲线
        private List<PointF> PointList = new List<PointF>();
        private int[] snbatArr;                                                 //记录对应扳手的作业号中的尾数
        private string[] opsnTimeArr;                                           //记录对应扳手的作业号中的时间标志

        private Dictionary<string, List<double>> TorquedataGroups = new Dictionary<string, List<double>>(); //作业号字典，存储每次拧紧任务的扭矩集合
        private Dictionary<string, List<double>> AngledataGroups = new Dictionary<string, List<double>>(); //作业号字典，存储每次拧紧任务的角度集合

        private Dictionary<string, bool> dataGroupResults = new Dictionary<string, bool>();//作业号字典，存储每次作业的拧紧结果

        #endregion

        public MenuActualDataForm()
        {
            //设置窗体的双缓冲
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();

            //
            InitializeComponent();

            //利用反射设置DataGridView的双缓冲
            Type myType = this.dataGridView1.GetType();
            PropertyInfo pi = myType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(this.dataGridView1, true, null);
        }

        #region 页面加载关闭

        //页面加载
        private void MenuActualDataForm_Load(object sender, EventArgs e)
        {
            InitForm();

            #region 表格初始化

            //表格初始化
            dataGridView1.EnableHeadersVisualStyles = false;//允许自定义行头样式
            dataGridView1.RowHeadersVisible = false; //第一列空白隐藏掉
            dataGridView1.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dataGridView1.AllowUserToAddRows = false;//禁止用户添加行
            dataGridView1.AllowUserToDeleteRows = false;//禁止用户删除行
            dataGridView1.AllowUserToOrderColumns = false;//禁止用户手动重新定位行
            dataGridView1.AllowUserToResizeRows = false;//禁止用户调整行大小
            dataGridView1.AllowUserToResizeColumns = false;//禁止用户调整列大小
            //dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;//表格自动填充

            dataGridView1.ReadOnly = true; // 如果只需要展示数据，设置为只读可以提高性能

            #endregion

            #region 下拉框初始化

            //曲线
            List<KeyValuePair<string, string>> curve_Mode = new List<KeyValuePair<string, string>>();
            curve_Mode.Add(new KeyValuePair<string, string>("0", "扭矩曲线分析"));
            curve_Mode.Add(new KeyValuePair<string, string>("1", "角度曲线分析"));
            curve_Mode.Add(new KeyValuePair<string, string>("2", "扭矩角度分析"));

            ucCombox1.Source = curve_Mode;
            ucCombox1.SelectedIndex = 0;
            ucCombox1.ConerRadius = 2;
            ucCombox1.RectColor = SystemColors.GradientActiveCaption;
            ucCombox1.BoxStyle = ComboBoxStyle.DropDownList;

            //设备号
            List<KeyValuePair<string, string>> device_Mode = new List<KeyValuePair<string, string>>();
            for (int i = 0; i < MyDevice.devSum; i++)
            {
                if ((MyDevice.protocol.type == COMP.XF && MyDevice.mXF[MyDevice.AddrList[i]].sTATE == STATE.WORKING) ||
                    (MyDevice.protocol.type == COMP.TCP && MyDevice.mTCP[MyDevice.AddrList[i]].sTATE == STATE.WORKING) ||
                    (MyDevice.protocol.type == COMP.RS485 && MyDevice.mRS[MyDevice.AddrList[i]].sTATE == STATE.WORKING) ||
                    (MyDevice.protocol.type == COMP.UART && MyDevice.mBUS[MyDevice.AddrList[i]].sTATE == STATE.WORKING)
                    )
                {
                    device_Mode.Add(new KeyValuePair<string, string>(i.ToString(), MyDevice.AddrList[i].ToString()));
                    DrawPicture picture = new DrawPicture(pictureBox1.Height, pictureBox1.Width, BackgroundImageType.OnlyXYAxis);
                    myPictures.Add(picture);

                    //增加对应的扳手需求变量
                    List<double> torquelist = new List<double>();
                    torqueLists.Add(torquelist);
                    List<double> anglelist = new List<double>();
                    angleLists.Add(anglelist);

                    //初始化所有扳手上下限
                    oldTorqueUpper.Add(0);
                    oldTorqueLower.Add(0);
                    newTorqueUpper.Add(0);
                    newTorqueLower.Add(0);
                    oldAngleUpper.Add(0);
                    oldAngleLower.Add(0);
                    newAngleUpper.Add(0);
                    newAngleLower.Add(0);
                }
            }

            if (device_Mode.Count == 0) return;//无效设备，终止函数

            #endregion

            //曲线区域初始化
            //myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] = new DrawPicture(pictureBox1.Height, pictureBox1.Width, BackgroundImageType.OnlyXYAxis);

            MyDevice.myTaskManager.SelectedDev = MyDevice.AddrList[0];
            MyDevice.protocol.addr = MyDevice.AddrList[0];

            //TCP模式要切换端口
            if (MyDevice.protocol.type == COMP.TCP)
            {
                //防止字典查询溢出
                if (MyDevice.addr_ip.ContainsKey(MyDevice.protocol.addr.ToString()) &&
                    MyDevice.clientConnectionItems.ContainsKey(MyDevice.addr_ip[MyDevice.protocol.addr.ToString()])
                    )
                {
                    MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
                }
                else
                {
                    return;
                }
            }
            actXET = MyDevice.actDev;
            actXET.torqueMultiple = (int)Math.Pow(10, actXET.devc.torque_decimal);
            actXET.angleMultiple = (int)Math.Pow(10, actXET.para.angle_decimal);

            snbatArr = new int[MyDevice.devSum];
            opsnTimeArr = new string[MyDevice.devSum];
            for (int i = 0; i < MyDevice.devSum; i++)
            {
                snbatArr[i] = 1;
                opsnTimeArr[i] = System.DateTime.Now.ToString("yyyyMMddHHmm");
            }
            if (opsnTimeArr.Count() > 0)
            {
                actXET.snBat = snbatArr[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)];    //读取结果帧作业号更新
                actXET.opsn = actXET.wlan.addr + opsnTimeArr[0] + " " + actXET.snBat.ToString().PadLeft(4, '0');
            }

            TorquedataGroups.Add(actXET.opsn, new List<double>());
            AngledataGroups.Add(actXET.opsn, new List<double>());
            dataGroupResults.Add(actXET.opsn, false);

            MyDevice.myTaskManager.Mode = AutoMode.UserAndActualData;
            //界面更新事件
            MyDevice.myTaskManager.UpdateUI += updateUI;

            //重置数据库数据存储信息（非工单）
            MyDevice.DataType = "ActualData";
            MyDevice.WorkId = 0;
            MyDevice.WorkNum = "";
            MyDevice.SequenceId = "";
            MyDevice.PointNum = "";
            MyDevice.DataResult = "NG";
            MyDevice.Vin = DateTime.Now.ToString("yyyyMMddHHmm");
        }

        //页面关闭
        private void MenuActualDataForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            TorquedataGroups.Clear();
            AngledataGroups.Clear();

            //界面更新事件
            MyDevice.myTaskManager.Mode = AutoMode.UserOnly;
            MyDevice.myTaskManager.UpdateUI -= updateUI;
            MyDevice.myTaskManager.SelectedDev = 0;
        }


        //页面大小变化
        private void MenuActualDataForm_SizeChanged(object sender, EventArgs e)
        {
            if (myPictures.Count > 0)
            {
                myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Width = pictureBox1.Width;
                myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Height = pictureBox1.Height;
                pictureBox1.BackgroundImage = myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].GetBackgroundImage();

                dataGridView1.Columns[0].Width = this.dataGridView1.Width * 1 / 10;
                dataGridView1.Columns[1].Width = this.dataGridView1.Width * 25 / 100;
                dataGridView1.Columns[2].Width = this.dataGridView1.Width * 15 / 100;
                dataGridView1.Columns[3].Width = this.dataGridView1.Width * 1 / 10;
                dataGridView1.Columns[4].Width = this.dataGridView1.Width * 2 / 10;
                dataGridView1.Columns[5].Width = this.dataGridView1.Width * 2 / 10;
            }
        }

        #endregion

        #region 控件功能

        //曲线模式切换
        private void ucCombox1_SelectedChangedEvent(object sender, EventArgs e)
        {
            pictureBox1.Image = null;

            if (myPictures.Count > 0)
            {
                myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] = new DrawPicture(pictureBox1.Height, pictureBox1.Width, BackgroundImageType.OnlyXYAxis);

                //横轴数量(网格)
                myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].HorizontalAxisNum = 11;

                if (ucCombox1.SelectedIndex == 0 && torqueLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Count > 0)
                {
                    myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].LimitUpperLeftY = newTorqueUpper[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)];
                    myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].LimitLowerLeftY = newTorqueLower[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)];

                    //画x轴,y轴,网格
                    pictureBox1.BackgroundImage = myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].GetBackgroundImage();

                    pictureBox1.Image = myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].GetForegroundImage(torqueLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].ToArray());
                }
                else if (ucCombox1.SelectedIndex == 1 && angleLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Count > 0)
                {
                    myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].LimitUpperLeftY = newAngleUpper[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)];
                    myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].LimitLowerLeftY = newAngleLower[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)];

                    //画x轴,y轴,网格
                    pictureBox1.BackgroundImage = myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].GetBackgroundImage();

                    pictureBox1.Image = myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].GetForegroundImage(angleLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].ToArray());
                }
                else if (ucCombox1.SelectedIndex == 2 && torqueLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Count > 0)
                {
                    myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].LimitUpperLeftY = newTorqueUpper[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)];
                    myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].LimitLowerLeftY = newTorqueLower[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)];

                    //画x轴,y轴,网格
                    pictureBox1.BackgroundImage = myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].GetBackgroundImage();

                    pictureBox1.Image = myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].GetForegroundImage_Two(angleLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].ToArray(), torqueLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].ToArray());
                }
            }
        }

        //设备切换
        private void ucCombox2_SelectedChangedEvent(object sender, EventArgs e)
        {
            if (myPictures.Count > 0)
            {
                //MyDevice.myTaskManager.SelectedDev = Convert.ToByte(ucCombox2.TextValue);
                //MyDevice.protocol.addr = Convert.ToByte(ucCombox2.TextValue);

                //TCP模式要切换端口
                if (MyDevice.protocol.type == COMP.TCP)
                {
                    //防止字典查询溢出
                    if (MyDevice.addr_ip.ContainsKey(MyDevice.protocol.addr.ToString()) &&
                        MyDevice.clientConnectionItems.ContainsKey(MyDevice.addr_ip[MyDevice.protocol.addr.ToString()])
                        )
                    {
                        MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
                    }
                    else
                    {
                        return;
                    }
                }
                
                //更新设备
                actXET = MyDevice.actDev;
                actXET.torqueMultiple = (int)Math.Pow(10, actXET.devc.torque_decimal);
                actXET.angleMultiple = (int)Math.Pow(10, actXET.para.angle_decimal);

                //更新作业号
                if (snbatArr != null)
                {
                    actXET.snBat = snbatArr[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)];
                    actXET.opsn = actXET.wlan.addr + opsnTimeArr[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] + " " + actXET.snBat.ToString().PadLeft(4, '0');

                    if (!TorquedataGroups.ContainsKey(actXET.opsn))
                    {
                        TorquedataGroups.Add(actXET.opsn, new List<double>());
                        AngledataGroups.Add(actXET.opsn, new List<double>());
                        dataGroupResults.Add(actXET.opsn, false);
                    }
                }

                myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Width = pictureBox1.Width;
                myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Height = pictureBox1.Height;
                ucCombox1_SelectedChangedEvent(null, null);
            }
        }

        //清除数据
        private void btn_Clear_Click(object sender, EventArgs e)
        {
            //清除表格数据
            dataGridView1.Rows.Clear();
            lines = 0;

            if (MyDevice.AddrList.IndexOf(MyDevice.protocol.addr) != -1)
            {
                //清除曲线
                torqueLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Clear();
                angleLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Clear();
                pictureBox1.Image = null;
                myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] = new DrawPicture(pictureBox1.Height, pictureBox1.Width, BackgroundImageType.OnlyXYAxis);

                //画x轴,y轴,网格
                pictureBox1.BackgroundImage = myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].GetBackgroundImage();

                oldTorqueUpper[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] = 0;
                oldTorqueLower[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] = 0;
                newTorqueUpper[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] = 0;
                newTorqueLower[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] = 0;
                oldAngleUpper[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] = 0;
                oldAngleLower[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] = 0;
                newAngleUpper[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] = 0;
                newAngleLower[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] = 0;
            }

            //清除峰值
            torquePeak = 0;
            anglePeak = 0;
            label3.Text = "扭矩峰值：0";
            label4.Text = "角度峰值：0";
        }

        //鼠标点击图片
        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (actXET.devc.type != TYPE.TQ_XH_XL01_06 - (UInt16)ADDROFFSET.TQ_XH_ADDR && dataGridView1.RowCount > 0)
            {
                int px;

                //角度和扭矩曲线横坐标对应的是表格下标
                if (ucCombox1.SelectedIndex != 2)
                {
                    //有效曲线坐标区间内触发

                    if (torqueLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Count < (myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StopIdx - myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx))
                    {
                        if (e.X > myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx && e.X <= myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx + torqueLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Count)
                        {
                            //鼠标点击图片,显示轴线(-1的是因为数组下标是从0开始的)
                            showDataInfo(e.X - 1);
                        }
                    }
                    else
                    {
                        if (e.X > myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx && e.X <= myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StopIdx)
                        {
                            //鼠标点击图片,显示轴线(-1的是因为数组下标是从0开始的)
                            showCompressDataInfo(e.X);
                        }
                    }
                }
                //扭矩角度曲线横坐标对应的是角度
                else
                {
                    //曲线长度
                    double curveLen = angleLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Max() > Math.Abs(angleLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Min()) ? angleLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Max() : Math.Abs(angleLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Min());

                    //有效曲线坐标区间内触发
                    if (e.X > myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx && e.X <= myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx + curveLen)
                    {
                        //鼠标点击图片,显示轴线
                        showDataInfo(e.X - 1);
                    }
                }
            }
        }

        //鼠标点击表格
        private void dataGridView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (actXET.devc.type != TYPE.TQ_XH_XL01_06 - (UInt16)ADDROFFSET.TQ_XH_ADDR && dataGridView1.RowCount > 0)
            {
                int x;
                dataGridView1.Rows[dataGridView1.CurrentRow.Index].Selected = true;
                if (ucCombox1.SelectedIndex != 2)
                {
                    if (torqueLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Count <= 5000)
                    {
                        //画信息纵轴的起始横坐标
                        x = dataGridView1.CurrentRow.Index + myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx;
                    }
                    else
                    {
                        x = dataGridView1.CurrentRow.Index + myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx + (torqueLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Count - 5000);
                    }
                    //有效曲线坐标区间内触发
                    showDataInfo(x);
                }
                else
                {
                    //曲线长度
                    double curveLen = angleLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Max() > Math.Abs(angleLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Min()) ? angleLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Max() : Math.Abs(angleLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Min());

                    //

                    //画信息纵轴的起始横坐标
                    x = dataGridView1.CurrentRow.Index + myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx;

                    //有效曲线坐标区间内触发
                    if (x > myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx && x <= myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx + curveLen)
                    {
                        //鼠标点击图片,显示轴线
                        showDataInfo(x);
                    }
                }
            }

            string resultKey;

            if (this.dataGridView1.RowCount > 0 && dataGridView1.CurrentRow.Index >= 0)
            {
                resultKey = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[1].Value.ToString();

                //点击表格判断当前表格所在拧紧任务是否合格
                if (dataGroupResults.ContainsKey(resultKey) && dataGroupResults[resultKey])
                {
                    panel3.BackColor = Color.Green;
                }
                else
                {
                    panel3.BackColor = Color.CadetBlue;
                }
            }
        }

        // 鼠标右击选择生成csv文件
        private void dataGridView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && dataGridView1.Rows.Count > 0)
            {
                DialogResult result = MessageBox.Show($"是否保存数据导出?", "确认保存", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    string datFolderPath = Application.StartupPath + @"\dat"; //导出目录

                    if (!Directory.Exists(datFolderPath))
                    {
                        Directory.CreateDirectory(datFolderPath);
                    }

                    System.Windows.Forms.SaveFileDialog DialogSave = new System.Windows.Forms.SaveFileDialog();
                    DialogSave.Filter = "Excel(*.csv)|*.csv";
                    DialogSave.InitialDirectory = datFolderPath;     //默认路径
                    DialogSave.FileName = "XhTorque数据汇总表" + DateTime.Now.ToString("yyyyMMddHHmmss");

                    //if (DialogSave.ShowDialog() == DialogResult.OK)
                    //{
                    //    string myExcel = DialogSave.FileName;  //导出文件名
                    //    if (saveActualDataToExcel(myExcel))
                    //    {
                    //        if (MyDevice.languageType == 0)
                    //        {
                    //            MessageBox.Show("导出数据成功！");
                    //        }
                    //        else
                    //        {
                    //            MessageBoxEX.Show("Export  successfully！", "Hint", MessageBoxButtons.OK, new string[] { "OK" });
                    //        }
                    //        return;
                    //    }
                    //}
                    //else
                    //{
                    //    return;
                    //}

                    // 显示对话框并获取结果
                    if (DialogSave.ShowDialog() == DialogResult.OK)
                    {
                        // 获取用户选择的文件路径
                        string filePath = DialogSave.FileName;

                        // 将DataTable数据转换为CSV文件并保存到用户选择的路径
                        if (saveActualDataToCsv2(filePath))
                        {
                            MessageBox.Show("导出数据成功！");
                        }
                        return;
                    }
                }
                else
                    return;
            }
        }

        #endregion

        #region 页面表格曲线更新

        // 更新数据表
        private void updateDataTable_XH07()
        {
            // 暂时挂起布局逻辑
            dataGridView1.SuspendLayout();

            for (int i = 0; i < 5; i++)
            {
                if (actXET.data[i].dtype == 0xF1 || actXET.data[i].dtype == 0xF2 || actXET.data[i].dtype == 0xF3)
                {
                    if (actXET.data[i].dtype == 0xF1 && actXET.data[i].torque == 0 && actXET.data[i].angle == 0) break;

                    //获取实时数据
                    if (actXET.data[i].dtype == 0xF1)
                    {
                        torque = actXET.data[i].torque / (double)actXET.torqueMultiple;
                        torqueLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Add(torque);
                        //单位更新
                        switch (actXET.data[i].torque_unit)
                        {
                            case UNIT.UNIT_nm: torqueUnit = "N·m"; break;
                            case UNIT.UNIT_lbfin: torqueUnit = "lbf·in"; break;
                            case UNIT.UNIT_lbfft: torqueUnit = "lbf·ft"; break;
                            case UNIT.UNIT_kgcm: torqueUnit = "kgf·cm"; break;
                            case UNIT.UNIT_kgm: torqueUnit = "kgf·m"; break;
                            default: break;
                        }
                    }
                    else if (actXET.data[i].dtype == 0xF2)
                    {
                        torque = actXET.data[i].torseries_pk / (double)actXET.torqueMultiple;
                        torqueLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Add(0);                         //用于画曲线描点对应的02值
                                                                                             //单位更新
                        switch (actXET.data[i].torque_unit)
                        {
                            case UNIT.UNIT_nm: torqueUnit = "N·m"; break;
                            case UNIT.UNIT_lbfin: torqueUnit = "lbf·in"; break;
                            case UNIT.UNIT_lbfft: torqueUnit = "lbf·ft"; break;
                            case UNIT.UNIT_kgcm: torqueUnit = "kgf·cm"; break;
                            case UNIT.UNIT_kgm: torqueUnit = "kgf·m"; break;
                            default: break;
                        }
                    }
                    else if (actXET.data[i].dtype == 0xF3)
                    {
                        torque = actXET.data[i].torgroup_pk / (double)actXET.torqueMultiple;
                        torqueLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Add(0);
                    }


                    if (actXET.data[i].dtype == 0xF1)
                    {
                        angle = actXET.data[i].angle / (double)actXET.angleMultiple;
                        angleLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Add(angle);
                        angleOld = angle;
                    }
                    else
                    {
                        angle = actXET.data[i].angle_acc / (double)actXET.angleMultiple;
                        angleLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Add(angleOld);
                    }


                    //计算峰值扭矩和峰值角度
                    torquePeak = Math.Abs(torque) > Math.Abs(torquePeak) ? torque : torquePeak;
                    anglePeak = Math.Abs(angle) > Math.Abs(anglePeak) ? angle : anglePeak;

                    //更新表格
                    int idx = dataGridView1.Rows.Add();            //实时表格的行数下标

                    if (actXET.data[i].dtype == 0xF1)
                    {
                        dataGridView1.Rows[idx].Cells[0].Value = (++lines).ToString();
                    }
                    else if (actXET.data[i].dtype == 0xF2)
                    {
                        dataGridView1.Rows[idx].Cells[0].Value = "☆" + (++lines);
                    }
                    else if (actXET.data[i].dtype == 0xF3)
                    {
                        dataGridView1.Rows[idx].Cells[0].Value = "★" + (++lines);
                    }

                    dataGridView1.Rows[idx].Cells[1].Value = actXET.opsn;
                    dataGridView1.Rows[idx].Cells[2].Value = actXET.data[i].stamp;
                    dataGridView1.Rows[idx].Cells[3].Value = actXET.wlan.addr;
                    dataGridView1.Rows[idx].Cells[4].Value = torque + " " + torqueUnit;
                    dataGridView1.Rows[idx].Cells[5].Value = angle + " °";
                    label3.Text = "扭矩峰值：" + torquePeak + torqueUnit;
                    label4.Text = "角度峰值：" + anglePeak + "°";

                    TorquedataGroups[actXET.opsn].Add(torque);
                    AngledataGroups[actXET.opsn].Add(angle);

                    //判断数据结果是否合格
                    if (IsValid(actXET.data[i], actXET, torque * actXET.torqueMultiple, angle * actXET.angleMultiple))
                    {
                        dataGroupResults[actXET.opsn] = true;
                        panel3.BackColor = Color.Green;
                    }
                    else
                    {
                        dataGroupResults[actXET.opsn] = false;
                        panel3.BackColor = Color.CadetBlue;
                    }

                    //更新作业号
                    if (actXET.data[i].dtype == 0xF3)
                    {

                        //计算残余扭矩
                        try
                        {
                            label2.Text = "残余扭矩值: " + GetResidualTorque(1, new List<double>(TorquedataGroups[actXET.opsn]), new List<double>(AngledataGroups[actXET.opsn]));
                        }
                        catch
                        {
                            Console.WriteLine("异常");
                        }

                        snbatArr[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)]++;
                        opsnTimeArr[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] = System.DateTime.Now.ToString("yyyyMMddHHmm");
                        actXET.snBat = snbatArr[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)];    //读取结果帧作业号更新
                        actXET.opsn = actXET.wlan.addr + opsnTimeArr[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] + " " + actXET.snBat.ToString().PadLeft(4, '0');

                        //新增新的扭矩角度作业
                        TorquedataGroups.Add(actXET.opsn, new List<double>());
                        AngledataGroups.Add(actXET.opsn, new List<double>());
                        dataGroupResults.Add(actXET.opsn, false);
                    }

                    //移到最后一行
                    if (dataGridView1.RowCount > 1)
                    {
                        dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.RowCount - 1;
                    }

                }
            }

            // 解除选择
            dataGridView1.ClearSelection();

            //超过5000行删除
            while (dataGridView1.Rows.Count > 5000)
            {
                dataGridView1.Rows.RemoveAt(0);
            }

            //恢复布局逻辑，并手动刷新 DataGridView
            dataGridView1.ResumeLayout();
            dataGridView1.Refresh(); // 手动刷新 DataGridView
        }

        // 更新数据表
        private void updateDataTable_XH06()
        {
            // 暂时挂起布局逻辑
            dataGridView1.SuspendLayout();

            for (int i = 0; i < 5; i++)
            {
                if (actXET.data[i].dtype == 0xF1 || actXET.data[i].dtype == 0xF2)
                {
                    if (actXET.data[i].dtype == 0xF1 && actXET.data[i].torque == 0 && actXET.data[i].angle == 0) break;

                    //单位更新
                    switch (actXET.data[i].torque_unit)
                    {
                        case UNIT.UNIT_nm: torqueUnit = "N·m"; break;
                        case UNIT.UNIT_lbfin: torqueUnit = "lbf·in"; break;
                        case UNIT.UNIT_lbfft: torqueUnit = "lbf·ft"; break;
                        case UNIT.UNIT_kgcm: torqueUnit = "kgf·cm"; break;
                        case UNIT.UNIT_kgm: torqueUnit = "kgf·m"; break;
                        default: break;
                    }

                    //获取实时数据
                    if (actXET.data[i].dtype == 0xF1)
                    {
                        torque = actXET.data[i].torque / (double)actXET.torqueMultiple;
                        torqueLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Add(torque);

                        angle = actXET.data[i].angle / (double)actXET.angleMultiple;
                        angleLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Add(angle);
                        angleOld = angle;
                    }
                    else if (actXET.data[i].dtype == 0xF2)
                    {
                        torque = actXET.data[i].torseries_pk / (double)actXET.torqueMultiple;
                        torqueLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Add(0);                         //用于画曲线描点对应的02值

                        angle = actXET.data[i].angle_acc / (double)actXET.angleMultiple;
                        angleLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Add(angleOld);
                    }

                    //计算峰值扭矩和峰值角度
                    torquePeak = Math.Abs(torque) > Math.Abs(torquePeak) ? torque : torquePeak;
                    anglePeak = Math.Abs(angle) > Math.Abs(anglePeak) ? angle : anglePeak;

                    if (!(actXET.data[i].mode_pt == 1 && actXET.data[i].dtype == 0xF1))
                    {
                        //更新表格
                        int idx = dataGridView1.Rows.Add();            //实时表格的行数下标

                        if (actXET.data[i].dtype == 0xF1)
                        {
                            dataGridView1.Rows[idx].Cells[0].Value = (++lines).ToString();
                        }
                        else if (actXET.data[i].dtype == 0xF2)
                        {
                            dataGridView1.Rows[idx].Cells[0].Value = "☆" + (++lines);
                        }
                        else if (actXET.data[i].dtype == 0xF3)
                        {
                            dataGridView1.Rows[idx].Cells[0].Value = "★" + (++lines);
                        }

                        dataGridView1.Rows[idx].Cells[1].Value = actXET.opsn;
                        dataGridView1.Rows[idx].Cells[2].Value = actXET.data[i].stamp;
                        dataGridView1.Rows[idx].Cells[3].Value = actXET.wlan.addr;
                        dataGridView1.Rows[idx].Cells[4].Value = torque + " " + torqueUnit;
                        dataGridView1.Rows[idx].Cells[5].Value = angle + " °";
                        label3.Text = "扭矩峰值：" + torquePeak + torqueUnit;
                        label4.Text = "角度峰值：" + anglePeak + "°";

                        TorquedataGroups[actXET.opsn].Add(torque);
                        AngledataGroups[actXET.opsn].Add(angle);
                    }

                    //判断数据结果是否合格
                    if (IsValid(actXET.data[i], actXET, torque * actXET.torqueMultiple, angle * actXET.angleMultiple))
                    {
                        dataGroupResults[actXET.opsn] = true;
                        panel3.BackColor = Color.Green;
                    }
                    else
                    {
                        dataGroupResults[actXET.opsn] = false;
                        panel3.BackColor = Color.CadetBlue;
                    }

                    //更新作业号
                    if (actXET.data[i].dtype == 0xF2)
                    {

                        //计算残余扭矩
                        try
                        {
                            label2.Text = "残余扭矩值: " + GetResidualTorque(1, new List<double>(TorquedataGroups[actXET.opsn]), new List<double>(AngledataGroups[actXET.opsn]));
                        }
                        catch
                        {
                            Console.WriteLine("异常");
                        }

                        snbatArr[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)]++;
                        opsnTimeArr[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] = System.DateTime.Now.ToString("yyyyMMddHHmm");
                        actXET.snBat = snbatArr[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)];    //读取结果帧作业号更新
                        actXET.opsn = actXET.wlan.addr + opsnTimeArr[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] + " " + actXET.snBat.ToString().PadLeft(4, '0');

                        //新增新的扭矩角度作业
                        TorquedataGroups.Add(actXET.opsn, new List<double>());
                        AngledataGroups.Add(actXET.opsn, new List<double>());
                        dataGroupResults.Add(actXET.opsn, false);
                    }

                    //移到最后一行
                    if (dataGridView1.RowCount > 1)
                    {
                        dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.RowCount - 1;
                    }
                }
            }

            // 解除选择
            dataGridView1.ClearSelection();

            //超过5000行删除
            while (dataGridView1.Rows.Count > 5000)
            {
                dataGridView1.Rows.RemoveAt(0);
            }

            //恢复布局逻辑，并手动刷新 DataGridView
            dataGridView1.ResumeLayout();
            dataGridView1.Refresh(); // 手动刷新 DataGridView
        }

        //画曲线(底层)
        private void pictureBoxScope_draw()
        {
            //获取坐标轴上下限
            for (int i = 0; i < 5; i++)
            {
                newTorqueUpper[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] = actXET.data[i].dtype == 0xF2 ? (actXET.data[i].torseries_pk / (actXET.torqueMultiple * 10) + 1) * 10 : (actXET.data[i].torque / (actXET.torqueMultiple * 10) + 1) * 10;
                newTorqueUpper[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] = newTorqueUpper[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] > oldTorqueUpper[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] ? newTorqueUpper[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] : oldTorqueUpper[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)];
                oldTorqueUpper[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] = newTorqueUpper[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)];

                newTorqueLower[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] = actXET.data[i].dtype == 0xF2 ? (actXET.data[i].torseries_pk / (actXET.torqueMultiple * 10) - 1) * 10 : (actXET.data[i].torque / (actXET.torqueMultiple * 10) - 1) * 10;
                newTorqueLower[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] = newTorqueLower[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] < oldTorqueLower[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] ? newTorqueLower[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] : oldTorqueLower[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)];
                oldTorqueLower[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] = newTorqueLower[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)];

                newAngleUpper[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] = actXET.data[i].dtype == 0xF2 ? (actXET.data[i].angle_acc / (actXET.angleMultiple * 10) + 1) * 10 : (actXET.data[i].angle / (actXET.angleMultiple * 10) + 1) * 10;
                newAngleUpper[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] = newAngleUpper[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] > oldAngleUpper[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] ? newAngleUpper[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] : oldAngleUpper[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)];
                oldAngleUpper[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] = newAngleUpper[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)];

                newAngleLower[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] = actXET.data[i].dtype == 0xF2 ? (actXET.data[i].angle_acc / (actXET.angleMultiple * 10) - 1) * 10 : (actXET.data[i].angle / (actXET.angleMultiple * 10) - 1) * 10;
                newAngleLower[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] = newAngleLower[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] < oldAngleLower[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] ? newAngleLower[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] : oldAngleLower[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)];
                oldAngleLower[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] = newAngleLower[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)];
            }

            //根据曲线模式调整曲线上下限
            switch (ucCombox1.SelectedIndex)
            {
                case 0:
                    //x轴上下限
                    myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].LimitLowerX = 0;

                    //y轴上下限
                    myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].LimitUpperLeftY = newTorqueUpper[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)];
                    myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].LimitLowerLeftY = newTorqueLower[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)];
                    break;
                case 1:
                    //x轴上下限
                    myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].LimitLowerX = 0;

                    //y轴上下限
                    myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].LimitUpperLeftY = newAngleUpper[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)];
                    myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].LimitLowerLeftY = newAngleLower[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)];
                    break;
                case 2:
                    break;
                default:
                    break;
            }

            //横轴数量(网格)
            myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].HorizontalAxisNum = 11;

            //画x轴,y轴,网格
            //pictureBox1.BackgroundImage = myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].GetBackgroundImage();

            for (int i = 0; i < 5; i++)
            {
                if (actXET.data[i].dtype == 0xF1 || actXET.data[i].dtype == 0xF2 || actXET.data[i].dtype == 0xF3)
                {
                    if (actXET.data[i].dtype == 0xF1 && actXET.data[i].torque == 0 && actXET.data[i].angle == 0) break;

                    //获取实时数据
                    torque = actXET.data[i].dtype == 0xF1 ? actXET.data[i].torque / (double)actXET.torqueMultiple : 0;

                    if (actXET.data[i].dtype == 0xF1)
                    {
                        angle = actXET.data[i].angle / (double)actXET.angleMultiple;
                        angleOld = angle;
                    }
                    else
                    {
                        angle = angleOld;
                    }

                    ////遇到数据就更新曲线
                    //if (ucCombox1.SelectedIndex == 0)
                    //{
                    //    pictureBox1.Image = myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].GetForegroundImage(torque);
                    //}
                    //else if (ucCombox1.SelectedIndex == 1)
                    //{
                    //    pictureBox1.Image = myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].GetForegroundImage(angle);
                    //}
                    //else if (ucCombox1.SelectedIndex == 2)
                    //{
                    //    pictureBox1.Image = myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].GetForegroundImage_Two(angleLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].ToArray(), torqueLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].ToArray());
                    //}

                    //遇到数据F2/F3才更新曲线
                    if (actXET.data[i].dtype == 0xF2 || actXET.data[i].dtype == 0xF3)
                    {
                        pictureBox1.BackgroundImage = myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].GetBackgroundImage();//画x轴,y轴
                        pictureBox1.Image = null;

                        if (myPictures.Count > 0)
                        {
                            myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] = new DrawPicture(pictureBox1.Height, pictureBox1.Width, BackgroundImageType.OnlyXYAxis);

                            //横轴数量(网格)
                            myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].HorizontalAxisNum = 11;

                            if (ucCombox1.SelectedIndex == 0 && torqueLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Count > 0)
                            {
                                myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].LimitUpperLeftY = newTorqueUpper[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)];
                                myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].LimitLowerLeftY = newTorqueLower[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)];

                                //画x轴,y轴,网格
                                pictureBox1.BackgroundImage = myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].GetBackgroundImage();

                                pictureBox1.Image = myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].GetForegroundImage(torqueLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].ToArray());
                            }
                            else if (ucCombox1.SelectedIndex == 1 && angleLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Count > 0)
                            {
                                myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].LimitUpperLeftY = newAngleUpper[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)];
                                myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].LimitLowerLeftY = newAngleLower[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)];

                                //画x轴,y轴,网格
                                pictureBox1.BackgroundImage = myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].GetBackgroundImage();

                                pictureBox1.Image = myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].GetForegroundImage(angleLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].ToArray());
                            }
                            else if (ucCombox1.SelectedIndex == 2 && torqueLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Count > 0)
                            {
                                myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].LimitUpperLeftY = newTorqueUpper[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)];
                                myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].LimitLowerLeftY = newTorqueLower[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)];

                                //画x轴,y轴,网格
                                pictureBox1.BackgroundImage = myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].GetBackgroundImage();

                                //06按照dtype=F2划分成n段，实时更新最后一段
                                if (actXET.devc.type == TYPE.TQ_XH_XL01_06 - (UInt16)ADDROFFSET.TQ_XH_ADDR || actXET.devc.type == TYPE.TQ_XH_XL01_05 - (UInt16)ADDROFFSET.TQ_XH_ADDR)
                                {
                                    pictureBox1.Image = myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].GetForegroundImage_Two(angleLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].ToArray(), torqueLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].ToArray());
                                }

                                //07按照dtype=F3划分成n段，实时更新最后一段
                                if (actXET.data[i].dtype == 0xF3)
                                {
                                    string index = AngledataGroups.Keys.ElementAt(AngledataGroups.Count - 2);//当前作业号的上一个
                                    pictureBox1.Image = myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].GetForegroundImage_Two(AngledataGroups[index].ToArray(), TorquedataGroups[index].ToArray());
                                }     
                            }
                        }
                    }
                }
            }
        }

        // 鼠标点击图片,显示轴线，选中对应表格(顶层)
        private void showDataInfo(int x)
        {
            int xline;

            //表格取消选中
            for (int i = 0; i < dataGridView1.SelectedRows.Count; i++)
            {
                dataGridView1.SelectedRows[i].Selected = false;
            }

            //绘制
            Graphics g = pictureBox1.CreateGraphics();
            this.pictureBox1.Refresh();

            //画数据纵轴
            xline = torqueLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Count < myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StopIdx - myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx ? x : (x - myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx) * (myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StopIdx - myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx) / torqueLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Count + myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx + 1;
            g.DrawLine(new Pen(Color.Green, 1.0f), new Point(xline, myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].TextInfo), new Point(xline, pictureBox1.Height - myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].TextInfo));

            if (ucCombox1.SelectedIndex != 2)
            {
                if (x - myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx < 5000)
                {
                    //画详细信息分析
                    g.DrawString("时间: " + dataGridView1[2, x - myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx].Value.ToString(), new Font("Arial", 8), Brushes.CadetBlue, xline + 5, pictureBox1.Height / 2);
                    g.DrawString("设备ID: " + dataGridView1[3, x - myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx].Value.ToString(), new Font("Arial", 8), Brushes.CadetBlue, xline + 5, pictureBox1.Height / 2 + 20);
                    g.DrawString("扭矩: " + dataGridView1[4, x - myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx].Value.ToString(), new Font("Arial", 8), Brushes.CadetBlue, xline + 5, pictureBox1.Height / 2 + 40);
                    g.DrawString("角度: " + dataGridView1[5, x - myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx].Value.ToString(), new Font("Arial", 8), Brushes.CadetBlue, xline + 5, pictureBox1.Height / 2 + 60);
                    g.DrawString("扭矩峰值: " + torquePeak.ToString() + torqueUnit, new Font("Arial", 8), Brushes.CadetBlue, xline + 5, pictureBox1.Height / 2 + 80);
                    g.DrawString("角度峰值: " + anglePeak.ToString() + "°", new Font("Arial", 8), Brushes.CadetBlue, xline + 5, pictureBox1.Height / 2 + 100);
                }
                else
                {
                    //画详细信息分析
                    g.DrawString("时间: " + dataGridView1[2, x - myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx - (torqueLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Count - 5000)].Value.ToString(), new Font("Arial", 8), Brushes.CadetBlue, xline + 5, pictureBox1.Height / 2);
                    g.DrawString("设备ID: " + dataGridView1[3, x - myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx - (torqueLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Count - 5000)].Value.ToString(), new Font("Arial", 8), Brushes.CadetBlue, xline + 5, pictureBox1.Height / 2 + 20);
                    g.DrawString("扭矩: " + dataGridView1[4, x - myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx - (torqueLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Count - 5000)].Value.ToString(), new Font("Arial", 8), Brushes.CadetBlue, xline + 5, pictureBox1.Height / 2 + 40);
                    g.DrawString("角度: " + dataGridView1[5, x - myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx - (torqueLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Count - 5000)].Value.ToString(), new Font("Arial", 8), Brushes.CadetBlue, xline + 5, pictureBox1.Height / 2 + 60);
                    g.DrawString("扭矩峰值: " + torquePeak.ToString() + torqueUnit, new Font("Arial", 8), Brushes.CadetBlue, xline + 5, pictureBox1.Height / 2 + 80);
                    g.DrawString("角度峰值: " + anglePeak.ToString() + "°", new Font("Arial", 8), Brushes.CadetBlue, xline + 5, pictureBox1.Height / 2 + 100);
                }
            }
            else
            {
                if (x - myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx < 5000)
                {
                    //画详细信息分析
                    g.DrawString("时间: " + dataGridView1[2, x - myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx].Value.ToString(), new Font("Arial", 8), Brushes.CadetBlue, xline + 5, pictureBox1.Height / 2);
                    g.DrawString("设备ID: " + dataGridView1[3, x - myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx].Value.ToString(), new Font("Arial", 8), Brushes.CadetBlue, xline + 5, pictureBox1.Height / 2 + 20);
                    g.DrawString("扭矩: " + dataGridView1[4, x - myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx].Value.ToString(), new Font("Arial", 8), Brushes.CadetBlue, xline + 5, pictureBox1.Height / 2 + 40);
                    g.DrawString("角度: " + dataGridView1[5, x - myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx].Value.ToString(), new Font("Arial", 8), Brushes.CadetBlue, xline + 5, pictureBox1.Height / 2 + 60);
                    g.DrawString("扭矩峰值: " + torquePeak.ToString() + torqueUnit, new Font("Arial", 8), Brushes.CadetBlue, xline + 5, pictureBox1.Height / 2 + 80);
                    g.DrawString("角度峰值: " + anglePeak.ToString() + "°", new Font("Arial", 8), Brushes.CadetBlue, xline + 5, pictureBox1.Height / 2 + 100);
                }
                else
                {
                    //画详细信息分析
                    g.DrawString("时间: " + dataGridView1[2, x - myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx - (torqueLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Count - 5000)].Value.ToString(), new Font("Arial", 8), Brushes.CadetBlue, xline + 5, pictureBox1.Height / 2);
                    g.DrawString("设备ID: " + dataGridView1[3, x - myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx - (torqueLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Count - 5000)].Value.ToString(), new Font("Arial", 8), Brushes.CadetBlue, xline + 5, pictureBox1.Height / 2 + 20);
                    g.DrawString("扭矩: " + dataGridView1[4, x - myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx - (torqueLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Count - 5000)].Value.ToString(), new Font("Arial", 8), Brushes.CadetBlue, xline + 5, pictureBox1.Height / 2 + 40);
                    g.DrawString("角度: " + dataGridView1[5, x - myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx - (torqueLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Count - 5000)].Value.ToString(), new Font("Arial", 8), Brushes.CadetBlue, xline + 5, pictureBox1.Height / 2 + 60);
                    g.DrawString("扭矩峰值: " + torquePeak.ToString() + torqueUnit, new Font("Arial", 8), Brushes.CadetBlue, xline + 5, pictureBox1.Height / 2 + 80);
                    g.DrawString("角度峰值: " + anglePeak.ToString() + "°", new Font("Arial", 8), Brushes.CadetBlue, xline + 5, pictureBox1.Height / 2 + 100);
                }
            }


            //
            g.Dispose();

            if (x - myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx < 5000)
            {
                //选中表格
                dataGridView1.Rows[x - myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx].Selected = true;

                //索引移到表格
                dataGridView1.FirstDisplayedScrollingRowIndex = x - myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx;
            }
            else
            {
                //选中表格
                dataGridView1.Rows[x - myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx - (torqueLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Count - 5000)].Selected = true;

                //索引移到表格
                dataGridView1.FirstDisplayedScrollingRowIndex = x - myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx - (torqueLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Count - 5000);
            }
        }

        //鼠标点击图片，显示压缩曲线及对应表格
        private void showCompressDataInfo(int x)
        {
            int idx;

            //表格取消选中
            for (int i = 0; i < dataGridView1.SelectedRows.Count; i++)
            {
                dataGridView1.SelectedRows[i].Selected = false;
            }

            //绘制
            Graphics g = pictureBox1.CreateGraphics();
            this.pictureBox1.Refresh();

            //画数据纵轴
            g.DrawLine(new Pen(Color.Green, 1.0f), new Point(x, myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].TextInfo), new Point(x, pictureBox1.Height - myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].TextInfo));

            idx = (x - myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx) * torqueLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Count / (myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StopIdx - myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].StartIdx);
            if (torqueLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Count > 5000) idx = idx - (torqueLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Count - 5000);

            if (ucCombox1.SelectedIndex != 2)
            {
                if (idx >= 0)
                {
                    //画详细信息分析
                    g.DrawString("时间: " + dataGridView1[2, idx].Value.ToString(), new Font("Arial", 8), Brushes.CadetBlue, x + 5, pictureBox1.Height / 2);
                    g.DrawString("设备ID: " + dataGridView1[3, idx].Value.ToString(), new Font("Arial", 8), Brushes.CadetBlue, x + 5, pictureBox1.Height / 2 + 20);
                    g.DrawString("扭矩: " + dataGridView1[4, idx].Value.ToString(), new Font("Arial", 8), Brushes.CadetBlue, x + 5, pictureBox1.Height / 2 + 40);
                    g.DrawString("角度: " + dataGridView1[5, idx].Value.ToString(), new Font("Arial", 8), Brushes.CadetBlue, x + 5, pictureBox1.Height / 2 + 60);
                    g.DrawString("扭矩峰值: " + torquePeak.ToString() + torqueUnit, new Font("Arial", 8), Brushes.CadetBlue, x + 5, pictureBox1.Height / 2 + 80);
                    g.DrawString("角度峰值: " + anglePeak.ToString() + "°", new Font("Arial", 8), Brushes.CadetBlue, x + 5, pictureBox1.Height / 2 + 100);
                }
                else
                {
                    //画详细信息分析
                    g.DrawString("设备ID: " + MyDevice.protocol.addr, new Font("Arial", 8), Brushes.CadetBlue, x + 5, pictureBox1.Height / 2 + 20);
                    g.DrawString("扭矩: " + torqueLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)][torqueLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Count - 5000 + idx].ToString(), new Font("Arial", 8), Brushes.CadetBlue, x + 5, pictureBox1.Height / 2 + 40);
                    g.DrawString("角度: " + angleLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)][angleLists[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Count - 5000 + idx].ToString(), new Font("Arial", 8), Brushes.CadetBlue, x + 5, pictureBox1.Height / 2 + 60);
                    g.DrawString("扭矩峰值: " + torquePeak.ToString() + torqueUnit, new Font("Arial", 8), Brushes.CadetBlue, x + 5, pictureBox1.Height / 2 + 80);
                    g.DrawString("角度峰值: " + anglePeak.ToString() + "°", new Font("Arial", 8), Brushes.CadetBlue, x + 5, pictureBox1.Height / 2 + 100);
                }
            }
            else
            {
                if (idx >= 0)
                {
                    //画详细信息分析
                    g.DrawString("时间: " + dataGridView1[2, idx].Value.ToString(), new Font("Arial", 8), Brushes.CadetBlue, x + 5, pictureBox1.Height / 2);
                    g.DrawString("设备ID: " + dataGridView1[3, idx].Value.ToString(), new Font("Arial", 8), Brushes.CadetBlue, x + 5, pictureBox1.Height / 2 + 20);
                    g.DrawString("扭矩: " + dataGridView1[4, idx].Value.ToString(), new Font("Arial", 8), Brushes.CadetBlue, x + 5, pictureBox1.Height / 2 + 40);
                    g.DrawString("角度: " + dataGridView1[5, idx].Value.ToString(), new Font("Arial", 8), Brushes.CadetBlue, x + 5, pictureBox1.Height / 2 + 60);
                    g.DrawString("扭矩峰值: " + torquePeak.ToString() + torqueUnit, new Font("Arial", 8), Brushes.CadetBlue, x + 5, pictureBox1.Height / 2 + 80);
                    g.DrawString("角度峰值: " + anglePeak.ToString() + "°", new Font("Arial", 8), Brushes.CadetBlue, x + 5, pictureBox1.Height / 2 + 100);
                }
            }


            //
            g.Dispose();

            if (idx >= 0)
            {
                //选中表格
                dataGridView1.Rows[idx].Selected = true;

                //索引移到表格
                dataGridView1.FirstDisplayedScrollingRowIndex = idx;
            }
        }

        #endregion

        #region 表格数据存储为csv

        // 数据表保存为csv文件
        public bool saveActualDataToExcel(string mePath)
        {
            //空
            if (mePath == null)
            {
                return false;
            }

            //写入
            try
            {
                //excel的每一行
                var lines = new List<string>();

                lines.Add("序号,作业号,时间,设备站点,实时扭矩,实时角度");
                DATA dataInfoToExcel = new DATA();

                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    dataInfoToExcel = new DATA();

                    //加=\，使csv文件用excel打开时能正常显示数据
                    lines.Add($"{dataGridView1.Rows[i].Cells[0].Value},=\"{dataGridView1.Rows[i].Cells[1].Value}\",=\"{dataGridView1.Rows[i].Cells[2].Value}\",=\"{dataGridView1.Rows[i].Cells[3].Value}\"," +
                              $"{dataGridView1.Rows[i].Cells[4].Value},=\"{dataGridView1.Rows[i].Cells[5].Value}\",");
                }

                File.WriteAllLines(mePath, lines, System.Text.Encoding.Default);
                System.IO.File.SetAttributes(mePath, FileAttributes.ReadOnly);
                return true;
            }
            catch
            {
                return false;
            }
        }

        //存储为csv，指定格式，用于数据分析
        public bool saveActualDataToCsv2(string filePath)
        {
            DataTable dataTable = new DataTable();
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
            List<DSData> filteredList = JDBC.GetDataByWoNumAndSeIdAndTimeAndVid("", "", DateTime.Now.Date, MyDevice.Vin);
            string torUnit = "";
            if (filteredList != null && filteredList.Count != 0)
            {
                Int32 filteredDataCnt = filteredList.Count;

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
                }

                // 确保文件路径目录存在
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }
            else
            {
                return false;
            }

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

                    return true;
                }
            }
            catch (IOException)
            {
                // 文件被占用
                MessageBox.Show("csv文件被打开，请先关闭");
            }
            return false;
        }

        #endregion

        #region 控件大小随窗体变化

        //控件大小随窗体变化
        private Boolean firstStart = true;
        private float X;//定义当前窗体的宽度
        private float Y;//定义当前窗体的高度

        /// <summary>
        /// 初始化所有控件尺寸及字体大小
        /// </summary>
        public void InitForm()
        {
            X = this.Width;//赋值初始窗体宽度
            Y = this.Height;//赋值初始窗体高度
            setTag(this);
        }

        /// <summary>
        /// 窗体尺寸变化时触发
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (firstStart) { firstStart = false; return; }     //OnResize()在Form_Load()之前触发,退出

            try
            {
                float newX = this.Width / X;//获取当前宽度与初始宽度的比例
                float newY = this.Height / Y;//获取当前高度与初始高度的比例
                setControls(newX, newY, this);
            }
            catch (Exception ex)
            {
                //捕获异常

            }
        }

        /// <summary>
        /// 获取控件的尺寸/位置/字体
        /// </summary>
        /// <param name="cons"></param>
        private void setTag(Control cons)
        {
            //遍历窗体中的控件
            foreach (Control con in cons.Controls)
            {
                con.Tag = con.Width + ":" + con.Height + ":" + con.Left + ":" + con.Top + ":" + con.Font.Size;

                //如果是容器控件，则递归继续纪录
                if (con.Controls.Count > 0)
                {
                    setTag(con);
                }
            }
        }

        /// <summary>
        /// 窗体尺寸变化时，按比例调整控件尺寸及字体
        /// </summary>
        /// <param name="newX"></param>
        /// <param name="newY"></param>
        /// <param name="cons"></param>
        public void setControls(float newX, float newY, Control cons)
        {
            try
            {
                //遍历窗体中的控件，重新设置控件的值
                foreach (Control con in cons.Controls)
                {
                    if (Convert.ToString(con.Tag) == string.Empty) continue;
                    string[] mytag = con.Tag.ToString().Split(new char[] { ':' });//获取控件的Tag属性值，并分割后存储字符串数组
                    if (mytag.Length < 5) return;

                    float a = Convert.ToSingle(mytag[0]) * newX;//根据窗体缩放比例确定控件的值，宽度//89*300
                    con.Width = (int)(a);//宽度

                    a = Convert.ToSingle(mytag[1]) * newY;//根据窗体缩放比例确定控件的值，高度//12*300
                    con.Height = (int)(a);//高度

                    a = Convert.ToSingle(mytag[2]) * newX;//根据窗体缩放比例确定控件的值，左边距离//
                    con.Left = (int)(a);//左边距离

                    a = Convert.ToSingle(mytag[3]) * newY;//根据窗体缩放比例确定控件的值，上边缘距离
                    con.Top = (int)(a);//上边缘距离

                    if (con is Panel == false && newY != 0)//Panel容器控件不改变字体--Panel字体变后，若panel调用了UserControl控件，则UserControl及其上控件的尺寸会出现不可控变化;newY=0时，字体设置会报错
                    {
                        Single currentSize = Convert.ToSingle(mytag[4]) * newY;//根据窗体缩放比例确定控件的值，字体大小
                        con.Font = new Font(con.Font.Name, currentSize, con.Font.Style, con.Font.Unit);//字体大小
                    }

                    if (con.Controls.Count > 0)
                    {
                        setControls(newX, newY, con);
                    }
                    //Remarks：
                    //控件当前宽度：控件初始宽度=窗体当前宽度：窗体初始宽度
                    //控件当前宽度=控件初始宽度*(窗体当前宽度/窗体初始宽度)
                }
            }
            catch (Exception ex)
            {
                //捕获异常

            }
        }

        #endregion

        //UI更新事件
        private void updateUI(object sender, UpdateUIEventArgs e)
        {
            Action action = () =>
            {
                Command currentCommand = e.Command;
                switch (currentCommand.TaskState)
                {
                    case TASKS.REG_BLOCK2_DAT:
                        if (actXET.devc.type == TYPE.TQ_XH_XL01_06 - (UInt16)ADDROFFSET.TQ_XH_ADDR || actXET.devc.type == TYPE.TQ_XH_XL01_05 - (UInt16)ADDROFFSET.TQ_XH_ADDR)
                        {
                            updateDataTable_XH06();
                            pictureBoxScope_draw();
                        }
                        else
                        {
                            updateDataTable_XH07();
                            pictureBoxScope_draw();
                        }
                        break;
                    default:
                        break;
                }

                //收到指令切换设备
                if (myPictures.Count > 0)
                {
                    //MyDevice.myTaskManager.SelectedDev = Convert.ToByte(ucCombox2.TextValue);
                    //MyDevice.protocol.addr = Convert.ToByte(ucCombox2.TextValue);
                    MyDevice.myTaskManager.SelectedDev = MyDevice.AddrList[(MyDevice.AddrList.IndexOf(MyDevice.protocol.addr) + 1) % MyDevice.AddrList.Count];
                    MyDevice.protocol.addr = MyDevice.AddrList[(MyDevice.AddrList.IndexOf(MyDevice.protocol.addr) + 1) % MyDevice.AddrList.Count];

                    //TCP模式要切换端口
                    if (MyDevice.protocol.type == COMP.TCP)
                    {
                        //防止字典查询溢出
                        if (MyDevice.addr_ip.ContainsKey(MyDevice.protocol.addr.ToString()) &&
                            MyDevice.clientConnectionItems.ContainsKey(MyDevice.addr_ip[MyDevice.protocol.addr.ToString()])
                            )
                        {
                            MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
                        }
                        else
                        {
                            return;
                        }
                    }

                    //更新设备
                    actXET = MyDevice.actDev;
                    actXET.torqueMultiple = (int)Math.Pow(10, actXET.devc.torque_decimal);
                    actXET.angleMultiple = (int)Math.Pow(10, actXET.para.angle_decimal);

                    //更新作业号
                    if (snbatArr != null)
                    {
                        actXET.snBat = snbatArr[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)];
                        actXET.opsn = actXET.wlan.addr + opsnTimeArr[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)] + " " + actXET.snBat.ToString().PadLeft(4, '0');

                        if (!TorquedataGroups.ContainsKey(actXET.opsn))
                        {
                            TorquedataGroups.Add(actXET.opsn, new List<double>());
                            AngledataGroups.Add(actXET.opsn, new List<double>());
                            dataGroupResults.Add(actXET.opsn, false);
                        }
                    }

                    myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Width = pictureBox1.Width;
                    myPictures[MyDevice.AddrList.IndexOf(MyDevice.protocol.addr)].Height = pictureBox1.Height;
                    ucCombox1_SelectedChangedEvent(null, null);
                }
            };
            Invoke(action);
        }

        //计算再拧紧扭矩值
        private double GetResidualTorque(int mode, List<double> torqueList, List<double> angleList)
        {
            List<double> agvTorqueList = new List<double>();
            List<double> agvAngleList = new List<double>();
            List<double> slopeList = new List<double>();      //斜率集合
            double avgTorque;
            double avgAngle;
            double slope;
            if (mode == 1)
            {
                //先求8个一组的平均值，再求斜率，最后滤波
                if (torqueList.Count > 10)
                {
                    //求平均值
                    for (int i = 0; i < torqueList.Count - 8 + 1; i++)
                    {
                        // 使用 LINQ 的 Skip 和 Take 方法获取List中第 n 到第 m 个元素，并计算它们的平均值
                        // List.Skip(n).Take(m - n + 1).Average();
                        avgTorque = torqueList.Skip(i).Take(8).Average();
                        avgAngle = angleList.Skip(i).Take(8).Average();

                        agvTorqueList.Add(avgTorque);
                        agvAngleList.Add(avgAngle);
                    }
                    //求斜率 △t / △a
                    for (int i = 1; i < agvTorqueList.Count; i++)
                    {
                        slope = (agvTorqueList[i] - agvTorqueList[i - 1]) / (agvAngleList[i] - agvAngleList[i - 1]);
                        slopeList.Add(slope);
                    }
                    //滤波（正 -> 负 中的负值对应的avgTorque）
                    for (int i = 1; i < slopeList.Count; i++)
                    {
                        if (slopeList[i] < 0 && slopeList[i - 1] > 0)
                        {
                            //斜率集合的数量 = 平均扭矩集合 - 1
                            return agvTorqueList[i + 1];
                        }
                    }
                }
            }

            return 0;
        }

        //判断过程数据是否合格
        private bool IsValid(DATA data, XET xet, double torque, double angle)
        {
            bool isDataValid = false;
            byte torUnit = 0;

            //F3没有单位，故继承F2/F1的单位，直接读actXET.data[i].torque_unit不可取
            switch (torqueUnit)
            {
                case "N·m":
                    torUnit = 0;
                    break;
                case "lbf·in":
                    torUnit = 1;
                    break;
                case "lbf·ft":
                    torUnit = 2;
                    break;
                case "kgf·cm":
                    torUnit = 3;
                    break;
                case "kgf·m":
                    torUnit = 4;
                    break;
                default:
                    break;
            }

            //XH07-F3 结果判断合格
            if ((xet.devc.type == TYPE.TQ_XH_XL01_07 - (UInt16)ADDROFFSET.TQ_XH_ADDR
                || xet.devc.type == TYPE.TQ_XH_XL01_08 - (UInt16)ADDROFFSET.TQ_XH_ADDR
                || xet.devc.type == TYPE.TQ_XH_XL01_09 - (UInt16)ADDROFFSET.TQ_XH_ADDR)
                && data.dtype == 0xF3)
            {
                //超量程结束
                if (torque > actXET.devc.torque_over[torUnit])
                {
                    isDataValid = false;
                    return isDataValid;
                }

                //单位更新
                switch (data.mode_ax)
                {
                    //EN模式
                    case 0:
                        break;
                    //EA模式
                    case 1:
                        break;
                    //SN模式
                    case 2:
                        //峰值扭矩 >= 预设扭矩 = 合格
                        if (torque >= data.alarm[0])
                        {
                            isDataValid = true;
                        }
                        else
                        {
                            isDataValid = false;
                        }
                        break;
                    //SA模式
                    case 3:
                        //峰值扭矩 >= 预设扭矩 && 峰值角度 >= 预设角度 = 合格
                        if (torque >= data.alarm[0] && angle >= data.alarm[1])
                        {
                            isDataValid = true;
                        }
                        else
                        {
                            isDataValid = false;
                        }
                        break;
                    //MN模式
                    case 4:
                        // 扭矩下限 <= 峰值扭矩 <= 扭矩上限  = 合格
                        if (data.alarm[0] <= torque && torque <= data.alarm[1])
                        {
                            isDataValid = true;
                        }
                        else
                        {
                            isDataValid = false;
                        }
                        break;
                    //MA模式
                    case 5:
                        //峰值扭矩 >= 预设扭矩 && 角度下限 <= 峰值角度 <= 角度上限 = 合格
                        if (torque >= data.alarm[0]
                            && data.alarm[1] <= angle && angle <= data.alarm[2])
                        {
                            isDataValid = true;
                        }
                        else
                        {
                            isDataValid = false;
                        }
                        break;
                    //AZ模式
                    case 6:
                        //峰值扭矩 >= 预设扭矩
                        if (torque >= data.alarm[2])
                        {
                            isDataValid = true;
                        }
                        else
                        {
                            isDataValid = false;
                        }
                        break;
                    default:
                        break;
                }
            }
            //XH06—F2结果判断合格
            //存在局限性，由于F2数据包中不含modeAx，无法判断当前设备是那种模式，只能通过读para中的modeAx
            //客户在数据页面手动按键多种模式切换，modeAx的具体就不确定以哪种作为标准
            else if ((xet.devc.type == TYPE.TQ_XH_XL01_06 - (UInt16)ADDROFFSET.TQ_XH_ADDR || xet.devc.type == TYPE.TQ_XH_XL01_05 - (UInt16)ADDROFFSET.TQ_XH_ADDR) && data.dtype == 0xF2)
            {
                //超量程结束
                if (torque > actXET.devc.torque_over[torUnit])
                {
                    isDataValid = false;
                    return isDataValid;
                }

                //单位更新
                switch (xet.para.mode_ax)
                {
                    //EN模式
                    case 0:
                        break;
                    //EA模式
                    case 1:
                        break;
                    //SN模式
                    case 2:
                        //峰值扭矩 >= 预设扭矩 = 合格
                        if (torque >= xet.alam.SN_target[xet.para.mode_mx, (int)data.torque_unit])
                        {
                            isDataValid = true;
                        }
                        else
                        {
                            isDataValid = false;
                        }
                        break;
                    //SA模式
                    case 3:
                        //峰值扭矩 >= 预设扭矩 && 峰值角度 >= 预设角度 = 合格
                        if (torque >= xet.alam.SA_pre[xet.para.mode_mx, (int)data.torque_unit] && angle >= xet.alam.SA_ang[xet.para.mode_mx])
                        {
                            isDataValid = true;
                        }
                        else
                        {
                            isDataValid = false;
                        }
                        break;
                    //MN模式
                    case 4:
                        // 扭矩下限 <= 峰值扭矩 <= 扭矩上限  = 合格
                        if (xet.alam.MN_low[xet.para.mode_mx, (int)data.torque_unit] <= torque && torque <= xet.alam.MN_high[xet.para.mode_mx, (int)data.torque_unit])
                        {
                            isDataValid = true;
                        }
                        else
                        {
                            isDataValid = false;
                        }
                        break;
                    //MA模式
                    case 5:
                        //峰值扭矩 >= 预设扭矩 && 角度下限 <= 峰值角度 <= 角度上限 = 合格
                        if (torque >= xet.alam.MA_pre[xet.para.mode_mx, (int)data.torque_unit]
                            && xet.alam.MA_low[xet.para.mode_mx] <= angle && angle <= xet.alam.MA_high[xet.para.mode_mx])
                        {
                            isDataValid = true;
                        }
                        else
                        {
                            isDataValid = false;
                        }
                        break;
                    //AZ模式
                    case 6:
                        break;
                    default:
                        break;
                }
            }
            return isDataValid;
        }
    }
}
