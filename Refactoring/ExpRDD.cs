using Autodesk.AutoCAD.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Refact
{
    public class ExpRDD
    {
        private AcLayout acLayout;
        private FrmStation frmStation;

        public void ExportRDD()
        {

            acLayout = new AcLayout(ExpPt1.Commands.DwgPath, ExpPt1.Commands.DwgDir, ExpPt1.Commands.AssemblyDir);
            acLayout.PreLoadData(out bool error);
            if (error)
            {
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
            dataMapping.DataProcessor dataProcessor =
                new dataMapping.DataProcessor(ExpPt1.Commands.DwgDir, ExpPt1.Commands.AssemblyDir,
                                              frmStation.LoadFiles, frmStation.CheckData, lxList, pwsList, acLayout,
                                              frmStation.GetDocId(), frmStation.GetVersion(), frmStation.GetAuthor());
            RddXmlIO rddXml = new RddXmlIO();
            rddXml.WriteRddXml(dataProcessor.GetRdd(), frmStation.RddSaveTo, new List<string> { "Test Data" });
            acLayout.Dispose();
        }

        private void CreateRDD()
        {
            //throw new NotImplementedException();
        }
    }
}
