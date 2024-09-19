using System;

//Alvin,20240221

//变量名来源于硬件程序
//保持统一并同步更新

namespace Model
{
    public class WLAN
    {
        public Byte         addr;                                       //站点地址

        public Byte         rf_chan;                                    //RF信道,0x00~0xC8
        public Byte         rf_option;                                  //设置透传,发射功率
        public Byte         rf_para;                                    //RF参数,校验位,波特率,空中速率

        public Byte         rs485_baud;                                 //RS485波特率
        public Byte         rs485_stopbit;                              //RS485停止位
        public Byte         rs485_parity;                               //RS485校验位

        public String       wf_ssid;                                    //WiFi账号
        public String       wf_pwd;                                     //WiFi密码
        public String       wf_ip;                                      //网络IP
        public UInt32       wf_port;                                    //网络端口号
    }

    public class DEVC
    {
        public SERIES       series;                                     //系列BEM,KES,XH,XLT等
        public TYPE         type;                                       //型号
        public Byte         version;                                    //程序版本
        public UInt64       bohrcode;                                   //唯一序列号

        public UNIT         unit;                                       //标定的单位
        public Byte         caltype;                                    //标定方式,五点,七点,九点,十一点
        public Byte         torque_decimal;                             //扭矩小数点
        public Byte         torque_fdn;                                 //扭矩分度值
        public Int32        capacity;                                   //扭矩量程
        public Int32        ad_zero;                                    //标定零点
        public Int32        ad_pos_point1;                              //正向第1点内码
        public Int32        ad_pos_point2;                              //正向第2点内码
        public Int32        ad_pos_point3;                              //正向第3点内码
        public Int32        ad_pos_point4;                              //正向第4点内码
        public Int32        ad_pos_point5;                              //正向第5点内码
        public Int32        ad_neg_point1;                              //反向第1点内码
        public Int32        ad_neg_point2;                              //反向第2点内码
        public Int32        ad_neg_point3;                              //反向第3点内码
        public Int32        ad_neg_point4;                              //反向第4点内码
        public Int32        ad_neg_point5;                              //反向第5点内码

        public Int32        tq_pos_point1;                              //正向第1点扭矩值
        public Int32        tq_pos_point2;                              //正向第2点扭矩值
        public Int32        tq_pos_point3;                              //正向第3点扭矩值
        public Int32        tq_pos_point4;                              //正向第4点扭矩值
        public Int32        tq_pos_point5;                              //正向第5点扭矩值
        public Int32        tq_neg_point1;                              //反向第1点扭矩值
        public Int32        tq_neg_point2;                              //反向第2点扭矩值
        public Int32        tq_neg_point3;                              //反向第3点扭矩值
        public Int32        tq_neg_point4;                              //反向第4点扭矩值
        public Int32        tq_neg_point5;                              //反向第5点扭矩值

        public Int32        torque_disp;                                //最小显示扭矩值
        public Int32        torque_min;                                 //最小可调报警值
        public Int32        torque_max;                                 //最大可调报警值,等于量程
        public Int32[]      torque_over = new Int32[Constants.UNITS];   //扭矩超载报警值,等于量程的120%
        public Int32[]      torque_err = new Int32[Constants.UNITS];    //超量程使用扭矩值,nm,inlb,ftlb,kgcm,kgm
    }

    public class PARA
    {
        public UNIT         torque_unit;                                //扭矩单位
        public Byte         angle_speed;                                //角度挡位
        public Byte         angle_decimal;                              //角度小数点
        public Byte         mode_pt;                                    //模式modePt,track/peak0/peak1,抓峰值模式
        public Byte         mode_ax;                                    //模式modeAx,扭矩,角度,扭矩+角度
        public Byte         mode_mx;                                    //模式modeMx,M0~M9

