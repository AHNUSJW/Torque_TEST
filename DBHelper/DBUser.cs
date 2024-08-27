using SqlSugar;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DBHelper
{
    public class DBUser : DBHelper
    {
        //用单例模式
        public static SqlSugarScope Db = new SqlSugarScope(new ConnectionConfig()
        {
            ConnectionString = ConfigurationManager.AppSettings["AuthContextXh"],  //数据库连接字符串
            DbType = DbType.MySql,//数据库类型
            IsAutoCloseConnection = true //不设成true要手动close
        });

        #region 增加

        //增加一位用户
        public static int AddUser(DSUserInfo user)
        {
            try
            {
                //返回记录的person_id
                return Db.Insertable(user).ExecuteReturnIdentity();
            }
            catch
            {
                return -1;
            }
        }

        //增加多位用户
        public static int AddUsers(List<DSUserInfo> users)
        {
            try
            {
                //返回插入的记录数
                return Db.Insertable(users).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        #endregion

        #region 删除

        //依据person_id删除用户
        public static int DeleteUserByPersonId(uint person_id)
        {
            try
            {
                return Db.Deleteable<DSUserInfo>().Where(it => it.PersonId == person_id).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        //依据user_id删除用户
        public static int DeleteUserByUserId(int user_id)
        {
            try
            {
                return Db.Deleteable<DSUserInfo>().Where(it => it.UserId == user_id).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        //删除所有用户
        public static int DeleteAllUsers()
        {
            try
            {
                return Db.Deleteable<DSUserInfo>().Where(it => it.PersonId > 0).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        #endregion

        #region 查询

        //查询人员表的所有数据
        public static List<DSUserInfo> GetAllUsers()
        {
            try
            {
                var list = Db.Queryable<DSUserInfo>().ToList();
                return list;
            }
            catch
            {
                return null;
            }
        }

        //根据person_id查询用户
        public static DSUserInfo GetUserByPersonId(uint person_id)
        {
            try
            {
                var user = Db.Queryable<DSUserInfo>().Where(it => it.PersonId == person_id).First();
                return user;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region 修改

        //依据person_id修改用户信息
        public static int UpdateUserByPersonId(uint personid, DSUserInfo user)
        {
            try
            {
                //根据主键更新单条
                user.PersonId = personid;
                return Db.Updateable(user).Where(it => it.PersonId == user.PersonId).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        #endregion
    }
}
