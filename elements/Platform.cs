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

        public PlatformHeightType Height(int number)
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
    }
}
