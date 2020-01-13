using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Autodesk.AutoCAD.Runtime;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Autodesk.AutoCAD.Windows;
using System.Drawing;
using System;
using System.Text.RegularExpressions;

namespace ExportV2
{
    public class AcadSL: IDisposable
    {
        private DocumentCollection acDocMgr;
        private Database db;
        private Document acDoc;
        private string assemblyPath;
        private string dwgPath;
        private List<Block> blocks;
        private string  saveXmlTo;
        private InputData inputData;
        private List<TrackLine> tracksLines;
        private List<RailwayLine> railwayLines;
        private SigLayout sigLayout;
        private Dictionary<string, string> loadFiles;
        private Dictionary<string, bool> checkData;
        private string ZeroLevelLine;
        private string orderRddFileName;
        TFileDescr fileDescription;
        List<Point> points;
        List<LX> lxes;
        List<FoulPoint> foulPoints;
        public AcadSL(string dwgPath)
        {
            acDocMgr = AcadApp.DocumentManager;
            acDoc = acDocMgr.MdiActiveDocument;
            db = acDoc.Database;
            assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            dwgPath = Path.GetDirectoryName(db.Filename);
            blocks = new List<Block>();
            File.Delete(dwgPath + @"\Error.log");
            File.Delete(dwgPath + @"\Report.log");
            ErrLogger.filePath = dwgPath + @"\Error.log";
            Logger.filePath = dwgPath + @"\Report.log";
            ErrLogger.Start();
            Logger.Start();
            points = new List<Point>();
            lxes = new List<LX>();
            foulPoints = new List<FoulPoint>();
            tracksLines = new List<TrackLine>();
            railwayLines = new List<RailwayLine>();
        }
     
        public void Test()
        {
            if (!GetAcadBlocks())
            {
                AcadApp.ShowAlertDialog("No blocks found on drawing.");
                ErrLogger.Log("Program exit");
                return;
            }

            inputData = new InputData(assemblyPath, dwgPath, blocks);
            if (inputData.InitError)
            {
                AcadApp.ShowAlertDialog("Input data not found. See errors log.");
                ErrLogger.Log("Program exit");
                return;
            }

            if (!LoadSigLayout())
            {
                AcadApp.ShowAlertDialog("Unable to load Sig Layout.");
                ErrLogger.Log("Program exit");
                return;
            }
            else
            {
                inputData.PassStId(this.sigLayout.StId);
                if(!inputData.CheckLxPwsSxData())
                {
                    AcadApp.ShowAlertDialog("Unable to find LX external data. See error log.");
                    ErrLogger.Log("Program exit");
                    return;
                }
            }
            
            if (!GetSaveToXmlFileName())
            {
                Logger.Log("Program canceled by user.");
                return;
            }
            if (!GetTracksLines())
            {
                AcadApp.ShowAlertDialog("No track lines found.");
                ErrLogger.Log("Program exit");
                return;
            }

            if (!GetRailwayLines())
            {
                AcadApp.ShowAlertDialog("No railway lines found.");
                ErrLogger.Log("Program exit");
                return;
            }

            if (!StationDialog())
            {
                Logger.Log("Program canceled by user.");
                return;
            }
            if(!inputData.LoadData(loadFiles))
            {
                AcadApp.ShowAlertDialog("Unable to load external data. See errors log.");
                ErrLogger.Log("Program exit");
                return;
            }
            bool error = false;
            error = !LoadElements();
            RailwayDesignData railwayDesignData = new RailwayDesignData
            {
                version = "1.6.17",
                SchemaDocId = "7HA700001014_109EN",
                MetaData = new RailwayDesignDataMetaData
                {
                    FileDescription = fileDescription,
                    SignallingLayout = sigLayout.ConvertToRdd()
                }
            };
            RddXmlIO rddXml = new RddXmlIO();
            rddXml.WriteRddXml(railwayDesignData, saveXmlTo, new List<string> { "test Rdd" });
        }

