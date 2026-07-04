// File: UwpService.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NetFwTypeLib;

namespace MinimalFirewall
{
    public class UwpService
    {
        private readonly string _cachePath;
        private readonly FirewallRuleService _firewallRuleService;
        private static readonly SemaphoreSlim _cacheLock = new SemaphoreSlim(1, 1);

        // Matches "@{PackageFamilyName?ms-resource" or "@{PackageFamilyName}"
        private static readonly Regex _pfnRegex = new Regex(
            @"^@\{(?<pfn>[^?}]+)(\?ms-resource|\})",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public UwpService(FirewallRuleService firewallRuleService)
        {
            _cachePath = ConfigPathManager.GetConfigPath("uwp_apps.json");
            _firewallRuleService = firewallRuleService;
        }

        public async Task<List<UwpApp>> GetUwpAppsAsync(CancellationToken token)
        {
            return await Task.Run(async () =>
            {
                if (token.IsCancellationRequested) return [];

                var uwpApps = new Dictionary<string, UwpApp>(StringComparer.OrdinalIgnoreCase);

                try
                {
                    // Query Windows directly
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = "-NoProfile -Command \"@(Get-AppxPackage -ErrorAction SilentlyContinue | Select-Object Name, PackageFamilyName, Publisher) | ConvertTo-Json -Compress\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using var process = Process.Start(processInfo);
                    if (process != null)
                    {
                        string jsonOutput = await process.StandardOutput.ReadToEndAsync(token);
                        await process.WaitForExitAsync(token);

                        // Safely extract just the JSON array
                        int jsonStart = jsonOutput.IndexOf('[');
                        if (jsonStart >= 0)
                        {
                            string cleanJson = jsonOutput.Substring(jsonStart);
                            var apps = JsonSerializer.Deserialize<List<UwpApp>>(cleanJson, UwpAppJsonContext.Default.ListUwpApp);
                            if (apps != null)
                            {
                                foreach (var app in apps)
                                {
                                    if (!string.IsNullOrEmpty(app.PackageFamilyName) && !uwpApps.ContainsKey(app.PackageFamilyName))
                                    {
                                        uwpApps[app.PackageFamilyName] = app;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ERROR] Failed to fetch UWP apps via PowerShell: {ex.Message}");
                }

                if (token.IsCancellationRequested) return [];

                var sortedApps = uwpApps.Values.OrderBy(app => app.Name).ToList();
                SaveUwpAppsToCache(sortedApps);

                return sortedApps;

            }, token).ConfigureAwait(false);
        }

        public List<UwpApp> LoadUwpAppsFromCache()
        {
            _cacheLock.Wait(); 
            try
            {
                if (File.Exists(_cachePath))
                {
                    string json = File.ReadAllText(_cachePath);
                    var apps = JsonSerializer.Deserialize(json, UwpAppJsonContext.Default.ListUwpApp);
                    return apps ?? [];
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            {
                Debug.WriteLine("[ERROR] Failed to load UWP cache: " + ex.Message);
            }
            finally
            {
                _cacheLock.Release();
            }
            return [];
        }

        private void SaveUwpAppsToCache(List<UwpApp> apps)
        {
            _cacheLock.Wait(); 
            try
            {
                string json = JsonSerializer.Serialize(apps, UwpAppJsonContext.Default.ListUwpApp);
                File.WriteAllText(_cachePath, json);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Debug.WriteLine("[ERROR] Failed to save UWP cache: " + ex.Message);
            }
            finally
            {
                _cacheLock.Release();
            }
        }
    }
}
