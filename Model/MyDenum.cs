using System;
using System.ComponentModel;

//Alvin,20240221

//数值来源于硬件程序
//保持统一并同步更新

namespace Model
{
    public enum SERIES : Byte   //系列
    {
        TQ_BX                   = 0x01,
        TQ_KES                  = 0x02,
        TQ_BEM                  = 0x03,
        TQ_XLT                  = 0x04,
        TQ_XH                   = 0x05,
    }

    public enum ADDROFFSET : UInt16   //偏移量（用于区分不同系列产品指令）
    {
        TQ_BX_ADDR              = 0x0100,
        TQ_KES_ADDR             = 0x0200,
        TQ_BEM_ADDR             = 0x0300,
        TQ_XL_ADDRT             = 0x0400,
        TQ_XH_ADDR              = 0x0500,
    }

    public enum TYPE : UInt16    //产品型号
    {
        TQ_BEL                  = 0x0101,
        TQ_BEX                  = 0x0102,
        TQ_BFL4                 = 0x0103,
        TQ_BFX4                 = 0x0104,

        TQ_KES_51               = 0x0251,
        TQ_KES_61               = 0x0261,
        TQ_KES_62               = 0x0262,
        TQ_KES_71               = 0x0271,
        TQ_KES_72               = 0x0272,
        TQ_KES_80               = 0x0280,
        TQ_KES_81               = 0x0281,

        TQ_BEM1                 = 0x0301,
        TQ_BEM1G                = 0x0302,
        TQ_BEM1S                = 0x0303,
        TQ_BEM4                 = 0x0304,
        TQ_BEM4G                = 0x0305,
        TQ_BEM4S                = 0x0306,
        TQ_BEM4WF               = 0x0307,
        TQ_BEM4RF               = 0x0308,

        TQ_XLTB                 = 0x0401,
        TQ_XLTL_01              = 0x0402,
        TQ_XLTS_01              = 0x0403,
        TQ_XLTL_02              = 0x0404,
        TQ_XLTS_02              = 0x0405,
        TQ_XLTL_03              = 0x0406,
        TQ_XLTS_03              = 0x0407,

        TQ_XH_XL01_05           = 0x0505,
        TQ_XH_XL01_06           = 0x0506,
        TQ_XH_XL01_07           = 0x0507,
        TQ_XH_XL01_08           = 0x0508,
        TQ_XH_XL01_09           = 0x0509,
    }

    public enum AUTPAR_T : Byte  //用户权限
    {
        PAR_USE                 = 0,      //普通用户
        PAR_ADMIN               = 01,     //管理员
        PAR_SUPER               = 20,     //工厂超级用户
        PAR_MFR                 = 30,     //制造商
    }

    public enum ACCPAR_T : Byte   //角度累加模式
    {
        PAR_ACC_NULL            = 0,      //不累加
        PAR_ACC_RATCHET         = 1,      //棘论扳手累加
        PAR_ACC_OPENENDED       = 2,      //开口扳手累加
    }

    public enum KEYLOCK : Byte    //按键锁
    {
        KEY_UNLOCK              = 0,      //解锁
        KEY_LOCK                = 1,      //加锁
        KEY_BUZZER              = 2,      //蜂鸣器及加锁
    }

    public enum UNIT : Byte  //单位
    {
        [Description("N·m")]    UNIT_nm     = 0,
        [Description("lbf·in")] UNIT_lbfin  = 1,
        [Description("lbf·ft")] UNIT_lbfft  = 2,
        [Description("kgf·cm")] UNIT_kgcm   = 3,
        [Description("kgf·m")]  UNIT_kgm    = 4,
    }

    public enum AUTOZERO : Byte //自动归零
    {
        ATZ0                    = 0,        //不归零0%
        ATZ2                    = 2,        //归零范围2%
        ATZ4                    = 4,        //归零范围4%
        ATZ10                   = 10,       //归零范围10%
        ATZ20                   = 20,       //归零范围20%
        ATZ50                   = 50,       //归零范围50%
    }

