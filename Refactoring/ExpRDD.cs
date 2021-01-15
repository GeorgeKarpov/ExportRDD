using Autodesk.AutoCAD.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Refact
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
            lom.SetTotalOperations(1000);
            acLayout = new AcLayout(ExpPt1.Commands.DwgPath, ExpPt1.Commands.DwgDir, ExpPt1.Commands.AssemblyDir);
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
                DwgDir = ExpPt1.Commands.DwgDir
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
                new dataMapping.DataProcessor(ExpPt1.Commands.DwgDir, ExpPt1.Commands.AssemblyDir,
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
                              Path.GetFileNameWithoutExtension(frmStation.RddSaveTo) + "_Report.xlsx");
            error = acLayout.HasErrors() || dataProcessor.HasErrors();
            acLayout.Dispose();
            lom.Dispose();
            if (error)
            {
                Utils.ShowErrList(ExpPt1.Commands.DwgDir, "Errors Found",
                    "Export completed with errors. See errors log.", SystemIcons.Exclamation, true);
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

        private void CreateRDD()
        {
            //throw new NotImplementedException();
        }
    }
}
