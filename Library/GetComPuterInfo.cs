using System;
using System.Management;

namespace Library
{
    public class GetComPuterInfo
    {
        //判断某个Windows服务是否处于运行状态(该方法需要在项目引用中添加System.ServiceProcess)
        public static bool ServiceIsRunning(string serviceName, int serviceNameMaxLength)
        {
            bool result = false;
            System.ServiceProcess.ServiceController[] services = System.ServiceProcess.ServiceController.GetServices();
            foreach (System.ServiceProcess.ServiceController sc in services)
            {
                if (sc.ServiceName.ToLower().Contains(serviceName.ToLower()) && sc.ServiceName.Length < serviceNameMaxLength)
                {
                    if (sc.Status.ToString().ToLower() == "running")
                    {
                        result = true;
                        break;
                    }
                    //else
                    //{
                    //    //启动服务需要启动管理员权限
                    //    sc.Start();
                    //    result = true;
                    //    break; 
                    //}
                }
            }
            return result;
        }

        // 通过WMI读取系统信息里的网卡MAC
        public static string GetMacByWmi()
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
