using DBHelper;
using Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

//Ricardo 20240522

namespace Base.UI.MenuHomework
{
    public partial class MenuTicketWorkForm : Form
    {
        private int radius = 60;        //圆的半径
        private int pointX = 0;         //点位X坐标
        private int pointY = 0;         //点位Y坐标
        private int pointNum = 0;       //点位号码，按顺序递增
        private int selectPointNum = -1;//选中的点位号码

        private Font font = new Font("微软雅黑", 60, FontStyle.Bold, GraphicsUnit.Point);               //绘图点位字体
        private Brush brush = new SolidBrush(Color.White);                                              //绘图点位颜色
        private DSTicketInfo meTicketInfo = new DSTicketInfo();                                         //工单处理界面新建或选择的工单
        private DSTicketPoints passPoint = new DSTicketPoints();                                        //合格的点位
        private List<DSTicketPoints> ReadticketPoints = new List<DSTicketPoints>();                     //从数据库读取的点位集合
        private List<DSRelationsPointWrench> ReadPointWrenchList = new List<DSRelationsPointWrench>();  //从数据库读取扳手点位关系集合
        private List<Tuple<Color, Point, int>> points = new List<Tuple<Color, Point, int>>();           //点位信息集合（颜色，坐标，序号）
        private bool isAllPointPassShow = false;                                                        //全部拧紧弹窗
        private bool isFirstLoad = true;                                                                //是否第一次加载
        private bool isBegin = true;                                                                    //是否从头开始拧（针对扫码的已完成工单）

        private XET actXET;                   //操作的设备

        private double torque = 0;            //实时扭矩
        private double angle = 0;             //实时角度
        private double torquePeak = 0;        //峰值扭矩
        private double anglePeak = 0;         //峰值角度
        private byte torUnit = 0;             //扭矩单位
        private bool isDataValid = false;     //工单拧紧结果是否合格

        private bool isScanCode = false;      //是否扫码
        private int ticketNumLen = 4;         //工单号限定长度
        private int sequenceLen = 6;          //序列号限定长度
        private int ticketEncodeLen = 10;     //工单编码限定长度——工单编码 = 工单号 + 序列号
        private double angleResit = 0;           //工单绑定的复拧角度（仅扭矩优先模式触发）
        private DSProductInfo meProductInfo = new DSProductInfo();
        private List<DSProductResults> meProductResults = new List<DSProductResults>();

        public DSTicketInfo MeTicketInfo { get => meTicketInfo; set => meTicketInfo = value; }

        private readonly float x;       //定义当前窗体的宽度
        private readonly float y;       //定义当前窗体的高度

        public MenuTicketWorkForm()
        {
            InitializeComponent();
            x = this.ClientRectangle.Width;
            y = this.ClientRectangle.Height;
            setTag(this);
        }

        private void MenuTicketWorkForm_Load(object sender, System.EventArgs e)
        {
            tb_pointNum.Enabled = false;
            tb_wrenchAddr.Enabled = false;
            tb_screwName.Enabled = false;
            tb_screwSpecs.Enabled = false;
            tb_Unit.Enabled = false;
            tb_screwPt.Enabled = false;
            tb_screwMx.Enabled = false;
            tb_screwAx.Enabled = false;
            tb_alarm1.Enabled = false;
            tb_alarm2.Enabled = false;
            tb_alarm3.Enabled = false;

            label1.Text = "工单号：" + meTicketInfo.WoNum;//更新工单号
            angleResit = meTicketInfo.AngleResist;
            isAllPointPassShow = false;
            isBegin = true;

            pictureBox1.BackgroundImageLayout = ImageLayout.Stretch;//使图片充满整个控件
            try
            {
                if(meTicketInfo.ImagePath != null) pictureBox1.BackgroundImage = Image.FromFile(meTicketInfo.ImagePath);
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Error: " + ex.Message);
            }

            //清除指令序列（防止上次工单指令未结束，又打开新工单新指令，产生多次蜂鸣）
            MyDevice.myTaskManager.ClearCommand();
        }

        //页面稳定后读取历史点位（加载时读取不稳定）
        private void MenuCreateTicketForm_Shown(object sender, EventArgs e)
        {
            textBox1.Focus();//聚焦便于接收扫码枪获取的文本

            if (meTicketInfo.WoNum == null && meProductInfo.WorkNum == null) return;//工单号为空，相当于未导入任何工单，不触发事件

            //手动导入——该操作下只输入工单，没有序列号，无法查询历史纪录
            if (!isScanCode)
            {
                //更新数据库数据存储信息
                MyDevice.DataType = meTicketInfo.WoNum == null ? "ActualData" : "TicketData";
                MyDevice.WorkId = meTicketInfo.WoNum == null ? 0 : meTicketInfo.WorkId;
                MyDevice.WorkNum = meTicketInfo.WoNum == null ? "" : meTicketInfo.WoNum;
                MyDevice.SequenceId = "";
                MyDevice.Vin = DateTime.Now.ToString("yyyyMMddHHmm");
                //添加数据库数据纪录汇总表
                if (JDBC.GetDataSummaryByWorkNum(MyDevice.WorkNum).Count == 0)
                {
                    JDBC.AddDataSummary(new DSDataSummary()
                    {
                        DataId = 1,
                        DataType = MyDevice.DataType,
                        CreateTime = DateTime.Now.ToString(),
                        WorkId = MyDevice.WorkId,
                        WorkNum = MyDevice.WorkNum,
                    });
                }

                //导入不显示序列号
                label4.Visible = false;

                //处理手动导入
                HandleManualImport();
            }
            //扫码打开工单——根据扫码输入的序列号查询历史纪录
            else
            {
                //更新数据库数据存储信息
                MyDevice.DataType = meProductInfo.WorkNum == null ? "ActualData" : "TicketData";
                MyDevice.WorkId = meProductInfo.WorkNum == null ? 0 : meProductInfo.WorkId;
                MyDevice.WorkNum = meProductInfo.WorkNum == null ? "" : meProductInfo.WorkNum;
                MyDevice.SequenceId = meProductInfo.WorkNum == null ? "" : meProductInfo.SequenceId;
                MyDevice.Vin = DateTime.Now.ToString("yyyyMMddHHmm");
                //添加数据库数据纪录汇总表
                if (JDBC.GetDataSummaryByWorkNum(MyDevice.WorkNum).Count == 0)
                {
                    JDBC.AddDataSummary(new DSDataSummary()
                    {
                        DataId = 1,
                        DataType = MyDevice.DataType,
                        CreateTime = DateTime.Now.ToString(),
                        WorkId = MyDevice.WorkId,
                        WorkNum = MyDevice.WorkNum,
                    });
                }

                //显示序列号
                label4.Visible = true;
                label4.Text = "序列号：" + meProductInfo.SequenceId;

                //处理扫码导入
                HandleScanCodeImport();
            }

            actXET = MyDevice.actDev;
            MyDevice.myTaskManager.Mode = AutoMode.UserAndTicketWork;

            //第一次加载（避免手动导入工单重复加入更新事件）
            if (isFirstLoad)
            {
                //页面更新事件
                MyDevice.myTaskManager.UpdateUI += updateUI;
                isFirstLoad = false;
            }

            if (tb_wrenchAddr.Text == "")
            {
                MyDevice.protocol.addr = 1;
            }
            else
            {
                MyDevice.protocol.addr = Convert.ToByte(tb_wrenchAddr.Text);
            }

            //从第一个非pass点位开始拧
            if (isBegin)
            {
                changePoint(MyDevice.protocol.addr);
            }
            else
            {
                MyDevice.myTaskManager.UpdateUI -= updateUI;
            }


            //初始化加入复拧
            if (MyDevice.devSum > 0)
            {
                for (int i = 0; i < MyDevice.AddrList.Count; i++)
                {
                    if ((MyDevice.protocol.type == COMP.XF && MyDevice.mXF[MyDevice.AddrList[i]].sTATE == STATE.WORKING) ||
                        (MyDevice.protocol.type == COMP.TCP && MyDevice.mTCP[MyDevice.AddrList[i]].sTATE == STATE.WORKING) ||
                        (MyDevice.protocol.type == COMP.RS485 && MyDevice.mRS[MyDevice.AddrList[i]].sTATE == STATE.WORKING) ||
                        (MyDevice.protocol.type == COMP.UART && MyDevice.mBUS[MyDevice.AddrList[i]].sTATE == STATE.WORKING)
                    )
                    {
                        //仅07，09系列有复拧
                        if (MyDevice.mBUS[MyDevice.AddrList[i]].devc.type == TYPE.TQ_XH_XL01_07 - (UInt16)ADDROFFSET.TQ_XH_ADDR ||
                            MyDevice.mBUS[MyDevice.AddrList[i]].devc.type == TYPE.TQ_XH_XL01_09 - (UInt16)ADDROFFSET.TQ_XH_ADDR
                            )
                        {
                            MyDevice.myTaskManager.AddUserCommand(MyDevice.AddrList[i], ProtocolFunc.Protocol_Sequence_SendCOM, TASKS.REG_BLOCK3_PARA, this.Name);
                        }
                    }
                }
            }
        }

