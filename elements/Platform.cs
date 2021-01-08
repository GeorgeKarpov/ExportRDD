using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Refact.elements
{
    public class Platform: SLElement
    {
        public List<TSeg> Tsegs { get; set; }
        public decimal Km1 { get; set; }
        public decimal Km2 { get; set; }
        public LeftRightType PositionOfPlatform { get; set; }
        public UpDownBothType TrainDirection { get; set; }
        public PlatformHeightType Height { get; set; }
        public string Track { get; set; }
        public Platform(Block block, string stattionId) : base(block, stattionId)
        {
            Error = !base.Init();
            Error = !Init();
        }

        public override bool Init()
        {
            bool error = false;
            List<decimal> kms = new List<decimal>();
            if (!decimal.TryParse(Attributes["START_PLAT_1"].value, out decimal km1))
            {
                ErrLogger.Error("Unable to parse START_PLAT_1 value from attribute", this.ElType.ToString(), this.Designation);
                error = true;
            }
            kms.Add(km1);

            if (!decimal.TryParse(Attributes["END_PLAT_1"].value, out decimal km2))
            {
                ErrLogger.Error("Unable to parse END_PLAT_1 value from attribute", this.ElType.ToString(), this.Designation);
                error = true;
            }
            kms.Add(km2);
            kms = kms
                  .OrderBy(x => x)
                  .ToList();
            this.Km1 = kms[0];
            this.Km2 = kms[1];
            if (!Enum.TryParse(Attributes["POSITION_PLAT"].value.ToString().ToLower(), out LeftRightType position))
            {
                ErrLogger.Error("Unable to parse POSITION_PLAT attribute value", this.ElType.ToString(), this.Designation);
                error = true;
            }
            PositionOfPlatform = position;

            if (!Enum.TryParse(Attributes["DIRECTION_PLAT"].value.ToString().ToLower(), out UpDownBothType direction))
            {
                ErrLogger.Error("Unable to parse DIRECTION_PLAT attribute value", this.ElType.ToString(), this.Designation);
                error = true;
            }
            TrainDirection = direction;
            return !error;
        }
    }
}
