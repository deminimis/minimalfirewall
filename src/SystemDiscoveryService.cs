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
                using var results = searcher.Get();
                foreach (ManagementBaseObject serviceBaseObject in results)
                {
                    using var service = (ManagementObject)serviceBaseObject;
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
            if (string.IsNullOrEmpty(processId) || processId == "0" || !uint.TryParse(processId, out _))
            {
                return string.Empty;
            }

            var serviceNames = new List<string?>();
            try
            {
                var query = new ObjectQuery($"SELECT Name FROM Win32_Service WHERE ProcessId = {processId}");
                using var searcher = new ManagementObjectSearcher(query);
                using var results = searcher.Get();
                foreach (ManagementBaseObject serviceBaseObject in results)
                {
                    using (var service = (ManagementObject)serviceBaseObject)
                    {
                        serviceNames.Add(service["Name"]?.ToString());
                    }
                }
                return string.Join(", ", serviceNames.Where(n => !string.IsNullOrEmpty(n)));
            }
            catch (Exception ex) when (ex is ManagementException or System.Runtime.InteropServices.COMException)
            {
                Debug.WriteLine($"WMI Query for PID failed: {ex.Message}");
                return string.Empty;
            }
        }

        public static List<string> GetFilesInFolder(string directoryPath, List<string> searchPatterns)
        {
            var files = new List<string>();
            if (searchPatterns == null || searchPatterns.Count == 0 || !Directory.Exists(directoryPath))
            {
                return files;
            }

            var dirs = new Stack<string>();
            dirs.Push(directoryPath);

            while (dirs.Count > 0)
            {
                string currentDir = dirs.Pop();

                try
                {
                    foreach (string pattern in searchPatterns)
                    {
                        files.AddRange(Directory.EnumerateFiles(currentDir, pattern));
                    }

                    foreach (var subDir in Directory.EnumerateDirectories(currentDir))
                    {
                        dirs.Push(subDir);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // Ignore folders we don't have permission to access
                }
                catch (IOException ex)
                {
                    Debug.WriteLine($"Error scanning folder {currentDir}: {ex.Message}");
                }
            }
            return files;
        }
    }
}