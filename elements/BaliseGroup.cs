using System;
using System.Collections.Generic;

namespace Refact.elements
{
    /// <summary>
    /// Represents Balise Group railway element.
    /// </summary>
    public class BaliseGroup: SLElement
    {
        /// <summary>Balise Group Types</summary>
        /// <remarks>
        /// Default is 'Positioning Balise Group' with 'nominal' direction value
        /// </remarks>
        public List<BgType> BgTypes { get; set; }
        /// <summary>
        /// Balise group orientation.
        /// </summary>
        /// <remarks>
        /// Default value is 'single'.
        /// </remarks>
        public UpDownSingleType Orientation { get; set; }
        public BaliseGroup(Block block, string stattionId) : base(block, stattionId)
        {
            Error = !base.Init();
            Error = !Init();
        }

        /// <summary>
        /// Initializes new Balise Group element.
        /// Parses attributes and data from Acad Block.
        /// </summary>
        /// <returns>true if success</returns>
        public override bool Init()
        {
            bool error = false;
            if (!decimal.TryParse(Attributes["KMP"].value, out decimal km))
            {
                ErrLogger.Error("Unable to parse KMP value from attribute", this.ElType.ToString(), this.Designation);
                error = true;
            }
            this.Location = km;

            if (!Enum.TryParse(Attributes["DIRECTION"].value.ToString().ToLower(), out NominalReverseBothType direction))
            {
                ErrLogger.Error("Unable to parse DIRECTION attribute value", Designation,
                    Attributes["DIRECTION"].value.ToString());
                error = true;
            }
            BgTypes = new List<BgType>
            { new BgType
            {
                KindOfBG = KindOfBG.Positioningbalisegroup,
                Direction = direction
            } };
            if (!Enum.TryParse(Attributes["ORIENT"].value.ToString().ToLower(), out UpDownSingleType orient))
            {
                if (string.IsNullOrEmpty(Attributes["ORIENT"].value.ToString()))
                {
                    orient = UpDownSingleType.single;
                    ErrLogger.Information("ORIENT value is empty. Default value has been assigned.", Designation);
                }
                else
                {
                    ErrLogger.Error("Unable to parse ORIENT attribute value", Designation,
                    Attributes["ORIENT"].value.ToString());
                    error = true;
                }             
            }
            Orientation = orient;
            return !error;
        }
    }
}
