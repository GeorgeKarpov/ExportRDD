using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

[assembly: CommandClass(typeof(ExpPt1.Block))]

namespace ExpPt1
{
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

    /// <summary>
    /// Base class for blocks collected from dwg.
    /// </summary>
    public class Block
    {
        public BlockReference BlkRef;
        public string BlockName;
        public string XsdName;
        public string Designation;
        public string ElType;
        public string StId;
        public string StName;
        public Dictionary<string,Attribute> Attributes;
        public double X;
        public double Y;
        public int Rotation;
        public decimal Location;
        public decimal Location2;
        public decimal Location3;
        public string KindOf;
        public ConnectionBranchType PointBranch;
        public UpDownSingleType PointUpDown;
        public LeftRightType PointleftRight;
        public LeftRightType PointMachineLeftRight;
        public int PtMachines;
        public YesNoType Trailable;
        public bool IsOnCurrentArea;
        public bool IsOnNextStation;
        public string LineID;
        public string LineIDtip;
        public string LineIDleft;
        public string LineIDright;
        public bool Visible;
        public string TrackSegId;
        public double Sort;
        public bool Start_Cr;
        public bool Dest_Cr;
        public int Ac_angle;
        public int PlatformTrack;
    }

    /// <summary>
    /// Basic attribute class for acad attributes.
    /// </summary>
    public class Attribute
    {
        public string Name;
        public string XsdName;
        public string Value;
        public bool Visible;
    }

    /// <summary>
    /// Temporary track segment.
    /// </summary>
    public class TrackSegmentTmp
    {
        public string Designation;
        public Block Vertex1;
        public Block Vertex2;
        public List<Block> BlocksOnSegments;
        public List<Line> TrustedLines;
        public string Track;
        public bool mainTrack;
        public List<Line> TrackLines;
        public double minY;
        public bool betweenLevels;
        public string lineId;
    } 

    /// <summary>
    /// Track <SP>.
    /// </summary>
    public class Track
    {
        public string Name;
        public double X;
        public double Y;
    }

    /// <summary>
    /// Permanent shunting area.
    /// </summary>
    public class PSA
    {
        public string Name;
        public double MinX;
        public double MinY;
        public double MaxX;
        public double MaxY;
        public Polyline PsaPolyLine;
    }

    /// <summary>
    /// Trusted area.
    /// </summary>
    public class TrustedArea
    {
        public string Designation;
        public List<Line> Lines;
    }

    /// <summary>
    /// Railway line.
    /// </summary>
    public class RailwayLine
    {
        public string designation;
        public string start;
        public string end;
        public DirectionType direction;
        public System.Drawing.Color color;
    }

    /// <summary>
    /// Track line. Acad lines assigned to track layer.
    /// </summary>
    public class TrackLine
    {
        public Line line;
        public DirectionType direction;
        public string LineID;
        public System.Drawing.Color color;
    }

    /// <summary>
    /// Platform.
    /// </summary>
    public class Platform : Block
    {
        public int Number;
        public int Track;
        public string Station;
        public int Length;
        public int Height;
    }

    /// <summary>
    /// Station or stop.
    /// </summary>
    public class StationStop
    {
        public string Id;
        public string Name;
        public decimal StartKm;
        public decimal EndKm;
        public List<string> LineIDs;
        public KindOfSASType KindOfSAS;
    }

    /// <summary>
    /// Block properties class.
    /// </summary>
    public class BlockProperties
    {
        private string stationID;
        public BlockProperties(string StationID)
        {
            stationID = StationID;
        }
        /// <summary>
        /// Gets block designation.
        /// </summary>
        public string GetElemDesignation(Block Block, bool NamelowCase = false, bool PadZeros = true)
        {
            if (!Block.Attributes.ContainsKey("NAME"))
            {
                //ErrLogger.Log("Attribute 'NAME' does not exist: " + Block.BlkRef.Name);
                return Block.BlkRef.Name;
            }
            string Name = Block.Attributes["NAME"].Value.Split('_')[0];
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
                if (PadZeros && !names[2].All(x => char.IsLetter(x)))
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
                if (PadZeros && !names[1].All(x => char.IsLetter(x)))
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
                if (PadZeros && !Name.All(x => char.IsLetter(x)))
                {
                    Name = NamelowCase ? Name.ToLower().PadLeft(NameLength, '0') : Name.ToUpper().PadLeft(NameLength, '0');
                }
                else
                {
                    Name = NamelowCase ? Name.ToLower() : Name.ToUpper();
                }
                return Block.ElType.ToLower() + "-" + stationID.ToLower() + "-" +
                      Name.Trim();
            }
        }

        public string GetElemDesignation(string Name)
        {
            // string Name = BlkRef.Attributes["NAME"].Value.Split('_')[0];
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
                names[2] = names[2].ToUpper().PadLeft(NameLength, '0');
                return string.Join("-", names).Trim();
                //return Name.PadLeft(3, '0');       
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
                names[1] = names[2].ToUpper().PadLeft(NameLength, '0'); ;
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
                return Name.ToUpper().PadLeft(NameLength, '0').Trim();
            }
        }
    }

    /// <summary>
    /// Emergency stops group.
    /// </summary>
    public class EmSG
    {
        public string Designation;
        public double MinX;
        public double MinY;
        public double MaxX;
        public double MaxY;
        public Polyline PsaPolyLine;
        public int Order;
    }

    public class SspAct
    {
        public decimal speed;
        public decimal kmStart;
        public decimal kmEnd;
        public decimal kmGap;
    }

    public class LxActivation
    {
        public string id;
        public decimal km;
    }

    public class ActivationsSet
    {
        public List<LxActivation> lxActivations;
        public bool point;
        public string segDeviationId;
    }
}
