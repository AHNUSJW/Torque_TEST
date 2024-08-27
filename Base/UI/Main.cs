using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Base.UI.MenuData;
using Base.UI.MenuDevice;
using Base.UI.MenuHomework;
using Base.UI.MenuUser;
using Model;

namespace Base
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            // 获取当前日期
            DateTime now = DateTime.Now;

            // 设置警告的截止日期
            DateTime cutoffDate = new DateTime(2024, 7, 15);

            // 比较当前日期和截止日期
            if (now.Date > cutoffDate.Date)
            {
                // 当前日期超过截止日期，弹出警告
                MessageBox.Show("当前日期超过试用期截至日期2024年7月15日！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                //强制退出
                Environment.Exit(0);
            }
            else
            {
                // 当前日期在截止日期之前
                Console.WriteLine("当前日期未超过2024年7月15日。");
            }
        }

        private void 实时通讯ToolStripMenuItem_Click(object sender, EventArgs e)
        {
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

        private void 设备连接ToolStripMenuItem_Click(object sender, EventArgs e)
        {
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

        private void DataManageToolStripMenuItem_Click(object sender, EventArgs e)
        {
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

        private void DataAnalysisToolStripMenuItem_Click(object sender, EventArgs e)
        {
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

        private void DeviceSetToolStripMenuItem_Click(object sender, EventArgs e)
        {
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

        private void CreateTicketToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form form in this.MdiChildren)
            {
                if (form.GetType().Name == "MenuCreateTicketForm")
                {
                    form.BringToFront();
                    return;
                }
                else
                {
                    form.Close();
                }
            }

            MenuCreateTicketForm  menuCreateTicketForm = new MenuCreateTicketForm();
            menuCreateTicketForm.MdiParent = this;
            menuCreateTicketForm.StartPosition = FormStartPosition.CenterScreen;
            menuCreateTicketForm.Show();
            menuCreateTicketForm.WindowState = FormWindowState.Maximized;
        }
    }
}
