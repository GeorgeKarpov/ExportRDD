using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Refact.elements
{
    public class Point: SLElement
    {
        public LeftRightType Orient { get; set; }
        public LeftRightType BranchSide { get; set; }
        public TrackLine TipTrLine { get; set; }
        public TrackLine RightTrLine { get; set; }
        public TrackLine LeftTrLine { get; set; }
        public List<PointMachine> PointMachines { get; set; }
        public YesNoType Trailable { get; set; }
        public YesNoType PosIndicator { get; set; }
        public decimal KmpTip { get; set; }
        public decimal KmpRight { get; set; }
        public decimal KmpLeft { get; set; }
        public decimal KmpGapR { get; set; }
        public decimal KmpGapL { get; set; }
        public string LineIdTip { get; set; }
        public string LineIdRight { get; set; }
        public string LineIdLeft { get; set; }

        private bool hht;
        public Point(Block block, string stattionId) : base(block, stattionId)
        {
            Error = !base.Init();
            Error = !Init();
        }

        public override bool Init()
        {
            bool error = false;

            if (!decimal.TryParse(Attributes["KMP"].value, out decimal km))
            {
                ErrLogger.Error("Unable to parse KMP value from attribute", this.ElType.ToString(), this.Designation);
                error = true;
            }
            Location = km;
            KmpTip = km;

            if (Attributes.ContainsKey("KMP_CONTACT_2") && !string.IsNullOrEmpty(Attributes["KMP_CONTACT_2"].value))
            {
                if (!decimal.TryParse(Attributes["KMP_CONTACT_2"].value, out decimal kmR))
                {
                    ErrLogger.Error("Unable to parse " + Attributes["KMP_CONTACT_2"].name + 
                                        " value from attribute", this.ElType.ToString(), this.Designation);
                    error = true;
                }
                KmpRight = kmR;
            }
            else
            {
                KmpRight = KmpTip;
            }

            if (Attributes.ContainsKey("KMP_CONTACT_3") && !string.IsNullOrEmpty(Attributes["KMP_CONTACT_3"].value))
            {
                if (!decimal.TryParse(Attributes["KMP_CONTACT_3"].value, out decimal kmL))
                {
                    ErrLogger.Error("Unable to parse " + Attributes["KMP_CONTACT_3"].name +
                                        " value from attribute", this.ElType.ToString(), this.Designation);
                    error = true;
                }
                KmpLeft = kmL;
            }
            else
            {
                KmpLeft = KmpTip;
            }

            KmpGapR = KmpTip - KmpRight;
            KmpGapL = KmpTip - KmpLeft;

            GetpointMachines();
            Trailable = IsTrailable();
            PosIndicator = HasPosIndicator();
            Orient = GetPointOrient();
            BranchSide = GetBranchSide(out error);
            return !error;
        }

        private bool GetpointMachines()
        {
            bool error = false;
            Point2d cross = new Point2d();
            Point2d baseCross = new Point2d();
            Point2d hatchMidle = new Point2d();
            List<Hatch> pointMachines = new List<Hatch>();
            DBObjectCollection entset = new DBObjectCollection();
            this.Block.BlockReference.Explode(entset);
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
                        Line baseLine = line;
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
            double baseAngle = cross.GetVectorTo(baseCross).Angle;
            double pmAngle = cross.GetVectorTo(hatchMidle).Angle;
            double deltaPm =
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
            int count = pointMachines.Count;
            if (count == 0)
            {
                ErrLogger.Error("Point machines not found.", Designation, "");
                error = true;
                return error;
            }
            this.PointMachines = new List<PointMachine>();
            for (int i = 1; i <= count; i++)
            {
                this.PointMachines.Add(new PointMachine
                {
                    Designation = count == 1 ?
                                    Designation :
                                    Designation + "-" + i.ToString(),
                    TrackPosition = position
                });
            }
            return error;
        }

        private YesNoType IsTrailable()
        {
            DBObjectCollection entset = new DBObjectCollection();
            this.Block.BlockReference.Explode(entset);
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

        private YesNoType HasPosIndicator()
        {
            DBObjectCollection entset = new DBObjectCollection();
            Block.BlockReference.Explode(entset);
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
                        Calc.RndXY(line.Length, 3) == 2.165 ||
                        Calc.RndXY(line.Length, 4) == 4.7692)
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

        private LeftRightType GetPointOrient()
        {
            Line baseLine = GetPointBaseLine();
            DBObjectCollection entset = new DBObjectCollection();
            Block.BlockReference.Explode(entset);

            Extents3d ext = GetPointTipLine().GeometricExtents;
            Point2d cross = new Point2d(
            (ext.MinPoint.X + ext.MaxPoint.X) * 0.5,
            (ext.MinPoint.Y + ext.MaxPoint.Y) * 0.5
            );

            ext = baseLine.GeometricExtents;
            Point2d baseCross = new Point2d(
            (ext.MinPoint.X + ext.MaxPoint.X) * 0.5,
            (ext.MinPoint.Y + ext.MaxPoint.Y) * 0.5
            );

            double baseAngle = Calc.RadToDeg(cross.GetVectorTo(baseCross).Angle);
            if (Calc.Between(baseAngle, 90, 270))
            {
                return LeftRightType.left;
            }
            else
            {
                return LeftRightType.right;
            }
        }

        private LeftRightType GetBranchSide(out bool error)
        {
            error = false;
            Line baseLine = GetPointBaseLine();
            Line tip = GetPointTipLine();
            Line branchLine = GetPointBranchLine();

            LeftRightType branchSide = LeftRightType.left;

            if (tip == null)
            {
                ErrLogger.Error("Unable to find point's Acad entity", Designation, "tip");
            }
            if (baseLine == null)
            {
                ErrLogger.Error("Unable to find point's Acad entity", Designation, "base");
            }
            if (branchLine == null)
            {
                ErrLogger.Error("Unable to find point's Acad entity", Designation, "branch");
            }
            if (baseLine == null || tip == null || branchLine == null)
            {
                error = true;
                return branchSide;
            }
            Point3d crossPt = AcadTools.GetPointAtDist(tip, tip.StartPoint, tip.Length / 2);
            Point3d basePt = AcadTools.GetPointAtDist(baseLine, baseLine.StartPoint, baseLine.Length / 2);
            Point3d branchPt = AcadTools.GetPointAtDist(branchLine, branchLine.StartPoint, branchLine.Length / 2);
            Vector3d baseVector = basePt - crossPt;
            Vector3d branchVector = branchPt - crossPt;

            double angle = baseVector.GetAngleTo(branchVector, new Vector3d(0, 0, 1));

            if (ExtType == ExtType.derailer)
            {
                // SW agreement: derailer derails always in right
                branchSide =  LeftRightType.left;
            }
            else if (Calc.Between(angle, 3 / 2 * Math.PI, 2 * Math.PI, true))
            {
                branchSide = LeftRightType.right;
            }
            else
            {
                branchSide = LeftRightType.left;
            }
            return branchSide;
        }

        public bool SetPointRightLeftTrLines(List<TrackLine> trackLines)
        {
            Line baseLine = GetPointBaseLine();
            Line tip = GetPointTipLine();
            Line branchLine = GetPointBranchLine();

            if (tip == null)
            {
                ErrLogger.Error("Unable to find point's Acad entity", Designation, "tip");
            }
            if (baseLine == null)
            {
                ErrLogger.Error("Unable to find point's Acad entity", Designation, "base");
            }
            if (branchLine == null)
            {
                ErrLogger.Error("Unable to find point's Acad entity", Designation, "branch");
            }
            if (baseLine == null || tip == null || branchLine == null)
            {
                return false;
            }
            Point3d crossPt = AcadTools.GetPointAtDist(tip, tip.StartPoint, tip.Length / 2);
            Point3d basePt = AcadTools.GetPointAtDist(baseLine, baseLine.StartPoint, baseLine.Length / 2);
            Point3d branchPt = AcadTools.GetPointAtDist(branchLine, branchLine.StartPoint, branchLine.Length / 2);

            TrackLine baseTrLine = trackLines
                                   .Where(x => AcadTools.PointsAreEqual(x.line.GetClosestPointTo(basePt, false),basePt))
                                   .FirstOrDefault();

            TrackLine tipTrLine = trackLines
                                  .Where(x => (AcadTools.PointsAreEqual(x.line.GetClosestPointTo(crossPt, false), crossPt) ||
                                               AcadTools.ObjectsIntersects(x.line, tip, Intersect.OnBothOperands)) &&
                                               x != baseTrLine)
                                  .FirstOrDefault();
            TrackLine branchTrLine = trackLines
                                     .Where(x => AcadTools.LinesHasSameStartEnd(x.line, branchLine) &&
                                                  x != baseTrLine &&
                                                  x != tipTrLine)
                                     .FirstOrDefault();
            if (Exclude || NextStation)
            {
                if (tipTrLine == null && baseTrLine == null && branchTrLine == null)
                {
                    ErrLogger.Error("No Track lines for point found. " +
                        "At least one track line must be connected to point located on next station", Designation, "");
                    return false;
                }
            }
            else if (ExtType == ExtType.point && (tipTrLine == null ||
                     baseTrLine == null ||
                     branchTrLine == null) ||
                     ExtType == ExtType.derailer && (tipTrLine == null ||
                     baseTrLine == null))
            {
                ErrLogger.Error("Track line for point branch not found.", Designation, "");
                return false;
            }

            if (ExtType == ExtType.derailer)
            {
                // SW agreement: derailer derails always in right
                LeftTrLine = baseTrLine;
                TipTrLine = tipTrLine;
            }
            else if (BranchSide == LeftRightType.right)
            {
                LeftTrLine = baseTrLine;
                RightTrLine = branchTrLine;
                TipTrLine = tipTrLine;
            }
            else 
            {
                RightTrLine = baseTrLine;
                LeftTrLine = branchTrLine;
                TipTrLine = tipTrLine;
            }
            if (baseTrLine != null)
            {
                Branches.Add(baseTrLine);
            }
            if (branchTrLine != null)
            {
                Branches.Add(branchTrLine);
            }
            if (tipTrLine != null)
            {
                Branches.Add(tipTrLine);
            }       
            return true;
        }

        public bool SetLinesId()
        {
            bool error = false;
            if (TipTrLine != null)
            {
                LineIdTip = TipTrLine.LineID;
            }
            else if (!NextStation && !Exclude)
            {
                LineIdTip = "";
                ErrLogger.Error("Line Id for point not found", Designation, "tip");
                error = true;
            }
            if (LeftTrLine != null)
            {
                LineIdLeft = LeftTrLine.LineID;
            }
            else if (!NextStation && !Exclude)
            {
                LineIdLeft = "";
                ErrLogger.Error("Line Id for point not found", Designation, "left");
                error = true;
            }
            if (RightTrLine != null)
            {
                LineIdRight = RightTrLine.LineID;
            }
            else if (!NextStation && !Exclude && ExtType != ExtType.derailer)
            {
                LineIdRight = "";
                ErrLogger.Error("Line Id for point not found", Designation, "right");
                error = true;
            }
            return !error;
        }
        private Line GetPointBaseLine()
        {
            DBObjectCollection entset = new DBObjectCollection();
            Block.BlockReference.Explode(entset);
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

        private Line GetPointTipLine()
        {
            DBObjectCollection entset = new DBObjectCollection();
            Block.BlockReference.Explode(entset);
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

        private Line GetPointBranchLine()
        {
            DBObjectCollection entset = new DBObjectCollection();
            Block.BlockReference.Explode(entset);
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

        public void SetHht(List<Hht> hhts)
        {
            if (hhts.Any(x => AcadTools.PointsAreEqual(x.Position, Position, 1)))
            {
                hht = true;
            }
            else
            {
                hht = false;
            }
        }

        public KindOfPointType Kind()
        {
            if (hht)
            {
                if (ExtType == ExtType.derailer)
                {
                    return KindOfPointType.hhtDerailer;
                }
                else
                {
                    return KindOfPointType.hhtPoint;
                }
            }
            else
            {
                if (ExtType == ExtType.derailer)
                {
                    return KindOfPointType.derailer;
                }
                else if (ExtType == ExtType.trapPoint)
                {
                    return KindOfPointType.trapPoint;
                }
                else
                {
                    return KindOfPointType.point;
                }
            }
        }
    }
}
