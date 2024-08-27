using SqlSugar;

//数据库 —— 人员总表
//用于管理人员信息表

namespace DBHelper
{
    /// <summary>
    /// 人员信息
    /// </summary>
    [SugarTable("user_info", "人员信息")]
    public class DSUserInfo
    {
        #region 属性
        /// <summary>
        /// 序号
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "person_id", ColumnDescription = "序号")]
        public uint PersonId { get; set; }

        /// <summary>
        /// 操作工号
        /// </summary>
        [SugarColumn(ColumnName = "user_id", ColumnDescription = "操作工号")]
        public int UserId { get; set; }

        /// <summary>
        /// 操作姓名
        /// </summary>
        [SugarColumn(ColumnName = "user_name", ColumnDescription = "操作姓名")]
        public string UserName { get; set; }

        /// <summary>
        /// 角色
        /// </summary>
        [SugarColumn(ColumnName = "user_role", ColumnDescription = "角色")]
        public string UserRole { get; set; }

        /// <summary>
        /// 备用，供后期数据库增加字段用，无需显示在软件上
        /// </summary>
        [SugarColumn(ColumnName = "reserve", ColumnDescription = "备用")]
        public string Reserve { get; set; }

        #endregion

        public DSUserInfo()
        {
        }
    }
}
