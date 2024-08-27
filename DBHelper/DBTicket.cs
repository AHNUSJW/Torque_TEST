using SqlSugar;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;

namespace DBHelper
{
    public class DBTicket : DBHelper
    {
        //用单例模式
        public static SqlSugarScope Db = new SqlSugarScope(new ConnectionConfig()
        {
            ConnectionString = ConfigurationManager.AppSettings["AuthContextXh"],  //数据库连接字符串
            DbType = DbType.MySql,//数据库类型
            IsAutoCloseConnection = true //不设成true要手动close
        });

        #region 增加

        #region 工单

        //增加工单记录
        public static int AddTicket(DSTicketInfo ticket)
        {
            try
            {
                //返回work_id
                return Db.Insertable(ticket).ExecuteReturnIdentity();
            }
            catch
            {
                return -1;
            }
        }

        #endregion

        #region 点位

        //向已有的某一工单增加一个点位
        public static int AddPoint(DSTicketPoints point)
        {
            //加点位时，先判断是否存在对应的工单
            //再判断是否存在对应的螺丝报警方案
            try
            {
                //查询是否存在对应work_id的工单
                if (GetTicketByWorkId(point.WorkId) == null)
                {
                    throw new ArgumentException($"No ticket found with WorkId {point.WorkId}");
                }


                //查询是否存在对应的screw_id
                if (GetScrewAlarmByScrewId(point.ScrewsId) == null)
                {
                    throw new ArgumentException($"No ticket found with ScrewId {point.ScrewsId}");
                }

                //返回point_id
                return Db.Insertable(point).ExecuteReturnIdentity();
            }
            catch
            {
                return -1;
            }
        }