    public enum TRACKZERO : Byte //零点跟踪
    {
        TKZ0                    = 0,        //无零点跟踪
        TKZ1                    = 1,        //零点跟踪0.5字
        TKZ2                    = 2,        //零点跟踪1字
        TKZ4                    = 4,        //零点跟踪2字
        TKZ8                    = 8,        //零点跟踪4字
        TKZ30                   = 30,       //零点跟踪4字
    }

    public enum CMD : Byte //功能码
    {
        CMD_NULL                = 0x00,     //
        CMD_READ                = 0x03,     //读
        CMD_WRITE               = 0x06,     //写
        CMD_SEQUENCE            = 0x10,     //连续写
        CMD_SET                 = 0x09,     //
        CMD_CONFIG              = 0x0C,     //
    }

    public enum REG : UInt16
    {
        REG_W_ZERO              = 0x0001,   //清零
        REG_W_POWEROFF          = 0x0002,   //关机
        REG_W_KEYLOCK           = 0x0003,   //操作锁定
        REG_W_FIFOCLEAR         = 0x0004,   //清fifo数据,带删除的长度
        REG_W_MEMABLE           = 0x0006,   //保护不开放的字节读写
        REG_W_RESET             = 0x0007,   //重启设备，专用于改设备波特率

        REG_R_SERIES            = 0x0010,   //
        REG_R_TYPE              = 0x0011,   //
        REG_R_VERSION           = 0x0012,   //

        REG_R_BOHRCODE1         = 0x0014,   //
        REG_R_BOHRCODE2         = 0x0015,   //
        REG_R_BOHRCODE3         = 0x0016,   //
        REG_R_BOHRCODE4         = 0x0017,   //
        REG_R_BOHRCODE5         = 0x0018,   //
        REG_R_BOHRCODE6         = 0x0019,   //
        REG_R_TORQUERR          = 0x001A,   //

        //********以下需要加偏移地址********//

        REG_WR_CAL_UNIT         = 0x0020 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_CAL_TYPE         = 0x0021 + ADDROFFSET.TQ_XH_ADDR,   //扭矩量程
        REG_WR_TORQUE_DECIMAL   = 0x0022 + ADDROFFSET.TQ_XH_ADDR,   //扭矩小数点
        REG_WR_TORQUE_FDN       = 0x0023 + ADDROFFSET.TQ_XH_ADDR,   //扭矩分度值
        REG_WR_CAPACITY         = 0x0026 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AD_ZERO          = 0x0028 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AD_POS_POINT1    = 0x002A + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AD_POS_POINT2    = 0x002C + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AD_POS_POINT3    = 0x002E + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AD_POS_POINT4    = 0x0030 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AD_POS_POINT5    = 0x0032 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AD_NEG_POINT1    = 0x0034 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AD_NEG_POINT2    = 0x0036 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AD_NEG_POINT3    = 0x0038 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AD_NEG_POINT4    = 0x003A + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AD_NEG_POINT5    = 0x003C + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_TQ_POS_POINT1    = 0x003E + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_TQ_POS_POINT2    = 0x0040 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_TQ_POS_POINT3    = 0x0042 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_TQ_POS_POINT4    = 0x0044 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_TQ_POS_POINT5    = 0x0046 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_TQ_NEG_POINT1    = 0x0048 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_TQ_NEG_POINT2    = 0x004A + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_TQ_NEG_POINT3    = 0x004C + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_TQ_NEG_POINT4    = 0x004E + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_TQ_NEG_POINT5    = 0x0050 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_TORQUE_DISP      = 0x0052 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_TORQUE_MIN       = 0x0054 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_TORQUE_MAX       = 0x0056 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_TORQUE_OVER      = 0x0058 + ADDROFFSET.TQ_XH_ADDR,   //

