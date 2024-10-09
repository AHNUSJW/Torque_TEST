using System;
using System.ComponentModel;

//Alvin,20240221

//在硬件程序中不存在
//上位机软件需要使用的enum和const数值

namespace Model
{
    public enum COMP  //通讯方式
    {
        [Description("UART")] UART,
        [Description("TCP")] TCP,
        [Description("XF")] XF,
        [Description("RS485")] RS485,
    }

    public enum STATE : Byte //连接状态
    {
        [Description("未找到")] INVALID,   //无效,未初始化设备,不需要在界面展示设备信息和数据
        [Description("已连接")] CONNECTED, //已连接,SCAN和BOR成功的设备,可以读SCT参数和其它操作
        [Description("工作中")] WORKING,   //正常工作,RDX完成的设备,可以刷新界面设备信息和数据
        [Description("已掉线")] OFFLINE,   //掉线,刷新界面数据时没有通讯反应,不更新界面,也不调整界面
    }

    public enum TASKS : Byte //通讯状态机
    {
        NULL,

        WRITE_ZERO,         	//只写,归零
        WRITE_POWEROFF,         //只写,关机
        WRITE_KEYLOCK,          //只写,锁按键
        WRITE_FIFOCLEAR,        //只连续写,请FIFO缓存大小
        WRITE_MEMABLE,          //只写,打开硬件参数包含
        WRITE_RESET,            //只写，重启设备
        WRITE_CALVITO,          //只写，更新ad_point后计算斜率

        WRITE_FIFO_INDEX,       //只连续写

        REG_BLOCK1_DEV,         //只读,标定信息
        REG_BLOCK4_CAL1,        //读写
        REG_BLOCK5_CAL2,        //读写
        REG_BLOCK5_INFO,        //读写
        REG_BLOCK3_WLAN,        //读写
        REG_BLOCK3_PARA,        //读写
        REG_BLOCK5_AM1,         //读写
        REG_BLOCK5_AM2,         //读写
        REG_BLOCK5_AM3,         //读写
        REG_BLOCK3_JOB,         //读写
        REG_BLOCK3_OP,          //读写
        REG_BLOCK3_SCREW1,      //读写
        REG_BLOCK3_SCREW2,      //读写
        REG_BLOCK3_SCREW3,      //读写
        REG_BLOCK3_SCREW4,      //读写
        REG_BLOCK1_FIFO,        //只读+读写
        REG_BLOCK2_DAT,         //只读

        //****************针对蓝牙接收器******************//
        REG_R_BLUETOOTH_UNBIND,
        REG_W_BLUETOOTH_UNBIND,
    }

    public static class Constants
    {
        public const Byte       UNITS               = 5;

        public const UInt16     RxSize              = 8192;
        public const UInt16     TxSize              = 2048;

        public const UInt16     REG_BLOCK1_DEV      = (UInt16)REG.REG_R_SERIES;
        public const UInt16     REG_BLOCK4_CAL1     = (UInt16)REG.REG_WR_CAL_UNIT;
        public const UInt16     REG_BLOCK5_CAL2     = (UInt16)REG.REG_WR_CL2_ZERO;
        public const UInt16     REG_BLOCK5_INFO     = (UInt16)REG.REG_WR_SRNO;
        public const UInt16     REG_BLOCK4_WLAN     = (UInt16)REG.REG_WR_ADDR;
        public const UInt16     REG_BLOCK2_PARA     = (UInt16)REG.REG_WR_TORQUE_UNIT;
        public const UInt16     REG_BLOCK5_AM1      = (UInt16)REG.REG_WR_SN_TARGET0;
        public const UInt16     REG_BLOCK5_AM2      = (UInt16)REG.REG_WR_MN_LOW5;
        public const UInt16     REG_BLOCK5_AM3      = (UInt16)REG.REG_WR_AZ_START0;
        public const UInt16     REG_BLOCK3_JOB      = (UInt16)REG.REG_WR_WO_AREA;
        public const UInt16     REG_BLOCK3_OP       = (UInt16)REG.REG_WR_USER_ID;
        public const UInt16     REG_BLOCK3_SCREW1   = (UInt16)REG.REG_WR_SCREWORDER0;
        public const UInt16     REG_BLOCK3_SCREW2   = (UInt16)REG.REG_WR_SCREWORDER48;
        public const UInt16     REG_BLOCK3_SCREW3   = (UInt16)REG.REG_WR_SCREWORDER96;
        public const UInt16     REG_BLOCK3_SCREW4   = (UInt16)REG.REG_WR_SCREWORDER144;
        public const UInt16     REG_BLOCK1_FIFO     = (UInt16)REG.REG_R_FIFO_FULL;
        public const UInt16     REG_BLOCK2_DAT      = (UInt16)REG.REG_R_RECDAT;

        public const UInt16     REG_BLUETOOTH_UNBIND = (UInt16)REG.REG_WR_BLUETOOTH_UNBIND;
    }
}