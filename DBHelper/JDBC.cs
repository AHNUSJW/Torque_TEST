using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DBHelper
{
    /// <summary>
    /// 数据库操作类
    /// </summary>
    public static class JDBC
    {
        private static string tableName = "";
        public static string TableName { get => tableName; set => tableName = value; }

        #region 扳手操作

        #region 增加

        //wrench_wlan表增加一条记录
        public static int AddWrenchWlan(DSWrenchWlan wrenchWlan)
        {
            return DBWrench.AddWrenchWlan(wrenchWlan);
        }

        //wrench_alam表增加一条记录
        public static int AddWrenchAlam(DSWrenchAlam wrenchAlam)
        {
            return DBWrench.AddWrenchAlam(wrenchAlam);
        }

        //wrench_devc表增加一条记录
        public static int AddWrenchDevc(DSWrenchDevc wrenchDevc)
        {
            return DBWrench.AddWrenchDevc(wrenchDevc);
        }

        //wrench_para表增加一条记录
        public static int AddWrenchPara(DSWrenchPara wrenchPara)
        {
            return DBWrench.AddWrenchPara(wrenchPara);
        }

        //wrench_work表增加一条记录
        public static int AddWrenchWork(DSWrenchWork wrenchWork)
        {
            return DBWrench.AddWrenchWork(wrenchWork);
        }

        //同时为wrench_devc、wrench_para、wrench_work、wrench_wlan、wrench_alan五表各增加一条记录
        public static int AddWrench(DSWrenchDevc wrenchDevc, DSWrenchPara wrenchPara, DSWrenchWork wrenchWork, DSWrenchWlan wrenchWlan, DSWrenchAlam wrenchAlam)
        {
            return DBWrench.AddWrench(wrenchDevc, wrenchPara, wrenchWork, wrenchWlan, wrenchAlam);
        }

        #endregion

        #region 删除

        //依据wlan_id删除wrench_wlan表记录
        public static bool DeleteWrenchWlan(uint wlan_id)
        {
            return DBWrench.DeleteWrenchWlan(wlan_id) > 0;
        }

        //依据alam_id删除wrench_alam表记录
        public static bool DeleteWrenchAlam(uint alam_id)
        {
            return DBWrench.DeleteWrenchAlam(alam_id) > 0;
        }

        //依据wid删除一条扳手记录，包括wrench_devc、wrench_para、wrench_work三表
        public static bool DeleteWrench(uint wid)
        {
            return DBWrench.DeleteWrench(wid) > 0;
        }

        //依据wid从wrench_devc表删除一条记录
        public static bool DeleteWrenchDevc(uint wid)
        {
            return DBWrench.DeleteWrenchDevc(wid) > 0;
        }

        //依据wid从wrench_para表删除一条记录
        public static bool DeleteWrenchPara(uint wid)
        {
            return DBWrench.DeleteWrenchPara(wid) > 0;
        }

        //依据wid从wrench_work表删除一条记录  
        public static bool DeleteWrenchWork(uint wid)
        {
            return DBWrench.DeleteWrenchWork(wid) > 0;
        }

        #endregion

        #region 查询

        //查询wrench_devc表的所有数据
        public static List<DSWrenchDevc> GetAllWrenchDevc()
        {
            return DBWrench.GetAllWrenchDevc();
        }

        //依据wid查询wrench_devc表的数据
        public static DSWrenchDevc GetWrenchDevcByWid(uint wid)
        {
            return DBWrench.GetWrenchDevcByWid(wid);
        }

        //依据bohrcode查询wid
        public static uint GetWidByBohrcode(ulong bohrcode)
        {
            return DBWrench.GetWidByBohrcode(bohrcode);
        }

        //依据wid查询对应的addr
        public static byte GetAddrByWid(uint wid)
        {
            return DBWrench.GetAddrByWid(wid);
        }

        //查询wrench_para表的所有数据
        public static List<DSWrenchPara> GetAllWrenchPara()
        {
            return DBWrench.GetAllWrenchPara();
        }

        //查询wrench_work表的所有数据
        public static List<DSWrenchWork> GetAllWrenchWork()
        {
            return DBWrench.GetAllWrenchWork();
        }

        //查询wrench_wlan表的所有数据
        public static List<DSWrenchWlan> GetAllWrenchWlan()
        {
            return DBWrench.GetAllWrenchWlan();
        }

        //依据wid查询wlan_id
        public static uint GetWlanIdByWid(uint wid)
        {
            return DBWrench.GetWlanIdByWid(wid);
        }

        //依据wlan_id查询wrench_wlan表的记录
        public static DSWrenchWlan GetWrenchWlanByWlanId(uint wlan_id)
        {
            return DBWrench.GetWrenchWlanByWlanId(wlan_id);
        }

        //查询wrench_wlan表中是否存在name、addr、rf_chan、rf_option、rf_para、wf_ssid、wf_pwd、wf_ip、wf_port相同的记录
        public static bool IsExistWrenchWlan(DSWrenchWlan wrenchWlan)
        {
            return DBWrench.IsExistWrenchWlan(wrenchWlan);
        }

        //依据name、addr、rf_chan、rf_option、rf_para、wf_ssid、wf_pwd、wf_ip、wf_port查询wrench_wlan表的记录
        public static DSWrenchWlan GetWrenchWlanByDetails(DSWrenchWlan wrenchWlan)
        {
            return DBWrench.GetWrenchWlanByDetails(wrenchWlan);
        }

        //查询wrench_alam表的所有数据
        public static List<DSWrenchAlam> GetAllWrenchAlam()
        {
            return DBWrench.GetAllWrenchAlam();
        }

        //依据wid查询alam_id
        public static uint GetAlamIdByWid(uint wid)
        {
            return DBWrench.GetAlamIdByWid(wid);
        }

        //依据alam_id查询wrench_alam表的记录
        public static DSWrenchAlam GetWrenchAlamByAlamId(uint alam_id)
        {
            return DBWrench.GetWrenchAlamByAlamId(alam_id);
        }

        //查询alam表中是否存在name、en_target、en_pre、ea_ang、sn_target、sa_pre、sa_ang、mn_low、mn_high、ma_pre、ma_low、ma_high相同的记录
        public static bool IsExistWrenchAlam(DSWrenchAlam wrenchAlam)
        {
            return DBWrench.IsExistWrenchAlam(wrenchAlam);
        }

        //依据name、en_target、en_pre、ea_ang、sn_target、sa_pre、sa_ang、mn_low、mn_high、ma_pre、ma_low、ma_high查询wrench_alam表的记录
        public static DSWrenchAlam GetWrenchAlamByDetails(DSWrenchAlam wrenchAlam)
        {
            return DBWrench.GetWrenchAlamByDetails(wrenchAlam);
        }

        #endregion

        #region 修改

        //依据wid修改wrench_devc表的一条记录
        public static bool UpdateWrenchDevc(uint wid, DSWrenchDevc wrenchDevc)
        {
            return DBWrench.UpdateWrenchDevc(wid, wrenchDevc) > 0;
        }

        //依据wid修改wrench_para表的一条记录
        public static bool UpdateWrenchPara(uint wid, DSWrenchPara wrenchPara)
        {
            return DBWrench.UpdateWrenchPara(wid, wrenchPara) > 0;
        }

        //依据wid修改wrench_work表的一条记录
        public static bool UpdateWrenchWork(uint wid, DSWrenchWork wrenchWork)
        {
            return DBWrench.UpdateWrenchWork(wid, wrenchWork) > 0;
        }

        //依据wlan_id修改wrench_wlan表的一条记录
        public static bool UpdateWrenchWlan(uint wlan_id, DSWrenchWlan wrenchWlan)
        {
            return DBWrench.UpdateWrenchWlan(wlan_id, wrenchWlan) > 0;
        }

        //依据alam_id修改wrench_alam表的一条记录
        public static bool UpdateWrenchAlam(uint alam_id, DSWrenchAlam wrenchAlam)
        {
            return DBWrench.UpdateWrenchAlam(alam_id, wrenchAlam) > 0;
        }

        //依据wid修改wrench_devc、wrench_para、wrench_work、wrench_wlan、wrench_alarm五表的记录
        public static bool UpdateWrench(uint wid, DSWrenchDevc wrenchDevc, DSWrenchPara wrenchPara, DSWrenchWork wrenchWork, DSWrenchWlan wrenchWlan, DSWrenchAlam wrenchAlam)
        {
            return DBWrench.UpdateWrench(wid, wrenchDevc, wrenchPara, wrenchWork, wrenchWlan, wrenchAlam) > 0;
        }

        #endregion

        #endregion

        #region 数据操作

        #region 增加

        //数据组表增加一条数据组记录
        public static bool AddDataGroup(DSDataGroup dataGroup)
        {
            return DBData.AddDataGroup(dataGroup) > 0;
        }

        //数据组表增加多条数据组记录
        public static bool AddMultipleDataGroup(List<DSDataGroup> dataGroupList)
        {
            return DBData.AddMultipleDataGroup(dataGroupList) > 0;
        }

        //数据组表增加一条数据组记录
        public static List<long> AddData(DSData data)
        {
            return DBData.AddData(data);
        }

        //数据表增加多条（一组）数据
        public static List<long> AddDataList(List<DSData> data)
        {
            return DBData.AddDataList(data);
        }

        //
        public static bool AddDataSummary(DSDataSummary dataSummary)
        {
            return DBData.AddDataSummary(dataSummary) > 0;
        }

        //
        public static bool AddMultipleDataSummary(List<DSDataSummary> dataSummaryList)
        {
            return DBData.AddMultipleDataSummary(dataSummaryList) > 0;
        }

        #endregion

        #region 删除

        //依据group_id删除数据组表的数据
        public static bool DeleteDataGroupByGroupId(uint groupId)
        {
            return DBData.DeleteDataGroupByGroupId(groupId) > 0;
        }

        //依据流水号删除数据组表的数据
        public static bool DeleteDataGroupByVinId(string vinId)
        {
            return DBData.DeleteDataGroupByVinId(vinId) > 0;
        }

        //依据bohrcode删除数据组表的数据
        public static bool DeleteDataGroupByBohrCode(string bohrCode)
        {
            return DBData.DeleteDataGroupByBohrCode(bohrCode) > 0;
        }

        //流水号删除数据表的数据
        public static bool DeleteDataByVinId(string vinId)
        {
            return DBData.DeleteDataByVinId(vinId) > 0;
        }

        //删除最近的n张分表的数据
        public static bool DeleteDataByRecent(int num)
        {
            return DBData.DeleteDataByRecent(num) > 0;
        }

        //删除指定工单号 + 序列号 + 日期的数据
        public static bool DeleteDataByWidAndSeqAndTime(string workNum, string sequenceId, DateTime time)
        {
            return DBData.DeleteDataByWidAndSeqAndTime(workNum, sequenceId, time) > 0;
        }

        //删除指定工单号 + 序列号 + 日期 + 作业号的数据
        public static bool DeleteDataByWidAndSeqAndTimeAndVid(string workNum, string sequenceId, DateTime time, string vinId)
        {
            return DBData.DeleteDataByWidAndSeqAndTimeAndVid(workNum, sequenceId, time, vinId) > 0;
        }

        #endregion

        #region 查询

        //查询数据组表的所有数据
        public static List<DSDataGroup> GetAllDataGroup()
        {
            return DBData.GetAllDataGroup();
        }

        //依据流水号查询数据组表的数据
        public static List<DSDataGroup> GetDataGroupByVinId(string vinId)
        {
            return DBData.GetDataGroupByVinId(vinId);
        }

        //依据bohrcode查询数据组表的数据
        public static List<DSDataGroup> GetDataGroupByBohrCode(string bohrCode)
        {
            return DBData.GetDataGroupByBohrCode(bohrCode);
        }

        //依据work_id查询数据组表的数据
        public static List<DSDataGroup> GetDataGroupByWorkId(uint workId)
        {
            return DBData.GetDataGroupByWorkId(workId);
        }

        //依据point_id查询数据组表的数据
        public static List<DSDataGroup> GetDataGroupByPointId(uint pointId)
        {
            return DBData.GetDataGroupByPointId(pointId);
        }

        //查询数据表的所有数据
        public static List<DSData> GetAllData()
        {
            return DBData.GetAllData();
        }

        //依据流水号查询数据表的数据
        public static List<DSData> GetDataByVinId(string vinId)
        {
            return DBData.GetDataByVinId(vinId);
        }

        //依据日期查询数据表的数据
        public static List<DSData> GetDataByTime(DateTime time)
        {
            return DBData.GetDataByTime(time);
        }

        //依据工单号序列号时间查询数据表的数据
        public static List<DSData> GetDataByWoNumAndSeIdAndTime(string workNum, string sequenceId, DateTime time)
        {
            return DBData.GetDataByWoNumAndSeIdAndTime(workNum, sequenceId, time);
        }

        //依据工单号序列号时间作业号查询数据表的数据
        public static List<DSData> GetDataByWoNumAndSeIdAndTimeAndVid(string workNum, string sequenceId, DateTime time, string vid)
        {
            return DBData.GetDataByWoNumAndSeIdAndTimeAndVid(workNum, sequenceId, time, vid);
        }

        //查询某个时间范围内的数据
        public static List<DSData> GetDataByPeriod(DateTime startDate, DateTime endDate)
        {
            return DBData.GetDataByPeriod(startDate, endDate);
        }

        //查询最近n张分表的数据
        public static List<DSData> GetDataByRecent(int num)
        {
            return DBData.GetDataByRecent(num);
        }

        //获取特定列，按条件查询
        public static List<DSData> GetSpecificDataByWorkNumGroupedBySeqIdAndDate(string workNum)
        {
            Expression<Func<DSData, DSData>> selectExpression = it => new DSData
            {
                WorkNum = it.WorkNum,
                DataType = it.DataType,
                SequenceId = it.SequenceId,
                CreateTime = SqlFunc.AggregateMin(it.CreateTime)
            };
            Expression<Func<DSData, bool>> whereExpression = it => it.WorkNum == workNum;
            Expression<Func<DSData, object>> groupbyExpression = it => new { it.SequenceId, it.CreateTime.Date };

            return DBData.GetSpecificColumnsDataByWhereAndGroupBy(selectExpression, whereExpression, groupbyExpression);
        }

        //获取特定列，按条件查询
        public static List<DSData> GetSpecificDataByWorkNumdSeqIdAndDateGroupByVinId(string workNum, string sequenceId, DateTime time)
        {
            Expression<Func<DSData, DSData>> selectExpression = it => new DSData
            {
                WorkNum = it.WorkNum,
                SequenceId = it.SequenceId,
                DataType = it.DataType,
                VinId = it.VinId,
                CreateTime = SqlFunc.AggregateMin(it.CreateTime),
            };
            Expression<Func<DSData, bool>> whereExpression = it => it.WorkNum == workNum
                                                                && it.SequenceId == sequenceId
                                                                && it.VinId != "";
            Expression<Func<DSData, object>> groupbyExpression = it => new { it.VinId };

            return DBData.GetSpecificColumnsDataByWhereAndGroupBy(selectExpression, whereExpression, groupbyExpression, time.Date);
        }

        //获取特定列
        public static List<DSData> GetSpecificDataByWoNumAndSeIdAndTimeAndVid(string workNum, string sequenceId, string vid, DateTime time)
        {
            Expression<Func<DSData, DSData>> selectExpression = it => new DSData
            {
                WorkNum = it.WorkNum,
                SequenceId = it.SequenceId,
                VinId = it.VinId,
                Bohrcode = it.Bohrcode,
                DevType = it.DevType,
                PointNum = it.PointNum,
                DevAddr = it.DevAddr,
                CreateTime = it.CreateTime,
                DType = it.DType,
                Stamp = it.Stamp,
                Torque = it.Torque,
                TorqueUnit = it.TorqueUnit,
                TorquePeak = it.TorquePeak,
                Angle = it.Angle,
                AngleAcc = it.AngleAcc,
                DataResult = it.DataResult,
            };
            Expression<Func<DSData, bool>> whereExpression = it => it.WorkNum == workNum
                                                                && it.SequenceId == sequenceId
                                                                && it.VinId == vid;

            return DBData.GetSpecificColumnsDataByWhere(selectExpression, whereExpression, time.Date);
        }

        //查询数据汇总表的所有数据
        public static List<DSDataSummary> GetAllDataSummary()
        {
            return DBData.GetAllDataSummary();
        }

        //根据数据类型查阅数据汇总表
        public static List<DSDataSummary> GetDataSummaryByDateType(string dataType)
        {
            return DBData.GetDataSummaryByDateType(dataType);
        }

        //根据工单号查阅数据汇总表
        public static List<DSDataSummary> GetDataSummaryByWorkNum(string workNum)
        {
            return DBData.GetDataSummaryByWorkNum(workNum);
        }

        #endregion

        #endregion

        #region 工单操作

        #region 增加

        //增加工单记录
        public static int AddTicket(DSTicketInfo ticket)
        {
            return DBTicket.AddTicket(ticket);
        }

        //向已有的某一工单增加一个点位
        public static int AddPoint(DSTicketPoints point)
        {
            return DBTicket.AddPoint(point);
        }

        //向已有的某一工单增加多个点位
        public static bool AddPoints(List<DSTicketPoints> points)
        {
            return DBTicket.AddPoints(points) > 0;
        }

        //点位扳手关系表增加一条记录
        public static bool AddPointWrench(DSRelationsPointWrench pointWrench)
        {
            return DBTicket.AddPointWrench(pointWrench) > 0;
        }

        //螺丝报警值表增加一条记录
        public static int AddScrewAlarm(DSTicketScrews screw)
        {
            return DBTicket.AddScrewAlarm(screw);
        }

        //螺丝报警值表增加多条记录
        public static bool AddScrewAlarms(List<DSTicketScrews> screws)
        {
            return DBTicket.AddScrewAlarms(screws) > 0;
        }

        //增加产品
        public static int AddProduct(DSProductInfo product)
        {
            return DBTicket.AddProduct(product);
        }

        //增加产品结果
        public static int AddProductResult(DSProductResults productResults)
        {
            return DBTicket.AddProductResult(productResults);
        }

        #endregion

        #region 删除

        //根据work_id删除工单表中的记录
        public static bool DeleteTicketByWorkId(uint workId)
        {
            return DBTicket.DeleteTicketByWorkId(workId) > 0;
        }

        //依据work_id删除点位表中的记录
        public static bool DeletePointsByWorkId(uint workId)
        {
            return DBTicket.DeletePointsByWorkId(workId) > 0;
        }

        //根据point_id删除点位表中的记录
        public static bool DeletePointsByPointId(uint pointId)
        {
            return DBTicket.DeletePointsByPointId(pointId) > 0;
        }

        //依据screw_id删除点位表中的记录
        public static bool DeletePointsByScrewId(uint screwId)
        {
            return DBTicket.DeletePointsByScrewId(screwId) > 0;
        }

        //依据screw_id删除工单
        public static bool DeleteTicketByScrewId(uint screwId)
        {
            return DBTicket.DeleteTicketByScrewId(screwId) > 0;
        }

        //根据point_id删除点位扳手关系表中的记录
        public static bool DeletePointWrenchsByPointId(uint pointId)
        {
            return DBTicket.DeletePointWrenchsByPointId(pointId) > 0;
        }

        //依据point_id和wid删除点位扳手关系表中的记录
        public static bool DeletePointWrenchsByPointIdAndWid(uint pointId, uint wid)
        {
            return DBTicket.DeletePointWrenchsByPointIdAndWid(pointId, wid) > 0;
        }

        //根据screw_id删除螺丝报警值表中的记录
        public static bool DeleteScrewAlarmByScrewId(uint screwId)
        {
            return DBTicket.DeleteScrewAlarmByScrewId(screwId) > 0;
        }

        #endregion

        #region 查询

        //查询工单表的所有数据
        public static List<DSTicketInfo> GetAllTickets()
        {
            return DBTicket.GetAllTickets();
        }

        //依据workId查询对应的工单
        public static DSTicketInfo GetTicketByWorkId(uint workId)
        {
            return DBTicket.GetTicketByWorkId(workId);
        }

        //依据wo_num查询对应的工单
        public static List<DSTicketInfo> GetTicketByWoNum(string woNum)
        {
            return DBTicket.GetTicketByWoNum(woNum);
        }

        //依据wo_name查询对应的工单
        public static List<DSTicketInfo> GetTicketByWoName(string woName)
        {
            return DBTicket.GetTicketByWoName(woName);
        }

        //依据wo_name和wo_num查询对应的工单
        public static List<DSTicketInfo> GetTicketByWoNameAndWoNum(string woName, string woNum)
        {
            return DBTicket.GetTicketByWoNameAndWoNum(woName, woNum);
        }

        //查询点位表的所有数据
        public static List<DSTicketPoints> GetAllPoints()
        {
            return DBTicket.GetAllPoints();
        }

        //依据point_id查询点位
        public static DSTicketPoints GetPointsByPointId(uint pointId)
        {
            return DBTicket.GetPointsByPointId(pointId);
        }

        //依据work_id查询对应工单的所有点位
        public static List<DSTicketPoints> GetPointsByWorkId(uint work_id)
        {
            return DBTicket.GetPointsByWorkId(work_id);
        }

        //依据screw_id查询对应的点位
        public static List<DSTicketPoints> GetPointsByScrewId(uint screwId)
        {
            return DBTicket.GetPointsByScrewId(screwId);
        }

        //依据screw_id查询点位表中的work_id
        public static List<uint> GetWorkIdByScrewId(uint screwId)
        {
            return DBTicket.GetWorkIdByScrewId(screwId);
        }

        //查询点位扳手关系表的所有数据
        public static List<DSRelationsPointWrench> GetAllPointWrenches()
        {
            return DBTicket.GetAllPointWrenches();
        }

        //依据point_id查询该点位的所有关系数据
        public static List<DSRelationsPointWrench> GetWrenchesByPointId(uint pointId)
        {
            return DBTicket.GetWrenchesByPointId(pointId);
        }

        //查询螺丝报警值表的所有数据
        public static List<DSTicketScrews> GetAllScrewAlarms()
        {
            return DBTicket.GetAllScrewAlarms();
        }

        //依据screw_id查询对应的螺丝报警值数据
        public static DSTicketScrews GetScrewAlarmByScrewId(uint screwId)
        {
            return DBTicket.GetScrewAlarmByScrewId(screwId);
        }

        //依据name查询对应的螺丝报警值数据
        public static List<DSTicketScrews> GetScrewAlarmByName(string name)
        {
            return DBTicket.GetScrewAlarmByName(name);
        }

        //依据name查询对应的螺丝报警值数据（模糊查询）
        public static List<DSTicketScrews> GetScrewAlarmByLikeName(string name)
        {
            return DBTicket.GetScrewAlarmByLikeName(name);
        }

        //查询所有产品
        public static List<DSProductInfo> GetAllProducts()
        {
            return DBTicket.GetAllProducts();
        }

        //依据workId查询对应的所有产品
        public static List<DSProductInfo> GetProductsByWorkId(uint workId)
        {
            return DBTicket.GetProductsByWorkId(workId);
        }

        //依据workId和sequenceId查询指定产品
        public static DSProductInfo GetProductByWorkIdAndSequenceId(uint workId, string sequenceId)
        {
            return DBTicket.GetProductByWorkIdAndSequenceId(workId, sequenceId);
        }

        //依据workNum和sequenceId查询指定产品
        public static List<DSProductInfo> GetProductByWorkNumAndSequenceId(string workNum, string sequenceId)
        {
            return DBTicket.GetProductByWorkNumAndSequenceId(workNum, sequenceId);
        }

        //查询所有产品结果
        public static List<DSProductResults> GetAllProductResult()
        {
            return DBTicket.GetAllProductResult();
        }

        //依据workId查询对应的所有产品结果
        public static List<DSProductResults> GetProductResultsByWorkIdAndSequenceId(uint workId, string sequenceId)
        {
            return DBTicket.GetProductResultsByWorkIdAndSequenceId(workId, sequenceId);
        }

        //依据workId和sequenceId和pointNum查询指定产品结果
        public static DSProductResults GetProductResultByWIdAndSqIdAndPointNum(uint workId, string sequenceId, string pointNum)
        {
            return DBTicket.GetProductResultByWIdAndSqIdAndPointNum(workId, sequenceId, pointNum);
        }

        #endregion

        #region 修改

        //依据work_id修改工单表中的记录
        public static int UpdateTicketByWorkId(uint workId, DSTicketInfo ticket)
        {
            return DBTicket.UpdateTicketByWorkId(workId, ticket);
        }

        //依据point_id修改点位表中的记录
        public static bool UpdatePointByPointId(uint pointId, DSTicketPoints point)
        {
            return DBTicket.UpdatePointByPointId(pointId, point) > 0;
        }

        //依据point_id修改点位表对应记录的screw_id
        public static bool UpdatePointScrewIdByPointId(uint pointId, uint newScrewId)
        {
            return DBTicket.UpdatePointScrewIdByPointId(pointId, newScrewId) > 0;
        }

        //依据point_id修改关系
        public static bool UpdatePointWrenchByPointId(uint pointId, uint oldWid, uint newWid)
        {
            return DBTicket.UpdatePointWrenchByPointId(pointId, oldWid, newWid) > 0;
        }

        //根据screw_id修改螺丝报警值表中的记录
        public static bool UpdateScrewAlarmByScrewId(uint screwId, DSTicketScrews screw)
        {
            return DBTicket.UpdateScrewAlarmByScrewId(screwId, screw) > 0;
        }

        //依据screw_id修改螺丝报警值表中对应记录的name
        public static bool UpdateScrewAlarmNameByScrewId(uint screwId, string name)
        {
            return DBTicket.UpdateScrewAlarmNameByScrewId(screwId, name) > 0;
        }

        //依据screw_id修改螺丝报警值表中对应记录的description
        public static bool UpdateScrewAlarmDescriptionByScrewId(uint screwId, string description)
        {
            return DBTicket.UpdateScrewAlarmDescriptionByScrewId(screwId, description) > 0;
        }

        //依据point_id修改点位表对应记录的screw_id
        public static bool UpdateProductResultByWIdAndSqIdAndPointNum(uint workId, string sequenceId, string pointNum)
        {
            return DBTicket.UpdateProductResultByWIdAndSqIdAndPointNum(workId, sequenceId, pointNum) > 0;
        }

        #endregion

        #endregion

        #region 人员操作

        #region 增加

        //增加一位用户
        public static int AddUser(DSUserInfo user)
        {
            //返回记录的person_id
            return DBUser.AddUser(user);
        }

        //增加多位用户
        public static bool AddUsers(List<DSUserInfo> users)
        {
            //返回插入的记录数
            return DBUser.AddUsers(users) > 0;
        }

        #endregion

        #region 删除

        //依据person_id删除用户
        public static bool DeleteUserByPersonId(uint person_id)
        {
            return DBUser.DeleteUserByPersonId(person_id) > 0;
        }

        //依据user_id删除用户
        public static bool DeleteUserByUserId(int user_id)
        {
            return DBUser.DeleteUserByUserId(user_id) > 0;
        }

        //删除所有用户
        public static bool DeleteAllUsers()
        {
            return DBUser.DeleteAllUsers() > 0;
        }

        #endregion

        #region 查询

        //查询人员表的所有数据
        public static List<DSUserInfo> GetAllUsers()
        {
            return DBUser.GetAllUsers();
        }

        //根据person_id查询用户
        public static DSUserInfo GetUserByPersonId(uint person_id)
        {
            return DBUser.GetUserByPersonId(person_id);
        }

        #endregion

        #region 修改

        //依据person_id修改用户信息
        public static int UpdateUserByPersonId(uint personid, DSUserInfo user)
        {
            return DBUser.UpdateUserByPersonId(personid, user);
        }

        #endregion

        #endregion

    }
}