using SqlSugar;

//数据库 —— 扳手WLAN方案
//用于管理扳手WLAN信息

namespace DBHelper
{
    /// <summary>
    /// WLAN方案
    /// </summary>
    [SugarTable("wrench_wlan", "扳手WLAN方案")]
    public class DSWrenchWlan
    {
        #region 属性
        /// <summary>
        /// WLAN方案ID
        /// </summary>
        [SugarColumn(ColumnName = "wlan_id", IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "WLAN方案ID")]
        public uint WlanId { get; set; }

        /// <summary>
        /// 方案名称
        /// </summary>
        [SugarColumn(ColumnName = "name", ColumnDescription = "方案名称")]
        public string Name { get; set; }

        /// <summary>
        /// 站点地址
        /// </summary>
        [SugarColumn(ColumnName = "addr", ColumnDescription = "站点地址", ColumnDataType = "tinyint unsigned")]
        public byte Addr { get; set; }

        /// <summary>
        /// RF信道
        /// </summary>
        [SugarColumn(ColumnName = "rf_chan", ColumnDescription = "RF信道", ColumnDataType = "tinyint unsigned")]
        public byte RfChan { get; set; }

        /// <summary>
        /// 设置透传,发射功率
        /// </summary>
        [SugarColumn(ColumnName = "rf_option", ColumnDescription = "设置透传,发射功率", ColumnDataType = "tinyint unsigned")]
        public byte RfOption { get; set; }

        /// <summary>
        /// RF参数,校验位,波特率,空中速率
        /// </summary>
        [SugarColumn(ColumnName = "rf_para", ColumnDescription = "RF参数,校验位,波特率,空中速率", ColumnDataType = "tinyint unsigned")]
        public byte RfPara { get; set; }

        /// <summary>
        /// 波特率
        /// </summary>
        [SugarColumn(ColumnName = "baud", ColumnDescription = "波特率", ColumnDataType = "tinyint unsigned")]
        public byte Baud { get; set; }

        /// <summary>
        /// 停止位
        /// </summary>
        [SugarColumn(ColumnName = "stopbit", ColumnDescription = "停止位", ColumnDataType = "tinyint unsigned")]
        public byte Stopbit { get; set; }

        /// <summary>
        /// 校验位
        /// </summary>
        [SugarColumn(ColumnName = "parity", ColumnDescription = "校验位", ColumnDataType = "tinyint unsigned")]
        public byte Parity { get; set; }

        /// <summary>
        /// wifi/RF无线
        /// </summary>
        [SugarColumn(ColumnName = "wifimode", ColumnDescription = "wifi/RF无线")]
        public byte WifiMode { get; set; }

        /// <summary>
        /// WiFi账号
        /// </summary>
        [SugarColumn(ColumnName = "wf_ssid", ColumnDescription = "WiFi账号")]
        public string WFSsid { get; set; }

        /// <summary>
        /// WiFi密码
        /// </summary>
        [SugarColumn(ColumnName = "wf_pwd", ColumnDescription = "WiFi密码")]
        public string WFPwd { get; set; }

        /// <summary>
        /// 网络IP
        /// </summary>
        [SugarColumn(ColumnName = "wf_ip", ColumnDescription = "网络IP")]
        public string WFIp { get; set; }

        /// <summary>
        /// 网络端口号
        /// </summary>
        [SugarColumn(ColumnName = "wf_port", ColumnDescription = "网络端口号")]
        public ushort WFPort { get; set; }

        /// <summary>
        /// 备用，供后期数据库增加字段用，无需显示在软件上
        /// </summary>
        [SugarColumn(ColumnName = "reserve", ColumnDescription = "备用")]
        public string Reserve { get; set; }
    
        #endregion

        public DSWrenchWlan()
        {
        }
    }
}
