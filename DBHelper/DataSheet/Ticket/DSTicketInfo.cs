using SqlSugar;

//数据库 —— 工单表
//用于管理工单信息

namespace DBHelper
{
    /// <summary>
    /// 工单表
    /// </summary>
    [SugarTable("ticket_info", "工单表")]
    public class DSTicketInfo
    {
        #region 属性
        /// <summary>
        /// 工单id
        /// </summary>
        [SugarColumn(ColumnName = "work_id", IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "工单id")]
        public uint WorkId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [SugarColumn(ColumnName = "time", ColumnDescription = "创建时间")]
        public string Time { get; set; }

        /// <summary>
        /// 图片路径
        /// </summary>
        [SugarColumn(ColumnName = "image_path", ColumnDescription = "图片路径")]
        public string ImagePath { get; set; }

        /// <summary>
        /// 工单区
        /// </summary>
        [SugarColumn(ColumnName = "wo_area", ColumnDescription = "工单区")]
        public string WoArea { get; set; }

        /// <summary>
        /// 工单厂
        /// </summary>
        [SugarColumn(ColumnName = "wo_factory", ColumnDescription = "工单厂")]
        public string WoFactory { get; set; }

        /// <summary>
        /// 工单产线
        /// </summary>
        [SugarColumn(ColumnName = "wo_line", ColumnDescription = "工单产线")]
        public string WoLine { get; set; }

        /// <summary>
        /// 工单工位
        /// </summary>
        [SugarColumn(ColumnName = "wo_station", ColumnDescription = "工单工位")]
        public string WoStation { get; set; }

        /// <summary>
        /// 工单批号
        /// </summary>
        [SugarColumn(ColumnName = "wo_bat", ColumnDescription = "工单批号")]
        public string WoBat { get; set; }

        /// <summary>
        /// 工单编号
        /// </summary>
        [SugarColumn(ColumnName = "wo_num", ColumnDescription = "工单编号")]
        public string WoNum { get; set; }

        /// <summary>
        /// 工单时间标识
        /// </summary>
        [SugarColumn(ColumnName = "wo_stamp", ColumnDescription = "工单时间标识")]
        public string WoStamp { get; set; }

        /// <summary>
        /// 工单名称
        /// </summary>
        [SugarColumn(ColumnName = "wo_name", ColumnDescription = "工单名称")]
        public string WoName { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(ColumnName = "note", ColumnDescription = "备注")]
        public string Note { get; set; }

        /// <summary>
        /// 备用，供后期数据库增加字段用，无需显示在软件上
        /// </summary>
        [SugarColumn(ColumnName = "reserve", ColumnDescription = "备用")]
        public string Reserve { get; set; }

        #endregion

        public DSTicketInfo()
        {

        }
    }
}
