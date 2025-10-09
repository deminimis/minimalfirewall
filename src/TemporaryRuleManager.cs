// File: TemporaryRuleManager.cs
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MinimalFirewall
{
    public class TemporaryRuleManager
    {
        private readonly string _storagePath;
        private readonly ConcurrentDictionary<string, DateTime> _temporaryRules;

        public TemporaryRuleManager()
        {
            string exeDirectory = Path.GetDirectoryName(Environment.ProcessPath)!;
            _storagePath = Path.Combine(exeDirectory, "temporary_rules.json");
            _temporaryRules = Load();
        }

        private ConcurrentDictionary<string, DateTime> Load()
        {
            try
            {
                if (File.Exists(_storagePath))
                {
                    string json = File.ReadAllText(_storagePath);
                    var rules = JsonSerializer.Deserialize(json, TempRuleJsonContext.Default.DictionaryStringDateTime);
                    return new ConcurrentDictionary<string, DateTime>(rules ?? new Dictionary<string, DateTime>(), StringComparer.OrdinalIgnoreCase);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Failed to load temporary rules: {ex.Message}");
            }
            return new ConcurrentDictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
        }

        private void Save()
        {
            try
            {
                var rulesToSave = new Dictionary<string, DateTime>(_temporaryRules, StringComparer.OrdinalIgnoreCase);
                string json = JsonSerializer.Serialize(rulesToSave, TempRuleJsonContext.Default.DictionaryStringDateTime);
                File.WriteAllText(_storagePath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Failed to save temporary rules: {ex.Message}");
            }
        }

        public void Add(string ruleName, DateTime expiryTimeUtc)
        {
            _temporaryRules[ruleName] = expiryTimeUtc;
            Save();
        }

        public void Remove(string ruleName)
        {
            if (_temporaryRules.TryRemove(ruleName, out _))
            {
                Save();
            }
        }

        public Dictionary<string, DateTime> GetExpiredRules()
        {
            var now = DateTime.UtcNow;
            return _temporaryRules
                .Where(kvp => kvp.Value <= now)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(Dictionary<string, DateTime>))]
    internal partial class TempRuleJsonContext : JsonSerializerContext { }
}