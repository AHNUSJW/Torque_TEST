using DBHelper;
using Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Base.UI.MenuHomework
{
    public partial class MenuCreateTicketForm : Form
    {
        private int radius = 60;        //圆的半径
        private int pointX = 0;         //点位X坐标
        private int pointY = 0;         //点位Y坐标
        private int pointNum = 0;       //点位号码，按顺序递增
        private int selectPointNum = -1;//选中的点位号码
        private uint pointId;           //点位序号（数据库工单点位表主键）

        private DSTicketInfo meTicketInfo = new DSTicketInfo();                                             //工单处理界面新建或选择的工单                                                                                                           //添加点位
        private DSTicketPoints ticketPoint = new DSTicketPoints();                                          //工单点位
        private List<DSTicketPoints> ReadticketPoints = new List<DSTicketPoints>();                         //从数据库读取的点位集合
        private List<Tuple<Color, Point, int>> points = new List<Tuple<Color, Point, int>>();               //点位信息集合（颜色，坐标，序号）
        private List<DSRelationsPointWrench> ReadPointWrenchList= new List<DSRelationsPointWrench>();       //从数据库读取扳手点位关系集合

        public DSTicketInfo MeTicketInfo { get => meTicketInfo; set => meTicketInfo = value; }

        public MenuCreateTicketForm()
        {
            InitializeComponent();
        }

        //页面加载
        private void MenuCreateTicketForm_Load(object sender, EventArgs e)
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

            label1.Text += meTicketInfo.WoNum;//更新工单号

            pictureBox1.BackgroundImageLayout = ImageLayout.Stretch;//使图片充满整个控件
            try
            {
                pictureBox1.BackgroundImage = Image.FromFile(meTicketInfo.ImagePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        //页面稳定后读取历史点位（加载时读取不稳定）
        private void MenuCreateTicketForm_Shown(object sender, EventArgs e)
        {
            loadAllPoint();
            label_tip.Visible = ReadticketPoints.Count > 0 ? false : true;//页面提示词
            this.ActiveControl = label_tip;
        }

        //鼠标双击添加点位
        private void pictureBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //鼠标点击的点位置
            pointX = e.Location.X;
            pointY = e.Location.Y;

            //随机生成矩形的颜色
            Random rnd = new Random();
            Color rectColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));

            DialogResult result = MessageBox.Show("是否在此处添加位点?", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                //绘制序号
                pointNum++;

                //确认新增点位
                points.Add(new Tuple<Color, Point, int>(rectColor, new Point(pointX, pointY), pointNum));

                Bitmap bmp = new Bitmap(pictureBox1.ClientSize.Width, pictureBox1.ClientSize.Height); ;
                Graphics g = Graphics.FromImage(bmp);
                foreach (var item in points)
                {
                    //绘制圆形
                    g.FillEllipse(new SolidBrush(item.Item1), item.Item2.X - radius, item.Item2.Y - radius, radius * 2, radius * 2);

                    Font font = new Font("微软雅黑", 60, FontStyle.Bold, GraphicsUnit.Point);
                    Brush brush = new SolidBrush(Color.White);
                    SizeF textSize = g.MeasureString(item.Item3.ToString(), font);
                    g.DrawString(item.Item3.ToString(), font, brush, item.Item2.X - textSize.Width / 2, item.Item2.Y - textSize.Height / 2); // 调整数字显示的位置
                }
                g.Dispose();
                pictureBox1.Image = bmp;

                //防止保存工单后继续添加点位，不更新数据库造成溢出报错
                ReadticketPoints = JDBC.GetPointsByWorkId(meTicketInfo.WorkId);//读取数据库点位信息
                ReadPointWrenchList = JDBC.GetAllPointWrenches();//读取扳手点位

                //弹窗搜索螺栓号页面
                SearchSrcewInfoForm searchSrcewInfoForm = new SearchSrcewInfoForm();
                searchSrcewInfoForm.StartPosition = FormStartPosition.CenterScreen;
                searchSrcewInfoForm.DataSelected += SearchSrcewInfoForm_ScrewSelected;//订阅搜索螺栓页面的事件
                searchSrcewInfoForm.ShowDialog();

            }
        }

        //左键选点位
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            MouseEventArgs mouseEventArgs = e as MouseEventArgs;
            Point clickPoint = mouseEventArgs.Location;         //鼠标点击的点位置
            ReadticketPoints = JDBC.GetPointsByWorkId(meTicketInfo.WorkId);//读取数据库点位信息

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
                    }
                }
            } 
        }

        //清空工单
        private void btn_Clear_Click(object sender, EventArgs e)
        {
            if (ReadticketPoints.Count > 0)
            {
                //删除位点
                DialogResult result = MessageBox.Show("是否删除所有位点" + "？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    foreach (var item in ReadticketPoints)
                    {
                        JDBC.DeletePointsByPointId(item.PointId);//删除点位
                        JDBC.DeletePointWrenchsByPointId(item.PointId);//删除点位扳手关系
                    }
                }
                else
                    return;

                //删除所有点位后会重绘页面,变量需要初始化
                ReadticketPoints.Clear();
                ReadPointWrenchList.Clear();
                points.Clear();
                pointNum = 0;

                //无点位，初始化页面
                Bitmap bmp = new Bitmap(pictureBox1.ClientSize.Width, pictureBox1.ClientSize.Height); ;
                Graphics g = Graphics.FromImage(bmp);
                g.Dispose();
                pictureBox1.Image = bmp;
            }
        }

        //使用工单
        private void btn_Use_Click(object sender, EventArgs e)
        {
            if (JDBC.GetPointsByWorkId(meTicketInfo.WorkId).Count == 0)
            {
                MessageBox.Show("无有效点位，无法使用工单", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MenuTicketWorkForm menuTicketWorkForm = new MenuTicketWorkForm();
            menuTicketWorkForm.MeTicketInfo = meTicketInfo;
            menuTicketWorkForm.MdiParent = this.MdiParent;
            menuTicketWorkForm.Show();
            menuTicketWorkForm.WindowState = FormWindowState.Maximized;
            this.Close();
        }

        //更新选中螺栓表对应的内容，添加点位
        private void SearchSrcewInfoForm_ScrewSelected(DSTicketScrews screw)
        {
            //添加点位
            ticketPoint = new DSTicketPoints()
            {
                PointId = ++pointId,
                PointNumber = pointNum.ToString(),
                WorkId = meTicketInfo.WorkId,
                PointPosition = pointX.ToString() + "," + pointY.ToString(),
                ScrewsId = screw.ScrewId,
            };

            ReadticketPoints.Add(ticketPoint);
            pointId = (uint)JDBC.AddPoint(ticketPoint);//该方法返回pointId，保存点位
            label_tip.Visible =  false;//隐藏页面提示词

            //更新螺栓信息
            updatePointInfo(screw, pointNum.ToString());

            //弹窗搜索扳手号页面
            SearchWrenchInfoForm searchWrenchInfoForm = new SearchWrenchInfoForm();
            searchWrenchInfoForm.StartPosition = FormStartPosition.CenterScreen;
            searchWrenchInfoForm.DataSelected += SearchSrcewInfoForm_WrenchSelected;//订阅搜索扳手页面的事件
            searchWrenchInfoForm.ShowDialog();
        }

        //更新选中螺栓表对应的内容，添加点位
        private void SearchSrcewInfoForm_WrenchSelected(DSWrenchWlan wrench)
        {
            DSRelationsPointWrench meRelationsPointWrench = new DSRelationsPointWrench()
            {
                PointId = pointId,
                Addr = wrench.Addr,
            };

            ReadPointWrenchList.Add(meRelationsPointWrench);
            JDBC.AddPointWrench(meRelationsPointWrench);//保存点位与扳手关系

            //更新点位地址
            tb_wrenchAddr.Text = meRelationsPointWrench.Addr.ToString();
        }

        //更新螺栓信息
        private void updatePointInfo(DSTicketScrews screw, string pointNumber)
        {
            tb_pointNum.Text   = pointNumber;
            tb_screwName.Text  = screw.Name;
            tb_screwSpecs.Text = screw.Specification;
            tb_Unit.Text       = screw.Torque_unit;
            tb_screwPt.Text    = screw.ModePt;
            tb_screwMx.Text    = screw.ModeMx;
            tb_screwAx.Text    = screw.ModeAx;

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
                    break;

                default:
                    break;
            }
        }

        //更新选中扳手地址
        private void updateScrewAddr(string pointNumber)
        {
            //筛选
            foreach (var item in ReadPointWrenchList)
            {
                if (ReadticketPoints[Convert.ToInt16(pointNumber) - 1].PointId == item.PointId)
                {
                    tb_wrenchAddr.Text = item.Addr.ToString();
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

        //右击删除位点
        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ReadticketPoints = JDBC.GetPointsByWorkId(meTicketInfo.WorkId);//读取数据库点位信息
                string delPointNum = "";
                int delIndex = -1;

                //鼠标点击点位有效区域
                if (ReadticketPoints.Count > 0)
                {
                    foreach (var item in ReadticketPoints)
                    {
                        pointX = int.Parse(item.PointPosition.Split(',')[0]);
                        pointY = int.Parse(item.PointPosition.Split(',')[1]);
                        if (IsPointInsideCircle(e.Location, new Point(pointX, pointY), radius))
                        {
                            //删除位点
                            DialogResult result = MessageBox.Show("是否删除位点" + item.PointNumber + "？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            if (result == DialogResult.Yes)
                            {
                                JDBC.DeletePointsByPointId(item.PointId);//删除点位
                                JDBC.DeletePointWrenchsByPointId(item.PointId);//删除点位扳手关系
                                delPointNum = item.PointNumber;//纪录删除的点位号
                                break;
                            }

                        }
                    }
                    for (int i = 0; i < ReadticketPoints.Count; i++)
                    {
                        if (ReadticketPoints[i].PointNumber == delPointNum)
                        {
                            delIndex = i;
                            break;
                        }
                    }
                    if (delIndex >= 0)
                    {
                        for (int i = delIndex; i < ReadticketPoints.Count; i++)
                        {
                            ReadticketPoints[i].PointNumber = (Convert.ToInt32(ReadticketPoints[i].PointNumber) - 1).ToString();//删除点位之后的点位序号统一减1
                            JDBC.UpdatePointByPointId(ReadticketPoints[i].PointId, ReadticketPoints[i]);//点位序号发生改变，更新点位表
                        }

                        //删除点位后会重绘页面,变量需要初始化
                        ReadticketPoints.Clear();
                        ReadPointWrenchList.Clear();
                        points.Clear();
                        pointNum = 0;

                        //无点位，初始化页面
                        if (JDBC.GetPointsByWorkId(meTicketInfo.WorkId).Count == 0)
                        {
                            Bitmap bmp = new Bitmap(pictureBox1.ClientSize.Width, pictureBox1.ClientSize.Height); ;
                            Graphics g = Graphics.FromImage(bmp);
                            g.Dispose();
                            pictureBox1.Image = bmp;
                        }
                        //有点位，重绘页面
                        else
                        {
                            loadAllPoint();
                        }
                    }
                }
            }
        }
    
        //加载所有数据库点位
        private void loadAllPoint()
        {
            //已存在点位
            if (JDBC.GetPointsByWorkId(meTicketInfo.WorkId).Count > 0)
            {
                ReadticketPoints = JDBC.GetPointsByWorkId(meTicketInfo.WorkId);//读取数据库点位信息
                ReadPointWrenchList = JDBC.GetAllPointWrenches();//读取扳手点位
                pointId = ReadticketPoints[ReadticketPoints.Count - 1].PointId;//用于加载有位点的工单上新增位点序号

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

                    Font font = new Font("微软雅黑", 60, FontStyle.Bold, GraphicsUnit.Point);
                    Brush brush = new SolidBrush(Color.White);
                    SizeF textSize = g.MeasureString(item.PointNumber, font);
                    g.DrawString(item.PointNumber, font, brush, pointX - textSize.Width / 2, pointY - textSize.Height / 2); // 调整数字显示的位置

                    //避免加载之后新增点位从头开始
                    pointNum = int.Parse(item.PointNumber);
                    points.Add(new Tuple<Color, Point, int>(rectColor, new Point(pointX, pointY), pointNum));
                }
                g.Dispose();
                pictureBox1.Image = bmp;
            }

            //默认更新第一个点位
            if (ReadticketPoints.Count >= 1)
            {
                updatePointInfo(JDBC.GetScrewAlarmByScrewId(ReadticketPoints[0].ScrewsId), ReadticketPoints[0].PointNumber);
            }
        }
    
    }
}