        REG_WR_SRNO             = 0x0060 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_NUMBER           = 0x0062 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MFGTIME          = 0x0064 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_CALTIME          = 0x0066 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_CALREMIND        = 0x0068 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_NAME0            = 0x0070 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_NAME1            = 0x0071 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_NAME2            = 0x0072 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_NAME3            = 0x0073 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_NAME4            = 0x0074 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_NAME5            = 0x0075 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_NAME6            = 0x0076 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_NAME7            = 0x0077 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_NAME8            = 0x0078 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_NAME9            = 0x0079 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_NAME10           = 0x007A + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_NAME11           = 0x007B + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_NAME12           = 0x007C + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_NAME13           = 0x007D + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_NAME14           = 0x007E + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_NAME15           = 0x007F + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MANAGETXT0       = 0x0080 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MANAGETXT1       = 0x0081 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MANAGETXT2       = 0x0082 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MANAGETXT3       = 0x0083 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MANAGETXT4       = 0x0084 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MANAGETXT5       = 0x0085 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MANAGETXT6       = 0x0086 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MANAGETXT7       = 0x0087 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MANAGETXT8       = 0x0088 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MANAGETXT9       = 0x0089 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MANAGETXT10      = 0x008A + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MANAGETXT11      = 0x008B + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MANAGETXT12      = 0x008C + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MANAGETXT13      = 0x008D + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MANAGETXT14      = 0x008E + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MANAGETXT15      = 0x008F + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_DECRIPTION0      = 0x0090 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_DECRIPTION1      = 0x0091 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_DECRIPTION2      = 0x0092 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_DECRIPTION3      = 0x0093 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_DECRIPTION4      = 0x0094 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_DECRIPTION5      = 0x0095 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_DECRIPTION6      = 0x0096 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_DECRIPTION7      = 0x0097 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_DECRIPTION8      = 0x0098 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_DECRIPTION9      = 0x0099 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_DECRIPTION10     = 0x009A + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_DECRIPTION11     = 0x009B + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_DECRIPTION12     = 0x009C + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_DECRIPTION13     = 0x009D + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_DECRIPTION14     = 0x009E + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_DECRIPTION15     = 0x009F + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_DECRIPTION16     = 0x00A0 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_DECRIPTION17     = 0x00A1 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_DECRIPTION18     = 0x00A2 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_DECRIPTION19     = 0x00A3 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_DECRIPTION20     = 0x00A4 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_DECRIPTION21     = 0x00A5 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_DECRIPTION22     = 0x00A6 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_DECRIPTION23     = 0x00A7 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_DECRIPTION24     = 0x00A8 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_DECRIPTION25     = 0x00A9 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_DECRIPTION26     = 0x00AA + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_DECRIPTION27     = 0x00AB + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_DECRIPTION28     = 0x00AC + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_DECRIPTION29     = 0x00AD + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_DECRIPTION30     = 0x00AE + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_DECRIPTION31     = 0x00AF + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_RF_CHAN          = 0x00B0 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_RF_OPTION        = 0x00B1 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_RF_PARA          = 0x00B2 + ADDROFFSET.TQ_XH_ADDR,   //

        REG_WR_WF_SSID0         = 0x00C0 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WF_SSID1         = 0x00C1 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WF_SSID2         = 0x00C2 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WF_SSID3         = 0x00C3 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WF_SSID4         = 0x00C4 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WF_SSID5         = 0x00C5 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WF_SSID6         = 0x00C6 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WF_SSID7         = 0x00C7 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WF_SSID8         = 0x00C8 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WF_SSID9         = 0x00C9 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WF_SSID10        = 0x00CA + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WF_SSID11        = 0x00CB + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WF_SSID12        = 0x00CC + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WF_SSID13        = 0x00CD + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WF_SSID14        = 0x00CE + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WF_SSID15        = 0x00CF + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WF_PWD0          = 0x00D0 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WF_PWD1          = 0x00D1 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WF_PWD2          = 0x00D2 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WF_PWD3          = 0x00D3 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WF_PWD4          = 0x00D4 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WF_PWD5          = 0x00D5 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WF_PWD6          = 0x00D6 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WF_PWD7          = 0x00D7 + ADDROFFSET.TQ_XH_ADDR,   //

        REG_WR_ADDR             = 0x00E0 + ADDROFFSET.TQ_XH_ADDR,   //

        REG_WR_WF_IP0           = 0x00E2 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WF_IP1           = 0x00E3 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WF_IP2           = 0x00E4 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WF_IP3           = 0x00E5 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WF_PORT          = 0x00E6 + ADDROFFSET.TQ_XH_ADDR,   //

