using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Refact.elements
{
    public class FoulingPoint: SLElement
    {
        public bool LineChanges { get; set; }
        public decimal SecondLocation { get; set; }
        public FoulingPoint(Block block, string stattionId) : base(block, stattionId)
        {
            Error = !base.Init();
            Error = !Init();
        }

        public override bool Init()
        {
            bool error = false;

            //TODO implement locations for line change
            string[] kms = Attributes["KMP"].value.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
            if (kms == null || kms.Length == 0 || kms.Length > 2)
            {
                ErrLogger.Error("Unable to parse KMP value from attribute", ElType.ToString(), this.Designation);
                error = true;
                return !error;
            }
            if (!decimal.TryParse(kms[0], out decimal km))
            {
                ErrLogger.Error("Unable to parse KMP value from attribute", ElType.ToString(), this.Designation);
                error = true;
            }
            Location = km;
            if (kms.Length == 2)
            {
                LineChanges = true;
                if (!decimal.TryParse(kms[1], out km))
                {
                    ErrLogger.Error("Unable to parse KMP value from attribute", ElType.ToString(), this.Designation);
                    error = true;
                }
                SecondLocation = km;
            }
            return !error;
        }

        public decimal GetClosestLocation(decimal input)
        {
            List<decimal> vs = new List<decimal>
            {
                this.Location,
                this.SecondLocation
            };
            return vs.Aggregate((x, y) => Math.Abs(x - input) < Math.Abs(y - input) ? x : y);
        }
    }
}
