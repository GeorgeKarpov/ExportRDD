using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Internal;
using Autodesk.AutoCAD.Windows;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ExpPt1
{
    public class Palette
    {
        private static PaletteSet _ps = null;
        private static ElCntrl palCntrlSigLay = null;
        private static ElCntrl palCntrlSeg = null;
        private static ElCntrl palCntrlMb = null;
        private static ElCntrl palCntrlPt = null;
        private static ErrCntrl errCntrl = null;
        public string DwgPath { get; set; }
        DocumentCollection Docs { get; set; }
        public List<Block> Blocks { get; set; }

        public Palette(string dwgPath, DocumentCollection docs)
        {
            DwgPath = dwgPath;
            Docs = docs;
            Reload();

        }

        public void Reset()
        {
            if (_ps == null)
            {
                Reload();
            }
            else
            {
                palCntrlSigLay.DataGridView.DataSource = "";
                palCntrlSeg.DataGridView.DataSource = "";
                palCntrlMb.DataGridView.DataSource = "";
                palCntrlPt.DataGridView.DataSource = "";

                palCntrlSeg.LblInfo.Text = "";
                palCntrlMb.LblInfo.Text = "";
                palCntrlPt.LblInfo.Text = "";
            }
        }

        public void Reload()
        {
            if (_ps == null)
            {
                palCntrlSeg = new ElCntrl();
                palCntrlMb = new ElCntrl();
                palCntrlPt = new ElCntrl();
                palCntrlSigLay = new ElCntrl();
                errCntrl = new ErrCntrl();
                palCntrlSeg.BtnLoad.Click += BtnLoad_Click;
                palCntrlSeg.DataGridView.CellDoubleClick += DgwSegs_CellDoubleClick;
                palCntrlSeg.DataGridView.DataSourceChanged += Dgw_DataSourceChanged;

                palCntrlMb.BtnLoad.Click += BtnLoad_Click;
                palCntrlMb.DataGridView.CellDoubleClick += Dgw_CellDoubleClick;
                palCntrlMb.DataGridView.DataSourceChanged += Dgw_DataSourceChanged;

                palCntrlPt.BtnLoad.Click += BtnLoad_Click;
                palCntrlPt.DataGridView.CellDoubleClick += Dgw_CellDoubleClick;
                palCntrlPt.DataGridView.DataSourceChanged += Dgw_DataSourceChanged;

                palCntrlSigLay.BtnLoad.Click += BtnLoad_Click;
                palCntrlSigLay.DataGridView.DataSourceChanged += Dgw_DataSourceChanged;





#if DEBUG
                _ps = new PaletteSet("RDD");
#else
                _ps = new PaletteSet("RDD", new Guid("87374E16-C0DB-4F3F-9271-7A71ED921568"));
#endif

                _ps.Add("SL", palCntrlSigLay);
                _ps.Add("Track Segments", palCntrlSeg);
                _ps.Add("Sinals", palCntrlMb);
                _ps.Add("Points", palCntrlPt);
                _ps.Add("Errors", errCntrl);
                _ps.MinimumSize = new Size(200, 40);
                _ps.DockEnabled = (DockSides.Left | DockSides.Right);
                _ps.Visible = true;
            }
            else
            {
                _ps.Visible = true;
            }
        }

        private void Dgw_DataSourceChanged(object sender, EventArgs e)
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
        private void DgwSegs_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dataGrid = (DataGridView)sender;
            if (e.RowIndex != -1)
            {
                Entity[] entities = new Entity[2];
                string designation = dataGrid.Rows[e.RowIndex].Cells[2].Value.ToString();
                string designation1 = dataGrid.Rows[e.RowIndex].Cells[3].Value.ToString();
                Block element = Blocks
                                .Where(x => x.Designation == designation)
                                .FirstOrDefault();
                if (element != null)
                {
                    entities[0] = element.BlkRef;
                }
                element = Blocks
                          .Where(x => x.Designation == designation1)
                          .FirstOrDefault();
                if (element != null)
                {
                    entities[1] = element.BlkRef;
                }
                if (entities[0] != null && entities[1] != null)
                {
                    AcadTools.ZoomToObjects(entities, 150);
                }
            }
        }

        private void Dgw_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dataGrid = (DataGridView)sender;
            if (e.RowIndex != -1)
            {

                string designation = dataGrid.Rows[e.RowIndex].Cells[0].Value.ToString();
                Block element = Blocks
                                .Where(x => x.Designation == designation)
                                .FirstOrDefault();
                if (element != null)
                {
                    AcadTools.ZoomToObjects(element.BlkRef, 70);
                }              
            }
        }

        //private void BtnLoadMb_Click(object sender, EventArgs e)
        //{
        //    throw new NotImplementedException();
        //}

        //private void BtnExpXls_Click(object sender, EventArgs e)
        //{
        //    //Distance distance = new Distance(DwgPath);
        //    //distance.GetDistToDisplay();
        //    //distance.WriteDists(false);
        //    //distance.Dispose();
        //}

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            if (Docs.Count == 0)
            {
                return;
            }
            Display expDispl = new Display(this.DwgPath);
            expDispl.LoadData();
            if (expDispl.Segments != null && expDispl.Segments.Count > 0)
            {
                System.Data.DataTable sigLayout = Data.ToDataTable(expDispl.SigLayout);
                palCntrlSigLay.DataGridView.DataSource = sigLayout;

                System.Data.DataTable segments = Data.ToDataTable(expDispl.Segments);
                palCntrlSeg.DataGridView.DataSource = segments;
                palCntrlSeg.LblInfo.Text = "Segments count: " + segments.Rows.Count;

                System.Data.DataTable signals = Data.ToDataTable(expDispl.Signals);
                palCntrlMb.DataGridView.DataSource = signals;
                palCntrlMb.LblInfo.Text = "Signals count: " + signals.Rows.Count;

                System.Data.DataTable points = Data.ToDataTable(expDispl.Points);
                palCntrlPt.DataGridView.DataSource = points;
                palCntrlPt.LblInfo.Text = "Points count: " + points.Rows.Count;
            }
            errCntrl.ListView.Items.Clear();
            foreach (var line in File.ReadAllLines(ErrLogger.filePath)
                                .Where(x => x[0] != '#' && 
                                            !x.Contains("log begin") && 
                                            !x.Contains("log end")))
            {
                ListViewItem tmp = new ListViewItem(line.Split(new string[] { " -- ", }, StringSplitOptions.RemoveEmptyEntries), 2);
                errCntrl.ListView.Items.Add(tmp);
            }     
            if (ErrLogger.error)
            {
                _ps.Activate(_ps.Count - 1);
            }
            Blocks = expDispl.Blocks;
            expDispl.Dispose();
        }
    }
}
