using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ExpPt1
{
    partial class FrmStation
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.txtBoxStationId = new System.Windows.Forms.TextBox();
            this.lblStationId = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.lblLine = new System.Windows.Forms.Label();
            this.lblStationName = new System.Windows.Forms.Label();
            this.txtBoxStationName = new System.Windows.Forms.TextBox();
            this.lblxlsDetLock = new System.Windows.Forms.Label();
            this.btnDetLock = new System.Windows.Forms.Button();
            this.lblxlsFP = new System.Windows.Forms.Label();
            this.btnFP = new System.Windows.Forms.Button();
            this.lblxlsEmSg = new System.Windows.Forms.Label();
            this.btnEmSg = new System.Windows.Forms.Button();
            this.lblxlsSpProf = new System.Windows.Forms.Label();
            this.btnSpProf = new System.Windows.Forms.Button();
            this.lblxlsRoutes = new System.Windows.Forms.Label();
            this.btnRoutes = new System.Windows.Forms.Button();
            this.lblxlsCmpRoutes = new System.Windows.Forms.Label();
            this.btnCmpRoutes = new System.Windows.Forms.Button();
            this.lblxlsLxs = new System.Windows.Forms.Label();
            this.btnLXs = new System.Windows.Forms.Button();
            this.lblxlsBgs = new System.Windows.Forms.Label();
            this.BtnBgs = new System.Windows.Forms.Button();
            this.dgwLines = new System.Windows.Forms.DataGridView();
            this.Line = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Color = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.From = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.To = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Direction = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox13 = new System.Windows.Forms.GroupBox();
            this.checkBoxRts = new System.Windows.Forms.CheckBox();
            this.groupBox12 = new System.Windows.Forms.GroupBox();
            this.checkBoxLX = new System.Windows.Forms.CheckBox();
            this.groupBox11 = new System.Windows.Forms.GroupBox();
            this.checkBoxBG = new System.Windows.Forms.CheckBox();
            this.groupBox10 = new System.Windows.Forms.GroupBox();
            this.btnSigClos = new System.Windows.Forms.Button();
            this.lblxlsSigClos = new System.Windows.Forms.Label();
            this.checkBoxSC = new System.Windows.Forms.CheckBox();
            this.groupBox9 = new System.Windows.Forms.GroupBox();
            this.checkBoxSpProf = new System.Windows.Forms.CheckBox();
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.checkBoxEmSt = new System.Windows.Forms.CheckBox();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.checkBoxFP = new System.Windows.Forms.CheckBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.checkBoxDL = new System.Windows.Forms.CheckBox();
            this.checkBoxSCN = new System.Windows.Forms.CheckBox();
            this.lblxlsSigClosN = new System.Windows.Forms.Label();
            this.btnSigClosN = new System.Windows.Forms.Button();
            this.checkBoxBGN = new System.Windows.Forms.CheckBox();
            this.lblxlsBgsN = new System.Windows.Forms.Label();
            this.BtnBgsN = new System.Windows.Forms.Button();
            this.checkBoxCmRts = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lblDocId = new System.Windows.Forms.Label();
            this.txtBoxDocId = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtBoxVersion = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.lblxlsRdd = new System.Windows.Forms.Label();
            this.btnRdd = new System.Windows.Forms.Button();
            this.checkBoxRdd = new System.Windows.Forms.CheckBox();
            this.cmbLines = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.lblAcSections = new System.Windows.Forms.Label();
            this.checkBoxAc = new System.Windows.Forms.CheckBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.checkBoxLevel = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBoxR5 = new System.Windows.Forms.CheckBox();
            this.lblxlsOrderRdd = new System.Windows.Forms.Label();
            this.btnOrderRdd = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgwLines)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox13.SuspendLayout();
            this.groupBox12.SuspendLayout();
            this.groupBox11.SuspendLayout();
            this.groupBox10.SuspendLayout();
            this.groupBox9.SuspendLayout();
            this.groupBox8.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtBoxStationId
            // 
            this.txtBoxStationId.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtBoxStationId.Location = new System.Drawing.Point(349, 46);
            this.txtBoxStationId.Margin = new System.Windows.Forms.Padding(2);
            this.txtBoxStationId.MaxLength = 2;
            this.txtBoxStationId.Name = "txtBoxStationId";
            this.txtBoxStationId.Size = new System.Drawing.Size(93, 23);
            this.txtBoxStationId.TabIndex = 0;
            // 
            // lblStationId
            // 
            this.lblStationId.AutoSize = true;
            this.lblStationId.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblStationId.Location = new System.Drawing.Point(4, 46);
            this.lblStationId.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblStationId.Name = "lblStationId";
            this.lblStationId.Size = new System.Drawing.Size(82, 17);
            this.lblStationId.TabIndex = 1;
            this.lblStationId.Text = "Station Id:";
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(776, 464);
            this.btnOK.Margin = new System.Windows.Forms.Padding(2);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(90, 27);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // lblLine
            // 
            this.lblLine.AutoSize = true;
            this.lblLine.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblLine.Location = new System.Drawing.Point(4, 125);
            this.lblLine.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblLine.Name = "lblLine";
            this.lblLine.Size = new System.Drawing.Size(52, 17);
            this.lblLine.TabIndex = 4;
            this.lblLine.Text = "Lines:";
            // 
            // lblStationName
            // 
            this.lblStationName.AutoSize = true;
            this.lblStationName.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblStationName.Location = new System.Drawing.Point(4, 72);
            this.lblStationName.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblStationName.Name = "lblStationName";
            this.lblStationName.Size = new System.Drawing.Size(110, 17);
            this.lblStationName.TabIndex = 12;
            this.lblStationName.Text = "Station Name:";
            // 
            // txtBoxStationName
            // 
            this.txtBoxStationName.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtBoxStationName.Location = new System.Drawing.Point(349, 73);
            this.txtBoxStationName.Margin = new System.Windows.Forms.Padding(2);
            this.txtBoxStationName.MaxLength = 256;
            this.txtBoxStationName.Name = "txtBoxStationName";
            this.txtBoxStationName.Size = new System.Drawing.Size(93, 23);
            this.txtBoxStationName.TabIndex = 11;
            // 
            // lblxlsDetLock
            // 
            this.lblxlsDetLock.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblxlsDetLock.Location = new System.Drawing.Point(95, 13);
            this.lblxlsDetLock.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblxlsDetLock.Name = "lblxlsDetLock";
            this.lblxlsDetLock.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblxlsDetLock.Size = new System.Drawing.Size(260, 22);
            this.lblxlsDetLock.TabIndex = 18;
            this.lblxlsDetLock.Tag = "Detector Locking";
            this.lblxlsDetLock.Text = "Detector Locking";
            this.lblxlsDetLock.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnDetLock
            // 
            this.btnDetLock.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDetLock.Location = new System.Drawing.Point(24, 13);
            this.btnDetLock.Margin = new System.Windows.Forms.Padding(2);
            this.btnDetLock.Name = "btnDetLock";
            this.btnDetLock.Size = new System.Drawing.Size(66, 22);
            this.btnDetLock.TabIndex = 17;
            this.btnDetLock.Text = "Browse";
            this.btnDetLock.UseVisualStyleBackColor = true;
            this.btnDetLock.Click += new System.EventHandler(this.BtnDetLock_Click);
            // 
            // lblxlsFP
            // 
            this.lblxlsFP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblxlsFP.Location = new System.Drawing.Point(95, 13);
            this.lblxlsFP.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblxlsFP.Name = "lblxlsFP";
            this.lblxlsFP.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblxlsFP.Size = new System.Drawing.Size(260, 22);
            this.lblxlsFP.TabIndex = 20;
            this.lblxlsFP.Tag = "Flank Protection";
            this.lblxlsFP.Text = "Flank Protection";
            this.lblxlsFP.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnFP
            // 
            this.btnFP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFP.Location = new System.Drawing.Point(24, 13);
            this.btnFP.Margin = new System.Windows.Forms.Padding(2);
            this.btnFP.Name = "btnFP";
            this.btnFP.Size = new System.Drawing.Size(66, 22);
            this.btnFP.TabIndex = 19;
            this.btnFP.Text = "Browse";
            this.btnFP.UseVisualStyleBackColor = true;
            this.btnFP.Click += new System.EventHandler(this.BtnFP_Click);
            // 
            // lblxlsEmSg
            // 
            this.lblxlsEmSg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblxlsEmSg.Location = new System.Drawing.Point(95, 13);
            this.lblxlsEmSg.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblxlsEmSg.Name = "lblxlsEmSg";
            this.lblxlsEmSg.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblxlsEmSg.Size = new System.Drawing.Size(260, 22);
            this.lblxlsEmSg.TabIndex = 22;
            this.lblxlsEmSg.Tag = "Emergency Stops";
            this.lblxlsEmSg.Text = "Emergency Stops";
            this.lblxlsEmSg.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnEmSg
            // 
            this.btnEmSg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEmSg.Location = new System.Drawing.Point(24, 13);
            this.btnEmSg.Margin = new System.Windows.Forms.Padding(2);
            this.btnEmSg.Name = "btnEmSg";
            this.btnEmSg.Size = new System.Drawing.Size(66, 22);
            this.btnEmSg.TabIndex = 21;
            this.btnEmSg.Text = "Browse";
            this.btnEmSg.UseVisualStyleBackColor = true;
            this.btnEmSg.Click += new System.EventHandler(this.BtnEmSg_Click);
            // 
            // lblxlsSpProf
            // 
            this.lblxlsSpProf.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblxlsSpProf.Location = new System.Drawing.Point(95, 13);
            this.lblxlsSpProf.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblxlsSpProf.Name = "lblxlsSpProf";
            this.lblxlsSpProf.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblxlsSpProf.Size = new System.Drawing.Size(260, 22);
            this.lblxlsSpProf.TabIndex = 24;
            this.lblxlsSpProf.Tag = "Speed Profiles";
            this.lblxlsSpProf.Text = "Speed Profiles";
            this.lblxlsSpProf.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnSpProf
            // 
            this.btnSpProf.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSpProf.Location = new System.Drawing.Point(24, 13);
            this.btnSpProf.Margin = new System.Windows.Forms.Padding(2);
            this.btnSpProf.Name = "btnSpProf";
            this.btnSpProf.Size = new System.Drawing.Size(66, 22);
            this.btnSpProf.TabIndex = 23;
            this.btnSpProf.Text = "Browse";
            this.btnSpProf.UseVisualStyleBackColor = true;
            this.btnSpProf.Click += new System.EventHandler(this.BtnSpProf_Click);
            // 
            // lblxlsRoutes
            // 
            this.lblxlsRoutes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblxlsRoutes.Location = new System.Drawing.Point(95, 13);
            this.lblxlsRoutes.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblxlsRoutes.Name = "lblxlsRoutes";
            this.lblxlsRoutes.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblxlsRoutes.Size = new System.Drawing.Size(260, 22);
            this.lblxlsRoutes.TabIndex = 26;
            this.lblxlsRoutes.Tag = "Routes";
            this.lblxlsRoutes.Text = "Routes";
            this.lblxlsRoutes.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnRoutes
            // 
            this.btnRoutes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRoutes.Location = new System.Drawing.Point(24, 13);
            this.btnRoutes.Margin = new System.Windows.Forms.Padding(2);
            this.btnRoutes.Name = "btnRoutes";
            this.btnRoutes.Size = new System.Drawing.Size(66, 22);
            this.btnRoutes.TabIndex = 25;
            this.btnRoutes.Text = "Browse";
            this.btnRoutes.UseVisualStyleBackColor = true;
            this.btnRoutes.Click += new System.EventHandler(this.BtnRoutes_Click);
            // 
            // lblxlsCmpRoutes
            // 
            this.lblxlsCmpRoutes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblxlsCmpRoutes.Location = new System.Drawing.Point(100, 351);
            this.lblxlsCmpRoutes.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblxlsCmpRoutes.Name = "lblxlsCmpRoutes";
            this.lblxlsCmpRoutes.Size = new System.Drawing.Size(258, 22);
            this.lblxlsCmpRoutes.TabIndex = 28;
            this.lblxlsCmpRoutes.Text = "Compound Routes";
            this.lblxlsCmpRoutes.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblxlsCmpRoutes.Visible = false;
            // 
            // btnCmpRoutes
            // 
            this.btnCmpRoutes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCmpRoutes.Location = new System.Drawing.Point(28, 351);
            this.btnCmpRoutes.Margin = new System.Windows.Forms.Padding(2);
            this.btnCmpRoutes.Name = "btnCmpRoutes";
            this.btnCmpRoutes.Size = new System.Drawing.Size(66, 22);
            this.btnCmpRoutes.TabIndex = 27;
            this.btnCmpRoutes.Text = "Browse";
            this.btnCmpRoutes.UseVisualStyleBackColor = true;
            this.btnCmpRoutes.Visible = false;
            this.btnCmpRoutes.Click += new System.EventHandler(this.BtnCmpRoutes_Click);
            // 
            // lblxlsLxs
            // 
            this.lblxlsLxs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblxlsLxs.Location = new System.Drawing.Point(95, 13);
            this.lblxlsLxs.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblxlsLxs.Name = "lblxlsLxs";
            this.lblxlsLxs.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblxlsLxs.Size = new System.Drawing.Size(260, 22);
            this.lblxlsLxs.TabIndex = 30;
            this.lblxlsLxs.Tag = "Level Crossings parameters";
            this.lblxlsLxs.Text = "Level Crossings";
            this.lblxlsLxs.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnLXs
            // 
            this.btnLXs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLXs.Location = new System.Drawing.Point(24, 13);
            this.btnLXs.Margin = new System.Windows.Forms.Padding(2);
            this.btnLXs.Name = "btnLXs";
            this.btnLXs.Size = new System.Drawing.Size(66, 22);
            this.btnLXs.TabIndex = 29;
            this.btnLXs.Text = "Browse";
            this.btnLXs.UseVisualStyleBackColor = true;
            this.btnLXs.Click += new System.EventHandler(this.Button1_Click);
            // 
            // lblxlsBgs
            // 
            this.lblxlsBgs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblxlsBgs.Location = new System.Drawing.Point(95, 13);
            this.lblxlsBgs.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblxlsBgs.Name = "lblxlsBgs";
            this.lblxlsBgs.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblxlsBgs.Size = new System.Drawing.Size(260, 22);
            this.lblxlsBgs.TabIndex = 32;
            this.lblxlsBgs.Tag = "Balise Groups";
            this.lblxlsBgs.Text = "Balise Groups";
            this.lblxlsBgs.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // BtnBgs
            // 
            this.BtnBgs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnBgs.Location = new System.Drawing.Point(24, 13);
            this.BtnBgs.Margin = new System.Windows.Forms.Padding(2);
            this.BtnBgs.Name = "BtnBgs";
            this.BtnBgs.Size = new System.Drawing.Size(66, 22);
            this.BtnBgs.TabIndex = 31;
            this.BtnBgs.Text = "Browse";
            this.BtnBgs.UseVisualStyleBackColor = true;
            this.BtnBgs.Click += new System.EventHandler(this.BtnBgs_Click);
            // 
            // dgwLines
            // 
            this.dgwLines.AllowUserToAddRows = false;
            this.dgwLines.AllowUserToDeleteRows = false;
            this.dgwLines.AllowUserToResizeRows = false;
            this.dgwLines.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(186)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgwLines.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgwLines.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgwLines.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Line,
            this.Color,
            this.From,
            this.To,
            this.Direction});
            this.dgwLines.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgwLines.Enabled = false;
            this.dgwLines.Location = new System.Drawing.Point(8, 144);
            this.dgwLines.Margin = new System.Windows.Forms.Padding(2);
            this.dgwLines.Name = "dgwLines";
            this.dgwLines.ReadOnly = true;
            this.dgwLines.RowHeadersVisible = false;
            this.dgwLines.RowTemplate.Height = 24;
            this.dgwLines.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgwLines.Size = new System.Drawing.Size(436, 115);
            this.dgwLines.TabIndex = 33;
            // 
            // Line
            // 
            this.Line.HeaderText = "Id";
            this.Line.Name = "Line";
            this.Line.ReadOnly = true;
            this.Line.Width = 50;
            // 
            // Color
            // 
            this.Color.HeaderText = "Color";
            this.Color.Name = "Color";
            this.Color.ReadOnly = true;
            this.Color.Width = 80;
            // 
            // From
            // 
            this.From.HeaderText = "Km Start";
            this.From.Name = "From";
            this.From.ReadOnly = true;
            // 
            // To
            // 
            this.To.HeaderText = "Km End";
            this.To.Name = "To";
            this.To.ReadOnly = true;
            // 
            // Direction
            // 
            this.Direction.HeaderText = "Direction";
            this.Direction.Name = "Direction";
            this.Direction.ReadOnly = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.groupBox13);
            this.groupBox1.Controls.Add(this.groupBox12);
            this.groupBox1.Controls.Add(this.groupBox11);
            this.groupBox1.Controls.Add(this.groupBox10);
            this.groupBox1.Controls.Add(this.groupBox9);
            this.groupBox1.Controls.Add(this.groupBox8);
            this.groupBox1.Controls.Add(this.groupBox7);
            this.groupBox1.Controls.Add(this.groupBox6);
            this.groupBox1.Controls.Add(this.checkBoxSCN);
            this.groupBox1.Controls.Add(this.lblxlsSigClosN);
            this.groupBox1.Controls.Add(this.btnSigClosN);
            this.groupBox1.Controls.Add(this.checkBoxBGN);
            this.groupBox1.Controls.Add(this.lblxlsBgsN);
            this.groupBox1.Controls.Add(this.BtnBgsN);
            this.groupBox1.Controls.Add(this.checkBoxCmRts);
            this.groupBox1.Controls.Add(this.lblxlsCmpRoutes);
            this.groupBox1.Controls.Add(this.btnCmpRoutes);
            this.groupBox1.Location = new System.Drawing.Point(464, 10);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(402, 428);
            this.groupBox1.TabIndex = 35;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Input Data";
            // 
            // groupBox13
            // 
            this.groupBox13.Controls.Add(this.btnRoutes);
            this.groupBox13.Controls.Add(this.lblxlsRoutes);
            this.groupBox13.Controls.Add(this.checkBoxRts);
            this.groupBox13.Location = new System.Drawing.Point(9, 193);
            this.groupBox13.Name = "groupBox13";
            this.groupBox13.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.groupBox13.Size = new System.Drawing.Size(388, 41);
            this.groupBox13.TabIndex = 63;
            this.groupBox13.TabStop = false;
            this.groupBox13.Text = "RT";
            // 
            // checkBoxRts
            // 
            this.checkBoxRts.AutoSize = true;
            this.checkBoxRts.Checked = true;
            this.checkBoxRts.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxRts.Location = new System.Drawing.Point(5, 17);
            this.checkBoxRts.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxRts.Name = "checkBoxRts";
            this.checkBoxRts.Size = new System.Drawing.Size(15, 14);
            this.checkBoxRts.TabIndex = 39;
            this.checkBoxRts.UseVisualStyleBackColor = true;
            this.checkBoxRts.CheckedChanged += new System.EventHandler(this.CheckBoxRts_CheckedChanged);
            // 
            // groupBox12
            // 
            this.groupBox12.Controls.Add(this.btnLXs);
            this.groupBox12.Controls.Add(this.lblxlsLxs);
            this.groupBox12.Controls.Add(this.checkBoxLX);
            this.groupBox12.Location = new System.Drawing.Point(9, 238);
            this.groupBox12.Name = "groupBox12";
            this.groupBox12.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.groupBox12.Size = new System.Drawing.Size(388, 41);
            this.groupBox12.TabIndex = 62;
            this.groupBox12.TabStop = false;
            this.groupBox12.Text = "LX parameters";
            // 
            // checkBoxLX
            // 
            this.checkBoxLX.AutoSize = true;
            this.checkBoxLX.Checked = true;
            this.checkBoxLX.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxLX.Location = new System.Drawing.Point(5, 17);
            this.checkBoxLX.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxLX.Name = "checkBoxLX";
            this.checkBoxLX.Size = new System.Drawing.Size(15, 14);
            this.checkBoxLX.TabIndex = 41;
            this.checkBoxLX.UseVisualStyleBackColor = true;
            this.checkBoxLX.CheckedChanged += new System.EventHandler(this.CheckBoxLX_CheckedChanged);
            // 
            // groupBox11
            // 
            this.groupBox11.Controls.Add(this.lblxlsBgs);
            this.groupBox11.Controls.Add(this.BtnBgs);
            this.groupBox11.Controls.Add(this.checkBoxBG);
            this.groupBox11.Location = new System.Drawing.Point(9, 283);
            this.groupBox11.Name = "groupBox11";
            this.groupBox11.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.groupBox11.Size = new System.Drawing.Size(388, 41);
            this.groupBox11.TabIndex = 61;
            this.groupBox11.TabStop = false;
            this.groupBox11.Text = "BG";
            // 
            // checkBoxBG
            // 
            this.checkBoxBG.AutoSize = true;
            this.checkBoxBG.Checked = true;
            this.checkBoxBG.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxBG.Location = new System.Drawing.Point(5, 17);
            this.checkBoxBG.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxBG.Name = "checkBoxBG";
            this.checkBoxBG.Size = new System.Drawing.Size(15, 14);
            this.checkBoxBG.TabIndex = 42;
            this.checkBoxBG.UseVisualStyleBackColor = true;
            this.checkBoxBG.CheckedChanged += new System.EventHandler(this.CheckBoxBG_CheckedChanged);
            // 
            // groupBox10
            // 
            this.groupBox10.Controls.Add(this.btnSigClos);
            this.groupBox10.Controls.Add(this.lblxlsSigClos);
            this.groupBox10.Controls.Add(this.checkBoxSC);
            this.groupBox10.Location = new System.Drawing.Point(9, 328);
            this.groupBox10.Name = "groupBox10";
            this.groupBox10.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.groupBox10.Size = new System.Drawing.Size(388, 41);
            this.groupBox10.TabIndex = 60;
            this.groupBox10.TabStop = false;
            this.groupBox10.Text = "SC";
            // 
            // btnSigClos
            // 
            this.btnSigClos.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSigClos.Location = new System.Drawing.Point(24, 13);
            this.btnSigClos.Margin = new System.Windows.Forms.Padding(2);
            this.btnSigClos.Name = "btnSigClos";
            this.btnSigClos.Size = new System.Drawing.Size(66, 22);
            this.btnSigClos.TabIndex = 50;
            this.btnSigClos.Text = "Browse";
            this.btnSigClos.UseVisualStyleBackColor = true;
            this.btnSigClos.Click += new System.EventHandler(this.BtnSigClos_Click);
            // 
            // lblxlsSigClos
            // 
            this.lblxlsSigClos.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblxlsSigClos.Location = new System.Drawing.Point(95, 13);
            this.lblxlsSigClos.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblxlsSigClos.Name = "lblxlsSigClos";
            this.lblxlsSigClos.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblxlsSigClos.Size = new System.Drawing.Size(260, 22);
            this.lblxlsSigClos.TabIndex = 51;
            this.lblxlsSigClos.Tag = "Signals Closure";
            this.lblxlsSigClos.Text = "Signals Closure";
            this.lblxlsSigClos.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // checkBoxSC
            // 
            this.checkBoxSC.AutoSize = true;
            this.checkBoxSC.Checked = true;
            this.checkBoxSC.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSC.Location = new System.Drawing.Point(5, 17);
            this.checkBoxSC.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxSC.Name = "checkBoxSC";
            this.checkBoxSC.Size = new System.Drawing.Size(15, 14);
            this.checkBoxSC.TabIndex = 52;
            this.checkBoxSC.UseVisualStyleBackColor = true;
            this.checkBoxSC.CheckedChanged += new System.EventHandler(this.CheckBoxSC_CheckedChanged);
            // 
            // groupBox9
            // 
            this.groupBox9.Controls.Add(this.lblxlsSpProf);
            this.groupBox9.Controls.Add(this.btnSpProf);
            this.groupBox9.Controls.Add(this.checkBoxSpProf);
            this.groupBox9.Location = new System.Drawing.Point(9, 148);
            this.groupBox9.Name = "groupBox9";
            this.groupBox9.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.groupBox9.Size = new System.Drawing.Size(388, 41);
            this.groupBox9.TabIndex = 59;
            this.groupBox9.TabStop = false;
            this.groupBox9.Text = "SSP";
            // 
            // checkBoxSpProf
            // 
            this.checkBoxSpProf.AutoSize = true;
            this.checkBoxSpProf.Checked = true;
            this.checkBoxSpProf.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSpProf.Location = new System.Drawing.Point(5, 17);
            this.checkBoxSpProf.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxSpProf.Name = "checkBoxSpProf";
            this.checkBoxSpProf.Size = new System.Drawing.Size(15, 14);
            this.checkBoxSpProf.TabIndex = 38;
            this.checkBoxSpProf.UseVisualStyleBackColor = true;
            this.checkBoxSpProf.CheckedChanged += new System.EventHandler(this.CheckBoxSpProf_CheckedChanged);
            // 
            // groupBox8
            // 
            this.groupBox8.Controls.Add(this.btnEmSg);
            this.groupBox8.Controls.Add(this.lblxlsEmSg);
            this.groupBox8.Controls.Add(this.checkBoxEmSt);
            this.groupBox8.Location = new System.Drawing.Point(9, 103);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.groupBox8.Size = new System.Drawing.Size(388, 41);
            this.groupBox8.TabIndex = 58;
            this.groupBox8.TabStop = false;
            this.groupBox8.Text = "ES";
            // 
            // checkBoxEmSt
            // 
            this.checkBoxEmSt.AutoSize = true;
            this.checkBoxEmSt.Checked = true;
            this.checkBoxEmSt.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxEmSt.Location = new System.Drawing.Point(5, 17);
            this.checkBoxEmSt.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxEmSt.Name = "checkBoxEmSt";
            this.checkBoxEmSt.Size = new System.Drawing.Size(15, 14);
            this.checkBoxEmSt.TabIndex = 37;
            this.checkBoxEmSt.UseVisualStyleBackColor = true;
            this.checkBoxEmSt.CheckedChanged += new System.EventHandler(this.CheckBoxEmSt_CheckedChanged);
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.lblxlsFP);
            this.groupBox7.Controls.Add(this.btnFP);
            this.groupBox7.Controls.Add(this.checkBoxFP);
            this.groupBox7.Location = new System.Drawing.Point(9, 58);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.groupBox7.Size = new System.Drawing.Size(388, 41);
            this.groupBox7.TabIndex = 57;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "FP";
            // 
            // checkBoxFP
            // 
            this.checkBoxFP.AutoSize = true;
            this.checkBoxFP.Checked = true;
            this.checkBoxFP.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxFP.Location = new System.Drawing.Point(5, 17);
            this.checkBoxFP.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxFP.Name = "checkBoxFP";
            this.checkBoxFP.Size = new System.Drawing.Size(15, 14);
            this.checkBoxFP.TabIndex = 36;
            this.checkBoxFP.UseVisualStyleBackColor = true;
            this.checkBoxFP.CheckedChanged += new System.EventHandler(this.CheckBoxFP_CheckedChanged);
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.lblxlsDetLock);
            this.groupBox6.Controls.Add(this.btnDetLock);
            this.groupBox6.Controls.Add(this.checkBoxDL);
            this.groupBox6.Location = new System.Drawing.Point(9, 13);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.groupBox6.Size = new System.Drawing.Size(388, 41);
            this.groupBox6.TabIndex = 56;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "TDL";
            // 
            // checkBoxDL
            // 
            this.checkBoxDL.AutoSize = true;
            this.checkBoxDL.Checked = true;
            this.checkBoxDL.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxDL.Location = new System.Drawing.Point(5, 17);
            this.checkBoxDL.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxDL.Name = "checkBoxDL";
            this.checkBoxDL.Size = new System.Drawing.Size(15, 14);
            this.checkBoxDL.TabIndex = 35;
            this.checkBoxDL.UseVisualStyleBackColor = true;
            this.checkBoxDL.CheckedChanged += new System.EventHandler(this.CheckBoxDL_CheckedChanged);
            // 
            // checkBoxSCN
            // 
            this.checkBoxSCN.AutoSize = true;
            this.checkBoxSCN.Checked = true;
            this.checkBoxSCN.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSCN.Location = new System.Drawing.Point(9, 404);
            this.checkBoxSCN.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxSCN.Name = "checkBoxSCN";
            this.checkBoxSCN.Size = new System.Drawing.Size(15, 14);
            this.checkBoxSCN.TabIndex = 55;
            this.checkBoxSCN.UseVisualStyleBackColor = true;
            this.checkBoxSCN.Visible = false;
            this.checkBoxSCN.CheckedChanged += new System.EventHandler(this.CheckBoxSCN_CheckedChanged);
            // 
            // lblxlsSigClosN
            // 
            this.lblxlsSigClosN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblxlsSigClosN.Location = new System.Drawing.Point(100, 400);
            this.lblxlsSigClosN.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblxlsSigClosN.Name = "lblxlsSigClosN";
            this.lblxlsSigClosN.Size = new System.Drawing.Size(258, 22);
            this.lblxlsSigClosN.TabIndex = 54;
            this.lblxlsSigClosN.Text = "Signals Closure Neighbor";
            this.lblxlsSigClosN.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblxlsSigClosN.Visible = false;
            // 
            // btnSigClosN
            // 
            this.btnSigClosN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSigClosN.Location = new System.Drawing.Point(28, 400);
            this.btnSigClosN.Margin = new System.Windows.Forms.Padding(2);
            this.btnSigClosN.Name = "btnSigClosN";
            this.btnSigClosN.Size = new System.Drawing.Size(66, 22);
            this.btnSigClosN.TabIndex = 53;
            this.btnSigClosN.Text = "Browse";
            this.btnSigClosN.UseVisualStyleBackColor = true;
            this.btnSigClosN.Visible = false;
            this.btnSigClosN.Click += new System.EventHandler(this.BtnSigClosN_Click);
            // 
            // checkBoxBGN
            // 
            this.checkBoxBGN.AutoSize = true;
            this.checkBoxBGN.Checked = true;
            this.checkBoxBGN.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxBGN.Location = new System.Drawing.Point(9, 379);
            this.checkBoxBGN.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxBGN.Name = "checkBoxBGN";
            this.checkBoxBGN.Size = new System.Drawing.Size(15, 14);
            this.checkBoxBGN.TabIndex = 49;
            this.checkBoxBGN.UseVisualStyleBackColor = true;
            this.checkBoxBGN.Visible = false;
            this.checkBoxBGN.CheckedChanged += new System.EventHandler(this.CheckBoxBGN_CheckedChanged);
            // 
            // lblxlsBgsN
            // 
            this.lblxlsBgsN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblxlsBgsN.Location = new System.Drawing.Point(100, 375);
            this.lblxlsBgsN.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblxlsBgsN.Name = "lblxlsBgsN";
            this.lblxlsBgsN.Size = new System.Drawing.Size(258, 22);
            this.lblxlsBgsN.TabIndex = 48;
            this.lblxlsBgsN.Text = "Balise Groups Neighbor";
            this.lblxlsBgsN.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblxlsBgsN.Visible = false;
            // 
            // BtnBgsN
            // 
            this.BtnBgsN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnBgsN.Location = new System.Drawing.Point(28, 375);
            this.BtnBgsN.Margin = new System.Windows.Forms.Padding(2);
            this.BtnBgsN.Name = "BtnBgsN";
            this.BtnBgsN.Size = new System.Drawing.Size(66, 22);
            this.BtnBgsN.TabIndex = 47;
            this.BtnBgsN.Text = "Browse";
            this.BtnBgsN.UseVisualStyleBackColor = true;
            this.BtnBgsN.Visible = false;
            this.BtnBgsN.Click += new System.EventHandler(this.BtnBgsN_Click);
            // 
            // checkBoxCmRts
            // 
            this.checkBoxCmRts.AutoSize = true;
            this.checkBoxCmRts.Checked = true;
            this.checkBoxCmRts.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCmRts.Location = new System.Drawing.Point(9, 355);
            this.checkBoxCmRts.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxCmRts.Name = "checkBoxCmRts";
            this.checkBoxCmRts.Size = new System.Drawing.Size(15, 14);
            this.checkBoxCmRts.TabIndex = 40;
            this.checkBoxCmRts.UseVisualStyleBackColor = true;
            this.checkBoxCmRts.Visible = false;
            this.checkBoxCmRts.CheckedChanged += new System.EventHandler(this.CheckBoxCmRts_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.lblDocId);
            this.groupBox2.Controls.Add(this.txtBoxDocId);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.txtBoxVersion);
            this.groupBox2.Controls.Add(this.lblStationId);
            this.groupBox2.Controls.Add(this.dgwLines);
            this.groupBox2.Controls.Add(this.txtBoxStationId);
            this.groupBox2.Controls.Add(this.lblLine);
            this.groupBox2.Controls.Add(this.txtBoxStationName);
            this.groupBox2.Controls.Add(this.lblStationName);
            this.groupBox2.Location = new System.Drawing.Point(9, 10);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox2.Size = new System.Drawing.Size(451, 263);
            this.groupBox2.TabIndex = 36;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "RDD";
            // 
            // lblDocId
            // 
            this.lblDocId.AutoSize = true;
            this.lblDocId.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblDocId.Location = new System.Drawing.Point(5, 23);
            this.lblDocId.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblDocId.Name = "lblDocId";
            this.lblDocId.Size = new System.Drawing.Size(59, 17);
            this.lblDocId.TabIndex = 37;
            this.lblDocId.Text = "Doc Id:";
            // 
            // txtBoxDocId
            // 
            this.txtBoxDocId.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtBoxDocId.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtBoxDocId.Location = new System.Drawing.Point(260, 17);
            this.txtBoxDocId.Margin = new System.Windows.Forms.Padding(2);
            this.txtBoxDocId.MaxLength = 20;
            this.txtBoxDocId.Name = "txtBoxDocId";
            this.txtBoxDocId.Size = new System.Drawing.Size(182, 23);
            this.txtBoxDocId.TabIndex = 36;
            this.txtBoxDocId.TextChanged += new System.EventHandler(this.TxtBoxDocId_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(4, 98);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 17);
            this.label1.TabIndex = 35;
            this.label1.Text = "Version:";
            // 
            // txtBoxVersion
            // 
            this.txtBoxVersion.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtBoxVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtBoxVersion.Location = new System.Drawing.Point(349, 100);
            this.txtBoxVersion.Margin = new System.Windows.Forms.Padding(2);
            this.txtBoxVersion.MaxLength = 5;
            this.txtBoxVersion.Name = "txtBoxVersion";
            this.txtBoxVersion.Size = new System.Drawing.Size(93, 23);
            this.txtBoxVersion.TabIndex = 34;
            this.txtBoxVersion.TextChanged += new System.EventHandler(this.TxtBoxVersion_TextChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.lblxlsRdd);
            this.groupBox3.Controls.Add(this.btnRdd);
            this.groupBox3.Controls.Add(this.checkBoxRdd);
            this.groupBox3.Location = new System.Drawing.Point(9, 296);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox3.Size = new System.Drawing.Size(451, 62);
            this.groupBox3.TabIndex = 52;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Compare With";
            // 
            // lblxlsRdd
            // 
            this.lblxlsRdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblxlsRdd.Location = new System.Drawing.Point(4, 24);
            this.lblxlsRdd.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblxlsRdd.Name = "lblxlsRdd";
            this.lblxlsRdd.Size = new System.Drawing.Size(320, 22);
            this.lblxlsRdd.TabIndex = 51;
            this.lblxlsRdd.Tag = "Previous Rdd";
            this.lblxlsRdd.Text = "Previous Rdd";
            this.lblxlsRdd.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnRdd
            // 
            this.btnRdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRdd.Location = new System.Drawing.Point(381, 24);
            this.btnRdd.Margin = new System.Windows.Forms.Padding(2);
            this.btnRdd.Name = "btnRdd";
            this.btnRdd.Size = new System.Drawing.Size(66, 22);
            this.btnRdd.TabIndex = 47;
            this.btnRdd.Text = "Browse";
            this.btnRdd.UseVisualStyleBackColor = true;
            this.btnRdd.Click += new System.EventHandler(this.BtnRdd_Click);
            // 
            // checkBoxRdd
            // 
            this.checkBoxRdd.AutoSize = true;
            this.checkBoxRdd.Checked = true;
            this.checkBoxRdd.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxRdd.Location = new System.Drawing.Point(339, 29);
            this.checkBoxRdd.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxRdd.Name = "checkBoxRdd";
            this.checkBoxRdd.Size = new System.Drawing.Size(15, 14);
            this.checkBoxRdd.TabIndex = 49;
            this.checkBoxRdd.UseVisualStyleBackColor = true;
            this.checkBoxRdd.CheckedChanged += new System.EventHandler(this.CheckBoxRdd_CheckedChanged_1);
            // 
            // cmbLines
            // 
            this.cmbLines.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLines.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbLines.FormattingEnabled = true;
            this.cmbLines.Location = new System.Drawing.Point(412, 274);
            this.cmbLines.Name = "cmbLines";
            this.cmbLines.Size = new System.Drawing.Size(48, 21);
            this.cmbLines.TabIndex = 36;
            this.cmbLines.SelectedIndexChanged += new System.EventHandler(this.CmbLines_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.Location = new System.Drawing.Point(148, 273);
            this.label3.Margin = new System.Windows.Forms.Padding(0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(260, 22);
            this.label3.TabIndex = 56;
            this.label3.Text = "Zero level Line ID:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.lblAcSections);
            this.groupBox4.Controls.Add(this.checkBoxAc);
            this.groupBox4.Location = new System.Drawing.Point(9, 443);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(451, 48);
            this.groupBox4.TabIndex = 57;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Options";
            // 
            // lblAcSections
            // 
            this.lblAcSections.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAcSections.Location = new System.Drawing.Point(105, 13);
            this.lblAcSections.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblAcSections.Name = "lblAcSections";
            this.lblAcSections.Size = new System.Drawing.Size(320, 22);
            this.lblAcSections.TabIndex = 52;
            this.lblAcSections.Text = "Calculate Ac section from SL";
            this.lblAcSections.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // checkBoxAc
            // 
            this.checkBoxAc.AutoSize = true;
            this.checkBoxAc.Location = new System.Drawing.Point(429, 18);
            this.checkBoxAc.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxAc.Name = "checkBoxAc";
            this.checkBoxAc.Size = new System.Drawing.Size(15, 14);
            this.checkBoxAc.TabIndex = 50;
            this.checkBoxAc.UseVisualStyleBackColor = true;
            this.checkBoxAc.CheckedChanged += new System.EventHandler(this.CheckBoxAc_CheckedChanged);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.checkBoxLevel);
            this.groupBox5.Controls.Add(this.label2);
            this.groupBox5.Controls.Add(this.checkBoxR5);
            this.groupBox5.Controls.Add(this.lblxlsOrderRdd);
            this.groupBox5.Controls.Add(this.btnOrderRdd);
            this.groupBox5.Location = new System.Drawing.Point(9, 365);
            this.groupBox5.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox5.Size = new System.Drawing.Size(451, 73);
            this.groupBox5.TabIndex = 58;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Order By";
            // 
            // checkBoxLevel
            // 
            this.checkBoxLevel.AutoSize = true;
            this.checkBoxLevel.Location = new System.Drawing.Point(303, 54);
            this.checkBoxLevel.Name = "checkBoxLevel";
            this.checkBoxLevel.Size = new System.Drawing.Size(115, 17);
            this.checkBoxLevel.TabIndex = 54;
            this.checkBoxLevel.Text = "Copy Tracks Level";
            this.checkBoxLevel.UseVisualStyleBackColor = true;
            this.checkBoxLevel.CheckedChanged += new System.EventHandler(this.CheckBoxLevel_CheckedChanged);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.Location = new System.Drawing.Point(5, 50);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(211, 22);
            this.label2.TabIndex = 53;
            this.label2.Text = "R5 Order";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.label2.Visible = false;
            // 
            // checkBoxR5
            // 
            this.checkBoxR5.AutoSize = true;
            this.checkBoxR5.Location = new System.Drawing.Point(221, 55);
            this.checkBoxR5.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxR5.Name = "checkBoxR5";
            this.checkBoxR5.Size = new System.Drawing.Size(15, 14);
            this.checkBoxR5.TabIndex = 52;
            this.checkBoxR5.UseVisualStyleBackColor = true;
            this.checkBoxR5.Visible = false;
            // 
            // lblxlsOrderRdd
            // 
            this.lblxlsOrderRdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblxlsOrderRdd.Location = new System.Drawing.Point(52, 20);
            this.lblxlsOrderRdd.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblxlsOrderRdd.Name = "lblxlsOrderRdd";
            this.lblxlsOrderRdd.Size = new System.Drawing.Size(320, 22);
            this.lblxlsOrderRdd.TabIndex = 51;
            this.lblxlsOrderRdd.Tag = "Order By Rdd";
            this.lblxlsOrderRdd.Text = "Order By Rdd";
            this.lblxlsOrderRdd.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnOrderRdd
            // 
            this.btnOrderRdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOrderRdd.Location = new System.Drawing.Point(381, 20);
            this.btnOrderRdd.Margin = new System.Windows.Forms.Padding(2);
            this.btnOrderRdd.Name = "btnOrderRdd";
            this.btnOrderRdd.Size = new System.Drawing.Size(66, 22);
            this.btnOrderRdd.TabIndex = 47;
            this.btnOrderRdd.Text = "Browse";
            this.btnOrderRdd.UseVisualStyleBackColor = true;
            this.btnOrderRdd.Click += new System.EventHandler(this.BtnOrderRdd_Click);
            // 
            // FrmStation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(877, 502);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cmbLines);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmStation";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Station Data";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmStation_FormClosing);
            this.Load += new System.EventHandler(this.FrmStation_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgwLines)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox13.ResumeLayout(false);
            this.groupBox13.PerformLayout();
            this.groupBox12.ResumeLayout(false);
            this.groupBox12.PerformLayout();
            this.groupBox11.ResumeLayout(false);
            this.groupBox11.PerformLayout();
            this.groupBox10.ResumeLayout(false);
            this.groupBox10.PerformLayout();
            this.groupBox9.ResumeLayout(false);
            this.groupBox9.PerformLayout();
            this.groupBox8.ResumeLayout(false);
            this.groupBox8.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox txtBoxStationId;
        private System.Windows.Forms.Label lblStationId;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label lblLine;
        private System.Windows.Forms.Label lblStationName;
        private System.Windows.Forms.TextBox txtBoxStationName;
        private System.Windows.Forms.Label lblxlsDetLock;
        private System.Windows.Forms.Button btnDetLock;
        private System.Windows.Forms.Label lblxlsFP;
        private System.Windows.Forms.Button btnFP;
        private System.Windows.Forms.Label lblxlsEmSg;
        private System.Windows.Forms.Button btnEmSg;
        private System.Windows.Forms.Label lblxlsSpProf;
        private System.Windows.Forms.Button btnSpProf;
        private System.Windows.Forms.Label lblxlsRoutes;
        private System.Windows.Forms.Button btnRoutes;
        private System.Windows.Forms.Label lblxlsCmpRoutes;
        private System.Windows.Forms.Button btnCmpRoutes;
        private System.Windows.Forms.Label lblxlsLxs;
        private System.Windows.Forms.Button btnLXs;
        private System.Windows.Forms.Label lblxlsBgs;
        private System.Windows.Forms.Button BtnBgs;
        private System.Windows.Forms.DataGridView dgwLines;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox checkBoxBG;
        private System.Windows.Forms.CheckBox checkBoxLX;
        private System.Windows.Forms.CheckBox checkBoxCmRts;
        private System.Windows.Forms.CheckBox checkBoxRts;
        private System.Windows.Forms.CheckBox checkBoxSpProf;
        private System.Windows.Forms.CheckBox checkBoxEmSt;
        private System.Windows.Forms.CheckBox checkBoxFP;
        private System.Windows.Forms.CheckBox checkBoxDL;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtBoxVersion;
        private System.Windows.Forms.CheckBox checkBoxBGN;
        private System.Windows.Forms.Label lblxlsBgsN;
        private System.Windows.Forms.Button BtnBgsN;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label lblxlsRdd;
        private System.Windows.Forms.Button btnRdd;
        private System.Windows.Forms.CheckBox checkBoxRdd;
        private System.Windows.Forms.CheckBox checkBoxSCN;
        private System.Windows.Forms.Label lblxlsSigClosN;
        private System.Windows.Forms.Button btnSigClosN;
        private System.Windows.Forms.CheckBox checkBoxSC;
        private System.Windows.Forms.Label lblxlsSigClos;
        private System.Windows.Forms.Button btnSigClos;
        private System.Windows.Forms.ComboBox cmbLines;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblDocId;
        private System.Windows.Forms.TextBox txtBoxDocId;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label lblAcSections;
        private System.Windows.Forms.CheckBox checkBoxAc;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label lblxlsOrderRdd;
        private System.Windows.Forms.Button btnOrderRdd;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox checkBoxR5;
        private System.Windows.Forms.CheckBox checkBoxLevel;
        private System.Windows.Forms.GroupBox groupBox13;
        private System.Windows.Forms.GroupBox groupBox12;
        private System.Windows.Forms.GroupBox groupBox11;
        private System.Windows.Forms.GroupBox groupBox10;
        private System.Windows.Forms.GroupBox groupBox9;
        private System.Windows.Forms.GroupBox groupBox8;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.DataGridViewTextBoxColumn Line;
        private System.Windows.Forms.DataGridViewTextBoxColumn Color;
        private System.Windows.Forms.DataGridViewTextBoxColumn From;
        private System.Windows.Forms.DataGridViewTextBoxColumn To;
        private System.Windows.Forms.DataGridViewTextBoxColumn Direction;
    }
}