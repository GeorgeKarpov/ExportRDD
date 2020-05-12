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
        private static Cntrl palCntrlSigLay = null;
        private static Cntrl palCntrlSeg = null;
        private static Cntrl palCntrlMb = null;
        private static Cntrl palCntrlPt = null;
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
                palCntrlSeg = new Cntrl();
                palCntrlMb = new Cntrl();
                palCntrlPt = new Cntrl();
                palCntrlSigLay = new Cntrl();
                palCntrlSeg.BtnLoad.Click += BtnLoadSeg_Click;
                palCntrlMb.BtnLoad.Click += BtnLoadSeg_Click;
                palCntrlPt.BtnLoad.Click += BtnLoadSeg_Click;
                palCntrlSigLay.BtnLoad.Click += BtnLoadSeg_Click;
#if DEBUG
                _ps = new PaletteSet("RDD");
#else
                _ps = new PaletteSet("RDD", new Guid("87374E16-C0DB-4F3F-9271-7A71ED921567"));
#endif

                _ps.Add("SL", palCntrlSigLay);
                _ps.Add("Track Segments", palCntrlSeg);
                _ps.Add("Sinals", palCntrlMb);
                _ps.Add("Points", palCntrlPt);
                _ps.MinimumSize = new Size(200, 40);
                _ps.DockEnabled = (DockSides)(DockSides.Left | DockSides.Right);
                _ps.Visible = true;
            }
            else
            {
                _ps.Visible = true;
            }
        }

        private void BtnLoadMb_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
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
            palCntrlSeg.DataGridView.DataSource = null;
        }

        private void BtnLoadSeg_Click(object sender, EventArgs e)
        {
            Display expDispl = new Display(this.DwgPath);
            expDispl.LoadData();
            if (expDispl.Segments != null && expDispl.Segments.Count > 0)
            {
                DataTable segments = Data.SegsToDataTable(expDispl.Segments);
                palCntrlSeg.DataGridView.DataSource = segments;
                palCntrlSeg.LblInfo.Text = "Segments count: " + segments.Rows.Count;

                DataTable signals = Data.SignalsToDataTable(expDispl.Signals);
                palCntrlMb.DataGridView.DataSource = signals;
                palCntrlMb.LblInfo.Text = "Signals count: " + signals.Rows.Count;

                DataTable points = Data.PointsToDataTable(expDispl.Points);
                palCntrlPt.DataGridView.DataSource = points;
                palCntrlPt.LblInfo.Text = "Points count: " + points.Rows.Count;

                DataTable sigLayout = Data.SigLayouToDataTable(expDispl.SigLayout);
                palCntrlSigLay.DataGridView.DataSource = sigLayout;
            }
            expDispl.Dispose();
        }
    }
}
