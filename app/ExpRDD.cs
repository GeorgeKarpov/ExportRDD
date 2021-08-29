using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Windows;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace ExpRddApp
{
    public class ExpRDD
    {
        private AcLayout acLayout;
        private dataMapping.DataProcessor dataProcessor;
        private FrmStation frmStation;
        private LongOperationManager lom;

        public void ExportRDD()
        {
            lom = new LongOperationManager("Creating RDD Data");
            lom.SetTotalOperations(1001);
            lom.Tick(1);
            Thread.Sleep(100);
            acLayout = new AcLayout(Commands.DwgPath, Commands.DwgDir, Commands.AssemblyDir);

            if (acLayout.HasErrors())
            {
                lom.Dispose();
                return;
            }
            acLayout.ReportProgress += AcLayout_ReportProgress;
            acLayout.PreLoadData(out bool error);
            if (error)
            {
                lom.Dispose();
                return;
            }
            frmStation = new FrmStation
            {
                StationId = acLayout.SigLayout.StID,
                StationName = acLayout.SigLayout.StName,
                LoadFiles = acLayout.InputData.LoadIni(),
                DwgDir = Commands.DwgDir
            };
            frmStation.SetAuthors(acLayout.InputData.GetAuthors());

            if (Application.ShowModalDialog(null, frmStation, true) != System.Windows.Forms.DialogResult.OK)
            {
                lom.Dispose();
                return;
            }

            acLayout.PostLoad();
            acLayout.InputData.SaveIni(frmStation.LoadFiles);
            var lxList = acLayout.LevelCrossings
                        .Select(x => x.Designation)
                        .ToList();
            var pwsList = acLayout.Pws
                          .Select(x => x.Designation)
                          .ToList();
            dataProcessor =
                new dataMapping.DataProcessor(Commands.DwgDir, Commands.AssemblyDir,
                                              frmStation.LoadFiles, frmStation.CheckData, lxList, pwsList, acLayout,
                                              frmStation.GetDocId(), frmStation.GetVersion(), frmStation.GetAuthor());
            dataProcessor.ReportProgress += AcLayout_ReportProgress;
            dataProcessor.LoadData();
            RddXmlIO rddXml = new RddXmlIO();
            RddOrder rddOrder = new RddOrder();
            RailwayDesignData rdd = dataProcessor.GetRdd();
            if (frmStation.OrderRdd)
            {
                rdd = rddOrder.OrderRdd(rdd, rddXml.GetRdd(frmStation.GetOrderRddFileName()), true, true);
            }

            rddXml.WriteRddXml(rdd, frmStation.RddSaveTo, new List<string>
                                                        { "Created with ExpPt1 v" + Assembly.GetExecutingAssembly().GetName().Version.ToString(3)
                                                           + " (Georgijs Karpovs) - " + DateTime.Now });
            dataProcessor.ExportReport(Path.GetDirectoryName(frmStation.RddSaveTo) + "//" +
                              Path.GetFileNameWithoutExtension(frmStation.RddSaveTo) + "_" + acLayout.SigLayout.StID + "_report.xlsx");
            error = acLayout.HasErrors() || dataProcessor.HasErrors();
            acLayout.Dispose();
            lom.Dispose();
            if (error)
            {
                Utils.ShowErrList(Commands.DwgDir, "Errors Found",
                    "Export completed with errors. See errors log.", SystemIcons.Exclamation, false);
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Export successful",
                   "Rdd Export", System.Windows.Forms.MessageBoxButtons.OK,
                   System.Windows.Forms.MessageBoxIcon.Information);
            }
        }

        private void AcLayout_ReportProgress(object sender, ProgressEventArgs e)
        {
            if (!lom.Tick(e.Increment))
            {
                lom.Dispose();
                return;
            }
        }

        public void ExportRoutes()
        {
            acLayout = new AcLayout(Commands.DwgPath, Commands.DwgDir, Commands.AssemblyDir);
            SaveFileDialog saveFileDialog = new SaveFileDialog("Export routes",
                                                                Commands.DwgDir + "/" + acLayout.SigLayout.StID + "_routes.xlsx",
                                                                "xlsx", "Export routes",
                                                                SaveFileDialog.SaveFileDialogFlags.NoUrls);
            if (saveFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            var expRts = acLayout.ExportRoutes(out bool error);
            if (error)
            {
                Utils.ShowErrList(Commands.DwgDir, "Export Routes", "Routes export is blocked by following errors", SystemIcons.Error, true);
                return;
            }
            ExcelLib.WriteExcel writeExcel = new ExcelLib.WriteExcel();
            writeExcel.ExpRoutes(expRts, saveFileDialog.Filename, acLayout.SigLayout.StID);
            if (writeExcel.Error)
            {
                Utils.ShowErrList(Commands.DwgDir, "Export Routes",
                    "Unable to export routes. See errors log.", SystemIcons.Error, false);
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Export successful",
                   "Routes Export", System.Windows.Forms.MessageBoxButtons.OK,
                   System.Windows.Forms.MessageBoxIcon.Information);
            }
        }

        public void ExportSspTsegs()
        {
            acLayout = new AcLayout(Commands.DwgPath, Commands.DwgDir, Commands.AssemblyDir);
            SaveFileDialog saveFileDialog = new SaveFileDialog("Export SSP Tsegs",
                                                                Commands.DwgDir + "/" + acLayout.SigLayout.StID + "_sspTsegs.xlsx",
                                                                "xlsx", "Export SSP Tsegs",
                                                                SaveFileDialog.SaveFileDialogFlags.NoUrls);
            if (saveFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            var expSspTsegs = acLayout.ExportTsegs(out bool error);
            if (error)
            {
                Utils.ShowErrList(Commands.DwgDir, "Export SSP Tsegs", "SSP Tsegs export is blocked by following errors", SystemIcons.Error, true);
                return;
            }
            ExcelLib.WriteExcel writeExcel = new ExcelLib.WriteExcel();
            writeExcel.ExpSegments(expSspTsegs, saveFileDialog.Filename);
            if (writeExcel.Error)
            {
                Utils.ShowErrList(Commands.DwgDir, "Export SSP Tsegs",
                    "Unable to export SSP Tsegs. See errors log.", SystemIcons.Error, false);
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Export successful",
                   "SSP Tsegs Export", System.Windows.Forms.MessageBoxButtons.OK,
                   System.Windows.Forms.MessageBoxIcon.Information);
            }
        }

        public void ExportTdls()
        {
            acLayout = new AcLayout(Commands.DwgPath, Commands.DwgDir, Commands.AssemblyDir);
            SaveFileDialog saveFileDialog = new SaveFileDialog("Export TDL",
                                                                Commands.DwgDir + "/" + acLayout.SigLayout.StID + "_tdl.xlsx",
                                                                "xlsx", "Export TDL",
                                                                SaveFileDialog.SaveFileDialogFlags.NoUrls);
            if (saveFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            var expTdls = acLayout.ExportTdlPts(out bool error);
            if (error)
            {
                Utils.ShowErrList(Commands.DwgDir, "Export TDL", "TDL export is blocked by following errors", SystemIcons.Error, true);
                return;
            }
            ExcelLib.WriteExcel writeExcel = new ExcelLib.WriteExcel();
            writeExcel.ExpTdls(expTdls, saveFileDialog.Filename);
            if (writeExcel.Error)
            {
                Utils.ShowErrList(Commands.DwgDir, "Export TDL",
                    "Unable to export TDL. See errors log.", SystemIcons.Error, false);
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Export successful",
                   "TDL Export", System.Windows.Forms.MessageBoxButtons.OK,
                   System.Windows.Forms.MessageBoxIcon.Information);
            }
        }
    }
}
