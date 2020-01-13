using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AcadDbServ = Autodesk.AutoCAD.DatabaseServices;
using LXactSection = LevelCrossingsLevelCrossingLevelCrossingTracksLevelCrossingTrackActivationSectionsActivationSection;

namespace ExportV2
{
    class InputData
    {
        public Dictionary<string, string> LinesDefinitions { get; set; }
        public Dictionary<string, string> StationsDefinitions { get; set; }
        public Dictionary<string, string> BlocksToGet { get; set; }
        public Dictionary<string, List<string>> ColMap { get; set; }
        public Dictionary<string, LxActivation> LxActivations { get; set; }
        public LXParameters LxParams { get; set; }
        public BGs Bgs { get; set; }
        private Dictionary<string, string> loadFiles;
        private List<LX> lXes;
        public bool InitError { get; set; }
        private string assemblyPath;
        private string dwgPath;
        private List<Block> blockReferences;
        private bool xlsError;
        private string stId;

        public InputData(string AssemblyPath, string dwgPath, List<Block> blocks)
        {
            xlsError = false;
            assemblyPath = AssemblyPath;
            this.dwgPath = dwgPath;
            blockReferences = blocks;
            loadFiles = new Dictionary<string, string>();
            InitError = !Init();
        }

        public void PassStId(string stId)
        {
            this.stId = stId;
        }

        private bool Init()
        {
            bool error = false;
            error = !ReadLinesDefinitions();
            error = !ReadStationsDefinitions();
            error = !ReadBlocksDefinitions();
            error = !ReadColMaps();
            return !error;
        }
        private bool ReadLinesDefinitions()
        {
            if(!File.Exists(assemblyPath + @"\LinesDef.dat"))
            {
                ErrLogger.Log("File not found: '" + assemblyPath + @"\LinesDef.dat" + "'");
                return false;
            }
            LinesDefinitions = new Dictionary<string, string>();
            foreach (string line in File.ReadAllLines(assemblyPath + @"\LinesDef.dat")
                                        .Where(arg => !string.IsNullOrWhiteSpace(arg)))
            {
                if (!LinesDefinitions.ContainsKey(line.Split('\t')[0]))
                {
                    this.LinesDefinitions.Add(line.Split('\t')[0], line);
                }
            }
            return true;
        }

        private bool ReadStationsDefinitions()
        {
            if (!File.Exists(assemblyPath + @"\Stations.dat"))
            {
                ErrLogger.Log("File not found: '" + assemblyPath + @"\Stations.dat" + "'");
                return false;
            }
            StationsDefinitions = new Dictionary<string, string>();
            foreach (string line in File.ReadAllLines(assemblyPath + @"\Stations.dat")
                                        .Where(arg => !string.IsNullOrWhiteSpace(arg)))
            {
                if (!StationsDefinitions.ContainsKey(line.Split('\t')[0]))
                {
                    StationsDefinitions.Add(line.Split('\t')[0], line);
                }
            }
            return true;
        }

        private bool ReadBlocksDefinitions()
        {
            if (!File.Exists(this.assemblyPath + "//BlkMap.dat"))
            {
                ErrLogger.Log("File '" + this.assemblyPath + @"\BlkMap.dat" + "' does not exist");
                return false;
            }
            BlocksToGet = new Dictionary<string, string>();
            foreach (string line in File.ReadAllLines(this.assemblyPath + "//BlkMap.dat")
                                        .Where(arg => !string.IsNullOrWhiteSpace(arg) &&
                                          arg[0] != '#'))
            {
                if (!this.BlocksToGet.ContainsKey(line.Split('\t')[0]))
                {
                    this.BlocksToGet.Add(line.Split('\t')[0], line);
                }
            }
            if (this.BlocksToGet.Count > 0)
            {
                return true;
            }
            else
            {
                ErrLogger.Log("File '" + this.assemblyPath + "//BlkMap.dat" + "' is empty");
                return false;
            }
        }

