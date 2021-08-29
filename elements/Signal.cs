using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpRddApp.elements
{
    public class Signal : SLElement
    {
        public TKindOfSignal KindOfSignal { get; set; }
        public DirectionType Direction { get; set; }
        public LeftRightOthersType TrackPosition { get; set; }
        public string Track { get; set; }
        public DangerPoint DangerPoint { get; set; }
        public ShiftCesBG ShiftCesBG { get; set; }
        public bool ToPSA { get; internal set; }

        public Signal(Block block, string stattionId) : base(block, stattionId)
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
            this.Location = km;
            TrackPosition = GetSignalTrackPosition(ref error);
            return !error;
        }

        private LeftRightOthersType GetSignalTrackPosition(ref bool error)
        {
            List<Line> lines = AcadTools.GetBlockLines(this.Block.BlockReference);
            Point2d cross = AcadTools.GetBlockCross(this.Block.BlockReference);
            Point2d fromTrackY = new Point2d(0, 0);
            fromTrackY = AcadTools.GetMiddlPoint2d(lines.Where(x => x.Length > 5 &&
                                                (x.StartPoint.X != cross.X) &&
                                                (x.StartPoint.Y != cross.Y) &&
                                                (x.EndPoint.X != cross.X) &&
                                                (x.EndPoint.Y != cross.Y))
                                          .FirstOrDefault()
                                          .GeometricExtents
                                     );

            if (this.Attributes.ContainsKey("EOTMB") && this.Attributes["EOTMB"].value.Equals("yes"))
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
                ErrLogger.Error("Unable to find track position of signal", this.Designation, "");
                error = true;
            }

            return LeftRightOthersType.others;
        }

        public bool SetSignalDirection()
        {
            if (TrackLine == null)
            {
                ErrLogger.Error("Unable to get direction. Track line not found", Designation, ElType.ToString());
                return false;
            }
            bool error = false;
            DirectionType sigDir;
            List<Line> lines = AcadTools.GetBlockLines(this.Block.BlockReference);
            Point2d cross = AcadTools.GetBlockCross(this.Block.BlockReference);

            DirectionType lineDir = this.TrackLine.direction;
            int SigLinesCount = 0;
            if (lineDir == DirectionType.up)
            {
                SigLinesCount = lines
                                .Where(x => x.GeometricExtents.MinPoint.X > cross.X + 2)
                                .ToList()
                                .Count();
            }
            else if (lineDir == DirectionType.down)
            {
                SigLinesCount = lines
                                .Where(x => x.GeometricExtents.MaxPoint.X < cross.X - 2)
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
            if (this.Attributes.ContainsKey("DIRECTION"))
            {
                Enum.TryParse(this.Attributes["DIRECTION"].value, out DirectionType dirTmp);
                if (sigDir != dirTmp)
                {
                    error = true;
                    ErrLogger.Error("Signal direction not match with attribute '", this.Designation,
                            "atts:" + dirTmp + " calc:" + sigDir);
                    ErrLogger.ErrorsFound = true;
                }
            }
            this.Direction = sigDir;
            return error;
        }

        public bool SetKindOfSig()
        {
            bool error = Enum.TryParse(this.ExtType.ToString(), out TKindOfSignal kind);
            this.KindOfSignal = kind;
            return error;
        }

        public decimal GetShiftOces()
        {
            if (KindOfSignal == TKindOfSignal.foreignSignal ||
                KindOfSignal == TKindOfSignal.eotmb ||
                KindOfSignal == TKindOfSignal.L2ExitSignal ||
                ToPSA)
            {
                return 0;
            }
            if (DangerPoint.Distance == 0 && ShiftCesBG.Distance == 0)
            {
                return 0;
            }
            double shift = (DangerPoint.Distance + ShiftCesBG.Distance) * 0.05 - DangerPoint.Distance + 16;
            decimal value = 0;
            if (shift > 0)
            {
                value = (int)Math.Ceiling(shift);
            }
            return value;
        }
    }
}
