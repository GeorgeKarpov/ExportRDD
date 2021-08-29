using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using System.IO;

[assembly: CommandClass(typeof(ExpRddApp.Commands))]

namespace ExpRddApp
{
    public static class Commands
    {
        public static string DwgPath { get; set; }

        public static string DwgDir { get; set; }

        public static string AssemblyDir { get; set; }

        public static DocumentCollection Docs { get; set; }
        public static Palette Pl { get; set; }

        [CommandMethod("RDD_EXPORT")]
        public static void ExportRDD()
        {
            ExpRDD expRDD = new ExpRDD();
            expRDD.ExportRDD();
        }

        [CommandMethod("RDD_ERRORLOG")]
        public static void ErrorLogs()
        {
            Utils.ShowErrList(Path.GetDirectoryName(DwgPath),
               "RDD errors List", "Following errors were found.",
               System.Drawing.SystemIcons.Information, false);
        }

        [CommandMethod("RDD_PALETTE")]
        public static void AddPalette()
        {
            if (Pl == null)
            {
                Pl = new Palette(DwgPath, DwgDir, AssemblyDir, Docs);
            }
            else
            {
                Pl.Reload();
            }
        }

        [CommandMethod("RDD_EXPROUTES")]
        public static void ExportRoutes()
        {
            ExpRDD expRDD = new ExpRDD();
            expRDD.ExportRoutes();
            //Display display = new Display(DwgPath);
            //display.ExportRoutes();
            //display.Dispose();
        }

        [CommandMethod("RDD_EXPTSEGSSP")]
        public static void ExportTSegs()
        {
            ExpRDD expRDD = new ExpRDD();
            expRDD.ExportSspTsegs();
            //    Display display = new Display(DwgPath);
            //    display.ExportTsegs();
            //    display.Dispose();
        }

        [CommandMethod("RDD_EXPTDLPTS")]
        public static void ExportPoints()
        {
            ExpRDD expRDD = new ExpRDD();
            expRDD.ExportTdls();
            //Display display = new Display(DwgPath);
            //display.ExportPoints();
            //display.Dispose();
        }

        //[CommandMethod("ExportBlocks")]
        //public static void ExportBlocks()
        //{
        //    //Export export = new Export(DwgPath);
        //    //export.ExportBlocks();
        //    //export.Dispose();
        //}

        ////[CommandMethod("CopyAttributes")]
        //public static void CopyAtt()
        //{
        //    //Export export = new Export(DwgPath);
        //    //export.CopyAtributtesOnDrw();
        //    //export.Dispose();
        //}

        //[CommandMethod("CheckIntersSections")]
        //public static void TestInterSection()
        //{
        //    //Export export = new Export(DwgPath);
        //    //export.TestIntersection();
        //    //export.Dispose();
        //}

        [CommandMethod("RDD_PLATFORMS")]
        public static void DynPlat()
        {
            AcLayout acLayout = new AcLayout(DwgPath, DwgDir, AssemblyDir);
            acLayout.ReplacePlatforms();
        }

        [CommandMethod("RegExpPt1")]
        public static void RegApp()
        {
            Register register = new Register();
            register.RegisterMyApp();
        }

        [CommandMethod("UnregExpPt1")]
        public static void UnregApp()
        {
            Register register = new Register();
            register.UnregisterMyApp();
        }
    }
}