        private bool ReadColMaps()
        {
            if (!File.Exists(this.assemblyPath + "//ColMap.dat"))
            {
                ErrLogger.Log("File '" + this.assemblyPath + @"\ColMap.dat" + "' does not exist");
                return false;
            }
            ColMap = 
                    File.ReadAllLines(this.assemblyPath + "//ColMap.dat")
                    .Where(arg => !string.IsNullOrWhiteSpace(arg) &&
                                  arg[0] != '#')
                    .ToDictionary(x => x.Split('\t')[0], x => x.Split('\t')[1].Split(',').ToList());
            return true;
        }

        public bool CheckLxPwsSxData()
        {
            bool error = false;
            List<string> tempSig = BlocksToGet
                                   .Where(x => x.Value.Split('\t')[1] == "LevelCrossing")
                                   .Select(s => s.Key)
                                   .ToList();
            lXes = new List<LX>();
            foreach (var block in blockReferences.Where(x => tempSig.Contains(x.Name)))
            {
                LX lx = new LX(block, BlocksToGet[block.Name]);
                string lxFile =
                                Directory.EnumerateFiles(dwgPath, "LX" + lx.Attributes["NAME"].Value + "_*.xls*")
                                .FirstOrDefault();
                if(string.IsNullOrEmpty(lxFile))
                {
                    ErrLogger.Log("Excel document for LX '" + lx.Attributes["NAME"].Value + "' not found");
                    error = true;
                }
                else
                {
                    loadFiles.Add("LX\t" + lx.Attributes["NAME"].Value, lxFile);
                    if (!lx.GetElemDesignation(stId, true))
                    {
                        ErrLogger.Log("Cannot get LX designation of '" + lx.Attributes["NAME"].Value + "'");
                        error = true;
                    }
                    lXes.Add(lx);  
                }
            }

            tempSig = BlocksToGet
                      .Where(x => x.Value.Split('\t')[1] == "StaffPassengerCrossing")
                      .Select(s => s.Key)
                      .ToList();
            foreach (var block in blockReferences.Where(x => tempSig.Contains(x.Name)))
            {
                PWS pws = new PWS(block, BlocksToGet[block.Name]);
                string lxFile =
                                Directory.EnumerateFiles(dwgPath, "PWS" + pws.Attributes["NAME"].Value + "_*.xls*")
                                .FirstOrDefault();
                if (string.IsNullOrEmpty(lxFile))
                {
                    ErrLogger.Log("Excel document for PWS '" + pws.Attributes["NAME"].Value + "' not found");
                    error = true;
                }
                else
                {
                    loadFiles.Add("PWS\t" + pws.Attributes["NAME"].Value, lxFile);
                }
            }

            tempSig = BlocksToGet
                      .Where(x => x.Value.Split('\t')[1] == "StaffCrossing")
                      .Select(s => s.Key)
                      .ToList();
            foreach (var block in blockReferences.Where(x => tempSig.Contains(x.Name)))
            {
                SX sx = new SX(block, BlocksToGet[block.Name]);
                string lxFile =
                                Directory.EnumerateFiles(dwgPath, "SX" + sx.Attributes["NAME"].Value + "_*.xls*")
                                .FirstOrDefault();
                if (string.IsNullOrEmpty(lxFile))
                {
                    ErrLogger.Log("Excel document for SX '" + sx.Attributes["NAME"].Value + "' not found");
                    error = true;
                }
                else
                {
                    loadFiles.Add("SX\t" + sx.Attributes["NAME"].Value, lxFile);
                }
            }
            return !error;
        }

        public bool LoadData(Dictionary<string, string> loadFiles)
        {
            Dictionary<string, string> loadFilesLxs = this.loadFiles
                                                      .Where(x => x.Key.Split('\t').First() == "LX")
                                                      .ToDictionary(x => x.Key, x => x.Value);
            LxActivations = GetLxActivation(loadFilesLxs);
            LxParams = GetLxParams(lXes, loadFiles["lblxlsLxs"]);
            Bgs = GetBaliseGroups(loadFiles["lblxlsBgs"]);
            return !xlsError;
        }

        private string GetConnectionString(string dataSource, string ExtProp)
        {
            Dictionary<string, string> props = new Dictionary<string, string>
            {
                // XLSX - Excel 2007, 2010, 2012, 2013
                ["Provider"] = "Microsoft.ACE.OLEDB.12.0;",
                ["Extended Properties"] = ExtProp,
                ["Data Source"] = dataSource
            };

            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> prop in props)
            {
                sb.Append(prop.Key);
                sb.Append('=');
                sb.Append(prop.Value);
                sb.Append(';');
            }
            return sb.ToString();
        }

