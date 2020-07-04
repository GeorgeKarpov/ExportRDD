using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
//using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using LXactSection = LevelCrossingsLevelCrossingLevelCrossingTracksLevelCrossingTrackActivationSectionsActivationSection;

namespace ExpPt1
{
    public class Export : IDisposable
    {
        private string docId = "";
        private string docVrs = "";
        public string stationID = "";
        private string Version = "";
        private string DocID = "";
        private string stationName = "";
        private string ZeroLevelLine = "";
        private List<CompoundRoutesCompoundRoute> cmproutes = new List<CompoundRoutesCompoundRoute>();
        protected DocumentCollection acDocMgr;
        protected Database db;
        public Document acDoc;
        protected string assemblyPath;
        protected string dwgPath;
        protected string dwgDir;
        protected string dwgFileName;
        private string orderRddFileName;
        protected List<RailwayLine> RailwayLines;
        private bool copyLevel;
        protected Dictionary<string, string> BlocksToGet;
        protected TStatus Status;
        protected Dictionary<string, bool> checkData;
        private XmlWriterSettings settings;
        private List<LinesLine> lines;
        protected List<TrackSegmentTmp> TrackSegmentsTmp;
        protected List<TrackSegmentsTrackSegment> trcksegments;
        protected List<TrustedArea> TrustedAreas;
        protected List<TrackLine> TracksLines;
        protected List<string> ExportCigClosure;
        private List<string> ExportPoints;
        protected ReadExcel.Excel excel;
        private Dictionary<string, string> loadFiles;
        private List<TFileDescr> Documents;
        protected BlockProperties blckProp;
        private List<AcSection> acSections;
        private Dictionary<string, ReadExcel.XlsLxActivation> LxsActivations;
        protected List<Block> blocks;
        public List<Block> Blocks { get; set; }
        protected bool blocksErr;
        private FrmStation frmStation;
        protected List<PointsPoint> points;
        protected List<SignalsSignal> signals;
        protected List<SpeedProfilesSpeedProfile> speedProfiles;
        private List<AxleCounterSectionsAxleCounterSection> acsections;
        protected List<PSA> pSAs;

        public Export(string dwgPath)
        {
            this.assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);         
            this.acDocMgr = AcadApp.DocumentManager;
            this.acDoc = this.acDocMgr.MdiActiveDocument;
            this.db = this.acDoc.Database;
            this.dwgPath = dwgPath;
            this.dwgDir = Path.GetDirectoryName(dwgPath);
            this.dwgFileName = Path.GetFileNameWithoutExtension(dwgPath);
            this.settings = new XmlWriterSettings()
            {
                IndentChars = "\t",
                Indent = true
            };
            this.RailwayLines = new List<RailwayLine>();
            this.BlocksToGet = new Dictionary<string, string>();
            this.Status = new TStatus();
            ErrLogger.Prefix = Path.GetFileNameWithoutExtension(this.dwgPath);
            ErrLogger.StartTmpLog(this.dwgDir);
            ErrLogger.Information(Path.GetFileNameWithoutExtension(dwgPath), "Processed SL");
            ErrLogger.Error(Path.GetFileNameWithoutExtension(dwgPath), "Processed SL", "");
            ReadBlocksDefinitions();
            this.blocksErr = false;
            this.blocks = this.GetBlocks(ref this.blocksErr);
            this.acSections = new List<AcSection>();
            pSAs = new List<PSA>();
        }

        [CommandMethod("ReplacePlatorms")]
        public void ReplacePlatforms()
        {
            ReadBlocksDefinitions();
            List<Block> oldPlatforms = GetBlocks("Platform", db);
            foreach (Block block in oldPlatforms)
            {

                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    string blockFileName = @"C:\3.TRDD\DynamicBlocks\Blks_Dynamic.dwg";
                    if (!File.Exists(blockFileName))
                    {
                        AcadApp.ShowAlertDialog("File '" + blockFileName + "' doesn't exist");
                        return;
                    }
                    CopyBlockFromFile(this.assemblyPath + Constants.cfgFolder + @"\Blks_Dynamic.dwg", "Platform_Dynamic");

                    BlockReference blkRefInserted =
                        (BlockReference)tr.GetObject(InsertBlock("Platform_Dynamic",
                                                                  block.BlkRef.Position.X,
                                                                  block.BlkRef.Position.Y),
                                                     OpenMode.ForWrite);
                    CopyAtributtes(block.BlkRef, blkRefInserted, tr);
                    BlockReference Erase = (BlockReference)tr.GetObject(block.BlkRef.Id,
                                                                OpenMode.ForWrite);
                    DynamicBlockReferencePropertyCollection properties =
                        blkRefInserted.DynamicBlockReferencePropertyCollection;
                    foreach (DynamicBlockReferenceProperty prt in properties)
                    {
                        if (prt.PropertyName == "Width")
                        {
                            prt.Value =
                                Math.Abs(block.BlkRef.GeometricExtents.MaxPoint.X -
                                         block.BlkRef.GeometricExtents.MinPoint.X);
                        }
                    }
                    if (LayerExists("KMP"))
                    {
                        block.BlkRef.Layer = "KMP";
                    }

                    Erase.Erase();
                    tr.Commit();
                }
            }
        }

        public void ExportBlocks()
        {
            SaveFileDialog saveFile = new SaveFileDialog("Export Blocks", null, "dat", "ExportBlocks",
                                                            SaveFileDialog.SaveFileDialogFlags.DefaultIsFolder);
            if (saveFile.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            //Dictionary<string, string> blocks = new Dictionary<string, string>();
            List<string> attributes = new List<string>();

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                foreach (ObjectId btrId in bt)
                {
                    BlockTableRecord btr = (BlockTableRecord)trans.GetObject(btrId, OpenMode.ForRead);
                    ObjectIdCollection aRefIds = new ObjectIdCollection();
                    if (btr.IsDynamicBlock)
                    {
                        var blockIds = btr.GetAnonymousBlockIds();
                        foreach (ObjectId BlkId in blockIds)
                        {
                            BlockTableRecord btr2 =
                                (BlockTableRecord)trans.GetObject(BlkId, OpenMode.ForRead, false, false);
                            ObjectIdCollection aRefIds2 = btr2.GetBlockReferenceIds(true, true);
                            foreach (ObjectId id in aRefIds2)
                            {
                                aRefIds.Add(id);
                            }
                        }
                        if (blockIds.Count == 0)
                        {
                            aRefIds = btr.GetBlockReferenceIds(false, true);
                        }
                    }
                    else
                    {
                        aRefIds = btr.GetBlockReferenceIds(false, true);
                    }

                    foreach (ObjectId RefId in aRefIds)
                    {

                        BlockReference blkRef = (BlockReference)trans.GetObject(RefId, OpenMode.ForRead);
                        Dictionary<string, Attribute> Attributes = GetAttributes(blkRef);
                        foreach (KeyValuePair<string, Attribute> att in Attributes)
                        {
                            attributes.Add(btr.Name + "\t" + att.Value.Name);
                        }
                    }
                }
            }
            attributes.Sort();
            File.WriteAllLines(saveFile.Filename, attributes);
        }

        [CommandMethod("CheckIntersSections")]
        public void TestIntersection()
        {
            using (Transaction transaction = this.db.TransactionManager.StartTransaction())
            {
                PromptSelectionResult selection = this.acDoc.Editor.GetSelection();
                if (selection.Status == PromptStatus.Cancel)
                    return;
                SelectionSet selectionSet = selection.Value;
                if (selectionSet.Count != 2)
                {
                    Application.ShowAlertDialog("Two objects must be selected.");
                }
                else
                {
                    if (this.ObjectsIntersects(transaction.GetObject(selectionSet[0].ObjectId, (OpenMode)0) as Entity, transaction.GetObject(selectionSet[1].ObjectId, (OpenMode)0) as Entity, (Intersect)0, false))
                        Application.ShowAlertDialog("Intersect!");
                    else
                        Application.ShowAlertDialog("Not Intersect!");
                    transaction.Commit();
                }
            }
        }

        private void CopyAtributtes(BlockReference fromBlkRef, BlockReference toBlkRef, Transaction acTrans)
        {
            AttributeCollection attsOld = fromBlkRef.AttributeCollection;
            AttributeCollection attsNew = toBlkRef.AttributeCollection;
            Dictionary<string, AttributeReference> dicAttNew =
            new Dictionary<string, AttributeReference>();
            foreach (ObjectId arId in attsNew)
            {
                AttributeReference attRef =
                    (AttributeReference)acTrans.GetObject(arId, OpenMode.ForWrite);
                dicAttNew.Add(attRef.Tag.ToUpper(), attRef);

            }
            foreach (ObjectId arId in attsOld)
            {
                AttributeReference attRef =
                    (AttributeReference)acTrans.GetObject(arId, OpenMode.ForRead);
                string tag = attRef.Tag;
                if (attRef.Tag == "PLATFORM_1")
                {
                    tag = "NAME";
                }
                if (dicAttNew.ContainsKey(tag.ToUpper()))
                {
                    dicAttNew[tag.ToUpper()].TextString = attRef.TextString;
                }
            }
        }

        public void CopyAtributtesOnDrw()
        {
            using (Transaction acTrans = db.TransactionManager.StartTransaction())
            {
                PromptSelectionResult acSSPrompt = acDoc.Editor.GetSelection();
                if (acSSPrompt.Status == PromptStatus.OK)
                {
                    SelectionSet acSSet = acSSPrompt.Value;
                    if (acSSet.Count != 2)
                    {
                        AcadApp.ShowAlertDialog("Two objects must be selected.");
                        return;
                    }
                    BlockReference fromBlkRef = acTrans.GetObject(acSSet[0].ObjectId,
                                                                OpenMode.ForRead) as BlockReference;
                    BlockReference toBlkRef = acTrans.GetObject(acSSet[1].ObjectId,
                                                                OpenMode.ForWrite) as BlockReference;
                    AttributeCollection attsOld = fromBlkRef.AttributeCollection;
                    AttributeCollection attsNew = toBlkRef.AttributeCollection;
                    Dictionary<string, AttributeReference> dicAttNew =
                    new Dictionary<string, AttributeReference>();
                    foreach (ObjectId arId in attsNew)
                    {
                        AttributeReference attRef =
                            (AttributeReference)acTrans.GetObject(arId, OpenMode.ForWrite);
                        dicAttNew.Add(attRef.Tag.ToUpper(), attRef);

                    }
                    foreach (ObjectId arId in attsOld)
                    {
                        AttributeReference attRef =
                            (AttributeReference)acTrans.GetObject(arId, OpenMode.ForRead);
                        string tag = attRef.Tag;
                        //if (attRef.Tag == "PLATFORM_1")
                        //{
                        //    tag = "NAME";
                        //}
                        if (dicAttNew.ContainsKey(tag.ToUpper()))
                        {
                            dicAttNew[tag.ToUpper()].TextString = attRef.TextString;
                        }

                    }
                    acTrans.Commit();
                }
            }

        }

        [CommandMethod("ExportRDD")]
        public void ExportRdd()
        {
            TFileDescr siglayout = new TFileDescr();
            List<StationsAndStopsStationsAndStop> stationsandstops = new List<StationsAndStopsStationsAndStop>();
            List<DetectionPointsDetectionPoint> detpoints = new List<DetectionPointsDetectionPoint>();
            signals = new List<SignalsSignal>();
            List<ConnectorsConnector> connectors = new List<ConnectorsConnector>();
            points = new List<PointsPoint>();
            List<BaliseGroupsBaliseGroup> balises = new List<BaliseGroupsBaliseGroup>();
            acsections = new List<AxleCounterSectionsAxleCounterSection>();
            List<TrackSectionsTrackSection> tsections = new List<TrackSectionsTrackSection>();
            List<EndOfTracksEndOfTrack> endoftracks = new List<EndOfTracksEndOfTrack>();
            List<StaffPassengerCrossingsStaffPassengerCrossing> staffpassengercrossings =
                new List<StaffPassengerCrossingsStaffPassengerCrossing>();
            List<LevelCrossingsLevelCrossing> levelcrossings =
                new List<LevelCrossingsLevelCrossing>();
            List<EmergencyStopGroupsEmergencyStopGroup> emergencystopgroups =
                new List<EmergencyStopGroupsEmergencyStopGroup>();
            speedProfiles = new List<SpeedProfilesSpeedProfile>();
            List<RoutesRoute> routes = new List<RoutesRoute>();
            List<TrustedAreasTrustedArea> trustedareas = new List<TrustedAreasTrustedArea>();
            List<PlatformsPlatform> platforms = new List<PlatformsPlatform>();
            List<BlockInterfacesBlockInterface> blockInterfaces = new List<BlockInterfacesBlockInterface>();
            List<PermanentShuntingAreasPermanentShuntingArea> permanentshuntareas =
                new List<PermanentShuntingAreasPermanentShuntingArea>();
            List<Track> Tracks;

            TracksLines = new List<TrackLine>();
            List<Line> TrustedAreaLines = new List<Line>();

            lines = new List<LinesLine>();
            TrackSegmentsTmp = new List<TrackSegmentTmp>();
            TrustedAreas = new List<TrustedArea>();
            trcksegments = new List<TrackSegmentsTrackSegment>();
            Documents = new List<TFileDescr>();

            ExportCigClosure = new List<string>
            {
                "<Signal ID>\t"+
                "<Signal km>\t"+
                "<BG ID>\t" +
                "<BG km>\t" +
                "<Ac ID>\t" +
                "<Ac km>\t" +
                "<Connectors>"
            };
            ExportPoints = new List<string>
            {
                "<Station name>\t"+
                "<Point ID>\t" +
                "<Tip km>\t"+
                "<Right km>\t" +
                "<Left km>"
            };


            SaveFileDialog saveFile = new SaveFileDialog("Save RDD", dwgDir + "\\" + Constants.defaultFileName, "xml", "SaveRdd",
                                                SaveFileDialog.SaveFileDialogFlags.NoUrls);

            if (saveFile.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                ErrLogger.Information("Program canceled by user", "Station Dialog");
                return;
            }
            string saveTo = saveFile.Filename;

            if (File.Exists(dwgDir + "//" + dwgFileName + ".ini"))
            {
                loadFiles = File.ReadAllLines(dwgDir + "//" + dwgFileName + ".ini")
                            .Where(arg => !string.IsNullOrWhiteSpace(arg))
                            .ToDictionary(x => x.Split('\t')[0], x => x.Split('\t')[1]);
            }

            List<bool> err = new List<bool>
            {
                blocksErr
            };
            GetDocIdVrs();
            TracksLines = GetTracksLines();
            RailwayLines = GetRailwayLines(TracksLines, blocks);
            blckProp = new BlockProperties(stationID);
            excel = new ReadExcel.Excel(stationID);
            TrustedAreaLines = GetTrustedAreasLines();
            Tracks = GetTracksNames().ToList();

            ReadLines(ref lines);

            // Signaling Layout
            if (!ReadSigLayout(blocks, ref siglayout))
            {
                return;
            }

            err.Add(!CollectTrustedAreas(TrustedAreaLines, TracksLines));

            // Get PSAs
            pSAs = GetPsas().ToList();

            List<EmSG> emGs = GetEmGs().ToList();

            // Set Next station blocks
            SetBlocksNextStations(blocks);

            SetBlocksExclude(blocks);

            // Assign comp routes starts and destinations
            SetSignalsStartDestCompRoutes(blocks);

            // Segments
            err.Add(!GetSegments(blocks, TracksLines, Tracks, pSAs));


            //Test
            CheckAttSameKm(blocks);

            //Check blocks track segments
            err.Add(!GetBlocksWithoutSegment(blocks));


            //StationsAndStops
            ReadStationsStops(ref stationsandstops, GetStops(blocks).ToList());

            // Detection Points
            err.Add(!ReadDps(blocks, ref detpoints, pSAs));

            //PSA
            err.Add(!ReadPSAs(pSAs, ref permanentshuntareas));

            // Signals
            err.Add(!ReadSignals(blocks, ref signals));

            //Test routes list
            RoutesList();

            // Speed Profiles
            ReadSps(ref speedProfiles);

            //Points
            err.Add(!ReadPoints(blocks, ref points, speedProfiles, pSAs, emGs));

            // Connectors
            ReadConnectors(blocks, ref connectors);

            // AC Sections
            err.Add(!ReadAcSections(blocks, ref acsections));

            err.Add(!GetDetLockPoints());

            // Track Sections
            err.Add(!ReadTrSections(blocks, ref tsections, emGs));

            // End of Tracks
            err.Add(!ReadEOTs(blocks, ref endoftracks));

            // Balise Groups
            err.Add(!ReadBGs(blocks, ref balises, pSAs));

            // PWS
            err.Add(!ReadPWs(blocks, ref staffpassengercrossings, acsections));


            LxsActivations = excel.LoopLxActivations(dwgDir);

            // Level Crossings
            err.Add(!ReadLxs(blocks, ref levelcrossings, acsections));

            // Emergency Stop Groups
            ReadEms(blocks, ref emergencystopgroups, emGs);

            // Routes
            err.Add(!ReadRoutes(blocks, ref routes, acsections));

            // Compound Routes
            //err.Add(!ReadCRoutes(Blocks, ref cmproutes, signals, routes));
            //err.Add(!GetCompoundRoutes(blocks, routes));

            // Trusted Areas
            err.Add(!ReadTrustedAreas(ref trustedareas));

            // Platforms
            err.Add(!ReadPlatforms(blocks, ref platforms));

            // Block Interfaces
            err.Add(!ReadBlockinterfaces(blocks, ref blockInterfaces));


            RailwayDesignData RDD = new RailwayDesignData
            {
                version = "1.6.18",
                SchemaDocId = "7HA700001014_109EN",
                MetaData = new RailwayDesignDataMetaData
                {
                    FileDescription = new TFileDescr
                    {
                        title = "PT1 Tables " + stationName,
                        date = DateTime.Now,
                        version = Version,
                        docID = DocID,
                        creator = "Georgijs Karpovs"
                    },
                    SignallingLayout = siglayout,
                    Documents = new RailwayDesignDataMetaDataDocuments
                    {
                        Document = Documents.ToArray()
                    }
                },
                StationsAndStops = new StationsAndStops { StationsAndStop = stationsandstops.ToArray() },
                Signals = new Signals { Signal = signals.ToArray() },
                Points = new Points { Point = points.ToArray() },
                TrackSegments = new TrackSegments { TrackSegment = trcksegments.ToArray() },
                DetectionPoints = new DetectionPoints { DetectionPoint = detpoints.ToArray() },
                BaliseGroups = new BaliseGroups { BaliseGroup = balises.ToArray() },
                AxleCounterSections = new AxleCounterSections { AxleCounterSection = acsections.ToArray() },
                Connectors = new Connectors { Connector = connectors.ToArray() },
                Lines = new Lines { Line = lines.ToArray() },
                TrackSections = new TrackSections { TrackSection = tsections.ToArray() },
                LevelCrossings = (levelcrossings.Count == 0) ? null : new LevelCrossings
                {
                    LevelCrossing = levelcrossings.ToArray()
                },
                StaffPassengerCrossings = (staffpassengercrossings.Count == 0) ? null : new StaffPassengerCrossings
                {
                    StaffPassengerCrossing = staffpassengercrossings.ToArray()
                },
                EndOfTracks = (endoftracks.Count == 0) ? null : new EndOfTracks
                {
                    EndOfTrack = endoftracks.ToArray()
                },
                Platforms = (platforms.Count == 0) ? null : new Platforms
                {
                    Platform = platforms.ToArray()
                },
                BlockInterfaces = (blockInterfaces.Count == 0) ? null : new BlockInterfaces
                {
                    BlockInterface = blockInterfaces.ToArray()
                },
                EmergencyStopGroups = (emergencystopgroups.Count == 0) ? null : new EmergencyStopGroups
                {
                    EmergencyStopGroup = emergencystopgroups.ToArray()
                },
                SpeedProfiles = (speedProfiles.Count == 0) ? null : new SpeedProfiles
                {
                    SpeedProfile = speedProfiles.ToArray()
                },
                Routes = (routes.Count == 0) ? null : new Routes
                {
                    Route = routes.OrderBy(x => x.Designation).ToArray()
                },
                CompoundRoutes = (cmproutes.Count == 0) ? null : new CompoundRoutes
                {
                    CompoundRoute = cmproutes.ToArray()
                },
                TrustedAreas = (trustedareas.Count == 0) ? null : new TrustedAreas
                {
                    TrustedArea = trustedareas.ToArray()
                },
                PermanentShuntingAreas = (permanentshuntareas.Count == 0) ? null : new PermanentShuntingAreas
                {
                    PermanentShuntingArea = permanentshuntareas.ToArray()
                }
            };

            RddXmlIO rddXmlIO = new RddXmlIO();
            RddOrder rddOrder = new RddOrder();

            if (Path.GetFileNameWithoutExtension(saveTo) == Constants.defaultFileName)
            {
                saveTo = Path.GetDirectoryName(saveTo) + "//" +
                          stationID.ToUpper() + "-" +
                          Constants.defaultFileName + "-" +
                          Version + ".xml";
            }

            if (!File.Exists(orderRddFileName))
            {
                err.Add(true);
                ErrLogger.Error("Order Rdd not found", orderRddFileName, "");
            }
            else
            {
                RDD = rddOrder.OrderRdd(RDD, rddXmlIO.GetRdd(orderRddFileName), true, copyLevel);
            }

            if (checkData["checkBoxRdd"])
            {
                if (!File.Exists(loadFiles["lblxlsRdd"]))
                {
                    err.Add(true);
                    ErrLogger.Error("Previous Rdd version not found", loadFiles["lblxlsRdd"], "");
                }
                else
                {
                    RailwayDesignData PrevRdd = rddXmlIO.GetRdd(loadFiles["lblxlsRdd"]);
                    Compare compare = new Compare();
                    RDD = compare.CompareRdd(RDD, PrevRdd, saveTo);
                }
            }




            if (checkData["checkBoxR5"])
            {
                //rddXmlIO.WriteRddXml(RDD, saveTo, new List<string>());
                MemoryStream stream = rddXmlIO.WriteXmlToMemory(RDD);
                RddR5.RailwayDesignData rddR5 = rddXmlIO.ReadXmlFromMemoryR5(stream);
                //RddR5.RailwayDesignData rddR5 = rddXmlIO.GetRddR5(saveTo);
                rddXmlIO.WriteRddXml(rddR5, saveTo,
                    new List<string> { "Created with ExpPt1 v" + Assembly.GetExecutingAssembly().GetName().Version.ToString(3)
                                        + " (Thales Latvia) - " + DateTime.Now });
            }
            else
            {
                rddXmlIO.WriteRddXml(RDD, saveTo,
                    new List<string> { "Created with ExpPt1 v" + Assembly.GetExecutingAssembly().GetName().Version.ToString(3)
                                        + " (Thales Latvia) - " + DateTime.Now });
            }
            try
            {
                WriteData.ReportExcel(RDD.MetaData, Path.GetDirectoryName(saveTo) + "//" +
                              Path.GetFileNameWithoutExtension(saveTo) + "_Report.xlsx");
            }
            catch (System.Exception e)
            {
                AcadApp.ShowAlertDialog(e.Message);
            }
            if (RDD.SpeedProfiles != null && RDD.SpeedProfiles.SpeedProfile != null)
            {
                Verify verify = new Verify();
                verify.CheckSSPsSegments(RDD, TrackSegmentsTmp);
            }

            //File.WriteAllLines(Path.GetDirectoryName(saveTo) + "//" +
            //                   Path.GetFileNameWithoutExtension(saveTo) + "_Signals.txt", ExportCigClosure);
            //File.WriteAllLines(Path.GetDirectoryName(saveTo) + "//" +
            //                   Path.GetFileNameWithoutExtension(saveTo) + "_Points.txt", ExportPoints);

            if (err.Contains(true) || ErrLogger.ErrorsFound)
            {
                //File.WriteAllLines(Path.GetDirectoryName(saveTo) + "//" +
                //               Path.GetFileNameWithoutExtension(saveTo) + "_Errors.log", errorslogs);
                System.Windows.Forms.MessageBox.Show("Export completed with errors. See errors log.",
                    "Rdd Export", System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Exclamation);
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Export successful",
                    "Rdd Export", System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Information);
                //File.WriteAllLines(Path.GetDirectoryName(saveTo) + "//" +
                //               Path.GetFileNameWithoutExtension(saveTo) +
                //               "_Errors.log", new string[] { "No errors" });
            }
        }

        protected void GetDocIdVrs()
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                var Textsids = GetObjectsOfType(db, RXObject.GetClass(typeof(MText)));
                foreach (ObjectId ObjId in Textsids)
                {
                    var mtext = (MText)trans.GetObject(ObjId, OpenMode.ForRead);
                    if (mtext.Text.Contains("Internt Thales tegningsnr"))
                    {
                        docId = mtext.Text.Split(':')[1].Trim().Split(new char[0], StringSplitOptions.RemoveEmptyEntries)[0];
                        docVrs = mtext.Text.Split(':')[1].Trim()
                            .Split(new Char[] { ' ', ',', '.', ':', '\n', '\t', '/', 'v', 'V' },
                                       StringSplitOptions.RemoveEmptyEntries).Last();
                        break;
                    }
                }
                Textsids = GetObjectsOfType(db, RXObject.GetClass(typeof(DBText)));
                foreach (ObjectId ObjId in Textsids)
                {
                    var text = (DBText)trans.GetObject(ObjId, OpenMode.ForRead);
                    if (text.TextString.Contains("Internt Thales tegningsnr")) // Internt Thales tegningsnr
                    {
                        docId = text.TextString.Split(':')[1].Trim().Split(new char[0], StringSplitOptions.RemoveEmptyEntries)[0];
                        docVrs = text.TextString.Split(':')[1].Trim()
                            .Split(new Char[] { ' ', ',', '.', ':', '\n', '\t', '/', 'v', 'V' },
                                       StringSplitOptions.RemoveEmptyEntries).Last();
                        break;
                    }
                }
                trans.Commit();
            }
        }

        protected List<TrackLine> GetTracksLines()
        {
            List<TrackLine> TracksLines = new List<TrackLine>();
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                var Linesids = GetObjectsOfType(db, RXObject.GetClass(typeof(Line)));
                foreach (ObjectId ObjId in Linesids)
                {
                    Line line = (Line)trans.GetObject(ObjId, OpenMode.ForRead);
                    if (line.Layer == "_Spor" && line.Length > Constants.minTrLineLength)
                    {
                        Color color;
                        if (line.Color.ColorMethod == Autodesk.AutoCAD.Colors.ColorMethod.ByLayer)
                        {
                            LayerTableRecord layer =
                                (LayerTableRecord)trans.GetObject(line.LayerId, OpenMode.ForRead);
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
                }

                var PolyLines = GetObjectsOfType(db, RXObject.GetClass(typeof(Polyline)));
                foreach (ObjectId ObjId in PolyLines)
                {
                    Polyline polyline = (Polyline)trans.GetObject(ObjId, OpenMode.ForRead);
                    if (polyline.Layer == "_Spor")
                    {
                        DBObjectCollection entset = new DBObjectCollection();
                        polyline.Explode(entset);
                        foreach (DBObject obj in entset)
                        {
                            if (obj.GetType() == typeof(Line) && ((Line)obj).Length > 0)
                            {
                                Color color;
                                Line line = (Line)obj;
                                if (line.Color.ColorMethod == Autodesk.AutoCAD.Colors.ColorMethod.ByLayer)
                                {
                                    LayerTableRecord layer =
                                        (LayerTableRecord)trans.GetObject(line.LayerId, OpenMode.ForRead);
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
                        }
                    }
                }
                trans.Commit();
                return TracksLines;
            }
        }

        protected IEnumerable<Track> GetTracksNames()
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                var Textsids = GetObjectsOfType(db, RXObject.GetClass(typeof(MText)));
                foreach (ObjectId ObjId in Textsids)
                {
                    var mtext = (MText)trans.GetObject(ObjId, OpenMode.ForRead);
                    if (mtext.Text.Contains("Sp"))
                    {
                        yield return new Track
                        {
                            Name = Regex.Replace(mtext.Text, "[^0-9]", ""),
                            X = mtext.GeometricExtents.MinPoint.X,
                            Y = mtext.GeometricExtents.MinPoint.Y
                        };
                    }
                }
                Textsids = GetObjectsOfType(db, RXObject.GetClass(typeof(DBText)));
                foreach (ObjectId ObjId in Textsids)
                {
                    var mtext = (DBText)trans.GetObject(ObjId, OpenMode.ForRead);
                    if (mtext.TextString.Contains("Sp"))
                    {
                        yield return new Track
                        {
                            Name = Regex.Replace(mtext.TextString, "[^0-9]", ""),
                            X = mtext.GeometricExtents.MinPoint.X,
                            Y = mtext.GeometricExtents.MinPoint.Y
                        };
                    }
                }
                trans.Commit();
            }
        }

        protected IEnumerable<PSA> GetPsas()
        {
            List<PSA> psas = new List<PSA>();
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                List<Polyline> polylines = new List<Polyline>();
                var PolyLineIds = GetObjectsOfType(db, RXObject.GetClass(typeof(Polyline)));
                foreach (ObjectId ObjId in PolyLineIds)
                {
                    polylines.Add((Polyline)trans.GetObject(ObjId, OpenMode.ForRead));
                }

                var Textsids = GetObjectsOfType(db, RXObject.GetClass(typeof(MText)));
                foreach (ObjectId ObjId in Textsids)
                {
                    var mtext = (MText)trans.GetObject(ObjId, OpenMode.ForRead);
                    if (mtext.Text.Contains("PSA"))
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
                            yield return new PSA
                            {
                                Name = mtext.Text,
                                MinX = Math.Round(polylinePsa.GeometricExtents.MinPoint.X, 0, MidpointRounding.AwayFromZero),
                                MaxX = Math.Round(polylinePsa.GeometricExtents.MaxPoint.X, 0, MidpointRounding.AwayFromZero),
                                MinY = Math.Round(polylinePsa.GeometricExtents.MinPoint.Y, 0, MidpointRounding.AwayFromZero),
                                MaxY = Math.Round(polylinePsa.GeometricExtents.MaxPoint.Y, 0, MidpointRounding.AwayFromZero),
                                PsaPolyLine = polylinePsa
                            };
                        }
                    }
                }
                Textsids = GetObjectsOfType(db, RXObject.GetClass(typeof(DBText)));
                foreach (ObjectId ObjId in Textsids)
                {
                    var mtext = (DBText)trans.GetObject(ObjId, OpenMode.ForRead);
                    if (mtext.TextString.Contains("PSA"))
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
                            yield return new PSA
                            {
                                Name = mtext.TextString,
                                MinX = Math.Round(polylinePsa.GeometricExtents.MinPoint.X, 0, MidpointRounding.AwayFromZero),
                                MaxX = Math.Round(polylinePsa.GeometricExtents.MaxPoint.X, 0, MidpointRounding.AwayFromZero),
                                MinY = Math.Round(polylinePsa.GeometricExtents.MinPoint.Y, 0, MidpointRounding.AwayFromZero),
                                MaxY = Math.Round(polylinePsa.GeometricExtents.MaxPoint.Y, 0, MidpointRounding.AwayFromZero),
                                PsaPolyLine = polylinePsa
                            };
                        }
                    }
                }
                trans.Commit();
            }
        }

        protected IEnumerable<EmSG> GetEmGs()
        {

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                var PolyLineIds = GetObjectsOfType(db, RXObject.GetClass(typeof(Polyline)));
                int i = 0;
                List<Polyline> polylinesEmgs = new List<Polyline>();
                foreach (ObjectId ObjId in PolyLineIds)
                {
                    Polyline polyline = (Polyline)trans.GetObject(ObjId, OpenMode.ForRead);
                    if (polyline.Layer == "EmergencyStopElem")
                    {
                        polylinesEmgs.Add(polyline);
                    }
                }
                var Emgss = polylinesEmgs
                    .GroupBy(g => g.ColorIndex)
                    .OrderBy(g => g.Key); //.SelectMany(x => x).ToList();
                foreach (var emgs in Emgss)
                {
                    ++i;
                    foreach (var emgsPolyLine in emgs)
                    {
                        int order = i;
                        if (emgs.Key == 240 || emgs.Key == 242 || emgs.Key == 244)
                        {
                            order = 1;
                        }
                        yield return new EmSG
                        {
                            Designation = (i).ToString().PadLeft(3, '0'),
                            MinX = Math.Round(emgsPolyLine.GeometricExtents.MinPoint.X, 0, MidpointRounding.AwayFromZero),
                            MaxX = Math.Round(emgsPolyLine.GeometricExtents.MaxPoint.X, 0, MidpointRounding.AwayFromZero),
                            MinY = Math.Round(emgsPolyLine.GeometricExtents.MinPoint.Y, 0, MidpointRounding.AwayFromZero),
                            MaxY = Math.Round(emgsPolyLine.GeometricExtents.MaxPoint.Y, 0, MidpointRounding.AwayFromZero),
                            PsaPolyLine = emgsPolyLine,
                            Order = order
                        };
                    }
                }
                trans.Commit();
            }
        }

        private IEnumerable<StationStop> GetStops(List<Block> blocks)
        {

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                var Textsids = GetObjectsOfType(db, RXObject.GetClass(typeof(MText)));
                Regex StopPatt = new Regex(@"^[a-zæøåÆØÅA-Z].*\(([a-zæøåÆØÅA-Z]{2,3})\)");
                double StopX;
                Dictionary<string, string> Stkeys = new Dictionary<string, string>();
                foreach (ObjectId ObjId in Textsids)
                {
                    var mtext = (MText)trans.GetObject(ObjId, OpenMode.ForRead);
                    if (StopPatt.IsMatch(mtext.Text))
                    {
                        List<decimal> StopKmps = new List<decimal>();
                        List<string> LinesIds = new List<string>();
                        StopX = mtext.GeometricExtents.MinPoint.X +
                            (mtext.GeometricExtents.MaxPoint.X - mtext.GeometricExtents.MinPoint.X) / 2;
                        List<Block> platforms =
                            blocks.Where(x => x.XsdName == "PlatformDyn" &&
                                              StopX >= x.BlkRef.GeometricExtents.MinPoint.X &&
                                              StopX <= x.BlkRef.GeometricExtents.MaxPoint.X).ToList();
                        foreach (Block plat in platforms)
                        {
                            plat.StId = mtext.Text.Split('(').Last().TrimEnd(')').ToLower();
                            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                            plat.StName = textInfo.ToTitleCase(mtext.Text.Split('(').First().Trim().ToLower());
                            StopKmps.Add(Convert.ToDecimal(plat.Attributes["START_PLAT_1"].Value));
                            StopKmps.Add(Convert.ToDecimal(plat.Attributes["END_PLAT_1"].Value));
                            TrackLine trackLine = TracksLines
                                     .Where(x => ObjectsIntersects(x.line, plat.BlkRef, Intersect.OnBothOperands))
                                     .FirstOrDefault();
                            string lineId = RailwayLines.Where(x => x.color == trackLine.color)
                                                             .Select(y => y.designation)
                                                             .FirstOrDefault();
                            LinesIds.Add(lineId);
                        }
                        if (platforms.Count > 0)
                        {
                            string id = mtext.Text.Split('(').Last().TrimEnd(')').ToLower();
                            if (Stkeys.ContainsKey(id))
                            {
                                continue;
                            }
                            Stkeys.Add(id, id);
                            yield return new StationStop
                            {
                                Id = mtext.Text.Split('(').Last().TrimEnd(')').ToLower(),
                                Name = mtext.Text.Split('(').First().Trim(),
                                StartKm = StopKmps.Min(),
                                EndKm = StopKmps.Max(),
                                LineIDs = LinesIds,
                                KindOfSAS = KindOfSASType.stop
                            };
                        }
                        else
                        {
                            TrackSegmentTmp trackSeg = TrackSegmentsTmp
                                                                   .Where(t => t.Track != null &&
                                                                   StopX >= t.Vertex1.X &&
                                                                   StopX <= t.Vertex2.X)
                                                                   .FirstOrDefault();
                            if (trackSeg != null)
                            {
                                yield return new StationStop
                                {
                                    Id = mtext.Text.Split('(').Last().Trim().TrimEnd(')').ToLower(),
                                    Name = mtext.Text.Split('(').First().Trim(),
                                    KindOfSAS = KindOfSASType.station
                                };
                            }
                        }
                    }
                }
                Textsids = GetObjectsOfType(db, RXObject.GetClass(typeof(DBText)));
                foreach (ObjectId ObjId in Textsids)
                {
                    var mtext = (DBText)trans.GetObject(ObjId, OpenMode.ForRead);
                    if (StopPatt.IsMatch(mtext.TextString))
                    {
                        List<decimal> StopKmps = new List<decimal>();
                        List<string> LinesIds = new List<string>();
                        StopX = mtext.GeometricExtents.MinPoint.X +
                            (mtext.GeometricExtents.MaxPoint.X - mtext.GeometricExtents.MinPoint.X) / 2;
                        List<Block> platforms =
                            blocks.Where(x => x.XsdName == "PlatformDyn" &&
                                              StopX >= x.BlkRef.GeometricExtents.MinPoint.X &&
                                              StopX <= x.BlkRef.GeometricExtents.MaxPoint.X).ToList();
                        foreach (Block plat in platforms)
                        {
                            plat.StId = mtext.TextString.Split('(').Last().Trim().TrimEnd(')').ToLower();
                            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                            plat.StName =
                                textInfo.ToTitleCase(mtext.TextString.Split('(').First().Trim().ToLower());
                            StopKmps.Add(Convert.ToDecimal(plat.Attributes["START_PLAT_1"].Value));
                            StopKmps.Add(Convert.ToDecimal(plat.Attributes["END_PLAT_1"].Value));
                            TrackLine trackLine = TracksLines
                                     .Where(x => ObjectsIntersects(x.line, plat.BlkRef, Intersect.OnBothOperands))
                                     .FirstOrDefault();
                            string lineId = RailwayLines.Where(x => x.color == trackLine.color)
                                                             .Select(y => y.designation)
                                                             .FirstOrDefault();
                            LinesIds.Add(lineId);
                        }
                        if (platforms.Count > 0)
                        {
                            string id = mtext.TextString.Split('(').Last().Trim().TrimEnd(')').ToLower();
                            if (Stkeys.ContainsKey(id))
                            {
                                continue;
                            }
                            Stkeys.Add(id, id);
                            yield return new StationStop
                            {
                                Id = mtext.TextString.Split('(').Last().Trim().TrimEnd(')').ToLower(),
                                Name = mtext.TextString.Split('(').First().Trim(),
                                StartKm = StopKmps.Min(),
                                EndKm = StopKmps.Max(),
                                LineIDs = LinesIds,
                                KindOfSAS = KindOfSASType.stop
                            };
                        }
                        else
                        {
                            TrackSegmentTmp trackSeg = TrackSegmentsTmp
                                       .Where(t => t.Track != null &&
                                       StopX >= t.Vertex1.X &&
                                       StopX <= t.Vertex2.X)
                                       .FirstOrDefault();
                            if (trackSeg != null)
                            {
                                yield return new StationStop
                                {
                                    Id = mtext.TextString.Split('(').Last().Trim().TrimEnd(')').ToLower(),
                                    Name = mtext.TextString.Split('(').First().Trim(),
                                    KindOfSAS = KindOfSASType.station
                                };
                            }
                        }
                    }
                }
                trans.Commit();
            }
        }

        protected List<RailwayLine> GetRailwayLines(List<TrackLine> trackLines, List<Block> blocks)
        {
            List<RailwayLine> railwayLines = new List<RailwayLine>();
            RailwayLine DefaultRailwayLine = new RailwayLine();
            List<DBText> linesTexts = new List<DBText>();
            List<MText> linesMTexts = new List<MText>();
            Dictionary<string, string> LinesDefinitions = ReadLinesDefinitions();
            stationID = GetStationId(blocks);
            if (stationID == null)
            {
                return null;
            }
            string tmpLine = ReadStations()
                                  .Where(x => x.Key.ToLower() == stationID)
                                  .Select(x => x.Value)
                                  .FirstOrDefault();
            string DefaultLine = LinesDefinitions
                                 .Where(x => x.Key == tmpLine.Split('\t')[1])
                                 .Select(x => x.Value)
                                 .FirstOrDefault();

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                var Txts = GetObjectsOfType(db, RXObject.GetClass(typeof(DBText)));
                foreach (ObjectId ObjId in Txts)
                {
                    DBText text = (DBText)trans.GetObject(ObjId, OpenMode.ForRead);
                    if (text.TextString.Contains("TIB"))
                    {
                        linesTexts.Add(text);
                    }
                }
                var MTxts = GetObjectsOfType(db, RXObject.GetClass(typeof(MText)));
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
                    DefaultRailwayLine.designation = DefaultLine.Split('\t')[0];
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
                        trackline.LineID = DefaultRailwayLine.designation;
                    }
                }
                return railwayLines.OrderBy(x => x.designation).ToList();
            }

            if (linesTexts.Count > 0)
            {
                foreach (TrackLine trackline in trackLines)
                {
                    DBText text = linesTexts
                                  .Where(x => ObjectsIntersects(trackline.line, x, Intersect.ExtendThis) &&
                                              x.AlignmentPoint.X >= trackline.line.GeometricExtents.MinPoint.X - 30 &&
                                              x.AlignmentPoint.X <= trackline.line.GeometricExtents.MaxPoint.X + 30)
                                  .FirstOrDefault();
                    if (text != null)
                    {
                        RailwayLine railwayLine = new RailwayLine
                        {
                            designation = Regex.Replace(text.TextString, "[^.0-9]", ""),
                            color = trackline.color
                        };
                        string line = LinesDefinitions
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
                //return railwayLines.OrderBy(x => x.designation).ToList();
            }
            if (linesMTexts.Count > 0)
            {
                foreach (MText text in linesMTexts)
                {
                    RailwayLine railwayLine = new RailwayLine
                    {
                        designation = Regex.Replace(text.Text, "[^.0-9]", ""),
                    };
                    string line = LinesDefinitions
                                      .Where(x => x.Key == railwayLine.designation)
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
                //return railwayLines.OrderBy(x => x.designation).ToList();
            }
            foreach (var railwayLine in railwayLines)
            {
                foreach (var trackLine in trackLines.Where(x => x.color == railwayLine.color))
                {
                    trackLine.direction = railwayLine.direction;
                    trackLine.LineID = railwayLine.designation;
                }
            }
            return railwayLines.OrderBy(x => x.designation).ToList();
        }

        private string GetStationId(List<Block> blocks)
        {
            List<Block> BlkSigLayouts = blocks.Where(x => x.XsdName == "SignallingLayout")
                                    .Select(x => x).ToList();
            foreach (Block BlkSigLayout in BlkSigLayouts)
            {
                //string tmpCreator = BlkSigLayout.Attributes
                //       .Where(x => x.Key.Contains("KONSTRUERET") && x.Value.Value != "")
                //       .Select(y => y.Value.Value)
                //       .FirstOrDefault();
                //if (BlkSigLayout.Attributes["1-ST.NAVN"].Value.Split('(').Length > 1)
                //{
                return BlkSigLayout.Attributes["1-ST.NAVN"].Value
                       .Split(new char[] { '(', '-' }, StringSplitOptions.RemoveEmptyEntries)[1]
                       .Split(new char[] { ')', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries)[0]
                       .Trim()
                       .ToLower();
                //}
                //if (BlkSigLayout.Attributes["1-ST.NAVN"].Value.Split('-').Length > 1)
                //{
                //    return BlkSigLayout.Attributes["1-ST.NAVN"].Value.Split('-')[1].Trim().ToLower();
                //}
            }
            return null;
        }

        protected List<Line> GetTrustedAreasLines()
        {
            List<Line> TrustedAreaLines = new List<Line>();
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                var Linesids = GetObjectsOfType(db, RXObject.GetClass(typeof(Line)));
                foreach (ObjectId ObjId in Linesids)
                {
                    Line line = (Line)trans.GetObject(ObjId, OpenMode.ForRead);

                    if (line.Layer.Contains("Trusted") && line.Length > 0)
                    {
                        TrustedAreaLines.Add(line);
                    }
                }

                var PolyLines = GetObjectsOfType(db, RXObject.GetClass(typeof(Polyline)));
                foreach (ObjectId ObjId in PolyLines)
                {
                    Polyline polyline = (Polyline)trans.GetObject(ObjId, OpenMode.ForRead);
                    if (polyline.Layer == "Trusted area" || polyline.Layer == "TrustedArea")
                    {
                        DBObjectCollection entset = new DBObjectCollection();
                        polyline.Explode(entset);
                        foreach (DBObject obj in entset)
                        {
                            if (obj.GetType() == typeof(Line) && ((Line)obj).Length > 0)
                            {
                                TrustedAreaLines.Add((Line)obj);
                            }
                        }
                    }
                }
                trans.Commit();
                return TrustedAreaLines;
            }
        }

        protected List<Block> GetBlocks(ref bool error)
        {
            List<Block> Blocks = new List<Block>();
            blckProp = new BlockProperties("");
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                foreach (ObjectId btrId in bt)
                {
                    BlockTableRecord btr = (BlockTableRecord)trans.GetObject(btrId, OpenMode.ForRead);
                    if (!btr.IsLayout && BlocksToGet.ContainsKey(btr.Name))
                    {
                        ObjectIdCollection aRefIds = new ObjectIdCollection();
                        if (btr.IsDynamicBlock)
                        {
                            var blockIds = btr.GetAnonymousBlockIds();
                            foreach (ObjectId BlkId in blockIds)
                            {
                                BlockTableRecord btr2 =
                                    (BlockTableRecord)trans.GetObject(BlkId, OpenMode.ForRead, false, false);
                                ObjectIdCollection aRefIds2 = btr2.GetBlockReferenceIds(true, true);
                                foreach (ObjectId id in aRefIds2)
                                {
                                    aRefIds.Add(id);
                                }
                            }
                            foreach (ObjectId id in btr.GetBlockReferenceIds(false, true))
                            {
                                aRefIds.Add(id);
                            }
                        }
                        else
                        {
                            aRefIds = btr.GetBlockReferenceIds(false, true);
                        }
                        foreach (ObjectId RefId in aRefIds)
                        {

                            BlockReference blkRef = (BlockReference)trans.GetObject(RefId, OpenMode.ForRead);
                            LayerTableRecord layer =
                                (LayerTableRecord)trans.GetObject(blkRef.LayerId, OpenMode.ForRead);
                            if (layer.IsFrozen)
                            {
                                continue;
                            }
                            Enum.TryParse(BlocksToGet[btr.Name].Split('\t')[1], out XType xType);

                            Dictionary<string, Attribute> Attributes = GetAttributes(blkRef);
                            Block block = new Block
                            {
                                BlkRef = blkRef,
                                BlockName = btr.Name,
                                ElType = BlocksToGet[btr.Name].Split('\t')[2],
                                XsdName = BlocksToGet[btr.Name].Split('\t')[1],
                                X = Math.Round(blkRef.Position.X, 0, MidpointRounding.AwayFromZero),
                                Y = Math.Round(blkRef.Position.Y, 0, MidpointRounding.AwayFromZero),
                                Rotation = (int)(blkRef.Rotation * (180 / Math.PI)),
                                KindOf = BlocksToGet[btr.Name].Split('\t')[3],
                                Attributes = Attributes,
                                IsOnCurrentArea = true,
                                Visible = !(Attributes.Any(x => x.Value.Name == "NAME" &&
                                                               x.Value.Visible == false))
                            };
                            try
                            {
                                if (!blkRef.BlockName.Contains("MODEL_SPACE"))
                                {
                                    Blocks.Add(block);
                                    continue;
                                }
                            }
                            catch (Autodesk.AutoCAD.Runtime.Exception e)
                            {
                                ErrLogger.Information("AutoCAD block exception - " + e.Message, blkRef.Name);
                            }

                            //if (block.XsdName == "Point" &&
                            //    block.Attributes["NAME"].Value.Contains("SN") &&
                            //    block.KindOf != "hhtDerailer")
                            //{
                            //    block.KindOf = "hhtPoint";
                            //}
                            if (block.XsdName != "FoulingPoint" && block.Attributes.Any(x => x.Key.Contains("KMP")))
                            {
                                string[] attKeys = block.Attributes
                                               .Where(x => x.Key.Contains("KMP"))
                                               .OrderBy(x => x.Key)
                                               .Select(x => x.Key)
                                               .ToArray();
                                if (attKeys.Length > 0)
                                {
                                    string location =
                                    block.Attributes[attKeys[0]].Value;
                                    if (!decimal.TryParse(location, out decimal loc))
                                    {
                                        if (location == "")
                                        {
                                            loc = 0;
                                        }
                                        else
                                        {
                                            error = true;
                                            ErrLogger.Error("Can not convert to decimal", blckProp.GetElemDesignation(block), attKeys[0]);
                                        }
                                    }
                                    block.Location = loc;
                                }

                                if (attKeys.Length > 1)
                                {
                                    string location =
                                    block.Attributes[attKeys[1]].Value;
                                    if (!decimal.TryParse(location, out decimal loc) && block.XsdName != "Point")
                                    {
                                        if (location == "")
                                        {
                                            loc = 0;
                                        }
                                        else
                                        {
                                            error = true;
                                            ErrLogger.Error("Can not convert to decimal", blckProp.GetElemDesignation(block), attKeys[1]);
                                        }
                                    }
                                    block.Location2 = loc;
                                }

                                if (attKeys.Length > 2)
                                {
                                    string location =
                                    block.Attributes[attKeys[2]].Value;
                                    if (!decimal.TryParse(location, out decimal loc) && block.XsdName != "Point")
                                    {
                                        if (location == "")
                                        {
                                            loc = 0;
                                        }
                                        else
                                        {
                                            error = true;
                                            ErrLogger.Error("Can not convert to decimal", blckProp.GetElemDesignation(block), attKeys[2]);
                                        }
                                    }
                                    block.Location3 = loc;
                                }
                            }
                            else if (block.XsdName == "FoulingPoint") // Fouling Point
                            {
                                string[] attKeys = block.Attributes
                                               .Where(x => x.Key.Contains("KMP"))
                                               .OrderBy(x => x.Key)
                                               .Select(x => x.Key)
                                               .ToArray();
                                string location =
                                    block.Attributes[attKeys[0]].Value.Split('/').First();
                                string location2 =
                                    block.Attributes[attKeys[0]].Value.Split('/').Last();
                                if (!decimal.TryParse(location, out decimal loc))
                                {
                                    if (location == "")
                                    {
                                        loc = 0;
                                    }
                                    else
                                    {
                                        error = true;
                                        ErrLogger.Error("Can not convert to decimal", blckProp.GetElemDesignation(block), attKeys[0]);
                                    }
                                }
                                block.Location = loc;
                                if (!decimal.TryParse(location2, out decimal loc2))
                                {
                                    if (location2 == "")
                                    {
                                        loc2 = 0;
                                    }
                                    else
                                    {
                                        error = true;
                                        ErrLogger.Error("Can not convert to decimal", blckProp.GetElemDesignation(block), attKeys[0]);
                                    }
                                }
                                block.Location2 = loc2;

                            }
                            Blocks.Add(block);
                        }
                    }
                }
                trans.Commit();
            }
            return Blocks;
        }

        private List<Block> GetBlocks(string BlockName, Database db)
        {
            List<Block> Blocks = new List<Block>();
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                foreach (ObjectId btrId in bt)
                {

                    BlockTableRecord btr = (BlockTableRecord)trans.GetObject(btrId, OpenMode.ForRead);
                    if (!btr.IsLayout && BlocksToGet.ContainsKey(btr.Name) &&
                        BlocksToGet[btr.Name].Split('\t')[1] == BlockName)
                    {
                        ObjectIdCollection aRefIds = new ObjectIdCollection();
                        if (btr.IsDynamicBlock)
                        {
                            var blockIds = btr.GetAnonymousBlockIds();
                            foreach (ObjectId BlkId in blockIds)
                            {
                                BlockTableRecord btr2 =
                                    (BlockTableRecord)trans.GetObject(BlkId, OpenMode.ForRead, false, false);
                                ObjectIdCollection aRefIds2 = btr2.GetBlockReferenceIds(true, true);
                                foreach (ObjectId id in aRefIds2)
                                {
                                    aRefIds.Add(id);
                                }
                            }
                            if (blockIds.Count == 0)
                            {
                                aRefIds = btr.GetBlockReferenceIds(false, true);
                            }
                        }
                        else
                        {
                            aRefIds = btr.GetBlockReferenceIds(false, true);
                        }
                        //aRefIds = btr.GetBlockReferenceIds(false, true);
                        foreach (ObjectId RefId in aRefIds)
                        {

                            BlockReference blkRef = (BlockReference)trans.GetObject(RefId, OpenMode.ForRead);
                            //if (blkRef.Layer.Contains("Invisible")) { continue; }
                            //if (!blkRef.BlockName.Contains("MODEL_SPACE"))
                            //{
                            //    continue;
                            //}
                            Dictionary<string, Attribute> Attributes = GetAttributes(blkRef);
                            Block block = new Block
                            {
                                BlkRef = blkRef,
                                BlockName = btr.Name,
                                ElType = BlocksToGet[btr.Name].Split('\t')[2],
                                XsdName = BlocksToGet[btr.Name].Split('\t')[1],
                                X = Math.Round(blkRef.Position.X, 0, MidpointRounding.AwayFromZero),
                                Y = Math.Round(blkRef.Position.Y, 0, MidpointRounding.AwayFromZero),
                                Rotation = (int)(blkRef.Rotation * (180 / Math.PI)),
                                KindOf = BlocksToGet[btr.Name].Split('\t')[3],
                                Attributes = Attributes,
                                IsOnCurrentArea = true,
                                Visible = !(Attributes.Any(x => x.Value.Name == "NAME" &&
                                                               x.Value.Visible == false) ||
                                          !blkRef.BlockName.Contains("MODEL_SPACE"))
                            };
                            if (!block.Visible)
                            {
                                Blocks.Add(block);
                                continue;
                            }
                            //if (block.XsdName == "Point" &&
                            //    block.Attributes["NAME"].Value.Contains("SN") &&
                            //    block.KindOf != "hhtDerailer")
                            //{
                            //    block.KindOf = "hhtPoint";
                            //}
                            if (block.Attributes.Any(x => x.Key == "KMP"))
                            {
                                decimal.TryParse(Regex.Replace(block.Attributes["KMP"].Value, "[^0-9]", ""), out decimal location);
                                block.Location = location;
                                //block.Location = Convert.ToDecimal(block.Attributes["KMP"].Value);
                            }
                            if (block.Attributes.Any(x => x.Key == "OKMP2"))
                            {
                                block.Location = Convert.ToDecimal(block.Attributes["OKMP2"].Value);
                            }
                            Blocks.Add(block);
                        }
                    }
                }
                trans.Commit();
            }
            return Blocks;
        }

        private Dictionary<string, Attribute> GetAttributes(BlockReference blkRef)
        {
            Dictionary<string, Attribute> map = new Dictionary<string, Attribute>();
            AttributeCollection atts = blkRef.AttributeCollection;
            foreach (ObjectId arId in atts)
            {
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    AttributeReference attRef =
                    (AttributeReference)trans.GetObject(arId, OpenMode.ForRead);
                    Attribute attribute =
                        new Attribute
                        {
                            Name = attRef.Tag,
                            Value = attRef.TextString
                        };
                    if (attRef.Layer.Contains("Invisible") || attRef.Invisible == true)
                    {
                        attribute.Visible = false;
                    }
                    else
                    {
                        attribute.Visible = true;
                    }
                    try
                    {
                        map.Add(attRef.Tag, attribute);
                    }
                    catch (ArgumentException e)
                    {
                        ErrLogger.Error("Attribute exception " + e.Message, blkRef.Name, attRef.Tag );
                    }
                    trans.Commit();
                }
            }
            return map;
        }

        protected bool ReadSigLayout(List<Block> blocks, ref TFileDescr siglayout, bool display = false)
        {
            Block BlkSigLayout = blocks.Where(x => x.XsdName == "SignallingLayout").Select(x => x).First();
            string tmpCreator = BlkSigLayout.Attributes
                       .Where(x => x.Key.Contains("KONSTRUERET") && x.Value.Value != "")
                       .Select(y => y.Value.Value)
                       .FirstOrDefault();
            if (tmpCreator.Split(null).Length > 0)
            {
                siglayout.creator = tmpCreator
                    .Split(new char[] { ' ', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
            }
            string inputDate = Regex.Split(BlkSigLayout.Attributes["UDGAVE"].Value, @"\s{1,}")[1];

            siglayout.date = Calc.StringToDate(inputDate, out DateTime date, out bool flag);
            siglayout.title = BlkSigLayout.Attributes["2-TEGN.NAVN"].Value + " - " +
                              BlkSigLayout.Attributes["1-ST.NAVN"].Value;
            //siglayout.title += " (rev. " + Regex.Split(BlkSigLayout.Attributes["UDGAVE"].Value, @"\s{1,}")[0] + ")";
            siglayout.version = docVrs; //+ " (rev. " + Regex.Split(BlkSigLayout.Attributes["UDGAVE"].Value, @"\s{1,}")[0] + ")"; 
            siglayout.docID = docId.ToUpper();
            if (display)
            {
                return true;
            }

            char[] split = new char[] { '-', '(' };
            stationName =
                BlkSigLayout.Attributes["1-ST.NAVN"].Value.Split(split)[0].TrimEnd(')').Trim();
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            stationName = textInfo.ToTitleCase(stationName.ToLower());
            frmStation = new FrmStation
            {
                StationId = stationID,
                StationName = stationName
            };

            frmStation.Lines = RailwayLines;

            if (loadFiles != null)
            {
                frmStation.LoadFiles = loadFiles;
            }

            if (Application.ShowModalDialog(null, frmStation, true) == System.Windows.Forms.DialogResult.OK)
            {
                stationID = frmStation.StationId;
                Status.status = StatusType.@new;
                loadFiles = frmStation.LoadFiles;
                checkData = frmStation.CheckData;
                Version = frmStation.GetVersion();
                DocID = frmStation.GetDocId();
                ZeroLevelLine = frmStation.ZeroLevelLine;
                orderRddFileName = frmStation.GetOrderRddFileName();
                copyLevel = frmStation.CopyLevel;
                //List<string> tmpLoadFiles = loadFiles.Select(x => (x.Key + "\t" + x.Value)).ToList();
                File.WriteAllLines(dwgDir + "//" + dwgFileName + ".ini",
                    loadFiles.Select(x => (x.Key + '\t' + x.Value)).ToList());
            }
            else
            {
                return false;
            }
            return true;
        }

        protected void ReadLines(ref List<LinesLine> lines)
        {
            foreach (RailwayLine railwayLine in this.RailwayLines)
                lines.Add(new LinesLine()
                {
                    Designation = railwayLine.designation,
                    BeginKM = railwayLine.start,
                    EndKM = railwayLine.end,
                    Direction = railwayLine.direction,
                    Status = this.Status
                });
        }

        private void ReadStationsStops(ref List<StationsAndStopsStationsAndStop> stationsAndStops,
             List<StationStop> Stops)
        {
            StationStop station = Stops.Where(x => x.Id == stationID).FirstOrDefault();
            if (station != null)
            {
                stationName = station.Name;
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                stationName = textInfo.ToTitleCase(stationName.ToLower());
            }
            StationsAndStopsStationsAndStop stationsAndStop = new StationsAndStopsStationsAndStop
            {
                Designation = stationID,
                LongName = stationName,
                Status = Status,
                KindOfSAS = KindOfSASType.station,
            };
            List<StationsAndStopsStationsAndStopLinesLine> stationsAndStopLinesLine =
                new List<StationsAndStopsStationsAndStopLinesLine>();
            foreach (RailwayLine line in RailwayLines)
            {
                List<Block> LineBeginEnd = TrackSegmentsTmp.SelectMany(x => x.BlocksOnSegments)
                                     .Where(x => x.Visible == true &&
                                                 x.LineID == line.designation &&
                                                 (x.XsdName == "Signal" /*|| x.XsdName == "EndOfTrack"*/))
                                     .OrderBy(x => x.Location)
                                     .ToList();

                stationsAndStopLinesLine.Add(new StationsAndStopsStationsAndStopLinesLine
                {
                    LineID = line.designation,
                    StartKM = LineBeginEnd.Count == 0 ? "not found" : LineBeginEnd.First().Attributes["KMP"].Value,
                    EndKM = LineBeginEnd.Count == 0 ? "not found" : LineBeginEnd.Last().Attributes["KMP"].Value
                });
            }

            stationsAndStop.Lines = new StationsAndStopsStationsAndStopLines
            {
                Line = stationsAndStopLinesLine.ToArray()
            };
            stationsAndStops.Add(stationsAndStop);
            foreach (StationStop stop in Stops.Where(x => x.Id != stationID))
            {
                stationsAndStopLinesLine =
                new List<StationsAndStopsStationsAndStopLinesLine>();
                foreach (string line in stop.LineIDs)
                {
                    stationsAndStopLinesLine.Add(new StationsAndStopsStationsAndStopLinesLine
                    {
                        LineID = line,
                        StartKM = stop.StartKm.ToString(),
                        EndKM = stop.EndKm.ToString()
                    });
                }
                stationsAndStop = new StationsAndStopsStationsAndStop
                {
                    Designation = stop.Id,
                    LongName = stop.Name,
                    Status = Status,
                    KindOfSAS = KindOfSASType.stop,
                    Lines = new StationsAndStopsStationsAndStopLines
                    {
                        Line = stationsAndStopLinesLine.ToArray()
                    }
                };
                stationsAndStops.Add(stationsAndStop);
            }
            stationsAndStops = stationsAndStops
                               .OrderBy(x => x.Lines.Line.Max(l => l.StartKM))
                               .ToList();
        }

        protected bool ReadSignals(List<Block> blocks, ref List<SignalsSignal> signalsSignal)
        {
            bool error = false;
            List<Block> BlkSignals = blocks
                                        .Where(x => x.XsdName == "Signal" &&
                                                    x.IsOnCurrentArea == true &&
                                                    x.IsOnNextStation == false &&
                                                    x.Visible == true)
                                        .OrderBy(x => x.Attributes["NAME"].Value)
                                        .ToList();
            List<ReadExcel.XlsRoute> xlsRoutes = null;
            TFileDescr document;
            if (checkData["checkBoxRts"])
            {
                document =
               new TFileDescr();
                xlsRoutes =
                    excel.Routes(loadFiles["lblxlsRoutes"], ref document, ref error)
                    //.Where(x => x.Overlaps.Count > 0)
                    .ToList();
            }
            document = new TFileDescr();
            List<ReadExcel.Signal> CesLocs = null;
            if (checkData["checkBoxSC"])
            {
                CesLocs =
                    excel.DangerPoints(loadFiles["lblxlsSigClos"], ref document);
                Documents.Add(document);
                if (CesLocs == null)
                {
                    ErrLogger.Error("Can't get data from file",  "Signals closure table", "");
                    error = true;
                    //return !error;
                }
            }
            else
            {
                ErrLogger.Information("Data skipped", "Signals closure table");
            }
            
            foreach (Block BlkSignal in BlkSignals)
            {
                SignalsSignal signal = new SignalsSignal
                {
                    Designation = blckProp.GetElemDesignation(BlkSignal)
                };
                BlkSignal.Designation = signal.Designation;
                signal.Status = Status;
                Block checkEot = blocks
                                 .Where(x => x.XsdName == "EndOfTrack" &&
                                        x.Location == BlkSignal.Location)
                                 .FirstOrDefault();
                if (checkEot != null) //(BlkSignal.Attributes.ContainsKey("EOTMB") && BlkSignal.Attributes["EOTMB"].Value.Equals("yes"))
                {
                    signal.KindOfSignal = TKindOfSignal.eotmb; //signal.KindOfSignal = (TKindOfSignal)Enum.Parse(typeof(TKindOfSignal), "eotmb");
                }
                else
                {
                    signal.KindOfSignal = (TKindOfSignal)Enum.Parse(typeof(TKindOfSignal), BlkSignal.KindOf);
                }
                List<TrackSegmentTmp> tmpSegments = TrackSegmentsTmp
                    .Where(s => s.BlocksOnSegments
                    .Any(x => x.Designation.Equals(blckProp.GetElemDesignation(BlkSignal)))).ToList();
                if (tmpSegments.Count == 1)
                {
                    signal.TrackSegmentID = tmpSegments[0].Designation;
                }
                else
                {
                    signal.TrackSegmentID = "";
                    ErrLogger.Error("TrackSegmentId not found", blckProp.GetElemDesignation(BlkSignal), "");
                    error = true;
                }
                signal.LineID = BlkSignal.LineID;
                signal.Location = Convert.ToDecimal(BlkSignal.Attributes["KMP"].Value);
                signal.Direction = GetSignalDirection(BlkSignal, ref error);
                signal.TrackPosition = GetSignalTrackPosition(BlkSignal, ref error);

                // DK2.1 Danger point distance is calculated by DMT,
                // but requested to validation facilities
                bool toPsa = IsSignalToPSA(BlkSignal, signal.Direction);
                DangerPoint dangerPoint = new DangerPoint();
                if (/*signal.KindOfSignal == TKindOfSignal.L2EntrySignal ||
                              signal.KindOfSignal == TKindOfSignal.L2ExitSignal ||*/
                              signal.KindOfSignal == TKindOfSignal.foreignSignal ||
                              signal.KindOfSignal == TKindOfSignal.eotmb)
                {
                    dangerPoint.Distance = 0;
                    dangerPoint.DistanceSpecified = true;
                }
                else
                {
                    dangerPoint = GetDangerPoint(BlkSignal, signal.Direction);
                }
                signal.DangerPointDistanceSpecified = dangerPoint.DistanceSpecified;
                signal.DangerPointDistance = dangerPoint.Distance;
                signal.DangerPointID = dangerPoint.Id;

                //if (GetType() == typeof(Display))
                //{
                //    signalsSignal.Add(signal);
                //    continue;
                //}

                if (checkData["checkBoxSC"])
                {
                    if (CesLocs != null)
                    {
                        ReadExcel.Signal cesLoc = CesLocs
                                              .Where(x => x.Mb == BlkSignal.Attributes["NAME"].Value)
                                              .FirstOrDefault();
                        if (cesLoc == null)
                        {
                            ErrLogger.Error("Element not found in Signals Closure Table", signal.Designation, "");
                            error = true;
                        }
                        else
                        {
                            signal.ShiftCESLocation = cesLoc.OCes;
                            Block Ac = null;
                            Ac = blocks
                                   .Where(x => x.XsdName == "DetectionPoint" &&
                                               x.Attributes["NAME"].Value == cesLoc.Ac)
                                   .FirstOrDefault();
                            if (Ac == null)
                            {
                                ErrLogger.Error("Danger point from SC table not found on SL", signal.Designation, cesLoc.Ac);
                                error = true;
                            }
                        }
                    }
                }
                else
                {
                    if (signal.KindOfSignal == TKindOfSignal.eotmb ||
                    signal.KindOfSignal == TKindOfSignal.foreignSignal ||
                    toPsa ||
                    signal.KindOfSignal == TKindOfSignal.L2ExitSignal)
                    {
                        signal.ShiftCESLocation = 0;
                    }
                    else
                    {
                        ShiftCesBG cesBG = GetCesBalise(BlkSignal, signal.Direction);
                        if (cesBG != null && dangerPoint != null)
                        {
                            signal.ShiftCESLocation = GetShiftCESLocationValue(BlkSignal.Location, dangerPoint, cesBG);
                        }
                    }
                }
                signal.ShiftCESLocationSpecified = true;
                
                signalsSignal.Add(signal);
            }

            if (this.GetType() == typeof(Display))
            {
                return !error;
            }

            Regex spstSig = new Regex("^(spst-[a-zæøåÆØÅ]{2,3}-)S([0-9]{1,3})");
            List<ReadExcel.XlsRoute> checkSpsts = xlsRoutes.Where(x => spstSig.IsMatch(x.Start)).ToList();
            foreach (ReadExcel.XlsRoute route in checkSpsts)
            {
                Block spstBlock = blocks
                                  .Where(x => x.XsdName == "EndOfTrack" &&
                                              x.Designation == spstSig.Replace(route.Start, "$1$2"))
                                  .FirstOrDefault();
                if (spstBlock == null)
                {
                    error = true;
                    ErrLogger.Error("Unable to find EOT for route ", route.Start + "_" + route.Dest, "");
                    continue;
                }
                if (signalsSignal.Where(x => x.Designation == route.Start).Count() > 0)
                {
                    continue;
                }
                SignalsSignal signalVirtualEot = new SignalsSignal
                {
                    Designation = route.Start,
                    KindOfSignal = TKindOfSignal.mb,
                    DangerPointDistance = 0,
                    DangerPointDistanceSpecified = true,
                    DangerPointID = null,
                    Status = Status,
                    TrackSegmentID = spstBlock.TrackSegId,
                    LineID = spstBlock.LineID,
                    Location = spstBlock.Location,
                    ShiftCESLocationSpecified = true,
                    ShiftCESLocation = 0,
                    TrackPosition = LeftRightOthersType.others
                };
                DirectionType directionEot = GetEotDirection(spstBlock, ref error);
                if (directionEot == DirectionType.up)
                {
                    signalVirtualEot.Direction = DirectionType.down;
                }
                else if (directionEot == DirectionType.down)
                {
                    signalVirtualEot.Direction = DirectionType.up;
                }

                signalVirtualEot.DangerPointDistanceSpecified = true;
                signalVirtualEot.DangerPointDistance = 0;
                signalVirtualEot.DangerPointID = null;

                signalsSignal.Add(signalVirtualEot);
            }
            return !error;
        }

        protected bool ReadPoints(List<Block> blocks, ref List<PointsPoint> pointsPoints,
            List<SpeedProfilesSpeedProfile> speedProfiles, List<PSA> pSAs, List<EmSG> emGs)
        {
            bool error = false;
            List<Block> BlkPoints = blocks
                                      .Where(x => (x.XsdName == "Point" &&
                                                   x.IsOnCurrentArea == true &&
                                                   x.IsOnNextStation == false &&
                                                   x.Visible == true))
                                      .ToList();
            TFileDescr document;

            List<ReadExcel.FlankProtection> FlankProtection = new List<ReadExcel.FlankProtection>();
            if (checkData["checkBoxFP"])
            {
                document = new TFileDescr();
                FlankProtection = excel.FlankProtection(loadFiles["lblxlsFP"], ref document);
                Documents.Add(document);
            }
            else
            {
                ErrLogger.Information("Flank Protection data skipped", "Flank protection table");
            }

            List<ReadExcel.EmergStopGroup> emergStopGroups = new List<ReadExcel.EmergStopGroup>();
            if (checkData["checkBoxEmSt"])
            {
                document = new TFileDescr();
                emergStopGroups =
                    excel.EmergStops(loadFiles["lblxlsEmSg"], ref document);
                Documents.Add(document);
            }
            else
            {
                ErrLogger.Information("Emergency Stop Group data skipped", "Emergency Stop Group table");
            }

            foreach (Block BlkPoint in BlkPoints)
            {
                //Regex regexFp = new Regex("^N([0-9]{1,3})|^SN([0-9]{1,3})");
                List<PointsPointLinesLine> pointsPointLines = new List<PointsPointLinesLine>();
                Block FoulPoints = blocks
                    .Where(x => x.BlockName == "Fouling-Point")
                    .Where(y => y.Attributes["NAME"].Value ==
                        Regex.Replace(BlkPoint.Attributes["NAME"].Value, "^N([0-9]{1,3})|^SN([0-9]{1,3})", "$1$2"))
                    .FirstOrDefault();
                if (FoulPoints == null)
                {
                    pointsPointLines.Add(new PointsPointLinesLine
                    {
                        KindOfPC = KindOfPCType.all,
                        LineID = BlkPoint.LineID,
                        Location = Convert.ToDecimal(BlkPoint.Attributes["KMP"].Value).ToString()
                    });
                    ErrLogger.Information("Fouling point not found", blckProp.GetElemDesignation(BlkPoint));
                    //ExportPoints.Add(stationName + "\t" +
                    //                 BlkPoint.Attributes["NAME"].Value + "\t" +
                    //                 Convert.ToInt32(Convert.ToDecimal(BlkPoint.Attributes["KMP"].Value) * 1000) + "\t" +
                    //                 Convert.ToInt32(Convert.ToDecimal(BlkPoint.Attributes["KMP"].Value) * 1000) + "\t" +
                    //                 Convert.ToInt32(Convert.ToDecimal(BlkPoint.Attributes["KMP"].Value) * 1000));
                }
                else
                {

                    if (!decimal.TryParse(BlkPoint.Attributes["KMP"].Value, out decimal locTip))
                    {
                        locTip = 0;
                    }
                    if (!decimal.TryParse(BlkPoint.Attributes["KMP_CONTACT_2"].Value, out decimal locRight))
                    {
                        locRight = Convert.ToDecimal(BlkPoint.Attributes["KMP"].Value);
                    }
                    if (!decimal.TryParse(BlkPoint.Attributes["KMP_CONTACT_3"].Value, out decimal locLeft))
                    {
                        locLeft = Convert.ToDecimal(BlkPoint.Attributes["KMP"].Value);
                    }
                    pointsPointLines.Add(new PointsPointLinesLine
                    {
                        KindOfPC = KindOfPCType.tip,
                        LineID = BlkPoint.LineIDtip,
                        Location = Convert.ToDecimal(BlkPoint.Attributes["KMP"].Value).ToString()
                    });

                    pointsPointLines.Add(new PointsPointLinesLine
                    {
                        KindOfPC = KindOfPCType.right,
                        LineID = BlkPoint.LineIDright ?? BlkPoint.LineIDtip,
                        Location = BlkPoint.Attributes["KMP_CONTACT_2"].Value == "" ?
                                Convert.ToDecimal(BlkPoint.Attributes["KMP"].Value).ToString() :
                                Convert.ToDecimal(BlkPoint.Attributes["KMP_CONTACT_2"].Value).ToString(),
                        FoulingPointLocation =
                            GetFoulPointLocation(FoulPoints, BlkPoint, ConnectionBranchType.right)
                    });

                    pointsPointLines.Add(new PointsPointLinesLine
                    {
                        KindOfPC = KindOfPCType.left,
                        LineID = BlkPoint.LineIDleft ?? BlkPoint.LineIDtip,
                        Location = BlkPoint.Attributes["KMP_CONTACT_3"].Value == "" ?
                                Convert.ToDecimal(BlkPoint.Attributes["KMP"].Value).ToString() :
                                Convert.ToDecimal(BlkPoint.Attributes["KMP_CONTACT_3"].Value).ToString(),
                        FoulingPointLocation =
                            GetFoulPointLocation(FoulPoints, BlkPoint, ConnectionBranchType.left)
                    });
                    //ExportPoints.Add(stationName + "\t" +
                    //                 BlkPoint.Attributes["NAME"].Value + "\t" +
                    //                 Convert.ToInt32(locTip * 1000) + "\t" +
                    //                 Convert.ToInt32(locRight * 1000) + "\t" +
                    //                 Convert.ToInt32(locLeft * 1000));
                }

                List<PointsPointPointMachinesPointMachine> pointMachines =
                        new List<PointsPointPointMachinesPointMachine>();
                List<TrackSegmentTmp> speedSeg =
                    TrackSegmentsTmp.Where(x => x.Vertex1 == BlkPoint ||
                                                x.Vertex2 == BlkPoint).ToList();
                List<SpeedProfilesSpeedProfileTrainTypesTrainTyp> MaxSpeedTrainTypes =
                            speedProfiles
                            .Where(x => x.TrackSegments.TrackSegment
                                        .Any(t => speedSeg
                                                  .Select(y => y.Designation)
                                                                .Contains(t.Value)))
                            .Where(s => s.TrainTypes != null)
                            .SelectMany(t => t.TrainTypes.TrainTyp).ToList();
                decimal MaxSpeed = 0;
                if (MaxSpeedTrainTypes != null && MaxSpeedTrainTypes.Count != 0)
                {
                    MaxSpeed = MaxSpeedTrainTypes.Select(x => x.SpeedLimit).Max();
                }
                else if (speedProfiles.Count > 0)
                {
                    try
                    {
                        MaxSpeed = speedProfiles
                          .Where(x => x.TrackSegments.TrackSegment
                                      .Any(t => speedSeg
                                                .Select(y => y.Designation)
                                                              .Contains(t.Value)))
                           .Select(y => y.SpeedMax).Max();
                    }
                    catch
                    {
                        ErrLogger.Error("MaxSpeed not found", blckProp.GetElemDesignation(BlkPoint), "");
                        error = true;
                    }
                }
                //.Select(x => x.SpeedLimit).Max();

                KindOfPMType kindOfPM = KindOfPMType.L710H;
                if (MaxSpeed > 160 ||
                    BlkPoint.KindOf.ToLower().Contains("derailer") ||
                    BlkPoint.KindOf.ToLower().Contains("trap"))
                {
                    kindOfPM = KindOfPMType.L826H;
                }
                LeftRightType position = GetPointMachines(BlkPoint, out int count);
                for (int i = 1; i <= count; i++)
                {
                    pointMachines.Add(new PointsPointPointMachinesPointMachine
                    {
                        Designation = count == 1 ?
                                        blckProp.GetElemDesignation(BlkPoint) :
                                        blckProp.GetElemDesignation(BlkPoint) + "-" + i.ToString(),
                        KindOfPM = kindOfPM,
                        TrackPosition = position
                    });
                }

                PointsPoint point = new PointsPoint
                {
                    Designation = blckProp.GetElemDesignation(BlkPoint),
                    Status = Status,
                    KindOfPoint = IsPointHht(BlkPoint, ref blocks),
                    Lines = new PointsPointLines { Line = pointsPointLines.ToArray() },
                    Trailable = PointTrailable(BlkPoint),
                    PointPosIndicator = GetPointPosIndicator(BlkPoint),
                    PointMachines = new PointsPointPointMachines { PointMachine = pointMachines.ToArray() }
                };
                if (point.KindOfPoint == KindOfPointType.hhtDerailer)
                {
                    point.PointPosIndicator = YesNoType.yes;
                }


                point.FlankProtectionAbandonmentLeftSpecified = true;
                point.FlankProtectionAbandonmentRightSpecified = true;

                if (checkData["checkBoxFP"])
                {
                    ReadExcel.FlankProtection Fp = FlankProtection
                                              .FirstOrDefault(x => x.Pt == BlkPoint.Attributes["NAME"].Value);
                    if (Fp != null)
                    {
                        point.FlankProtectionAbandonmentLeft = Fp.Left;
                        point.FlankProtectionAbandonmentRight = Fp.Right;
                        if (Fp.LeftTdt != null)
                        {
                            point.AdditionalAxleCounterSectionsRight =
                                new PointsPointAdditionalAxleCounterSectionsRight
                                {
                                    AxleCounterSectionID = new string[1] {
                                        string.Join("-", new string[] { "tdt", stationID, Fp.LeftTdt.Split('-').Last() })
                                    }
                                };
                        }
                        if (Fp.RightTdt != null)
                        {
                            point.AdditionalAxleCounterSectionsLeft =
                                new PointsPointAdditionalAxleCounterSectionsLeft
                                {
                                    AxleCounterSectionID = new string[1] {
                                        string.Join("-", new string[] { "tdt", stationID, Fp.RightTdt.Split('-').Last() })
                                    }
                                };
                        }
                    }
                    else
                    {
                        ErrLogger.Information("Flank Protection not found", blckProp.GetElemDesignation(BlkPoint));
                    }
                }

                if (checkData["checkBoxEmSt"])
                {
                    ReadExcel.EmergStopGroup emergStop = emergStopGroups
                                                        .FirstOrDefault(x => x.Designation == point.Designation &&
                                                        x.EmergSg != null);
                    if (emergStop != null)
                    {
                        point.EmergencyStopGroup = emergStop.EmergSg;
                    }
                    else
                    {
                        ErrLogger.Information("Emergency Stop Group not found", point.Designation);
                    }
                }
                else
                {
                    EmSG egs = emGs
                                            .Where(x => BlkPoint.X >= x.MinX &&
                                                        BlkPoint.X <= x.MaxX &&
                                                        BlkPoint.Y >= x.MinY &&
                                                        BlkPoint.Y <= x.MaxY && x.Order == 1)
                                             .FirstOrDefault();
                    if (egs != null)
                    {
                        point.EmergencyStopGroup = "emgs-" + stationID.ToLower() + "-" + egs.Designation.ToLower();
                    }
                }
                PSA SegPsa = pSAs
                                             .Where(x => BlkPoint.X >= x.MinX &&
                                                         BlkPoint.X <= x.MaxX &&
                                                         BlkPoint.Y >= x.MinY &&
                                                         BlkPoint.Y <= x.MaxY)
                                              .FirstOrDefault();
                if (SegPsa != null)
                {
                    point.InsidePSA = SegPsa.Name.ToLower();
                }
                pointsPoints.Add(point);
            }
            return !error;
        }

        private bool ReadDps(List<Block> blocks, ref List<DetectionPointsDetectionPoint> detPointsDetPoints,
            List<PSA> pSAs)
        {
            bool error = false;
            List<Block> BlkDPs = blocks
                                    .Where(x => (x.XsdName == "DetectionPoint") &&
                                                 x.IsOnCurrentArea == true &&
                                                 x.IsOnNextStation == false &&
                                                 x.Visible == true)
                                    .OrderBy(x => blckProp.GetElemDesignation(x))
                                    .ToList();
            foreach (Block BlkDP in BlkDPs)
            {
                DetectionPointsDetectionPoint detpoint = new DetectionPointsDetectionPoint
                {
                    Designation = blckProp.GetElemDesignation(BlkDP)
                };
                BlkDP.Designation = detpoint.Designation;
                detpoint.Status = Status;
                detpoint.KindOfDP = KindOfDPType.axleCounter;
                List<TrackSegmentTmp> tmpSegments = TrackSegmentsTmp
                    .Where(s => s.BlocksOnSegments
                    .Any(x => x.Designation.Equals(blckProp.GetElemDesignation(BlkDP)) &&
                              x.IsOnCurrentArea)).ToList();
                if (tmpSegments.Count == 1)
                {
                    detpoint.TrackSegmentID = tmpSegments[0].Designation;
                }
                else
                {
                    detpoint.TrackSegmentID = "";
                    ErrLogger.Error("TrackSegmentId not found", detpoint.Designation, "");
                    error = true;
                }
                detpoint.LineID = BlkDP.LineID;
                detpoint.Location = BlkDP.Attributes["KMP"].Value;
                BlkDP.Location = Convert.ToDecimal(detpoint.Location);
                PSA SegPsa = pSAs
                                             .Where(x => BlkDP.X >= x.MinX + 2 &&
                                                         BlkDP.X <= x.MaxX - 2 &&
                                                         BlkDP.Y >= x.MinY + 2 &&
                                                         BlkDP.Y <= x.MaxY - 2 &&
                                                         !ObjectsIntersects(BlkDP.BlkRef, x.PsaPolyLine, Intersect.OnBothOperands))
                                              .FirstOrDefault();
                if (SegPsa != null)
                {
                    detpoint.InsidePSA = SegPsa.Name.ToLower();
                }
                detPointsDetPoints.Add(detpoint);
            }
            return !error;
        }

        private void ReadConnectors(List<Block> blocks, ref List<ConnectorsConnector> connectorsConnectors)
        {
            List<Block> BlkConnectors = blocks
                                           .Where(x => (x.XsdName == "Connector" &&
                                                        x.IsOnCurrentArea == true &&
                                                        x.IsOnNextStation == false &&
                                                        x.Visible == true))
                                           .OrderBy(x => x.Location)
                                           .ToList();
            foreach (Block BlkConn in BlkConnectors)
            {
                ConnectorsConnector connector = new ConnectorsConnector
                {
                    Designation = blckProp.GetElemDesignation(BlkConn)
                };
                BlkConn.Designation = connector.Designation;
                connector.Status = Status;
                connector.OperationalKM1 = BlkConn.Location.ToString();
                connector.OperationalKM2 = BlkConn.Location2.ToString();
                List<TrackSegmentTmp> trackSegments = TrackSegmentsTmp
                                .Where(x => (x.Vertex1.Designation == BlkConn.Designation ||
                                             x.Vertex2.Designation == BlkConn.Designation))
                                             .OrderBy(x => x.Vertex1.Location).ToList();
                if (trackSegments.Count == 1)
                {
                    connector.TrackSegmentID1 = trackSegments[0].Designation;
                    ErrLogger.Information("TrackSegmentID2 not found", connector.Designation);
                }
                if (trackSegments.Count > 1)
                {
                    connector.TrackSegmentID1 = trackSegments[0].Designation;
                    connector.TrackSegmentID2 = trackSegments[1].Designation;
                }
                connectorsConnectors.Add(connector);
            }
        }

        private bool ReadAcSections(List<Block> blocks,
                               ref List<AxleCounterSectionsAxleCounterSection> axleCounterSections)
        {
            bool error = false;
            List<Block> BlkAcSections = blocks
                                           .Where(x => (x.XsdName == "AxleCounterSection" ||
                                                        x.XsdName == "TrackSection") &&
                                                        x.IsOnNextStation == false &&
                                                        x.Visible == true)
                                           .OrderBy(x => blckProp.GetElemDesignation(x))
                                           .ToList();

            //List<BlockRef> BlkEOTs = blocks.Where(x => (x.XsdName == "EndOfTrack") &&
            //                                          x.IsOnCurrentArea == true
            //                                    )
            //                             .ToList();

            //TEst ac new
            if (frmStation.AutoAC)
            {
                ErrLogger.Information("AC sections are calculated from SL", "Auto AC");
                AssignDpsToTrckSections();
            }

            List<Block> BlkDPs = null;
            foreach (Block BlkAcSection in BlkAcSections)
            {

                Regex dpAttsPatt = new Regex(@"^DP{1}\d+");
                Regex patternDP = new Regex("^at-[0-9]{3}|^[0-9]{3}|^at-" + stationID.ToLower() + "-[0-9]{3}");
                string[] DPs = null;
                DPs = BlkAcSection.Attributes
                         .Where(x => dpAttsPatt.IsMatch(x.Value.Name) &&
                                      patternDP.IsMatch(x.Value.Value) &&
                                      !String.IsNullOrEmpty(x.Value.Value.ToString()))
                         //.Select(x => (x.Value.Value)
                         .Select(x => ("at-" + stationID.ToLower() + "-" + x.Value.Value.Split('-').Last())
                         .Trim())
                         .ToArray();
                BlkDPs = blocks
                         .Where(x => x.XsdName == "DetectionPoint")
                         .ToList();
                string[] DPsForeign = BlkAcSection.Attributes
                             .Where(x => (dpAttsPatt.IsMatch(x.Value.Name) &&
                                          Regex.IsMatch(x.Value.Value, "^at-[a-zæøåÆØÅ]{2,3}-[0-9]{3}") &&
                                          !String.IsNullOrEmpty(x.Value.Value.ToString())))
                             .Select(x => x.Value.Value.Trim())
                             .ToArray();
                DPs = DPs.Union(DPsForeign).ToArray();

                //List<AxleCounterSectionsAxleCounterSectionDetectionPointsDetectionPoint> AcDps =
                //    new List<AxleCounterSectionsAxleCounterSectionDetectionPointsDetectionPoint>();
                List<AxleCounterSectionsAxleCounterSectionElementsElement> elements =
                    new List<AxleCounterSectionsAxleCounterSectionElementsElement>();
                List<Block> tmpDps = null;
                //if (checkData.ContainsKey("checkBoxAc") && checkData["checkBoxAc"])
                //{
                //    tmpDps = GetDpsOfAC(BlkAcSection, trackLines, blocks);
                //}
                //else
                //{
                tmpDps = BlkDPs
                                        .Where(x => DPs.ToList()
                                        .Contains(blckProp.GetElemDesignation(x)))
                                        .OrderBy(x => x.Attributes["NAME"].Value)
                                        .Distinct()
                                        .ToList();
                //}

                List<Block> AcElements = TrackSegmentsTmp
                    .Where(x => x.BlocksOnSegments
                    .Any(a => tmpDps.Contains(a)))
                    .SelectMany(x => x.BlocksOnSegments)
                    .Where(x => x.XsdName == "Point" &&
                                x.X < tmpDps.Max(m => m.X) &&
                                x.X > tmpDps.Min(m => m.X) &&
                                x.Y <= tmpDps.Max(m => m.Y) &&
                                x.Y >= tmpDps.Min(m => m.Y))
                    .Distinct()
                    .ToList();

                if (BlkAcSection.XsdName == "TrackSection" ||
                    BlkAcSection.BlockName.Split('_').Last() == "EOT")
                {
                    elements.Add(new AxleCounterSectionsAxleCounterSectionElementsElement
                    {
                        Value = blckProp.GetElemDesignation(BlkAcSection)
                    });
                    Point2d tdt =
                        new Point2d(BlkAcSection.BlkRef.Position.X, BlkAcSection.BlkRef.Position.Y);
                    foreach (Block Dp in tmpDps)
                    {
                        Point2d dp =
                        new Point2d(Dp.BlkRef.Position.X, Dp.BlkRef.Position.Y);
                        Dp.Sort = tdt.GetVectorTo(dp).Angle;
                        if (Calc.Between(Calc.RadToDeg(Dp.Sort), 359, 360, true))
                        {
                            Dp.Sort = 0;
                        }
                    }
                    tmpDps = tmpDps.OrderByDescending(x => x.Sort).ToList();
                }
                else
                {
                    if (AcElements.Count > 0)
                    {
                        Line pointTip = GetPointTipLine(AcElements[0]);
                        Line pointBase = GetPointBaseLine(AcElements[0]);
                        Point2d Tip = AcadTools.GetMiddlPoint2d(pointTip.GeometricExtents);
                        Point2d Base = AcadTools.GetMiddlPoint2d(pointBase.GeometricExtents);
                        Vector2d pointStraight = Tip.GetVectorTo(Base);
                        foreach (Block Dp in tmpDps)
                        {
                            Point2d dp =
                            new Point2d(Dp.BlkRef.Position.X, Dp.BlkRef.Position.Y);
                            Vector2d pointDp = dp.GetVectorTo(Base);
                            int Angle = Calc.RadToDeg(pointStraight.GetAngleTo(pointDp));
                            Dp.Ac_angle = Angle;
                            if (Angle == 0)
                            {
                                Dp.Sort = 0;
                            }
                            else if (Calc.Between(Angle, 178, 180, true))
                            {
                                Dp.Sort = 1;
                            }
                            else
                            {
                                Dp.Sort = 2;
                            }
                        }
                        //tmpDps = tmpDps.OrderBy(x => x.Sort).ToList();
                        tmpDps = tmpDps.OrderBy(x => x.Ac_angle).ToList();
                    }

                    foreach (var element in AcElements)
                    {
                        elements.Add(new AxleCounterSectionsAxleCounterSectionElementsElement
                        {
                            Value = blckProp.GetElemDesignation(element)
                        });
                    }
                }


                //foreach (Block BlkDp in tmpDps)
                //{
                //    AcDps.Add(new AxleCounterSectionsAxleCounterSectionDetectionPointsDetectionPoint
                //    {
                //        Value = blckProp.GetElemDesignation(BlkDp)
                //    });
                //}
                AcSection section = this.acSections
                                     .FirstOrDefault(x => x.Designation == blckProp.GetElemDesignation(BlkAcSection));
                AxleCounterSectionsAxleCounterSection acSection;
                if (section == null)
                {
                    if (frmStation.AutoAC)
                    {
                        ErrLogger.Error("Unable to auto initialize, AC section created from attributes", blckProp.GetElemDesignation(BlkAcSection),
                            "");
                        error = true;
                    }
                    if (tmpDps.Count != DPs.Length)
                    {
                        ErrLogger.Error("Inconsistent count of Detection points found", blckProp.GetElemDesignation(BlkAcSection),
                            "SL:" + tmpDps.Count + " Atts:" + DPs.Length);
                        error = true;
                    }
                    else if (tmpDps.Count == 0)
                    {
                        ErrLogger.Error("Detection points not found", blckProp.GetElemDesignation(BlkAcSection),
                            "");
                        error = true;
                    }
                    else if (DPs.Count() > 0)
                    {
                        List<string> AttDps = DPs.OrderBy(x => x).ToList();
                        List<string> BlkDps = tmpDps.OrderBy(x => blckProp.GetElemDesignation(x)).Select(x => blckProp.GetElemDesignation(x)).ToList();
                        for (int j = 0; j < AttDps.Count; j++)
                        {
                            if (AttDps[j] != BlkDps[j])
                            {
                                ErrLogger.Error("Detection point not match", blckProp.GetElemDesignation(BlkAcSection),
                                        "Atts:" + AttDps[j] + " SL:" + BlkDps[j]);
                                error = true;
                            }
                        }
                    }
                    else
                    {
                        ErrLogger.Error("Detection points not found in attributes", blckProp.GetElemDesignation(BlkAcSection),
                            "");
                        error = true;
                    }
                    if (BlkAcSection.XsdName == "AxleCounterSection" && AcElements.Count == 0)
                    {
                        ErrLogger.Error("No Elements found for Ac section", blckProp.GetElemDesignation(BlkAcSection),
                            "");
                        error = true;
                        //Logger.Log("No Elements found for Ac section", blckProp.GetElemDesignation(BlkAcSection));
                    }
                    acSection = new AxleCounterSectionsAxleCounterSection
                    {
                        Designation = blckProp.GetElemDesignation(BlkAcSection),
                        Status = Status,
                        DetectionPoints = new AxleCounterSectionsAxleCounterSectionDetectionPoints
                        {
                            DetectionPoint = tmpDps
                                             .Select(x => new AxleCounterSectionsAxleCounterSectionDetectionPointsDetectionPoint
                                             {
                                                 Value = blckProp.GetElemDesignation(x)
                                             })
                                             .ToArray()
                        },
                        Elements = new AxleCounterSectionsAxleCounterSectionElements { Element = elements.ToArray() }
                    };
                    axleCounterSections.Add(acSection);
                }
                else
                {
                    acSection = new AxleCounterSectionsAxleCounterSection
                    {
                        Designation = blckProp.GetElemDesignation(BlkAcSection),
                        Status = Status,
                        DetectionPoints = new AxleCounterSectionsAxleCounterSectionDetectionPoints
                        {
                            DetectionPoint = section.Dps
                                             .Where(x => x.XsdName != "EndOfTrack")
                                             .Select(x => new AxleCounterSectionsAxleCounterSectionDetectionPointsDetectionPoint
                                             {
                                                 Value = x.Designation
                                             })
                                             .ToArray()
                        },
                        //Elements = new AxleCounterSectionsAxleCounterSectionElements
                        //{
                        //    Element = section.Elements
                        //              .Select(x => new AxleCounterSectionsAxleCounterSectionElementsElement
                        //              {
                        //                  Value = x
                        //              })
                        //              .ToArray()
                        //}
                    };
                    if (section.Elements != null && section.Elements.Count > 0)
                    {
                        acSection.Elements = new AxleCounterSectionsAxleCounterSectionElements
                        {
                            Element = section.Elements
                                      .Select(x => new AxleCounterSectionsAxleCounterSectionElementsElement
                                      {
                                          Value = x
                                      })
                                      .ToArray()
                        };
                    }
                    else
                    {
                        ErrLogger.Error("No Elements found for Ac section", acSection.Designation,
                            "");
                        error = true;
                    }
                    if (!DPs.OrderBy(x => x).SequenceEqual(section.Dps.Select(x => x.Designation).OrderBy(x => x)))
                    {
                        ErrLogger.Error("Calculated dps not equal to attributes", blckProp.GetElemDesignation(BlkAcSection),
                            "");
                        error = true;
                    }
                    axleCounterSections.Add(acSection);
                }
            }
            return !error;
        }

        private bool ReadTrSections(List<Block> blocks,
                               ref List<TrackSectionsTrackSection> TrackSections, List<EmSG> emGs)
        {
            bool error = false;
            List<Block> BlkTrckSections = blocks
                                             .Where(x => (x.XsdName == "TrackSection" || x.XsdName == "AXlecounter_Section_EOT") &&
                                             x.IsOnNextStation == false &&
                                                          x.Visible == true)
                                             .OrderBy(x => blckProp.GetElemDesignation(x))
                                             .ToList();

            List<Block> BlkEOTs = blocks.Where(x => (x.XsdName == "EndOfTrack") &&
                                                       x.IsOnCurrentArea == true
                                                 )
                                          .ToList();

            List<Block> BlkDPs = blocks.Where(x => (x.XsdName == "DetectionPoint" ||
                                                       x.XsdName == "EndOfTrack") &&
                                                       x.Visible == true
                                                 )
                                          .ToList().Concat(BlkEOTs).ToList();


            foreach (Block BlkTrackSection in BlkTrckSections)
            {
                Regex dpAttsPatt = new Regex(@"^DP{1}\d+|^EOT{1}[0-9]{0,1}");
                Regex patternDP = new Regex("^at-[0-9]{3}|^[0-9]{3}|^at-" + stationID.ToLower() + "-[0-9]{3}");
                Regex patternSpst = new Regex("^spst-[0-9]{3}|^spst-" + stationID.ToLower() + "-[0-9]{3}|^eot-[0-9]{3}|^eot-" + stationID.ToLower() + "-[0-9]{3}");

                string[] DPs = BlkTrackSection.Attributes
                            .Where(x => (dpAttsPatt.IsMatch(x.Value.Name) &&
                                         patternDP.IsMatch(x.Value.Value) &&
                                         !String.IsNullOrEmpty(x.Value.Value.ToString())))
                            .Select(x => ("at-" + stationID.ToLower() + "-" + x.Value.Value.Split('-').Last())
                                         .Trim())
                            .ToArray();
                string[] DPsForeign = BlkTrackSection.Attributes
                             .Where(x => (dpAttsPatt.IsMatch(x.Value.Name) &&
                                          Regex.IsMatch(x.Value.Value, "^at-[a-zæøåÆØÅ]{2,3}-[0-9]{3}") &&
                                          !String.IsNullOrEmpty(x.Value.Value.ToString())))
                             .Select(x => x.Value.Value.Trim())
                             .ToArray();
                string[] SPSTs = BlkTrackSection.Attributes
                           .Where(x => (dpAttsPatt.IsMatch(x.Value.Name) &&
                                        !String.IsNullOrEmpty(x.Value.Value.ToString())) &&
                                        patternSpst.IsMatch(x.Value.Value))
                           .Select(x => ("spst-" + stationID.ToLower() + "-" + x.Value.Value.Split('-').Last())
                                        .Trim())
                           .ToArray();
                DPs = DPs.Union(DPsForeign).Union(SPSTs).ToArray();

                List<Block> tmpDps = BlkDPs
                                            .Where(x => DPs.ToList()
                                            .Contains(blckProp.GetElemDesignation(x)))
                                            .OrderBy(x => x.X)
                                            .Distinct()
                                            .ToList();

                TrackSectionsTrackSection TrackSection = new TrackSectionsTrackSection
                {
                    Designation = blckProp.GetElemDesignation(BlkTrackSection),
                    Status = Status,
                    StationName = stationName
                };

                if (checkData["checkBoxFP"])
                {
                    List<ReadExcel.TrckLackOfClearence> trckLackOfs = excel.LackOfClearance(loadFiles["lblxlsFP"]);
                    ReadExcel.TrckLackOfClearence trckLackOf = trckLackOfs
                        .Where(x => x.TrackSection == blckProp.GetElemDesignation(BlkTrackSection))
                        .FirstOrDefault();
                    if (trckLackOf != null)
                    {
                        TrackSection.TrackCausingLackOfClearance = trckLackOf.Value;
                    }
                }

                if (tmpDps.Count == 2)
                {
                    TrackSection.Limitation1 = blckProp.GetElemDesignation(tmpDps[0]);
                    TrackSection.Limitation2 = blckProp.GetElemDesignation(tmpDps[1]);
                }
                else
                {
                    ErrLogger.Error("Unable to find limitations", blckProp.GetElemDesignation(BlkTrackSection), "");
                    error = true;
                }

                if (checkData["checkBoxEmSt"])
                {
                    var document =
                        new TFileDescr();
                    List<ReadExcel.EmergStopGroup> emergStopGroups =
                        excel.EmergStops(loadFiles["lblxlsEmSg"], ref document);
                    ReadExcel.EmergStopGroup emergStop = emergStopGroups
                                        .FirstOrDefault(x => x.Designation == TrackSection.Designation &&
                                        x.EmergSg != null);
                    if (emergStop != null)
                    {
                        TrackSection.EmergencyStopGroup = emergStop.EmergSg;
                    }
                    else
                    {
                        ErrLogger.Information("Emergency Stop Group not found", TrackSection.Designation);
                    }
                }
                else
                {
                    EmSG egs = emGs
                                            .Where(x => BlkTrackSection.X >= x.MinX &&
                                                        BlkTrackSection.X <= x.MaxX &&
                                                        BlkTrackSection.Y >= x.MinY &&
                                                        BlkTrackSection.Y <= x.MaxY && x.Order == 1)
                                             .FirstOrDefault();
                    if (egs != null)
                    {
                        TrackSection.EmergencyStopGroup = "emgs-" + stationID.ToLower() + "-" + egs.Designation.ToLower();
                    }
                    else
                    {
                        egs = emGs
                                            .Where(x => BlkTrackSection.X >= x.MinX &&
                                                        BlkTrackSection.X <= x.MaxX &&
                                                        BlkTrackSection.Y >= x.MinY &&
                                                        BlkTrackSection.Y <= x.MaxY)
                                             .FirstOrDefault();
                        if (egs != null)
                        {
                            TrackSection.EmergencyStopGroup = "emgs-" + stationID.ToLower() + "-" + egs.Designation.ToLower();
                        }
                    }
                }

                TrackSections.Add(TrackSection);
            }
            return !error;
        }

        private bool ReadEOTs(List<Block> blocks,
                                 ref List<EndOfTracksEndOfTrack> endOfTracks)
        {
            bool error = false;
            List<Block> BlkEndOfTracks = blocks
                                             .Where(x => x.XsdName == "EndOfTrack" &&
                                                          x.IsOnCurrentArea &&
                                                          x.IsOnNextStation == false)
                                             .OrderBy(x => blckProp.GetElemDesignation(x))
                                             .ToList();

            foreach (Block BlkEndOfTrack in BlkEndOfTracks)
            {
                EndOfTracksEndOfTrack EndOfTrack = new EndOfTracksEndOfTrack
                {
                    Designation = blckProp.GetElemDesignation(BlkEndOfTrack),
                    Status = Status,
                    LineID = BlkEndOfTrack.LineID,
                    //Direction = (DirectionType)

                    KindOfEOT = GetKindOfEOT(BlkEndOfTrack),
                    Location = BlkEndOfTrack.Attributes["KMP"].Value,
                };
                EndOfTrack.Direction = GetEotDirection(BlkEndOfTrack, ref error);
                endOfTracks.Add(EndOfTrack);
            }
            return !error;
        }

        private bool ReadBGs(
          List<Block> blocks,
          ref List<BaliseGroupsBaliseGroup> baliseGroups,
          List<PSA> pSAs)
        {
            bool flag = false;
            List<BaliseGroupsBaliseGroup> source = new List<BaliseGroupsBaliseGroup>();
            //BaliseGroupsBaliseGroup groupsBaliseGroup1 = new BaliseGroupsBaliseGroup();
            if (!this.checkData["checkBoxBG"])
                ErrLogger.Information("Balise Group data skipped", "Balise Group table");
            List<Block> list1 = blocks.Where((Func<Block, bool>)(x => x.XsdName == "BaliseGroup" && !x.IsOnNextStation && x.Visible)).OrderBy((Func<Block, string>)(x => x.Attributes["KMP"].Value)).ToList();
            foreach (Block Block in blocks.Where((Func<Block, bool>)(x => x.XsdName == "BaliseGroup" && x.TrackSegId == null)).ToList())
            {
                ErrLogger.Error("BG without TrackSegment", this.blckProp.GetElemDesignation(Block, false, true), "");
                flag = true;
            }
            TFileDescr document1 = new TFileDescr();
            if (this.checkData["checkBoxBG"])
            {
                source = this.excel.BaliseGroups(this.loadFiles["lblxlsBgs"], ref document1);
                this.Documents.Add(document1);
            }
            if (this.checkData["checkBoxBGN"])
            {
                TFileDescr document2 = new TFileDescr();
                this.excel.BaliseGroups(this.loadFiles["lblxlsBgsN"], ref document2);
                this.Documents.Add(document2);
            }
            foreach (Block block in list1)
            {
                Block BlkBalisGrp = block;
                List<TrackSegmentTmp> list2 = this.TrackSegmentsTmp.Where(s => s.BlocksOnSegments.Any(x => x.Designation.Equals(this.blckProp.GetElemDesignation(BlkBalisGrp, false, true)))).ToList();
                NominalReverseBothType result1;
                if (!Enum.TryParse(BlkBalisGrp.Attributes["DIRECTION"].Value.ToString().ToLower(), out result1))
                {
                    ErrLogger.Error("Unable to parse DIRECTION attribute value", this.blckProp.GetElemDesignation(BlkBalisGrp, false, true),
                        BlkBalisGrp.Attributes["DIRECTION"].Value.ToString());
                    flag = true;
                }
                string str1 = (string)null;
                BlkBalisGrp.Attributes.Where((Func<KeyValuePair<string, Attribute>, bool>)(x => x.Value.Value.ToString() == "yes")).Select((Func<KeyValuePair<string, Attribute>, Attribute>)(x => x.Value)).ToList();
                string lineId = BlkBalisGrp.LineID;
                BaliseGroupsBaliseGroup groupsBaliseGroup2 = source.Where((Func<BaliseGroupsBaliseGroup, bool>)(x => x.Designation == this.blckProp.GetElemDesignation(BlkBalisGrp, false, true))).FirstOrDefault();
                BaliseGroupsBaliseGroupBaliseGroupTypesKindOfBG[] groupTypesKindOfBgArray = new BaliseGroupsBaliseGroupBaliseGroupTypesKindOfBG[1]
                {
          new BaliseGroupsBaliseGroupBaliseGroupTypesKindOfBG()
          {
            direction = result1,
            Value = KindOfBG.Positioningbalisegroup
          }
                };
                UpDownSingleType result2;
                if (groupsBaliseGroup2 != null)
                {
                    //BaliseGroupsBaliseGroupBaliseGroupTypesKindOfBG[] kindOfBg = groupsBaliseGroup2.BaliseGroupTypes.KindOfBG;
                    result2 = groupsBaliseGroup2.Orientation;
                    lineId = groupsBaliseGroup2.LineID;
                }
                else
                {
                    groupsBaliseGroup2 = new BaliseGroupsBaliseGroup()
                    {
                        BaliseGroupTypes = new BaliseGroupsBaliseGroupBaliseGroupTypes()
                        {
                            KindOfBG = groupTypesKindOfBgArray
                        }
                    };
                    if (!Enum.TryParse(BlkBalisGrp.Attributes["ORIENT"].Value.ToString().ToLower(), out result2))
                    {
                        ErrLogger.Error("Unable to parse ORIENT attribute value", this.blckProp.GetElemDesignation(BlkBalisGrp, false, true),
                            BlkBalisGrp.Attributes["ORIENT"].Value.ToString());
                        flag = true;
                    }
                }
                BaliseGroupsBaliseGroup groupsBaliseGroup3 = new BaliseGroupsBaliseGroup()
                {
                    Designation = this.blckProp.GetElemDesignation(BlkBalisGrp, true, true),
                    Status = this.Status,
                    BaliseGroupTypes = groupsBaliseGroup2.BaliseGroupTypes,
                    LineID = lineId,
                    Location = BlkBalisGrp.Attributes["KMP"].Value,
                    Orientation = result2,
                    Remarks = str1
                };
                if (list2.Count == 1)
                    groupsBaliseGroup3.TrackSegmentID = list2[0].Designation;
                else if (groupsBaliseGroup2.TrackSegmentID != null && list2.Count == 0)
                {
                    string[] strArray = groupsBaliseGroup2.TrackSegmentID.Split('-');
                    strArray[strArray.Length - 1] = strArray[strArray.Length - 1].ToUpper();
                    strArray[strArray.Length - 2] = strArray[strArray.Length - 2].ToUpper();
                    groupsBaliseGroup3.TrackSegmentID = string.Join("-", strArray);
                    ErrLogger.Error("TrackSegmentId copied from table", BlkBalisGrp.Designation, "");
                    flag = true;
                }
                else if (list2.Count == 0)
                {
                    groupsBaliseGroup3.TrackSegmentID = "";
                    ErrLogger.Error("TrackSegmentId not found", this.blckProp.GetElemDesignation(BlkBalisGrp, false, true), "");
                    flag = true;
                }
                else
                {
                    string str2 = "";
                    foreach (TrackSegmentTmp trackSegmentTmp in list2)
                        str2 = str2 + trackSegmentTmp.Designation + "; ";
                    groupsBaliseGroup3.TrackSegmentID = "";
                    ErrLogger.Error("To many Segments found", this.blckProp.GetElemDesignation(BlkBalisGrp, false, true), str2);
                    flag = true;
                }
                PSA psa = pSAs
                          .Where(x => BlkBalisGrp.X >= x.MinX - 1.0 && 
                                      BlkBalisGrp.X <= x.MaxX + 1.0 && 
                                      BlkBalisGrp.Y >= x.MinY - 1.0 && 
                                      BlkBalisGrp.Y <= x.MaxY + 1.0)
                          .FirstOrDefault();
                if (psa != null)
                    groupsBaliseGroup3.InsidePSA = psa.Name.ToLower();
                baliseGroups.Add(groupsBaliseGroup3);
            }
            return !flag;
        }

        private bool ReadPWs(List<Block> blocks,
           ref List<StaffPassengerCrossingsStaffPassengerCrossing> pWss, List<AxleCounterSectionsAxleCounterSection> acsections)
        {
            bool error = false;
            List<Block> BlkSxs = blocks
                                     .Where(x => (x.XsdName == "StaffPassengerCrossing" &&
                                                  x.IsOnCurrentArea == true &&
                                                  x.IsOnNextStation == false &&
                                                  x.Visible == true))
                                     .OrderBy(x => x.Attributes["KMP"].Value)
                                     .ToList();
            foreach (Block BlkPws in BlkSxs)
            {

                ReadExcel.XlsPwsActivation xlsPwsActivation = new ReadExcel.XlsPwsActivation();
                var firstTextFile = new DirectoryInfo(dwgDir)
                   .EnumerateFiles("PWS" + BlkPws.Attributes["NAME"].Value + "_*.xls*")
                   .FirstOrDefault();
                if (firstTextFile != null)
                {
                    TFileDescr documentPwsAct =
                    new TFileDescr();
                    xlsPwsActivation =
                    excel.PwsActivation(firstTextFile.FullName, ref documentPwsAct, BlkPws, "PWS");
                    Documents.Add(documentPwsAct);
                }
                else
                {
                    ErrLogger.Error("Activations table not found", blckProp.GetElemDesignation(BlkPws, true, false), "");
                    error = true;
                }
                StaffPassengerCrossingsStaffPassengerCrossing staffPassengerCrossing =
                    new StaffPassengerCrossingsStaffPassengerCrossing
                    {
                        Designation = blckProp.GetElemDesignation(BlkPws, false, false),
                        Status = Status,
                        KindOfSPC = KindOfSPCType.passenger
                    };
                List<StaffPassengerCrossingsStaffPassengerCrossingStaffPassengerCrossingTracksStaffPassengerCrossingTrack> levelCrossingTracks =
                    new List<StaffPassengerCrossingsStaffPassengerCrossingStaffPassengerCrossingTracksStaffPassengerCrossingTrack>();
                List<TrackSegmentTmp> tmpSegments = TrackSegmentsTmp
                    .Where(s => s.BlocksOnSegments
                    .Any(x => x.Designation.Equals(blckProp.GetElemDesignation(BlkPws))))
                    .OrderByDescending(s => s.mainTrack)
                    .ToList();
                if (tmpSegments.Count > 0)
                {
                    char a1 = 'a';
                    int num1 = 1;
                    int trckCounter = 0;
                    foreach (TrackSegmentTmp tmpSegment in tmpSegments)
                    {
                        trckCounter++;
                        //List<decimal> speedIfUp = new List<decimal>();
                        //List<decimal> speedIfDown = new List<decimal>();
                        List<StaffPassengerCrossingsStaffPassengerCrossingStaffPassengerCrossingTracksStaffPassengerCrossingTrackActivationSectionsActivationSection> activationSections =
                            new List<StaffPassengerCrossingsStaffPassengerCrossingStaffPassengerCrossingTracksStaffPassengerCrossingTrackActivationSectionsActivationSection>();
                        if (firstTextFile != null)
                        {
                            List<string> DpsOnSeg = blocks
                                                .Where(x => x.XsdName == "DetectionPoint" &&
                                                            x.TrackSegId == tmpSegment.Designation)
                                                .Select(x => blckProp.GetElemDesignation(x))
                                                .ToList();
                            if (DpsOnSeg == null)
                            {
                                ErrLogger.Error("Detection points not found on Track Segment",
                                    blckProp.GetElemDesignation(BlkPws, true, false), tmpSegment.Designation);
                                error = true;
                                continue;
                            }
                            List<string> AcSectionOnSeg = acsections
                                                 .Where(x => DpsOnSeg
                                                                .Intersect(x.DetectionPoints.DetectionPoint
                                                                            .Select(y => y.Value).ToList()).Any())
                                                 .Select(x => x.Designation)
                                                 .ToList();
                            if (AcSectionOnSeg == null)
                            {
                                ErrLogger.Error("Ac sections not found on Track Segment",
                                    blckProp.GetElemDesignation(BlkPws, true, false),
                                        "");
                                error = true;
                                continue;
                            }
                            var ActivOnSeg = xlsPwsActivation.ActivationSections
                                            .Where(x => AcSectionOnSeg.Contains(x.LxAxleCounterSectionID))
                                            .ToList();
                            if (ActivOnSeg.Count == 0)
                            {
                                error = true;
                                ErrLogger.Error("LxAxleCounterSectionID not found on segment",
                                    staffPassengerCrossing.Designation, tmpSegment.Designation);
                            }
                            foreach (var act in ActivOnSeg)
                            {
                                foreach (var pwsAct in act.ActivationDelays.ActivationDelay)
                                {
                                    if (pwsAct.ActivationDelayTime == 1)
                                    {
                                        pwsAct.ActivationDelayTime = 2;
                                        ErrLogger.Information("Activation Delay time changed from 1 to 2", blckProp.GetElemDesignation(BlkPws, true, false));
                                    }
                                }
                                activationSections.Add(new StaffPassengerCrossingsStaffPassengerCrossingStaffPassengerCrossingTracksStaffPassengerCrossingTrackActivationSectionsActivationSection
                                {
                                    ActivationAxleCounterSectionID = act.ActivationAxleCounterSectionID,
                                    ActivationDelays = act.ActivationDelays,
                                    AxleCounterSections = act.AxleCounterSections,
                                    DeactivationAxleCounterSectionID = act.DeactivationAxleCounterSectionID,
                                    LxAxleCounterSectionID = act.LxAxleCounterSectionID,
                                    MaxLineSpeedAtActivationSection = act.MaxLineSpeedAtActivationSection,
                                    MaxLineSpeedAtActivationSectionSpecified = act.MaxLineSpeedAtActivationSectionSpecified,
                                    RouteChain = act.RouteChain
                                });
                            }
                        }
                        string TrackDesign = "";
                        if (tmpSegments.Count == 1)
                        {
                            TrackDesign = blckProp.GetElemDesignation(BlkPws, false, false)
                                          .Replace("va-", "sp-");
                        }
                        else if (char.IsDigit(BlkPws.Attributes["NAME"].Value[BlkPws.Attributes["NAME"].Value.Length - 1]))
                        {
                            TrackDesign = blckProp.GetElemDesignation(BlkPws, false, false)
                                          .Replace("va-", "sp-") + a1++;
                        }
                        else
                        {
                            TrackDesign = blckProp.GetElemDesignation(BlkPws, false, false)
                                          .Replace("va-", "sp-") + num1++;
                        }
                        TrackLine LxTrLine =
                        TracksLines
                        .Where(x => tmpSegment.TrackLines.Contains(x.line))
                        .FirstOrDefault();
                        bool ifUpSpec = decimal.TryParse(xlsPwsActivation.SpeedIfUnprotectedUp, out decimal ifUp);
                        bool ifDownSpec = decimal.TryParse(xlsPwsActivation.SpeedIfUnprotectedDown, out decimal ifDown);
                        bool tsrStartUpSpec = decimal.TryParse(xlsPwsActivation.TSRStartInRearOfAreaUp, out decimal tsrStartUp);
                        bool tsrStartDownSpec = decimal.TryParse(xlsPwsActivation.TSRStartInRearOfAreaDown, out decimal tsrStartDown);
                        bool tsrExtUpSpec = decimal.TryParse(xlsPwsActivation.TSRExtensionBeyondAreaUp, out decimal tsrExtUp);
                        bool tsrExtDownSpec = decimal.TryParse(xlsPwsActivation.TSRExtensionBeyondAreaDown, out decimal tsrExtDown);

                        StaffPassengerCrossingsStaffPassengerCrossingStaffPassengerCrossingTracksStaffPassengerCrossingTrack pwsTrack =
                        new StaffPassengerCrossingsStaffPassengerCrossingStaffPassengerCrossingTracksStaffPassengerCrossingTrack
                        {
                            Designation = TrackDesign,
                            TrackSegmentID = tmpSegment.Designation,
                            LineID = LxTrLine.LineID,

                            LengthSPCASpecified = true,
                            ActivationSections = new StaffPassengerCrossingsStaffPassengerCrossingStaffPassengerCrossingTracksStaffPassengerCrossingTrackActivationSections
                            {
                                ActivationSection = activationSections.ToArray()
                            },

                            SpeedIfUnprotectedUp = ifUp,
                            SpeedIfUnprotectedUpSpecified = ifUpSpec,
                            SpeedIfUnprotectedDown = ifDown,
                            SpeedIfUnprotectedDownSpecified = ifDownSpec,
                            // not requested by BDK yet
                            //TSRStartInRearOfAreaUp = tsrStartUp,
                            //TSRStartInRearOfAreaUpSpecified = tsrStartUpSpec,
                            //TSRStartInRearOfAreaDown = tsrStartDown,
                            //TSRStartInRearOfAreaDownSpecified = tsrStartDownSpec,
                            //TSRExtensionBeyondAreaUp = tsrExtUp,
                            //TSRExtensionBeyondAreaUpSpecified = tsrExtUpSpec,
                            //TSRExtensionBeyondAreaDown = tsrExtDown,
                            //TSRExtensionBeyondAreaDownSpecified = tsrExtDownSpec
                        };
                        decimal endLca = 0;
                        if (tmpSegments.Count > 1)
                        {
                            if (!decimal.TryParse(BlkPws.Attributes["START_KMP_" + trckCounter.ToString()].Value, out decimal beginLca))
                            {
                                error = true;
                                ErrLogger.Error("Unable to parse attribute value", staffPassengerCrossing.Designation,
                                    ":  'START_KMP_" + trckCounter.ToString());
                            }
                            else if (!decimal.TryParse(BlkPws.Attributes["END_KMP_" + trckCounter.ToString()].Value, out endLca))
                            {
                                error = true;
                                ErrLogger.Error("Unable to parse attribute value", staffPassengerCrossing.Designation,
                                    "END_KMP_" + trckCounter.ToString());
                            }
                            else
                            {
                                if (beginLca > endLca)
                                {
                                    beginLca = endLca;
                                    ErrLogger.Error("BeginLca less then EndLca. BeginLca and EndLca have been swapped",
                                        blckProp.GetElemDesignation(BlkPws, true, false), "");
                                    error = true;
                                }
                            }
                            pwsTrack.BeginSPCA = string.Format("{0:0.000}", beginLca);

                            if (!decimal.TryParse(BlkPws.Attributes["LENGTH_" + trckCounter.ToString()].Value.Replace(',', '.'), out decimal length))
                            {
                                error = true;
                                ErrLogger.Error("Unable to parse attribute value", staffPassengerCrossing.Designation,
                                    "LENGTH_" + trckCounter.ToString());
                            }
                            length = Convert.ToDecimal(Calc.RoundUp(Convert.ToDouble(length), 0));
                            if (beginLca + length / 1000 != endLca)
                            {
                                ErrLogger.Error("beginLca+lengthLca not equal to endLca", blckProp.GetElemDesignation(BlkPws, true, false),
                                    pwsTrack.Designation);
                                error = true;
                            }
                            pwsTrack.LengthSPCA = length;
                        }
                        else
                        {
                            if (!decimal.TryParse(BlkPws.Attributes["START_KMP"].Value, out decimal beginLca))
                            {
                                error = true;
                                ErrLogger.Error("Unable to parse attribute value", staffPassengerCrossing.Designation, "START_KMP");
                            }
                            else if (!decimal.TryParse(BlkPws.Attributes["END_KMP"].Value, out endLca))
                            {
                                error = true;
                                ErrLogger.Error("Unable to parse attribute value", staffPassengerCrossing.Designation, "END_KMP");
                            }
                            else
                            {
                                if (beginLca > endLca)
                                {
                                    beginLca = endLca;
                                    ErrLogger.Error("BeginLca less then EndLca. BeginLca and EndLca have been swapped",
                                       blckProp.GetElemDesignation(BlkPws, true, false), "");
                                    error = true;
                                }
                            }
                            pwsTrack.BeginSPCA = string.Format("{0:0.000}", beginLca);

                            if (!decimal.TryParse(BlkPws.Attributes["LENGTH_1"].Value.Replace(',', '.'), out decimal length))
                            {
                                error = true;
                                ErrLogger.Error("Unable to parse attribute value", staffPassengerCrossing.Designation, "LENGTH_1");
                            }
                            length = Convert.ToDecimal(Calc.RoundUp(Convert.ToDouble(length), 0));
                            if (beginLca + length / 1000 != endLca)
                            {
                                ErrLogger.Error("beginLca+lengthLca not equal to endLca", blckProp.GetElemDesignation(BlkPws, true, false), pwsTrack.Designation);
                                error = true;
                            }
                            pwsTrack.LengthSPCA = length;
                        }
                        if (!decimal.TryParse(BlkPws.Attributes["KMP"].Value, out decimal location))
                        {
                            error = true;
                            ErrLogger.Error("Unable to parse attribute value", staffPassengerCrossing.Designation, "KMP");
                        }
                        pwsTrack.Location = string.Format("{0:0.000}", location);

                        levelCrossingTracks.Add(pwsTrack);
                    }
                    staffPassengerCrossing.StaffPassengerCrossingTracks =
                        new StaffPassengerCrossingsStaffPassengerCrossingStaffPassengerCrossingTracks
                        {
                            StaffPassengerCrossingTrack = levelCrossingTracks.ToArray()
                        };
                }
                else
                {

                    ErrLogger.Error("Level Crossing Tracks not found", blckProp.GetElemDesignation(BlkPws, true, false), "");
                    error = true;


                }
                pWss.Add(staffPassengerCrossing);
            }
            BlkSxs = blocks
                                    .Where(x => (x.XsdName == "StaffCrossing" &&
                                                 x.IsOnCurrentArea == true &&
                                                 x.Visible == true))
                                    .OrderBy(x => x.Attributes["KMP"].Value)
                                    .ToList();
            foreach (Block BlkSx in BlkSxs)
            {

                ReadExcel.XlsPwsActivation xlsPwsActivation = new ReadExcel.XlsPwsActivation();
                var firstTextFile = new DirectoryInfo(dwgDir)
                   .EnumerateFiles("SX" + BlkSx.Attributes["NAME"].Value + "_*.xls*")
                   .FirstOrDefault();
                if (firstTextFile != null)
                {
                    TFileDescr documentPwsAct =
                    new TFileDescr();
                    xlsPwsActivation =
                    excel.PwsActivation(firstTextFile.FullName, ref documentPwsAct, BlkSx, "SX");
                    Documents.Add(documentPwsAct);
                }
                else
                {
                    ErrLogger.Error("Activations table not found", blckProp.GetElemDesignation(BlkSx, true, false), "");
                    error = true;
                }
                StaffPassengerCrossingsStaffPassengerCrossing staffPassengerCrossing =
                    new StaffPassengerCrossingsStaffPassengerCrossing
                    {
                        Designation = blckProp.GetElemDesignation(BlkSx, true, false),
                        Status = Status,
                        KindOfSPC = KindOfSPCType.staff
                    };
                List<StaffPassengerCrossingsStaffPassengerCrossingStaffPassengerCrossingTracksStaffPassengerCrossingTrack> levelCrossingTracks =
                    new List<StaffPassengerCrossingsStaffPassengerCrossingStaffPassengerCrossingTracksStaffPassengerCrossingTrack>();
                List<TrackSegmentTmp> tmpSegments = TrackSegmentsTmp
                    .Where(s => s.BlocksOnSegments
                    .Any(x => x.Designation.Equals(blckProp.GetElemDesignation(BlkSx))))
                    .OrderByDescending(s => s.mainTrack)
                    .ThenBy(s => s.minY)
                    .ToList();
                if (tmpSegments.Count > 0)
                {
                    char a1 = 'a';
                    int num1 = 1;
                    int trckCounter = 0;
                    foreach (TrackSegmentTmp tmpSegment in tmpSegments)
                    {
                        //List<decimal> speedIfUp = new List<decimal>();
                        //List<decimal> speedIfDown = new List<decimal>();
                        trckCounter++;
                        List<StaffPassengerCrossingsStaffPassengerCrossingStaffPassengerCrossingTracksStaffPassengerCrossingTrackActivationSectionsActivationSection> activationSections =
                            new List<StaffPassengerCrossingsStaffPassengerCrossingStaffPassengerCrossingTracksStaffPassengerCrossingTrackActivationSectionsActivationSection>();
                        if (firstTextFile != null)
                        {
                            List<string> DpsOnSeg = blocks
                                                .Where(x => x.XsdName == "DetectionPoint" &&
                                                            x.TrackSegId == tmpSegment.Designation)
                                                .Select(x => blckProp.GetElemDesignation(x))
                                                .ToList();
                            if (DpsOnSeg == null)
                            {
                                ErrLogger.Error("Detection points not found on Track Segment",
                                    blckProp.GetElemDesignation(BlkSx, true, false), tmpSegment.Designation);
                                error = true;
                                continue;
                            }
                            List<string> AcSectionOnSeg = acsections
                                                 .Where(x => DpsOnSeg
                                                                .Intersect(x.DetectionPoints.DetectionPoint
                                                                            .Select(y => y.Value).ToList()).Any())
                                                 .Select(x => x.Designation)
                                                 .ToList();
                            if (AcSectionOnSeg == null)
                            {
                                ErrLogger.Error("Ac sections not found on Track Segment",
                                    blckProp.GetElemDesignation(BlkSx, true, false), "");
                                error = true;
                                continue;
                            }
                            var ActivOnSeg = xlsPwsActivation.ActivationSections
                                            .Where(x => AcSectionOnSeg.Contains(x.LxAxleCounterSectionID))
                                            .ToList();
                            if (ActivOnSeg.Count == 0)
                            {
                                error = true;
                                ErrLogger.Error("LxAxleCounterSectionID not found on segment", 
                                    staffPassengerCrossing.Designation, tmpSegment.Designation);
                            }
                            foreach (var act in ActivOnSeg)
                            {
                                foreach (var pwsAct in act.ActivationDelays.ActivationDelay)
                                {
                                    if (pwsAct.ActivationDelayTime == 1)
                                    {
                                        pwsAct.ActivationDelayTime = 2;
                                        ErrLogger.Information("Activation Delay time changed from 1 to 2", blckProp.GetElemDesignation(BlkSx, true, false));
                                    }
                                }

                                activationSections.Add(new StaffPassengerCrossingsStaffPassengerCrossingStaffPassengerCrossingTracksStaffPassengerCrossingTrackActivationSectionsActivationSection
                                {
                                    ActivationAxleCounterSectionID = act.ActivationAxleCounterSectionID,
                                    ActivationDelays = act.ActivationDelays,
                                    AxleCounterSections = act.AxleCounterSections,
                                    DeactivationAxleCounterSectionID = act.DeactivationAxleCounterSectionID,
                                    LxAxleCounterSectionID = act.LxAxleCounterSectionID,
                                    MaxLineSpeedAtActivationSection = act.MaxLineSpeedAtActivationSection,
                                    MaxLineSpeedAtActivationSectionSpecified = act.MaxLineSpeedAtActivationSectionSpecified,
                                    RouteChain = act.RouteChain
                                });
                            }
                        }
                        bool ifUpSpec = decimal.TryParse(xlsPwsActivation.SpeedIfUnprotectedUp, out decimal ifUp);
                        bool ifDownSpec = decimal.TryParse(xlsPwsActivation.SpeedIfUnprotectedDown, out decimal ifDown);
                        bool tsrStartUpSpec = decimal.TryParse(xlsPwsActivation.TSRStartInRearOfAreaUp, out decimal tsrStartUp);
                        bool tsrStartDownSpec = decimal.TryParse(xlsPwsActivation.TSRStartInRearOfAreaDown, out decimal tsrStartDown);
                        bool tsrExtUpSpec = decimal.TryParse(xlsPwsActivation.TSRExtensionBeyondAreaUp, out decimal tsrExtUp);
                        bool tsrExtDownSpec = decimal.TryParse(xlsPwsActivation.TSRExtensionBeyondAreaDown, out decimal tsrExtDown);
                        string TrackDesign = "";
                        if (tmpSegments.Count == 1)
                        {
                            TrackDesign = blckProp.GetElemDesignation(BlkSx, true, false)
                                          .Replace("ovk-", "sp-");
                        }
                        else if (char.IsDigit(BlkSx.Attributes["NAME"].Value[BlkSx.Attributes["NAME"].Value.Length - 1]))
                        {
                            TrackDesign = blckProp.GetElemDesignation(BlkSx, true, false)
                                          .Replace("ovk-", "sp-") + a1++;
                        }
                        else
                        {
                            TrackDesign = blckProp.GetElemDesignation(BlkSx, true, false)
                                          .Replace("ovk-", "sp-") + num1++;
                        }
                        StaffPassengerCrossingsStaffPassengerCrossingStaffPassengerCrossingTracksStaffPassengerCrossingTrack staffTrack =
                            new StaffPassengerCrossingsStaffPassengerCrossingStaffPassengerCrossingTracksStaffPassengerCrossingTrack
                            {
                                Designation = TrackDesign,
                                TrackSegmentID = tmpSegment.Designation,
                                LineID = BlkSx.LineID,
                                //Location = BlkSx.Attributes["KMP"].Value,
                                //BeginSPCA = BlkSx.Attributes["START_KM_1"].Value,
                                //LengthSPCA = Convert.ToInt32(Math.Abs(Convert.ToDecimal(BlkSx.Attributes["START_KM_1"].Value) -
                                //Convert.ToDecimal(BlkSx.Attributes["END_KM_1"].Value)) * 1000),
                                LengthSPCASpecified = true,
                                ActivationSections = new StaffPassengerCrossingsStaffPassengerCrossingStaffPassengerCrossingTracksStaffPassengerCrossingTrackActivationSections
                                {
                                    ActivationSection = activationSections.ToArray()
                                },
                                SpeedIfUnprotectedUp = ifUp,
                                SpeedIfUnprotectedUpSpecified = ifUpSpec,
                                SpeedIfUnprotectedDown = ifDown,
                                SpeedIfUnprotectedDownSpecified = ifDownSpec,
                                // not requested by BDK yet
                                //TSRStartInRearOfAreaUp = tsrStartUp,
                                //TSRStartInRearOfAreaUpSpecified = tsrStartUpSpec,
                                //TSRStartInRearOfAreaDown = tsrStartDown,
                                //TSRStartInRearOfAreaDownSpecified = tsrStartDownSpec,
                                //TSRExtensionBeyondAreaUp = tsrExtUp,
                                //TSRExtensionBeyondAreaUpSpecified = tsrExtUpSpec,
                                //TSRExtensionBeyondAreaDown = tsrExtDown,
                                //TSRExtensionBeyondAreaDownSpecified = tsrExtDownSpec
                            };
                        decimal length = 0;
                        decimal endLca = 0;
                        if (tmpSegments.Count > 1)
                        {
                            if (!decimal.TryParse(BlkSx.Attributes["START_KM_" + trckCounter.ToString()].Value, out decimal beginLca))
                            {
                                error = true;
                                ErrLogger.Error("Unable to parse attribute value", staffPassengerCrossing.Designation,
                                    "START_KM_" + trckCounter.ToString());
                            }
                            else if (!decimal.TryParse(BlkSx.Attributes["END_KM_" + trckCounter.ToString()].Value, out endLca))
                            {
                                error = true;
                                ErrLogger.Error("Unable to parse attribute value", staffPassengerCrossing.Designation,
                                    "END_KM_" + trckCounter.ToString());
                            }
                            else
                            {
                                if (beginLca > endLca)
                                {
                                    beginLca = endLca;
                                    ErrLogger.Error("BeginLca less than EndLca. BeginLca and EndLca have been swapped",
                                        blckProp.GetElemDesignation(BlkSx, true, false), "");
                                    error = true;
                                }
                            }
                            staffTrack.BeginSPCA = string.Format("{0:0.000}", beginLca);

                            if (!decimal.TryParse(BlkSx.Attributes["LENGTH_" + trckCounter.ToString()].Value.Replace(',', '.'), out length))
                            {
                                error = true;
                                ErrLogger.Error("Unable to parse attribute value", staffPassengerCrossing.Designation,
                                    "LENGTH_" + trckCounter.ToString());
                            }
                            if (beginLca + length / 1000 != endLca)
                            {
                                ErrLogger.Error("beginLca+lengthLca not equal to endLca",
                                    blckProp.GetElemDesignation(BlkSx, true, false),staffTrack.Designation);
                                error = true;
                            }
                            staffTrack.LengthSPCA = length;
                        }
                        else
                        {
                            if (!decimal.TryParse(BlkSx.Attributes["START_KM"].Value, out decimal beginLca))
                            {
                                error = true;
                                ErrLogger.Error("Unable to parse attribute value", staffPassengerCrossing.Designation,
                                    "START_KMP");
                            }
                            else if (!decimal.TryParse(BlkSx.Attributes["END_KM"].Value, out endLca))
                            {
                                error = true;
                                ErrLogger.Error("Unable to parse attribute value", staffPassengerCrossing.Designation,
                                    "END_KMP");
                            }
                            else
                            {
                                if (beginLca > endLca)
                                {
                                    beginLca = endLca;
                                    ErrLogger.Error("BeginLca less than EndLca.  BeginLca and EndLca have been swapped",
                                        blckProp.GetElemDesignation(BlkSx, true, false), "");
                                    error = true;
                                }
                            }
                            staffTrack.BeginSPCA = string.Format("{0:0.000}", beginLca);

                            if (!decimal.TryParse(BlkSx.Attributes["LENGTH_1"].Value.Replace(',', '.'), out length))
                            {
                                error = true;
                                ErrLogger.Error("Unable to parse attribute value", staffPassengerCrossing.Designation,
                                    "LENGTH_1");
                            }
                            if (beginLca + length / 1000 != endLca)
                            {
                                ErrLogger.Error("beginLca+lengthLca not equal to endLca", blckProp.GetElemDesignation(BlkSx, true, false),  staffTrack.Designation);
                                error = true;
                            }
                            staffTrack.LengthSPCA = length;
                        }
                        if (!decimal.TryParse(BlkSx.Attributes["KMP"].Value, out decimal location))
                        {
                            error = true;
                            ErrLogger.Error("Unable to parse attribute value", staffPassengerCrossing.Designation,
                                "KMP");
                        }
                        staffTrack.Location = string.Format("{0:0.000}", location);
                        levelCrossingTracks.Add(staffTrack);
                    }
                    staffPassengerCrossing.StaffPassengerCrossingTracks =
                        new StaffPassengerCrossingsStaffPassengerCrossingStaffPassengerCrossingTracks
                        {
                            StaffPassengerCrossingTrack = levelCrossingTracks.ToArray()
                        };
                }
                else
                {

                    ErrLogger.Error("Tracks not found", blckProp.GetElemDesignation(BlkSx, true, false), "");
                    error = true;


                }
                pWss.Add(staffPassengerCrossing);
            }
            return !error;
        }

        private bool ReadLxs(List<Block> blocks,
            ref List<LevelCrossingsLevelCrossing> crossings, List<AxleCounterSectionsAxleCounterSection> acsections)
        {
            bool error = false;
            List<Block> BlkLxs = blocks
                                     .Where(x => x.XsdName == "LevelCrossing" &&
                                                 x.IsOnNextStation == false &&
                                                 x.Visible == true)
                                     .OrderBy(x => blckProp.GetElemDesignation(x, true))
                                     .ToList();
            if (BlkLxs.Count == 0)
            {
                return !error;
            }
            TFileDescr document = new TFileDescr();

            foreach (Block BlkLx in BlkLxs)
            {
                List<LXactSection> xlsLxActivations = null;
                if (LxsActivations.ContainsKey(BlkLx.Attributes["NAME"].Value))
                {
                    xlsLxActivations = LxsActivations[BlkLx.Attributes["NAME"].Value].ActivationSections;
                    if (xlsLxActivations.Count == 0)
                    {
                        ErrLogger.Error("Activations not found in table", blckProp.GetElemDesignation(BlkLx, true, false), "");
                        error = true;
                    }
                    Documents.Add(LxsActivations[BlkLx.Attributes["NAME"].Value].Document);
                }
                else
                {
                    ErrLogger.Error("Activations table not found", blckProp.GetElemDesignation(BlkLx, true, false), "");
                    error = true;
                }


                LevelCrossingsLevelCrossing crossing =
                    new LevelCrossingsLevelCrossing
                    {
                        Designation = blckProp.GetElemDesignation(BlkLx, true, false),
                        Status = Status,
                        KeepAcousticWarningOn = YesNoType.no,
                        PermittedSpeedInOverlapSpecified = true,
                        PermittedSpeedSpecified = true,
                        PermittedSpeed = 10,
                        PermittedSpeedInOverlap = 15,
                    };

                List<LevelCrossingsLevelCrossingLevelCrossingTracksLevelCrossingTrack> levelCrossingTracks =
                    new List<LevelCrossingsLevelCrossingLevelCrossingTracksLevelCrossingTrack>();
                List<TrackSegmentTmp> tmpSegments = TrackSegmentsTmp
                    .Where(s => s.BlocksOnSegments
                    .Any(x => x.Designation.Equals(blckProp.GetElemDesignation(BlkLx))))
                    .OrderByDescending(s => s.mainTrack)
                    .Distinct()
                    .ToList();
                if (tmpSegments.Count > 0)
                {
                    int trksegcount = 1;
                    char a1 = 'a';
                    int num1 = 1;
                    foreach (TrackSegmentTmp tmpSegment in tmpSegments)
                    {
                        List<LXactSection> activationSections =
                            new List<LXactSection>();
                        List<string> DpsOnSeg = blocks
                                                .Where(x => x.XsdName == "DetectionPoint" &&
                                                            x.TrackSegId == tmpSegment.Designation)
                                                .Select(x => blckProp.GetElemDesignation(x))
                                                .ToList();
                        if (DpsOnSeg == null)
                        {
                            ErrLogger.Error("Detection points not found on Track Segment",
                                blckProp.GetElemDesignation(BlkLx, true, false), tmpSegment.Designation);
                            error = true;
                            continue;
                        }
                        List<string> AcSectionOnSeg = acsections
                                             .Where(x => DpsOnSeg
                                                            .Intersect(x.DetectionPoints.DetectionPoint
                                                                        .Select(y => y.Value).ToList()).Any())
                                             .Select(x => x.Designation)
                                             .ToList();
                        if (AcSectionOnSeg == null)
                        {
                            ErrLogger.Error("Ac sections not found on Track Segment",
                                blckProp.GetElemDesignation(BlkLx, true, false), tmpSegment.Designation);
                            error = true;
                            continue;
                        }
                        if (xlsLxActivations != null)
                        {
                            var ActivOnSeg = xlsLxActivations
                                        .Where(x => AcSectionOnSeg.Contains(x.LxAxleCounterSectionID))
                                        .ToList();
                            foreach (var act in ActivOnSeg)
                            {
                                if (act.ActivationDelayTime == 1)
                                {
                                    act.ActivationDelayTime = 2;
                                    ErrLogger.Information("Activation Delay time changed from 1 to 2", blckProp.GetElemDesignation(BlkLx, true, false));
                                }
                                activationSections.Add(act);

                            }
                        }

                        string TrackDesign = "";
                        if (tmpSegments.Count == 1)
                        {
                            TrackDesign = blckProp.GetElemDesignation(BlkLx, true, false)
                                          .Replace("ovk-", "sp-");
                        }
                        else if (char.IsDigit(BlkLx.Attributes["NAME"].Value[BlkLx.Attributes["NAME"].Value.Length - 1]))
                        {
                            TrackDesign = blckProp.GetElemDesignation(BlkLx, true, false)
                                          .Replace("ovk-", "sp-") + a1++;
                        }
                        else
                        {
                            TrackDesign = blckProp.GetElemDesignation(BlkLx, true, false)
                                          .Replace("ovk-", "sp-") + num1++;
                        }
                        TrackLine LxTrLine =
                        TracksLines
                        .Where(x => tmpSegment.TrackLines.Contains(x.line) &&
                                    ObjectsIntersects(BlkLx.BlkRef, x.line, Intersect.OnBothOperands))
                        .FirstOrDefault();
                        LevelCrossingsLevelCrossingLevelCrossingTracksLevelCrossingTrack crossingTrack =
                            new LevelCrossingsLevelCrossingLevelCrossingTracksLevelCrossingTrack
                            {
                                Designation = TrackDesign,
                                LineID = LxTrLine.LineID,
                                TrackSegmentID = tmpSegment.Designation,
                                LengthLCASpecified = true,
                                ActivationSections = new LevelCrossingsLevelCrossingLevelCrossingTracksLevelCrossingTrackActivationSections
                                {
                                    ActivationSection = activationSections.ToArray()
                                }
                            };
                        //string[] lxtrcksType = BlkLx.BlockName.Split(new Char[] { '_', '-' });
                        decimal location;
                        decimal beginLca;
                        decimal endLca;
                        decimal lengthLca;
                        if (BlkLx.BlockName.Contains("DoubleTrack"))
                        {
                            if (!decimal.TryParse(BlkLx.Attributes["KMP_TSEG" + trksegcount.ToString()].Value, out location))
                            {
                                ErrLogger.Error("Unable to parse attribute value",
                                     blckProp.GetElemDesignation(BlkLx, true, false), "KMP_TSEG" + trksegcount.ToString());
                                error = true;
                            }
                            if (Convert.ToDouble(location) == 0)
                            {
                                ErrLogger.Error("Location is '0' ", blckProp.GetElemDesignation(BlkLx, true, false),
                                    crossingTrack.Designation);
                                error = true;
                            }
                            crossingTrack.Location =
                                string.Format("{0:0.000}", location);

                            if (!decimal.TryParse(BlkLx.Attributes["BEGIN_LCA_TSEG" + trksegcount.ToString()].Value, out beginLca))
                            {
                                ErrLogger.Error("Unable to parse attribute value",
                                    blckProp.GetElemDesignation(BlkLx, true, false), "BEGIN_LCA_TSEG" + trksegcount.ToString());
                                error = true;
                            }
                            if (!decimal.TryParse(BlkLx.Attributes["END_LCA_TSEG" + trksegcount.ToString()].Value, out endLca))
                            {
                                ErrLogger.Error("Unable to parse attribute value",
                                    blckProp.GetElemDesignation(BlkLx, true, false), "END_LCA_TSEG" + trksegcount.ToString());
                                error = true;
                            }
                            if (beginLca > endLca)
                            {
                                beginLca = endLca;
                                ErrLogger.Error("BeginLca less than EndLca. BeginLca and EndLca have been swapped",
                                    blckProp.GetElemDesignation(BlkLx, true, false),
                                    tmpSegment.Designation);
                                error = true;
                            }
                            if (Convert.ToDouble(beginLca) == 0)
                            {
                                ErrLogger.Error("BeginLca is '0' ", blckProp.GetElemDesignation(BlkLx, true, false),
                                    tmpSegment.Designation);
                                error = true;
                            }
                            crossingTrack.BeginLCA =
                                string.Format("{0:0.000}", beginLca);

                            Attribute LcaLenght = BlkLx.Attributes
                                .Select(x => x.Value)
                                .Where(y => y.Name.Replace("_", "") == "LENGHTLCATSEG" + trksegcount.ToString())
                                .FirstOrDefault();
                            if (LcaLenght != null)
                            {
                                if (!decimal.TryParse(LcaLenght.Value.Replace(',', '.'), out lengthLca))
                                {
                                    ErrLogger.Error("Unable to parse attribute value",
                                        blckProp.GetElemDesignation(BlkLx, true, false), "LENGHT_LCA_TSEG" + trksegcount.ToString());
                                    error = true;
                                }
                                if (Convert.ToDouble(lengthLca) == 0)
                                {
                                    ErrLogger.Error("LengthLca is '0'", blckProp.GetElemDesignation(BlkLx, true, false),"");
                                    error = true;
                                }
                                if (beginLca + lengthLca / 1000 != endLca)
                                {
                                    ErrLogger.Error("beginLca+lengthLca not equal to endLca",
                                        blckProp.GetElemDesignation(BlkLx, true, false), crossingTrack.Designation);
                                    error = true;
                                }
                                crossingTrack.LengthLCA = lengthLca;
                            }
                            else
                            {
                                ErrLogger.Error("Attribute not found in Block",
                                    blckProp.GetElemDesignation(BlkLx, true, false),"LENGHT_LCA_TSEG" + trksegcount.ToString());
                                error = true;
                            }
                        }
                        else
                        {
                            if (!decimal.TryParse(BlkLx.Attributes["KMP"].Value, out location))
                            {
                                ErrLogger.Error("Unable to parse attribute value", 
                                    blckProp.GetElemDesignation(BlkLx, true, false), "KMP");
                                error = true;
                            }
                            if (Convert.ToDouble(location) == 0)
                            {
                                ErrLogger.Error("Location is '0'",
                                    blckProp.GetElemDesignation(BlkLx, true, false), crossingTrack.Designation);
                                error = true;
                            }
                            crossingTrack.Location =
                            string.Format("{0:0.000}", location);

                            if (!decimal.TryParse(BlkLx.Attributes["BEGIN_LCA"].Value, out beginLca))
                            {
                                ErrLogger.Error("Unable to parse attribute value",
                                    blckProp.GetElemDesignation(BlkLx, true, false), "BEGIN_LCA");
                                error = true;
                            }
                            if (!decimal.TryParse(BlkLx.Attributes["END_LCA"].Value, out endLca))
                            {
                                ErrLogger.Error("Unable to parse attribute value",
                                    blckProp.GetElemDesignation(BlkLx, true, false), "END_LCA");
                                error = true;
                            }
                            if (beginLca > endLca)
                            {
                                beginLca = endLca;
                                ErrLogger.Error("BeginLca less than EndLca. BeginLca and EndLca have been swapped",
                                  blckProp.GetElemDesignation(BlkLx, true, false), crossingTrack.Designation);
                                error = true;
                            }
                            if (Convert.ToDouble(beginLca) == 0)
                            {
                                ErrLogger.Error("BeginLca is '0'", blckProp.GetElemDesignation(BlkLx, true, false),
                                    crossingTrack.Designation);
                                error = true;
                            }
                            crossingTrack.BeginLCA = string.Format("{0:0.000}", beginLca);
                            if (!decimal.TryParse(BlkLx.Attributes["LENGHT_LCA"].Value.Replace(',', '.'), out lengthLca))
                            {
                                ErrLogger.Error("Unable to parse attribute value", blckProp.GetElemDesignation(BlkLx, true, false),
                                    "LENGHT_LCA");
                                error = true;
                            }
                            if (Convert.ToDouble(lengthLca) == 0)
                            {
                                ErrLogger.Error("LengthLca is '0'", blckProp.GetElemDesignation(BlkLx, true, false), "");
                                error = true;
                            }
                            if (beginLca + lengthLca / 1000 != endLca)
                            {
                                ErrLogger.Error("beginLca+lengthLca not equal to endLca", 
                                    blckProp.GetElemDesignation(BlkLx, true, false), crossingTrack.Designation);
                                error = true;
                            }
                            crossingTrack.LengthLCA = lengthLca;
                        }
                        levelCrossingTracks.Add(crossingTrack);
                        trksegcount++;
                    }
                    crossing.LevelCrossingTracks =
                        new LevelCrossingsLevelCrossingLevelCrossingTracks
                        {
                            LevelCrossingTrack = levelCrossingTracks.ToArray()
                        };
                    if (LxsActivations.ContainsKey(BlkLx.Attributes["NAME"].Value))
                    {
                        string remark = string.Join(" ", LxsActivations[BlkLx.Attributes["NAME"].Value].Remarks.ToArray());
                        crossing.Remarks = string.IsNullOrEmpty(remark) ? null : remark;
                    }
                }
                else
                {

                    ErrLogger.Error("Level Crossing Tracks not found", blckProp.GetElemDesignation(BlkLx, true, false), "");
                    //Logger.Log("Level Crossing Tracks not found", blckProp.GetElemDesignation(BlkLx, true));
                    error = true;
                }


                if (!checkData["checkBoxLX"])
                {
                    crossings.Add(crossing);
                    continue;
                }
                bool TableError = false;
                List<ReadExcel.XlsLX> xlsLXes =
                excel.LevelCrossings(loadFiles["lblxlsLxs"], ref document,
                                        blckProp.GetElemDesignation(BlkLx, true), ref TableError);
                if (TableError)
                {
                    ErrLogger.Error("Unable to read LXs table", blckProp.GetElemDesignation(BlkLx, true, false), "");
                    error = true;
                    crossings.Add(crossing);
                    continue;
                }
                string barriers = xlsLXes.Where(x => x.Reference != null && x.Reference.Contains("Barriers".ToLower()))
                          .Select(x => x.Value).FirstOrDefault();
                if (barriers != null)
                {
                    crossing.Barriers =
                     (BarriersType)Enum.Parse(typeof(BarriersType), barriers.Split(' ').First(), true);
                }
                else
                {
                    ErrLogger.Error("Value 'Barriers' not found", "LX params", BlkLx.Designation);
                    error = true;
                }

                string MaxCloseTime = xlsLXes.Where(x => x.Reference != null && x.Reference.Contains("MaxRestrictTime".ToLower()))
                          .Select(x => x.Value).FirstOrDefault();
                if (MaxCloseTime != null)
                {
                    crossing.MaxCloseTime = MaxCloseTime;
                }
                else
                {
                    ErrLogger.Error("Value 'MaxCloseTime(MaxRestrictTime)' not found", "LX params", BlkLx.Designation);
                    error = true;
                }

                string ExpMaxClosTim = xlsLXes.Where(x => x.Reference != null && x.Reference.Contains("ExpMaxClosTim".ToLower()))
                         .Select(x => x.Value).FirstOrDefault();
                if (ExpMaxClosTim != null)
                {
                    crossing.ExpMaxClosTim = (YesNoNAType)Enum.Parse(typeof(YesNoNAType), ExpMaxClosTim, true);
                }
                else
                {
                    ErrLogger.Error("Value 'ExpMaxClosTim' not found", "LX params", BlkLx.Designation);
                    error = true;
                }

                string MinOpenTime = xlsLXes.Where(x => x.Reference != null && x.Reference.Contains("MinOpenTime".ToLower()))
                         .Select(x => x.Value).FirstOrDefault();
                if (MinOpenTime != null)
                {
                    crossing.MinOpenTime = Convert.ToDecimal(MinOpenTime);
                }
                else
                {
                    ErrLogger.Error("Value 'MinOpenTime' not found", "LX params", BlkLx.Designation);
                    error = true;
                }

                string InterfaceRTLS = xlsLXes.Where(x => x.Reference != null && x.Reference.Contains("InterfaceRTLS".ToLower()))
                         .Select(x => x.Value).FirstOrDefault();
                if (InterfaceRTLS != null)
                {
                    crossing.InterfaceRTLS = (YesNoType)Enum.Parse(typeof(YesNoType), InterfaceRTLS, true);
                    if (crossing.InterfaceRTLS == YesNoType.yes || crossing.InterfaceRTLS == YesNoType.Yes)
                    {
                        string DebouncingDelayRTLS =
                            xlsLXes.Where(x => x.Reference != null && x.Reference.Contains("delaytimeobstructionrtls"))
                                   .Select(x => x.Value).FirstOrDefault();
                        if (DebouncingDelayRTLS != null)
                        {
                            crossing.DebouncingDelayRTLS = DebouncingDelayRTLS;
                        }
                        else
                        {
                            ErrLogger.Error("Value 'DebouncingDelayRTLS' not found", "LX params", BlkLx.Designation);
                            error = true;
                        }
                    }
                    else
                    {
                        crossing.DebouncingDelayRTLS = "n/a";
                    }
                }
                else
                {
                    ErrLogger.Error("Value 'InterfaceRTLS' not found", "LX params", BlkLx.Designation);
                    error = true;
                }

                string DelayTimeRTLS = xlsLXes.Where(x => x.Reference != null && x.Reference.Contains("DelayTimeRTLS".ToLower()))
                         .Select(x => x.Value).FirstOrDefault();
                if (DelayTimeRTLS != null)
                {
                    crossing.DelayTimeRTLS = DelayTimeRTLS;
                }
                else
                {
                    ErrLogger.Error("Value 'DelayTimeRTLS' not found", "LX params", BlkLx.Designation);
                    error = true;
                }

                string CheckTimerRTLS = xlsLXes.Where(x => x.Reference != null && x.Reference.Contains("CheckTimerRTLS".ToLower()))
                         .Select(x => x.Value).FirstOrDefault();
                if (CheckTimerRTLS != null)
                {
                    crossing.CheckTimerRTLS = CheckTimerRTLS;
                }
                else
                {
                    ErrLogger.Error("Value 'CheckTimerRTLS' not found", "LX params", BlkLx.Designation);
                    error = true;
                }

                string DelayTimeActivationBarriers = xlsLXes
                         .Where(x => x.Reference != null && x.Reference.Contains("DelayTimeActivationBarriers".ToLower()))
                         .Select(x => x.Value).FirstOrDefault();
                if (DelayTimeActivationBarriers != null)
                {
                    crossing.DelayTimeActivationBarriers = DelayTimeActivationBarriers;
                }
                else
                {
                    crossing.DelayTimeActivationBarriers = "0";
                }

                if (xlsLXes.Count(x => x.Reference != null && x.Reference.Contains("DelayTimeExitBarriers".ToLower())) > 0)
                {
                    string DelayTimeExitBarriers = xlsLXes
                         .Where(x => x.Reference == "DelayTimeExitBarriers".ToLower())
                         .Select(x => x.Value).First();
                    crossing.DelayTimeExitBarriers = Calc.RemoveCharsFromString(DelayTimeExitBarriers) ?? "n/a";
                }
                else
                {
                    ErrLogger.Error("Value 'DelayTimeExitBarriers' not found", "LX params", BlkLx.Designation);
                    error = true;
                }

                string DelayTimeShortBarriers = xlsLXes
                         .Where(x => x.Reference != null && x.Reference.Contains("DelayTimeShortBarriers".ToLower()))
                         .Select(x => x.Value).FirstOrDefault();
                if (DelayTimeShortBarriers != null)
                {
                    crossing.DelayTimeShortBarriers = DelayTimeShortBarriers;
                }
                else
                {
                    ErrLogger.Error("Value 'DelayTimeShortBarriers' not found", "LX params", BlkLx.Designation);
                    error = true;
                }

                string SupervisionTimer = xlsLXes
                         .Where(x => x.Reference != null && x.Reference.Contains("SupervisionTimer".ToLower()))
                         .Select(x => x.Value).FirstOrDefault();
                if (SupervisionTimer != null)
                {
                    crossing.SupervisionTimer = Convert.ToDecimal(SupervisionTimer);
                }
                else
                {
                    ErrLogger.Error("Value 'SupervisionTimer' not found", "LX params", BlkLx.Designation);
                    error = true;
                }

                if (xlsLXes.Count(x => x.Reference != null && x.Reference.Contains("WaitBeforeForcedUp".ToLower())) > 0)
                {
                    string WaitBeforeForcedUp = xlsLXes
                         .Where(x => x.Reference == "WaitBeforeForcedUp".ToLower())
                         .Select(x => x.Value).First();
                    crossing.WaitBeforeForcedUp =
                        WaitBeforeForcedUp == null ? 0 : Convert.ToDecimal(WaitBeforeForcedUp);
                }
                else
                {
                    ErrLogger.Error("Value 'WaitBeforeForcedUp' not found", "LX params", BlkLx.Designation);
                    error = true;
                }

                if (xlsLXes.Count(x => x.Reference != null && x.Reference.Contains("DeactivateOnClerance".ToLower())) > 0)
                {
                    string DeactivateOnClearance = xlsLXes
                         .Where(x => x.Reference == "DeactivateOnClerance".ToLower())
                         .Select(x => x.Value).First();
                    crossing.DeactivateOnClearance =
                        DeactivateOnClearance == null ? YesNoType.no :
                        (YesNoType)Enum.Parse(typeof(YesNoType), DeactivateOnClearance, true);
                }
                else
                {
                    ErrLogger.Error("Value 'DeactivateOnClearance' not found.", "LX params", BlkLx.Designation);
                    error = true;
                }

                string BarrierClosingTime = xlsLXes
                         .Where(x => x.Reference != null && x.Reference.Contains("BarrierClosingTime".ToLower()))
                         .Select(x => x.Value).FirstOrDefault();
                if (BarrierClosingTime != null)
                {
                    crossing.BarrierClosingTime = Convert.ToDecimal(BarrierClosingTime);
                    crossing.BarrierClosingTimeSpecified = true;
                }
                else
                {
                    ErrLogger.Error("Value 'BarrierClosingTime' not found", "LX params", BlkLx.Designation);
                    error = true;
                }

                string MaxWaitTime = xlsLXes
                         .Where(x => x.Reference != null && x.Reference.Contains("MaxWaitTime".ToLower()))
                         .Select(x => x.Value).FirstOrDefault();
                if (MaxWaitTime != null)
                {
                    crossing.MaxWaitTimeSpecified = MaxWaitTime == "no" ? false : true;
                    if (MaxWaitTime != "no")
                    {
                        crossing.MaxWaitTime = Convert.ToDecimal(MaxWaitTime.Split(' ')[0]);
                    }
                }
                else
                {
                    ErrLogger.Error("Value 'MaxWaitTime' not found", "LX params", BlkLx.Designation);
                    error = true;
                }

                //if (xlsLXes.Count(x => x.Reference != null && x.Reference.Contains("PermittedSpeed".ToLower())) > 0)
                //{
                //    string PermittedSpeed = xlsLXes
                //         .Where(x => x.Reference != null && x.Reference.Contains("PermittedSpeed".ToLower()))
                //         .Select(x => x.Value).First();
                //    if (PermittedSpeed != null && PermittedSpeed != "" && PermittedSpeed != "0")
                //    {
                //        crossing.PermittedSpeedSpecified = true;
                //        crossing.PermittedSpeed = Convert.ToDecimal(PermittedSpeed);
                //    }
                //}
                //else
                //{
                //    ErrLogger.Warning("LX: '" + BlkLx.Designation + "'. Value 'PermittedSpeed' not found.");
                //    error = true;
                //}

                crossings.Add(crossing);
                //FindLxActivations(crossing, blocks, speedProfiles);
            }
            if (checkData["checkBoxLX"])
            {
                Documents.Add(document);
            }
            return !error;
        }

        private void ReadEms(List<Block> blocks,
             ref List<EmergencyStopGroupsEmergencyStopGroup> emergencystopgroups, List<EmSG> emSGs)
        {
            if (!checkData["checkBoxEmSt"])
            {
                ErrLogger.Information("Emergency Stop groups data skipped", "Emergency Stop");

                foreach (var emsg in emSGs.GroupBy(g => g.Designation))
                {
                    EmergencyStopGroupsEmergencyStopGroup emergencyStopGroup =
                    new EmergencyStopGroupsEmergencyStopGroup
                    {
                        Designation = "emgs-" + stationID.ToLower() + "-" + emsg.Key.ToLower(),
                        Status = Status
                    };
                    List<string> values = new List<string>();

                    foreach (var emgsOrder in emsg)
                    {
                        values.AddRange(blocks
                                 .Where(x => x.XsdName == "Signal" &&
                                            x.Visible == true &&
                                            x.X >= emgsOrder.MinX &&
                                            x.X <= emgsOrder.MaxX &&
                                            x.Y >= emgsOrder.MinY &&
                                            x.Y <= emgsOrder.MaxY)
                                 .Select(x => blckProp.GetElemDesignation(x))
                                 .ToList());
                    }
                    emergencyStopGroup.EmergencyStop =
                    new EmergencyStopGroupsEmergencyStopGroupEmergencyStop { Value = values.ToArray() };
                    emergencystopgroups.Add(emergencyStopGroup);
                }
                return;
            }
            TFileDescr document =
                new TFileDescr();
            var emsgs = excel.EmergStops(loadFiles["lblxlsEmSg"], ref document)
                .Select(x => new { x.EmergSg })
                .Where(x => x.EmergSg.Contains("emgs")).Distinct().ToList();
            //List<EmergencyStopGroupsEmergencyStopGroupEmergencyStop> emergencyStopGroupEmergencyStops = new
            //         List<EmergencyStopGroupsEmergencyStopGroupEmergencyStop>();
            foreach (var emsg in emsgs)
            {
                EmergencyStopGroupsEmergencyStopGroup emergencyStopGroup =
                    new EmergencyStopGroupsEmergencyStopGroup
                    {
                        Designation = emsg.EmergSg,
                        Status = Status,
                    };
                List<string> values = new List<string>();
                document =
                new TFileDescr();
                var emergStopGroups =
                    excel.EmergStops(loadFiles["lblxlsEmSg"], ref document)
                    .Where(x => x.Designation == emsg.EmergSg)
                    .Select(x => new { x.Designation, x.EmergSg })
                    .Distinct();
                //List<ReadExcel.EmergStopGroup> emergStopGroups =
                //excel.EmergStops(loadFiles["lblxlsEmSg"], ref document).Where(x => x.Designation == emsg.EmergSg).ToList();
                foreach (var mb in emergStopGroups)
                {
                    values.Add(mb.EmergSg);
                }
                emergencyStopGroup.EmergencyStop =
                    new EmergencyStopGroupsEmergencyStopGroupEmergencyStop { Value = values.ToArray() };
                emergencystopgroups.Add(emergencyStopGroup);
            }
        }

        private bool ReadSps(ref List<SpeedProfilesSpeedProfile> speedprofiles)
        {
            bool error = false;
            if (!this.checkData["checkBoxSpProf"])
            {
                ErrLogger.Information("Speed profiles data skipped", "Read SSP");
                return !error;
            }
            TFileDescr document = new TFileDescr();
            speedprofiles = this.excel.SpeedProfiles(this.loadFiles["lblxlsSpProf"], ref document, this.stationID, ref error);
            this.Documents.Add(document);
            return !error;
        }

        private bool ReadRoutes(List<Block> blocks,
             ref List<RoutesRoute> routes, List<AxleCounterSectionsAxleCounterSection> acsections)
        {
            bool error = false;
            if (!checkData["checkBoxRts"])
            {
                ErrLogger.Information("Routes data skipped", "Routes");
                return !error;
            }
            TFileDescr document =
               new TFileDescr();
            List<ReadExcel.XlsRoute> xlsRoutes =
                excel.Routes(loadFiles["lblxlsRoutes"], ref document, ref error);
            if (xlsRoutes == null)
            {
                return false;
            }
            Documents.Add(document);
            foreach (ReadExcel.XlsRoute route in xlsRoutes)
            {

                if (route.Overlaps.Count > 0)
                {

                }
                RoutesRoute Route = new RoutesRoute()
                {
                    Designation = route.Start + "_" + route.Dest,
                    Status = Status,
                    KindOfRoute = route.Type,
                    Start = route.Start,
                    Destination = route.Dest,
                    Default = route.Default,
                    SdLastElementID = route.SdLast,
                    SafetyDistance = route.SafeDist ?? null,
                };
                List<RoutesRouteStartAreaGroupStartArea> areas = new List<RoutesRouteStartAreaGroupStartArea>();
                route.StartAreas.Sort();
                foreach (string StartArea in route.StartAreas)
                {
                    if (StartArea != null && StartArea != "n" && StartArea != "-")
                    {
                        areas.Add(new RoutesRouteStartAreaGroupStartArea
                        {
                            ElementID = blckProp.GetElemDesignation(StartArea)
                        });
                    }
                }
                if (areas.Count > 0)
                {
                    Route.StartAreaGroup = new RoutesRouteStartAreaGroup { StartArea = areas.ToArray() };
                }

                if (route.ExtDest != null)
                {
                    var section =
                    acsections.Where(x => x.Designation == route.ExtDest).FirstOrDefault();
                    if (section != null)
                    {
                        Route.DestinationArea =
                            new RoutesRouteDestinationArea { ElementID = section.Elements.Element[0].Value };
                    }
                    else
                    {
                        Route.DestinationArea = new RoutesRouteDestinationArea { ElementID = route.ExtDest };
                    }
                }

                if (route.SdLast != null)
                {
                    Block SdLast = blocks
                    .Where(x => (x.Attributes.Any(y => y.Key == "NAME") && x.Attributes["NAME"].Value == route.SdLast))
                    .FirstOrDefault();
                    if (SdLast != null)
                    {
                        Route.SdLastElementID = blckProp.GetElemDesignation(SdLast);
                    }
                }

                List<ReadExcel.XlsRoute> xlsDefault = xlsRoutes
                    .Where(x => (x.Start + "_" + x.Dest == Route.Designation))
                    .ToList();
                if (xlsDefault.Count == 1)
                {
                    Route.Default = YesNoType.yes;
                }

                if (route.ActCross.Count() != 0)
                {
                    List<RoutesRouteActivateCrossingElementGroupActivateCrossingElement> crossingElements =
                   new List<RoutesRouteActivateCrossingElementGroupActivateCrossingElement>();
                    foreach (string cract in route.ActCross)
                    {
                        crossingElements.Add(new RoutesRouteActivateCrossingElementGroupActivateCrossingElement
                        {
                            Value = cract
                        });
                    }
                    Route.ActivateCrossingElementGroup = new RoutesRouteActivateCrossingElementGroup
                    {
                        ActivateCrossingElement = crossingElements.ToArray()
                    };
                }

                List<RoutesRoutePointGroupPoint> groupPoints = new List<RoutesRoutePointGroupPoint>();
                if (route.PointsGrps.Count() != 0)
                {
                    foreach (ReadExcel.XlsPoint xlsPoint in route.PointsGrps.Where(x => x.Designation != null))
                    {
                        ReadExcel.XlsPoint point = xlsPoint;
                        Block pointBlk = blocks
                                         .Where(x => x.XsdName == "Point" &&
                                                     x.Attributes["NAME"].Value == point.Designation)
                                         .FirstOrDefault();
                        string pointDesignation = point.Designation;
                        if (pointBlk != null)
                        {
                            pointDesignation = this.blckProp.GetElemDesignation(pointBlk, false, true);
                        }
                        else
                        {
                            ErrLogger.Error("Point not found on SL", Route.Designation, point.Designation);
                            ErrLogger.ErrorsFound = true;
                        }
                        groupPoints.Add(new RoutesRoutePointGroupPoint()
                        {
                            Value = pointDesignation,
                            RequiredPosition = point.ReqPosition
                        });
                    }
                }
                if (groupPoints != null && groupPoints.Count > 0)
                    Route.PointGroup = new RoutesRoutePointGroup()
                    {
                        Point = groupPoints.ToArray()
                    };
                else
                    Route.PointGroup = (RoutesRoutePointGroup)null;
                routes.Add(Route);
            }
            List<RoutesRoute> dubplicatesRoutes = routes.GroupBy(s => s.Designation)
                                                        .SelectMany(grp => grp.Skip(1))
                                                        .Distinct()
                                                        .ToList();
            foreach (RoutesRoute duproute in dubplicatesRoutes)
            {
                List<RoutesRoute> sameRoutes = routes
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
            return !error;
        }

        public void RoutesList()
        {
            List<RoutesRoute> routes = new List<RoutesRoute>();
            foreach (Block signalBlk in this.blocks.Where(x => x.XsdName == "Signal" && !x.Virtual))
            {
                var signal = this.signals
                             .Where(x => x.Designation == signalBlk.Designation &&
                                         x.KindOfSignal != TKindOfSignal.eotmb)
                             .FirstOrDefault();
                if (signal != null)
                {
                    routes.AddRange(GetDestSignals(signal));
                }
                else
                {
                    ErrLogger.Error("Start signal for routes list not found in RDD", signalBlk.Designation, "");
                    ErrLogger.ErrorsFound = true;
                }
            }

            XmlSerializer serializer = new XmlSerializer(typeof(Routes));
            XmlWriterSettings settings = new XmlWriterSettings
            {
                IndentChars = "\t",
                Indent = true
            };
            using (StreamWriter stream = new StreamWriter(dwgDir + @"/routest_test.xml"))
            {
                XmlWriter writer = XmlWriter.Create(stream, settings);
                writer.WriteStartDocument();
                serializer.Serialize(writer, new Routes { Route = routes.DistinctBy(x => x.Designation).OrderBy(x => x.Designation).ToArray() });
                writer.WriteEndDocument();
                writer.Flush();
            }
        }

        private bool ReadTrustedAreas(ref List<TrustedAreasTrustedArea> trustedAreas)
        {
            bool error = false;
            foreach (TrustedArea trustedArea in TrustedAreas)
            {
                //BlockRef block1 = null;
                //BlockRef block2 = null;
                List<TrackSegmentType> trackSegments = new List<TrackSegmentType>();
                List<TrackSegmentTmp> tmpSegments = TrackSegmentsTmp
                    .Where(s => s.TrustedLines.Any(x => trustedArea.Lines.Contains(x)))
                    .OrderBy(x => x.Vertex1.Location)
                    .ToList();
                for (int j = 0; j < tmpSegments.Count; j++)
                {
                    if (j > 0 && j < tmpSegments.Count - 1)
                    {
                        continue;
                    }
                    List<Block> edges = tmpSegments[j].BlocksOnSegments
                                .Where(x => trustedArea.Lines
                                .Any(l => ObjectsIntersects(l, x.BlkRef, Intersect.OnBothOperands) && LineIsVertical(l) &&
                                (x.XsdName == "BaliseGroup" || x.XsdName == "Signal") &&
                                tmpSegments[j].TrackLines
                                    .Any(t => ObjectsIntersects(t, x.BlkRef, Intersect.OnBothOperands))))
                                .GroupBy(g => g.Location)
                                .Select(g => g.FirstOrDefault())
                                .OrderBy(x => x.Location)
                                .ToList();
                    if (edges.Count == 0)
                    {
                        error = true;
                        ErrLogger.Error("Edges not found for Track Segment", "Trusted area", tmpSegments[j].Designation);
                        continue;
                    }
                    if (tmpSegments.Count > 1)
                    {
                        if (j == 0)
                        {
                            trackSegments.Add(new TrackSegmentType
                            {
                                Value = tmpSegments.First().Designation,
                                OperationalKM1 = edges.First().Attributes["KMP"].Value,
                            });
                            TrackSegmentTmp nextSegment = tmpSegments.First();
                            while (nextSegment != tmpSegments.Last())
                            {
                                nextSegment = TrackSegmentsTmp
                                              .Where(x => x.Vertex1 == nextSegment.Vertex2)
                                              .FirstOrDefault();
                                if (nextSegment == null)
                                {
                                    break;
                                }
                                if (nextSegment != tmpSegments.First() && nextSegment != tmpSegments.Last())
                                {
                                    trackSegments.Add(new TrackSegmentType
                                    {
                                        Value = nextSegment.Designation,
                                    });
                                }
                            }
                        }

                        if (j == tmpSegments.Count - 1)
                        {
                            trackSegments.Add(new TrackSegmentType
                            {
                                Value = tmpSegments.Last().Designation,
                                OperationalKM2 = edges.First().Attributes["KMP"].Value
                            });
                        }
                    }
                    else if (tmpSegments.Count == 1)
                    {
                        if (edges.Count == 2)
                        {
                            trackSegments.Add(new TrackSegmentType
                            {
                                Value = tmpSegments.First().Designation,
                                OperationalKM1 = edges.First().Attributes["KMP"].Value,
                                OperationalKM2 = edges.Last().Attributes["KMP"].Value
                            });
                        }
                        else if (edges.Count > 2)
                        {
                            ErrLogger.Information("More then 2 edges found for trusted area", tmpSegments.First().Designation);
                            trackSegments.Add(new TrackSegmentType
                            {
                                Value = tmpSegments.First().Designation,
                                OperationalKM1 = edges.First().Attributes["KMP"].Value,
                                OperationalKM2 = edges.Last().Attributes["KMP"].Value
                            });
                        }
                        else
                        {
                            error = true;
                            ErrLogger.Error("Edges not found for Track Segment", "Trusted area", tmpSegments[j].Designation);
                            continue;
                        }
                    }
                }

                TrustedAreasTrustedArea trustedAreasTrustedArea = new TrustedAreasTrustedArea
                {
                    Designation = trustedArea.Designation,
                    Status = Status,
                    TrackSegments = new TrustedAreasTrustedAreaTrackSegments
                    {
                        TrackSegment = trackSegments.ToArray()
                    }
                };
                trustedAreas.Add(trustedAreasTrustedArea);
            }
            return !error;
        }

        private bool ReadPlatforms(List<Block> blocks,
           ref List<PlatformsPlatform> platforms)
        {
            bool errors = false;
            List<Block> BlkPlatforms = blocks
                                        .Where(x => x.XsdName == "PlatformDyn" &&
                                                    x.IsOnNextStation == false)
                                        .ToList();
            foreach (Block Blkplatform in BlkPlatforms)
            {
                List<TrackSegmentTmp> trackSegments = TrackSegmentsTmp
                                        .Where(x => x.TrackLines
                                        .Any(y => ObjectsIntersects(y, Blkplatform.BlkRef,
                                                                Intersect.OnBothOperands))) //&& x.Track != null
                                        .OrderBy(x => x.Vertex1.Location)
                                        .ThenBy(x => Convert.ToInt32(x.Track))
                                        .ToList();
                int Track = trackSegments
                    .Where(x => x.Track != null || !String.IsNullOrEmpty(x.Track))
                    .Select(x => Convert.ToInt32(x.Track))
                    .FirstOrDefault();
                Blkplatform.PlatformTrack = Track;
            }

            var query = BlkPlatforms.OrderBy(x => x.PlatformTrack).GroupBy(g => new { g.PlatformTrack, g.StId }).Select(p => p).ToList();
            foreach (var group in query)
            {
                string a = "";
                char a1 = 'a';
                int plCount = group.Count();
                foreach (Block Blkplatform in group)
                {
                    if (plCount > 1)
                    {
                        a += a1++;
                    }
                    platforms.Add(PlatformsCalc(Blkplatform, ref errors, a));
                }
            }

            //var duplPlatfs = platforms.GroupBy(x => x.Designation)
            //  .Where(g => g.Count() > 1)
            //  .SelectMany(y => y)
            //  .OrderBy(y => y.PositionOfPlatform)
            //  .ToList();
            //char a1 = 'a';
            //foreach (var pr in duplPlatfs)
            //{
            //    pr.Designation = pr.Designation + a1++;
            //}
            return !errors;
        }

        private PlatformsPlatform PlatformsCalc(Block Blkplatform, ref bool errors, string a)
        {
            List<Platform> platformsFromFile = ReadPlatforms(Blkplatform.StName);
            List<TrackSegmentTmp> trackSegments = TrackSegmentsTmp
                                    .Where(x => x.TrackLines
                                    .Any(y => ObjectsIntersects(y, Blkplatform.BlkRef,
                                                            Intersect.OnBothOperands))) //&& x.Track != null
                                    .OrderBy(x => x.Vertex1.Location)
                                    .ThenBy(x => Convert.ToInt32(x.Track))
                                    .ToList();
            //int Track = trackSegments
            //    .Where(x => x.Track != null || !String.IsNullOrEmpty(x.Track))
            //    .Select(x => Convert.ToInt32(x.Track))
            //    .FirstOrDefault();
            //int number =
            //    Convert.ToInt32(Regex.Replace(Blkplatform.Attributes["NAME"].Value, "[^0-9]", ""));
            //if (!int.TryParse(Regex.Replace(Blkplatform.Attributes["NAME"].Value, "[^0-9]", ""), out int number))
            //{
            //    ErrLogger.Warning("Unable parse platform name to number: '" + Blkplatform.Attributes["NAME"].Value + "'");
            //}
            List<TrackSegmentType> segmentTypes = new List<TrackSegmentType>();
            if (trackSegments.Count == 1)
            {
                List<string> KmValues =
                    new List<string>
                    {
                            Blkplatform.Attributes["START_PLAT_1"].Value,
                            Blkplatform.Attributes["END_PLAT_1"].Value
                    }
                    .OrderBy(x => x).ToList();
                segmentTypes.Add(
                new TrackSegmentType
                {
                    OperationalKM1 = KmValues[0],
                    OperationalKM2 = KmValues[1],
                    Value = trackSegments.First().Designation
                });
            }
            else if (trackSegments.Count > 1)
            {
                TrackSegmentTmp nextSegment = trackSegments.First();
                List<string> KmValues =
                    new List<string>
                    {
                            Blkplatform.Attributes["START_PLAT_1"].Value,
                            Blkplatform.Attributes["END_PLAT_1"].Value
                    }
                    .OrderBy(x => x).ToList();
                segmentTypes.Add(
                            new TrackSegmentType
                            {
                                OperationalKM1 = KmValues[0],
                                OperationalKM2 = null,
                                Value = trackSegments.First().Designation
                            });
                while (nextSegment != trackSegments.Last())
                {
                    TrackLine segLine = TracksLines
                                        .Where(x => x.line == nextSegment.TrackLines[0])
                                        .FirstOrDefault();
                    if (segLine != null)
                    {
                        if (segLine.direction == DirectionType.up)
                        {
                            nextSegment = TrackSegmentsTmp
                                  .Where(x => x.Vertex1 == nextSegment.Vertex2 &&
                                              x.Vertex1.X < Blkplatform.BlkRef.GeometricExtents.MaxPoint.X)
                                  .OrderBy(x => x.Vertex1.X)
                                  .FirstOrDefault();
                        }
                        else
                        {
                            nextSegment = TrackSegmentsTmp
                                  .Where(x => x.Vertex2 == nextSegment.Vertex1 &&
                                              x.Vertex2.X < Blkplatform.BlkRef.GeometricExtents.MaxPoint.X)
                                  .OrderBy(x => x.Vertex2.X)
                                  .FirstOrDefault();
                        }
                    }
                    else
                    {
                        nextSegment = TrackSegmentsTmp
                                  .Where(x => x.Vertex1 == nextSegment.Vertex2 &&
                                              x.Vertex1.X < Blkplatform.BlkRef.GeometricExtents.MaxPoint.X)
                                  .OrderBy(x => x.Vertex1.X)
                                  .FirstOrDefault();
                    }
                    if (nextSegment == null)
                    {
                        break;
                    }
                    if (nextSegment != trackSegments.First() && nextSegment != trackSegments.Last())
                    {
                        segmentTypes.Add(
                            new TrackSegmentType
                            {
                                OperationalKM1 = null,
                                OperationalKM2 = null,
                                Value = nextSegment.Designation
                            });
                    }
                }
                segmentTypes.Add(
                            new TrackSegmentType
                            {
                                OperationalKM1 = null,
                                OperationalKM2 = KmValues[1],
                                Value = trackSegments.Last().Designation
                            });
            }
            //Regex re = new Regex("^Pl|^Per|^pl|^per");
            //string[] platDesignTmp =
            //    re.Replace(blckProp.GetElemDesignation(Blkplatform, PadZeros: false), "pr", 1).Split('-');
            //string platDesign = "";
            //if (platDesignTmp.Count() < 2)
            //{
            //    errors = true;
            //    ErrLogger.Warning("Platform: " + blckProp.GetElemDesignation(Blkplatform) + "Platform name wrong format");
            //    platDesign = blckProp.GetElemDesignation(Blkplatform);
            //}
            //else
            //{
            //    platDesign = "pr-" +
            //                 platDesignTmp.ElementAt(platDesignTmp.Count() - 2) +
            //                 "-" +
            //                 platDesignTmp.Last();
            //}
            PlatformsPlatform platformsPlatform = new PlatformsPlatform
            {
                //Designation = "pr-" + stationID + "-" +
                //Regex.Replace(Blkplatform.Attributes["PLATFORM_1"].Value, "[^0-9]", ""),
                Designation = "pr-" + Blkplatform.StId + "-" + Blkplatform.PlatformTrack.ToString() + a, //.Replace("0",""),
                Status = Status,
                TrackSegments = new PlatformsPlatformTrackSegments
                {
                    TrackSegment = segmentTypes.ToArray()
                },
                Remarks = "Default Platform Height. Not found in 'Banedanmarks Netredegørelse 2018 Bilag 3.6A Perronlængder og - højder'"
            };

            UpDownBothType direction;
            if (Enum.TryParse(Blkplatform.Attributes["DIRECTION_PLAT"].Value, out direction))
            {
                platformsPlatform.TrainRunningDirection = direction;
            }
            else
            {
                errors = true;
                ErrLogger.Error("Unable to parse attribute value", platformsPlatform.Designation, "DIRECTION_PLAT");
            }

            if (Enum.TryParse(Blkplatform.Attributes["POSITION_PLAT"].Value,
                          out LeftRightType ParsePosition))
            {
                platformsPlatform.PositionOfPlatform = ParsePosition;
            }
            else
            {
                errors = true;
                ErrLogger.Error("Unable to parse attribute value", platformsPlatform.Designation, "POSITION_PLAT");
            }
            Platform platformFromFile = platformsFromFile
                                       .Where(x => x.Track == Blkplatform.PlatformTrack &&
                                        x.Number == Blkplatform.PlatformTrack)
                                        .OrderBy(x => x.Track)
                                       .FirstOrDefault();
            if (platformFromFile == null)
            {
                ErrLogger.Error("Platform not found in data file", platformsPlatform.Designation,
                    "track:" + Blkplatform.PlatformTrack);
                errors = true;
            }
            else
            {
                platformsPlatform.PlatformHeight = PlatformHeight(platformFromFile.Height);
                platformsPlatform.Remarks = "Real Platform Height is  "
                    + platformFromFile.Height.ToString() +
                    "mm. (Extracted from Banedanmarks Netredegørelse 2018 Bilag 3.6A Perronlængder og - højder)";
            }
            return platformsPlatform;
        }

        protected bool ReadPSAs(List<PSA> pSAs,
           ref List<PermanentShuntingAreasPermanentShuntingArea> areas)
        {
            bool errors = false;
            foreach (PSA pSA in pSAs)
            {
                PermanentShuntingAreasPermanentShuntingArea shuntingArea =
                    new PermanentShuntingAreasPermanentShuntingArea
                    {
                        Designation = pSA.Name.ToLower(),
                        Status = Status
                    };
                List<TrackSegmentTmp> trackSegmentsTmp = TrackSegmentsTmp
                    .Where(x => x.TrackLines
                                .Any(l => ObjectsIntersects(l, pSA.PsaPolyLine, Intersect.OnBothOperands)))
                    .ToList();
                if (trackSegmentsTmp.Count == 0)
                {
                    ErrLogger.Error("Track Segments not found", pSA.Name.ToLower(), "");
                    areas.Add(shuntingArea);
                    errors = true;
                    continue;
                }
                Block blockBeginPsa = trackSegmentsTmp
                                           .Where(t => t.Vertex1.XsdName != "EndOfTrack" &&
                                                       t.Vertex2.XsdName != "EndOfTrack")
                                           .SelectMany(t => t.BlocksOnSegments)
                    .Where(x => ObjectsIntersects(x.BlkRef, pSA.PsaPolyLine, Intersect.OnBothOperands) &&
                                x.XsdName == "DetectionPoint")
                    .FirstOrDefault();
                if (blockBeginPsa != null)
                {
                    shuntingArea.LineID = blockBeginPsa.LineID;
                    shuntingArea.BeginOfPSA = blockBeginPsa.Location.ToString();
                }
                else
                {
                    blockBeginPsa = trackSegmentsTmp
                                           .SelectMany(t => t.BlocksOnSegments)
                    .Where(x => ObjectsIntersects(x.BlkRef, pSA.PsaPolyLine, Intersect.OnBothOperands) &&
                                x.XsdName == "DetectionPoint")
                    .FirstOrDefault();
                    if (blockBeginPsa != null)
                    {
                        shuntingArea.LineID = blockBeginPsa.LineID;
                        shuntingArea.BeginOfPSA = blockBeginPsa.Location.ToString();
                    }
                    else
                    {
                        ErrLogger.Error("Begin PSA block not found", pSA.Name.ToLower(), "");
                        errors = true;
                    }
                }
                if (blockBeginPsa != null)
                {
                    pSA.begin = blockBeginPsa.Location;
                }
                areas.Add(shuntingArea);
            }
            return !errors;
        }

        private bool ReadBlockinterfaces(List<Block> blocks,
          ref List<BlockInterfacesBlockInterface> blockInterfaces)
        {
            bool errors = false;
            List<Block> BlkBis = blocks
                                        .Where(x => x.XsdName == "BlockInterface")
                                        .ToList();
            foreach (Block bi in BlkBis)
            {
                YesNoType PerHand = new YesNoType();
                if (!Enum.TryParse(bi.Attributes["PER_HAND"].Value, out PerHand))
                {
                    errors = true;
                    ErrLogger.Error("Unable to parse attribute value", blckProp.GetElemDesignation(bi), "PER_HAND");
                }
                blockInterfaces.Add(new BlockInterfacesBlockInterface
                {
                    Designation = blckProp.GetElemDesignation(bi, PadZeros: false),
                    KindOfBI = bi.Attributes["KIND"].Value,
                    PermissionHandling = PerHand,
                    Status = Status
                });
            }
            return !errors;
        }

        private PlatformHeightType PlatformHeight(int number)
        {
            List<int> list = new List<int>
            { 200, 300, 380, 550, 580, 680, 685,
                730, 760, 840, 900, 915, 920, 960, 1100
            };

            switch (list.Aggregate((x, y) => Math.Abs(x - number) < Math.Abs(y - number) ? x : y))
            {
                case 200:
                    return PlatformHeightType.Item200;
                case int n when (n <= 380 && n >= 300):
                    return PlatformHeightType.Item300380;
                case 550:
                    return PlatformHeightType.Item550;
                case 580:
                    return PlatformHeightType.Item580;
                case 680:
                    return PlatformHeightType.Item680;
                case 685:
                    return PlatformHeightType.Item685;
                case 730:
                    return PlatformHeightType.Item730;
                case 760:
                    return PlatformHeightType.Item760;
                case 840:
                    return PlatformHeightType.Item840;
                case 900:
                    return PlatformHeightType.Item900;
                case 915:
                    return PlatformHeightType.Item915;
                case 920:
                    return PlatformHeightType.Item920;
                case 960:
                    return PlatformHeightType.Item960;
                case 1100:
                    return PlatformHeightType.Item1100;
                default:
                    return PlatformHeightType.Item200;
            }
        }

        private Point2d GetBlockCross(Block block)
        {
            DBObjectCollection entset = new DBObjectCollection();
            block.BlkRef.Explode(entset);
            List<Line> tmpCross = new List<Line>();
            // if cross not found take insertion point of block
            Point2d cross = new Point2d(block.BlkRef.Position.X, block.BlkRef.Position.Y);
            foreach (DBObject obj in entset)
            {
                if (obj.GetType() == typeof(Line))
                {
                    if (((Line)obj).Layer == "Cross")
                    {
                        tmpCross.Add((Line)obj);
                        if (tmpCross.Count == 2)
                        {
                            Point3dCollection intersections = new Point3dCollection();
                            tmpCross[0].IntersectWith(tmpCross[1], Intersect.OnBothOperands, intersections, IntPtr.Zero, IntPtr.Zero);
                            if (intersections != null && intersections.Count > 0)
                            {
                                return cross =
                                    new Point2d(intersections[0].X, intersections[0].Y);
                            }
                        }
                    }

                }
            }
            return cross;
        }

        private LeftRightOthersType GetSignalTrackPosition(Block BlkSignal, ref bool error)
        {
            List<Line> lines = GetBlockLines(BlkSignal);
            Point2d cross = GetBlockCross(BlkSignal);
            Point2d fromTrackY = new Point2d(0, 0);
            fromTrackY = AcadTools.GetMiddlPoint2d(lines.Where(x => x.Length > 5 &&
                                                (x.StartPoint.X != cross.X) &&
                                                (x.StartPoint.Y != cross.Y) &&
                                                (x.EndPoint.X != cross.X) &&
                                                (x.EndPoint.Y != cross.Y))
                                          .FirstOrDefault()
                                          .GeometricExtents
                                     );

            if (BlkSignal.Attributes.ContainsKey("EOTMB") && BlkSignal.Attributes["EOTMB"].Value.Equals("yes"))
            {
                return LeftRightOthersType.others;
            }
            else if ((fromTrackY.Y > cross.Y && fromTrackY.X > cross.X) ||
                     (fromTrackY.Y < cross.Y && fromTrackY.X < cross.X))
            {
                return LeftRightOthersType.left;
            }
            else if ((fromTrackY.Y > cross.Y && fromTrackY.X < cross.X) ||
                     (fromTrackY.Y < cross.Y && fromTrackY.X > cross.X))
            {
                return LeftRightOthersType.right;
            }
            else
            {
                ErrLogger.Error("Unable to find track position of signal", blckProp.GetElemDesignation(BlkSignal), "");
                error = true;
            }

            return LeftRightOthersType.others;
        }

        private DirectionType GetSignalDirection(Block BlkSignal, ref bool error, bool suppressLog = true)
        {
            DirectionType sigDir;

            List<Line> lines = GetBlockLines(BlkSignal);
            Point2d cross = GetBlockCross(BlkSignal);

            RailwayLine Line = RailwayLines
                .Where(x => x.designation == BlkSignal.LineID)
                .FirstOrDefault();
            DirectionType lineDir = new DirectionType();
            if (Line != null)
            {
                lineDir = Line.direction;
            }
            int SigLinesCount = 0;
            if (lineDir == DirectionType.up)
            {
                SigLinesCount = lines
                                .Where(x => x.GeometricExtents.MinPoint.X > cross.X + 2 /*&&
                                                (x.StartPoint.X != cross.X) &&
                                                (x.StartPoint.Y != cross.Y) &&
                                                (x.EndPoint.X != cross.X) &&
                                                (x.EndPoint.Y != cross.Y)*/)
                                .ToList()
                                .Count();
            }
            else if (lineDir == DirectionType.down)
            {
                SigLinesCount = lines
                                .Where(x => x.GeometricExtents.MaxPoint.X < cross.X - 2 /*&&
                                                (x.StartPoint.X != cross.X) &&
                                                (x.StartPoint.Y != cross.Y) &&
                                                (x.EndPoint.X != cross.X) &&
                                                (x.EndPoint.Y != cross.Y)*/)
                                .ToList()
                                .Count();
            }
            if (SigLinesCount > 2)
            {
                sigDir = DirectionType.up;
            }
            else //if (SigLinesCount > 0)
            {
                sigDir = DirectionType.down;
            }
            if (BlkSignal.Attributes.ContainsKey("DIRECTION"))
            {
                Enum.TryParse(BlkSignal.Attributes["DIRECTION"].Value, out DirectionType dirTmp);
                if (sigDir != dirTmp)
                {
                    error = true;
                    if (!suppressLog)
                    {
                        ErrLogger.Error("Signal direction not match with attribute '", blckProp.GetElemDesignation(BlkSignal),
                            "atts:" + BlkSignal.Attributes["DIRECTION"].Value + " calc:" + dirTmp);
                        ErrLogger.ErrorsFound = true;
                    }                  
                }
            }
            return sigDir;
        }

        private bool IsBlockInsidePSA(BlockReference block, double tol)
        {
            return pSAs
                   .Any(x => block.Position.X >= x.MinX - tol &&
                             block.Position.X <= x.MaxX + tol &&
                             block.Position.Y >= x.MinY - tol &&
                             block.Position.Y <= x.MaxY + tol);
        }

        private bool IsSignalToPSA(Block signal, DirectionType direction)
        {
            if (IsBlockInsidePSA(signal.BlkRef, Constants.psaTol))
            {
                return false;
            }
            List<TrackSegmentTmp> segNodes = this.TrackSegmentsTmp
                              .Where(x => x.Designation == signal.TrackSegId)
                              .ToList();
            if (segNodes.Count == 0)
            {
                ErrLogger.Error("Start Segment(s) not found", signal.Designation, "Sig To PSA");
                ErrLogger.ErrorsFound = true;
                return false;
            }
            Stack<Stack<TrackSegmentTmp>> stackNodes = new Stack<Stack<TrackSegmentTmp>>();
            stackNodes.Push(new Stack<TrackSegmentTmp>(segNodes));
            int iterCount = 0;
            bool sigDirError = false;
            decimal tmpSigLocation = signal.Location;
            decimal kmGap = 0;
            while (stackNodes.Count > 0)
            {
                Block nextSignal = null;
                if (direction == DirectionType.up)
                {
                    nextSignal = this.blocks
                             .Where(x => x.XsdName == "Signal" &&
                                         x.Location > tmpSigLocation &&
                                         GetSignalDirection(x, ref sigDirError, true) == direction &&
                                         x.TrackSegId == stackNodes.Peek().Peek().Designation)
                             .OrderBy(x => Convert.ToDecimal(x.Location))
                             .FirstOrDefault();
                }
                else
                {
                    nextSignal = this.blocks
                             .Where(x => x.XsdName == "Signal" &&
                                         x.Location < tmpSigLocation &&
                                         GetSignalDirection(x, ref sigDirError, true) == direction &&
                                         x.TrackSegId == stackNodes.Peek().Peek().Designation)
                             .OrderByDescending(x => Convert.ToDecimal(x.Location))
                             .FirstOrDefault();
                }
                PSA pSA = null;
                pSA = pSAs
                      .Where(x => stackNodes.Peek().Peek().TrackLines
                                  .Any(l => ObjectsIntersects(l, x.PsaPolyLine, Intersect.OnBothOperands)))
                      .FirstOrDefault();
                if (pSA != null)
                {
                    if (nextSignal == null)
                    {
                        return true;
                    }
                    if (direction == DirectionType.up && pSA.begin > nextSignal.Location)
                    {
                        return false;
                    }
                    if (direction == DirectionType.down && pSA.begin < nextSignal.Location)
                    {
                        return false;
                    }
                }
                if (nextSignal == null)
                {
                    iterCount++;
                    segNodes = new List<TrackSegmentTmp>();
                    if (IsLineChanging(stackNodes.Peek().Peek(), direction, out DirectionType newDirection, out kmGap, out TrackSegmentTmp changeSeg, out Block _))
                    {
                        segNodes.Add(changeSeg);
                        tmpSigLocation += kmGap;
                        direction = newDirection;
                        ErrLogger.Error("Check Signal to PSA: Line change detected. Should be rechecked manually.", signal.Designation, "");
                        ErrLogger.ErrorsFound = true;
                    }
                    if (direction == DirectionType.up)
                    {
                        if (stackNodes.Peek().Peek().Vertex2.XsdName == "Point" ||
                            stackNodes.Peek().Peek().Vertex2.XsdName == "EndOfTrack")
                        {
                            return false;
                        }
                        segNodes = this.TrackSegmentsTmp
                                   .Where(x => x.Vertex1 == stackNodes.Peek().Peek().Vertex2 &&
                                                x != changeSeg)
                                   .ToList();
                    }
                    else
                    {
                        if (stackNodes.Peek().Peek().Vertex1.XsdName == "Point" ||
                            stackNodes.Peek().Peek().Vertex1.XsdName == "EndOfTrack")
                        {
                            return false;
                        }
                        segNodes = this.TrackSegmentsTmp
                                   .Where(x => x.Vertex2 == stackNodes.Peek().Peek().Vertex1 &&
                                               x != changeSeg)
                                   .ToList();
                    }
                    iterCount++;
                    if (segNodes.Count == 0 || Constants.dpIterLimit == iterCount)
                    {
                        if (Constants.dpIterLimit == iterCount)
                        {
                            ErrLogger.Error("Iteration limit reached", signal.Designation, "Sig to PSA");
                        }
                        else
                        {
                            ErrLogger.Error("Segment(s) for next signal not found", signal.Designation, "Sig to PSA");
                        }
                        ErrLogger.ErrorsFound = true;
                        break;
                    }
                    else
                    {
                        stackNodes.Push(new Stack<TrackSegmentTmp>(segNodes));
                    }
                }
                else
                {
                    return false;

                }
            }
            return false;
        }

        private DangerPoint GetDangerPoint(Block signal, DirectionType direction)
        {
            DangerPoint dangerPoint = new DangerPoint();
            List<Block> dpsFound = new List<Block>();
            List<TrackSegmentTmp> segNodes = this.TrackSegmentsTmp
                              .Where(x => x.Designation == signal.TrackSegId)
                              .ToList();
            if (segNodes.Count == 0)
            {
                ErrLogger.Error("Start Segment(s) not found", signal.Designation, "Danger point");
                ErrLogger.ErrorsFound = true;
                return dangerPoint;
            }
            Stack<Stack<TrackSegmentTmp>> stackNodes = new Stack<Stack<TrackSegmentTmp>>();
            stackNodes.Push(new Stack<TrackSegmentTmp>(segNodes));
            int iterCount = 0;
            decimal KmGap = 0;
            decimal tmpSigLocation = signal.Location;
            TrackSegmentTmp prevSegment = null;
            while (stackNodes.Count > 0)
            {
                Block nextDp = null;
                if (prevSegment != null)
                {
                    string nextLineId = stackNodes.Peek().Peek().lineId;
                    if (prevSegment.lineId != nextLineId)
                    {
                        ErrLogger.Error("Line change between signal and danger point. Should be rechecked manually.", signal.Designation, "");
                        ErrLogger.ErrorsFound = true;
                        RailwayLine nextLine = this.RailwayLines
                                   .Where(x => x.designation == nextLineId)
                                   .FirstOrDefault();
                        if (nextLine != null)
                        {
                            direction = nextLine.direction;
                            tmpSigLocation += KmGap;
                        }
                    }
                }
                else
                {
                    tmpSigLocation = signal.Location;
                }
                if (direction == DirectionType.up)
                {
                    nextDp = this.blocks
                             .Where(x => x.XsdName == "DetectionPoint" &&
                                         x.Location >= tmpSigLocation &&
                                         x.TrackSegId == stackNodes.Peek().Peek().Designation)
                             .OrderBy(x => Convert.ToDecimal(x.Location))
                             .FirstOrDefault();
                }
                else
                {
                    nextDp = this.blocks
                             .Where(x => x.XsdName == "DetectionPoint" &&
                                         x.Location <= tmpSigLocation &&
                                         x.TrackSegId == stackNodes.Peek().Peek().Designation)
                             .OrderByDescending(x => Convert.ToDecimal(x.Location))
                             .FirstOrDefault();
                }
                prevSegment = stackNodes.Peek().Peek();
                if (nextDp != null)
                {
                    dpsFound.Add(nextDp);
                    do
                    {
                        if (stackNodes.Peek().Count == 2)
                        {
                            stackNodes.Peek().Pop();
                            prevSegment = stackNodes.Peek().Peek();
                            break;
                        }
                        stackNodes.Pop();
                        iterCount--;
                    }
                    while (stackNodes.Count > 0);
                }
                else
                {
                    
                    if (direction == DirectionType.up)
                    {
                        if (stackNodes.Peek().Peek().Vertex2.XsdName == "Connector")
                        {
                            if (decimal.TryParse(stackNodes.Peek().Peek().Vertex2.Attributes["KMGAP"].Value, out decimal tmp))
                            {
                                if (!tmp.ToString().Contains('.'))
                                {
                                    tmp *= 0.001M;
                                }
                                KmGap += tmp * -1;
                            }
                            else
                            {
                                ErrLogger.Error("Unable to parse attribute value", stackNodes.Peek().Peek().Vertex2.Designation, "danger point");
                                ErrLogger.ErrorsFound = true;
                            }
                        }
                        if (stackNodes.Peek().Peek().Vertex2.XsdName == "Point")
                        {
                            ErrLogger.Error("No axle counter between marker board and first interlocked movable element", signal.Designation, stackNodes.Peek().Peek().Vertex2.Designation);
                            ErrLogger.ErrorsFound = true;
                            return GetDangerPoint(stackNodes.Peek().Peek().Vertex2, DirectionType.down);                         
                        }
                        segNodes = this.TrackSegmentsTmp
                               .Where(x => (x.Vertex1 == stackNodes.Peek().Peek().Vertex2))
                               .ToList();
                    }
                    else
                    {
                        if (stackNodes.Peek().Peek().Vertex1.XsdName == "Connector")
                        {
                            if (decimal.TryParse(stackNodes.Peek().Peek().Vertex1.Attributes["KMGAP"].Value, out decimal tmp))
                            {
                                if (!tmp.ToString().Contains('.'))
                                {
                                    tmp *= 0.001M;
                                }
                                KmGap += tmp * -1;
                            }
                            else
                            {
                                ErrLogger.Error("Unable to parse attribute value", stackNodes.Peek().Peek().Vertex1.Designation, "danger point");
                                ErrLogger.ErrorsFound = true;
                            }
                        }
                        if (stackNodes.Peek().Peek().Vertex1.XsdName == "Point")
                        {
                            ErrLogger.Error("No axle counter between marker board and first interlocked movable element", signal.Designation, stackNodes.Peek().Peek().Vertex2.Designation);
                            ErrLogger.ErrorsFound = true;
                            return GetDangerPoint(stackNodes.Peek().Peek().Vertex1, DirectionType.up);
                        }
                        segNodes = this.TrackSegmentsTmp
                               .Where(x => (x.Vertex2 == stackNodes.Peek().Peek().Vertex1))
                               .ToList();
                    }                  

                    iterCount++;
                    if (segNodes.Count == 0 || Constants.dpIterLimit == iterCount)
                    {
                        if (Constants.dpIterLimit == iterCount)
                        {
                            ErrLogger.Error("Iteration limit reached", signal.Designation, "danger point");
                        }
                        else
                        {
                            ErrLogger.Error("Segment(s) for next dp not found", signal.Designation, "danger point");
                        }
                        ErrLogger.ErrorsFound = true;
                        break;
                    }
                    else
                    {
                        stackNodes.Push(new Stack<TrackSegmentTmp>(segNodes));
                    }
                }
            }

            if (dpsFound.Count == 0)
            {
                ErrLogger.Error("Unable to find danger point.", signal.Designation, "");
                ErrLogger.ErrorsFound = true;
                return dangerPoint;
            }
            Block dpFound = null;
            if (dpsFound.Count == 1)
            {
                dpFound = dpsFound[0];
            }
            else
            {
                decimal min = dpsFound
                              .Min(x => Math.Abs(x.Location - signal.Location));
                dpFound = dpsFound
                          .Where(x => Math.Abs(x.Location - signal.Location) == min)
                          .FirstOrDefault();
            }
                      
            if (dpFound.Location >= signal.Location)
            {
                dangerPoint.Distance = (int)((Math.Abs(dpFound.Location - signal.Location) - KmGap) * 1000);
                dangerPoint.Location = dpFound.Location - KmGap;
            }
            else if (dpFound.Location <= signal.Location)
            {
                dangerPoint.Distance = (int)((Math.Abs(dpFound.Location - signal.Location) + KmGap) * 1000);
                dangerPoint.Location = dpFound.Location + KmGap;
            }
            dangerPoint.Id = dpFound.Designation;
            dangerPoint.DistanceSpecified = true;
            return dangerPoint;
        }

        private ShiftCesBG GetCesBalise(Block signal, DirectionType direction)
        {
            ShiftCesBG shiftCesBG = null;
            List<Block> bgsFound = new List<Block>();
            List<TrackSegmentTmp> segNodes = this.TrackSegmentsTmp
                              .Where(x => x.Designation == signal.TrackSegId)
                              .ToList();
           
            if (segNodes.Count == 0)
            {
                ErrLogger.Error("Start Segment(s) not found", signal.Designation, "Oces Balise");
                ErrLogger.ErrorsFound = true;
                return null;
            }
            Stack<Stack<TrackSegmentTmp>> stackNodes = new Stack<Stack<TrackSegmentTmp>>();
            stackNodes.Push(new Stack<TrackSegmentTmp>(segNodes));
            int iterCount = 0;
            decimal KmGap = 0;
            decimal tmpSigLocation = signal.Location;
            TrackSegmentTmp prevSegment = null;
            while (stackNodes.Count > 0)
            {
                Block nextBg = null;
                if (prevSegment != null)
                {
                    string nextLineId = stackNodes.Peek().Peek().lineId;
                    if (prevSegment.lineId != nextLineId)
                    {
                        ErrLogger.Error("Line change between signal and oCes Bg", signal.Designation, "");
                        ErrLogger.ErrorsFound = true;
                        RailwayLine nextLine = this.RailwayLines
                                   .Where(x => x.designation == nextLineId)
                                   .FirstOrDefault();
                        if (nextLine != null)
                        {
                            direction = nextLine.direction;
                            tmpSigLocation += KmGap;
                        }
                    }
                }
                else
                {
                    tmpSigLocation = signal.Location;
                }
                if (direction == DirectionType.up)
                {
                    nextBg = this.blocks
                             .Where(x => x.XsdName == "BaliseGroup" &&
                                         x.Location <= tmpSigLocation &&
                                         x.TrackSegId == stackNodes.Peek().Peek().Designation)
                             .OrderByDescending(x => Convert.ToDecimal(x.Location))
                             .FirstOrDefault();
                }
                else
                {
                    nextBg = this.blocks
                             .Where(x => x.XsdName == "BaliseGroup" &&
                                         x.Location >= tmpSigLocation &&
                                         x.TrackSegId == stackNodes.Peek().Peek().Designation)
                             .OrderBy(x => Convert.ToDecimal(x.Location))
                             .FirstOrDefault();
                }
                prevSegment = stackNodes.Peek().Peek();
                if (nextBg != null)
                {
                    bgsFound.Add(nextBg);
                    do
                    {
                        if (stackNodes.Peek().Count == 2)
                        {
                            stackNodes.Peek().Pop();
                            prevSegment = stackNodes.Peek().Peek();
                            break;
                        }
                        stackNodes.Pop();
                        iterCount--;
                    }
                    while (stackNodes.Count > 0);
                }
                else
                {
                    if (direction == DirectionType.down)
                    {
                        if (stackNodes.Peek().Peek().Vertex2.XsdName == "Connector")
                        {
                            if (decimal.TryParse(stackNodes.Peek().Peek().Vertex2.Attributes["KMGAP"].Value, out decimal tmp))
                            {
                                if (!tmp.ToString().Contains('.'))
                                {
                                    tmp *= 0.001M;
                                }
                                KmGap += tmp * -1;
                            }
                            else
                            {
                                ErrLogger.Error("Unable to parse attribute value", stackNodes.Peek().Peek().Vertex2.Designation, "danger point");
                                ErrLogger.ErrorsFound = true;
                            }
                        }
                        else if (stackNodes.Peek().Peek().Vertex2.XsdName == "Point")
                        {
                            KmGap +=
                                GetPointHidConnValue(stackNodes.Peek().Peek().Vertex2, stackNodes.Peek().Peek().ConnV2, ConnectionBranchType.tip);
                        }
                        segNodes = this.TrackSegmentsTmp
                               .Where(x => (x != stackNodes.Peek().Peek()) && 
                                           (x.Vertex1 == stackNodes.Peek().Peek().Vertex2 ||
                                            x.Vertex2 == stackNodes.Peek().Peek().Vertex2))
                               .ToList();
                    }
                    else
                    {
                        if (stackNodes.Peek().Peek().Vertex1.XsdName == "Connector")
                        {
                            if (decimal.TryParse(stackNodes.Peek().Peek().Vertex1.Attributes["KMGAP"].Value, out decimal tmp))
                            {
                                if (!tmp.ToString().Contains('.'))
                                {
                                    tmp *= 0.001M;
                                }
                                KmGap += tmp * -1;
                            }
                            else
                            {
                                ErrLogger.Error("Unable to parse attribute value", stackNodes.Peek().Peek().Vertex1.Designation, "danger point");
                                ErrLogger.ErrorsFound = true;
                            }
                        }
                        else if (stackNodes.Peek().Peek().Vertex1.XsdName == "Point")
                        {
                            KmGap +=
                                GetPointHidConnValue(stackNodes.Peek().Peek().Vertex1, stackNodes.Peek().Peek().ConnV1, ConnectionBranchType.tip);
                        }
                        segNodes = this.TrackSegmentsTmp
                               .Where(x => (x != stackNodes.Peek().Peek()) &&
                                           (x.Vertex2 == stackNodes.Peek().Peek().Vertex1 ||
                                            x.Vertex1 == stackNodes.Peek().Peek().Vertex1))
                               .ToList();
                    }

                    iterCount++;
                    if (segNodes.Count == 0 || Constants.dpIterLimit == iterCount)
                    {
                        if (Constants.dpIterLimit == iterCount)
                        {
                            ErrLogger.Error("Iteration limit reached", signal.Designation, "Ces BG");
                        }
                        else
                        {
                            ErrLogger.Error("Segment(s) for next BG not found", signal.Designation, "Ces BG");
                        }
                        ErrLogger.ErrorsFound = true;
                        break;
                    }
                    else
                    {
                        stackNodes.Push(new Stack<TrackSegmentTmp>(segNodes));
                    }
                }
            }

            if (bgsFound.Count == 0)
            {
                ErrLogger.Error("Unable to Ces BG.", signal.Designation, "");
                ErrLogger.ErrorsFound = true;
                return shiftCesBG;
            }
            shiftCesBG = new ShiftCesBG();
            Block bgFound = null;
            if (bgsFound.Count == 1)
            {
                bgFound = bgsFound[0];
            }
            else
            {
                decimal max = bgsFound
                              .Max(x => Math.Abs(x.Location - signal.Location));
                bgFound = bgsFound
                          .Where(x => Math.Abs(x.Location - signal.Location) == max)
                          .FirstOrDefault();
            }
           
            if (signal.Location >= bgFound.Location)
            {
                shiftCesBG.Location = bgFound.Location - KmGap;
            }
            else if (signal.Location <= bgFound.Location)
            {
                shiftCesBG.Location = bgFound.Location + KmGap;
            }
            shiftCesBG.Id = bgFound.Designation;
            return shiftCesBG;
        }

        private decimal GetShiftCESLocationValue(decimal sigLocation, DangerPoint dangerPoint, ShiftCesBG cesBG)
        {
            double shift = 0.05 * Convert.ToDouble(Math.Abs(dangerPoint.Location - cesBG.Location) * 1000) - 
                                  Convert.ToDouble(Math.Abs(sigLocation - dangerPoint.Location) * 1000) + 16.0;
            decimal value = 0;
            if (shift > 0)
            {
                value = (int)Math.Ceiling(shift);
            }
            return value;
        }

        private decimal GetPointHidConnValue(Block point, ConnectionBranchType branchType1, ConnectionBranchType branchType2)
        {
            if (point.XsdName != "Point")
            {
                ErrLogger.Error("Wrong XSD type to get hidden connector", point.Designation, "");
                ErrLogger.ErrorsFound = true;
                return 0;
            }
            if (branchType1 != ConnectionBranchType.tip && 
                branchType1 != ConnectionBranchType.left && branchType1 != ConnectionBranchType.right)
            {
                ErrLogger.Error("Wrong branch1 type to get hidden connector", point.Designation, branchType1.ToString());
                ErrLogger.ErrorsFound = true;
                return 0;
            }
            if (branchType2 != ConnectionBranchType.tip &&
                branchType2 != ConnectionBranchType.left && branchType2 != ConnectionBranchType.right)
            {
                ErrLogger.Error("Wrong branch2 type to get hidden connector", point.Designation, branchType2.ToString());
                ErrLogger.ErrorsFound = true;
                return 0;
            }
            if (branchType1 == branchType2)
            {
                //ErrLogger.Error("Unable to get hidden connector - branches are equal", point.Designation, branchType1.ToString());
                //ErrLogger.ErrorsFound = true;
                return 0;
            }
            decimal kmgap = 0;
            decimal kmpSide = 0;
            if (!decimal.TryParse(point.Attributes["KMP"].Value, out decimal kmp))
            {
                ErrLogger.Error("Unable to parse attribute value", point.Designation, "KMP");
                ErrLogger.ErrorsFound = true;
                return kmgap;
            }

            if (branchType1 == ConnectionBranchType.right || branchType2 == ConnectionBranchType.right)
            {
                if (!decimal.TryParse(point.Attributes["KMP_CONTACT_2"].Value, out kmpSide))
                {
                    ErrLogger.Error("Unable to parse attribute value", point.Designation, "KMP_CONTACT_2");
                    ErrLogger.ErrorsFound = true;
                    return kmgap;
                }
            }
            else if (branchType1 == ConnectionBranchType.left || branchType2 == ConnectionBranchType.left)
            {
                if (!decimal.TryParse(point.Attributes["KMP_CONTACT_3"].Value, out kmpSide))
                {
                    ErrLogger.Error("Unable to parse attribute value", point.Designation, "KMP_CONTACT_3");
                    ErrLogger.ErrorsFound = true;
                    return kmgap;
                }
            }
            return kmp - kmpSide;
        }

        private List<RoutesRoute> GetDestSignals(SignalsSignal startSignal)
        {
            DirectionType direction = startSignal.Direction;
            List<RoutesRoute> routes = new List<RoutesRoute>();
            List<TrackSegmentTmp> segNodes = this.TrackSegmentsTmp
                              .Where(x => x.Designation == startSignal.TrackSegmentID)
                              .ToList();

            if (segNodes.Count == 0)
            {
                ErrLogger.Error("Start Segment(s) not found", startSignal.Designation, "Exp routes list");
                ErrLogger.ErrorsFound = true;
                return null;
            }
            Stack<Stack<TrackSegmentTmp>> stackNodes = new Stack<Stack<TrackSegmentTmp>>();
            Stack<RoutesRoutePointGroupPoint> points = new Stack<RoutesRoutePointGroupPoint>();
            stackNodes.Push(new Stack<TrackSegmentTmp>(segNodes));
            int iterCount = 0;
            
            decimal tmpSigLocation = startSignal.Location;
            while (stackNodes.Count > 0)
            {
                Block nextSig = null;
                bool error = false;
                bool lineChange = false;
                if (direction == DirectionType.up)
                {
                    nextSig = this.blocks
                             .Where(x => x.XsdName == "Signal" &&
                                         x.Designation != startSignal.Designation &&
                                         GetSignalDirection(x, ref error) == direction &&
                                         x.Location > tmpSigLocation &&
                                         x.TrackSegId == stackNodes.Peek().Peek().Designation)
                             .OrderBy(x => Convert.ToDecimal(x.Location))
                             .FirstOrDefault();
                    if (stackNodes.Peek().Peek().Vertex1.Location > tmpSigLocation &&
                        stackNodes.Peek().Peek().Vertex1.XsdName == "Point" &&
                                stackNodes.Peek().Peek().ConnV1 != ConnectionBranchType.tip)
                    {
                        RoutesRoutePointGroupPoint groupPoint = new RoutesRoutePointGroupPoint
                        {
                            Value = stackNodes.Peek().Peek().Vertex1.Designation,
                            RequiredPosition =
                            stackNodes.Peek().Peek().ConnV1 == ConnectionBranchType.right ? LeftRightType.right : LeftRightType.left
                        };
                        points.Push(groupPoint);
                    }
                }
                else if (direction == DirectionType.down)
                {
                    nextSig = this.blocks
                             .Where(x => x.XsdName == "Signal" &&
                                         x.Designation != startSignal.Designation &&
                                         GetSignalDirection(x, ref error) == direction &&
                                         x.Location < tmpSigLocation &&
                                         x.TrackSegId == stackNodes.Peek().Peek().Designation)
                             .OrderByDescending(x => Convert.ToDecimal(x.Location))
                             .FirstOrDefault();
                    if (stackNodes.Peek().Peek().Vertex2.Location < tmpSigLocation && 
                        stackNodes.Peek().Peek().Vertex2.XsdName == "Point" &&
                                stackNodes.Peek().Peek().ConnV2 != ConnectionBranchType.tip)
                    {
                        RoutesRoutePointGroupPoint groupPoint = new RoutesRoutePointGroupPoint
                        {
                            Value = stackNodes.Peek().Peek().Vertex2.Designation,
                            RequiredPosition =
                            stackNodes.Peek().Peek().ConnV2 == ConnectionBranchType.right ? LeftRightType.right : LeftRightType.left
                        };
                        points.Push(groupPoint);
                    }
                }
                bool eot = false;
                if (nextSig == null)
                {
                    eot = IsNextElEot(stackNodes.Peek().Peek(), direction, tmpSigLocation);
                    if (eot)
                    {
                        ErrLogger.Information("End of track found as route destination", "Start Signal: " + startSignal.Designation);
                    }
                }
                if (nextSig != null || eot)
                {
                    if (!eot)
                    {
                        RoutesRoute route = new RoutesRoute
                        {
                            Designation = startSignal.Designation + "_" + nextSig.Designation,
                            Start = startSignal.Designation,
                            Destination = nextSig.Designation,
                            PointGroup = new RoutesRoutePointGroup { Point = points.ToArray() }
                        };
                        routes.Add(route);
                    }                  
                    do
                    {
                        if (stackNodes.Peek().Count == 2)
                        {
                            if (points.Count > 0)
                            {
                                if (direction == DirectionType.up)
                                {
                                    if (points.Peek().Value == stackNodes.Peek().Peek().Vertex1.Designation)
                                    {
                                        points.Pop();
                                    }
                                }
                                else if (direction == DirectionType.down)
                                {
                                    if (points.Peek().Value == stackNodes.Peek().Peek().Vertex2.Designation)
                                    {
                                        points.Pop();
                                    }
                                }
                            }
                            if (IsLineChanging(stackNodes.Peek().Peek(), direction,
                                out DirectionType newDirBack, out decimal kmGapBack, out TrackSegmentTmp _, out Block _, true ))
                            {
                                tmpSigLocation -= kmGapBack;
                                direction = newDirBack;
                            }
                            stackNodes.Peek().Pop();
                            break;
                        }
                        if (points.Count > 0)
                        {
                            if (direction == DirectionType.up)
                            {
                                if (points.Peek().Value == stackNodes.Peek().Peek().Vertex1.Designation)
                                {
                                    points.Pop();
                                }
                            }
                            else if (direction == DirectionType.down)
                            {
                                if (points.Peek().Value == stackNodes.Peek().Peek().Vertex2.Designation)
                                {
                                    points.Pop();
                                }
                            }
                        }
                        if (IsLineChanging(stackNodes.Peek().Peek(), direction, 
                            out DirectionType newDirBack1, out decimal kmGapBack1, out TrackSegmentTmp _, out Block _, true))
                        {
                            tmpSigLocation -= kmGapBack1;
                            direction = newDirBack1;
                        }
                        stackNodes.Pop();
                        iterCount--;
                    }
                    while (stackNodes.Count > 0);
                }
                else
                {
                    segNodes = new List<TrackSegmentTmp>();
                    if (IsLineChanging(stackNodes.Peek().Peek(), direction, out DirectionType newDir, out decimal kmGap, out TrackSegmentTmp nextSeg, out Block changeVertex))
                    {
                        tmpSigLocation += kmGap;
                        lineChange = true;
                        segNodes.Add(nextSeg);
                        ErrLogger.Error("Create routes: Line change on route path. Should be rechecked manually.", startSignal.Designation, nextSeg.Designation);
                        ErrLogger.ErrorsFound = true;
                    }
                    
                    if (direction == DirectionType.up)
                    {
                        if (stackNodes.Peek().Peek().ConnV2 == ConnectionBranchType.tip)
                        {
                            segNodes.AddRange(this.TrackSegmentsTmp
                                       .Where(x => (x != stackNodes.Peek().Peek()) &&
                                                   (x != nextSeg) &&
                                                   (x.Vertex1 == stackNodes.Peek().Peek().Vertex2))
                                       .ToList());
                        }
                        else
                        {
                            segNodes.AddRange(this.TrackSegmentsTmp
                                       .Where(x => (x != stackNodes.Peek().Peek()) &&
                                                   (x != nextSeg) &&
                                                   (x.Vertex1 == stackNodes.Peek().Peek().Vertex2 && (x.ConnV1 == ConnectionBranchType.tip || x.ConnV1 == ConnectionBranchType.none) ||
                                                    x.Vertex2 == stackNodes.Peek().Peek().Vertex2 && (x.ConnV2 == ConnectionBranchType.tip || x.ConnV2 == ConnectionBranchType.none)))
                                       .ToList());
                        }
                    }
                    else if (direction == DirectionType.down)
                    {
                        if (stackNodes.Peek().Peek().ConnV1 == ConnectionBranchType.tip)
                        {
                            segNodes.AddRange(this.TrackSegmentsTmp
                                       .Where(x => (x != stackNodes.Peek().Peek()) &&
                                                   (x != nextSeg) &&
                                                   (x.Vertex2 == stackNodes.Peek().Peek().Vertex1))
                                       .ToList());
                        }
                        else
                        {
                            segNodes.AddRange(this.TrackSegmentsTmp
                                  .Where(x => (x != stackNodes.Peek().Peek()) &&
                                              (x != nextSeg) &&
                                              (x.Vertex1 == stackNodes.Peek().Peek().Vertex1 && (x.ConnV1 == ConnectionBranchType.tip || x.ConnV1 == ConnectionBranchType.none) ||
                                               x.Vertex2 == stackNodes.Peek().Peek().Vertex1 && (x.ConnV2 == ConnectionBranchType.tip || x.ConnV2 == ConnectionBranchType.none)))
                                  .ToList());
                        }

                    }
                    if (lineChange)
                    {
                        direction = newDir;
                    }
                    segNodes = segNodes
                               .OrderBy(x => x == nextSeg)
                               .ToList();
                    iterCount++;
                    if (segNodes.Count == 0)
                    {
                        if (points.Count > 0)
                        {
                            if (direction == DirectionType.up)
                            {
                                if (points.Peek().Value == stackNodes.Peek().Peek().Vertex1.Designation)
                                {
                                    points.Pop();
                                }
                            }
                            else if (direction == DirectionType.down)
                            {
                                if (points.Peek().Value == stackNodes.Peek().Peek().Vertex2.Designation)
                                {
                                    points.Pop();
                                }
                            }
                        }
                        stackNodes.Peek().Pop();
                        if (stackNodes.Peek().Count == 0)
                        {
                            stackNodes.Pop();
                        }
                        if (stackNodes.Count == 0)
                        {
                            ErrLogger.Error("Segment(s) for next Signal not found", startSignal.Designation, "Exp routes list");
                            ErrLogger.ErrorsFound = true;
                            break;
                        }
                        iterCount--;
                    }
                    if (/*segNodes.Count == 0 ||*/ Constants.sigIterLimit == iterCount)
                    {
                        if (Constants.sigIterLimit == iterCount)
                        {
                            ErrLogger.Error("Iteration limit reached", startSignal.Designation, "Exp routes list");
                        }
                        ErrLogger.ErrorsFound = true;
                        break;
                    }
                    else
                    {
                        if (segNodes.Count > 0)
                        {
                            stackNodes.Push(new Stack<TrackSegmentTmp>(segNodes));
                        }                     
                    }
                }
            }
            return routes;
        }

        private bool IsLineChanging(TrackSegmentTmp segmentTmp, DirectionType direction, 
            out DirectionType newDirection, out decimal kmGap, out TrackSegmentTmp changedSeg, out Block changeVertex, bool back = false)
        {
            changedSeg = null;
            kmGap = 0;
            if (back)
            {
                direction = ReverseDirection(direction);
            }
            newDirection = direction;
            bool result = false;
            TrackSegmentTmp nextSeg = null;
            changeVertex = null;
            Block opositChangeVertex = null;
            Block opositNativeVertex = null;
            ConnectionBranchType connV1 = ConnectionBranchType.none;
            ConnectionBranchType connV2 = ConnectionBranchType.none;
            DirectionType nextSegDirection = DirectionType.up;
            if (direction == DirectionType.up)
            {
                if (segmentTmp.ConnV2 == ConnectionBranchType.tip)
                {
                    nextSeg = this.TrackSegmentsTmp
                          .Where(x => (x.Vertex1 == segmentTmp.Vertex2 ||
                                       x.Vertex2 == segmentTmp.Vertex2) &&
                                       x.lineId != segmentTmp.lineId)
                          .FirstOrDefault();
                }
                else
                {
                    nextSeg = this.TrackSegmentsTmp
                          .Where(x => ((x.Vertex1 == segmentTmp.Vertex2 && (x.ConnV1 == ConnectionBranchType.tip || x.ConnV1 == ConnectionBranchType.none)) ||
                                       (x.Vertex2 == segmentTmp.Vertex2 && (x.ConnV2 == ConnectionBranchType.tip || x.ConnV2 == ConnectionBranchType.none))) &&
                                       x.lineId != segmentTmp.lineId)
                          .FirstOrDefault();
                }
                changeVertex = segmentTmp.Vertex2;
                if (nextSeg != null)
                {
                    if (nextSeg.Vertex2 == changeVertex)
                    {
                        opositChangeVertex = nextSeg.Vertex1;
                        connV1 = segmentTmp.ConnV2;
                        connV2 = nextSeg.ConnV2;
                    }
                    else if (nextSeg.Vertex1 == changeVertex)
                    {
                        opositChangeVertex = nextSeg.Vertex2;
                        connV1 = segmentTmp.ConnV2;
                        connV2 = nextSeg.ConnV1;
                    }
                }              
                opositNativeVertex = segmentTmp.Vertex1;
                
            }
            else if (direction == DirectionType.down)
            {
                if (segmentTmp.ConnV1 == ConnectionBranchType.tip)
                {
                    nextSeg = this.TrackSegmentsTmp
                          .Where(x => (x.Vertex1 == segmentTmp.Vertex1 ||
                                       x.Vertex2 == segmentTmp.Vertex1) &&
                                       x.lineId != segmentTmp.lineId)
                          .FirstOrDefault();
                }
                else
                {
                    nextSeg = this.TrackSegmentsTmp
                          .Where(x => ((x.Vertex1 == segmentTmp.Vertex1 && (x.ConnV1 == ConnectionBranchType.tip || x.ConnV1 == ConnectionBranchType.none)) ||
                                       (x.Vertex2 == segmentTmp.Vertex1 && (x.ConnV2 == ConnectionBranchType.tip || x.ConnV2 == ConnectionBranchType.none))) &&
                                       x.lineId != segmentTmp.lineId)
                          .FirstOrDefault();
                }

                changeVertex = segmentTmp.Vertex1;
                if (nextSeg != null)
                {
                    if (nextSeg.Vertex2 == changeVertex)
                    {
                        opositChangeVertex = nextSeg.Vertex1;
                        connV1 = segmentTmp.ConnV1;
                        connV2 = nextSeg.ConnV2;
                    }
                    else if (nextSeg.Vertex1 == changeVertex)
                    {
                        opositChangeVertex = nextSeg.Vertex2;
                        connV1 = segmentTmp.ConnV1;
                        connV2 = nextSeg.ConnV1;
                    }
                }            
                opositNativeVertex = segmentTmp.Vertex2;
                
            }
            if (nextSeg == null)
            {
                result = false;
            }
            else
            {
                changedSeg = nextSeg;
               
                if (changeVertex.XsdName == "Connector")
                {
                    if (decimal.TryParse(changeVertex.Attributes["KMGAP"].Value, out decimal tmp))
                    {
                        if (!tmp.ToString().Contains('.'))
                        {
                            tmp *= 0.001M;
                        }
                        kmGap += tmp * -1;
                    }
                    else
                    {
                        ErrLogger.Error("Unable to parse attribute value", changeVertex.Designation, "change Line");
                        ErrLogger.ErrorsFound = true;
                    }
                }
                else if (changeVertex.XsdName == "Point")
                {
                    kmGap +=
                        GetPointHidConnValue(changeVertex, connV1, connV2);
                }
                if (changeVertex.Location + kmGap < opositChangeVertex.Location)
                {
                    nextSegDirection = DirectionType.up;
                }
                else if (changeVertex.Location + kmGap > opositChangeVertex.Location)
                {
                    nextSegDirection = DirectionType.down;
                }

                if (direction != nextSegDirection)
                {
                    if (direction == DirectionType.up)
                    {
                        newDirection = DirectionType.down;
                    }
                    else if (direction == DirectionType.down)
                    {
                        newDirection = DirectionType.up;
                    }
                }
                if (back)
                {
                    newDirection = ReverseDirection(newDirection);
                }
                result = true;
            }
            return result;
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

        private bool IsNextElEot(TrackSegmentTmp segment, DirectionType direction, decimal refLocation)
        {
            Block eot = null;
            bool error = false;
            if (direction == DirectionType.up)
            {
                var eots = this.blocks
                         .Where(x => x.XsdName == "EndOfTrack" &&
                                     x.Location > refLocation &&
                                     x.TrackSegId == segment.Designation)
                         .OrderBy(x => Convert.ToDecimal(x.Location))
                         .ToList();
                eot = eots
                      .Where(x => GetEotDirection(x, ref error) == direction)
                      .FirstOrDefault();
            }
            else if (direction == DirectionType.down)
            {
                var eots = this.blocks
                         .Where(x => x.XsdName == "EndOfTrack" &&
                                     x.Location < refLocation &&
                                     x.TrackSegId == segment.Designation)
                         .OrderBy(x => Convert.ToDecimal(x.Location))
                         .ToList();
                eot = eots
                      .Where(x => GetEotDirection(x, ref error) == direction)
                      .FirstOrDefault();
            }
            return eot != null;
        }

        private void GetDangerPointOld(Block BlkSignal, ref SignalsSignal signal,
                                     List<Block> blocks, ref bool error,
                                     ref List<string> overPts, ref Block blockTdt)
        {
            if (signal.TrackPosition == LeftRightOthersType.others)
            {
                signal.DangerPointDistance = 0;
                return;
            }
            Block BlkDangerPt = null;
            //decimal SigLocation = signal.Location;
            decimal KmGap = 0;
            //double SigY = BlkSignal.Y;
            TrackSegmentTmp SigSegment = TrackSegmentsTmp
                                         .Where(x => x.Designation == BlkSignal.TrackSegId)
                                         .FirstOrDefault();
            if (SigSegment == null)
            {
                error = true;
                ErrLogger.Error("Track Segment not found", signal.Designation , "danger point");
                return;
            }


            FindNextAc(ref BlkDangerPt, signal, SigSegment, blocks, blockTdt);

            if (BlkDangerPt != null)
            {
                List<TrackSegmentTmp> tmpSegments = new List<TrackSegmentTmp>
                {
                    TrackSegmentsTmp
                        .Where(x => x.Designation == BlkSignal.TrackSegId)
                        .FirstOrDefault()
                };
                if (BlkSignal.TrackSegId != BlkDangerPt.TrackSegId)
                {
                    TrackSegmentTmp firstSeg = TrackSegmentsTmp
                        .Where(x => x.Designation == BlkSignal.TrackSegId)
                        .FirstOrDefault();
                    TrackSegmentTmp lastSeg = TrackSegmentsTmp
                        .Where(x => x.Designation == BlkDangerPt.TrackSegId)
                        .FirstOrDefault();
                    TrackSegmentTmp nextSegment = firstSeg;
                    int iterationlimit = 0;
                    while (nextSegment != lastSeg)
                    {
                        if (signal.Direction == DirectionType.down)
                        {
                            if (overPts != null && overPts.Count > 0)
                            {
                                TrackSegmentTmp TmpNextSegment = null;
                                bool pointSeg = false;
                                foreach (string overpt in overPts)
                                {
                                    if (nextSegment.Vertex1.Designation.Contains(overpt.Split('-').First()))
                                    {
                                        TmpNextSegment = TrackSegmentsTmp
                                                 .Where(x => x.Vertex2 == nextSegment.Vertex1)
                                                 .FirstOrDefault();
                                        VertexType overPtVertex = trcksegments
                                                                  .Where(x => x.Vertex1.vertexID == nextSegment.Vertex1.Designation &&
                                                                              x.Designation == nextSegment.Designation)
                                                                  .Select(x => x.Vertex1)
                                                                  .FirstOrDefault();
                                        if (overPtVertex != null)
                                        {
                                            if (overPtVertex.connection == ConnectionBranchType.right && nextSegment.Vertex1.Location2 != 0)
                                            {
                                                KmGap += nextSegment.Vertex1.Location2 - nextSegment.Vertex1.Location;
                                            }
                                            else if (overPtVertex.connection == ConnectionBranchType.left && nextSegment.Vertex1.Location3 != 0)
                                            {
                                                KmGap += (nextSegment.Vertex1.Location3 - nextSegment.Vertex1.Location) * 1000;
                                            }
                                        }
                                        pointSeg = true;
                                        break;
                                    }
                                }
                                if (TmpNextSegment == null)
                                {
                                    ErrLogger.Error("Unable to calculate segments path",
                                        signal.Designation, "Danger Point");
                                    error = true;
                                    break;
                                }

                                if (TmpNextSegment.Vertex2.XsdName == "Point")
                                {
                                    //bool pointSeg = overPts
                                    //    .Contains(TmpNextSegment.Designation
                                    //              .Replace("spsk-" + stationID + "-", ""));
                                    if (pointSeg)
                                    {
                                        nextSegment = TmpNextSegment;
                                        //Logger.Log(signal.Designation + ": Overlap via points '" + blckProp.GetElemDesignation(blockTdt) + "'");
                                    }
                                    else
                                    {
                                        TmpNextSegment = TrackSegmentsTmp
                                                         .Where(x => x.Vertex2 == nextSegment.Vertex1 &&
                                                         x.Designation != TmpNextSegment.Designation)
                                                         .FirstOrDefault();
                                        pointSeg = overPts
                                                  .Contains(TmpNextSegment.Designation
                                                  .Replace("spsk-" + stationID + "-", ""));
                                        if (pointSeg)
                                        {
                                            nextSegment = TmpNextSegment;
                                            //Logger.Log(signal.Designation + ": Overlap via points '" + blckProp.GetElemDesignation(blockTdt) + "'");
                                        }
                                        else
                                        {
                                            ErrLogger.Error("Unable to calculate segments path",
                                                    signal.Designation,  "danger point");
                                            error = true;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    nextSegment = TrackSegmentsTmp
                                      .Where(x => x.Vertex2 == nextSegment.Vertex1)
                                      .FirstOrDefault();
                                }
                            }
                            else
                            {
                                nextSegment = TrackSegmentsTmp
                                      .Where(x => x.Vertex2 == nextSegment.Vertex1)
                                      .FirstOrDefault();
                            }
                        }
                        else if (signal.Direction == DirectionType.up)
                        {
                            if (overPts != null && overPts.Count > 0)
                            {
                                TrackSegmentTmp TmpNextSegment = null;
                                bool pointSeg = false;
                                foreach (string overpt in overPts)
                                {
                                    if (nextSegment.Vertex2.Designation.Contains(overpt.Split('-').First()))
                                    {
                                        TmpNextSegment = TrackSegmentsTmp
                                                 .Where(x => x.Vertex1 == nextSegment.Vertex2)
                                                 .FirstOrDefault();
                                        VertexType overPtVertex = trcksegments
                                                                  .Where(x => x.Vertex2.vertexID == nextSegment.Vertex2.Designation &&
                                                                              x.Designation == nextSegment.Designation)
                                                                  .Select(x => x.Vertex2)
                                                                  .FirstOrDefault();
                                        if (overPtVertex != null)
                                        {
                                            if (overPtVertex.connection == ConnectionBranchType.right && nextSegment.Vertex2.Location2 != 0)
                                            {
                                                KmGap += nextSegment.Vertex2.Location2 - nextSegment.Vertex2.Location;
                                            }
                                            else if (overPtVertex.connection == ConnectionBranchType.left && nextSegment.Vertex2.Location3 != 0)
                                            {
                                                KmGap += (nextSegment.Vertex2.Location3 - nextSegment.Vertex2.Location) * 1000;
                                            }
                                        }
                                        pointSeg = true;
                                        break;
                                    }
                                }

                                if (TmpNextSegment == null)
                                {
                                    ErrLogger.Error("Unable to calculate segments path",
                                                    signal.Designation, "danger point");
                                    error = true;
                                    break;
                                }

                                if (TmpNextSegment.Vertex1.XsdName == "Point")
                                {
                                    //bool pointSeg = overPts
                                    //    .Contains(TmpNextSegment.Designation
                                    //              .Replace("spsk-" + stationID + "-", ""));
                                    if (pointSeg)
                                    {
                                        nextSegment = TmpNextSegment;
                                        //Logger.Log(signal.Designation + ": Overlap via points '" + blckProp.GetElemDesignation(blockTdt) + "'");
                                    }
                                    else
                                    {
                                        TmpNextSegment = TrackSegmentsTmp
                                                         .Where(x => x.Vertex1 == nextSegment.Vertex2 &&
                                                         x.Designation != TmpNextSegment.Designation)
                                                         .FirstOrDefault();
                                        pointSeg = overPts
                                                  .Contains(TmpNextSegment.Designation
                                                  .Replace("spsk-" + stationID + "-", ""));
                                        if (pointSeg)
                                        {
                                            nextSegment = TmpNextSegment;
                                            //Logger.Log(signal.Designation + ": Overlap via points '" + blckProp.GetElemDesignation(blockTdt) + "'");
                                        }
                                        else
                                        {
                                            ErrLogger.Error("Unable to calculate segments path",
                                                    signal.Designation, "danger point");
                                            error = true;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    nextSegment = TrackSegmentsTmp
                                      .Where(x => x.Vertex1 == nextSegment.Vertex2)
                                      .FirstOrDefault();
                                }
                            }
                            else
                            {
                                nextSegment = TrackSegmentsTmp
                                      .Where(x => x.Vertex1 == nextSegment.Vertex2)
                                      .FirstOrDefault();
                            }
                        }

                        if (nextSegment == null)
                        {
                            ErrLogger.Error("Unable to calculate segments path",
                                signal.Designation, "danger point");
                            error = true;
                            break;
                        }
                        tmpSegments.Add(nextSegment);
                        iterationlimit++;
                        if (iterationlimit >= Constants.nextNodeMaxAttemps)
                        {
                            ErrLogger.Error("Danger point track segments limit reached",
                                blckProp.GetElemDesignation(BlkSignal), "danger point");
                            error = true;
                            break;
                        }
                    }
                }
                List<Block> Connectors = tmpSegments
                    .SelectMany(y => y.BlocksOnSegments)
                    .Where(y => y.XsdName == "Connector" &&
                                Calc.Between(y.Location, BlkSignal.Location, BlkDangerPt.Location))
                    .Distinct()
                    .ToList();
                foreach (Block dpConn in Connectors)
                {
                    KmGap += (Convert.ToDecimal(dpConn.Attributes["KMGAP"].Value) * -1);
                }

                signal.DangerPointID = blckProp.GetElemDesignation(BlkDangerPt);
                signal.DangerPointDistance = Math.Round(
                    Math.Abs(signal.Location - BlkDangerPt.Location) * 1000 + KmGap, 0);
            }
            else
            {
                error = true;
                ErrLogger.Error("Danger Point not found", signal.Designation, "");
            }
        }

        private bool FindNextAc(ref Block Ac, SignalsSignal signal,
                                 TrackSegmentTmp startSeg, List<Block> blocks, Block blockTdt = null)
        {
            List<Block> tmpDps = null;
            Block AcLast = null;
            /* If Signal has extended overlap then find the farthest axle counter
               of overlap tdt */
            if (blockTdt != null)
            {
                Regex dpAttsPatt = new Regex(@"^DP{1}\d+");
                Regex patternDP = new Regex("^at-[0-9]{3}|^[0-9]{3}|^at-" + stationID.ToLower() + "-[0-9]{3}");

                string[] DPs = blockTdt.Attributes
                             .Where(x => (dpAttsPatt.IsMatch(x.Value.Name) &&
                                          patternDP.IsMatch(x.Value.Value) &&
                                          !String.IsNullOrEmpty(x.Value.Value.ToString())))
                             .Select(x => ("at-" + stationID.ToLower() + "-" + x.Value.Value.Split('-').Last())
                             .Trim())
                             .ToArray();
                string[] DPsForeign = blockTdt.Attributes
                             .Where(x => (dpAttsPatt.IsMatch(x.Value.Name) &&
                                          Regex.IsMatch(x.Value.Value, "^at-[a-zæøåÆØÅ]{2,3}-[0-9]{3}") &&
                                          !String.IsNullOrEmpty(x.Value.Value.ToString())))
                             .Select(x => x.Value.Value.Trim())
                             .ToArray();
                DPs = DPs.Union(DPsForeign).ToArray();

                tmpDps = blocks
                                            .Where(x => DPs.ToList()
                                            .Contains(blckProp.GetElemDesignation(x)) &&
                                            x.XsdName == "DetectionPoint")
                                            .OrderBy(x => x.X)
                                            .Distinct()
                                            .ToList();
                if (signal.Direction == DirectionType.up)
                {
                    AcLast = tmpDps
                                .Where(x => x.Location == tmpDps.Max(l => l.Location))
                                .FirstOrDefault();
                }
                else if (signal.Direction == DirectionType.down)
                {
                    AcLast = tmpDps
                                .Where(x => x.Location == tmpDps.Min(l => l.Location))
                                .FirstOrDefault();
                }
                if (AcLast != null)
                {
                    Ac = AcLast;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            /* For Signals without extended overlaps find the nearest axle counter
               behind signal*/
            TrackSegmentTmp nextSeg = startSeg;
            int iterationlimit = 0;
            List<Block> excludAcs = new List<Block>();
            while (Ac == null && iterationlimit <= Constants.nextNodeMaxAttemps)
            {
                iterationlimit++;
                if (signal.TrackPosition == LeftRightOthersType.others)
                {
                    Ac = blocks
                     .Where(x => x.XsdName == "DetectionPoint" &&
                                 x.TrackSegId == nextSeg.Designation &&
                                 !excludAcs.Contains(x))
                     .OrderBy(item => Math.Abs(signal.Location - item.Location))
                     .FirstOrDefault();
                    if (Ac == null)
                    {
                        nextSeg = TrackSegmentsTmp
                              .Where(x => x.Vertex1 == nextSeg.Vertex2)
                              .FirstOrDefault();
                    }
                }
                else if (signal.Direction == DirectionType.up)
                {
                    Ac = blocks
                     .Where(x => x.XsdName == "DetectionPoint" &&
                                 x.TrackSegId == nextSeg.Designation &&
                                 x.Location >= signal.Location &&
                                 !excludAcs.Contains(x))
                     .OrderBy(item => Math.Abs(signal.Location - item.Location))
                     .FirstOrDefault();
                    if (tmpDps != null)
                    {
                        //if (tmpDps.Contains(Ac))
                        //{
                        //    Ac = tmpDps
                        //        .Where(x => x.Location == tmpDps.Max(l => l.Location))
                        //        .FirstOrDefault();
                        //}
                        if (Ac != AcLast)
                        {
                            excludAcs.Add(Ac);
                            Ac = null;
                        }
                    }
                    if (Ac == null)
                    {
                        if (AcLast != null)
                        {
                            if (nextSeg.Designation != AcLast.TrackSegId)
                            {
                                nextSeg = TrackSegmentsTmp
                              .Where(x => x.Vertex1 == nextSeg.Vertex2)
                              .FirstOrDefault();
                            }
                        }
                        else
                        {
                            nextSeg = TrackSegmentsTmp
                              .Where(x => x.Vertex1 == nextSeg.Vertex2)
                              .FirstOrDefault();
                        }
                    }
                }
                else if (signal.Direction == DirectionType.down)
                {
                    Ac = blocks
                     .Where(x => x.XsdName == "DetectionPoint" &&
                                 x.TrackSegId == nextSeg.Designation &&
                                 x.Location <= signal.Location &&
                                 !excludAcs.Contains(x))
                     .OrderBy(item => Math.Abs(signal.Location - item.Location))
                     .FirstOrDefault();

                    if (tmpDps != null)
                    {
                        if (Ac != AcLast)
                        {
                            excludAcs.Add(Ac);
                            Ac = null;
                        }
                    }
                    if (Ac == null)
                    {

                        if (AcLast != null)
                        {
                            if (nextSeg.Designation != AcLast.TrackSegId)
                            {
                                nextSeg = TrackSegmentsTmp
                              .Where(x => x.Vertex2 == nextSeg.Vertex1)
                              .FirstOrDefault();
                            }
                        }
                        else
                        {
                            nextSeg = TrackSegmentsTmp
                              .Where(x => x.Vertex2 == nextSeg.Vertex1)
                              .FirstOrDefault();
                        }
                    }
                }
                if (nextSeg == null)
                {
                    return false;
                }
            }
            if (Ac != null)
            {
                return true;
            }
            return false;
        }

        private void GetSigClosure(Block BlkSignal, SignalsSignal signal, List<Block> blocks)
        {
            if (signal.KindOfSignal == TKindOfSignal.eotmb)
            {
                ExportCigClosure.Add(BlkSignal.Attributes["NAME"].Value + "\t" +
                                                 Math.Round(signal.Location * 1000, 0) + "\t" +
                                                 "" + "\t" +
                                                 "0" + "\t" +
                                                 "" + "\t" +
                                                 "0" + '\t' +
                                                 signal.KindOfSignal.ToString().ToUpper());
                return;
            }
            if (signal.KindOfSignal == TKindOfSignal.L2EntrySignal || signal.KindOfSignal == TKindOfSignal.L2ExitSignal)
            {
                ExportCigClosure.Add(BlkSignal.Attributes["NAME"].Value + "\t" +
                                                                 Math.Round(signal.Location * 1000, 0) + "\t" +
                                                                 "N/A" + "\t" +
                                                                 "0" + "\t" +
                                                                 "N/A" + "\t" +
                                                                 "0" + '\t' +
                                                                 signal.KindOfSignal.ToString());
                return;
            }
            Block Ac = null;
            Block Bg = null;
            TrackSegmentTmp SigSegment = TrackSegmentsTmp
                                         .Where(x => x.Designation == BlkSignal.TrackSegId)
                                         .FirstOrDefault();
            TrackSegmentTmp AcSegment = SigSegment;
            decimal KmGapAc = 0;
            decimal KmGapBg = 0;
            List<TrackSegmentTmp> tmpSegmentsAc = new List<TrackSegmentTmp>
                {
                    TrackSegmentsTmp
                        .Where(x => x.Designation == BlkSignal.TrackSegId)
                        .FirstOrDefault()
                };
            List<TrackSegmentTmp> tmpSegmentsBg = new List<TrackSegmentTmp>
                {
                    TrackSegmentsTmp
                        .Where(x => x.Designation == BlkSignal.TrackSegId)
                        .FirstOrDefault()
                };
            if (signal.Direction == DirectionType.up)
            {
                Ac = blocks
                .Where(x => x.XsdName == "DetectionPoint" &&
                            x.TrackSegId == BlkSignal.TrackSegId &&
                            x.Location >= BlkSignal.Location)
                .OrderBy(x => x.Location)
                .FirstOrDefault();
                if (Ac == null)
                {
                    TrackSegmentTmp NextSeg = SigSegment;
                    int counter = 0;
                    while (Ac == null && counter < Constants.nextNodeMaxAttemps && NextSeg != null)
                    {
                        counter++;
                        NextSeg = TrackSegmentsTmp
                                  .Where(x => x.Vertex1 == NextSeg.Vertex2)
                                  .FirstOrDefault();
                        if (NextSeg != null)
                        {
                            if (NextSeg.Vertex1.XsdName == "Point")
                            {
                                ErrLogger.Information("Closure Ac via point " + blckProp.GetElemDesignation(NextSeg.Vertex1), signal.Designation);
                            }
                            tmpSegmentsAc.Add(NextSeg);
                            AcSegment = NextSeg;
                            Ac = blocks
                                .Where(x => x.XsdName == "DetectionPoint" &&
                                            x.TrackSegId == NextSeg.Designation &&
                                            x.Location >= BlkSignal.Location)
                                .OrderBy(x => x.Location)
                                .FirstOrDefault();
                        }
                    }
                }
                if (Ac == null)
                {
                    ExportCigClosure.Add(BlkSignal.Attributes["NAME"].Value + "\t" +
                                 Math.Round(signal.Location * 1000, 0) + "\t" +
                                 "not found" + "\t" +
                                 "error" + "\t" +
                                 "not found" + "\t" +
                                 "error" + '\t');
                    return;
                }
                Bg = blocks
                    .Where(x => x.XsdName == "BaliseGroup" &&
                                x.TrackSegId == BlkSignal.TrackSegId &&
                                x.Location <= Ac.Location)
                    .OrderByDescending(x => x.Location)
                    .FirstOrDefault();
                if (Bg == null)
                {
                    TrackSegmentTmp NextSeg = AcSegment;
                    TrackSegmentTmp NextTmp = NextSeg;
                    int counter = 0;
                    while (Bg == null && counter < Constants.nextNodeMaxAttemps && NextSeg != null)
                    {
                        counter++;
                        NextSeg = TrackSegmentsTmp
                                  .Where(x => x.Vertex2 == NextSeg.Vertex1)
                                  .FirstOrDefault();
                        if (NextSeg == null)
                        {
                            NextSeg = TrackSegmentsTmp
                                  .Where(x => x.Vertex1 == NextTmp.Vertex1 && x != NextTmp)
                                  .FirstOrDefault();
                        }
                        //List<TrackSegmentTmp> test = TrackSegmentsTmp
                        //.Where(x => x.Vertex2 == NextTmp.Vertex1)
                        //.ToList();
                        if (NextSeg != null)
                        {
                            if (NextSeg.Vertex2.XsdName == "Point")
                            {
                                ErrLogger.Information("Closure BG via point " + blckProp.GetElemDesignation(NextSeg.Vertex2), signal.Designation );
                                if (NextSeg.Designation.Split('-').Last() == "T")
                                {
                                    tmpSegmentsBg.Add(NextSeg);
                                    Bg = blocks
                                        .Where(x => x.XsdName == "BaliseGroup" &&
                                                    x.TrackSegId == NextSeg.Designation &&
                                                    x.Location <= BlkSignal.Location)
                                        .OrderByDescending(x => x.Location)
                                        .FirstOrDefault();
                                }
                                else if (GetPointBranch(NextSeg.Vertex2) == ConnectionBranchType.right && NextSeg.Designation.Split('-')[1] == "L")
                                {
                                    tmpSegmentsBg.Add(NextSeg);
                                    Bg = blocks
                                        .Where(x => x.XsdName == "BaliseGroup" &&
                                                    x.TrackSegId == BlkSignal.TrackSegId &&
                                                    x.Location <= BlkSignal.Location)
                                        .OrderByDescending(x => x.Location)
                                        .FirstOrDefault();
                                }
                                else if (GetPointBranch(NextSeg.Vertex2) == ConnectionBranchType.left && NextSeg.Designation.Split('-')[1] == "R")
                                {
                                    tmpSegmentsBg.Add(NextSeg);
                                    Bg = blocks
                                        .Where(x => x.XsdName == "BaliseGroup" &&
                                                    x.TrackSegId == BlkSignal.TrackSegId &&
                                                    x.Location <= BlkSignal.Location)
                                        .OrderByDescending(x => x.Location)
                                        .FirstOrDefault();
                                }
                                else
                                {

                                    if (TrackSegmentsTmp.Where(x => x.Vertex2 == NextTmp.Vertex1).Count() < 2)
                                    {
                                        ErrLogger.Error("Cannot find CES Bg", signal.Designation, "");
                                        break;
                                    }
                                    else
                                    {
                                        if (NextSeg.Designation.Split('-')[1] == "T")
                                        {
                                            tmpSegmentsBg.Add(NextSeg);
                                            Bg = blocks
                                                .Where(x => x.XsdName == "BaliseGroup" &&
                                                            x.TrackSegId == NextSeg.Designation &&
                                                            x.Location <= BlkSignal.Location)
                                                .OrderByDescending(x => x.Location)
                                                .FirstOrDefault();
                                        }
                                    }

                                    NextSeg = TrackSegmentsTmp
                                              .Where(x => x.Vertex2 == NextTmp.Vertex1)
                                              .ElementAt(1);
                                    tmpSegmentsBg.Add(NextSeg);
                                    Bg = blocks
                                        .Where(x => x.XsdName == "BaliseGroup" &&
                                                    x.TrackSegId == NextSeg.Designation &&
                                                    x.Location <= BlkSignal.Location)
                                        .OrderByDescending(x => x.Location)
                                        .FirstOrDefault();
                                }
                            }
                            else
                            {
                                tmpSegmentsBg.Add(NextSeg);
                                Bg = blocks
                                    .Where(x => x.XsdName == "BaliseGroup" &&
                                                x.TrackSegId == NextSeg.Designation &&
                                                x.Location <= BlkSignal.Location)
                                    .OrderByDescending(x => x.Location)
                                    .FirstOrDefault();
                            }
                        }
                    }
                }
            }
            else if (signal.Direction == DirectionType.down)
            {
                Ac = blocks
                .Where(x => x.XsdName == "DetectionPoint" &&
                            x.TrackSegId == BlkSignal.TrackSegId &&
                            x.Location <= BlkSignal.Location)
                .OrderByDescending(x => x.Location)
                .FirstOrDefault();
                if (Ac == null)
                {
                    TrackSegmentTmp NextSeg = SigSegment;
                    int counter = 0;
                    while (Ac == null && counter < Constants.nextNodeMaxAttemps && NextSeg != null)
                    {
                        counter++;
                        //NextSeg = TrackSegmentsTmp
                        //          .Where(x => x.Vertex2 == NextSeg.Vertex1 && x.Vertex2.XsdName == "Connector")
                        //          .FirstOrDefault();
                        NextSeg = TrackSegmentsTmp
                                  .Where(x => x.Vertex2 == NextSeg.Vertex1)
                                  .FirstOrDefault();
                        if (NextSeg != null)
                        {
                            if (NextSeg.Vertex2.XsdName == "Point")
                            {
                                ErrLogger.Information("Closure Ac via point " + blckProp.GetElemDesignation(NextSeg.Vertex2), signal.Designation);
                            }
                            tmpSegmentsAc.Add(NextSeg);
                            AcSegment = NextSeg;
                            Ac = blocks
                                .Where(x => x.XsdName == "DetectionPoint" &&
                                            x.TrackSegId == NextSeg.Designation &&
                                            x.Location <= BlkSignal.Location)
                                .OrderByDescending(x => x.Location)
                                .FirstOrDefault();
                        }
                    }
                }
                if (Ac == null)
                {
                    ExportCigClosure.Add(BlkSignal.Attributes["NAME"].Value + "\t" +
                                 Math.Round(signal.Location * 1000, 0) + "\t" +
                                 "not found" + "\t" +
                                 "error" + "\t" +
                                 "not found" + "\t" +
                                 "error" + '\t');
                    return;
                }
                Bg = blocks
                    .Where(x => x.XsdName == "BaliseGroup" &&
                                x.TrackSegId == BlkSignal.TrackSegId &&
                                x.Location >= Ac.Location)
                    .OrderBy(x => x.Location)
                    .FirstOrDefault();
                if (Bg == null)
                {
                    TrackSegmentTmp NextSeg = AcSegment;
                    int counter = 0;
                    while (Bg == null && counter < Constants.nextNodeMaxAttemps && NextSeg != null)
                    {
                        counter++;
                        TrackSegmentTmp NextTmp = NextSeg;
                        NextSeg = TrackSegmentsTmp
                                  .Where(x => x.Vertex1 == NextSeg.Vertex2)
                                  .FirstOrDefault();
                        if (NextSeg == null)
                        {
                            NextSeg = TrackSegmentsTmp
                                  .Where(x => x.Vertex2 == NextTmp.Vertex2 && x != NextTmp)
                                  .FirstOrDefault();
                        }
                        if (NextSeg != null)
                        {
                            if (NextSeg.Vertex1.XsdName == "Point")
                            {
                                ErrLogger.Information("Closure BG via point " + blckProp.GetElemDesignation(NextSeg.Vertex2), signal.Designation);
                                if (NextSeg.Designation.Split('-').Last() == "T")
                                {
                                    tmpSegmentsBg.Add(NextSeg);
                                    Bg = blocks
                                        .Where(x => x.XsdName == "BaliseGroup" &&
                                                    x.TrackSegId == NextSeg.Designation &&
                                                    x.Location >= BlkSignal.Location)
                                        .OrderBy(x => x.Location)
                                        .FirstOrDefault();
                                }
                                else if (GetPointBranch(NextSeg.Vertex1) == ConnectionBranchType.right && NextSeg.Designation.Split('-')[1] == "L")
                                {
                                    tmpSegmentsBg.Add(NextSeg);
                                    Bg = blocks
                                        .Where(x => x.XsdName == "BaliseGroup" &&
                                                    x.TrackSegId == NextSeg.Designation &&
                                                    x.Location >= BlkSignal.Location)
                                        .OrderBy(x => x.Location)
                                        .FirstOrDefault();
                                }
                                else if (GetPointBranch(NextSeg.Vertex1) == ConnectionBranchType.left && NextSeg.Designation.Split('-')[1] == "R")
                                {
                                    tmpSegmentsBg.Add(NextSeg);
                                    Bg = blocks
                                        .Where(x => x.XsdName == "BaliseGroup" &&
                                                    x.TrackSegId == NextSeg.Designation &&
                                                    x.Location >= BlkSignal.Location)
                                        .OrderBy(x => x.Location)
                                        .FirstOrDefault();
                                }
                                else
                                {

                                    if (TrackSegmentsTmp.Where(x => x.Vertex1 == NextTmp.Vertex2).Count() < 2)
                                    {
                                        ErrLogger.Error("Cannot find CES Bg", signal.Designation, "");
                                        break;
                                    }
                                    else
                                    {
                                        NextSeg = TrackSegmentsTmp
                                              .Where(x => x.Vertex1 == NextTmp.Vertex2)
                                              .ElementAt(1);
                                        tmpSegmentsBg.Add(NextSeg);
                                        Bg = blocks
                                            .Where(x => x.XsdName == "BaliseGroup" &&
                                                        x.TrackSegId == NextSeg.Designation &&
                                                        x.Location >= BlkSignal.Location)
                                            .OrderBy(x => x.Location)
                                            .FirstOrDefault();
                                    }
                                }
                            }
                            else
                            {
                                tmpSegmentsBg.Add(NextSeg);
                                Bg = blocks
                                    .Where(x => x.XsdName == "BaliseGroup" &&
                                                x.TrackSegId == NextSeg.Designation &&
                                                x.Location >= BlkSignal.Location)
                                    .OrderBy(x => x.Location)
                                    .FirstOrDefault();
                            }
                        }
                    }
                }
            }
            string kmAc = "error";
            string kmBg = "error";
            string nameAc = "not found";
            string nameBg = "not found";
            List<Block> Connectors = new List<Block>();
            List<string> closConns = new List<string>();

            //if (Ac == null)
            //{
            //    ErrLogger.Warning("Cannot find CES Ac for " + signal.Designation);
            //    //return false;
            //}
            //else
            //{
            Connectors = tmpSegmentsAc
                .SelectMany(y => y.BlocksOnSegments)
                .Where(y => y.XsdName == "Connector" &&
                            Calc.Between(y.Location, BlkSignal.Location, Ac.Location))
                .Distinct()
                .ToList();
            foreach (Block dpConn in Connectors)
            {
                //KmGapAc += (Convert.ToDecimal(dpConn.Attributes["KMGAP"].Value) * -1);
                KmGapAc += (Convert.ToDecimal(dpConn.Attributes["KMGAP"].Value));
                closConns.Add(dpConn.Attributes["NAME"].Value);
            }
            //kmAc = (Math.Abs(Math.Round(Ac.Location * 1000, 0)) + KmGapAc).ToString();
            if (Ac.Location > BlkSignal.Location)
            {
                kmAc = (Math.Abs(Math.Round(Ac.Location * 1000, 0)) - KmGapAc).ToString();
            }
            else
            {
                kmAc = (Math.Abs(Math.Round(Ac.Location * 1000, 0)) + KmGapAc).ToString();
            }
            nameAc = Ac.Attributes["NAME"].Value;
            //}
            if (Bg == null)
            {
                ErrLogger.Error("Cannot find CES Bg", signal.Designation, "");
                //return false;
            }
            else
            {

                Connectors = tmpSegmentsBg
                    .SelectMany(y => y.BlocksOnSegments)
                    .Where(y => y.XsdName == "Connector" &&
                                Calc.Between(y.Location, BlkSignal.Location, Bg.Location))
                    .Distinct()
                    .ToList();
                foreach (Block BgConn in Connectors)
                {
                    if (Bg.LineID == BlkSignal.LineID)
                    {
                        //KmGapBg += (Convert.ToDecimal(BgConn.Attributes["KMGAP"].Value) * -1);
                        KmGapBg += (Convert.ToDecimal(BgConn.Attributes["KMGAP"].Value));
                        closConns.Add(BgConn.Attributes["NAME"].Value);
                    }
                    else
                    {
                        ErrLogger.Error("Lines ID changing between BG and AC", signal.Designation, "sig closure");
                        //return false;
                    }
                }
                //kmBg = (Math.Abs(Math.Round(Bg.Location * 1000, 0)) + KmGapBg).ToString();
                if (Bg.Location > Ac.Location)
                {
                    kmBg = (Math.Abs(Math.Round(Bg.Location * 1000, 0)) - KmGapBg).ToString();
                }
                else
                {
                    kmBg = (Math.Abs(Math.Round(Bg.Location * 1000, 0)) + KmGapBg).ToString();
                }
                nameBg = Bg.Attributes["NAME"].Value;
            }
            string connects = string.Join(",", closConns);

            //decimal kmSig = Math.Round(signal.Location * 1000, 0);

            ExportCigClosure.Add(BlkSignal.Attributes["NAME"].Value + "\t" +
                                 Math.Round(signal.Location * 1000, 0) + "\t" +
                                 nameBg + "\t" +
                                 kmBg + "\t" +
                                 nameAc + "\t" +
                                 kmAc + '\t' +
                                 connects);

            //decimal kmBgToAc = 0;
            //decimal kmMbToAc = 0;
            //if (kmAc > 0)
            //{
            //    kmBgToAc = Math.Abs(kmAc - kmBg); // - KmGapAc - KmGapBg);
            //}
            //else
            //{
            //    kmBgToAc = 0;
            //}
            //if (kmAc > 0)
            //{
            //    if (kmBg > kmAc)
            //    {
            //        kmMbToAc = kmSig - kmAc - KmGapAc;
            //    }
            //    else
            //    {
            //        kmMbToAc = kmAc - kmSig - KmGapAc;
            //    }
            //}
            //else
            //{
            //    kmMbToAc = 0;
            //}

            //decimal Ces = Convert.ToDecimal(Convert.ToDouble(kmBgToAc) * 0.05 + 16 - Convert.ToDouble(kmMbToAc));
            //if (Ces > 0)
            //{
            //    return Math.Ceiling(Ces);
            //}
            //else
            //{
            //    return 0;
            //}
        }

        protected bool GetSegments(List<Block> blocks, List<TrackLine> trackslines,
            List<Track> tracks, List<PSA> pSAs, bool skipLevels = false)
        {
            bool error = false;
            List<string> NodesCheck = new List<string>();

            List<Block> SegmentsNodes = blocks.Where(x => (x.XsdName == "Point" ||
                                                             x.XsdName == "Connector"))
                                                        .OrderBy(x => x.Location)
                                                        .ToList();
            //var tst = SegmentsNodes.Where(x => x.Attributes["NAME"].Value == "N104").ToList();
            SegmentsNodes.AddRange(blocks.Where(x => (x.XsdName == "EndOfTrack"))
                                                        .OrderBy(x => x.Location)
                                                        .ToList());
            foreach (Block Node in SegmentsNodes)
            {
                List<TrackLine> branches = new List<TrackLine>(trackslines
                                  .Where(x => ObjectsIntersects(x.line,
                                                                Node.BlkRef,
                                                                Intersect.OnBothOperands))
                                  .ToList());
                if (Node.XsdName == "Point" &&
                    !Node.Attributes["NAME"].Value.Contains("SN") &&
                    branches.Count < 2 &&
                    Node.KindOf != "trapPoint" &&
                    Node.KindOf != "derailer" &&
                    Node.IsOnCurrentArea)
                {
                    ErrLogger.Error("Not all branches were found", blckProp.GetElemDesignation(Node), "track segments");
                    error = true;
                }
                else if (branches.Count == 0)
                {
                    ErrLogger.Error("Branches not found",  blckProp.GetElemDesignation(Node), "track segments");
                    error = true;
                }
                foreach (TrackLine branch in branches)
                {
                    Line LastLine = null;
                    Block Vertex1 = Node;

                    if (Vertex1.XsdName == "EndOfTrack" &&
                            NodesCheck.Contains(blckProp.GetElemDesignation(Vertex1)))
                    {
                        continue;
                    }
                    if (Vertex1.XsdName == "Connector")
                    {
                        if (branch.direction == DirectionType.up)
                        {
                            if (Node.BlkRef.Position.X == branch.line.GeometricExtents.MaxPoint.X &&
                            Node.BlkRef.Position.Y == branch.line.GeometricExtents.MaxPoint.Y)
                            {
                                continue;
                            }
                        }
                        else if (branch.direction == DirectionType.down)
                        {
                            if (Node.BlkRef.Position.X == branch.line.GeometricExtents.MinPoint.X &&
                            Node.BlkRef.Position.Y == branch.line.GeometricExtents.MinPoint.Y)
                            {
                                continue;
                            }
                        }
                        //Vertex1.Location = Convert.ToDecimal(Vertex1.Attributes["OKMP1"].Value);
                    }
                    else
                    {
                        try
                        {
                            Vertex1.Location =
                            Convert.ToDecimal(Vertex1.Attributes["KMP"].Value.Split('/')[0]);
                            if (Vertex1.Attributes["KMP"].Value.Split('/').Length > 1)
                            {
                                Vertex1.Attributes["KMP"].Value = Vertex1.Attributes["KMP"].Value.Split('/')[0];
                            }
                        }
                        catch (FormatException)
                        {
                            Vertex1.Location = 0;
                        }
                    }
                    List<Block> blocksOnSegement = new List<Block>();
                    List<Line> TrustLinesOnSegment = new List<Line>();
                    List<Line> TrackLinesOnSegment = new List<Line>();
                    ConnectionBranchType branchType1 =
                        GetConnType(Vertex1, branch.line, 1, branch.direction, ref error);
                    AddLinesToPoint(Vertex1, branchType1, branch);

                    if (Vertex1.XsdName == "Point" && branch.direction == DirectionType.up &&
                        GetPointOrient(Vertex1) == LeftRightType.left && branchType1 != ConnectionBranchType.tip)
                    {
                        continue;
                    }
                    if (Vertex1.XsdName == "Point" && branch.direction == DirectionType.down &&
                        GetPointOrient(Vertex1) == LeftRightType.right && branchType1 != ConnectionBranchType.tip)
                    {
                        continue;
                    }
                    int nextNodeAttemps = 0;
                    Block Vertex2 = GetNextNode(blocks, trackslines, branch, Node.X,
                        ref LastLine, ref blocksOnSegement, ref TrustLinesOnSegment, ref TrackLinesOnSegment,
                        ref nextNodeAttemps);
                    if (Vertex2 != null)
                    {
                        if (Vertex1.BlkRef.Id == Vertex2.BlkRef.Id)
                        {
                            continue;
                        }
                        ConnectionBranchType branchType2 =
                            GetConnType(Vertex2, LastLine, 2, branch.direction, ref error);
                        AddLinesToPoint(Vertex2, branchType2, branch);
                        if (NodesCheck.Contains(blckProp.GetElemDesignation(Vertex1) + "-" + branchType1 +
                                                    "-" + blckProp.GetElemDesignation(Vertex2) + "-" + branchType2))
                        {
                            continue;
                        }

                        string designation;
                        if (Vertex1.XsdName == "Point")
                        {
                            designation = blckProp.GetElemDesignation(Vertex1) + "-" +
                                            branchType1.ToString().Substring(0, 1).ToUpper();
                        }
                        else
                        {
                            designation = blckProp.GetElemDesignation(Vertex1) + "-R";
                            if (branch.direction == DirectionType.down)
                            {
                                designation = blckProp.GetElemDesignation(Vertex1) + "-L";
                            }
                        }
                        string track = tracks
                                       .Where(x => TrackLinesOnSegment
                                                   .Any(y => x.X >= y.GeometricExtents.MinPoint.X &&
                                                             x.X <= y.GeometricExtents.MaxPoint.X &&
                                                             (x.Y >= y.GeometricExtents.MinPoint.Y - 6 &&
                                                             x.Y <= y.GeometricExtents.MinPoint.Y + 6))
                                              )
                                       .Select(x => x.Name)
                                       .FirstOrDefault();
                        Dictionary<string, string> mainTracks = ReadMainTracks();
                        bool mainTrack = false;
                        if (mainTracks.ContainsKey(stationID))
                        {
                            if (mainTracks[stationID] == track)
                            {
                                mainTrack = true;
                            }
                        }
                        double MinY = TrackLinesOnSegment
                                      .Where(x => Math.Round(x.Angle * 180 / Math.PI) == 0 ||
                                                  Math.Round(x.Angle * 180 / Math.PI) == 180 ||
                                                  Math.Round(x.Angle * 180 / Math.PI) == 360)
                                      .OrderBy(x => Calc.RndXY(x.GeometricExtents.MinPoint.Y))
                                      .Select(x => Calc.RndXY(x.GeometricExtents.MinPoint.Y))
                                      .FirstOrDefault();
                        bool betweenLevel = MinY == 0;
                        if (betweenLevel)
                        {
                            MinY = TrackLinesOnSegment
                                   .OrderBy(x => Calc.RndXY((x.GeometricExtents.MinPoint.Y +
                                                            x.GeometricExtents.MaxPoint.Y) * 0.5))
                                   .Select(x => Calc.RndXY((x.GeometricExtents.MinPoint.Y +
                                                            x.GeometricExtents.MaxPoint.Y) * 0.5))
                                   .FirstOrDefault();
                        }
                        foreach (Block block in blocksOnSegement)
                        {
                            block.TrackSegId = designation;
                        }
                        TrackSegmentsTmp.Add(new TrackSegmentTmp
                        {
                            Designation = designation,
                            Vertex1 = Vertex1,
                            Vertex2 = Vertex2,
                            ConnV1 = branchType1,
                            ConnV2 = branchType2,
                            BlocksOnSegments = blocksOnSegement,
                            TrustedLines = TrustLinesOnSegment,
                            TrackLines = TrackLinesOnSegment,
                            Track = track,
                            mainTrack = mainTrack,
                            minY = MinY,
                            betweenLevels = betweenLevel,
                            lineId = Vertex1.LineID
                        });

                        if (!(Vertex1.IsOnNextStation == true || Vertex2.IsOnNextStation == true))
                        {

                            TrackSegmentsTrackSegment trcksegement = new TrackSegmentsTrackSegment
                            {
                                Designation = designation,
                                Status = Status,
                                LineID = Vertex2.LineID,
                                Vertex1 = new VertexType
                                {
                                    connection = branchType1,
                                    vertexID = blckProp.GetElemDesignation(Vertex1)
                                },
                                Vertex2 = new VertexType
                                {
                                    connection = branchType2,
                                    vertexID = blckProp.GetElemDesignation(Vertex2)
                                },
                            };
                            PSA SegPsa = pSAs
                                             .Where(x => Vertex1.X >= x.MinX &&
                                                         Vertex1.X <= x.MaxX &&
                                                         Vertex1.Y >= x.MinY &&
                                                         Vertex1.Y <= x.MaxY &&
                                                         Vertex2.X >= x.MinX &&
                                                         Vertex2.X <= x.MaxX &&
                                                         Vertex2.Y >= x.MinY &&
                                                         Vertex2.Y <= x.MaxY)
                                              .FirstOrDefault();

                            if (SegPsa != null)
                            {
                                trcksegement.InsidePSA = SegPsa.Name.ToLower();
                            }
                            trcksegments.Add(trcksegement);
                            //string Okm1 = "";
                            //string Okm2 = "";
                            //if (Vertex1.XsdName == "Connector" && Vertex1.Location != Vertex1.Location2)
                            //{
                            //    Okm1 = Vertex1.Location.ToString() + "," + Vertex1.Location2.ToString();
                            //}
                            //else
                            //{
                            //    Okm1 = Vertex1.Location.ToString();
                            //}
                            //if (Vertex2.XsdName == "Connector" && Vertex2.Location != Vertex2.Location2)
                            //{
                            //    Okm2 = Vertex2.Location.ToString() + "," + Vertex2.Location2.ToString();
                            //}
                            //else
                            //{
                            //    Okm2 = Vertex2.Location.ToString();
                            //}
                            //ExportList.Add(trcksegement.Designation + "\t" + Okm1 + "\t" + Okm2);
                        }

                        if (Vertex2.XsdName == "EndOfTrack")
                        {
                            NodesCheck.Add(blckProp.GetElemDesignation(Vertex2));
                        }
                        //else
                        {
                            NodesCheck.Add(blckProp.GetElemDesignation(Vertex1) + "-" + branchType1 +
                            "-" + blckProp.GetElemDesignation(Vertex2) + "-" + branchType2);
                        }
                    }
                    else
                    {
                        if (Vertex1.IsOnCurrentArea)
                        {
                            ErrLogger.Error("Vertex2 not found", blckProp.GetElemDesignation(Vertex1), "track segment");
                            error = true;
                        }
                        //logs.Add(DateTime.Now +"Vertex2 for '" + blckProp.GetElemDesignation(Vertex1) + "' not found");
                    }
                }
            }
            if (!skipLevels)
            {
                error = !CalcTrackSegsLevels();
            }
            return !error;
        }

        private bool CalcTrackSegsLevels()
        {
            //var tst = TrackSegmentsTmp
            //                .Where(x => !x.betweenLevels &&
            //                            !x.Vertex1.IsOnNextStation &&
            //                            !x.Vertex2.IsOnNextStation)
            //                .GroupBy(g => new { g.lineId, g.minY})
            //                .OrderBy(g => g.Key.lineId).ThenBy(g => g.Count()).ThenBy(g => g.Key.minY)
            //                .SelectMany(x => x)
            //                .ToList();
            var SegByLevel = TrackSegmentsTmp
                             .Where(x => !x.betweenLevels &&
                                         !x.Vertex1.IsOnNextStation &&
                                         !x.Vertex2.IsOnNextStation)
                             .GroupBy(g => g.minY)
                             .OrderByDescending(g => g.Count()).ThenBy(g => g.Key)
                             .ToList();
            var SegByBetwLevel = TrackSegmentsTmp
                             .Where(x => x.betweenLevels &&
                                         !x.Vertex1.IsOnNextStation &&
                                         !x.Vertex2.IsOnNextStation)
                             .GroupBy(g => g.minY)
                             .OrderByDescending(g => g.Count()).ThenBy(g => g.Key)
                             .ToList();
            int countSegLev = TrackSegmentsTmp
                             .Where(x => !x.Vertex1.IsOnNextStation &&
                                         !x.Vertex2.IsOnNextStation)
                             .GroupBy(g => g.minY)
                             .SelectMany(g => g).Count();

            if (trcksegments.Count != countSegLev)
            {
                ErrLogger.Error("Unable to calculate Track segments levels", "Track Segments", "");
                return false;
            }
            else
            {
                var ZeroLevel = SegByLevel.Where(x => x.Any(l => l.lineId == ZeroLevelLine)).OrderByDescending(x => x.Count()).First();
                var LevelsUp = SegByLevel
                               .Where(x => x.Key > ZeroLevel.Key)
                               .OrderBy(x => x.Key);
                int levUp = 1;
                foreach (var LevelUp in LevelsUp)
                {
                    foreach (var seg in LevelUp)
                    {
                        trcksegments
                            .Where(x => x.Designation == seg.Designation)
                            .FirstOrDefault().TrackLevel = levUp;
                    }
                    levUp++;
                }
                var LevelsDown = SegByLevel
                                 .Where(x => x.Key < ZeroLevel.Key)
                                 .OrderByDescending(x => x.Key); ;
                int levDown = -1;
                foreach (var LevelD in LevelsDown)
                {
                    foreach (var seg in LevelD)
                    {
                        trcksegments
                            .Where(x => x.Designation == seg.Designation)
                            .FirstOrDefault().TrackLevel = levDown;
                    }
                    levDown--;
                }
                foreach (var LevelBetw in SegByBetwLevel)
                {
                    string Highest = SegByLevel
                                 .Where(x => x.Key > LevelBetw.Key)
                                 .OrderBy(x => x.Key)
                                 .FirstOrDefault()
                                 .Select(t => t.Designation).FirstOrDefault();
                    //string Lowest = SegByLevel
                    //             .Where(x => x.Key < LevelBetw.Key)
                    //             .OrderByDescending(x => x.Key)
                    //             .FirstOrDefault()
                    //             .Select(t => t.Designation).FirstOrDefault();
                    foreach (var seg in LevelBetw)
                    {
                        trcksegments
                            .Where(x => x.Designation == seg.Designation)
                            .FirstOrDefault().TrackLevel = trcksegments
                                                           .Where(x => x.Designation == Highest)
                                                           .FirstOrDefault().TrackLevel -
                                                           Convert.ToDecimal(0.5);
                    }
                }
            }
            return true;
        }

        private bool ObjectsIntersects(Entity Ent1, Entity Ent2, Intersect intersect, bool checkWholeBlock = false)
        {
            Point3dCollection intersections;

            if (Ent2.GetType() == typeof(BlockReference))
            {
                if (checkWholeBlock)
                {
                    intersections = new Point3dCollection();
                    Ent1.IntersectWith(Ent2, intersect, intersections, IntPtr.Zero, IntPtr.Zero);
                    if (intersections != null && intersections.Count > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    DBObjectCollection entset = new DBObjectCollection();
                    Ent2.Explode(entset);
                    foreach (DBObject obj in entset)
                    {
                        if (obj.GetType() == typeof(Line) ||
                            obj.GetType() == typeof(Polyline) ||
                            obj.GetType() == typeof(Solid))
                        {
                            intersections = new Point3dCollection();
                            Ent1.IntersectWith(((Entity)obj), intersect, intersections,
                                                  IntPtr.Zero, IntPtr.Zero);
                            if (intersections != null && intersections.Count > 0)
                            {
                                return true;
                            }
                            if (obj.GetType() == typeof(Line) && Ent1.GetType() == typeof(Line))
                            {
                                if (LinesHasSamePoint((Line)obj, (Line)Ent1))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    return false;
                }
            }
            else
            {
                intersections = new Point3dCollection();
                Ent1.IntersectWith(Ent2, intersect, intersections, IntPtr.Zero, IntPtr.Zero);
                if (intersections != null && intersections.Count > 0)
                {
                    return true;
                }
                else if (Ent1.GetType() == typeof(Line) && Ent2.GetType() == typeof(Line))
                {
                    return LinesHasSamePoint((Line)Ent1, (Line)Ent2); ;
                }
                else
                {
                    return false;
                }
            }
        }

        private bool LinesHasSamePoint(Line line1, Line line2)
        {
            if (line1.StartPoint.X == line2.StartPoint.X &&
                    line1.StartPoint.Y == line2.StartPoint.Y)
            {
                return true;
            }
            if (line1.EndPoint.X == line2.EndPoint.X &&
                line1.EndPoint.Y == line2.EndPoint.Y)
            {
                return true;
            }
            if (line1.StartPoint.X == line2.EndPoint.X &&
                line1.StartPoint.Y == line2.EndPoint.Y)
            {
                return true;
            }
            if (line1.EndPoint.X == line2.StartPoint.X &&
                line1.EndPoint.Y == line2.StartPoint.Y)
            {
                return true;
            }
            return false;
        }

        private TrackLine GetNextIntersectedTrackLine(TrackLine branch, List<TrackLine> trackslines)
        {
            List<TrackLine> tmpLines = trackslines
                        .Where(x => x.line.GeometricExtents.MinPoint.X > branch.line.GeometricExtents.MinPoint.X)
                        .OrderBy(x => x.line.GeometricExtents.MinPoint.X)
                        .ToList();
            if (branch.direction == DirectionType.down)
            {
                tmpLines = trackslines
                        .Where(x => x.line.GeometricExtents.MaxPoint.X < branch.line.GeometricExtents.MaxPoint.X)
                        .OrderByDescending(x => x.line.GeometricExtents.MaxPoint.X)
                        .ToList();
            }
            foreach (TrackLine line in tmpLines)
            {
                Point3dCollection intersections = new Point3dCollection();
                branch.line.IntersectWith(line.line, Intersect.OnBothOperands, intersections, IntPtr.Zero, IntPtr.Zero);
                if (intersections != null && intersections.Count > 0)
                {
                    return line;
                }
            }
            return null;
        }

        private TrackLine GetNextNonIntersectedTrackLine(TrackLine branch, List<TrackLine> trackslines)
        {
            TrackLine tmpLine;
            double x1 = Calc.RndXY(branch.line.StartPoint.X);
            double y1 = Calc.RndXY(branch.line.StartPoint.Y);
            double x2 = Calc.RndXY(branch.line.EndPoint.X);
            double y2 = Calc.RndXY(branch.line.EndPoint.Y);
            if (y1 == y2)
            {
                tmpLine = trackslines
                        .Where(x => x.line.GeometricExtents.MinPoint.X >= branch.line.GeometricExtents.MaxPoint.X &&
                                    Calc.RndXY(x.line.GeometricExtents.MinPoint.Y) <= y2 + Constants.yTol &&
                                    Calc.RndXY(x.line.GeometricExtents.MinPoint.Y) >= y2 - Constants.yTol)
                        .OrderBy(x => x.line.GeometricExtents.MinPoint.X)
                        .FirstOrDefault();
                if (branch.direction == DirectionType.down)
                {
                    tmpLine = trackslines
                        .Where(x => x.line.GeometricExtents.MaxPoint.X <= branch.line.GeometricExtents.MinPoint.X &&
                                    Calc.RndXY(x.line.GeometricExtents.MaxPoint.Y) <= y2 + Constants.yTol &&
                                    Calc.RndXY(x.line.GeometricExtents.MaxPoint.Y) >= y2 - Constants.yTol)
                        .OrderByDescending(x => x.line.GeometricExtents.MaxPoint.X)
                        .FirstOrDefault();
                }
                if (tmpLine != null)
                {
                    return tmpLine;
                }
            }
            else if (x1 != x2)
            {
                tmpLine = trackslines
                        .Where(x => x.line.GeometricExtents.MinPoint.X > branch.line.GeometricExtents.MaxPoint.X &&
                                    (Calc.RndXY((Calc.RndXY(x.line.StartPoint.X) - x1) / (x2 - x1)) ==
                                     Calc.RndXY((Calc.RndXY(x.line.StartPoint.Y) - y1) / (y2 - y1)))
                               )
                        .OrderBy(x => x.line.GeometricExtents.MinPoint.X)
                        .FirstOrDefault();
                if (branch.direction == DirectionType.down)
                {
                    tmpLine = trackslines
                        .Where(x => x.line.GeometricExtents.MaxPoint.X < branch.line.GeometricExtents.MinPoint.X &&
                                    (Calc.RndXY((Calc.RndXY(x.line.StartPoint.X) - x1) / (x2 - x1)) ==
                                     Calc.RndXY((Calc.RndXY(x.line.StartPoint.Y) - y1) / (y2 - y1)))
                               )
                        .OrderByDescending(x => x.line.GeometricExtents.MaxPoint.X)
                        .FirstOrDefault();
                }
                if (tmpLine != null)
                {
                    return tmpLine;
                }
            }
            return null;
        }

        private IEnumerable<Block> GetBlocksBelongToTrackSegment(TrackLine line, List<Block> blocks,
                                        double xStartedFrom, double xTo, bool toLineEnd)
        {
            if (toLineEnd)
            {
                xTo = line.line.GeometricExtents.MaxPoint.X + 1.5;
                if (line.direction == DirectionType.down)
                {
                    xTo = line.line.GeometricExtents.MinPoint.X - 1.5;
                }
            }

            List<Block> tmpBlocks = blocks.Where(x => (x.XsdName == "DetectionPoint" ||
                                                             x.XsdName == "BaliseGroup" ||
                                                             x.XsdName == "Signal" ||
                                                             x.XsdName == "Point" ||
                                                             x.XsdName == "Connector" ||
                                                             x.XsdName == "StaffPassengerCrossing" ||
                                                             x.XsdName == "StaffCrossing" ||
                                                             x.XsdName == "EndOfTrack" ||
                                                             x.XsdName == "LevelCrossing") &&
                                                             (x.X >= xStartedFrom) &&
                                                             (x.X <= xTo))
                                                        .OrderBy(x => x.Location).ToList();
            if (line.direction == DirectionType.down)
            {
                tmpBlocks = blocks.Where(x => (x.XsdName == "DetectionPoint" ||
                                                             x.XsdName == "BaliseGroup" ||
                                                             x.XsdName == "Signal" ||
                                                             x.XsdName == "Point" ||
                                                             x.XsdName == "Connector" ||
                                                             x.XsdName == "StaffPassengerCrossing" ||
                                                             x.XsdName == "StaffCrossing" ||
                                                             x.XsdName == "EndOfTrack" ||
                                                             x.XsdName == "LevelCrossing") &&
                                                             (x.X <= xStartedFrom) &&
                                                             (x.X >= xTo))
                                                        .OrderBy(x => x.Location).ToList();
            }
            foreach (Block blockRef in tmpBlocks)
            {
                DBObjectCollection entset = new DBObjectCollection();
                blockRef.BlkRef.Explode(entset);
                if (ObjectsIntersects(line.line, blockRef.BlkRef, Intersect.OnBothOperands))
                {
                    blockRef.Designation = blckProp.GetElemDesignation(blockRef);

                    blockRef.LineID = RailwayLines.Where(x => x.color == line.color)
                                                             .Select(y => y.designation)
                                                             .FirstOrDefault();
                    yield return blockRef;
                    // break;
                }
            }

        }

        private IEnumerable<Line> GetTrustedLinesBelongToTrackSegment(TrackLine line, List<Block> blocks,
                                        double xStartedFrom, double xTo, bool toLineEnd)
        {
            if (toLineEnd)
            {
                xTo = line.line.GeometricExtents.MaxPoint.X + 1.5;
                if (line.direction == DirectionType.down)
                {
                    xTo = line.line.GeometricExtents.MinPoint.X - 1.5;
                }
            }

            foreach (TrustedArea area in TrustedAreas)
            {
                List<Line> TrustedLines = area.Lines
                .Where(x => ObjectsIntersects(x, line.line, Intersect.OnBothOperands) == true &&
                                              x.GeometricExtents.MaxPoint.X >= xStartedFrom &&
                                              x.GeometricExtents.MaxPoint.X <= xTo)
                .ToList();
                if (line.direction == DirectionType.down)
                {
                    TrustedLines = area.Lines
                .Where(x => ObjectsIntersects(x, line.line, Intersect.OnBothOperands) == true &&
                                              x.GeometricExtents.MinPoint.X <= xStartedFrom &&
                                              x.GeometricExtents.MinPoint.X >= xTo)
                .ToList();
                }
                foreach (Line tmpline in TrustedLines)
                    yield return TrustedLines[0];
            }
        }

        private Block GetNextNode(List<Block> blocks, List<TrackLine> trackslines,
                                     TrackLine branch, double XstartedFrom, ref Line lastLine,
                                     ref List<Block> blocksOnSegement, ref List<Line> trustLinesOnSegment,
                                     ref List<Line> TrackLinesOnSegment, ref int attemps)
        {
            if (attemps >= Constants.nextNodeMaxAttemps)
            {
                ErrLogger.Error("Max attempts reached finding next node of segment",
                    "X:" + branch.line.GeometricExtents.MaxPoint.X +
                    " Y:" + branch.line.GeometricExtents.MaxPoint.Y, "track segments");
                ErrLogger.ErrorsFound = true;
                return null;
            }
            lastLine = branch.line;
            TrackLinesOnSegment.Add(lastLine);
            TrackLine trackLine = branch;

            // check if current entity intersects with next node
            List<Block> blockRefs = blocks.Where(x => (x.XsdName == "Point" ||
                                                             x.XsdName == "Connector" ||
                                                             x.XsdName == "EndOfTrack") &&
                                                             (x.X > XstartedFrom))
                                                       .OrderBy(x => x.Location).ToList();
            if (branch.direction == DirectionType.down)
            {
                blockRefs = blocks.Where(x => (x.XsdName == "Point" ||
                                                             x.XsdName == "Connector" ||
                                                             x.XsdName == "EndOfTrack") &&
                                                             (x.X < XstartedFrom))
                                                       .OrderBy(x => x.Location).ToList();
            }

            if (blockRefs.Count != 0)
            {
                foreach (Block blkRef in blockRefs)
                {
                    if (ObjectsIntersects(branch.line, blkRef.BlkRef, Intersect.OnBothOperands))
                    {
                        blocksOnSegement.AddRange(GetBlocksBelongToTrackSegment(trackLine,
                                          blocks, XstartedFrom, blkRef.X, false).ToList());
                        trustLinesOnSegment.AddRange(GetTrustedLinesBelongToTrackSegment(branch,
                                          blocks, XstartedFrom, blkRef.X, false).ToList());
                        return blkRef;
                    }
                }
            }
            blocksOnSegement.AddRange(GetBlocksBelongToTrackSegment(trackLine,
                                                  blocks, XstartedFrom, 0, true).ToList());
            trustLinesOnSegment.AddRange(GetTrustedLinesBelongToTrackSegment(branch,
                                                  blocks, XstartedFrom, 0, true).ToList());
            if (branch.direction == DirectionType.up)
            {
                XstartedFrom = branch.line.GeometricExtents.MinPoint.X;
            }
            else if (branch.direction == DirectionType.down)
            {
                XstartedFrom = branch.line.GeometricExtents.MaxPoint.X;
            }

            // check if ent intersects with line
            TrackLine nextLine = GetNextIntersectedTrackLine(branch, trackslines);
            if (nextLine != null)
            {
                lastLine = nextLine.line;
                double tmpXstarted = 0;
                if (branch.direction == DirectionType.up)
                {
                    tmpXstarted = nextLine.line.GeometricExtents.MinPoint.X;
                }
                else if (branch.direction == DirectionType.down)
                {
                    tmpXstarted = nextLine.line.GeometricExtents.MaxPoint.X;
                }
                Block blockRef = GetNextNode(blocks, trackslines, nextLine,
                    tmpXstarted, ref lastLine,
                    ref blocksOnSegement, ref trustLinesOnSegment, ref TrackLinesOnSegment, ref attemps);

                if (blockRef != null)
                {
                    return blockRef;
                };
            }

            // get next non-intersected line
            nextLine = GetNextNonIntersectedTrackLine(branch, trackslines);
            if (nextLine != null)
            {
                lastLine = nextLine.line;
                double tmpXstarted = 0;
                if (branch.direction == DirectionType.up)
                {
                    tmpXstarted = nextLine.line.GeometricExtents.MinPoint.X;
                }
                else if (branch.direction == DirectionType.down)
                {
                    tmpXstarted = nextLine.line.GeometricExtents.MaxPoint.X;
                }
                Block blockRef = GetNextNode(blocks, trackslines, nextLine,
                    tmpXstarted, ref lastLine,
                    ref blocksOnSegement, ref trustLinesOnSegment, ref TrackLinesOnSegment, ref attemps);
                if (blockRef != null)
                {
                    return blockRef;
                };
            }
            return null;
        }

        private Line GetPointTipLine(Block BlkPoint)
        {
            DBObjectCollection entset = new DBObjectCollection();
            BlkPoint.BlkRef.Explode(entset);
            foreach (DBObject obj in entset)
            {
                if (obj.GetType() == typeof(Line))
                {
                    Line line = (Line)obj;
                    if (line.Layer == "Cross" || Math.Round(line.Length, 4) == 1.4459)
                    {
                        return line;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets Point's branch line
        /// </summary>
        /// <param name="BlkPoint"></param>
        /// <returns></returns>
        private Line GetPointBranchLine(Block BlkPoint)
        {
            DBObjectCollection entset = new DBObjectCollection();
            BlkPoint.BlkRef.Explode(entset);
            foreach (DBObject obj in entset)
            {
                if (obj.GetType() == typeof(Line))
                {
                    Line line = (Line)obj;
                    if (Calc.Between(line.Length, 0.70, 0.72) ||
                        Calc.Between(line.Length, 3.96, 3.98))
                    {
                        return line;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="BlkPoint"></param>
        /// <returns></returns>
        private Line GetPointBaseLine(Block BlkPoint)
        {
            DBObjectCollection entset = new DBObjectCollection();
            BlkPoint.BlkRef.Explode(entset);
            foreach (DBObject obj in entset)
            {
                if (obj.GetType() == typeof(Line))
                {
                    Line line = (Line)obj;
                    if (Calc.RndXY(line.Length) == 8 ||
                        Calc.RndXY(line.Length, 1) == 4.5 ||
                        Calc.RndXY(line.Length, 1) == 11.5)
                    {
                        return line;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets tracks segments connection branch type.
        /// </summary>
        private ConnectionBranchType GetConnType(Block BlkPoint, Line ConnectionLine,
                                                    int node, DirectionType direction, ref bool error)
        {
            Line Tip = GetPointTipLine(BlkPoint);
            Line branchLine = GetPointBranchLine(BlkPoint);
            ConnectionBranchType branchType = new ConnectionBranchType();

            if (Tip == null)
            {
                if (BlkPoint.XsdName == "Point")
                {
                    error = true;
                    ErrLogger.Error("Branch not found", blckProp.GetElemDesignation(BlkPoint), "Tip");
                }
                return ConnectionBranchType.none;
            }
            if (branchLine == null)
            {
                if (BlkPoint.XsdName == "Point")
                {
                    error = true;
                    ErrLogger.Error("Branch not found", blckProp.GetElemDesignation(BlkPoint), "not Tip");
                }
                return ConnectionBranchType.none;
            }

            ConnectionBranchType branchTypeTmp = GetPointBranch(BlkPoint);
            LeftRightType leftRightTypeTmp = GetPointOrient(BlkPoint);
            if (BlkPoint.KindOf == "derailer" || BlkPoint.KindOf == "hhtDerailer")
            {
                switch (node)
                {
                    case 1:
                        if (direction == DirectionType.up)
                        {
                            if (leftRightTypeTmp == LeftRightType.left)
                            {
                                branchType = ConnectionBranchType.tip;
                            }
                            if (leftRightTypeTmp == LeftRightType.right)
                            {
                                branchType = branchTypeTmp;
                            }
                        }
                        if (direction == DirectionType.down)
                        {
                            if (leftRightTypeTmp == LeftRightType.left)
                            {
                                branchType = branchTypeTmp;
                            }
                            if (leftRightTypeTmp == LeftRightType.right)
                            {
                                branchType = ConnectionBranchType.tip;
                            }
                        }
                        return branchType;
                    case 2:
                        if (direction == DirectionType.up)
                        {
                            if (leftRightTypeTmp == LeftRightType.left)
                            {
                                branchType = branchTypeTmp;
                            }
                            if (leftRightTypeTmp == LeftRightType.right)
                            {
                                branchType = ConnectionBranchType.tip;
                            }
                        }
                        if (direction == DirectionType.down)
                        {
                            if (leftRightTypeTmp == LeftRightType.left)
                            {
                                branchType = ConnectionBranchType.tip;
                            }
                            if (leftRightTypeTmp == LeftRightType.right)
                            {
                                branchType = branchTypeTmp;
                            }
                        }
                        return branchType;
                }



                //if (direction == DirectionType.up && node == 1 && leftRightTypeTmp == LeftRightType.right)
                //{
                //    branchType = ConnectionBranchType.tip; ;
                //}
                //else if (direction == DirectionType.down && node == 1 && leftRightTypeTmp == LeftRightType.right)
                //{
                //    branchType = branchTypeTmp;
                //}
                //else if (direction == DirectionType.up && node == 1 && leftRightTypeTmp == LeftRightType.left)
                //{
                //    branchType = branchTypeTmp;
                //}
                //else if (direction == DirectionType.down && node == 1 && leftRightTypeTmp == LeftRightType.left)
                //{
                //    branchType = ConnectionBranchType.tip;
                //}

                return branchType;

            }
            else if (ObjectsIntersects(ConnectionLine, branchLine, Intersect.OnBothOperands))
            {
                branchType = branchTypeTmp;
            }
            else if (ObjectsIntersects(ConnectionLine, Tip, Intersect.OnBothOperands))
            {
                if (node == 1)
                {
                    if (direction == DirectionType.up)
                    {
                        if (leftRightTypeTmp == LeftRightType.right)
                        {
                            if (branchTypeTmp == ConnectionBranchType.right)
                            {
                                branchType = ConnectionBranchType.left;
                            }
                            if (branchTypeTmp == ConnectionBranchType.left)
                            {
                                branchType = ConnectionBranchType.right;
                            }
                        }
                        if (leftRightTypeTmp == LeftRightType.left)
                        {
                            branchType = ConnectionBranchType.tip;
                        }
                    }
                    if (direction == DirectionType.down)
                    {
                        if (leftRightTypeTmp == LeftRightType.left)
                        {
                            if (branchTypeTmp == ConnectionBranchType.right)
                            {
                                branchType = ConnectionBranchType.left;
                            }
                            if (branchTypeTmp == ConnectionBranchType.left)
                            {
                                branchType = ConnectionBranchType.right;
                            }
                        }
                        if (leftRightTypeTmp == LeftRightType.right)
                        {
                            branchType = ConnectionBranchType.tip;
                        }
                    }
                }
                if (node == 2)
                {
                    if (direction == DirectionType.up)
                    {
                        if (leftRightTypeTmp == LeftRightType.left)
                        {
                            if (branchTypeTmp == ConnectionBranchType.right)
                            {
                                branchType = ConnectionBranchType.left;
                            }
                            if (branchTypeTmp == ConnectionBranchType.left)
                            {
                                branchType = ConnectionBranchType.right;
                            }
                        }
                        if (leftRightTypeTmp == LeftRightType.right)
                        {
                            branchType = ConnectionBranchType.tip;
                        }
                    }
                    if (direction == DirectionType.down)
                    {
                        if (leftRightTypeTmp == LeftRightType.right)
                        {
                            if (branchTypeTmp == ConnectionBranchType.right)
                            {
                                branchType = ConnectionBranchType.left;
                            }
                            if (branchTypeTmp == ConnectionBranchType.left)
                            {
                                branchType = ConnectionBranchType.right;
                            }
                        }
                        if (leftRightTypeTmp == LeftRightType.left)
                        {
                            branchType = ConnectionBranchType.tip;
                        }
                    }
                }
            }
            if (branchType == ConnectionBranchType.none)
            {
                error = true;
                ErrLogger.Error("Branch not found", blckProp.GetElemDesignation(BlkPoint), "");
            }
            return branchType;
        }

        private void AddLinesToPoint(Block BlkPoint, ConnectionBranchType branchType, TrackLine branch)
        {
            if (BlkPoint.XsdName == "Point")
            {
                switch (branchType)
                {
                    case ConnectionBranchType.tip:
                        BlkPoint.LineIDtip = branch.LineID;
                        break;
                    case ConnectionBranchType.left:
                        BlkPoint.LineIDleft = branch.LineID;
                        break;
                    case ConnectionBranchType.right:
                        BlkPoint.LineIDright = branch.LineID;
                        break;
                }
            }
        }

        /// <summary>
        /// Gets Point indicator
        /// </summary>
        /// <param name="BlkPoint"></param>
        /// <returns></returns>
        private YesNoType GetPointPosIndicator(Block BlkPoint)
        {
            DBObjectCollection entset = new DBObjectCollection();
            BlkPoint.BlkRef.Explode(entset);
            bool polyline = false;
            bool hatch = false;
            List<Line> tmpLines = new List<Line>();
            foreach (DBObject obj in entset)
            {
                if (obj.GetType() == typeof(Polyline))
                {
                    Polyline Flag = (Polyline)obj;
                    if (Calc.RndXY(Flag.Area, 1) == 4.5 && Calc.RndXY(Flag.Length) == 9)
                    {
                        polyline = true;
                    }
                }
                if (obj.GetType() == typeof(Hatch))
                {
                    Hatch FlagHatch = (Hatch)obj;
                    if (Calc.RndXY(FlagHatch.Area, 1) == 4.5 || Calc.RndXY(FlagHatch.Area, 4) == 2.4413)
                    {
                        hatch = true;
                    }
                }
                if (obj.GetType() == typeof(Line))
                {
                    Line line = (Line)obj;
                    if (Calc.RndXY(line.Length, 4) == 1.1276 ||
                        Calc.RndXY(line.Length, 4) == 3.0871 ||
                        Calc.RndXY(line.Length, 3) == 2.165)
                    {
                        tmpLines.Add(line);
                        if (tmpLines.Count == 4)
                        {
                            Polyline Flag = new Polyline();
                            Flag.TransformBy(tmpLines[0].Ecs);
                            Point3d start = tmpLines[0].StartPoint.TransformBy(Flag.Ecs.Inverse());
                            Flag.AddVertexAt(0, new Point2d(start.X, start.Y), 0, 0, 0);
                            Flag.JoinEntity(tmpLines[0]);
                            Flag.JoinEntity(tmpLines[1]);
                            Flag.JoinEntity(tmpLines[2]);
                            Flag.JoinEntity(tmpLines[3]);
                            if (Calc.RndXY(Flag.Area, 4) == 2.4413)
                            {
                                polyline = true;
                            }
                        }
                    }
                }

                if (polyline && hatch)
                {
                    return YesNoType.yes;
                }

            }
            return YesNoType.no;
        }

        /// <summary>
        /// Gets Point orient related to Y ax
        /// </summary>
        /// <param name="BlkPoint"></param>
        /// <returns></returns>
        private LeftRightType GetPointOrient(Block BlkPoint)
        {
            Line baseLine = GetPointBaseLine(BlkPoint);
            double baseAngle = 0;
            Point2d cross = new Point2d();
            Point2d baseCross = new Point2d();
            DBObjectCollection entset = new DBObjectCollection();
            BlkPoint.BlkRef.Explode(entset);

            Extents3d ext = GetPointTipLine(BlkPoint).GeometricExtents;
            cross = new Point2d(
            (ext.MinPoint.X + ext.MaxPoint.X) * 0.5,
            (ext.MinPoint.Y + ext.MaxPoint.Y) * 0.5
            );

            ext = baseLine.GeometricExtents;
            baseCross = new Point2d(
            (ext.MinPoint.X + ext.MaxPoint.X) * 0.5,
            (ext.MinPoint.Y + ext.MaxPoint.Y) * 0.5
            );

            baseAngle = Calc.RadToDeg(cross.GetVectorTo(baseCross).Angle);
            if (Calc.Between(baseAngle, 90, 270))
            {
                return LeftRightType.left;
            }
            else
            {
                return LeftRightType.right;
            }
        }

        /// <summary>
        /// Gets point branch side (left/right) form block geometry.
        /// </summary>
        private ConnectionBranchType GetPointBranch(Block BlkPoint)
        {
            //char[] delimiter = "_".ToCharArray();
            //string[] pointTypes = BlkPoint.BlockName.Split(delimiter, 3, StringSplitOptions.RemoveEmptyEntries);

            ConnectionBranchType branchType = new ConnectionBranchType();
            Line baseLine = GetPointBaseLine(BlkPoint);
            Line tip = GetPointTipLine(BlkPoint);
            Line branchLine = GetPointBranchLine(BlkPoint);
            double deltaBranch = 0;
            double baseAngle = 0;
            double branchAngle = 0;
            Point2d cross = new Point2d();
            Point2d baseCross = new Point2d();
            Point2d branchCross = new Point2d();
            Extents3d ext = new Extents3d();
            DBObjectCollection entset = new DBObjectCollection();
            BlkPoint.BlkRef.Explode(entset);

            ext = tip.GeometricExtents;
            cross = new Point2d(
            (ext.MinPoint.X + ext.MaxPoint.X) * 0.5,
            (ext.MinPoint.Y + ext.MaxPoint.Y) * 0.5
            );

            ext = baseLine.GeometricExtents;
            baseCross = new Point2d(
            (ext.MinPoint.X + ext.MaxPoint.X) * 0.5,
            (ext.MinPoint.Y + ext.MaxPoint.Y) * 0.5
            );

            ext = branchLine.GeometricExtents;
            branchCross = new Point2d(
            (ext.MinPoint.X + ext.MaxPoint.X) * 0.5,
            (ext.MinPoint.Y + ext.MaxPoint.Y) * 0.5
            );

            baseAngle = cross.GetVectorTo(baseCross).Angle;
            branchAngle = cross.GetVectorTo(branchCross).Angle;
            deltaBranch =
                cross.GetVectorTo(baseCross).GetAngleTo(cross.GetVectorTo(branchCross));

            if (BlkPoint.KindOf == "derailer" || BlkPoint.KindOf == "hhtDerailer")
            {
                // SW agreement: derailer derails always in right
                branchType = ConnectionBranchType.left;
                return branchType;

                //Point3dCollection intersections = new Point3dCollection();
                //baseLine.IntersectWith(branchLine, Intersect.ExtendThis, intersections, IntPtr.Zero, IntPtr.Zero);
                //Vector2d AflBranch = new Vector2d();
                //if (intersections != null && intersections.Count > 0)
                //{
                //    if (intersections[0].DistanceTo(branchLine.EndPoint) >
                //        intersections[0].DistanceTo(branchLine.StartPoint))
                //    {
                //        AflBranch =
                //            new Point2d(branchLine.EndPoint.X, branchLine.EndPoint.Y) -
                //            new Point2d(branchLine.StartPoint.X, branchLine.StartPoint.Y);
                //    }
                //    else
                //    {
                //        AflBranch =
                //            new Point2d(branchLine.StartPoint.X, branchLine.StartPoint.Y) -
                //            new Point2d(branchLine.EndPoint.X, branchLine.EndPoint.Y);
                //    }
                //}
                //Vector2d baseVector = cross.GetVectorTo(baseCross);
                //branchAngle = AflBranch.GetAngleTo(baseVector);
                //if (Calc.Between(Calc.RadToDeg(branchAngle), 44, 46, true))
                //{
                //    branchType = ConnectionBranchType.left;
                //}
                //else
                //{
                //    branchType = ConnectionBranchType.right;
                //}
                //return branchType;
            }

            if (branchAngle > 0 && branchAngle < deltaBranch && cross.Y < branchCross.Y)
            {
                branchType = ConnectionBranchType.left;
            }
            else if (branchAngle > 0 && branchAngle >= (2 * Math.PI - deltaBranch) && cross.Y > branchCross.Y)
            {
                branchType = ConnectionBranchType.right;
            }
            else if (branchAngle < baseAngle)
            {
                branchType = ConnectionBranchType.right;
            }
            else if (branchAngle > baseAngle)
            {
                branchType = ConnectionBranchType.left;
            }
            return branchType;
        }

        /// <summary>
        /// Is point trailable or not
        /// </summary>
        /// <param name="BlkPoint"></param>
        /// <returns></returns>
        private YesNoType PointTrailable(Block BlkPoint)
        {
            DBObjectCollection entset = new DBObjectCollection();
            BlkPoint.BlkRef.Explode(entset);
            foreach (DBObject obj in entset)
            {
                if (obj.GetType() == typeof(Hatch))
                {
                    Hatch hatch = (Hatch)obj;
                    if (Calc.Between(hatch.Area, 2.35, 2.42))
                    {
                        return YesNoType.no;
                    }
                }
            }
            return YesNoType.yes;
        }

        /// <summary>
        /// Gets position and count of point machines
        /// </summary>
        /// <param name="BlkPoint"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private LeftRightType GetPointMachines(Block BlkPoint, out int count)
        {
            Line baseLine = null;
            //Line tip = null;
            double deltaPm = 0;
            double baseAngle = 0;
            double pmAngle = 0;
            Point2d cross = new Point2d();
            Point2d baseCross = new Point2d();
            Point2d hatchMidle = new Point2d();
            List<Hatch> pointMachines = new List<Hatch>();
            DBObjectCollection entset = new DBObjectCollection();
            BlkPoint.BlkRef.Explode(entset);
            LeftRightType position = LeftRightType.right;

            foreach (DBObject obj in entset)
            {
                if (obj.GetType() == typeof(Line))
                {
                    Line line = (Line)obj;
                    if (line.Layer == "Cross" || Math.Round(line.Length, 4) == 1.4459)
                    {
                        //tip = line;
                        Extents3d ext = line.GeometricExtents;
                        cross = new Point2d(
                        (ext.MinPoint.X + ext.MaxPoint.X) * 0.5,
                        (ext.MinPoint.Y + ext.MaxPoint.Y) * 0.5
                        );
                        break;
                    }
                }
            }
            foreach (DBObject obj in entset)
            {
                if (obj.GetType() == typeof(Line))
                {
                    Line line = (Line)obj;
                    if (Calc.RndXY(line.Length) == 8 ||
                        Calc.RndXY(line.Length, 1) == 4.5 ||
                        Calc.RndXY(line.Length, 1) == 11.5)
                    {
                        baseLine = line;
                        Extents3d ext = baseLine.GeometricExtents;
                        baseCross = new Point2d(
                        (ext.MinPoint.X + ext.MaxPoint.X) * 0.5,
                        (ext.MinPoint.Y + ext.MaxPoint.Y) * 0.5
                        );
                        break;
                    }
                }
            }
            foreach (DBObject obj in entset)
            {
                if (obj.GetType() == typeof(Hatch))
                {
                    Hatch hatch = (Hatch)obj;
                    if (Calc.RndXY(hatch.Area, 4) == 4.8106)
                    {
                        pointMachines.Add(hatch);
                    }
                    if (Calc.Between(hatch.Area, 2.35, 2.42))
                    {
                        pointMachines.Add(hatch);
                    }
                }
            }
            Extents3d extHatch = pointMachines[0].GeometricExtents;
            hatchMidle = new Point2d(
            (extHatch.MinPoint.X + extHatch.MaxPoint.X) * 0.5,
            (extHatch.MinPoint.Y + extHatch.MaxPoint.Y) * 0.5
            );
            baseAngle = cross.GetVectorTo(baseCross).Angle;
            pmAngle = cross.GetVectorTo(hatchMidle).Angle;
            deltaPm =
                cross.GetVectorTo(baseCross).GetAngleTo(cross.GetVectorTo(hatchMidle));
            if (pmAngle > 0 && pmAngle < deltaPm && cross.Y < hatchMidle.Y)
            {
                position = LeftRightType.left;
            }
            else if (pmAngle > 0 && pmAngle >= (2 * Math.PI - deltaPm) && cross.Y > hatchMidle.Y)
            {
                position = LeftRightType.right;
            }
            else if (pmAngle < baseAngle)
            {
                position = LeftRightType.right;
            }
            else if (pmAngle > baseAngle)
            {
                position = LeftRightType.left;
            }
            count = pointMachines.Count;
            return position;
        }

        /// <summary>
        /// Gets all autocad objects of certain type.
        /// </summary>
        public static IEnumerable<ObjectId> GetObjectsOfType(Database db, RXClass cls, string layer = "")
        {
            using (var tr = db.TransactionManager.StartOpenCloseTransaction())
            {
                var btr = (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db),
                                                            OpenMode.ForRead);
                foreach (ObjectId id in btr)
                {
                    if (id.ObjectClass.IsDerivedFrom(cls))
                    {
                        if (true)
                        {

                        }
                        yield return id;
                    }
                }
                tr.Commit();
            }
        }

        /// <summary>
        /// Gets type of End Of Track from block name.
        /// </summary>
        private KindOfEOTType GetKindOfEOT(Block BlkEot)
        {
            string KindOfEot = BlkEot.BlockName.Split('_')[1];
            if (KindOfEot == "DBS")
            {
                return KindOfEOTType.dynamicBufferStop;
            }
            else if (KindOfEot == "NDY")
            {
                return KindOfEOTType.nonDynamicBufferStop;
            }
            else
            {
                return KindOfEOTType.endOfTrack;
            }
        }

        /// <summary>
        /// Finds Trusted Areas.
        /// </summary>
        protected bool CollectTrustedAreas(List<Line> TrustedLines, List<TrackLine> trackLines)
        {
            bool error = false;
            List<Line> SkipLines = new List<Line>();
            int AreaCounter = 1;
            foreach (Line FirstTrustedLine in TrustedLines)
            {
                if (SkipLines.Contains(FirstTrustedLine))
                {
                    continue;
                }
                SkipLines.Add(FirstTrustedLine);
                List<Line> TrustedArea = new List<Line>
                {
                    FirstTrustedLine
                };
                int lineCounter = 0;
                Line testLine = FirstTrustedLine;
                while (lineCounter < TrustedLines.Count)
                {
                    if (FirstTrustedLine == TrustedLines[lineCounter])
                    {
                        lineCounter++;
                        continue;
                    }
                    if (ObjectsIntersects(testLine, TrustedLines[lineCounter], Intersect.OnBothOperands))
                    {
                        if (SkipLines.Contains(TrustedLines[lineCounter]))
                        {
                            lineCounter++;
                            continue;
                        }
                        SkipLines.Add(TrustedLines[lineCounter]);
                        TrustedArea.Add(TrustedLines[lineCounter]);
                        testLine = TrustedLines[lineCounter];
                        lineCounter = 0;
                    }
                    else
                    {
                        lineCounter++;
                    }
                }
                //if (TrustedArea.Count % 2 != 0)
                //{
                //    //&&
                //    //!trackLines.Any(x => !TrustedArea.Any(l => ObjectsIntersects(x.line, l, Intersect.OnBothOperands)))
                //    ErrLogger.Warning("Trusted area lines inconsistence. First line start X -'"
                //    + TrustedArea[0].StartPoint.X.ToString() + "'");
                //    error = true;
                //}
                //else 
                if (trackLines.Any(x => TrustedArea.Any(l => ObjectsIntersects(x.line, l, Intersect.OnBothOperands))))
                {
                    TrustedAreas.Add(new TrustedArea
                    {
                        Designation = "tsta-" + stationID + "-" + AreaCounter++.ToString().PadLeft(3, '0'),
                        Lines = TrustedArea.OrderBy(x => x.StartPoint.X).ToList()
                    });
                }
            }
            return !error;
        }

        /// <summary>
        /// Checks line is vertical.
        /// </summary>
        private bool LineIsVertical(Line line)
        {
            if ((line.Angle * (180 / Math.PI) >= 89 && line.Angle * (180 / Math.PI) <= 91) ||
                (line.Angle * (180 / Math.PI) >= 269 && line.Angle * (180 / Math.PI) <= 271))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected void ReadBlocksDefinitions()
        {
            foreach (string line in File.ReadAllLines(assemblyPath + Constants.cfgFolder + "//BlkMap.dat")
                                        .Where(arg => !string.IsNullOrWhiteSpace(arg) &&
                                          arg[0] != '#'))
            {
                if (!BlocksToGet.ContainsKey(line.Split('\t')[0]))
                {
                    BlocksToGet.Add(line.Split('\t')[0], line);
                }
            }
        }

        private Dictionary<string, string> ReadLinesDefinitions()
        {
            string path;
            if (File.Exists(dwgDir + "//LinesDef.dat"))
            {
                path = dwgDir + "//LinesDef.dat";
            }
            else
            {
                path = assemblyPath + Constants.cfgFolder + "//LinesDef.dat";
            }
                Dictionary<string, string> LinesDefinitions = new Dictionary<string, string>();
            foreach (string line in File.ReadAllLines(path)
                                        .Where(arg => !string.IsNullOrWhiteSpace(arg)))
            {
                if (!LinesDefinitions.ContainsKey(line.Split('\t')[0]))
                {
                    LinesDefinitions.Add(line.Split('\t')[0], line);
                }
            }
            return LinesDefinitions;
        }

        private List<Platform> ReadPlatforms(string Station)
        {
            //List<Platform> platformList = new List<Platform>();
            return ((IEnumerable<string>)File.ReadAllLines(this.assemblyPath + Constants.cfgFolder + "//Platforms.dat")).Where((Func<string, bool>)(arg =>
            {
                if (string.IsNullOrWhiteSpace(arg) || arg[0] == '#')
                    return false;
                return Station.ToLower() == arg.Split('\t')[1].Trim().ToLower();
            })).Select((Func<string, Platform>)(x => new Platform()
            {
                Station = x.Split('\t')[1].Trim(),
                Number = Convert.ToInt32(x.Split('\t')[2].Trim()),
                Track = Convert.ToInt32(x.Split('\t')[3].Trim()),
                Length = Convert.ToInt32(x.Split('\t')[4].Trim()),
                Height = Convert.ToInt32(x.Split('\t')[5].Trim()) * 10
            })).ToList();
        }

        private Dictionary<string, string> ReadStations()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach (string str in ((IEnumerable<string>)File.ReadAllLines(this.assemblyPath + Constants.cfgFolder + "//Stations.dat")).Where((Func<string, bool>)(arg => !string.IsNullOrWhiteSpace(arg) && arg[0] != '#')))
            {
                if (!dictionary.ContainsKey(str.Split('\t')[0]))
                    dictionary.Add(str.Split('\t')[0], str);
            }
            return dictionary;
        }

        private Dictionary<string, string> ReadMainTracks()
        {
            //Dictionary<string, string> dictionary = new Dictionary<string, string>();
            return ((IEnumerable<string>)File.ReadAllLines(this.assemblyPath + Constants.cfgFolder + "//MainTracks.dat")).Where((Func<string, bool>)(arg => !string.IsNullOrWhiteSpace(arg) && arg[0] != '#')).ToDictionary((Func<string, string>)(arg => arg.Split('\t')[0]), (Func<string, string>)(arg => arg.Split('\t')[1]));
        }

        private bool GetBlocksWithoutSegment(List<Block> blocks)
        {
            bool error = false;
            List<Block> nonSegBlocks = blocks                //TrackSegmentsTmp
                                .Where(x => x.TrackSegId == null &&
                                            !x.IsOnNextStation &&
                                             (x.XsdName == "Point" ||
                                              x.XsdName == "Signal" ||
                                              x.XsdName == "DetectionPoint" ||
                                              x.XsdName == "EndOfTrack" ||
                                              x.XsdName == "BaliseGroup" ||
                                              x.XsdName == "StaffPassengerCrossing"))
                                .ToList();
            foreach (var check in nonSegBlocks)
            {
                ErrLogger.Error("Element without TrackSegment", blckProp.GetElemDesignation(check), "");
                error = true;
            }
            return !error;
        }

        protected void SetBlocksNextStations(List<Block> blocks)
        {
            List<Block> nextStations = blocks
                                  .Where(x => x.XsdName == "NextStation")
                                  .ToList();
            foreach (var nextStation in nextStations)
            {
                double minX = nextStation.BlkRef.GeometricExtents.MinPoint.X;
                double maxX = nextStation.BlkRef.GeometricExtents.MaxPoint.X;
                double minY = nextStation.BlkRef.GeometricExtents.MinPoint.Y;
                double maxY = nextStation.BlkRef.GeometricExtents.MaxPoint.Y;
                List<Block> blocksNextSt = blocks
                                              .Where(x => x.X >= minX &&
                                                          x.X <= maxX &&
                                                          x.Y >= minY &&
                                                          x.Y <= maxY)
                                              .ToList();
                foreach (var blk in blocksNextSt)
                {
                    blk.IsOnCurrentArea = false;
                    blk.IsOnNextStation = true;
                }
            }
        }

        public void SetBlocksExclude(List<Block> blocks)
        {
            List<Block> nextStations = blocks
                                  .Where(x => x.XsdName == "ExcludeBlock")
                                  .ToList();
            foreach (var nextStation in nextStations)
            {
                double minX = nextStation.BlkRef.GeometricExtents.MinPoint.X;
                double maxX = nextStation.BlkRef.GeometricExtents.MaxPoint.X;
                double minY = nextStation.BlkRef.GeometricExtents.MinPoint.Y;
                double maxY = nextStation.BlkRef.GeometricExtents.MaxPoint.Y;
                List<Block> blocksNextSt = blocks
                                              .Where(x => x.X >= minX &&
                                                          x.X <= maxX &&
                                                          x.Y >= minY &&
                                                          x.Y <= maxY)
                                              .ToList();
                foreach (var blk in blocksNextSt)
                {
                    blk.IsOnCurrentArea = false;
                }
            }
        }

        private void SetSignalsStartDestCompRoutes(List<Block> blocks)
        {
            List<Block> startCr = blocks
                                  .Where(x => x.XsdName == "CrStart")
                                  .ToList();
            foreach (var start in startCr)
            {
                double minX = start.BlkRef.GeometricExtents.MinPoint.X;
                double maxX = start.BlkRef.GeometricExtents.MaxPoint.X;
                double minY = start.BlkRef.GeometricExtents.MinPoint.Y;
                double maxY = start.BlkRef.GeometricExtents.MaxPoint.Y;
                List<Block> blocksNextSt = blocks
                                              .Where(x => x.X >= minX &&
                                                          x.X <= maxX &&
                                                          x.Y >= minY &&
                                                          x.Y <= maxY && x.XsdName == "Signal")
                                              .ToList();
                foreach (var blk in blocksNextSt)
                {
                    blk.Start_Cr = true;
                }
            }
            List<Block> destCr = blocks
                                  .Where(x => x.XsdName == "CrDestination")
                                  .ToList();
            foreach (var dest in destCr)
            {
                double minX = dest.BlkRef.GeometricExtents.MinPoint.X;
                double maxX = dest.BlkRef.GeometricExtents.MaxPoint.X;
                double minY = dest.BlkRef.GeometricExtents.MinPoint.Y;
                double maxY = dest.BlkRef.GeometricExtents.MaxPoint.Y;
                List<Block> blocksNextSt = blocks
                                              .Where(x => x.X >= minX &&
                                                          x.X <= maxX &&
                                                          x.Y >= minY &&
                                                          x.Y <= maxY && x.XsdName == "Signal")
                                              .ToList();
                foreach (var blk in blocksNextSt)
                {
                    blk.Dest_Cr = true;
                }
            }
        }

        private void CopyBlockFromFile(string FilePath, string BlockName)

        {
            using (Database OpenDb = new Database(false, true))
            {
                OpenDb.ReadDwgFile(FilePath, FileShare.ReadWrite, true, "");
                ObjectIdCollection ids = new ObjectIdCollection();
                using (Transaction tr =
                        OpenDb.TransactionManager.StartTransaction())

                {
                    BlockTable bt;
                    bt = (BlockTable)tr.GetObject(OpenDb.BlockTableId, OpenMode.ForRead);
                    if (bt.Has(BlockName))
                    {
                        ids.Add(bt[BlockName]);

                    }
                    tr.Commit();
                }

                if (ids.Count != 0)
                {
                    IdMapping iMap = new IdMapping();
                    db.WblockCloneObjects(ids, db.BlockTableId,
                                              iMap, DuplicateRecordCloning.Ignore, false);
                }

            }
        }

        private ObjectId InsertBlock(string BlockName, double x, double y)
        {
            ObjectId objectId = ObjectId.Null;
            using (Transaction acTrans = db.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl =
                    acTrans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                ObjectId blkRecId = ObjectId.Null;
                blkRecId = acBlkTbl[BlockName];
                BlockTableRecord blockDef =
                    acBlkTbl[BlockName].GetObject(OpenMode.ForRead) as BlockTableRecord;
                if (blkRecId != ObjectId.Null)
                {
                    using (BlockReference acBlkRef = new BlockReference(new Point3d(x, y, 0), blkRecId))
                    {
                        BlockTableRecord acCurSpaceBlkTblRec;
                        acCurSpaceBlkTblRec =
                            acTrans.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                        acCurSpaceBlkTblRec.AppendEntity(acBlkRef);
                        acTrans.AddNewlyCreatedDBObject(acBlkRef, true);
                        foreach (ObjectId id in blockDef)
                        {
                            DBObject obj = id.GetObject(OpenMode.ForRead);
                            AttributeDefinition attDef = obj as AttributeDefinition;
                            if ((attDef != null) && (!attDef.Constant))
                            {
                                using (AttributeReference attRef = new AttributeReference())
                                {
                                    attRef.SetAttributeFromBlock(attDef, acBlkRef.BlockTransform);
                                    acBlkRef.AttributeCollection.AppendAttribute(attRef);
                                    acTrans.AddNewlyCreatedDBObject(attRef, true);
                                }
                            }
                        }
                        objectId = acBlkRef.Id;
                    }
                }
                acTrans.Commit();
            }
            return objectId;
        }

        private string GetFoulPointLocation(Block FoulPoint, Block BlkPoint,
            ConnectionBranchType branchType)
        {
            string LineId = BlkPoint.LineIDtip;
            string foulLocation = FoulPoint.Location.ToString();

            switch (branchType)
            {
                case ConnectionBranchType.tip:
                    LineId = BlkPoint.LineIDtip;
                    break;
                case ConnectionBranchType.left:
                    if (BlkPoint.LineIDleft == null)
                    {
                        LineId = BlkPoint.LineIDtip;
                    }
                    else
                    {
                        LineId = BlkPoint.LineIDleft;
                    }
                    break;
                case ConnectionBranchType.right:
                    if (BlkPoint.LineIDright == null)
                    {
                        LineId = BlkPoint.LineIDtip;
                    }
                    else
                    {
                        LineId = BlkPoint.LineIDright;
                    }
                    break;
            }

            RailwayLine rLine =
                     RailwayLines.Where(x => x.designation == LineId).FirstOrDefault();

            if (FoulPoint.Location2 > 0)
            {
                rLine =
                    RailwayLines.Where(x => x.designation == LineId).FirstOrDefault();
                if (Calc.Between(FoulPoint.Location2, rLine.start, rLine.end, true))
                {
                    foulLocation = FoulPoint.Location2.ToString();
                }
            }

            if (Calc.Between(FoulPoint.Location, rLine.start, rLine.end, true))
            {
                foulLocation = FoulPoint.Location.ToString();
            }
            return foulLocation;
        }

        //private Point3d GetMiddlPoint3d(Extents3d extents3)
        //{
        //    return extents3.MinPoint +
        //                            (extents3.MaxPoint -
        //                            (extents3.MinPoint)) * 0.5;
        //}

        //private Point2d GetMiddlPoint2d(Extents3d extents3)
        //{
        //    Point3d point3 = extents3.MinPoint +
        //                            (extents3.MaxPoint -
        //                            (extents3.MinPoint)) * 0.5;
        //    return new Point2d(point3.X, point3.Y);
        //}

        private bool GetNextDps(Block VertexStart, LeftRightType leftRight, DirectionType direction, List<Block> blocks,
                               TrackSegmentTmp segmentTmp, decimal AcSectLoc, ref List<Block> Dps, ref int iterLimit)
        {
            Block DP = null;
            List<Block> ExistDps = Dps;
            //TrackSegmentTmp segmentTmp;
            while (DP == null && Constants.nextAcMaxAttemps > iterLimit)
            {
                iterLimit++;
                if (direction == DirectionType.up)
                {
                    if (leftRight == LeftRightType.right)
                    {
                        DP = blocks
                            .Where(x => x.TrackSegId == segmentTmp.Designation &&
                                        x.Location < segmentTmp.Vertex2.Location &&
                                        x.Location > AcSectLoc &&
                                        x.XsdName == "DetectionPoint" && !ExistDps.Contains(x))
                            .OrderBy(x => x.Location)
                            .FirstOrDefault();
                        if (DP != null)
                        {
                            Dps.Add(DP);
                            if (VertexStart.Location > DP.Location)
                            {
                                return true;
                            }
                        }
                        List<TrackSegmentTmp> NextSegments = TrackSegmentsTmp
                                                             .Where(x => x.Vertex1 == VertexStart && x != segmentTmp)
                                                             .ToList();
                        if (NextSegments != null && NextSegments.Count > 1)
                        {
                            int dpsCount = Dps.Count;
                            foreach (var nextSeg in NextSegments)
                            {
                                GetNextDps(nextSeg.Vertex2, leftRight, direction, blocks, nextSeg, AcSectLoc, ref Dps, ref iterLimit);
                                segmentTmp = nextSeg;
                            }
                            if (Dps.Count - dpsCount == NextSegments.Count)
                            {
                                return true;
                            }
                        }
                        else if (NextSegments != null && NextSegments.Count == 1 && DP == null)
                        {
                            TrackSegmentTmp CheckSegPoint = TrackSegmentsTmp
                                                             .Where(x => x.Vertex2 == VertexStart && x != segmentTmp)
                                                             .FirstOrDefault();
                            if (CheckSegPoint != null && VertexStart.Location > AcSectLoc && GetPointOrient(VertexStart) == LeftRightType.left)
                            {
                                GetNextDps(VertexStart, LeftRightType.left, direction, blocks, CheckSegPoint, AcSectLoc, ref Dps, ref iterLimit);
                            }
                            if (GetNextDps(NextSegments[0].Vertex2, leftRight, direction, blocks, NextSegments[0], AcSectLoc, ref Dps, ref iterLimit))
                            {
                                return true;
                            }
                            segmentTmp = NextSegments[0];
                        }
                    }
                    else if (leftRight == LeftRightType.left)
                    {
                        DP = blocks
                            .Where(x => x.TrackSegId == segmentTmp.Designation &&
                                        x.Location > segmentTmp.Vertex1.Location &&
                                        x.Location < AcSectLoc &&
                                        x.XsdName == "DetectionPoint" && !ExistDps.Contains(x))
                            .OrderByDescending(x => x.Location)
                            .FirstOrDefault();
                        if (DP != null)
                        {
                            Dps.Add(DP);
                            if (VertexStart.Location < DP.Location)
                            {
                                return true;
                            }
                        }
                        List<TrackSegmentTmp> NextSegments = TrackSegmentsTmp
                                                             .Where(x => x.Vertex2 == VertexStart && x != segmentTmp)
                                                             .ToList();
                        if (NextSegments != null && NextSegments.Count > 1)
                        {
                            int dpsCount = Dps.Count;
                            foreach (var nextSeg in NextSegments)
                            {
                                GetNextDps(nextSeg.Vertex1, leftRight, direction, blocks, nextSeg, AcSectLoc, ref Dps, ref iterLimit);
                                segmentTmp = nextSeg;
                            }
                            if (Dps.Count - dpsCount == NextSegments.Count)
                            {
                                return true;
                            }
                        }
                        else if (NextSegments != null && NextSegments.Count == 1 && DP == null)
                        {
                            TrackSegmentTmp CheckSegPoint = TrackSegmentsTmp
                                                             .Where(x => x.Vertex1 == VertexStart && x != segmentTmp)
                                                             .FirstOrDefault();
                            if (CheckSegPoint != null && VertexStart.Location < AcSectLoc && GetPointOrient(VertexStart) == LeftRightType.right)
                            {
                                GetNextDps(VertexStart, LeftRightType.right, direction, blocks, CheckSegPoint, AcSectLoc, ref Dps, ref iterLimit);
                            }
                            if (GetNextDps(NextSegments[0].Vertex1, leftRight, direction, blocks, NextSegments[0], AcSectLoc, ref Dps, ref iterLimit))
                            {
                                return true;
                            }
                            segmentTmp = NextSegments[0];
                        }
                    }
                }
                else if (direction == DirectionType.down)
                {
                    if (leftRight == LeftRightType.right)
                    {
                        DP = blocks
                            .Where(x => x.TrackSegId == segmentTmp.Designation &&
                                        x.Location > segmentTmp.Vertex1.Location &&
                                        x.Location < AcSectLoc &&
                                        x.XsdName == "DetectionPoint" && !ExistDps.Contains(x))
                            .OrderByDescending(x => x.Location)
                            .FirstOrDefault();
                        if (DP != null)
                        {
                            Dps.Add(DP);
                            if (VertexStart.Location < DP.Location)
                            {
                                return true;
                            }
                        }
                        List<TrackSegmentTmp> NextSegments = TrackSegmentsTmp
                                                             .Where(x => x.Vertex2 == VertexStart && x != segmentTmp)
                                                             .ToList();
                        if (NextSegments != null && NextSegments.Count > 1)
                        {
                            int dpsCount = Dps.Count;
                            foreach (var nextSeg in NextSegments)
                            {
                                GetNextDps(nextSeg.Vertex1, leftRight, direction, blocks, nextSeg, AcSectLoc, ref Dps, ref iterLimit);
                                segmentTmp = nextSeg;
                            }
                            if (Dps.Count - dpsCount == NextSegments.Count)
                            {
                                return true;
                            }
                        }
                        else if (NextSegments != null && NextSegments.Count == 1 && DP == null)
                        {
                            TrackSegmentTmp CheckSegPoint = TrackSegmentsTmp
                                                             .Where(x => x.Vertex1 == VertexStart && x != segmentTmp)
                                                             .FirstOrDefault();
                            if (CheckSegPoint != null && VertexStart.Location < AcSectLoc && GetPointOrient(VertexStart) == LeftRightType.left)
                            {
                                GetNextDps(VertexStart, LeftRightType.left, direction, blocks, CheckSegPoint, AcSectLoc, ref Dps, ref iterLimit);
                            }
                            if (GetNextDps(NextSegments[0].Vertex1, leftRight, direction, blocks, NextSegments[0], AcSectLoc, ref Dps, ref iterLimit))
                            {
                                return true;
                            }
                            segmentTmp = NextSegments[0];
                        }
                    }
                    else if (leftRight == LeftRightType.left)
                    {
                        DP = blocks
                            .Where(x => x.TrackSegId == segmentTmp.Designation &&
                                        x.Location < segmentTmp.Vertex2.Location &&
                                        x.Location > AcSectLoc &&
                                        x.XsdName == "DetectionPoint" && !ExistDps.Contains(x))
                            .OrderBy(x => x.Location)
                            .FirstOrDefault();
                        if (DP != null)
                        {
                            Dps.Add(DP);
                            if (VertexStart.Location > DP.Location)
                            {
                                return true;
                            }
                        }
                        List<TrackSegmentTmp> NextSegments = TrackSegmentsTmp
                                                             .Where(x => x.Vertex1 == VertexStart && x != segmentTmp)
                                                             .ToList();
                        if (NextSegments != null && NextSegments.Count > 1)
                        {
                            int dpsCount = Dps.Count;
                            foreach (var nextSeg in NextSegments)
                            {
                                GetNextDps(nextSeg.Vertex2, leftRight, direction, blocks, nextSeg, AcSectLoc, ref Dps, ref iterLimit);
                                segmentTmp = nextSeg;
                            }
                            if (Dps.Count - dpsCount == NextSegments.Count)
                            {
                                return true;
                            }
                        }
                        else if (NextSegments != null && NextSegments.Count == 1 && DP == null)
                        {
                            TrackSegmentTmp CheckSegPoint = TrackSegmentsTmp
                                                             .Where(x => x.Vertex2 == VertexStart && x != segmentTmp)
                                                             .FirstOrDefault();
                            if (CheckSegPoint != null && VertexStart.Location > AcSectLoc && GetPointOrient(VertexStart) == LeftRightType.right)
                            {
                                GetNextDps(VertexStart, LeftRightType.right, direction, blocks, CheckSegPoint, AcSectLoc, ref Dps, ref iterLimit);
                            }
                            if (GetNextDps(NextSegments[0].Vertex2, leftRight, direction, blocks, NextSegments[0], AcSectLoc, ref Dps, ref iterLimit))
                            {
                                return true;
                            }
                            segmentTmp = NextSegments[0];
                        }
                    }
                }
            }
            return (DP != null);
        }

        //private decimal SetAcSectionLocation(Block VertexRight, Block VertexLeft,
        //                                        TrackSegmentTmp segmentRight, TrackSegmentTmp segmentLeft,
        //                                        DirectionType directionR, DirectionType directionL, List<Block> blocks,
        //                                        TrackLine lineRight, TrackLine lineLeft)
        //{
        //    Block right, left;
        //    if (directionR == DirectionType.up)
        //    {
        //        right = blocks
        //              .Where(x => x.TrackSegId == segmentRight.Designation &&
        //                          x.Location <= VertexRight.Location &&
        //                          ObjectsIntersects(lineRight.line, x.BlkRef, Intersect.OnBothOperands) &&
        //                          !x.XsdName.Contains("Crossing"))
        //              .OrderBy(x => x.Location)
        //              .FirstOrDefault();
        //        if (right == null)
        //        {
        //            int iterCount = 0;
        //            List<Line> linesToSkip = new List<Line>();
        //            Line line = lineRight.line;
        //            while (right == null && iterCount < segmentRight.TrackLines.Count)
        //            {
        //                iterCount++;
        //                line = segmentRight.TrackLines
        //                        .Where(x => ObjectsIntersects(x, lineRight.line, Intersect.OnBothOperands) && !linesToSkip.Contains(x))
        //                        .FirstOrDefault();
        //                if (line != null)
        //                {
        //                    right = blocks
        //                        .Where(x => x.TrackSegId == segmentRight.Designation &&
        //                                    x.Location <= VertexRight.Location &&
        //                                    ObjectsIntersects(line, x.BlkRef, Intersect.OnBothOperands) &&
        //                          !x.XsdName.Contains("Crossing"))
        //                        .OrderBy(x => x.Location)
        //                        .FirstOrDefault();
        //                    lineRight.line = line;
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        right = blocks
        //              .Where(x => x.TrackSegId == segmentRight.Designation &&
        //                          x.Location >= VertexRight.Location &&
        //                          ObjectsIntersects(lineRight.line, x.BlkRef, Intersect.OnBothOperands) &&
        //                          !x.XsdName.Contains("Crossing"))
        //              .OrderByDescending(x => x.Location)
        //              .FirstOrDefault();
        //        if (right == null)
        //        {
        //            int iterCount = 0;
        //            List<Line> linesToSkip = new List<Line>();
        //            Line line = lineRight.line;
        //            while (right == null && iterCount < segmentRight.TrackLines.Count)
        //            {
        //                iterCount++;
        //                line = segmentRight.TrackLines
        //                        .Where(x => ObjectsIntersects(x, lineRight.line, Intersect.OnBothOperands) && !linesToSkip.Contains(x))
        //                        .FirstOrDefault();
        //                if (line != null)
        //                {
        //                    right = blocks
        //                        .Where(x => x.TrackSegId == segmentRight.Designation &&
        //                                    x.Location >= VertexRight.Location &&
        //                                    ObjectsIntersects(line, x.BlkRef, Intersect.OnBothOperands) &&
        //                          !x.XsdName.Contains("Crossing"))
        //                        .OrderByDescending(x => x.Location)
        //                        .FirstOrDefault();
        //                    lineRight.line = line;
        //                }
        //            }
        //        }
        //    }
        //    if (directionL == DirectionType.up)
        //    {
        //        left = blocks
        //             .Where(x => x.TrackSegId == segmentLeft.Designation &&
        //                         x.Location >= VertexLeft.Location &&
        //                          ObjectsIntersects(lineLeft.line, x.BlkRef, Intersect.OnBothOperands) &&
        //                          !x.XsdName.Contains("Crossing"))
        //              .OrderByDescending(x => x.Location)
        //              .FirstOrDefault();
        //        if (left == null)
        //        {
        //            int iterCount = 0;
        //            List<Line> linesToSkip = new List<Line>();
        //            Line line = lineRight.line;
        //            while (left == null && iterCount < segmentRight.TrackLines.Count)
        //            {
        //                iterCount++;
        //                line = segmentRight.TrackLines
        //                        .Where(x => ObjectsIntersects(x, lineLeft.line, Intersect.OnBothOperands) && !linesToSkip.Contains(x))
        //                        .FirstOrDefault();
        //                if (line != null)
        //                {
        //                    left = blocks
        //                        .Where(x => x.TrackSegId == segmentRight.Designation &&
        //                                    x.Location >= VertexLeft.Location &&
        //                                    ObjectsIntersects(line, x.BlkRef, Intersect.OnBothOperands) &&
        //                          !x.XsdName.Contains("Crossing"))
        //                        .OrderByDescending(x => x.Location)
        //                        .FirstOrDefault();
        //                    lineLeft.line = line;
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        left = blocks
        //             .Where(x => x.TrackSegId == segmentLeft.Designation &&
        //                         x.Location <= VertexLeft.Location &&
        //                          ObjectsIntersects(lineLeft.line, x.BlkRef, Intersect.OnBothOperands) &&
        //                          !x.XsdName.Contains("Crossing"))
        //              .OrderBy(x => x.Location)
        //              .FirstOrDefault();
        //        if (left == null)
        //        {
        //            int iterCount = 0;
        //            List<Line> linesToSkip = new List<Line>();
        //            Line line = lineRight.line;
        //            while (left == null && iterCount < segmentRight.TrackLines.Count)
        //            {
        //                iterCount++;
        //                line = segmentRight.TrackLines
        //                        .Where(x => ObjectsIntersects(x, lineLeft.line, Intersect.OnBothOperands) && !linesToSkip.Contains(x))
        //                        .FirstOrDefault();
        //                if (line != null)
        //                {
        //                    left = blocks
        //                        .Where(x => x.TrackSegId == segmentRight.Designation &&
        //                                    x.Location <= VertexLeft.Location &&
        //                                    ObjectsIntersects(line, x.BlkRef, Intersect.OnBothOperands) &&
        //                          !x.XsdName.Contains("Crossing"))
        //                        .OrderBy(x => x.Location)
        //                        .FirstOrDefault();
        //                    lineLeft.line = line;
        //                }
        //            }
        //        }
        //    }
        //    if (right == null)
        //    {
        //        right = VertexRight;
        //    }
        //    if (left == null)
        //    {
        //        left = VertexLeft;
        //    }
        //    return (right.Location + left.Location) / 2;
        //}

        private bool CheckExtOverlap(Block BlkSignal, List<ReadExcel.XlsRoute> routes, List<Block> blocks, ref List<string> overPts, ref Block blockTdt)
        {
            List<ReadExcel.XlsRoute> overlapsRoutes = routes
                .Where(x => x.Dest == blckProp.GetElemDesignation(BlkSignal) &&
                x.Type != KindOfRouteType.shunting && x.Overlaps.Count > 0)
                .GroupBy(g => g.Dest)
                .SelectMany(x => x.Select(o => o))
                .Distinct()
                .ToList();
            foreach (var overRoute in overlapsRoutes)
            {
                bool shortOverlap =
                    routes.Any(x => x.Start == overRoute.Start &&
                                    x.Dest == overRoute.Dest &&
                                    x.Overlaps.Count == 0 &&
                                    x.Type != KindOfRouteType.shunting);
                if (shortOverlap)
                {
                    overRoute.Overlaps = new List<string>();
                }
            }
            Regex regEx = new Regex("^N[0-9]{1,3}");
            List<string> overlaps = routes
                .Where(x => x.Dest == blckProp.GetElemDesignation(BlkSignal) &&
                x.Type != KindOfRouteType.shunting && x.Overlaps.Count > 0)
                .GroupBy(g => g.Dest)
                .SelectMany(x => x.SelectMany(o => o.Overlaps).Where(o => !regEx.IsMatch(o)))
                .Distinct()
                .ToList();
            overPts = routes
                .Where(x => x.Dest == blckProp.GetElemDesignation(BlkSignal) &&
                x.Type != KindOfRouteType.shunting && x.Overlaps.Count > 0)
                .GroupBy(g => g.Dest)
                .SelectMany(x => x.SelectMany(o => o.Overlaps).Where(o => regEx.IsMatch(o)))
                .Distinct()
                .ToList();
            if (overPts.Count == 0)
            {
                overPts = routes
                .Where(x => x.Dest == blckProp.GetElemDesignation(BlkSignal) &&
                            x.Type != KindOfRouteType.shunting && x.Overlaps.Count > 0 &&
                            x.SdLast != null)
                .GroupBy(g => g.Dest)
                .SelectMany(x => x.Select(o => o.SdLast).Where(o => regEx.IsMatch(o)))
                .Distinct()
                .ToList();
            }

            if (overlaps.Count > 0)
            {
                blockTdt = blocks
                                 .Where(x => (x.XsdName == "AxleCounterSection" ||
                                             x.XsdName == "TrackSection") &&
                                             overlaps.Contains(blckProp.GetElemDesignation(x)))
                                 .FirstOrDefault();
                ErrLogger.Information("Extended overlap " + blckProp.GetElemDesignation(blockTdt), blckProp.GetElemDesignation(BlkSignal));
                return true;
            }
            return false;
        }

        private List<Line> GetBlockLines(Block BlkSignal)
        {
            DBObjectCollection entset = new DBObjectCollection();
            BlkSignal.BlkRef.Explode(entset);
            List<Line> lines = new List<Line>();
            foreach (DBObject obj in entset)
            {
                if (obj.GetType() == typeof(Line))
                {
                    lines.Add((Line)obj);
                }
                if (obj.GetType() == typeof(Polyline))
                {
                    DBObjectCollection entsetPoly = new DBObjectCollection();
                    ((Polyline)obj).Explode(entsetPoly);
                    foreach (DBObject objPoly in entsetPoly)
                    {
                        lines.Add((Line)objPoly);
                    }
                }
            }
            return lines;
        }

        public void CheckAttSameKm(List<Block> blocks)
        {
            //ErrLogger.filePath = Path.GetDirectoryName(db.Filename) + @"\KmpErrors.log";
            //ErrLogger.Start();
            var test = blocks
                      .Where(x => x.Location > 0 && 
                                  x.IsOnCurrentArea && 
                                  x.XsdName != "FoulingPoint")
                      .GroupBy(x => new { x.X, x.TrackSegId })
                      .Where(x => x.Count() > 1)
                      .ToList();
            foreach (var km in test)
            {
                Block block = km.First();
                var test1 = km.Where(x => x.Location != block.Location);
                if (test1.Count() > 0)
                {
                    ErrLogger.Error("KM inconsistence", block.Designation, block.Location.ToString());
                }
            }
            //ErrLogger.Stop();
        }

        public bool GetCompoundRoutes(List<Block> Blocks, List<RoutesRoute> routesExist)
        {
            var Splineids = GetObjectsOfType(db, RXObject.GetClass(typeof(Spline)));
            List<RoutesRoute> routes = routesExist.ToList();
            List<Spline> crSplines = new List<Spline>();
            List<CompoundRoutesCompoundRoute> existingCrs = new List<CompoundRoutesCompoundRoute>();
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                foreach (ObjectId ObjId in Splineids)
                {
                    Spline spline = (Spline)trans.GetObject(ObjId, OpenMode.ForRead);
                    if (spline.Layer.Contains("Compound_Route"))
                    {
                        crSplines.Add(spline);
                    }
                }
            }
            try
            {
                string[] filesRts =
                Directory.GetFiles(Directory.GetParent(dwgPath) + @"\Routes", "*.xml", SearchOption.AllDirectories);
                RddXmlIO rddXmlIO = new RddXmlIO();
                foreach (var f in filesRts)
                {
                    RailwayDesignData existRdd = rddXmlIO.GetRdd(f);
                    routes.AddRange(existRdd.Routes.Route.ToList());
                    existingCrs.AddRange(existRdd.CompoundRoutes.CompoundRoute.ToList());
                }
            }
            catch (DirectoryNotFoundException e)
            {
                ErrLogger.Error(e.Message,"CR exception", "");
                ErrLogger.ErrorsFound = true;
            }
            List<CrStartEnd> crStartEnds = new List<CrStartEnd>();
            foreach (var crSpline in crSplines.OrderBy(x => x.GeometricExtents.MinPoint.X).ThenBy(x => x.Layer))
            {
                List<Block> signals = Blocks
                                      .Where(x => SplineSignalSamePos(crSpline, x) && x.XsdName == "Signal")
                                      .ToList();
                if (signals.Count < 2)
                {
                    ErrLogger.Error("Spline not connected X - '" +
                                   crSpline.GeometricExtents.MinPoint.X + "', Y - '" +
                                   crSpline.GeometricExtents.MinPoint.Y + "'", "CR calc", "");
                    ErrLogger.ErrorsFound = true;
                    continue;
                }
                bool error = false;
                if (GetSignalDirection(signals[0], ref error) == DirectionType.up)
                {
                    signals = signals.OrderBy(x => x.Location).ToList();
                }
                else
                {
                    signals = signals.OrderByDescending(x => x.Location).ToList();
                }
                crStartEnds.Add(new CrStartEnd
                {
                    Start = blckProp.GetElemDesignation(signals[0]),
                    End = blckProp.GetElemDesignation(signals[1])
                });
            }
            CRs cRs = new CRs(routes, crStartEnds, existingCrs);
            cmproutes = cRs.GetCompoundRoutes();
            return !cRs.error;
        }

        private bool SplineSignalSamePos(Spline spline, Block signal)
        {
            if ((Calc.RndXYMid(spline.StartPoint.X) == signal.X &&
                Calc.RndXYMid(spline.StartPoint.Y) == signal.Y) ||
                (Calc.RndXYMid(spline.EndPoint.X) == signal.X &&
                Calc.RndXYMid(spline.EndPoint.Y) == signal.Y))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private KindOfPointType IsPointHht(Block point, ref List<Block> blocks)
        {
            if (blocks.Any(x => x.XsdName == "Hht" && ObjectsIntersects(x.BlkRef, point.BlkRef, Intersect.OnBothOperands)))
            {
                if (point.XsdName == "Point")
                {
                    if (point.KindOf == "trapPoint")
                    {
                        ErrLogger.Error("KindOfPointType 'hhtTrapPoint' not defined. hhtPoint used instead",
                            blckProp.GetElemDesignation(point), "hht");
                        ErrLogger.ErrorsFound = true;
                        return KindOfPointType.hhtPoint;
                    }
                    else if (point.KindOf == "derailer")
                    {
                        return KindOfPointType.hhtDerailer;
                    }
                    return KindOfPointType.hhtPoint;
                }
                else
                {
                    ErrLogger.Error("Hht and xsd name not match", blckProp.GetElemDesignation(point), point.XsdName);
                    ErrLogger.ErrorsFound = true;
                    return KindOfPointType.point;
                }
            }
            else
            {
                if (!Enum.TryParse(point.KindOf, out KindOfPointType kindOfPoint))
                {
                    ErrLogger.Error("Unable to parse kind of point", blckProp.GetElemDesignation(point), point.KindOf);
                    ErrLogger.ErrorsFound = true;
                }
                return kindOfPoint;
            }
        }

        private DirectionType GetEotDirection(
          Block BlkEndOfTrack,
          ref bool error)
        {
            DirectionType directionType = DirectionType.up;
            Block block = this.TrackSegmentsTmp
                         .Where(x => x.Designation == BlkEndOfTrack.TrackSegId && x.Vertex1 != BlkEndOfTrack)
                         .Select(x => x.Vertex1).FirstOrDefault() ??
                                this.TrackSegmentsTmp
                                .Where(x => x.Designation == BlkEndOfTrack.TrackSegId && x.Vertex2 != BlkEndOfTrack)
                                .Select(x => x.Vertex2).FirstOrDefault();
            if (block != null)
            {
                if (block.Location < BlkEndOfTrack.Location)
                    directionType = DirectionType.up;
                if (block.Location > BlkEndOfTrack.Location)
                    directionType = DirectionType.down;
            }
            else
            {
                ErrLogger.Error("Unable to get EOT direction", BlkEndOfTrack.Designation, "");
                DirectionType result;
                if (!Enum.TryParse(BlkEndOfTrack.Attributes["DIRECTION"].Value, out result))
                    ErrLogger.Error("Unable to parse attribute value", BlkEndOfTrack.Designation, "DIRECTION");
                else
                    ErrLogger.Error("Value copied from attribute",
                        BlkEndOfTrack.Designation, "DIRECTION");
                directionType = result;
                error = true;
            }
            return directionType;
        }

        private void AssignDpsToTrckSections()
        {
            List<Block> dps = this.blocks
                           .Where(x => x.XsdName == "DetectionPoint")
                           .OrderBy(x => Convert.ToDecimal(x.Location))
                           .ToList();
            List<Block> skipFirstDps = new List<Block>();
            List<List<Block>> skipTrackings = new List<List<Block>>();

            foreach (Block firstDp in dps)
            {
                List<Block> sectionDps = new List<Block>
                {
                    firstDp
                };
                Block eot = null;
                skipFirstDps.Add(firstDp);
                List<TrackSegmentTmp> segNodes = this.TrackSegmentsTmp
                                  .Where(x => x.Designation == firstDp.TrackSegId)
                                  .ToList();
                //skipNodes.AddRange(segNodes);
                if (segNodes.Count == 0)
                {
                    ErrLogger.Error("Start Segment(s) not found", firstDp.Designation, "Auto AC");
                    ErrLogger.ErrorsFound = true;
                    continue;
                }
                Stack<Stack<TrackSegmentTmp>> stackNodes = new Stack<Stack<TrackSegmentTmp>>();
                stackNodes.Push(new Stack<TrackSegmentTmp>(segNodes));
                int iterCount = 0;
                bool exists = false;
                List<TrackSegmentTmp> skipNodes = new List<TrackSegmentTmp>();
                skipNodes.AddRange(segNodes);
                List<Block> elements = new List<Block>();
                while (stackNodes.Count > 0)
                {
                    Block nextDp = this.blocks
                        .Where(x => x.XsdName == "DetectionPoint" &&
                                    x.TrackSegId == stackNodes.Peek().Peek().Designation &&
                                    !skipFirstDps.Contains(x) &&
                                    x != firstDp)
                        .OrderBy(x => Convert.ToDecimal(x.Location))
                        .FirstOrDefault();
                    if (nextDp != null)
                    {
                        //List<Block> testDp = new List<Block> { firstDp, nextDp };
                        if (skipTrackings.Any(x => x.Contains(firstDp) && x.Contains(nextDp)))
                        {
                            exists = true;
                            break;
                        }
                        sectionDps.Add(nextDp);
                        if (stackNodes.Peek().Peek().Vertex2.XsdName == "EndOfTrack")
                        {
                            eot = stackNodes.Peek().Peek().Vertex2;
                        }
                        //skipDps.Add(nextDp);
                        do
                        {
                            if (stackNodes.Peek().Count == 2)
                            {
                                stackNodes.Peek().Pop();
                                break;
                            }
                            stackNodes.Pop();
                            iterCount--;
                        }
                        while (stackNodes.Count > 0);
                    }
                    else if (stackNodes.Peek().Peek().Vertex2.XsdName == "EndOfTrack")
                    {
                        eot = stackNodes.Peek().Peek().Vertex2;
                        do
                        {
                            if (stackNodes.Peek().Count == 2)
                            {
                                stackNodes.Peek().Pop();
                                break;
                            }
                            stackNodes.Pop();
                            iterCount--;
                        }
                        while (stackNodes.Count > 0);
                    }
                    else
                    {
                        if (stackNodes.Peek().Peek().Vertex2.XsdName == "Point" &&
                             stackNodes.Peek().Peek().Vertex2.KindOf != "derailer")
                        {
                            elements.Add(stackNodes.Peek().Peek().Vertex2);
                        }
                        segNodes = this.TrackSegmentsTmp
                                   .Where(x => (x.Vertex1 == stackNodes.Peek().Peek().Vertex2 ||
                                               x.Vertex2 == stackNodes.Peek().Peek().Vertex2 /*||
                                               x.Vertex2.Id == stackNodes.Peek().Peek().Vertex1.Id*/) &&
                                               x != stackNodes.Peek().Peek() &&
                                               !skipNodes.Contains(x))
                                   .ToList();
                        if (segNodes.Count == 0)
                        {
                            segNodes = this.TrackSegmentsTmp
                                   .Where(x => x.Vertex2 == stackNodes.Peek().Peek().Vertex1 &&
                                               x != stackNodes.Peek().Peek() &&
                                               !skipNodes.Contains(x))
                                               .ToList();
                        }
                        skipNodes.AddRange(segNodes);
                        iterCount++;
                        if (segNodes.Count == 0 || Constants.dpIterLimit == iterCount)
                        {
                            if (sectionDps.Count == 1 && skipTrackings.Any(x => x.Contains(sectionDps[0])))
                            {
                                exists = true;
                                break;
                            }
                            if (Constants.dpIterLimit == iterCount)
                            {
                                ErrLogger.Error("Iteration limit reached", firstDp.Designation, "Auto AC");
                            }
                            else
                            {
                                ErrLogger.Error("Segment(s) for next dp not found", firstDp.Designation, "Auto AC");
                            }
                            ErrLogger.ErrorsFound = true;
                            break;
                        }
                        else
                        {
                            stackNodes.Push(new Stack<TrackSegmentTmp>(segNodes));
                        }
                    }
                }
                if (exists)
                {
                    continue;
                }
                skipTrackings.Add(sectionDps);
                if (sectionDps.Count == 1 && eot != null)
                {
                    sectionDps.Add(eot);
                }
                AcSection acSectionTmp = CreateAcSection(sectionDps, skipNodes, elements);
                if (acSectionTmp != null)
                {
                    this.acSections.Add(acSectionTmp);
                }
            }
        }

        private AcSection CreateAcSection(List<Block> dps, List<TrackSegmentTmp> segNodes, List<Block> elements)
        {
            if (dps.Count < 2)
            {
                ErrLogger.Error("Ac section with single dp", this.blckProp.GetElemDesignation(dps[0], false, true), "Auto AC");
                ErrLogger.ErrorsFound = true;
                return (AcSection)null;
            }
            AcSection acSection = new AcSection
            {
                Dps = dps
            };
            //decimal min = dps.Min(x => x.Location);
            //decimal max = dps.Max(x => x.Location);
            var segLines = segNodes.SelectMany(x => x.TrackLines).ToList();
            var blockSection = this.blocks
                               .FirstOrDefault(x => (x.XsdName == "AxleCounterSection" ||
                                                    x.XsdName == "TrackSection") &&
                                                    Calc.Between(x.X, dps[0].X, dps[1].X, true) &&
                                                    segLines.Any(l => (l.GetClosestPointTo(x.BlkRef.Position, false) - x.BlkRef.Position).Length <= 5));
            if (blockSection == null)
            {
                ErrLogger.Error("Block for AC section not found", string.Join(",", dps.Select(x => x.Designation).ToArray()), "Auto AC");
                ErrLogger.ErrorsFound = true;
                return null;
            }
            acSection.Designation = this.blckProp.GetElemDesignation(blockSection); //.Split('P').Last();
            if (blockSection.XsdName == "AxleCounterSection")
            {
                //List<Block> points = this.blocks
                //                 .Where(x => x.XsdName == "Point" &&
                //                             Calc.Between(x.Location, min, max, true) &&
                //                             segNodes.Select(s => s.Designation).Contains(x.TrackSegId))
                //                  .OrderBy(x => Convert.ToDouble(x.Location))
                //                 .ToList();
                if (elements == null || elements.Count == 0)
                {
                    ErrLogger.Error("No points for AC section found", dps[0].Designation, "Auto AC");
                    ErrLogger.ErrorsFound = true;
                    return acSection;
                }
                Line pointTip = GetPointTipLine(elements[0]);
                Line pointBase = GetPointBaseLine(elements[0]);
                Point2d Tip = AcadTools.GetMiddlPoint2d(pointTip.GeometricExtents);
                Point2d Base = AcadTools.GetMiddlPoint2d(pointBase.GeometricExtents);
                Vector2d pointStraight = Tip.GetVectorTo(Base);
                foreach (Block Dp in acSection.Dps)
                {
                    Point2d dp =
                    new Point2d(Dp.BlkRef.Position.X, Dp.BlkRef.Position.Y);
                    Vector2d pointDp = dp.GetVectorTo(Base);
                    int Angle = Calc.RadToDeg(pointStraight.GetAngleTo(pointDp));
                    Dp.Ac_angle = Angle;
                    if (Angle == 0)
                    {
                        Dp.Sort = 0;
                    }
                    else if (Calc.Between(Angle, 178, 180, true))
                    {
                        Dp.Sort = 1;
                    }
                    else
                    {
                        Dp.Sort = 2;
                    }
                }
                acSection.Dps = acSection.Dps.OrderBy(x => x.Ac_angle).ToList();
                acSection.Elements = elements.Select(x => x.Designation).ToList();
            }
            else
            {
                Point2d tdt =
                       new Point2d(blockSection.BlkRef.Position.X, blockSection.BlkRef.Position.Y);
                foreach (Block Dp in acSection.Dps)
                {
                    Point2d dp =
                    new Point2d(Dp.BlkRef.Position.X, Dp.BlkRef.Position.Y);
                    Dp.Sort = tdt.GetVectorTo(dp).Angle;
                    if (Calc.Between(Calc.RadToDeg(Dp.Sort), 359, 360, true))
                    {
                        Dp.Sort = 0;
                    }
                }
                acSection.Dps = acSection.Dps.OrderByDescending(x => x.Sort).ToList();
                acSection.Elements = new List<string> { acSection.Designation };
            }
            return acSection;
        }

        private bool GetDetLockPoints()
        {
            bool error = false;
            if (this.checkData["checkBoxDL"])
            {
                TFileDescr document = new TFileDescr();
                List<ReadExcel.DetLock> source = this.excel.DetectorLockings(this.loadFiles["lblxlsDetLock"], ref document, ref error);
                this.Documents.Add(document);
                foreach (Block block in this.blocks
                                            .Where(x => x.XsdName == "Point" &&
                                                   x.IsOnCurrentArea && !x.IsOnNextStation && x.Visible))
                {
                    Block BlkPoint = block;
                    List<PointsPointTracksForDetectorLockingTrackforDetectorLocking> trackforDetectorLockingList =
                        new List<PointsPointTracksForDetectorLockingTrackforDetectorLocking>();
                    foreach (ReadExcel.DetLock detLock in source
                                                          .Where(x => x.Pt == BlkPoint.Attributes["NAME"].Value)
                                                          .ToList())
                    {
                        foreach (ReadExcel.DetLock.Adjacent adjacent in detLock.Adjacents)
                        {
                            List<string> pts = adjacent.Pts;
                            foreach (string str in pts)
                            {
                                string point = str;
                                Block Block = this.blocks
                                              .Where(x => x.XsdName == "Point" && x.Attributes["NAME"].Value == point)
                                              .FirstOrDefault();
                                if (Block != null)
                                {
                                    PointsPointTracksForDetectorLockingTrackforDetectorLocking trackforDetectorLocking =
                                        new PointsPointTracksForDetectorLockingTrackforDetectorLocking()
                                        {
                                            Value = this.blckProp.GetElemDesignation(Block, false, true)
                                        };
                                    trackforDetectorLockingList.Add(trackforDetectorLocking);
                                }
                                else
                                {
                                    ErrLogger.Error("Point for detector locking not found on SL",
                                        this.blckProp.GetElemDesignation(BlkPoint, false, true), "");
                                    error = true;
                                }
                            }
                            foreach (string tdt1 in adjacent.Tdts)
                            {
                                string tdt = tdt1;
                                if (this.blocks
                                        .Where(x => x.XsdName == "TrackSection" && x.Attributes["NAME"].Value == tdt)
                                        .FirstOrDefault() != null)
                                {
                                    PointsPointTracksForDetectorLockingTrackforDetectorLocking trackforDetectorLocking =
                                        new PointsPointTracksForDetectorLockingTrackforDetectorLocking()
                                        {
                                            Value = "tdt-" + this.stationID + "-" + tdt
                                        };
                                    trackforDetectorLockingList.Add(trackforDetectorLocking);
                                }
                                else
                                {
                                    AxleCounterSectionsAxleCounterSection axleCounterSection = this.acsections
                                                                                                    .Where(x => x.Designation == "tdt-" + this.stationID + "-" + tdt)
                                                                                                    .FirstOrDefault();
                                    if (axleCounterSection == null)
                                    {
                                        ErrLogger.Error("Detector locking section not found neither in Ac sections nor in track sections",
                                            this.blckProp.GetElemDesignation(BlkPoint, false, true), tdt);
                                        error = true;
                                    }
                                    else
                                    {
                                        foreach (string str in pts)
                                        {
                                            string checkPoint = str;
                                            if (!axleCounterSection.Elements.Element
                                                .Any(x => x.Value == "spsk-" + this.stationID + "-" + checkPoint))
                                            {
                                                ErrLogger.Error("No points found in TDL section", 
                                                    this.blckProp.GetElemDesignation(checkPoint), tdt);
                                                error = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    PointsPoint pointsPoint = this.points.Where((Func<PointsPoint, bool>)(x => x.Designation == this.blckProp.GetElemDesignation(BlkPoint, false, true))).FirstOrDefault();
                    if (pointsPoint == null)
                    {
                        ErrLogger.Error("Detector locking point not found in RDD points",
                            this.blckProp.GetElemDesignation(BlkPoint, false, true), "");
                        error = true;
                    }
                    else if (trackforDetectorLockingList.Count > 0)
                        pointsPoint.TracksForDetectorLocking = new PointsPointTracksForDetectorLocking()
                        {
                            TrackforDetectorLocking = trackforDetectorLockingList.ToArray()
                        };
                }
                return !error;
            }
            ErrLogger.Information("Track for detector locking data skipped", "TDL table");
            return false;
        }

        private bool LayerExists(string name)
        {
            using (Transaction acTrans = this.db.TransactionManager.StartTransaction())
            {
                LayerTable acLayTbl;
                acLayTbl = acTrans.GetObject(this.db.LayerTableId,
                                             OpenMode.ForRead) as LayerTable;
                foreach (ObjectId acObjId in acLayTbl)
                {
                    LayerTableRecord acLyrTblRec;
                    acLyrTblRec = acTrans.GetObject(acObjId,
                                                    OpenMode.ForRead) as LayerTableRecord;
                    if (acLyrTblRec.Name == name)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void Dispose()
        {
            ErrLogger.StopTmpLog();
        }
    }
}
