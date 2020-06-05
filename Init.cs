using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Serilog;
using Serilog.Events;
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
            Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Fatal)
                        .WriteTo.File(@"log\Fatal.log", rollingInterval: RollingInterval.Day))
                    .CreateLogger();

            MyAcadCommands.DwgPath = AcadApp.DocumentManager.CurrentDocument.Name;
            MyAcadCommands.Docs = AcadApp.DocumentManager;
            MyAcadCommands.AddPalette();
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
                MyAcadCommands.Pl.Reset();
            }
        }

        private void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            Log.Logger.Fatal(e.Exception.Message, e.Exception);
            //using (StreamWriter streamWriter =
            //    new StreamWriter(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
            //    Constants.logFolder + @"\error.log", append: true))
            //{
            //    streamWriter.WriteLine(DateTime.Now.ToString("MM.dd.yyyy HH:mm:ss") + ": " + e.Exception.Message + " " + e.Exception.StackTrace);
            //    streamWriter.Close();
            //}
        }

        static void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            System.Exception e = (System.Exception)args.ExceptionObject;
            Log.Logger.Fatal(e.Message, e);
            //using (StreamWriter streamWriter =
            //    new StreamWriter(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
            //    Constants.logFolder + @"\error.log", append: true))
            //{
            //    streamWriter.WriteLine(DateTime.Now.ToString("MM.dd.yyyy HH:mm:ss") + " " + e.Message + " " + e.StackTrace);
            //    streamWriter.Close();
            //}
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Log.Logger.Fatal(e.Exception.Message, e.Exception);
            //using (StreamWriter streamWriter =
            //    new StreamWriter(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
            //    Constants.logFolder + @"\error.log", append: true))
            //{
            //    streamWriter.WriteLine(DateTime.Now.ToString("MM.dd.yyyy HH:mm:ss") + ": " + e.Exception.Message + " " + e.Exception.StackTrace);
            //    streamWriter.Close();
            //}
        }

        public void DocSave(object senderObj, DatabaseIOEventArgs docColDocActEvtArgs)
        {
            MyAcadCommands.DwgPath = ((Database)senderObj).Filename;
        }

        public void Terminate()
        {
            Log.CloseAndFlush();
        }

        public void DocColDocAct(object senderObj, DocumentCollectionEventArgs docColDocActEvtArgs)
        {
            MyAcadCommands.DwgPath = docColDocActEvtArgs.Document.Database.Filename;
            if (MyAcadCommands.Pl != null)
            {
                MyAcadCommands.Pl.DwgPath = docColDocActEvtArgs.Document.Database.Filename;
                MyAcadCommands.Pl.Reset();
            }
            docColDocActEvtArgs.Document.Database.SaveComplete += new DatabaseIOEventHandler(DocSave);
        }
    }
}