        REG_WR_TORQUE_UNIT      = 0x00F0 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_ANGLE_SPEED      = 0x00F1 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_ANGLE_DECIMAL    = 0x00F2 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MODE_PT          = 0x00F3 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MODE_AX          = 0x00F4 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MODE_MX          = 0x00F5 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_FIFOMODE         = 0x00F6 + ADDROFFSET.TQ_XH_ADDR,   //缓存满覆盖还是不覆盖
        REG_WR_FIFOREC          = 0x00F7 + ADDROFFSET.TQ_XH_ADDR,   //实时缓存还是峰值缓存
        REG_WR_FIFOSPEED        = 0x00F8 + ADDROFFSET.TQ_XH_ADDR,   //实时缓存速率
        REG_WR_HEARTFORMAT      = 0x00F9 + ADDROFFSET.TQ_XH_ADDR,   //心跳数据格式
        REG_WR_HEARTCOUNT       = 0x00FA + ADDROFFSET.TQ_XH_ADDR,   //心跳回复帧数,最小1
        REG_WR_HEARTCYCLE       = 0x00FB + ADDROFFSET.TQ_XH_ADDR,   //心跳间隔,最小20ms
        REG_WR_ACCMODE          = 0x00FC + ADDROFFSET.TQ_XH_ADDR,   //是否关闭角度累加
        REG_WR_ALARMODE         = 0x00FD + ADDROFFSET.TQ_XH_ADDR,   //是否关闭声光报警省电
        REG_WR_WIFIMODE         = 0x00FE + ADDROFFSET.TQ_XH_ADDR,   //是否关闭wifi/RF无线
        REG_WR_TIMEOFF          = 0x00FF + ADDROFFSET.TQ_XH_ADDR,   //自动关机时间
        REG_WR_TIMEBACK         = 0x00F0 + ADDROFFSET.TQ_XH_ADDR,   //自动关背光时间
        REG_WR_TIMEZERO         = 0x0101 + ADDROFFSET.TQ_XH_ADDR,   //自动归零时间
        REG_WR_DISPTYPE         = 0x0102 + ADDROFFSET.TQ_XH_ADDR,   //横屏,竖屏
        REG_WR_DISPTHEME        = 0x0103 + ADDROFFSET.TQ_XH_ADDR,   //显示主题
        REG_WR_DISPLAN          = 0x0104 + ADDROFFSET.TQ_XH_ADDR,   //语言
        REG_WR_UNHOOK           = 0x0105 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_ADSPEED          = 0x0106 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AUTOZERO         = 0x0107 + ADDROFFSET.TQ_XH_ADDR,   //归零范围
        REG_WR_TRACKZERO        = 0x0108 + ADDROFFSET.TQ_XH_ADDR,   //零点跟踪

        REG_WR_ANGCORR          = 0x010A + ADDROFFSET.TQ_XH_ADDR,   //角度修正