        public Byte         fifomode;                                   //缓存满覆盖还是不覆盖
        public Byte         fiforec;                                    //实时缓存还是峰值缓存
        public Byte         fifospeed;                                  //实时缓存速率10Hz,40Hz,100Hz,200Hz,320Hz,640Hz

        public Byte         heartformat;                                //心跳数据格式
        public Byte         heartcount;                                 //心跳回复帧数,最小1
        public UInt16       heartcycle;                                 //心跳间隔,最小20ms

        public Byte         accmode;                                    //是否关闭角度累加
        public Byte         alarmode;                                   //是否关闭声光报警省电
        public Byte         wifimode;                                   //是否关闭wifi/RF无线

        public Byte         timeoff;                                    //自动关机时间
        public Byte         timeback;                                   //自动关背光时间
        public Byte         timezero;                                   //自动归零时间

        public Byte         disptype;                                   //横屏,竖屏
        public Byte         disptheme;                                  //显示主题
        public Byte         displan;                                    //语言

        public UInt16       unhook;                                     //脱钩的保持时间,默认200ms
        public float        angcorr;                                    //角度修正系数
        public Byte         adspeed;                                    //adc采样速率和增益
        public AUTOZERO     autozero;                                   //归零范围
        public TRACKZERO    trackzero;                                  //零点跟踪

        public ushort       amenable;                                   //使能扳手的按键修改报警值
        public byte         screwmax;                                   //离线工单有效数量
        public byte         runmode;                                    //离线执行工单模式，0无，1手动，2自动
    }

    public class ALAM
    {
        public Int32[]      EN_target = new Int32[Constants.UNITS];     //ET模式, EN目标扭矩,nm,inlb,ftlb,kgcm,kgm
        public Int32[]      EA_pre = new Int32[Constants.UNITS];        //ET模式, EA预设扭矩,nm,inlb,ftlb,kgcm,kgm
        public Int32        EA_ang;                                     //ET模式, EA目标角度

        public Int32[,]     SN_target = new Int32[10,Constants.UNITS];  //A1/ST模式, A1,SN,目标扭矩,nm,inlb,ftlb,kgcm,kgm
        public Int32[,]     SA_pre = new Int32[10,Constants.UNITS];     //A1/ST模式, A1,SA,预设扭矩,nm,inlb,ftlb,kgcm,kgm
        public Int32[]      SA_ang = new Int32[10];                     //A1/ST模式, A1,SA,目标角度

        public Int32[,]     MN_low = new Int32[10,Constants.UNITS];     //A2,MT模式, A2,MN,扭矩下限,nm,inlb,ftlb,kgcm,kgm
        public Int32[,]     MN_high = new Int32[10,Constants.UNITS];    //A2,MT模式, A2,MN,扭矩上限,nm,inlb,ftlb,kgcm,kgm
        public Int32[,]     MA_pre = new Int32[10,Constants.UNITS];     //A2,MT模式, A2,MA,预设扭矩,nm,inlb,ftlb,kgcm,kgm
        public Int32[]      MA_low = new Int32[10];                     //A2,MT模式, A2,MA,角度下限
        public Int32[]      MA_high = new Int32[10];                    //A2,MT模式, A2,MA,角度上限

        public Int32[,]     AZ_start = new Int32[10, Constants.UNITS];  //AZ模式 安装应力检测角度起始的扭矩值
        public Int32[,]     AZ_stop = new Int32[10, Constants.UNITS];   //AZ模式 安装应力检测角度结束的扭矩值
        public Int32[,]     AZ_hock = new Int32[10, Constants.UNITS];   //AZ模式 安装应力检测的目标扭矩值
    }

    public class WORK
    {
        public UInt32       srno;                                       //生产批号
        public UInt32       number;                                     //数字批号

        public UInt32       mfgtime;                                    //出厂时间
        public UInt32       caltime;                                    //校准时间
        public UInt32       calremind;                                  //复校时间

        public String       name;                                   	//扳手名称
        public String       managetxt;                              	//管理编号
        public String       decription;                             	//备注信息

