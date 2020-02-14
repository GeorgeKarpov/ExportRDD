﻿using System;
using System.Windows.Forms;

namespace ExpPt1
{
    public partial class CtrlMb : UserControl
    {
        public Button BtnDist { get; set; }
        public DataGridView DataGridView { get; set; }
        public CtrlMb()
        {
            InitializeComponent();
            BtnDist = this.btnLoad;
            DataGridView = this.dgwMb;
        }

        private void DgwKms_DataSourceChanged(object sender, EventArgs e)
        {
            for (int i = 0; i <= dgwMb.Columns.Count - 1; i++)
            {
                dgwMb.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                //store autosized widths
                int colw = dgwMb.Columns[i].Width;
                //remove autosizing
                dgwMb.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                //set width to calculated by autosize
                dgwMb.Columns[i].Width = colw;
            }
        }
    }
}
