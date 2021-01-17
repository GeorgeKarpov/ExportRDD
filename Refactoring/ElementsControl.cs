using System;
using System.Windows.Forms;

namespace Refact
{
    public partial class ElCntrl : UserControl
    {
        public Button BtnLoad { get; set; }
        public DataGridView DataGridView { get; set; }

        public Label LblInfo { get; set; }
        public ElCntrl()
        {
            InitializeComponent();
            BtnLoad = this.btnLoad;
            DataGridView = this.dgwMb;
            LblInfo = lblInfo;
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
    }
}
