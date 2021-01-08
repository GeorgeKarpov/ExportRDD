using Autodesk.AutoCAD.DatabaseServices;
using System;

namespace Refact.elements
{
    public class SigLayout : SLElement
    {
        public string DocId { get; set; }
        public string Title { get; set; }
        public string Creator { get; set; }
        public string Version { get; set; }
        public DateTime Date { get; set; }

        public SigLayout(Block blockReference, string stattionId) : base(blockReference, stattionId)
        {
            Error = !base.Init();
        }
    }
}
