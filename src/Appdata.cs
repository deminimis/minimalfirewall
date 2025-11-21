using System.Diagnostics;
using System.IO;

namespace MinimalFirewall
{
    internal static class ConfigPathManager
    {
        private static readonly string _exeDirectory = Path.GetDirectoryName(Environment.ProcessPath)!;
        private static readonly string _standardAppDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MinimalFirewall");

        // Logic to look for settigns files
        private static readonly string _activeRootDirectory = DetermineActivePath();

        private static string DetermineActivePath()
        {
            string exeSettings = Path.Combine(_exeDirectory, "settings.json");
            string appDataSettings = Path.Combine(_standardAppDataDirectory, "settings.json");

            if (File.Exists(exeSettings)) return _exeDirectory;
            if (File.Exists(appDataSettings)) return _standardAppDataDirectory;

            return _exeDirectory;
        }

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

        public static void EnsureStorageDirectoryExists()
        {
            try
            {
                Directory.CreateDirectory(_activeRootDirectory);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Could not create storage directory: {ex.Message}");
            }
        }

        public static string GetConfigPath(string fileName)
        {
            return Path.Combine(_activeRootDirectory, fileName);
        }

        public static string GetSettingsPath()
        {
            return Path.Combine(_activeRootDirectory, "settings.json");
        }

        public static string GetExeDirectory() => _exeDirectory;
        public static string GetStandardAppDataDirectory() => _standardAppDataDirectory;
        public static bool IsPortableMode() => string.Equals(_activeRootDirectory, _exeDirectory, StringComparison.OrdinalIgnoreCase);
        public static List<string> GetManagedConfigFileNames() => _managedConfigFiles;
    }
}