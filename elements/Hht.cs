using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Refact.elements
{
    public class Hht
    {
        public Point3d Position { get; set; }
        public Block Block { get; set; }

        public Hht(Block block)
        {
            Block = block;
            Position = block.BlockReference.Position;
        }
    }
}
