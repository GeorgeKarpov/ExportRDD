using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Windows;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace ExpRddApp
{
    public class Palette
    {
        private static PaletteSet _ps = null;
        private static ElCntrl palCntrlSigLay = null;
        private static ElCntrl palCntrlSeg = null;
        private static ElCntrl palCntrlMb = null;
        private static ElCntrl palCntrlPt = null;
        private static ElCntrl palCntrlRt = null;
        private static ErrCntrl errCntrl = null;
        public string DwgPath { get; set; }
        public string DwgDir { get; set; }

        public string AssemblyDir { get; set; }
        DocumentCollection Docs { get; set; }
        public List<elements.SLElement> Elements { get; set; }

        private LongOperationManager lom;

        public Palette(string dwgPath, string dwgDir, string assDir, DocumentCollection docs)
        {
            DwgPath = dwgPath;
            DwgDir = dwgDir;
            AssemblyDir = assDir;
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
                palCntrlRt.DataGridView.DataSource = "";
                errCntrl.ListView.Items.Clear();

                palCntrlSeg.LblInfo.Text = "";
                palCntrlMb.LblInfo.Text = "";
                palCntrlPt.LblInfo.Text = "";
                palCntrlRt.LblInfo.Text = "";
            }
        }

        public void Reload()
        {
            if (_ps == null)
            {
                palCntrlSeg = new ElCntrl();
                palCntrlMb = new ElCntrl();
                palCntrlPt = new ElCntrl();
                palCntrlRt = new ElCntrl();
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

                palCntrlRt.BtnLoad.Click += BtnLoad_Click;
                palCntrlRt.DataGridView.CellDoubleClick += DgwRts_CellDoubleClick;
                palCntrlRt.DataGridView.DataSourceChanged += Dgw_DataSourceChanged;

                palCntrlSigLay.BtnLoad.Click += BtnLoad_Click;
                palCntrlSigLay.DataGridView.DataSourceChanged += Dgw_DataSourceChanged;

                errCntrl.ListView.DoubleClick += ListView_DoubleClick;
                errCntrl.BtnLoad.Click += BtnLoad_Click;


#if DEBUG
                _ps = new PaletteSet("RDD");
#else
                _ps = new PaletteSet("RDD", new Guid("87374E16-C0DB-4F3F-9271-7A71ED921568"));
#endif

                _ps.Add("SL", palCntrlSigLay);
                _ps.Add("Track Segments", palCntrlSeg);
                _ps.Add("Sinals", palCntrlMb);
                _ps.Add("Points", palCntrlPt);
                _ps.Add("Routes", palCntrlRt);
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

        private void ListView_DoubleClick(object sender, EventArgs e)
        {
            ListView lstView = (ListView)(sender);
            if (lstView.SelectedItems.Count > 0)
            {

                string designation = lstView.SelectedItems[0].SubItems[1].Text.Split(new string[] { "|", " " }, options: StringSplitOptions.RemoveEmptyEntries)[0];
                elements.SLElement element = Elements
                                   .Where(x => x.Designation == designation)
                                   .FirstOrDefault();
                if (element != null)
                {
                    AcadTools.ZoomToObjects(element.Block.BlockReference, 70);
                }
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
                elements.SLElement element = Elements
                                .Where(x => x.Designation == designation)
                                .FirstOrDefault();
                if (element != null)
                {
                    entities[0] = element.Block.BlockReference;
                }
                element = Elements
                          .Where(x => x.Designation == designation1)
                          .FirstOrDefault();
                if (element != null)
                {
                    entities[1] = element.Block.BlockReference;
                }
                if (entities[0] != null && entities[1] != null)
                {
                    AcadTools.ZoomToObjects(entities, 150);
                }
            }
        }

        private void DgwRts_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dataGrid = (DataGridView)sender;
            if (e.RowIndex != -1)
            {
                Entity[] entities = new Entity[2];
                string designation = dataGrid.Rows[e.RowIndex].Cells[0].Value.ToString();
                string designation1 = dataGrid.Rows[e.RowIndex].Cells[1].Value.ToString();
                elements.SLElement element = Elements
                                   .Where(x => x.Designation == designation)
                                   .FirstOrDefault();
                if (element != null)
                {
                    entities[0] = element.Block.BlockReference;
                }
                element = Elements
                          .Where(x => x.Designation == designation1)
                          .FirstOrDefault();
                if (element != null)
                {
                    entities[1] = element.Block.BlockReference;
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
                elements.SLElement element = Elements
                                  .Where(x => x.Designation == designation)
                                  .FirstOrDefault();
                if (element != null)
                {
                    AcadTools.ZoomToObjects(element.Block.BlockReference, 70);
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
            AcLayout acLayout = new AcLayout(this.DwgPath, this.DwgDir, this.AssemblyDir);
            lom = new LongOperationManager("Loading Data");
            lom.SetTotalOperations(700);
            if (acLayout.HasErrors())
            {
                AcadApp.ShowAlertDialog("Data Initialization error. See Errors Tab");
                ErrLogger.ErrorsFound = true;
                errCntrl.LoadList();
                _ps.Activate(_ps.Count - 1);
                lom.Dispose();
                return;
            }
            acLayout.ReportProgress += AcLayout_ReportProgress;
            acLayout.LoadDisplayData();

            if (acLayout.Tsegs != null && acLayout.Tsegs.Count > 0)
            {
                System.Data.DataTable sigLayout = Data.ToDataTable(acLayout.SigLayout);
                palCntrlSigLay.DataGridView.DataSource = sigLayout;

                System.Data.DataTable segments = Data.ToDataTable(acLayout.Tsegs);
                palCntrlSeg.DataGridView.DataSource = segments;
                palCntrlSeg.LblInfo.Text = "Segments count: " + segments.Rows.Count;

                System.Data.DataTable signals = Data.ToDataTable(acLayout.Signals);
                palCntrlMb.DataGridView.DataSource = signals;
                palCntrlMb.LblInfo.Text = "Signals count: " + signals.Rows.Count;

                System.Data.DataTable points = Data.ToDataTable(acLayout.Points);
                palCntrlPt.DataGridView.DataSource = points;
                palCntrlPt.LblInfo.Text = "Points count: " + points.Rows.Count;


                System.Data.DataTable routes = Data.ToDataTable(acLayout.Routes);
                palCntrlRt.DataGridView.DataSource = routes;
                palCntrlRt.LblInfo.Text = "Routes count: " + routes.Rows.Count;
            }
            errCntrl.LoadList();
            if (acLayout.HasErrors())
            {
                _ps.Activate(_ps.Count - 1);
            }
            Elements = acLayout.Elements;
            lom.Dispose();
            acLayout.Dispose();
        }

        private void AcLayout_ReportProgress(object sender, ProgressEventArgs e)
        {
            if (!lom.Tick(e.Increment))
            {
                lom.Dispose();
                return;
            }
        }
    }
}
