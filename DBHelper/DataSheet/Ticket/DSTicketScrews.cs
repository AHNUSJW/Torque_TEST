using SqlSugar;

//数据库 —— 工单螺丝信息表
//用于管理螺丝拧紧信息

namespace DBHelper
{
    /// <summary>
    /// 工单螺丝表
    /// </summary>
    [SugarTable("ticket_screws", "工单螺丝表")]
    public class DSTicketScrews
    {
        #region 属性
        /// <summary>
        /// 螺丝id
        /// </summary>
        [SugarColumn(ColumnName = "screw_id", IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "螺丝id")]
        public uint ScrewId { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [SugarColumn(ColumnName = "name", ColumnDescription = "名称", Length = 10)]
        public string Name { get; set; }

        /// <summary>
        /// 规格
        /// </summary>
        [SugarColumn(ColumnName = "specification", ColumnDescription = "规格")]
        public string Specification { get; set; }

        /// <summary>
        /// 标准
        /// </summary>
        [SugarColumn(ColumnName = "standard", ColumnDescription = "标准")]
        public string Standard { get; set; }

        /// <summary>
        /// 材料
        /// </summary>
        [SugarColumn(ColumnName = "material", ColumnDescription = "材料")]
        public string Material { get; set; }

        /// <summary>
        /// 螺栓头尺寸
        /// </summary>
        [SugarColumn(ColumnName = "screw_headSize", ColumnDescription = "螺栓头尺寸")]
        public string Screw_headSize { get; set; }

        /// <summary>
        /// 螺栓头结构
        /// </summary>
        [SugarColumn(ColumnName = "screw_headStructure", ColumnDescription = "螺栓头结构")]
        public string Screw_headStructure { get; set; }

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
        /// 描述
        /// </summary>
        [SugarColumn(ColumnName = "description", ColumnDescription = "描述")]
        public string Description { get; set; }

        #endregion

        public DSTicketScrews()
        {
        }
    }
}
