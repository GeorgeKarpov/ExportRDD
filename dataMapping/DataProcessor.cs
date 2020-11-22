using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpPt1.dataMapping
{
    public static class DataProcessor
    {
        static public Dictionary<string, bool> checkData;
        static public Dictionary<string, string> loadFiles;
        static public List<string> lxList;
        static public List<string> pwsList;
        static public List<TFileDescr> docsDescrs = new List<TFileDescr>();

        static ExcelLib.ReadExcel readExcel = new ExcelLib.ReadExcel();
        static ExcelLib.ReadWord readWord = new ExcelLib.ReadWord();

        static List<ExcelLib.Route> routes;
        static List<ExcelLib.Tdl> tdls;
        static List<ExcelLib.Ssp> ssps;
        static List<ExcelLib.Fp> fps;
        static List<ExcelLib.Bg> bgs;

        static Dictionary<string ,ExcelLib.Lx> lxes;
        static Dictionary<string, ExcelLib.Pws> pwses;


        public static bool LoadData(string dgwDir)
        {
            docsDescrs = new List<TFileDescr>();
            if (lxList != null)
            {
                lxes = readExcel.GetLxs(lxList, loadFiles["lblxlsLxs"], dgwDir, checkData["checkBoxLX"]);
            }
            if (pwsList != null)
            {
                pwses = readExcel.GetPwSs(pwsList, dgwDir);
            }

            GetCoversFileDescrs(dgwDir);
            if (!checkData["checkBoxRts"])
            {
                ErrLogger.Information("Routes data skipped", "Routes");
            }
            else
            {
                routes = readExcel.GetRoutes(loadFiles["lblxlsRoutes"]);
            }

            if (!checkData["checkBoxDL"])
            {
                ErrLogger.Information("Tdls data skipped", "Tdl");
            }
            else
            {
                tdls = readExcel.GetTdls(loadFiles["lblxlsDetLock"]);
            }

            if (!checkData["checkBoxSpProf"])
            {
                ErrLogger.Information("Ssp data skipped", "Ssp");
            }
            else
            {
                ssps = readExcel.GetSsps(loadFiles["lblxlsSpProf"]);
            }

            if (!checkData["checkBoxFP"])
            {
                ErrLogger.Information("Fp data skipped", "Fp");
            }
            else
            {
                fps = readExcel.GetFps(loadFiles["lblxlsFP"]);
            }


            if (!checkData["checkBoxBG"])
            {
                ErrLogger.Information("Bg data skipped", "Bg");
            }
            else
            {
                bgs = readExcel.GetBgs(loadFiles["lblxlsBgs"]);
            }
            docsDescrs.AddRange(readExcel.FileDescrs);
            //loadFiles["lblxlsRoutes"]
            return true;
        }

        private static void GetCoversFileDescrs(string dgwDir)
        {
            foreach (var file in Directory.GetFiles(dgwDir, "*.docx"))
            {
                readWord.CoverPage(file, out TFileDescr fileDescr);
                docsDescrs.Add(fileDescr);
            }
        }
    }
}
