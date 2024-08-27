using Base.UI.MenuData;
using Base.UI.MenuDevice;
using Base.UI.MenuHelp;
using Base.UI.MenuHomework;
using Base.UI.MenuUser;
using GenerateCode;
using Library;
using LicenseActivation;
using Model;
using RecXF;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Base
{
    public partial class XhTorqueShow : Form
    {
        MainUser menuHelpLicenseForm = new MainUser();

        public XhTorqueShow()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {        
            //检查激活
            CheckLicense();

            //启动自动指令处理
            MyDevice.myTaskManager.Start();

            //判断能否自动连接
            if (MyDevice.ReadConnetType(MyDevice.userDAT + @"\AutoStart.txt"))
            {
                MyDevice.myTaskManager.AutoConnect();
            }
        }

        //检查激活
        private void CheckLicense()
        {
            if (menuHelpLicenseForm.AreRegistryKeysExist())
            {
                if (menuHelpLicenseForm.IsLicenseExpired())
                {
                    //证书已过期
                    MessageBox.Show("证书已过期，请重新激活软件！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Enable_Menu(false);
                }
                else if (menuHelpLicenseForm.IsDateTampered())
                {
                    //读取到的电脑时间早于上次使用日期
                    MessageBox.Show("日期有误，请重新激活软件", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Enable_Menu(false);
                }
                else
                {
                    //将当前日期写入注册表
                    menuHelpLicenseForm.WriteCurrentDateToRegistry();
                    //已激活
                    Enable_Menu(true);
                    //数据库状态判断
                    MySqlStatus();
                }
            }
            else
            {
                //未激活 检查试用期
                if (menuHelpLicenseForm.CheckAndUpdateTrialStatus())
                {
                    Enable_Menu(true);
                    //数据库状态判断
                    MySqlStatus();
                }
                else
                {
                    MessageBox.Show("试用期已过，请激活软件！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Enable_Menu(false);
                }
            }
        }

        //调整菜单是否可用
        private void Enable_Menu(bool enabled)
        {
            //工单系列
            ManageScrewToolStripMenuItem.Enabled = enabled;
            ManageWrenchToolStripMenuItem.Enabled = enabled;
            ManageBarcodeToolStripMenuItem.Enabled = enabled;
            ManageTicketToolStripMenuItem.Enabled = enabled;
            CreateTicketToolStripMenuItem.Enabled = enabled;

            //数据分析
            DataManageToolStripMenuItem.Enabled = enabled;
            DataAnalysisToolStripMenuItem.Enabled = enabled;
            DataSaveToolStripMenuItem.Enabled = enabled;

            //设备功能
            DeviceConnectToolStripMenuItem.Enabled = enabled;
            DeviceSetToolStripMenuItem.Enabled = enabled;
            DeviceDataToolStripMenuItem.Enabled = enabled;
            QuickConnectToolStripMenuItem.Enabled = enabled;

            //无线配置
            RecXFConfigToolStripMenuItem.Enabled = enabled;
            WiFiConfigToolStripMenuItem.Enabled = enabled;
        }

        //判断MySql状态调整菜单开发权限
        private void MySqlStatus()
        {
            //判断当前电脑是否启动数据库服务
            if (!GetComPuterInfo.ServiceIsRunning("MySQL", 21))
            {
                //Mysql数据库未安装，工单系列不开放
                ManageScrewToolStripMenuItem.Enabled = false;
                ManageWrenchToolStripMenuItem.Enabled = false;
                ManageTicketToolStripMenuItem.Enabled = false;
                CreateTicketToolStripMenuItem.Enabled = false;

                //Mysql数据库未安装，数据分析系列不开放
                DataManageToolStripMenuItem.Enabled = false;
                DataAnalysisToolStripMenuItem.Enabled = false;
                DataSaveToolStripMenuItem.Enabled = false;

                //
                QuickConnectToolStripMenuItem.Enabled = false;

                MyDevice.IsMySqlStart = false;
            }
            else
            {
                //开放工单
                ManageScrewToolStripMenuItem.Enabled = true;
                ManageWrenchToolStripMenuItem.Enabled = true;
                ManageTicketToolStripMenuItem.Enabled = true;
                CreateTicketToolStripMenuItem.Enabled = true;

                //开放数据分析
                DataManageToolStripMenuItem.Enabled = true;
                DataAnalysisToolStripMenuItem.Enabled = true;
                DataSaveToolStripMenuItem.Enabled = true;

                //
                QuickConnectToolStripMenuItem.Enabled = true;

                MyDevice.IsMySqlStart = true;
            }
        }

        #region 用户栏功能

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserLoginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form form in this.MdiChildren)
            {
                if (form.GetType().Name == "MenuAccountForm")
                {
                    form.BringToFront();
                    return;
                }
                else
                {
                    form.Close();
                }
            }

            MenuAccountForm menuAccountForm = new MenuAccountForm();
            menuAccountForm.StartPosition = FormStartPosition.CenterScreen;
            menuAccountForm.ShowDialog();
        }

        /// <summary>
        /// 退出系统
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //退出所有窗口
            System.Environment.Exit(0);
        }

        #endregion

        #region 数据栏功能

        /// <summary>
        /// 数据管理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataManageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MyDevice.myTaskManager.CancelAutoConnect();

            foreach (Form form in this.MdiChildren)
            {
                if (form.GetType().Name == "MenuDataManageForm")
                {
                    form.BringToFront();
                    return;
                }
                else
                {
                    form.Close();
                }
            }

            MenuDataManageForm menuDataManageForm = new MenuDataManageForm();
            menuDataManageForm.MdiParent = this;
            menuDataManageForm.StartPosition = FormStartPosition.CenterScreen;
            menuDataManageForm.Show();
            menuDataManageForm.WindowState = FormWindowState.Maximized;
        }

        /// <summary>
        /// 数据分析
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataAnalysisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MyDevice.myTaskManager.CancelAutoConnect();

            foreach (Form form in this.MdiChildren)
            {
                if (form.GetType().Name == "MenuDataAnalysisForm")
                {
                    form.BringToFront();
                    return;
                }
                else
                {
                    form.Close();
                }
            }

            MenuDataAnalysisForm menuDataAnalysisForm = new MenuDataAnalysisForm();
            menuDataAnalysisForm.MdiParent = this;
            menuDataAnalysisForm.StartPosition = FormStartPosition.CenterScreen;
            menuDataAnalysisForm.Show();
            menuDataAnalysisForm.WindowState = FormWindowState.Maximized;
        }

        #endregion

        #region 作业栏功能

        /// <summary>
        /// 螺栓管理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ManageScrewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form form in this.MdiChildren)
            {
                if (form.GetType().Name == "MenuManageScrewForm")
                {
                    form.BringToFront();
                    return;
                }
                else
                {
                    form.Close();
                }
            }

            MenuManageScrewForm menuManageScrewForm = new MenuManageScrewForm();
            menuManageScrewForm.MdiParent = this;
            menuManageScrewForm.StartPosition = FormStartPosition.CenterScreen;
            menuManageScrewForm.Show();
            menuManageScrewForm.WindowState = FormWindowState.Maximized;
        }

        /// <summary>
        /// 扳手管理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ManageWrenchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form form in this.MdiChildren)
            {
                if (form.GetType().Name == "MenuManageWrenchForm")
                {
                    form.BringToFront();
                    return;
                }
                else
                {
                    form.Close();
                }
            }

            MenuManageWrenchForm menuManageWrenchForm = new MenuManageWrenchForm();
            menuManageWrenchForm.MdiParent = this;
            menuManageWrenchForm.StartPosition = FormStartPosition.CenterScreen;
            menuManageWrenchForm.Show();
            menuManageWrenchForm.WindowState = FormWindowState.Maximized;
        }

        /// <summary>
        /// 条码管理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ManageBarcodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BarcodeMainForm barcodeMainForm = new BarcodeMainForm();
            barcodeMainForm.MaximizeBox = false;
            barcodeMainForm.StartPosition = FormStartPosition.CenterScreen;
            barcodeMainForm.Show();
        }

        /// <summary>
        /// 工单管理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ManageTicketToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form form in this.MdiChildren)
            {
                if (form.GetType().Name == "MenuManageTicketForm")
                {
                    form.BringToFront();
                    return;
                }
                else
                {
                    form.Close();
                }
            }

            MenuManageTicketForm menuManageTicketForm = new MenuManageTicketForm();
            menuManageTicketForm.MdiParent = this;
            menuManageTicketForm.StartPosition = FormStartPosition.CenterScreen;
            menuManageTicketForm.Show();
            menuManageTicketForm.WindowState = FormWindowState.Maximized;
        }

        /// <summary>
        /// 工单生成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateTicketToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form form in this.MdiChildren)
            {
                if (form.GetType().Name == "MenuTicketWorkForm")
                {
                    form.BringToFront();
                    return;
                }
                else
                {
                    form.Close();
                }
            }

            MenuTicketWorkForm menuTicketWorkForm = new MenuTicketWorkForm();
            menuTicketWorkForm.MdiParent = this;
            menuTicketWorkForm.StartPosition = FormStartPosition.CenterScreen;
            menuTicketWorkForm.Show();
            menuTicketWorkForm.WindowState = FormWindowState.Maximized;
        }

        #endregion

        #region 设备栏功能

        /// <summary>
        /// 设备连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeviceConnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MyDevice.myTaskManager.CancelAutoConnect();

            foreach (Form form in this.MdiChildren)
            {
                if (form.GetType().Name == "MenuConnectForm")
                {
                    form.BringToFront();
                    return;
                }
                else
                {
                    form.Close();
                }
            }

            MenuConnectForm menuConnectForm = new MenuConnectForm();
            menuConnectForm.StartPosition = FormStartPosition.CenterScreen;
            menuConnectForm.ShowDialog();
        }

        /// <summary>
        /// 快捷连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QuickConnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MyDevice.myTaskManager.CancelAutoConnect();

            foreach (Form form in this.MdiChildren)
            {
                if (form.GetType().Name == "MenuQuickConnectForm")
                {
                    form.BringToFront();
                    return;
                }
                else
                {
                    form.Close();
                }
            }

            MenuQuickConnectForm menuQuickConnectForm = new MenuQuickConnectForm();
            menuQuickConnectForm.StartPosition = FormStartPosition.CenterScreen;
            menuQuickConnectForm.ShowDialog();
        }

        /// <summary>
        /// 设备设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeviceSetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MyDevice.myTaskManager.CancelAutoConnect();

            //RS485特殊设置
            if (MyDevice.protocol.type == COMP.RS485)
            {
                if (MyDevice.devSum > 0)
                {
                    MyDevice.protocol.addr = MyDevice.AddrList[0];
                }
            }

            //XH-05
            if (MyDevice.actDev.devc.type == TYPE.TQ_XH_XL01_05 - (UInt16)ADDROFFSET.TQ_XH_ADDR)
            {
                foreach (Form form in this.MdiChildren)
                {
                    if (form.GetType().Name == "MenuDeviceSetForm3")
                    {
                        form.BringToFront();
                        return;
                    }
                    else
                    {
                        form.Close();
                    }
                }

                MenuDeviceSetForm3 menuDeviceSetForm3 = new MenuDeviceSetForm3();
                menuDeviceSetForm3.StartPosition = FormStartPosition.CenterScreen;
                menuDeviceSetForm3.ShowDialog();
            }
            //XH-08
            else if (MyDevice.actDev.devc.type == TYPE.TQ_XH_XL01_08 - (UInt16)ADDROFFSET.TQ_XH_ADDR)
            {
                foreach (Form form in this.MdiChildren)
                {
                    if (form.GetType().Name == "MenuDeviceSetForm2")
                    {
                        form.BringToFront();
                        return;
                    }
                    else
                    {
                        form.Close();
                    }
                }

                MenuDeviceSetForm2 menuDeviceSetForm2 = new MenuDeviceSetForm2();
                menuDeviceSetForm2.StartPosition = FormStartPosition.CenterScreen;
                menuDeviceSetForm2.ShowDialog();
            }
            //XH-6 / 7 / 9
            else
            {
                foreach (Form form in this.MdiChildren)
                {
                    if (form.GetType().Name == "MenuDeviceSetForm")
                    {
                        form.BringToFront();
                        return;
                    }
                    else
                    {
                        form.Close();
                    }
                }

                MenuDeviceSetForm menuDeviceSetForm = new MenuDeviceSetForm();
                menuDeviceSetForm.StartPosition = FormStartPosition.CenterScreen;
                menuDeviceSetForm.ShowDialog();
            }
        }

        /// <summary>
        /// 设备数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeviceDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MyDevice.myTaskManager.CancelAutoConnect();

            foreach (Form form in this.MdiChildren)
            {
                if (form.GetType().Name == "MenuActualDataForm")
                {
                    form.BringToFront();
                    return;
                }
                else
                {
                    form.Close();
                }
            }

            MenuActualDataForm menuActualData = new MenuActualDataForm();
            menuActualData.MdiParent = this;
            menuActualData.StartPosition = FormStartPosition.CenterScreen;
            menuActualData.Show();
            menuActualData.WindowState = FormWindowState.Maximized;
        }

        #endregion

        #region 网络栏功能

        /// <summary>
        /// 接收器配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RecXFConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MyDevice.myTaskManager.CancelAutoConnect();

            MainFormRecConfig menuToolReceiverForm = new MainFormRecConfig();
            menuToolReceiverForm.MaximizeBox = false;
            menuToolReceiverForm.StartPosition = FormStartPosition.CenterScreen;
            if (menuToolReceiverForm.ShowDialog() == DialogResult.OK)
            {
                menuToolReceiverForm.MainFormRecConfig_FormClosing(null, null);
            }
        }

        #endregion

        #region 帮助栏功能

        /// <summary>
        /// 打开软件相关操作pdf
        /// </summary>
        /// <param name="searchText"></param>
        private void OpenHelpPdfFile(string searchText)
        {
            // 指定文件夹路径
            string folderPath = Path.Combine(Application.StartupPath, "pic");

            // 检查目录是否存在
            if (!Directory.Exists(folderPath))
            {
                MessageBox.Show("软件自带的操作说明书被删除，请找开发商寻求帮助", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 获取指定文件夹下的所有PDF文件
            string[] pdfFiles = Directory.GetFiles(folderPath, "*.pdf");
            bool fileFound = false;

            // 遍历所有PDF文件
            foreach (string filePath in pdfFiles)
            {
                // 获取文件名
                string fileName = Path.GetFileNameWithoutExtension(filePath);

                // 检查文件名是否包含指定文本
                if (fileName.Contains(searchText))
                {
                    // 打开PDF文件
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true // 使用系统默认应用打开文件
                    });
                    fileFound = true;
                    break;
                }
            }

            // 如果没有找到匹配的文件，显示提示框
            if (!fileFound)
            {
                MessageBox.Show($"未找到包含 '{searchText}' 的PDF文件，请找开发商寻求帮助", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // 软件操作手册
        private void SoftwareHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenHelpPdfFile("软件操作");
        }

        // 数据库操作手册
        private void MysqlHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenHelpPdfFile("数据库");
        }

        // 软件激活手册
        private void ActivationHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenHelpPdfFile("软件激活");
        }

        /// <summary>
        /// 软件激活
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LicenseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form form in this.MdiChildren)
            {
                if (form.GetType().Name == "menuHelpLicenseForm")
                {
                    form.BringToFront();
                    return;
                }
                else
                {
                    form.Close();
                }
            }

            menuHelpLicenseForm.StartPosition = FormStartPosition.CenterScreen;
            menuHelpLicenseForm.MaximizeBox = false;
            menuHelpLicenseForm.ShowDialog();
            CheckLicense();
        }

        /// <summary>
        /// 选型指导
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeviceTypeGuideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form form in this.MdiChildren)
            {
                if (form.GetType().Name == "MenuModelSelectForm")
                {
                    form.BringToFront();
                    return;
                }
                else
                {
                    form.Close();
                }
            }

            MenuModelSelectForm menuModelSelectForm = new MenuModelSelectForm();
            menuModelSelectForm.MdiParent = this;
            menuModelSelectForm.StartPosition = FormStartPosition.CenterScreen;
            menuModelSelectForm.Show();
            menuModelSelectForm.WindowState = FormWindowState.Maximized;
        }

        /// <summary>
        /// 自动连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoConnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MyDevice.myTaskManager.CancelAutoConnect();

            MyDevice.SaveConnectType(true);

            MessageBox.Show("已开启开机自动连接,请重启软件");

            // 获取当前进程信息
            Process currentProcess = Process.GetCurrentProcess();

            // 获取应用程序的完整路径
            string processPath = currentProcess.MainModule.FileName;

            // 关闭当前应用程序
            Application.Exit();

            // 启动新的应用程序实例
            Process.Start(processPath);
        }

        /// <summary>
        /// 手动连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ManualConnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MyDevice.myTaskManager.CancelAutoConnect();

            MyDevice.SaveConnectType(false);

            MessageBox.Show("已开启手动连接");

            // 获取当前进程信息
            Process currentProcess = Process.GetCurrentProcess();

            // 获取应用程序的完整路径
            string processPath = currentProcess.MainModule.FileName;

            // 关闭当前应用程序
            Application.Exit();

            // 启动新的应用程序实例
            Process.Start(processPath);
        }

        #endregion

    }
}
