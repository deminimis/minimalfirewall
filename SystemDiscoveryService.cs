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
        private static Dictionary<string, (string DisplayName, string ServiceName)> _serviceCache;
        private static bool _wmiQueryFailedMessageShown = false;

        public static Dictionary<string, (string DisplayName, string ServiceName)> GetServicesWithExePaths()
        {
            if (_serviceCache != null)
            {
                return _serviceCache;
            }

            var services = new Dictionary<string, (string, string)>(StringComparer.OrdinalIgnoreCase);
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

                        if (!string.IsNullOrEmpty(pathName) && !services.ContainsKey(pathName))
                        {
                            services[pathName] = (service["DisplayName"]?.ToString() ?? "", service["Name"]?.ToString() ?? "");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // THIS CATCH BLOCK IS NOW IMPROVED
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

        public static List<ProgramViewModel> ScanDirectoryForExecutables(string directoryPath, ICollection<string> existingRulePaths)
        {
            var programs = new List<ProgramViewModel>();
            if (!Directory.Exists(directoryPath)) return programs;

            var existingRuleSet = new HashSet<string>(existingRulePaths, StringComparer.OrdinalIgnoreCase);
            try
            {
                var exeFiles = Directory.GetFiles(directoryPath, "*.exe", SearchOption.AllDirectories);
                foreach (var exe in exeFiles)
                {
                    if (!existingRuleSet.Contains(exe))
                    {
                        programs.Add(new ProgramViewModel { Name = Path.GetFileName(exe), ExePath = exe });
                        existingRuleSet.Add(exe);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Access denied to a subdirectory within '" + directoryPath + "'. Try running as administrator or choosing a different folder.", "Permission Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while scanning: " + ex.Message, "Scanning Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return programs;
        }
    }
}