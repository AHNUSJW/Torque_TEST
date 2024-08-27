using SqlSugar;

//数据库 —— 扳手ALAM方案
//用于管理扳手ALAM信息

namespace DBHelper
{
    /// <summary>
    /// ALAM方案
    /// </summary>
    [SugarTable("wrench_alam", "扳手ALAM方案")]
    public class DSWrenchAlam
    {
        #region 属性
        /// <summary>
        /// ALAM方案ID
        /// </summary>
        [SugarColumn(ColumnName = "alam_id", IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "ALAM方案ID")]
        public uint AlamId { get; set; }

        /// <summary>
        /// 方案名称
        /// </summary>
        [SugarColumn(ColumnName = "name", ColumnDescription = "方案名称")]
        public string Name { get; set; }

        /// <summary>
        /// ET模式, EN目标扭矩
        /// </summary>
        [SugarColumn(ColumnName = "en_target", ColumnDescription = "ET模式, EN目标扭矩")]
        public string EnTarget { get; set; }

        /// <summary>
        /// ET模式, EA预设扭矩
        /// </summary>
        [SugarColumn(ColumnName = "en_pre", ColumnDescription = "ET模式, EA预设扭矩")]
        public string EnPre { get; set; }

        /// <summary>
        /// ET模式, EA目标角度
        /// </summary>
        [SugarColumn(ColumnName = "ea_ang", ColumnDescription = "ET模式, EA目标角度")]
        public int EaAng { get; set; }

        /// <summary>
        /// A1/ST模式, A1,SN,目标扭矩
        /// </summary>
        [SugarColumn(ColumnName = "sn_target", ColumnDescription = "A1/ST模式, A1,SN,目标扭矩")]
        public string SnTarget { get; set; }

        /// <summary>
        /// A1/ST模式, A1,SA,预设扭矩
        /// </summary>
        [SugarColumn(ColumnName = "sa_pre", ColumnDescription = "A1/ST模式, A1,SA,预设扭矩")]
        public string SaPre { get; set; }

        /// <summary>
        /// A1/ST模式, A1,SA,目标角度
        /// </summary>
        [SugarColumn(ColumnName = "sa_ang", ColumnDescription = "A1/ST模式, A1,SA,目标角度")]
        public string SaAng { get; set; }

        /// <summary>
        /// A2,MT模式, A2,MN,扭矩下限
        /// </summary>
        [SugarColumn(ColumnName = "mn_low", ColumnDescription = "A2,MT模式, A2,MN,扭矩下限")]
        public string MnLow { get; set; }

        /// <summary>
        /// A2,MT模式, A2,MN,扭矩上限
        /// </summary>
        [SugarColumn(ColumnName = "mn_high", ColumnDescription = "A2,MT模式, A2,MN,扭矩上限")]
        public string MnHigh { get; set; }

        /// <summary>
        /// A2,MT模式, A2,MA,预设扭矩
        /// </summary>
        [SugarColumn(ColumnName = "ma_pre", ColumnDescription = "A2,MT模式, A2,MA,预设扭矩")]
        public string MaPre { get; set; }

        /// <summary>
        /// A2,MT模式, A2,MA,角度下限
        /// </summary>
        [SugarColumn(ColumnName = "ma_low", ColumnDescription = "A2,MT模式, A2,MA,角度下限")]
        public string MaLow { get; set; }

        /// <summary>
        /// A2,MT模式, A2,MA,角度上限
        /// </summary>
        [SugarColumn(ColumnName = "ma_high", ColumnDescription = "A2,MT模式, A2,MA,角度上限")]
        public string MaHigh { get; set; }

        #endregion

        public DSWrenchAlam()
        {
        }
    }
}
