using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Refact.elements;

namespace Refact
{
    /// <summary>
    /// This class represents Signalling layout from AutoCAD drawing and
    /// has basic methods to collect Railway elements.
    /// </summary>
    public class AcLayout : IDisposable
    {
        public InputData InputData { get; set; }
        public SigLayout SigLayout { get; set; }
        public List<Connector> Connectors { get; set; }
        public List<elements.Point> Points { get; set; }
        public List<Hht> Hhts { get; set; }
        public List<EndOfTrack> EndOfTracks { get; set; }
        public List<Signal> Signals { get; set; }
        public List<DetectionPoint> DetectionPoints { get; set; }
        public List<BlockInterface> BlockInterfaces { get; set; }
        public List<BaliseGroup> BaliseGroups { get; set; }
        public List<AcSection> AcSections { get; set; }
        public List<SLElement> Elements { get; set; }
        public List<TSeg> Tsegs { get; set; }
        public List<LevelCrossing> LevelCrossings { get; set; }
        public List<Pws> Pws { get; set; }
        public List<TrustedArea> TrustedAreas { get; set; }
        public List<RailwayLine> RailwayLines { get; set; }
        public List<Platform> Platforms { get; set; }
        public List<StationStop> StationsStops { get; set; }
        public List<FoulingPoint> FoulingPoints { get; set; }
        public List<Psa> Psas { get; set; }
        public List<ExcelLib.ExpRoute> Routes { get; set; }

        private bool error;

        private string assemblyDir;
        private string dwgPath;
        private string dwgDir;
        private DocumentCollection docMngr;
        private Document doc;
        private Database db;
        private List<Refact.Block> blocks;
        
        private Dictionary<string, string> blkDat;
        private Dictionary<string, string> stationsDat;
        private Dictionary<string, string> linesDat;
        private Dictionary<string, string> mnTracksDat;
        private List<TrackLine> trackLines;
        /// <summary>
        /// Track Lines representing Railway track path.
        /// </summary>
        
        private List<EnclosedArea> enclosedAreas;
        
        
        private List<SLElement> TsegVertexes;

        private List<Track> tracks;

        private List<OldPlatform> oldPlatforms;

        public event EventHandler<ProgressEventArgs> ReportProgress;

        protected virtual void OnReportProgress(ProgressEventArgs e)
        {
            ReportProgress?.Invoke(this, e);
        }
        ProgressEventArgs args = new ProgressEventArgs
        {
            Increment = 0
        };

        // COMMENT: create methods documentation
        public AcLayout(string dwgPath, string dwgDir, string assDir)
        {
            assemblyDir = assDir;
            docMngr = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager;
            doc = docMngr.MdiActiveDocument;
            db = doc.Database;
            this.dwgPath = dwgPath;
            this.dwgDir = dwgDir;
            ErrLogger.Prefix = Path.GetFileNameWithoutExtension(this.dwgPath);
            ErrLogger.StartTmpLog(dwgDir);
            ErrLogger.Information(Path.GetFileNameWithoutExtension(this.dwgPath), "Processed SL");
            ErrLogger.Error(Path.GetFileNameWithoutExtension(this.dwgPath), "Processed SL", "");

            InputData = new InputData(assemblyDir, dwgDir, this.dwgPath);
            blkDat = InputData.GetBlocks();
            stationsDat = InputData.GetStations();
            linesDat = InputData.GetLines();
            mnTracksDat = InputData.GetMnTracks();
            tracks = new List<Track>();

            bool blkError = false;
            blocks = AcadTools.GetBlocks(ref blkError, db, blkDat);
            bool error = blkError = InputData.Error;
            if (error)
            {
                ShowBlockRDDDialog();
                this.error = true;
                return;
            }
            GetSigLayout(ref error);
            
            if (error)
            {
                ShowBlockRDDDialog();
                this.error = true;
                return;
            }
            trackLines = GetTracksLines();
            RailwayLines = GetRailwayLines(ref error);
            if (error)
            {
                ShowBlockRDDDialog();
                this.error = true;
                return;
            }
            if (error)
            {
                this.error = true;
            }         
        }

        public void PreLoadData(out bool error)
        {           
            InitElements();
            SplitLinesOnVertexes(out error);
            args.Increment = 100;
            OnReportProgress(args);
            if (error)
            {
                ShowBlockRDDDialog();
                return;
            }
            SetNextExclude();
            ProcessElements(ref error);
            tracks = GetTracks().ToList();
            args.Increment = 100;
            OnReportProgress(args);

            Tsegs = GetTsegs(ref error);
            args.Increment = 50;
            OnReportProgress(args);

            AssignTsegsToConns();
            args.Increment = 50;
            OnReportProgress(args);
            if (error)
            {
                ShowBlockRDDDialog();
                return;
            }        
        }

        public void PostLoad()
        {
            bool error = false;
            AssignTrackToTsegs();
            SetTsegIdToElements(ref error);
            SetSigsEotmb();
            SetSigsKind();
            SetEotsDir();
            args.Increment = 50;
            OnReportProgress(args);

            TrustedAreas = GetTrustedAreas();
            InitAcSections(ref error);
            args.Increment = 50;
            OnReportProgress(args);

            CheckAcTrackSections();
            Psas = GetPsas().ToList();
            InitPsas();
            CheckSignalsToPSA();
            args.Increment = 200;
            OnReportProgress(args);

            GetSignalsClosures();
            InitLxsPwss();
            ProcessPlatforms();
            StationsStops = GetStationsStops();
            args.Increment = 200;
            OnReportProgress(args);
            if (error)
            {
                this.error = true;
            }
        }

        public void LoadDisplayData()
        {
            args.Increment = 100;
            OnReportProgress(args);
            bool error;
            InitElements();
            SplitLinesOnVertexes(out error);
            if (error)
            {
                this.error = true;
            }
            SetNextExclude();
            ProcessElements(ref error);

            args.Increment = 200;
            OnReportProgress(args);
            Tsegs = GetTsegs(ref error);
            tracks = GetTracks().ToList();

            args.Increment = 200;
            OnReportProgress(args);
            AssignTsegsToConns();
            AssignTrackToTsegs();
            SetTsegIdToElements(ref error);
            SetSigsEotmb();
            SetSigsKind();
            SetEotsDir();

            args.Increment = 200;
            OnReportProgress(args);
            GetSignalsClosures();

            Routes = GetExpRoutes();
        }

        private void AssignTsegsToConns()
        {
            foreach (var conn in Connectors.Where(c => !c.Exclude && !c.NextStation))
            {
                var tsegs = Tsegs
                          .Where(x => x.TrackLines
                                      .Any(b => conn.Branches.Contains(b)))
                          .DistinctBy(x => x.Id)
                          .OrderBy(o => o.Vertex1.Km)
                          .ToList();
                if (tsegs.Count < 2)
                {
                    ErrLogger.Error("Unable to found Track segments for connector", conn.Designation, "");
                    error = true;
                }
                else
                {
                    conn.Tseg1 = tsegs[0];
                    conn.Tseg2 = tsegs[1];
                }
            }
        }

        private void SetEotsDir()
        {
            foreach ( EndOfTrack eot in EndOfTracks)
            {
                eot.Direction = GetEotDirection(eot);
            }
        }

        private void CheckSignalsToPSA()
        {
            foreach (Signal signal in Signals)
            {
                if (signal.ToPSA = IsSigToPsa(signal))
                {
                    signal.Remarks = "Signal is towards PSA";
                }  
            }
        }

        private void ProcessPlatforms()
        {
            var stationTexts = AcadTools.GetMtextsByRegex(Constants.stationsStopsTextReg, db);
            foreach (var platform in Platforms)
            {
                MText text = stationTexts
                            .Where(x => AcadTools.GetMiddlPoint3d(x.GeometricExtents).X >=
                                             platform.Block.BlockReference.GeometricExtents.MinPoint.X &&
                                        AcadTools.GetMiddlPoint3d(x.GeometricExtents).X <=
                                             platform.Block.BlockReference.GeometricExtents.MaxPoint.X)
                            .FirstOrDefault();
                if (text == null)
                {
                    ErrLogger.Error("Station/Stop text not found", "Platform", platform.Designation);
                    error = true;
                }
                else
                {
                    platform.StID = text.Contents
                                    .ToLower()            
                                    .Split(new string[] { "(", ")" }, StringSplitOptions.RemoveEmptyEntries)
                                    .Last()
                                    .Trim();
                    platform.StName = text.Contents
                                    .ToLower()
                                    .Split(new string[] { "(", ")" }, StringSplitOptions.RemoveEmptyEntries)
                                    .First()
                                    .Trim();
                }
                var tsegs = Tsegs
                            .Where(x => x.TrackLines
                                  .Any(a => AcadTools.ObjectsIntersects(a.line, platform.Block.BlockReference, Intersect.OnBothOperands)))
                            .ToList();
                if (tsegs.Count == 0)
                {
                    ErrLogger.Error("Tsegs not found", "Platform", platform.Designation);
                    error = true;
                    platform.Tsegs = new List<TSeg>();
                }
                else if (tsegs.Count > 2)
                {
                    ErrLogger.Error("To many Tsegs found: " + string.Join(",", tsegs.Select(x => x.Id)) , "Platform", platform.Designation);
                    error = true;
                    platform.Tsegs = new List<TSeg>();
                }
                else if (tsegs.Count == 2)
                {

                    platform.Tsegs = GetTsegsBetweenTwoTsegs(tsegs[0], tsegs[1])
                                     .OrderBy(x => x.Vertex1.Km)
                                     .ThenBy(l => l.LineID)
                                     .ToList();
                }
                else if (tsegs.Count == 1)
                {
                    platform.Tsegs = tsegs
                                     .OrderBy(x => x.Vertex1.Km)
                                     .ThenBy(l => l.LineID)
                                     .ToList();
                }
                var track = platform.Tsegs
                            .Select(x => x.Track).Where(t => t.Id != "-1")
                            .FirstOrDefault();
                if (track != null)
                {
                    platform.Track = track.Id;
                }
                else
                {
                    platform.Track = "1";
                }           
            }
            var query = Platforms
                            .OrderBy(x => x.Track)
                            .GroupBy(g => new { g.Track, g.StID })
                            .Select(p => p)
                            .ToList();
            foreach (var group in query)
            {
                char a = 'a';
                int plCount = group.Count();
                foreach (Platform platformGrp in group)
                {
                    if (plCount > 1)
                    {
                        platformGrp.Attributes["NAME"].value = platformGrp.Track + a.ToString();
                        a++;
                    }
                    else
                    {
                        platformGrp.Attributes["NAME"].value = platformGrp.Track;
                    }
                    platformGrp.Designation = platformGrp.GetElemDesignation(PadZeros: false);
                }    
            }
            Platforms = Platforms
                        .OrderBy(x => x.Track)
                        .ToList();
        }

        private List<StationStop> GetStationsStops()
        {
            List<StationStop> StationsStops = new List<StationStop>();
            foreach (Platform platform in Platforms.Where(x => x.StID != SigLayout.StID))
            {
                StationStop stop = new StationStop
                {
                    StId = platform.StID,
                    StName = platform.StName,
                    Kind = KindOfSASType.stop,
                    Lines = new List<RailwayLine>
                    {
                        new RailwayLine
                        {
                            Designation = platform.TrackLine.LineID,
                            start = platform.Km1.ToString(),
                            end = platform.Km2.ToString()
                        }
                    }
                };
                StationsStops.Add(stop);
            }
            List<RailwayLine> stationLines = new List<RailwayLine>();
            foreach (RailwayLine line in RailwayLines)
            {
                decimal start = Signals
                                .Where(x => x.Visible == true &&
                                            x.LineID == line.Designation)
                                .Min(x => x.Location);
                decimal end = Signals
                                .Where(x => x.Visible == true &&
                                            x.LineID == line.Designation)
                                .Max(x => x.Location);

                stationLines.Add(new RailwayLine
                {
                    Designation = line.Designation,
                    start = start.ToString(),
                    end = end.ToString()
                });
            }
            StationStop station = new StationStop
            {
                StId = SigLayout.StID,
                StName = SigLayout.StName,
                Kind = KindOfSASType.station,
                Lines = stationLines
            };
            StationsStops.Add(station);
            return StationsStops;
        }

        private IEnumerable<Track> GetTracks()
        {
            var mTexts = AcadTools.GetMtextsByRegex(Constants.trackRegex, db);
            foreach (MText text in mTexts)
            {
                string id = Regex.Replace(text.Text, "[^0-9]", "");
                bool mTrack = false;
                if (mnTracksDat.TryGetValue(SigLayout.StID, out string mTrackdatLine))
                {
                    if (id == mTrackdatLine.Split('\t')[1])
                    {
                        mTrack = true;
                    }
                }
                
                yield return new Track 
                {
                    Id = id, 
                    Position = text.Location,
                    Main = mTrack
                };
            }
        }

