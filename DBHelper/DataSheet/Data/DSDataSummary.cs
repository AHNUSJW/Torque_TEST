using SqlSugar;

//数据库 —— 数据组表
//用于按组管理数据的信息

namespace DBHelper
{
    /// <summary>
    /// 数据组表
    /// </summary>
    [SugarTable("data_summary", "数据组表")]
    public class DSDataSummary
    {
        #region 属性
        /// <summary>
        /// 组号
        /// </summary>
        [SugarColumn(ColumnName = "data_id", IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "组号")]
        public int DataId { get; set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        [SugarColumn(ColumnName = "data_type", ColumnDescription = "数据类型")]
        public string DataType { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [SugarColumn(ColumnName = "create_time", ColumnDescription = "创建时间")]
        public string CreateTime { get; set; }

        /// <summary>
        /// 工单唯一识别字符
        /// </summary>
        [SugarColumn(ColumnName = "work_id", ColumnDescription = "工单唯一识别字符")]
        public uint WorkId { get; set; }

        /// <summary>
        /// 工单名称
        /// </summary>
        [SugarColumn(ColumnName = "work_num", ColumnDescription = "工单名称")]
        public string WorkNum { get; set; }

        /// <summary>
        /// 序列号
        /// </summary>
        [SugarColumn(ColumnName = "sequence_id", ColumnDescription = "序列号")]
        public string SequenceId { get; set; }

        /// <summary>
        /// 备用
        /// </summary>
        [SugarColumn(ColumnName = "reserve", ColumnDescription = "备用")]
        public string Reserve { get; set; }

        #endregion

        public DSDataSummary()
        {
        }
    }
}
