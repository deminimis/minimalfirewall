// ForeignRuleTracker.cs
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

        public ForeignRuleTracker()
        {
            string baseDirectory = AppContext.BaseDirectory;
            _baselinePath = Path.Combine(baseDirectory, "foreign_rules_baseline.json");
            LoadAcknowledgedRules();
        }

        private void LoadAcknowledgedRules()
        {
            try
            {
                if (File.Exists(_baselinePath))
                {
                    string json = File.ReadAllText(_baselinePath);
                    _acknowledgedRuleNames = JsonSerializer.Deserialize(json, ForeignRuleTrackerJsonContext.Default.HashSetString) ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Failed to load foreign rule baseline: {ex.Message}");
                _acknowledgedRuleNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private void Save()
        {
            try
            {
                string json = JsonSerializer.Serialize(_acknowledgedRuleNames, ForeignRuleTrackerJsonContext.Default.HashSetString);
                File.WriteAllText(_baselinePath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Failed to save foreign rule baseline: {ex.Message}");
            }
        }

        public void Clear()
        {
            _acknowledgedRuleNames.Clear();
            Save();
        }

        public bool IsAcknowledged(string ruleName)
        {
            return _acknowledgedRuleNames.Contains(ruleName);
        }

        public void AcknowledgeRules(IEnumerable<string> ruleNames)
        {
            foreach (var name in ruleNames)
            {
                _acknowledgedRuleNames.Add(name);
            }
            Save();
        }
    }

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(HashSet<string>))]
    internal partial class ForeignRuleTrackerJsonContext : JsonSerializerContext { }
}