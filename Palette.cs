﻿using Autodesk.AutoCAD.Windows;
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
        public List<Block> Blocks { get; set; }

        public Palette(string dwgPath)
        {
            DwgPath = dwgPath;
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
                palCntrlMb.BtnLoad.Click += BtnLoad_Click;
                palCntrlPt.BtnLoad.Click += BtnLoad_Click;
                palCntrlSigLay.BtnLoad.Click += BtnLoad_Click;
                palCntrlMb.DataGridView.CellDoubleClick += DataGridView_CellDoubleClick;
                palCntrlPt.DataGridView.CellDoubleClick += DataGridView_CellDoubleClick;

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

        private void DataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
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
                    AcadTools.ZoomToObjects(element.BlkRef, 70, element.BlkRef.Id);
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
            Display expDispl = new Display(this.DwgPath);
            expDispl.LoadData();
            if (expDispl.Segments != null && expDispl.Segments.Count > 0)
            {
                DataTable sigLayout = Data.ToDataTable(expDispl.SigLayout);
                palCntrlSigLay.DataGridView.DataSource = sigLayout;

                DataTable segments = Data.ToDataTable(expDispl.Segments);
                palCntrlSeg.DataGridView.DataSource = segments;
                palCntrlSeg.LblInfo.Text = "Segments count: " + segments.Rows.Count;

                DataTable signals = Data.ToDataTable(expDispl.Signals);
                palCntrlMb.DataGridView.DataSource = signals;
                palCntrlMb.LblInfo.Text = "Signals count: " + signals.Rows.Count;

                DataTable points = Data.ToDataTable(expDispl.Points);
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
