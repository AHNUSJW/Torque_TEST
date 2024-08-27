using SqlSugar;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

//数据库 —— 数据组表
//用于存储实时数据
//按日期，自动分表
//参考：https://www.donet5.com/Home/Doc?typeId=1201

namespace DBHelper
{
    /// <summary>
    /// 数据组表
    /// </summary>
    [SplitTable(SplitType.Day)]
    [SugarTable("data_{year}{month}{day}")]//3个变量必须要有，这么设计为了兼容开始按年，后面改成按月、按日
    public class DSData
    {
        #region 属性
        /// <summary>
        /// 数据id
        /// </summary>
        [SugarColumn(ColumnName = "data_id", IsPrimaryKey = true, ColumnDescription = "数据id")]
        public long DataId { get; set; }

        /// <summary>
        /// 数据场景
        /// </summary>
        [SugarColumn(ColumnName = "data_type", ColumnDescription = "数据场景", IsNullable = true)]
        public string DataType { get; set; }

        /// <summary>
        /// 设备唯一编号
        /// </summary>
        [SugarColumn(ColumnName = "bohrcode", ColumnDescription = "设备唯一编号")]
        public ulong Bohrcode { get; set; }

        /// <summary>
        /// 设备型号
        /// </summary>
        [SugarColumn(ColumnName = "dev_type", ColumnDescription = "设备型号")]
        public string DevType { get; set; }

        /// <summary>
        /// 工单唯一识别字符
        /// </summary>
        [SugarColumn(ColumnName = "work_id", ColumnDescription = "工单唯一识别字符", IsNullable = true)]
        public uint WorkId { get; set; }

        /// <summary>
        /// 工单名称
        /// </summary>
        [SugarColumn(ColumnName = "work_num", ColumnDescription = "工单名称", IsNullable = true)]
        public string WorkNum { get; set; }

        /// <summary>
        /// 序列号
        /// </summary>
        [SugarColumn(ColumnName = "sequence_id", ColumnDescription = "序列号", IsNullable = true)]
        public string SequenceId { get; set; }

        /// <summary>
        /// 点位号
        /// </summary>
        [SugarColumn(ColumnName = "point_num", ColumnDescription = "点位号", IsNullable = true)]
        public string PointNum { get; set; }

        /// <summary>
        /// 设备站点
        /// </summary>
        /// 如何是通过sugarsql自建表，定义byte类型时需要添加语句ColumnDataType = "tinyint unsigned "，否则默认是tinyint型
        ///  mysql 中 tinyint 的类型数据范围为 【-128,127】，而C# 的byte 类型范围为 【0,255】
        ///  参考 https://www.donet5.com/Ask/9/19514
        [SugarColumn(ColumnName = "dev_addr", ColumnDescription = "设备站点", ColumnDataType = "tinyint unsigned ")]
        public byte DevAddr { get; set; }