        //页面关闭
        private void MenuTicketWorkForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //解按键锁,防止被锁住
            if (MyDevice.devSum > 0)
            {
                for (int i = 0; i < MyDevice.AddrList.Count; i++)
                {
                    MyDevice.keyLock = (byte)KEYLOCK.KEY_UNLOCK;
                    MyDevice.myTaskManager.AddUserCommand(MyDevice.AddrList[i], ProtocolFunc.Protocol_Write_SendCOM, TASKS.WRITE_KEYLOCK, MyDevice.keyLock, this.Name);
                }
            }

            MyDevice.myTaskManager.Mode = AutoMode.UserOnly;
            //界面更新事件
            MyDevice.myTaskManager.UpdateUI -= updateUI;
            isFirstLoad = true;

            //重置数据库数据存储信息（非工单）
            MyDevice.DataType = "ActualData";
            MyDevice.WorkId   = 0;
            MyDevice.WorkNum  = "";
            MyDevice.SequenceId = "";
            MyDevice.PointNum = "";
            MyDevice.DataResult = "NG";
            MyDevice.Vin = "";

        }

        //左键选点位
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            MouseEventArgs mouseEventArgs = e as MouseEventArgs;
            Point clickPoint = mouseEventArgs.Location;         //鼠标点击的点位置

