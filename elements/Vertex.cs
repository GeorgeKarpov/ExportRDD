using System.Linq;

namespace ExpRddApp.elements
{
    public class Vertex
    {
        public string Id { get; set; }
        public SLElement Element { get; set; }
        public ConnectionBranchType Conn { get; set; }
        public decimal Km { get; set; }

        public bool Error { get; set; }

        private TrackLine branch;

        private VertNumber vertNumber;

        public Vertex(SLElement element, TrackLine branch, VertNumber vertNumber)
        {
            Id = element.Designation;
            Element = element;
            this.branch = branch;
            this.vertNumber = vertNumber;
            Conn = GetConnType();
            Km = GetVertKm();
        }

        private ConnectionBranchType GetConnType()
        {
            if (Element.ElType == XType.Point)
            {
                Point point = (Point)Element;
                if (branch == point.TipTrLine)
                {
                    return ConnectionBranchType.tip;
                }
                if (branch == point.LeftTrLine)
                {
                    return ConnectionBranchType.left;
                }
                if (branch == point.RightTrLine)
                {
                    return ConnectionBranchType.right;
                }
            }
            return ConnectionBranchType.none;
        }

        private decimal GetVertKm()
        {
            if (Element.ElType == XType.Connector)
            {
                if (vertNumber == VertNumber.start)
                {
                    return ((Connector)Element).Kmp1;
                }
                else
                {
                    return ((Connector)Element).Kmp2;
                }
            }
            else if (Element.ElType == XType.EndOfTrack)
            {
                return Element.Location;
            }
            else if (Element.ElType == XType.Point)
            {
                if (Conn == ConnectionBranchType.tip)
                {
                    return ((Point)Element).KmpTip;
                }
                else if (Conn == ConnectionBranchType.left)
                {
                    return ((Point)Element).KmpLeft;
                }
                else if (Conn == ConnectionBranchType.right)
                {
                    return ((Point)Element).KmpRight;
                }
            }
            ErrLogger.Error("Unable to get Vertex Km location", Element.Designation, "");
            Error = true;
            return 0;
        }

        public string UniqueId()
        {
            string id = "";
            if (Element.ElType == XType.Point || Element.ElType == XType.EndOfTrack)
            {
                id = Id + "-" + Conn; //.ToString();
            }
            else if (Element.ElType == XType.Connector)
            {
                id = Id + "-" + vertNumber; //.ToString();
            }
            return id;
        }

        public string GetTsegId()
        {
            string id = Id + "-";
            if (Element.ElType == XType.Point)
            {
                id += Conn.ToString().First().ToString().ToUpper(); //.ToString();
            }
            else if (Element.ElType == XType.EndOfTrack || Element.ElType == XType.Connector)
            {
                if (branch.direction == DirectionType.up)
                {
                    id += "R";
                }
                else if (branch.direction == DirectionType.down)
                {
                    id += "L";
                }

            }
            return id;
        }
    }
}