        private void AssignTrackToTsegs()
        {
            if (Tsegs == null || Tsegs.Count == 0)
            {
                ErrLogger.Error("Unable to get first TSeg for track", SigLayout.StID, "");
                error = true;
                return;
            }
            foreach (Track track in tracks)
            {
                TSeg firstTseg = Tsegs
                                 .Where(x => x.TrackLines
                .Any(t => (t.line.GetClosestPointTo(track.Position, false) - track.Position).Length <= Constants.mTrackDist))
                                 .FirstOrDefault();
                if (firstTseg == null)
                {
                    ErrLogger.Error("Unable to get first TSeg for track", track.Id, "");
                    error = true;
                    continue;
                }
                firstTseg.Track = track;
                TSeg nextSeg = firstTseg;
                DirectionType direction = DirectionType.up;
                int limit = 1;
                while(nextSeg != null)
                {
                    if (limit >= Constants.nextNodeMaxAttemps)
                    {
                        ErrLogger.Error("Iteration limit reached looking for track segments", track.Id,firstTseg.Id);
                        error = true;
                        break;
                    }
                    nextSeg = GetNextTsegNoBranch(nextSeg, ref direction);
                    if (nextSeg != null)
                    {
                        nextSeg.Track = track;
                    }
                    limit++;
                }
                direction = DirectionType.down;
                limit = 1;
                nextSeg = firstTseg;
                while (nextSeg != null)
                {
                    if (limit >= Constants.nextNodeMaxAttemps)
                    {
                        ErrLogger.Error("Iteration limit reached looking for track segments", track.Id, firstTseg.Id);
                        error = true;
                        break;
                    }
                    nextSeg = GetNextTsegNoBranch(nextSeg, ref direction);
                    if (nextSeg != null)
                    {
                        nextSeg.Track = track;
                    }
                }
            }
            foreach (var tseg in Tsegs.Where(x => x.Track == null))
            {
                tseg.Track = new Track
                {
                    Id = "-1",
                    Main = false,
                    Position = new Point3d()
                };
            }
        }

        private void InitLxsPwss()
        {
            bool error = false;
            foreach (LevelCrossing lx in LevelCrossings)
            {
                lx.Tracks = GetLxPwsTracks(lx);
            }
            foreach (Pws pws in Pws)
            {
                pws.Tracks = GetLxPwsTracks(pws);
            }
            if (error)
            {
                this.error = true;
            }
        }

        private List<LxTrack> GetLxPwsTracks(SLElement lxPws)
        {
            List<LxTrack> lxTracks = new List<LxTrack>();
            var lxTrackLines = trackLines
                    .Where(x => AcadTools.ObjectsIntersects(x.line, lxPws.Block.BlockReference, Intersect.OnBothOperands))
                    .ToList();
            if (lxTrackLines.Count == 0)
            {
                ErrLogger.Error("No tracks for Level Crossing found.", lxPws.Designation, "");
                error = true;
                return lxTracks;
            }

            char a1 = 'a';
            int num1 = 1;
            foreach (TrackLine trLine in lxTrackLines)
            {
                TSeg tsegTrack = Tsegs
                                 .Where(x => x.TrackLines.Contains(trLine))
                                 .FirstOrDefault();

                if (tsegTrack != null)
                {
                    LxTrack lxTrack = new LxTrack
                    {
                        Track = tsegTrack.Track,
                        TrackLine = trLine,
                        TSeg = tsegTrack
                    };
                    
                    lxTracks.Add(lxTrack);
                }
                else
                {
                    ErrLogger.Error("Track segment fol Level Crossing track not found.", lxPws.Designation, "");
                    error = true;
                }
            }
            if (lxTracks.Any(x => x.Track.Main))
            {
                lxTracks = lxTracks
                           .OrderByDescending(x => x.Track.Main)
                           .ThenBy(y => y.Track.Id)
                           .ToList();
            }
            else
            {
                lxTracks = lxTracks
                           .OrderBy(y => y.Track.Id)
                           .ToList();
            }
            foreach (var lxTrack in lxTracks)
            {
                lxTrack.Id = lxTrack.GetId(num1, a1, lxTrackLines.Count, lxPws);
                if (lxPws.ElType == XType.LevelCrossing)
                {
                    lxTrack.SetLocations((LevelCrossing)lxPws, num1, lxTrackLines.Count, out bool error);
                }
                else
                {
                    lxTrack.SetLocations((Pws)lxPws, num1, lxTrackLines.Count, out bool error);
                }
                a1++;
                num1++;
                //We set Lx/Pws location to get detection points only
                //In Rdd for Lx/Pws locations level crossing tracks are used instead
                lxPws.Location = lxTrack.Location;
                lxTrack.DetectionPoints = GetDpsLxPwsTrack(lxPws, lxTrack.TSeg);
                lxTrack.LxAcSection = GetLxAcSection(lxTrack.DetectionPoints, lxPws.Designation + ": " + lxTrack.TSeg.Id);
            }
            return lxTracks;
        }

        private string GetLxAcSection(List<DetectionPoint> detectionPoints, string lxTrackId)
        {
            if (detectionPoints == null || detectionPoints.Count < 2)
            {
                ErrLogger.Error("LxAcSection not found", lxTrackId, "");
                error = true;
                return null;
            }
            var lxAcSection = AcSections
                             .Where(dp => dp.DetectionPoints.Contains(detectionPoints[0]) &&
                                          dp.DetectionPoints.Contains(detectionPoints[1]))
                             .FirstOrDefault();
            if (lxAcSection == null)
            {
                ErrLogger.Error("LxAcSection not found", lxTrackId, "");
                error = true;
                return null;
            }
            return lxAcSection.Designation;
        }

        private List<DetectionPoint> GetDpsLxPwsTrack(SLElement lxPws, TSeg tsegTrack)
        {
            SearchElement dp1 = GetElementsOnNodes(tsegTrack, lxPws, typeof(DetectionPoint), DirectionType.up)
                                        .FirstOrDefault();
            SearchElement dp2 = GetElementsOnNodes(tsegTrack, lxPws, typeof(DetectionPoint), DirectionType.down)
                                .FirstOrDefault();
            List<DetectionPoint> dps = new List<DetectionPoint>();
            if (dp1 != null && dp2 != null)
            {
                dps.Add((DetectionPoint)dp1.Element);
                dps.Add((DetectionPoint)dp2.Element);
            }
            else
            {
                ErrLogger.Error("Not all axel counter found for Lx track", lxPws.Designation, tsegTrack.Id);
                error = true;
            }
            return dps;
        }

        /// <summary>
        /// Shows errors to be resolved dialog.
        /// </summary>
        private void ShowBlockRDDDialog()
        {
            Utils.ShowErrList(dwgDir, "Errors Found",
                    "Following errors are blocking RDD export. Please resolve them and try again.", SystemIcons.Error, true);
        }

        /// <summary>
        /// Gets SL meta data (doc id, authors etc.)
        /// </summary>
        /// <param name="error">true if error occurs</param>
        private void GetSigLayout(ref bool error)
        {
            List<Refact.Block> blkSigLayouts = blocks
                                                .Where(x => x.Xtype == XType.SignallingLayout)
                                                .ToList();
            List<SigLayout> sigLays = new List<SigLayout>();
            foreach (var blkSigLayout in blkSigLayouts)
            {

                sigLays.Add(new SigLayout(blkSigLayout, ""));
            }
            sigLays = sigLays
                     .Where(s => s.Attributes != null && s.Attributes.ContainsKey("1-ST.NAVN") &&
                                 s.Attributes["1-ST.NAVN"].value
                                       .Split(new char[] { '(', '-' }, StringSplitOptions.RemoveEmptyEntries).Count() >= 1)
                     .ToList();

            SigLayout foundSig = null;
            string id = "";
            foreach (var testLay in sigLays)
            {
                id = testLay.Attributes["1-ST.NAVN"].value
                                 .Split(new char[] { '(', '-' }, StringSplitOptions.RemoveEmptyEntries)[1]
                                 .Split(new char[] { ')', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries)[0]
                                 .Trim()
                                 .ToLower();
                if (stationsDat.ContainsKey(id.ToUpper()))
                {
                    foundSig = testLay;
                    break;
                }
            }
            if (foundSig == null)
            {
                ErrLogger.Error("Unable to get Stamp from drawing. Station Id not found.", "", "");
                error = true;
                return;
            }

            string tmpCreator = foundSig.Attributes
                      .Where(x => x.Key.Contains("KONSTRUERET") && x.Value.value != "")
                      .Select(y => y.Value.value)
                      .FirstOrDefault();
            if (tmpCreator.Split(null).Length > 0)
            {
                foundSig.Creator = tmpCreator
                    .Split(new char[] { ' ', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
            }
            string inputDate = Regex.Split(foundSig.Attributes["UDGAVE"].value, @"\s{1,}")[1];

            foundSig.Date = Calc.StringToDate(inputDate, out DateTime date, out bool docError);
            error = docError;
            foundSig.Title = foundSig.Attributes["2-TEGN.NAVN"].value + " - " +
                              foundSig.Attributes["1-ST.NAVN"].value;
            //siglayout.title += " (rev. " + Regex.Split(BlkSigLayout.Attributes["UDGAVE"].Value, @"\s{1,}")[0] + ")";
            foundSig.Version = GetDocVers(db, ref docError); //+ " (rev. " + Regex.Split(BlkSigLayout.Attributes["UDGAVE"].Value, @"\s{1,}")[0] + ")"; 
            foundSig.DocId = GetDocId(db, ref docError);
            if (foundSig.DocId != null)
            {
                foundSig.DocId = foundSig.DocId.ToUpper();
            }

            char[] split = new char[] { '-', '(' };
            string stationName =
                foundSig.Attributes["1-ST.NAVN"].value.Split(split)[0].TrimEnd(')').Trim();
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            if (stationName != null)
            {
                stationName = textInfo.ToTitleCase(stationName.ToLower());
            }
            if (id != null)
            {
                foundSig.StID = id;
            }
            foundSig.StName = stationName;
            SigLayout = foundSig;
        }

        /// <summary>
        /// Initializes Railway Elements on SL
        /// </summary>
        private void InitElements()
        {

            Connectors = new List<Connector>();
            Points = new List<elements.Point>();
            Hhts = new List<Hht>();
            EndOfTracks = new List<EndOfTrack>();
            Signals = new List<Signal>();
            DetectionPoints = new List<DetectionPoint>();
            BlockInterfaces = new List<BlockInterface>();
            BaliseGroups = new List<BaliseGroup>();
            AcSections = new List<AcSection>();
            Elements = new List<SLElement>();
            Tsegs = new List<TSeg>();
            enclosedAreas = new List<EnclosedArea>();
            LevelCrossings = new List<LevelCrossing>();
            Pws = new List<Pws>();
            TrustedAreas = new List<TrustedArea>();
            Platforms = new List<Platform>();
            oldPlatforms = new List<OldPlatform>();
            FoulingPoints = new List<FoulingPoint>();

            foreach (var element in this.blocks)
            {
                switch (element.Xtype)
                {
                    case XType.Connector:
                        Connector connector = new Connector(element, SigLayout.StID);
                        Connectors.Add(connector);
                        Elements.Add(connector);
                        break;
                    case XType.Point:
                        elements.Point point = new elements.Point(element, SigLayout.StID);
                        Points.Add(point);
                        Elements.Add(point);
                        break;
                    case XType.EndOfTrack:
                        EndOfTrack endOfTrack = new EndOfTrack(element, SigLayout.StID);
                        EndOfTracks.Add(endOfTrack);
                        Elements.Add(endOfTrack);
                        break;
                    case XType.Signal:
                        Signal signal = new Signal(element, SigLayout.StID);
                        Signals.Add(signal);
                        Elements.Add(signal);
                        break;
                    case XType.DetectionPoint:
                        DetectionPoint dp = new DetectionPoint(element, SigLayout.StID);
                        DetectionPoints.Add(dp);
                        Elements.Add(dp);
                        break;
                    case XType.BlockInterface:
                        BlockInterface bi = new BlockInterface(element, SigLayout.StID);
                        BlockInterfaces.Add(bi);
                        Elements.Add(bi);
                        break;
                    case XType.BaliseGroup:
                        BaliseGroup bg = new BaliseGroup(element, SigLayout.StID);
                        BaliseGroups.Add(bg);
                        Elements.Add(bg);
                        break;
                    case XType.AxleCounterSection:
                        AcSection acSection = new AcSection(element, SigLayout.StID);
                        AcSections.Add(acSection);
                        Elements.Add(acSection);
                        break;
                    case XType.TrackSection:
                        AcSection tSection = new AcSection(element, SigLayout.StID);
                        AcSections.Add(tSection);
                        Elements.Add(tSection);
                        break;
                    case XType.NextStation:
                        EnclosedArea nextSt = new EnclosedArea(element.Xtype, element.BlockReference.GeometricExtents.MinPoint.X,
                            element.BlockReference.GeometricExtents.MinPoint.Y, element.BlockReference.GeometricExtents.MaxPoint.X,
                            element.BlockReference.GeometricExtents.MaxPoint.Y);
                        enclosedAreas.Add(nextSt);
                        break;
                    case XType.ExcludeBlock:
                        EnclosedArea exclude = new EnclosedArea(element.Xtype, element.BlockReference.GeometricExtents.MinPoint.X,
                            element.BlockReference.GeometricExtents.MinPoint.Y, element.BlockReference.GeometricExtents.MaxPoint.X,
                            element.BlockReference.GeometricExtents.MaxPoint.Y);
                        enclosedAreas.Add(exclude);
                        break;
                    case XType.LevelCrossing:
                        LevelCrossing levelCrossing = new LevelCrossing(element, SigLayout.StID);
                        LevelCrossings.Add(levelCrossing);
                        Elements.Add(levelCrossing);
                        break;
                    case XType.StaffPassengerCrossing:
                        Pws pws = new Pws(element, SigLayout.StID);
                        Pws.Add(pws);
                        Elements.Add(pws);
                        break;
                    case XType.Hht:
                        Hht hht = new Hht(element);
                        Hhts.Add(hht);
                        break;
                    case XType.PlatformDyn:
                        Platform platform = new Platform(element, SigLayout.StID);
                        Platforms.Add(platform);
                        Elements.Add(platform);
                        break;
                    case XType.Platform:
                        OldPlatform Oldplatform = new OldPlatform(element, SigLayout.StID);
                        oldPlatforms.Add(Oldplatform);
                        break;
                    case XType.FoulingPoint:
                        FoulingPoint foulingPoint = new FoulingPoint(element, SigLayout.StID);
                        FoulingPoints.Add(foulingPoint);
                        Elements.Add(foulingPoint);
                        break;
                }
            }
            TsegVertexes = new List<SLElement>();
            TsegVertexes.AddRange(Points.OrderBy(x => x.Location));
            TsegVertexes.AddRange(Connectors.OrderBy(x => x.Location));
            TsegVertexes.AddRange(EndOfTracks.OrderBy(x => x.Location));
        }

        /// <summary>
        /// Processes collected Railway Elements
        /// </summary>
        /// <param name="error">true if error occurs</param>
        private void ProcessElements(ref bool error)
        {
            foreach (SLElement sLElement in Elements.Where(x => x.ElType != XType.AxleCounterSection &&
                                                                x.ElType != XType.TrackSection &&
                                                                x.ElType != XType.BlockInterface &&
                                                                x.ElType != XType.FoulingPoint))
            {
                SetTrackLineAndId(sLElement, ref error);
            }
            foreach (Signal signal in Signals)
            {
                error = signal.SetSignalDirection();
            }
            foreach (elements.Point point in Points)
            {
                if (!point.SetPointRightLeftTrLines(trackLines))
                {
                    error = true;
                }
                if (!point.SetLinesId())
                {
                    error = true;
                }
                point.SetHht(Hhts);
            }
            //foreach (LevelCrossing lx in LevelCrossings)
            //{
            //    lx.TrType = lx.GetTrType(trackLines);
            //}
            //foreach (Pws pws in Pws)
            //{
            //    pws.TrType = pws.GetTrType(trackLines);
            //}
        }

        /// <summary>
        /// Assigns Track Lines to Railway Elements
        /// </summary>
        /// <param name="element">Railway element</param>
        /// <param name="error">true if error occurs</param>
        private void SetTrackLineAndId(SLElement element, ref bool error)
        {
            string lineId = "";
            TrackLine line = trackLines
                             .Where(x => AcadTools.ObjectsIntersects(x.line, element.Block.BlockReference, Intersect.OnBothOperands))
                             .FirstOrDefault();
            if (line == null)
            {
                ErrLogger.Error("Unable to get Track Line for element", element.Designation, "");
                error = true;
                return;
            }
            else
            {
                element.TrackLine = line;
            }

            lineId = RailwayLines.Where(x => x.color == line.color)
                                 .Select(y => y.Designation)
                                 .FirstOrDefault();
            if (lineId == null)
            {
                ErrLogger.Error("Unable to get Line Id for element", element.Designation, "");
                error = true;
            }
            element.LineID = lineId;
            if (element.ElType == XType.Connector)
            {
                ((Connector)element).Branches = trackLines
                                            .Where(x => AcadTools
                                                       .PointsAreEqual(x.line.GetClosestPointTo(element.Position, false),
                                                                            element.Position))
                                            .ToList();
            }
            else if (element.ElType == XType.EndOfTrack)
            {
                ((EndOfTrack)element).Branches = trackLines
                                            .Where(x => AcadTools
                                                  .PointsAreEqual(element.Position, x.line.GetClosestPointTo(element.Position, false)))
                                            .ToList();
            }
        }

        /// <summary>
        /// Gets Document Id
        /// </summary>
        /// <param name="db">Acad drawing database</param>
        /// <param name="error">true if error occurs</param>
        /// <returns>document id if found null otherwise</returns>
        private string GetDocId(Database db, ref bool error)
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                var Textsids = AcadTools.GetObjectsOfType(db, RXObject.GetClass(typeof(MText)));
                foreach (ObjectId ObjId in Textsids)
                {
                    var mtext = (MText)trans.GetObject(ObjId, OpenMode.ForRead);
                    if (mtext.Text.Contains("Internt Thales tegningsnr") ||
                        mtext.Text.Contains("Leverandør nr"))
                    {
                        return mtext.Text.Split(':')[1].Trim().Split(new char[0], StringSplitOptions.RemoveEmptyEntries)[0];
                    }
                }
                Textsids = AcadTools.GetObjectsOfType(db, RXObject.GetClass(typeof(DBText)));
                foreach (ObjectId ObjId in Textsids)
                {
                    var text = (DBText)trans.GetObject(ObjId, OpenMode.ForRead);
                    if (text.TextString.Contains("Internt Thales tegningsnr") ||
                        text.TextString.Contains("Leverandør nr")) // Internt Thales tegningsnr
                    {
                        return text.TextString.Split(':')[1].Trim().Split(new char[0], StringSplitOptions.RemoveEmptyEntries)[0];
                    }
                }
                trans.Commit();
            }
            ErrLogger.Error("Unable to get document Id from drawing", "", "");
            error = true;
            return null;
        }

        /// <summary>
        /// Gets Document version
        /// </summary>
        /// <param name="db">Acad drawing database</param>
        /// <param name="error">true if error occurs</param>
        /// <returns>document version if found null otherwise</returns>
        private string GetDocVers(Database db, ref bool error)
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                var Textsids = AcadTools.GetObjectsOfType(db, RXObject.GetClass(typeof(MText)));
                foreach (ObjectId ObjId in Textsids)
                {
                    var mtext = (MText)trans.GetObject(ObjId, OpenMode.ForRead);
                    if (mtext.Text.Contains("Internt Thales tegningsnr") ||
                        mtext.Text.Contains("Leverandør nr")) //Leverandør nr
                    {
                        return mtext.Text.Split(':')[1].Trim()
                            .Split(new Char[] { ' ', ',', '.', ':', '\n', '\t', '/', 'v', 'V' },
                                       StringSplitOptions.RemoveEmptyEntries).Last();
                    }
                }
                Textsids = AcadTools.GetObjectsOfType(db, RXObject.GetClass(typeof(DBText)));
                foreach (ObjectId ObjId in Textsids)
                {
                    var text = (DBText)trans.GetObject(ObjId, OpenMode.ForRead);
                    if (text.TextString.Contains("Internt Thales tegningsnr") ||
                        text.TextString.Contains("Leverandør nr")) // Internt Thales tegningsnr
                    {
                        return text.TextString.Split(':')[1].Trim()
                            .Split(new Char[] { ' ', ',', '.', ':', '\n', '\t', '/', 'v', 'V' },
                                       StringSplitOptions.RemoveEmptyEntries).Last();
                    }
                }
                trans.Commit();
            }
            ErrLogger.Error("Unable to get document Version from drawing", "", "");
            error = true;
            return null;
        }

