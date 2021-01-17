using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ExpRddApp
{
    public partial class ErrCntrl : UserControl
    {
        public ListView ListView { get; set; }
        public Button BtnLoad { get; set; }
        public ErrCntrl()
        {
            InitializeComponent();
            BtnLoad = this.btnLoad;
            ListView = this.ListViewErr;
            imageList1.Images.Add("Exclamation", SystemIcons.Error);
            imageList1.Images.Add("Information", SystemIcons.Information);
            imageList1.Images.Add("Warning", SystemIcons.Warning);
            ListView.Columns[ListView.Columns.Count - 1].Width = -2;
        }

        public void LoadList()
        {
            this.ListView.Items.Clear();
            foreach (var line in ErrLogger.GetWarnLines().Skip(1))
            {
                ListViewItem tmp = new ListViewItem(line.Split(new string[] { " -- ", }, StringSplitOptions.RemoveEmptyEntries), 2);
                this.ListView.Items.Add(tmp);
            }
            ListView.Columns[0].Width = -1;
        }

        private void ListViewErr_SizeChanged(object sender, EventArgs e)
        {
            if (sender == null)
            {
                return;
            }
            ListView.Columns[ListView.Columns.Count - 1].Width = -2;
        }
    }
}
