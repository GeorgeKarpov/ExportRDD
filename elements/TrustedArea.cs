using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Refact.elements
{
    public class TrustedArea
    {
        public string Id { get; set; }
        public List<TSeg> Tsegs { get; set; }
        public decimal Km1 { get; set; }
        public decimal Km2 { get; set; }
    }
}