        /// <summary>
        /// Gets Track Lines from SL.
        /// </summary>
        /// <returns>List of found Track Lines</returns>
        private List<TrackLine> GetTracksLines()
        {
            List<TrackLine> TracksLines = new List<TrackLine>();
            foreach (Line line in AcadTools.GetLinesByLayer(Constants.trackLinesLayer, db, Constants.minTrLineLength))
            {
                Color color;
                if (line.Color.ColorMethod == Autodesk.AutoCAD.Colors.ColorMethod.ByLayer)
                {
                    LayerTableRecord layer = AcadTools.GetLayerById(line.LayerId, db);
                    color = Color.FromArgb(layer.Color.ColorValue.A,
                                               layer.Color.ColorValue.R,
                                               layer.Color.ColorValue.G,
                                               layer.Color.ColorValue.B);
                }
                else
                {
                    color = Color.FromArgb(line.Color.ColorValue.A,
                                               line.Color.ColorValue.R,
                                               line.Color.ColorValue.G,
                                               line.Color.ColorValue.B);
                }
                TracksLines.Add(new TrackLine
                {
                    line = line,
                    color = color
                });
            }
            return TracksLines;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="error">true if error occurs</param>
        /// <returns>List of found railway lines</returns>
        private List<RailwayLine> GetRailwayLines(ref bool error)
        {
            List<RailwayLine> railwayLines = new List<RailwayLine>();
            RailwayLine DefaultRailwayLine = new RailwayLine();
            List<DBText> linesTexts = new List<DBText>();
            List<MText> linesMTexts = new List<MText>();

            string tmpLine = stationsDat
                             .Where(x => x.Key.ToLower() == SigLayout.StID)
                             .Select(x => x.Value)
                             .FirstOrDefault();
            string DefaultLine = linesDat
                                 .Where(x => x.Key == tmpLine.Split('\t')[1])
                                 .Select(x => x.Value)
                                 .FirstOrDefault();

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                var Txts = AcadTools.GetObjectsOfType(db, RXObject.GetClass(typeof(DBText)));
                foreach (ObjectId ObjId in Txts)
                {
                    DBText text = (DBText)trans.GetObject(ObjId, OpenMode.ForRead);
                    if (text.TextString.Contains("TIB"))
                    {
                        linesTexts.Add(text);
                    }
                }
                var MTxts = AcadTools.GetObjectsOfType(db, RXObject.GetClass(typeof(MText)));
                foreach (ObjectId ObjId in MTxts)
                {
                    MText text = (MText)trans.GetObject(ObjId, OpenMode.ForRead);
                    if (text.Text.Contains("km line"))
                    {
                        linesMTexts.Add(text);
                    }
                }
                trans.Commit();
            }

            if (linesTexts.Count == 0 && linesMTexts.Count == 0)
            {
                if (DefaultLine != null)
                {
                    DefaultRailwayLine.Designation = DefaultLine.Split('\t')[0];
                    DefaultRailwayLine.start = DefaultLine.Split('\t')[1];
                    DefaultRailwayLine.end = DefaultLine.Split('\t')[2];
                    DefaultRailwayLine.direction =
                        (DirectionType)Enum.Parse(typeof(DirectionType), DefaultLine.Split('\t')[3]);
                    DefaultRailwayLine.color = trackLines
                                               .Select(x => x.color)
                                               .Distinct()
                                               .FirstOrDefault();
                    railwayLines.Add(DefaultRailwayLine);
                    foreach (var trackline in trackLines)
                    {
                        trackline.direction = DefaultRailwayLine.direction;
                        trackline.LineID = DefaultRailwayLine.Designation;
                    }
                }
                else
                {
                    ErrLogger.Error("Default line for station not found in stations.dat", "SL", SigLayout.StName);
                    error = true;
                }
                return railwayLines.OrderBy(x => x.Designation).ToList();
            }

            if (linesTexts.Count > 0)
            {
                foreach (TrackLine trackline in trackLines)
                {
                    var test = trackline.line.ObjectId.ToString();
                    DBText text = linesTexts
                                  .Where(x => AcadTools.ObjectsIntersects(trackline.line, x, Intersect.ExtendThis) &&
                                 (trackline.line.GetClosestPointTo(x.AlignmentPoint, false) - x.AlignmentPoint).Length <= 30)
                                  .FirstOrDefault();
                    if (text != null)
                    {
                        RailwayLine railwayLine = new RailwayLine
                        {
                            Designation = Regex.Replace(text.TextString, "[^.0-9]", ""),
                            color = trackline.color
                        };
                        string line = linesDat
                                      .Where(x => x.Key == Regex.Replace(text.TextString, "[^.0-9]", ""))
                                      .Select(x => x.Value)
                                      .FirstOrDefault();
                        if (line != null)
                        {
                            railwayLine.start = line.Split('\t')[1];
                            railwayLine.end = line.Split('\t')[2];
                            railwayLine.direction =
                                (DirectionType)Enum.Parse(typeof(DirectionType), line.Split('\t')[3]);
                        }
                        railwayLines.Add(railwayLine);
                    }
                }
            }
            if (linesMTexts.Count > 0)
            {
                foreach (MText text in linesMTexts)
                {
                    RailwayLine railwayLine = new RailwayLine
                    {
                        Designation = Regex.Replace(text.Text, "[^.0-9]", ""),
                    };
                    string line = linesDat
                                  .Where(x => x.Key == railwayLine.Designation)
                                  .Select(x => x.Value)
                                  .FirstOrDefault();
                    if (line != null)
                    {
                        railwayLine.start = line.Split('\t')[1];
                        railwayLine.end = line.Split('\t')[2];
                        railwayLine.direction =
                            (DirectionType)Enum.Parse(typeof(DirectionType), line.Split('\t')[3]);
                    }
                    Color color;
                    if (text.Color.ColorMethod == Autodesk.AutoCAD.Colors.ColorMethod.ByLayer)
                    {
                        using (Transaction trans = db.TransactionManager.StartTransaction())
                        {
                            LayerTableRecord layer =
                                (LayerTableRecord)trans.GetObject(text.LayerId, OpenMode.ForRead);
                            color = Color.FromArgb(layer.Color.ColorValue.A,
                                                   layer.Color.ColorValue.R,
                                                   layer.Color.ColorValue.G,
                                                   layer.Color.ColorValue.B);
                            trans.Commit();
                        }
                    }
                    else
                    {
                        color = Color.FromArgb(text.Color.ColorValue.A,
                                                   text.Color.ColorValue.R,
                                                   text.Color.ColorValue.G,
                                                   text.Color.ColorValue.B);
                    }
                    railwayLine.color = color;
                    railwayLines.Add(railwayLine);
                }
            }
            foreach (var railwayLine in railwayLines)
            {
                foreach (var trackLine in trackLines.Where(x => x.color == railwayLine.color))
                {
                    trackLine.direction = railwayLine.direction;
                    trackLine.LineID = railwayLine.Designation;
                }
            }
            if (railwayLines.Count != linesTexts.Count + linesMTexts.Count)
            {
                string lines = 
                    string.Join(",", linesTexts
                                     .Select(x => x.TextString).ToList()
                                     .Union(linesMTexts.Select(x => x.Text).ToList())
                                );
                ErrLogger.Error("Railway lines mismatch on SL. Lines definitions(TIB) found: " + lines + "Colored Track Lines found: " +
                                string.Join(",", railwayLines.Select(x => x.Designation)), "SL",""); 
                error = true;
            }
            return railwayLines.OrderBy(x => x.Designation).ToList();
        }

