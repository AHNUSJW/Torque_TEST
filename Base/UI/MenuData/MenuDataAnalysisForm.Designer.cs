namespace Base.UI.MenuData
{
    partial class MenuDataAnalysisForm
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.comboBox_Point = new System.Windows.Forms.ComboBox();
            this.comboBox_Dev = new System.Windows.Forms.ComboBox();
            this.ucProcessLine1 = new HZH_Controls.Controls.UCProcessLine();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label_torqueDiff = new System.Windows.Forms.Label();
            this.label_slope = new System.Windows.Forms.Label();
            this.label_torqueResidual = new System.Windows.Forms.Label();
            this.label_torqueSeparate = new System.Windows.Forms.Label();
            this.label_anglePeak = new System.Windows.Forms.Label();
            this.label_torquePeak = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.序号 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.formsPlot1 = new ScottPlot.FormsPlot();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btn_secondCursor = new System.Windows.Forms.Button();
            this.btn_save = new System.Windows.Forms.Button();
            this.btn_curveRevert = new System.Windows.Forms.Button();
            this.btn_slopeLine = new System.Windows.Forms.Button();
            this.btn_firstCursor = new System.Windows.Forms.Button();
            this.btn_dealData = new System.Windows.Forms.Button();
            this.btn_import = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.36072F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 66.63927F));
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel2, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.splitContainer1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.panel3, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 17.17557F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 72.90076F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1392, 786);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.comboBox_Point);
            this.panel1.Controls.Add(this.comboBox_Dev);
            this.panel1.Controls.Add(this.ucProcessLine1);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 81);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(458, 128);
            this.panel1.TabIndex = 0;
            // 
            // comboBox_Point
            // 
            this.comboBox_Point.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_Point.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.comboBox_Point.FormattingEnabled = true;
            this.comboBox_Point.Location = new System.Drawing.Point(151, 68);
            this.comboBox_Point.Name = "comboBox_Point";
            this.comboBox_Point.Size = new System.Drawing.Size(121, 29);
            this.comboBox_Point.TabIndex = 169;
            this.comboBox_Point.Visible = false;
            this.comboBox_Point.SelectedIndexChanged += new System.EventHandler(this.comboBox_Point_SelectedIndexChanged);
            // 
            // comboBox_Dev
            // 
            this.comboBox_Dev.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_Dev.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.comboBox_Dev.FormattingEnabled = true;
            this.comboBox_Dev.Location = new System.Drawing.Point(151, 24);
            this.comboBox_Dev.Name = "comboBox_Dev";
            this.comboBox_Dev.Size = new System.Drawing.Size(121, 29);
            this.comboBox_Dev.TabIndex = 168;
            this.comboBox_Dev.SelectedIndexChanged += new System.EventHandler(this.comboBox_Dev_SelectedIndexChanged);
            // 
            // ucProcessLine1
            // 
            this.ucProcessLine1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(228)))), ((int)(((byte)(231)))), ((int)(((byte)(237)))));
            this.ucProcessLine1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.ucProcessLine1.ForeColor = System.Drawing.Color.White;
            this.ucProcessLine1.Location = new System.Drawing.Point(32, 103);
            this.ucProcessLine1.MaxValue = 100;
            this.ucProcessLine1.Name = "ucProcessLine1";
            this.ucProcessLine1.Size = new System.Drawing.Size(200, 15);
            this.ucProcessLine1.TabIndex = 2;
            this.ucProcessLine1.Text = "ucProcessLine1";
            this.ucProcessLine1.Value = 0;
            this.ucProcessLine1.ValueBGColor = System.Drawing.Color.FromArgb(((int)(((byte)(228)))), ((int)(((byte)(231)))), ((int)(((byte)(237)))));
            this.ucProcessLine1.ValueColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(59)))));
            this.ucProcessLine1.ValueTextType = HZH_Controls.Controls.ValueTextType.Percent;
            this.ucProcessLine1.Visible = false;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("微软雅黑", 14F);
            this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label2.Location = new System.Drawing.Point(28, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(107, 25);
            this.label2.TabIndex = 167;
            this.label2.Text = "点位编号：";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 14F);
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(28, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 25);
            this.label1.TabIndex = 166;
            this.label1.Text = "设备编号：";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label_torqueDiff);
            this.panel2.Controls.Add(this.label_slope);
            this.panel2.Controls.Add(this.label_torqueResidual);
            this.panel2.Controls.Add(this.label_torqueSeparate);
            this.panel2.Controls.Add(this.label_anglePeak);
            this.panel2.Controls.Add(this.label_torquePeak);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(467, 81);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(922, 128);
            this.panel2.TabIndex = 1;
            // 
            // label_torqueDiff
            // 
            this.label_torqueDiff.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_torqueDiff.AutoSize = true;
            this.label_torqueDiff.Font = new System.Drawing.Font("微软雅黑", 14F);
            this.label_torqueDiff.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label_torqueDiff.Location = new System.Drawing.Point(805, 11);
            this.label_torqueDiff.Name = "label_torqueDiff";
            this.label_torqueDiff.Size = new System.Drawing.Size(107, 25);
            this.label_torqueDiff.TabIndex = 187;
            this.label_torqueDiff.Text = "扭矩差值：";
            // 
            // label_slope
            // 
            this.label_slope.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_slope.AutoSize = true;
            this.label_slope.Font = new System.Drawing.Font("微软雅黑", 14F);
            this.label_slope.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label_slope.Location = new System.Drawing.Point(649, 11);
            this.label_slope.Name = "label_slope";
            this.label_slope.Size = new System.Drawing.Size(107, 25);
            this.label_slope.TabIndex = 186;
            this.label_slope.Text = "切线斜率：";
            // 
            // label_torqueResidual
            // 
            this.label_torqueResidual.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_torqueResidual.AutoSize = true;
            this.label_torqueResidual.Font = new System.Drawing.Font("微软雅黑", 14F);
            this.label_torqueResidual.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label_torqueResidual.Location = new System.Drawing.Point(493, 11);
            this.label_torqueResidual.Name = "label_torqueResidual";
            this.label_torqueResidual.Size = new System.Drawing.Size(107, 25);
            this.label_torqueResidual.TabIndex = 170;
            this.label_torqueResidual.Text = "残余扭矩：";
            // 
            // label_torqueSeparate
            // 
            this.label_torqueSeparate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_torqueSeparate.AutoSize = true;
            this.label_torqueSeparate.Font = new System.Drawing.Font("微软雅黑", 14F);
            this.label_torqueSeparate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label_torqueSeparate.Location = new System.Drawing.Point(337, 11);
            this.label_torqueSeparate.Name = "label_torqueSeparate";
            this.label_torqueSeparate.Size = new System.Drawing.Size(107, 25);
            this.label_torqueSeparate.TabIndex = 169;
            this.label_torqueSeparate.Text = "分离扭矩：";
            // 
            // label_anglePeak
            // 
            this.label_anglePeak.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_anglePeak.AutoSize = true;
            this.label_anglePeak.Font = new System.Drawing.Font("微软雅黑", 14F);
            this.label_anglePeak.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label_anglePeak.Location = new System.Drawing.Point(181, 11);
            this.label_anglePeak.Name = "label_anglePeak";
            this.label_anglePeak.Size = new System.Drawing.Size(107, 25);
            this.label_anglePeak.TabIndex = 168;
            this.label_anglePeak.Text = "峰值角度：";
            // 
            // label_torquePeak
            // 
            this.label_torquePeak.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_torquePeak.AutoSize = true;
            this.label_torquePeak.Font = new System.Drawing.Font("微软雅黑", 14F);
            this.label_torquePeak.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label_torquePeak.Location = new System.Drawing.Point(25, 11);
            this.label_torquePeak.Name = "label_torquePeak";
            this.label_torquePeak.Size = new System.Drawing.Size(107, 25);
            this.label_torquePeak.TabIndex = 167;
            this.label_torquePeak.Text = "峰值扭矩：";
            // 
            // splitContainer1
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.splitContainer1, 2);
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 215);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.dataGridView1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.formsPlot1);
            this.splitContainer1.Size = new System.Drawing.Size(1386, 568);
            this.splitContainer1.SplitterDistance = 458;
            this.splitContainer1.TabIndex = 2;
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.序号,
            this.Column1});
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.Size = new System.Drawing.Size(458, 568);
            this.dataGridView1.TabIndex = 1;
            this.dataGridView1.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView1_CellMouseDoubleClick);
            // 
            // 序号
            // 
            this.序号.HeaderText = "序号";
            this.序号.Name = "序号";
            // 
            // Column1
            // 
            this.Column1.HeaderText = "数据";
            this.Column1.Name = "Column1";
            // 
            // formsPlot1
            // 
            this.formsPlot1.BackColor = System.Drawing.Color.Transparent;
            this.formsPlot1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.formsPlot1.Location = new System.Drawing.Point(0, 0);
            this.formsPlot1.Name = "formsPlot1";
            this.formsPlot1.Size = new System.Drawing.Size(924, 568);
            this.formsPlot1.TabIndex = 2;
            this.formsPlot1.Visible = false;
            this.formsPlot1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.formsPlot1_MouseDown);
            // 
            // panel3
            // 
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tableLayoutPanel1.SetColumnSpan(this.panel3, 2);
            this.panel3.Controls.Add(this.btn_secondCursor);
            this.panel3.Controls.Add(this.btn_save);
            this.panel3.Controls.Add(this.btn_curveRevert);
            this.panel3.Controls.Add(this.btn_slopeLine);
            this.panel3.Controls.Add(this.btn_firstCursor);
            this.panel3.Controls.Add(this.btn_dealData);
            this.panel3.Controls.Add(this.btn_import);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(3, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(1386, 72);
            this.panel3.TabIndex = 3;
            // 
            // btn_secondCursor
            // 
            this.btn_secondCursor.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_secondCursor.Location = new System.Drawing.Point(454, 8);
            this.btn_secondCursor.Name = "btn_secondCursor";
            this.btn_secondCursor.Size = new System.Drawing.Size(101, 50);
            this.btn_secondCursor.TabIndex = 15;
            this.btn_secondCursor.Text = "第二光标";
            this.btn_secondCursor.UseVisualStyleBackColor = false;
            this.btn_secondCursor.Click += new System.EventHandler(this.btn_secondCursor_Click);
            // 
            // btn_save
            // 
            this.btn_save.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_save.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_save.Location = new System.Drawing.Point(1227, 9);
            this.btn_save.Name = "btn_save";
            this.btn_save.Size = new System.Drawing.Size(101, 50);
            this.btn_save.TabIndex = 13;
            this.btn_save.Text = "数据另存";
            this.btn_save.UseVisualStyleBackColor = false;
            this.btn_save.Click += new System.EventHandler(this.btn_save_Click);
            // 
            // btn_curveRevert
            // 
            this.btn_curveRevert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_curveRevert.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_curveRevert.Location = new System.Drawing.Point(1090, 8);
            this.btn_curveRevert.Name = "btn_curveRevert";
            this.btn_curveRevert.Size = new System.Drawing.Size(101, 50);
            this.btn_curveRevert.TabIndex = 10;
            this.btn_curveRevert.Text = "曲线还原";
            this.btn_curveRevert.UseVisualStyleBackColor = false;
            this.btn_curveRevert.Click += new System.EventHandler(this.btn_curveRevert_Click);
            // 
            // btn_slopeLine
            // 
            this.btn_slopeLine.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_slopeLine.Location = new System.Drawing.Point(584, 8);
            this.btn_slopeLine.Name = "btn_slopeLine";
            this.btn_slopeLine.Size = new System.Drawing.Size(101, 50);
            this.btn_slopeLine.TabIndex = 9;
            this.btn_slopeLine.Text = "切线生成";
            this.btn_slopeLine.UseVisualStyleBackColor = false;
            this.btn_slopeLine.Click += new System.EventHandler(this.btn_slopeLine_Click);
            // 
            // btn_firstCursor
            // 
            this.btn_firstCursor.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_firstCursor.Location = new System.Drawing.Point(324, 9);
            this.btn_firstCursor.Name = "btn_firstCursor";
            this.btn_firstCursor.Size = new System.Drawing.Size(101, 50);
            this.btn_firstCursor.TabIndex = 8;
            this.btn_firstCursor.Text = "第一光标";
            this.btn_firstCursor.UseVisualStyleBackColor = false;
            this.btn_firstCursor.Click += new System.EventHandler(this.btn_firstCursor_Click);
            // 
            // btn_dealData
            // 
            this.btn_dealData.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_dealData.Location = new System.Drawing.Point(181, 8);
            this.btn_dealData.Name = "btn_dealData";
            this.btn_dealData.Size = new System.Drawing.Size(101, 50);
            this.btn_dealData.TabIndex = 7;
            this.btn_dealData.Text = "数据预处理";
            this.btn_dealData.UseVisualStyleBackColor = false;
            this.btn_dealData.Click += new System.EventHandler(this.btn_dealData_Click);
            // 
            // btn_import
            // 
            this.btn_import.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_import.Location = new System.Drawing.Point(32, 9);
            this.btn_import.Name = "btn_import";
            this.btn_import.Size = new System.Drawing.Size(101, 50);
            this.btn_import.TabIndex = 6;
            this.btn_import.Text = "数据导入";
            this.btn_import.UseVisualStyleBackColor = false;
            this.btn_import.Click += new System.EventHandler(this.btn_import_Click);
            // 
            // MenuDataAnalysisForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1392, 786);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "MenuDataAnalysisForm";
            this.Text = "数据分析";
            this.Load += new System.EventHandler(this.MenuDataAnalysisForm_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.panel3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label_torqueResidual;
        private System.Windows.Forms.Label label_torqueSeparate;
        private System.Windows.Forms.Label label_anglePeak;
        private System.Windows.Forms.Label label_torquePeak;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button btn_import;
        private System.Windows.Forms.Button btn_save;
        private System.Windows.Forms.Button btn_curveRevert;
        private System.Windows.Forms.Button btn_slopeLine;
        private System.Windows.Forms.Button btn_firstCursor;
        private System.Windows.Forms.Button btn_dealData;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn 序号;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private ScottPlot.FormsPlot formsPlot1;
        private HZH_Controls.Controls.UCProcessLine ucProcessLine1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label label_slope;
        private System.Windows.Forms.Button btn_secondCursor;
        private System.Windows.Forms.ComboBox comboBox_Point;
        private System.Windows.Forms.ComboBox comboBox_Dev;
        private System.Windows.Forms.Label label_torqueDiff;
    }
}