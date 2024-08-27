using SqlSugar;

//数据库 —— 扳手总表
//用于管理扳手固定信息DEVC和wlan、alam方案id

namespace DBHelper
{
    /// <summary>
    /// 扳手设备信息
    /// </summary>
    [SugarTable("wrench_devc", "扳手DEVC")]
    public class DSWrenchDevc
    {

        #region 属性
        /// <summary>
        /// 扳手id
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "wid", ColumnDescription = "扳手id")]
        public uint Wid { get; set; }

        /// <summary>
        /// WLAN方案ID
        /// </summary>
        [SugarColumn(ColumnName = "wlan_id", ColumnDescription = "WLAN方案ID")]
        public uint WlanId { get; set; }

        /// <summary>
        /// ALAM方案ID
        /// </summary>
        [SugarColumn(ColumnName = "alam_id", ColumnDescription = "ALAM方案ID")]
        public uint AlamId { get; set; }

        /// <summary>
        /// 系列
        /// </summary>
        [SugarColumn(ColumnName = "series", ColumnDescription = "系列")]
        public string Series { get; set; }

        /// <summary>
        /// 型号
        /// </summary>
        [SugarColumn(ColumnName = "type", ColumnDescription = "型号")]
        public string Type { get; set; }

        /// <summary>
        /// 程序版本
        /// </summary>
        [SugarColumn(ColumnName = "version", ColumnDescription = "程序版本")]
        public byte Version { get; set; }

        /// <summary>
        /// 唯一序列号
        /// </summary>
        [SugarColumn(ColumnName = "bohrcode", ColumnDescription = "唯一序列号")]
        public ulong BohrCode { get; set; }

        /// <summary>
        /// 标定的单位
        /// </summary>
        [SugarColumn(ColumnName = "unit", ColumnDescription = "标定的单位")]
        public string Unit { get; set; }

        /// <summary>
        /// 扭矩小数点
        /// </summary>
        [SugarColumn(ColumnName = "torque_decimal", ColumnDescription = "扭矩小数点")]
        public byte TorqueDecimal { get; set; }

        /// <summary>
        /// 扭矩分度值
        /// </summary>
        [SugarColumn(ColumnName = "torque_fdn", ColumnDescription = "扭矩分度值")]
        public byte TorqueFdn { get; set; }

        /// <summary>
        /// 标定方式
        /// </summary>
        [SugarColumn(ColumnName = "caltype", ColumnDescription = "标定方式")]
        public byte CalType { get; set; }

        /// <summary>
        /// 扭矩量程
        /// </summary>
        [SugarColumn(ColumnName = "capacity", ColumnDescription = "扭矩量程")]
        public int Capacity { get; set; }

        /// <summary>
        /// 标定零点
        /// </summary>
        [SugarColumn(ColumnName = "ad_zero", ColumnDescription = "标定零点")]
        public int AdZero { get; set; }

        /// <summary>
        /// 正向第1点内码
        /// </summary>
        [SugarColumn(ColumnName = "ad_pos_point1", ColumnDescription = "正向第1点内码")]
        public int AdPosPoint1 { get; set; }

        /// <summary>
        /// 正向第2点内码
        /// </summary>
        [SugarColumn(ColumnName = "ad_pos_point2", ColumnDescription = "正向第2点内码")]
        public int AdPosPoint2 { get; set; }

        /// <summary>
        /// 正向第3点内码
        /// </summary>
        [SugarColumn(ColumnName = "ad_pos_point3", ColumnDescription = "正向第3点内码")]
        public int AdPosPoint3 { get; set; }

        /// <summary>
        /// 正向第4点内码
        /// </summary>
        [SugarColumn(ColumnName = "ad_pos_point4", ColumnDescription = "正向第4点内码")]
        public int AdPosPoint4 { get; set; }

        /// <summary>
        /// 正向第5点内码
        /// </summary>
        [SugarColumn(ColumnName = "ad_pos_point5", ColumnDescription = "正向第5点内码")]
        public int AdPosPoint5 { get; set; }

        /// <summary>
        /// 反向第1点内码
        /// </summary>
        [SugarColumn(ColumnName = "ad_neg_point1", ColumnDescription = "反向第1点内码")]
        public int AdNegPoint1 { get; set; }

        /// <summary>
        /// 反向第2点内码
        /// </summary>
        [SugarColumn(ColumnName = "ad_neg_point2", ColumnDescription = "反向第2点内码")]
        public int AdNegPoint2 { get; set; }

        /// <summary>
        /// 反向第3点内码
        /// </summary>
        [SugarColumn(ColumnName = "ad_neg_point3", ColumnDescription = "反向第3点内码")]
        public int AdNegPoint3 { get; set; }

        /// <summary>
        /// 反向第4点内码
        /// </summary>
        [SugarColumn(ColumnName = "ad_neg_point4", ColumnDescription = "反向第4点内码")]
        public int AdNegPoint4 { get; set; }

        /// <summary>
        /// 反向第5点内码
        /// </summary>
        [SugarColumn(ColumnName = "ad_neg_point5", ColumnDescription = "反向第5点内码")]
        public int AdNegPoint5 { get; set; }

        /// <summary>
        /// 正向第1点扭矩值
        /// </summary>
        [SugarColumn(ColumnName = "tq_pos_point1", ColumnDescription = "正向第1点扭矩值")]
        public int TqPosPoint1 { get; set; }

        /// <summary>
        /// 正向第2点扭矩值
        /// </summary>
        [SugarColumn(ColumnName = "tq_pos_point2", ColumnDescription = "正向第2点扭矩值")]
        public int TqPosPoint2 { get; set; }

        /// <summary>
        /// 正向第3点扭矩值
        /// </summary>
        [SugarColumn(ColumnName = "tq_pos_point3", ColumnDescription = "正向第3点扭矩值")]
        public int TqPosPoint3 { get; set; }

        /// <summary>
        /// 正向第4点扭矩值
        /// </summary>
        [SugarColumn(ColumnName = "tq_pos_point4", ColumnDescription = "正向第4点扭矩值")]
        public int TqPosPoint4 { get; set; }

        /// <summary>
        /// 正向第5点扭矩值
        /// </summary>
        [SugarColumn(ColumnName = "tq_pos_point5", ColumnDescription = "正向第5点扭矩值")]
        public int TqPosPoint5 { get; set; }

        /// <summary>
        /// 反向第1点扭矩值
        /// </summary>
        [SugarColumn(ColumnName = "tq_neg_point1", ColumnDescription = "反向第1点扭矩值")]
        public int TqNegPoint1 { get; set; }

        /// <summary>
        /// 反向第2点扭矩值
        /// </summary>
        [SugarColumn(ColumnName = "tq_neg_point2", ColumnDescription = "反向第2点扭矩值")]
        public int TqNegPoint2 { get; set; }

        /// <summary>
        /// 反向第3点扭矩值
        /// </summary>
        [SugarColumn(ColumnName = "tq_neg_point3", ColumnDescription = "反向第3点扭矩值")]
        public int TqNegPoint3 { get; set; }

        /// <summary>
        /// 反向第4点扭矩值
        /// </summary>
        [SugarColumn(ColumnName = "tq_neg_point4", ColumnDescription = "反向第4点扭矩值")]
        public int TqNegPoint4 { get; set; }

        /// <summary>
        /// 反向第5点扭矩值
        /// </summary>
        [SugarColumn(ColumnName = "tq_neg_point5", ColumnDescription = "反向第5点扭矩值")]
        public int TqNegPoint5 { get; set; }

        /// <summary>
        /// 最小显示扭矩值
        /// </summary>
        [SugarColumn(ColumnName = "torque_disp", ColumnDescription = "最小显示扭矩值")]
        public int TorqueDisp { get; set; }

        /// <summary>
        /// 最小可调报警值
        /// </summary>
        [SugarColumn(ColumnName = "torque_min", ColumnDescription = "最小可调报警值")]
        public int TorqueMin { get; set; }

        /// <summary>
        /// 最大可调报警值
        /// </summary>
        [SugarColumn(ColumnName = "torque_max", ColumnDescription = "最大可调报警值")]
        public int TorqueMax { get; set; }

        /// <summary>
        /// 扭矩超载报警值
        /// </summary>
        [SugarColumn(ColumnName = "torque_over", ColumnDescription = "扭矩超载报警值")]
        public int TorqueOver { get; set; }

        /// <summary>
        /// 超量程使用扭矩值
        /// </summary>
        [SugarColumn(ColumnName = "torque_err", ColumnDescription = "超量程使用扭矩值")]
        public string TorqueErr { get; set; }

        /// <summary>
        /// 连接方式
        /// </summary>
        [SugarColumn(ColumnName = "connect_type", ColumnDescription = "连接方式")]
        public string ConnectType { get; set; }

        /// <summary>
        /// 允许自动连接
        /// </summary>
        [SugarColumn(ColumnName = "connect_auto", ColumnDescription = "允许自动连接")]
        public string ConnectAuto { get; set; }

        /// <summary>
        /// 备用，供后期数据库增加字段用，无需显示在软件上
        /// </summary>
        [SugarColumn(ColumnName = "reserve", ColumnDescription = "备用")]
        public string Reserve { get; set; }

        #endregion

        public DSWrenchDevc()
        {
        }

    }
}
