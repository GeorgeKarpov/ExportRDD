using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using System.IO;
using System.Reflection;
using System.Threading;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

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
            docColDocActEvtArgs.Document.Database.SaveComplete += new DatabaseIOEventHandler(DocSave);
        }
    }
}
