// UserActivityLogger.cs
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Diagnostics;

namespace MinimalFirewall
{
    public class UserActivityLogger
    {
        private readonly string _debugLogFilePath;
        private readonly string _changeLogFilePath;
        public bool IsEnabled { get; set; }

        public UserActivityLogger()
        {
            string baseDirectory = AppContext.BaseDirectory;
            _debugLogFilePath = Path.Combine(baseDirectory, "debug_log.txt");
            _changeLogFilePath = Path.Combine(baseDirectory, "changelog.json");
        }

        public void LogChange(string action, string details)
        {
            if (!IsEnabled)
            {
                return;
            }

            try
            {
                var newLogEntry = new { Timestamp = DateTime.Now, Action = action, Details = details };
                List<object> logEntries;

                if (File.Exists(_changeLogFilePath))
                {
                    string json = File.ReadAllText(_changeLogFilePath);
                    logEntries = JsonSerializer.Deserialize<List<object>>(json) ?? new List<object>();
                }
                else
                {
                    logEntries = new List<object>();
                }

                logEntries.Add(newLogEntry);
                string newJson = JsonSerializer.Serialize(logEntries, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_changeLogFilePath, newJson);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[FATAL LOGGING ERROR] {ex.Message}");
            }
        }

        public void LogDebug(string message)
        {
            if (!IsEnabled)
            {
                return;
            }
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string logEntry = $"[{timestamp}] {message}{Environment.NewLine}";
                File.AppendAllText(_debugLogFilePath, logEntry);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FATAL DEBUG LOGGING ERROR] {ex.Message}");
            }
        }
    }
}
