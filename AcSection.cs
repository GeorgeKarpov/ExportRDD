using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpPt1
{
    public class AcSection
    {
        public string Designation { get; set; }
        public List<Block> Dps { get; set; }
        public List<string> Elements { get; set; }
        List<Block> AcElements { get; set; }
        private List<TrackLine> tracksLines;
        List<TrackSegmentTmp> trackSegmentsTmp;
        List<Block> blocks;
        //public AcSection(List<Block> dps, List<TrackLine> tracksLines, List<TrackSegmentTmp> trackSegmentsTmp, List<Block> blocks)
        //{
        //    this.Dps = dps;
        //    this.tracksLines = tracksLines;
        //    this.trackSegmentsTmp = trackSegmentsTmp;
        //    this.blocks = blocks;
        //}
    }
}
