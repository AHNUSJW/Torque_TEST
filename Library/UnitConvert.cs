using System;
using System.ComponentModel;

//Ricardo 20240308

namespace Library
{
    public class UnitConvert
    {
        #region 扭矩单位换算

        //扭矩常用单位
        public enum UNIT : Byte  //单位
        {
            [Description("N·m")]    UNIT_nm    = 0,
            [Description("lbf·in")] UNIT_lbfin = 1,
            [Description("lbf·ft")] UNIT_lbfft = 2,
            [Description("kgf·cm")] UNIT_kgcm  = 3,
            [Description("kgf·m")]  UNIT_kgm   = 4,
        }


        //N·m 转为其他单位
        public static Int32 Torque_nmTrans(Int32 torque, Byte unit)
        {
            switch (unit)
            {
                case (byte)UNIT.UNIT_nm:
                    return torque;
                case (byte)UNIT.UNIT_lbfin:
                    return (int)(torque * 8.85075f + 0.5f);
                case (byte)UNIT.UNIT_lbfft:
                    return (int)(torque * 0.73756214927727f + 0.5f);
                case (byte)UNIT.UNIT_kgcm:
                    return (int)(torque * 10.1971621f + 0.5f);
                case (byte)UNIT.UNIT_kgm:
                    return (int)(torque * 0.101971621f + 0.5f);
                default:
                    return 0;
            }
        }

        //lbf·in 转为其他单位
        public static Int32 Torque_lbfinTrans(Int32 torque, byte unit)
        {
            switch (unit)
            {
                case (byte)UNIT.UNIT_nm:
                    return (int)(torque * 0.11298477530153f + 0.5f);
                case (byte)UNIT.UNIT_lbfin:
                    return torque;
                case (byte)UNIT.UNIT_lbfft:
                    return (int)(torque * 0.083333293707f + 0.5f);
                case (byte)UNIT.UNIT_kgcm:
                    return (int)(torque * 1.152124068582f + 0.5f);
                case (byte)UNIT.UNIT_kgm:
                    return (int)(torque * 0.01152124068582f + 0.5f);
                default:
                    return 0;
            }
        }

        //lbf·ft 转为其他单位
        public static Int32 Torque_lbfftTrans(Int32 torque, byte unit)
        {
            switch (unit)
            {
                case (byte)UNIT.UNIT_nm:
                    return (int)(torque * 1.3558179483314f + 0.5f);
                case (byte)UNIT.UNIT_lbfin:
                    return (int)(torque * 12.0000057061941f + 0.5f);
                case (byte)UNIT.UNIT_lbfft:
                    return torque;
                case (byte)UNIT.UNIT_kgcm:
                    return (int)(torque * 13.8254953972247f + 0.5f);
                case (byte)UNIT.UNIT_kgm:
                    return (int)(torque * 0.138254953972247f + 0.5f);
                default:
                    return 0;
            }
        }

        //kgf·cm 转为其他单位
        public static Int32 Torque_kgfcmTrans(Int32 torque, byte unit)
        {
            switch (unit)
            {
                case (byte)UNIT.UNIT_nm:
                    return (int)(torque * 0.0980665002863885f + 0.5f);
                case (byte)UNIT.UNIT_lbfin:
                    return (int)(torque * 0.867962077409753f + 0.5f);
                case (byte)UNIT.UNIT_lbfft:
                    return (int)(torque * 0.0723301387233283f + 0.5f);
                case (byte)UNIT.UNIT_kgcm:
                    return torque;
                case (byte)UNIT.UNIT_kgm:
                    return torque / 100;
                default:
                    return 0;
            }
        }

        //kgf·m 转为其他单位
        public static Int32 Torque_kgfmTrans(Int32 torque, byte unit)
        {
            switch (unit)
            {
                case (byte)UNIT.UNIT_nm:
                    return (int)(torque * 9.80665002863885f + 0.5f);
                case (byte)UNIT.UNIT_lbfin:
                    return (int)(torque * 86.7962077409753f + 0.5f);
                case (byte)UNIT.UNIT_lbfft:
                    return (int)(torque * 7.23301387233283f + 0.5f);
                case (byte)UNIT.UNIT_kgcm:
                    return torque * 100;
                case (byte)UNIT.UNIT_kgm:
                    return torque;
                default:
                    return 0;
            }
        }