        private bool GetSaveToXmlFileName()
        {
            SaveFileDialog saveFile = new SaveFileDialog("Save RDD", Path.GetDirectoryName(db.Filename)
                                    + "\\" + Constants.defaultFileName, "xml", "SaveRdd",
                                    SaveFileDialog.SaveFileDialogFlags.NoUrls);

            if (saveFile.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return false;
            }
            this.saveXmlTo = saveFile.Filename;
            return true;
        }

        private bool StationDialog()
        {
            FrmStation frmStation = new FrmStation
            {
                StationId = sigLayout.StId,
                StationName = sigLayout.StName
            };

            frmStation.Lines = railwayLines;
            if (File.Exists(Path.GetDirectoryName(db.Filename) + "//" + Path.GetFileNameWithoutExtension(db.Filename) + ".ini"))
            {
                loadFiles = File.ReadAllLines(Path.GetDirectoryName(db.Filename) + "//"
                                              + Path.GetFileNameWithoutExtension(db.Filename)
                                                + ".ini")
                            .Where(arg => !string.IsNullOrWhiteSpace(arg))
                            .ToDictionary(x => x.Split('\t')[0], x => x.Split('\t')[1]);
            }
            if (loadFiles != null)
            {
                frmStation.LoadFiles = loadFiles;
            }

            if (Application.ShowModalDialog(null, frmStation, true) == System.Windows.Forms.DialogResult.OK)
            {
                loadFiles = frmStation.LoadFiles;
                checkData = frmStation.CheckData;
                fileDescription = new TFileDescr
                {
                    version = frmStation.GetVersion(),
                    docID = frmStation.GetDocId(),
                    title = "PT1 Tables " + sigLayout.StName,
                    creator = "Georgijs Karpovs",
                    date = DateTime.Now
                };
                ZeroLevelLine = frmStation.ZeroLevelLine;
                orderRddFileName = frmStation.GetOrderRddFileName();
                File.WriteAllLines(Path.GetDirectoryName(db.Filename) + "//" + Path.GetFileNameWithoutExtension(db.Filename) + ".ini",
                    loadFiles.Select(x => (x.Key + '\t' + x.Value)).ToList());
            }
            else
            {
                return false;
            }
            return true;
        }

