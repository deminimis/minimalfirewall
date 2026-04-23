using DarkModeForms;
using System.IO;
using System.Management;
using System.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Security.Cryptography;

namespace MinimalFirewall
{
    public static class SystemDiscoveryService
    {
        private static bool _wmiQueryFailedMessageShown = false;
        public record ProcessExtendedDetails(string CommandLine, string ParentProcessId, string ParentProcessName, string ProcessOwner);
        private static readonly MemoryCache _processDetailsCache = new(new MemoryCacheOptions());

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

        public static ProcessExtendedDetails GetExtendedProcessDetailsByPID(string processId)
        {
            if (string.IsNullOrEmpty(processId) || processId == "0" || !uint.TryParse(processId, out _))
            {
                return new ProcessExtendedDetails(string.Empty, string.Empty, string.Empty, string.Empty);
            }

            string cacheKey = $"procdetails_{processId}";
            if (_processDetailsCache.TryGetValue(cacheKey, out ProcessExtendedDetails? cachedDetails) && cachedDetails != null)
            {
                return cachedDetails;
            }

            string commandLine = string.Empty;
            string parentPid = string.Empty;
            string parentName = string.Empty;
            string owner = string.Empty;

            try
            {
                var query = new ObjectQuery($"SELECT CommandLine, ParentProcessId FROM Win32_Process WHERE ProcessId = {processId}");
                using var searcher = new ManagementObjectSearcher(query);
                using var results = searcher.Get();

                foreach (ManagementBaseObject processBaseObject in results)
                {
                    using (var process = (ManagementObject)processBaseObject)
                    {
                        commandLine = process["CommandLine"]?.ToString() ?? string.Empty;
                        parentPid = process["ParentProcessId"]?.ToString() ?? string.Empty;

                        // retrieve process owner
                        try
                        {
                            var ownerResult = process.InvokeMethod("GetOwner", null, null);
                            if (ownerResult != null && (uint)ownerResult["returnValue"] == 0)
                            {
                                string user = ownerResult["User"]?.ToString() ?? string.Empty;
                                string domain = ownerResult["Domain"]?.ToString() ?? string.Empty;
                                owner = string.IsNullOrEmpty(domain) ? user : $"{domain}\\{user}";
                            }
                        }
                        catch { /* Ignore errors if lacking permissions */ }

                        break;
                    }
                }

                // name query for parent PID
                if (!string.IsNullOrEmpty(parentPid))
                {
                    var parentQuery = new ObjectQuery($"SELECT Name FROM Win32_Process WHERE ProcessId = {parentPid}");
                    using var parentSearcher = new ManagementObjectSearcher(parentQuery);
                    using var parentResults = parentSearcher.Get();
                    foreach (ManagementBaseObject parentObj in parentResults)
                    {
                        using (var pObj = (ManagementObject)parentObj)
                        {
                            parentName = pObj["Name"]?.ToString() ?? string.Empty;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is ManagementException or System.Runtime.InteropServices.COMException)
            {
                Debug.WriteLine($"WMI Query for Process Details failed: {ex.Message}");
            }

            var details = new ProcessExtendedDetails(commandLine, parentPid, parentName, owner);
            var cacheOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5));
            _processDetailsCache.Set(cacheKey, details, cacheOptions);

            return details;
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

        public static async Task<string> CalculateSHA256Async(string filePath)
        {
            if (!File.Exists(filePath)) return string.Empty;
            try
            {
                using var sha256 = SHA256.Create();
                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                byte[] hash = await sha256.ComputeHashAsync(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to hash file: {ex.Message}");
                return string.Empty;
            }
        }
    }
}