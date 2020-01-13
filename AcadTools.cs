using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System.Collections.Generic;
using System;

namespace ExportV2 
{
    public static class AcadTools
    {

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
