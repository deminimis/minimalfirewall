using Microsoft.Win32;
using System;
using System.Reflection;

namespace MinimalFirewall
{
    public class StartupService
    {
        private const string RegistryKeyPath = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        private readonly string _appName;
        private readonly string _appPath;

        public StartupService()
        {
            _appName = Assembly.GetExecutingAssembly().GetName().Name;
            _appPath = Assembly.GetExecutingAssembly().Location;
        }

        public void SetStartup(bool isEnabled)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true))
                {
                    if (key == null) return;

                    if (isEnabled)
                    {
                        // To ensure the path is correct and enclosed in quotes
                        key.SetValue(_appName, $"\"{_appPath}\"");
                    }
                    else
                    {
                        if (key.GetValue(_appName) != null)
                        {
                            key.DeleteValue(_appName, false);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Errors writing to registry should be handled gracefully
                // Optionally, log the exception
            }
        }
    }
}