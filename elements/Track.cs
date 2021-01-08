using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Refact.elements
{
    public class Track
    {
        public string Id { get; set; }
        public Point3d Position { get; set; }
        public bool Main { get; set; }
    }
}
