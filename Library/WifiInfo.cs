using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace Library
{
    public class WifiInfo
    {
        //获取本地WIFI的ssid集合
        public static List<string> GetWIFISsids()
        {
            List<string> wifis = null;
            Regex regex = new Regex(@"^SSID[*0-9]:[^\\\/\^]+", RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);
            if (WinCmdHelper.ExcuteDosCommand("netsh wlan show networks mode=BSSID\r\n", false, true, out string res))
            {
                wifis = new List<string>();
                string[] lines = res.Split('\n');
                foreach (var item in lines)
                {
                    var _item = item.Replace(" ", "");
                    bool match = regex.IsMatch(_item);
                    if (match)
                    {
                        wifis.Add(_item.Split(':')?[1]);
                    }
                }
            }
            return wifis;
        }

        //获取本地WIFI的ssid
        public static string GetWIFISsid()
        {
            Regex regex = new Regex(@"^SSID[*0-9]:[^\\\/\^]+", RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);
            if (WinCmdHelper.ExcuteDosCommand("netsh wlan show networks mode=BSSID\r\n", false, true, out string res))
            {
                string[] lines = res.Split('\n');
                foreach (var item in lines)
                {
                    var _item = item.Replace(" ", "");
                    bool match = regex.IsMatch(_item);
                    if (match)
                    {
                        return _item.Split(':')?[1];
                    }
                }
            }
            return null;
        }

        //获取本地的IP地址集合
        public static List<string> GetIPs()
        {
            List<string> Ips = null;
            string AddressIP = string.Empty;
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    AddressIP = _IPAddress.ToString();
                    if (!Ips.Contains(AddressIP))
                    {
                        Ips.Add(AddressIP);
                    }
                }
            }
            return Ips;
        }

        //获取本地的IP地址
        public static string GetIP()
        {
            string AddressIP = string.Empty;
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    AddressIP = _IPAddress.ToString();
                    return AddressIP;
                }
            }
            return string.Empty;
        }

        public class WinCmdHelper
        {
            #region 执行Dos命令

            /// <summary>
            /// 执行Dos命令
            /// </summary>
            /// <param name="cmd">Dos命令及参数</param>
            /// <param name="isShowCmdWindow">是否显示cmd窗口</param>
            /// <param name="isCloseCmdProcess">执行完毕后是否关闭cmd进程</param>
            /// <returns>成功返回true，失败返回false</returns>
            public static bool ExcuteDosCommand(string cmd, bool isShowCmdWindow, bool isCloseCmdProcess, out string outlines)
            {
                string res = string.Empty;
                try
                {
                    Process p = new Process();
                    p.StartInfo.FileName = "cmd";
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardInput = true;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.CreateNoWindow = !isShowCmdWindow;
                    p.OutputDataReceived += new DataReceivedEventHandler(delegate (object sender, DataReceivedEventArgs e) {
                        if (!String.IsNullOrEmpty(e.Data))
                        {
                            res += e.Data + "\r\n";
                        }
                    });
                    p.Start();
                    StreamWriter cmdWriter = p.StandardInput;
                    p.BeginOutputReadLine();
                    if (!String.IsNullOrEmpty(cmd))
                    {
                        cmdWriter.WriteLine(cmd);
                    }
                    cmdWriter.Close();
                    p.WaitForExit();
                    if (isCloseCmdProcess)
                    {
                        p.Close();
                    }
                    //LogHelper.Logger.Write(Serilog.Events.LogEventLevel.Information, String.Format("成功执行Dos命令[{0}]!", cmd));
                    outlines = res;
                    return true;
                }
                catch (Exception ex)
                {
                    //    LogHelper.Logger.Write(Serilog.Events.LogEventLevel.Information, "执行命令失败，请检查输入的命令是否正确:" + ex.Message, ex);
                    outlines = res;
                    return false;
                }
            }

            #endregion

            #region 判断指定的进程是否在运行中

            /// <summary>
            /// 判断指定的进程是否在运行中
            /// </summary>
            /// <param name="processName">要判断的进程名称，不包括扩展名exe</param>
            /// <param name="processFileName">进程文件的完整路径</param>
            /// <returns>存在返回true，否则返回false</returns>
            public static bool CheckProcessExists(string processName, string processFileName)
            {
                Process[] processes = Process.GetProcessesByName(processName);
                foreach (Process p in processes)
                {
                    if (!String.IsNullOrEmpty(processFileName))
                    {
                        if (processFileName == p.MainModule.FileName)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
                return false;
            }

            #endregion

            #region 结束指定的windows进程

            /// <summary>
            /// 结束指定的windows进程，如果进程存在
            /// </summary>
            /// <param name="processName">进程名称，不包含扩展名</param>
            /// <param name="processFileName">进程文件完整路径，如果为空则删除所有进程名为processName参数值的进程</param>
            public static bool KillProcessExists(string processName, string processFileName)
            {
                try
                {
                    Process[] processes = Process.GetProcessesByName(processName);
                    foreach (Process p in processes)
                    {
                        if (!String.IsNullOrEmpty(processFileName))
                        {
                            if (processFileName == p.MainModule.FileName)
                            {
                                p.Kill();
                                p.Close();
                            }
                        }
                        else
                        {
                            p.Kill();
                            p.Close();
                        }
                    }
                    //LogHelper.Logger.Write(Serilog.Events.LogEventLevel.Information, String.Format("成功结束[{0}]进程!", processes));
                    return true;
                }
                catch (Exception ex)
                {
                    //LogHelper.Logger.Write(Serilog.Events.LogEventLevel.Information, "结束指定的Widnows进程异常:" + ex.Message, ex);
                    return false;
                }
            }

            #endregion
        }
    }
}
