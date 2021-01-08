using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace Refact.elements
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
