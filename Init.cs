using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using System.IO;
using System.Reflection;
using System.Threading;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Application = System.Windows.Forms.Application;
using System;

[assembly: ExtensionApplication(typeof(ExpPt1.Init))]
namespace ExpPt1
{
    class Init : IExtensionApplication
    {
        public void Initialize()
        {
            MyAcadCommands.DwgPath = AcadApp.DocumentManager.CurrentDocument.Name;
            MyAcadCommands.AddPalette();
            AcadApp.DocumentManager.DocumentActivated += new DocumentCollectionEventHandler(DocColDocAct);
            AcadApp.DocumentManager.DocumentCreated += new DocumentCollectionEventHandler(DocColDocAct);

            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyHandler);
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
        }

        private void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            using (StreamWriter streamWriter = 
                new StreamWriter(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + 
                Constants.logFolder + @"\error.log", append: true))
            {
                streamWriter.WriteLine(DateTime.Now.ToString("MM.dd.yyyy HH:mm:ss") + ": " + e.Exception.Message + " "  + e.Exception.StackTrace);
                streamWriter.Close();
            }
        }

        static void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            System.Exception e = (System.Exception)args.ExceptionObject;
            
            using (StreamWriter streamWriter = 
                new StreamWriter(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + 
                Constants.logFolder + @"\error.log", append: true))
            {
                streamWriter.WriteLine(DateTime.Now.ToString("MM.dd.yyyy HH:mm:ss") + " " + e.Message +  " " + e.StackTrace);
                streamWriter.Close();
            }
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            using (StreamWriter streamWriter = 
                new StreamWriter(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + 
                Constants.logFolder + @"\error.log", append: true))
            {
                streamWriter.WriteLine(DateTime.Now.ToString("MM.dd.yyyy HH:mm:ss") + ": " + e.Exception.Message + " " + e.Exception.StackTrace);
                streamWriter.Close();
            }
        }

        public void DocSave(object senderObj, DatabaseIOEventArgs docColDocActEvtArgs)
        {
            MyAcadCommands.DwgPath = ((Database)senderObj).Filename;
        }

        public void Terminate()
        {
            //throw new System.NotImplementedException();
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
