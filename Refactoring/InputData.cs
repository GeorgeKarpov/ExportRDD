﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Refact
{
    public class InputData
    {
        public bool Error { get; set; }
        public string AssemblyDir { get; set; }
        public string DwgDir { get; set; }

        public string DwgPath { get; set; }

        public InputData(string assemblyDir, string dwgDir, string dwgPath)
        {
            AssemblyDir = assemblyDir;
            DwgDir = dwgDir;
            DwgPath = dwgPath;
        }
        public Dictionary<string, string> LoadIni()
        {
            Dictionary<string, string> loadFiles = new Dictionary<string, string>();
            if (File.Exists(DwgDir + "//" + Path.GetFileNameWithoutExtension(DwgPath) + ".ini"))
            {
                loadFiles = File.ReadAllLines(DwgDir + "//" + Path.GetFileNameWithoutExtension(DwgPath) + ".ini")
                            .Where(arg => !string.IsNullOrWhiteSpace(arg))
                            .ToDictionary(x => x.Split('\t')[0], x => x.Split('\t')[1]);
            }
            return loadFiles;
        }
        public void SaveIni(Dictionary<string, string> loadFiles)
        {
            if (loadFiles != null && loadFiles.Count > 0)
            {
                File.WriteAllLines(DwgDir + "//" + Path.GetFileNameWithoutExtension(DwgPath) + ".ini",
                    loadFiles.Select(x => (x.Key + '\t' + x.Value)).ToList());
            }
        }

        public Dictionary<string, string> GetStations()
        {
            if (!File.Exists(AssemblyDir + Constants.cfgFolder + "//Stations.dat"))
            {
                ErrLogger.Error("Input file not found", AssemblyDir + Constants.cfgFolder + "//Stations.dat", "");
                Error = true;
                return null;
            }
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach (string str in ((IEnumerable<string>)File.ReadAllLines(AssemblyDir + Constants.cfgFolder + "//Stations.dat"))
                                    .Where(arg => !string.IsNullOrWhiteSpace(arg) && arg[0] != '#'))
            {
                if (!dictionary.ContainsKey(str.Split('\t')[0]))
                    dictionary.Add(str.Split('\t')[0], str);
            }
            return dictionary;
        }

        public Dictionary<string, string> GetMnTracks()
        {
            string file = AssemblyDir + Constants.cfgFolder + "//MainTracks.dat";
            if (!File.Exists(file))
            {
                ErrLogger.Error("Input file not found", file, "");
                Error = true;
                return null;
            }
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            int linenumber = 1;
            foreach (string str in ((IEnumerable<string>)File.ReadAllLines(file))
                                    .Where(arg => !string.IsNullOrWhiteSpace(arg) && arg[0] != '#'))
            {
                if (!dictionary.ContainsKey(str.Split('\t')[0]))
                {
                    if (str.Split('\t').Count() < 1)
                    {
                        ErrLogger.Error("Unable to read line.", file, "Line number: " + linenumber);
                        Error = true;
                    }
                    else
                    {
                        dictionary.Add(str.Split('\t')[0], str);
                    }                   
                }
                linenumber++;
            }
            return dictionary;
        }

        public Dictionary<string, string> GetLines()
        {
            string path;
            if (File.Exists(DwgDir + "//LinesDef.dat"))
            {
                path = DwgDir + "//LinesDef.dat";
            }
            else
            {
                if (!File.Exists(AssemblyDir + Constants.cfgFolder + "//LinesDef.dat"))
                {
                    ErrLogger.Error("Input file not found", AssemblyDir + Constants.cfgFolder + "//LinesDef.dat", "");
                    Error = true;
                    return null;
                }
                path = AssemblyDir + Constants.cfgFolder + "//LinesDef.dat";
            }
            Dictionary<string, string> LinesDefinitions = new Dictionary<string, string>();
            foreach (string line in File.ReadAllLines(path)
                                        .Where(arg => !string.IsNullOrWhiteSpace(arg)))
            {
                if (!LinesDefinitions.ContainsKey(line.Split('\t')[0]))
                {
                    LinesDefinitions.Add(line.Split('\t')[0], line);
                }
            }
            return LinesDefinitions;
        }

        public Dictionary<string, string> GetBlocks()
        {
            if (!File.Exists(AssemblyDir + Constants.cfgFolder + "//BlkMap.dat"))
            {
                ErrLogger.Error("Input file not found", AssemblyDir + Constants.cfgFolder + "//BlkMap.dat", "");
                Error = true;
                return null;
            }
            Dictionary<string, string> tmpBlocks = new Dictionary<string, string>();
            foreach (string line in File.ReadAllLines(AssemblyDir + Constants.cfgFolder + "//BlkMap.dat")
                                        .Where(arg => !string.IsNullOrWhiteSpace(arg) &&
                                          arg[0] != '#'))
            {
                if (!tmpBlocks.ContainsKey(line.Split('\t')[0]))
                {
                    tmpBlocks.Add(line.Split('\t')[0], line);
                }
            }
            return tmpBlocks;
        }

        public List<string> GetAuthors()
        {
            if (!File.Exists(AssemblyDir + Constants.cfgFolder + "//Authors.dat"))
            {
                ErrLogger.Error("Input file not found", AssemblyDir + Constants.cfgFolder + "//Authors.dat", "");
                Error = true;
                return null;
            }
            return File.ReadAllLines(AssemblyDir + Constants.cfgFolder + "//Authors.dat")
                                        .Where(arg => !string.IsNullOrWhiteSpace(arg) &&
                                          arg[0] != '#')
                                        .ToList();
        }
    }
}
