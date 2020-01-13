using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using LXactSection = LevelCrossingsLevelCrossingLevelCrossingTracksLevelCrossingTrackActivationSectionsActivationSection;

namespace ExportV2
{
    public class Attribute
    {
        public string Name;
        public string XsdName;
        public string Value;
        public bool Visible;
    }

    public class Block
    {
        public BlockReference BlkRef { get; set; }
        public string Name { get; set; }
    }

    public abstract class SLElement
    {
        public Block Block;
        public string BlockName;
        public XType ElType;
        public string DesignType;
        public string Designation;
        public string StId;
        public string StName;
        public Dictionary<string, Attribute> Attributes;
        public double X;
        public double Y;
        public int Rotation;
        public decimal Location;
        public bool InitError;
        public string blkMap;
        public bool IsOnCurrentArea;
        public bool IsOnNextStation;
        public bool Visible;

        public abstract dynamic ConvertToRdd();

        public virtual bool Init()
        {
            GetAttributes();

            if (Enum.TryParse(this.blkMap.Split('\t')[1], out XType xType))
            {
                ElType = xType;
            }
            else
            {
                ErrLogger.Log("Unable to parse element type '" + Block.Name.Split('\t')[1] + "'");
                return false;
            }

            BlockName = Block.Name;
            X = Math.Round(Block.BlkRef.Position.X, 0, MidpointRounding.AwayFromZero);
            Y = Math.Round(Block.BlkRef.Position.Y, 0, MidpointRounding.AwayFromZero);
            Rotation = (int)(Block.BlkRef.Rotation * (180 / Math.PI));
            IsOnCurrentArea = false;
            Visible = !(Attributes.Any(x => x.Value.Name == "NAME" &&
                                            x.Value.Visible == false));
            DesignType = this.blkMap.Split('\t')[2];

            return true;
        }

        

        protected bool GetAttributes()
        {
            Database db = this.Block.BlkRef.DynamicBlockTableRecord.Database;
            Dictionary<string, Attribute> map = new Dictionary<string, Attribute>();
            AttributeCollection atts = this.Block.BlkRef.AttributeCollection;
            foreach (ObjectId arId in atts)
            {
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    AttributeReference attRef =
                    (AttributeReference)trans.GetObject(arId, OpenMode.ForRead);
                    Attribute attribute =
                        new Attribute
                        {
                            Name = attRef.Tag,
                            Value = attRef.TextString
                        };
                    if (attRef.Layer.Contains("Invisible") || attRef.Invisible == true)
                    {
                        attribute.Visible = false;
                    }
                    else
                    {
                        attribute.Visible = true;
                    }
                    try
                    {
                        map.Add(attRef.Tag, attribute);
                    }
                    catch (ArgumentException e)
                    {
                        ErrLogger.Log(e.Message + " Attribute:" + attRef.Tag + " Block:" + this.Block.Name);
                    }
                    trans.Commit();
                }
            }
            this.Attributes = map;
            return true;
        }

        public bool GetElemDesignation(string stationID, bool NamelowCase = false, bool PadZeros = true)
        {
            if (!this.Attributes.ContainsKey("NAME"))
            {
                ErrLogger.Log("Attribute 'NAME' does not exist: " + this.Block.Name);
                return false;
            }
            string Name = this.Attributes["NAME"].Value.Split('_')[0];
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
                this.Designation = string.Join("-", names).Trim();
                return true;
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

                this.Designation = string.Join("-", names).Trim();
                return true;
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
                this.Designation = this.DesignType.ToLower() + "-" + stationID.ToLower() + "-" +
                      Name.Trim();
                return true;
            }
        }

    }

    public class LX : SLElement
    {
        public LX(Block blockReference, string BlkMap)
        {
            Block = blockReference;
            blkMap = BlkMap;
            InitError = !Init();
        }

        public override bool Init()
        {
            bool error = false;
            error = !base.Init();

            return !error;

        }

        public override dynamic ConvertToRdd()
        {
            throw new NotImplementedException();
        }
    }

    public class LxActivation
    {
        public TFileDescr Document { get; set; }
        public List<LXactSection> ActivationSections { get; set; }
    }

    public class LxParam
    {
        public int Index { get; set; }
        public string Value { get; set; }
        public string Reference { get; set; }
    }

    public class LXParameters
    {
        public TFileDescr Document { get; set; }
        public Dictionary<string,List<LxParam>> LxParams { get; set; }
    }

    public class BGs
    {
        public TFileDescr Document { get; set; }
        public List<BaliseGroupsBaliseGroup> BaliseGroups { get; set; }
    }

    public class PWS : SLElement
    {
        public PWS(Block blockReference, string BlkMap)
        {
            Block = blockReference;
            blkMap = BlkMap;
            InitError = !Init();
        }

        public override bool Init()
        {
            bool error = false;
            error = !base.Init();

            return !error;

        }

        public override dynamic ConvertToRdd()
        {
            throw new NotImplementedException();
        }
    }

    public class SX : SLElement
    {
        public SX(Block blockReference, string BlkMap)
        {
            Block = blockReference;
            blkMap = BlkMap;
            InitError = !Init();
        }

        public override bool Init()
        {
            bool error = false;
            error = !base.Init();

            return !error;

        }

        public override dynamic ConvertToRdd()
        {
            throw new NotImplementedException();
        }
    }



    public class FoulPoint : SLElement
    {
        public decimal Location2;
        public FoulPoint(Block blockReference, string BlkMap)
        {
            Block = blockReference;
            blkMap = BlkMap;
            InitError = !Init();
        }

        public override bool Init()
        {
            bool error = false;
            error = !base.Init();
            string[] attKeys = this.Attributes
                                               .Where(x => x.Key.Contains("KMP"))
                                               .OrderBy(x => x.Key)
                                               .Select(x => x.Key)
                                               .ToArray();
            if (!decimal.TryParse(this.Attributes[attKeys[0]].Value.Split('/').First(), out decimal loc))
            {
                error = true;
                ErrLogger.Log(this.Designation + ": Can not convert '" + attKeys[0] + "' to decimal");
            }
            this.Location = loc;
            if (this.Attributes[attKeys[0]].Value.Split('/').Length > 1)
            {
                if (!decimal.TryParse(this.Attributes[attKeys[0]].Value.Split('/').Last(), out decimal loc2))
                {
                    error = true;
                    ErrLogger.Log(this.Designation + ": Can not convert '" + attKeys[0] + "' to decimal");
                }
                this.Location2 = loc2;
            }
            return !error;
        }

        public override dynamic ConvertToRdd()
        {
            throw new NotImplementedException();
        }
    }

    public class RailwayLine
    {
        public string designation;
        public string start;
        public string end;
        public DirectionType direction;
        public System.Drawing.Color color;
    }

    public class TrackLine
    {
        public Line line;
        public DirectionType direction;
        public string LineID;
        public System.Drawing.Color color;
    }

    public enum XType
    {
        Point,
        Signal,
        AxleCounterSection,
        TrackSection,
        DetectionPoint,
        BaliseGroup,
        BlockInterface,
        Connector,
        EndOfTrack,
        FoulingPoint,
        LevelCrossing,
        StaffPassengerCrossing,
        PlatformDyn,
        Platform,
        StaffCrossing,
        SignallingLayout,
        NextStation,
        CrStart,
        CrDestination
    }
}
