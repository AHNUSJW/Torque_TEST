using SqlSugar;

//数据库 —— 数据组表
//用于按组管理数据的信息

namespace DBHelper
{
    /// <summary>
    /// 数据组表
    /// </summary>
    [SugarTable("data_group_info", "数据组表")]
    public class DSDataGroup
    {
        #region 属性
        /// <summary>
        /// 组号
        /// </summary>
        [SugarColumn(ColumnName = "group_id", IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "组号")]
        public uint GroupId { get; set; }

        /// <summary>
        /// 流水号
        /// </summary>
        [SugarColumn(ColumnName = "vin_id", ColumnDescription = "流水号")]
        public string VinId { get; set; }

        /// <summary>
        /// 唯一序列号
        /// </summary>
        [SugarColumn(ColumnName = "bohrcode", ColumnDescription = "唯一序列号")]
        public string BohrCode { get; set; }

        /// <summary>
        /// 操作工号
        /// </summary>
        [SugarColumn(ColumnName = "user_id", ColumnDescription = "操作工号")]
        public uint UserId { get; set; }

        /// <summary>
        /// 工单id
        /// </summary>
        [SugarColumn(ColumnName = "work_id", ColumnDescription = "工单id")]
        public uint WorkId { get; set; }

        /// <summary>
        /// 点位id
        /// </summary>
        [SugarColumn(ColumnName = "point_id", ColumnDescription = "点位id")]
        public uint PointId { get; set; }

        #endregion

        public DSDataGroup()
        {
        }
    }
}