        /// <summary>
        /// Sets 'Next station' and 'Exclude' properties to
        /// railway elements.
        /// </summary>
        private void SetNextExclude()
        {
            var elementsNextStat = Elements.Where(x => enclosedAreas.Any(a => x.Block.BlockReference.Position.X >= a.MinX &&
                                                                  x.Block.BlockReference.Position.X <= a.MaxX &&
                                                                  x.Block.BlockReference.Position.Y >= a.MinY &&
                                                                  x.Block.BlockReference.Position.Y <= a.MaxY &&
                                                                  a.ElType == XType.NextStation));
            foreach (var item in elementsNextStat)
            {
                item.NextStation = true;
            }

            var elementsExclude = Elements.Where(x => enclosedAreas.Any(a => x.Block.BlockReference.Position.X >= a.MinX &&
                                                                  x.Block.BlockReference.Position.X <= a.MaxX &&
                                                                  x.Block.BlockReference.Position.Y >= a.MinY &&
                                                                  x.Block.BlockReference.Position.Y <= a.MaxY &&
                                                                  a.ElType == XType.ExcludeBlock));
            foreach (var item in elementsExclude)
            {
                item.Exclude = true;
            }

        }

        /// <summary>
        /// Gets PSA enclosing areas.
        /// </summary>
        /// <returns>List of found areas</returns>
        private IEnumerable<Psa> GetPsas()
        {
            List<Psa> psas = new List<Psa>();
            List<Polyline> polylines = new List<Polyline>();
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {              
                var PolyLineIds = AcadTools.GetObjectsOfType(db, RXObject.GetClass(typeof(Polyline)));
                foreach (ObjectId ObjId in PolyLineIds)
                {
                    polylines.Add((Polyline)trans.GetObject(ObjId, OpenMode.ForRead));
                }
            }
            foreach (MText mtext in AcadTools.GetMtextsByRegex(".*PSA.*", db))
            {
                Polyline polylinePsa = polylines
                        .Where(x => mtext.GeometricExtents.MinPoint.X >= x.GeometricExtents.MinPoint.X &&
                                    mtext.GeometricExtents.MinPoint.X <= x.GeometricExtents.MaxPoint.X &&
                                    mtext.GeometricExtents.MinPoint.Y >= x.GeometricExtents.MinPoint.Y &&
                                    mtext.GeometricExtents.MinPoint.Y <= x.GeometricExtents.MaxPoint.Y &&
                                    x.Layer == "KMP")
                         .FirstOrDefault();
                if (polylinePsa != null)
                {
                    yield return new Psa
                    (
                        mtext.Text,
                        polylinePsa.GeometricExtents.MinPoint.X,
                        polylinePsa.GeometricExtents.MinPoint.Y,
                        polylinePsa.GeometricExtents.MaxPoint.X,
                        polylinePsa.GeometricExtents.MaxPoint.Y,
                        polylinePsa
                    );
                }
            }
        }

        /// <summary>
        /// Sets Psa Id property to railway elements.
        /// </summary>
        private void InitPsas()
        {
            foreach (var psa in Psas)
            {
                var elementsPSA = Elements.Where(x => x.Block.BlockReference.Position.X >= psa.MinX + Constants.insidePsaToler &&
                                                      x.Block.BlockReference.Position.X <= psa.MaxX - Constants.insidePsaToler &&
                                                      x.Block.BlockReference.Position.Y >= psa.MinY + Constants.insidePsaToler &&
                                                      x.Block.BlockReference.Position.Y <= psa.MaxY - Constants.insidePsaToler);
                foreach (var item in elementsPSA)
                {
                    item.InsidePSA = psa.Id;
                }
                TrackLine trLinePsa = trackLines
                           .Where(x => AcadTools.ObjectsIntersects(x.line, psa.Polyline, Intersect.OnBothOperands))
                           .FirstOrDefault();
                if (trLinePsa != null)
                {
                    Point3dCollection intersections = new Point3dCollection();
                    trLinePsa.line.IntersectWith(psa.Polyline, Intersect.OnBothOperands, intersections, IntPtr.Zero, IntPtr.Zero);
                    if (intersections.Count > 0)
                    {
                        SLElement beginElement = Elements
                                      .Where(x => AcadTools.PointsAreEqual(x.Position, intersections[0]))
                                      .FirstOrDefault();
                        if (beginElement != null)
                        {
                            psa.Begin = beginElement.Location;
                            psa.Tseg = beginElement.Tseg;
                        }
                        else 
                        {
                            ErrLogger.Error("Unable to get Begin and TSeg of PSA.", psa.Id, "");
                            error = true;
                        }
                    }                          
                }
                           
            }
            foreach (TSeg tseg in Tsegs)
            {
                if (!string.IsNullOrEmpty(tseg.Vertex1.Element.InsidePSA) && 
                    !string.IsNullOrEmpty(tseg.Vertex2.Element.InsidePSA) &&
                    tseg.Vertex1.Element.InsidePSA == tseg.Vertex2.Element.InsidePSA)
                {
                    tseg.InsidePSA = tseg.Vertex1.Element.InsidePSA;
                }
            }
        }

        /// <summary>
        /// Calculates Track segments in increasing millage direction.
        /// </summary>
        /// <param name="error">true if error occurs</param>
        /// <returns>List of calculated Track segments</returns>
        private List<TSeg> GetTsegs(ref bool error)
        {
            List<TSeg> tsegs = new List<TSeg>();
            List<string> NodesCheck = new List<string>();
            List<TrackLine> skipLines = new List<TrackLine>();
#if DEBUG
            ErrLogger.Information("begin", "Track Segments");
#endif
            foreach (SLElement node in TsegVertexes.OrderBy(x => x.Location))
            {
                foreach (TrackLine branch in node.Branches.Where(x => !skipLines.Contains(x)))
                {
                    elements.Vertex vertex1 = new elements.Vertex(node, branch, VertNumber.start);
                    string vertUnigueId = vertex1.UniqueId();
                    if (node.ElType == XType.Connector &&
                        node.Branches.Count > 1 && 
                        node.Branches[0].LineID != node.Branches[1].LineID)
                    {
                        if (NodesCheck.Contains(vertUnigueId + "1"))
                        {
                            vertUnigueId = vertex1.UniqueId() + "2";
                            vertex1.Km = ((Connector)vertex1.Element).Kmp2;
                        }
                        else
                        {
                            vertUnigueId = vertex1.UniqueId() + "1";
                        }
                    }
                    if (NodesCheck.Contains(vertUnigueId))
                    {
                        continue;
                    }
                    NodesCheck.Add(vertUnigueId);

                    if (vertex1.Element.ElType == XType.Point && branch.direction == DirectionType.up &&
                        ((elements.Point)vertex1.Element).Orient == LeftRightType.left && vertex1.Conn != ConnectionBranchType.tip)
                    {
                        continue;
                    }
                    if (vertex1.Element.ElType == XType.Point && branch.direction == DirectionType.down &&
                        ((elements.Point)vertex1.Element).Orient == LeftRightType.right && vertex1.Conn != ConnectionBranchType.tip)
                    {
                        continue;
                    }

                    elements.Vertex vertex2 = GetVertex2(branch, vertex1, out List<TrackLine> TrackLinesOnSegment, out error);
                    if (vertex2 == null)
                    {
                        continue;
                    }
                    skipLines.AddRange(TrackLinesOnSegment);
                    NodesCheck.Add(vertex2.UniqueId());
                    TSeg tseg = new TSeg
                    {
                        Id = vertex1.GetTsegId(),
                        Vertex1 = vertex1,
                        Vertex2 = vertex2,
                        TrackLines = TrackLinesOnSegment,
                        LineID = branch.LineID,
                        LineDirection = branch.direction,
                    };
                    tsegs.Add(tseg);
#if DEBUG
                    ErrLogger.Information(vertUnigueId + "----->" + tseg.Vertex2.UniqueId(), tseg.Id);
#endif
                }
            }
#if DEBUG
            ErrLogger.Information("end", "Track Segments");
#endif
            if (tsegs.Count == 0)
            {
                ErrLogger.Error("No Track segments found", SigLayout.StID, "");
                error = true;
                this.error = true;
            }
            return tsegs;
        }


        /// <summary>
        /// Gets second Vertex of Track segment.
        /// </summary>
        /// <param name="branch">Track line start from</param>
        /// <param name="vertex1">Vertex1 of Track segment</param>
        /// <param name="trackLinesTseg">collected Track lines belonging to Tseg</param>
        /// <param name="error">true if error occurs</param>
        /// <returns>Vertex2 if found, null otherwise</returns>
        private elements.Vertex GetVertex2(TrackLine branch, elements.Vertex vertex1, out List<TrackLine> trackLinesTseg, out bool error)
        {
            error = false;
            SLElement element = null;
            TrackLine trackLineTmp = branch;
            trackLinesTseg = new List<TrackLine>();
            List<SLElement> tempVertexes = TsegVertexes
                .Where(x => x.Location > vertex1.Km)
                .ToList();
            Point3d acStartPt = trackLineTmp.line.GetClosestPointTo(vertex1.Element.Position, false);
            int limit = 0;
            while (element == null)
            {
                if (limit >= Constants.nextNodeMaxAttemps)
                {
                    break;
                }
                limit++;
                acStartPt = trackLineTmp.line.GetClosestPointTo(acStartPt, false);
                element = tempVertexes
                      .Where(x => x.Branches.Contains(trackLineTmp) &&
                                  x != vertex1.Element)
                      .OrderBy(o => o.Location)
                      .FirstOrDefault();
                trackLinesTseg.Add(trackLineTmp);
                if (element == null)
                {                
                    trackLineTmp = GetNextTmpLines(trackLineTmp, trackLinesTseg, acStartPt)
                                   .FirstOrDefault();
                    if (trackLineTmp == null)
                    {
                        break;
                    }
                }
                else
                {
                    return new elements.Vertex(element, trackLineTmp, VertNumber.end);
                }
            };
            if (vertex1.Element.Exclude || vertex1.Element.NextStation)
            {
                ErrLogger.Information("Track lines path not found from Vertex1.", vertex1.Id + "-" + vertex1.Conn.ToString());
            }
            else
            {
                ErrLogger.Error("Track lines path not found from Vertex1.", vertex1.Id + "-" + vertex1.Conn.ToString(), "");
                error = true;
            }
            return null;
        }

        /// <summary>
        /// Gets Track Lines related to <paramref name="branch"/> Track line.
        /// </summary>
        /// <param name="branch">Initial track line</param>
        /// <param name="trackLinesSkip">Already processed lines to be ignored</param>
        /// <param name="startPt">Start point of <paramref name="branch"/> line</param>
        /// <returns>List of found track lines ordered by distance from <paramref name="startPt"/></returns>
        private List<TrackLine> GetNextTmpLines(TrackLine branch, List<TrackLine> trackLinesSkip, Point3d startPt)
        {
            List<TrackLine> trackLinesTmp = new List<TrackLine>();
            foreach (TrackLine trackLine in trackLines)
            {
                if (!trackLinesSkip.Contains(trackLine) &&
                    AcadTools.LinesHasSameStartEnd(trackLine.line, branch.line, out Point3d ptCheck))
                {
                    if (!AcadTools.PointsAreEqual(startPt, ptCheck))
                    {
                        trackLinesTmp.Add(trackLine);
                    }
                }
            }
            if (trackLinesTmp.Count() > 0)
            {
                return trackLinesTmp
                       .OrderBy(x => (x.line.GetPointAtDist(x.line.Length / 2) - startPt).Length)
                       .ToList();
            }
            Point3d branchPt = branch.line.GetPointAtDist(branch.line.Length / 2);

            trackLinesTmp = trackLines
                       .Where(x => AcadTools.LineBelongsToBeam(branch.line, x.line) &&
                                   (Calc.Between(AcadTools
                                                 .GetBeamAngleToPoint(startPt, branchPt, x.line.StartPoint),
                                                0, Math.PI / 2, true) ||
                                    Calc.Between(AcadTools
                                                 .GetBeamAngleToPoint(startPt, branchPt, x.line.StartPoint),
                                                3 * Math.PI / 2, 2 * Math.PI, true)) &&
                                    (Calc.Between(AcadTools
                                                 .GetBeamAngleToPoint(startPt, branchPt, x.line.EndPoint),
                                                0, Math.PI / 2, true) ||
                                    Calc.Between(AcadTools
                                                 .GetBeamAngleToPoint(startPt, branchPt, x.line.EndPoint),
                                                3 * Math.PI / 2, 2 * Math.PI, true)) &&
                                   !trackLinesSkip.Contains(x) &&
                                   !trackLinesTmp.Contains(x))
                       .OrderBy(x => (x.line.GetPointAtDist(x.line.Length / 2) - startPt).Length)
                       .ToList();
            return trackLinesTmp;
        }

