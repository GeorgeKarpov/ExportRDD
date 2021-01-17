using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Refact
{
    public static class AcadTools
    {
        
        public static List<Block> GetBlocks(ref bool error, Database db, Dictionary<string, string> blocksToGet)
        {
            List<Block> Blocks = new List<Block>();

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                foreach (ObjectId btrId in bt)
                {
                    BlockTableRecord btr = (BlockTableRecord)trans.GetObject(btrId, OpenMode.ForRead);
                    if (!btr.IsLayout && blocksToGet.ContainsKey(btr.Name))
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

                            if (Enum.TryParse(blocksToGet[btr.Name].Split('\t')[1], out XType xType))
                            {
                                Blocks.Add(new Block
                                {
                                    BlockReference = blkRef,
                                    Xtype = xType,
                                    Visible = !layer.IsFrozen,
                                    BlkMap = blocksToGet[btr.Name]
                                });
                            }
                            else
                            {
                                ErrLogger.Error("Unable to parse block xType", blocksToGet[btr.Name].Split('\t')[1], "");
                                ErrLogger.ErrorsFound = true;
                                error = true;
                            }
                        }
                    }
                }
                trans.Commit();
            }
            return Blocks;
        }

        public static Point2d GetMiddlPoint2d(Extents3d extents3)
        {
            Point3d point3 = extents3.MinPoint +
                                    (extents3.MaxPoint -
                                    (extents3.MinPoint)) * 0.5;
            return new Point2d(point3.X, point3.Y);
        }

        public static Point3d GetMiddlPoint3d(Extents3d extents3)
        {
            Point3d point3 = extents3.MinPoint +
                                    (extents3.MaxPoint -
                                    extents3.MinPoint) * 0.5;
            return point3;
        }

        public static void ZoomToObjects(Entity ent, double zoomFactor)
        {
            Extents3d ext = ent.GeometricExtents;
            Autodesk.AutoCAD.ApplicationServices.Document doc =
                Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            ext.TransformBy(ed.CurrentUserCoordinateSystem.Inverse());

            Point2d min2d = new Point2d(ext.MinPoint.X - (zoomFactor / 2), ext.MinPoint.Y);
            Point2d max2d = new Point2d(ext.MaxPoint.X - (zoomFactor / 2), ext.MaxPoint.Y);

            ViewTableRecord view = new ViewTableRecord();
            if (ent.GetType() == typeof(BlockReference))
            {
                view.CenterPoint = new Point2d(((BlockReference)ent).Position.X - (zoomFactor / 2), ((BlockReference)ent).Position.Y);
            }
            else
            {
                view.CenterPoint = min2d + ((max2d - min2d) / 2.0);
            }
            view.Height = zoomFactor; //(max2d.Y - min2d.Y) * 10;
            view.Width = zoomFactor; // (max2d.X - min2d.X) * 10;
            try
            {
                ed.SetCurrentView(view);
            }
            catch
            {
                return;
            }         
            ObjectId[] ids = new[] { ent.Id };
            ed.SetImpliedSelection(ids);
        }

        public static void ZoomToObjects(Entity[] entities, double zoomFactor)
        {
            Extents3d ext = entities[0].GeometricExtents;
            Extents3d ext1 = entities[1].GeometricExtents;
            Autodesk.AutoCAD.ApplicationServices.Document doc =
                Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            ext.TransformBy(ed.CurrentUserCoordinateSystem.Inverse());
            ext1.TransformBy(ed.CurrentUserCoordinateSystem.Inverse());

            Point2d point2d;
            Point2d point2d1;
            if (entities[0].GetType() == typeof(BlockReference))
            {
                point2d = new Point2d(((BlockReference)entities[0]).Position.X, 
                                      ((BlockReference)entities[0]).Position.Y);
            }
            else
            {
                point2d = GetMiddlPoint2d(entities[0].GeometricExtents);
            }
            if (entities[1].GetType() == typeof(BlockReference))
            {
                point2d1 = new Point2d(((BlockReference)entities[1]).Position.X,
                                      ((BlockReference)entities[1]).Position.Y);
            }
            else
            {
                point2d1 = GetMiddlPoint2d(entities[1].GeometricExtents);
            }

            Vector2d center = point2d.GetVectorTo(point2d1);
            Point2d point2dCenter = point2d + center * 0.5;

            ViewTableRecord view = new ViewTableRecord();
            view.CenterPoint = new Point2d(point2dCenter.X - zoomFactor / 2, point2dCenter.Y);
            view.Height = zoomFactor; //(max2d.Y - min2d.Y) * 10;
            view.Width = (point2d - point2d1).Length + zoomFactor / 2; // (max2d.X - min2d.X) * 10;
            try
            {
                ed.SetCurrentView(view);
            }
            catch
            {
                return;
            }
            ObjectId[] ids = new[] { entities[0].Id, entities[1].Id };
            ed.SetImpliedSelection(ids);
        }

        public static List<DBPoint> GetAcadPoints(Database db)
        {
            List<DBPoint> dBPoints = new List<DBPoint>();
            var PointsIds = GetObjectsOfType(db, RXObject.GetClass(typeof(DBPoint)));
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                foreach (ObjectId ObjId in PointsIds)
                {
                    DBPoint point = (DBPoint)trans.GetObject(ObjId, OpenMode.ForRead);
                    dBPoints.Add(point);
                }
            }        
            return dBPoints;
        }
        public static bool UpdateAttribute(Database db, BlockReference blockRef, string attTag, string Value)
        {
            bool success = false;
            using (Transaction acTrans = db.TransactionManager.StartTransaction())
            {
                AttributeCollection atts = blockRef.AttributeCollection;
                foreach (ObjectId arId in atts)
                {
                    AttributeReference attRef =
                        (AttributeReference)acTrans.GetObject(arId, OpenMode.ForRead);
                    if (attRef.Tag == attTag.ToUpper())
                    {
                        attRef.UpgradeOpen();
                        attRef.TextString = Value;
                        attRef.DowngradeOpen();
                        success = true;
                        break;
                    }
                }
                acTrans.Commit();
            }
            return success;
        }
        
        public static void CopyBlockFromFile(string FilePath, string BlockName, Database db)

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
        public static ObjectId InsertBlock(string BlockName, double x, double y, Database db)
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

        public static bool DistBetweenPoints(Point3d point1, Point3d point2, double offset)
        {
            if ((point1 - point2).Length <= offset)
            {
                return true;
            }
            return false;
        }

        public static bool IsPointOnCurve(Curve cv, Point3d pt, double tolerance)

        {
            pt = new Point3d(pt.X, pt.Y, 0);
            try
            {
                Point3d p = cv.GetClosestPointTo(pt, false);
                return (p - pt).Length <= tolerance;
            }
            catch { }
            return false;
        }

        public static IEnumerable<ObjectId> GetObjectsOfType(Database db, RXClass cls)
        {
            using (var tr = db.TransactionManager.StartOpenCloseTransaction())
            {
                var btr = (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db),
                                                            OpenMode.ForRead);
                foreach (ObjectId id in btr)
                {
                    if (id.ObjectClass.IsDerivedFrom(cls))
                    {
                        yield return id;
                    }
                }
                btr = (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockPaperSpaceId(db),
                                                            OpenMode.ForRead);
                foreach (ObjectId id in btr)
                {
                    if (id.ObjectClass.IsDerivedFrom(cls))
                    {
                        yield return id;
                    }
                }
                tr.Commit();
            }
        }

        public static IEnumerable<Point3d> GetVertexPoints(this Curve pline)
        {
            if (pline == null)
                throw new ArgumentNullException("pline");
            if (!((pline is Polyline) || (pline is Polyline2d) || pline is Polyline3d))
                throw new Autodesk.AutoCAD.Runtime.Exception(ErrorStatus.WrongObjectType);
            if (pline.EndParam % 1.0 > 1.0e-10)
                throw new Autodesk.AutoCAD.Runtime.Exception(ErrorStatus.InvalidInput);
            int vertices = ((int)pline.EndParam) + (pline.Closed ? 0 : 1);
            for (int i = 0; i < vertices; i++)
                yield return pline.GetPointAtParameter(i);
        }

        public static Line CreateTempLine(Line line)
        {
            Line newLine = new Line
                        (
                            new Point3d(line.StartPoint.X, line.StartPoint.Y, 0),
                            new Point3d(line.EndPoint.X, line.EndPoint.Y, 0)
                        );
            newLine.SetDatabaseDefaults();
            newLine.Layer = line.Layer;
            return newLine;
        }

        public static Line CreateTempLine(Polyline polyline)
        {
            Line newLine = new Line
                       (
                           new Point3d(polyline.StartPoint.X, polyline.StartPoint.Y, 0),
                           new Point3d(polyline.EndPoint.X, polyline.EndPoint.Y, 0)
                       );
            newLine.SetDatabaseDefaults();
            newLine.Layer = polyline.Layer;
            return newLine;
        }

        public static Line CreateTempLine(Polyline2d polyline)
        {
            Line newLine = new Line
                       (
                           new Point3d(polyline.StartPoint.X, polyline.StartPoint.Y, 0),
                           new Point3d(polyline.EndPoint.X, polyline.EndPoint.Y, 0)
                       );
            newLine.SetDatabaseDefaults();
            newLine.Layer = polyline.Layer;
            return newLine;
        }

        public static Polyline CreateTempPolyline(Polyline polyline)
        {
            Polyline newPline = new Polyline();
            newPline.SetDatabaseDefaults();
            newPline.Layer = polyline.Layer;
            for (int j = 0; j < polyline.NumberOfVertices; j++)
            {
                Point2d point = polyline.GetPoint2dAt(j);
                newPline.AddVertexAt(j, point, 0, 0, 0);
            }
            return newPline;
        }

        public static Polyline CreateTempPolyline(Polyline2d polyline)
        {
            Polyline newPline = new Polyline();
            newPline.SetDatabaseDefaults();
            newPline.Layer = polyline.Layer;
            List<Point3d> vertices = AcadTools.GetVertexPoints(polyline).ToList();
            for (int j = 0; j < vertices.Count; j++)
            {
                Point2d point = new Point2d(vertices[j].X, vertices[j].Y);
                newPline.AddVertexAt(j, point, 0, 0, 0);
            }
            return newPline;
        }

        public static Polyline CreateTempPolyline(Line line)
        {
            Polyline newPline = new Polyline();
            newPline.SetDatabaseDefaults();
            newPline.Layer = line.Layer;
            Point2d point = new Point2d(line.StartPoint.X, line.StartPoint.Y);
            newPline.AddVertexAt(0, point, 0, 0, 0);
            point = new Point2d(line.EndPoint.X, line.EndPoint.Y);
            newPline.AddVertexAt(1, point, 0, 0, 0);
            return newPline;
        }

        public static bool ObjectsIntersects(Entity Ent1, Entity Ent2, Intersect intersect, bool checkWholeBlock = false)
        {
            Point3dCollection intersections;
            if (Ent1.GetType() == typeof(BlockReference))
            {
                throw new WrongArgUsageException("First entity must be not block reference");
            }
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
                                if (LinesHasSameStartEnd((Line)obj, (Line)Ent1))
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
                    return LinesHasSameStartEnd((Line)Ent1, (Line)Ent2); ;
                }
                else
                {
                    return false;
                }
            }
        }

        public static bool LinesHasSameStartEnd(Line line1, Line line2)
        {
            if (PointsAreEqual(line1.StartPoint, line2.StartPoint))
            {
                return true;
            }
            if (PointsAreEqual(line1.EndPoint, line2.EndPoint))
            {
                return true;
            }
            if (PointsAreEqual(line1.StartPoint, line2.EndPoint))
            {
                return true;
            }
            if (PointsAreEqual(line1.EndPoint, line2.StartPoint))
            {
                return true;
            }
            return false;
        }

        public static bool LinesHasSameStartEnd(Line line1, Line line2, out Point3d samePt)
        {
            samePt = new Point3d();
            if (PointsAreEqual(line1.StartPoint, line2.StartPoint))
            {
                samePt = line1.StartPoint;
                return true;
            }
            if (PointsAreEqual(line1.EndPoint, line2.EndPoint))
            {
                samePt = line1.EndPoint;
                return true;
            }
            if (PointsAreEqual(line1.StartPoint, line2.EndPoint))
            {
                samePt = line1.StartPoint;
                return true;
            }
            if (PointsAreEqual(line1.EndPoint, line2.StartPoint))
            {
                samePt = line1.EndPoint;
                return true;
            }
            return false;
        }

        public static bool PointsAreEqual(Point3d point1, Point3d point2, double tolerance = Constants.pointsEqTol)
        {
            if ((point1 - point2).Length <= tolerance)
            {
                return true;
            }
            return false;
        }

        public static List<Line> GetBlockLines(BlockReference blkRef)
        {
            DBObjectCollection entset = new DBObjectCollection();
            blkRef.Explode(entset);
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

        public static Point2d GetBlockCross(BlockReference block)
        {
            DBObjectCollection entset = new DBObjectCollection();
            block.Explode(entset);
            List<Line> tmpCross = new List<Line>();
            // if cross not found take insertion point of block
            Point2d cross = new Point2d(block.Position.X, block.Position.Y);
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

        public static bool LineBelongsToBeam(Line beamLine, Line refLine)
        {
            Vector2d vector1 = new Vector2d(beamLine.GeometricExtents.MaxPoint.X - beamLine.GeometricExtents.MinPoint.X,
                                            beamLine.GeometricExtents.MaxPoint.Y - beamLine.GeometricExtents.MinPoint.Y);
            Vector2d vector2 = new Vector2d(refLine.GeometricExtents.MaxPoint.X - beamLine.GeometricExtents.MinPoint.X,
                                            refLine.GeometricExtents.MaxPoint.Y - beamLine.GeometricExtents.MinPoint.Y);
            Vector2d vector3 = new Vector2d(refLine.GeometricExtents.MinPoint.X - beamLine.GeometricExtents.MinPoint.X,
                                           refLine.GeometricExtents.MinPoint.Y - beamLine.GeometricExtents.MinPoint.Y);

            return Math.Abs((vector1.X * vector2.Y) - (vector2.X * vector1.Y)) <= Constants.beamLineTol &&
                   Math.Abs((vector1.X * vector3.Y) - (vector3.X * vector1.Y)) <= Constants.beamLineTol;
        }

        public static double GetBeamAngleToPoint(Line line1, Point3d point3d)
        {
            Vector3d vector1 = new Vector3d(line1.GeometricExtents.MaxPoint.X - line1.GeometricExtents.MinPoint.X,
                                            line1.GeometricExtents.MaxPoint.Y - line1.GeometricExtents.MinPoint.Y,
                                            line1.GeometricExtents.MaxPoint.Z - line1.GeometricExtents.MinPoint.Z);
            Vector3d vector2 = new Vector3d(point3d.X - line1.GeometricExtents.MinPoint.X,
                                            point3d.Y - line1.GeometricExtents.MinPoint.Y,
                                            point3d.Z - line1.GeometricExtents.MinPoint.Z);
            return vector1.GetAngleTo(vector2, new Vector3d(0, 0, 1));
        }

        public static double GetBeamAngleToPoint(Point3d point1, Point3d point2, Point3d pointTo)
        {
            Vector3d vector1 = point2 - point1;
            Vector3d vector2 = pointTo - point1;
            return vector1.GetAngleTo(vector2, new Vector3d(0, 0, 1));
        }

        public static Point3d GetPointAtDist(Line line, Point3d initPt, double distance)
        {
            Point3d point = new Point3d(0, 0, 0);
            if (line.Length < distance)
            {
                point = line.GetPointAtDist(line.Length);
            }
            else if (line.StartPoint.IsEqualTo(initPt, Tolerance.Global))
            {
                point = line.GetPointAtDist(distance);
            }
            else if (line.EndPoint.IsEqualTo(initPt, Tolerance.Global))
            {
                point = line.GetPointAtDist(line.Length - distance);
            }
            else
            {
                point = line.GetPointAtDist(line.GetDistAtPoint(initPt) + distance);
            }
            return point;
        }

        public static List<Line> SplitLineOnPoint(Line splitLine, Point3d splitPoint)
        {
            Point3dCollection points = new Point3dCollection { splitPoint };
            List<Line> newLines = new List<Line>();
            DBObjectCollection result;
            try
            {
                result = splitLine.GetSplitCurves(points);
            }
            catch
            {
                return new List<Line> { splitLine };
            }
            foreach (var item in result)
            {
                newLines.Add((Line)item);                         
            }
            return newLines;
        }

        public static IEnumerable<MText> GetMtextsByRegex(string regexString, Database db)
        {
            Regex regex = new Regex(regexString);
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                var Textsids = GetObjectsOfType(db, RXObject.GetClass(typeof(MText)));
                foreach (ObjectId ObjId in Textsids)
                {
                    var mtext = (MText)trans.GetObject(ObjId, OpenMode.ForRead);
                    if (regex.IsMatch(mtext.Text))
                    {
                        yield return mtext;
                    }
                }
                Textsids = GetObjectsOfType(db, RXObject.GetClass(typeof(DBText)));
                foreach (ObjectId ObjId in Textsids)
                {
                    var dbtext = (DBText)trans.GetObject(ObjId, OpenMode.ForRead);
                    if (regex.IsMatch(dbtext.TextString))
                    {
                        MText mText = new MText();
                        mText.SetDatabaseDefaults();
                        mText.Contents = dbtext.TextString;
                        mText.Location = dbtext.Position;
                        yield return mText;
                    }
                }
                trans.Commit();
            }
        }

        public static IEnumerable<Line> GetLinesByLayer(string layer, Database db, double length = 0)
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                var Linesids = GetObjectsOfType(db, RXObject.GetClass(typeof(Line)));
                foreach (ObjectId ObjId in Linesids)
                {
                    Line line = (Line)trans.GetObject(ObjId, OpenMode.ForRead);
                    if (line.Layer == layer && line.Length > length)
                    {
                        yield return line;
                    }
                }

                var PolyLines = GetObjectsOfType(db, RXObject.GetClass(typeof(Polyline)));
                foreach (ObjectId ObjId in PolyLines)
                {
                    Polyline polyline = (Polyline)trans.GetObject(ObjId, OpenMode.ForRead);
                    if (polyline.Layer == layer)
                    {
                        DBObjectCollection entset = new DBObjectCollection();
                        polyline.Explode(entset);
                        foreach (DBObject obj in entset)
                        {
                            if (obj.GetType() == typeof(Line))
                            {
                                Line line = (Line)obj;
                                if (line.Layer == layer && line.Length > length)
                                {
                                    yield return line;
                                }
                            }
                        }
                    }
                }
                trans.Commit();
            }
        }

        public static IEnumerable<Line> GetLinesByLayer(Regex layer, Database db, double length = 0)
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                var Linesids = GetObjectsOfType(db, RXObject.GetClass(typeof(Line)));
                foreach (ObjectId ObjId in Linesids)
                {
                    Line line = (Line)trans.GetObject(ObjId, OpenMode.ForRead);
                    if (layer.IsMatch(line.Layer) && line.Length > length)
                    {
                        yield return line;
                    }
                }

                var PolyLines = GetObjectsOfType(db, RXObject.GetClass(typeof(Polyline)));
                foreach (ObjectId ObjId in PolyLines)
                {
                    Polyline polyline = (Polyline)trans.GetObject(ObjId, OpenMode.ForRead);
                    if (layer.IsMatch(polyline.Layer))
                    {
                        DBObjectCollection entset = new DBObjectCollection();
                        polyline.Explode(entset);
                        foreach (DBObject obj in entset)
                        {
                            if (obj.GetType() == typeof(Line))
                            {
                                Line line = (Line)obj;
                                if (layer.IsMatch(line.Layer) && line.Length > length)
                                {
                                    yield return line;
                                }
                            }
                        }
                    }
                }
                trans.Commit();
            }
        }

        public static LayerTableRecord GetLayerById(ObjectId id, Database db)
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                LayerTableRecord layer =
                        (LayerTableRecord)trans.GetObject(id, OpenMode.ForRead);
                trans.Commit();
                return layer;
            }          
        }

        public static void CopyAtributtes(BlockReference fromBlkRef, BlockReference toBlkRef, Transaction acTrans)
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

        public static bool LayerExists(string name, Database db)
        {
            using (Transaction acTrans = db.TransactionManager.StartTransaction())
            {
                LayerTable acLayTbl;
                acLayTbl = acTrans.GetObject(db.LayerTableId,
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

        public static void Message(string msg)
        {
            Autodesk.AutoCAD.ApplicationServices.Core.Application.ShowAlertDialog(msg);
        }
    }
}
