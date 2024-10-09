using MySqlX.XDevAPI.Common;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;

namespace DBHelper
{
    public class DBData : DBHelper
    {
        //用单例模式
        public static SqlSugarScope Db = new SqlSugarScope(new ConnectionConfig()
        {
            ConnectionString = ConfigurationManager.AppSettings["AuthContextXhData"],  //数据库连接字符串
            DbType = DbType.MySql,//数据库类型
            IsAutoCloseConnection = true //不设成true要手动close
        });

        public static void SyncDatabaseStructure()
        {
            try
            {
                // 同步数据库结构并添加索引
                Db.CodeFirst.SetStringDefaultLength(200).InitTables(typeof(DSData));
            }
            catch (Exception ex)
            {
                // 处理异常
                Console.WriteLine($"同步数据库结构时发生错误: {ex.Message}");
            }
        }

        #region 增加

        #region 数据组表 data_group_info

        //数据组表增加一条数据组记录
        public static int AddDataGroup(DSDataGroup dataGroup)
        {
            try
            {
                //返回插入的记录数
                return Db.Insertable(dataGroup).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        //数据组表增加多条数据组记录
        public static int AddMultipleDataGroup(List<DSDataGroup> dataGroupList)
        {
            try
            {
                //返回插入的记录数
                return Db.Insertable(dataGroupList).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        #endregion

        #region 数据表

        //数据表增加一条数据组记录
        public static List<long> AddData(DSData data)
        {
            try
            {
                data.CreateTime = DateTime.Now;
                return Db.Insertable(data).SplitTable().ExecuteReturnSnowflakeIdList();
            }
            catch
            {
                return null;
            }
        }

        //数据表增加多条（一组）数据
        public static List<long> AddDataList(List<DSData> data)
        {
            try
            {
                foreach (var item in data)
                {
                    item.CreateTime = DateTime.Now;
                }
                return Db.Insertable(data).SplitTable().ExecuteReturnSnowflakeIdList();
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region 数据汇总表 data_summary

        //数据汇总表增加一条数据纪录
        public static int AddDataSummary(DSDataSummary dataSummary)
        {
            try
            {
                //返回插入的记录数
                return Db.Insertable(dataSummary).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        //数据组表增加多条数据组记录
        public static int AddMultipleDataSummary(List<DSDataSummary> dataSummaryList)
        {
            try
            {
                //返回插入的记录数
                return Db.Insertable(dataSummaryList).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        #endregion

        #endregion

        #region 删除

        #region 数据组表 data_group_info

        //依据group_id删除数据组表的数据
        public static int DeleteDataGroupByGroupId(uint groupId)
        {
            try
            {
                //返回删除的记录数
                return Db.Deleteable<DSDataGroup>().Where(it => it.GroupId == groupId).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        //依据流水号删除数据组表的数据
        public static int DeleteDataGroupByVinId(string vinId)
        {
            try
            {
                //返回删除的记录数
                return Db.Deleteable<DSDataGroup>().Where(it => it.VinId == vinId).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        //依据bohrcode删除数据组表的数据
        public static int DeleteDataGroupByBohrCode(string bohrCode)
        {
            try
            {
                //返回删除的记录数
                return Db.Deleteable<DSDataGroup>().Where(it => it.BohrCode == bohrCode).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        #endregion

        #region 数据表

        //流水号删除数据表的数据
        public static int DeleteDataByVinId(string vinId)
        {
            try
            {
                //返回删除的记录数
                return Db.Deleteable<DSData>().Where(it => it.VinId == vinId).SplitTable().ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        //删除最近的n张分表的数据
        public static int DeleteDataByRecent(int num)
        {
            try
            {
                //返回删除的记录数
                return Db.Deleteable<DSData>().SplitTable(tabs => tabs.Take(num)).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        //删除指定日期的分表数据
        public static int DeleteDataByTime(DateTime time)
        {
            try
            {
                //返回删除的记录数
                var tableName = Db.SplitHelper<DSData>().GetTableName(time);//根据时间获取表名
                return Db.Deleteable<DSData>().AS(tableName).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        //删除指定时间区间的数据
        public static int DeleteDataByPeriod(DateTime startDate, DateTime endDate)
        {
            try
            {
                //这里的ToList可以升级为ToPageList实现分页
                return Db.Deleteable<DSData>()
                       .Where(it => it.CreateTime >= startDate && it.CreateTime <= endDate)
                       .ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        //删除指定工单号 + 序列号 + 日期的数据
        public static int DeleteDataByWidAndSeqAndTime(string workNum, string sequenceId, DateTime time)
        {
            try
            {
                var tableName = Db.SplitHelper<DSData>().GetTableName(time);//根据时间获取表名
                return Db.Deleteable<DSData>().AS(tableName).Where(it => it.WorkNum == workNum && it.SequenceId == sequenceId).ExecuteCommand();//返回删除的记录数
            }
            catch
            {
                return -1;
            }
        }

        //删除指定工单号 + 序列号 + 日期 + 作业号的数据
        public static int DeleteDataByWidAndSeqAndTimeAndVid(string workNum, string sequenceId, DateTime time, string vinId)
        {
            try
            {
                var tableName = Db.SplitHelper<DSData>().GetTableName(time);//根据时间获取表名
                return Db.Deleteable<DSData>().AS(tableName).Where(d => d.WorkNum == workNum
                                                                  && d.SequenceId == sequenceId
                                                                  && d.VinId == vinId)
                                                            .ExecuteCommand();//返回删除的记录数
            }
            catch
            {
                return -1;
            }
        }

        #endregion

        #endregion

        #region 查询

        #region 数据组表 data_group_info

        //查询数据组表的所有数据
        public static List<DSDataGroup> GetAllDataGroup()
        {
            try
            {
                var list = Db.Queryable<DSDataGroup>().ToList();
                return list;
            }
            catch
            {
                return null;
            }
        }

        //依据流水号查询数据组表的数据
        public static List<DSDataGroup> GetDataGroupByVinId(string vinId)
        {
            try
            {
                var list = Db.Queryable<DSDataGroup>().Where(it => it.VinId == vinId).ToList();
                return list;
            }
            catch
            {
                return null;
            }
        }

        //依据bohrcode查询数据组表的数据
        public static List<DSDataGroup> GetDataGroupByBohrCode(string bohrCode)
        {
            try
            {
                var list = Db.Queryable<DSDataGroup>().Where(it => it.BohrCode == bohrCode).ToList();
                return list;
            }
            catch
            {
                return null;
            }
        }

        //依据work_id查询数据组表的数据
        public static List<DSDataGroup> GetDataGroupByWorkId(uint workId)
        {
            try
            {
                var list = Db.Queryable<DSDataGroup>().Where(it => it.WorkId == workId).ToList();
                return list;
            }
            catch
            {
                return null;
            }
        }

        //依据point_id查询数据组表的数据
        public static List<DSDataGroup> GetDataGroupByPointId(uint pointId)
        {
            try
            {
                var list = Db.Queryable<DSDataGroup>().Where(it => it.PointId == pointId).ToList();
                return list;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region 数据表

        //查询数据表的所有数据
        public static List<DSData> GetAllData()
        {
            try
            {
                var list = Db.Queryable<DSData>().SplitTable().ToList();
                return list;
            }
            catch
            {
                return null;
            }
        }

        //依据流水号查询数据表的数据
        public static List<DSData> GetDataByVinId(string vinId)
        {
            try
            {
                //条件尽可能写在SplitTable前面
                var list = Db.Queryable<DSData>().Where(it => it.VinId == vinId).SplitTable().ToList();
                return list;
            }
            catch
            {
                return null;
            }
        }

        //依据日期查询数据表的数据
        public static List<DSData> GetDataByTime(DateTime time)
        {
            try
            {
                var list = Db.Queryable<DSData>().Where(it => it.CreateTime.Date == time.Date).SplitTable().ToList();
                return list;
            }
            catch
            {
                return null;
            }
        }

        //依据工单号序列号时间查询数据表的数据
        public static List<DSData> GetDataByWoNumAndSeIdAndTime(string workNum, string sequenceId, DateTime time)
        {
            try
            {
                var list = Db.Queryable<DSData>().Where(it => (it.WorkNum == workNum
                                                            && it.SequenceId == sequenceId
                                                            && it.CreateTime.Date == time.Date)).SplitTable().ToList();
                return list;
            }
            catch
            {
                return null;
            }
        }

        //依据工单号序列号时间作业号查询数据表的数据
        public static List<DSData> GetDataByWoNumAndSeIdAndTimeAndVid(string workNum, string sequenceId, DateTime time, string vid)
        {
            try
            {
                var list = Db.Queryable<DSData>().Where(it => (it.WorkNum == workNum
                                                            && it.SequenceId == sequenceId
                                                            && it.CreateTime.Date == time.Date)
                                                            && it.VinId == vid).SplitTable().ToList();
                return list;
            }
            catch
            {
                return null;
            }
        }

        //查询某个时间范围内的数据
        public static List<DSData> GetDataByPeriod(DateTime startDate, DateTime endDate)
        {
            try
            {
                //这里的ToList可以升级为ToPageList实现分页
                var list = Db.Queryable<DSData>().SplitTable(startDate, endDate).ToList();
                return list;
            }
            catch
            {
                return null;
            }
        }

        //查询最近n张分表的数据
        public static List<DSData> GetDataByRecent(int num)
        {
            try
            {
                var list = Db.Queryable<DSData>().SplitTable(tabs => tabs.Take(num)).ToList();
                return list;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 查询特定列的数据
        /// </summary>
        /// <param name="columnNames">select语句</param>
        /// <returns></returns>
        public static List<dynamic> GetSpecificColumnsData(Expression<Func<DSData, object>> selectExpression)
        {
            try
            {
                // 构建查询
                var list = Db.Queryable<DSData>()
                             .SplitTable()
                             .Select<dynamic>(selectExpression)
                             .ToList();

                // 执行查询并返回结果
                return list;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 查询特定列的数据
        /// </summary>
        /// <param name="columnNames">select语句</param>
        /// <returns></returns>
        public static List<DSData> GetSpecificColumnsData(Expression<Func<DSData, DSData>> selectExpression)
        {
            try
            {
                // 构建查询
                var list = Db.Queryable<DSData>()
                             .SplitTable()
                             .Select(selectExpression)
                             .ToList();

                // 执行查询并返回结果
                return list;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 依据动态条件查询特定列的数据
        /// </summary>
        /// <param name="columnNames">select语句</param>
        /// <param name="whereExpression">where条件</param>
        /// <returns></returns>
        public static List<dynamic> GetSpecificColumnsDataByWhere(
            Expression<Func<DSData, object>> selectExpression,
            Expression<Func<DSData, bool>> whereExpression)
        {
            try
            {
                // 构建查询
                var list = Db.Queryable<DSData>()
                             .Where(whereExpression)
                             .SplitTable()
                             .Select<dynamic>(selectExpression)
                             .ToList();

                // 执行查询并返回结果
                return list;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 依据动态条件查询特定列的数据
        /// </summary>
        /// <param name="columnNames">select语句</param>
        /// <param name="whereExpression">where条件</param>
        /// <returns></returns>
        public static List<DSData> GetSpecificColumnsDataByWhere(
            Expression<Func<DSData, DSData>> selectExpression,
            Expression<Func<DSData, bool>> whereExpression)
        {
            try
            {
                // 构建查询
                var list = Db.Queryable<DSData>()
                             .Where(whereExpression)
                             .SplitTable()
                             .Select(selectExpression)
                             .ToList();

                // 执行查询并返回结果
                return list;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 依据动态条件查询特定列的数据(精确到某一日期的分表)
        /// </summary>
        /// <param name="selectExpression">select的表达式</param>
        /// <param name="whereExpression">where筛选表达式</param>
        /// <param name="date">分表表名</param>
        /// <returns></returns>
        public static List<DSData> GetSpecificColumnsDataByWhere(
            Expression<Func<DSData, DSData>> selectExpression,
            Expression<Func<DSData, bool>> whereExpression,
            DateTime date)
        {
            try
            {
                var name = Db.SplitHelper<DSData>().GetTableName(date);
                // 构建查询
                var list = Db.Queryable<DSData>()
                             .Where(whereExpression)
                             .SplitTable(tabs => tabs.InTableNames(name))
                             .Select(selectExpression)
                             .ToList();

                // 执行查询并返回结果
                return list;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 依据动态条件查询特定列的数据
        /// </summary>
        /// <param name="selectExpression">select的表达式</param>
        /// <param name="whereExpression">where筛选表达式</param>
        /// <param name="groupbyExpression">分组表达式</param>
        /// <returns></returns>
        public static List<dynamic> GetSpecificColumnsDataByWhereAndGroupBy(
            Expression<Func<DSData, object>> selectExpression,
            Expression<Func<DSData, bool>> whereExpression,
            Expression<Func<DSData, object>> groupbyExpression)
        {
            try
            {
                // 构建查询
                var list = Db.Queryable<DSData>()
                             .Where(whereExpression)
                             .SplitTable()
                             .GroupBy(groupbyExpression)
                             .Select<dynamic>(selectExpression)
                             .ToList();

                // 执行查询并返回结果
                return list;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 依据动态条件查询特定列的数据
        /// </summary>
        /// <param name="selectExpression">select的表达式</param>
        /// <param name="whereExpression">where筛选表达式</param>
        /// <param name="groupbyExpression">分组表达式</param>
        /// <returns></returns>
        public static List<DSData> GetSpecificColumnsDataByWhereAndGroupBy(
            Expression<Func<DSData, DSData>> selectExpression,
            Expression<Func<DSData, bool>> whereExpression,
            Expression<Func<DSData, object>> groupbyExpression)
        {
            try
            {
                // 构建查询
                var list = Db.Queryable<DSData>()
                             .Where(whereExpression)
                             .SplitTable()
                             .GroupBy(groupbyExpression)
                             .Select(selectExpression)
                             .ToList();

                // 执行查询并返回结果
                return list;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 依据动态条件查询特定列的数据（精准定位到某一日期分表）
        /// </summary>
        /// <param name="selectExpression">select的表达式</param>
        /// <param name="whereExpression">where筛选表达式</param>
        /// <param name="groupbyExpression">分组表达式</param>
        /// <param name="date">分表表名</param>
        /// <returns></returns>
        public static List<dynamic> GetSpecificColumnsDataByWhereAndGroupBy(
            Expression<Func<DSData, object>> selectExpression,
            Expression<Func<DSData, bool>> whereExpression,
            Expression<Func<DSData, object>> groupbyExpression,
            DateTime date)
        {
            try
            {
                var name = Db.SplitHelper<DSData>().GetTableName(date);
                // 构建查询
                var list = Db.Queryable<DSData>()
                             .Where(whereExpression)
                             .SplitTable(tabs => tabs.InTableNames(name))
                             .GroupBy(groupbyExpression)
                             .Select<dynamic>(selectExpression)
                             .ToList();

                // 执行查询并返回结果
                return list;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 依据动态条件查询特定列的数据（精准定位到某一日期分表）
        /// </summary>
        /// <param name="selectExpression">select的表达式</param>
        /// <param name="whereExpression">where筛选表达式</param>
        /// <param name="groupbyExpression">分组表达式</param>
        /// <param name="date">分表表名</param>
        /// <returns></returns>
        public static List<DSData> GetSpecificColumnsDataByWhereAndGroupBy(
            Expression<Func<DSData, DSData>> selectExpression,
            Expression<Func<DSData, bool>> whereExpression,
            Expression<Func<DSData, object>> groupbyExpression,
            DateTime date)
        {
            try
            {
                var name = Db.SplitHelper<DSData>().GetTableName(date);
                // 构建查询
                var list = Db.Queryable<DSData>()
                             .Where(whereExpression)
                             .SplitTable(tabs => tabs.InTableNames(name))
                             .GroupBy(groupbyExpression)
                             .Select(selectExpression)
                             .ToList();

                // 执行查询并返回结果
                return list;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region 数据汇总表 data_summary

        //查询数据汇总表的所有数据
        public static List<DSDataSummary> GetAllDataSummary()
        {
            try
            {
                var list = Db.Queryable<DSDataSummary>().ToList();
                return list;
            }
            catch
            {
                return null;
            }
        }

        //根据数据类型查阅数据汇总表
        public static List<DSDataSummary> GetDataSummaryByDateType(string dataType)
        {
            try
            {
                var list = Db.Queryable<DSDataSummary>().Where(it => it.DataType == dataType).ToList();
                return list;
            }
            catch
            {
                return null;
            }
        }

        //根据工单号查阅数据汇总表
        public static List<DSDataSummary> GetDataSummaryByWorkNum(string workNum)
        {
            try
            {
                var list = Db.Queryable<DSDataSummary>().Where(it => it.WorkNum == workNum).ToList();
                return list;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #endregion

        //数据表不支持修改
    }
}
