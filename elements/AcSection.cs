using System.Collections.Generic;

namespace ExpRddApp.elements
{

    /// <summary>Represents Railway Ac Section</summary>
    public class AcSection : SLElement
    {
        /// <summary>
        /// Detection Points of Ac section.
        /// </summary>
        /// <remarks>
        /// Includes EOT for Ac sections next to end of track
        /// and no detection point in eot direction found.
        /// </remarks>
        public List<SLElement> DetectionPoints { get; set; }
        /// <summary>Elements of Ac section.</summary>
        /// <remarks>Includes points existing inside section.</remarks>
        public List<SLElement> Elements { get; set; }
        public AcSection(Block block, string stattionId) : base(block, stattionId)
        {
            Error = !base.Init();
            //InitError = !Init();
        }
    }
}
