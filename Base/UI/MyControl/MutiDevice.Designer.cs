namespace MaciX.UI.MyControl
{
    partial class MutiDevice
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.ucSignalLamp1 = new HZH_Controls.Controls.UCSignalLamp();
            this.address = new System.Windows.Forms.Label();
            this.signalOutput1 = new System.Windows.Forms.Label();
            this.signalUnit1 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.buttonX2 = new DTiws.View.ButtonX();
            this.buttonX1 = new DTiws.View.ButtonX();
            this.SuspendLayout();
            // 
            // ucSignalLamp1
            // 
            this.ucSignalLamp1.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ucSignalLamp1.IsHighlight = true;
            this.ucSignalLamp1.IsShowBorder = false;
            this.ucSignalLamp1.LampColor = new System.Drawing.Color[] {
        System.Drawing.Color.Black};
            this.ucSignalLamp1.Location = new System.Drawing.Point(9, 9);
            this.ucSignalLamp1.Name = "ucSignalLamp1";
            this.ucSignalLamp1.Size = new System.Drawing.Size(27, 27);
            this.ucSignalLamp1.TabIndex = 115;
            this.ucSignalLamp1.TwinkleSpeed = 0;
            // 
            // address
            // 
            this.address.AutoSize = true;
            this.address.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.address.Location = new System.Drawing.Point(36, 14);
            this.address.Name = "address";
            this.address.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.address.Size = new System.Drawing.Size(86, 19);
            this.address.TabIndex = 116;
            this.address.Text = "ID: 255";
            this.address.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // signalOutput1
            // 
            this.signalOutput1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.signalOutput1.AutoSize = true;
            this.signalOutput1.Cursor = System.Windows.Forms.Cursors.Default;
            this.signalOutput1.Font = new System.Drawing.Font("Courier New", 46F, System.Drawing.FontStyle.Bold);
            this.signalOutput1.ForeColor = System.Drawing.Color.Black;
            this.signalOutput1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.signalOutput1.Location = new System.Drawing.Point(-6, 47);
            this.signalOutput1.Name = "signalOutput1";
            this.signalOutput1.Size = new System.Drawing.Size(363, 70);
            this.signalOutput1.TabIndex = 119;
            this.signalOutput1.Text = "0000.0000";
            this.signalOutput1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.signalOutput1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.signalOutput1_MouseDoubleClick);
            // 
            // signalUnit1
            // 
            this.signalUnit1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.signalUnit1.AutoSize = true;
            this.signalUnit1.Font = new System.Drawing.Font("等线", 25F, System.Drawing.FontStyle.Bold);
            this.signalUnit1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.signalUnit1.Location = new System.Drawing.Point(359, 64);
            this.signalUnit1.Name = "signalUnit1";
            this.signalUnit1.Size = new System.Drawing.Size(103, 36);
            this.signalUnit1.TabIndex = 130;
            this.signalUnit1.Text = "mV/V";
            this.signalUnit1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // comboBox1
            // 
            this.comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(352, 10);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(89, 29);
            this.comboBox1.TabIndex = 133;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // buttonX2
            // 
            this.buttonX2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonX2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonX2.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.buttonX2.EnterForeColor = System.Drawing.Color.White;
            this.buttonX2.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonX2.HoverBackColor = System.Drawing.Color.CornflowerBlue;
            this.buttonX2.HoverForeColor = System.Drawing.Color.White;
            this.buttonX2.Location = new System.Drawing.Point(214, 10);
            this.buttonX2.Name = "buttonX2";
            this.buttonX2.PressBackColor = System.Drawing.Color.SkyBlue;
            this.buttonX2.PressForeColor = System.Drawing.Color.White;
            this.buttonX2.Radius = 6;
            this.buttonX2.Size = new System.Drawing.Size(60, 29);
            this.buttonX2.TabIndex = 134;
            this.buttonX2.Text = "去皮";
            this.buttonX2.UseVisualStyleBackColor = true;
            this.buttonX2.Click += new System.EventHandler(this.buttonX2_Click);
            // 
            // buttonX1
            // 
            this.buttonX1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonX1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonX1.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.buttonX1.EnterForeColor = System.Drawing.Color.White;
            this.buttonX1.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonX1.HoverBackColor = System.Drawing.Color.CornflowerBlue;
            this.buttonX1.HoverForeColor = System.Drawing.Color.White;
            this.buttonX1.Location = new System.Drawing.Point(283, 10);
            this.buttonX1.Name = "buttonX1";
            this.buttonX1.PressBackColor = System.Drawing.Color.SkyBlue;
            this.buttonX1.PressForeColor = System.Drawing.Color.White;
            this.buttonX1.Radius = 6;
            this.buttonX1.Size = new System.Drawing.Size(60, 29);
            this.buttonX1.TabIndex = 118;
            this.buttonX1.Text = "归零";
            this.buttonX1.UseVisualStyleBackColor = true;
            this.buttonX1.Click += new System.EventHandler(this.buttonX1_Click);
            // 
            // MutiDevice
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Controls.Add(this.buttonX2);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.signalUnit1);
            this.Controls.Add(this.signalOutput1);
            this.Controls.Add(this.buttonX1);
            this.Controls.Add(this.address);
            this.Controls.Add(this.ucSignalLamp1);
            this.Cursor = System.Windows.Forms.Cursors.Default;
            this.Name = "MutiDevice";
            this.Size = new System.Drawing.Size(456, 108);
            this.Load += new System.EventHandler(this.MutiDevice_Load);
            this.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.MutiDevice_MouseDoubleClick);
            this.Resize += new System.EventHandler(this.MutiDevice_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private HZH_Controls.Controls.UCSignalLamp ucSignalLamp1;
        private System.Windows.Forms.Label address;
        private DTiws.View.ButtonX buttonX1;
        private System.Windows.Forms.Label signalOutput1;
        private System.Windows.Forms.Label signalUnit1;
        private System.Windows.Forms.ComboBox comboBox1;
        private DTiws.View.ButtonX buttonX2;
    }
}
