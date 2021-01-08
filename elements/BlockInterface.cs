using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Refact.elements
{
    public class BlockInterface: SLElement 
    {
        public string KindOfBI { get; set; }
        public YesNoType PermissionHandling { get; set; }
        public BlockInterface(Block block, string stattionId) : base(block, stattionId)
        {
            Error = !base.Init();
            Error = !Init();
        }

        public override bool Init()
        {
            bool error = false;

            if (!Enum.TryParse(Attributes["PER_HAND"].value, out YesNoType perHand))
            {
                error = true;
                ErrLogger.Error("Unable to parse attribute value", Designation, "PER_HAND");
            }
            PermissionHandling = perHand;
            KindOfBI = Attributes["KIND"].value;
            return !error;
        }
    }
}
