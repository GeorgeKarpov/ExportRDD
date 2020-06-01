namespace ExpPt1
{
    partial class ErrCntrl
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
            this.components = new System.ComponentModel.Container();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.ListViewErr = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Message = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // ListViewErr
            // 
            this.ListViewErr.AutoArrange = false;
            this.ListViewErr.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.Message});
            this.ListViewErr.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ListViewErr.FullRowSelect = true;
            this.ListViewErr.GridLines = true;
            this.ListViewErr.LargeImageList = this.imageList1;
            this.ListViewErr.Location = new System.Drawing.Point(0, 0);
            this.ListViewErr.MultiSelect = false;
            this.ListViewErr.Name = "ListViewErr";
            this.ListViewErr.Size = new System.Drawing.Size(586, 366);
            this.ListViewErr.SmallImageList = this.imageList1;
            this.ListViewErr.TabIndex = 1;
            this.ListViewErr.UseCompatibleStateImageBehavior = false;
            this.ListViewErr.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Time";
            this.columnHeader1.Width = 91;
            // 
            // Message
            // 
            this.Message.Text = "Message";
            this.Message.Width = 525;
            // 
            // ErrCntrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ListViewErr);
            this.Name = "ErrCntrl";
            this.Size = new System.Drawing.Size(586, 366);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ImageList imageList1;
        internal System.Windows.Forms.ListView ListViewErr;
        internal System.Windows.Forms.ColumnHeader Message;
        private System.Windows.Forms.ColumnHeader columnHeader1;
    }
}
