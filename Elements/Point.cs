using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;

namespace ExportV2
{
    public class Point : SLElement
    {
        public decimal Location2;
        public decimal Location3;
        public string KindOf;
        public ConnectionBranchType Branch;
        public LeftRightType HorizOrient;
        public LeftRightType MachineLeftRight;
        public int PtMachines;
        public YesNoType Trailable;
        public YesNoType PosIndicator;

        public Point(Block blockReference, string BlkMap)
        {
            Block = blockReference;
            blkMap = BlkMap;
            InitError = !Init();
        }

        public override bool Init()
        {
            bool error = false;
            error = !base.Init();
            KindOf = this.blkMap.Split('\t')[3];
            if (this.Attributes["NAME"].Value.Contains("SN") &&
                this.KindOf != "hhtDerailer")
            {
                this.KindOf = "hhtPoint";
            }
            IsTrailable();
            GetPointMachines();
            GetBranch();
            GetOrient();
            GetPosIndicator();
            return !error;
        }

        private void IsTrailable()
        {
            DBObjectCollection entset = new DBObjectCollection();
            this.Block.BlkRef.Explode(entset);
            foreach (DBObject obj in entset)
            {
                if (obj.GetType() == typeof(Hatch))
                {
                    Hatch hatch = (Hatch)obj;
                    if (Calc.Between(hatch.Area, 2.35, 2.42))
                    {
                        this.Trailable = YesNoType.no;
                    }
                }
            }
        }

        private Line GetTipLine()
        {
            DBObjectCollection entset = new DBObjectCollection();
            this.Block.BlkRef.Explode(entset);
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

        private Line GetBaseLine()
        {
            DBObjectCollection entset = new DBObjectCollection();
            this.Block.BlkRef.Explode(entset);
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

        private Line GetBranchLine()
        {
            DBObjectCollection entset = new DBObjectCollection();
            this.Block.BlkRef.Explode(entset);
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

        private void GetPointMachines()
        {
            Line baseLine = GetBaseLine();
            Line tip = GetTipLine();
            double deltaPm = 0;
            double baseAngle = 0;
            double pmAngle = 0;
            Point2d cross = new Point2d();
            Point2d baseCross = new Point2d();
            Point2d hatchMidle = new Point2d();
            List<Hatch> pointMachines = new List<Hatch>();
            DBObjectCollection entset = new DBObjectCollection();
            this.Block.BlkRef.Explode(entset);

            Extents3d ext = tip.GeometricExtents;
            cross = new Point2d(
            (ext.MinPoint.X + ext.MaxPoint.X) * 0.5,
            (ext.MinPoint.Y + ext.MaxPoint.Y) * 0.5
            );

            ext = baseLine.GeometricExtents;
            baseCross = new Point2d(
            (ext.MinPoint.X + ext.MaxPoint.X) * 0.5,
            (ext.MinPoint.Y + ext.MaxPoint.Y) * 0.5
            );

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
                this.MachineLeftRight = LeftRightType.left;
            }
            else if (pmAngle > 0 && pmAngle >= (2 * Math.PI - deltaPm) && cross.Y > hatchMidle.Y)
            {
                this.MachineLeftRight = LeftRightType.right;
            }
            else if (pmAngle < baseAngle)
            {
                this.MachineLeftRight = LeftRightType.right;
            }
            else if (pmAngle > baseAngle)
            {
                this.MachineLeftRight = LeftRightType.left;
            }
            this.PtMachines = pointMachines.Count;
        }

        private void GetBranch()
        {
            char[] delimiter = "_".ToCharArray();
            string[] pointTypes = this.BlockName.Split(delimiter, 3, StringSplitOptions.RemoveEmptyEntries);

            //ConnectionBranchType branchType = new ConnectionBranchType();
            Line baseLine = GetBaseLine();
            Line tip = GetTipLine();
            Line branchLine = GetBranchLine();
            double deltaBranch = 0;
            double baseAngle = 0;
            double branchAngle = 0;
            Point2d cross = new Point2d();
            Point2d baseCross = new Point2d();
            Point2d branchCross = new Point2d();
            Extents3d ext = new Extents3d();
            DBObjectCollection entset = new DBObjectCollection();
            this.Block.BlkRef.Explode(entset);

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

            if (pointTypes[0].ToLower() == "afl")
            {
                Point3dCollection intersections = new Point3dCollection();
                baseLine.IntersectWith(branchLine, Intersect.ExtendThis, intersections, IntPtr.Zero, IntPtr.Zero);
                Vector2d AflBranch = new Vector2d();
                if (intersections != null && intersections.Count > 0)
                {
                    if (intersections[0].DistanceTo(branchLine.EndPoint) >
                        intersections[0].DistanceTo(branchLine.StartPoint))
                    {
                        AflBranch =
                            new Point2d(branchLine.EndPoint.X, branchLine.EndPoint.Y) -
                            new Point2d(branchLine.StartPoint.X, branchLine.StartPoint.Y);
                    }
                    else
                    {
                        AflBranch =
                            new Point2d(branchLine.StartPoint.X, branchLine.StartPoint.Y) -
                            new Point2d(branchLine.EndPoint.X, branchLine.EndPoint.Y);
                    }
                }
                Vector2d baseVector = cross.GetVectorTo(baseCross);
                branchAngle = AflBranch.GetAngleTo(baseVector);
                if (Calc.Between(Calc.RadToDeg(branchAngle), 44, 46, true))
                {
                    this.Branch = ConnectionBranchType.left;
                }
                else
                {
                    this.Branch = ConnectionBranchType.right;
                }
            }

            if (branchAngle > 0 && branchAngle < deltaBranch && cross.Y < branchCross.Y)
            {
                this.Branch = ConnectionBranchType.left;
            }
            else if (branchAngle > 0 && branchAngle >= (2 * Math.PI - deltaBranch) && cross.Y > branchCross.Y)
            {
                this.Branch = ConnectionBranchType.right;
            }
            else if (branchAngle < baseAngle)
            {
                this.Branch = ConnectionBranchType.right;
            }
            else if (branchAngle > baseAngle)
            {
                this.Branch = ConnectionBranchType.left;
            }
        }

        private void GetOrient()
        {
            Line baseLine = GetBaseLine();
            double baseAngle = 0;
            Point2d cross = new Point2d();
            Point2d baseCross = new Point2d();
            DBObjectCollection entset = new DBObjectCollection();
            this.Block.BlkRef.Explode(entset);

            Extents3d ext = GetTipLine().GeometricExtents;
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
                this.HorizOrient = LeftRightType.left;
            }
            else
            {
                this.HorizOrient = LeftRightType.right;
            }
        }

        private void GetPosIndicator()
        {
            DBObjectCollection entset = new DBObjectCollection();
            this.Block.BlkRef.Explode(entset);
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
                    this.PosIndicator = YesNoType.yes;
                    return;
                }

            }
            this.PosIndicator = YesNoType.no;
        }

        public override dynamic ConvertToRdd()
        {
            PointsPoint point = new PointsPoint
            {
                Designation = this.Designation,
                //Status = new TStatus {  status = StatusType.@new},
                KindOfPoint = (KindOfPointType)Enum.Parse(typeof(KindOfPointType), this.KindOf),
                //Lines = new PointsPointLines { Line = pointsPointLines.ToArray() },
                Trailable = this.Trailable,
                PointPosIndicator = PosIndicator,
                //PointMachines = new PointsPointPointMachines { PointMachine = pointMachines.ToArray() }
            };
            return point;

        }
    }
}
