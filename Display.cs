using Autodesk.AutoCAD.DatabaseServices;
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

        public Display(string dwgPath) : base(dwgPath)
        {
            assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            acDocMgr = AcadApp.DocumentManager;
            acDoc = acDocMgr.MdiActiveDocument;
            db = acDoc.Database;
            this.dwgPath = dwgPath;
            this.dwgDir = Path.GetDirectoryName(dwgPath);
            this.dwgFileName = Path.GetFileNameWithoutExtension(dwgPath);

            RailwayLines = new List<RailwayLine>();
            BlocksToGet = new Dictionary<string, string>();
            Status = new TStatus { };

            TrackSegmentsTmp = new List<TrackSegmentTmp>();
            TrustedAreas = new List<TrustedArea>();
            trcksegments = new List<TrackSegmentsTrackSegment>();
            signals = new List<SignalsSignal>();
            points = new List<PointsPoint>();
            speedProfiles = new List<SpeedProfilesSpeedProfile>();

            ReadBlocksDefinitions();
            blocksErr = false;
            blocks = GetBlocks(ref blocksErr);
            TracksLines = GetTracksLines();
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
            GetDocIdVrs();
            ReadSigLayout(blocks, ref sigLayout, true);
            ReadPSAs(pSAs, ref areas);
            ReadSignals(blocks, ref signals);
            ReadPoints(blocks, ref points, speedProfiles, pSAs, emGs);
            Routes = RoutesList();
            SigLayout = sigLayout;
            Segments = trcksegments;
            Signals = signals;
            Points = points;
            Blocks = blocks;
        }
    }
}
