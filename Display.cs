using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace ExpPt1
{
    public class Display : Export
    {
        List<Track> Tracks;
        List<Line> TrustedAreaLines = new List<Line>();
        List<PSA> pSAs;
        List<LinesLine> lines = new List<LinesLine>();
        TFileDescr sigLayout = new TFileDescr();
        public TFileDescr SigLayout { get; set; }
        public List<TrackSegmentsTrackSegment> Segments { get; set; }
        public List<SignalsSignal> Signals { get; set; }
        public List<PointsPoint> Points { get; set; }
        List<SignalsSignal> signals;
        List<PointsPoint> points;
        List<SpeedProfilesSpeedProfile> speedProfiles;
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

            ReadLines(blocks, ref lines);

            // Signaling Layout
            //if (!ReadSigLayout(blocks, ref siglayout))
            //{
            //    return;
            //}
            CollectTrustedAreas(TrustedAreaLines, TracksLines);
            //if (!CollectTrustedAreas(TrustedAreaLines, TracksLines))
            //{
            //    AcadApp.ShowAlertDialog("Trusted Areas error. See error log");
            //    return;
            //}

            // Get PSAs
            pSAs = GetPsas().ToList();
        }

        public void LoadData()
        {
            SetBlocksNextStations(blocks);
            List<PSA> pSAs = GetPsas().ToList();
            // Segments
            if (!GetSegments(blocks, TracksLines, Tracks, pSAs, true))
            {
                AcadApp.ShowAlertDialog("Track Segments error. See error log");
                return;
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
            ExportCigClosure = new List<string>();
            GetDocIdVrs();
            ReadSigLayout(blocks, ref sigLayout, true);
            ReadSignals(blocks, ref signals);
            ReadPoints(blocks, ref points, speedProfiles, pSAs, emGs);
            SigLayout = sigLayout;
            Segments =  trcksegments;
            Signals = signals;
            Points = points;
        }
    }
}
