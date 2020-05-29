using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExpPt1
{
    public partial class ErrCntrl : UserControl
    {
        public ListView ListView { get; set; }
        public ErrCntrl()
        {
            InitializeComponent();
            ListView = this.ListViewErr;
            imageList1.Images.Add("Exclamation", SystemIcons.Error);
            imageList1.Images.Add("Information", SystemIcons.Information);
            imageList1.Images.Add("Warning", SystemIcons.Warning);
        }
    }
}