        /// <summary>
        /// 流水号
        /// </summary>
        [SugarColumn(ColumnName = "vin_id", ColumnDescription = "流水号")]
        public string VinId { get; set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        [SugarColumn(ColumnName = "dtype", ColumnDescription = "数据类型")]
        public int DType { get; set; }

        /// <summary>
        /// 时间标识
        /// </summary>
        [SugarColumn(ColumnName = "stamp", ColumnDescription = "时间标识")]
        public uint Stamp { get; set; }

        /// <summary>
        /// 实时扭矩
        /// </summary>
        [SugarColumn(ColumnName = "torque", ColumnDescription = "实时扭矩", IsNullable = true)]
        public double Torque { get; set; }

        /// <summary>
        /// 扭矩峰值
        /// </summary>
        [SugarColumn(ColumnName = "torque_peak", ColumnDescription = "扭矩峰值")]
        public double TorquePeak { get; set; }

        /// <summary>
        /// 扭矩单位
        /// </summary>
        [SugarColumn(ColumnName = "torque_unit", ColumnDescription = "扭矩单位", Length = 10, IsNullable = true)]
        public string TorqueUnit { get; set; }

        /// <summary>
        /// 实时角度
        /// </summary>
        [SugarColumn(ColumnName = "angle", ColumnDescription = "实时角度", IsNullable = true)]
        public double Angle { get; set; }

        /// <summary>
        /// 累加角度
        /// </summary>
        [SugarColumn(ColumnName = "angle_acc", ColumnDescription = "累加角度")]
        public double AngleAcc { get; set; }

        /// <summary>
        /// 数据结果
        /// </summary>
        [SugarColumn(ColumnName = "data_result", ColumnDescription = "数据结果")]
        public string DataResult { get; set; }

        /// <summary>
        /// 模式modePt
        /// </summary>
        [SugarColumn(ColumnName = "mode_pt", ColumnDescription = "模式modePt",ColumnDataType = "tinyint unsigned ")]
        public byte ModePt { get; set; }

        /// <summary>
        /// 模式modeAx
        /// </summary>
        [SugarColumn(ColumnName = "mode_ax", ColumnDescription = "模式modeAx", ColumnDataType = "tinyint unsigned ")]
        public byte ModeAx { get; set; }

        /// <summary>
        /// 模式modeMx
        /// </summary>
        [SugarColumn(ColumnName = "mode_mx", ColumnDescription = "模式modeMx", ColumnDataType = "tinyint unsigned ")]
        public byte ModeMx { get; set; }

        /// <summary>
        /// 电量
        /// </summary>
        [SugarColumn(ColumnName = "battery", ColumnDescription = "电量", IsNullable = true, ColumnDataType = "tinyint unsigned ")]
        public byte? Battery { get; set; }

        /// <summary>
        /// 按键值
        /// </summary>
        [SugarColumn(ColumnName = "keybuf", ColumnDescription = "按键值", IsNullable = true, ColumnDataType = "tinyint unsigned ")]
        public byte? KeyBuf { get; set; }

        /// <summary>
        /// 操作锁定
        /// </summary>
        [SugarColumn(ColumnName = "keylock", ColumnDescription = "操作锁定", Length = 10, IsNullable = true)]
        public string KeyLock { get; set; }

        /// <summary>
        /// 锁定通讯
        /// </summary>
        [SugarColumn(ColumnName = "memable", ColumnDescription = "锁定通讯", Length = 10, IsNullable = true)]
        public string MemAble { get; set; }

        /// <summary>
        /// 按键改参数
        /// </summary>
        [SugarColumn(ColumnName = "update", ColumnDescription = "按键改参数", Length = 10, IsNullable = true)]
        public string Update { get; set; }

        /// <summary>
        /// 超量程
        /// </summary>
        [SugarColumn(ColumnName = "error", ColumnDescription = "超量程", Length = 10, IsNullable = true)]
        public string Error { get; set; }

        /// <summary>
        /// 报警数值
        /// </summary>
        [SugarColumn(ColumnName = "alarm", ColumnDescription = "报警数值", Length = 255, IsNullable = true)]
        public string Alarm { get; set; }

        /// <summary>
        /// 分表字段 在插入的时候会根据这个字段插入哪个表，在更新删除的时候用这个字段找出相关表
        /// </summary>
        [SplitField]
        public DateTime CreateTime { get; set; }

        #endregion

        public DSData()
        {
            CreateTime = DateTime.Now;
        }

        /// <summary>
        /// 类型转换
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<DSData> ConvertToDSDataList(List<dynamic> list)
        {
            var dsDataList = new List<DSData>();

            Parallel.ForEach(list, item =>
            {
                var dsData = new DSData
                {
                    WorkNum = item.work_num,
                    SequenceId = item.sequence_id,
                    VinId = item.vin_id,
                    Bohrcode = (ulong)item.bohrcode,
                    DevType = item.dev_type,
                    PointNum = item.point_num,
                    DevAddr = item.dev_addr,
                    CreateTime = item.CreateTime,
                    DType = item.dtype,
                    Stamp = (uint)item.stamp,
                    Torque = item.torque,
                    TorquePeak = item.torque_peak,
                    TorqueUnit = item.torque_unit,
                    Angle = item.angle,
                    AngleAcc = item.angle_acc,
                    DataResult = item.data_result
                };

                lock (dsDataList)
                {
                    dsDataList.Add(dsData);
                }
            });

            return dsDataList;
        }
    }
}