        //向已有的某一工单增加多个点位
        public static int AddPoints(List<DSTicketPoints> points)
        {
            //加点位时，先判断是否存在对应的工单
            //再判断是否存在对应的螺丝报警方案
            try
            {
                if (points.Count == 0)
                {
                    throw new ArgumentException("Points list cannot be empty");
                }

                uint firstWorkId = points[0].WorkId;
                //判断这些点位的work_id是否相同
                if (!points.All(p => p.WorkId == firstWorkId))
                {
                    throw new ArgumentException("All WorkId in points list must be the same");
                }

                //查询是否存在对应work_id的工单
                if (GetTicketByWorkId(firstWorkId) == null)
                {
                    throw new ArgumentException($"No ticket found with WorkId {firstWorkId}");
                }

                foreach (DSTicketPoints point in points)
                {
                    //查询是否存在对应的screw_id
                    if (GetScrewAlarmByScrewId(point.ScrewsId) == null)
                    {
                        throw new ArgumentException($"No ticket found with ScrewId {point.ScrewsId}");
                    }
                }

                //返回插入的记录数
                return Db.Insertable(points).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        #endregion

        #region 关系

        //点位扳手关系表增加一条记录
        public static int AddPointWrench(DSRelationsPointWrench pointWrench)
        {
            try
            {
                //判断该点位是否在点位表存在
                if (GetPointsByPointId(pointWrench.PointId) == null)
                {
                    throw new ArgumentException($"No point found with PointId {pointWrench.PointId}");
                }

                //查找关系表是否已存在对应的记录
                var relationList = GetWrenchesByPointId(pointWrench.PointId);
                foreach (var r in relationList)
                {
                    if (r.PointId == pointWrench.PointId && r.Addr == pointWrench.Addr)
                    {
                        //throw new ArgumentException("The record already exists");
                    }
                }

                //不存在则加入新记录
                //返回插入的记录数
                return Db.Insertable(pointWrench).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        #endregion

        #region 螺丝报警值

        //螺丝报警值表增加一条记录
        public static int AddScrewAlarm(DSTicketScrews screw)
        {
            try
            {
                //返回screw_id
                return Db.Insertable(screw).ExecuteReturnIdentity();
            }
            catch
            {
                return -1;
            }
        }

        //螺丝报警值表增加多条记录
        public static int AddScrewAlarms(List<DSTicketScrews> screws)
        {
            try
            {
                //返回插入的记录数
                return Db.Insertable(screws).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        #endregion

        #region 产品

        //增加产品
        public static int AddProduct(DSProductInfo product)
        {
            try
            {
                //查询是否存在对应work_id的工单
                if (GetProductsByWorkId(product.WorkId) == null)
                {
                    throw new ArgumentException($"No product found with WorkId {product.WorkId}");
                }

                //查询是否存在对应work_id和sequence_id的工单
                if (GetProductByWorkIdAndSequenceId(product.WorkId, product.SequenceId) == null)
                {
                    Console.WriteLine($"No product found with WorkId {product.WorkId}" + product.SequenceId);
                }

                //返回product_id
                return Db.Insertable(product).ExecuteReturnIdentity();
            }
            catch
            {
                return -1;
            }
        }

        #endregion

        #region 产品结果

        //增加产品结果
        public static int AddProductResult(DSProductResults productResult)
        {
            try
            {
                //查询是否存在对应work_id和sequence_id的工单
                if (GetProductResultsByWorkIdAndSequenceId(productResult.WorkId, productResult.SequenceId) == null)
                {
                    throw new ArgumentException($"No prodcutResult found with WorkId {productResult.WorkId}" + productResult.SequenceId);
                }

                //查询是否存在对应work_id和sequence_id和point_num的工单结果
                if (GetProductResultByWIdAndSqIdAndPointNum(productResult.WorkId, productResult.SequenceId, productResult.PointNumber) == null)
                {
                    //throw new ArgumentException($"No prodcutResult found with WorkId {productResult.WorkId}" + productResult.SequenceId);
                }

                //返回product_id
                return Db.Insertable(productResult).ExecuteReturnIdentity();
            }
            catch
            {
                return -1;
            }
        }

        #endregion

        #endregion

        #region 删除

        //根据work_id删除工单表中的记录
        public static int DeleteTicketByWorkId(uint workId)
        {
            try
            {
                //从位点表中删除点位记录
                DeletePointsByWorkId(workId);

                //从工单表中删工单除记录
                //返回删除的记录数
                return Db.Deleteable<DSTicketInfo>().In(workId).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        //依据work_id删除点位表中的记录
        public static int DeletePointsByWorkId(uint workId)
        {
            try
            {
                //查找该工单下的所有点位
                var points = GetPointsByWorkId(workId);
                foreach (var point in points)
                {
                    //从位点扳手关系表中删除记录
                    DeletePointWrenchsByPointId(point.PointId);
                }

                //再从点位表删除点位记录
                //返回删除的记录数
                return Db.Deleteable<DSTicketPoints>().Where(it => it.WorkId == workId).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        //根据point_id删除点位表中的记录
        public static int DeletePointsByPointId(uint pointId)
        {
            try
            {
                //先从关系表删除记录
                DeletePointWrenchsByPointId(pointId);

                //再删除点位记录
                //返回删除的记录数
                return Db.Deleteable<DSTicketPoints>().In(pointId).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        //依据screw_id删除点位表中的记录
        public static int DeletePointsByScrewId(uint screwId)
        {
            try
            {
                //返回删除的记录数
                return Db.Deleteable<DSTicketPoints>().Where(it => it.ScrewsId == screwId).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        //依据screw_id删除工单
        public static int DeleteTicketByScrewId(uint screwId)
        {
            try
            {
                var works = GetWorkIdByScrewId(screwId);
                foreach (var work in works)
                {
                    DeleteTicketByWorkId(work);
                }

                return 1;
            }
            catch
            {
                return -1;
            }
        }   

        //根据point_id删除点位扳手关系表中的记录
        public static int DeletePointWrenchsByPointId(uint pointId)
        {
            try
            {
                var list = GetWrenchesByPointId(pointId);
                //返回删除的记录数
                return Db.Deleteable<DSRelationsPointWrench>().WhereColumns(list, it => new { it.PointId }).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        //依据point_id和addr删除点位扳手关系表中的记录
        public static int DeletePointWrenchsByPointIdAndWid(uint pointId, uint addr)
        {
            try
            {
                //返回删除的记录数
                return Db.Deleteable<DSRelationsPointWrench>().Where(it => it.PointId == pointId && it.Addr == addr).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        //根据screw_id删除螺丝报警值表中的记录
        public static int DeleteScrewAlarmByScrewId(uint screwId)
        {
            try
            {
                //返回删除的记录数
                return Db.Deleteable<DSTicketScrews>().In(screwId).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        #endregion

        #region 查询

        #region 工单

        //查询工单表的所有数据
        public static List<DSTicketInfo> GetAllTickets()
        {
            try
            {
                var list = Db.Queryable<DSTicketInfo>().ToList();
                return list;
            }
            catch
            {
                return null;
            }
        }

        //依据workId查询对应的工单
        public static DSTicketInfo GetTicketByWorkId(uint workId)
        {
            try
            {
                var ticket = Db.Queryable<DSTicketInfo>().Where(it => it.WorkId == workId).First();
                return ticket;
            }
            catch
            {
                return null;
            }
        }

        //依据wo_num查询对应的工单
        public static List<DSTicketInfo> GetTicketByWoNum(string woNum)
        {
            try
            {
                var tickets = Db.Queryable<DSTicketInfo>().Where(it => it.WoNum == woNum).ToList();
                return tickets;
            }
            catch
            {
                return null;
            }
        }

        //依据wo_name查询对应的工单
        public static List<DSTicketInfo> GetTicketByWoName(string woName)
        {
            try
            {
                var tickets = Db.Queryable<DSTicketInfo>().Where(it => it.WoName == woName).ToList();
                return tickets;
            }
            catch
            {
                return null;
            }
        }

        //依据wo_name和wo_num查询对应的工单
        public static List<DSTicketInfo> GetTicketByWoNameAndWoNum(string woName, string woNum)
        {
            try
            {
                var tickets = Db.Queryable<DSTicketInfo>().Where(it => it.WoName == woName && it.WoNum == woNum).ToList();
                return tickets;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region 点位

        //查询点位表的所有数据
        public static List<DSTicketPoints> GetAllPoints()
        {
            try
            {
                var list = Db.Queryable<DSTicketPoints>().ToList();
                return list;
            }
            catch
            {
                return null;
            }
        }

        //依据point_id查询点位
        public static DSTicketPoints GetPointsByPointId(uint pointId)
        {
            try
            {
                var points = Db.Queryable<DSTicketPoints>().Where(it => it.PointId == pointId).First();
                return points;
            }
            catch
            {
                return null;
            }
        }

        //依据work_id查询对应工单的所有点位
        public static List<DSTicketPoints> GetPointsByWorkId(uint work_id)
        {
            try
            {
                var points = Db.Queryable<DSTicketPoints>().Where(it => it.WorkId == work_id).ToList();
                return points;
            }
            catch
            {
                return null;
            }
        }

        //依据screw_id查询对应的点位
        public static List<DSTicketPoints> GetPointsByScrewId(uint screwId)
        {
            try
            {
                var points = Db.Queryable<DSTicketPoints>().Where(it => it.ScrewsId == screwId).ToList();
                return points;
            }
            catch
            {
                return null;
            }
        }

        //依据screw_id查询点位表中的work_id
        public static List<uint> GetWorkIdByScrewId(uint screwId)
        {
            try
            {
                var workIds = Db.Queryable<DSTicketPoints>().Where(it => it.ScrewsId == screwId).Select(it => it.WorkId).ToList();
                return workIds;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region 关系

        //查询点位扳手关系表的所有数据
        public static List<DSRelationsPointWrench> GetAllPointWrenches()
        {
            try
            {
                var list = Db.Queryable<DSRelationsPointWrench>().ToList();
                return list;
            }
            catch
            {
                return null;
            }
        }

        //依据point_id查询该点位的所有关系数据
        public static List<DSRelationsPointWrench> GetWrenchesByPointId(uint pointId)
        {
            try
            {
                var wrenches = Db.Queryable<DSRelationsPointWrench>().Where(it => it.PointId == pointId).ToList();
                return wrenches;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region 螺丝报警值

        //查询螺丝报警值表的所有数据
        public static List<DSTicketScrews> GetAllScrewAlarms()
        {
            try
            {
                var list = Db.Queryable<DSTicketScrews>().ToList();
                return list;
            }
            catch
            {
                return null;
            }
        }

        //依据screw_id查询对应的螺丝报警值数据
        public static DSTicketScrews GetScrewAlarmByScrewId(uint screwId)
        {
            try
            {
                var screw = Db.Queryable<DSTicketScrews>().Where(it => it.ScrewId == screwId).First();
                return screw;
            }
            catch
            {
                return null;
            }
        }

        //依据name查询对应的螺丝报警值数据
        public static List<DSTicketScrews> GetScrewAlarmByName(string name)
        {
            try
            {
                //select  * from ticket_screws where name = 'name'
                var screws = Db.Queryable<DSTicketScrews>().Where(it => it.Name == name).ToList();
                return screws;
            }
            catch
            {
                return null;
            }
        }

        //依据name查询对应的螺丝报警值数据（模糊查询）
        public static List<DSTicketScrews> GetScrewAlarmByLikeName(string name)
        {
            try
            {
                //select  * from  ticket_screws where name like %name%
                var screws = Db.Queryable<DSTicketScrews>().Where(it => it.Name.Contains(name)).ToList();
                return screws;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region 产品

        //查询工单对应的所有产品
        public static List<DSProductInfo> GetAllProducts()
        {
            try
            {
                var list = Db.Queryable<DSProductInfo>().ToList();
                return list;
            }
            catch
            {
                return null;
            }
        }

        //依据work_id和sequence_id查询对应的工单
        public static DSProductInfo GetProductByWorkIdAndSequenceId(uint workId, string sequenceId)
        {
            try
            {
                var product = Db.Queryable<DSProductInfo>().Where(it => it.WorkId == workId && it.SequenceId == sequenceId).First();
                return product;
            }
            catch
            {
                return null;
            }
        }

        //依据work_id和sequence_id查询对应的工单集合.
        public static List<DSProductInfo> GetProductByWorkNumAndSequenceId(string workNum, string sequenceId)
        {
            try
            {
                var product = Db.Queryable<DSProductInfo>().Where(it => it.WorkNum == workNum && it.SequenceId == sequenceId).ToList();
                return product;
            }
            catch
            {
                return null;
            }
        }


        //依据workId查询对应的产品集合
        public static List<DSProductInfo> GetProductsByWorkId(uint workId)
        {
            try
            {
                var product = Db.Queryable<DSProductInfo>().Where(it => it.WorkId == workId).ToList();
                return product;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region 产品结果

        //查询工单对应的所有产品
        public static List<DSProductResults> GetAllProductResult()
        {
            try
            {
                var list = Db.Queryable<DSProductResults>().ToList();
                return list;
            }
            catch
            {
                return null;
            }
        }

        //依据work_id和sequence_id查询对应的工单结果汇总
        public static List<DSProductResults> GetProductResultsByWorkIdAndSequenceId(uint workId, string sequenceId)
        {
            try
            {
                var productResults = Db.Queryable<DSProductResults>().Where(it => it.WorkId == workId && it.SequenceId == sequenceId).ToList();
                return productResults;
            }
            catch
            {
                return null;
            }
        }

        //依据work_id、sequence_id、point_num查询对应的工单结果
        public static DSProductResults GetProductResultByWIdAndSqIdAndPointNum(uint workId, string sequenceId, string pointNum)
        {
            try
            {
                var productResult = Db.Queryable<DSProductResults>().Where(it => it.WorkId == workId && it.SequenceId == sequenceId && it.PointNumber == pointNum).First();
                return productResult;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #endregion

        #region 修改

        #region 工单

        //依据work_id修改工单表中的记录
        public static int UpdateTicketByWorkId(uint workId, DSTicketInfo ticket)
        {
            try
            {
                //根据主键更新单条
                ticket.WorkId = workId;
                //返回修改的记录数
                return Db.Updateable(ticket).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        #endregion

        #region 点位

        //依据point_id修改点位表中的记录
        public static int UpdatePointByPointId(uint pointId, DSTicketPoints point)
        {
            try
            {
                // 根据主键更新单条记录
                point.PointId = pointId;

                //查询对应的工单是否存在
                if (GetTicketByWorkId(point.WorkId) == null)
                {
                    throw new ArgumentException($"No ticket found with WorkId {point.WorkId}");
                }

                //查询是否存在对应的screw_id
                if (GetScrewAlarmByScrewId(point.ScrewsId) == null)
                {
                    throw new ArgumentException($"No ticket found with ScrewId {point.ScrewsId}");
                }

                // 返回修改的记录数
                return Db.Updateable(point).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        //依据point_id修改点位表对应记录的screw_id
        public static int UpdatePointScrewIdByPointId(uint pointId, uint newScrewId)
        {
            try
            {
                // 首先，找到对应的点位记录
                DSTicketPoints point = GetPointsByPointId(pointId);
                if (point == null)
                {
                    throw new ArgumentException($"No point found with PointId {pointId}");
                }

                // 然后，确保新的screw_id对应的螺丝报警值数据存在
                if (GetScrewAlarmByScrewId(newScrewId) == null)
                {
                    throw new ArgumentException($"No screw alarm found with ScrewId {newScrewId}");
                }

                // 更新点位记录的screw_id
                point.ScrewsId = newScrewId;

                // 返回修改的记录数
                return Db.Updateable(point).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        #endregion

        #region 关系
        //依据point_id修改关系
        public static int UpdatePointWrenchByPointId(uint pointId, uint oldAddr, uint newAddr)
        {
            try
            {
                //删除原记录
                DeletePointWrenchsByPointIdAndWid(pointId, oldAddr);
                //增加新记录
                DSRelationsPointWrench pointWrench = new DSRelationsPointWrench();
                pointWrench.PointId = pointId;
                pointWrench.Addr = newAddr;
                return AddPointWrench(pointWrench);
            }
            catch
            {
                return -1;
            }
        }
        #endregion

        #region 螺丝报警值

        //根据screw_id修改螺丝报警值表中的记录
        public static int UpdateScrewAlarmByScrewId(uint screwId, DSTicketScrews screw)
        {
            try
            {
                //根据主键更新单条
                screw.ScrewId = screwId;
                //返回修改的记录数
                return Db.Updateable(screw).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        //依据screw_id修改螺丝报警值表中对应记录的name
        public static int UpdateScrewAlarmNameByScrewId(uint screwId, string name)
        {
            try
            {
                DSTicketScrews screw = new DSTicketScrews()
                {
                    ScrewId = screwId,
                    Name = name
                };
                //返回修改的记录数
                return Db.Updateable(screw).UpdateColumns(it => new { it.Name }).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        //依据screw_id修改螺丝报警值表中对应记录的description
        public static int UpdateScrewAlarmDescriptionByScrewId(uint screwId, string description)
        {
            try
            {
                //返回修改的记录数
                return Db.Updateable<DSTicketScrews>().SetColumns(it => new DSTicketScrews() { Description = description }).Where(it => it.ScrewId == screwId).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        #endregion

        #region 产品结果

        //依据work_id修改点位表中的记录
        public static int UpdateProductResultByWIdAndSqIdAndPointNum(uint workId, string sequenceId, string pointNum)
        {
            try
            {
                DSProductResults productResult = GetProductResultByWIdAndSqIdAndPointNum(workId, sequenceId, pointNum);
                //查询是否存在对应的结果
                if (productResult == null)
                {
                    throw new ArgumentException($"No prodcutResult found with WorkId {workId} and SequenceId {sequenceId} and PointNum {pointNum}");
                }

                //更新结果
                productResult.Result = "pass";

                //返回product_id
                return Db.Updateable(productResult).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }


        #endregion

        #endregion
    }
}