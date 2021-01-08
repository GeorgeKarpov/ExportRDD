﻿using Autodesk.AutoCAD.DatabaseServices;

namespace Refact
{
    /// <summary>
    /// Represents AutoCAD block.
    /// </summary>
    public class Block
    {
        public BlockReference BlockReference { get; set; }
        public XType Xtype { get; set; }
        public bool Visible { get; set; }

        public string BlkMap { get; set; }
    }
}
