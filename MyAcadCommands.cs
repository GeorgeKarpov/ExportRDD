using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using System.IO;

[assembly: CommandClass(typeof(ExpPt1.MyAcadCommands))]

namespace ExpPt1
{
    public static class MyAcadCommands
    {
        public static string DwgPath { get; set; }

        public static DocumentCollection Docs { get; set; }
        public static Palette Pl { get; set; }

        [CommandMethod("RDDERRORLOG")]
        public static void Test()
        {
            System.Windows.Forms.Form frmErrors = new System.Windows.Forms.Form
            {
                Text = "RDD errors List"
            };
            ErrCntrl err = new ErrCntrl();
            err.BtnLoad.Visible = false;
            err.ListView.Dock = System.Windows.Forms.DockStyle.Fill;
            err.Dock = System.Windows.Forms.DockStyle.Fill;
            ErrLogger.Configure(logDirTmp: Path.GetDirectoryName(DwgPath));
            err.LoadList();
            frmErrors.Controls.Add(err);
            Application.ShowModelessDialog(null, frmErrors, true);
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
