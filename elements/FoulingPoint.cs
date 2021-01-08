﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Refact.elements
{
    public class FoulingPoint: SLElement
    {
        public FoulingPoint(Block block, string stattionId) : base(block, stattionId)
        {
            Error = !base.Init();
            Error = !Init();
        }

        public override bool Init()
        {
            bool error = false;

            if (!decimal.TryParse(Attributes["KMP"].value, out decimal km))
            {
                ErrLogger.Error("Unable to parse KMP value from attribute", ElType.ToString(), this.Designation);
                error = true;
            }
            Location = km;
            return !error;
        }
    }
}
