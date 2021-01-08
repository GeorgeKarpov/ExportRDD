using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Refact.dataMapping
{
    public class DataProcessor
    {
        string dgwDir;
        Dictionary<string, bool> checkData;
        Dictionary<string, string> loadFiles;
        List<string> lxList;
        List<string> pwsList;

        public List<TFileDescr> DocsDescrs { get; set; }

        ExcelLib.ReadExcel readExcel;
        ExcelLib.ReadWord readWord;

        List<ExcelLib.Route> routes;
        List<ExcelLib.Tdl> tdls;
        List<ExcelLib.Ssp> ssps;
        ExcelLib.FpsData fpsData;
        List<ExcelLib.Bg> bgs;
        ExcelLib.EmgsData emgsData;

        Dictionary<string ,ExcelLib.Lx> lxes;
        Dictionary<string, ExcelLib.Pws> pwses;

        public bool Error { get; set; }

        private AcLayout acLayout;
        private string rddId;
        private string rddVers;
        private string rddCreator;
        private TStatus status;

        SpeedProfiles SpeedProfiles;

        public DataProcessor(string dgwDir, string assDir, Dictionary<string, string> loadFiles, Dictionary<string, bool> checkData,
            List<string> lxList, List<string> pwsList, AcLayout layout, string rddId, string rddVers, string creator)
        {
            this.dgwDir = dgwDir;
            this.loadFiles = loadFiles;
            this.checkData = checkData;
            this.lxList = lxList;
            this.pwsList = pwsList;
            this.acLayout = layout;
            this.rddId = rddId;
            this.rddVers = rddVers;
            rddCreator = creator;
            ExcelLib.Config.SetFilesPaths(assDir);
            readExcel = new ExcelLib.ReadExcel();
            readWord = new ExcelLib.ReadWord();
            DocsDescrs = new List<TFileDescr>();
            status = new TStatus { status = StatusType.@new };
            LoadData();
        }        

        public void LoadData()
        {
            if (lxList != null)
            {
                lxes = readExcel.GetLxs(lxList, loadFiles["lblxlsLxs"], dgwDir, checkData["checkBoxLX"]);
            }
            if (pwsList != null)
            {
                pwses = readExcel.GetPwSs(pwsList, dgwDir);
            }

            GetCoversFileDescrs();
            if (!checkData["checkBoxRts"])
            {
                ErrLogger.Information("Routes data skipped", "Routes");
            }
            else
            {
                routes = readExcel.GetRoutes(loadFiles["lblxlsRoutes"]);
            }

            if (!checkData["checkBoxDL"])
            {
                ErrLogger.Information("Tdls data skipped", "Tdl");
            }
            else
            {
                tdls = readExcel.GetTdls(loadFiles["lblxlsDetLock"]);
            }

            if (!checkData["checkBoxSpProf"])
            {
                ErrLogger.Information("Ssp data skipped", "Ssp");
            }
            else
            {
                ssps = readExcel.GetSsps(loadFiles["lblxlsSpProf"]);
            }

            if (!checkData["checkBoxFP"])
            {
                ErrLogger.Information("Fp data skipped", "Fp");
            }
            else
            {
                fpsData = readExcel.GetFps(loadFiles["lblxlsFP"]);
            }


            if (!checkData["checkBoxBG"])
            {
                ErrLogger.Information("Bg data skipped", "Bg");
            }
            else
            {
                bgs = readExcel.GetBgs(loadFiles["lblxlsBgs"]);
            }


            if (!checkData["checkBoxEmSt"])
            {
                ErrLogger.Information("ES data skipped", "ES");
            }
            else
            {
                emgsData = readExcel.GetEs(loadFiles["lblxlsEmSg"], acLayout.SigLayout.StName);
            }
            DocsDescrs.AddRange(readExcel.FileDescrs
                                .Select(x => new TFileDescr 
                                {
                                    docID = x.DocID, 
                                    creator = x.Creator, 
                                    date = x.Date, 
                                    title = x.Title, 
                                    version = x.Version  
                                }));
            Error = readExcel.ErrFound = readWord.ErrFound;
        }

        private void GetCoversFileDescrs()
        {
            foreach (var file in Directory.GetFiles(dgwDir, "*.docx"))
            {
                readWord.CoverPage(file, out ExcelLib.TFileDescr fileDescr);
                DocsDescrs.Add(new TFileDescr
                {
                    docID = fileDescr.DocID,
                    creator = fileDescr.Creator,
                    date = fileDescr.Date,
                    title = fileDescr.Title,
                    version = fileDescr.Version
                });
            }
        }

        public RailwayDesignData GetRdd()
        {
            SpeedProfiles = GetSpeedProfiles();
            CheckEmgs();
            return new RailwayDesignData
            {
                MetaData = GetMetaData(),
                Lines = GetRddLines(),
                StationsAndStops = GetStationsStops(),
                TrackSegments = GetTrackSegments(),
                Connectors = GetConnectors(),
                Points = GetPoints(),
                EndOfTracks = GetEndOfTracks(),
                Signals = GetSignals(),
                DetectionPoints = GetDetectionPoints(),
                AxleCounterSections = GetAcSections(),
                TrackSections = GetTrackSections(),
                SpeedProfiles = SpeedProfiles
            };
        }  

        private RailwayDesignDataMetaData GetMetaData()
        {
            RailwayDesignDataMetaData metaData = new RailwayDesignDataMetaData
            {
                FileDescription = new TFileDescr
                {
                    title = "PT1 Tables " + acLayout.SigLayout.StName,
                    date = DateTime.Now.Date,
                    docID = rddId,
                    version = rddVers,
                    creator = rddCreator
                },
                SignallingLayout = new TFileDescr
                {
                    title = "Signalling Layout - " + acLayout.SigLayout.StName + " - " + acLayout.SigLayout.StID,
                    date = acLayout.SigLayout.Date,
                    docID = acLayout.SigLayout.DocId,
                    version = acLayout.SigLayout.Version,
                    creator = acLayout.SigLayout.Creator
                },
                Documents = new RailwayDesignDataMetaDataDocuments
                {
                    Document = DocsDescrs.ToArray()
                }
            };
            return metaData;
        }
        
        private Lines GetRddLines()
        {
            if (acLayout.RailwayLines == null || acLayout.RailwayLines.Count == 0)
            {
                return null;
            }
            return new Lines
            {
                Line = acLayout.RailwayLines
                       .Select(l => new LinesLine
                       {
                           Status = status,
                           Designation = l.Designation,
                           BeginKM = l.start,
                           EndKM = l.end,
                           Direction = l.direction
                       })
                       .ToArray()
            };
        }
        
        private StationsAndStops GetStationsStops()
        {
            if (acLayout.StationsStops == null || acLayout.StationsStops.Count == 0)
            {
                return null;
            }
            return new StationsAndStops
            {
                StationsAndStop = acLayout.StationsStops
                .Select(s => new StationsAndStopsStationsAndStop
                {
                    Status = status,
                    Designation = s.StId,
                    LongName = s.StName,
                    KindOfSAS = s.Kind,
                    Lines = new StationsAndStopsStationsAndStopLines
                    {
                        Line = s.Lines
                                .Select(l => new StationsAndStopsStationsAndStopLinesLine
                                {
                                    LineID = l.Designation,
                                    StartKM = l.start,
                                    EndKM = l.end
                                }).ToArray()
                    }
                })
                .ToArray()
            };
        }
        
        private TrackSegments GetTrackSegments()
        {
            if (acLayout.Tsegs == null || acLayout.Tsegs.Count == 0)
            {
                return null;
            }
            return new TrackSegments
            {
                TrackSegment = acLayout.Tsegs
                               .Where(t => !t.Vertex1.Element.NextStation &&
                                           !t.Vertex2.Element.NextStation)
                               .Select(t => new TrackSegmentsTrackSegment
                               {
                                   Status = status,
                                   Designation = t.Id,
                                   InsidePSA = t.InsidePSA,
                                   LineID = t.LineID,
                                   Vertex1 = new VertexType
                                   {
                                       connection = t.Vertex1.Conn,
                                       vertexID = t.Vertex1.Id
                                   },
                                   Vertex2 = new VertexType
                                   {
                                       connection = t.Vertex2.Conn,
                                       vertexID = t.Vertex2.Id
                                   },
                               })
                               .ToArray()
            };
        }
        
        private Connectors GetConnectors()
        {
            if (acLayout.Connectors == null || acLayout.Connectors.Count == 0)
            {
                return null;
            }
            return new Connectors
            {
                 Connector = acLayout.Connectors
                             .Where(c => !c.Exclude && !c.NextStation)
                             .Select(c => new ConnectorsConnector
                             {
                                 Status = status,
                                 Designation = c.Designation,
                                 OperationalKM1 = c.Kmp1.ToString(),
                                 OperationalKM2 = c.Kmp2.ToString(),
                                 TrackSegmentID1 = c.Tseg1.Id,
                                 TrackSegmentID2 = c.Tseg2.Id
                             })
                             .ToArray()
            };
        }
       
        private Points GetPoints()
        {
            if (acLayout.Points == null || acLayout.Points.Count == 0)
            {
                return null;
            }
            List<PointsPoint> rddPoints = new List<PointsPoint>();
            var tdls = GetTdls();
            foreach (elements.Point point in acLayout.Points.Where(p => !p.Exclude && !p.NextStation))
            {
                var foulingPoint = acLayout.FoulingPoints
                           .Where(f => point.Designation.Contains(f.Designation.Split('-').Last()))
                           .FirstOrDefault();
                PointsPointLinesLine[] lines;
                if (foulingPoint != null)
                {
                    lines = new PointsPointLinesLine[] 
                    { 
                        new PointsPointLinesLine 
                        {
                            KindOfPC = KindOfPCType.tip,
                            LineID = point.LineIdTip,
                            Location = point.KmpTip.ToString()
                        },
                        new PointsPointLinesLine 
                        {
                            KindOfPC = KindOfPCType.right,
                            LineID = point.LineIdRight,
                            Location = point.KmpRight.ToString(),
                            FoulingPointLocation = foulingPoint.Location.ToString()
                        },
                        new PointsPointLinesLine
                        {
                            KindOfPC = KindOfPCType.left,
                            LineID = point.LineIdLeft,
                            Location = point.KmpLeft.ToString(),
                            FoulingPointLocation = foulingPoint.Location.ToString()
                        }
                    };
                }
                else
                {
                    lines = new PointsPointLinesLine[]
                    {
                        new PointsPointLinesLine
                        {
                            KindOfPC = KindOfPCType.all,
                            LineID = point.LineIdTip,
                            Location = point.KmpTip.ToString()
                        }
                    };
                }
                var flankProtection = fpsData.GetFpByElId(point.Designation);
                                      //.Where(x => GetElemDesignation(point.RddType, x.Pt) == point.Designation)
                                      //.FirstOrDefault();
                PointsPoint rddPoint = new PointsPoint
                {
                    Status = status,
                    Designation = point.Designation,
                    KindOfPoint = point.Kind(),
                    InsidePSA = point.InsidePSA,
                    Lines = new PointsPointLines { Line = lines },
                    Trailable = point.Trailable,
                    PointPosIndicator = point.PosIndicator,
                    PointMachines = GetPointMachines(point),
                    TracksForDetectorLocking =
                        tdls.ContainsKey(point.Designation) ? tdls[point.Designation] : null,
                    EmergencyStopGroup = emgsData.GetEmgsByElId(point.Designation)
                                         //.Where(x => x.ElemId == point.Designation)
                                         //.Select(f => f.Id).FirstOrDefault()
                };
                rddPoint.FlankProtectionAbandonmentLeftSpecified = true;
                rddPoint.FlankProtectionAbandonmentRightSpecified = true;
                if (flankProtection != null)
                {
                    rddPoint.FlankProtectionAbandonmentLeft = 
                        flankProtection.RightNo == "x" ? YesNoType.yes : YesNoType.no;
                    rddPoint.FlankProtectionAbandonmentRight =
                        flankProtection.LeftNo == "x" ? YesNoType.yes : YesNoType.no;                   
                    var addTdtLeft = SplitTdts(flankProtection.RightTdt, "FP table");
                    var addTdtRight = SplitTdts(flankProtection.LeftTdt, "FP table");
                    if (addTdtLeft != null && addTdtLeft.Length > 0)
                    {
                        rddPoint.AdditionalAxleCounterSectionsLeft = new PointsPointAdditionalAxleCounterSectionsLeft
                        {
                            AxleCounterSectionID = addTdtLeft
                        };
                    }
                    if (addTdtRight != null && addTdtRight.Length > 0)
                    {
                        rddPoint.AdditionalAxleCounterSectionsRight = new PointsPointAdditionalAxleCounterSectionsRight
                        {
                            AxleCounterSectionID = SplitTdts(flankProtection.LeftTdt, "FP table")
                        };
                    }             
                }
                rddPoints.Add(rddPoint);
            }
            return new Points { Point = rddPoints.ToArray()};
        }

        private EndOfTracks GetEndOfTracks()
        {
            if (acLayout.EndOfTracks == null || acLayout.EndOfTracks.Count == 0)
            {
                return null;
            }
            return new EndOfTracks
            {
                EndOfTrack = acLayout.EndOfTracks
                            .Select(x => new EndOfTracksEndOfTrack 
                            {
                                Status = status,
                                Designation = x.Designation,
                                KindOfEOT = x.KindOfEOT,
                                LineID = x.LineID,
                                Location = x.Location.ToString(),
                                Direction = x.Direction 
                            })
                            .ToArray()
            };
        }

        private Signals GetSignals()
        {
            if (acLayout.Signals == null || acLayout.Signals.Count == 0)
            {
                return null;
            }
            var virtuals = GetVirtualSignals();
            return new Signals
            {
                Signal = acLayout.Signals
                         .Where(x => !x.Exclude && !x.NextStation)
                         .Select(s => new SignalsSignal
                         {
                             Status = status,
                             Designation = s.Designation,
                             KindOfSignal = s.KindOfSignal,
                             TrackSegmentID = s.GetTsegId(),
                             LineID = s.LineID,
                             Location = s.Location,
                             Direction = s.Direction,
                             TrackPosition = s.TrackPosition,
                             DangerPointID = s.DangerPoint.Id,
                             DangerPointDistanceSpecified = true,
                             DangerPointDistance = s.DangerPoint.Distance,
                             ShiftCESLocation = s.GetShiftOces(),
                             ShiftCESLocationSpecified = true,
                             Remarks = s.Remark
                         })
                         .OrderBy(o => o.Location)
                         .Union(virtuals)
                         .ToArray()
            };
        }

        private List<SignalsSignal> GetVirtualSignals()
        {
            var vSignalsTmp = acLayout.Signals
                              .Where(x => x.KindOfSignal == TKindOfSignal.eotmb);
            if (vSignalsTmp.Count() == 0)
            {
                return new List<SignalsSignal>();
            }
            List<SignalsSignal> vSignals = new List<SignalsSignal>();
            foreach (var vSigTmp in vSignalsTmp)
            {
                DirectionType direction;
                if (vSigTmp.Direction == DirectionType.up)
                {
                    direction = DirectionType.down;
                }
                else
                {
                    direction = DirectionType.up;
                }
                string[] newDesigSplit = vSigTmp.Designation.Split('-');
                newDesigSplit[0] = "spst";
                newDesigSplit[newDesigSplit.Length - 1] = "S" + newDesigSplit[newDesigSplit.Length - 1];
                string newDesig = string.Join("-", newDesigSplit);
                vSignals.Add(new SignalsSignal 
                {
                    Status = status,
                    Designation = newDesig,
                    KindOfSignal = TKindOfSignal.mb,
                    TrackSegmentID = vSigTmp.Tseg.Id,
                    LineID = vSigTmp.LineID,
                    Location = vSigTmp.Location,
                    Direction = direction,
                    TrackPosition = vSigTmp.TrackPosition,
                    DangerPointID = vSigTmp.DangerPoint.Id,
                    DangerPointDistanceSpecified = true,
                    DangerPointDistance = vSigTmp.DangerPoint.Distance,
                    ShiftCESLocation = vSigTmp.GetShiftOces(),
                    ShiftCESLocationSpecified = true,
                    Remarks = "Virtual signal"
                });             
            }
            return vSignals;
        }

        private DetectionPoints GetDetectionPoints()
        {
            if (acLayout.DetectionPoints == null || acLayout.DetectionPoints.Count == 0)
            {
                return null;
            }

            return new DetectionPoints
            {
                DetectionPoint = acLayout.DetectionPoints
                                 .Where(x => !x.Exclude && !x.NextStation)
                                 .Select(d => new DetectionPointsDetectionPoint 
                                 { 
                                    Status = status,
                                    Designation = d.Designation,
                                    KindOfDP = d.KindOfDP,
                                    InsidePSA = d.InsidePSA,
                                    TrackSegmentID = d.GetTsegId(),
                                    LineID = d.LineID,
                                    Location = d.Location.ToString()
                                 }).ToArray()
            };
        }

        private AxleCounterSections GetAcSections()
        {
            if (acLayout.AcSections == null || acLayout.AcSections.Count == 0)
            {
                return null;
            }
            return new AxleCounterSections
            {
                AxleCounterSection = acLayout.AcSections
                                     .Select(acs => new AxleCounterSectionsAxleCounterSection
                                     {
                                         Status = status,
                                         Designation = acs.Designation,
                                         DetectionPoints = new AxleCounterSectionsAxleCounterSectionDetectionPoints 
                                         { 
                                             DetectionPoint = acs.DetectionPoints
                                                              .Where(x => x.ElType == XType.DetectionPoint)
                                                              .Select(dp => new AxleCounterSectionsAxleCounterSectionDetectionPointsDetectionPoint 
                                                              { 
                                                                  Value = dp.Designation 
                                                              })
                                                              .ToArray()
                                         },
                                         Elements = new AxleCounterSectionsAxleCounterSectionElements 
                                         { 
                                             Element =  acs.Elements
                                                       .Select(e => new AxleCounterSectionsAxleCounterSectionElementsElement
                                                       { 
                                                           Value = e.Designation 
                                                       })
                                                       .ToArray()
                                         }
                                     })
                                     .ToArray()
            };
        }

        private TrackSections GetTrackSections()
        {
            List<elements.AcSection> trackSections = acLayout.AcSections
                                                     .Where(x => x.DetectionPoints.Count == 2)
                                                     .ToList();
            if (trackSections == null || trackSections.Count == 0)
            {
                return null;
            }

            return new TrackSections
            {
                TrackSection = trackSections
                               .Select(ts => new TrackSectionsTrackSection 
                               { 
                                    Status = status,
                                    Designation = ts.Designation,
                                    Limitation1 = ts.DetectionPoints[0].Designation,
                                    Limitation2 = ts.DetectionPoints[1].Designation,
                                    StationName = ts.StName,
                                    EmergencyStopGroup = emgsData.GetEmgsByElId(ts.Designation),
                                    TrackCausingLackOfClearance = fpsData.GetLocByElId(ts.Designation)
                               })
                               .ToArray()
            };
        }

        private string[] SplitTdts(string tdts, string log)
        {
            if (string.IsNullOrEmpty(tdts))
            {
                return null;
            }
            string[] split = tdts
                             .Split(Constants.splitSepar, StringSplitOptions.RemoveEmptyEntries)
                             .Select(t => GetElemDesignation(RddType.tdt, t))
                             .ToArray();
            foreach (string tdt in split)
            {
                if (!acLayout.AcSections.Any(ac => ac.Designation == tdt))
                {
                    ErrLogger.Error("Tdt not found in SL data", tdt, log);
                    Error = true;
                }
            }
            return split;
        }

        private List<string> SplitPoints(string pts, string log)
        {
            if (string.IsNullOrEmpty(pts))
            {
                return null;
            }
            List<string> split = pts
                             .Split(Constants.splitSepar, StringSplitOptions.RemoveEmptyEntries)
                             .Select(t => GetElemDesignation(RddType.tdt, t))
                             .ToList();
            foreach (string pt in split)
            {
                if (!acLayout.Points.Any(ac => ac.Designation == pt))
                {
                    ErrLogger.Error("Point not found in SL data", pt, log);
                    Error = true;
                }
            }
            return split;
        }

        private List<string> SplitPoints(List<string> pts, string log)
        {
            List<string> tmpList = new List<string>();
            if (pts.Count == 0)
            {
                return tmpList;
            }
            foreach (var pt in pts)
            {
                List<string> split = pt
                             .Split(Constants.splitSepar, StringSplitOptions.RemoveEmptyEntries)
                             .Select(t => GetElemDesignation(RddType.spsk, t))
                             .ToList();
                foreach (string ptSp in split)
                {
                    if (!acLayout.Points.Any(ac => ac.Designation == ptSp))
                    {
                        ErrLogger.Error("Point not found in SL data", ptSp, log);
                        Error = true;
                    }
                    else
                    {
                        tmpList.Add(ptSp);
                    }
                }
            }
            return tmpList;
        }

        private List<elements.AcSection> GetTdts(string tdts)
        {
            List<elements.AcSection> acSections = new List<elements.AcSection>();
            if (string.IsNullOrEmpty(tdts))
            {
                return acSections;
            }
            string[] split = tdts
                             .Split(Constants.splitSepar, StringSplitOptions.RemoveEmptyEntries)
                             .Select(t => GetElemDesignation(RddType.tdt, t))
                             .ToArray();
            foreach (string tdt in split)
            {
                var section = acLayout.AcSections
                              .Where(ac => ac.Designation == tdt)
                              .FirstOrDefault();
                if (section == null)
                {
                    ErrLogger.Error("Tdt not found in SL data", tdt, "");
                    Error = true;
                }
                else
                {
                    acSections.Add(section);
                }
            }
            return acSections;
        }

        private Dictionary<string, PointsPointTracksForDetectorLocking> GetTdls()
        {
            Dictionary<string, PointsPointTracksForDetectorLocking> rddTdls = 
                new Dictionary<string, PointsPointTracksForDetectorLocking>();
            
            foreach (var tdl in tdls)
            {
                var check = acLayout.Points
                            .Where(x => GetElemDesignation(RddType.spsk, tdl.Pt) == x.Designation)
                            .FirstOrDefault();
                if (check == null)
                {
                    ErrLogger.Error("Point not found in SL data", tdl.Pt, "TDL Table");
                    Error = true;
                    continue;
                }
                var tdts = GetTdts(string.Join(" ", tdl.AdjacentsTdt));
                var pts = SplitPoints(tdl.AdjacentsPoint, "TDL Table")
                          .Select(x => GetElemDesignation(RddType.spsk, x));
                List<string> foundTdls = pts.ToList();
                foreach (var tdt in tdts)
                {
                    if (!tdt.Elements.Any(e => pts.Any(x => x == e.Designation)))
                    {
                        foundTdls.Add(tdt.Designation);
                    }
                }
                if (foundTdls.Count == 0)
                {
                    rddTdls.Add(check.Designation, null);
                }
                else
                {
                    rddTdls.Add(check.Designation, new PointsPointTracksForDetectorLocking
                    {
                        TrackforDetectorLocking = foundTdls
                            .Select(x => new PointsPointTracksForDetectorLockingTrackforDetectorLocking 
                            {
                                Value = x
                            }).ToArray()
                    });
                }
            }
            return rddTdls;
        }

        private string GetElemDesignation(RddType rddType, string name, bool NamelowCase = false, bool PadZeros = true)
        {
            name = name.Split('-').Last();
            Regex re = new Regex(@"(\d+)([a-zA-Z]+)");
            Match result;
            int NameLength;
            if (name.Split('-').Length > 2)
            {
                string[] names = name.Split('-');
                result = re.Match(names[2]);
                if (result.Value.Length > 0)
                {
                    NameLength = 3 + result.Groups[2].Value.Length;
                }
                else
                {
                    NameLength = names[2].Length < 3 ? 3 : names[2].Length;
                }
                names[0] = names[0].ToLower();
                names[1] = names[1].ToLower();
                if (PadZeros)
                {
                    names[2] = NamelowCase ?
                        names[2].ToLower().PadLeft(NameLength, '0') :
                        names[2].ToUpper().PadLeft(NameLength, '0');
                }
                else
                {
                    names[2] = NamelowCase ? names[2].ToLower() : names[2].ToUpper();
                }
                return string.Join("-", names).Trim();
            }
            else if (name.Split('-').Length > 1)
            {
                string[] names = name.Split('-');
                result = re.Match(names[1]);
                if (result.Value.Length > 0)
                {
                    NameLength = 3 + result.Groups[2].Value.Length;
                }
                else
                {
                    NameLength = names[1].Length < 3 ? 3 : names[1].Length;
                }
                names[0] = names[0].ToLower();
                //names[1] = names[1].ToLower();
                if (PadZeros)
                {
                    names[1] = NamelowCase ?
                        names[1].ToLower().PadLeft(NameLength, '0') :
                        names[1].ToUpper().PadLeft(NameLength, '0');
                }
                else
                {
                    names[1] = NamelowCase ?
                        names[1].ToLower() :
                        names[1].ToUpper();
                }

                return string.Join("-", names).Trim();
            }
            else
            {
                result = re.Match(name);
                if (result.Value.Length > 0)
                {
                    NameLength = 3 + result.Groups[2].Value.Length;
                }
                else
                {
                    NameLength = name.Length < 3 ? 3 : name.Length;
                }
                if (PadZeros)
                {
                    name = NamelowCase ? name.ToLower().PadLeft(NameLength, '0') : name.ToUpper().PadLeft(NameLength, '0');
                }
                else
                {
                    name = NamelowCase ? name.ToLower() : name.ToUpper();
                }
                return rddType.ToString().ToLower() + "-" + acLayout.SigLayout.StID.ToLower() + "-" +
                      name.Trim();
            }
        }
        
        private PointsPointPointMachines GetPointMachines(elements.Point point)
        {
            var tsegs = acLayout.Tsegs
                       .Where(x => x.Vertex1.Element == point || x.Vertex2.Element == point)
                       .ToList();
            var sspsPt = SpeedProfiles.SpeedProfile
                         .Where(x => x.TrainTypes != null && 
                                     tsegs.Any(a => x.TrackSegments.TrackSegment.Any(t => t.Value == a.Id)))
                         .ToList();
            decimal maxSpeed = 0;
            if (sspsPt.Count > 0)
            {
                maxSpeed = sspsPt.Max(m => m.TrainTypes.TrainTyp.Select(x => x.SpeedLimit).Max());
            }         

            KindOfPMType kindOfPM = KindOfPMType.L710H;
            if (maxSpeed > 160 ||
                point.ExtType == ExtType.derailer ||
                point.ExtType == ExtType.trapPoint)
            {
                kindOfPM = KindOfPMType.L826H;
            }

            List<PointsPointPointMachinesPointMachine> pointMachines =
                        new List<PointsPointPointMachinesPointMachine>();
            int index = 1;
            foreach (var pm in point.PointMachines)
            {
                pointMachines.Add(new PointsPointPointMachinesPointMachine
                {
                    Designation = point.PointMachines.Count == 1 ?
                                    point.Designation :
                                    point.Designation + "-" + index.ToString(),
                    KindOfPM = kindOfPM,
                    TrackPosition = pm.TrackPosition
                });
                index++;
            }
            if (pointMachines.Count > 0)
            {
                return new PointsPointPointMachines { PointMachine = pointMachines.ToArray() };
            }
            else
            {
                ErrLogger.Error("Unable to get point machines", "Rdd", point.Designation);
                Error = true;
                return null;
            }
            
        }
        
        private SpeedProfiles GetSpeedProfiles()
        {
            if (ssps == null || ssps.Count == 0)
            {
                return null;
            }
            var sspGrps = ssps
                              .OrderBy(x => Convert.ToDouble(x.Km1))
                              .GroupBy(x => new
                              {
                                  x.Basic,
                                  x.AlC2,
                                  x.Fp,
                                  x.Fg,
                                  x.Cd100,
                                  x.Cd130,
                                  x.Cd150,
                                  x.Cd165,
                                  x.Cd180,
                                  x.Cd210,
                                  x.Cd225,
                                  x.Cd245,
                                  x.Cd275,
                                  x.Cd300
                              }).ToList();
            List<SpeedProfilesSpeedProfile> speedProfiles = new List<SpeedProfilesSpeedProfile>();
            int sspCount = 1;
            foreach (var sspGrp in sspGrps)
            {
                List<SpeedProfilesSpeedProfileTrainTypesTrainTyp> typesTrainTyps = new List<SpeedProfilesSpeedProfileTrainTypesTrainTyp>();
                List<TrackSegmentType> trackSegments = new List<TrackSegmentType>();
                foreach (var ssp in sspGrp)
                {
                    string[] tsegSplit = ssp.TrackSegment.Split('-');
                    tsegSplit[0] = tsegSplit[0].ToLower();
                    string tseg = string.Join("-", tsegSplit);
                    if (!acLayout.Tsegs.Any(x => x.Id == tseg))
                    {
                        ErrLogger.Error("Tseg not found on SL", "SSP", tseg);
                        Error = true;
                    }
                    trackSegments.Add(new TrackSegmentType
                    {
                        Value = tseg,
                        OperationalKM1 = ssp.Km1,
                        OperationalKM2 = ssp.Km2,
                    });
                }
                if (sspGrp.First().Remarks != null && sspGrp.First().Remarks.Trim() == "-")
                {
                    sspGrp.First().Remarks = null;
                }
                if (sspGrp.First().AlC2 != null && sspGrp.First().AlC2 != "-")
                {
                    typesTrainTyps.Add(new SpeedProfilesSpeedProfileTrainTypesTrainTyp
                    {
                        Item = "C2",
                        SpeedLimit = Convert.ToDecimal(sspGrp.First().AlC2)
                    });
                }
                if (sspGrp.First().Fp != null && sspGrp.First().Fp != "-")
                {
                    typesTrainTyps.Add(new SpeedProfilesSpeedProfileTrainTypesTrainTyp
                    {
                        Item = new SpeedProfilesSpeedProfileTrainTypesTrainTypTrainCategory
                        {
                            CategoryType = KindOfTrainCategory.freightTrainInP,
                            ReplaceCDAndBasicLimit = YesNoType.no
                        },
                        SpeedLimit = Convert.ToDecimal(sspGrp.First().Fp)
                    });
                }
                if (sspGrp.First().Fg != null && sspGrp.First().Fg != "-")
                {
                    typesTrainTyps.Add(new SpeedProfilesSpeedProfileTrainTypesTrainTyp
                    {
                        Item = new SpeedProfilesSpeedProfileTrainTypesTrainTypTrainCategory
                        {
                            CategoryType = KindOfTrainCategory.freightTrainInG,
                            ReplaceCDAndBasicLimit = YesNoType.no
                        },
                        SpeedLimit = Convert.ToDecimal(sspGrp.First().Fg)
                    });
                }
                if (sspGrp.First().Cd100 != null && sspGrp.First().Cd100 != "-")
                {
                    typesTrainTyps.Add(new SpeedProfilesSpeedProfileTrainTypesTrainTyp
                    {
                        Item = KindOfCantDeficiancy.Item100,
                        SpeedLimit = Convert.ToDecimal(sspGrp.First().Cd100)
                    });
                }
                if (sspGrp.First().Cd130 != null && sspGrp.First().Cd130 != "-")
                {
                    typesTrainTyps.Add(new SpeedProfilesSpeedProfileTrainTypesTrainTyp
                    {
                        Item = KindOfCantDeficiancy.Item130,
                        SpeedLimit = Convert.ToDecimal(sspGrp.First().Cd130)
                    });
                }
                if (sspGrp.First().Cd150 != null && sspGrp.First().Cd150 != "-")
                {
                    typesTrainTyps.Add(new SpeedProfilesSpeedProfileTrainTypesTrainTyp
                    {
                        Item = KindOfCantDeficiancy.Item150,
                        SpeedLimit = Convert.ToDecimal(sspGrp.First().Cd150)
                    });
                }
                if (sspGrp.First().Cd165 != null && sspGrp.First().Cd165 != "-")
                {
                    typesTrainTyps.Add(new SpeedProfilesSpeedProfileTrainTypesTrainTyp
                    {
                        Item = KindOfCantDeficiancy.Item165,
                        SpeedLimit = Convert.ToDecimal(sspGrp.First().Cd165)
                    });
                }
                if (sspGrp.First().Cd180 != null && sspGrp.First().Cd180 != "-")
                {
                    typesTrainTyps.Add(new SpeedProfilesSpeedProfileTrainTypesTrainTyp
                    {
                        Item = KindOfCantDeficiancy.Item180,
                        SpeedLimit = Convert.ToDecimal(sspGrp.First().Cd180)
                    });
                }
                if (sspGrp.First().Cd210 != null && sspGrp.First().Cd210 != "-")
                {
                    typesTrainTyps.Add(new SpeedProfilesSpeedProfileTrainTypesTrainTyp
                    {
                        Item = KindOfCantDeficiancy.Item210,
                        SpeedLimit = Convert.ToDecimal(sspGrp.First().Cd210)
                    });
                }
                if (sspGrp.First().Cd225 != null && sspGrp.First().Cd225 != "-")
                {
                    typesTrainTyps.Add(new SpeedProfilesSpeedProfileTrainTypesTrainTyp
                    {
                        Item = KindOfCantDeficiancy.Item225,
                        SpeedLimit = Convert.ToDecimal(sspGrp.First().Cd225)
                    });
                }
                if (sspGrp.First().Cd245 != null && sspGrp.First().Cd245 != "-")
                {
                    typesTrainTyps.Add(new SpeedProfilesSpeedProfileTrainTypesTrainTyp
                    {
                        Item = KindOfCantDeficiancy.Item245,
                        SpeedLimit = Convert.ToDecimal(sspGrp.First().Cd245)
                    });
                }
                if (sspGrp.First().Cd275 != null && sspGrp.First().Cd275 != "-")
                {
                    typesTrainTyps.Add(new SpeedProfilesSpeedProfileTrainTypesTrainTyp
                    {
                        Item = KindOfCantDeficiancy.Item275,
                        SpeedLimit = Convert.ToDecimal(sspGrp.First().Cd275)
                    });
                }
                if (sspGrp.First().Cd300 != null && sspGrp.First().Cd300 != "-")
                {
                    typesTrainTyps.Add(new SpeedProfilesSpeedProfileTrainTypesTrainTyp
                    {
                        Item = KindOfCantDeficiancy.Item300,
                        SpeedLimit = Convert.ToDecimal(sspGrp.First().Cd300)
                    });
                }
                if (!Enum.TryParse(sspGrp.First().Direction.ToLower(), out UpDownBothType upDownBoth))
                {
                    ErrLogger.Error("Unable to parse direction value", "SSP" + sspCount, sspGrp.First().Direction);
                    Error = true;
                }
                SpeedProfilesSpeedProfile speedProfile = new SpeedProfilesSpeedProfile
                {
                    Designation = "ssp-" + acLayout.SigLayout.StID + "-" + sspCount++.ToString().PadLeft(3, '0'),
                    SpeedMax = Convert.ToDecimal(sspGrp.First().Basic),
                    Remarks = sspGrp.First().Remarks,
                    DirectionAll = upDownBoth,

                };
                speedProfile.TrackSegments = new SpeedProfilesSpeedProfileTrackSegments
                {
                    TrackSegment = trackSegments.ToArray()
                };
                if (typesTrainTyps.Count > 0)
                {
                    speedProfile.TrainTypes = new SpeedProfilesSpeedProfileTrainTypes
                    {
                        TrainTyp = typesTrainTyps.ToArray()
                    };
                }
                speedProfiles.Add(speedProfile);
            }
            return new SpeedProfiles {SpeedProfile = speedProfiles.ToArray() };
        }

        private void CheckEmgs()
        {
            foreach (var emgs in emgsData.EmergencyStops)
            {
                if (!acLayout.Elements.Any(x => x.Designation == emgs.ElemId))
                {
                    ErrLogger.Error("Element defined in ES table not found on SL", emgs.ElemId, "ES Table");
                    Error = true;
                }
            }
            foreach (var signal in acLayout.Signals.Where(x => !x.Exclude && !x.NextStation))
            {
                if (!emgsData.EmergencyStops.Any(x => x.ElemId == signal.Designation))
                {
                    ErrLogger.Error("Signal not covered by emgs", signal.Designation, "ES Table");
                    Error = true;
                }
            }
        }
    }
}
