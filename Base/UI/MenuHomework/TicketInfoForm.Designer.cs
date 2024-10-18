namespace Base.UI.MenuHomework
{
    partial class TicketInfoForm
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
            this.label2 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.bt_create = new System.Windows.Forms.Button();
            this.tb_time = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.tb_note = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tb_WoName = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.tb_WoNum = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tb_WoArea = new System.Windows.Forms.TextBox();
            this.bt_picLoad = new System.Windows.Forms.Button();
            this.tb_ImagePath = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tb_WoFactory = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tb_WoLine = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tb_WoStation = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.tb_WoStamp = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.tb_WoBat = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.tb_AngleResist = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 10.8F);
            this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label2.Location = new System.Drawing.Point(65, 222);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 15);
            this.label2.TabIndex = 53;
            this.label2.Text = "工单名称";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label9.Location = new System.Drawing.Point(56, 56);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(281, 12);
            this.label9.TabIndex = 52;
            this.label9.Text = "----------------------------------------------";
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("楷体", 22.2F);
            this.label8.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label8.Location = new System.Drawing.Point(110, 28);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(178, 30);
            this.label8.TabIndex = 51;
            this.label8.Text = "工 单 信 息";
            // 
            // bt_create
            // 
            this.bt_create.Font = new System.Drawing.Font("宋体", 10.8F);
            this.bt_create.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.bt_create.Location = new System.Drawing.Point(287, 618);
            this.bt_create.Margin = new System.Windows.Forms.Padding(2);
            this.bt_create.Name = "bt_create";
            this.bt_create.Size = new System.Drawing.Size(80, 38);
            this.bt_create.TabIndex = 50;
            this.bt_create.Text = "生成工单";
            this.bt_create.UseVisualStyleBackColor = true;
            this.bt_create.Click += new System.EventHandler(this.bt_create_Click);
            // 
            // tb_time
            // 
            this.tb_time.Font = new System.Drawing.Font("宋体", 10.8F);
            this.tb_time.Location = new System.Drawing.Point(137, 81);
            this.tb_time.Margin = new System.Windows.Forms.Padding(2);
            this.tb_time.Name = "tb_time";
            this.tb_time.ReadOnly = true;
            this.tb_time.Size = new System.Drawing.Size(200, 24);
            this.tb_time.TabIndex = 49;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("宋体", 10.8F);
            this.label7.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label7.Location = new System.Drawing.Point(73, 84);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(37, 15);
            this.label7.TabIndex = 48;
            this.label7.Text = "日期";
            // 
            // tb_note
            // 
            this.tb_note.Font = new System.Drawing.Font("宋体", 10.8F);
            this.tb_note.Location = new System.Drawing.Point(138, 569);
            this.tb_note.Margin = new System.Windows.Forms.Padding(2);
            this.tb_note.MaxLength = 15;
            this.tb_note.Name = "tb_note";
            this.tb_note.Size = new System.Drawing.Size(200, 24);
            this.tb_note.TabIndex = 43;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("宋体", 10.8F);
            this.label4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label4.Location = new System.Drawing.Point(74, 572);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(37, 15);
            this.label4.TabIndex = 42;
            this.label4.Text = "备注";
            // 
            // tb_WoName
            // 
            this.tb_WoName.Font = new System.Drawing.Font("宋体", 10.8F);
            this.tb_WoName.Location = new System.Drawing.Point(136, 219);
            this.tb_WoName.Margin = new System.Windows.Forms.Padding(2);
            this.tb_WoName.MaxLength = 15;
            this.tb_WoName.Name = "tb_WoName";
            this.tb_WoName.Size = new System.Drawing.Size(200, 24);
            this.tb_WoName.TabIndex = 41;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("宋体", 10.8F);
            this.label15.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label15.Location = new System.Drawing.Point(65, 264);
            this.label15.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(52, 15);
            this.label15.TabIndex = 40;
            this.label15.Text = "工单区";
            // 
            // tb_WoNum
            // 
            this.tb_WoNum.Font = new System.Drawing.Font("宋体", 10.8F);
            this.tb_WoNum.Location = new System.Drawing.Point(137, 174);
            this.tb_WoNum.Margin = new System.Windows.Forms.Padding(2);
            this.tb_WoNum.MaxLength = 8;
            this.tb_WoNum.Name = "tb_WoNum";
            this.tb_WoNum.Size = new System.Drawing.Size(200, 24);
            this.tb_WoNum.TabIndex = 39;
            this.tb_WoNum.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tb_WoNum_KeyPress);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 10.8F);
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(65, 178);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 15);
            this.label1.TabIndex = 38;
            this.label1.Text = "工单编号";
            // 
            // tb_WoArea
            // 
            this.tb_WoArea.Font = new System.Drawing.Font("宋体", 10.8F);
            this.tb_WoArea.Location = new System.Drawing.Point(137, 261);
            this.tb_WoArea.Margin = new System.Windows.Forms.Padding(2);
            this.tb_WoArea.MaxLength = 15;
            this.tb_WoArea.Name = "tb_WoArea";
            this.tb_WoArea.Size = new System.Drawing.Size(200, 24);
            this.tb_WoArea.TabIndex = 54;
            // 
            // bt_picLoad
            // 
            this.bt_picLoad.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.bt_picLoad.Location = new System.Drawing.Point(340, 131);
            this.bt_picLoad.Margin = new System.Windows.Forms.Padding(2);
            this.bt_picLoad.Name = "bt_picLoad";
            this.bt_picLoad.Size = new System.Drawing.Size(27, 22);
            this.bt_picLoad.TabIndex = 57;
            this.bt_picLoad.Text = "...";
            this.bt_picLoad.UseVisualStyleBackColor = true;
            this.bt_picLoad.Click += new System.EventHandler(this.bt_picLoad_Click);
            // 
            // tb_ImagePath
            // 
            this.tb_ImagePath.Font = new System.Drawing.Font("宋体", 10.8F);
            this.tb_ImagePath.Location = new System.Drawing.Point(136, 131);
            this.tb_ImagePath.Margin = new System.Windows.Forms.Padding(2);
            this.tb_ImagePath.Name = "tb_ImagePath";
            this.tb_ImagePath.ReadOnly = true;
            this.tb_ImagePath.Size = new System.Drawing.Size(200, 24);
            this.tb_ImagePath.TabIndex = 56;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("宋体", 10.8F);
            this.label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label3.Location = new System.Drawing.Point(64, 135);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 15);
            this.label3.TabIndex = 55;
            this.label3.Text = "图片路径";
            // 
            // tb_WoFactory
            // 
            this.tb_WoFactory.Font = new System.Drawing.Font("宋体", 10.8F);
            this.tb_WoFactory.Location = new System.Drawing.Point(137, 306);
            this.tb_WoFactory.Margin = new System.Windows.Forms.Padding(2);
            this.tb_WoFactory.MaxLength = 15;
            this.tb_WoFactory.Name = "tb_WoFactory";
            this.tb_WoFactory.Size = new System.Drawing.Size(200, 24);
            this.tb_WoFactory.TabIndex = 59;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("宋体", 10.8F);
            this.label5.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label5.Location = new System.Drawing.Point(65, 310);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(52, 15);
            this.label5.TabIndex = 58;
            this.label5.Text = "工单厂";
            // 
            // tb_WoLine
            // 
            this.tb_WoLine.Font = new System.Drawing.Font("宋体", 10.8F);
            this.tb_WoLine.Location = new System.Drawing.Point(137, 347);
            this.tb_WoLine.Margin = new System.Windows.Forms.Padding(2);
            this.tb_WoLine.MaxLength = 15;
            this.tb_WoLine.Name = "tb_WoLine";
            this.tb_WoLine.Size = new System.Drawing.Size(200, 24);
            this.tb_WoLine.TabIndex = 61;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("宋体", 10.8F);
            this.label6.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label6.Location = new System.Drawing.Point(65, 351);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(67, 15);
            this.label6.TabIndex = 60;
            this.label6.Text = "工单产线";
            // 
            // tb_WoStation
            // 
            this.tb_WoStation.Font = new System.Drawing.Font("宋体", 10.8F);
            this.tb_WoStation.Location = new System.Drawing.Point(137, 387);
            this.tb_WoStation.Margin = new System.Windows.Forms.Padding(2);
            this.tb_WoStation.MaxLength = 15;
            this.tb_WoStation.Name = "tb_WoStation";
            this.tb_WoStation.Size = new System.Drawing.Size(200, 24);
            this.tb_WoStation.TabIndex = 63;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("宋体", 10.8F);
            this.label10.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label10.Location = new System.Drawing.Point(65, 391);
            this.label10.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(67, 15);
            this.label10.TabIndex = 62;
            this.label10.Text = "工单工位";
            // 
            // tb_WoStamp
            // 
            this.tb_WoStamp.Font = new System.Drawing.Font("宋体", 10.8F);
            this.tb_WoStamp.Location = new System.Drawing.Point(138, 479);
            this.tb_WoStamp.Margin = new System.Windows.Forms.Padding(2);
            this.tb_WoStamp.MaxLength = 15;
            this.tb_WoStamp.Name = "tb_WoStamp";
            this.tb_WoStamp.Size = new System.Drawing.Size(200, 24);
            this.tb_WoStamp.TabIndex = 67;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("宋体", 10.8F);
            this.label11.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label11.Location = new System.Drawing.Point(66, 483);
            this.label11.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(67, 15);
            this.label11.TabIndex = 66;
            this.label11.Text = "时间标识";
            // 
            // tb_WoBat
            // 
            this.tb_WoBat.Font = new System.Drawing.Font("宋体", 10.8F);
            this.tb_WoBat.Location = new System.Drawing.Point(137, 432);
            this.tb_WoBat.Margin = new System.Windows.Forms.Padding(2);
            this.tb_WoBat.MaxLength = 15;
            this.tb_WoBat.Name = "tb_WoBat";
            this.tb_WoBat.Size = new System.Drawing.Size(200, 24);
            this.tb_WoBat.TabIndex = 65;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("宋体", 10.8F);
            this.label12.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label12.Location = new System.Drawing.Point(65, 438);
            this.label12.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(67, 15);
            this.label12.TabIndex = 64;
            this.label12.Text = "工单批号";
            // 
            // tb_AngleResist
            // 
            this.tb_AngleResist.Font = new System.Drawing.Font("宋体", 10.8F);
            this.tb_AngleResist.Location = new System.Drawing.Point(138, 522);
            this.tb_AngleResist.Margin = new System.Windows.Forms.Padding(2);
            this.tb_AngleResist.MaxLength = 15;
            this.tb_AngleResist.Name = "tb_AngleResist";
            this.tb_AngleResist.Size = new System.Drawing.Size(200, 24);
            this.tb_AngleResist.TabIndex = 68;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("宋体", 10.8F);
            this.label13.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label13.Location = new System.Drawing.Point(67, 526);
            this.label13.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(67, 15);
            this.label13.TabIndex = 69;
            this.label13.Text = "复拧角度";
            // 
            // TicketInfoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(417, 667);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.tb_AngleResist);
            this.Controls.Add(this.tb_WoStamp);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.tb_WoBat);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.tb_WoStation);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.tb_WoLine);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.tb_WoFactory);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.bt_picLoad);
            this.Controls.Add(this.tb_ImagePath);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tb_WoArea);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.bt_create);
            this.Controls.Add(this.tb_time);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.tb_note);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tb_WoName);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.tb_WoNum);
            this.Controls.Add(this.label1);
            this.Name = "TicketInfoForm";
            this.Text = "工单信息";
            this.Load += new System.EventHandler(this.TicketInfoForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button bt_create;
        private System.Windows.Forms.TextBox tb_time;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tb_note;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tb_WoName;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox tb_WoNum;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tb_WoArea;
        private System.Windows.Forms.Button bt_picLoad;
        private System.Windows.Forms.TextBox tb_ImagePath;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tb_WoFactory;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tb_WoLine;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tb_WoStation;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox tb_WoStamp;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox tb_WoBat;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox tb_AngleResist;
        private System.Windows.Forms.Label label13;
    }
}