        /// <summary>
        /// Sets Tseg Id property to Elements.
        /// </summary>
        /// <param name="error">true if error occurs</param>
        private void SetTsegIdToElements(ref bool error)
        {
#if DEBUG
            ErrLogger.Information("begin", "Elements on Tseg");
#endif
            foreach (TSeg tseg in Tsegs)
            {
#if DEBUG
                ErrLogger.Information("", tseg.Id);
#endif
                List<SLElement> elemsForTsegId = Elements
                                                .Where(x => (x.ElType != XType.Point &&
                                                             x.ElType != XType.Connector &&
                                                             x.ElType != XType.EndOfTrack) &&
                                                             tseg.TrackLines.Contains(x.TrackLine) &&
                                                             Calc.Between(x.Location, tseg.Vertex1.Km, tseg.Vertex2.Km, true))
                                                .ToList();
                foreach (SLElement sLElement in elemsForTsegId)
                {
                    sLElement.Tseg = tseg;
#if DEBUG
                    ErrLogger.Information(sLElement.Designation, "");
#endif
                }
            }
            List<SLElement> elemsNotFoundTsegId = Elements
                                               .Where(x => (x.ElType != XType.Point &&
                                                            x.ElType != XType.Connector &&
                                                            x.ElType != XType.EndOfTrack &&
                                                            x.ElType != XType.LevelCrossing &&
                                                            x.ElType != XType.StaffPassengerCrossing &&
                                                            x.ElType != XType.AxleCounterSection &&
                                                            x.ElType != XType.TrackSection &&
                                                            x.ElType != XType.Platform &&
                                                            x.ElType != XType.PlatformDyn &&
                                                            x.ElType != XType.FoulingPoint &&
                                                            x.ElType != XType.BlockInterface)  &&
                                                            x.Tseg == null)
                                               .ToList();
            foreach (SLElement element in elemsNotFoundTsegId)
            {
                ErrLogger.Error("Element without Tseg Id found.", element.Designation, "");
                error = true;
            }
#if DEBUG
            ErrLogger.Information("end", "Elements on Tseg");
#endif
        }

        /// <summary>
        /// Adds detection points and elements(Points) to Ac section elements: <see cref="AcSections"/>.
        /// For Ac sections next to End of Track the EOT element is added to list.
        /// </summary>
        /// <param name="error">true if error occurs</param>
        private void InitAcSections(ref bool error)
        {
#if DEBUG
            ErrLogger.Information("begin", "Ac sections");
#endif
            foreach (AcSection section in AcSections) 
            {
                List<TrackLine> acTrlines = GetAcInitLines(section, out Point3d acMidPt, out error);
                if (error)
                {
                    continue;
                }
                List<SLElement> dps = new List<SLElement>();
                List<SLElement> sectionElements = new List<SLElement>();
                List<SLElement> skipElements = new List<SLElement>();
                List<TrackLine> trackLinesSkip = new List<TrackLine>();
                Point3d acStartPt = acTrlines[0].line.GetClosestPointTo(acMidPt, false);

                Stack<Stack<TrackLine>> trLinesStack = new Stack<Stack<TrackLine>>();
                trLinesStack.Push(new Stack<TrackLine>(acTrlines));
                List<Type> types = new List<Type> 
                {
                    typeof(DetectionPoint),
                    typeof(EndOfTrack),
                    typeof(Connector),
                    typeof(elements.Point)
                };
                int limit = 0;
                while (trLinesStack.Count > 0)
                {
                    if (limit >= Constants.dpIterLimit)
                    {
                        ErrLogger.Error("Unable to found detection point for Ac section.", section.Designation, "");
                        error = true;
                        trLinesStack = TrLineStackPop(trLinesStack, out int back);
                        limit -= back;
                    }
                    limit++;
                    acStartPt = trLinesStack.Peek().Peek().line.GetClosestPointTo(acStartPt, false);
                    SLElement element = GetFirstElemenOnTrLine(trLinesStack.Peek().Peek(),
                                                                acStartPt, types, skipElements);
                    if (element == null)
                    {
                        trackLinesSkip.Add(trLinesStack.Peek().Peek());
                        acTrlines = GetNextTmpLines(trLinesStack.Peek().Peek(), trackLinesSkip, acStartPt);
                        if (acTrlines.Count == 0)
                        {
                            trLinesStack = TrLineStackPop(trLinesStack, out int back);
                            ErrLogger.Error("Unable to found detection point for Ac section.", section.Designation, "");
                            error = true;
                        }
                        else
                        {
                            trLinesStack.Push(new Stack<TrackLine>(acTrlines));
                        }                     
                    }
                    else
                    if (element.ElType == XType.DetectionPoint ||
                        element.ElType == XType.EndOfTrack)
                    {
                        dps.Add(element);
                        trLinesStack = TrLineStackPop(trLinesStack, out int back);
                        limit -= back;
                        skipElements.Add(element);
                    }
                    else
                    if(element.ElType == XType.Connector ||
                            element.ElType == XType.Point )
                    {
                        acTrlines = element.Branches
                                    .Where(x => x != trLinesStack.Peek().Peek())
                                    .ToList();
                        trLinesStack.Push(new Stack<TrackLine>(acTrlines));
                        skipElements.Add(element);
                        if (element.ElType == XType.Point && 
                            element.ExtType != ExtType.derailer)
                        {
                            sectionElements.Add(element);
                        }
                    }
                };
                section.DetectionPoints = dps;
                if (sectionElements.Count == 0)
                {
                    sectionElements.Add(section);
                }
                section.Elements = sectionElements;
                section.StName = SigLayout.StName;

#if DEBUG
                ErrLogger.Information(section.Tseg + " " + string.Join(", ", dps.Select(x => x.Designation)) , section.Designation);
#endif
            }
#if DEBUG
            ErrLogger.Information("end", "Ac sections");
#endif
        }

        private void CheckAcTrackSections()
        {
            var ac = AcSections.Where(x => x.DetectionPoints.Count > 2);
            var tr = AcSections.Where(x => x.DetectionPoints.Count == 2);
            if (AcSections.Count != ac.Count() + tr.Count())
            {
                ErrLogger.Error("Ac Sections TrackSections inconsistence found", "", "");
                error = true;
            }
        }

        /// <summary>
        /// Pops Track Lines stack until branch.
        /// </summary>
        /// <param name="stack">Track Lines</param>
        /// <param name="steps">count of performed pops</param>
        /// <returns>result stack</returns>
        private Stack<Stack<TrackLine>> TrLineStackPop(Stack<Stack<TrackLine>> stack, out int steps)
        {
            steps = 0;
            while (stack.Count > 0 && stack.Peek().Count != 2)
            {
                stack.Pop();
                steps++;
            }
            if (stack.Count > 0 && stack.Peek().Count == 2)
            {
                stack.Peek().Pop();
            }
            return stack;
        }

        /// <summary>
        /// Pops Track segments stack until branch.
        /// </summary>
        /// <param name="stack">Track segments</param>
        /// <param name="steps">count of performed pops</param>
        /// <returns>result stack</returns>
        private Stack<Stack<TSeg>> TsegsStackPop(Stack<Stack<TSeg>> stack, out int steps)
        {
            steps = 0;
            while (stack.Count > 0 && stack.Peek().Count != 2)
            {
                stack.Pop();
                steps++;
            }
            if (stack.Count > 0 && stack.Peek().Count == 2)
            {
                stack.Peek().Pop();
            }
            return stack;
        }

        /// <summary>
        /// Gets closest Track line/lines for Ac/Track section Element.
        /// </summary>
        /// <param name="section">Ac section element</param>
        /// <param name="acMidPt">Middle point of Ac/Track Section Text</param>
        /// <param name="error">true if error occurs</param>
        /// <returns></returns>
        private List<TrackLine> GetAcInitLines(AcSection section, out Point3d acMidPt, out bool error)
        {
            error = false;
            acMidPt = new Point3d();
            Point3d acMidPtTmp = AcadTools.GetMiddlPoint3d(section.Block.BlockReference.GeometricExtents);
            List<TrackLine> closLines = trackLines
                            .Where(x => (x.line.GetClosestPointTo(acMidPtTmp, false) - acMidPtTmp).Length <= 5)
                            .OrderBy(x => (x.line.GetClosestPointTo(acMidPtTmp, false) - acMidPtTmp).Length)
                            //.Take(2)
                            .ToList();
            acMidPt = acMidPtTmp;
            if (closLines.Count == 0)
            {
                ErrLogger.Error("Unable to get Ac section initial track lines.", section.Designation, "");
                error = true;
                return null;
            }
            else if (closLines.Count == 1)
            {
                Point3d acStartPt = closLines[0].line.GetClosestPointTo(acMidPt, false);
                List<Line> splitLines = AcadTools.SplitLineOnPoint(closLines[0].line, acStartPt);
                if (splitLines.Count != 2)
                {
                    ErrLogger.Error("Unable to get Ac section initial track lines.", section.Designation, "");
                    error = true;
                    return null;
                }
                List<TrackLine> splitTrLines = new List<TrackLine>();
                foreach (Line line in splitLines)
                {
                    splitTrLines.Add(new TrackLine
                    {
                        color = closLines[0].color,
                        direction = closLines[0].direction,
                        line = line,
                        LineID = closLines[0].LineID
                    });
                }
                return splitTrLines;
            }
            else if (closLines.Count > 2)
            {
                return closLines.Take(2).ToList();
            }
            return closLines;
        }

        /// <summary>
        /// Gets closest element on Track lines starting from <paramref name="ptStart"/>.
        /// </summary>
        /// <param name="trackLine">Track line get element on</param>
        /// <param name="ptStart">Point calculate closest from</param>
        /// <param name="types">Types of Elements to look for</param>
        /// <param name="exclude">List of elements to be excluded from search</param>
        /// <returns>Closest element if found otherwise null</returns>
        private SLElement GetFirstElemenOnTrLine(TrackLine trackLine, Point3d ptStart, List<Type> types, 
            List<SLElement> exclude) 
        {
            SLElement element = null;
            element = Elements
                      .Where(x => (AcadTools.PointsAreEqual(trackLine.line.GetClosestPointTo(x.Position, false), x.Position) ||
                                   AcadTools.ObjectsIntersects(trackLine.line, x.Block.BlockReference, Intersect.OnBothOperands)) &&
                                  types.Contains(x.GetType()) &&
                                  !exclude.Contains(x))
                      .OrderBy(o => (ptStart - o.Position).Length)
                      .FirstOrDefault();              
            return element;
        }

        /// <summary>
        /// Splits Track Line on Vertex Position to distinguish tip and branches on points
        /// and search directions on connectors.
        /// </summary>
        /// <remarks>
        /// If line is already is split it is ignored.
        /// Split Lines are stored in memory during calculation and will be not saved into drawing.
        /// </remarks>
        /// <param name="error"></param>
        private void SplitLinesOnVertexes(out bool error)
        {
            error = false;
            foreach (SLElement element in TsegVertexes.Where(x => x.ElType != XType.EndOfTrack))
            {
                TrackLine trackLine = trackLines
                                      .Where(x => AcadTools
                                              .ObjectsIntersects(x.line, element.Block.BlockReference, Intersect.OnBothOperands))
                                      .OrderBy(o => (o.line.GetClosestPointTo(element.Position, false) - element.Position).Length)
                                      .FirstOrDefault();
                
                if (trackLine == null)
                {
                    trackLine = trackLines
                                .Where(x => AcadTools.PointsAreEqual(x.line.GetClosestPointTo(element.Position, false),
                                                                            element.Position) &&
                                            AcadTools.PointsAreEqual(x.line.GetClosestPointTo(element.Position, false), element.Position))
                                      .OrderBy(o => (o.line.GetClosestPointTo(element.Position, false) - element.Position).Length)
                                      .FirstOrDefault();
                    if (trackLine == null)
                    {
                        ErrLogger.Error("Unable to split track line on Vertex", element.Designation, "");
                        error = true;
                        continue;
                    }               
                }
                Point3d splitPt = trackLine.line.GetClosestPointTo(element.Position, false);
                List<Line> splitLines =
                    AcadTools.SplitLineOnPoint(trackLine.line, splitPt);
                if (splitLines.Count > 1)
                {
                    using (Transaction trans = db.TransactionManager.StartTransaction())
                    {
                        foreach (var line in splitLines)
                    {
                        trackLines.Add(new TrackLine
                        {
                            color = trackLine.color,
                            direction = trackLine.direction,
                            line = line,
                            LineID = trackLine.LineID
                        });
                            //BlockTableRecord btr = (BlockTableRecord)trans.GetObject(db.CurrentSpaceId, OpenMode.ForWrite, false);
                            //btr.AppendEntity(line);
                            //trans.AddNewlyCreatedDBObject(line, true);
                        }
                        //Line ent = (Line)trans.GetObject(trackLine.line.ObjectId, OpenMode.ForWrite);
                        //ent.Erase();
                        //trans.Commit();
                    }
                    trackLines.Remove(trackLine);
                }
            }
        }

