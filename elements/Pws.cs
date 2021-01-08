using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using System.Linq;

namespace Refact.elements
{
    
    public class Pws: SLElement
    {
        public List<LxTrack> Tracks { get; set; }
        public Pws(Block block, string stattionId) : base(block, stattionId)
        {
            Error = !base.Init();
        }
    }
}
