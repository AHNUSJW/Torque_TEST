using System.Drawing;

namespace Model
{
    public partial class WRE
    {
        public WRE()
        {
            wlan.addr           = 1;
            wlan.rf_chan        = 0;
            wlan.rf_option      = 0;
            wlan.rs485_baud     = 1;
            wlan.rs485_stopbit  = 1;
            wlan.rs485_parity   = 0;
            wlan.rf_para        = 0;
            wlan.wf_ssid        = "welcom";
            wlan.wf_pwd         = "12345678";
            wlan.wf_ip          = "C0A80101";
            wlan.wf_port        = 5678;

            devc.series         = SERIES.TQ_XH;
            devc.type           = TYPE.TQ_XH_XL01_07;
            devc.version        = 0;
            devc.bohrcode       = 0;
            devc.unit           = UNIT.UNIT_nm;
            devc.caltype        = 0;
            devc.torque_decimal = 0;
            devc.torque_fdn     = 1;
            devc.capacity       = 3000;
            devc.ad_zero        = 0;
            devc.ad_pos_point1  = 0;
            devc.ad_pos_point2  = 0;
            devc.ad_pos_point3  = 0;
            devc.ad_pos_point4  = 0;
            devc.ad_pos_point5  = 0;
            devc.ad_neg_point1  = 0;
            devc.ad_neg_point2  = 0;
            devc.ad_neg_point3  = 0;
            devc.ad_neg_point4  = 0;
            devc.ad_neg_point5  = 0;
            devc.tq_pos_point1  = 0;
            devc.tq_pos_point2  = 0;
            devc.tq_pos_point3  = 0;
            devc.tq_pos_point4  = 0;
            devc.tq_pos_point5  = 0;
            devc.tq_neg_point1  = 0;
            devc.tq_neg_point2  = 0;
            devc.tq_neg_point3  = 0;
            devc.tq_neg_point4  = 0;
            devc.tq_neg_point5  = 0;
            devc.torque_disp    = 0;
            devc.torque_min     = 0;
            devc.torque_max     = 0;
            devc.torque_over[0] = 0;
            devc.torque_over[1] = 0;
            devc.torque_over[2] = 0;
            devc.torque_over[3] = 0;
            devc.torque_over[4] = 0;
            devc.torque_err[0]  = 0;
            devc.torque_err[1]  = 0;
            devc.torque_err[2]  = 0;
            devc.torque_err[3]  = 0;
            devc.torque_err[4]  = 0;

            para.torque_unit    = UNIT.UNIT_nm;
            para.angle_speed    = 0;
            para.angle_decimal  = 0;
            para.mode_pt        = 0;
            para.mode_ax        = 0;
            para.mode_mx        = 0;
            para.fifomode       = 0;
            para.fiforec        = 0;
            para.fifospeed      = 0;
            para.heartformat    = 0;
            para.heartcount     = 0;
            para.heartcycle     = 0;
            para.accmode        = 0;
            para.alarmode       = 0;
            para.wifimode       = 0;
            para.timeoff        = 0;
            para.timeback       = 0;
            para.timezero       = 0;
            para.disptype       = 0;
            para.disptheme      = 0;
            para.displan        = 0;
            para.unhook         = 0;
            para.angcorr        = 1.0f;
            para.adspeed        = 0;
            para.autozero       = AUTOZERO.ATZ4;
            para.trackzero      = TRACKZERO.TKZ4;
            para.amenable       = 0;
            para.screwmax       = 16;
            para.runmode        = 0;

            for (byte i = 0; i < 5; i++)
            {
                alam.EN_target[i]  = 0;
                alam.EA_pre[i]     = 0;
                alam.EA_ang        = 0;

                for (byte k = 0; k < 10; k++)
                {
                    alam.SN_target[k, i]  = 0;
                    alam.SA_pre[k, i]     = 0;
                    alam.SA_ang[k]        = 0;

                    alam.MN_low[k, i]     = 0;
                    alam.MN_high[k, i]    = 0;
                    alam.MA_pre[k, i]     = 0;
                    alam.MA_low[k]        = 0;
                    alam.MA_high[k]       = 0;

                    alam.AZ_start[k, i]   = 0;
                    alam.AZ_stop[k, i]    = 0;
                    alam.AZ_hock[k, i]    = 0;
                }
            }

            work.srno           = 0;
            work.number         = 0;
            work.mfgtime        = 0;
            work.caltime        = 0;
            work.calremind      = 0;
            work.name           = "";
            work.managetxt      = "";
            work.decription     = "";
            work.wo_area        = 0;
            work.wo_factory     = 0;
            work.wo_line        = 0;
            work.wo_station     = 0;
            work.wo_name        = "";
            work.wo_bat         = 0;
            work.wo_num         = 0;
            work.wo_stamp       = 0;
            work.user_name      = "";
            work.user_ID        = 0;

            for (byte k = 0; k < 32; k++)
            {
                work.screworder[k] = 0;
            }

            fifo.full           = false;
            fifo.empty          = true;
            fifo.size           = 0;
            fifo.count          = 0;
            fifo.read           = 0;
            fifo.index          = 0;
            fifo.write          = 0;

            for (int i = 0; i < 5; i++)
            {
                data[i] = new DATA();
                data[i].dtype          = 0;
                data[i].stamp          = 0;
                data[i].torque         = 0;
                data[i].torseries_pk   = 0;
                data[i].torque_unit    = 0;
                data[i].angle          = 0;
                data[i].angle_acc      = 0;
                data[i].mode_pt        = 0;
                data[i].mode_ax        = 0;
                data[i].mode_mx        = 0;
                data[i].battery        = 0;
                data[i].keybuf         = 0;
                data[i].keylock        = false;
                data[i].memable        = false;
                data[i].update         = false;
                data[i].error          = false;
                data[i].mark           = 0;
                data[i].angle_decimal  = 0;
                data[i].begin_series   = 0;
                data[i].begin_group    = 0;
                data[i].len            = 0;
                data[i].torgroup_pk    = 0;
                data[i].alarm[0]       = 0;
                data[i].alarm[1]       = 0;
                data[i].alarm[2]       = 0;
            }

            for (int i = 0; i <32; i++)
            {
                screw[i] = new SCREW();
                screw[i].scw_ticketAxMx = 0;
                screw[i].scw_ticketCnt  = 1;
                screw[i].scw_ticketNum  = 1;
                screw[i].scw_ticketAxMx = 1;
            }
        }

