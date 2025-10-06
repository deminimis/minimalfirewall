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
    public class AppSettings : INotifyPropertyChanged
    {
        private static readonly string _configPath = Path.Combine(AppContext.BaseDirectory, "settings.json");
        private bool _isPopupsEnabled = false;
        private bool _isLoggingEnabled;
        private string _theme = "Dark";
        private bool _startOnSystemStartup;
        private bool _closeToTray = true;
        private int _autoRefreshIntervalMinutes = 10;
        private bool _isTrafficMonitorEnabled = false;
        private bool _showAppIcons = true;
        private bool _autoAllowSystemTrusted = false;
        private bool _alertOnForeignRules = true;
        private bool _filterPrograms = true;
        private bool _filterServices = true;
        private bool _filterUwp = true;
        private bool _filterWildcards = true;
        private bool _filterSystem = false;
        private string _rulesSearchText = "";
        private int _rulesSortColumn = -1;
        private int _rulesSortOrder = 0;
        private string _auditSearchText = "";
        private int _auditSortColumn = -1;
        private int _auditSortOrder = 0;
        private int _liveConnectionsSortColumn = -1;
        private int _liveConnectionsSortOrder = 0;

        public bool IsPopupsEnabled { get => _isPopupsEnabled; set => SetField(ref _isPopupsEnabled, value); }
        public bool IsLoggingEnabled { get => _isLoggingEnabled; set => SetField(ref _isLoggingEnabled, value); }
        public string Theme { get => _theme; set => SetField(ref _theme, value); }
        public bool StartOnSystemStartup { get => _startOnSystemStartup; set => SetField(ref _startOnSystemStartup, value); }
        public bool CloseToTray { get => _closeToTray; set => SetField(ref _closeToTray, value); }
        public int AutoRefreshIntervalMinutes { get => _autoRefreshIntervalMinutes; set => SetField(ref _autoRefreshIntervalMinutes, value); }
        public bool IsTrafficMonitorEnabled { get => _isTrafficMonitorEnabled; set => SetField(ref _isTrafficMonitorEnabled, value); }
        public bool ShowAppIcons { get => _showAppIcons; set => SetField(ref _showAppIcons, value); }
        public bool AutoAllowSystemTrusted { get => _autoAllowSystemTrusted; set => SetField(ref _autoAllowSystemTrusted, value); }
        public bool AlertOnForeignRules { get => _alertOnForeignRules; set => SetField(ref _alertOnForeignRules, value); }
        public bool FilterPrograms { get => _filterPrograms; set => SetField(ref _filterPrograms, value); }
        public bool FilterServices { get => _filterServices; set => SetField(ref _filterServices, value); }
        public bool FilterUwp { get => _filterUwp; set => SetField(ref _filterUwp, value); }
        public bool FilterWildcards { get => _filterWildcards; set => SetField(ref _filterWildcards, value); }
        public bool FilterSystem { get => _filterSystem; set => SetField(ref _filterSystem, value); }
        public string RulesSearchText { get => _rulesSearchText; set => SetField(ref _rulesSearchText, value); }
        public int RulesSortColumn { get => _rulesSortColumn; set => SetField(ref _rulesSortColumn, value); }
        public int RulesSortOrder { get => _rulesSortOrder; set => SetField(ref _rulesSortOrder, value); }
        public string AuditSearchText { get => _auditSearchText; set => SetField(ref _auditSearchText, value); }
        public int AuditSortColumn { get => _auditSortColumn; set => SetField(ref _auditSortColumn, value); }
        public int AuditSortOrder { get => _auditSortOrder; set => SetField(ref _auditSortOrder, value); }
        public int LiveConnectionsSortColumn { get => _liveConnectionsSortColumn; set => SetField(ref _liveConnectionsSortColumn, value); }
        public int LiveConnectionsSortOrder { get => _liveConnectionsSortOrder; set => SetField(ref _liveConnectionsSortOrder, value); }

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