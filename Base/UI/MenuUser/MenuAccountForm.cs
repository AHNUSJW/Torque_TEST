using Model;
using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.Win32;

//Lumi 20240509

//用户权限role 0-普通用户 1-管理员 20-工厂超级用户 30-制造商
//新建账号权限为0-普通用户
//工厂账号不能在此页面修改密码

namespace Base.UI.MenuUser
{
    public partial class MenuAccountForm : Form
    {
        private Boolean isSave = false;
        private Boolean isNew = false;
        private String myUser = "user";
        private String myPSW = "";
        private String myRole = "0";
        private String myDatPath = MyDevice.userDAT;

        public MenuAccountForm()
        {
            InitializeComponent();
        }

        private void MenuAccountForm_Load(object sender, EventArgs e)
        {
            ////初始化注册
            InitializationRegister();

            //窗口元素调整
            if (this.Text == "欢迎使用！")
            {
                button2.Visible = true;
                button3.Visible = true;
                button5.Visible = false;
                label3.Visible = false;
                textBox2.Visible = false;
            }
            else
            {
                button2.Visible = false;
                button3.Visible = true;
                button5.Visible = false;
                label3.Visible = false;
                textBox2.Visible = false;
            }
            //用户加载
            if (Directory.Exists(myDatPath))
            {
                //存在
                comboBox1.Items.Clear();
                DirectoryInfo meDirectory = new DirectoryInfo(myDatPath);
                String meString;
                foreach (FileInfo meFiles in meDirectory.GetFiles("user.*.dat"))
                {
                    meString = meFiles.Name;
                    String[] parts = meString.Split('.');
                    if (parts.Length == 3 && parts[0] == "user" && parts[2] == "dat")
                    {
                        comboBox1.Items.Add(parts[1]);
                    }
                }
            }
            else
            {
                //不存在则创建文件夹
                Directory.CreateDirectory(myDatPath);
                //不存在则创建文件
                myUser = "user";
                myPSW = "";
                myRole = "0";
                myDatPath = MyDevice.userDAT;
                isSave = true;
                isNew = true;
                //增加初始用户
                comboBox1.Items.Add("user");
            }

            //用户名加载
            comboBox1.Text = myUser;
            textBox1.Text = "";
        }

        //退出帐号登录
        private void MenuAccountForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            timer1.Enabled = false;

            //关闭退出
            if (this.Text == "欢迎使用！")
            {
                if (isSave)
                {
                    MyDevice.userName = myUser;
                    MyDevice.userPassword = myPSW;
                    MyDevice.userRole = myRole;
                }
                if (isNew)
                {
                    SaveToDat();  //保存账号
                }
            }
            else if (this.Text == "新建账号" || this.Text == "修改密码")
            {
                if (isNew)
                {
                    SaveToDat();  //保存账号
                }
            }
            else
            {
                isSave = false;
                isNew = false;
            }
        }

