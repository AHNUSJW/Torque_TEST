namespace Base.UI.MenuDevice
{
    partial class MenuConnectForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MenuConnectForm));
            System.Windows.Forms.TreeNode treeNode16 = new System.Windows.Forms.TreeNode("  有线连接");
            System.Windows.Forms.TreeNode treeNode17 = new System.Windows.Forms.TreeNode("  蓝牙连接");
            System.Windows.Forms.TreeNode treeNode18 = new System.Windows.Forms.TreeNode("  RS485连接");
            System.Windows.Forms.TreeNode treeNode19 = new System.Windows.Forms.TreeNode("  接收器连接");
            System.Windows.Forms.TreeNode treeNode20 = new System.Windows.Forms.TreeNode("  路由器WiFi连接");
            this.treeViewEx1 = new HZH_Controls.Controls.TreeViewEx();
            this.groupBoxConnect = new System.Windows.Forms.GroupBox();
            this.comboBox_baud = new System.Windows.Forms.ComboBox();
            this.label_parity = new System.Windows.Forms.Label();
            this.comboBox_stopbit = new System.Windows.Forms.ComboBox();
            this.label_stopbit = new System.Windows.Forms.Label();
            this.comboBox_parity = new System.Windows.Forms.ComboBox();
            this.label_baud = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.bt_connect = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.bt_close = new System.Windows.Forms.Button();
            this.bt_scan = new System.Windows.Forms.Button();
            this.bt_refresh = new System.Windows.Forms.Button();
            this.comboBox0_port = new System.Windows.Forms.ComboBox();
            this.label13 = new System.Windows.Forms.Label();
            this.timer_USB = new System.Windows.Forms.Timer(this.components);
            this.timer_XF = new System.Windows.Forms.Timer(this.components);
            this.timer_TCP = new System.Windows.Forms.Timer(this.components);
            this.timer_RS485 = new System.Windows.Forms.Timer(this.components);
            this.btn_unbind = new System.Windows.Forms.Button();
            this.groupBoxConnect.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeViewEx1
            // 
            this.treeViewEx1.BackColor = System.Drawing.Color.CadetBlue;
            this.treeViewEx1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeViewEx1.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawAll;
            this.treeViewEx1.FullRowSelect = true;
            this.treeViewEx1.HideSelection = false;
            this.treeViewEx1.IsShowByCustomModel = true;
            this.treeViewEx1.IsShowTip = false;
            this.treeViewEx1.ItemHeight = 60;
            this.treeViewEx1.Location = new System.Drawing.Point(7, 25);
            this.treeViewEx1.LstTips = ((System.Collections.Generic.Dictionary<string, string>)(resources.GetObject("treeViewEx1.LstTips")));
            this.treeViewEx1.Name = "treeViewEx1";
            this.treeViewEx1.NodeBackgroundColor = System.Drawing.Color.CadetBlue;
            this.treeViewEx1.NodeDownPic = ((System.Drawing.Image)(resources.GetObject("treeViewEx1.NodeDownPic")));
            this.treeViewEx1.NodeForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(62)))));
            this.treeViewEx1.NodeHeight = 60;
            this.treeViewEx1.NodeIsShowSplitLine = true;
            treeNode16.Name = "Wired";
            treeNode16.NodeFont = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Bold);
            treeNode16.Text = "  有线连接";
            treeNode17.Name = "Bluetooth";
            treeNode17.NodeFont = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Bold);
            treeNode17.Text = "  蓝牙连接";
            treeNode18.Name = "RS485";
            treeNode18.NodeFont = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Bold);
            treeNode18.Text = "  RS485连接";
            treeNode19.Name = "Receiver";
            treeNode19.NodeFont = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Bold);
            treeNode19.Text = "  接收器连接";
            treeNode20.Name = "Router";
            treeNode20.NodeFont = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Bold);
            treeNode20.Text = "  路由器WiFi连接";
            this.treeViewEx1.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode16,
            treeNode17,
            treeNode18,
            treeNode19,
            treeNode20});
            this.treeViewEx1.NodeSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.treeViewEx1.NodeSelectedForeColor = System.Drawing.Color.White;
            this.treeViewEx1.NodeSplitLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(232)))), ((int)(((byte)(232)))));
            this.treeViewEx1.NodeUpPic = ((System.Drawing.Image)(resources.GetObject("treeViewEx1.NodeUpPic")));
            this.treeViewEx1.ParentNodeCanSelect = true;
            this.treeViewEx1.ShowLines = false;
            this.treeViewEx1.ShowPlusMinus = false;
            this.treeViewEx1.ShowRootLines = false;
            this.treeViewEx1.Size = new System.Drawing.Size(202, 443);
            this.treeViewEx1.TabIndex = 168;
            this.treeViewEx1.TipFont = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.treeViewEx1.TipImage = ((System.Drawing.Image)(resources.GetObject("treeViewEx1.TipImage")));
            this.treeViewEx1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewEx1_AfterSelect);
            // 
            // groupBoxConnect
            // 
            this.groupBoxConnect.Controls.Add(this.btn_unbind);
            this.groupBoxConnect.Controls.Add(this.comboBox_baud);
            this.groupBoxConnect.Controls.Add(this.label_parity);
            this.groupBoxConnect.Controls.Add(this.comboBox_stopbit);
            this.groupBoxConnect.Controls.Add(this.label_stopbit);
            this.groupBoxConnect.Controls.Add(this.comboBox_parity);
            this.groupBoxConnect.Controls.Add(this.label_baud);
            this.groupBoxConnect.Controls.Add(this.textBox2);
            this.groupBoxConnect.Controls.Add(this.bt_connect);
            this.groupBoxConnect.Controls.Add(this.textBox1);
            this.groupBoxConnect.Controls.Add(this.label17);
            this.groupBoxConnect.Controls.Add(this.bt_close);
            this.groupBoxConnect.Controls.Add(this.bt_scan);
            this.groupBoxConnect.Controls.Add(this.bt_refresh);
            this.groupBoxConnect.Controls.Add(this.comboBox0_port);
            this.groupBoxConnect.Controls.Add(this.label13);
            this.groupBoxConnect.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxConnect.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBoxConnect.Location = new System.Drawing.Point(0, 0);
            this.groupBoxConnect.Name = "groupBoxConnect";
            this.groupBoxConnect.Size = new System.Drawing.Size(679, 472);
            this.groupBoxConnect.TabIndex = 169;
            this.groupBoxConnect.TabStop = false;
            this.groupBoxConnect.Text = "设备连接";
            // 
            // comboBox_baud
            // 
            this.comboBox_baud.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_baud.FormattingEnabled = true;
            this.comboBox_baud.Items.AddRange(new object[] {
            "1200",
            "2400",
            "4800",
            "9600",
            "14400",
            "19200",
            "38400",
            "57600",
            "115200"});
            this.comboBox_baud.Location = new System.Drawing.Point(292, 128);
            this.comboBox_baud.Name = "comboBox_baud";
            this.comboBox_baud.Size = new System.Drawing.Size(74, 25);
            this.comboBox_baud.TabIndex = 66;
            // 
            // label_parity
            // 
            this.label_parity.AutoSize = true;
            this.label_parity.Font = new System.Drawing.Font("宋体", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_parity.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label_parity.Location = new System.Drawing.Point(515, 131);
            this.label_parity.Name = "label_parity";
            this.label_parity.Size = new System.Drawing.Size(60, 15);
            this.label_parity.TabIndex = 71;
            this.label_parity.Text = "校验位:";
            // 
            // comboBox_stopbit
            // 
            this.comboBox_stopbit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_stopbit.FormattingEnabled = true;
            this.comboBox_stopbit.Items.AddRange(new object[] {
            "1",
            "2"});
            this.comboBox_stopbit.Location = new System.Drawing.Point(429, 128);
            this.comboBox_stopbit.Name = "comboBox_stopbit";
            this.comboBox_stopbit.Size = new System.Drawing.Size(74, 25);
            this.comboBox_stopbit.TabIndex = 67;
            // 
            // label_stopbit
            // 
            this.label_stopbit.AutoSize = true;
            this.label_stopbit.Font = new System.Drawing.Font("宋体", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_stopbit.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label_stopbit.Location = new System.Drawing.Point(368, 131);
            this.label_stopbit.Name = "label_stopbit";
            this.label_stopbit.Size = new System.Drawing.Size(60, 15);
            this.label_stopbit.TabIndex = 70;
            this.label_stopbit.Text = "停止位:";
            // 
            // comboBox_parity
            // 
            this.comboBox_parity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_parity.FormattingEnabled = true;
            this.comboBox_parity.Items.AddRange(new object[] {
            "None",
            "Odd(奇校验)",
            "Even(偶校验)",
            "Mark",
            "Space"});
            this.comboBox_parity.Location = new System.Drawing.Point(575, 128);
            this.comboBox_parity.Name = "comboBox_parity";
            this.comboBox_parity.Size = new System.Drawing.Size(92, 25);
            this.comboBox_parity.TabIndex = 68;
            // 
            // label_baud
            // 
            this.label_baud.AutoSize = true;
            this.label_baud.Font = new System.Drawing.Font("宋体", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_baud.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label_baud.Location = new System.Drawing.Point(231, 131);
            this.label_baud.Name = "label_baud";
            this.label_baud.Size = new System.Drawing.Size(60, 15);
            this.label_baud.TabIndex = 69;
            this.label_baud.Text = "波特率:";
            // 
            // textBox2
            // 
            this.textBox2.Font = new System.Drawing.Font("宋体", 14.25F);
            this.textBox2.Location = new System.Drawing.Point(230, 171);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox2.Size = new System.Drawing.Size(437, 297);
            this.textBox2.TabIndex = 59;
            this.textBox2.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            // 
            // bt_connect
            // 
            this.bt_connect.BackColor = System.Drawing.Color.CadetBlue;
            this.bt_connect.Font = new System.Drawing.Font("微软雅黑", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bt_connect.ForeColor = System.Drawing.Color.White;
            this.bt_connect.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.bt_connect.Location = new System.Drawing.Point(605, 22);
            this.bt_connect.Name = "bt_connect";
            this.bt_connect.Size = new System.Drawing.Size(62, 38);
            this.bt_connect.TabIndex = 58;
            this.bt_connect.Text = "连 接";
            this.bt_connect.UseVisualStyleBackColor = false;
            this.bt_connect.Click += new System.EventHandler(this.bt_connect_Click);
            // 
            // textBox1
            // 
            this.textBox1.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox1.Location = new System.Drawing.Point(351, 80);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(89, 29);
            this.textBox1.TabIndex = 53;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("宋体", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label17.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label17.Location = new System.Drawing.Point(231, 87);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(101, 15);
            this.label17.TabIndex = 49;
            this.label17.Text = "站点(1-255):";
            // 
            // bt_close
            // 
            this.bt_close.BackColor = System.Drawing.Color.CadetBlue;
            this.bt_close.Font = new System.Drawing.Font("微软雅黑", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bt_close.ForeColor = System.Drawing.Color.White;
            this.bt_close.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.bt_close.Location = new System.Drawing.Point(529, 22);
            this.bt_close.Name = "bt_close";
            this.bt_close.Size = new System.Drawing.Size(62, 38);
            this.bt_close.TabIndex = 50;
            this.bt_close.Text = "打 开";
            this.bt_close.UseVisualStyleBackColor = false;
            this.bt_close.Click += new System.EventHandler(this.bt_close_Click);
            // 
            // bt_scan
            // 
            this.bt_scan.BackColor = System.Drawing.Color.CadetBlue;
            this.bt_scan.Font = new System.Drawing.Font("微软雅黑", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bt_scan.ForeColor = System.Drawing.Color.White;
            this.bt_scan.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.bt_scan.Location = new System.Drawing.Point(446, 74);
            this.bt_scan.Name = "bt_scan";
            this.bt_scan.Size = new System.Drawing.Size(62, 38);
            this.bt_scan.TabIndex = 51;
            this.bt_scan.Text = "扫 描";
            this.bt_scan.UseVisualStyleBackColor = false;
            this.bt_scan.Click += new System.EventHandler(this.bt_scan_Click);
            // 
            // bt_refresh
            // 
            this.bt_refresh.BackColor = System.Drawing.Color.CadetBlue;
            this.bt_refresh.Font = new System.Drawing.Font("微软雅黑", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bt_refresh.ForeColor = System.Drawing.Color.White;
            this.bt_refresh.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.bt_refresh.Location = new System.Drawing.Point(446, 22);
            this.bt_refresh.Name = "bt_refresh";
            this.bt_refresh.Size = new System.Drawing.Size(62, 38);
            this.bt_refresh.TabIndex = 52;
            this.bt_refresh.Text = "刷 新";
            this.bt_refresh.UseVisualStyleBackColor = false;
            this.bt_refresh.Click += new System.EventHandler(this.bt_refresh_Click);
            // 
            // comboBox0_port
            // 
            this.comboBox0_port.BackColor = System.Drawing.Color.Snow;
            this.comboBox0_port.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox0_port.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.comboBox0_port.FormattingEnabled = true;
            this.comboBox0_port.Location = new System.Drawing.Point(283, 26);
            this.comboBox0_port.Name = "comboBox0_port";
            this.comboBox0_port.Size = new System.Drawing.Size(157, 33);
            this.comboBox0_port.TabIndex = 45;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("宋体", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label13.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label13.Location = new System.Drawing.Point(231, 37);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(52, 15);
            this.label13.TabIndex = 41;
            this.label13.Text = "串口：";
            // 
            // timer_USB
            // 
            this.timer_USB.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // timer_XF
            // 
            this.timer_XF.Interval = 500;
            this.timer_XF.Tick += new System.EventHandler(this.timer2_Tick);
            // 
            // timer_TCP
            // 
            this.timer_TCP.Interval = 300;
            this.timer_TCP.Tick += new System.EventHandler(this.timer3_Tick);
            // 
            // timer_RS485
            // 
            this.timer_RS485.Tick += new System.EventHandler(this.timer_RS485_Tick);
            // 
            // btn_unbind
            // 
            this.btn_unbind.BackColor = System.Drawing.Color.CadetBlue;
            this.btn_unbind.Font = new System.Drawing.Font("微软雅黑", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_unbind.ForeColor = System.Drawing.Color.White;
            this.btn_unbind.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btn_unbind.Location = new System.Drawing.Point(605, 75);
            this.btn_unbind.Name = "btn_unbind";
            this.btn_unbind.Size = new System.Drawing.Size(62, 38);
            this.btn_unbind.TabIndex = 72;
            this.btn_unbind.Text = "解 绑";
            this.btn_unbind.UseVisualStyleBackColor = false;
            this.btn_unbind.Visible = false;
            this.btn_unbind.Click += new System.EventHandler(this.btn_unbind_Click);
            // 
            // MenuConnectForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(679, 472);
            this.Controls.Add(this.treeViewEx1);
            this.Controls.Add(this.groupBoxConnect);
            this.Name = "MenuConnectForm";
            this.Text = "设备连接";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MenuConnectForm_FormClosing);
            this.Load += new System.EventHandler(this.MenuConnectForm_Load);
            this.groupBoxConnect.ResumeLayout(false);
            this.groupBoxConnect.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private HZH_Controls.Controls.TreeViewEx treeViewEx1;
        private System.Windows.Forms.GroupBox groupBoxConnect;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Button bt_connect;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Button bt_close;
        private System.Windows.Forms.Button bt_scan;
        private System.Windows.Forms.Button bt_refresh;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.ComboBox comboBox0_port;
        private System.Windows.Forms.Timer timer_USB;
        private System.Windows.Forms.Timer timer_XF;
        private System.Windows.Forms.Timer timer_TCP;
        private System.Windows.Forms.ComboBox comboBox_baud;
        private System.Windows.Forms.Label label_parity;
        private System.Windows.Forms.ComboBox comboBox_stopbit;
        private System.Windows.Forms.Label label_stopbit;
        private System.Windows.Forms.ComboBox comboBox_parity;
        private System.Windows.Forms.Label label_baud;
        private System.Windows.Forms.Timer timer_RS485;
        private System.Windows.Forms.Button btn_unbind;
    }
}