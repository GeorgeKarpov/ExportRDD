namespace Refact
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
            this.btnLoad = new System.Windows.Forms.Button();
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
            this.ListViewErr.Size = new System.Drawing.Size(653, 391);
            this.ListViewErr.SmallImageList = this.imageList1;
            this.ListViewErr.TabIndex = 1;
            this.ListViewErr.UseCompatibleStateImageBehavior = false;
            this.ListViewErr.View = System.Windows.Forms.View.Details;
            this.ListViewErr.SizeChanged += new System.EventHandler(this.ListViewErr_SizeChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Date";
            this.columnHeader1.Width = 91;
            // 
            // Message
            // 
            this.Message.Text = "Message";
            this.Message.Width = 542;
            // 
            // btnLoad
            // 
            this.btnLoad.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnLoad.Location = new System.Drawing.Point(0, 391);
            this.btnLoad.Margin = new System.Windows.Forms.Padding(0);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(653, 23);
            this.btnLoad.TabIndex = 3;
            this.btnLoad.Text = "Load";
            this.btnLoad.UseVisualStyleBackColor = true;
            // 
            // ErrCntrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ListViewErr);
            this.Controls.Add(this.btnLoad);
            this.Name = "ErrCntrl";
            this.Size = new System.Drawing.Size(653, 414);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ImageList imageList1;
        internal System.Windows.Forms.ListView ListViewErr;
        internal System.Windows.Forms.ColumnHeader Message;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Button btnLoad;
    }
}