        //登录
        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "登 录")
            {
                login_button1_Click();
            }
            else if (button1.Text == "注 册")
            {
                create_button1_Click();
            }
        }

        //注册
        private void button2_Click(object sender, EventArgs e)
        {
            button2.Visible = false;
            button3.Visible = false;
            label3.Visible = true;
            textBox2.Visible = true;
            button1.Text = "注 册";
            this.Text = "新建账号";
        }

        //取消
        private void button4_Click(object sender, EventArgs e)
        {
            if (this.Text == "欢迎使用！")
            {
                this.Close();
            }
            else if (this.Text == "修改密码")
            {
                button1.Visible = true;
                button2.Visible = true;
                button3.Visible = true;
                button5.Visible = false;
                label3.Visible = false;
                textBox2.Visible = false;
                label2.Text = "密  码：";
                label3.Text = "确认密码：";
                this.Text = "欢迎使用！";
            }
            else if (this.Text == "新建账号")
            {
                button2.Visible = true;
                button3.Visible = true;
                label3.Visible = false;
                textBox2.Visible = false;
                button1.Text = "登 录";
                this.Text = "欢迎使用！";
            }
            else
            {
                isSave = false;
                isNew = false;
                this.Close();
            }
        }

        //修改密码
        private void button3_Click(object sender, EventArgs e)
        {
            //工厂使用超级账号密码
            if ((comboBox1.Text == "XhTorque"))
            {
                warning_NI("账号" + comboBox1.Text + "不支持修改密码");
                return;
            }

            button1.Visible = false;
            button2.Visible = false;
            button3.Visible = false;
            button5.Visible = true;
            label3.Visible = true;
            textBox2.Visible = true;
            label2.Text = "旧密码：";
            label3.Text = "新密码：";
            this.Text = "修改密码";
        }

        //确认修改密码键
        private void button5_Click(object sender, EventArgs e)
        {
            save_button1_Click();
        }

        //按enter键登录
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 13)
            {
                this.button1.Focus();
                button1_Click(sender, e);   //调用登录按钮的事件处理代码
            }
        }

        //按enter键登录
        private void comboBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 13)
            {
                this.button1.Focus();
                button1_Click(sender, e);   //调用登录按钮的事件处理代码
            }
        }

        //登录创建保存按钮- 登录
        private void login_button1_Click()
        {
            //工厂使用超级账号密码
            if ((comboBox1.Text == "XhTorque") && (textBox1.Text == "123"))
            {
                myUser = comboBox1.Text;
                myPSW = textBox1.Text;
                myRole = "32";
                myDatPath = MyDevice.userDAT;
                isSave = true;
                isNew = false;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            //客户使用dat账号密码
            else
            {
                //用户文件
                String meString = myDatPath + @"\user." + comboBox1.Text + ".dat";

                //验证用户
                if (File.Exists(meString))
                {
                    //读取用户信息
                    FileStream meFS = new FileStream(meString, FileMode.Open, FileAccess.Read);
                    BinaryReader meRead = new BinaryReader(meFS);
                    if (meFS.Length > 0)
                    {
                        //有内容文件
                        myUser = meRead.ReadString();
                        myPSW = meRead.ReadString();
                        myRole = meRead.ReadString();
                        myDatPath = MyDevice.userDAT;
                        isNew = false;
                    }
                    else
                    {
                        //空文件
                        myUser = comboBox1.Text;
                        myPSW = "";
                        myRole = "0";
                        myDatPath = MyDevice.userDAT;
                        isNew = true;
                    }
                    meRead.Close();
                    meFS.Close();

                    //验证密码
                    if (myPSW == textBox1.Text)
                    {
                        isSave = true;
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        warning_NI("密码错误！");
                    }
                }
                else
                {
                    //admin用户
                    if ((comboBox1.Text == "admin") && (textBox1.Text == "123"))
                    {
                        myUser = "admin";
                        myPSW = "123";
                        myRole = "1";
                        myDatPath = MyDevice.userDAT;
                        isSave = true;
                        isNew = true;
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    //user用户
                    else if ((comboBox1.Text == "user") && (textBox1.Text == ""))
                    {
                        myUser = "user";
                        myPSW = "";
                        myRole = "0";
                        myDatPath = MyDevice.userDAT;
                        isSave = true;
                        isNew = true;
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    //不存在用户提示
                    else
                    {
                        warning_NI("不存在用户！");
                    }
                }
            }
        }

        //登录创建保存按钮- 创建
        private void create_button1_Click()
        {
            if (comboBox1.SelectedIndex < 0)//帐号验证
            {
                if (textBox1.Text == textBox2.Text)//密码验证
                {
                    myUser = comboBox1.Text;
                    myPSW = textBox1.Text;
                    myRole = "0";
                    myDatPath = MyDevice.userDAT;
                    isSave = true;
                    isNew = true;
                    MessageBox.Show("账号创建成功", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    warning_NI("密码错误！");
                }
            }
            else
            {
                warning_NI("已存在账号！");
            }
        }

        //登录创建保存按钮- 修改密码
        private void save_button1_Click()
        {
            String meString = myDatPath + @"\user." + comboBox1.Text + ".dat";

            //验证用户
            if (File.Exists(meString))
            {
                //读取用户信息
                FileStream meFS = new FileStream(meString, FileMode.Open, FileAccess.Read);
                BinaryReader meRead = new BinaryReader(meFS);
                if (meFS.Length > 0)
                {
                    //有内容文件
                    myUser = meRead.ReadString();
                    myPSW = meRead.ReadString();
                    myRole = meRead.ReadString();
                    myDatPath = MyDevice.userDAT;
                    isNew = false;
                }
                else
                {
                    //空文件
                    myUser = comboBox1.Text;
                    myPSW = "";
                    myRole = "0";
                    myDatPath = MyDevice.userDAT;
                    isNew = true;
                }
                meRead.Close();
                meFS.Close();

                //验证密码
                if (myPSW == textBox1.Text)
                {
                    if (myPSW == textBox2.Text)
                    {
                        warning_NI("新密码不能与旧密码相同！");
                        return;
                    }

                    myUser = comboBox1.Text;
                    myPSW = textBox2.Text;
                    myDatPath = MyDevice.userDAT;
                    isSave = true;
                    isNew = true;
                    this.DialogResult = DialogResult.OK;
                    this.Close();

                    MessageBox.Show("密码修改成功", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    warning_NI("旧密码错误！");
                }
            }
            else
            {
                warning_NI("不存在账户" + textBox1.Text + "!");
            }
        }

        //报警提示
        private void warning_NI(string meErr)
        {
            timer1.Enabled = true;
            timer1.Interval = 3000;
            label4.Location = new Point(28, 180);
            label4.Text = meErr;
            label4.Visible = true;
        }

        //账号密码规则
        private void psw_KeyPress(object sender, KeyPressEventArgs e)
        {
            //不可以有以下特殊字符
            // \/:*?"<>|
            // \\
            // \|
            // ""
            Regex meRgx = new Regex(@"[\\/:*?""<>\|]");
            if (meRgx.IsMatch(e.KeyChar.ToString()))
            {
                warning_NI("不能使用\\/:*?\"<>|");
                e.Handled = true;
            }
        }

        //保存帐号
        private bool SaveToDat()
        {
            //空
            if (myDatPath == null)
            {
                return false;
            }
            //创建新路径
            else if (!Directory.Exists(myDatPath))
            {
                Directory.CreateDirectory(myDatPath);
            }

            //写入
            try
            {
                String mePath = myDatPath + @"\user." + myUser + ".dat";
                if (File.Exists(mePath))
                {
                    System.IO.File.SetAttributes(mePath, FileAttributes.Normal);
                }
                FileStream meFS = new FileStream(mePath, FileMode.Create, FileAccess.Write);
                BinaryWriter meWrite = new BinaryWriter(meFS);
                //
                meWrite.Write(myUser);
                meWrite.Write(myPSW);
                meWrite.Write(myRole);
                //
                meWrite.Close();
                meFS.Close();
                System.IO.File.SetAttributes(mePath, FileAttributes.ReadOnly);
                return true;
            }
            catch
            {
                return false;
            }
        }

        //时间控制
        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
        }

        //注册
        private void InitializationRegister()
        {
            //验证MAC地址
            Int64 net_Mac = 0;
            Int64 net_Var = 0;
            //验证regedit
            Int64 reg_Mac = 0;
            Int64 reg_Var = 0;
            //验证C盘文件
            Int64 sys_Mac = 0;
            Int64 sys_Var = 0;
            Int32 sys_num = 0;
            //验证本地文件
            Int64 use_Mac = 0;
            Int64 use_Var = 0;
            Int32 use_num = 0;

            //验证MAC地址
            string macAddress = "";
            Process myProcess = null;
            StreamReader reader = null;
            try
            {
                ProcessStartInfo start = new ProcessStartInfo("cmd.exe");

                start.FileName = "ipconfig";
                start.Arguments = "/all";
                start.CreateNoWindow = true;
                start.RedirectStandardOutput = true;
                start.RedirectStandardInput = true;
                start.UseShellExecute = false;
                myProcess = Process.Start(start);
                reader = myProcess.StandardOutput;
                string line = reader.ReadLine();
                while (!reader.EndOfStream)
                {
                    if (line.ToLower().IndexOf("physical address") > 0 || line.ToLower().IndexOf("物理地址") > 0)
                    {
                        int index = line.IndexOf(":");
                        index += 2;
                        macAddress = line.Substring(index);
                        macAddress = macAddress.Replace('-', ':');
                        break;
                    }
                    line = reader.ReadLine();
                }
            }
            catch
            {

            }
            finally
            {
                if (myProcess != null)
                {
                    reader.ReadToEnd();
                    myProcess.WaitForExit();
                    myProcess.Close();
                }
                if (reader != null)
                {
                    reader.Close();
                }
            }

            if (macAddress.Length == 17)
            {
                macAddress = macAddress.Replace(":", "");
                net_Mac = Convert.ToInt64(macAddress, 16);
                net_Var = net_Mac;
                while ((net_Var % 2) == 0)
                {
                    net_Var = net_Var / 2;
                }
                while ((net_Var % 3) == 0)
                {
                    net_Var = net_Var / 3;
                }
                while ((net_Var % 5) == 0)
                {
                    net_Var = net_Var / 5;
                }
                while ((net_Var % 7) == 0)
                {
                    net_Var = net_Var / 7;
                }
            }

            //验证regedit
            RegistryKey myKey = Registry.LocalMachine.OpenSubKey("software");
            string[] names = myKey.GetSubKeyNames();
            foreach (string keyName in names)
            {
                if (keyName == "WinES")
                {
                    myKey = Registry.LocalMachine.OpenSubKey("software\\WinES");
                    reg_Mac = Convert.ToInt64(myKey.GetValue("input").ToString());
                    reg_Var = Convert.ToInt64(myKey.GetValue("ouput").ToString());
                }
            }
            myKey.Close();

            //验证C盘文件
            if (!File.Exists("C:\\Windows\\user.dat"))
            {
                if (File.Exists(Application.StartupPath + @"\dat" + @"\user.num"))
                {
                    File.Copy((Application.StartupPath + @"\dat" + @"\user.num"), ("C:\\Windows\\user.dat"), true);
                }
            }
            if (File.Exists("C:\\Windows\\user.dat"))
            {
                //读取用户信息
                FileStream meFS = new FileStream("C:\\Windows\\user.dat", FileMode.Open, FileAccess.Read);
                BinaryReader meRead = new BinaryReader(meFS);
                if (meFS.Length > 0)
                {
                    //有内容文件
                    sys_Mac = meRead.ReadInt64();
                    sys_Var = meRead.ReadInt64();
                    sys_num = meRead.ReadInt32();
                }
                meRead.Close();
                meFS.Close();
            }

            //验证本地文件
            if (!File.Exists(Application.StartupPath + @"\dat" + @"\user.num"))
            {
                if (File.Exists("C:\\Windows\\user.dat"))
                {
                    if (!Directory.Exists(Application.StartupPath + @"\dat"))
                    {
                        Directory.CreateDirectory(Application.StartupPath + @"\dat");
                    }
                    File.Copy(("C:\\Windows\\user.dat"), (Application.StartupPath + @"\dat" + @"\user.num"), true);
                }
            }
            if (File.Exists(Application.StartupPath + @"\dat" + @"\user.num"))
            {
                //读取用户信息
                FileStream meFS = new FileStream((Application.StartupPath + @"\dat" + @"\user.num"), FileMode.Open, FileAccess.Read);
                BinaryReader meRead = new BinaryReader(meFS);
                if (meFS.Length > 0)
                {
                    //有内容文件
                    use_Mac = meRead.ReadInt64();
                    use_Var = meRead.ReadInt64();
                    use_num = meRead.ReadInt32();
                }
                meRead.Close();
                meFS.Close();
            }

            //注册分析
            if ((net_Mac == reg_Mac) && (net_Var == reg_Var) && (sys_Mac == use_Mac) && (sys_Var == use_Var) && (net_Mac == use_Mac) && (net_Var == use_Var))
            {
                MyDevice.myPC = 1;
                MyDevice.myMac = sys_Mac.ToString();
                MyDevice.myVar = sys_Var;
            }
            else
            {
            }
        }
    }
}
