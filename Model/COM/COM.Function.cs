using System;
using System.IO.Ports;

namespace Model
{
    public interface IProtocol
    {
        #region

        COMP type { get; }      	                //接口类型
        Byte addr { set; get; }                     //设备站点
        Boolean Is_serial_listening { set; get; }   //串口正在监听标记
        Boolean Is_serial_closing { set; get; }     //串口正在关闭标记
        Object port { set; get; }                   //接口用的串口
        String portName { set; get; }               //接口用的串口/端口名称
        Boolean IsOpen { get; }                     //接口端口/串口是否打开
        TASKS trTASK { set; get; }                  //接口读写状态机
        Int32 txCount { get; }                      //接口发送字节计数
        Int32 rxCount { get; }                      //接口接收字节计数
        Boolean isEqual { get; }                    //接收的指令校验结果

        String rxString { get; }    //收到的字符串

        #endregion

        //打开串口
        void Protocol_PortOpen(String name, Int32 baud, StopBits stb, Parity pay);

        //关闭串口
        bool Protocol_PortClose();

        //清除串口任务
        void Protocol_ClearState();

        //刷新IsEQ
        void Protocol_ChangeEQ();

        //发送读命令
        void Protocol_Read_SendCOM(TASKS meTask);

        //发送写命令
        void Protocol_Write_SendCOM(TASKS meTask, UInt16 data = 0xFFFF);

        //发送连续写命令
        void Protocool_Sequence_FifoClear(Int32 num);

        //发送连续写命令
        void Protocool_Sequence_FifoIndex(UInt32 index);

        //发送连续写命令
        void Protocool_Sequence_SendCOM(TASKS meTask);

        //串口读取所有任务状态机 DEV -> CAL -> INFO -> WLAN -> ID -> PARA -> AM1 -> AM2 -> AM3 -> JOB -> OP -> HEART -> FIFO -> DAT
        void Protocol_mePort_ReadAllTasks();

        //串口写入所有任务状态机 INFO -> WLAN -> ID -> PARA -> AM1 -> AM2 -> AM3 -> JOB -> OP
        void Protocol_mePort_WriteAllTasks();
    }
}