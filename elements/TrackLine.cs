using Autodesk.AutoCAD.DatabaseServices;


namespace ExpRddApp.elements
{
    public class TrackLine
    {
        public Line line;
        public DirectionType direction;
        public TsegCalcDir calcDir;
        public string LineID;
        public System.Drawing.Color color;
    }
}
