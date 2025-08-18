// AppSettings.cs
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;

namespace MinimalFirewall
{
    public enum AutoAllowSignedAppsOption
    {
        Disabled,
        AllowSystemTrusted,
        AllowWhitelisted
    }

    public class AppSettings : INotifyPropertyChanged
    {
        private static readonly string _configPath = Path.Combine(AppContext.BaseDirectory, "settings.json");
        private bool _isPopupsEnabled = false;
        private bool _isLoggingEnabled;
        private string _theme = "Dark";
        private bool _startOnSystemStartup;
        private bool _closeToTray = true;
        private int _autoRefreshIntervalMinutes = 10;
        private bool _isTrafficMonitorEnabled = true;
        private bool _showAppIcons = true;
        private AutoAllowSignedAppsOption _autoAllowSignedApps = AutoAllowSignedAppsOption.Disabled;

        public bool IsPopupsEnabled { get => _isPopupsEnabled; set => SetField(ref _isPopupsEnabled, value); }
        public bool IsLoggingEnabled { get => _isLoggingEnabled; set => SetField(ref _isLoggingEnabled, value); }
        public string Theme { get => _theme; set => SetField(ref _theme, value); }
        public bool StartOnSystemStartup { get => _startOnSystemStartup; set => SetField(ref _startOnSystemStartup, value); }
        public bool CloseToTray { get => _closeToTray; set => SetField(ref _closeToTray, value); }
        public int AutoRefreshIntervalMinutes { get => _autoRefreshIntervalMinutes; set => SetField(ref _autoRefreshIntervalMinutes, value); }
        public bool IsTrafficMonitorEnabled { get => _isTrafficMonitorEnabled; set => SetField(ref _isTrafficMonitorEnabled, value); }
        public bool ShowAppIcons { get => _showAppIcons; set => SetField(ref _showAppIcons, value); }
        public AutoAllowSignedAppsOption AutoAllowSignedApps { get => _autoAllowSignedApps; set => SetField(ref _autoAllowSignedApps, value); }

        public Point WindowLocation { get; set; } = new Point(100, 100);
        public Size WindowSize { get; set; } = new Size(1280, 800);
        public int WindowState { get; set; } = (int)FormWindowState.Maximized;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName!);
            Save();
            return true;
        }

        public void Save()
        {
            try
            {
                string json = JsonSerializer.Serialize(this, AppSettingsJsonContext.Default.AppSettings);
                File.WriteAllText(_configPath, json);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Debug.WriteLine($"[ERROR] Failed to save settings due to a file error: {ex.Message}");
            }
        }

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    string json = File.ReadAllText(_configPath);
                    return JsonSerializer.Deserialize(json, AppSettingsJsonContext.Default.AppSettings) ?? new AppSettings();
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            {
                Debug.WriteLine($"[ERROR] Failed to load settings: {ex.Message}");
            }
            return new AppSettings();
        }
    }

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(AppSettings))]
    internal partial class AppSettingsJsonContext : JsonSerializerContext
    {
    }
}