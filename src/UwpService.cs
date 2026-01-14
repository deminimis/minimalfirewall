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
            return await Task.Run(() =>
            {
                var allRules = _firewallRuleService.GetAllRules();
                var uwpApps = new Dictionary<string, UwpApp>(StringComparer.OrdinalIgnoreCase);

                try
                {
                    foreach (INetFwRule2 rule in allRules)
                    {
                        if (token.IsCancellationRequested) break;

                        try
                        {
                            string name = rule.Name ?? string.Empty;

                            // Fast check to skip non-UWP rules before Regex
                            if (name.StartsWith("@{"))
                            {
                                var match = _pfnRegex.Match(name);
                                if (match.Success)
                                {
                                    string pfn = match.Groups["pfn"].Value;

                                    if (!string.IsNullOrEmpty(pfn) && !uwpApps.ContainsKey(pfn))
                                    {
                                        uwpApps[pfn] = new UwpApp
                                        {
                                            Name = name, // Note: You might want to sanitize this name later
                                            PackageFamilyName = pfn,
                                            Publisher = ""
                                        };
                                    }
                                }
                            }
                        }
                        finally
                        {
                            if (rule != null) Marshal.ReleaseComObject(rule);
                        }
                    }

                    if (token.IsCancellationRequested) return new List<UwpApp>();

                    var sortedApps = uwpApps.Values.OrderBy(app => app.Name).ToList();

                    SaveUwpAppsToCache(sortedApps);

                    return sortedApps;
                }
                finally
                {
                    if (allRules != null && Marshal.IsComObject(allRules))
                    {
                        Marshal.ReleaseComObject(allRules);
                    }
                }
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
                    return apps ?? new List<UwpApp>();
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
            return new List<UwpApp>();
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