        /// <summary>
        /// Finds danger point (axle counter) of signal
        /// </summary>
        /// <param name="signal">Signal (Markerboard)</param>
        /// <returns>Danger point if found null otherwise</returns>
        private DangerPoint GetDangerPoint(Signal signal)
        {
            DangerPoint dangerPoint = new DangerPoint 
            { 
                Distance = 0,
                Id = null
            };
            if (signal.KindOfSignal == TKindOfSignal.foreignSignal || 
                signal.KindOfSignal == TKindOfSignal.eotmb)
            {
                return dangerPoint;
            }
            TSeg tSeg = signal.Tseg;
            if (tSeg == null)
            {
                ErrLogger.Error("Initial Tseg for danger point calculation not found", signal.Designation, "");
                error = true;
                return dangerPoint;
            }
            DirectionType direction = signal.Direction;
            DetectionPoint dp = null;
            List<TSeg> tSegsToDp = new List<TSeg> { tSeg };
            int iter = 0;
            while (true)
            {
                if (iter >= Constants.dpIterLimit)
                {
                    ErrLogger.Error("No axle counter for danger point found. Limit exceeded", signal.Designation, "");
                    error = true;
                    return dangerPoint;
                }
                if (direction == DirectionType.up)
                {
                    dp = DetectionPoints
                         .Where(x => x.Location >= signal.Location &&
                                     x.Tseg == tSeg)
                         .OrderBy(x => x.Location)
                         .FirstOrDefault();
                }
                else if (direction == DirectionType.down)
                {
                    dp = DetectionPoints
                         .Where(x => x.Location <= signal.Location &&
                                     x.Tseg == tSeg)
                         .OrderByDescending(x => x.Location)
                         .FirstOrDefault();
                }
                if (dp != null)
                {
                    break;
                }
                tSeg = GetNextTsegNoBranch(tSeg, ref direction);
                if (tSeg == null)
                {
                    ErrLogger.Error("Tseg for danger point calculation not found", signal.Designation, "");
                    error = true;
                    return dangerPoint;
                }
                tSegsToDp.Add(tSeg);
                iter++;
            }
            
            decimal distance = GetDistBetweenLocsByTsegs(tSegsToDp, signal, dp);
            dangerPoint = new DangerPoint
            {
                Id = dp.Designation,
                Distance = (int)(distance * 1000)
            };
            return dangerPoint;
        }

        /// <summary>
        /// Finds Balise Group for ShiftOces calculation
        /// </summary>
        /// <param name="signal">Signal (Markerboard)</param>
        /// <returns>Balise Group if found null otherwise</returns>
        private ShiftCesBG GetOcesBg(Signal signal)
        {
            ShiftCesBG OcesBg = new ShiftCesBG 
            { 
                Id = null,
                Distance = 0 
            };
            if (signal.KindOfSignal == TKindOfSignal.foreignSignal || 
                signal.KindOfSignal == TKindOfSignal.eotmb ||
                signal.KindOfSignal == TKindOfSignal.L2ExitSignal ||
                signal.ToPSA)
            {
                return OcesBg;
            }
            TSeg tSeg = signal.Tseg;
            if (tSeg == null)
            {
                ErrLogger.Error("Initial Tseg for Oces Bg calculation not found", signal.Designation, "");
                error = true;
                return OcesBg;
            }
            DirectionType direction = ReverseDirection(signal.Direction);

            List<SearchElement> foundSearchElements = GetElementsOnNodes(tSeg, signal, typeof(BaliseGroup), direction);
            SearchElement bg = foundSearchElements
                       .Where(x => GetDistBetweenLocsByTsegs(x.Tsegs, signal, x.Element) ==
                                   foundSearchElements.Max(m => GetDistBetweenLocsByTsegs(m.Tsegs, signal, m.Element)))
                       .FirstOrDefault();
            if (bg == null)
            {
                ErrLogger.Error("No BG for Oces found.", signal.Designation, "");
                error = true;
                return OcesBg;
            }
            
            OcesBg = new ShiftCesBG
            {
                Id = bg.Element.Designation,
                Distance = (int)(GetDistBetweenLocsByTsegs(bg.Tsegs, signal, bg.Element) * 1000)
            };
            return OcesBg;
        }

        /// <summary>
        /// Calculates Signal Closures:
        /// - Danger point and distance to it,
        /// - Balise group for Oces,
        /// - ShiftOces value
        /// </summary>
        private void GetSignalsClosures()
        {
#if DEBUG
            ErrLogger.Information("begin", "Sig closure");
#endif
            foreach (Signal signal in Signals.OrderBy(s => s.Designation))
            {
                if (signal.Exclude || signal.NextStation)
                {
                    signal.DangerPoint = new DangerPoint();
                    signal.ShiftCesBG = new ShiftCesBG();
                    continue;
                }
                signal.DangerPoint = GetDangerPoint(signal);
#if DEBUG
                string info = "";
                if (signal.DangerPoint != null)
                {
                    info = signal.DangerPoint.Id + "-->" + signal.DangerPoint.Distance;
                }
#endif
                signal.ShiftCesBG = GetOcesBg(signal);
#if DEBUG
                if (signal.ShiftCesBG != null)
                {
                    info += " \t" + signal.ShiftCesBG.Id + " AC-->BG:" + (signal.DangerPoint.Distance + signal.ShiftCesBG.Distance) + 
                        " \tOces:" + signal.GetShiftOces();
                }
                if (signal.ToPSA)
                {
                    info += " \tTo PSA";
                }
                if (signal.ExtType != ExtType.eotmb && signal.ExtType != ExtType.mb)
                {
                    info += " \t" + signal.ExtType.ToString();
                }              
                ErrLogger.Information(info, signal.Designation);
#endif
            }
#if DEBUG
            ErrLogger.Information("end", "Sig closure");
#endif
        }

        /// <summary>
        /// Finds Signals(mb) located on end of track
        /// and set <paramref name="ExtType"/> value to 'eotmb'
        /// </summary>
        private void SetSigsEotmb()
        {
            if (EndOfTracks == null || EndOfTracks.Count == 0)
            {
                return;
            }
            var tsegs = Tsegs
                       .Where(x => x.Vertex1.Element.ElType == XType.EndOfTrack ||
                                   x.Vertex2.Element.ElType == XType.EndOfTrack);
            var signals = Signals
                        .Where(x => tsegs.Contains(x.Tseg) &&
                                    tsegs.Any(a => a.Vertex1.Km == x.Location ||
                                                  a.Vertex2.Km == x.Location))
                        .ToList();
            foreach (Signal signal in signals)
            {
                signal.ExtType = ExtType.eotmb;
            }
        }

        private void SetSigsKind()
        {
            foreach (Signal signal in Signals)
            {
                if (!signal.SetKindOfSig())
                {
                    ErrLogger.Error("Unable to parse KindOfSignal", signal.Designation, signal.ExtType.ToString());
                    error = true;
                }
            }
        }

        /// <summary>
        /// Detects if Signal towards PSA and no movable elements in between.
        /// </summary>
        /// <param name="signal">Signal(Markerboard)</param>
        /// <returns>True if Signal towards PSA false otherwise</returns>
        private bool IsSigToPsa(Signal signal)
        {
            if (signal.ExtType == ExtType.eotmb)
            {
                return false;
            }
            if (signal.InsidePSA != null)
            {
                return false;
            }
            TSeg tSeg = signal.Tseg;
            if (tSeg == null)
            {
                ErrLogger.Error("Initial Tseg for danger point calculation not found", signal.Designation, "");
                error = true;
                return false;
            }
            DirectionType direction = signal.Direction;
            Signal nextSignal = null;
            Psa psa = null;
            int iter = 0;
            while (true)
            {
                if (iter >= Constants.sigIterLimit)
                {
                    ErrLogger.Error("No axle counter for danger point found. Limit exceeded", signal.Designation, "");
                    error = true;
                    return false;
                }
                if (direction == DirectionType.up)
                {
                    nextSignal = Signals
                                 .Where(x => x.Location >= signal.Location &&
                                             x.Direction == signal.Direction &&
                                             x != signal &&
                                             x.Tseg == tSeg)
                                 .OrderBy(x => x.Location)
                                 .FirstOrDefault();
                    psa = Psas
                        .Where(x => x.Begin >= signal.Location &&
                                    x.Tseg == tSeg)
                        .FirstOrDefault();
                    if (nextSignal != null)
                    {
                        if (psa != null && psa.Begin < nextSignal.Location)
                        {
                            return true;
                        }
                        return false; ;
                    }
                }
                else if (direction == DirectionType.down)
                {
                    nextSignal = Signals
                                 .Where(x => x.Location <= signal.Location &&
                                             x.Direction == signal.Direction &&
                                             x != signal &&
                                             x.Tseg == tSeg)
                                 .OrderByDescending(x => x.Location)
                                 .FirstOrDefault();
                    psa = Psas
                        .Where(x => x.Begin <= signal.Location &&
                                    x.Tseg == tSeg)
                        .FirstOrDefault();
                    if (nextSignal != null)
                    {
                        if (psa != null && psa.Begin > nextSignal.Location)
                        {
                            return true;
                        }
                        return false; ;
                    }
                }
                if (psa != null)
                {
                    return true;
                }
                tSeg = GetNextTsegNoBranch(tSeg, ref direction);
                if (tSeg == null)
                {
                    return false;
                }
                iter++;
            }
        }

        //TODO: to document methods
        private TSeg GetNextTsegNoBranch(TSeg initTseg, ref DirectionType direction)
        {
            TSeg nextTseg = null;
            if (direction == DirectionType.up)
            {
                if (initTseg.Vertex2.Element.ElType == XType.Point)
                {
                    return null;
                }
                nextTseg = Tsegs
                       .Where(x => (x.Vertex1.Element == initTseg.Vertex2.Element ||
                                    x.Vertex2.Element == initTseg.Vertex2.Element) &&
                                   x != initTseg)
                       .FirstOrDefault();
            }
            else if (direction == DirectionType.down)
            {
                if (initTseg.Vertex1.Element.ElType == XType.Point)
                {
                    return null;
                }
                nextTseg = Tsegs
                       .Where(x => (x.Vertex2.Element == initTseg.Vertex1.Element ||
                                    x.Vertex1.Element == initTseg.Vertex1.Element) &&
                                   x != initTseg)
                       .FirstOrDefault();
            }
            if (nextTseg != null)
            {
                if (nextTseg.LineDirection != initTseg.LineDirection)
                {
                    direction = nextTseg.LineDirection;
                }
            }
            return nextTseg;
        }

        private List<TSeg> GetTsegsBetweenTwoTsegs(TSeg start, TSeg end)
        {
            DirectionType direction = DirectionType.up;
            List<TSeg> foundTsegs = new List<TSeg>();
            int limit = 0;
            TSeg tmpTseg = start;
            do
            {
                if (limit >= Constants.nextNodeMaxAttemps)
                {
                    break;
                }
                foundTsegs.Add(tmpTseg);
                tmpTseg = GetNextTsegNoBranch(tmpTseg, ref direction);
                if (tmpTseg == null)
                {
                    break;
                }
                limit++;
            }
            while (tmpTseg != end);

            direction = DirectionType.down;
            foundTsegs.Clear();
            limit = 0;
            tmpTseg = start;
            if (tmpTseg != end)
            {
                do
                {
                    if (limit >= Constants.nextNodeMaxAttemps)
                    {
                        break;
                    }
                    foundTsegs.Add(tmpTseg);
                    tmpTseg = GetNextTsegNoBranch(tmpTseg, ref direction);
                    if (tmpTseg == null)
                    {
                        break;
                    }
                    limit++;
                }
                while (tmpTseg != end);
            }
            if (tmpTseg == end)
            {
                foundTsegs.Add(end);
                return foundTsegs;
            }
            foundTsegs.Clear();
            foundTsegs.Add(start);
            foundTsegs.Add(end);
            ErrLogger.Error("No tsegs between two tsegs found", start.Id, end.Id);
            error = true;
            return foundTsegs;
        }

        private List<TSeg> GetNextTsegsAllBranches(TSeg initTseg, DirectionType direction, List<TSeg> excludeTsegs)
        {
            List<TSeg> nextTsegs = null;
            if (direction == DirectionType.up)
            {
                nextTsegs = Tsegs
                       .Where(x => (x.Vertex1.Element == initTseg.Vertex2.Element ||
                                    x.Vertex2.Element == initTseg.Vertex2.Element) &&
                                    x != initTseg &&
                                    !excludeTsegs.Contains(x))
                       .ToList();
            }
            else if (direction == DirectionType.down)
            {
                nextTsegs = Tsegs
                       .Where(x => (x.Vertex2.Element == initTseg.Vertex1.Element ||
                                    x.Vertex1.Element == initTseg.Vertex1.Element) &&
                                    x != initTseg &&
                                    !excludeTsegs.Contains(x))
                       .ToList();
            }
            return nextTsegs;
        }

