using System;
using System.Collections.Generic;

namespace ExpRddApp.elements
{
    public class TSeg
    {
        public string Id { get; set; }
        public Vertex Vertex1 { get; set; }
        public Vertex Vertex2 { get; set; }
        public List<TrackLine> TrackLines { get; set; }
        public DirectionType LineDirection { get; set; }
        public string LineID { get; set; }
        public Track Track { get; set; }
        public string InsidePSA { get; set; }
        public decimal Length()
        {
            return Math.Abs(Vertex1.Km - Vertex2.Km);
        }
    }
}
