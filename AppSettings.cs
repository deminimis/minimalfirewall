using System;
using System.IO;
using System.Text.Json;

namespace MinimalFirewall
{
    public class AppSettings
    {
        private static readonly string _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
        public bool ShowSystemRules { get; set; } = true;
        public bool IsPopupsEnabled { get; set; } = false;
        public bool IsLoggingEnabled { get; set; } = false;
        public string Theme { get; set; } = "Light";
        public bool StartOnSystemStartup { get; set; } = false;
        public bool CloseToTray { get; set; } = true;
        public bool ShowIcons { get; set; } = true;

        public void Save()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(_configPath, json);
            }
            catch (Exception)
            {
                // A failure to save settings should not crash the app
            }
        }

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    string json = File.ReadAllText(_configPath);
                    return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch (Exception)
            {
                // If loading fails, fall back to default settings
            }
            return new AppSettings();
        }
    }
}