        public UInt32       wo_area;                                    //工单区
        public UInt32       wo_factory;                                 //工单厂
        public UInt32       wo_line;                                    //工单产线
        public UInt32       wo_station;                                 //工单工位
        public UInt32       wo_stamp;                                   //工单时间标识
        public UInt32       wo_bat;                                     //工单批号
        public UInt32       wo_num;                                     //工单编号
        public String       wo_name;                            		//工单名称

        public UInt32       user_ID;                                    //操作工号
        public String       user_name;                          		//操作姓名

        public Byte[]       screworder = new Byte[32];                  //离线工单组合顺序
    }

    public class FIFO
    {
        public Boolean      full;                                       //满标志
        public Boolean      empty;                                      //空标志

        public UInt32       size;                                       //深度
        public UInt32       count;                                      //计数

        public UInt32       read;                                       //读地址,影响count
        public UInt32       index;                                      //读地址,不影响count
        public UInt32       write;                                      //写地址
    }

    public class DATA
    {
        public Byte         dtype;                                      //数据类型
        public UInt32       stamp;                                      //时间标识

        public Int32        torque;                                     //实时扭矩
        public Int32        torseries_pk;                               //扭矩峰值
        public UNIT         torque_unit;                                //扭矩单位
        public Int32        angle;                                      //实时角度
        public Int32        angle_acc;                                  //累加角度

        public Byte         mode_pt;                                    //模式modePt
        public Byte         mode_ax;                                    //模式modeAx
        public Byte         mode_mx;                                    //模式modeMx

        public Byte         battery;                                    //电量
        public Byte         keybuf;                                     //按键值

        public Boolean      keylock;                                    //操作锁定,不能使用按键
        public Boolean      memable;                                    //锁定通讯改MEM
        public Boolean      update;                                     //按键改参数
        public Boolean      error;                                      //超量程

        /**************02专有参数**************/
        public Byte         mark;                                       //删除标志（0-删除  !0-有效）
        public Byte         angle_decimal;                              //角度小数点
        public UInt32       begin_series;                               //本组数据起始
        public UInt32       begin_group;                                //多组数据起始
        public UInt16       len;                                        //本组数据长度

        /**************03专有参数**************/
        public Int32        torgroup_pk;                                //扭矩峰值
        public Int32[]      alarm = new Int32[3];                       //报警数值
        public Int32        angle_resist;                               //F39版本及以上该参数替换03数据中stamp

        /**************04专有参数**************/
        public Byte         mode;                                       //工单模式AxMx
        public Byte         screwNum;                                   //螺栓数量
        public UInt32       work_ID;                                    //工单号
        public UInt64       work_psq;                                   //工单序列号
        public Byte         screwSeq;                                   //螺栓下标

    }

    public class SpeC
    {
        public Int32        angle_resist;                               //复拧角度（angle_acc < angle_resist则提示重复拧紧）
    }

    public class SCREW
    {
        public Byte          scw_ticketAxMx;                            //离线工单拧紧模式
        public Byte          scw_ticketCnt;                             //离线工单数量
        public UInt32        scw_ticketNum;                             //离线工单号
        public UInt64        scw_ticketSerial;                          //离线工单序列号（1个工单可以有 n 个序列号）
    }

    public partial class WRE
    {
        public WLAN wlan    = new WLAN();                               //无线配置参数
        public DEVC devc    = new DEVC();                               //设备信息,量程,标定值
        public PARA para    = new PARA();                               //设备参数
        public ALAM alam    = new ALAM();                               //报警值
        public WORK work    = new WORK();                               //工单信息
        public FIFO fifo    = new FIFO();                               //缓存状态
        public DATA[] data  = new DATA[5];                              //测量数据
        public SCREW[] screw = new SCREW[32];                           //离线工单
        public SpeC spec    = new SpeC();                               //特殊属性（后期客户新增）
    }
}



