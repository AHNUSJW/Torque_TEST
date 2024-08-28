using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Library
{
    /// <summary>
    /// 轴模式
    /// </summary>
    public enum BackgroundImageType
    {
        [Description("仅有x轴")] OnlyXAxis,
        [Description("仅有xy轴")] OnlyXYAxis,
        [Description("有一个x轴两个y轴")] OneXTwoYAxis,
    }

    public class DrawPicture
    {
        private int height;         //画图的高度
        private int width;          //画图的宽度
        private int textInfo = 50;  //数字显示宽度
        private int startIdx;       //选取数据起始idx
        private int stopIdx;        //选取数据结束idx
        private int curIdx;         //选中数据当前idx

        private BackgroundImageType imageType;   //轴模式
        private int horizontalAxisNum;           //横轴数量
        private int verticalAxisNum;             //竖轴数量
        private int offsetX = 0;                 //X轴偏移量
        private int offsetY = 0;                 //Y轴偏移量
        private double limitUpperX;              //x轴上限
        private double limitLowerX;              //x轴下限
        private double limitUpperLeftY;          //y轴左轴上限
        private double limitLowerLeftY;          //y轴左轴下限
        private double limitUpperRightY;         //y轴右轴上限
        private double limitLowerRightY;         //y轴右轴下限


        private int lineNumbers;//画线数量
        private List<List<double>> data = new List<List<double>>();//存储数据
        private List<List<PointF>> points = new List<List<PointF>>();//存储数据点位坐标

        /// <summary>
        /// 画线颜色
        /// </summary>
        private Color[] drawLines = new Color[20] {
            Color.AntiqueWhite,
            Color.Aqua,
            Color.Aquamarine,
            Color.Blue,
            Color.BlueViolet,
            Color.Brown,
            Color.BurlyWood,
            Color.CadetBlue,
            Color.Chartreuse,
            Color.CornflowerBlue,
            Color.Crimson,
            Color.DarkBlue,
            Color.DarkBlue,
            Color.DarkCyan,
            Color.DarkSlateBlue,
            Color.DarkSlateGray,
            Color.Gold,
            Color.LightCoral,
            Color.SaddleBrown,
            Color.SlateGray
        };
        //定义背景画笔
        private Pen mypen = new Pen(Color.Black, 1.0f);
        //定义文字体和文字大小
        private Font fontText = new Font("Arial", 12);
        //定义文字颜色
        private Brush brushAxis = Brushes.Silver;

        #region set and get

        public int Height
        {
            get => height;
            set
            {
                height = value;
                if (height < 200)
                {
                    height = 200;
                }
            }
        }
        public int Width
        {
            get => width;
            set
            {
                width = value;
                if (width < 200)
                {
                    width = 200;
                }
            }
        }
        public int TextInfo { get => textInfo; set => textInfo = value; }
        public int StartIdx { get => startIdx; set => startIdx = value; }
        public int StopIdx { get => stopIdx; set => stopIdx = value; }
        public int CurIdx { get => curIdx; set => curIdx = value; }
        public BackgroundImageType ImageType { get => imageType; set => imageType = value; }
        public int HorizontalAxisNum { get => horizontalAxisNum; set => horizontalAxisNum = value; }
        public int VerticalAxisNum { get => verticalAxisNum; set => verticalAxisNum = value; }
        public int OffsetX { get => offsetX; set => offsetX = value; }
        public int OffsetY { get => offsetY; set => offsetY = value; }
        public double LimitUpperX { get => limitUpperX; set => limitUpperX = value; }
        public double LimitLowerX { get => limitLowerX; set => limitLowerX = value; }
        public double LimitUpperLeftY { get => limitUpperLeftY; set => limitUpperLeftY = value; }
        public double LimitLowerLeftY { get => limitLowerLeftY; set => limitLowerLeftY = value; }
        public double LimitUpperRightY { get => limitUpperRightY; set => limitUpperRightY = value; }
        public double LimitLowerRightY { get => limitLowerRightY; set => limitLowerRightY = value; }
        public int LineNumbers
        {
            get => lineNumbers;
            set
            {
                lineNumbers = value;
                Data.Clear();
                for (int i = 0; i < lineNumbers; i++)
                {
                    Data.Add(new List<double>());
                }
            }
        }
        public List<List<double>> Data { get => data; set => data = value; }

        #endregion

        /// <summary>
        /// 初始化picture信息
        /// </summary>
        public DrawPicture()
        {
        }

        /// <summary>
        /// 初始化picture信息
        /// </summary>
        /// <param name="height">picture高度</param>
        /// <param name="width">picture宽度</param>
        public DrawPicture(int height, int width)
            : this(height, width, BackgroundImageType.OnlyXAxis, 1)
        {
        }

        /// <summary>
        /// 初始化picture信息
        /// </summary>
        /// <param name="height">picture高度</param>
        /// <param name="width">picture宽度</param>
        /// <param name="imageType">轴类型选择</param>
        public DrawPicture(int height, int width, BackgroundImageType imageType)
            : this(height, width, imageType, 1)
        {
        }

        /// <summary>
        /// 初始化picture信息
        /// </summary>
        /// <param name="height">picture高度</param>
        /// <param name="width">picture宽度</param>
        /// <param name="imageType">轴类型选择</param>
        /// <param name="lineNumbers">画曲线数量</param>
        public DrawPicture(int height, int width, BackgroundImageType imageType, int lineNumbers)
        {
            Height = height;
            Width = width;
            ImageType = imageType;
            LineNumbers = lineNumbers;
        }

        /// <summary>
        /// 底层画图仅有x轴
        /// </summary>
        /// <param name="g"></param>
        private void BackgroundImageTypeOne(Graphics g)
        {
            StartIdx = TextInfo;
            StopIdx = Width;

            //画横线
            if (HorizontalAxisNum > 1)
            {
                int intervalHeight = (Height - 2 * TextInfo) / (HorizontalAxisNum - 1);//间隔高度
                double intervalNumber = (LimitUpperLeftY - LimitLowerLeftY) / (HorizontalAxisNum - 1);//间隔差值

                for (int i = 0; i < HorizontalAxisNum; i++)
                {
                    g.DrawLine(mypen, new PointF(StartIdx, Height - TextInfo - i * intervalHeight), new PointF(StopIdx, Height - TextInfo - i * intervalHeight));

                    //标y轴坐标值
                    if (LimitUpperLeftY != -1 || LimitLowerLeftY != -1)
                    {
                        g.DrawString((LimitLowerLeftY + i * intervalNumber).ToString("F2").PadLeft(6, ' '), fontText, brushAxis, new PointF(0, Height - TextInfo - i * intervalHeight - 5));
                    }
                }
            }
            else if (HorizontalAxisNum == 1)
            {
                g.DrawLine(mypen, new PointF(StartIdx, Height - TextInfo), new PointF(StopIdx, Height - TextInfo));

                //标y轴坐标值
                if (LimitUpperLeftY != -1 || LimitLowerLeftY != -1)
                {
                    g.DrawString(LimitLowerLeftY.ToString("F2"), fontText, brushAxis, new PointF(0, Height - TextInfo));
                }
            }
        }

        /// <summary>
        /// 底层画图仅有xy轴
        /// </summary>
        /// <param name="g"></param>
        private void BackgroundImageTypeTwo(Graphics g)
        {
            StartIdx = TextInfo;
            StopIdx = Width - TextInfo;

            //画x轴
            g.DrawLine(mypen, new PointF(StartIdx, Height - TextInfo - OffsetY), new PointF(StopIdx, Height - TextInfo - OffsetY));
            //画y轴
            g.DrawLine(mypen, new PointF(StartIdx + OffsetX, TextInfo), new PointF(StartIdx + OffsetX, Height - TextInfo));

            //标y轴坐标值
            if (LimitUpperLeftY != -1 || LimitLowerLeftY != -1)
            {
                int intervalHeight = (Height - 2 * TextInfo - OffsetY) / (HorizontalAxisNum - 1);//间隔高度
                double intervalNumber = (LimitUpperLeftY - LimitLowerLeftY) / (HorizontalAxisNum - 1);//间隔差值
                int stringX;//坐标值x轴位置

                //计算坐标值x轴的位置
                if (StopIdx < 2 * OffsetX)
                {
                    stringX = TextInfo + OffsetX + 4;
                }
                else
                {
                    stringX = OffsetY;
                }

                //画y轴坐标值和标识
                for (int i = 0; i < HorizontalAxisNum; i++)
                {
                    g.DrawLine(mypen, new PointF(StartIdx + OffsetX - 1, (Height - 2 * TextInfo - OffsetY) * i / (HorizontalAxisNum - 1)+ TextInfo), new PointF(StartIdx + OffsetX + 1, (Height - 2 * TextInfo - OffsetY) * i / (HorizontalAxisNum - 1)+ TextInfo));
                    g.DrawString((LimitLowerLeftY + i * intervalNumber).ToString("F2").PadLeft(6, ' '), fontText, brushAxis, new PointF(stringX, Height - TextInfo - i * intervalHeight - 5));
                }
            }
        }

        /// <summary>
        /// 底层画图有两个y轴一个x轴
        /// </summary>
        /// <param name="g"></param>
        private void BackgroundImageTypeThree(Graphics g)
        {
            StartIdx = TextInfo;
            StopIdx = Width - TextInfo;

            //画x轴
            g.DrawLine(mypen, new PointF(StartIdx, Height - TextInfo), new PointF(StopIdx, Height - TextInfo));
            //画y轴
            g.DrawLine(mypen, new PointF(StartIdx, TextInfo), new PointF(StartIdx, Height - TextInfo));
            g.DrawLine(mypen, new PointF(StopIdx, TextInfo), new PointF(StopIdx, Height - TextInfo));

            //标y轴坐标值
            if (limitUpperLeftY != -1 || limitLowerLeftY != -1)
            {
                int intervalHeight = (Height - 2 * TextInfo - OffsetY) / (HorizontalAxisNum - 1);//间隔高度
                double intervalNumberLeft = (LimitUpperLeftY - LimitLowerLeftY) / (HorizontalAxisNum - 1);//间隔差值
                double intervalNumberRight = (LimitUpperRightY - LimitUpperRightY) / (HorizontalAxisNum - 1);//间隔差值

                //画y轴坐标值和标识
                for (int i = 0; i < HorizontalAxisNum; i++)
                {
                    mypen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;      //画虚线
                    g.DrawLine(mypen, new PointF(StartIdx, Height - TextInfo - i * intervalHeight), new PointF(StopIdx, Height - TextInfo - i * intervalHeight));//画背景网格
                    g.DrawString((LimitLowerLeftY + i * intervalNumberLeft).ToString("F2").PadLeft(6, ' '), fontText, brushAxis, new PointF(0, Height - TextInfo - i * intervalHeight - 5));//标y轴坐标值
                    g.DrawString((LimitLowerRightY + i * intervalNumberRight).ToString("F2").PadLeft(6, ' '), fontText, brushAxis, new PointF(StopIdx, Height - TextInfo - i * intervalHeight - 5));//标y轴坐标值
                }
            }
        }

        /// <summary>
        /// 画底层
        /// </summary>
        /// <returns></returns>
        public Bitmap GetBackgroundImage()
        {
            //层图
            Bitmap BGimg = new Bitmap(Width, Height);

            //绘制
            Graphics g = Graphics.FromImage(BGimg);

            //填充白色
            g.FillRectangle(Brushes.White, 0, 0, Width, Height);

            switch (ImageType)
            {
                case BackgroundImageType.OnlyXAxis:
                    BackgroundImageTypeOne(g);
                    break;
                case BackgroundImageType.OnlyXYAxis:
                    BackgroundImageTypeTwo(g);
                    break;
                case BackgroundImageType.OneXTwoYAxis:
                    BackgroundImageTypeThree(g);
                    break;
            }
            g.Dispose();
            GC.Collect();
            return BGimg;
        }

        /// <summary>
        /// 轴模式一计算坐标
        /// </summary>
        /// <param name="nums"></param>
        private void DataToPointFTypeOne()
        {
            //间隔高度
            double interval = (Height - 2 * TextInfo) / (LimitUpperLeftY - LimitLowerLeftY);

            //将数据转换为坐标
            for (int j = 0; j < Data.Count; j++)
            {
                //初始化一个空的list存储点坐标
                List<PointF> point1 = new List<PointF>();

                //计算起始坐标数；超过图形范围不显示
                int index = Data[j].Count - (StopIdx - StartIdx);
                index = index < 0 ? 0 : index;

                for (int i = index; i < Data[j].Count; i++)
                {
                    point1.Add(new PointF(TextInfo + i - index, (float)(Height - TextInfo - interval * (Data[j][i] - LimitLowerLeftY))));
                }

                points.Add(point1);
            }
        }

        /// <summary>
        /// 轴模式二计算坐标
        /// </summary>
        /// <param name="nums"></param>
        private void DataToPointFTypeTwo()
        {
            int px;     //求x坐标
            int py;     //求x坐标

            //间隔高度
            double interval = (Height - 2 * TextInfo - OffsetY) / (LimitUpperLeftY - LimitLowerLeftY);

            //将数据转换为坐标
            for (int j = 0; j < Data.Count; j++)
            {
                //初始化一个空的list存储点坐标
                List<PointF> point1 = new List<PointF>();

                //计算起始坐标数
                int index = 0;

                //数据量 < 图像宽度总像素（无需压缩曲线）
                if (Data[j].Count < StopIdx - StartIdx)
                {
                    for (int i = index; i < Data[j].Count; i++)
                    {
                        px = StartIdx + OffsetX + i - index;
                        py = (int)(Height - TextInfo - interval * (Data[j][i] - LimitLowerLeftY));
                        point1.Add(new Point(px, py));
                    }

                }
                //数据量 > 图像宽度总像素（压缩曲线）
                else
                {
                    for (int i = index; i < Data[j].Count; i++)
                    {
                        //坐标x压缩原则:  数量 / 像素和 = index / px
                        px = (StartIdx + OffsetX) + (i - index) * (StopIdx - StartIdx) / Data[j].Count;
                        py = (int)(Height - TextInfo - interval * (Data[j][i] - LimitLowerLeftY));
                        point1.Add(new Point(px, py));
                    }
                }

                points.Add(point1);
            }
        }

        /// <summary>
        /// 轴模式三计算坐标
        /// </summary>
        /// <param name="nums"></param>
        private void DataToPointFTypeThree()
        {
            //间隔高度
            double intervalLeft = (Height - 2 * TextInfo - OffsetY) / (LimitUpperLeftY - LimitLowerLeftY);
            double intervalRight = (Height - 2 * TextInfo - OffsetY) / (LimitUpperRightY - LimitLowerRightY);

            //将数据转换为坐标
            for (int j = 0; j < Data.Count; j++)
            {
                //初始化一个空的list存储点坐标
                List<PointF> point1 = new List<PointF>();

                //计算起始坐标数；超过图形范围不显示
                int index = Data[j].Count - (StopIdx - StartIdx);
                index = index < 0 ? 0 : index;

                for (int i = index; i < Data[j].Count; i++)
                {
                    point1.Add(new PointF(StartIdx + OffsetX + i - index, (float)(Height - TextInfo - intervalLeft * (Data[j][i] - LimitLowerLeftY))));
                }

                points.Add(point1);
            }
        }

        /// <summary>
        /// 计算坐标点
        /// </summary>
        private void DataToPointF(params double[] datas)
        {
            //添加数据
            for (int i = 0; i < Data.Count; i++)
            {
                for (int j = 0; j < datas.Length; j++)
                {
                    Data[i].Add(datas[j]);
                }
            }

            //清空点位
            points.Clear();

            //计算坐标点
            switch (ImageType)
            {
                case BackgroundImageType.OnlyXAxis:
                    DataToPointFTypeOne();
                    break;
                case BackgroundImageType.OnlyXYAxis:
                    DataToPointFTypeTwo();
                    break;
                case BackgroundImageType.OneXTwoYAxis:
                    DataToPointFTypeThree();
                    break;
            }
        }

        /// <summary>
        /// 画上层
        /// </summary>
        /// <returns></returns>
        public Bitmap GetForegroundImage(params double[] datas)
        {
            //层图
            Bitmap img = new Bitmap(Width, Height);

            //绘制
            Graphics g = Graphics.FromImage(img);

            //设置曲线呈现质量（此模式可以避免画直线出现轻微折痕现象）
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            //画网格线
            DrawPictureGrid(g);

            //计算点位
            DataToPointF(datas);

            //画图
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].Count < 2) continue;
                g.DrawCurve(new Pen(Color.Blue, 1.0f), points[i].ToArray(), 0);
            }

            g.Dispose();

            return img;
        }

        /// <summary>
        /// 画上层(提供x坐标和y坐标)
        /// </summary>
        /// <returns></returns>
        public Bitmap GetForegroundImage_Two(double[] xDatas, double[] yDatas)
        {
            //层图
            Bitmap img = new Bitmap(Width, Height);

            //绘制
            Graphics g = Graphics.FromImage(img);

            //设置曲线呈现质量（此模式可以避免画直线出现轻微折痕现象）
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            //画网格线
            DrawPictureGrid(g);

            //清除历史数据
            points.Clear();

            //间隔高度
            double interval = (Height - 2 * TextInfo - OffsetY) / (LimitUpperLeftY - LimitLowerLeftY);

            //设置坐标集合

            List<PointF> point1 = new List<PointF>();

            //将数据转换为坐标
            int Cnt = xDatas.Length;
            double xMin = Math.Abs(xDatas.Min()) > xDatas.Max() ? Math.Abs(xDatas.Min()) : xDatas.Max();

            for (int i = 0; i < Cnt; i++)
            {
                point1.Add(new PointF((float)(TextInfo + xMin + xDatas[i]), (float)(Height - TextInfo - interval * (yDatas[i] - LimitLowerLeftY))));

            }
            points.Add(point1);

            //画图
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].Count < 2) continue;
                g.DrawCurve(new Pen(Color.Blue, 1.0f), points[i].ToArray(), 0);
            }

            g.Dispose();

            return img;
        }
        
        /// <summary>
        /// 画网格线
        /// </summary>
        /// <param name="g"></param>
        private void DrawPictureGrid(Graphics g)
        {
            try
            {
                int x1 = 0;      //Y轴网格线横坐标
                int y1 = 0;      //X轴网格线纵坐标
                int gridnum = 0; //网格线数量

                //画X轴网格线
                gridnum = (HorizontalAxisNum - 1) * 2;
                for (int i = 0; i <= gridnum; i++)
                {
                    y1 = (int)((Height - 2 * TextInfo - OffsetY) * i / gridnum * 1.0) + TextInfo;

                    mypen = new Pen(Color.Gainsboro, 1.00f);   //定义背景网格画笔
                    mypen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;      //画虚线

                    //间隔4条画实线
                    if (i % 4 == 0)
                    {
                        mypen = new Pen(Color.DarkGray, 1.00f);   //定义背景网格画笔
                        mypen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;      //画实线
                    }

                    g.DrawLine(mypen, new Point(StartIdx, y1), new Point(StopIdx, y1));      //画X轴网格
                }


                //画Y轴网格线
                gridnum = 5;
                for (int i = 0; i <= gridnum; i++)
                {
                    x1 = (int)((StopIdx - StartIdx) * i / gridnum * 1.0) + TextInfo;

                    mypen = new Pen(Color.Gainsboro, 1.00f);   //定义背景网格画笔
                    mypen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;      //画虚线

                    //间隔5条画实线
                    if (i % 5 == 0)
                    {
                        mypen = new Pen(Color.DarkGray, 1.00f);   //定义背景网格画笔
                        mypen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;      //画实线
                    }

                    g.DrawLine(mypen, new PointF(x1, TextInfo), new PointF(x1, Height - TextInfo));      //画Y轴网格线
                }
            }
            catch (Exception ex)
            {
                //捕获异常
            }

        }

        #region 画多设备曲线 Ricardo

        //画上层(提供y坐标)—— 多设备
        public Bitmap GetForegroundImageFromDevs(params double[][] datas)
        {
            //层图
            Bitmap img = new Bitmap(Width, Height);

            //绘制
            Graphics g = Graphics.FromImage(img);

            //设置曲线呈现质量（此模式可以避免画直线出现轻微折痕现象）
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            //画网格线
            DrawPictureGrid(g);

            // 遍历每个数组，并分别绘制曲线
            for (int i = 0; i < datas.Length; i++)
            {
                // 计算点位
                List<PointF> points = GetDataToPointF(datas[i], imageType);

                // 画图
                if (points.Count >= 2)
                {
                    g.DrawCurve(new Pen(drawLines[(i + 3) % drawLines.Length], 1.0f), points.ToArray(), 0);//i + 3是为了从颜色blue开始
                }
            }

            g.Dispose();

            return img;
        }

        //画上层(提供x坐标和y坐标)
        public Bitmap GetForegroundImage_TwoFromDevs(params Tuple<double[], double[]>[] curves)
        {
            //层图
            Bitmap img = new Bitmap(Width, Height);

            //绘制
            Graphics g = Graphics.FromImage(img);

            //设置曲线呈现质量（此模式可以避免画直线出现轻微折痕现象）
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            //画网格线
            DrawPictureGrid(g);

            //遍历
            foreach (var curve in curves)
            {
                double[] xDatas = curve.Item1;
                double[] yDatas = curve.Item2;

                // 计算间隔高度
                double interval = (Height - 2 * TextInfo - OffsetY) / (LimitUpperLeftY - LimitLowerLeftY);

                // 设置坐标集合
                List<PointF> point1 = new List<PointF>();

                // 将数据转换为坐标
                int Cnt = xDatas.Length;
                double xMin = Math.Abs(xDatas.Min()) > xDatas.Max() ? Math.Abs(xDatas.Min()) : xDatas.Max();

                for (int i = 0; i < Cnt; i++)
                {
                    point1.Add(new PointF(
                        (float)(TextInfo + xMin + xDatas[i]),
                        (float)(Height - TextInfo - interval * (yDatas[i] - LimitLowerLeftY))
                    ));
                }

                // 将当前曲线的点集合添加到points列表中
                points.Add(point1);
            }

            // 绘制所有曲线
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].Count < 2) continue;
                g.DrawCurve(new Pen(drawLines[(i + 3) % drawLines.Length], 1.0f), points[i].ToArray(), 0);
            }

            g.Dispose();

            return img;
        }
        
        //获取点位
        public List<PointF> GetDataToPointF(double[] data, BackgroundImageType targetImageType)
        {
            List<PointF> points = new List<PointF>();

            //计算坐标点
            switch (targetImageType)
            {
                case BackgroundImageType.OnlyXAxis:
                    GetDataToPointFTypeOne(data);
                    break;
                case BackgroundImageType.OnlyXYAxis:
                    GetDataToPointFTypeTwo(data);
                    break;
                case BackgroundImageType.OneXTwoYAxis:
                    GetDataToPointFTypeThree(data);
                    break;
            }

            return points;
        }

        //轴模式一计算坐标
        public List<PointF> GetDataToPointFTypeOne(double[] data)
        {
            //初始化一个空的list存储点坐标
            List<PointF> pointsTypeOne = new List<PointF>();

            //间隔高度
            double interval = (Height - 2 * TextInfo) / (LimitUpperLeftY - LimitLowerLeftY);

            //计算起始坐标数；超过图形范围不显示
            int index = data.Length - (StopIdx - StartIdx);
            index = index < 0 ? 0 : index;

            for (int i = index; i < data.Length; i++)
            {
                pointsTypeOne.Add(new PointF(TextInfo + i - index, (float)(Height - TextInfo - interval * (data[i] - LimitLowerLeftY))));
            }

            return pointsTypeOne;
        }

        //轴模式二计算坐标
        public List<PointF> GetDataToPointFTypeTwo(double[] data)
        {
            //初始化一个空的list存储点坐标
            List<PointF> pointsTypeTwo = new List<PointF>();

            int px;     //求x坐标
            int py;     //求x坐标

            //间隔高度
            double interval = (Height - 2 * TextInfo - OffsetY) / (LimitUpperLeftY - LimitLowerLeftY);

            //计算起始坐标数
            int index = 0;

            //数据量 < 图像宽度总像素（无需压缩曲线）
            if (data.Length < StopIdx - StartIdx)
            {
                for (int i = index; i < data.Length; i++)
                {
                    px = StartIdx + OffsetX + i - index;
                    py = (int)(Height - TextInfo - interval * (data[i] - LimitLowerLeftY));
                    pointsTypeTwo.Add(new Point(px, py));
                }

            }
            //数据量 > 图像宽度总像素（压缩曲线）
            else
            {
                for (int i = index; i < data.Length; i++)
                {
                    //坐标x压缩原则:  数量 / 像素和 = index / px
                    px = (StartIdx + OffsetX) + (i - index) * (StopIdx - StartIdx) / data.Length;
                    py = (int)(Height - TextInfo - interval * (data[i] - LimitLowerLeftY));
                    pointsTypeTwo.Add(new Point(px, py));
                }
            }

            return pointsTypeTwo;
        }

        //轴模式三计算坐标
        public List<PointF> GetDataToPointFTypeThree(double[] data)
        {
            //初始化一个空的list存储点坐标
            List<PointF> pointsTypeThree = new List<PointF>();

            //间隔高度
            double intervalLeft = (Height - 2 * TextInfo - OffsetY) / (LimitUpperLeftY - LimitLowerLeftY);
            double intervalRight = (Height - 2 * TextInfo - OffsetY) / (LimitUpperRightY - LimitLowerRightY);

            //计算起始坐标数；超过图形范围不显示
            int index = data.Length - (StopIdx - StartIdx);
            index = index < 0 ? 0 : index;

            for (int i = index; i < data.Length; i++)
            {
                pointsTypeThree.Add(new PointF(StartIdx + OffsetX + i - index, (float)(Height - TextInfo - intervalLeft * (data[i] - LimitLowerLeftY))));
            }

            return pointsTypeThree;
        }

        #endregion

    }
}
