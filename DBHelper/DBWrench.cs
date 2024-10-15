using SqlSugar;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace DBHelper
{
    public class DBWrench : DBHelper
    {
        //用单例模式
        public static SqlSugarScope Db = new SqlSugarScope(new ConnectionConfig()
        {
            ConnectionString = ConfigurationManager.AppSettings["AuthContextXh"],  //数据库连接字符串
            DbType = DbType.MySql,//数据库类型
            IsAutoCloseConnection = true //不设成true要手动close
        });

        #region 增加

        //wrench_wlan表增加一条记录
        public static int AddWrenchWlan(DSWrenchWlan wrenchWlan)
        {
            try
            {
                //返回插入wlan_id
                return Db.Insertable(wrenchWlan).ExecuteReturnIdentity();
            }
            catch
            {
                return -1;
            }
        }

        //wrench_alam表增加一条记录
        public static int AddWrenchAlam(DSWrenchAlam wrenchAlam)
        {
            try
            {
                //返回插入的alam_id
                return Db.Insertable(wrenchAlam).ExecuteReturnIdentity();
            }
            catch
            {
                return -1;
            }
        }

        //wrench_devc表增加一条记录
        public static int AddWrenchDevc(DSWrenchDevc wrenchDevc)
        {
            try
            {
                //判断wlan_id是否已存对应的记录
                if (GetWrenchWlanByWlanId(wrenchDevc.WlanId) == null)
                {
                    throw new Exception("wlan_id不存在");
                }

                //判断alam_id是否已存对应的记录
                if (GetWrenchAlamByAlamId(wrenchDevc.AlamId) == null)
                {
                    throw new Exception("alam_id不存在");
                }

                //返回插入的wid
                return Db.Insertable(wrenchDevc).ExecuteReturnIdentity();
            }
            catch
            {
                return -1;
            }
        }

        //wrench_para表增加一条记录
        public static int AddWrenchPara(DSWrenchPara wrenchPara)
        {
            try
            {
                //Para表的wid不是自增主键，需要保证devc表中有对应的wid
                if (GetWrenchDevcByWid(wrenchPara.Wid) == null)
                {
                    throw new Exception("wid不存在");
                }

                //返回插入的wid
                return Db.Insertable(wrenchPara).ExecuteReturnIdentity();
            }
            catch
            {
                return -1;
            }
        }

        //wrench_work表增加一条记录
        public static int AddWrenchWork(DSWrenchWork wrenchWork)
        {
            try
            {
                //Para表的wid不是自增主键，需要保证devc表中有对应的wid
                if (GetWrenchDevcByWid(wrenchWork.Wid) == null)
                {
                    throw new Exception("wid不存在");
                }

                //返回插入的wid
                return Db.Insertable(wrenchWork).ExecuteReturnIdentity();
            }
            catch
            {
                return -1;
            }
        }

        //同时为wrench_devc、wrench_para、wrench_work、wrench_wlan、wrench_alan五表各增加一条记录
        public static int AddWrench(DSWrenchDevc wrenchDevc, DSWrenchPara wrenchPara, DSWrenchWork wrenchWork, DSWrenchWlan wrenchWlan, DSWrenchAlam wrenchAlam)
        {
            try
            {
                uint wlan_id = 0;
                //判断wlan表里是否已有相同的记录
                if (!IsExistWrenchWlan(wrenchWlan))
                {
                    //向wrench_wlan表增加一条记录
                    wlan_id = (uint)AddWrenchWlan(wrenchWlan);
                }
                else
                {
                    wlan_id = GetWrenchWlanByDetails(wrenchWlan).WlanId;
                }

                uint alam_id = 0;
                //判断alam表里是否已有相同的记录
                if (!IsExistWrenchAlam(wrenchAlam))
                {
                    //向wrench_alam表增加一条记录
                    alam_id = (uint)AddWrenchAlam(wrenchAlam);
                }
                else
                {
                    alam_id = GetWrenchAlamByDetails(wrenchAlam).AlamId;
                }

                wrenchDevc.WlanId = wlan_id;
                wrenchDevc.AlamId = alam_id;

                //向wrench_devc表增加一条记录,并获取wid
                uint wid = (uint)AddWrenchDevc(wrenchDevc);

                //再wrench_para、wrench_work二表各增加一条记录
                wrenchPara.Wid = wid;
                wrenchWork.Wid = wid;
                return AddWrenchPara(wrenchPara) + AddWrenchWork(wrenchWork);
            }
            catch
            {
                return -1;
            }
        }

        #endregion

        #region 删除

        //依据wlan_id删除wrench_wlan表记录
        public static int DeleteWrenchWlan(uint wlan_id)
        {
            try
            {
                return Db.Deleteable<DSWrenchWlan>().Where(it => it.WlanId == wlan_id).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        //依据alam_id删除wrench_alam表记录
        public static int DeleteWrenchAlam(uint alam_id)
        {
            try
            {
                return Db.Deleteable<DSWrenchAlam>().Where(it => it.AlamId == alam_id).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        //依据wid删除一条扳手记录，包括wrench_devc、wrench_para、wrench_work三表
        public static int DeleteWrench(uint wid)
        {
            try
            {
                return DeleteWrenchDevc(wid) + DeleteWrenchPara(wid) + DeleteWrenchWork(wid);
            }
            catch
            {
                return -1;
            }
        }

        //依据wid从wrench_devc表删除一条记录
        public static int DeleteWrenchDevc(uint wid)
        {
            try
            {
                //返回删除的记录数
                return Db.Deleteable<DSWrenchDevc>().Where(it => it.Wid == wid).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        //依据wid从wrench_para表删除一条记录
        public static int DeleteWrenchPara(uint wid)
        {
            try
            {
                //返回删除的记录数
                return Db.Deleteable<DSWrenchPara>().Where(it => it.Wid == wid).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        //依据wid从wrench_work表删除一条记录  
        public static int DeleteWrenchWork(uint wid)
        {
            try
            {
                //返回删除的记录数
                return Db.Deleteable<DSWrenchWork>().Where(it => it.Wid == wid).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        #endregion

        #region 查询

        //查询wrench_devc表的所有数据
        public static List<DSWrenchDevc> GetAllWrenchDevc()
        {
            try
            {
                var list = Db.Queryable<DSWrenchDevc>().ToList();
                return list;
            }
            catch
            {
                return null;
            }
        }

        //依据wid查询wrench_devc表的数据
        public static DSWrenchDevc GetWrenchDevcByWid(uint wid)
        {
            try
            {
                var list = Db.Queryable<DSWrenchDevc>().Where(it => it.Wid == wid).ToList();
                return list.Count > 0 ? list[0] : null;
            }
            catch
            {
                return null;
            }
        }

        //依据bohrcode查询wid
        public static uint GetWidByBohrcode(ulong bohrcode)
        {
            try
            {
                var list = Db.Queryable<DSWrenchDevc>().Where(it => it.BohrCode == bohrcode).ToList();
                return list.Count > 0 ? list[0].Wid : 0;
            }
            catch
            {
                return 0;
            }
        }

        //依据wid查询对应的addr
        public static byte GetAddrByWid(uint wid)
        {
            try
            {
                //获取对应的wlan_id
                var wlan_id = GetWlanIdByWid(wid);

                //获取对应的wlan记录
                var wrenchWlan = GetWrenchWlanByWlanId(wlan_id);

                return wrenchWlan.Addr;
            }
            catch
            {
                return 0;
            }
        }

        //查询wrench_para表的所有数据
        public static List<DSWrenchPara> GetAllWrenchPara()
        {
            try
            {
                var list = Db.Queryable<DSWrenchPara>().ToList();
                return list;
            }
            catch
            {
                return null;
            }
        }

        //查询wrench_work表的所有数据
        public static List<DSWrenchWork> GetAllWrenchWork()
        {
            try
            {
                var list = Db.Queryable<DSWrenchWork>().ToList();
                return list;
            }
            catch
            {
                return null;
            }
        }

        //查询wrench_wlan表的所有数据
        public static List<DSWrenchWlan> GetAllWrenchWlan()
        {
            try
            {
                var list = Db.Queryable<DSWrenchWlan>().ToList();
                return list;
            }
            catch
            {
                return null;
            }
        }

        //依据wid查询wlan_id
        public static uint GetWlanIdByWid(uint wid)
        {
            try
            {
                var list = Db.Queryable<DSWrenchDevc>().Where(it => it.Wid == wid).ToList();
                return list.Count > 0 ? list[0].WlanId : 0;
            }
            catch
            {
                return 0;
            }
        }

        //依据wlan_id查询wrench_wlan表的记录
        public static DSWrenchWlan GetWrenchWlanByWlanId(uint wlan_id)
        {
            try
            {
                var list = Db.Queryable<DSWrenchWlan>().Where(it => it.WlanId == wlan_id).ToList();
                return list.Count > 0 ? list[0] : null;
            }
            catch
            {
                return null;
            }
        }

        //查询wrench_wlan表中是否存在name、addr、rf_chan、rf_option、rf_para、wf_ssid、wf_pwd、wf_ip、wf_port相同的记录
        public static bool IsExistWrenchWlan(DSWrenchWlan wrenchWlan)
        {
            try
            {
                var list = Db.Queryable<DSWrenchWlan>()
                             .Where(it => it.Name == wrenchWlan.Name &&
                                    it.Addr == wrenchWlan.Addr &&
                                    it.RfChan == wrenchWlan.RfChan &&
                                    it.RfOption == wrenchWlan.RfOption &&
                                    it.RfPara == wrenchWlan.RfPara &&
                                    it.Baud == wrenchWlan.Baud &&
                                    it.Stopbit == wrenchWlan.Stopbit &&
                                    it.Parity == wrenchWlan.Parity &&
                                    it.WifiMode == wrenchWlan.WifiMode &&
                                    it.WFSsid == wrenchWlan.WFSsid &&
                                    it.WFPwd == wrenchWlan.WFPwd &&
                                    it.WFIp == wrenchWlan.WFIp &&
                                    it.WFPort == wrenchWlan.WFPort)
                             .ToList();
                return list.Count > 0;
            }
            catch
            {
                return false;
            }
        }

        //依据name、addr、rf_chan、rf_option、rf_para、wf_ssid、wf_pwd、wf_ip、wf_port查询wrench_wlan表的记录
        public static DSWrenchWlan GetWrenchWlanByDetails(DSWrenchWlan wrenchWlan)
        {
            try
            {
                var list = Db.Queryable<DSWrenchWlan>()
                             .Where(it => it.Name == wrenchWlan.Name &&
                                          it.Addr == wrenchWlan.Addr &&
                                          it.RfChan == wrenchWlan.RfChan &&
                                          it.RfOption == wrenchWlan.RfOption &&
                                          it.RfPara == wrenchWlan.RfPara &&
                                          it.Baud == wrenchWlan.Baud &&
                                          it.Stopbit == wrenchWlan.Stopbit &&
                                          it.Parity == wrenchWlan.Parity &&
                                          it.WifiMode == wrenchWlan.WifiMode &&
                                          it.WFSsid == wrenchWlan.WFSsid &&
                                          it.WFPwd == wrenchWlan.WFPwd &&
                                          it.WFIp == wrenchWlan.WFIp &&
                                          it.WFPort == wrenchWlan.WFPort)
                             .ToList();
                return list.Count > 0 ? list[0] : null;
            }
            catch
            {
                return null;
            }
        }

        //查询wrench_alam表的所有数据
        public static List<DSWrenchAlam> GetAllWrenchAlam()
        {
            try
            {
                var list = Db.Queryable<DSWrenchAlam>().ToList();
                return list;
            }
            catch
            {
                return null;
            }
        }

        //依据wid查询alam_id
        public static uint GetAlamIdByWid(uint wid)
        {
            try
            {
                var list = Db.Queryable<DSWrenchDevc>().Where(it => it.Wid == wid).ToList();
                return list.Count > 0 ? list[0].AlamId : 0;
            }
            catch
            {
                return 0;
            }
        }

        //依据alam_id查询wrench_alam表的记录
        public static DSWrenchAlam GetWrenchAlamByAlamId(uint alam_id)
        {
            try
            {
                var list = Db.Queryable<DSWrenchAlam>().Where(it => it.AlamId == alam_id).ToList();
                return list.Count > 0 ? list[0] : null;
            }
            catch
            {
                return null;
            }
        }

        //查询alam表中是否存在name、en_target、en_pre、ea_ang、sn_target、sa_pre、sa_ang、mn_low、mn_high、ma_pre、ma_low、ma_high相同的记录
        public static bool IsExistWrenchAlam(DSWrenchAlam wrenchAlam)
        {
            try
            {
                var list = Db.Queryable<DSWrenchAlam>()
                             .Where(it => it.Name == wrenchAlam.Name &&
                                          it.EnTarget == wrenchAlam.EnTarget &&
                                          it.EnPre == wrenchAlam.EnPre &&
                                          it.EaAng == wrenchAlam.EaAng &&
                                          it.SnTarget == wrenchAlam.SnTarget &&
                                          it.SaPre == wrenchAlam.SaPre &&
                                          it.SaAng == wrenchAlam.SaAng &&
                                          it.MnLow == wrenchAlam.MnLow &&
                                          it.MnHigh == wrenchAlam.MnHigh &&
                                          it.MaPre == wrenchAlam.MaPre &&
                                          it.MaLow == wrenchAlam.MaLow &&
                                          it.MaHigh == wrenchAlam.MaHigh)
                             .ToList();
                return list.Count > 0;
            }
            catch
            {
                return false;
            }
        }

        //依据name、en_target、en_pre、ea_ang、sn_target、sa_pre、sa_ang、mn_low、mn_high、ma_pre、ma_low、ma_high查询wrench_alam表的记录
        public static DSWrenchAlam GetWrenchAlamByDetails(DSWrenchAlam wrenchAlam)
        {
            try
            {
                var list = Db.Queryable<DSWrenchAlam>()
                             .Where(it => it.Name == wrenchAlam.Name &&
                                          it.EnTarget == wrenchAlam.EnTarget &&
                                          it.EnPre == wrenchAlam.EnPre &&
                                          it.EaAng == wrenchAlam.EaAng &&
                                          it.SnTarget == wrenchAlam.SnTarget &&
                                          it.SaPre == wrenchAlam.SaPre &&
                                          it.SaAng == wrenchAlam.SaAng &&
                                          it.MnLow == wrenchAlam.MnLow &&
                                          it.MnHigh == wrenchAlam.MnHigh &&
                                          it.MaPre == wrenchAlam.MaPre &&
                                          it.MaLow == wrenchAlam.MaLow &&
                                          it.MaHigh == wrenchAlam.MaHigh)
                             .ToList();
                return list.Count > 0 ? list[0] : null;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region 修改

        //依据wid修改wrench_devc表的一条记录
        public static int UpdateWrenchDevc(uint wid, DSWrenchDevc wrenchDevc)
        {
            try
            {
                wrenchDevc.Wid = wid;
                //返回修改的记录数
                return Db.Updateable(wrenchDevc).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        //依据wid修改wrench_para表的一条记录
        public static int UpdateWrenchPara(uint wid, DSWrenchPara wrenchPara)
        {
            try
            {
                wrenchPara.Wid = wid;
                //返回修改的记录数
                return Db.Updateable(wrenchPara).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        //依据wid修改wrench_work表的一条记录
        public static int UpdateWrenchWork(uint wid, DSWrenchWork wrenchWork)
        {
            try
            {
                wrenchWork.Wid = wid;
                //返回修改的记录数
                return Db.Updateable(wrenchWork).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        //依据wlan_id修改wrench_wlan表的一条记录
        public static int UpdateWrenchWlan(uint wlan_id, DSWrenchWlan wrenchWlan)
        {
            try
            {
                wrenchWlan.WlanId = wlan_id;
                //返回修改的记录数
                return Db.Updateable(wrenchWlan).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        //依据alam_id修改wrench_alam表的一条记录
        public static int UpdateWrenchAlam(uint alam_id, DSWrenchAlam wrenchAlam)
        {
            try
            {
                wrenchAlam.AlamId = alam_id;
                //返回修改的记录数
                return Db.Updateable(wrenchAlam).ExecuteCommand();
            }
            catch
            {
                return -1;
            }
        }

        //依据wid修改wrench_devc、wrench_para、wrench_work、wrench_wlan、wrench_alarm五表的记录
        public static int UpdateWrench(uint wid, DSWrenchDevc wrenchDevc, DSWrenchPara wrenchPara, DSWrenchWork wrenchWork, DSWrenchWlan wrenchWlan, DSWrenchAlam wrenchAlam)
        {
            try
            {
                uint wlan_id = 0;
                //判断wlan表里是否已有相同的记录
                if (!IsExistWrenchWlan(wrenchWlan))
                {
                    //向wrench_wlan表增加一条记录
                    wlan_id = (uint)AddWrenchWlan(wrenchWlan);
                }
                else
                {
                    wlan_id = GetWrenchWlanByDetails(wrenchWlan).WlanId;
                }

                uint alam_id = 0;
                //判断alam表里是否已有相同的记录
                if (!IsExistWrenchAlam(wrenchAlam))
                {
                    //向wrench_alam表增加一条记录
                    alam_id = (uint)AddWrenchAlam(wrenchAlam);
                }
                else
                {
                    alam_id = GetWrenchAlamByDetails(wrenchAlam).AlamId;
                }

                wrenchDevc.WlanId = wlan_id;
                wrenchDevc.AlamId = alam_id;

                //更新wrench_devc、wrench_para、wrench_work三表
                return UpdateWrenchDevc(wid, wrenchDevc) + UpdateWrenchPara(wid, wrenchPara) + UpdateWrenchWork(wid, wrenchWork);
            }
            catch
            {
                return -1;
            }
        }

        #endregion
    }
}
