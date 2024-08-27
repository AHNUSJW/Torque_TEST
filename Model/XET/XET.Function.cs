using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace Model
{
    public partial class XET : WRE
    {
        public XET()
        {
            sTATE = STATE.INVALID;
        }

        
        // 通过WMI读取系统信息里的网卡MAC
        public string GetMacByWmi()
        {
            try
            {
                //创建ManagementClass对象
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();
                string macAddress = string.Empty;
                foreach (ManagementObject mo in moc)//遍历获取的集合
                {
                    if ((bool)mo["IPEnabled"])//判断IPEnabled的属性是否为true
                    {
                        macAddress = mo["MacAddress"].ToString();//获取网卡的序列号
                    }
                }
                return macAddress;
            }
            catch (Exception e)
            {
                return "";
            }
        }
    }
}