        private List<TSeg> GetNextTsegsAllBranchesFacingPoints(TSeg initTseg, DirectionType direction, List<TSeg> excludeTsegs)
        {
            List<TSeg> nextTsegs = null;
            Func<TSeg, bool> vertexCriteria;
            if (direction == DirectionType.up)
            {
                if (initTseg.Vertex2.Element.ElType == XType.Point && 
                    initTseg.Vertex2.Conn != ConnectionBranchType.tip)
                {
                    vertexCriteria = new Func<TSeg, bool>(
                        x => ((x.Vertex1.Element == initTseg.Vertex2.Element && x.Vertex1.Conn == ConnectionBranchType.tip) ||
                              (x.Vertex2.Element == initTseg.Vertex2.Element && x.Vertex2.Conn == ConnectionBranchType.tip)) &&
                             x != initTseg &&
                             !excludeTsegs.Contains(x));
                }
                else
                {
                    vertexCriteria = new Func<TSeg, bool>(x => (x.Vertex1.Element == initTseg.Vertex2.Element ||
                                                                x.Vertex2.Element == initTseg.Vertex2.Element) &&
                                                               x != initTseg &&
                                                               !excludeTsegs.Contains(x));
                }
                nextTsegs = Tsegs
                            .Where(vertexCriteria)
                            .ToList();
            }
            else if (direction == DirectionType.down)
            {
                if (initTseg.Vertex1.Element.ElType == XType.Point &&
                    initTseg.Vertex1.Conn != ConnectionBranchType.tip)
                {
                    vertexCriteria = new Func<TSeg, bool>(
                        x => ((x.Vertex1.Element == initTseg.Vertex1.Element && x.Vertex1.Conn == ConnectionBranchType.tip) ||
                              (x.Vertex2.Element == initTseg.Vertex1.Element && x.Vertex2.Conn == ConnectionBranchType.tip)) &&
                             x != initTseg &&
                             !excludeTsegs.Contains(x));
                }
                else
                {
                    vertexCriteria = new Func<TSeg, bool>(x => (x.Vertex1.Element == initTseg.Vertex1.Element ||
                                                                x.Vertex2.Element == initTseg.Vertex1.Element) &&
                                                               x != initTseg &&
                                                               !excludeTsegs.Contains(x));
                }
                nextTsegs = Tsegs
                            .Where(vertexCriteria)
                            .ToList();
            }
            return nextTsegs;
        }

        private List<SearchElement> GetElementsOnNodes(TSeg startTseg, SLElement startElem,Type type, DirectionType direction)
        {
            List<SearchElement> elements = new List<SearchElement>();
            List<TSeg> nextTsegs = new List<TSeg> { startTseg };
            Stack<Stack<TSeg>> tSegsStack = new Stack<Stack<TSeg>>();
            tSegsStack.Push(new Stack<TSeg>(nextTsegs));
            decimal searchLocation = startElem.Location;
            int limit = 0;
            while (tSegsStack.Count > 0)
            {
                if (limit >= Constants.nextNodeMaxAttemps)
                {
                    ErrLogger.Error("Unable to find Element.", startElem.Designation, "");
                    error = true;
                    tSegsStack = TsegsStackPop(tSegsStack, out int back);
                    if (tSegsStack.Count == 0 || back == 0)
                    {
                        break;
                    }
                    limit -= back;
                }
                limit++;
                SLElement element = null;
                direction = GetStackChangeDir(tSegsStack, direction);
                GetSearchLocation(tSegsStack, direction, ref searchLocation);
                if (direction == DirectionType.up)
                {
                    element = Elements
                              .Where(x => x.Location >= searchLocation &&
                                          x.Tseg == tSegsStack.Peek().Peek() &&
                                          x.GetType() == type)
                              .OrderBy(x => x.Location)
                              .FirstOrDefault();
                }
                else if (direction == DirectionType.down)
                {
                    element = Elements
                              .Where(x => x.Location <= searchLocation &&
                                          x.Tseg == tSegsStack.Peek().Peek() &&
                                          x.GetType() == type)
                              .OrderByDescending(x => x.Location)
                              .FirstOrDefault();
                }

                if (element == null)
                {
                    nextTsegs = GetNextTsegsAllBranches(tSegsStack.Peek().Peek(), direction,
                                                          tSegsStack.Select(x => x.Peek()).ToList());
                    if (nextTsegs.Count == 0)
                    {
                        tSegsStack = TsegsStackPop(tSegsStack, out int back);
                        ErrLogger.Error("Unable to find Element.", startElem.Designation, "");
                        error = true;
                    }
                    else
                    {
                        tSegsStack.Push(new Stack<TSeg>(nextTsegs));
                    }
                }
                else
                {
                    var searchTsegs = tSegsStack.Select(x => x.Peek()).ToList();
                    searchTsegs.Reverse();
                    elements.Add(new SearchElement { Element = element , Tsegs = searchTsegs}) ;
                    tSegsStack = TsegsStackPop(tSegsStack, out int back);
                    limit -= back;
                }
            };
            return elements;
        }

        private List<SearchElement> GetElementsOnNodes(TSeg startTseg, SLElement startElem, 
                                                       Func<SLElement, bool> elemCriteria, DirectionType direction,
                                                       bool reportError = true)
        {
            List<SearchElement> elements = new List<SearchElement>();
            List<TSeg> nextTsegs = new List<TSeg> { startTseg };
            Stack<Stack<TSeg>> tSegsStack = new Stack<Stack<TSeg>>();
            tSegsStack.Push(new Stack<TSeg>(nextTsegs));
            decimal searchLocation = startElem.Location;
            int limit = 0;
            while (tSegsStack.Count > 0)
            {
                if (limit >= Constants.nextNodeMaxAttemps)
                {
                    if (reportError)
                    {
                        ErrLogger.Error("Unable to find Element.", startElem.Designation, "");
                        error = true;
                    }
                    tSegsStack = TsegsStackPop(tSegsStack, out int back);
                    if (tSegsStack.Count == 0 || back == 0)
                    {
                        break;
                    }
                    limit -= back;
                }
                limit++;
                SLElement element = null;
                direction = GetStackChangeDir(tSegsStack, direction);
                GetSearchLocation(tSegsStack, direction, ref searchLocation);
                if (direction == DirectionType.up)
                {
                    element = Elements
                              .Where(x => x.Location >= searchLocation &&
                                          x.Tseg == tSegsStack.Peek().Peek())
                              .Where(elemCriteria)
                              .OrderBy(x => x.Location)
                              .FirstOrDefault();
                }
                else if (direction == DirectionType.down)
                {
                    element = Elements
                              .Where(x => x.Location <= searchLocation &&
                                          x.Tseg == tSegsStack.Peek().Peek())
                              .Where(elemCriteria)
                              .OrderByDescending(x => x.Location)
                              .FirstOrDefault();
                }

                if (element == null)
                {
                    nextTsegs = GetNextTsegsAllBranches(tSegsStack.Peek().Peek(), direction,
                                                         tSegsStack.Select(x => x.Peek()).ToList());
                    if (nextTsegs.Count == 0)
                    {
                        tSegsStack = TsegsStackPop(tSegsStack, out int back);
                        if (reportError)
                        {
                            ErrLogger.Error("Unable to find Element.", startElem.Designation, "");
                            error = true;
                        }               
                    }
                    else
                    {
                        tSegsStack.Push(new Stack<TSeg>(nextTsegs));
                    }
                }
                else
                {
                    var searchTsegs = tSegsStack.Select(x => x.Peek()).ToList();
                    searchTsegs.Reverse();
                    elements.Add(new SearchElement { Element = element, Tsegs = searchTsegs });
                    tSegsStack = TsegsStackPop(tSegsStack, out int back);
                    limit -= back;
                }
            };
            return elements;
        }

        private List<SearchElement> GetDestSigsOnNodes(TSeg startTseg, Signal startSignal,
                                                       DirectionType direction,
                                                       bool reportError = true)
        {
            List<SearchElement> elements = new List<SearchElement>();
            List<TSeg> nextTsegs = new List<TSeg> { startTseg };
            Stack<Stack<TSeg>> tSegsStack = new Stack<Stack<TSeg>>();
            tSegsStack.Push(new Stack<TSeg>(nextTsegs));
            decimal searchLocation = startSignal.Location;
            int limit = 0;
            while (tSegsStack.Count > 0)
            {
                if (limit >= Constants.nextNodeMaxAttemps)
                {
                    if (reportError)
                    {
                        ErrLogger.Error("Unable to find Element.", startSignal.Designation, "");
                        error = true;
                    }
                    tSegsStack = TsegsStackPop(tSegsStack, out int back);
                    if (tSegsStack.Count == 0 || back == 0)
                    {
                        break;
                    }
                    limit -= back;
                }
                limit++;
                Signal element = null;
                direction = GetStackChangeDir(tSegsStack, direction);
                GetSearchLocation(tSegsStack, direction, ref searchLocation);
                if (direction == DirectionType.up)
                {
                    element = Signals
                              .Where(x => x.Location >= searchLocation &&
                                          x.Tseg == tSegsStack.Peek().Peek() &&
                                          x.Direction == direction && 
                                          x != startSignal)
                              .OrderBy(x => x.Location)
                              .FirstOrDefault();
                }
                else if (direction == DirectionType.down)
                {
                    element = Signals
                              .Where(x => x.Location <= searchLocation &&
                                          x.Tseg == tSegsStack.Peek().Peek() &&
                                          x.Direction == direction &&
                                          x != startSignal)
                              .OrderByDescending(x => x.Location)
                              .FirstOrDefault();
                }

                if (element == null)
                {
                    nextTsegs = GetNextTsegsAllBranchesFacingPoints(tSegsStack.Peek().Peek(), direction,
                                                         tSegsStack.Select(x => x.Peek()).ToList());
                    if (nextTsegs.Count == 0)
                    {
                        tSegsStack = TsegsStackPop(tSegsStack, out int back);
                        if (reportError)
                        {
                            ErrLogger.Error("Unable to find Element.", startSignal.Designation, "");
                            error = true;
                        }
                    }
                    else
                    {
                        tSegsStack.Push(new Stack<TSeg>(nextTsegs));
                    }
                }
                else
                {
                    var searchTsegs = tSegsStack.Select(x => x.Peek()).ToList();
                    searchTsegs.Reverse();
                    elements.Add(new SearchElement { Element = element, Tsegs = searchTsegs });
                    tSegsStack = TsegsStackPop(tSegsStack, out int back);
                    limit -= back;
                }
            };
            return elements;
        }

        private DirectionType GetStackChangeDir(Stack<Stack<TSeg>> segs, DirectionType direction)
        {
            if (segs != null && segs.Count > 1)
            {
                var tmp = segs.Pop();
                if (segs.Peek().Peek().LineDirection != tmp.Peek().LineDirection)
                {
                    direction = ReverseDirection(direction);
                }
                segs.Push(tmp);
            }
            return direction;
        }

        private void GetSearchLocation(Stack<Stack<TSeg>> segs, DirectionType direction, ref decimal searchLocation)
        {
            if (segs != null && segs.Count > 1 && segs.Peek() != null && segs.Peek().Count > 0)
            {
                if (direction == DirectionType.up)
                {
                    searchLocation = segs.Peek().Peek().Vertex1.Km;
                }
                else
                {
                    searchLocation = segs.Peek().Peek().Vertex2.Km;
                }
            }          
        }

        private decimal GetKmGapBetweenSegments(TSeg tSeg1, TSeg tSeg2)
        {
            if (tSeg1.LineID != tSeg2.LineID)
            {
                return 0;
            }
            ConnectionBranchType branchType1 = ConnectionBranchType.none;
            ConnectionBranchType branchType2 = ConnectionBranchType.none;
            elements.Vertex changeVertex = GetCommVertex(tSeg1, tSeg2, GetCommVertOptions.byFirstSeg);
            if (changeVertex == null)
            {
                return 0;
            }
            if (changeVertex.Element.ElType == XType.Connector)
            {
                return ((Connector)changeVertex.Element).KmpGap;
            }

            if (branchType1 == ConnectionBranchType.right || branchType2 == ConnectionBranchType.right)
            {
                return ((elements.Point)changeVertex.Element).KmpGapR * -1;
            }
            else if (branchType1 == ConnectionBranchType.left || branchType2 == ConnectionBranchType.left)
            {
                return ((elements.Point)changeVertex.Element).KmpGapL * -1;
            }
            else
            {
                return 0;
            }
        }

        private elements.Vertex GetCommVertex(TSeg tsegFrom, TSeg nextTseg, GetCommVertOptions vertOptions)
        {
            if (tsegFrom.Vertex1.Element == nextTseg.Vertex1.Element)
            {
                if (vertOptions == GetCommVertOptions.byFirstSeg)
                {
                    return tsegFrom.Vertex1;
                }
                else
                {
                    return nextTseg.Vertex1;
                }               
            }
            else if (tsegFrom.Vertex1.Element == nextTseg.Vertex2.Element)
            {
                if (vertOptions == GetCommVertOptions.byFirstSeg)
                {
                    return tsegFrom.Vertex1;
                }
                else
                {
                    return nextTseg.Vertex2;
                }          
            }
            else if (tsegFrom.Vertex2.Element == nextTseg.Vertex1.Element)
            {
                if (vertOptions == GetCommVertOptions.byFirstSeg)
                {
                    return tsegFrom.Vertex2;
                }
                else
                {
                    return nextTseg.Vertex1;
                }              
            }
            else if (tsegFrom.Vertex2.Element == nextTseg.Vertex2.Element)
            {
                if (vertOptions == GetCommVertOptions.byFirstSeg)
                {
                    return tsegFrom.Vertex2;
                }
                else
                {
                    return nextTseg.Vertex2;
                }            
            }
            ErrLogger.Error("Unable to get common Vertex between Tsegs.", tsegFrom.Id, nextTseg.Id);
            error = true;
            return null;
        }

