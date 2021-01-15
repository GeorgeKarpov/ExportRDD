using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace ExpPt1
{
    public class Display : Export
    {
        List<Track> Tracks;
        List<Line> TrustedAreaLines = new List<Line>();
        List<LinesLine> lines = new List<LinesLine>();
        TFileDescr sigLayout = new TFileDescr();
        List<PermanentShuntingAreasPermanentShuntingArea> areas = new List<PermanentShuntingAreasPermanentShuntingArea>();
        public TFileDescr SigLayout { get; set; }
        public List<TrackSegmentsTrackSegment> Segments { get; set; }
        public List<SignalsSignal> Signals { get; set; }
        public List<PointsPoint> Points { get; set; }
        public List<RoutesRoute> Routes { get; set; }

        public bool InitError { get; set; }

        public Display(string dwgPath) : base(dwgPath)
        {
            InitError = false;
            assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            acDocMgr = AcadApp.DocumentManager;
            acDoc = acDocMgr.MdiActiveDocument;
            db = acDoc.Database;
            this.dwgPath = dwgPath;
            this.dwgDir = Path.GetDirectoryName(dwgPath);
            this.dwgFileName = Path.GetFileNameWithoutExtension(dwgPath);

            RailwayLines = new List<RailwayLine>();
            BlocksToGet = new Dictionary<string, string>();
            ConnLinesKm1 = new Dictionary<string, decimal>();
            Status = new TStatus { };

            TrackSegmentsTmp = new List<TrackSegmentTmp>();
            TrustedAreas = new List<TrustedArea>();
            trcksegments = new List<TrackSegmentsTrackSegment>();
            signals = new List<SignalsSignal>();
            points = new List<PointsPoint>();
            speedProfiles = new List<SpeedProfilesSpeedProfile>();
            acsections = new List<AxleCounterSectionsAxleCounterSection>();

            ReadBlocksDefinitions();
            ReadConnLinesDefinitions();
            blocksErr = false;
            blocks = GetBlocks(ref blocksErr);
            TracksLines = GetTracksLines();
            stationID = GetStationId(blocks);
            if (stationID == null)
            {
                InitError = true;
                return;
            }
            RailwayLines = GetRailwayLines(TracksLines, blocks);
            blckProp = new BlockProperties(stationID);
            excel = new ReadExcel.Excel(stationID);

            TrustedAreaLines = GetTrustedAreasLines();
            Tracks = GetTracksNames().ToList();
            ReadLines(ref lines);
            CollectTrustedAreas(TrustedAreaLines, TracksLines);
        }

        public void LoadData()
        {
            ProgressMeter pm = new ProgressMeter();           
            pm.Start("Loading Data");
            pm.SetLimit(100);
            pm.MeterProgress();
            System.Windows.Forms.Application.DoEvents();
            SetBlocksNextStations(blocks);
            SetBlocksExclude(blocks);
            pSAs = GetPsas().ToList();
            // Segments
            if (!GetSegments(blocks, TracksLines, Tracks, pSAs, true))
            {
                AcadApp.ShowAlertDialog("Track Segments errors. See Errors Tab");
                ErrLogger.ErrorsFound = true;
                //return;
            }
            for (int i = 0; i < 50; i++)
            {
                pm.MeterProgress();
                System.Windows.Forms.Application.DoEvents();
            }
            
            checkData = new Dictionary<string, bool> {
                { "checkBoxRts", false },
                { "checkBoxSC", false },
                { "checkBoxSCN", false },
                { "checkBoxDL", false },
                { "checkBoxFP", false },
                { "checkBoxEmSt", false },
                //checkBoxDL
            };

            List<EmSG> emGs = GetEmGs().ToList();
            // ExportCigClosure = new List<string>();
            docId = GetDocId(out bool docError);
            docVrs = GetDocVers(out docError);

            ReadSigLayout(blocks, ref sigLayout, true);
            ReadPSAs(pSAs, ref areas);
            ReadSignals(blocks, ref signals);
            ReadPoints(blocks, ref points, speedProfiles, pSAs, emGs);
            
            for (int i = 0; i < 50; i++)
            {
                pm.MeterProgress();
                System.Windows.Forms.Application.DoEvents();
            }
            //Routes = GetRoutesList();
            SigLayout = sigLayout;
            Segments = trcksegments;
            Signals = signals;
            Points = points;
            Blocks = blocks;
            pm.Stop();
        }

        public void ExportRoutes()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog("Export routes", this.dwgDir + @"\" + "Routes_exp" + ".xlsx", "xlsx", "Export routes",
                   SaveFileDialog.SaveFileDialogFlags.NoUrls);
            if (saveFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            try
            {
                SetBlocksNextStations(blocks);
                SetBlocksExclude(blocks);
                if (!GetSegments(blocks, TracksLines, Tracks, pSAs, true))
                {
                    AcadApp.ShowAlertDialog("Track Segments errors. See Errors Log");
                    ErrLogger.ErrorsFound = true;
                }
                //ExcelLib.WriteExcel.ExpRoutes(GetRoutesList(), saveFileDialog.Filename);
            }
            catch (IOException e)
            {
                AcadApp.ShowAlertDialog(e.Message);
            }
        }

        public void ExportTsegs()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog("Export routes", this.dwgDir + @"\" + "SSP_Tseg_exp" + ".xlsx", "xlsx", "Export Tsegs",
                   SaveFileDialog.SaveFileDialogFlags.NoUrls);
            if (saveFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            try
            {
                SetBlocksNextStations(blocks);
                SetBlocksExclude(blocks);
                if (!GetSegments(blocks, TracksLines, Tracks, pSAs, true))
                {
                    AcadApp.ShowAlertDialog("Track Segments errors. See Errors Log");
                    ErrLogger.ErrorsFound = true;
                }
                List<ExcelLib.elements.ExpTseg> expTsegs = this.TrackSegmentsTmp
                                                           .Select(x => new ExcelLib.elements.ExpTseg { Designation = x.Designation })
                                                           .ToList();
                //ExcelLib.WriteExcel.ExpSegments(expTsegs, saveFileDialog.Filename);
            }
            catch (IOException e)
            {
                AcadApp.ShowAlertDialog(e.Message);
            }
        }

        public void ExportPoints()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog("Export points", this.dwgDir + @"\" + "TDL_Points_exp" + ".xlsx", "xlsx", "Export TDL",
                   SaveFileDialog.SaveFileDialogFlags.NoUrls);
            bool error = false;
            if (saveFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            try
            {
                SetBlocksNextStations(blocks);
                SetBlocksExclude(blocks);
                if (!GetSegments(blocks, TracksLines, Tracks, pSAs, true))
                {
                    AcadApp.ShowAlertDialog("Track Segments errors. See Errors Log");
                    ErrLogger.ErrorsFound = true;
                }
                var points = blocks
                            .Where(x => x.XsdName == "Point" && x.IsOnCurrentArea)
                            .ToList();
                frmStation = new FrmStation
                {
                    AutoAC = true
                };
                error = !ReadAcSections(blocks, ref acsections);
                List<ExcelLib.ExpTdlPt> Tdls = new List<ExcelLib.ExpTdlPt>();
                foreach (var pt in points)
                {
                    var ownsect = acsections
                                  .Where(x => x.Elements.Element
                                  .Any(e => e.Value == pt.Designation))
                                  .FirstOrDefault();
                    if (ownsect == null)
                    {
                        ErrLogger.Error("Own Tdl not found", pt.Designation, "Tdl export");
                        ErrLogger.ErrorsFound = true;
                        ownsect = new AxleCounterSectionsAxleCounterSection 
                        { 
                            Designation = ""
                        };
                    }
                    Tdls.Add(new ExcelLib.ExpTdlPt
                    {
                        Designation = pt.Attributes["NAME"].Value,
                        OwnTdt = ownsect.Designation.Split('-').Last() 
                    });
                }
                //ExcelLib.WriteExcel.ExpTdls(Tdls, saveFileDialog.Filename);
            }
            catch (IOException e)
            {
                AcadApp.ShowAlertDialog(e.Message);
            }
        }
    }
}
