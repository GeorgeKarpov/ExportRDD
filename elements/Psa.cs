using Autodesk.AutoCAD.DatabaseServices;


namespace ExpRddApp.elements
{
    public class Psa
    {
        public string Id { get; set; }
        public decimal Begin { get; set; }
        public double MinX { get; set; }
        public double MinY { get; set; }
        public double MaxX { get; set; }
        public double MaxY { get; set; }
        public Polyline Polyline { get; set; }
        public TSeg Tseg { get; set; }

        public Psa(string id, double xMin, double yMin, double xMax, double yMax, Polyline polyline)
        {
            MinX = xMin;
            MinY = yMin;
            MaxX = xMax;
            MaxY = yMax;
            Id = id.ToLower();
            Polyline = polyline;
        }
    }
}