        private Dictionary<string, LxActivation> GetLxActivation(Dictionary<string, string> loadFiles)
        {
            DataSet ds = new DataSet();
            string connectionString = null;
            string LXname = null;
            xlsError = false;
            Dictionary<string, LxActivation> lxActs = new Dictionary<string, LxActivation>();

            foreach (var lxLoad in loadFiles)
            {
                connectionString = GetConnectionString(lxLoad.Value, "'Excel 12.0;HDR=NO;IMEX=1'");
                LXname = lxLoad.Key.Split('\t').Last();
                TFileDescr document = new TFileDescr
                {
                    title = "LX OVK " + LXname + " Activation delays"
                };
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                    }
                    catch (OleDbException e)
                    {
                        ErrLogger.Log("LX " + LXname + " Activations: " + e.Message);
                        xlsError = true;
                        return null;
                    }

                    OleDbCommand cmd = new OleDbCommand
                    {
                        Connection = conn
                    };
                    var dtExcelsheetname = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });

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
                    try
                    {
                        da.Fill(dt);
                    }
                    catch (OleDbException e)
                    {
                        ErrLogger.Log("LX " + LXname + " Activations: " + e.Message);
                        xlsError = true;
                        return null;
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
                            if (dt.Rows[a][i] != DBNull.Value && ((string)dt.Rows[a][i]).Contains("Version"))
                            {
                                document.version = dt.Rows[a][i + 3].ToString();
                            }
                            if (dt.Rows[a][i] != DBNull.Value && ((string)dt.Rows[a][i]).Contains("Prepared by"))
                            {
                                document.creator = dt.Rows[a + 1][i].ToString();
                                try
                                {
                                    document.date = Convert.ToDateTime(dt.Rows[a + 2][i].ToString());
                                }
                                catch
                                {
                                    ErrLogger.Log("LX " + LXname + " Activations: Unable to parse document date");
                                    xlsError = true;
                                }

                            }
                        }
                    }

                    cmd.CommandText = "SELECT * FROM [Activation sections$]";
                    dt = new DataTable();

                    da = new OleDbDataAdapter(cmd);
                    try
                    {
                        da.Fill(dt);
                    }
                    catch (OleDbException e)
                    {
                        ErrLogger.Log("LX " + LXname + " Activations: " + e.Message);
                        xlsError = true;
                        return null;
                    }
                    ds.Tables.Add(dt);

                    List<LXactSection> activationSections =
                        new List<LXactSection>();
                    for (int a = 0; a < dt.Rows.Count; a++)
                    {
                        int ColumnNumber = 0;
                        for (int i = 0; i < dt.Columns.Count - 1; i++)
                        {
                            if (dt.Rows[a][i] != DBNull.Value && ((string)dt.Rows[a][i]).Contains("mrk"))
                            {
                                ColumnNumber = i;
                                break;
                            }
                        }
                        LXactSection section =
                                    new LXactSection();
                        int j = 1;
                        while (((a + j) <= (dt.Rows.Count - 1)) &&
                            !((dt.Rows[a + j][ColumnNumber] != DBNull.Value && ((string)dt.Rows[a + j][ColumnNumber]).Contains("mrk"))))
                        {
                            if (dt.Rows[a + j][ColumnNumber].ToString() != "" &&
                                ((string)dt.Rows[a + j][ColumnNumber]).Contains("ActivationDelayTime"))
                            {
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
                            }
                            if (dt.Rows[a + j][ColumnNumber].ToString() != "" &&
                                ((string)dt.Rows[a + j][ColumnNumber]).Contains("ActivationAxleCounterSectionID"))
                            {
                                if (dt.Rows[a + j][ColumnNumber + 1] != null &&
                                    dt.Rows[a + j][ColumnNumber + 1].ToString() != "")
                                {
                                    section.ActivationAxleCounterSectionID = (string)dt.Rows[a + j][ColumnNumber + 1];
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
                                section.LxAxleCounterSectionID = (string)dt.Rows[a + j][ColumnNumber + 1];
                            }
                            j++;
                        }
                        activationSections.Add(section);
                        a += j - 1;
                    }
                    lxActs.Add(LXname, new LxActivation
                    {
                        ActivationSections = activationSections,
                        Document = document
                    });
                }
            }         
            return lxActs;
        }

        private LXParameters GetLxParams(List<LX> lXes, string datasource)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            string connectionString = GetConnectionString(datasource, "'Excel 12.0;HDR=NO;IMEX=1'");
            string LxId = null;
            xlsError = false;
            LXParameters lXParameters = new LXParameters
            {
                Document = new TFileDescr
                {
                    title = "Level Crossing Parameters"
                },
                LxParams = new Dictionary<string, List<LxParam>>()
            };

            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                try
                {
                    conn.Open();
                }
                catch (OleDbException e)
                {
                    ErrLogger.Log("Level Crossing Parameters: " + e.Message);
                    xlsError = true;
                    return null;
                }
                OleDbCommand cmd = new OleDbCommand
                {
                    Connection = conn
                };
                cmd.CommandText = "SELECT * FROM [FrontPage$]";
                OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                try
                {
                    da.Fill(dt);
                }
                catch (OleDbException e)
                {
                    ErrLogger.Log("Level Crossing Parameters: " + e.Message);
                    xlsError = true;
                    return null;
                }
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
                            lXParameters.Document.docID = dt.Rows[a][i + d].ToString();
                        }
                        if (dt.Rows[a][i] != DBNull.Value && ((string)dt.Rows[a][i]).Contains("Version"))
                        {
                            int d = 1;
                            while (dt.Rows[a][i + d].ToString() == "")
                            {
                                d++;
                            }
                            lXParameters.Document.version = dt.Rows[a][i + d].ToString();
                        }
                        if (dt.Rows[a][i] != DBNull.Value && ((string)dt.Rows[a][i]).Contains("Prepared by"))
                        {
                            lXParameters.Document.creator = dt.Rows[a + 1][i].ToString();
                            DateTime docDate = new DateTime();
                            if (!DateTime.TryParse(dt.Rows[a + 2][i].ToString(), out docDate))
                            {
                                if (!DateTime.TryParse(dt.Rows[a + 3][i].ToString(), out docDate))
                                {
                                    ErrLogger.Log("Cannot parse doc date of LX parameters.");
                                }
                            }
                            lXParameters.Document.date = docDate;
                        }
                    }
                }
                cmd.CommandText = "SELECT * FROM [Level Crossing Parameters$]";
                da = new OleDbDataAdapter(cmd);
                dt = new DataTable();
                try
                {
                    da.Fill(dt);
                }
                catch (OleDbException e)
                {
                    ErrLogger.Log("Level Crossing Parameters: " + e.Message);
                    xlsError = true;
                    return null;
                }
                ds.Tables.Add(dt);
                string ValueColumn = "";
                string DecrColumn = "";
                foreach (var lxLoad in lXes)
                {
                    LxId = lxLoad.Designation;
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
                        ErrLogger.Log("LX: " + LxId.ToLower() + " not found in crossings parameters table");
                        xlsError = true;
                        return null;
                    }
                    int index = 1;
                    List<LxParam> query = (from p in dt.AsEnumerable()
                                           where p.Field<object>(DecrColumn) != null
                                           select new LxParam
                                           {
                                               Index = index++,
                                               Value = p.Field<object>(ValueColumn) != null ?
                                               p.Field<string>(ValueColumn)
                                               .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                                               .First() :
                                               null,
                                               Reference = p.Field<string>(DecrColumn).ToLower()
                                           }).ToList();
                    lXParameters.LxParams.Add(LxId, query);
                }
            }
            return lXParameters;
        }

        private BGs GetBaliseGroups(string dataSource)
        {
            xlsError = false;
            string connectionString = GetConnectionString(dataSource, "'Excel 12.0;IMEX=1;HDR=YES;TypeGuessRows=0;ImportMixedTypes=Text'");
            TFileDescr document = new TFileDescr
            {
                title = "Balise Group"
            };
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                try
                {
                    conn.Open();
                }
                catch (OleDbException e)
                {
                    ErrLogger.Log("BG table: " + e.Message);
                    xlsError = true;
                    return null;
                }
                OleDbCommand cmd = new OleDbCommand
                {
                    Connection = conn
                };
                cmd.CommandText = "SELECT * FROM [FrontPage$]";

                DataTable dt = new DataTable();
                OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                try
                {
                    da.Fill(dt);
                }
                catch (OleDbException e)
                {
                    ErrLogger.Log("BG table: " + e.Message);
                    xlsError = true;
                    return null;
                }  

                for (int a = 0; a < dt.Rows.Count; a++)
                {
                    for (int i = 0; i < dt.Columns.Count - 1; i++)
                    {
                        if (dt.Rows[a][i] != DBNull.Value && (((string)dt.Rows[a][i]).Contains("Document Number") ||
                                                             ((string)dt.Rows[a][i]).Contains("Tegningsnr")))
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
                            document.date = Convert.ToDateTime(dt.Rows[a + 2][i].ToString());
                        }
                    }
                }
                DataTable dtSheet = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                foreach (DataRow dr in dtSheet.Rows)
                {
                    if (!dr["TABLE_NAME"].ToString().Trim(new char[] { (char)39 }).EndsWith("$"))
                        continue;
                    if (dr["TABLE_NAME"].ToString().Contains("BG"))
                    {
                        cmd.CommandText = "SELECT * FROM [" + dr["TABLE_NAME"].ToString() + "]";
                        break;
                    }
                }

                try
                {
                    da.Fill(dt);
                }
                catch (OleDbException e)
                {
                    ErrLogger.Log("BG table: " + e.Message);
                    xlsError = true;
                    return null;
                }

                if (!CheckTableCols(dt, "BG"))
                {
                    xlsError = true;
                    return null;
                }

                int row = 1;
                dt.Columns.Add("RowNum", typeof(Int32));
                foreach (DataRow r in dt.Rows)
                {
                    r["RowNum"] = row++;
                }
                DataRow[] BGs;
                try
                {
                    BGs =
                    dt.Select("[Balise Groupe Types] is not NULL AND [Balise Groupe Types] <> ''", "RowNum");
                }
                catch (EvaluateException e)
                {
                    ErrLogger.Log("BG table: " + e.Message);
                    xlsError = true;
                    return null;
                }

                

                List<BaliseGroupsBaliseGroup> baliseGroups = new List<BaliseGroupsBaliseGroup>();
                for (int b = 0; b < BGs.Count(); b++)
                {
                    List<BaliseGroupsBaliseGroupBaliseGroupTypesKindOfBG> kindOfBGs =
                        new List<BaliseGroupsBaliseGroupBaliseGroupTypesKindOfBG>();
                    BaliseGroupsBaliseGroupBaliseGroupTypesKindOfBG KindOfBG =
                        new BaliseGroupsBaliseGroupBaliseGroupTypesKindOfBG
                        {
                            Value = (KindOfBG)Enum.Parse(typeof(KindOfBG),
                                BGs[b]["Balise Groupe Types"].ToString().Replace(" ", string.Empty)),
                            direction = (NominalReverseBothType)Enum.Parse(typeof(NominalReverseBothType),
                                    BGs[b]["Direction"].ToString().ToLower())
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
                        KindOfBG = new BaliseGroupsBaliseGroupBaliseGroupTypesKindOfBG
                        {
                            Value = (KindOfBG)Enum.Parse(typeof(KindOfBG),
                               BGs[b + routeChainCounter]["Balise Groupe Types"].ToString().Replace(" ", string.Empty)),
                            direction = (NominalReverseBothType)Enum.Parse(typeof(NominalReverseBothType),
                                   BGs[b + routeChainCounter]["Direction"].ToString().ToLower()),
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
                return new BGs
                {
                    BaliseGroups = baliseGroups,
                    Document = document
                };
            }
        }

        private bool CheckTableCols(DataTable dt, string name)
        {
            bool error = false;
            if (!ColMap.ContainsKey(name))
            {
                ErrLogger.Log("Columns mapping not found for " + name + "'");
                error = true;
            }
            foreach (var col in ColMap[name])
            {
                if(!dt.Columns.Contains(col))
                {
                    ErrLogger.Log("Column '" + col + "' not found in data table '" + name + "'");
                    error = true;
                }
            }
            return !error;
        }
    }


}
