using System.Collections.Generic;

namespace ExpRddApp.elements
{
    public class TrustedArea
    {
        public string Id { get; set; }
        public List<TSeg> Tsegs { get; set; }
        public decimal Km1 { get; set; }
        public decimal Km2 { get; set; }
    }
}
