// File: Appdata.cs
using System.Diagnostics;
using System.IO;

namespace MinimalFirewall
{
    internal static class ConfigPathManager
    {
        private static readonly string _exeDirectory = Path.GetDirectoryName(Environment.ProcessPath)!;
        private static readonly string _appDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MinimalFirewall");
        private static bool _useAppDataStorage = false;

        private static readonly List<string> _managedConfigFiles = new List<string>
        {
            "wildcard_rules.json",
            "foreign_rules_baseline.json",
            "temporary_rules.json",
            "debug_log.txt",
            "changelog.json",
            "uwp_apps.json",
            "trusted_publishers.json"
        };

        public static void Initialize(AppSettings settings)
        {
            _useAppDataStorage = settings.UseAppDataStorage;
            if (_useAppDataStorage)
            {
                try
                {
                    Directory.CreateDirectory(_appDataDirectory);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ERROR] Could not create AppData directory: {ex.Message}");
                    _useAppDataStorage = false; 
                }
            }
        }

        public static string GetConfigPath(string fileName)
        {
            string basePath = _useAppDataStorage ? _appDataDirectory : _exeDirectory;
            return Path.Combine(basePath, fileName);
        }

        public static string GetSettingsPath()
        {
            return Path.Combine(_exeDirectory, "settings.json");
        }

        public static string GetExeDirectory() => _exeDirectory;
        public static string GetAppDataDirectory() => _appDataDirectory;
        public static List<string> GetManagedConfigFileNames() => _managedConfigFiles;
    }
}