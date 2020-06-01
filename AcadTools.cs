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

namespace ExpPt1
{
    public static class AcadTools
    {
        public static Point2d GetMiddlPoint2d(Extents3d extents3)
        {
            Point3d point3 = extents3.MinPoint +
                                    (extents3.MaxPoint -
                                    (extents3.MinPoint)) * 0.5;
            return new Point2d(point3.X, point3.Y);
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
            view.CenterPoint = point2dCenter;
            view.Height = zoomFactor; //(max2d.Y - min2d.Y) * 10;
            view.Width = (point2d - point2d1).Length + 20; // (max2d.X - min2d.X) * 10;
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

        public static bool LinesHasSamePoint(Line line1, Line line2)
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
    }
}
