using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MinimalFirewall
{
    public class RuleTimestampService
    {
        private readonly string _path;
        private Dictionary<string, DateTime> _timestamps = new(StringComparer.OrdinalIgnoreCase);
        private readonly object _lock = new();
        private bool _dirty;

        public RuleTimestampService()
        {
            _path = ConfigPathManager.GetConfigPath("rule_timestamps.json");
            Load();
        }

        private void Load()
        {
            lock (_lock)
            {
                try
                {
                    if (!File.Exists(_path)) return;
                    string json = File.ReadAllText(_path);
                    if (string.IsNullOrWhiteSpace(json)) return;

                    var loaded = JsonSerializer.Deserialize(json, RuleTimestampJsonContext.Default.DictionaryStringDateTime);
                    if (loaded != null)
                    {
                        _timestamps = new Dictionary<string, DateTime>(loaded, StringComparer.OrdinalIgnoreCase);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ERROR] Failed to load rule timestamps: {ex.Message}");
                    _timestamps = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
                }
            }
        }

        // Returns existing stamp if known, otherwise records and returns the supplied stamp.
        public DateTime EnsureStamped(string ruleName, DateTime stamp)
        {
            if (string.IsNullOrEmpty(ruleName)) return stamp;
            lock (_lock)
            {
                if (_timestamps.TryGetValue(ruleName, out var existing)) return existing;
                _timestamps[ruleName] = stamp;
                _dirty = true;
                return stamp;
            }
        }

        public DateTime? Get(string ruleName)
        {
            if (string.IsNullOrEmpty(ruleName)) return null;
            lock (_lock)
            {
                return _timestamps.TryGetValue(ruleName, out var ts) ? ts : null;
            }
        }

        public void Set(string ruleName, DateTime stamp)
        {
            if (string.IsNullOrEmpty(ruleName)) return;
            lock (_lock)
            {
                _timestamps[ruleName] = stamp;
                _dirty = true;
            }
        }

        public void PruneTo(IEnumerable<string> activeRuleNames)
        {
            var active = new HashSet<string>(activeRuleNames, StringComparer.OrdinalIgnoreCase);
            lock (_lock)
            {
                var toRemove = _timestamps.Keys.Where(k => !active.Contains(k)).ToList();
                if (toRemove.Count == 0) return;
                foreach (var k in toRemove) _timestamps.Remove(k);
                _dirty = true;
            }
        }

        public void Flush()
        {
            lock (_lock)
            {
                if (!_dirty) return;
                try
                {
                    string json = JsonSerializer.Serialize(_timestamps, RuleTimestampJsonContext.Default.DictionaryStringDateTime);
                    string tempPath = _path + ".tmp";
                    File.WriteAllText(tempPath, json);
                    File.Move(tempPath, _path, overwrite: true);
                    _dirty = false;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ERROR] Failed to save rule timestamps: {ex.Message}");
                }
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                if (_timestamps.Count == 0) return;
                _timestamps.Clear();
                _dirty = true;
            }
            Flush();
        }
    }

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(Dictionary<string, DateTime>))]
    internal partial class RuleTimestampJsonContext : JsonSerializerContext { }
}