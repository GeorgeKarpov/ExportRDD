using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Refact.elements
{
    public class EnclosedArea
    {
        public XType ElType { get; set; }
        public double MinX { get; set; }
        public double MinY { get; set; }
        public double MaxX { get; set; }
        public double MaxY { get; set; }

        public EnclosedArea(XType xType, double xMin, double yMin, double xMax, double yMax)
        {
            MinX = xMin;
            MinY = yMin;
            MaxX = xMax;
            MaxY = yMax;
            ElType = xType;
        }
    }

}
