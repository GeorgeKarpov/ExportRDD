using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ExpRddApp
{
    public partial class ElCntrl : UserControl
    {
        public Button BtnLoad { get; set; }
        public DataGridView DataGridView { get; set; }
        public BindingSource BindingSource { get; set; }
        public Label LblInfo { get; set; }

        private BindingSource bindingSource;
        public ElCntrl()
        {
            InitializeComponent();
            BtnLoad = this.btnLoad;
            DataGridView = this.dgwMb;
            LblInfo = lblInfo;
            bindingSource = new BindingSource();
            BindingSource = bindingSource;
            dgwMb.DataSource = bindingSource;
            bindingSource.ListChanged += BindingSource_ListChanged;
        }

        private void BindingSource_ListChanged(object sender, System.ComponentModel.ListChangedEventArgs e)
        {
            lblCount.Text = string.Format("{0}", bindingSource.List.Count);
        }

        private void DgwKms_DataSourceChanged(object sender, EventArgs e)
        {
            DataGridView dgw = (DataGridView)(sender);
            dgw.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
            for (int i = 0; i <= dgw.Columns.Count - 1; i++)
            {
                dgw.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                //store autosized widths
                int colw = dgw.Columns[i].Width;
                //remove autosizing
                if (i == dgw.Columns.Count - 1)
                {
                    dgw.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
                else
                {
                    dgw.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    //set width to calculated by autosize
                    dgw.Columns[i].Width = colw;
                }
            }
        }

        private void ChckBoxHide_CheckedChanged(object sender, EventArgs e)
        {
            bindingSource.Filter = ((CheckBox)sender).Checked ? "[Export to RDD] = TRUE" : "";
        }
    }
}
