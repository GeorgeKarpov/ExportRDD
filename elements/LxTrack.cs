﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ExpRddApp.elements
{
    public class LxTrack
    {
        public string Id { get; set; }
        public Track Track { get; set; }
        public TrackLine TrackLine { get; set; }
        public List<DetectionPoint> DetectionPoints { get; set; }
        public string LxAcSection { get; set; }
        public decimal Location { get; set; }
        public decimal BeginLca { get; set; }
        public decimal EndLca { get; set; }
        public int LengthLca { get; set; }
        public TSeg TSeg { get; set; }

        public string GetId(int i, char a, int trackCount, SLElement lxPws)
        {
            string replace = "ovk-";
            if (lxPws.ElType == XType.StaffPassengerCrossing)
            {
                replace = "va-";
            }
            if (trackCount == 1)
            {
                return lxPws.GetElemDesignation(PadZeros: false).Replace(replace, "sp-");
            }
            else if (char.IsDigit(lxPws.Designation.Last()))
            {
                return lxPws.GetElemDesignation(PadZeros: false).Replace(replace, "sp-") + a;
            }
            else
            {
                return lxPws.GetElemDesignation(PadZeros: false).Replace(replace, "sp-") + i;
            }
        }

        public void SetLocations(LevelCrossing lx, int num, int tracksCount, out bool error)
        {
            error = false;
            string suffix = "";
            if (tracksCount > 1)
            {
                suffix = num.ToString();
            }
            Regex regexLoc = new Regex(@"^\s*KMP(_*TSEG" + suffix + @"){0,1}$\s*");
            Regex regexBegin = new Regex(@"^\s*(BEGIN|START)_*(LCA|KMP){0,1}(_*TSEG" + suffix + @"){0,1}$\s*");
            Regex regexEnd = new Regex(@"^\s*END_*(LCA|KMP){0,1}(_*TSEG" + suffix + @"){0,1}$\s*");
            Regex regexLength = new Regex(@"^\s*LENG(HT|TH)_*(LCA|" + suffix + @"){0,1}(_*TSEG" + suffix + @"){0,1}$\s*");
            Attribute lxTrLocation = null;
            lxTrLocation = lx
                               .Attributes.Where(x => regexLoc.IsMatch(x.Key))
                               .FirstOrDefault()
                               .Value;
            if (lxTrLocation == null)
            {
                ErrLogger.Error("Unable to get Lx track Location(KMP) attribute", lx.Designation, Id);
                Location = 0;
                error = true;
            }
            else
            {
                if (!decimal.TryParse(lxTrLocation.value, out decimal location))
                {
                    ErrLogger.Error("Unable to parse Lx track Location(KMP) from attribute", lx.Designation, lxTrLocation.name);
                    error = true;
                }
                Location = location;
            }
            //if (tracksCount == 1)
            //{             
            lxTrLocation = lx
                           .Attributes.Where(x => regexBegin.IsMatch(x.Key))
                           .FirstOrDefault()
                           .Value;
            if (lxTrLocation == null)
            {
                ErrLogger.Error("Unable to get Lx track Begin Kmp attribute", lx.Designation, Id);
                BeginLca = 0;
                error = true;
            }
            else
            {
                if (!decimal.TryParse(lxTrLocation.value, out decimal begin))
                {
                    ErrLogger.Error("Unable to parse Lx track Begin Kmp from attribute", lx.Designation, lxTrLocation.name);
                    error = true;
                }
                BeginLca = begin;
            }
            lxTrLocation = lx
                           .Attributes.Where(x => regexEnd.IsMatch(x.Key))
                           .FirstOrDefault()
                           .Value;
            if (lxTrLocation == null)
            {
                ErrLogger.Error("Unable to get Lx track End Kmp attribute", lx.Designation, Id);
                EndLca = 0;
                error = true;
            }
            else
            {
                if (!decimal.TryParse(lxTrLocation.value, out decimal end))
                {
                    ErrLogger.Error("Unable to parse Lx track End Kmp from attribute", lx.Designation, lxTrLocation.name);
                    error = true;
                }
                EndLca = end;
            }
            lxTrLocation = lx
                           .Attributes.Where(x => regexLength.IsMatch(x.Key))
                           .FirstOrDefault()
                           .Value;
            if (lxTrLocation == null)
            {
                ErrLogger.Error("Unable to get Lx track Length attribute", lx.Designation, Id);
                LengthLca = 0;
                error = true;
            }
            else
            {
                if (!int.TryParse(lxTrLocation.value, out int length))
                {
                    ErrLogger.Error("Unable to parse Lx track Length from attribute", lx.Designation, lxTrLocation.name);
                    error = true;
                }
                LengthLca = length;
            }
            //}
            //else if (tracksCount > 1)
            //{
            //    lxTrLocation = lx
            //                   .Attributes.Where(x => regexBegin.IsMatch(x.Key) && 
            //                                          x.Key.Last().ToString() == suffix)
            //                   .FirstOrDefault()
            //                   .Value;
            //    if (lxTrLocation == null)
            //    {
            //        ErrLogger.Error("Unable to get Lx track Begin Kmp attribute", lx.Designation, Id);
            //        BeginLca = 0;
            //        error = true;
            //    }
            //    else
            //    {
            //        if (!decimal.TryParse(lxTrLocation.value, out decimal begin))
            //        {
            //            ErrLogger.Error("Unable to parse Lx track Begin Kmp from attribute", lx.Designation, lxTrLocation.name);
            //            error = true;
            //        }
            //        BeginLca = begin;
            //    }
            //    lxTrLocation = lx
            //                   .Attributes.Where(x => regexEnd.IsMatch(x.Key) &&
            //                                          x.Key.Last().ToString() == suffix)
            //                   .FirstOrDefault()
            //                   .Value;
            //    if (lxTrLocation == null)
            //    {
            //        ErrLogger.Error("Unable to get Lx track End Kmp attribute", lx.Designation, Id);
            //        EndLca = 0;
            //        error = true;
            //    }
            //    else
            //    {
            //        if (!decimal.TryParse(lxTrLocation.value, out decimal end))
            //        {
            //            ErrLogger.Error("Unable to parse Lx track End Kmp from attribute", lx.Designation, lxTrLocation.name);
            //            error = true;
            //        }
            //        EndLca = end;
            //    }
            //    lxTrLocation = lx
            //                   .Attributes.Where(x => regexLength.IsMatch(x.Key) &&
            //                                          x.Key.Last().ToString() == suffix)
            //                   .FirstOrDefault()
            //                   .Value;
            //    if (lxTrLocation == null)
            //    {
            //        ErrLogger.Error("Unable to get Lx track Length attribute", lx.Designation, Id);
            //        LengthLca = 0;
            //        error = true;
            //    }
            //    else
            //    {
            //        if (!int.TryParse(lxTrLocation.value, out int length))
            //        {
            //            ErrLogger.Error("Unable to parse Lx track Length from attribute", lx.Designation, lxTrLocation.name);
            //            error = true;
            //        }
            //        LengthLca = length;
            //    }
            //}
        }

        public void SetLocations(Pws pws, int num, int tracksCount, out bool error)
        {
            error = false;
            string suffix = "";
            if (tracksCount > 1)
            {
                suffix = num.ToString();
            }
            Regex regexLoc = new Regex(Constants.lxTrackLoc);
            Regex regexBegin = new Regex(Constants.lxTrackBeginReg);
            Regex regexEnd = new Regex(Constants.lxTrackEndReg);
            Regex regexLength = new Regex(Constants.lxTrackLengthReg);
            Attribute lxTrLocation = null;
            if (tracksCount == 1)
            {
                lxTrLocation = pws
                               .Attributes.Where(x => regexLoc.IsMatch(x.Key))
                               .FirstOrDefault()
                               .Value;
                if (lxTrLocation == null)
                {
                    ErrLogger.Error("Unable to get Lx track Location(KMP) attribute", pws.Designation, Id);
                    Location = 0;
                    error = true;
                }
                else
                {
                    if (!decimal.TryParse(lxTrLocation.value, out decimal location))
                    {
                        ErrLogger.Error("Unable to parse Lx track Location(KMP) from attribute", pws.Designation, lxTrLocation.name);
                        error = true;
                    }
                    Location = location;
                }
                lxTrLocation = pws
                               .Attributes.Where(x => regexBegin.IsMatch(x.Key))
                               .FirstOrDefault()
                               .Value;
                if (lxTrLocation == null)
                {
                    ErrLogger.Error("Unable to get Lx track Begin Kmp attribute", pws.Designation, Id);
                    BeginLca = 0;
                    error = true;
                }
                else
                {
                    if (!decimal.TryParse(lxTrLocation.value, out decimal begin))
                    {
                        ErrLogger.Error("Unable to parse Lx track Begin Kmp from attribute", pws.Designation, lxTrLocation.name);
                        error = true;
                    }
                    BeginLca = begin;
                }
                lxTrLocation = pws
                               .Attributes.Where(x => regexEnd.IsMatch(x.Key))
                               .FirstOrDefault()
                               .Value;
                if (lxTrLocation == null)
                {
                    ErrLogger.Error("Unable to get Lx track End Kmp attribute", pws.Designation, Id);
                    EndLca = 0;
                    error = true;
                }
                else
                {
                    if (!decimal.TryParse(lxTrLocation.value, out decimal end))
                    {
                        ErrLogger.Error("Unable to parse Lx track End Kmp from attribute", pws.Designation, lxTrLocation.name);
                        error = true;
                    }
                    EndLca = end;
                }
                lxTrLocation = pws
                               .Attributes.Where(x => regexLength.IsMatch(x.Key))
                               .FirstOrDefault()
                               .Value;
                if (lxTrLocation == null)
                {
                    ErrLogger.Error("Unable to get Lx track Length attribute", pws.Designation, Id);
                    LengthLca = 0;
                    error = true;
                }
                else
                {
                    if (!int.TryParse(lxTrLocation.value, out int length))
                    {
                        ErrLogger.Error("Unable to parse Lx track Length from attribute", pws.Designation, lxTrLocation.name);
                        error = true;
                    }
                    LengthLca = length;
                }
            }
            else if (tracksCount > 1)
            {
                lxTrLocation = pws
                               .Attributes.Where(x => regexLoc.IsMatch(x.Key) &&
                                                      x.Key.Last().ToString() == suffix)
                               .FirstOrDefault()
                               .Value;
                if (lxTrLocation == null)
                {
                    ErrLogger.Error("Unable to get Lx track Location(KMP) attribute", pws.Designation, Id);
                    Location = 0;
                    error = true;
                }
                else
                {
                    if (!decimal.TryParse(lxTrLocation.value, out decimal location))
                    {
                        ErrLogger.Error("Unable to parse Lx track Location(KMP) from attribute", pws.Designation, lxTrLocation.name);
                        error = true;
                    }
                    Location = location;
                }
                lxTrLocation = pws
                               .Attributes.Where(x => regexBegin.IsMatch(x.Key) &&
                                                      x.Key.Last().ToString() == suffix)
                               .FirstOrDefault()
                               .Value;
                if (lxTrLocation == null)
                {
                    ErrLogger.Error("Unable to get Lx track Begin Kmp attribute", pws.Designation, Id);
                    BeginLca = 0;
                    error = true;
                }
                else
                {
                    if (!decimal.TryParse(lxTrLocation.value, out decimal begin))
                    {
                        ErrLogger.Error("Unable to parse Lx track Begin Kmp from attribute", pws.Designation, lxTrLocation.name);
                        error = true;
                    }
                    BeginLca = begin;
                }
                lxTrLocation = pws
                               .Attributes.Where(x => regexEnd.IsMatch(x.Key) &&
                                                      x.Key.Last().ToString() == suffix)
                               .FirstOrDefault()
                               .Value;
                if (lxTrLocation == null)
                {
                    ErrLogger.Error("Unable to get Lx track End Kmp attribute", pws.Designation, Id);
                    EndLca = 0;
                    error = true;
                }
                else
                {
                    if (!decimal.TryParse(lxTrLocation.value, out decimal end))
                    {
                        ErrLogger.Error("Unable to parse Lx track End Kmp from attribute", pws.Designation, lxTrLocation.name);
                        error = true;
                    }
                    EndLca = end;
                }
                lxTrLocation = pws
                               .Attributes.Where(x => regexLength.IsMatch(x.Key) &&
                                                      x.Key.Last().ToString() == suffix)
                               .FirstOrDefault()
                               .Value;
                if (lxTrLocation == null)
                {
                    ErrLogger.Error("Unable to get Lx track Length attribute", pws.Designation, Id);
                    LengthLca = 0;
                    error = true;
                }
                else
                {
                    if (!int.TryParse(lxTrLocation.value, out int length))
                    {
                        ErrLogger.Error("Unable to parse Lx track Length from attribute", pws.Designation, lxTrLocation.name);
                        error = true;
                    }
                    LengthLca = length;
                }
            }
        }
    }


}
