// File: SystemDiscoveryService.cs
using DarkModeForms;
using System.IO;
using System.Management;
using System.Diagnostics;

namespace MinimalFirewall
{
    public static class SystemDiscoveryService
    {
        private static bool _wmiQueryFailedMessageShown = false;

        public static List<ServiceViewModel> GetServicesWithExePaths()
        {
            var services = new List<ServiceViewModel>();
            try
            {
                var wmiQuery = new ObjectQuery("SELECT Name, DisplayName, PathName FROM Win32_Service WHERE PathName IS NOT NULL");
                using var searcher = new ManagementObjectSearcher(wmiQuery);
                foreach (ManagementObject service in searcher.Get().Cast<ManagementObject>())
                {
                    string rawPath = service["PathName"]?.ToString() ?? string.Empty;
                    if (string.IsNullOrEmpty(rawPath)) continue;

                    string pathName = rawPath.Trim('"');
                    int exeIndex = pathName.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
                    if (exeIndex > 0)
                    {
                        pathName = pathName[..(exeIndex + 4)];
                    }

                    if (!string.IsNullOrEmpty(pathName))
                    {
                        services.Add(new ServiceViewModel
                        {
                            ExePath = pathName,
                            DisplayName = service["DisplayName"]?.ToString() ?? "",
                            ServiceName = service["Name"]?.ToString() ?? ""
                        });
                    }
                }
            }
            catch (Exception ex) when (ex is ManagementException or System.Runtime.InteropServices.COMException)
            {
                Debug.WriteLine("WMI Query failed: " + ex.Message);
                if (!_wmiQueryFailedMessageShown)
                {
                    Messenger.MessageBox("Could not query Windows Services (WMI).", "Feature Unavailable", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _wmiQueryFailedMessageShown = true;
                }
            }
            return services;
        }

        public static string GetServicesByPID(string processId)
        {
            if (string.IsNullOrEmpty(processId) || processId == "0") return string.Empty;
            try
            {
                var query = new ObjectQuery($"SELECT Name FROM Win32_Service WHERE ProcessId = {processId}");
                using var searcher = new ManagementObjectSearcher(query);
                var serviceNames = searcher.Get().Cast<ManagementObject>()
                                           .Select(s => s["Name"]?.ToString())
                                           .Where(n => !string.IsNullOrEmpty(n));
                return string.Join(", ", serviceNames);
            }
            catch (Exception ex) when (ex is ManagementException or System.Runtime.InteropServices.COMException)
            {
                Debug.WriteLine($"WMI Query for PID failed: {ex.Message}");
                return string.Empty;
            }
        }

        public static List<string> GetExecutablesInFolder(string directoryPath, string? exeName = null)
        {
            var files = new List<string>();
            string searchPattern = string.IsNullOrWhiteSpace(exeName) ? "*.exe" : exeName;
            GetExecutablesInFolderRecursive(directoryPath, searchPattern, files);
            return files;
        }

        private static void GetExecutablesInFolderRecursive(string directoryPath, string searchPattern, List<string> files)
        {
            try
            {
                files.AddRange(Directory.GetFiles(directoryPath, searchPattern));
                foreach (var directory in Directory.GetDirectories(directoryPath))
                {
                    GetExecutablesInFolderRecursive(directory, searchPattern, files);
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (IOException ex)
            {
                Debug.WriteLine($"Error scanning folder {directoryPath}: {ex.Message}");
            }
        }
    }
}