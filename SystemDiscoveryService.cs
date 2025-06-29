using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Windows;

namespace MinimalFirewall
{
    public static class SystemDiscoveryService
    {
        private static List<ServiceViewModel> _serviceCache;
        private static bool _wmiQueryFailedMessageShown = false;

        public static List<ServiceViewModel> GetServicesWithExePaths()
        {
            if (_serviceCache != null)
            {
                return _serviceCache;
            }

            var services = new List<ServiceViewModel>();
            try
            {
                var wmiQuery = new ObjectQuery("SELECT Name, DisplayName, PathName FROM Win32_Service WHERE PathName IS NOT NULL");
                using (var searcher = new ManagementObjectSearcher(wmiQuery))
                {
                    foreach (ManagementObject service in searcher.Get().Cast<ManagementObject>())
                    {
                        string rawPath = service["PathName"]?.ToString() ?? string.Empty;
                        if (string.IsNullOrEmpty(rawPath))
                        {
                            continue;
                        }

                        string pathName = rawPath;
                        int exeIndex = rawPath.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);

                        if (exeIndex > 0)
                        {
                            pathName = rawPath.Substring(0, exeIndex + 4);
                        }
                        else
                        {
                            if (rawPath.StartsWith("\""))
                            {
                                int closingQuoteIndex = rawPath.IndexOf('"', 1);
                                if (closingQuoteIndex > 0)
                                {
                                    pathName = rawPath.Substring(0, closingQuoteIndex + 1);
                                }
                            }
                            else
                            {
                                int firstSpaceIndex = rawPath.IndexOf(' ');
                                if (firstSpaceIndex > 0)
                                {
                                    pathName = rawPath.Substring(0, firstSpaceIndex);
                                }
                            }
                        }

                        pathName = pathName.Trim('"');

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
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("WMI Query failed: " + ex.Message);
                if (!_wmiQueryFailedMessageShown)
                {
                    MessageBox.Show(
                        "Could not query Windows Services (WMI). This may be due to security restrictions in your environment.\n\nThe 'Services' tab will be empty.",
                        "Feature Unavailable",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    _wmiQueryFailedMessageShown = true;
                }
            }

            _serviceCache = services;
            return services;
        }

        public static string GetServicesByPID(string processId)
        {
            if (string.IsNullOrEmpty(processId) || processId == "0") return string.Empty;

            try
            {
                var query = new ObjectQuery($"SELECT Name FROM Win32_Service WHERE ProcessId = {processId}");
                using (var searcher = new ManagementObjectSearcher(query))
                {
                    var serviceNames = searcher.Get().Cast<ManagementObject>()
                                               .Select(s => s["Name"]?.ToString())
                                               .Where(n => !string.IsNullOrEmpty(n));
                    return string.Join(", ", serviceNames);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WMI Query for PID failed: {ex.Message}");
                return string.Empty;
            }
        }

        public static List<ProgramViewModel> ScanDirectoryForExecutables(string directoryPath, ICollection<string> existingRulePaths)
        {
            var programs = new List<ProgramViewModel>();
            if (!Directory.Exists(directoryPath)) return programs;

            var existingRuleSet = new HashSet<string>(existingRulePaths, StringComparer.OrdinalIgnoreCase);
            try
            {
                var exeFiles = GetExecutablesInFolder(directoryPath, "*.exe");
                foreach (var exe in exeFiles)
                {
                    if (!existingRuleSet.Contains(exe))
                    {
                        programs.Add(new ProgramViewModel { Name = Path.GetFileName(exe), ExePath = exe });
                        existingRuleSet.Add(exe);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while scanning: " + ex.Message, "Scanning Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return programs;
        }

        public static List<string> GetExecutablesInFolder(string directoryPath, string exeName = null)
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
            catch (UnauthorizedAccessException)
            {
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error scanning folder {directoryPath}: {ex.Message}");
            }
        }
    }
}