namespace Base.UI.MenuHomework
{
    partial class SearchWrenchInfoForm
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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.textBoxEx1 = new HZH_Controls.Controls.TextBoxEx();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.复制ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btn_Search = new System.Windows.Forms.Button();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.BackgroundColor = System.Drawing.SystemColors.ControlLight;
            this.dataGridView1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2,
            this.Column3});
            this.dataGridView1.GridColor = System.Drawing.SystemColors.ControlLight;
            this.dataGridView1.Location = new System.Drawing.Point(10, 57);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.Size = new System.Drawing.Size(593, 557);
            this.dataGridView1.TabIndex = 16;
            this.dataGridView1.TabStop = false;
            this.dataGridView1.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellDoubleClick);
            // 
            // textBoxEx1
            // 
            this.textBoxEx1.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxEx1.DecLength = 2;
            this.textBoxEx1.Font = new System.Drawing.Font("微软雅黑", 12.75F);
            this.textBoxEx1.ForeColor = System.Drawing.Color.Black;
            this.textBoxEx1.InputType = HZH_Controls.TextInputType.Regex;
            this.textBoxEx1.Location = new System.Drawing.Point(10, 13);
            this.textBoxEx1.MaxLength = 3;
            this.textBoxEx1.MaxValue = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.textBoxEx1.MinValue = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.textBoxEx1.Multiline = true;
            this.textBoxEx1.MyRectangle = new System.Drawing.Rectangle(0, 0, 0, 0);
            this.textBoxEx1.Name = "textBoxEx1";
            this.textBoxEx1.OldText = null;
            this.textBoxEx1.PromptColor = System.Drawing.Color.Gray;
            this.textBoxEx1.PromptFont = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.textBoxEx1.PromptText = "";
            this.textBoxEx1.RegexPattern = "^([1-9]|[1-9]\\d|1\\d\\d|2[0-4]\\d|25[0-5])$";
            this.textBoxEx1.Size = new System.Drawing.Size(466, 30);
            this.textBoxEx1.TabIndex = 17;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.复制ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(137, 26);
            // 
            // 复制ToolStripMenuItem
            // 
            this.复制ToolStripMenuItem.Name = "复制ToolStripMenuItem";
            this.复制ToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.复制ToolStripMenuItem.Text = "复制工单号";
            // 
            // btn_Search
            // 
            this.btn_Search.BackColor = System.Drawing.Color.CadetBlue;
            this.btn_Search.Font = new System.Drawing.Font("Arial Narrow", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Search.ForeColor = System.Drawing.Color.Honeydew;
            this.btn_Search.Location = new System.Drawing.Point(504, 8);
            this.btn_Search.Name = "btn_Search";
            this.btn_Search.Size = new System.Drawing.Size(99, 41);
            this.btn_Search.TabIndex = 18;
            this.btn_Search.Text = "搜索";
            this.btn_Search.UseVisualStyleBackColor = false;
            this.btn_Search.Click += new System.EventHandler(this.btn_Search_Click);
            // 
            // Column1
            // 
            this.Column1.HeaderText = "扳手地址";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            // 
            // Column2
            // 
            this.Column2.HeaderText = "扳手量程";
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            // 
            // Column3
            // 
            this.Column3.HeaderText = "扳手唯一序列号";
            this.Column3.Name = "Column3";
            this.Column3.ReadOnly = true;
            // 
            // SearchWrenchInfoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(615, 620);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.textBoxEx1);
            this.Controls.Add(this.btn_Search);
            this.Name = "SearchWrenchInfoForm";
            this.Text = "查找扳手方案";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SearchWrenchInfoForm_FormClosing);
            this.Load += new System.EventHandler(this.SearchWrenchInfoForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private HZH_Controls.Controls.TextBoxEx textBoxEx1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 复制ToolStripMenuItem;
        private System.Windows.Forms.Button btn_Search;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
    }
}