        private bool GetAcadBlocks()
        {
            using (Transaction trans = this.db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)trans.GetObject(this.db.BlockTableId, OpenMode.ForRead);
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
                        this.blocks.Add( new Block
                        {
                            BlkRef = (BlockReference)trans.GetObject(RefId, OpenMode.ForRead),
                            Name = btr.Name
                        });
                    }
                }
                trans.Commit();
            }
            if (this.blocks.Count == 0)
            {
                ErrLogger.Log("No any blocks found on drawing");
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool LoadSigLayout()
        {
            List<string> tempSig = inputData.BlocksToGet
                                               .Where(x => x.Value.Split('\t')[1] == "SignallingLayout")
                                               .Select(s => s.Key)
                                               .ToList();
            Block blkRef = this.blocks
                           .Where(x => tempSig.Contains(x.Name))
                           .FirstOrDefault();
            if (blkRef == null)
            {
                ErrLogger.Log("Signalling Layout block not found");
                return false;
            }
            else
            {
                this.sigLayout = new SigLayout(blkRef, inputData.BlocksToGet[blkRef.Name]);

                return !this.sigLayout.InitError;
            }
        }

        private bool LoadElements()
        {
            bool error = false;
            foreach (var block in this.blocks)
            {
                if (!inputData.BlocksToGet.ContainsKey(block.Name))
                {
                    Logger.Log("Block '" + block.Name + "' not found in BlkMap");
                    continue;
                }
                switch (inputData.BlocksToGet[block.Name].Split('\t')[1])
                {
                    case "Point":
                        Point point = new Point(block, inputData.BlocksToGet[block.Name]);
                        error = !point.GetElemDesignation(sigLayout.StId);
                        if (error)
                        {
                            ErrLogger.Log("Cannot get point designation of '" + point.Attributes["NAME"].Value + "'");
                        }
                        points.Add(point);
                        break;
                    case "FoulingPoint":
                        FoulPoint foulPoint = new FoulPoint(block, inputData.BlocksToGet[block.Name]);
                        foulPoints.Add(foulPoint);
                        break;
                    //case 'C':
                    //    Console.WriteLine("Well done");
                    //    break;
                    //case 'D':
                    //    Console.WriteLine("You passed");
                    //    break;
                    //case 'F':
                    //    Console.WriteLine("Better try again");
                    //    break;
                    default:
                        Logger.Log("Block '" + block.Name + "' have no implementation");
                        break;
                }
            }

            return !error;
        }

        //private bool LoadLxPwsSx()
        //{
        //    bool error = false;
        //    List<string> tempSig = FilterBlockNames("LevelCrossing");
        //    foreach (var block in this.blockReferences.Where(x => tempSig.Contains(x.Name)))
        //    {
        //        LX lx = new LX(block, inputData.BlocksToGet[block.Name]);
        //        error = !lx.GetElemDesignation(sigLayout.StId);
        //        if (error)
        //        {
        //            ErrLogger.Log("Cannot get point designation of '" + lx.Attributes["NAME"].Value + "'");
        //        }
        //        lxes.Add(lx);
        //    }
        //    return !error;
        //}

        private List<string> FilterBlockNames(string xsdName)
        {
            List<string> tempSig = this.inputData.BlocksToGet
                                               .Where(x => x.Value.Split('\t')[1] == xsdName)
                                               .Select(s => s.Key)
                                               .ToList();
            return tempSig;
        }

        private bool GetTracksLines()
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                var Linesids = AcadTools.GetObjectsOfType(db, RXObject.GetClass(typeof(Line)));
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
                        this.tracksLines.Add(new TrackLine
                        {
                            line = line,
                            color = color
                        });
                    }
                }

                var PolyLines = AcadTools.GetObjectsOfType(db, RXObject.GetClass(typeof(Polyline)));
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
                                this.tracksLines.Add(new TrackLine
                                {
                                    line = line,
                                    color = color
                                });
                            }
                        }
                    }
                }
                trans.Commit();
                if(tracksLines.Count == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                } 
            }
        }

        private bool GetRailwayLines()
        {
            
            RailwayLine DefaultRailwayLine = new RailwayLine();
            List<DBText> linesTexts = new List<DBText>();
            List<MText> linesMTexts = new List<MText>();

            string tmpLine = inputData.StationsDefinitions
                                  .Where(x => x.Key.ToLower() == sigLayout.StId)
                                  .Select(x => x.Value)
                                  .FirstOrDefault();
            string DefaultLine = inputData.LinesDefinitions
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
                    DefaultRailwayLine.designation = DefaultLine.Split('\t')[0];
                    DefaultRailwayLine.start = DefaultLine.Split('\t')[1];
                    DefaultRailwayLine.end = DefaultLine.Split('\t')[2];
                    DefaultRailwayLine.direction =
                        (DirectionType)Enum.Parse(typeof(DirectionType), DefaultLine.Split('\t')[3]);
                    DefaultRailwayLine.color = tracksLines
                                               .Select(x => x.color)
                                               .Distinct()
                                               .FirstOrDefault();
                    railwayLines.Add(DefaultRailwayLine);
                    foreach (var trackline in tracksLines)
                    {
                        trackline.direction = DefaultRailwayLine.direction;
                        trackline.LineID = DefaultRailwayLine.designation;
                    }
                }
                railwayLines = railwayLines.OrderBy(x => x.designation).ToList();
                return true;
            }

            if (linesTexts.Count > 0)
            {
                foreach (TrackLine trackline in tracksLines)
                {
                    DBText text = linesTexts
                                  .Where(x => AcadTools.ObjectsIntersects(trackline.line, x, Intersect.ExtendThis) &&
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
                        string line = inputData.LinesDefinitions
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
                    string line = inputData.LinesDefinitions
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
                foreach (var trackLine in tracksLines.Where(x => x.color == railwayLine.color))
                {
                    trackLine.direction = railwayLine.direction;
                    trackLine.LineID = railwayLine.designation;
                }
            }
            railwayLines = railwayLines.OrderBy(x => x.designation).ToList();
            return true;
        }

        public void Dispose()
        {
            ErrLogger.Stop();
            Logger.Stop();
        }

        /// <summary>
        /// Gets blocks defined in BlkMap.dat config from drawing.
        /// </summary>
        /// <returns>found blocks</returns>
        //private List<Block> GetBlocks(ref bool error)
        //{
        //    List<Block> Blocks = new List<Block>();
        //    blckProp = new BlockProperties("");
        //    using (Transaction trans = db.TransactionManager.StartTransaction())
        //    {
        //        BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
        //        foreach (ObjectId btrId in bt)
        //        {
        //            BlockTableRecord btr = (BlockTableRecord)trans.GetObject(btrId, OpenMode.ForRead);
        //            if (!btr.IsLayout && BlocksToGet.ContainsKey(btr.Name))
        //            {
        //                ObjectIdCollection aRefIds = new ObjectIdCollection();
        //                if (btr.IsDynamicBlock)
        //                {
        //                    var blockIds = btr.GetAnonymousBlockIds();
        //                    foreach (ObjectId BlkId in blockIds)
        //                    {
        //                        BlockTableRecord btr2 =
        //                            (BlockTableRecord)trans.GetObject(BlkId, OpenMode.ForRead, false, false);
        //                        ObjectIdCollection aRefIds2 = btr2.GetBlockReferenceIds(true, true);
        //                        foreach (ObjectId id in aRefIds2)
        //                        {
        //                            aRefIds.Add(id);
        //                        }
        //                    }
        //                    foreach (ObjectId id in btr.GetBlockReferenceIds(false, true))
        //                    {
        //                        aRefIds.Add(id);
        //                    }
        //                }
        //                else
        //                {
        //                    aRefIds = btr.GetBlockReferenceIds(false, true);
        //                }
        //                foreach (ObjectId RefId in aRefIds)
        //                {

        //                    BlockReference blkRef = (BlockReference)trans.GetObject(RefId, OpenMode.ForRead);
        //                    LayerTableRecord layer =
        //                        (LayerTableRecord)trans.GetObject(blkRef.LayerId, OpenMode.ForRead);
        //                    if (layer.IsFrozen)
        //                    {
        //                        continue;
        //                    }
        //                    Enum.TryParse(BlocksToGet[btr.Name].Split('\t')[1], out XType xType);

        //                    Dictionary<string, Attribute> Attributes = GetAttributes(blkRef);
        //                    Block block = new Block
        //                    {
        //                        BlkRef = blkRef,
        //                        BlockName = btr.Name,
        //                        ElType = BlocksToGet[btr.Name].Split('\t')[2],
        //                        XsdName = BlocksToGet[btr.Name].Split('\t')[1],
        //                        X = Math.Round(blkRef.Position.X, 0, MidpointRounding.AwayFromZero),
        //                        Y = Math.Round(blkRef.Position.Y, 0, MidpointRounding.AwayFromZero),
        //                        Rotation = (int)(blkRef.Rotation * (180 / Math.PI)),
        //                        KindOf = BlocksToGet[btr.Name].Split('\t')[3],
        //                        Attributes = Attributes,
        //                        IsOnCurrentArea = false,
        //                        Visible = !(Attributes.Any(x => x.Value.Name == "NAME" &&
        //                                                       x.Value.Visible == false))
        //                    };
        //                    if (!blkRef.BlockName.Contains("MODEL_SPACE"))
        //                    {
        //                        Blocks.Add(block);
        //                        continue;
        //                    }
        //                    if (block.XsdName == "Point" &&
        //                        block.Attributes["NAME"].Value.Contains("SN") &&
        //                        block.KindOf != "hhtDerailer")
        //                    {
        //                        block.KindOf = "hhtPoint";
        //                    }
        //                    if (block.XsdName != "Fouling Point" && block.Attributes.Any(x => x.Key.Contains("KMP")))
        //                    {
        //                        string[] attKeys = block.Attributes
        //                                       .Where(x => x.Key.Contains("KMP"))
        //                                       .OrderBy(x => x.Key)
        //                                       .Select(x => x.Key)
        //                                       .ToArray();
        //                        if (attKeys.Length > 0)
        //                        {
        //                            string location =
        //                            block.Attributes[attKeys[0]].Value;
        //                            if (!decimal.TryParse(location, out decimal loc))
        //                            {
        //                                if (location == "")
        //                                {
        //                                    loc = 0;
        //                                }
        //                                else
        //                                {
        //                                    error = true;
        //                                    ErrLogger.Log(blckProp.GetElemDesignation(block) +
        //                                        ": Can not convert '" + attKeys[0] + "' to decimal");
        //                                }
        //                            }
        //                            block.Location = loc;
        //                        }

        //                        if (attKeys.Length > 1)
        //                        {
        //                            string location =
        //                            block.Attributes[attKeys[1]].Value;
        //                            if (!decimal.TryParse(location, out decimal loc) && block.XsdName != "Point")
        //                            {
        //                                if (location == "")
        //                                {
        //                                    loc = 0;
        //                                }
        //                                else
        //                                {
        //                                    error = true;
        //                                    ErrLogger.Log(blckProp.GetElemDesignation(block) +
        //                                        ": Can not convert '" + attKeys[1] + "' to decimal");
        //                                }
        //                            }
        //                            block.Location2 = loc;
        //                        }

        //                        if (attKeys.Length > 2)
        //                        {
        //                            string location =
        //                            block.Attributes[attKeys[2]].Value;
        //                            if (!decimal.TryParse(location, out decimal loc) && block.XsdName != "Point")
        //                            {
        //                                if (location == "")
        //                                {
        //                                    loc = 0;
        //                                }
        //                                else
        //                                {
        //                                    error = true;
        //                                    ErrLogger.Log(blckProp.GetElemDesignation(block) +
        //                                        ": Can not convert '" + attKeys[2] + "' to decimal");
        //                                }
        //                            }
        //                            block.Location3 = loc;
        //                        }
        //                    }
        //                    else if (block.XsdName == "Fouling Point") // Fouling Point
        //                    {
        //                        string[] attKeys = block.Attributes
        //                                       .Where(x => x.Key.Contains("KMP"))
        //                                       .OrderBy(x => x.Key)
        //                                       .Select(x => x.Key)
        //                                       .ToArray();
        //                        string location =
        //                            block.Attributes[attKeys[0]].Value.Split('/').First();
        //                        string location2 =
        //                            block.Attributes[attKeys[0]].Value.Split('/').Last();
        //                        if (!decimal.TryParse(location, out decimal loc))
        //                        {
        //                            if (location == "")
        //                            {
        //                                loc = 0;
        //                            }
        //                            else
        //                            {
        //                                error = true;
        //                                ErrLogger.Log(blckProp.GetElemDesignation(block) +
        //                                    ": Can not convert '" + attKeys[0] + "' to decimal");
        //                            }
        //                        }
        //                        block.Location = loc;
        //                        if (!decimal.TryParse(location2, out decimal loc2))
        //                        {
        //                            if (location2 == "")
        //                            {
        //                                loc2 = 0;
        //                            }
        //                            else
        //                            {
        //                                error = true;
        //                                ErrLogger.Log(blckProp.GetElemDesignation(block) +
        //                                    ": Can not convert '" + attKeys[0] + "' to decimal");
        //                            }
        //                        }
        //                        block.Location2 = loc2;

        //                    }
        //                    Blocks.Add(block);
        //                }
        //            }
        //        }
        //        trans.Commit();
        //    }
        //    return Blocks;
        //}
    }
}