        REG_WR_SN_TARGET0       = 0x0110 + ADDROFFSET.TQ_XH_ADDR,   // SN模式 预设扭矩，3字节
        REG_WR_SN_TARGET1       = 0x0112 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SN_TARGET2       = 0x0114 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SN_TARGET3       = 0x0116 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SN_TARGET4       = 0x0118 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SN_TARGET5       = 0x011A + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SN_TARGET6       = 0x011C + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SN_TARGET7       = 0x011E + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SN_TARGET8       = 0x0120 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SN_TARGET9       = 0x0122 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SA_PRE0          = 0x0124 + ADDROFFSET.TQ_XH_ADDR,   // SA模式 预设扭矩，3字节
        REG_WR_SA_ANG0          = 0x0126 + ADDROFFSET.TQ_XH_ADDR,   // SA模式 预设角度，3字节
        REG_WR_SA_PRE1          = 0x0128 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SA_ANG1          = 0x012A + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SA_PRE2          = 0x012C + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SA_ANG2          = 0x012E + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SA_PRE3          = 0x0130 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SA_ANG3          = 0x0132 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SA_PRE4          = 0x0134 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SA_ANG4          = 0x0136 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SA_PRE5          = 0x0138 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SA_ANG5          = 0x013A + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SA_PRE6          = 0x013C + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SA_ANG6          = 0x013E + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SA_PRE7          = 0x0140 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SA_ANG7          = 0x0142 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SA_PRE8          = 0x0144 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SA_ANG8          = 0x0146 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SA_PRE9          = 0x0148 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SA_ANG9          = 0x014A + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MN_LOW0          = 0x014C + ADDROFFSET.TQ_XH_ADDR,   // MN模式 预设扭矩下限，3字节
        REG_WR_MN_HIGH0         = 0x014E + ADDROFFSET.TQ_XH_ADDR,   // MN模式 预设扭矩上限，3字节
        REG_WR_MN_LOW1          = 0x0150 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MN_HIGH1         = 0x0152 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MN_LOW2          = 0x0154 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MN_HIGH2         = 0x0156 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MN_LOW3          = 0x0158 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MN_HIGH3         = 0x015A + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MN_LOW4          = 0x015C + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MN_HIGH4         = 0x015E + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MN_LOW5          = 0x0160 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MN_HIGH5         = 0x0162 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MN_LOW6          = 0x0164 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MN_HIGH6         = 0x0166 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MN_LOW7          = 0x0168 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MN_HIGH7         = 0x016A + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MN_LOW8          = 0x016C + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MN_HIGH8         = 0x016E + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MN_LOW9          = 0x0170 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MN_HIGH9         = 0x0172 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MA_PRE0          = 0x0174 + ADDROFFSET.TQ_XH_ADDR,   // MA模式 预设扭矩，3字节
        REG_WR_MA_LOW0          = 0x0176 + ADDROFFSET.TQ_XH_ADDR,   // MA模式 预设角度下限，3字节
        REG_WR_MA_HIGH0         = 0x0178 + ADDROFFSET.TQ_XH_ADDR,   // MA模式 预设角度上限，3字节
        REG_WR_MA_PRE1          = 0x017A + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MA_LOW1          = 0x017C + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MA_HIGH1         = 0x017E + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MA_PRE2          = 0x0180 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MA_LOW2          = 0x0182 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MA_HIGH2         = 0x0184 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MA_PRE3          = 0x0186 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MA_LOW3          = 0x0188 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MA_HIGH3         = 0x018A + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MA_PRE4          = 0x018C + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MA_LOW4          = 0x018E + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MA_HIGH4         = 0x0190 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MA_PRE5          = 0x0192 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MA_LOW5          = 0x0194 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MA_HIGH5         = 0x0196 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MA_PRE6          = 0x0198 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MA_LOW6          = 0x019A + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MA_HIGH6         = 0x019C + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MA_PRE7          = 0x019E + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MA_LOW7          = 0x01A0 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MA_HIGH7         = 0x01A2 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MA_PRE8          = 0x01A4 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MA_LOW8          = 0x01A6 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MA_HIGH8         = 0x01A8 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MA_PRE9          = 0x01AA + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MA_LOW9          = 0x01AC + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_MA_HIGH9         = 0x01AE + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AZ_START0        = 0x01B0 + ADDROFFSET.TQ_XH_ADDR,   // AZ模式 安装应力检测角度起始的扭矩值
        REG_WR_AZ_STOP0         = 0x01B2 + ADDROFFSET.TQ_XH_ADDR,   // AZ模式 安装应力检测角度结束的扭矩值
        REG_WR_AZ_HOCK0         = 0x01B4 + ADDROFFSET.TQ_XH_ADDR,   // AZ模式 安装应力检测的目标扭矩值
        REG_WR_AZ_START1        = 0x01B6 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AZ_STOP1         = 0x01B8 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AZ_HOCK1         = 0x01BA + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AZ_START2        = 0x01BC + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AZ_STOP2         = 0x01BE + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AZ_HOCK2         = 0x01C0 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AZ_START3        = 0x01C2 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AZ_STOP3         = 0x01C4 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AZ_HOCK3         = 0x01C6 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AZ_START4        = 0x01C8 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AZ_STOP4         = 0x01CA + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AZ_HOCK4         = 0x01CC + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AZ_START5        = 0x01CE + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AZ_STOP5         = 0x01D0 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AZ_HOCK5         = 0x01D2 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AZ_START6        = 0x01D4 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AZ_STOP6         = 0x01D6 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AZ_HOCK6         = 0x01D8 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AZ_START7        = 0x01DA + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AZ_STOP7         = 0x01DC + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AZ_HOCK7         = 0x01DE + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AZ_START8        = 0x01E0 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AZ_STOP8         = 0x01E2 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AZ_HOCK8         = 0x01E4 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AZ_START9        = 0x01E6 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AZ_STOP9         = 0x01E8 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_AZ_HOCK9         = 0x01EA + ADDROFFSET.TQ_XH_ADDR,   //

