using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

[assembly: CommandClass(typeof(ExportV2.MyAcadCommands))]

namespace ExportV2
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

        [CommandMethod("TestDK")]
        public static void OtherTest()
        {
            ExpPt1.Export export = new ExpPt1.Export(DwgPath);
            export.Test();
            export.Dispose();
        }

        [CommandMethod("DynPlatforms")]
        public static void DynPlat()
        {
            ExpPt1.Export export = new ExpPt1.Export(DwgPath);
            export.ReplacePlatforms();
        }
        [CommandMethod("Test")]
        public static void Test()
        {
            using (AcadSL acadSL = new AcadSL(DwgPath))
            {
                acadSL.Test();
            }
        }

        [CommandMethod("AdjustSIGCr")]
        public static void AjustSignals()
        {
            ExpPt1.Export export = new ExpPt1.Export(DwgPath);
            export.AdjustSigForCr();
        }

        //[CommandMethod("InitCompRoutes")]
        //public static void InitCr()
        //{           
        //    exportTmp = new ExpPt1.Export(DwgPath);
        //    var ed = exportTmp.acDoc.Editor;

        //    var promptResult = ed.GetString("\nEnter station Id: ");
        //    if (promptResult.Status != PromptStatus.OK)
        //    {
        //        return;
        //    }
        //    exportTmp.stationID = promptResult.StringResult;
        //    exportTmp.ResetCompoundRoutes();
        //}

        //[CommandMethod("SaveCompRoutes")]
        //public static void SaveCr()
        //{
        //    exportTmp.SaveCompoundRoutes();
        //}

        //[CommandMethod("CreateCompRoutes", CommandFlags.UsePickSet)]
        //public static void CreateCr()
        //{
        //    exportTmp.CreateCompoundRoutes();
        //}

        //[CommandMethod("ResetCompRoutes")]
        //public static void ResetCr()
        //{
        //    exportTmp.ResetCompoundRoutes();
        //}

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

        //[CommandMethod("CheckCrRtIds")]
        //public static void CheckCr()
        //{
        //    exportTmp = new ExpPt1.Export(DwgPath);
        //    exportTmp.CheckCr();
        //}

        //[CommandMethod("TestCrs")]
        //public static void TestCrs()
        //{
        //    exportTmp = new ExpPt1.Export(DwgPath);
        //    exportTmp.TestCr();
        //}



        //[CommandMethod("ExportBlocks")]
        //public static void ExportBlocks()
        //{
        //    exportTmp = new ExpPt1.Export(DwgPath);
        //    exportTmp.ExportBlocks();
        //    exportTmp = null;
        //}
    }
}
