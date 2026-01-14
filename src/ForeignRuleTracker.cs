using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Diagnostics;

namespace MinimalFirewall
{
    public class ForeignRuleTracker
    {
        private readonly string _baselinePath;
        private HashSet<string> _acknowledgedRuleNames = new(StringComparer.OrdinalIgnoreCase);
        // Lock object for thread safety
        private readonly object _lock = new();

        public ForeignRuleTracker()
        {
            _baselinePath = ConfigPathManager.GetConfigPath("foreign_rules_baseline.json");
            LoadAcknowledgedRules();
        }

        private void LoadAcknowledgedRules()
        {
            lock (_lock)
            {
                try
                {
                    if (File.Exists(_baselinePath))
                    {
                        string json = File.ReadAllText(_baselinePath);
                        var loaded = JsonSerializer.Deserialize(json, ForeignRuleTrackerJsonContext.Default.HashSetString);
                        _acknowledgedRuleNames = loaded ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ERROR] Failed to load foreign rule baseline: {ex.Message}");
                    _acknowledgedRuleNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                }
            }
        }

        private void Save()
        {
            lock (_lock)
            {
                try
                {
                    string json = JsonSerializer.Serialize(_acknowledgedRuleNames, ForeignRuleTrackerJsonContext.Default.HashSetString);

                    // Write to a temp file first to prevent corruption on crash
                    string tempPath = _baselinePath + ".tmp";
                    File.WriteAllText(tempPath, json);

                    // Atomic move/overwrite
                    File.Move(tempPath, _baselinePath, overwrite: true);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ERROR] Failed to save foreign rule baseline: {ex.Message}");
                }
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _acknowledgedRuleNames.Clear();
                Save();
            }
        }

        public bool IsAcknowledged(string ruleName)
        {
            lock (_lock)
            {
                return _acknowledgedRuleNames.Contains(ruleName);
            }
        }

        public void AcknowledgeRules(IEnumerable<string> ruleNames)
        {
            lock (_lock)
            {
                _acknowledgedRuleNames.UnionWith(ruleNames);
                Save();
            }
        }
    }

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(HashSet<string>))]
    internal partial class ForeignRuleTrackerJsonContext : JsonSerializerContext { }
}