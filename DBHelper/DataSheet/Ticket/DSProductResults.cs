using SqlSugar;

//数据库 —— 产品结果表
//用于管理各个产品使用工单的结果

namespace DBHelper
{
    /// <summary>
    /// 工单结果表
    /// </summary>
    [SugarTable("product_results", "工单结果表")]
    public class DSProductResults
    {
        #region 属性
        /// <summary>
        /// 结果id
        /// </summary>
        [SugarColumn(ColumnName = "result_id", IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "结果id")]
        public uint ResultId { get; set; }

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
        /// 序列号
        /// </summary>
        [SugarColumn(ColumnName = "sequence_id", IsPrimaryKey = true, ColumnDescription = "序列号")]
        public string SequenceId { get; set; }

        /// <summary>
        /// 扳手地址
        /// </summary>
        [SugarColumn(ColumnName = "addr", ColumnDescription = "扳手地址")]
        public uint Addr { get; set; }

        /// <summary>
        /// 点位id
        /// </summary>
        [SugarColumn(ColumnName = "point_id", ColumnDescription = "点位id")]
        public uint PointId { get; set; }

        /// <summary>
        /// 点位编号，点位在图片上的编号
        /// </summary>
        [SugarColumn(ColumnName = "point_num", ColumnDescription = "点位编号，点位在图片上的编号")]
        public string PointNumber { get; set; }

        /// <summary>
        /// 点位坐标
        /// </summary>
        [SugarColumn(ColumnName = "point_position", ColumnDescription = "点位坐标")]
        public string PointPosition { get; set; }

        /// <summary>
        /// 螺丝编号
        /// </summary>
        [SugarColumn(ColumnName = "screw_id", ColumnDescription = "螺丝编号")]
        public uint ScrewId { get; set; }

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
        /// 螺丝名称
        /// </summary>
        [SugarColumn(ColumnName = "name", ColumnDescription = "螺丝名称")]
        public string Name { get; set; }

        /// <summary>
        /// 螺丝规格
        /// </summary>
        [SugarColumn(ColumnName = "specification", ColumnDescription = "螺丝规格")]
        public string Specification { get; set; }

        /// <summary>
        /// 扭矩单位
        /// </summary>
        [SugarColumn(ColumnName = "torque_unit", ColumnDescription = "扭矩单位")]
        public string Torque_unit { get; set; }

        /// <summary>
        /// 模式modePt
        /// </summary>
        [SugarColumn(ColumnName = "mode_pt", ColumnDescription = "模式modePt")]
        public string ModePt { get; set; }

        /// <summary>
        /// 模式modeMx
        /// </summary>
        [SugarColumn(ColumnName = "mode_mx", ColumnDescription = "模式modeMx")]
        public string ModeMx { get; set; }

        /// <summary>
        /// 模式modeAx
        /// </summary>
        [SugarColumn(ColumnName = "mode_ax", ColumnDescription = "模式modeAx")]
        public string ModeAx { get; set; }

        /// <summary>
        /// alarm0
        /// </summary>
        [SugarColumn(ColumnName = "alarm0", ColumnDescription = "alarm0")]
        public string Alarm0 { get; set; }

        /// <summary>
        /// alarm1
        /// </summary>
        [SugarColumn(ColumnName = "alarm1", ColumnDescription = "alarm1")]
        public string Alarm1 { get; set; }

        /// <summary>
        /// alarm2
        /// </summary>
        [SugarColumn(ColumnName = "alarm2", ColumnDescription = "alarm2")]
        public string Alarm2 { get; set; }

        /// <summary>
        /// 拧紧结果
        /// </summary>
        [SugarColumn(ColumnName = "result", ColumnDescription = "拧紧结果", Length = 10)]
        public string Result { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(ColumnName = "reserve", ColumnDescription = "备用")]
        public string Reserve { get; set; }

        #endregion

        public DSProductResults()
        {
        }
    }
}