        private elements.Vertex GetCommVertex(TSeg tsegFrom, TSeg nextTseg, SLElement refElement)
        {
            if (tsegFrom.Vertex1.Element == nextTseg.Vertex1.Element)
            {
                if (refElement.LineID == tsegFrom.LineID)
                {
                    return tsegFrom.Vertex1;
                }
                else if (refElement.LineID == nextTseg.LineID)
                {
                    return nextTseg.Vertex1;
                }
            }
            else if (tsegFrom.Vertex1.Element == nextTseg.Vertex2.Element)
            {
                if (refElement.LineID == tsegFrom.LineID)
                {
                    return tsegFrom.Vertex1;
                }
                else if (refElement.LineID == nextTseg.LineID)
                {
                    return nextTseg.Vertex2;
                }
            }
            else if (tsegFrom.Vertex2.Element == nextTseg.Vertex1.Element)
            {
                if (refElement.LineID == tsegFrom.LineID)
                {
                    return tsegFrom.Vertex2;
                }
                else if (refElement.LineID == nextTseg.LineID)
                {
                    return nextTseg.Vertex1;
                }
            }
            else if (tsegFrom.Vertex2.Element == nextTseg.Vertex2.Element)
            {
                if (refElement.LineID == tsegFrom.LineID)
                {
                    return tsegFrom.Vertex2;
                }
                else if (refElement.LineID == nextTseg.LineID)
                {
                    return nextTseg.Vertex2;
                }
            }
            ErrLogger.Error("Unable to get common Vertex between Tsegs.", tsegFrom.Id, nextTseg.Id);
            error = true;
            return null;
        }

        private DirectionType ReverseDirection(DirectionType direction)
        {
            if (direction == DirectionType.up)
            {
                return DirectionType.down;
            }
            else if (direction == DirectionType.down)
            {
                return DirectionType.up;
            }
            else
            {
                return direction;
            }
        }

        private decimal GetDistBetweenLocsByTsegs(List<TSeg> tSegs, SLElement start, SLElement end)
        {
            decimal distance = 0;
            decimal kmGap = 0;
            if (tSegs == null || tSegs.Count == 0)
            {
                ErrLogger.Error("Unable to get distance between locations - Tsegs not found", start, end);
                error = true;
            }

            if (tSegs.Count == 1)
            {
                return Math.Abs(start.Location - end.Location);
            }
            for (int i = 0; i < tSegs.Count; i++)
            {
                elements.Vertex commVertex;
                if (i == 0)
                {
                    commVertex = GetCommVertex(tSegs[i], tSegs[i + 1], start);
                    if (commVertex != null)
                    {
                        kmGap += GetKmGapBetweenSegments(tSegs[i], tSegs[i + 1]);
                        distance =
                        Math.Abs(start.Location - commVertex.Km) + kmGap;
                        
                    }
                }
                else if (i < tSegs.Count - 1)
                {
                    distance += tSegs[i].Length(); // +
                    kmGap += GetKmGapBetweenSegments(tSegs[i], tSegs[i + 1]);
                }
                else
                {
                    commVertex = GetCommVertex(tSegs[i - 1], tSegs[i], end);
                    if (commVertex != null)
                    {
                        distance +=
                        Math.Abs(end.Location - commVertex.Km);
                    }
                }
            }
            distance += kmGap;


            //if (tSegs.Count == 1)
            //{
            //    distance = Math.Abs(start.Location - end.Location);
            //}
            //else if (tSegs.Count > 0)
            //{
            //    for (int i = 0; i < tSegs.Count; i++)
            //    {
            //        elements.Vertex commVertex;
            //        if (i == 0)
            //        {
            //            commVertex = GetCommVertex(tSegs[i], tSegs[i + 1], start);
            //            if (commVertex != null)
            //            {
            //                distance =
            //                Math.Abs(start.Location - commVertex.Km) +
            //                GetKmGapBetweenSegments(tSegs[i], tSegs[i + 1]);
            //            }
            //        }
            //        else if (i < tSegs.Count - 1)
            //        {
            //            distance += tSegs[i].Length(); // +
            //                GetKmGapBetweenSegments(tSegs[i], tSegs[i + 1]);
            //        }
            //        else
            //        {
            //            commVertex = GetCommVertex(tSegs[i - 1], tSegs[i], end);
            //            if (commVertex != null)
            //            {
            //                distance +=
            //                Math.Abs(end.Location - commVertex.Km); 
            //            }
            //        }
            //    }
            //}
            //else
            //{
            //    ErrLogger.Error("Unable to get distance between locations - Tsegs not found", start, end);
            //    error = true;
            //}
            return distance;
        }

        private List<TrustedArea> GetTrustedAreas()
        {
            List<TrustedArea> trustedAreas = new List<TrustedArea>();
            List<Line> tstaLinesToSkip = new List<Line>();
            List<Line> tstaFoundLines = new List<Line>();
            Line start = null;
            List<Line> tstaLines = AcadTools.GetLinesByLayer(new Regex(Constants.trustAreaLayer), db)
                              .ToList();
            int index = 1;
            do
            {
                start = tstaLines
                        .Where(x => trackLines
                                     .Any(a => AcadTools.ObjectsIntersects(a.line, x, Intersect.OnBothOperands)) &&
                                     !tstaLinesToSkip.Contains(x))
                        .FirstOrDefault();
                if (start != null)
                {
                    tstaFoundLines = new List<Line> { start };
                    tstaLinesToSkip.Add(start);
                    Line end = start;
                    do
                    {                      
                        end = tstaLines
                              .Where(x => AcadTools.LinesHasSameStartEnd(end, x) &&
                              !tstaLinesToSkip.Contains(x))
                              .FirstOrDefault();
                        if (end != null)
                        {
                            tstaFoundLines.Add(end);
                            tstaLinesToSkip.Add(end);
                        }
                    } while (end != null);
                    TrustedArea trustedArea = CreateTstaArea(tstaFoundLines, index++);
                    if (trustedArea != null)
                    {
                        trustedAreas.Add(trustedArea);
                    }
                }
            } while (start != null);
            return trustedAreas;
        }

        private TrustedArea CreateTstaArea(List<Line> tstaLines, int index)
        {
            TrustedArea area = new TrustedArea();
            tstaLines = tstaLines
                        .Where(x => trackLines
                                    .Any(a => AcadTools.ObjectsIntersects(a.line, x, Intersect.OnBothOperands)))
                        .ToList();
            var edges = Elements
                       .Where(x => tstaLines
                                   .Any(a => AcadTools.ObjectsIntersects(a, x.Block.BlockReference, Intersect.OnBothOperands)) &&
                                   (x.ElType == XType.Signal || x.ElType == XType.BaliseGroup))
                       .DistinctBy(x => x.Location)
                       .OrderBy(x => x.LineID)
                       .ThenBy(o => o.Location)
                       .Take(2)
                       .ToList();
            if (edges.Count() == 2)
            {
                var foundEdge = GetElementsOnNodes(edges[0].Tseg, edges[0], x => x == edges[1], DirectionType.up, false)
                            .FirstOrDefault();
                if (foundEdge == null)
                {
                    foundEdge = GetElementsOnNodes(edges[0].Tseg, edges[0], x => x == edges[1], DirectionType.down)
                            .FirstOrDefault();
                }
                if (foundEdge == null)
                {
                    ErrLogger.Error("Unable to find Tsegs between tsta edges", edges[0].Designation, edges[1].Designation);
                    error = true;
                    return null;
                }
                else
                {
                    area.Id = "tsta-" + SigLayout.StID + "-" + index.ToString("D3");
                    area.Km1 = edges[0].Location;
                    area.Km2 = edges[1].Location;
                    area.Tsegs = foundEdge.Tsegs;
                }
            }
            else
            {
                ErrLogger.Error("Edges Elements not found", "Trusted Area", "");
                error = true;
                return null;
            }
            return area;
        }

        private DirectionType GetEotDirection(EndOfTrack eot)
        {

            var tseg = Tsegs
                       .Where(x => x.Vertex1.Element == eot || 
                                   x.Vertex2.Element == eot)
                       .FirstOrDefault();
            if (tseg != null)
            {
                if (tseg.Vertex1.Element == eot)
                {
                    if (tseg.Vertex1.Km > tseg.Vertex2.Km)
                    {
                        return DirectionType.up;
                    }
                    else
                    {
                        return DirectionType.down;
                    }
                }
                else if (tseg.Vertex2.Element == eot)
                {
                    if (tseg.Vertex2.Km > tseg.Vertex1.Km)
                    {
                        return DirectionType.up;
                    }
                    else
                    {
                        return DirectionType.down;
                    }
                }
            }
            
            ErrLogger.Error("Unable to calculate eot direction", eot.Designation, "");
            error = true;
            return DirectionType.up;
        }

        public List<ExcelLib.ExpRoute> ExportRoutes(out bool error)
        {
            InitElements();
            SplitLinesOnVertexes(out error);
            if (error)
            {
                return null;
            }
            SetNextExclude();
            ProcessElements(ref error);
            Tsegs = GetTsegs(ref error);
            AssignTsegsToConns();
            bool elemTsegError = false;
            SetTsegIdToElements(ref elemTsegError);
            if (error)
            {
                return null;
            }
            return GetExpRoutes();
        }

        private List<ExcelLib.ExpRoute> GetExpRoutes()
        {
            List<ExcelLib.ExpRoute> routes = new List<ExcelLib.ExpRoute>();
            foreach (var signal in Signals.Where(x => x.Tseg != null))
            {
                var tmp = GetDestSigsOnNodes(signal.Tseg, signal, signal.Direction);
                foreach (var item in tmp)
                {
                    routes.Add(new ExcelLib.ExpRoute
                    {
                        Start = signal.Designation,
                        Destination = item.Element.Designation,
                        Points = GetPointsPathBySegs(item.Tsegs)
                    });
                }
            }
            return routes;
        }

        public List<ExcelLib.elements.ExpTseg> ExportTsegs(out bool error)
        {
            InitElements();
            SplitLinesOnVertexes(out error);
            if (error)
            {
                return null;
            }
            SetNextExclude();
            ProcessElements(ref error);
            Tsegs = GetTsegs(ref error);
            return GetExpTsegs();
        }

        private List<ExcelLib.elements.ExpTseg> GetExpTsegs()
        {
            List<ExcelLib.elements.ExpTseg> tsegs = new List<ExcelLib.elements.ExpTseg>();
            foreach (var tseg in Tsegs)
            {
                tsegs.Add(new ExcelLib.elements.ExpTseg 
                {
                    Designation = tseg.Id 
                });
            }
            return tsegs;
        }
        
        public List<ExcelLib.ExpTdlPt> ExportTdlPts(out bool error)
        {
            InitElements();
            SplitLinesOnVertexes(out error);
            if (error)
            {
                return null;
            }
            SetNextExclude();
            ProcessElements(ref error);
            Tsegs = GetTsegs(ref error);
            InitAcSections(ref error);
            return GetExpTdlPts();
        }

        private List<ExcelLib.ExpTdlPt> GetExpTdlPts()
        {
            List<ExcelLib.ExpTdlPt> tdlPts = new List<ExcelLib.ExpTdlPt>();
            foreach (var point in Points.Where(x => !x.Exclude && !x.NextStation))
            {
                var ownTdt = AcSections
                             .Where(x => x.Elements.Contains(point))
                             .FirstOrDefault();
                var expTdl = new ExcelLib.ExpTdlPt
                {
                    Designation = point.GetShortName() 
                };
                if (ownTdt != null)
                {
                    expTdl.OwnTdt = ownTdt.GetShortName();
                }
                tdlPts.Add(expTdl);
            }
            return tdlPts;
        }

        private List<string> GetPointsPathBySegs(List<TSeg> segs )
        {
            List<string> vs = new List<string>();
            for (int i = 0; i < segs.Count - 1; i++)
            {
                var vv = GetCommVertex(segs[i], segs[i + 1], GetCommVertOptions.bySecondSeg);
                if (vv != null && vv.Element.ElType == XType.Point && vv.Conn != ConnectionBranchType.tip)
                {
                    vs.Add(vv.Element.GetShortName() + "-" + vv.Conn.ToString().ToUpper().First());
                }
            }
            return vs;
        }

        public void ReplacePlatforms()
        {
            InitElements();//TODO test replace platforms

            foreach (var oldPltfrm in oldPlatforms)
            {

                try
                {
                    var test = oldPltfrm.Block.BlockReference.GeometricExtents.MaxPoint.X;
                }
                catch
                {
                    continue;
                }
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    AcadTools.CopyBlockFromFile(this.assemblyDir + Constants.cfgFolder + @"\Blks_Dynamic.dwg", "Platform_Dynamic", db);

                    BlockReference blkRefInserted =
                        (BlockReference)tr.GetObject(AcadTools.InsertBlock("Platform_Dynamic",
                                                                  oldPltfrm.Block.BlockReference.Position.X,
                                                                  oldPltfrm.Block.BlockReference.Position.Y, db),
                                                     OpenMode.ForWrite);
                    AcadTools.CopyAtributtes(oldPltfrm.Block.BlockReference, blkRefInserted, tr);
                    BlockReference Erase = (BlockReference)tr.GetObject(oldPltfrm.Block.BlockReference.Id,
                                                                OpenMode.ForWrite);
                    DynamicBlockReferencePropertyCollection properties =
                        blkRefInserted.DynamicBlockReferencePropertyCollection;
                    foreach (DynamicBlockReferenceProperty prt in properties)
                    {
                        if (prt.PropertyName == "Width")
                        {
                            prt.Value =
                                Math.Abs(oldPltfrm.Block.BlockReference.GeometricExtents.MaxPoint.X -
                                         oldPltfrm.Block.BlockReference.GeometricExtents.MinPoint.X);
                        }
                    }
                    if (AcadTools.LayerExists("KMP", db))
                    {
                        oldPltfrm.Block.BlockReference.Layer = "KMP";
                    }

                    Erase.Erase();
                    tr.Commit();
                }
            }
        }

        public bool HasErrors()
        {
            bool elem = false;
            if (Elements != null)
            {
                elem = Elements.Any(x => x.Error);
            }
            if (elem || this.error)
            {
                return true;
            }
            else return false;
        }

        public void Dispose()
        {
            ErrLogger.StopTmpLog();
        }
    }
}
