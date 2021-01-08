using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using System.IO;

[assembly: CommandClass(typeof(ExpPt1.Commands))]

namespace ExpPt1
{
    public static class Commands
    {
        public static string DwgPath { get; set; }

        public static string DwgDir { get; set; }

        public static string AssemblyDir { get; set; }

        public static DocumentCollection Docs { get; set; }
        public static Palette Pl { get; set; }

        [CommandMethod("TESTREFACT")]
        public static void TestRefact()
        {
            Refact.ExpRDD expRDD = new Refact.ExpRDD();
            expRDD.ExportRDD();
        }

        [CommandMethod("RDDERRORLOG")]
        public static void Test()
        {
            Refact.Utils.ShowErrList(Path.GetDirectoryName(DwgPath),
               "RDD errors List", "Following errors were found.",
               System.Drawing.SystemIcons.Information, false);
        }

        [CommandMethod("ExportRDD")]
        public static void ExportRdd()
        {
            Export export = new Export(DwgPath);
            export.ExportRdd();
            export.Dispose();
        }

        [CommandMethod("RDDPALETTE")]
        public static void AddPalette()
        {
            if (Pl == null)
            {
                Pl = new Palette(DwgPath, Docs);
            }
            else
            {
                Pl.Reload();
            }
        }

        [CommandMethod("RDDEXPROUTES")]
        public static void ExportRoutes()
        {
            Display display = new Display(DwgPath);
            display.ExportRoutes();
            display.Dispose();
        }

        [CommandMethod("RDDTSEGSSP")]
        public static void ExportTSegs()
        {
            Display display = new Display(DwgPath);
            display.ExportTsegs();
            display.Dispose();
        }

        [CommandMethod("RDDTDLPTS")]
        public static void ExportPoints()
        {
            Display display = new Display(DwgPath);
            display.ExportPoints();
            display.Dispose();
        }

        //[CommandMethod("ExportBlocks")]
        public static void ExportBlocks()
        {
            Export export = new Export(DwgPath);
            export.ExportBlocks();
            export.Dispose();
        }

        //[CommandMethod("CopyAttributes")]
        public static void CopyAtt()
        {
            Export export = new Export(DwgPath);
            export.CopyAtributtesOnDrw();
            export.Dispose();
        }

        [CommandMethod("CheckIntersSections")]
        public static void TestInterSection()
        {
            Export export = new Export(DwgPath);
            export.TestIntersection();
            export.Dispose();
        }

        [CommandMethod("DynPlatforms")]
        public static void DynPlat()
        {
            Export export = new Export(DwgPath);
            export.ReplacePlatforms();
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
