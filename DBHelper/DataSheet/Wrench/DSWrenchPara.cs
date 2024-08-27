using SqlSugar;

//数据库 —— 扳手参数PARA
//用于管理扳手参数信息PARA

namespace DBHelper
{
    /// <summary>
    /// 扳手设备信息
    /// </summary>
    [SugarTable("wrench_para", "扳手PARA")]
    public class DSWrenchPara
    {
        #region 属性
        /// <summary>
        /// 扳手id
        /// </summary>
        [SugarColumn(ColumnName = "wid", IsPrimaryKey = true, ColumnDescription = "扳手id")]
        public uint Wid { get; set; }

        /// <summary>
        /// 扭矩单位
        /// </summary>
        [SugarColumn(ColumnName = "torque_unit", ColumnDescription = "扭矩单位")]
        public string TorqueUnit { get; set; }

        /// <summary>
        /// 角度挡位
        /// </summary>
        [SugarColumn(ColumnName = "angle_speed", ColumnDescription = "角度挡位")]
        public byte AngleSpeed { get; set; }

        /// <summary>
        /// 角度小数点
        /// </summary>
        [SugarColumn(ColumnName = "angle_decimal", ColumnDescription = "角度小数点")]
        public byte AngleDecimal { get; set; }

        /// <summary>
        /// 模式modePt
        /// </summary>
        [SugarColumn(ColumnName = "mode_pt", ColumnDescription = "模式modePt")]
        public byte ModePt { get; set; }

        /// <summary>
        /// 模式modeAx
        /// </summary>
        [SugarColumn(ColumnName = "mode_ax", ColumnDescription = "模式modeAx")]
        public byte ModeAx { get; set; }

        /// <summary>
        /// 模式modeMx
        /// </summary>
        [SugarColumn(ColumnName = "mode_mx", ColumnDescription = "模式modeMx")]
        public byte ModeMx { get; set; }

        /// <summary>
        /// 缓存满覆盖
        /// </summary>
        [SugarColumn(ColumnName = "fifomode", ColumnDescription = "缓存满覆盖")]
        public byte FifoMode { get; set; }

        /// <summary>
        /// 缓存模式
        /// </summary>
        [SugarColumn(ColumnName = "fiforec", ColumnDescription = "缓存模式")]
        public byte FifoRec { get; set; }

        /// <summary>
        /// 缓存速率
        /// </summary>
        [SugarColumn(ColumnName = "fifospeed", ColumnDescription = "缓存速率")]
        public byte FifoSpeed { get; set; }

        /// <summary>
        /// 心跳回复帧数
        /// </summary>
        [SugarColumn(ColumnName = "heartcount", ColumnDescription = "心跳回复帧数")]
        public byte HeartCount { get; set; }

        /// <summary>
        /// 心跳间隔
        /// </summary>
        [SugarColumn(ColumnName = "heartcycle", ColumnDescription = "心跳间隔")]
        public ushort HeartCycle { get; set; }

        /// <summary>
        /// 角度累加
        /// </summary>
        [SugarColumn(ColumnName = "accmode", ColumnDescription = "角度累加")]
        public byte AccMode { get; set; }

        /// <summary>
        /// 声光报警
        /// </summary>
        [SugarColumn(ColumnName = "alarmode", ColumnDescription = "声光报警")]
        public byte AlarmMode { get; set; }

        /// <summary>
        /// wifi/RF无线
        /// </summary>
        [SugarColumn(ColumnName = "wifimode", ColumnDescription = "wifi/RF无线")]
        public byte WifiMode { get; set; }

        /// <summary>
        /// 自动关机时间
        /// </summary>
        [SugarColumn(ColumnName = "timeoff", ColumnDescription = "自动关机时间")]
        public byte TimeOff { get; set; }

        /// <summary>
        /// 自动关背光时间
        /// </summary>
        [SugarColumn(ColumnName = "timeback", ColumnDescription = "自动关背光时间")]
        public byte TimeBack { get; set; }

        /// <summary>
        /// 自动归零时间
        /// </summary>
        [SugarColumn(ColumnName = "timezero", ColumnDescription = "自动归零时间")]
        public byte TimeZero { get; set; }

        /// <summary>
        /// 横屏竖屏
        /// </summary>
        [SugarColumn(ColumnName = "disptype", ColumnDescription = "横屏竖屏")]
        public byte DispType { get; set; }

        /// <summary>
        /// 主题
        /// </summary>
        [SugarColumn(ColumnName = "disptheme", ColumnDescription = "主题")]
        public byte DispTheme { get; set; }

        /// <summary>
        /// 语言
        /// </summary>
        [SugarColumn(ColumnName = "displan", ColumnDescription = "语言")]
        public byte DispLan { get; set; }

        /// <summary>
        /// 脱钩时间
        /// </summary>
        [SugarColumn(ColumnName = "unhook", ColumnDescription = "脱钩时间")]
        public ushort Unhook { get; set; }

        /// <summary>
        /// 角度修正系数
        /// </summary>
        [SugarColumn(ColumnName = "angcorr", ColumnDescription = "角度修正系数")]
        public string AngCorr { get; set; }

        /// <summary>
        /// adc采样速率和增益
        /// </summary>
        [SugarColumn(ColumnName = "adspeed", ColumnDescription = "adc采样速率和增益")]
        public byte AdSpeed { get; set; }

        /// <summary>
        /// 归零范围
        /// </summary>
        [SugarColumn(ColumnName = "autozero", ColumnDescription = "归零范围")]
        public string AutoZero { get; set; }

        /// <summary>
        /// 零点跟踪
        /// </summary>
        [SugarColumn(ColumnName = "trackzero", ColumnDescription = "零点跟踪")]
        public string TrackZero { get; set; }

        /// <summary>
        /// 备用，供后期数据库增加字段用，无需显示在软件上
        /// </summary>
        [SugarColumn(ColumnName = "reserve", ColumnDescription = "备用")]
        public string Reserve { get; set; }

        #endregion

        public DSWrenchPara()
        {
        }
    }
}
