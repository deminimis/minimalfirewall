// File: UwpService.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NetFwTypeLib;

namespace MinimalFirewall
{
    public class UwpService
    {
        private readonly string _cachePath;
        private readonly FirewallRuleService _firewallRuleService;

        public UwpService(FirewallRuleService firewallRuleService)
        {
            string exeDirectory = Path.GetDirectoryName(Environment.ProcessPath)!;
            _cachePath = Path.Combine(exeDirectory, "uwp_apps.json");
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
                        if (token.IsCancellationRequested) return new List<UwpApp>();

                        string? pfn = null;
                        string name = rule.Name ?? string.Empty;

                        if (name.StartsWith("@{") && name.Contains("}"))
                        {
                            int startIndex = 2;
                            int endIndex = name.IndexOf("?ms-resource");
                            if (endIndex == -1)
                            {
                                endIndex = name.IndexOf("}");
                            }
                            if (endIndex > startIndex)
                            {
                                pfn = name.Substring(startIndex, endIndex - startIndex);
                            }
                        }

                        if (!string.IsNullOrEmpty(pfn) && !uwpApps.ContainsKey(pfn))
                        {
                            uwpApps[pfn] = new UwpApp
                            {
                                Name = name,
                                PackageFamilyName = pfn,
                                Publisher = ""
                            };
                        }
                    }

                    var sortedApps = uwpApps.Values.OrderBy(app => app.Name).ToList();
                    SaveUwpAppsToCache(sortedApps);
                    return sortedApps;
                }
                finally
                {
                    foreach (var rule in allRules)
                    {
                        if (rule != null)
                        {
                            Marshal.ReleaseComObject(rule);
                        }
                    }
                }
            }, token).ConfigureAwait(false);
        }

        public List<UwpApp> LoadUwpAppsFromCache()
        {
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
            return new List<UwpApp>();
        }

        private void SaveUwpAppsToCache(List<UwpApp> apps)
        {
            try
            {
                string json = JsonSerializer.Serialize(apps, UwpAppJsonContext.Default.ListUwpApp);
                File.WriteAllText(_cachePath, json);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Debug.WriteLine("[ERROR] Failed to save UWP cache: " + ex.Message);
            }
        }
    }
}