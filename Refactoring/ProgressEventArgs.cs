using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Refact
{
    public class ProgressEventArgs: EventArgs
    {
        public int Increment { get; set; }
    }
}
