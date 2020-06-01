using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpPt1
{
    public class DangerPoint
    {
        public string Id { get; set; }
        public decimal Distance { get; set; }
        public bool DistanceSpecified { get; set; } = false;
    }
}
