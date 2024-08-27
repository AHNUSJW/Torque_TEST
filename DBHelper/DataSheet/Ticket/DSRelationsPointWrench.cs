using SqlSugar;

namespace DBHelper
{
    /// <summary>
    /// 点位与扳手关系表
    /// </summary>
    [SugarTable("relations_points_wrench", "点位与扳手关系表")]
    public class DSRelationsPointWrench
    {
        #region 属性
        /// <summary>
        /// 点位id
        /// </summary>
        [SugarColumn(ColumnName = "point_id", ColumnDescription = "点位id")]
        public uint PointId { get; set; }

        /// <summary>
        /// 扳手addr
        /// </summary>
        [SugarColumn(ColumnName = "addr", ColumnDescription = "扳手addr")]
        public uint Addr { get; set; }

        #endregion

        public DSRelationsPointWrench()
        {
        }
    }
}