            //鼠标点击点位有效区域
            if (ReadticketPoints.Count > 0)
            {
                foreach (var item in ReadticketPoints)
                {
                    pointX = int.Parse(item.PointPosition.Split(',')[0]);
                    pointY = int.Parse(item.PointPosition.Split(',')[1]);
                    if (IsPointInsideCircle(clickPoint, new Point(pointX, pointY), radius))
                    {
                        //更新左边栏信息
                        updatePointInfo(JDBC.GetScrewAlarmByScrewId(item.ScrewsId), item.PointNumber);
                        changePoint(Convert.ToByte(tb_wrenchAddr.Text));
                    }
                }
            }
        }

        //导入工单
        private void btn_Import_Click(object sender, EventArgs e)
        {
            isScanCode = false;//手动导入而非扫码

            if (JDBC.GetAllTickets().Count < 1)
            {
                MessageBox.Show("无有效工单，请先创建工单", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MyDevice.myTaskManager.Pause();

            //导入工单弹窗
            ImportTicketForm importTicketForm = new ImportTicketForm();
            importTicketForm.Owner = this;
            importTicketForm.StartPosition = FormStartPosition.CenterParent;
            importTicketForm.ShowDialog();
            meTicketInfo = importTicketForm.MeTicketInfo;

            if (JDBC.GetPointsByWorkId(meTicketInfo.WorkId).Count == 0)
            {
                MessageBox.Show("该工单无有效点位，无法使用工单", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                MyDevice.myTaskManager.Resume();//恢复线程
                return;
            }
            MenuTicketWorkForm_Load(null, null);
            MenuCreateTicketForm_Shown(null, null);

            //恢复自动机
            MyDevice.myTaskManager.Resume();
        }

        //扫码枪文本框触发打开工单
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 检查是否按下了回车键
            if (e.KeyChar == (char)Keys.Enter)
            {
                MyDevice.myTaskManager.Pause();//暂停线程，不做此操作将造成由于打开新工单时关闭旧页面会导致线程调用UI为空报错

                // 根据扫码枪内容打开对应工单
                MenuTicketWorkForm menuTicketWorkForm = new MenuTicketWorkForm();

                //针对不含空格的工单模板
                if (!textBox1.Text.Contains(" "))
                {
                    //识别到输入的工单号在库中
                    if (JDBC.GetTicketByWoNum(textBox1.Text).Count > 0)
                    {
                        menuTicketWorkForm.MeTicketInfo = JDBC.GetTicketByWoNum(textBox1.Text)[0];
                        if (menuTicketWorkForm.MeTicketInfo != null)
                        {
                            if (JDBC.GetPointsByWorkId(menuTicketWorkForm.MeTicketInfo.WorkId).Count == 0)
                            {
                                MessageBox.Show("无有效点位，无法使用工单", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                MyDevice.myTaskManager.Resume();//恢复线程
                                textBox1.Clear();
                                return;
                            }

                            menuTicketWorkForm.MdiParent = this.MdiParent;
                            menuTicketWorkForm.Show();
                            menuTicketWorkForm.WindowState = FormWindowState.Maximized;
                            this.Close();
                            MyDevice.myTaskManager.Resume();//恢复线程
                        }
                    }
                }
                //针对含空格的工单系列
                else
                {
                    string targetWorknum;//目标工单号
                    string targetSequenceId;//目标产品序列号
                    uint targetWorkId;//目标工单对应数据库的主键id

                    targetWorknum = textBox1.Text.Split(' ')[0];
                    targetSequenceId = textBox1.Text.Split(' ')[1];

                    //特殊情况处理，"1 ",出现工单号不为空，序列号为空的条码
                    if (targetSequenceId == "")
                    {
                        MessageBox.Show("扫码内容格式不合格", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        textBox1.Clear();
                        return;
                    }

                    if (JDBC.GetTicketByWoNum(targetWorknum).Count != 0)
                    {
                        menuTicketWorkForm.MeTicketInfo = JDBC.GetTicketByWoNum(targetWorknum)[0];
                        targetWorkId = menuTicketWorkForm.MeTicketInfo.WorkId;
                        //如果所建工单无有效位点，则不能创建工单
                        if (JDBC.GetPointsByWorkId(targetWorkId).Count == 0)
                        {
                            MessageBox.Show("无有效点位，无法使用工单" + targetWorknum, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            textBox1.Clear();
                            return;
                        }

                    }
                    else
                    {
                        //针对扫码时，以前的工单模板被删除的情况
                        if (JDBC.GetProductByWorkNumAndSequenceId(targetWorknum, targetSequenceId).Count == 0)
                        {
                            MyDevice.myTaskManager.Resume();//恢复线程
                            textBox1.Clear();
                            return;
                        }

                        //工单被多次反复删除再创建，调用最近的一个
                        targetWorkId = JDBC.GetProductByWorkNumAndSequenceId(targetWorknum, targetSequenceId).Last().WorkId;
                    }

                    //根据序号产生新的产品表
                    DSProductInfo productInfo = new DSProductInfo()
                    {
                        ProductId = 0,
                        WorkId = targetWorkId,//myTicketInfo.WoId默认是0，需要手动获取最新值
                        WorkNum = targetWorknum,
                        SequenceId = targetSequenceId,
                        ImagePath = JDBC.GetProductsByWorkId(targetWorkId).First().ImagePath,
                        AngleResist = JDBC.GetProductsByWorkId(targetWorkId).First().AngleResist,
                    };

                    List<DSProductInfo> allProduct = JDBC.GetAllProducts();
                    //是否是旧序列号
                    foreach (var item in allProduct)
                    {
                        if (item.WorkId == productInfo.WorkId && item.WorkNum == productInfo.WorkNum && item.SequenceId == productInfo.SequenceId)
                        {
                            if (MeTicketInfo != null)
                            {
                                menuTicketWorkForm.isScanCode = true;
                                menuTicketWorkForm.meProductInfo = productInfo;
                                menuTicketWorkForm.meTicketInfo.ImagePath = productInfo.ImagePath;

                                menuTicketWorkForm.MdiParent = this.MdiParent;
                                menuTicketWorkForm.Show();
                                menuTicketWorkForm.WindowState = FormWindowState.Maximized;
                                this.Close();
                                MyDevice.myTaskManager.Resume(); //恢复线程
                            }
                            textBox1.Clear();
                            return;
                        }
                    }

                    JDBC.AddProduct(productInfo);

                    //产品衍生对应的结果表（工单有几个点位就生成几条结果纪录）
                    ReadticketPoints = JDBC.GetPointsByWorkId(productInfo.WorkId);//读取数据库记录
                    foreach (var item in ReadticketPoints)
                    {
                        //螺丝表
                        DSTicketScrews screwAlarm = JDBC.GetScrewAlarmByScrewId(item.ScrewsId);
                        //螺丝扳手关系表
                        List<DSRelationsPointWrench> relationsPointWrench = JDBC.GetWrenchesByPointId(item.PointId);

                        //工单详细信息
                        DSProductResults productResult = new DSProductResults
                        {
                            ResultId = 0,//需要将对应工单设置为自动递增，否则报错
                            WorkId = productInfo.WorkId,
                            WorkNum = productInfo.WorkNum,
                            SequenceId = productInfo.SequenceId,
                            Addr = relationsPointWrench[0].Addr,
                            PointId = item.PointId,
                            PointNumber = item.PointNumber,
                            PointPosition = item.PointPosition,
                            ScrewId = item.ScrewsId,
                            ScrewNum = item.ScrewNum,
                            ScrewSeq = item.ScrewSeq,
                            Name = screwAlarm.Name,
                            Specification = screwAlarm.Specification,
                            Torque_unit = screwAlarm.Torque_unit,
                            ModePt = screwAlarm.ModePt,
                            ModeMx = screwAlarm.ModeMx,
                            ModeAx = screwAlarm.ModeAx,
                            Alarm0 = screwAlarm.Alarm0,
                            Alarm1 = screwAlarm.Alarm1,
                            Alarm2 = screwAlarm.Alarm2,
                        };
                        JDBC.AddProductResult(productResult);
                    }

                    if (MeTicketInfo != null)
                    {
                        menuTicketWorkForm.isScanCode = true;
                        menuTicketWorkForm.meProductInfo = productInfo;
                        menuTicketWorkForm.meTicketInfo.ImagePath = productInfo.ImagePath;

                        menuTicketWorkForm.MdiParent = this.MdiParent;
                        menuTicketWorkForm.Show();
                        menuTicketWorkForm.WindowState = FormWindowState.Maximized;
                        this.Close();
                        MyDevice.myTaskManager.Resume();//恢复线程
                    }
                }

                textBox1.Clear();//每次调用都需要清除文本，防止无效调用导致文本累加
            }
        }

        //处理手动导入
        private void HandleManualImport()
        {
            //已存在点位
            if (JDBC.GetPointsByWorkId(meTicketInfo.WorkId).Count > 0)
            {
                ReadticketPoints = JDBC.GetPointsByWorkId(meTicketInfo.WorkId);//读取数据库记录
                ReadPointWrenchList = JDBC.GetAllPointWrenches();//读取扳手点位
                points.Clear();

                Bitmap bmp = new Bitmap(pictureBox1.ClientSize.Width, pictureBox1.ClientSize.Height); ;
                Graphics g = Graphics.FromImage(bmp);
                foreach (var item in ReadticketPoints)
                {
                    //随机生成矩形的颜色
                    Random rnd = new Random();
                    Color rectColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));

                    //防止加载过快，造成点位颜色一致
                    Thread.Sleep(2);

                    pointX = int.Parse(item.PointPosition.Split(',')[0]);
                    pointY = int.Parse(item.PointPosition.Split(',')[1]);

                    //绘制圆形
                    g.FillEllipse(new SolidBrush(rectColor), pointX - radius, pointY - radius, radius * 2, radius * 2);

                    SizeF textSize = g.MeasureString(item.PointNumber, font);

                    //根据结果更新当前工单情况（区分已合格的和未合格的螺栓）
                    if (item.Result == "pass")
                    {
                        g.DrawString("√", font, brush, pointX - textSize.Width / 2, pointY - textSize.Height / 2); // 合格画"√"
                    }
                    else
                    {
                        g.DrawString(item.PointNumber, font, brush, pointX - textSize.Width / 2, pointY - textSize.Height / 2); // 不合格或者未拧过画"数字"
                    }

                    //避免加载之后新增点位从头开始
                    pointNum = int.Parse(item.PointNumber);
                    points.Add(new Tuple<Color, Point, int>(rectColor, new Point(pointX, pointY), pointNum));
                }
                g.Dispose();
                pictureBox1.Image = bmp;
            }

            //默认更新第一个（没有拧紧的）点位
            if (ReadticketPoints.Count >= 1)
            {
                int ngPointNum = 0;//第一个没有合格的点位

                //点击新点位切换设备
                for (int i = 0; i < ReadticketPoints.Count; i++)
                {
                    if (ReadticketPoints[i].Result != "pass")
                    {
                        updatePointInfo(JDBC.GetScrewAlarmByScrewId(ReadticketPoints[i].ScrewsId), ReadticketPoints[i].PointNumber);
                        ngPointNum++;
                        break;
                    }
                }

                if (ngPointNum == 0)
                {
                    MessageBox.Show("所有点位均合格，无需使用该工单", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    updatePointInfo(JDBC.GetScrewAlarmByScrewId(ReadticketPoints[0].ScrewsId), ReadticketPoints[0].PointNumber);
                }
            }
            else if (ReadticketPoints.Count == 1)
            {
                updatePointInfo(JDBC.GetScrewAlarmByScrewId(ReadticketPoints[0].ScrewsId), ReadticketPoints[0].PointNumber);
            }
        }

        //处理扫码导入
        private void HandleScanCodeImport()
        {
            //已存在点位
            if (JDBC.GetProductResultsByWorkIdAndSequenceId(meProductInfo.WorkId, meProductInfo.SequenceId).Count > 0)
            {
                ReadticketPoints.Clear();//切换模式，需要重置工单信息
                ReadPointWrenchList.Clear();

                meProductResults = JDBC.GetProductResultsByWorkIdAndSequenceId(meProductInfo.WorkId, meProductInfo.SequenceId);//读取数据库记录，工单实例
                foreach (var item in meProductResults)
                {
                    ReadticketPoints.Add(new DSTicketPoints()
                    {
                        PointId = item.PointId,
                        PointNumber = item.PointNumber,
                        WorkId = item.WorkId,
                        PointPosition = item.PointPosition,
                        ScrewsId = item.ScrewId,
                        ScrewNum = item.ScrewNum,
                        ScrewSeq = item.ScrewSeq,
                        Result = item.Result,
                    });
                    ReadPointWrenchList.Add(new DSRelationsPointWrench
                    {
                        PointId = item.PointId,
                        Addr = item.Addr,
                    });
                }
                points.Clear();

                //画所有点位
                DrawPoints(ReadticketPoints);        
            }

            DSTicketScrews targetScrews = new DSTicketScrews();//对应螺丝
                                                               //默认更新第一个（没有拧紧的）点位
            if (meProductResults.Count >= 1)
            {
                int ngPointNum = 0;//第一个没有合格的点位
                                   //点击新点位切换设备
                for (int i = 0; i < meProductResults.Count; i++)
                {
                    if (meProductResults[i].Result != "pass")
                    {
                        targetScrews = new DSTicketScrews()
                        {
                            ScrewId = meProductResults[i].ScrewId,
                            Name = meProductResults[i].Name,
                            Specification = meProductResults[i].Specification,
                            Torque_unit = meProductResults[i].Torque_unit,
                            ModePt = meProductResults[i].ModePt,
                            ModeAx = meProductResults[i].ModeAx,
                            ModeMx = meProductResults[i].ModeMx,
                            Alarm0 = meProductResults[i].Alarm0,
                            Alarm1 = meProductResults[i].Alarm1,
                            Alarm2 = meProductResults[i].Alarm2,
                        };
                        updatePointInfo(targetScrews, meProductResults[i].PointNumber);
                        ngPointNum++;
                        break;
                    }
                }

                if (ngPointNum == 0)
                {
                    //MessageBox.Show("所有点位均合格，无需使用该工单", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    //updatePointInfo(JDBC.GetScrewAlarmByScrewId(ReadticketPoints[0].ScrewsId), ReadticketPoints[0].PointNumber);

                    DialogResult chioceResult = MessageBox.Show("所有点位均合格，是否继续使用该工单", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    //根据选项选择是否继续拧已完成的工单
                    if (chioceResult == DialogResult.Yes)
                    {
                        isBegin = true;
                    }
                    else
                    {
                        isBegin = false;
                    }
                    updatePointInfo(JDBC.GetScrewAlarmByScrewId(ReadticketPoints[0].ScrewsId), ReadticketPoints[0].PointNumber);
                }
            }
            else if (ReadticketPoints.Count == 1)
            {
                targetScrews = new DSTicketScrews()
                {
                    ScrewId = meProductResults[0].ScrewId,
                    Name = meProductResults[0].Name,
                    Specification = meProductResults[0].Specification,
                    Torque_unit = meProductResults[0].Torque_unit,
                    ModePt = meProductResults[0].ModePt,
                    ModeAx = meProductResults[0].ModeAx,
                    ModeMx = meProductResults[0].ModeMx,
                    Alarm0 = meProductResults[0].Alarm0,
                    Alarm1 = meProductResults[0].Alarm1,
                    Alarm2 = meProductResults[0].Alarm2,
                };
                updatePointInfo(targetScrews, ReadticketPoints[0].PointNumber);
            }
        }

        //画点位信息
        private void DrawPoints(List<DSTicketPoints> ticketPoints)
        {
            Bitmap bmp = new Bitmap(pictureBox1.ClientSize.Width, pictureBox1.ClientSize.Height); ;
            Graphics g = Graphics.FromImage(bmp);
            foreach (var item in ticketPoints)
            {
                //随机生成矩形的颜色
                Random rnd = new Random();
                Color rectColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));

                //防止加载过快，造成点位颜色一致
                Thread.Sleep(2);

                pointX = int.Parse(item.PointPosition.Split(',')[0]);
                pointY = int.Parse(item.PointPosition.Split(',')[1]);

                //绘制圆形
                g.FillEllipse(new SolidBrush(rectColor), pointX - radius, pointY - radius, radius * 2, radius * 2);

                SizeF textSize = g.MeasureString(item.PointNumber, font);

                //根据结果更新当前工单情况（区分已合格的和未合格的螺栓）
                if (item.Result == "pass")
                {
                    g.DrawString("√", font, brush, pointX - textSize.Width / 2, pointY - textSize.Height / 2); // 合格画"√"
                }
                else
                {
                    g.DrawString(item.PointNumber, font, brush, pointX - textSize.Width / 2, pointY - textSize.Height / 2); // 不合格或者未拧过画"数字"
                }

                //避免加载之后新增点位从头开始
                pointNum = int.Parse(item.PointNumber);
                points.Add(new Tuple<Color, Point, int>(rectColor, new Point(pointX, pointY), pointNum));
            }
            g.Dispose();
            pictureBox1.Image = bmp;
        }

        //更新螺栓信息
        private void updatePointInfo(DSTicketScrews screw, string pointNumber)
        {
            //防止螺栓删除后扫工单查询出现调用null报错
            if (screw == null)
            {
                MessageBox.Show("该螺栓在螺栓库中被删除，无法查询该螺栓信息");
                return;
            }

            tb_pointNum.Text = pointNumber;
            tb_screwName.Text = screw.Name;
            tb_screwSpecs.Text = screw.Specification;
            tb_Unit.Text = screw.Torque_unit;
            tb_screwPt.Text = screw.ModePt;
            tb_screwMx.Text = screw.ModeMx;
            tb_screwAx.Text = screw.ModeAx;
            label_result.Text = pointNumber;

            //更新扳手地址
            updateScrewAddr(pointNumber);

            //更新报警值
            switch (screw.ModeAx)
            {
                case "SN":
                    label7.Text = "扭矩限制值";
                    tb_alarm1.Text = screw.Alarm0;
                    label8.Visible = false;
                    tb_alarm2.Visible = false;
                    label9.Visible = false;
                    tb_alarm3.Visible = false;

                    label_torqueAlarm.Text = "预设扭矩 " + screw.Alarm0 + " " + tb_Unit.Text;
                    label_angleAlarm.Text = "预设角度 ";

                    if (actXET == null)
                    {

                    }
                    if (actXET != null && 
                        (actXET.devc.type != TYPE.TQ_XH_XL01_07 - (UInt16)ADDROFFSET.TQ_XH_ADDR || actXET.devc.type != TYPE.TQ_XH_XL01_09 - (UInt16)ADDROFFSET.TQ_XH_ADDR)
                        )
                    {
                        label_angleAlarm.Text = "复拧角度 " + angleResit;
                    }
                    break;

                case "SA":
                    label7.Text = "扭矩限制值";
                    tb_alarm1.Text = screw.Alarm0;
                    label8.Text = "角度限制值";
                    label8.Visible = true;
                    tb_alarm2.Visible = true;
                    tb_alarm2.Text = screw.Alarm1;
                    label9.Visible = false;
                    tb_alarm3.Visible = false;

                    label_torqueAlarm.Text = "预设扭矩 " + screw.Alarm0 + " " + tb_Unit.Text;
                    label_angleAlarm.Text = "预设角度 " + screw.Alarm1;
                    break;

                case "MN":
                    label7.Text = "扭矩下限值";
                    tb_alarm1.Text = screw.Alarm0;
                    label8.Text = "扭矩上限值";
                    label8.Visible = true;
                    tb_alarm2.Visible = true;
                    tb_alarm2.Text = screw.Alarm1;
                    label9.Visible = false;
                    tb_alarm3.Visible = false;

                    label_torqueAlarm.Text = "预设扭矩 " + "( " + screw.Alarm0 + ", " + screw.Alarm1 + " )" + " " + tb_Unit.Text;
                    label_angleAlarm.Text = "预设角度 ";

                    if (actXET != null &&
                        (actXET.devc.type != TYPE.TQ_XH_XL01_07 - (UInt16)ADDROFFSET.TQ_XH_ADDR || actXET.devc.type != TYPE.TQ_XH_XL01_09 - (UInt16)ADDROFFSET.TQ_XH_ADDR)
                        )
                    {
                        label_angleAlarm.Text = "复拧角度 " + angleResit;
                    }
                    break;

                case "MA":
                    label7.Text = "扭矩限制值";
                    tb_alarm1.Text = screw.Alarm0;
                    label8.Text = "角度下限值";
                    label9.Text = "角度上限值";
                    label8.Visible = true;
                    tb_alarm2.Visible = true;
                    tb_alarm2.Text = screw.Alarm1;
                    label9.Visible = true;
                    tb_alarm3.Visible = true;
                    tb_alarm3.Text = screw.Alarm2;

                    label_torqueAlarm.Text = "预设扭矩 " + screw.Alarm0 + " " + tb_Unit.Text;
                    label_angleAlarm.Text = "预设角度 " + "( " + screw.Alarm1 + ", " + screw.Alarm2 + " )";
                    break;

                default:
                    break;
            }

            //数据库存储数据点位更新
            MyDevice.PointNum = pointNumber;
        }

        //更新选中扳手地址
        private void updateScrewAddr(string pointNumber)
        {
            //所有点位扳手关系
            List<DSRelationsPointWrench> AllpointWrench = new List<DSRelationsPointWrench>();
            foreach (var item1 in ReadPointWrenchList)
            {
                AllpointWrench.Add(item1);
            }

            //所有点位
            List<DSTicketPoints> AllPoints = new List<DSTicketPoints>();
            foreach (var item1 in ReadticketPoints)
            {
                AllPoints.Add(item1);
            }

            foreach (var item in AllpointWrench)
            {
                if (AllPoints[Convert.ToInt16(pointNumber) - 1].PointId == item.PointId)
                {
                    tb_wrenchAddr.Text = item.Addr.ToString();
                }

            }
        }

        //更新参数（用于发送写参数指令）
        private void updatePara()
        {
            switch (tb_Unit.Text)
            {
                case "N·m":
                    actXET.para.torque_unit = (UNIT)0;
                    break;
                case "lbf·in":
                    actXET.para.torque_unit = (UNIT)1;
                    break;
                case "lbf·ft":
                    actXET.para.torque_unit = (UNIT)2;
                    break;
                case "kgf·cm":
                    actXET.para.torque_unit = (UNIT)3;
                    break;
                default:
                    break;
            }
            switch (tb_screwPt.Text)
            {
                case "Track":
                    actXET.para.mode_pt = 0;
                    break;
                case "Peak":
                    actXET.para.mode_pt = 1;
                    break;
                default:
                    break;
            }
            switch (tb_screwAx.Text)
            {
                case "EN":
                    actXET.para.mode_ax = 0;
                    actXET.para.angle_resist = (int)(angleResit * Math.Pow(10, actXET.para.angle_decimal));
                    break;
                case "EA":
                    actXET.para.mode_ax = 1;
                    break;
                case "SN":
                    actXET.para.mode_ax = 2;
                    actXET.para.angle_resist = (int)(angleResit * Math.Pow(10, actXET.para.angle_decimal));
                    break;
                case "SA":
                    actXET.para.mode_ax = 3;
                    break;
                case "MN":
                    actXET.para.mode_ax = 4;
                    actXET.para.angle_resist = (int)(angleResit * Math.Pow(10, actXET.para.angle_decimal));
                    break;
                case "MA":
                    actXET.para.mode_ax = 5;
                    break;
                default:
                    break;
            }
            switch (tb_screwMx.Text)
            {
                case "M0":
                    actXET.para.mode_mx = 0;
                    break;
                case "M1":
                    actXET.para.mode_mx = 1;
                    break;
                case "M2":
                    actXET.para.mode_mx = 2;
                    break;
                case "M3":
                    actXET.para.mode_mx = 3;
                    break;
                case "M4":
                    actXET.para.mode_mx = 4;
                    break;
                case "M5":
                    actXET.para.mode_mx = 5;
                    break;
                case "M6":
                    actXET.para.mode_mx = 6;
                    break;
                case "M7":
                    actXET.para.mode_mx = 7;
                    break;
                case "M8":
                    actXET.para.mode_mx = 8;
                    break;
                case "M9":
                    actXET.para.mode_mx = 9;
                    break;
                default:
                    break;
            }
        }

        //更新报警值
        private void updateAlarm()
        {
            if (tb_screwAx.Text != "")
            {
                switch (tb_screwAx.Text)
                {
                    case "EN":
                        break;
                    case "EA":
                        break;
                    case "SN":
                        actXET.alam.SN_target[actXET.para.mode_mx, (int)actXET.para.torque_unit] = (Int32)(Convert.ToDouble(tb_alarm1.Text) * actXET.torqueMultiple + 0.5);
                        break;
                    case "SA":
                        actXET.alam.SA_pre[actXET.para.mode_mx, (int)actXET.para.torque_unit] = (Int32)(Convert.ToDouble(tb_alarm1.Text) * actXET.torqueMultiple + 0.5);
                        actXET.alam.SA_ang[actXET.para.mode_mx] = (Int32)(Convert.ToDouble(tb_alarm2.Text) * actXET.angleMultiple + 0.5);
                        break;
                    case "MN":
                        actXET.alam.MN_low[actXET.para.mode_mx, (int)actXET.para.torque_unit] = (Int32)(Convert.ToDouble(tb_alarm1.Text) * actXET.torqueMultiple + 0.5);
                        actXET.alam.MN_high[actXET.para.mode_mx, (int)actXET.para.torque_unit] = (Int32)(Convert.ToDouble(tb_alarm2.Text) * actXET.torqueMultiple + 0.5);
                        break;
                    case "MA":
                        actXET.alam.MA_pre[actXET.para.mode_mx, (int)actXET.para.torque_unit] = (Int32)(Convert.ToDouble(tb_alarm1.Text) * actXET.torqueMultiple + 0.5);
                        actXET.alam.MA_low[actXET.para.mode_mx] = (Int32)(Convert.ToDouble(tb_alarm2.Text) * actXET.angleMultiple + 0.5);
                        actXET.alam.MA_high[actXET.para.mode_mx] = (Int32)(Convert.ToDouble(tb_alarm3.Text) * actXET.angleMultiple + 0.5);
                        break;
                    default:
                        break;
                }
            }
        }

        //判断鼠标触点是否在有效点位代表的圆内
        private bool IsPointInsideCircle(Point point, Point center, int radius)
        {
            // 计算点击位置到圆心的距离
            double distance = Math.Sqrt(Math.Pow(point.X - center.X, 2) + Math.Pow(point.Y - center.Y, 2));

            // 判断距离是否小于圆的半径
            return distance <= radius;
        }

        //判断扳手数据是否合格
        private bool checkDataValid()
        {
            for (int i = 0; i < 5; i++)
            {
                if (actXET.data[i].dtype == 0xF1 || actXET.data[i].dtype == 0xF2 || actXET.data[i].dtype == 0xF3)
                {
                    //获取实时扭矩数据
                    if (actXET.data[i].dtype == 0xF1)
                    {
                        torque = actXET.data[i].torque;
                        torUnit = (byte)actXET.data[i].torque_unit;
                    }
                    else if (actXET.data[i].dtype == 0xF2)
                    {
                        torque = actXET.data[i].torseries_pk;
                        torUnit = (byte)actXET.data[i].torque_unit;
                    }
                    else if (actXET.data[i].dtype == 0xF3)
                    {
                        torque = actXET.data[i].torgroup_pk;
                    }

                    //获取实时角度数据
                    angle = (actXET.data[i].dtype == 0xF1) ? actXET.data[i].angle : actXET.data[i].angle_acc;

                    //实时显示扭矩和角度
                    label_torque.Text = "实时扭矩 " + torque / actXET.torqueMultiple + " " + tb_Unit.Text;
                    label_angle.Text = "实时角度 " + angle / actXET.angleMultiple;

                    //超量程结束
                    if (torque > actXET.devc.torque_over[torUnit])
                    {
                        isDataValid = false;
                        return isDataValid;
                    }

                    if (actXET.data[i].dtype == 0xF3)
                    {
                        //单位更新
                        switch (tb_screwAx.Text)
                        {
                            case "EN":
                                break;
                            case "EA":
                                break;
                            case "SN":
                                //峰值扭矩 >= 预设扭矩 = 合格
                                if (torque >= actXET.alam.SN_target[actXET.para.mode_mx, (int)actXET.para.torque_unit])
                                {
                                    isDataValid = true;
                                    //扭矩优先模式下再判断复拧角度
                                    isDataValid = !IsAngleResist(actXET.data[i], actXET, angle, angleResit);
                                }
                                else
                                {
                                    isDataValid = false;
                                }
                                break;
                            case "SA":
                                //峰值扭矩 >= 预设扭矩 && 峰值角度 >= 预设角度 = 合格
                                if (torque >= actXET.alam.SA_pre[actXET.para.mode_mx, (int)actXET.para.torque_unit] && angle >= actXET.alam.SA_ang[actXET.para.mode_mx])
                                {
                                    isDataValid = true;
                                }
                                else
                                {
                                    isDataValid = false;
                                }
                                break;
                            case "MN":
                                // 扭矩下限 <= 峰值扭矩 <= 扭矩上限  = 合格
                                if (actXET.alam.MN_low[actXET.para.mode_mx, (int)actXET.para.torque_unit] <= torque && torque <= actXET.alam.MN_high[actXET.para.mode_mx, (int)actXET.para.torque_unit])
                                {
                                    isDataValid = true;
                                    isDataValid = !IsAngleResist(actXET.data[i], actXET, angle, angleResit);
                                }
                                else
                                {
                                    isDataValid = false;
                                }
                                break;
                            case "MA":
                                //峰值扭矩 >= 预设扭矩 && 角度下限 <= 峰值角度 <= 角度上限 = 合格
                                if (torque >= actXET.alam.MA_pre[actXET.para.mode_mx, (int)actXET.para.torque_unit]
                                    && actXET.alam.MA_low[actXET.para.mode_mx] <= angle && angle <= actXET.alam.MA_high[actXET.para.mode_mx])
                                {
                                    isDataValid = true;
                                }
                                else
                                {
                                    isDataValid = false;
                                }
                                break;
                            case "AZ":
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            return isDataValid;
        }

        //判断扳手数据是否合格
        private bool checkDataValid_XH06()
        {
            torquePeak = 0;
            anglePeak = 0;
            for (int i = 0; i < 5; i++)
            {
                if (actXET.data[i].dtype == 0xF1 || actXET.data[i].dtype == 0xF2)
                {
                    //获取实时扭矩数据
                    if (actXET.data[i].dtype == 0xF1)
                    {
                        torque = actXET.data[i].torque;
                    }
                    else if (actXET.data[i].dtype == 0xF2)
                    {
                        torque = actXET.data[i].torseries_pk;
                    }

                    //获取实时角度数据
                    angle = (actXET.data[i].dtype == 0xF1) ? actXET.data[i].angle : actXET.data[i].angle_acc;

                    //实时显示扭矩和角度
                    label_torque.Text = "实时扭矩 " + torque / actXET.torqueMultiple + " " + tb_Unit.Text;
                    label_angle.Text = "实时角度 " + angle / actXET.angleMultiple;

                    //超量程结束
                    if (torque > actXET.devc.torque_over[(int)actXET.data[i].torque_unit])
                    {
                        isDataValid = false;
                        return isDataValid;
                    }

                    //XH06需要02数据才判断合格
                    if (actXET.data[i].dtype == 0xF2)
                    {
                        //单位更新
                        switch (tb_screwAx.Text)
                        {
                            case "EN":
                                break;
                            case "EA":
                                break;
                            case "SN":
                                //峰值扭矩 >= 预设扭矩 = 合格
                                if (torque >= actXET.alam.SN_target[actXET.para.mode_mx, (int)actXET.para.torque_unit])
                                {
                                    isDataValid = true;
                                    return isDataValid;
                                }
                                else
                                {
                                    isDataValid = false;
                                }
                                break;
                            case "SA":
                                //峰值扭矩 >= 预设扭矩 && 峰值角度 >= 预设角度 = 合格
                                if (torque >= actXET.alam.SA_pre[actXET.para.mode_mx, (int)actXET.para.torque_unit] && angle >= actXET.alam.SA_ang[actXET.para.mode_mx])
                                {
                                    isDataValid = true;
                                    return isDataValid;
                                }
                                else
                                {
                                    isDataValid = false;
                                }
                                break;
                            case "MN":
                                // 扭矩下限 <= 峰值扭矩 <= 扭矩上限  = 合格
                                if (actXET.alam.MN_low[actXET.para.mode_mx, (int)actXET.para.torque_unit] <= torque && torque <= actXET.alam.MN_high[actXET.para.mode_mx, (int)actXET.para.torque_unit])
                                {
                                    isDataValid = true;
                                    return isDataValid;
                                }
                                else
                                {
                                    isDataValid = false;
                                }
                                break;
                            case "MA":
                                //峰值扭矩 >= 预设扭矩 && 角度下限 <= 峰值角度 <= 角度上限 = 合格
                                if (torque >= actXET.alam.MA_pre[actXET.para.mode_mx, (int)actXET.para.torque_unit]
                                    && actXET.alam.MA_low[actXET.para.mode_mx] <= angle && angle <= actXET.alam.MA_high[actXET.para.mode_mx])
                                {
                                    isDataValid = true;
                                    return isDataValid;
                                }
                                else
                                {
                                    isDataValid = false;
                                }
                                break;
                            case "AZ":
                                break;
                            default:
                                break;
                        }
                    }

                }
            }
            return isDataValid;
        }

        //切换点位
        private void changePoint(byte addr)
        {
            //点击新点位切换设备
            MyDevice.protocol.addr = addr;
            if (MyDevice.protocol.type == COMP.TCP)
            {
                if (MyDevice.addr_ip.ContainsKey(addr.ToString()) == true)
                {
                    MyDevice.protocol.port = MyDevice.clientConnectionItems[MyDevice.addr_ip[MyDevice.protocol.addr.ToString()]];
                }
            }
            actXET = MyDevice.actDev;
            actXET.torqueMultiple = (int)Math.Pow(10, actXET.devc.torque_decimal);
            actXET.angleMultiple = (int)Math.Pow(10, actXET.para.angle_decimal);
            updatePara();
            updateAlarm();

            if (isMatchDevType(actXET.devc.type.ToString(), tb_Unit.Text, tb_screwAx.Text, tb_screwMx.Text) == false)
            {
                return;
            }

            MyDevice.myTaskManager.SelectedDev = addr;
            MyDevice.myTaskManager.Mode = AutoMode.UserAndTicketWork;
            //解按键锁,防止被锁住
            MyDevice.keyLock = (byte)KEYLOCK.KEY_UNLOCK;
            MyDevice.myTaskManager.AddUserCommand(addr, ProtocolFunc.Protocol_Write_SendCOM, TASKS.WRITE_KEYLOCK, MyDevice.keyLock, this.Name);
            //设置参数及报警值
            List<TASKS> tasks = new List<TASKS>
            {
                TASKS.REG_BLOCK3_PARA,
                TASKS.REG_BLOCK5_AM1,
                TASKS.REG_BLOCK5_AM2,
                TASKS.REG_BLOCK5_AM3,
            };
            MyDevice.myTaskManager.AddUserCommands(addr, ProtocolFunc.Protocol_Sequence_SendCOM, tasks, this.Name);
            //启动蜂鸣器并且开启按键锁
            MyDevice.keyLock = (byte)KEYLOCK.KEY_BUZZER;
            MyDevice.myTaskManager.AddUserCommand(addr, ProtocolFunc.Protocol_Write_SendCOM, TASKS.WRITE_KEYLOCK, MyDevice.keyLock, this.Name);
            isDataValid = false;
            isAllPointPassShow = false;
            panel2.BackColor = Color.White;
            panel3.BackColor = Color.White;
            torquePeak = 0;
            anglePeak = 0;
            torque = 0;
            angle = 0;
            label_torque.Text = "实时扭矩 0";
            label_angle.Text = "实时角度 0";
        }

        //点位合格打“√”
        private void isPass()
        {
            Bitmap bmp = new Bitmap(pictureBox1.ClientSize.Width, pictureBox1.ClientSize.Height);
            Graphics g = Graphics.FromImage(bmp);
            for (int i = 0; i < ReadticketPoints.Count; i++)
            {
                Color rectColor = points[i].Item1;

                pointX = int.Parse(ReadticketPoints[i].PointPosition.Split(',')[0]);
                pointY = int.Parse(ReadticketPoints[i].PointPosition.Split(',')[1]);

                //绘制圆形
                g.FillEllipse(new SolidBrush(rectColor), pointX - radius, pointY - radius, radius * 2, radius * 2);

                SizeF textSize = g.MeasureString(ReadticketPoints[i].PointNumber, font);

                //根据结果更新当前工单情况（区分已合格的和未合格的螺栓）
                if (ReadticketPoints[i].Result == "pass")
                {
                    g.DrawString("√", font, brush, pointX - textSize.Width / 2, pointY - textSize.Height / 2); // 合格画"√"
                }
                else
                {
                    g.DrawString(ReadticketPoints[i].PointNumber, font, brush, pointX - textSize.Width / 2, pointY - textSize.Height / 2); // 不合格或者未拧过画"数字"
                }
            }
            g.Dispose();
            pictureBox1.Image = bmp;
        }

        //判断是否所有点位都合格
        private bool isAllPointPass()
        {
            //默认更新第一个（没有拧紧的）点位
            if (ReadticketPoints.Count > 0)
            {
                //点击新点位切换设备
                for (int i = 0; i < ReadticketPoints.Count; i++)
                {
                    if (ReadticketPoints[i].Result != "pass")
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        //判断当前点位的模式和单位是否匹配该型号的扳手
        private bool isMatchDevType(string devType, string unit, string ax, string mx)
        {
            bool isMatch = true;

            if (devType.Contains("5"))
            {
                //XH05 只有EN,SN,MN模式， 单位没有kgm
                switch (ax)
                {
                    case "SN":
                    case "MN":
                        if (unit == "kgf·m")
                        {
                            isMatch = false;
                            MessageBox.Show($"XH05系列扳手无{unit}单位，请更换扳手", "确认", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        break;
                    case "SA":
                    case "MA":
                    case "AZ":
                        isMatch = false;
                        MessageBox.Show($"XH05系列扳手无{ax}模式，请更换扳手", "确认", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;
                    default:
                        break;
                }
            }
            else if (devType.Contains("6"))
            {
                //XH06 无AZ， 单位没有kgm
                switch (ax)
                {
                    case "SN":
                    case "SA":
                    case "MN":
                    case "MA":
                        if (unit == "kgf·m")
                        {
                            isMatch = false;
                            MessageBox.Show($"XH05系列扳手无{unit}单位，请更换扳手", "确认", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        break;
                    case "AZ":
                        isMatch = false;
                        MessageBox.Show($"XH05系列扳手无{ax}模式，请更换扳手", "确认", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;
                    default:
                        break;
                }
            }
            else if (devType.Contains("7") || devType.Contains("9"))
            {
                //XH07/09 无AZ
                switch (ax)
                {
                    case "SN":
                        break;
                    case "SA":
                        break;
                    case "MN":
                        break;
                    case "MA":
                        break;
                    case "AZ":
                        isMatch = false;
                        if (devType.Contains("7"))
                        {
                            MessageBox.Show($"XH07系列扳手无{ax}模式，请更换扳手", "确认", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else
                        {
                            MessageBox.Show($"XH09系列扳手无{ax}模式，请更换扳手", "确认", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        break;
                    default:
                        break;
                }
            }
            else if (devType.Contains("8"))
            {
                //XH08 只有SN,AZ模式， 单位只有N·m, Mx只有M0
                switch (ax)
                {
                    case "SN":
                    case "AZ":
                        if (unit != "N·m")
                        {
                            isMatch = false;
                            MessageBox.Show($"XH08系列扳手无{unit}单位，请更换扳手", "确认", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return isMatch;
                        }

                        if (mx != "M0")
                        {
                            isMatch = false;
                            MessageBox.Show($"XH08系列扳手无{mx}单位，请更换扳手", "确认", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        break;
                    case "SA":
                    case "MN":
                    case "MA":
                        isMatch = false;
                        MessageBox.Show($"XH08系列扳手无{ax}模式，请更换扳手", "确认", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;
                    default:
                        break;
                }
            }            

            return isMatch;
        }

        //判断结果数据是否需要重复拧紧(F3专属)
        private bool IsAngleResist(DATA data, XET xet, double angle, double angleResist)
        {
            bool isAngleResist = false;

            //F3 结果数据是否需要重复拧紧
            if (data.dtype == 0xF3)
            {
                if (data.mode_ax == 0 || data.mode_ax == 2 || data.mode_ax == 4)// EN|SN|MN (限定扭矩优先模式)
                {
                    //累加角度 < 复拧角度 ，提示客户重复拧紧
                    if (angle < angleResist)
                    {
                        isAngleResist = true;
                    }
                    else
                    {
                        isAngleResist = false;
                    }
                }
            }

            return isAngleResist;
        }

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
                            //判断该数据是否合格
                            MyDevice.myTaskManager.IsDataValid = checkDataValid_XH06();
                            if (isDataValid)
                            {
                                passPoint = ReadticketPoints[Convert.ToByte(tb_pointNum.Text) - 1];
                                //passPoint.Result = "pass";
                                if (isScanCode)
                                {
                                    passPoint.Result = "pass";
                                    //扫码调用的工单存储历史合格纪录
                                    DSProductResults product = meProductResults[Convert.ToByte(tb_pointNum.Text) - 1];

                                    JDBC.UpdateProductResultByWIdAndSqIdAndPointNum(product.WorkId, product.SequenceId, product.PointNumber);
                                }
                                else
                                {
                                    JDBC.UpdatePointByPointId(passPoint.PointId, passPoint);//更新位点，合格
                                }
                                isPass();//更新合格点位打"√"
                                panel2.BackColor = Color.Green;
                            }
                        }
                        else if (actXET.devc.type == TYPE.TQ_XH_XL01_07 - 1280 || (actXET.devc.type == TYPE.TQ_XH_XL01_09 - 1280) || (actXET.devc.type == TYPE.TQ_XH_XL01_08 - 1280))
                        {
                            //判断该数据是否合格
                            MyDevice.myTaskManager.IsDataValid = checkDataValid();
                            if (isDataValid)
                            {
                                passPoint = ReadticketPoints[Convert.ToByte(tb_pointNum.Text) - 1];
                                //passPoint.Result = "pass";
                                if (isScanCode)
                                {
                                    passPoint.Result = "pass";
                                    //扫码调用的工单存储历史合格纪录
                                    DSProductResults product = meProductResults[Convert.ToByte(tb_pointNum.Text) - 1];

                                    JDBC.UpdateProductResultByWIdAndSqIdAndPointNum(product.WorkId, product.SequenceId, product.PointNumber);
                                }
                                else
                                {
                                    JDBC.UpdatePointByPointId(passPoint.PointId, passPoint);//更新位点，合格
                                }
                                isPass();//更新合格点位打"√"
                                panel2.BackColor = Color.Green;
                            }
                        }
                        break;
                    case TASKS.WRITE_FIFOCLEAR:
                        //合格清缓存并且切点
                        if (isDataValid)
                        {
                            //所有位点合格
                            if (isAllPointPass())
                            {
                                if (!isAllPointPassShow)
                                {
                                    MessageBox.Show("所有点位均合格，无需使用该工单", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    isAllPointPassShow = true;
                                }
                                updatePointInfo(JDBC.GetScrewAlarmByScrewId(ReadticketPoints[0].ScrewsId), ReadticketPoints[0].PointNumber);
                                MyDevice.myTaskManager.Mode = AutoMode.UserOnly;
                                return;
                            }

                            for (byte i = 0; i < points.Count; i++)
                            {
                                if (points[i].Item3 == Convert.ToByte(tb_pointNum.Text))
                                {
                                    if (ReadticketPoints[(i + 1) % points.Count].Result != "pass")
                                    {
                                        updatePointInfo(JDBC.GetScrewAlarmByScrewId(ReadticketPoints[(i + 1) % points.Count].ScrewsId), ReadticketPoints[(i + 1) % points.Count].PointNumber);
                                        changePoint(Convert.ToByte(tb_wrenchAddr.Text));
                                    }
                                    else
                                    {
                                        tb_pointNum.Text = ReadticketPoints[(i + 1) % points.Count].PointNumber;//合格，直接跳过这个点
                                        continue;
                                    }
                                    break;
                                }
                            }
                        }
                        //不合格清缓存，数据初始化
                        else
                        {
                            torquePeak = 0;
                            anglePeak = 0;
                            torque = 0;
                            angle = 0;
                            label_torque.Text = "实时扭矩 0";
                            label_angle.Text = "实时角度 0";
                        }
                        break;
                    default:
                        break;
                }
            };
            Invoke(action);
        }

        #region 屏幕自适应

        //
        public static void setTag(Control cons)
        {
            foreach (Control con in cons.Controls)//循环窗体中的控件
            {
                con.Tag = con.Width + ":" + con.Height + ":" + con.Left + ":" + con.Top + ":" + con.Font.Size;
                if (con.Controls.Count > 0)
                    setTag(con);
            }
        }

        public static void ReWinformLayout(Rectangle control, float x, float y, Control cn)
        {
            var newx = control.Width / x;
            var newy = control.Height / y;
            if (newx == 0) { newx = 0.01f; }
            if (newy == 0) { newy = 0.01f; }
            setControls(newx, newy, cn);
        }

        public static void setControls(float newx, float newy, Control cons)
        {
            //遍历窗体中的控件，重新设置控件的值
            foreach (Control con in cons.Controls)
            {
                if (con.Tag == null)
                {
                    continue;
                }
                string[] mytag = con.Tag.ToString().Split(new char[] { ':' });//获取控件的Tag属性值，并分割后存储字符串数组
                float a = System.Convert.ToSingle(mytag[0]) * newx;//根据窗体缩放比例确定控件的值，宽度
                con.Width = (int)a;//宽度
                a = System.Convert.ToSingle(mytag[1]) * newy;//高度
                con.Height = (int)(a);
                a = System.Convert.ToSingle(mytag[2]) * newx;//左边距离
                con.Left = (int)(a);
                a = System.Convert.ToSingle(mytag[3]) * newy;//上边缘距离
                con.Top = (int)(a);
                Single currentSize = System.Convert.ToSingle(mytag[4]) * newy;//字体大小
                con.Font = new Font(con.Font.Name, currentSize, con.Font.Style, con.Font.Unit);
                if (con.Controls.Count > 0)
                {
                    setControls(newx, newy, con);
                }
            }
        }

        private void MenuDeviceSetForm_Resize(object sender, EventArgs e)
        {
            ReWinformLayout(this.ClientRectangle, x, y, this);

        }

        #endregion

    }
}
