// File: UserActivityLogger.cs
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

        private readonly object _debugLock = new object();
        private readonly object _changeLock = new object();

        // Cache options to avoid recreating them on every log entry
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public bool IsEnabled { get; set; }

        public UserActivityLogger()
        {
            _debugLogFilePath = ConfigPathManager.GetConfigPath("debug_log.txt");
            _changeLogFilePath = ConfigPathManager.GetConfigPath("changelog.json");
        }

        public void LogChange(string action, string details)
        {
            if (!IsEnabled) return;

            // Use a lock to ensure only one thread writes to the JSON file at a time
            lock (_changeLock)
            {
                try
                {
                    var newLogEntry = new { Timestamp = DateTime.Now, Action = action, Details = details };
                    List<object> logEntries;

                    if (File.Exists(_changeLogFilePath))
                    {
                        string json = File.ReadAllText(_changeLogFilePath);
                        // Handle case where file exists but is empty
                        if (string.IsNullOrWhiteSpace(json))
                        {
                            logEntries = new List<object>();
                        }
                        else
                        {
                            logEntries = JsonSerializer.Deserialize<List<object>>(json) ?? new List<object>();
                        }
                    }
                    else
                    {
                        logEntries = new List<object>();
                    }

                    logEntries.Add(newLogEntry);

                    string newJson = JsonSerializer.Serialize(logEntries, _jsonOptions);
                    File.WriteAllText(_changeLogFilePath, newJson);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
                {
                    // Fallback to debug log if the change log fails
                    LogDebug($"[FATAL LOGGING ERROR] Could not write to changelog: {ex.Message}");
                }
            }
        }

        public void LogDebug(string message)
        {
            if (!IsEnabled) return;
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string logEntry = $"[{timestamp}] {message}{Environment.NewLine}";

            WriteToDebugFile(logEntry);
        }

        public void LogException(string context, Exception ex)
        {
            if (!IsEnabled) return;

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string hex = $"0x{ex.HResult:X8}";
            string type = ex.GetType().Name;
            string msg = ex.Message?.Replace(Environment.NewLine, " ").Trim() ?? "";

            string logEntry = $"[{timestamp}] ERROR {context} {type} HResult={hex} Message={msg}{Environment.NewLine}";

            WriteToDebugFile(logEntry);
        }

        private void WriteToDebugFile(string text)
        {
            lock (_debugLock)
            {
                try
                {
                    File.AppendAllText(_debugLogFilePath, text);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    // If we can't write to the debug log, we write to the system debug listener
                    Debug.WriteLine($"[FATAL DEBUG LOGGING ERROR] {ex.Message}");
                }
            }
        }
    }
}