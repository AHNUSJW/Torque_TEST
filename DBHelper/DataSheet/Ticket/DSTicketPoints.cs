using SqlSugar;

//数据库 —— 工单点位表
//用于管理各个工单的点位信息

namespace DBHelper
{
    /// <summary>
    /// 工单点位表
    /// </summary>
    [SugarTable("ticket_points", "工单点位表")]
    public class DSTicketPoints
    {
        #region 属性
        /// <summary>
        /// 点位id
        /// </summary>
        [SugarColumn(ColumnName = "point_id", IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "点位id")]
        public uint PointId { get; set; }

        /// <summary>
        /// 点位编号，点位在图片上的编号
        /// </summary>
        [SugarColumn(ColumnName = "point_number", ColumnDescription = "点位编号，点位在图片上的编号")]
        public string PointNumber { get; set; }

        /// <summary>
        /// 工单id
        /// </summary>
        [SugarColumn(ColumnName = "work_id", IsPrimaryKey = true, ColumnDescription = "工单id")]
        public uint WorkId { get; set; }

        /// <summary>
        /// 图位信息
        /// </summary>
        [SugarColumn(ColumnName = "point_position", ColumnDescription = "图位信息")]
        public string PointPosition { get; set; }

        /// <summary>
        /// 螺丝id
        /// </summary>
        [SugarColumn(ColumnName = "screws_id", ColumnDescription = "螺丝id")]
        public uint ScrewsId { get; set; }

        /// <summary>
        /// 螺丝数量
        /// </summary>
        [SugarColumn(ColumnName = "screw_num", ColumnDescription = "螺丝数量", ColumnDataType = "tinyint unsigned ")]
        public byte ScrewNum { get; set; }

        /// <summary>
        /// 螺丝顺序
        /// </summary>
        [SugarColumn(ColumnName = "screw_seq", ColumnDescription = "螺丝顺序", ColumnDataType = "tinyint unsigned ")]
        public byte ScrewSeq { get; set; }

        /// <summary>
        /// 拧紧结果
        /// </summary>
        [SugarColumn(ColumnName = "result", ColumnDescription = "拧紧结果", Length = 10)]
        public string Result { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(ColumnName = "note", ColumnDescription = "备注")]
        public string Note { get; set; }

        #endregion

        public DSTicketPoints()
        {
        }
    }
}
