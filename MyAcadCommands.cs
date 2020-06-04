using Autodesk.AutoCAD.Runtime;

[assembly: CommandClass(typeof(ExpPt1.MyAcadCommands))]

namespace ExpPt1
{
    public static class MyAcadCommands 
    {
        public static string DwgPath { get; set; }
        public static Palette Pl { get; set; }
        //static ExpPt1.Export exportTmp;
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
                Pl = new Palette(DwgPath);
            }
            else
            {
                Pl.Reload();
            }
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

        //[CommandMethod("CheckIntersSections")]
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
