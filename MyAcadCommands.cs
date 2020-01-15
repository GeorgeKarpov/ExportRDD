using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

[assembly: CommandClass(typeof(ExpPt1.MyAcadCommands))]

namespace ExpPt1
{
    public static class MyAcadCommands 
    {
        public static string DwgPath { get; set; }

        //static ExpPt1.Export exportTmp;
        [CommandMethod("ExportRDD")]
        public static void ExportRdd()
        {
            ExpPt1.Export export = new ExpPt1.Export(DwgPath);
            export.ExportRdd();
            export.Dispose();
        }

        [CommandMethod("CopyAttributes")]
        public static void CopyAtt()
        {
            ExpPt1.Export export = new ExpPt1.Export(DwgPath);
            export.CopyAtributtesOnDrw();
            export.Dispose();
        }

        [CommandMethod("CheckIntersSections")]
        public static void TestInterSection()
        {
            ExpPt1.Export export = new ExpPt1.Export(DwgPath);
            export.TestIntersection();
            export.Dispose();
        }

        [CommandMethod("DynPlatforms")]
        public static void DynPlat()
        {
            ExpPt1.Export export = new ExpPt1.Export(DwgPath);
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
