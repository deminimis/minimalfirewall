using System;
using System.IO;

namespace MinimalFirewall
{
    public class UserActivityLogger
    {
        private readonly string _logFilePath;
        public bool IsEnabled { get; set; }

        public UserActivityLogger()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _logFilePath = Path.Combine(baseDirectory, "user_log.txt");
        }

        public void Log(string action, string details)
        {
            if (!IsEnabled)
            {
                return;
            }

            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string logEntry = $"[{timestamp}] {action}: {details}{Environment.NewLine}";
                File.AppendAllText(_logFilePath, logEntry);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FATAL LOGGING ERROR] {ex.Message}");
            }
        }
    }
}