using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MinimalFirewall
{
    public class UwpService
    {
        private readonly string _cachePath;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { WriteIndented = true, PropertyNameCaseInsensitive = true };

        public UwpService()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _cachePath = Path.Combine(baseDirectory, "uwp_apps.json");
        }

        public async Task<List<UwpApp>> ScanForUwpApps()
        {
            return await Task.Run(() =>
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = "-NoProfile -ExecutionPolicy Bypass -Command \"Get-AppxPackage | Where-Object { !$_.IsFramework -and !$_.IsResourcePackage } | Select-Object Name, PackageFamilyName | ConvertTo-Json\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8
                };

                try
                {
                    // This 'using' block is rewritten for C# 7.3 compatibility
                    using (var process = Process.Start(psi))
                    {
                        var output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit();

                        if (string.IsNullOrWhiteSpace(output))
                        {
                            return new List<UwpApp>();
                        }

                        var apps = JsonSerializer.Deserialize<List<UwpApp>>(output, _jsonOptions);
                        var sortedApps = (apps ?? new List<UwpApp>()).OrderBy(app => app.Name).ToList();

                        SaveUwpAppsToCache(sortedApps);
                        return sortedApps;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[ERROR] Failed to scan for UWP apps via PowerShell: " + ex.Message);
                    return new List<UwpApp>();
                }
            });
        }

        public List<UwpApp> LoadUwpAppsFromCache()
        {
            try
            {
                if (File.Exists(_cachePath))
                {
                    string json = File.ReadAllText(_cachePath);
                    var apps = JsonSerializer.Deserialize<List<UwpApp>>(json);
                    return apps ?? new List<UwpApp>();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[ERROR] Failed to load UWP cache: " + ex.Message);
            }
            return new List<UwpApp>();
        }

        private void SaveUwpAppsToCache(List<UwpApp> apps)
        {
            try
            {
                // We no longer load icons, so the list is already serializable.
                string json = JsonSerializer.Serialize(apps, _jsonOptions);
                File.WriteAllText(_cachePath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[ERROR] Failed to save UWP cache: " + ex.Message);
            }
        }
    }
}