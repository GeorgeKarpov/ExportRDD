using System.Collections.Generic;

namespace ExpRddApp.elements
{
    public class StationStop
    {
        public string StId { get; set; }
        public string StName { get; set; }
        public KindOfSASType Kind { get; set; }
        public List<RailwayLine> Lines { get; set; }
    }
}
