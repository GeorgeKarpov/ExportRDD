using Autodesk.AutoCAD.Windows;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpPt1
{
    public class Palette
    {
        private static PaletteSet _ps = null;
        private static CtrlMb palCntrlKm = null;
        private static CtrlMb palCntrlTest = null;
        public string DwgPath { get; set;}
        public Palette(string dwgPath)
        {
            DwgPath = dwgPath;
            Reload();
        }

        public void Reload()
        {
            if (_ps == null)
            {
                palCntrlKm = new CtrlMb();
                palCntrlTest = new CtrlMb();
                palCntrlKm.BtnDist.Click += BtnDist_Click;
                _ps = new PaletteSet("RDD"/*, new Guid("87374E16-C0DB-4F3F-9271-7A71ED921567")*/);
                _ps.Add("Markerboards", palCntrlKm);
                //_ps.Add("Points", palCntrlKm);
                _ps.MinimumSize = new Size(200, 40);
                _ps.DockEnabled = (DockSides)(DockSides.Left | DockSides.Right);
                _ps.Visible = true;
            }
            else
            {
                _ps.Visible = true;
            }
        }

        private void BtnExpXls_Click(object sender, EventArgs e)
        {
            //Distance distance = new Distance(DwgPath);
            //distance.GetDistToDisplay();
            //distance.WriteDists(false);
            //distance.Dispose();
        }

        public void ClearKmData()
        {
            palCntrlKm.DataGridView.DataSource = null;
        }

        private void BtnDist_Click(object sender, EventArgs e)
        {
            //Export export = new Export(DwgPath);
            
            //if (distance.Slices != null && distance.Slices.Count > 0)
            //{
            //    DataTable slices = Data.ToDataTable(distance.Slices);
            //    palCntrlKm.DataGridView.DataSource = slices;
            //    palCntrlKm.DataGridView.Columns["Distance"].DisplayIndex = 2;
            //    palCntrlKm.DataGridView.Columns[2].Visible = false;
            //}
            //export.Dispose();
        }
    }
}
