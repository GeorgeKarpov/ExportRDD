using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Refact.elements
{
    /// <summary>
    /// Class to store Acad attributes of blocks
    /// </summary>
    public class Attribute
    {
        public string name;
        public string xsdName;
        public string value;
        public bool visible;
    }

    /// <summary>
    /// Abstract class for Railway elements on SL.
    /// Has common basic properties and methods.
    /// </summary>
    public abstract class SLElement
    {
        public Block Block { get; set; }
        public string BlockName { get; set; }
        public XType ElType { get; set; }
        public ExtType ExtType { get; set; }
        public RddType RddType { get; set; }
        public string Designation { get; set; }
        public string StID { get; set; }
        public string StName { get; set; }
        public Dictionary<string, Attribute> Attributes { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public Point3d Position { get; set; }
        public decimal Location { get; set; }
        public TSeg Tseg { get; set; }
        public bool Error { get; set; }
        public string BlkMap { get; set; }
        public bool Visible { get; set; }
        public TrackLine TrackLine { get; set; }
        public List<TrackLine> Branches { get; set; }
        public string LineID { get; set; }
        public string InsidePSA { get; set; }
        public bool Exclude { get; set; }
        public bool NextStation { get; set; }
        public string Remark { get; set; }

        public SLElement(Block block, string stattionId)
        {
            Block = block;
            this.BlkMap = block.BlkMap;
            StID = stattionId;
            Branches = new List<TrackLine>();
        }

        /// <summary>
        /// Initializes new Railway element.
        /// Parses attributes and data from Acad Block.
        /// </summary>
        /// <returns>true if success</returns>
        public virtual bool Init()
        {
            Attributes = GetAttributes();

            if (Enum.TryParse(this.BlkMap.Split('\t')[1], out XType xType))
            {
                ElType = xType;
            }
            else
            {
                ErrLogger.Error("Unable to parse element type", "Block", Block.BlockReference.Name);
                return false;
            }

            if (!string.IsNullOrEmpty(this.BlkMap.Split('\t')[2]))
            {
                if (Enum.TryParse(this.BlkMap.Split('\t')[2], out RddType rddType))
                {
                    RddType = rddType;
                }
                else
                {
                    ErrLogger.Error("Unable to parse element Rdd type", "Block", Block.BlockReference.Name);
                    return false;
                }
            }
            

            if (this.BlkMap.Split('\t').Length > 3 && !string.IsNullOrEmpty(this.BlkMap.Split('\t')[3]))
            {
                if (Enum.TryParse(this.BlkMap.Split('\t')[3], out ExtType extType))
                {
                    ExtType = extType;
                }
                else
                {
                    ErrLogger.Error("Unable to parse element Ext type", "Block", Block.BlockReference.Name);
                    return false;
                }
            }

            BlockName = Block.BlockReference.Name;
            X = Math.Round(Block.BlockReference.Position.X, 0, MidpointRounding.AwayFromZero);
            Y = Math.Round(Block.BlockReference.Position.Y, 0, MidpointRounding.AwayFromZero);
            Position = Block.BlockReference.Position;
            Visible = !(Attributes.Any(x => x.Value.name == "NAME" &&
                                            x.Value.visible == false)) ||
                      this.Block.Visible;
            
            
            Designation = GetElemDesignation();

            return true;
        }

        /// <summary>
        /// Gets attributes from Acad block.
        /// </summary>
        /// <returns>Dictionary of attributes</returns>
        protected Dictionary<string, Attribute> GetAttributes()
        {
            Database db = this.Block.BlockReference.DynamicBlockTableRecord.Database;
            Dictionary<string, Attribute> map = new Dictionary<string, Attribute>();
            AttributeCollection atts = this.Block.BlockReference.AttributeCollection;
            foreach (ObjectId arId in atts)
            {
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    AttributeReference attRef =
                    (AttributeReference)trans.GetObject(arId, OpenMode.ForRead);
                    Attribute attribute =
                        new Attribute
                        {
                            name = attRef.Tag,
                            value = attRef.TextString
                        };
                    if (attRef.Layer.Contains("Invisible") || attRef.Invisible == true)
                    {
                        attribute.visible = false;
                    }
                    else
                    {
                        attribute.visible = true;
                    }
                    try
                    {
                        map.Add(attRef.Tag, attribute);
                    }
                    catch (ArgumentException e)
                    {
                        ErrLogger.Error(e.Message, this.Block.BlockReference.Name, attRef.Tag);
                    }
                    trans.Commit();
                }
            }
            return map;
        }

        /// <summary>
        /// Gets full element designation in format 'type'-'station'-'number'.
        /// </summary>
        /// <param name="NamelowCase">convert to lower case</param>
        /// <param name="PadZeros">add leading zeros to numbered name ('001')</param>
        /// <returns>full element designation</returns>
        public string GetElemDesignation(bool NamelowCase = false, bool PadZeros = true)
        {
            if (!this.Attributes.ContainsKey("NAME"))
            {
                //ErrLogger.Warning("Attribute 'NAME' does not exist: " + Block.BlkRef.Name);
                return Block.BlockReference.Name;
            }
            string Name = this.Attributes["NAME"].value.Split('_')[0];
            Regex re = new Regex(@"(\d+)([a-zA-Z]+)");
            Match result;
            int NameLength;
            if (Name.Split('-').Length > 2)
            {
                string[] names = Name.Split('-');
                result = re.Match(names[2]);
                if (result.Value.Length > 0)
                {
                    NameLength = 3 + result.Groups[2].Value.Length;
                }
                else
                {
                    NameLength = names[2].Length < 3 ? 3 : names[2].Length;
                }
                names[0] = names[0].ToLower();
                names[1] = names[1].ToLower();
                if (PadZeros)
                {
                    names[2] = NamelowCase ?
                        names[2].ToLower().PadLeft(NameLength, '0') :
                        names[2].ToUpper().PadLeft(NameLength, '0');
                }
                else
                {
                    names[2] = NamelowCase ? names[2].ToLower() : names[2].ToUpper();
                }
                return string.Join("-", names).Trim();
            }
            else if (Name.Split('-').Length > 1)
            {
                string[] names = Name.Split('-');
                result = re.Match(names[1]);
                if (result.Value.Length > 0)
                {
                    NameLength = 3 + result.Groups[2].Value.Length;
                }
                else
                {
                    NameLength = names[1].Length < 3 ? 3 : names[1].Length;
                }
                names[0] = names[0].ToLower();
                //names[1] = names[1].ToLower();
                if (PadZeros)
                {
                    names[1] = NamelowCase ?
                        names[1].ToLower().PadLeft(NameLength, '0') :
                        names[1].ToUpper().PadLeft(NameLength, '0');
                }
                else
                {
                    names[1] = NamelowCase ?
                        names[1].ToLower() :
                        names[1].ToUpper();
                }

                return string.Join("-", names).Trim();
            }
            else
            {
                result = re.Match(Name);
                if (result.Value.Length > 0)
                {
                    NameLength = 3 + result.Groups[2].Value.Length;
                }
                else
                {
                    NameLength = Name.Length < 3 ? 3 : Name.Length;
                }
                if (PadZeros)
                {
                    Name = NamelowCase ? Name.ToLower().PadLeft(NameLength, '0') : Name.ToUpper().PadLeft(NameLength, '0');
                }
                else
                {
                    Name = NamelowCase ? Name.ToLower() : Name.ToUpper();
                }
                return this.RddType.ToString().ToLower() + "-" + this.StID.ToLower() + "-" +
                      Name.Trim();
            }
        }

        public string GetTsegId()
        {
            if (this.Tseg == null)
            {
                return "not found";
            }
            else
            {
                return this.Tseg.Id;
            }
        }
    }

}
