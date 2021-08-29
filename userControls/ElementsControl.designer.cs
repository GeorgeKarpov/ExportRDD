namespace ExpRddApp
{
    partial class ElCntrl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.btnLoad = new System.Windows.Forms.Button();
            this.dgwMb = new System.Windows.Forms.DataGridView();
            this.lblInfo = new System.Windows.Forms.Label();
            this.chckBoxHide = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblCount = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgwMb)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnLoad
            // 
            this.btnLoad.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnLoad.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnLoad.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLoad.Location = new System.Drawing.Point(0, 225);
            this.btnLoad.Margin = new System.Windows.Forms.Padding(0);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(288, 25);
            this.btnLoad.TabIndex = 2;
            this.btnLoad.Text = "Load";
            this.btnLoad.UseVisualStyleBackColor = true;
            // 
            // dgwMb
            // 
            this.dgwMb.AllowUserToAddRows = false;
            this.dgwMb.AllowUserToDeleteRows = false;
            this.dgwMb.AllowUserToResizeRows = false;
            this.dgwMb.BackgroundColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.dgwMb.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(186)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.ControlDark;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgwMb.DefaultCellStyle = dataGridViewCellStyle1;
            this.dgwMb.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgwMb.Location = new System.Drawing.Point(0, 0);
            this.dgwMb.Name = "dgwMb";
            this.dgwMb.ReadOnly = true;
            this.dgwMb.RowHeadersVisible = false;
            this.dgwMb.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgwMb.Size = new System.Drawing.Size(288, 198);
            this.dgwMb.TabIndex = 1;
            this.dgwMb.DataSourceChanged += new System.EventHandler(this.DgwKms_DataSourceChanged);
            // 
            // lblInfo
            // 
            this.lblInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(186)));
            this.lblInfo.Location = new System.Drawing.Point(3, 2);
            this.lblInfo.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblInfo.Size = new System.Drawing.Size(55, 23);
            this.lblInfo.TabIndex = 3;
            this.lblInfo.Text = "Count:";
            this.lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // chckBoxHide
            // 
            this.chckBoxHide.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.chckBoxHide.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.chckBoxHide.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.chckBoxHide.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chckBoxHide.Location = new System.Drawing.Point(154, 6);
            this.chckBoxHide.Name = "chckBoxHide";
            this.chckBoxHide.Size = new System.Drawing.Size(134, 17);
            this.chckBoxHide.TabIndex = 4;
            this.chckBoxHide.Text = "Hide non exported data";
            this.chckBoxHide.UseVisualStyleBackColor = true;
            this.chckBoxHide.CheckedChanged += new System.EventHandler(this.ChckBoxHide_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblCount);
            this.panel1.Controls.Add(this.lblInfo);
            this.panel1.Controls.Add(this.chckBoxHide);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 198);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(288, 27);
            this.panel1.TabIndex = 5;
            // 
            // lblCount
            // 
            this.lblCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(186)));
            this.lblCount.Location = new System.Drawing.Point(50, 2);
            this.lblCount.Margin = new System.Windows.Forms.Padding(0);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(43, 23);
            this.lblCount.TabIndex = 5;
            this.lblCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ElCntrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dgwMb);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.btnLoad);
            this.Name = "ElCntrl";
            this.Size = new System.Drawing.Size(288, 250);
            ((System.ComponentModel.ISupportInitialize)(this.dgwMb)).EndInit();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.DataGridView dgwMb;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.CheckBox chckBoxHide;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblCount;
    }
}
