using System;
using System.Collections.Generic;

//Alvin,20240221

//在硬件程序中不存在
//上位机软件需要使用的变量

namespace Model
{
    public partial class XET : WRE
    {
        public STATE sTATE = STATE.INVALID;                   //设备连接和工作状态
        public List<DATA> dataList = new List<DATA>();        //设备读取数据集合

        public String opsn = "";         //流水号
        public Int32 snBat = 1;          //流水号
        public Int32 torqueMultiple = 1; //扭矩倍数（扭矩小数点2位，扭矩3000，实际显示是30.00,倍数是100）
        public Int32 angleMultiple = 1;  //角度倍数

        public AUTO auto = new AUTO();  //自动发送指令用
    }

    //自动发送指令用
    public class AUTO
    {
        public TASKS nextTask = TASKS.REG_BLOCK1_FIFO;        //设备指令
        public UInt32 fifoCount = 0;     //fifo中data数量
        public UInt32 fifoIndex = 0;     //读fifo的起始下标
        public int dataTick = 0;         //发送读数据指令次数
        public int readDataNum = 0;      //读取data包数
        public int requiredCount = 1;    //需要发送指令次数
        public int sentCount = 0;        //已发送次数
    }
}
