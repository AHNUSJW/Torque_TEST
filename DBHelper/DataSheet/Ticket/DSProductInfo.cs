using SqlSugar;

//数据库 —— 产品表
//用于管理各个产品

namespace DBHelper
{
    /// <summary>
    /// 工单点位表
    /// </summary>
    [SugarTable("product_info", "产品表")]
    public class DSProductInfo
    {
        #region 属性
        /// <summary>
        /// 点位id
        /// </summary>
        [SugarColumn(ColumnName = "product_id", IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "产品id")]
        public uint ProductId { get; set; }

        /// <summary>
        /// 工单id
        /// </summary>
        [SugarColumn(ColumnName = "work_id", IsPrimaryKey = true, ColumnDescription = "工单id")]
        public uint WorkId { get; set; }

        /// <summary>
        /// 工单编号
        /// </summary>
        [SugarColumn(ColumnName = "work_num", ColumnDescription = "工单编号")]
        public string WorkNum { get; set; }

        /// <summary>
        /// 图位信息
        /// </summary>
        [SugarColumn(ColumnName = "sequence_id", IsPrimaryKey = true, ColumnDescription = "序列号")]
        public string SequenceId { get; set; }

        /// <summary>
        /// 工单图片路径
        /// </summary>
        [SugarColumn(ColumnName = "image_path", ColumnDescription = "工单图片路径")]
        public string ImagePath { get; set; }

        /// <summary>
        /// 备用，供后期数据库增加字段用，无需显示在软件上
        /// </summary>
        [SugarColumn(ColumnName = "reserve", ColumnDescription = "备用")]
        public string Reserve { get; set; }

        #endregion

        public DSProductInfo()
        {
        }
    }
}