        //其他单位转为 N·m
        public static Int32 TorqueTransNm(Int32 torque, byte unit)
        {
            switch (unit)
            {
                case (byte)UNIT.UNIT_nm:
                    return torque;
                case (byte)UNIT.UNIT_lbfin:
                    return (int)(torque * 0.11298477530153f + 0.5f);
                case (byte)UNIT.UNIT_lbfft:
                    return ((int)(torque * 1.3558179483314f + 0.5f));
                case (byte)UNIT.UNIT_kgcm:
                    return (int)(torque * 0.0980665002863885f + 0.5f);
                case (byte)UNIT.UNIT_kgm:
                    return (int)(torque * 9.80665002863885f + 0.5f);
                default:
                    return 0;
            }
        }

        //其他单位转为 lbf·in
        public static Int32 TorqueTransLbfin(Int32 torque, byte unit)
        {
            switch (unit)
            {
                case (byte)UNIT.UNIT_nm:
                    return (int)(torque * 8.85075f + 0.5f);
                case (byte)UNIT.UNIT_lbfin:
                    return torque;
                case (byte)UNIT.UNIT_lbfft:
                    return ((int)(torque * 12.0000057061941f + 0.5f));
                case (byte)UNIT.UNIT_kgcm:
                    return (int)(torque * 0.867962077409753f + 0.5f);
                case (byte)UNIT.UNIT_kgm:
                    return (int)(torque * 86.7962077409753f + 0.5f);
                default:
                    return 0;
            }
        }

        //其他单位转为 lbf·ft
        public static Int32 TorqueTransLbfft(Int32 torque, byte unit)
        {
            switch (unit)
            {
                case (byte)UNIT.UNIT_nm:
                    return (int)(torque * 0.73756214927727f + 0.5f);
                case (byte)UNIT.UNIT_lbfin:
                    return (int)(torque * 0.083333293707f + 0.5f);
                case (byte)UNIT.UNIT_lbfft:
                    return torque;
                case (byte)UNIT.UNIT_kgcm:
                    return (int)(torque * 0.0723301387233283f + 0.5f);
                case (byte)UNIT.UNIT_kgm:
                    return (int)(torque * 7.23301387233283f + 0.5f);
                default:
                    return 0;
            }
        }

        //其他单位转为 kgf·cm
        public static Int32 TorqueTransKgfcm(Int32 torque, byte unit)
        {
            switch (unit)
            {
                case (byte)UNIT.UNIT_nm:
                    return (int)(torque * 10.1971621f + 0.5f);
                case (byte)UNIT.UNIT_lbfin:
                    return (int)(torque * 1.152124068582f + 0.5f);
                case (byte)UNIT.UNIT_lbfft:
                    return ((int)(torque * 13.8254953972247f + 0.5f));
                case (byte)UNIT.UNIT_kgcm:
                    return torque;
                case (byte)UNIT.UNIT_kgm:
                    return torque * 100;
                default:
                    return 0;
            }
        }

        //其他单位转为 kgf·m
        public static Int32 TorqueTransKgfm(Int32 torque, byte unit)
        {
            switch (unit)
            {
                case (byte)UNIT.UNIT_nm:
                    return (int)(torque * 0.11971621f + 0.5f);
                case (byte)UNIT.UNIT_lbfin:
                    return (int)(torque * 0.01152124068582f + 0.5f);
                case (byte)UNIT.UNIT_lbfft:
                    return ((int)(torque * 0.138254953972247f + 0.5f));
                case (byte)UNIT.UNIT_kgcm:
                    return torque / 100;
                case (byte)UNIT.UNIT_kgm:
                    return torque;
                default:
                    return 0;
            }
        }

        #endregion

        #region 时间单位换算

        // 获取时间戳(秒）
        public static string GetTimeStamp()
        {
            TimeSpan ts = DateTime.Now - TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

        // 时间戳（秒）转换成时间
        public static DateTime GetTime(string time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));//当地时区
            return startTime.AddSeconds(Convert.ToInt64(time));
        }

        // 获取时间戳(毫秒）
        public static string GetMilTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds).ToString();
        }

        // 时间（毫秒）转换成时间戳
        public static string GetTimeStamp(DateTime time)
        {
            //东八区时间偏差8小时
            TimeSpan ts = time - new DateTime(1970, 1, 1, 8, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64(ts.TotalMilliseconds).ToString();
        }

        // 时间戳（毫秒）转换成时间
        public static DateTime GetMilTime(string time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));//当地时区  
            return startTime.AddMilliseconds(Convert.ToInt64(time));
        }


        #endregion
    }
}
