
//using DocumentFormat.OpenXml.Drawing;
using ExpPt1;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LXactSection = LevelCrossingsLevelCrossingLevelCrossingTracksLevelCrossingTrackActivationSectionsActivationSection;
using PwsActDl = StaffPassengerCrossingsStaffPassengerCrossingStaffPassengerCrossingTracksStaffPassengerCrossingTrackActivationSectionsActivationSectionActivationDelaysActivationDelay;
using PwsActDls = StaffPassengerCrossingsStaffPassengerCrossingStaffPassengerCrossingTracksStaffPassengerCrossingTrackActivationSectionsActivationSectionActivationDelays;
using PWSactSection = StaffPassengerCrossingsStaffPassengerCrossingStaffPassengerCrossingTracksStaffPassengerCrossingTrackActivationSectionsActivationSection;
using PwsChain = StaffPassengerCrossingsStaffPassengerCrossingStaffPassengerCrossingTracksStaffPassengerCrossingTrackActivationSectionsActivationSectionRouteChain;


namespace ReadExcel
{
    public class Excel
    {
        private ExpPt1.BlockProperties blckProp;

        public Excel(string stationId)
        {
            this.blckProp = new ExpPt1.BlockProperties(stationId);
        }

        private string GetConnectionString(string dataSource, string ExtProp)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary["Provider"] = "Microsoft.ACE.OLEDB.12.0;";
            dictionary["Extended Properties"] = ExtProp;
            dictionary["Data Source"] = dataSource;
            StringBuilder stringBuilder = new StringBuilder();
            foreach (KeyValuePair<string, string> keyValuePair in dictionary)
            {
                stringBuilder.Append(keyValuePair.Key);
                stringBuilder.Append('=');
                stringBuilder.Append(keyValuePair.Value);
                stringBuilder.Append(';');
            }
            return stringBuilder.ToString();
        }

        public List<Signal> DangerPoints(string dataSource,
            ref TFileDescr document)
        {
            DataSet ds = new DataSet();
            string connectionString = GetConnectionString(dataSource, "'Excel 12.0;HDR=NO;IMEX=1'");
            document.title = "Signal closure";
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                OleDbCommand cmd = new OleDbCommand
                {
                    Connection = conn
                };
                cmd.CommandText = "SELECT * FROM [FrontPage$]";
                //DataTable dt = new DataTable();
                //dt.TableName = sheetName;
                DataTable dt = new DataTable();
                OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                da.Fill(dt);
                ds.Tables.Add(dt);

                for (int a = 0; a < dt.Rows.Count; a++)
                {
                    for (int i = 0; i < dt.Columns.Count - 1; i++)
                    {
                        if (dt.Rows[a][i] != DBNull.Value && ((string)dt.Rows[a][i]).Contains("Document Number"))
                        {
                            document.docID = dt.Rows[a][i + 3].ToString();
                        }
                        if (dt.Rows[a][i] != DBNull.Value && ((string)dt.Rows[a][i]).Contains("Version"))
                        {
                            document.version = dt.Rows[a][i + 3].ToString();
                        }
                        if (dt.Rows[a][i] != DBNull.Value && ((string)dt.Rows[a][i]).Contains("Prepared by"))
                        {
                            document.creator = dt.Rows[a + 1][i].ToString();
                            document.date = ExpPt1.Calc.StringToDate(dt.Rows[a + 2][i].ToString(), out DateTime date, out bool flag);
                            //document.date = Convert.ToDateTime(dt.Rows[a + 2][i].ToString());
                        }
                    }
                }
            }

            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                OleDbCommand cmd = new OleDbCommand
                {
                    Connection = conn
                };

