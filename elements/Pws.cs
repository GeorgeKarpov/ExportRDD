using System.Collections.Generic;

namespace ExpRddApp.elements
{

    public class Pws : SLElement
    {
        public List<LxTrack> Tracks { get; set; }
        public Pws(Block block, string stattionId) : base(block, stattionId)
        {
            Error = !base.Init();
        }
    }
}
