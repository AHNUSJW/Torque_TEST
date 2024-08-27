using SqlSugar;

//数据库 —— 扳手参数WORK
//用于管理扳手参数信息WORK

namespace DBHelper
{
    /// <summary>
    /// 扳手WORK信息
    /// </summary>
    [SugarTable("wrench_work", "扳手WORK")]
    public class DSWrenchWork
    {
        #region 属性
        /// <summary>
        /// 扳手id
        /// </summary>
        [SugarColumn(ColumnName = "wid", IsPrimaryKey = true, ColumnDescription = "扳手id")]
        public uint Wid { get; set; }

        /// <summary>
        /// 生产批号
        /// </summary>
        [SugarColumn(ColumnName = "srno", ColumnDescription = "生产批号")]
        public uint SrNo { get; set; }

        /// <summary>
        /// 数字批号
        /// </summary>
        [SugarColumn(ColumnName = "number", ColumnDescription = "数字批号")]
        public uint Number { get; set; }

        /// <summary>
        /// 出厂时间
        /// </summary>
        [SugarColumn(ColumnName = "mfgtime", ColumnDescription = "出厂时间")]
        public uint MfgTime { get; set; }

        /// <summary>
        /// 校准时间
        /// </summary>
        [SugarColumn(ColumnName = "caltime", ColumnDescription = "校准时间")]
        public uint CalTime { get; set; }

        /// <summary>
        /// 复校时间
        /// </summary>
        [SugarColumn(ColumnName = "calremind", ColumnDescription = "复校时间")]
        public uint CalRemind { get; set; }

        /// <summary>
        /// 扳手名称
        /// </summary>
        [SugarColumn(ColumnName = "name", ColumnDescription = "扳手名称")]
        public string Name { get; set; }

        /// <summary>
        /// 管理编号
        /// </summary>
        [SugarColumn(ColumnName = "managetxt", ColumnDescription = "管理编号")]
        public string ManageTxt { get; set; }

        /// <summary>
        /// 备注信息
        /// </summary>
        [SugarColumn(ColumnName = "decription", ColumnDescription = "备注信息")]
        public string Decription { get; set; }

        /// <summary>
        /// 工单区
        /// </summary>
        [SugarColumn(ColumnName = "wo_area", ColumnDescription = "工单区")]
        public uint WoArea { get; set; }

        /// <summary>
        /// 工单厂
        /// </summary>
        [SugarColumn(ColumnName = "wo_factory", ColumnDescription = "工单厂")]
        public uint WoFactory { get; set; }

        /// <summary>
        /// 工单产线
        /// </summary>
        [SugarColumn(ColumnName = "wo_line", ColumnDescription = "工单产线")]
        public uint WoLine { get; set; }

        /// <summary>
        /// 工单工位
        /// </summary>
        [SugarColumn(ColumnName = "wo_station", ColumnDescription = "工单工位")]
        public uint WoStation { get; set; }

        /// <summary>
        /// 工单批号
        /// </summary>
        [SugarColumn(ColumnName = "wo_bat", ColumnDescription = "工单批号")]
        public uint WoBat { get; set; }

        /// <summary>
        /// 工单编号
        /// </summary>
        [SugarColumn(ColumnName = "wo_num", ColumnDescription = "工单编号")]
        public uint WoNum { get; set; }

        /// <summary>
        /// 工单时间标识
        /// </summary>
        [SugarColumn(ColumnName = "wo_stamp", ColumnDescription = "工单时间标识")]
        public uint WoStamp { get; set; }

        /// <summary>
        /// 工单名称
        /// </summary>
        [SugarColumn(ColumnName = "wo_name", ColumnDescription = "工单名称")]
        public string WoName { get; set; }

        /// <summary>
        /// 操作工号
        /// </summary>
        [SugarColumn(ColumnName = "user_id", ColumnDescription = "操作工号")]
        public uint UserId { get; set; }

        /// <summary>
        /// 操作姓名
        /// </summary>
        [SugarColumn(ColumnName = "user_name", ColumnDescription = "操作姓名")]
        public string UserName { get; set; }

        /// <summary>
        /// 离线工单组合顺序
        /// </summary>
        [SugarColumn(ColumnName = "screworder", ColumnDescription = "离线工单组合顺序")]
        public string Screworder { get; set; }

        /// <summary>
        /// 备用，供后期数据库增加字段用，无需显示在软件上
        /// </summary>
        [SugarColumn(ColumnName = "reserve", ColumnDescription = "备用")]
        public string Reserve { get; set; }

        #endregion

        public DSWrenchWork()
        {
        }
    }
}