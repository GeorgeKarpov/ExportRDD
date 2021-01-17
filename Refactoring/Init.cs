using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Controls;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Application = System.Windows.Forms.Application;


[assembly: ExtensionApplication(typeof(Refact.Init))]
namespace Refact
{
    class Init : IExtensionApplication
    {
        private static ObjectId toolTipObjectId = ObjectId.Null;
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
            //ComponentManager.ToolTipOpened += ComponentManager_ToolTipOpened; ;
            //docColDocActEvtArgs.Document.Editor.PointMonitor += Editor_PointMonitor; ;
        }

        private void ComponentManager_ToolTipOpened(object sender, EventArgs e)
        {
            if (toolTipObjectId != ObjectId.Null && AcadApp.GetSystemVariable("RollOverTips").ToString() == "1")
            {
                var toolTip = sender as ToolTip;

                // This check is needed to distinguish between the ribbon tooltips and the entity tooltips
                if (toolTip != null)
                {
                    BlockReference member = null;
                    using (Transaction transaction = HostApplicationServices.WorkingDatabase.TransactionManager.StartOpenCloseTransaction())
                    {
                        member = transaction.GetObject(toolTipObjectId, 0) as BlockReference;
                        transaction.Commit();
                    }
                    if (member != null)
                    {
                        var memberToolTip = new Refact.SuperToolTipDisplay
                        {
                            MaxWidth = 600,
                            ClassName = { Text = member.Name }
                        };


                        // Repeat this section for each property of the object to add to the tooltip
                        // I added generic text for this sample but you would instead get properties from the object
                        // and display them here.  The blockName would be the name of the property and the blockValue
                        // would be the value of the property.
                        //{
                        //    var blockName = new TextBlock();
                        //    var blockValue = new TextBlock();

                        //    blockName.Text = "Property Name";
                        //    blockName.Margin = new Thickness(0.0, 5.0, 0.0, 0.0);
                        //    blockName.HorizontalAlignment = HorizontalAlignment.Left;
                        //    blockName.VerticalAlignment = VerticalAlignment.Center;

                        //    blockValue.Text = "Property Value";

                        //    blockValue.Margin = new Thickness(10.0, 5.0, 10.0, 0.0);
                        //    blockValue.TextWrapping = TextWrapping.Wrap;
                        //    blockValue.HorizontalAlignment = HorizontalAlignment.Left;
                        //    blockValue.VerticalAlignment = VerticalAlignment.Center;

                        //    memberToolTip.StackPanelName.Children.Add(blockName);
                        //    memberToolTip.StackPanelValue.Children.Add(blockValue);

                        //    // Because BlockValue textblock can have wrapped text we need to set the height
                        //    // of the BlockName textblock to equal that of the BlockValue textblock.
                        //    // We need to give the wpf layout engine time to calculate the actual height
                        //    // so that we can set the values.
                        //    memberToolTip.StackPanelValue.Dispatcher.BeginInvoke(
                        //        DispatcherPriority.Background,
                        //        new DispatcherOperationCallback(delegate
                        //        {
                        //            blockName.Height = blockValue.ActualHeight;
                        //            return null;
                        //        }), null);
                        //}



                        // Swap out the AutoCAD ToolTip with our own ToolTip
                        toolTip.Content = memberToolTip;
                        //member.Dispose();
                    }
                }

                // Reset the object for the next tooltip
                toolTipObjectId = ObjectId.Null;
            }
          }

        private void Editor_PointMonitor(object sender, Autodesk.AutoCAD.EditorInput.PointMonitorEventArgs e)
        {
            toolTipObjectId = ObjectId.Null;

            if (AcadApp.GetSystemVariable("RollOverTips").ToString() != "1")
                return;

            if ((e.Context.History & PointHistoryBits.FromKeyboard) == PointHistoryBits.FromKeyboard)
                return;

            FullSubentityPath[] paths = e.Context.GetPickedEntities();

            if (paths == null || paths.Length == 0)
                return;

            ObjectId[] ids = paths[0].GetObjectIds();

            if (ids == null || ids.Length == 0)
                return;

            var i = 0;

            if (!ids[i].IsValid)
                return;
            if (ids[i].ObjectClass.Name == "AcDbBlockReference")
            {
                toolTipObjectId = ids[i];
            }       
            ;
        }
    }
}
