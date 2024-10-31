namespace Base.UI.MenuUser
{
    partial class MenuAccountForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MenuAccountForm));
            this.label5 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.button5 = new DTiws.View.ButtonX();
            this.button3 = new DTiws.View.ButtonX();
            this.button4 = new DTiws.View.ButtonX();
            this.button2 = new DTiws.View.ButtonX();
            this.button1 = new DTiws.View.ButtonX();
            this.SuspendLayout();
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("宋体", 10.5F);
            this.label5.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label5.Location = new System.Drawing.Point(101, 146);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(0, 14);
            this.label5.TabIndex = 132;
            this.label5.Visible = false;
            // 
            // textBox1
            // 
            this.textBox1.Font = new System.Drawing.Font("宋体", 10.5F);
            this.textBox1.Location = new System.Drawing.Point(107, 71);
            this.textBox1.Name = "textBox1";
            this.textBox1.PasswordChar = '*';
            this.textBox1.Size = new System.Drawing.Size(100, 23);
            this.textBox1.TabIndex = 127;
            this.textBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
            // 
            // textBox2
            // 
            this.textBox2.Font = new System.Drawing.Font("宋体", 10.5F);
            this.textBox2.Location = new System.Drawing.Point(107, 107);
            this.textBox2.Name = "textBox2";
            this.textBox2.PasswordChar = '*';
            this.textBox2.Size = new System.Drawing.Size(100, 23);
            this.textBox2.TabIndex = 128;
            this.textBox2.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 10.5F);
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(31, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 14);
            this.label1.TabIndex = 122;
            this.label1.Text = "用户名：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 10.5F);
            this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label2.Location = new System.Drawing.Point(31, 74);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 14);
            this.label2.TabIndex = 123;
            this.label2.Text = "密  码：";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("宋体", 10.5F);
            this.label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label3.Location = new System.Drawing.Point(27, 110);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 14);
            this.label3.TabIndex = 124;
            this.label3.Text = "确认密码：";
            this.label3.Visible = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("宋体", 10.5F);
            this.label4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label4.Location = new System.Drawing.Point(110, 152);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(0, 14);
            this.label4.TabIndex = 125;
            this.label4.Visible = false;
            // 
            // comboBox1
            // 
            this.comboBox1.Font = new System.Drawing.Font("宋体", 10.5F);
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "admin"});
            this.comboBox1.Location = new System.Drawing.Point(107, 35);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(100, 22);
            this.comboBox1.TabIndex = 126;
            this.comboBox1.Text = "admin";
            this.comboBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.comboBox1_KeyDown);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // button5
            // 
            this.button5.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.button5.EnterForeColor = System.Drawing.Color.White;
            this.button5.Font = new System.Drawing.Font("宋体", 12F);
            this.button5.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.button5.HoverForeColor = System.Drawing.Color.White;
            this.button5.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.button5.Location = new System.Drawing.Point(132, 146);
            this.button5.Name = "button5";
            this.button5.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.button5.PressForeColor = System.Drawing.Color.White;
            this.button5.Radius = 6;
            this.button5.Size = new System.Drawing.Size(75, 29);
            this.button5.TabIndex = 134;
            this.button5.Text = "确 认";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button3
            // 
            this.button3.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.button3.EnterForeColor = System.Drawing.Color.White;
            this.button3.Font = new System.Drawing.Font("宋体", 12F);
            this.button3.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.button3.HoverForeColor = System.Drawing.Color.White;
            this.button3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.button3.Location = new System.Drawing.Point(28, 146);
            this.button3.Name = "button3";
            this.button3.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.button3.PressForeColor = System.Drawing.Color.White;
            this.button3.Radius = 6;
            this.button3.Size = new System.Drawing.Size(75, 29);
            this.button3.TabIndex = 133;
            this.button3.Text = "修改密码";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.button4.EnterForeColor = System.Drawing.Color.White;
            this.button4.Font = new System.Drawing.Font("宋体", 12F);
            this.button4.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.button4.HoverForeColor = System.Drawing.Color.White;
            this.button4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.button4.Location = new System.Drawing.Point(249, 146);
            this.button4.Name = "button4";
            this.button4.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.button4.PressForeColor = System.Drawing.Color.White;
            this.button4.Radius = 6;
            this.button4.Size = new System.Drawing.Size(59, 29);
            this.button4.TabIndex = 131;
            this.button4.Text = "取 消";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button2
            // 
            this.button2.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.button2.EnterForeColor = System.Drawing.Color.White;
            this.button2.Font = new System.Drawing.Font("宋体", 12F);
            this.button2.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.button2.HoverForeColor = System.Drawing.Color.White;
            this.button2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.button2.Location = new System.Drawing.Point(249, 102);
            this.button2.Name = "button2";
            this.button2.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.button2.PressForeColor = System.Drawing.Color.White;
            this.button2.Radius = 6;
            this.button2.Size = new System.Drawing.Size(59, 29);
            this.button2.TabIndex = 130;
            this.button2.Text = "注 册";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.button1.EnterForeColor = System.Drawing.Color.White;
            this.button1.Font = new System.Drawing.Font("宋体", 12F);
            this.button1.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.button1.HoverForeColor = System.Drawing.Color.White;
            this.button1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.button1.Location = new System.Drawing.Point(249, 35);
            this.button1.Name = "button1";
            this.button1.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.button1.PressForeColor = System.Drawing.Color.White;
            this.button1.Radius = 6;
            this.button1.Size = new System.Drawing.Size(59, 53);
            this.button1.TabIndex = 129;
            this.button1.Text = "登 录";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // MenuAccountForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(334, 211);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.comboBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MenuAccountForm";
            this.Text = "欢迎使用！";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MenuAccountForm_FormClosed);
            this.Load += new System.EventHandler(this.MenuAccountForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DTiws.View.ButtonX button3;
        private System.Windows.Forms.Label label5;
        private DTiws.View.ButtonX button4;
        private DTiws.View.ButtonX button2;
        private DTiws.View.ButtonX button1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Timer timer1;
        private DTiws.View.ButtonX button5;
    }
}