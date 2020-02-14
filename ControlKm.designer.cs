namespace ExpPt1
{
    partial class CtrlMb
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
            this.btnLoad = new System.Windows.Forms.Button();
            this.dgwMb = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgwMb)).BeginInit();
            this.SuspendLayout();
            // 
            // btnLoad
            // 
            this.btnLoad.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnLoad.Location = new System.Drawing.Point(0, 269);
            this.btnLoad.Margin = new System.Windows.Forms.Padding(0);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(257, 23);
            this.btnLoad.TabIndex = 2;
            this.btnLoad.Text = "Load";
            this.btnLoad.UseVisualStyleBackColor = true;
            // 
            // dgwKms
            // 
            this.dgwMb.AllowUserToAddRows = false;
            this.dgwMb.AllowUserToDeleteRows = false;
            this.dgwMb.AllowUserToResizeRows = false;
            this.dgwMb.BackgroundColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.dgwMb.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgwMb.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgwMb.Location = new System.Drawing.Point(0, 0);
            this.dgwMb.Name = "dgwKms";
            this.dgwMb.ReadOnly = true;
            this.dgwMb.RowHeadersVisible = false;
            this.dgwMb.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgwMb.Size = new System.Drawing.Size(257, 292);
            this.dgwMb.TabIndex = 1;
            this.dgwMb.DataSourceChanged += new System.EventHandler(this.DgwKms_DataSourceChanged);
            // 
            // CtrlMb
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.dgwMb);
            this.Name = "CtrlMb";
            this.Size = new System.Drawing.Size(257, 292);
            ((System.ComponentModel.ISupportInitialize)(this.dgwMb)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.DataGridView dgwMb;
    }
}