                // Get all Sheets in Excel File
                DataTable dtSheet = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                string MbCoulumn = "";
                string AcColumn = "";
                string DistanceColumn = "";
                string OCesColumn = "";
                foreach (DataRow dr in dtSheet.Rows)
                {
                    if (!dr["TABLE_NAME"].ToString().Trim(new char[] { (char)39 }).EndsWith("$"))
                        continue;
                    cmd.CommandText = "SELECT * FROM [" + dr["TABLE_NAME"].ToString() + "]";
                    DataTable dt = new DataTable();
                    OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                    da.Fill(dt);
                    ds.Tables.Add(dt);

                    foreach (DataRow drow in dt.Rows)
                    {
                        for (int i = 0; i < dt.Columns.Count - 1; i++)
                        {
                            if (drow[i] != DBNull.Value && (string)drow[i] == "MB -> AxC")
                            {
                                DistanceColumn = dt.Columns[i].ColumnName;
                            }
                            if (drow[i] != DBNull.Value && (string)drow[i] == "Markerboard")
                            {
                                MbCoulumn = dt.Columns[i].ColumnName;
                            }
                            if (drow[i] != DBNull.Value && (string)drow[i] == "AxC beyond MB") //AxC beyond MB
                            {
                                AcColumn = dt.Columns[i].ColumnName;
                            }
                            if (drow[i] != DBNull.Value && (string)drow[i] == "Oces / m")
                            {
                                OCesColumn = dt.Columns[i].ColumnName;
                            }
                        }
                    }
                    if (MbCoulumn != string.Empty && DistanceColumn != string.Empty && AcColumn != string.Empty)
                    {
                        List<Signal> query = (from p in dt.AsEnumerable()
                                              where p.Field<object>(MbCoulumn) != null &&
                                                         p.Field<object>(DistanceColumn) != null
                                                      && p.Field<string>(DistanceColumn) != "MB -> AxC"
                                              select new Signal
                                              {
                                                  Mb = p.Field<string>(MbCoulumn),
                                                  Ac = p.Field<string>(AcColumn),
                                                  Distance = p.Field<string>(DistanceColumn),
                                                  OCes = Convert.ToDecimal(p.Field<string>(OCesColumn))
                                              }).ToList();
                        //document.title += "SL " + dt.Rows[0][5].ToString() + " " + dt.Rows[0][6].ToString();
                        //document.title += ")";
                        return query;
                    }
                }
                cmd = null;
                conn.Close();
            }
            return null;
        }

        public List<FlankProtection> FlankProtection(string dataSource,
            ref TFileDescr document)
        {
            DataSet ds = new DataSet();
            string connectionString = GetConnectionString(dataSource, "'Excel 12.0;HDR=NO;IMEX=1'");
            document.title = "Flank Protection";
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                OleDbCommand cmd = new OleDbCommand
                {
                    Connection = conn
                };
                cmd.CommandText = "SELECT * FROM [FrontPage$]";
                //DataTable dt = new DataTable();
                //dt.TableName = sheetName;
                DataTable dt = new DataTable();
                OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                try
                {
                    da.Fill(dt);
                }

                catch (OleDbException ex)
                {
                    if (ex.Message.Contains("not a valid name"))
                    {
                        cmd.CommandText = "SELECT * FROM [Cover$]";
                        da = new OleDbDataAdapter(cmd);
                        try
                        {
                            da.Fill(dt);
                        }
                        catch (OleDbException ex1)
                        {
                            ErrLogger.Error(ex1.Message, "Flank Protection", "");
                        }
                    }
                    else
                    {
                        throw new Exception(ex.Message);
                    }
                }

                ds.Tables.Add(dt);

                for (int a = 0; a < dt.Rows.Count; a++)
                {
                    for (int i = 0; i < dt.Columns.Count - 1; i++)
                    {
                        if (dt.Rows[a][i] != DBNull.Value && ((string)dt.Rows[a][i]).Contains("Document Number"))
                        {
                            document.docID = dt.Rows[a][i + 3].ToString();
                        }
                        if (dt.Rows[a][i] != DBNull.Value && ((string)dt.Rows[a][i]).Contains("Internt Thales Tegningsnr"))
                        {
                            document.docID = dt.Rows[a][i].ToString().Split(':')[1].Trim();
                        }
                        if (dt.Rows[a][i] != DBNull.Value && ((string)dt.Rows[a][i]).Contains("Version"))
                        {
                            document.version = dt.Rows[a][i + 3].ToString();
                        }
                        if (dt.Rows[a][i] != DBNull.Value && ((string)dt.Rows[a][i]).Contains("Ed"))
                        {
                            document.version = dt.Rows[a][i].ToString().Split(new string[] { "Ed" }, StringSplitOptions.RemoveEmptyEntries)[0];
                        }
                        //Udarbejdet

                        if (dt.Rows[a][i] != DBNull.Value && ((string)dt.Rows[a][i]).Contains("Prepared by"))
                        {
                            document.creator = dt.Rows[a + 1][i].ToString();
                            document.date =
                            ExpPt1.Calc.StringToDate(dt.Rows[a + 2][i].ToString(), out DateTime date, out bool flag, false);
                            //document.date = Convert.ToDateTime(dt.Rows[a + 2][i].ToString());
                        }
                        else if (dt.Rows[a][i] != DBNull.Value && ((string)dt.Rows[a][i]).Contains("Udarbejdet"))
                        {
                            int offset = 0;
                            do
                            {
                                offset++;
                            } while (dt.Rows[a][i + offset].ToString() == "");
                            document.creator = dt.Rows[a][i + offset].ToString()
                                .Split(new char[0], StringSplitOptions.RemoveEmptyEntries)[1].Trim();
                            document.date =
                            ExpPt1.Calc.StringToDate(dt.Rows[a][i + offset].ToString()
                            .Split(new char[0], StringSplitOptions.RemoveEmptyEntries)[0].Trim(), out DateTime date, out bool flag);
                            //document.date = Convert.ToDateTime(dt.Rows[a][i + offset].ToString().Split(new char[0], StringSplitOptions.RemoveEmptyEntries)[0].Trim());
                        }
                    }
                }
            }

            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                OleDbCommand cmd = new OleDbCommand
                {
                    Connection = conn
                };

                // Get all Sheets in Excel File
                DataTable dtSheet = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                string PointColumn = "";
                string LeftYesColumn = "";
                string RightYesColumn = "";
                string LeftFromColumn = "";
                string RightFromColumn = "";
                string LeftTdtColumn = "";
                string RightTdtColumn = "";
                foreach (DataRow dr in dtSheet.Rows)
                {
                    if (!dr["TABLE_NAME"].ToString().Trim(new char[] { (char)39 }).EndsWith("$"))
                        continue;
                    cmd.CommandText = "SELECT * FROM [" + dr["TABLE_NAME"].ToString() + "]";
                    DataTable dt = new DataTable();
                    //dt.TableName = sheetName;

                    OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                    da.Fill(dt);
                    ds.Tables.Add(dt);

                    foreach (DataRow drow in dt.Rows)
                    {
                        for (int i = 0; i < dt.Columns.Count - 1; i++)
                        {
                            if (drow[i] != DBNull.Value && (drow[i].ToString().Contains("Flank Protection") ||
                                                            drow[i].ToString().Contains("Points")))
                            {
                                PointColumn = dt.Columns[i].ColumnName;
                            }
                            if (drow[i] != DBNull.Value && (string)drow[i] == "Left")
                            {
                                LeftYesColumn = dt.Columns[i].ColumnName;
                                LeftFromColumn = dt.Columns[i + 1].ColumnName;
                                LeftTdtColumn = dt.Columns[i + 3].ColumnName;
                            }
                            if (drow[i] != DBNull.Value && (string)drow[i] == "Right")
                            {
                                RightYesColumn = dt.Columns[i].ColumnName;
                                RightFromColumn = dt.Columns[i + 1].ColumnName;
                                RightTdtColumn = dt.Columns[i + 3].ColumnName;
                            }
                        }
                    }
                    if (PointColumn != string.Empty && LeftYesColumn != string.Empty && RightYesColumn != string.Empty)
                    {
                        List<FlankProtection> query = (from p in dt.AsEnumerable()
                                                       where p.Field<object>(PointColumn) != null &&
                                                             !p.Field<string>(PointColumn).Contains("Flank Protection") &&
                                                              p.Field<string>(PointColumn).Length <= 5
                                                       select new FlankProtection
                                                       {
                                                           Pt = p.Field<string>(PointColumn),
                                                           Left = p.Field<string>(RightYesColumn) == "x" ? YesNoType.no : YesNoType.yes,
                                                           LeftFrom = p.Field<string>(LeftFromColumn),
                                                           LeftTdt = p.Field<string>(LeftTdtColumn),
                                                           Right = p.Field<string>(LeftYesColumn) == "x" ? YesNoType.no : YesNoType.yes,
                                                           RightFrom = p.Field<string>(RightFromColumn),
                                                           RightTdt = p.Field<string>(RightTdtColumn)
                                                       }).ToList();
                        return query;
                    }
                }
                cmd = null;
                conn.Close();
            }
            return null;
        }

        public List<TrckLackOfClearence> LackOfClearance(string dataSource)
        {
            DataSet ds = new DataSet();
            string connectionString = GetConnectionString(dataSource,
                "'Excel 12.0;IMEX=1;HDR=NO'");

            List<TrckLackOfClearence> lackOfClear = new List<TrckLackOfClearence>();

            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                OleDbCommand cmd = new OleDbCommand
                {
                    Connection = conn
                };

                DataTable dtSheet = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                foreach (DataRow dr in dtSheet.Rows)
                {
                    if (!dr["TABLE_NAME"].ToString().Trim(new char[] { (char)39 }).EndsWith("$"))
                        continue;
                    if (dr["TABLE_NAME"].ToString().Contains("Flank Protection"))
                    {
                        cmd.CommandText = "SELECT * FROM [" + dr["TABLE_NAME"].ToString() + "]";
                        break;
                    }
                    if (dr["TABLE_NAME"].ToString().Contains("Flanksikring"))
                    {
                        cmd.CommandText = "SELECT * FROM [" + dr["TABLE_NAME"].ToString() + "]";
                        break;
                    }
                    //Flanksikring
                }

                DataTable dt = new DataTable();
                OleDbDataAdapter da = new OleDbDataAdapter(cmd);

                da.Fill(dt);

                ds.Tables.Add(dt);
                for (int dr = 0; dr < dt.Rows.Count - 1; dr++)
                {
                    for (int cl = 0; cl < dt.Columns.Count - 1; cl++)
                    {
                        if (dt.Rows[dr][cl] != DBNull.Value && (string)dt.Rows[dr][cl] == "Section requesting")
                        {
                            int count = 1;
                            while ((dr + count) < dt.Rows.Count && dt.Rows[dr + count][cl] != DBNull.Value)
                            {
                                lackOfClear.Add(new TrckLackOfClearence
                                {
                                    TrackSection = (string)dt.Rows[dr + count][cl],
                                    Value = (string)dt.Rows[dr + count][cl + 1]
                                });
                                count++;
                            }
                            break;
                        }
                    }
                }
                cmd = null;
                conn.Close();
            }
            return lackOfClear;
        }

        public List<EmergStopGroup> EmergStops(string dataSource,
             ref TFileDescr document)
        {
            DataSet ds = new DataSet();
            string connectionString = GetConnectionString(dataSource, "'Excel 12.0;HDR=NO;IMEX=1'");

            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                OleDbCommand cmd = new OleDbCommand
                {
                    Connection = conn
                };
                cmd.CommandText = "SELECT * FROM [FrontPage$]";
                //DataTable dt = new DataTable();
                //dt.TableName = sheetName;
                DataTable dt = new DataTable();
                OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                da.Fill(dt);
                ds.Tables.Add(dt);
                document.title = "Emergency Stop Adjacent Track";
                for (int a = 0; a < dt.Rows.Count; a++)
                {
                    for (int i = 0; i < dt.Columns.Count - 1; i++)
                    {
                        if (dt.Rows[a][i] != DBNull.Value && ((string)dt.Rows[a][i]).Contains("Document Number"))
                        {
                            document.docID = dt.Rows[a][i + 3].ToString();
                        }
                        if (dt.Rows[a][i] != DBNull.Value && ((string)dt.Rows[a][i]).Contains("Version"))
                        {
                            document.version = dt.Rows[a][i + 3].ToString();
                        }
                        if (dt.Rows[a][i] != DBNull.Value && ((string)dt.Rows[a][i]).Contains("Prepared by"))
                        {
                            document.creator = dt.Rows[a + 1][i].ToString();
                            document.date =
                            ExpPt1.Calc.StringToDate(dt.Rows[a + 2][i].ToString(), out DateTime date, out bool flag);
                            //document.date = Convert.ToDateTime(dt.Rows[a + 2][i].ToString());
                        }
                    }
                }
            }

            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                OleDbCommand cmd = new OleDbCommand
                {
                    Connection = conn
                };

                // Get all Sheets in Excel File
                DataTable dtSheet = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                string DesignationCloumn = "";
                string EmergSgColumn = "";

                foreach (DataRow dr in dtSheet.Rows)
                {
                    if (!dr["TABLE_NAME"].ToString().Trim(new char[] { (char)39 }).EndsWith("$"))
                        continue;
                    cmd.CommandText = "SELECT * FROM [" + dr["TABLE_NAME"].ToString() + "]";
                    DataTable dt = new DataTable();
                    //dt.TableName = sheetName;

                    OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                    da.Fill(dt);
                    ds.Tables.Add(dt);
                    int EmRow = 0;
                    int EmCol = 0;
                    for (int r = 0; r < dt.Rows.Count - 1; r++)
                    {
                        for (int i = 0; i < dt.Columns.Count - 1; i++)
                        {
                            if (dt.Columns[i].DataType != typeof(string))
                            {
                                continue;
                            }
                            if (dt.Rows[r][i] != DBNull.Value && dt.Rows[r][i].ToString().Contains("Designation"))
                            {
                                DesignationCloumn = dt.Columns[i].ColumnName;
                            }
                            if (dt.Rows[r][i] != DBNull.Value && (string)dt.Rows[r][i] == "EmergencyStopGroup")
                            {
                                EmergSgColumn = dt.Columns[i].ColumnName;
                            }
                            if (dt.Rows[r][i] != DBNull.Value && (string)dt.Rows[r][i] == "Emergency Stop Group")
                            {
                                EmRow = r;
                                EmCol = i;
                            }
                        }
                    }
                    if (DesignationCloumn != string.Empty && EmergSgColumn != string.Empty)
                    {
                        List<EmergStopGroup> query = (from p in dt.AsEnumerable()
                                                      where p.Field<object>(DesignationCloumn) != null &&
                                                             !p.Field<string>(DesignationCloumn).Contains("Designation") &&
                                                             !p.Field<string>(DesignationCloumn).Contains("Emergency") &&
                                                             p.Field<string>(DesignationCloumn).Split('-').Length == 3

                                                      select new EmergStopGroup
                                                      {
                                                          Designation = p.Field<string>(DesignationCloumn),
                                                          EmergSg = p.Field<string>(EmergSgColumn)

                                                      }).ToList();
                        string Design = (string)dt.Rows[++EmRow][EmCol];

                        while (EmRow < dt.Rows.Count - 1 &&
                               dt.Rows[EmRow + 1][EmCol + 1] != DBNull.Value &&
                               (string)dt.Rows[EmRow + 1][EmCol + 1] != "")
                        {
                            query.Add(new EmergStopGroup
                            {
                                Designation = Design,
                                EmergSg = (string)dt.Rows[EmRow][EmCol + 1]

                            });
                            if (dt.Rows[EmRow + 1][EmCol] != DBNull.Value &&
                                (string)dt.Rows[EmRow + 1][EmCol] != "")
                            {
                                Design = (string)dt.Rows[EmRow + 1][EmCol];
                            }
                            EmRow++;
                        }
                        query.Add(new EmergStopGroup
                        {
                            Designation = Design,
                            EmergSg = (string)dt.Rows[EmRow][EmCol + 1]

                        });
                        //document.title += "SL " + dt.Rows[0][5].ToString() + " " + dt.Rows[0][6].ToString();
                        //document.title += ")";
                        return query;
                    }
                }
                cmd = null;
                conn.Close();
            }
            return null;
        }

        public List<DetLock> DetectorLockings(string dataSource, ref bool error)
        {
            DataSet ds = new DataSet();
            string connectionString = GetConnectionString(dataSource, "'Excel 12.0;HDR=NO;IMEX=1'");
            string path =
                Directory.GetFiles(Path.GetDirectoryName(dataSource),
                                   Path.GetFileNameWithoutExtension(dataSource) + "*.doc*").FirstOrDefault();
            
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                OleDbCommand cmd = new OleDbCommand
                {
                    Connection = conn
                };
                var dtExcelsheetname = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                string[] excelSheets = new String[dtExcelsheetname.Rows.Count];
                int j = 0;
                foreach (DataRow row in dtExcelsheetname.Rows)
                {
                    excelSheets[j] = row["TABLE_NAME"].ToString();
                    j++;
                }
                if (excelSheets.Contains("Cover$"))
                {
                    cmd.CommandText = "SELECT * FROM [Cover$]";
                    DataTable dt = new DataTable();
                    OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                    da.Fill(dt);
                    bool found = false;
                    for (int i = 0; i < dt.Columns.Count - 1; i++)
                    {
                        for (int a = 0; a < dt.Rows.Count; a++) //int i = 0; i < dt.Columns.Count - 1; i++
                        {
                            if (dt.Rows[a][i] != DBNull.Value && (((string)dt.Rows[a][i]).Contains("Document Number") ||
                                                                 ((string)dt.Rows[a][i]).Contains("Tegningsnr")))
                            {
                                //document.docID = dt.Rows[a][i + 3].ToString();
                                //document.version = dt.Rows[a][i + 7].ToString();
                            }
                            //if (dt.Rows[a][i] != DBNull.Value && ((string)dt.Rows[a][i]).Contains("Version"))
                            //{
                            //    document.version = dt.Rows[a][i + 3].ToString();
                            //}
                            if (dt.Rows[a][i] != DBNull.Value && ((string)dt.Rows[a][i]).Contains("Udarbejdet"))
                            {
                                //document.creator = dt.Rows[a][i + 2].ToString().Split(' ')[1];
                                //document.date =
                                //ExpPt1.Calc.StringToDate(dt.Rows[a][i + 2].ToString().Split(' ')[0], out DateTime date, out bool flag);
                                //document.date = Convert.ToDateTime(dt.Rows[a][i + 2].ToString().Split(' ')[0]);
                                found = true;
                                break;
                            }
                        }
                        if (found)
                        {
                            break;
                        }
                    }
                }
            }
            //if (document.docID == null)
            //{
            //    document = Word.GetCoverData(path, ref error);
            //    document.title = "Tracks for Detector Locking";
            //}

            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                OleDbCommand cmd = new OleDbCommand
                {
                    Connection = conn
                };

                // Get all Sheets in Excel File
                DataTable dtSheet = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                string PointCoulumn = "";
                string TdtTipColumn = "";
                string TdtLeftColumn = "";
                string TdtRightColumn = "";
                string TdtPtTipColumn = "";
                string TdtPtLeftColumn = "";
                string TdtPtRightColumn = "";
                foreach (DataRow dr in dtSheet.Rows)
                {
                    if (!dr["TABLE_NAME"].ToString().Trim(new char[] { (char)39 }).EndsWith("$"))
                        continue;
                    cmd.CommandText = "SELECT * FROM [" + dr["TABLE_NAME"].ToString() + "]";
                    DataTable dt = new DataTable();
                    //dt.TableName = sheetName;

                    OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                    da.Fill(dt);
                    ds.Tables.Add(dt);
                    foreach (DataRow drow in dt.Rows)
                    {
                        for (int i = 0; i < dt.Columns.Count - 1; i++)
                        {
                            if (drow[i] != DBNull.Value && (((string)drow[i]).ToLower() == "point"))
                            {
                                PointCoulumn = dt.Columns[i].ColumnName;
                            }
                            if (drow[i] != DBNull.Value && (((string)drow[i]).Contains("Tip") ||
                                ((string)drow[i]).Contains("tip")))
                            {
                                TdtTipColumn = dt.Columns[i].ColumnName;
                                TdtPtTipColumn = dt.Columns[i + 1].ColumnName;
                            }
                            if (drow[i] != DBNull.Value && (((string)drow[i]).Contains("Left") ||
                                ((string)drow[i]).Contains("left")))
                            {
                                TdtLeftColumn = dt.Columns[i].ColumnName;
                                TdtPtLeftColumn = dt.Columns[i + 1].ColumnName;
                            }
                            if (drow[i] != DBNull.Value && (((string)drow[i]).Contains("Right") ||
                                ((string)drow[i]).Contains("right")))
                            {
                                TdtRightColumn = dt.Columns[i].ColumnName;
                                TdtPtRightColumn = dt.Columns[i + 1].ColumnName;
                            }
                        }
                    }
                    if (PointCoulumn != string.Empty)
                    {
                        List<DetLock> query = (from p in dt.AsEnumerable()
                                               where p.Field<object>(PointCoulumn) != null &&
                                                          p.Field<string>(PointCoulumn) != "Point"
                                               select new DetLock
                                               {
                                                   Pt = p.Field<string>(PointCoulumn),
                                                   Adjacents = new List<DetLock.Adjacent>
                                                   {
                                                       new DetLock.Adjacent
                                                       {
                                                           Tdts = p.Field<object>(TdtTipColumn) == null ? new List<string>() :
                                                           p.Field<string>(TdtTipColumn).Split(new string[] {"\r\n",";",",","\n","\t"," "},
                                                           StringSplitOptions.RemoveEmptyEntries).ToList(),
                                                           Pts = p.Field<object>(TdtPtTipColumn) == null ? new List<string>() :
                                                           p.Field<string>(TdtPtTipColumn).Split(new string[] {"\r\n",";",",","\n","\t"," "},
                                                           StringSplitOptions.RemoveEmptyEntries).ToList()
                                                       },
                                                       new DetLock.Adjacent
                                                       {
                                                           Tdts = p.Field<object>(TdtLeftColumn) == null ? new List<string>() :
                                                           p.Field<string>(TdtLeftColumn).Split(new string[] {"\r\n",";",",","\n","\t"," "},
                                                           StringSplitOptions.RemoveEmptyEntries).ToList(),
                                                           Pts = p.Field<object>(TdtPtLeftColumn) == null ? new List<string>() :
                                                           p.Field<string>(TdtPtLeftColumn).Split(new string[] {"\r\n",";",",","\n","\t"," "},
                                                           StringSplitOptions.RemoveEmptyEntries).ToList()
                                                       },
                                                       new DetLock.Adjacent
                                                       {
                                                           Tdts = p.Field<object>(TdtRightColumn) == null ? new List<string>() :
                                                           p.Field<string>(TdtRightColumn).Split(new string[] {"\r\n",";",",","\n","\t"," "},
                                                           StringSplitOptions.RemoveEmptyEntries).ToList(),
                                                           Pts = p.Field<object>(TdtPtRightColumn) == null ? new List<string>() :
                                                           p.Field<string>(TdtPtRightColumn).Split(new string[] {"\r\n",";",",","\n","\t"," "},
                                                           StringSplitOptions.RemoveEmptyEntries).ToList()
                                                       }
                                                   }
                                               }).ToList();
                        return query;
                    }
                }
                cmd = null;
                conn.Close();
            }
            return null;
        }

        public List<SpeedProfilesSpeedProfile> SpeedProfiles(string dataSource, string stationID, ref bool error)
        {
            DataSet ds = new DataSet();

            string connectionString = GetConnectionString(dataSource,
                "'Excel 12.0;IMEX=1;HDR=NO'");
            //ReadWord.Word Word = new ReadWord.Word();
            string path =
                Directory.GetFiles(Path.GetDirectoryName(dataSource),
                                   Path.GetFileNameWithoutExtension(dataSource) + "*.doc?").FirstOrDefault();
            //document = Word.GetCoverData(path, ref error);
            

            List<SpeedProfilesSpeedProfile> speedProfiles = new List<SpeedProfilesSpeedProfile>();
            if (!File.Exists(dataSource))
            {
                ErrLogger.Error("File does not exist", "SSP", dataSource);
                error = true;
                return speedProfiles;
            }

            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                OleDbCommand cmd = new OleDbCommand
                {
                    Connection = conn
                };

                DataTable dtSheet = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                foreach (DataRow dr in dtSheet.Rows)
                {
                    if (!dr["TABLE_NAME"].ToString().Trim(new char[] { (char)39 }).EndsWith("$"))
                        continue;
                    if (dr["TABLE_NAME"].ToString().Contains("SpeedProfile") || dr["TABLE_NAME"].ToString().Contains("Speed Profile"))
                    {
                        cmd.CommandText = "SELECT * FROM [" + dr["TABLE_NAME"].ToString() + "]";
                        break;
                    }
                }

                DataTable dt = new DataTable();
                OleDbDataAdapter da = new OleDbDataAdapter(cmd);

                da.Fill(dt);
                DataTable dtCloned = null;
                dtCloned = dt.Clone();

                ds.Tables.Add(dt);
                int sspCount = 1;
                foreach (DataColumn col in dtCloned.Columns)
                {
                    col.DataType = typeof(string);
                }
                foreach (DataRow row in dt.Rows)
                {
                    dtCloned.ImportRow(row);
                }
                foreach (DataColumn col in dtCloned.Columns)
                {
                    if (dt.Rows[0][col.ColumnName] != DBNull.Value)
                    {
                        if (!dtCloned.Columns.Contains(dt.Rows[0][col.ColumnName].ToString()))
                        {
                            col.ColumnName = dt.Rows[0][col.ColumnName].ToString();
                        }
                    }
                }
                dtCloned.Rows[0].Delete();
                dtCloned.AcceptChanges();
                if (dtCloned.Columns.IndexOf("CD=100mm") == -1)
                {
                    dtCloned.Columns.Add("CD=100mm", typeof(string));
                }
                if (dtCloned.Columns.IndexOf("CD=130mm") == -1)
                {
                    dtCloned.Columns.Add("CD=130mm", typeof(string));
                }
                if (dtCloned.Columns.IndexOf("CD=150mm") == -1)
                {
                    dtCloned.Columns.Add("CD=150mm", typeof(string));
                }

                if (dtCloned.Columns.IndexOf("CD=165mm") == -1)
                {
                    dtCloned.Columns.Add("CD=165mm", typeof(string));
                }
                if (dtCloned.Columns.IndexOf("CD=180mm") == -1)
                {
                    dtCloned.Columns.Add("CD=180mm", typeof(string));
                }
                if (dtCloned.Columns.IndexOf("CD=210mm") == -1)
                {
                    dtCloned.Columns.Add("CD=210mm", typeof(string));
                }
                if (dtCloned.Columns.IndexOf("CD=225mm") == -1)
                {
                    dtCloned.Columns.Add("CD=225mm", typeof(string));
                }
                if (dtCloned.Columns.IndexOf("CD=245mm") == -1)
                {
                    dtCloned.Columns.Add("CD=245mm", typeof(string));
                }
                if (dtCloned.Columns.IndexOf("CD=275mm") == -1)
                {
                    dtCloned.Columns.Add("CD=275mm", typeof(string));
                }
                if (dtCloned.Columns.IndexOf("CD=300mm") == -1)
                {
                    dtCloned.Columns.Add("CD=300mm", typeof(string));
                }
                if (dtCloned.Columns.IndexOf("Axle Load Category C2") == -1)
                {
                    dtCloned.Columns.Add("Axle Load Category C2", typeof(string));
                }
                if (dtCloned.Columns.IndexOf("Other, Freight P, not replacing CD") == -1)
                {
                    dtCloned.Columns.Add("Other, Freight P, not replacing CD", typeof(string));
                }
                if (dtCloned.Columns.IndexOf("Other, Freight G, not replacing CD") == -1)
                {
                    dtCloned.Columns.Add("Other, Freight G, not replacing CD", typeof(string));
                }
                //dt.Columns[0].DataType                
                List<ExpPt1.SspSimp> ssps = dtCloned.AsEnumerable()
                                            .Where(x => x.Field<object>("Basic") != null)
                                            .Select(x => new ExpPt1.SspSimp
                                            {
                                                TrackSegment = x.Field<string>("TrackSegments"),
                                                Km1 = x.Field<string>("OperationalKm1").Replace(",", "."),
                                                Km2 = x.Field<string>("OperationalKm2").Replace(",", "."),
                                                Basic = x.Field<string>("Basic"),
                                                AlC2 = x.Field<string>("Axle Load Category C2"),
                                                Fp = x.Field<string>("Other, Freight P, not replacing CD"),
                                                Fg = x.Field<string>("Other, Freight G, not replacing CD"),
                                                Cd100 = x.Field<string>("CD=100mm"),
                                                Cd130 = x.Field<string>("CD=130mm"),
                                                Cd150 = x.Field<string>("CD=150mm"),
                                                Cd165 = x.Field<string>("CD=165mm"),
                                                Cd180 = x.Field<string>("CD=180mm"),
                                                Cd210 = x.Field<string>("CD=210mm"),
                                                Cd225 = x.Field<string>("CD=225mm"),
                                                Cd245 = x.Field<string>("CD=245mm"),
                                                Cd275 = x.Field<string>("CD=275mm"),
                                                Cd300 = x.Field<string>("CD=300mm"),
                                                Direction = (UpDownBothType)Enum.Parse(typeof(UpDownBothType),
                                                                 x.Field<string>("Direction").ToLower()),
                                                Remarks = x.Field<string>("Remarks")
                                            })
                                            .ToList();

                var sspGrps = ssps
                              .OrderBy(x => Convert.ToDouble(x.Km1))
                              .GroupBy(x => new
                              {
                                  x.Basic,
                                  x.AlC2,
                                  x.Fp,
                                  x.Fg,
                                  x.Cd100,
                                  x.Cd130,
                                  x.Cd150,
                                  x.Cd165,
                                  x.Cd180,
                                  x.Cd210,
                                  x.Cd225,
                                  x.Cd245,
                                  x.Cd275,
                                  x.Cd300
                              }).ToList();
                foreach (var sspGrp in sspGrps)
                {
                    List<SpeedProfilesSpeedProfileTrainTypesTrainTyp> typesTrainTyps = new List<SpeedProfilesSpeedProfileTrainTypesTrainTyp>();
                    List<TrackSegmentType> trackSegments = new List<TrackSegmentType>();
                    foreach (var ssp in sspGrp)
                    {
                        trackSegments.Add(new TrackSegmentType
                        {
                            Value = blckProp.GetElemDesignation(ssp.TrackSegment),
                            OperationalKM1 = ssp.Km1,
                            OperationalKM2 = ssp.Km2,
                        });
                    }
                    if (sspGrp.First().Remarks != null && sspGrp.First().Remarks.Trim() == "-")
                    {
                        sspGrp.First().Remarks = null;
                    }
                    if (sspGrp.First().AlC2 != null && sspGrp.First().AlC2 != "-")
                    {
                        typesTrainTyps.Add(new SpeedProfilesSpeedProfileTrainTypesTrainTyp
                        {
                            Item = "C2",
                            SpeedLimit = Convert.ToDecimal(sspGrp.First().AlC2)
                        });
                    }
                    if (sspGrp.First().Fp != null && sspGrp.First().Fp != "-")
                    {
                        typesTrainTyps.Add(new SpeedProfilesSpeedProfileTrainTypesTrainTyp
                        {
                            Item = new SpeedProfilesSpeedProfileTrainTypesTrainTypTrainCategory
                            {
                                CategoryType = KindOfTrainCategory.freightTrainInP,
                                ReplaceCDAndBasicLimit = YesNoType.no
                            },
                            SpeedLimit = Convert.ToDecimal(sspGrp.First().Fp)
                        });
                    }
                    if (sspGrp.First().Fg != null && sspGrp.First().Fg != "-")
                    {
                        typesTrainTyps.Add(new SpeedProfilesSpeedProfileTrainTypesTrainTyp
                        {
                            Item = new SpeedProfilesSpeedProfileTrainTypesTrainTypTrainCategory
                            {
                                CategoryType = KindOfTrainCategory.freightTrainInG,
                                ReplaceCDAndBasicLimit = YesNoType.no
                            },
                            SpeedLimit = Convert.ToDecimal(sspGrp.First().Fg)
                        });
                    }
                    if (sspGrp.First().Cd100 != null && sspGrp.First().Cd100 != "-")
                    {
                        typesTrainTyps.Add(new SpeedProfilesSpeedProfileTrainTypesTrainTyp
                        {
                            Item = KindOfCantDeficiancy.Item100,
                            SpeedLimit = Convert.ToDecimal(sspGrp.First().Cd100)
                        });
                    }
                    if (sspGrp.First().Cd130 != null && sspGrp.First().Cd130 != "-")
                    {
                        typesTrainTyps.Add(new SpeedProfilesSpeedProfileTrainTypesTrainTyp
                        {
                            Item = KindOfCantDeficiancy.Item130,
                            SpeedLimit = Convert.ToDecimal(sspGrp.First().Cd130)
                        });
                    }
                    if (sspGrp.First().Cd150 != null && sspGrp.First().Cd150 != "-")
                    {
                        typesTrainTyps.Add(new SpeedProfilesSpeedProfileTrainTypesTrainTyp
                        {
                            Item = KindOfCantDeficiancy.Item150,
                            SpeedLimit = Convert.ToDecimal(sspGrp.First().Cd150)
                        });
                    }
                    if (sspGrp.First().Cd165 != null && sspGrp.First().Cd165 != "-")
                    {
                        typesTrainTyps.Add(new SpeedProfilesSpeedProfileTrainTypesTrainTyp
                        {
                            Item = KindOfCantDeficiancy.Item165,
                            SpeedLimit = Convert.ToDecimal(sspGrp.First().Cd165)
                        });
                    }
                    if (sspGrp.First().Cd180 != null && sspGrp.First().Cd180 != "-")
                    {
                        typesTrainTyps.Add(new SpeedProfilesSpeedProfileTrainTypesTrainTyp
                        {
                            Item = KindOfCantDeficiancy.Item180,
                            SpeedLimit = Convert.ToDecimal(sspGrp.First().Cd180)
                        });
                    }
                    if (sspGrp.First().Cd210 != null && sspGrp.First().Cd210 != "-")
                    {
                        typesTrainTyps.Add(new SpeedProfilesSpeedProfileTrainTypesTrainTyp
                        {
                            Item = KindOfCantDeficiancy.Item210,
                            SpeedLimit = Convert.ToDecimal(sspGrp.First().Cd210)
                        });
                    }
                    if (sspGrp.First().Cd225 != null && sspGrp.First().Cd225 != "-")
                    {
                        typesTrainTyps.Add(new SpeedProfilesSpeedProfileTrainTypesTrainTyp
                        {
                            Item = KindOfCantDeficiancy.Item225,
                            SpeedLimit = Convert.ToDecimal(sspGrp.First().Cd225)
                        });
                    }
                    if (sspGrp.First().Cd245 != null && sspGrp.First().Cd245 != "-")
                    {
                        typesTrainTyps.Add(new SpeedProfilesSpeedProfileTrainTypesTrainTyp
                        {
                            Item = KindOfCantDeficiancy.Item245,
                            SpeedLimit = Convert.ToDecimal(sspGrp.First().Cd245)
                        });
                    }
                    if (sspGrp.First().Cd275 != null && sspGrp.First().Cd275 != "-")
                    {
                        typesTrainTyps.Add(new SpeedProfilesSpeedProfileTrainTypesTrainTyp
                        {
                            Item = KindOfCantDeficiancy.Item275,
                            SpeedLimit = Convert.ToDecimal(sspGrp.First().Cd275)
                        });
                    }
                    if (sspGrp.First().Cd300 != null && sspGrp.First().Cd300 != "-")
                    {
                        typesTrainTyps.Add(new SpeedProfilesSpeedProfileTrainTypesTrainTyp
                        {
                            Item = KindOfCantDeficiancy.Item300,
                            SpeedLimit = Convert.ToDecimal(sspGrp.First().Cd300)
                        });
                    }
                    SpeedProfilesSpeedProfile speedProfile = new SpeedProfilesSpeedProfile
                    {
                        Designation = "ssp-" + stationID + "-" + sspCount++.ToString().PadLeft(3, '0'),
                        SpeedMax = Convert.ToDecimal(sspGrp.First().Basic),
                        Remarks = sspGrp.First().Remarks,
                        DirectionAll = sspGrp.First().Direction,

                    };
                    speedProfile.TrackSegments = new SpeedProfilesSpeedProfileTrackSegments
                    {
                        TrackSegment = trackSegments.ToArray()
                    };
                    if (typesTrainTyps.Count > 0)
                    {
                        speedProfile.TrainTypes = new SpeedProfilesSpeedProfileTrainTypes
                        {
                            TrainTyp = typesTrainTyps.ToArray()
                        };
                    }
                    speedProfiles.Add(speedProfile);
                }
                cmd = null;
                conn.Close();
            }
            return speedProfiles;
        }

        public List<XlsRoute> Routes(string dataSource, ref bool error)
        {
            DataSet ds = new DataSet();
            string connectionString = GetConnectionString(dataSource, "'Excel 12.0;HDR=NO;IMEX=1'");
            //ReadWord.Word Word = new ReadWord.Word();
            string path =
                Directory.GetFiles(Path.GetDirectoryName(dataSource),
                                   Path.GetFileNameWithoutExtension(dataSource) + "*.doc*").FirstOrDefault();
            //document = Word.GetCoverData(path, ref error);
            //document.title = "Routes Table";

            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                OleDbCommand cmd = new OleDbCommand
                {
                    Connection = conn
                };

                // Get all Sheets in Excel File
                DataTable dtSheet = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                string TypeCoulumn = "";
                string StartColumn = "";
                string DestColumn = "";
                string PrefColumn = "";
                string PointsColumn = "";
                string PointsGrpColumn = "";
                string ActCrossColumn = "";
                string SafeDistColumn = "";
                string SdLastColumn = "";
                string StartAreaColumn = "";
                string ExtendDestColumn = "";
                string OverLapColumn = "";
                string NotesColumn = "";
                foreach (DataRow dr in dtSheet.Rows)
                {
                    if (!dr["TABLE_NAME"].ToString().Trim(new char[] { (char)39 }).EndsWith("$"))
                        continue;
                    cmd.CommandText = "SELECT * FROM [" + dr["TABLE_NAME"].ToString() + "]";
                    DataTable dt = new DataTable();
                    //dt.TableName = sheetName;

                    OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                    da.Fill(dt);
                    ds.Tables.Add(dt);
                    foreach (DataRow drow in dt.Rows)
                    {
                        for (int i = 0; i < dt.Columns.Count - 1; i++)
                        {
                            if (drow[i] != DBNull.Value && (string)drow[i] == "Type of route")
                            {
                                TypeCoulumn = dt.Columns[i].ColumnName;
                            }
                            if (drow[i] != DBNull.Value && ((string)drow[i]).Contains("Start of"))
                            {
                                StartColumn = dt.Columns[i].ColumnName;
                            }
                            if (drow[i] != DBNull.Value && ((string)drow[i]).Contains("End of"))
                            {
                                DestColumn = dt.Columns[i].ColumnName;
                            }
                            if (drow[i] != DBNull.Value && ((string)drow[i]).Contains("Preferred"))
                            {
                                PrefColumn = dt.Columns[i].ColumnName;
                            }
                            if (drow[i] != DBNull.Value && ((string)drow[i]).Contains("Via points"))
                            {
                                PointsColumn = dt.Columns[i].ColumnName;
                            }
                            if (drow[i] != DBNull.Value && ((string)drow[i]).ToLower().Contains("point group"))
                            {
                                PointsGrpColumn = dt.Columns[i].ColumnName;
                            }
                            if (drow[i] != DBNull.Value && ((string)drow[i]).ToLower() == "overlap")
                            {
                                OverLapColumn = dt.Columns[i].ColumnName;
                            }
                            if (drow[i] != DBNull.Value && ((string)drow[i]).ToLower().Trim() == "notes")
                            {
                                NotesColumn = dt.Columns[i].ColumnName;
                            }
                            if (drow[i] != DBNull.Value && ((string)drow[i]).Contains("Activate crossing"))
                            {
                                ActCrossColumn = dt.Columns[i].ColumnName;
                            }
                            if (drow[i] != DBNull.Value && ((string)drow[i]).Contains("Safety Distance"))
                            {
                                SafeDistColumn = dt.Columns[i].ColumnName;
                            }
                            if (drow[i] != DBNull.Value && ((string)drow[i]).Contains("Sd Last Element"))
                            {
                                SdLastColumn = dt.Columns[i].ColumnName;
                            }
                            if (drow[i] != DBNull.Value && ((string)drow[i]).Contains("Start Area"))
                            {
                                StartAreaColumn = dt.Columns[i].ColumnName;
                            }
                            if (drow[i] != DBNull.Value && ((string)drow[i]).Contains("Extended destination"))
                            {
                                ExtendDestColumn = dt.Columns[i].ColumnName;
                            }
                        }
                    }
                    if (TypeCoulumn != string.Empty && StartColumn != string.Empty)
                    {
                        try
                        {
                            List<XlsRoute> query = (from p in dt.AsEnumerable()
                                                    where p.Field<object>(TypeCoulumn) != null &&
                                                               p.Field<string>(TypeCoulumn) != "Type of route" &&
                                                               p.Field<string>(NotesColumn).ToLower() != "not available"
                                                    select new XlsRoute
                                                    {
                                                        Type =
                                                                (KindOfRouteType)Enum
                                                                .Parse(typeof(KindOfRouteType),
                                                                 p.Field<string>(TypeCoulumn).ToLower().Contains("shunt") ?
                                                                    "shunting" : p.Field<string>(TypeCoulumn).ToLower()),
                                                        Start = p.Field<string>(StartColumn),
                                                        Dest = p.Field<string>(DestColumn),
                                                        Default = p.Field<string>(PrefColumn)
                                                                  .Trim().Any(x => x
                                                                  .ToString()
                                                                  .ToLower() == "x") ? YesNoType.yes : YesNoType.no,
                                                        SafeDist = p.Field<string>(SafeDistColumn).Trim()
                                                                   .Split(new string[] { "integrated", "-" }, StringSplitOptions.RemoveEmptyEntries).Length < 2 ? null :
                                                                   p.Field<string>(SafeDistColumn),
                                                        Points = (from pt in p.Field<string>(PointsColumn)
                                                                           .Split(new char[] { ';', ',', '.', ':', '\n', '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                                                           .Where(x => x.Trim() != "-")
                                                                  select new XlsPoint
                                                                  {
                                                                      Designation = pt.Split('-')[1].Trim() == "R" || pt.Split('-')[1].Trim() == "L" ?
                                                                         pt.Split('-')[0].Trim() : null,
                                                                      ReqPosition = pt.Split('-')[1].Trim() == "R" ? LeftRightType.right : LeftRightType.left,
                                                                  }),
                                                        PointsGrps = (from pt in p.Field<string>(PointsGrpColumn)
                                                                           .Split(new Char[] { ';', ',', '.', ':', '\n', '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                                                           .Where(x => x.Trim() != "-")
                                                                      select new XlsPoint
                                                                      {
                                                                          Designation = pt.Split('-')[1].Trim() == "R" || pt.Split('-')[1].Trim() == "L" ?
                                                                            pt.Split('-')[0].Trim() : null,
                                                                          ReqPosition = pt.Split('-')[1].Trim() == "R" ? LeftRightType.right : LeftRightType.left,
                                                                      }),
                                                        ActCross = p.Field<string>(ActCrossColumn)
                                                                   .Split(new string[] { "\r\n", ";", ",", ".", ":", "\n", "\t" },
                                                                            StringSplitOptions.RemoveEmptyEntries)
                                                                   .Where(x => x.Trim() != "-"),
                                                        SdLast = p.Field<object>(SdLastColumn) == null ||
                                                                 p.Field<string>(SdLastColumn).Trim() == "-" ?
                                                                 null : p.Field<string>(SdLastColumn),
                                                        StartAreas = p.Field<string>(StartAreaColumn)
                                                                     .Split(new string[] { "\r\n", ";", ",", ".", ":", "\n", "\t" },
                                                                            StringSplitOptions.RemoveEmptyEntries)
                                                                     .ToList(),
                                                        Overlaps = p.Field<string>(OverLapColumn)
                                                                     .Split(new string[] { "\r\n", ";", ",", "(", ")", "\n", "\t" },
                                                                            StringSplitOptions.RemoveEmptyEntries)
                                                                     .Where(x => x.Trim() != "-")
                                                                     .ToList(),
                                                        ExtDest = p.Field<object>(ExtendDestColumn) == null ||
                                                                 p.Field<string>(ExtendDestColumn).Trim() == "-" ?
                                                                 null : p.Field<string>(ExtendDestColumn),
                                                    }).ToList();
                            //foreach (var route in query)
                            //{
                            //    for (int j = 0; j < route.Overlaps.Count; j++)
                            //    {
                            //        route.Overlaps[j] = 
                            //            route.Overlaps[j].Trim().TrimStart('(').TrimEnd(')');
                            //    }
                            //}
                            return query;
                        }
                        catch (Exception ex)
                        {
                            ErrLogger.Error(ex.Message, "Routes", dataSource);
                            cmd = null;
                            conn.Close();
                            return null;
                        }
                    }
                    else
                    {

                    }
                }
                cmd = null;
                conn.Close();
            }
            return null;
        }

        public List<XlsCmpRoute> CompoundRoutes(
          string dataSource,
          ref TFileDescr document)
        {
            DataSet dataSet = new DataSet();
            string connectionString = this.GetConnectionString(dataSource, "'Excel 12.0;HDR=YES'");
            document.title = "Compound Routes Table";
            using (OleDbConnection oleDbConnection = new OleDbConnection(connectionString))
            {
                oleDbConnection.Open();
                OleDbCommand selectCommand = new OleDbCommand();
                selectCommand.Connection = oleDbConnection;
                selectCommand.CommandText = "SELECT * FROM [FrontPage$]";
                DataTable dataTable = new DataTable();
                new OleDbDataAdapter(selectCommand).Fill(dataTable);
                dataSet.Tables.Add(dataTable);
                for (int index1 = 0; index1 < dataTable.Rows.Count; ++index1)
                {
                    for (int index2 = 0; index2 < dataTable.Columns.Count - 1; ++index2)
                    {
                        if (dataTable.Rows[index1][index2] != DBNull.Value && ((string)dataTable.Rows[index1][index2]).Contains("Document Number"))
                            document.docID = dataTable.Rows[index1][index2 + 3].ToString();
                        if (dataTable.Rows[index1][index2] != DBNull.Value && ((string)dataTable.Rows[index1][index2]).Contains("Version"))
                            document.version = dataTable.Rows[index1][index2 + 3].ToString();
                        if (dataTable.Rows[index1][index2] != DBNull.Value && ((string)dataTable.Rows[index1][index2]).Contains("Prepared by"))
                        {
                            document.creator = dataTable.Rows[index1 + 1][index2].ToString();
                            document.date =
                            ExpPt1.Calc.StringToDate(dataTable.Rows[index1 + 2][index2].ToString(), out DateTime date, out bool flag);
                            //document.date = Convert.ToDateTime(dataTable.Rows[index1 + 2][index2].ToString());
                        }
                    }
                }
            }
            using (OleDbConnection oleDbConnection = new OleDbConnection(connectionString))
            {
                oleDbConnection.Open();
                OleDbCommand selectCommand = new OleDbCommand()
                {
                    Connection = oleDbConnection
                };
                DataTable oleDbSchemaTable = oleDbConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[4]
                {
          null,
          null,
          null,
          (object) "TABLE"
                });
                string str = "";
                Regex regex = new Regex("^.*Compound.*[$][']{0,1}$");
                foreach (DataRow row in (InternalDataCollectionBase)oleDbSchemaTable.Rows)
                {
                    if (regex.IsMatch(row["TABLE_NAME"].ToString()))
                        str = row["TABLE_NAME"].ToString();
                }
                selectCommand.CommandText = "SELECT * FROM [" + str + "]";
                DataTable dataTable = new DataTable();
                OleDbDataAdapter oleDbDataAdapter = new OleDbDataAdapter(selectCommand);
                try
                {
                    oleDbDataAdapter.Fill(dataTable);
                }
                catch
                {
                    ErrLogger.Error("Unable to query compound routes table", "RT table", dataSource);
                    oleDbConnection.Close();
                    return (List<XlsCmpRoute>)null;
                }
                dataSet.Tables.Add(dataTable);
                List<XlsCmpRoute> xlsCmpRouteList = new List<XlsCmpRoute>();
                for (int index1 = 0; index1 < dataTable.Rows.Count; ++index1)
                {
                    if (!(dataTable.Rows[index1]["Compound Route"].ToString() == ""))
                    {
                        List<string> stringList = new List<string>();
                        for (int index2 = 5; index2 < dataTable.Columns.Count; ++index2)
                        {
                            if (dataTable.Rows[index1][index2].ToString() != "")
                                stringList.Add(dataTable.Rows[index1][index2].ToString());
                        }
                        XlsCmpRoute xlsCmpRoute = new XlsCmpRoute()
                        {
                            Designation = dataTable.Rows[index1]["Compound Route"].ToString(),
                            Start = dataTable.Rows[index1]["Start Marker Board"].ToString(),
                            Dest = dataTable.Rows[index1]["Destination Marker Board"].ToString(),
                            Routes = stringList
                        };
                        xlsCmpRouteList.Add(xlsCmpRoute);
                    }
                }
                return xlsCmpRouteList;
            }
        }

        public List<XlsLX> LevelCrossings(string dataSource,
            ref TFileDescr document, string LxId, ref bool error)
        {
            DataSet ds = new DataSet();
            string connectionString = GetConnectionString(dataSource, "'Excel 12.0;HDR=NO;IMEX=1'");
            document.title = "Level Crossing Parameters";
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                OleDbCommand cmd = new OleDbCommand
                {
                    Connection = conn
                };
                cmd.CommandText = "SELECT * FROM [FrontPage$]";
                //DataTable dt = new DataTable();
                //dt.TableName = sheetName;
                DataTable dt = new DataTable();
                OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                da.Fill(dt);
                ds.Tables.Add(dt);

                for (int a = 0; a < dt.Rows.Count; a++)
                {
                    for (int i = 0; i < dt.Columns.Count - 1; i++)
                    {
                        if (dt.Rows[a][i] != DBNull.Value && ((string)dt.Rows[a][i]).Contains("Document Number"))
                        {
                            int d = 1;
                            while (dt.Rows[a][i + d].ToString() == "")
                            {
                                d++;
                            }
                            document.docID = dt.Rows[a][i + d].ToString();
                        }
                        if (dt.Rows[a][i] != DBNull.Value && ((string)dt.Rows[a][i]).Contains("Version"))
                        {
                            int d = 1;
                            while (dt.Rows[a][i + d].ToString() == "")
                            {
                                d++;
                            }
                            document.version = dt.Rows[a][i + d].ToString();
                        }
                        if (dt.Rows[a][i] != DBNull.Value && ((string)dt.Rows[a][i]).Contains("Prepared by"))
                        {
                            document.creator = dt.Rows[a + 1][i].ToString();
                            DateTime docDate = new DateTime();
                            Calc.StringToDate(dt.Rows[a + 2][i].ToString(), out docDate, out bool flag, false);
                            if (!flag)
                            {
                                Calc.StringToDate(dt.Rows[a + 3][i].ToString(), out docDate, out flag, true);
                            }
                            document.date = docDate;
                        }
                    }
                }
            }
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                OleDbCommand cmd = new OleDbCommand
                {
                    Connection = conn
                };
                cmd.CommandText = "SELECT * FROM [Level Crossing Parameters$]";
                DataTable dt = new DataTable();
                //dt.TableName = sheetName;

                OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                da.Fill(dt);
                ds.Tables.Add(dt);
                string ValueColumn = "";
                string DecrColumn = "";

                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    for (int j = 0; j < 7; j++)
                    {
                        if (dt.Rows[j][i] != DBNull.Value && dt.Rows[j][i].ToString().ToLower().Contains(LxId.ToLower()))
                        {
                            ValueColumn = dt.Columns[i].ColumnName;
                            DecrColumn = dt.Columns[4].ColumnName;
                            break;
                        }
                    }
                }
                if (ValueColumn == "")
                {
                    ErrLogger.Error("LX not found in crossings parameters table", LxId.ToLower(), "");
                    error = true;
                    return null;
                }
                int index = 1;
                List<XlsLX> query = (from p in dt.AsEnumerable()
                                     where p.Field<object>(DecrColumn) != null
                                     select new XlsLX
                                     {
                                         Index = index++,
                                         Value = p.Field<object>(ValueColumn) != null ?
                                         p.Field<string>(ValueColumn)
                                         .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                                         .First() :
                                         null,
                                         Reference = p.Field<string>(DecrColumn).ToLower()
                                     }).ToList();
                return query;
            }

        }

        public XlsPwsActivation PwsActivation(string dataSource,
             ref TFileDescr document, ExpPt1.Block blkRefLx, string name)
        {
            DataSet ds = new DataSet();
            string connectionString = GetConnectionString(dataSource, "'Excel 12.0;HDR=NO;IMEX=1'");
            document.title = name + " " + blkRefLx.Attributes["NAME"].Value + " Activation delays";
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                OleDbCommand cmd = new OleDbCommand
                {
                    Connection = conn
                };

                //var dtExcelsheetname = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                //string InputDocSheet = "";
                //foreach (DataRow row in dtExcelsheetname.Rows)
                //{
                //    if (row["TABLE_NAME"].ToString().Contains("mrk-"))
                //    {
                //        InputDocSheet = row["TABLE_NAME"].ToString();
                //        break;
                //    }
                //}
                //cmd.CommandText = "SELECT * FROM [" + InputDocSheet + "]";
                //DataTable dtInput = new DataTable();
                //OleDbDataAdapter daInput = new OleDbDataAdapter(cmd);
                //daInput.Fill(dtInput);
                //var sl = dtInput.Rows[0][1].ToString()
                //            .Split(new string[]{ "Signalling_layout_",
                //                                 "Signalling layout ",
                //                                 "signalling_layout_",
                //                                 "signalling layout ",
                //                                 "Signalling_Layout_",
                //                                 "Signalling Layout ",
                //                                 " ", "_", "Ed", "ED", "ed" }, StringSplitOptions.RemoveEmptyEntries);
                //if (sl.Length >= 4)
                //{
                //    document.title += "SL v/" + sl[3] + ", " + sl[1] + "";
                //}
                //var ssp = dtInput.Rows[1][1].ToString()
                //            .Split(new string[] { "Speed_profile_",
                //                                  "Speed profile ",
                //                                  "speed_profile_",
                //                                  "speed profile ",
                //                                  "Speed_Profile_",
                //                                  "Speed Profile ",
                //                                  " ", "_", "Ed", "ED", "ed" }, StringSplitOptions.RemoveEmptyEntries);
                //if (ssp.Length >= 4)
                //{
                //    document.title += "; SSP v/" + ssp[3] + ", " + ssp[1] + "";
                //}
                //document.title += ")";

                cmd.CommandText = "SELECT * FROM [FrontPage$]";
                //DataTable dt = new DataTable();
                //dt.TableName = sheetName;
                DataTable dt = new DataTable();
                OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                da.Fill(dt);
                ds.Tables.Add(dt);

                for (int a = 0; a < dt.Rows.Count; a++)
                {
                    for (int i = 0; i < dt.Columns.Count - 1; i++)
                    {
                        if (dt.Rows[a][i] != DBNull.Value && ((string)dt.Rows[a][i]).Contains("Document Number"))
                        {
                            document.docID = dt.Rows[a][i + 3].ToString();
                        }
                        if (dt.Rows[a][i] != DBNull.Value && ((string)dt.Rows[a][i]).Contains("Version"))
                        {
                            document.version = dt.Rows[a][i + 3].ToString();
                        }
                        if (dt.Rows[a][i] != DBNull.Value && ((string)dt.Rows[a][i]).Contains("Prepared by"))
                        {
                            document.creator = dt.Rows[a + 1][i].ToString();
                            document.date =
                            ExpPt1.Calc.StringToDate(dt.Rows[a + 2][i].ToString(), out DateTime date, out bool flag);
                            //document.date = Convert.ToDateTime(dt.Rows[a + 2][i].ToString());
                        }
                    }
                }
            }
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                OleDbCommand cmd = new OleDbCommand
                {
                    Connection = conn
                };
                cmd.CommandText = "SELECT * FROM [Activation sections$]";
                DataTable dt = new DataTable();
                //dt.TableName = sheetName;

                OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                da.Fill(dt);
                ds.Tables.Add(dt);

                XlsPwsActivation activation =
                    new XlsPwsActivation
                    {
                        ActivationSections =
                        new List<PWSactSection>()
                    };
                int ColumnNumber = 0;
                int RowNumber = 0;
                for (int r = 0; r < dt.Rows.Count; r++)
                {
                    bool br = false;
                    for (int i = 0; i < dt.Columns.Count - 1; i++)
                    {
                        if (dt.Rows[r][i] != DBNull.Value && ((string)dt.Rows[r][i]).Contains("ActivationDelays"))
                        {
                            ColumnNumber = i;
                            RowNumber = r;
                            br = true;
                            break;
                        }
                    }
                    if (br)
                    {
                        break;
                    }
                }
                int iterCount = 0;
                Regex tdtRegex = new Regex(ExpPt1.Constants.tdtNameRegexp);
                for (int a = RowNumber; a < dt.Rows.Count; a++)
                {
                    if (ExpPt1.Constants.maxSxDelays == iterCount)
                    {
                        break;
                    }
                    else
                    {
                        iterCount = 0;
                    }
                    PWSactSection section = new PWSactSection();
                    Regex delays = new Regex("ActivationDelays");
                    List<PwsActDl> activationDelays =
                                 new List<PwsActDl>();
                    string actName = (string)dt.Rows[a - 2][ColumnNumber + 1];

                    activation.SpeedIfUnprotectedUp = (dt.Rows[a][ColumnNumber + 6] as string) ?? "";
                    activation.SpeedIfUnprotectedDown = (dt.Rows[a + 1][ColumnNumber + 6] as string) ?? "";
                    activation.TSRStartInRearOfAreaUp = (dt.Rows[a + 2][ColumnNumber + 6] as string) ?? "";
                    activation.TSRStartInRearOfAreaDown = (dt.Rows[a + 3][ColumnNumber + 6] as string) ?? "";
                    activation.TSRExtensionBeyondAreaUp = (dt.Rows[a + 4][ColumnNumber + 6] as string) ?? "";
                    activation.TSRExtensionBeyondAreaDown = (dt.Rows[a + 5][ColumnNumber + 6] as string) ?? "";

                    while ((a <= (dt.Rows.Count - 1)) &&
                           (dt.Rows[a][ColumnNumber] == DBNull.Value || delays.IsMatch((string)dt.Rows[a][ColumnNumber])))
                    {
                        if (dt.Rows[a][ColumnNumber] != DBNull.Value &&
                           ((string)dt.Rows[a][ColumnNumber]) == "ActivationAxleCounterSectionID")
                        {
                            break;
                        }
                        if (!decimal.TryParse((string)dt.Rows[a][ColumnNumber + 1], out decimal mTrSpeed))
                        {
                            ErrLogger.Error("Unable parse maxTrainSpeed", blckProp.GetElemDesignation(blkRefLx),actName);
                            ErrLogger.ErrorsFound = true;
                        }
                        if (!decimal.TryParse((string)dt.Rows[a][ColumnNumber + 2], out decimal actDelT))
                        {
                            ErrLogger.Error("Unable parse ActivationDelayTime", blckProp.GetElemDesignation(blkRefLx), actName);
                            ErrLogger.ErrorsFound = true;
                        }
                        activationDelays.Add(new PwsActDl
                        {
                            maxTrainSpeed = mTrSpeed,
                            ActivationDelayTime = actDelT
                        });
                        a += 1;
                    }
                    section.ActivationDelays = new PwsActDls
                    {
                        ActivationDelay = activationDelays.ToArray()
                    };

                    if (tdtRegex.IsMatch((string)dt.Rows[a][ColumnNumber + 1]))
                    {
                        section.ActivationAxleCounterSectionID = (string)dt.Rows[a][ColumnNumber + 1];
                    }
                    else
                    {
                        ErrLogger.Error("wrong ActivationAxleCounterSectionID", blckProp.GetElemDesignation(blkRefLx), actName + " " +
                                 (string)dt.Rows[a][ColumnNumber + 1]);
                        ErrLogger.ErrorsFound = true;
                    }

                    a++;
                    section.RouteChain = new PwsChain();
                    List<string> chains = new List<string>();
                    while ((a <= (dt.Rows.Count - 1)) &&
                           !(dt.Rows[a][ColumnNumber] != DBNull.Value && ((string)dt.Rows[a][ColumnNumber]).Contains("DestinationArea")))
                    {
                        if (dt.Rows[a][ColumnNumber + 1] != DBNull.Value)
                        {
                            chains.Add(dt.Rows[a][ColumnNumber + 1].ToString());
                        }
                        a++;
                    }
                    section.RouteChain.RouteID = chains.ToArray();
                    a++;
                    if (tdtRegex.IsMatch((string)dt.Rows[a][ColumnNumber + 1]))
                    {
                        section.LxAxleCounterSectionID = (string)dt.Rows[a][ColumnNumber + 1];
                    }
                    else
                    {
                        ErrLogger.Error("Wrong LxAxleCounterSectionID", blckProp.GetElemDesignation(blkRefLx), actName + " " +
                                 (string)dt.Rows[a][ColumnNumber + 1]);
                        ErrLogger.ErrorsFound = true;
                    }

                    a++;

                    while ((a <= (dt.Rows.Count - 1)) &&
                           !(dt.Rows[a][ColumnNumber] != DBNull.Value && ((string)dt.Rows[a][ColumnNumber]).Contains("DeactivationAxleCounterSectionID")))
                    {
                        a++;
                    }
                    if (tdtRegex.IsMatch((string)dt.Rows[a][ColumnNumber + 1]))
                    {
                        section.DeactivationAxleCounterSectionID = (string)dt.Rows[a][ColumnNumber + 1];
                    }
                    else
                    {
                        ErrLogger.Error("Wrong DeactivationAxleCounterSectionID", blckProp.GetElemDesignation(blkRefLx), actName + " " +
                                 (string)dt.Rows[a][ColumnNumber + 1]);
                        ErrLogger.ErrorsFound = true;
                    }
                    activation.ActivationSections.Add(section);

                    while ((a <= (dt.Rows.Count - 1)) &&
                           !(dt.Rows[a][ColumnNumber] != DBNull.Value && ((string)dt.Rows[a][ColumnNumber]).Contains("ActivationDelays")))
                    {
                        iterCount++;
                        a++;
                    }
                    a--;
                }
                return activation;
            }
        }

        public Dictionary<string, XlsLxActivation> LoopLxActivations(string DwgDir)
        {
            List<FileInfo> fileInfos = new DirectoryInfo(DwgDir)
                   .EnumerateFiles("LX*_*.xls*")
                   .ToList();
            Dictionary<string, XlsLxActivation> LxActs = new Dictionary<string, XlsLxActivation>();
            foreach (var info in fileInfos)
            {
                string lxName = info.Name.Split('_').First().Replace("LX", "");
                LxActs.Add(lxName, LxActivations(info.FullName, lxName));
            }
            return LxActs;
        }

        private XlsLxActivation LxActivations(string dataSource, string LXname)
        {
            DataSet ds = new DataSet();
            string connectionString = GetConnectionString(dataSource, "'Excel 12.0;HDR=NO;IMEX=1'");
            TFileDescr document = new TFileDescr
            {
                title = "LX OVK " + LXname + " Activation delays"
            };
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                OleDbCommand cmd = new OleDbCommand
                {
                    Connection = conn
                };
                var dtExcelsheetname = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                //string InputDocSheet = "";
                //foreach (DataRow row in dtExcelsheetname.Rows)
                //{
                //    if (row["TABLE_NAME"].ToString().Contains("mrk-"))
                //    {
                //        InputDocSheet = row["TABLE_NAME"].ToString();
                //        break;
                //    }
                //}
                //cmd.CommandText = "SELECT * FROM [" + InputDocSheet + "]";
                //DataTable dtInput = new DataTable();
                //OleDbDataAdapter daInput = new OleDbDataAdapter(cmd);
                //daInput.Fill(dtInput);
                //var sl = dtInput.Rows[0][1].ToString()
                //            .Split(new string[]{ "Signalling_layout_",
                //                                 "Signalling layout ",
                //                                 "signalling_layout_",
                //                                 "signalling layout ",
                //                                 "Signalling_Layout_",
                //                                 "Signalling Layout ",
                //                                 " ", "_", "Ed", "ED", "ed" }, StringSplitOptions.RemoveEmptyEntries);
                //if (sl.Length >= 4)
                //{
                //    document.title += "SL v/" + sl[3] + ", " + sl[1] + "";
                //}
                //var ssp = dtInput.Rows[1][1].ToString()
                //            .Split(new string[] { "Speed_profile_", 
                //                                  "Speed profile ",
                //                                  "speed_profile_",
                //                                  "speed profile ",
                //                                  "Speed_Profile_",
                //                                  "Speed Profile ",
                //                                  " ", "_", "Ed", "ED", "ed" }, StringSplitOptions.RemoveEmptyEntries);
                //if (ssp.Length >= 4)
                //{
                //    document.title += "; SSP v/" + ssp[3] + ", " + ssp[1] + "";
                //}
                //document.title += ")";
                string TableName = "";
                Regex name = new Regex("^.*Front.*[P-p]{1}age.*[$][']{0,1}$");
                foreach (DataRow row in dtExcelsheetname.Rows)
                {
                    if (name.IsMatch(row["TABLE_NAME"].ToString()))
                    {
                        TableName = row["TABLE_NAME"].ToString();
                        break;
                    }
                }
                cmd.CommandText = "SELECT * FROM [" + TableName + "]";
                DataTable dt = new DataTable();
                OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                da.Fill(dt);
                ds.Tables.Add(dt);

                for (int a = 0; a < dt.Rows.Count; a++)
                {
                    for (int i = 0; i < dt.Columns.Count - 1; i++)
                    {
                        if (dt.Rows[a][i] != DBNull.Value && ((string)dt.Rows[a][i]).Contains("Document Number"))
                        {
                            document.docID = dt.Rows[a][i + 3].ToString();
                        }
                        if (dt.Rows[a][i] != DBNull.Value && ((string)dt.Rows[a][i]).Contains("Version"))
                        {
                            document.version = dt.Rows[a][i + 3].ToString();
                        }
                        if (dt.Rows[a][i] != DBNull.Value && ((string)dt.Rows[a][i]).Contains("Prepared by"))
                        {
                            document.creator = dt.Rows[a + 1][i].ToString();
                            try
                            {
                                document.date =
                                ExpPt1.Calc.StringToDate(dt.Rows[a + 2][i].ToString(), out DateTime date, out bool flag);
                                //document.date = Convert.ToDateTime(dt.Rows[a + 2][i].ToString());
                            }
                            catch
                            {

                            }

                        }
                    }
                }
            }
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                OleDbCommand cmd = new OleDbCommand
                {
                    Connection = conn
                };
                cmd.CommandText = "SELECT * FROM [Activation sections$]";
                DataTable dt = new DataTable();
                //dt.TableName = sheetName;

                OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                da.Fill(dt);
                ds.Tables.Add(dt);

                List<LXactSection> activationSections =
                    new List<LXactSection>();
                Regex tdtRegex = new Regex(ExpPt1.Constants.tdtNameRegexp);
                XlsLxActivation activation = new XlsLxActivation
                {
                    Document = document,
                    Remarks = new List<string>()
                };
                int ColumnNumber = 0;
                for (int a = 0; a < dt.Rows.Count; a++)
                {
                    bool exit = false;
                    for (int i = 0; i < dt.Columns.Count - 1; i++)
                    {
                        if (dt.Rows[a][i] != DBNull.Value && ((string)dt.Rows[a][i]).Contains("mrk"))
                        {
                            ColumnNumber = i;
                            exit = true;
                            break;
                        }
                    }
                    if (exit)
                    {
                        break;
                    }
                }

                for (int a = 0; a < dt.Rows.Count; a++)
                {
                    LXactSection section =
                                new LXactSection();
                    int j = 1;
                    string actName = "";
                    while (((a + j) <= (dt.Rows.Count - 1)) &&
                        !(dt.Rows[a + j][ColumnNumber] != DBNull.Value && ((string)dt.Rows[a + j][ColumnNumber]).Contains("mrk")))
                    {
                        if (dt.Rows[a + j][ColumnNumber].ToString() != "" &&
                            ((string)dt.Rows[a + j][ColumnNumber]).Contains("ActivationDelayTime"))
                        {
                            actName = dt.Rows[a + j - 1][ColumnNumber].ToString();
                            if (dt.Rows[a + j][ColumnNumber + 1] != null &&
                                dt.Rows[a + j][ColumnNumber + 1].ToString() != "")
                            {
                                section.ActivationDelayTime = Convert.ToDecimal(dt.Rows[a + j][ColumnNumber + 1]);
                                section.ActivationDelayTimeSpecified = true;
                            }
                            else
                            {
                                section.ActivationDelayTime = 0;
                                section.ActivationDelayTimeSpecified = true;
                            }
                            if (dt.Rows[a + j][ColumnNumber + 2] != null &&
                                dt.Rows[a + j][ColumnNumber + 2].ToString() != "")
                            {
                                activation.Remarks.Add(actName + " - " + dt.Rows[a + j][ColumnNumber + 2].ToString().Trim().TrimEnd('.') + ".");
                            }
                        }
                        if (dt.Rows[a + j][ColumnNumber].ToString() != "" &&
                            ((string)dt.Rows[a + j][ColumnNumber]).Contains("ActivationAxleCounterSectionID"))
                        {
                            if (dt.Rows[a + j][ColumnNumber + 1] != null &&
                                dt.Rows[a + j][ColumnNumber + 1].ToString() != "")
                            {
                                if (tdtRegex.IsMatch((string)dt.Rows[a + j][ColumnNumber + 1]))
                                {
                                    section.ActivationAxleCounterSectionID = (string)dt.Rows[a + j][ColumnNumber + 1];
                                }
                                else
                                {
                                    ErrLogger.Error("Wrong ActivationAxleCounterSectionID", LXname, actName + " " +
                                        (string)dt.Rows[a + j][ColumnNumber + 1]);
                                    ErrLogger.ErrorsFound = true;
                                }
                            }
                        }
                        Regex routeChain = new Regex("[R-r]oute.*[C-c]hain");
                        if (dt.Rows[a + j][ColumnNumber].ToString() != "" &&
                            routeChain.IsMatch((string)dt.Rows[a + j][ColumnNumber]))
                        {
                            int routeChainCounter = 1;
                            List<string> chains = new List<string>
                            {
                                dt.Rows[a + j][ColumnNumber + 1].ToString()
                            };
                            while (dt.Rows[a + j + routeChainCounter][ColumnNumber].ToString() == "" &&
                                dt.Rows[a + j + routeChainCounter][ColumnNumber + 1].ToString() != "")
                            {
                                chains.Add(dt.Rows[a + j + routeChainCounter][ColumnNumber + 1].ToString());
                                routeChainCounter++;
                            }
                            section.RouteChain = new LevelCrossingsLevelCrossingLevelCrossingTracksLevelCrossingTrackActivationSectionsActivationSectionRouteChain
                            {
                                RouteID = chains.ToArray()
                            };
                            j += routeChainCounter - 1;
                        }
                        if (dt.Rows[a + j][ColumnNumber].ToString() != "" &&
                            ((string)dt.Rows[a + j][ColumnNumber]).Contains("LxAxleCounterSectionID"))
                        {
                            if (tdtRegex.IsMatch((string)dt.Rows[a + j][ColumnNumber + 1]))
                            {
                                section.LxAxleCounterSectionID = (string)dt.Rows[a + j][ColumnNumber + 1];
                            }
                            else
                            {
                                ErrLogger.Error("Wrong LxAxleCounterSectionID", LXname, actName + " " +
                                         (string)dt.Rows[a + j][ColumnNumber + 1]);
                                ErrLogger.ErrorsFound = true;
                            }
                        }
                        j++;
                    }
                    activationSections.Add(section);
                    a += j - 1;
                }
                activation.ActivationSections = activationSections;
                return activation;
            }
        }

        public List<BaliseGroupsBaliseGroup> BaliseGroups(
          string dataSource,
          ref TFileDescr document)
        {
            string connectionString = this.GetConnectionString(dataSource,
                "'Excel 12.0;IMEX=1;HDR=NO;TypeGuessRows=0;ImportMixedTypes=Text'");
            document.title = "Balise Group";
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                OleDbCommand selectCommand = new OleDbCommand()
                {
                    Connection = conn
                };
                selectCommand.CommandText = "SELECT * FROM [FrontPage$]";
                DataTable dt = new DataTable();
                OleDbDataAdapter oleDbDataAdapter = new OleDbDataAdapter(selectCommand);
                oleDbDataAdapter.Fill(dt);
                for (int r = 0; r < dt.Rows.Count; ++r)
                {
                    for (int c = 0; c < dt.Columns.Count - 1; ++c)
                    {
                        if (dt.Rows[r][c] != DBNull.Value && (((string)dt.Rows[r][c]).Contains("Document Number") || ((string)dt.Rows[r][c]).Contains("Tegningsnr")))
                            document.docID = dt.Rows[r][c + 3].ToString();
                        if (dt.Rows[r][c] != DBNull.Value && ((string)dt.Rows[r][c]).Contains("Version"))
                            document.version = dt.Rows[r][c + 3].ToString();
                        if (dt.Rows[r][c] != DBNull.Value && ((string)dt.Rows[r][c]).Contains("Prepared by"))
                        {
                            document.creator = dt.Rows[r + 1][c].ToString();
                            document.date =
                            ExpPt1.Calc.StringToDate(dt.Rows[r + 2][c].ToString(), out DateTime date, out bool flag);
                            //document.date = Convert.ToDateTime(dt.Rows[r + 2][c].ToString());
                        }
                    }
                }
                DataTable dtSheet = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                foreach (DataRow dr in dtSheet.Rows)
                {
                    if (dr["TABLE_NAME"].ToString().Trim('\'').EndsWith("$") && dr["TABLE_NAME"].ToString().Contains("BG"))
                    {
                        selectCommand.CommandText = "SELECT * FROM [" + dr["TABLE_NAME"].ToString() + "]";
                        break;
                    }
                }
                oleDbDataAdapter.Fill(dt);
                int num1 = 1;
                dt.Columns.Add("RowNum", typeof(int));
                foreach (DataRow row in (InternalDataCollectionBase)dt.Rows)
                {
                    row["RowNum"] = (object)num1++;
                    for (int c = 0; c < dt.Columns.Count; ++c)
                    {
                        if (row[c].ToString().ToLower().Contains("designation") && !dt.Columns.Contains("Designation"))
                            dt.Columns[c].ColumnName = "Designation";
                        if (row[c].ToString().ToLower().Contains("status") && !dt.Columns.Contains("Status"))
                            dt.Columns[c].ColumnName = "Status";
                        if (row[c].ToString().ToLower().Contains("orientation") && !dt.Columns.Contains("Orientation"))
                            dt.Columns[c].ColumnName = "Orientation";
                        if (row[c].ToString().ToLower().Contains("direction") && !dt.Columns.Contains("Direction"))
                            dt.Columns[c].ColumnName = "Direction";
                        if (row[c].ToString().ToLower().Contains("balise groupe types") && !dt.Columns.Contains("Balise Groupe Types"))
                            dt.Columns[c].ColumnName = "Balise Groupe Types";
                        if (row[c].ToString().ToLower().Contains("border balise") && !dt.Columns.Contains("Border Balise"))
                            dt.Columns[c].ColumnName = "Border Balise";
                        if (row[c].ToString().ToLower().Contains("inside psa") && !dt.Columns.Contains("Inside PSA"))
                            dt.Columns[c].ColumnName = "Inside PSA";
                        if (row[c].ToString().ToLower().Contains("duplicated") && !dt.Columns.Contains("Duplicated"))
                            dt.Columns[c].ColumnName = "Duplicated";
                        if (row[c].ToString().ToLower().Contains("track segment") && !dt.Columns.Contains("Track Segment"))
                            dt.Columns[c].ColumnName = "Track Segment";
                        if (row[c].ToString().ToLower().Contains("line id") && !dt.Columns.Contains("Line ID"))
                            dt.Columns[c].ColumnName = "Line ID";
                        if (row[c].ToString().ToLower().Contains("location") && !dt.Columns.Contains("Location"))
                            dt.Columns[c].ColumnName = "Location";
                        if (row[c].ToString().ToLower().Contains("remarks") && !dt.Columns.Contains("Remarks"))
                            dt.Columns[c].ColumnName = "Remarks";
                    }
                }
                DataRow[] BGs =
                    dt.Select("([Balise Groupe Types] is not NULL AND[Balise Groupe Types] <> 'Balise Groupe Types')", "RowNum");

                List<BaliseGroupsBaliseGroup> baliseGroups = new List<BaliseGroupsBaliseGroup>();
                for (int b = 0; b < BGs.Count(); b++)
                {
                    List<BaliseGroupsBaliseGroupBaliseGroupTypesKindOfBG> kindOfBGs =
                        new List<BaliseGroupsBaliseGroupBaliseGroupTypesKindOfBG>();
                    if (!Enum.TryParse(BGs[b]["Balise Groupe Types"].ToString().Replace(" ", string.Empty), out KindOfBG kindOfBG))
                    {
                        ErrLogger.Error("Unable to parse KindOfBG from BG table", BGs[b]["Designation"].ToString().ToLower(), 
                            BGs[b]["Balise Groupe Types"].ToString().Replace(" ", string.Empty));
                    }
                    if (!Enum.TryParse(BGs[b]["Direction"].ToString().ToLower(),
                        out NominalReverseBothType nominalReverseBothType))
                    {
                        ErrLogger.Error("Unable parse NominalReverseBothType from BG table", BGs[b]["Designation"].ToString().ToLower(),
                               BGs[b]["Direction"].ToString().ToLower());
                        ErrLogger.ErrorsFound = true;
                    }
                    BaliseGroupsBaliseGroupBaliseGroupTypesKindOfBG KindOfBG =
                        new BaliseGroupsBaliseGroupBaliseGroupTypesKindOfBG
                        {
                            Value = kindOfBG,
                            direction = nominalReverseBothType
                        };
                    if (BGs[b].Table.Columns.Contains("Duplicated"))
                    {
                        if (BGs[b]["Duplicated"].ToString() != "")
                        {
                            KindOfBG.duplicatedSpecified = true;
                            KindOfBG.duplicated = YesNoType.yes;
                        }
                    }
                    if (BGs[b].Table.Columns.Contains("Remarks"))
                    {
                        if (BGs[b]["Remarks"].ToString() != "" && BGs[b]["Remarks"].ToString().ToLower() == "duplicated")
                        {
                            KindOfBG.duplicatedSpecified = true;
                            KindOfBG.duplicated = YesNoType.yes;
                        }
                    }
                    kindOfBGs.Add(KindOfBG);
                    int routeChainCounter = 1;
                    while ((b + routeChainCounter) < BGs.Count() &&
                            BGs[b + routeChainCounter]["Designation"].ToString() == "")
                    {
                        if (!Enum.TryParse(BGs[b + routeChainCounter]["Balise Groupe Types"].ToString()
                                      .Replace(" ", string.Empty), out kindOfBG))
                        {
                            ErrLogger.Error("Unable to parse KindOfBG from BG table", BGs[b]["Designation"].ToString().ToLower(),
                                BGs[b + routeChainCounter]["Balise Groupe Types"].ToString().Replace(" ", string.Empty));
                            ErrLogger.ErrorsFound = true;
                        }
                        if (!Enum.TryParse(BGs[b + routeChainCounter]["Direction"].ToString().ToLower(), out NominalReverseBothType reverseBothType))
                        {
                            ErrLogger.Error("Unable to parse NominalReverseBothType from BG table", BGs[b]["Designation"].ToString().ToLower(),
                                BGs[b + routeChainCounter]["Direction"].ToString().ToLower());
                            ErrLogger.ErrorsFound = true;
                        }
                        KindOfBG = new BaliseGroupsBaliseGroupBaliseGroupTypesKindOfBG
                        {
                            Value = kindOfBG,
                            direction = reverseBothType
                        };
                        if (BGs[b + routeChainCounter].Table.Columns.Contains("Remarks"))
                        {
                            if (BGs[b + routeChainCounter]["Remarks"].ToString() != "" && BGs[b + routeChainCounter]["Remarks"].ToString().ToLower() == "duplicated")
                            {
                                KindOfBG.duplicatedSpecified = true;
                                KindOfBG.duplicated = YesNoType.yes;
                            }
                        }
                        if (BGs[b + routeChainCounter].Table.Columns.Contains("Duplicated"))
                        {
                            if (BGs[b + routeChainCounter]["Duplicated"].ToString() != "")
                            {
                                KindOfBG.duplicatedSpecified = true;
                                KindOfBG.duplicated = YesNoType.yes;
                            }
                        }
                        kindOfBGs.Add(KindOfBG);
                        routeChainCounter++;
                    }
                    BaliseGroupsBaliseGroup BG = new BaliseGroupsBaliseGroup
                    {
                        Designation = BGs[b]["Designation"].ToString().ToLower(),
                        TrackSegmentID = BGs[b]["Track Segment"].ToString().ToLower(),
                        LineID = BGs[b]["Line ID"].ToString().ToLower(),//Line ID
                        BaliseGroupTypes = new BaliseGroupsBaliseGroupBaliseGroupTypes
                        {
                            KindOfBG = kindOfBGs.ToArray()
                        },
                        Orientation = (UpDownSingleType)Enum.Parse(typeof(UpDownSingleType),
                                   BGs[b]["Orientation"].ToString().ToLower()),
                    };
                    if (BGs[b].Table.Columns.Contains("Border Balise"))
                    {
                        if (BGs[b]["Border Balise"].ToString() != "")
                        {
                            BG.BorderBaliseSpecified = true;
                            BG.BorderBalise = YesNoType.yes;
                        }
                    }
                    baliseGroups.Add(BG);
                    b += routeChainCounter - 1;
                }
                return baliseGroups;
            }
        }
    }
    public class DetLock
    {
        public string Pt { get; set; }

        public List<Adjacent> Adjacents { get; set; }

        public class Adjacent
        {
            public List<string> Tdts { get; set; }

            public List<string> Pts { get; set; }
        }
    }

    public class EmergStopGroup
    {
        public string Designation { get; set; }

        public string EmergSg { get; set; }
    }

    public class FlankProtection
    {
        public string Pt { get; set; }

        public YesNoType Left { get; set; }

        public YesNoType Right { get; set; }

        public string LeftFrom { get; set; }

        public string RightFrom { get; set; }

        public string LeftTdt { get; set; }

        public string RightTdt { get; set; }
    }

    public class Signal
    {
        public string Mb { get; set; }

        public string Ac { get; set; }

        public string Distance { get; set; }

        public Decimal OCes { get; set; }
    }

    public class SpeedProfile
    {
        public string Designation { get; set; }

        public List<SpeedProfilesTrackSegment> TrackSegments { get; set; }

        public KindOfCantDeficiancy? CD { get; set; }

        public KindOfTrainCategory? TrainCat { get; set; }

        public string AxLoad { get; set; }
    }

    public class SpeedProfilesTrackSegment
    {
        public string TrckSg { get; set; }

        public string Okm1 { get; set; }

        public string Okm2 { get; set; }

        public UpDownBothType Direction { get; set; }

        public string Remarks { get; set; }

        public string SpeedMax { get; set; }

        public string SpeedLimit { get; set; }
    }

    public class TrckLackOfClearence
    {
        public string TrackSection { get; set; }

        public string Value { get; set; }
    }

    public class XlsCmpRoute
    {
        public string Designation { get; set; }

        public string Start { get; set; }

        public string Dest { get; set; }

        public List<string> Routes { get; set; }
    }

    public class XlsLX
    {
        public int Index { get; set; }

        public string Value { get; set; }

        public string Reference { get; set; }
    }

    public class XlsLxActivation
    {
        public TFileDescr Document { get; set; }

        public List<LevelCrossingsLevelCrossingLevelCrossingTracksLevelCrossingTrackActivationSectionsActivationSection> ActivationSections { get; set; }

        public List<string> Remarks { get; set; }
    }

    public class XlsPoint
    {
        public string Designation { get; set; }

        public LeftRightType ReqPosition { get; set; }
    }

    public class XlsPwsActivation
    {
        public TFileDescr Document { get; set; }

        public List<PWSactSection> ActivationSections { get; set; }

        public string SpeedIfUnprotectedUp { get; set; }

        public string SpeedIfUnprotectedDown { get; set; }

        public string TSRStartInRearOfAreaUp { get; set; }

        public string TSRExtensionBeyondAreaUp { get; set; }

        public string TSRStartInRearOfAreaDown { get; set; }

        public string TSRExtensionBeyondAreaDown { get; set; }
    }

    public class XlsRoute
    {
        public KindOfRouteType Type { get; set; }

        public string Start { get; set; }

        public string Dest { get; set; }

        public YesNoType Default { get; set; }

        public IEnumerable<string> ActCross { get; set; }

        public string SafeDist { get; set; }

        public IEnumerable<XlsPoint> Points { get; set; }

        public IEnumerable<XlsPoint> PointsGrps { get; set; }

        public string SdLast { get; set; }

        public List<string> StartAreas { get; set; }

        public List<string> Overlaps { get; set; }

        public string ExtDest { get; set; }
    }
}


