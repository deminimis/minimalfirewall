// File: UwpService.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MinimalFirewall
{
    public class UwpService
    {
        private readonly string _cachePath;
        public UwpService()
        {
            string baseDirectory = AppContext.BaseDirectory;
            _cachePath = Path.Combine(baseDirectory, "uwp_apps.json");
        }

        public async Task<List<UwpApp>> GetUwpAppsAsync(CancellationToken token)
        {
            return await Task.Run(() =>
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = "-NoProfile -ExecutionPolicy Bypass -Command \"Get-AppxPackage | Where-Object { !$_.IsFramework -and !$_.IsResourcePackage } | Select-Object Name, PackageFamilyName, Publisher | ConvertTo-Json\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8
                };

                try
                {
                    using (var process = Process.Start(psi))
                    {
                        if (process == null) return new List<UwpApp>();
                        var output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit();

                        if (token.IsCancellationRequested) return new List<UwpApp>();

                        if (string.IsNullOrWhiteSpace(output))
                        {
                            return new List<UwpApp>();
                        }

                        var apps = JsonSerializer.Deserialize(output, UwpAppJsonContext.Default.ListUwpApp);
                        var sortedApps = (apps ?? new List<UwpApp>()).OrderBy(app => app.Name).ToList();

                        SaveUwpAppsToCache(sortedApps);
                        return sortedApps;
                    }
                }
                catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException or JsonException)
                {
                    Debug.WriteLine("[ERROR] Failed to scan for UWP apps via PowerShell: " + ex.Message);
                    return new List<UwpApp>();
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