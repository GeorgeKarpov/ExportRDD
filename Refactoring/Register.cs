using Autodesk.AutoCAD.DatabaseServices;
using System.Reflection;
using Win = Microsoft.Win32;

//[assembly: CommandClass(typeof(ExpPt1.Register))]

namespace Refact
{
    /// <summary>
    /// Register .Net app in Win registry.
    /// </summary>
    class Register
    {

        public void RegisterMyApp()
        {
            // Get the AutoCAD Applications key
            string sProdKey = HostApplicationServices.Current.MachineRegistryProductRootKey;
            //string sAppName = "ExpPt1";
            string sAppName = Assembly.GetExecutingAssembly().GetName().Name.ToString();

            Win.RegistryKey regAcadProdKey = Win.Registry.CurrentUser.OpenSubKey(sProdKey);
            Win.RegistryKey regAcadAppKey = regAcadProdKey.OpenSubKey("Applications", true);

            // Check to see if the sAppName key exists
            string[] subKeys = regAcadAppKey.GetSubKeyNames();
            foreach (string subKey in subKeys)
            {
                // If the application is already registered, exit
                if (subKey.Equals(sAppName))
                {
                    regAcadAppKey.Close();
                    return;
                }
            }

            // Get the location of this module
            string sAssemblyPath = Assembly.GetExecutingAssembly().Location;

            // Register the application
            Win.RegistryKey regAppAddInKey = regAcadAppKey.CreateSubKey(sAppName);
            regAppAddInKey.SetValue("DESCRIPTION", sAppName, Win.RegistryValueKind.String);
            regAppAddInKey.SetValue("LOADCTRLS", 14, Win.RegistryValueKind.DWord);
            regAppAddInKey.SetValue("LOADER", sAssemblyPath, Win.RegistryValueKind.String);
            regAppAddInKey.SetValue("MANAGED", 1, Win.RegistryValueKind.DWord);

            regAcadAppKey.Close();
        }


        public void UnregisterMyApp()
        {
            // Get the AutoCAD Applications key
            string sProdKey = HostApplicationServices.Current.MachineRegistryProductRootKey;
            //string sAppName = "ExpPt1";
            string sAppName = Assembly.GetExecutingAssembly().GetName().Name.ToString();

            Win.RegistryKey regAcadProdKey = Win.Registry.CurrentUser.OpenSubKey(sProdKey);
            Win.RegistryKey regAcadAppKey = regAcadProdKey.OpenSubKey("Applications", true);

            // Delete the key for the application
            regAcadAppKey.DeleteSubKeyTree(sAppName);
            regAcadAppKey.Close();
        }
    }
}
