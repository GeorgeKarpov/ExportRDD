using Autodesk.AutoCAD.Geometry;

namespace ExpRddApp.elements
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
