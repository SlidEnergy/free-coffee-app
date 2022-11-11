using Microsoft.Win32;
using System.Windows.Forms;

namespace PointsChecker
{
    internal class StartUpApp
    {
        //Startup registry key and value
        private static readonly string StartupKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        private static readonly string StartupValue = "MyApplicationName";
        
        public void SetStartup()
        {
            try
            {
                //Set the application to run at startup
                RegistryKey key = Registry.CurrentUser.OpenSubKey(StartupKey, true);
                key.SetValue(Application.ProductName, Application.ExecutablePath.ToString());
            }
            catch
            {

            }
        }

        public void RemoveFromStartup()
        {
            try
            {
                //Set the application to run at startup
                RegistryKey key = Registry.CurrentUser.OpenSubKey(StartupKey, true);
                key.DeleteValue(Application.ProductName);
            }
            catch
            {

            }
        }
    }
}