        REG_WR_WO_AREA          = 0x0300 + ADDROFFSET.TQ_XH_ADDR,   //工单区
        REG_WR_WO_FACTORY       = 0x0302 + ADDROFFSET.TQ_XH_ADDR,   //工单厂
        REG_WR_WO_LINE          = 0x0304 + ADDROFFSET.TQ_XH_ADDR,   //工单产线
        REG_WR_WO_STATION       = 0x0306 + ADDROFFSET.TQ_XH_ADDR,   //工单工位

        REG_WR_WO_STAMP         = 0x0310 + ADDROFFSET.TQ_XH_ADDR,   //工单时间标志
        REG_WR_WO_BAT           = 0x0312 + ADDROFFSET.TQ_XH_ADDR,   //工单批号
        REG_WR_WO_NUM           = 0x0314 + ADDROFFSET.TQ_XH_ADDR,   //工单编号
        REG_WR_WO_NAME0         = 0x0316 + ADDROFFSET.TQ_XH_ADDR,   //工单名称
        REG_WR_WO_NAME1         = 0x0317 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WO_NAME2         = 0x0318 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WO_NAME3         = 0x0319 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WO_NAME4         = 0x031A + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WO_NAME5         = 0x031B + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WO_NAME6         = 0x031C + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WO_NAME7         = 0x031D + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WO_NAME8         = 0x031E + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WO_NAME9         = 0x031F + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WO_NAME10        = 0x0320 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WO_NAME11        = 0x0321 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WO_NAME12        = 0x0322 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WO_NAME13        = 0x0323 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WO_NAME14        = 0x0324 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WO_NAME15        = 0x0325 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WO_NAME16        = 0x0326 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WO_NAME17        = 0x0327 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WO_NAME18        = 0x0328 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_WO_NAME19        = 0x0329 + ADDROFFSET.TQ_XH_ADDR,   //

        REG_WR_USER_ID          = 0x0330 + ADDROFFSET.TQ_XH_ADDR,   //操作工号
        REG_WR_USER_NAME0       = 0x0332 + ADDROFFSET.TQ_XH_ADDR,   //操作姓名
        REG_WR_USER_NAME1       = 0x0333 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_USER_NAME2       = 0x0334 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_USER_NAME3       = 0x0335 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_USER_NAME4       = 0x0336 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_USER_NAME5       = 0x0337 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_USER_NAME6       = 0x0338 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_USER_NAME7       = 0x0339 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_USER_NAME8       = 0x033A + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_USER_NAME9       = 0x033B + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_USER_NAME10      = 0x033C + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_USER_NAME11      = 0x033D + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_USER_NAME12      = 0x033E + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_USER_NAME13      = 0x033F + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_USER_NAME14      = 0x0340 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_USER_NAME15      = 0x0341 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_USER_NAME16      = 0x0342 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_USER_NAME17      = 0x0343 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_USER_NAME18      = 0x0344 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_USER_NAME19      = 0x0345 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_USER_NAME20      = 0x0346 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_USER_NAME21      = 0x0347 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_USER_NAME22      = 0x0348 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_USER_NAME23      = 0x0349 + ADDROFFSET.TQ_XH_ADDR,   //

        REG_R_TORQUE            = 0x0360 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_R_TORQUE_PEAK       = 0x0362 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_R_ANGLE             = 0x0364 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_R_ANGLE_ACC         = 0x0366 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_R_UPDATE            = 0x0368 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_R_ERROR             = 0x0369 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_R_BATTERY           = 0x036A + ADDROFFSET.TQ_XH_ADDR,   //
        REG_R_KEYBUF            = 0x036B + ADDROFFSET.TQ_XH_ADDR,   //

