
namespace MaciX.UI.MyControl
{
    partial class MutiDevice485
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
            this.signalUnit1 = new System.Windows.Forms.Label();
            this.signalOutput1 = new System.Windows.Forms.Label();
            this.address = new System.Windows.Forms.Label();
            this.ucSignalLamp1 = new HZH_Controls.Controls.UCSignalLamp();
            this.SuspendLayout();
            // 
            // signalUnit1
            // 
            this.signalUnit1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.signalUnit1.AutoSize = true;
            this.signalUnit1.Font = new System.Drawing.Font("等线", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.signalUnit1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.signalUnit1.Location = new System.Drawing.Point(292, 54);
            this.signalUnit1.Name = "signalUnit1";
            this.signalUnit1.Size = new System.Drawing.Size(73, 25);
            this.signalUnit1.TabIndex = 140;
            this.signalUnit1.Text = "mV/V";
            this.signalUnit1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // signalOutput1
            // 
            this.signalOutput1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.signalOutput1.AutoSize = true;
            this.signalOutput1.Cursor = System.Windows.Forms.Cursors.Default;
            this.signalOutput1.Font = new System.Drawing.Font("Courier New", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.signalOutput1.ForeColor = System.Drawing.Color.Black;
            this.signalOutput1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.signalOutput1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.signalOutput1.Location = new System.Drawing.Point(24, 33);
            this.signalOutput1.Name = "signalOutput1";
            this.signalOutput1.Size = new System.Drawing.Size(52, 54);
            this.signalOutput1.TabIndex = 139;
            this.signalOutput1.Text = "0";
            this.signalOutput1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // address
            // 
            this.address.AutoSize = true;
            this.address.Font = new System.Drawing.Font("等线", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.address.Location = new System.Drawing.Point(34, 11);
            this.address.Name = "address";
            this.address.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.address.Size = new System.Drawing.Size(61, 18);
            this.address.TabIndex = 136;
            this.address.Text = "ID: 255";
            this.address.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ucSignalLamp1
            // 
            this.ucSignalLamp1.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ucSignalLamp1.IsHighlight = true;
            this.ucSignalLamp1.IsShowBorder = false;
            this.ucSignalLamp1.LampColor = new System.Drawing.Color[] {
        System.Drawing.Color.Black};
            this.ucSignalLamp1.Location = new System.Drawing.Point(7, 6);
            this.ucSignalLamp1.Name = "ucSignalLamp1";
            this.ucSignalLamp1.Size = new System.Drawing.Size(24, 24);
            this.ucSignalLamp1.TabIndex = 135;
            this.ucSignalLamp1.TwinkleSpeed = 0;
            // 
            // MutiDevice485
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.Controls.Add(this.ucSignalLamp1);
            this.Controls.Add(this.signalOutput1);
            this.Controls.Add(this.signalUnit1);
            this.Controls.Add(this.address);
            this.Name = "MutiDevice485";
            this.Size = new System.Drawing.Size(368, 109);
            this.Load += new System.EventHandler(this.MutiDevice485_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label signalUnit1;
        private System.Windows.Forms.Label signalOutput1;
        private System.Windows.Forms.Label address;
        private HZH_Controls.Controls.UCSignalLamp ucSignalLamp1;
    }
}
