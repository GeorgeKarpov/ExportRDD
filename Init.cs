using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Application = System.Windows.Forms.Application;


[assembly: ExtensionApplication(typeof(ExpPt1.Init))]
namespace ExpPt1
{
    class Init : IExtensionApplication
    {
        public void Initialize()
        {
            Commands.AssemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            ErrLogger.Configure(logDirApp: Directory.GetCurrentDirectory() + "\\log", startExplicitly: false, createDirectory: true);
            Commands.DwgPath = AcadApp.DocumentManager.CurrentDocument.Name;
            Commands.DwgDir = Path.GetDirectoryName(Commands.DwgPath);
            Commands.Docs = AcadApp.DocumentManager;
            Commands.AddPalette();
            AcadApp.DocumentManager.DocumentActivated += new DocumentCollectionEventHandler(DocColDocAct);
            AcadApp.DocumentManager.DocumentCreated += new DocumentCollectionEventHandler(DocColDocAct);
            AcadApp.DocumentManager.DocumentDestroyed += DocumentManager_DocumentDestroyed;

            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyHandler);
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
        }

        private void DocumentManager_DocumentDestroyed(object sender, DocumentDestroyedEventArgs e)
        {
            if (((DocumentCollection)sender).Count == 1)
            {
                Commands.Pl.Reset();
            }
        }

        private void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            ErrLogger.Fatal(e.Exception.Message);
        }

        static void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            System.Exception e = (System.Exception)args.ExceptionObject;
            ErrLogger.Fatal(e.Message);
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            ErrLogger.Fatal(e.Exception.Message);
        }

        public void DocSave(object senderObj, DatabaseIOEventArgs docColDocActEvtArgs)
        {
            Commands.DwgPath = ((Database)senderObj).Filename;
            Commands.DwgDir = Path.GetDirectoryName(Commands.DwgPath);
        }

        public void Terminate()
        {
            ErrLogger.Stop();
        }

        public void DocColDocAct(object senderObj, DocumentCollectionEventArgs docColDocActEvtArgs)
        {
            Commands.DwgPath = docColDocActEvtArgs.Document.Database.Filename;
            Commands.DwgDir = Path.GetDirectoryName(Commands.DwgPath);
            if (Commands.Pl != null)
            {
                Commands.Pl.DwgPath = docColDocActEvtArgs.Document.Database.Filename;
                Commands.Pl.Reset();
            }
            docColDocActEvtArgs.Document.Database.SaveComplete += new DatabaseIOEventHandler(DocSave);
        }
    }
}