        public WRE(XET xet)
        {
            wlan.addr           = xet.wlan.addr;
            wlan.rf_chan        = xet.wlan.rf_chan;
            wlan.rf_option      = xet.wlan.rf_option;
            wlan.rf_para        = xet.wlan.rf_para;
            wlan.rs485_baud     = xet.wlan.rs485_baud;
            wlan.rs485_stopbit  = xet.wlan.rs485_stopbit;
            wlan.rs485_parity   = xet.wlan.rs485_parity;
            wlan.wf_ssid        = xet.wlan.wf_ssid;
            wlan.wf_pwd         = xet.wlan.wf_pwd;
            wlan.wf_ip          = xet.wlan.wf_ip;
            wlan.wf_port        = xet.wlan.wf_port;

            devc.series         = xet.devc.series;
            devc.type           = xet.devc.type;
            devc.version        = xet.devc.version;
            devc.bohrcode       = xet.devc.bohrcode;
            devc.unit           = xet.devc.unit;
            devc.caltype        = xet.devc.caltype;
            devc.torque_decimal = xet.devc.torque_decimal;
            devc.torque_fdn     = xet.devc.torque_fdn;
            devc.capacity       = xet.devc.capacity;
            devc.ad_zero        = xet.devc.ad_zero;
            devc.ad_pos_point1  = xet.devc.ad_pos_point1;
            devc.ad_pos_point2  = xet.devc.ad_pos_point2;
            devc.ad_pos_point3  = xet.devc.ad_pos_point3;
            devc.ad_pos_point4  = xet.devc.ad_pos_point4;
            devc.ad_pos_point5  = xet.devc.ad_pos_point5;
            devc.ad_neg_point1  = xet.devc.ad_neg_point1;
            devc.ad_neg_point2  = xet.devc.ad_neg_point2;
            devc.ad_neg_point3  = xet.devc.ad_neg_point3;
            devc.ad_neg_point4  = xet.devc.ad_neg_point4;
            devc.ad_neg_point5  = xet.devc.ad_neg_point5;
            devc.tq_pos_point1  = xet.devc.tq_pos_point1;
            devc.tq_pos_point2  = xet.devc.tq_pos_point2;
            devc.tq_pos_point3  = xet.devc.tq_pos_point3;
            devc.tq_pos_point4  = xet.devc.tq_pos_point4;
            devc.tq_pos_point5  = xet.devc.tq_pos_point5;
            devc.tq_neg_point1  = xet.devc.tq_neg_point1;
            devc.tq_neg_point2  = xet.devc.tq_neg_point2;
            devc.tq_neg_point3  = xet.devc.tq_neg_point3;
            devc.tq_neg_point4  = xet.devc.tq_neg_point4;
            devc.tq_neg_point5  = xet.devc.tq_neg_point5;
            devc.torque_disp    = xet.devc.torque_disp;
            devc.torque_min     = xet.devc.torque_min;
            devc.torque_max     = xet.devc.torque_max;
            devc.torque_over[0] = xet.devc.torque_over[0];
            devc.torque_over[1] = xet.devc.torque_over[1];
            devc.torque_over[2] = xet.devc.torque_over[2];
            devc.torque_over[3] = xet.devc.torque_over[3];
            devc.torque_over[4] = xet.devc.torque_over[4];
            devc.torque_err[0]  = xet.devc.torque_err[0];
            devc.torque_err[1]  = xet.devc.torque_err[1];
            devc.torque_err[2]  = xet.devc.torque_err[2];
            devc.torque_err[3]  = xet.devc.torque_err[3];
            devc.torque_err[4]  = xet.devc.torque_err[4];

            para.torque_unit    = xet.para.torque_unit;
            para.angle_speed    = xet.para.angle_speed;
            para.angle_decimal  = xet.para.angle_decimal;
            para.mode_pt        = xet.para.mode_pt;
            para.mode_ax        = xet.para.mode_ax;
            para.mode_mx        = xet.para.mode_mx;
            para.fifomode       = xet.para.fifomode;
            para.fiforec        = xet.para.fiforec;
            para.fifospeed      = xet.para.fifospeed;
            para.heartformat    = xet.para.heartformat;
            para.heartcount     = xet.para.heartcount;
            para.heartcycle     = xet.para.heartcycle;
            para.accmode        = xet.para.accmode;
            para.alarmode       = xet.para.alarmode;
            para.wifimode       = xet.para.wifimode;
            para.timeoff        = xet.para.timeoff;
            para.timeback       = xet.para.timeback;
            para.timezero       = xet.para.timezero;
            para.disptype       = xet.para.disptype;
            para.disptheme      = xet.para.disptheme;
            para.displan        = xet.para.displan;
            para.unhook         = xet.para.unhook;
            para.angcorr        = xet.para.angcorr;
            para.adspeed        = xet.para.adspeed;
            para.autozero       = xet.para.autozero;
            para.trackzero      = xet.para.trackzero;
            para.amenable       = xet.para.amenable;
            para.screwmax       = xet.para.screwmax;
            para.runmode        = xet.para.runmode;

            for (byte i = 0; i < 5; i++)
            {
                alam.EN_target[i]   = xet.alam.EN_target[i];
                alam.EA_pre[i]      = xet.alam.EA_pre[i];
                alam.EA_ang         = xet.alam.EA_ang;

                for (byte k = 0; k < 10; k++)
                {
                    alam.SN_target[k, i]= xet.alam.SN_target[k, i];
                    alam.SA_pre[k, i]   = xet.alam.SA_pre[k, i];
                    alam.SA_ang[k]      = xet.alam.SA_ang[k];

                    alam.MN_low[k, i]   = xet.alam.MN_low[k, i];
                    alam.MN_high[k, i]  = xet.alam.MN_high[k, i];
                    alam.MA_pre[k, i]   = xet.alam.MA_pre[k, i];
                    alam.MA_low[k]      = xet.alam.MA_low[k];
                    alam.MA_high[k]     = xet.alam.MA_high[k];

                    alam.AZ_start[k, i] = xet.alam.AZ_start[k, i];
                    alam.AZ_stop[k, i]  = xet.alam.AZ_stop[k, i];
                    alam.AZ_hock[k, i]  = xet.alam.AZ_hock[k, i];
                }
            }

            work.srno           = xet.work.srno;
            work.number         = xet.work.number;
            work.mfgtime        = xet.work.mfgtime;
            work.caltime        = xet.work.caltime;
            work.calremind      = xet.work.calremind;
            work.name           = xet.work.name;
            work.managetxt      = xet.work.managetxt;
            work.decription     = xet.work.decription;
            work.wo_area        = xet.work.wo_area;
            work.wo_factory     = xet.work.wo_factory;
            work.wo_line        = xet.work.wo_line;
            work.wo_station     = xet.work.wo_station;
            work.wo_name        = xet.work.wo_name;
            work.wo_bat         = xet.work.wo_bat;
            work.wo_num         = xet.work.wo_num;
            work.wo_stamp       = xet.work.wo_stamp;
            work.user_name      = xet.work.user_name;
            work.user_ID        = xet.work.user_ID;

            for (byte k = 0; k < 16; k++)
            {
                work.screworder[k]  = xet.work.screworder[k];
            }

            fifo.full           = xet.fifo.full;
            fifo.empty          = xet.fifo.empty;
            fifo.size           = xet.fifo.size;
            fifo.count          = xet.fifo.count;
            fifo.read           = xet.fifo.read;
            fifo.index          = xet.fifo.index;
            fifo.write          = xet.fifo.write;

            for (int i = 0; i < 5; i++)
            {
                data[i].dtype          = xet.data[i].dtype;
                data[i].stamp          = xet.data[i].stamp;
                data[i].torque         = xet.data[i].torque;
                data[i].torseries_pk   = xet.data[i].torseries_pk;
                data[i].torque_unit    = xet.data[i].torque_unit;
                data[i].angle          = xet.data[i].angle;
                data[i].angle_acc      = xet.data[i].angle_acc;
                data[i].mode_pt        = xet.data[i].mode_pt;
                data[i].mode_ax        = xet.data[i].mode_ax;
                data[i].mode_mx        = xet.data[i].mode_mx;
                data[i].battery        = xet.data[i].battery;
                data[i].keybuf         = xet.data[i].keybuf;
                data[i].keylock        = xet.data[i].keylock;
                data[i].memable        = xet.data[i].memable;
                data[i].update         = xet.data[i].update;
                data[i].error          = xet.data[i].error;
                data[i].mark           = xet.data[i].mark;
                data[i].angle_decimal  = xet.data[i].angle_decimal;
                data[i].begin_series   = xet.data[i].begin_series;
                data[i].begin_group    = xet.data[i].begin_group;
                data[i].len            = xet.data[i].len;
                data[i].torgroup_pk    = xet.data[i].torgroup_pk;
                data[i].alarm[0]       = xet.data[i].alarm[0];
                data[i].alarm[1]       = xet.data[i].alarm[1];
                data[i].alarm[2]       = xet.data[i].alarm[2];
            }

            for (int i = 0; i < 32; i++)
            {
                screw[i].scw_ticketAxMx = xet.screw[i].scw_ticketAxMx;
                screw[i].scw_ticketCnt  = xet.screw[i].scw_ticketCnt ;
                screw[i].scw_ticketNum  = xet.screw[i].scw_ticketNum ;
                screw[i].scw_ticketAxMx = xet.screw[i].scw_ticketAxMx;
            }
        }
    }
}
