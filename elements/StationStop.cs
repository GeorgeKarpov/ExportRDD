using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Refact.elements
{
    public class StationStop
    {
        public string StId { get; set; }
        public string StName { get; set; }
        public KindOfSASType Kind { get; set; }
        public List<RailwayLine> Lines { get; set; }
    }
}
