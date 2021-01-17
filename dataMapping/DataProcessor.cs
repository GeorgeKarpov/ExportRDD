using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LxAct = LevelCrossingsLevelCrossingLevelCrossingTracksLevelCrossingTrackActivationSectionsActivationSection;
using LxActRtChain = LevelCrossingsLevelCrossingLevelCrossingTracksLevelCrossingTrackActivationSectionsActivationSectionRouteChain;
using PwsTrack = StaffPassengerCrossingsStaffPassengerCrossingStaffPassengerCrossingTracksStaffPassengerCrossingTrack;
using PwsActSections = StaffPassengerCrossingsStaffPassengerCrossingStaffPassengerCrossingTracksStaffPassengerCrossingTrackActivationSections;
using PwsActSection = StaffPassengerCrossingsStaffPassengerCrossingStaffPassengerCrossingTracksStaffPassengerCrossingTrackActivationSectionsActivationSection;
using PwsActSectionDelays = StaffPassengerCrossingsStaffPassengerCrossingStaffPassengerCrossingTracksStaffPassengerCrossingTrackActivationSectionsActivationSectionActivationDelays;
using PwsActSectionDelay = StaffPassengerCrossingsStaffPassengerCrossingStaffPassengerCrossingTracksStaffPassengerCrossingTrackActivationSectionsActivationSectionActivationDelaysActivationDelay;
using PwsActRtChain = StaffPassengerCrossingsStaffPassengerCrossingStaffPassengerCrossingTracksStaffPassengerCrossingTrackActivationSectionsActivationSectionRouteChain;
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
        ExcelLib.WriteExcel writeExcel;

        List<ExcelLib.Route> routes;
        List<ExcelLib.Tdl> tdls;
        List<ExcelLib.Ssp> ssps;
        ExcelLib.FpsData fpsData;
        List<ExcelLib.Bg> bgs;
        ExcelLib.EmgsData emgsData;

        Dictionary<string ,ExcelLib.Lx> lxes;
        Dictionary<string, ExcelLib.Pws> pwses;

        private bool error;

        private AcLayout acLayout;
        private string rddId;
        private string rddVers;
        private string rddCreator;
        private TStatus status;

        SpeedProfiles SpeedProfiles;

        public event EventHandler<ProgressEventArgs> ReportProgress;

        protected virtual void OnReportProgress(ProgressEventArgs e)
        {
            ReportProgress?.Invoke(this, e);
        }

        ProgressEventArgs args = new ProgressEventArgs
        {
            Increment = 0
        };

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
            //LoadData();
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
            error = readExcel.ErrFound || readWord.ErrFound;
            args.Increment = 100;
            OnReportProgress(args);
        }

        public void ExportReport(string fileName)
        {
            List<ExcelLib.DocumentMetaData> docs = new List<ExcelLib.DocumentMetaData>
            {
                new ExcelLib.DocumentMetaData
                {
                    title = "PT1 Tables " + acLayout.SigLayout.StName,
                    date = DateTime.Now.Date,
                    docID = rddId,
                    version = rddVers,
                    creator = rddCreator
                },
                new ExcelLib.DocumentMetaData
                {
                    title = "Signalling Layout - " + acLayout.SigLayout.StName + " - " + acLayout.SigLayout.StID,
                    date = acLayout.SigLayout.Date,
                    docID = acLayout.SigLayout.DocId,
                    version = acLayout.SigLayout.Version,
                    creator = acLayout.SigLayout.Creator
                }
            };
            writeExcel = new ExcelLib.WriteExcel();
            docs.AddRange(DocsDescrs
                   .Select(x => new ExcelLib.DocumentMetaData 
                   {
                       docID = x.docID, 
                       creator = x.creator, 
                       date = x.date, 
                       title = x.title, 
                       version = x.version 
                   })
                   .ToList());
            writeExcel.ReportExcel(docs, fileName);
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
            CheckBgs();
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
                Routes = GetRoutes(),
                BlockInterfaces = GetBlockInterfaces(),
                BaliseGroups = GetBaliseGroups(),
                LevelCrossings = GetLevelCrossings(),
                StaffPassengerCrossings = GetPwss(),
                SpeedProfiles = SpeedProfiles,
                Platforms = GetPlatforms(),
                TrustedAreas =GetTrustedAreas(),
                EmergencyStopGroups = GetEmergencyStopGroups(),
                PermanentShuntingAreas = GetShuntingAreas(),
                Bridges = GetBridges(),
                Catenaries = GetCatenaries(),
                Radii = GetRadii(),
                CompoundRoutes = GetCompoundRoutes()
            };         
        }

        private CompoundRoutes GetCompoundRoutes()
        {
            ErrLogger.Information("Compound routes are not yet supported", "Export RDD");
            return null;
            //throw new NotImplementedException();
        }

        private Radii GetRadii()
        {
            ErrLogger.Information("Radii are not yet supported", "Export RDD");
            return null;
            //throw new NotImplementedException();
        }

        private Catenaries GetCatenaries()
        {
            ErrLogger.Information("Catenaries are not yet supported", "Export RDD");
            return null;
            //throw new NotImplementedException();
        }

        private Bridges GetBridges()
        {
            ErrLogger.Information("Bridges are not yet supported", "Export RDD");
            return null;
            //throw new NotImplementedException();
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
               
                var flankProtection = fpsData.GetFpByElId(point.GetShortName());
                                      //.Where(x => GetElemDesignation(point.RddType, x.Pt) == point.Designation)
                                      //.FirstOrDefault();
                PointsPoint rddPoint = new PointsPoint
                {
                    Status = status,
                    Designation = point.Designation,
                    KindOfPoint = point.Kind(),
                    InsidePSA = point.InsidePSA,
                    Lines = new PointsPointLines { Line = GetPointLines(point) },
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

        private PointsPointLinesLine[] GetPointLines(elements.Point point)
        {
            var foulingPoint = acLayout.FoulingPoints
                          .Where(f => point.Designation.Contains(f.Designation.Split('-').Last()))
                          .FirstOrDefault();
            PointsPointLinesLine[] lines;
            if (foulingPoint != null)
            {
                decimal fpRight = 0;
                decimal fpLeft = 0;
                if (foulingPoint.LineChanges)
                {
                    fpRight = foulingPoint.GetClosestLocation(point.KmpRight);
                    fpLeft = foulingPoint.GetClosestLocation(point.KmpLeft);
                }
                else
                {
                    fpRight = foulingPoint.Location;
                    fpLeft = foulingPoint.Location;
                }
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
                            FoulingPointLocation = fpRight.ToString()
                        },
                        new PointsPointLinesLine
                        {
                            KindOfPC = KindOfPCType.left,
                            LineID = point.LineIdLeft,
                            Location = point.KmpLeft.ToString(),
                            FoulingPointLocation = fpLeft.ToString()
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
            if (lines == null || lines.Length == 0)
            {
                return null;
            }
            else
            {
                return lines;
            }
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
                             Remarks = s.Remarks
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

        private Routes GetRoutes()
        {
            args.Increment = 100;
            OnReportProgress(args);
            if (routes == null || routes.Count == 0)
            {
                return null;
            }
            routes = SetRoutesDefault(routes);
            List<RoutesRoute> routesRoutes = new List<RoutesRoute>();
            foreach (var r in routes)
            {
                RoutesRoute route = new RoutesRoute
                {
                    Status = status,
                    Designation = r.Start + "_" + r.Dest,
                    KindOfRoute = GetRouteKind(r.Type),
                    Start = CheckElementExists(r.Start, "RT table - " + r.Id + ". Start", out _),
                    Destination = CheckElementExists(r.Dest, "RT table - " + r.Id + ". Destination", out _),
                    Default = IsRouteDefault(r.Default),
                    SafetyDistance = GetSafeDist(r),
                    SdLastElementID = GetSdLast(r),
                    StartAreaGroup = GetStartAreaGroup(r),
                    PointGroup = SplitPointsGroups(r.PointsGrps, "RT table - " + r.Id + ". Point group"),
                    DestinationArea = GetDestinationArea(r),
                    ActivateCrossingElementGroup = GetActivateCrossingElementGroup(r),

                };
                routesRoutes.Add(route);
                // EPT-8, EPT-12 implementation
                if (route.KindOfRoute == KindOfRouteType.both &&
                    (route.ActivateCrossingElementGroup != null || route.StartAreaGroup != null))
                {
                    route.KindOfRoute = KindOfRouteType.main;
                    RoutesRoute routeActShunt = new RoutesRoute()
                    {
                        Designation = route.Designation,
                        Status = route.Status,
                        KindOfRoute = KindOfRouteType.shunting,
                        Start = route.Start,
                        Destination = route.Destination,
                        Default = YesNoType.no,
                        SdLastElementID = route.SdLastElementID,
                        SafetyDistance = route.SafetyDistance,
                        PointGroup = route.PointGroup,
                        DestinationArea = route.DestinationArea,
                        ActivateCrossingElementGroup = null,
                        StartAreaGroup = null
                    };
                    routesRoutes.Add(routeActShunt);
                }   
            }
            return SetUnigueRtIds(new Routes { Route = routesRoutes.ToArray() });
        }

        private BlockInterfaces GetBlockInterfaces()
        {
            if (acLayout.BlockInterfaces == null || acLayout.BlockInterfaces.Count == 0)
            {
                return null;
            }
            return new BlockInterfaces
            {
                BlockInterface = acLayout.BlockInterfaces
                                 .Select(b => new BlockInterfacesBlockInterface
                                 {
                                     Status = status,
                                     Designation = b.Designation,
                                     KindOfBI = b.KindOfBI,
                                     PermissionHandling = b.PermissionHandling
                                 })
                                 .ToArray()
            };
        }

        private BaliseGroups GetBaliseGroups()
        {
            if (acLayout.BaliseGroups == null || acLayout.BaliseGroups.Count == 0)
            {
                return null;
            }
            return new BaliseGroups
            {
                BaliseGroup = acLayout.BaliseGroups
                              .Where(x => !x.Exclude && !x.NextStation)
                              .Select(b => GetBaliseGroupTypes(b))
                              .OrderBy(b => b.Location)
                              .ThenBy(b => b.Designation)
                              .ToArray()
            };
        }

        private LevelCrossings GetLevelCrossings()
        {
            if (acLayout.LevelCrossings == null || acLayout.LevelCrossings.Count == 0)
            {
                return null;
            }
            return new LevelCrossings 
            {
                LevelCrossing = acLayout.LevelCrossings
                                .Select(lc => GetLevelCrossing(lc))
                                .ToArray()
            };
        }

        private StaffPassengerCrossings GetPwss()
        {
            if (acLayout.Pws == null || acLayout.Pws.Count == 0)
            {
                return null;
            }
            return new StaffPassengerCrossings
            {
                StaffPassengerCrossing = acLayout.Pws
                                        .Select(pws => GetPws(pws))
                                        .ToArray()
            };
        }

        private Platforms GetPlatforms()
        {
            bool error = false;
            if (acLayout.Platforms == null || acLayout.Platforms.Count == 0)
            {
                return null;
            }
            List<PlatformsPlatform> platformsPlatforms = new List<PlatformsPlatform>();
            foreach (var platform in acLayout.Platforms)
            {
                PlatformsPlatform platformsPlatform = new PlatformsPlatform
                {
                    Status = status,
                    Designation = platform.Designation,
                    PositionOfPlatform = platform.PositionOfPlatform,
                    TrainRunningDirection = platform.TrainDirection,
                    PlatformHeight = GetPlatformHeight(platform, ref error),
                    Remarks = platform.Remarks
                };
                List<TrackSegmentType> trackSegments = new List<TrackSegmentType>();
                if (platform.Tsegs.Count == 1)
                {
                    trackSegments.Add(new TrackSegmentType 
                    { 
                        Value = platform.Tsegs[0].Id,
                        OperationalKM1 = platform.Km1.ToString(),
                        OperationalKM2 = platform.Km2.ToString()
                    });
                }
                else if (platform.Tsegs.Count > 1)
                {
                    for (int i = 0; i < platform.Tsegs.Count; i++)
                    {
                        if (i == 0)
                        {
                            trackSegments.Add(new TrackSegmentType
                            {
                                Value = platform.Tsegs[i].Id,
                                OperationalKM1 = platform.Km1.ToString(),
                            });
                        }
                        else if (i == platform.Tsegs.Count - 1)
                        {
                            trackSegments.Add(new TrackSegmentType
                            {
                                Value = platform.Tsegs[i].Id,
                                OperationalKM2 = platform.Km2.ToString()
                            });
                        }
                        else
                        {
                            trackSegments.Add(new TrackSegmentType
                            {
                                Value = platform.Tsegs[i].Id
                            });
                        }
                    }
                }
                platformsPlatform.TrackSegments = new PlatformsPlatformTrackSegments
                {
                    TrackSegment = trackSegments.ToArray()
                };
                platformsPlatforms.Add(platformsPlatform);
            }
            Platforms platforms = new Platforms
            {
                Platform = platformsPlatforms
                           .ToArray()
            };
            if (error)
            {
                this.error = true;
            }         
            return platforms;
        }

        private TrustedAreas GetTrustedAreas()
        {
            if (acLayout.TrustedAreas == null || acLayout.TrustedAreas.Count == 0)
            {
                return null;
            }
            List<TrustedAreasTrustedArea> trustedAreas = new List<TrustedAreasTrustedArea>();
            foreach (var tsta in acLayout.TrustedAreas)
            {
                TrustedAreasTrustedArea trustedArea = new TrustedAreasTrustedArea
                {
                    Status = status,
                    Designation = tsta.Id
                };
                List<elements.TSeg> tsegs = tsta.Tsegs
                                            .OrderBy(km => km.Vertex1.Km)
                                            .ThenBy(l => l.LineID)
                                            .ToList();
                List<TrackSegmentType> trackSegments = new List<TrackSegmentType>();
                if (tsegs.Count == 1)
                {
                    trackSegments.Add(new TrackSegmentType
                    {
                        Value = tsegs[0].Id,
                        OperationalKM1 = tsta.Km1.ToString(),
                        OperationalKM2 = tsta.Km2.ToString()
                    });
                }
                else if (tsegs.Count > 1)
                {
                    for (int i = 0; i < tsegs.Count; i++)
                    {
                        if (i == 0)
                        {
                            trackSegments.Add(new TrackSegmentType
                            {
                                Value = tsegs[i].Id,
                                OperationalKM1 = tsta.Km1.ToString(),
                            });
                        }
                        else if (i == tsegs.Count - 1)
                        {
                            trackSegments.Add(new TrackSegmentType
                            {
                                Value = tsegs[i].Id,
                                OperationalKM2 = tsta.Km2.ToString().ToString()
                            });
                        }
                        else
                        {
                            trackSegments.Add(new TrackSegmentType
                            {
                                Value = tsegs[i].Id
                            });
                        }
                    }
                }
                  
                trustedArea.TrackSegments = new TrustedAreasTrustedAreaTrackSegments 
                { 
                    TrackSegment = trackSegments.ToArray()
                };
                trustedAreas.Add(trustedArea);
            }
            return new TrustedAreas 
            { 
                TrustedArea = trustedAreas.ToArray() 
            };
        }

        private EmergencyStopGroups GetEmergencyStopGroups()
        {
            if (emgsData == null || emgsData.EmergencyStops == null || emgsData.EmergencyStops.Count == 0)
            {
                return null;
            }
            var tst = emgsData.EmergencyStops
                      .Where(s => acLayout.Signals.Any(a => a.Designation == s.ElemId))
                      .GroupBy(grp => grp.Id);
            return new EmergencyStopGroups 
            { 
                EmergencyStopGroup = tst
                                     .Select(em => new EmergencyStopGroupsEmergencyStopGroup
                                     {
                                        Status = status,
                                        Designation = em.Key,
                                        EmergencyStop = new EmergencyStopGroupsEmergencyStopGroupEmergencyStop 
                                        { 
                                            Value = em.Select(v => v.ElemId).ToArray()
                                        }
                                     })
                                     .ToArray() 
            };
        }

        private PermanentShuntingAreas GetShuntingAreas()
        {
            if (acLayout.Psas == null || acLayout.Psas.Count == 0)
            {
                return null;
            }
            return new PermanentShuntingAreas 
            {
                PermanentShuntingArea = acLayout.Psas
                                        .Select(p => new PermanentShuntingAreasPermanentShuntingArea 
                                        {
                                            Status = status,
                                            Designation = p.Id,
                                            LineID = p.Tseg.LineID,
                                            BeginOfPSA = p.Begin.ToString()
                                        })
                                        .ToArray()
            };
        }

        private PlatformHeightType GetPlatformHeight(elements.Platform platform, ref bool error)
        {
            var bdkPlat = acLayout.InputData.GetSPlatforms()
                          .Where(p => p.Split('\t').Count() == 6 && 
                                      p.Split('\t')[1].ToLower().Trim() == platform.StName.ToLower() &&
                                      p.Split('\t')[3] == platform.Track)
                          .FirstOrDefault();
            if (bdkPlat == null)
            {
                platform.Remarks = "Default Platform Height. Not found in 'Banedanmarks Netredegørelse 2018 Bilag 3.6A Perronlængder og - højder'";
                error = true;
                ErrLogger.Error("Platform not found in Platforms.dat", platform.StName, platform.Track);
                return PlatformHeightType.Item200;
            }
            int number = bdkPlat.Split('\t')[5].ToInt(platform.StName, platform.Track, ref error) * 10;

            PlatformHeightType platformHeight = platform.Height(number);

            platform.Remarks = "Real Platform Height is  "
                    + number.ToString() +
                    "mm. (Extracted from Banedanmarks Netredegørelse 2018 Bilag 3.6A Perronlængder og - højder)";
            return platformHeight;

        }

        private LevelCrossingsLevelCrossing GetLevelCrossing(elements.LevelCrossing levelCrossing)
        {
            LevelCrossingsLevelCrossing lc = new LevelCrossingsLevelCrossing
            {
                Status = status,
                Designation = levelCrossing.GetElemDesignation(PadZeros: false)
            };
            if (!lxes.ContainsKey(levelCrossing.Designation))
            {
                ErrLogger.Error("LX activation table not found", levelCrossing.Designation, "");
                error = true;
                return lc;
            }
            var excelLx = lxes[levelCrossing.Designation];
            lc.LevelCrossingTracks = GetLxTracks(levelCrossing, excelLx);
            var remarks = string.Join("; ", excelLx.Acts
                                         .Where(x => !string.IsNullOrEmpty(x.Remarks))
                                         .Select(s => s.Remarks)
                                         .ToList());
            if (!string.IsNullOrEmpty(remarks))
            {
                lc.Remarks = remarks;
            }           
            return ParseLxParams(lc, excelLx);
        }

        private StaffPassengerCrossingsStaffPassengerCrossing GetPws(elements.Pws pws)
        {
            StaffPassengerCrossingsStaffPassengerCrossing rddPws = new StaffPassengerCrossingsStaffPassengerCrossing
            {
                Status = status,
                Designation = pws.Designation,
                KindOfSPC = KindOfSPCType.passenger
            };
            if (!pwses.ContainsKey(pws.Designation))
            {
                ErrLogger.Error("LX activation table not found", pws.Designation, "");
                error = true;
                return rddPws;
            }
            var excelPws = pwses[pws.Designation];
            rddPws.StaffPassengerCrossingTracks = GetPwsTracks(pws, excelPws);
            return rddPws;
        }

        private LevelCrossingsLevelCrossingLevelCrossingTracks GetLxTracks(elements.LevelCrossing levelCrossing, ExcelLib.Lx excelLx)
        {
            if (levelCrossing.Tracks == null || levelCrossing.Tracks.Count == 0)
            {
                return null;
            }
            List<LevelCrossingsLevelCrossingLevelCrossingTracksLevelCrossingTrack> foundTracks = 
                new List<LevelCrossingsLevelCrossingLevelCrossingTracksLevelCrossingTrack>();
            foreach (var lxTrack in levelCrossing.Tracks)
            {
                var actTrack = excelLx.Acts
                               .Where(x => x.LxAxleCounterSectionID == lxTrack.LxAcSection);
                               //.FirstOrDefault();
                if (actTrack.Count() == 0)
                {
                    ErrLogger.Error("Activation for LX track not found", levelCrossing.Designation, lxTrack.Id);
                    this.error = true;
                    continue;
                }
                bool error = false;
                foundTracks.Add(new LevelCrossingsLevelCrossingLevelCrossingTracksLevelCrossingTrack
                {
                    Designation = lxTrack.Id,
                    TrackSegmentID = lxTrack.TSeg.Id,
                    LineID = lxTrack.TSeg.LineID,
                    Location = lxTrack.Location.ToString(),
                    BeginLCA = lxTrack.BeginLca.ToString(),
                    LengthLCA = lxTrack.LengthLca,
                    LengthLCASpecified = true,
                    ActivationSections = new LevelCrossingsLevelCrossingLevelCrossingTracksLevelCrossingTrackActivationSections
                    {
                        ActivationSection = actTrack
                                            .Select(act => new LxAct
                                            {
                                                RouteChain = new LxActRtChain 
                                                { 
                                                    RouteID = act.RouteChain.ToArray() 
                                                },
                                                ActivationDelayTime = act.ActivationDelayTime.ToDecimal("Lx delay", 
                                                                                        lxTrack.Id, ref error),
                                                ActivationDelayTimeSpecified = true,
                                                ActivationAxleCounterSectionID = act.ActivationAxleCounterSectionID,
                                                LxAxleCounterSectionID = act.LxAxleCounterSectionID
                                            })
                                            .ToArray()
                    }
                });
                if (error)
                {
                    this.error = true;
                }
            }
            return new LevelCrossingsLevelCrossingLevelCrossingTracks 
            { 
                LevelCrossingTrack = foundTracks.ToArray() 
            };
        }

        private StaffPassengerCrossingsStaffPassengerCrossingStaffPassengerCrossingTracks GetPwsTracks(elements.Pws pws, ExcelLib.Pws excelPws)
        {
            if (pws.Tracks == null || pws.Tracks.Count == 0)
            {
                return null;
            }
            List<PwsTrack> foundTracks =
                new List<PwsTrack>();
            foreach (var pwsTrack in pws.Tracks)
            {
                var actTrack = excelPws.Acts
                               .Where(x => x.LxAxleCounterSectionID == pwsTrack.LxAcSection)
                               .FirstOrDefault();
                if (actTrack == null)
                {
                    ErrLogger.Error("Activation for LX track not found", pws.Designation, pwsTrack.Id);
                    this.error = true;
                    continue;
                }

                bool error = false;
                foundTracks.Add(new PwsTrack
                {
                    Designation = pwsTrack.Id,
                    TrackSegmentID = pwsTrack.TSeg.Id,
                    LineID = pwsTrack.TSeg.LineID,
                    Location = pwsTrack.Location.ToString(),
                    BeginSPCA = pwsTrack.BeginLca.ToString(),
                    LengthSPCA = pwsTrack.LengthLca,
                    LengthSPCASpecified = true,
                    ActivationSections = new PwsActSections
                    {
                        ActivationSection = excelPws.Acts
                                            .Select(act => new PwsActSection
                                            {
                                                ActivationDelays = new PwsActSectionDelays
                                                {
                                                    ActivationDelay = act.Delays
                                                                      .Select(d => new PwsActSectionDelay
                                                                      {
                                                                          ActivationDelayTime = d.Delay
                                                                                                  .ToDecimal("Pws Delay", pwsTrack.Id, ref error),
                                                                          maxTrainSpeed = d.MaxSpeed
                                                                                            .ToDecimal("Pws Delay", pwsTrack.Id, ref error)
                                                                      }).ToArray(),
                                                },
                                                RouteChain = new PwsActRtChain
                                                {
                                                    RouteID = act.RouteChain.ToArray()
                                                },
                                                DeactivationAxleCounterSectionID = act.DeactivationAxleCounterSectionID,
                                                ActivationAxleCounterSectionID = act.ActivationAxleCounterSectionID,
                                                LxAxleCounterSectionID = act.LxAxleCounterSectionID
                                            })
                                            .ToArray()
                    },
                    SpeedIfUnprotectedUp = excelPws.Tracks
                                           .First().SpeedIfUnprotectedUp
                                           .ToDecimal("Pws Delay", pwsTrack.Id, ref error),
                    SpeedIfUnprotectedDown = excelPws.Tracks
                                             .First().SpeedIfUnprotectedDown
                                             .ToDecimal("Pws Delay", pwsTrack.Id, ref error),
                    SpeedIfUnprotectedUpSpecified = true,
                    SpeedIfUnprotectedDownSpecified = true
                });
                if (error)
                {
                    this.error = true;
                }
            }
            return new StaffPassengerCrossingsStaffPassengerCrossingStaffPassengerCrossingTracks
            {
                 StaffPassengerCrossingTrack = foundTracks.ToArray()
            };
        }

        private LevelCrossingsLevelCrossing ParseLxParams(LevelCrossingsLevelCrossing lc, ExcelLib.Lx excelLx)
        {
            if (!Enum.TryParse(excelLx.Params[ExcelLib.LxParam.Barriers]
                               .Split(Constants.splitSepar, StringSplitOptions.RemoveEmptyEntries).First(), out BarriersType barriers))
            {
                ErrLogger.Error("Unable to parse Barriers parameter", lc.Designation, excelLx.Params[ExcelLib.LxParam.Barriers]);
                error = true;
            }
            lc.Barriers = barriers;
            lc.MaxCloseTime = excelLx.Params[ExcelLib.LxParam.MaxCloseTime];

            if (!Enum.TryParse(excelLx.Params[ExcelLib.LxParam.ExpMaxClosTim], out YesNoNAType expMaxClose))
            {
                ErrLogger.Error("Unable to parse ExpMaxClosTim parameter", lc.Designation, excelLx.Params[ExcelLib.LxParam.ExpMaxClosTim]);
                error = true;
            }
            lc.ExpMaxClosTim = expMaxClose;

            if (!decimal.TryParse(excelLx.Params[ExcelLib.LxParam.MinOpenTime], out decimal decParam))
            {
                ErrLogger.Error("Unable to parse MinOpenTime parameter", lc.Designation, excelLx.Params[ExcelLib.LxParam.MinOpenTime]);
                error = true;
            }
            lc.MinOpenTime = decParam;

            bool maxWaitSpec = true;
            if (!decimal.TryParse(excelLx.Params[ExcelLib.LxParam.MaxWaitTime], out decParam))
            {
                if (!string.IsNullOrEmpty(excelLx.Params[ExcelLib.LxParam.MaxWaitTime]) &&
                    excelLx.Params[ExcelLib.LxParam.MaxWaitTime] != "no")
                {
                    ErrLogger.Error("Unable to parse MaxWaitTime parameter", lc.Designation, excelLx.Params[ExcelLib.LxParam.MaxWaitTime]);
                    error = true;
                }
                else
                {
                    maxWaitSpec = false;
                }              
            }
            lc.MaxWaitTime = decParam;
            lc.MaxWaitTimeSpecified = maxWaitSpec;

            if (!Enum.TryParse(excelLx.Params[ExcelLib.LxParam.InterfaceRTLS], out YesNoType interfaceRTLS))
            {
                ErrLogger.Error("Unable to parse InterfaceRTLS parameter", lc.Designation, excelLx.Params[ExcelLib.LxParam.InterfaceRTLS]);
                error = true;
            }
            lc.InterfaceRTLS = interfaceRTLS;
            if (interfaceRTLS == YesNoType.yes)
            {
                lc.DebouncingDelayRTLS = excelLx.Params[ExcelLib.LxParam.DebouncingDelayRTLS];
            }
            else
            {
                lc.DebouncingDelayRTLS = "n/a";
            }

            lc.DelayTimeRTLS = excelLx.Params[ExcelLib.LxParam.DelayTimeRTLS];
            lc.CheckTimerRTLS = excelLx.Params[ExcelLib.LxParam.CheckTimerRTLS];           
            lc.DelayTimeActivationBarriers = excelLx.Params[ExcelLib.LxParam.DelayTimeActivationBarriers];
            lc.DelayTimeExitBarriers = excelLx.Params[ExcelLib.LxParam.DelayTimeExitBarriers];
            lc.DelayTimeShortBarriers = excelLx.Params[ExcelLib.LxParam.DelayTimeShortBarriers];

            if (!Enum.TryParse(ExcelLib.Config.lxParamsConst["KeepAcousticWarningOn"], out YesNoType yesNoParam))
            {
                ErrLogger.Error("Unable to parse KeepAcousticWarningOn parameter", lc.Designation, ExcelLib.Config.lxParamsConst["KeepAcousticWarningOn"]);
                error = true;
            }
            lc.KeepAcousticWarningOn = yesNoParam;

            if (!decimal.TryParse(excelLx.Params[ExcelLib.LxParam.SupervisionTimer], out decParam))
            {
                ErrLogger.Error("Unable to parse SupervisionTimer parameter", lc.Designation, excelLx.Params[ExcelLib.LxParam.SupervisionTimer]);
                error = true;
            }
            lc.SupervisionTimer = decParam;

            if (!decimal.TryParse(excelLx.Params[ExcelLib.LxParam.WaitBeforeForcedUp], out decParam))
            {
                ErrLogger.Error("Unable to parse WaitBeforeForcedUp parameter", lc.Designation, excelLx.Params[ExcelLib.LxParam.WaitBeforeForcedUp]);
                error = true;
            }
            lc.WaitBeforeForcedUp = decParam;

            if (!Enum.TryParse(excelLx.Params[ExcelLib.LxParam.DeactivateOnClearance], out yesNoParam))
            {
                ErrLogger.Error("Unable to parse DeactivateOnClearance parameter", lc.Designation, excelLx.Params[ExcelLib.LxParam.DeactivateOnClearance]);
                error = true;
            }
            lc.DeactivateOnClearance = yesNoParam;

            bool permitSpec = true;
            if (!decimal.TryParse(ExcelLib.Config.lxParamsConst["PermittedSpeed"], out decParam))
            {
                ErrLogger.Error("Unable to parse PermittedSpeed parameter", lc.Designation, ExcelLib.Config.lxParamsConst["PermittedSpeed"]);
                error = true;
                permitSpec = false;
            }
            lc.PermittedSpeed = decParam;
            lc.PermittedSpeedSpecified = permitSpec;

            permitSpec = true;
            if (!decimal.TryParse(ExcelLib.Config.lxParamsConst["PermittedSpeedInOverlap"], out decParam))
            {
                ErrLogger.Error("Unable to parse PermittedSpeedInOverlap parameter", lc.Designation, ExcelLib.Config.lxParamsConst["PermittedSpeedInOverlap"]);
                error = true;
                permitSpec = false;
            }
            lc.PermittedSpeedInOverlap = decParam;
            lc.PermittedSpeedInOverlapSpecified = permitSpec;

            bool barClosTimeSpec = true;
            if (!decimal.TryParse(excelLx.Params[ExcelLib.LxParam.BarrierClosingTime], out decParam))
            {
                ErrLogger.Error("Unable to parse BarrierClosingTime parameter", lc.Designation, excelLx.Params[ExcelLib.LxParam.BarrierClosingTime]);
                error = true;
                barClosTimeSpec = false;
            }
            lc.BarrierClosingTime = decParam;
            lc.BarrierClosingTimeSpecified = barClosTimeSpec;

            return lc;
        }

        private BaliseGroupsBaliseGroup GetBaliseGroupTypes(elements.BaliseGroup balise)
        {
            BaliseGroupsBaliseGroup baliseGroup = new BaliseGroupsBaliseGroup 
            { 
                Status = status,
                Designation = balise.Designation,
                InsidePSA = balise.InsidePSA,
                TrackSegmentID = balise.GetTsegId(),
                LineID = balise.LineID,
                Location = balise.Location.ToString(),
                Orientation = balise.Orientation,
                BorderBaliseSpecified = false
            };
            List<BaliseGroupsBaliseGroupBaliseGroupTypesKindOfBG> kindOfBG =
                    new List<BaliseGroupsBaliseGroupBaliseGroupTypesKindOfBG>();
           
            ExcelLib.Bg check = bgs
                                .Where(x => x.Design == balise.Designation)
                                .FirstOrDefault();
            if (check != null)
            {
                YesNoType border = YesNoType.no;
                bool borderSpec = false;
                if (!Enum.TryParse(check.Orient, out UpDownSingleType orient))
                {
                    ErrLogger.Error("Unable to parse Orientation value", check.Orient, "BG table " + check.Design);
                    error = true;
                }
                if (!Enum.TryParse(check.Border, out border))
                {
                    ErrLogger.Error("Unable to parse Border value", check.Border, "BG table " + check.Design);
                    error = true;
                    borderSpec = false;
                }
                else if (border == YesNoType.yes)
                {
                    borderSpec = true;
                }
                Dictionary<string, string> bgTypeMap = acLayout.InputData.GetBGTypes();
                foreach (var bgType in check.BgGroupeTypes)
                {
                    if (!GetKindOfBG(bgType.KindOfBG, bgTypeMap, out KindOfBG kind))
                    {
                        ErrLogger.Error("Unable to parse Kind of BG value", bgType.KindOfBG, "BG table " + check.Design);
                        error = true;
                    }
                    if (!Enum.TryParse(bgType.Direction, out NominalReverseBothType direction))
                    {
                        ErrLogger.Error("Unable to parse Orientation value", check.Design, "BG table " + check.Design);
                        error = true;
                    }
                    bool dupSpec = true;
                    if (!Enum.TryParse(bgType.Duplicated, out YesNoType duplicated))
                    {
                        dupSpec = false;
                    }
                    
                    kindOfBG.Add(new BaliseGroupsBaliseGroupBaliseGroupTypesKindOfBG
                    {
                        duplicatedSpecified = dupSpec,
                        direction = direction,
                        duplicated = duplicated,
                        Value = kind
                    });
                }
                baliseGroup.BaliseGroupTypes = new BaliseGroupsBaliseGroupBaliseGroupTypes 
                { 
                    KindOfBG = kindOfBG.ToArray()
                };
                baliseGroup.Orientation = orient;
                baliseGroup.BorderBalise = border;
                baliseGroup.BorderBaliseSpecified = borderSpec;
            }
            else
            {
                baliseGroup.BaliseGroupTypes = new BaliseGroupsBaliseGroupBaliseGroupTypes
                {
                    KindOfBG = balise.BgTypes
                           .Select(bgt => new BaliseGroupsBaliseGroupBaliseGroupTypesKindOfBG
                           {
                               duplicatedSpecified = false,
                               direction = bgt.Direction,
                               Value = bgt.KindOfBG
                           }).ToArray()
                };
            }
            return baliseGroup;
        }

        private bool GetKindOfBG(string bgTypeInput, Dictionary<string, string> bgTypeMap, out KindOfBG kindOfBG)
        {
            kindOfBG = KindOfBG.Positioningbalisegroup;
            var test = string.Join("", bgTypeInput.ToLower().Split(Constants.splitSepar, StringSplitOptions.RemoveEmptyEntries));
            if (bgTypeMap.ContainsKey(test))
            {
                if (Enum.TryParse(bgTypeMap[test], out KindOfBG kind))
                {
                    kindOfBG = kind;
                    return true;
                }
            }
            return false;
        }

        private void CheckBgs()
        {
            if (bgs != null && bgs.Count > 0)
            {
                foreach (var item in bgs)
                {
                    CheckElementExists(item.Design, "BG table", out _);
                }
            }
        }

        private List<ExcelLib.Route> SetRoutesDefault(List<ExcelLib.Route> routes)
        {
            var grp = routes
                      .GroupBy(g => g.Start + "_" + g.Dest)
                      .Where(group => group.Count() == 1);
            foreach (var item in grp)
            {
                item.First().Default = "x";
            }
            return routes;
        }

        private Routes SetUnigueRtIds(Routes routes)
        {
            List<RoutesRoute> dubplicatesRoutes = routes.Route.GroupBy(s => s.Designation)
                                                        .SelectMany(grp => grp.Skip(1))
                                                        .Distinct()
                                                        .ToList();
            foreach (RoutesRoute duproute in dubplicatesRoutes)
            {
                List<RoutesRoute> sameRoutes = routes.Route
                                  .Where(x => x.Designation == duproute.Designation)
                                  .OrderBy(x => x.Default)
                                  .ToList();
                for (int j = 0; j < sameRoutes.Count; j++)
                {
                    if (j > 0)
                    {
                        sameRoutes[j].Designation += "_" + (j + 1).ToString();
                    }
                }
            }
            return routes;
        }

        private RoutesRouteDestinationArea GetDestinationArea(ExcelLib.Route r)
        {
            string destArea = null;
            if (r.ExtDest.ToLower().Trim() != "n" &&
                r.ExtDest.ToLower().Trim() != "-")
            {
                destArea = CheckElementExists(r.ExtDest.Trim(), "RT table - " + r.Id + ". Ext destination area", out _);
            }
            if (destArea == null || destArea.Count() == 0)
            {
                return null;
            }
            return new RoutesRouteDestinationArea { ElementID = destArea };
        }

        private string GetSdLast(ExcelLib.Route r)
        {
            string sdLast = null;
            if (r.SdLast.ToLower().Trim() != "n" &&
                r.SdLast.ToLower().Trim() != "-")
            {
                sdLast = CheckElementExists(GetElemDesignation(RddType.spsk, r.SdLast.Trim()), 
                            "RT table - " + r.Id + ". Sd last element", out bool exists, false);
                if (!exists)
                {
                    sdLast = CheckElementExists(GetElemDesignation(RddType.tdt, r.SdLast.Trim()),
                            "RT table - " + r.Id + ". Sd last element", out _);
                }
            }
            return sdLast;
        }

        private string GetSafeDist(ExcelLib.Route r)
        {
            string safeDist = null;
            if (r.SafeDist.ToLower().Trim() != "n" &&
                r.SafeDist.ToLower().Trim() != "-" &&
                r.SafeDist.ToLower().Trim() != "integrated")
            {
                safeDist = CheckElementExists(GetElemDesignation(RddType.spsk, r.SafeDist.Trim()), 
                            "RT table - " + r.Id + ". Ext destination area", out bool exists, false);
                if (!exists)
                {
                    safeDist = CheckElementExists(GetElemDesignation(RddType.tdt, r.SafeDist.Trim()),
                            "RT table - " + r.Id + ". Ext destination area", out _);
                }
            }
            return safeDist;
        }

        private RoutesRouteStartAreaGroup GetStartAreaGroup(ExcelLib.Route route)
        {
            if (string.IsNullOrEmpty(route.StartAreas))
            {
                return null;
            }
            var startArea = route.StartAreas
                       .Split(Constants.splitSepar, StringSplitOptions.RemoveEmptyEntries)
                       .Where(x => x.ToLower().Trim() != "n" &&
                                   x.Trim() != "-")
                       .Select(s => new RoutesRouteStartAreaGroupStartArea 
                       { 
                           ElementID = GetStartAreaValue(s, route)
                       })
                       .ToArray();
            if (startArea == null || startArea.Count() == 0)
            {
                return null;
            }
            return new RoutesRouteStartAreaGroup { StartArea = startArea };
        }

        private string GetStartAreaValue(string input, ExcelLib.Route r)
        {
            string startArea = CheckElementExists(input, "RT table - " + r.Id + ". Start Area Group", out bool exists, false);
            if (!exists)
            {
                startArea = CheckElementExists(GetElemDesignation(RddType.ovk, input),
                            "RT table - " + r.Id + ". Start Area Group", out exists, false);
                startArea = GetElemDesignation(RddType.ovk, startArea, PadZeros: false);
                if (!exists)
                {
                    startArea = CheckElementExists(GetElemDesignation(RddType.va, input),
                            "RT table - " + r.Id + ". Start Area Group", out exists, false);
                    startArea = GetElemDesignation(RddType.ovk, startArea, PadZeros: false);
                    if (!exists)
                    {
                        startArea = CheckElementExists(GetElemDesignation(RddType.spsk, input),
                                "RT table - " + r.Id + ". Start Area Group", out _, true);
                    }
                }
            }                             
            return startArea;
        }

        private string GetActivateCrossingElement(string input, ExcelLib.Route r)
        {
            string actCross = CheckElementExists(input, "RT table - " + r.Id + ". Activation crossing element group", out bool exists, false);
            if (!exists)
            {
                actCross = CheckElementExists(GetElemDesignation(RddType.ovk, input),
                            "RT table - " + r.Id + ". Activation crossing element group", out exists, false);
                actCross = GetElemDesignation(RddType.ovk, actCross, PadZeros: false);
                if (!exists)
                {
                    actCross = CheckElementExists(GetElemDesignation(RddType.va, input),
                            "RT table - " + r.Id + ". Activation crossing element group", out exists, false);
                    actCross = GetElemDesignation(RddType.ovk, actCross, PadZeros: false);
                    if (!exists)
                    {
                        actCross = CheckElementExists(GetElemDesignation(RddType.spsk, input),
                                "RT table - " + r.Id + ". Activation crossing element group", out _, true);
                    }
                }
            }
            return actCross;
        }

        private RoutesRouteActivateCrossingElementGroup GetActivateCrossingElementGroup (ExcelLib.Route route)
        {
            if (string.IsNullOrEmpty(route.ActCross))
            {
                return null;
            }
            var actCrossGrp = route.ActCross
                       .Split(Constants.splitSepar, StringSplitOptions.RemoveEmptyEntries)
                       .Where(x => x.ToLower().Trim() != "n" &&
                                   x.Trim() != "-")
                       .Select(s => new RoutesRouteActivateCrossingElementGroupActivateCrossingElement
                       {
                             Value  = GetActivateCrossingElement(s, route)
                       })
                       .ToArray();
            if (actCrossGrp == null || actCrossGrp.Count() == 0)
            {
                return null;
            }
            return new RoutesRouteActivateCrossingElementGroup { ActivateCrossingElement = actCrossGrp };
        }

        private KindOfRouteType GetRouteKind(string value)
        {
            if (!Enum.TryParse(value, out KindOfRouteType type))
            {
                ErrLogger.Error("Unable to parse type of route", value, "RT table");
                error = true;
            }
            return type;
        }

        private YesNoType IsRouteDefault(string value)
        {
            if (value != null && value.ToLower().Trim() == "x")
            {
                return YesNoType.yes;
            }
            return YesNoType.no;
        }

        private string[] SplitTdts(string tdts, string log)
        {
            if (string.IsNullOrEmpty(tdts))
            {
                return null;
            }
            string[] split = tdts
                             .Split(Constants.splitSepar, StringSplitOptions.RemoveEmptyEntries)
                             .Select(t => CheckElementExists(GetElemDesignation(RddType.tdt, t), log, out _))
                             .ToArray();
            return split;
        }

        private RoutesRoutePointGroup SplitPointsGroups(string pts, string log)
        {
            if (string.IsNullOrEmpty(pts))
            {
                return null;
            }
            var split = pts
                             .Split(Constants.splitSepar, StringSplitOptions.RemoveEmptyEntries)
                             .Where(x => x.ToLower().Trim() != "n" &&
                                         x.Trim() != "-")
                             .Select(p => new
                             {
                                 point = CheckElementExists(GetElemDesignation(RddType.spsk, p.Split('-').First()), log, out _),
                                 pos = p.Split('-').Last()
                             }
                             )
                             .ToList();
            List<RoutesRoutePointGroupPoint> groupPoints = new List<RoutesRoutePointGroupPoint>();
            foreach (var item in split)
            {
                string position = "";
                if (item.pos.ToUpper() == "L")
                {
                    position = "left";
                }
                else if (item.pos.ToUpper() == "R")
                {
                    position = "right";
                }
                if (!Enum.TryParse(position, out LeftRightType leftRightType))
                {
                    ErrLogger.Error("Unable to parse point position from points group", item.point, log);
                    error = true;
                }
                groupPoints.Add(new RoutesRoutePointGroupPoint 
                { 
                    Value = item.point, 
                    RequiredPosition = leftRightType 
                });
            }
            if (groupPoints == null || groupPoints.Count() == 0)
            {
                return null;
            }
            return new RoutesRoutePointGroup { Point = groupPoints.ToArray() };
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
                             .Select(t => CheckElementExists(GetElemDesignation(RddType.spsk, t), log, out _))
                             .ToList();
                tmpList.AddRange(split);
            }
            return tmpList;
        }

        private List<elements.AcSection> GetTdts(string tdts, string log)
        {
            List<elements.AcSection> acSections = new List<elements.AcSection>();
            if (string.IsNullOrEmpty(tdts))
            {
                return acSections;
            }
            string[] split = tdts
                             .Split(Constants.splitSepar, StringSplitOptions.RemoveEmptyEntries)
                             .Select(t => CheckElementExists(GetElemDesignation(RddType.tdt, t), log, out _))
                             .ToArray();
            foreach (string tdt in split)
            {
                var section = acLayout.AcSections
                              .Where(ac => ac.Designation == tdt)
                              .FirstOrDefault();
                if (section != null)
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
                var check = CheckElementExists(GetElemDesignation(RddType.spsk, tdl.Pt), "TDL Table", out _);

                var tdts = GetTdts(string.Join(" ", tdl.AdjacentsTdt), "TDL Table");
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
                    rddTdls.Add(check, null);
                }
                else
                {
                    rddTdls.Add(check, new PointsPointTracksForDetectorLocking
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

        private string CheckElementExists(string id, string log, out bool exists, bool reportError = true)
        {
            exists = false;
            if (acLayout.Elements.Any(x => x.Designation == id))
            {
                exists = true;
            }
            else if (reportError)
            {
                ErrLogger.Error("Element not found on SL", id, log);
                error = true;
            }
            return id;
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
                    names[2] = NamelowCase ? names[2].ToLower().TrimStart(new Char[] { '0' }) : names[2].ToUpper().TrimStart(new Char[] { '0' });
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
                        names[1].ToLower().TrimStart(new char[] { '0' }) :
                        names[1].ToUpper().TrimStart(new char[] { '0' });
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
                    name = NamelowCase ? name.ToLower().TrimStart(new Char[] { '0' }) : name.ToUpper().TrimStart(new Char[] { '0' });
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
                error = true;
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
                        error = true;
                    }
                    trackSegments.Add(new TrackSegmentType
                    {
                        Value = tseg,
                        OperationalKM1 = ssp.Km1.Contains(".") ? 
                                          ssp.Km1 : 
                                          string.Format("{0:0.000}", ssp.Km1.ToDecimal("SSP Table", ssp.Km1, ref error)/1000),
                        OperationalKM2 = ssp.Km2.Contains(".") ?
                                          ssp.Km2 :
                                          string.Format("{0:0.000}", ssp.Km2.ToDecimal("SSP Table", ssp.Km2, ref error)/1000),
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
                    error = true;
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
                    error = true;
                }
            }
            foreach (var signal in acLayout.Signals.Where(x => !x.Exclude && !x.NextStation))
            {
                if (!emgsData.EmergencyStops.Any(x => x.ElemId == signal.Designation))
                {
                    ErrLogger.Error("Signal not covered by emgs", signal.Designation, "ES Table");
                    error = true;
                }
            }
        }

        public bool HasErrors()
        {
           return error;
        }
    }
}
