using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;

namespace MinimalFirewall
{
    public static class SystemDiscoveryService
    {
        private static Dictionary<string, (string DisplayName, string ServiceName)> _serviceCache;

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

                        // --- Corrected Path Parsing Logic ---
                        string pathName = rawPath;
                        int exeIndex = rawPath.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);

                        if (exeIndex > 0)
                        {
                            // If ".exe" is found, take the substring up to and including it.
                            // This correctly handles paths with spaces that are not quoted.
                            pathName = rawPath.Substring(0, exeIndex + 4);
                        }
                        else
                        {
                            // Fallback for cases without .exe but with arguments (less common).
                            // This splits by the first space only if it's outside of quotes.
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

                        // Always trim quotes from the final result.
                        pathName = pathName.Trim('"');
                        // --- End of Corrected Logic ---

                        if (!string.IsNullOrEmpty(pathName) && !services.ContainsKey(pathName))
                        {
                            services[pathName] = (service["DisplayName"]?.ToString() ?? "", service["Name"]?.ToString() ?? "");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("WMI Query failed: " + ex.Message);
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
                System.Windows.MessageBox.Show("Access denied to a subdirectory within '" + directoryPath + "'. Try running as administrator or choosing a different folder.", "Permission Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("An error occurred while scanning: " + ex.Message, "Scanning Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }

            return programs;
        }
    }
}
