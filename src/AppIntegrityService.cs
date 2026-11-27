using NetFwTypeLib;
using System.Text.Json;

namespace MinimalFirewall
{
    // On-demand check that the executables allowed by Minimal Firewall rules were not replaced.
    // The first check records a baseline (SHA-256, publisher, size, write time) per file.
    // Later checks hash a file only when its size or write time changed, and report it only when
    // its signature is no longer valid or its publisher changed: a signed update by the same
    // publisher updates the baseline silently. Nothing runs in the background.
    public static class AppIntegrityService
    {
        private sealed record Entry(string Sha256, string Publisher, long Size, DateTime LastWriteUtc);

        private static readonly string StorePath = Path.Combine(AppContext.BaseDirectory, "app_integrity.json");

        // Returns one line per allowed executable whose content changed without a valid signature
        // from the same publisher, and the number of executables checked.
        public static async Task<(List<string> Changed, int Checked)> CheckAllowedAppsAsync()
        {
            var paths = await Task.Run(GetAllowedAppPaths);
            var store = Load();
            var changed = new List<string>();

            foreach (string path in paths)
            {
                var info = new FileInfo(path);
                if (!info.Exists)
                {
                    continue;
                }

                store.TryGetValue(path, out Entry? known);
                if (known != null && known.Size == info.Length && known.LastWriteUtc == info.LastWriteTimeUtc)
                {
                    continue;
                }

                string hash = await SystemDiscoveryService.CalculateSHA256Async(path);
                if (string.IsNullOrEmpty(hash))
                {
                    continue;
                }

                string publisher = string.Empty;
                if (known == null || hash != known.Sha256)
                {
                    publisher = await Task.Run(() => SignatureValidationService.IsSignatureTrusted(path, out string? name) ? name ?? string.Empty : string.Empty);
                }

                if (known != null && hash != known.Sha256 &&
                    (publisher.Length == 0 || !string.Equals(publisher, known.Publisher, StringComparison.OrdinalIgnoreCase)))
                {
                    string before = known.Publisher.Length == 0 ? "unsigned" : known.Publisher;
                    string after = publisher.Length == 0 ? "no valid signature" : publisher;
                    changed.Add($"{path}\n    {before} -> {after}");
                }

                store[path] = new Entry(hash, known != null && hash == known.Sha256 ? known.Publisher : publisher, info.Length, info.LastWriteTimeUtc);
            }

            Save(store);
            return (changed, paths.Count);
        }

        private static List<string> GetAllowedAppPaths()
        {
            var paths = FirewallRuleService.GetAllRulesMapped(rule =>
            {
                string grouping = rule.Grouping ?? string.Empty;
                bool isMfwAllow = rule.Action == NET_FW_ACTION_.NET_FW_ACTION_ALLOW &&
                                  grouping.EndsWith(MFWConstants.firewallRulesuffix, StringComparison.Ordinal);
                string app = rule.ApplicationName ?? string.Empty;
                return isMfwAllow && app.Length > 0 && app != "*" ? PathResolver.NormalizePath(app) : string.Empty;
            });
            return [.. paths.Where(p => p.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase)];
        }

        private static Dictionary<string, Entry> Load()
        {
            try
            {
                if (File.Exists(StorePath))
                {
                    var loaded = JsonSerializer.Deserialize<Dictionary<string, Entry>>(File.ReadAllText(StorePath));
                    if (loaded != null)
                    {
                        return new Dictionary<string, Entry>(loaded, StringComparer.OrdinalIgnoreCase);
                    }
                }
            }
            catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
            {
                System.Diagnostics.Debug.WriteLine($"[WARN] AppIntegrityService.Load: {ex.Message}");
            }
            return new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
        }

        private static void Save(Dictionary<string, Entry> store)
        {
            try
            {
                File.WriteAllText(StorePath, JsonSerializer.Serialize(store));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                System.Diagnostics.Debug.WriteLine($"[WARN] AppIntegrityService.Save: {ex.Message}");
            }
        }
    }
}
