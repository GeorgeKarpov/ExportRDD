using Autodesk.AutoCAD.Geometry;

namespace ExpRddApp.elements
{
    public class Track
    {
        public string Id { get; set; }
        public Point3d Position { get; set; }
        public bool Main { get; set; }
    }
}