        REG_R_FIFO_FULL         = 0x0370 + ADDROFFSET.TQ_XH_ADDR,   //满标志
        REG_R_FIFO_EMPTY        = 0x0371 + ADDROFFSET.TQ_XH_ADDR,   //空标志
        REG_R_FIFO_SIZE         = 0x0372 + ADDROFFSET.TQ_XH_ADDR,   //深度
        REG_R_FIFO_COUNT        = 0x0374 + ADDROFFSET.TQ_XH_ADDR,   //计数
        REG_R_FIFO_READ         = 0x0376 + ADDROFFSET.TQ_XH_ADDR,   //读地址
        REG_R_FIFO_WRITE        = 0x0378 + ADDROFFSET.TQ_XH_ADDR,   //写地址

        REG_R_FIFO_STAMP        = 0x0380 + ADDROFFSET.TQ_XH_ADDR,   //时间标识
        REG_R_FIFO_DTYPE        = 0x0382 + ADDROFFSET.TQ_XH_ADDR,   //数据类型
        REG_R_FIFO_MODE_PT      = 0x0383 + ADDROFFSET.TQ_XH_ADDR,   //模式modePt
        REG_R_FIFO_MODE_AX      = 0x0384 + ADDROFFSET.TQ_XH_ADDR,   //模式modeAx
        REG_R_FIFO_MODE_MX      = 0x0385 + ADDROFFSET.TQ_XH_ADDR,   //模式modeMx
        REG_R_FIFO_BATTERY      = 0x0386 + ADDROFFSET.TQ_XH_ADDR,   //电池电量
        REG_R_FIFO_TORQUE_UNIT  = 0x0387 + ADDROFFSET.TQ_XH_ADDR,   //扭矩单位
        REG_R_FIFO_TORQUE       = 0x0388 + ADDROFFSET.TQ_XH_ADDR,   //实时扭矩
        REG_R_FIFO_TORQUE_PEAK  = 0x038A + ADDROFFSET.TQ_XH_ADDR,   //扭矩峰值
        REG_R_FIFO_ANGLE        = 0x038C + ADDROFFSET.TQ_XH_ADDR,   //实时角度
        REG_R_FIFO_ANGLE_ACC    = 0x038E + ADDROFFSET.TQ_XH_ADDR,   //累加角度
        REG_R_FIFO_ALM0         = 0x0390 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_R_FIFO_ALM1         = 0x0392 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_R_FIFO_ALM2         = 0x0394 + ADDROFFSET.TQ_XH_ADDR,   //


        REG_R_RECDAT            = 0x0400 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER0      = 0x0600 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER1      = 0x0601 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER2      = 0x0602 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER3      = 0x0603 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER4      = 0x0604 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER5      = 0x0605 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER6      = 0x0606 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER7      = 0x0607 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER8      = 0x0608 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER9      = 0x0609 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER10     = 0x060A + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER11     = 0x060B + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER12     = 0x060C + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER13     = 0x060D + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER14     = 0x060E + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER15     = 0x060F + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER16     = 0x0610 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER17     = 0x0611 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER18     = 0x0612 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER19     = 0x0613 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER20     = 0x0614 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER21     = 0x0615 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER22     = 0x0616 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER23     = 0x0617 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER24     = 0x0618 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER25     = 0x0619 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER26     = 0x061A + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER27     = 0x061B + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER28     = 0x061C + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER29     = 0x061D + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER30     = 0x061E + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER31     = 0x061F + ADDROFFSET.TQ_XH_ADDR,   //

        REG_WR_SCREWORDER48     = 0x0630 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER64     = 0x0640 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER80     = 0x0650 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER96     = 0x0660 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER112    = 0x0670 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER128    = 0x0680 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER144    = 0x0690 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER160    = 0x06A0 + ADDROFFSET.TQ_XH_ADDR,   //
        REG_WR_SCREWORDER176    = 0x06B0 + ADDROFFSET.TQ_XH_ADDR,   //


        //********以下蓝牙接收器专用********//
        REG_WR_BLUETOOTH_UNBIND = 0xFF50,
    }

    //发送接口类型
    public enum ProtocolFunc
    {
        Protocol_Read_SendCOM,
        Protocol_Write_SendCOM,
        Protocol_Sequence_FifoClear,
        Protocol_Sequence_FifoIndex,
        Protocol_Sequence_SendCOM
    }
}
