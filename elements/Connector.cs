using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Refact.elements
{
    public class Connector: SLElement
    {
        public decimal Kmp1 { get; set; }
        public decimal Kmp2 { get; set; }
        public decimal KmpGap { get; set; }
        public TSeg Tseg1 { get; set; }
        public TSeg Tseg2 { get; set; }
        public Connector(Block block, string stattionId) : base(block, stattionId)
        {
            Error = !base.Init();
            Error = !Init();
        }

        public override bool Init()
        {
            bool error = false;

            if (!decimal.TryParse(Attributes["OKMP1"].value, out decimal km1))
            {
                ErrLogger.Error("Unable to parse OKMP1 value from attribute", ElType.ToString(), this.Designation);
                error = true;
            }
            Location = km1;
            Kmp1 = km1;

            if (!decimal.TryParse(Attributes["OKMP2"].value, out decimal km2))
            {
                ErrLogger.Error("Unable to parse OKMP2 value from attribute", ElType.ToString(), this.Designation);
                error = true;
            }
            Kmp2 = km2;

            if (decimal.TryParse(this.Attributes["KMGAP"].value, out decimal kmGap))
            {
                if (!kmGap.ToString().Contains('.'))
                {
                    kmGap *= 0.001M;
                }
                KmpGap = kmGap * -1;
            }
            return !error;
        }
    }
}
