using System.Collections.Generic;

namespace ExpRddApp.elements
{
    public class LevelCrossing : SLElement
    {
        public List<LxTrack> Tracks { get; set; }
        public LevelCrossing(Block block, string stattionId) : base(block, stattionId)
        {
            Error = !base.Init();
        }
    }
}
