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
        private bool _isBaselineSession;

        public RuleTimestampService()
        {
            _path = ConfigPathManager.GetConfigPath("rule_timestamps.json");
            _isBaselineSession = !File.Exists(_path);
            Load();
        }

        // True until the first refresh has flushed; rules observed during this window are
        // recorded with a sentinel so they display as Unknown rather than as the install-time stamp.
        public bool IsBaselineSession
        {
            get { lock (_lock) { return _isBaselineSession; } }
        }

        public void EndBaselineSession()
        {
            lock (_lock) { _isBaselineSession = false; }
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

        // Returns existing stamp if known, otherwise records the supplied stamp.
        // When treatAsBaseline is true and the rule is unseen, stores DateTime.MinValue
        // as an Unknown sentinel and returns null. MinValue is also translated to null on read.
        public DateTime? EnsureStamped(string ruleName, DateTime stamp, bool treatAsBaseline = false)
        {
            if (string.IsNullOrEmpty(ruleName)) return treatAsBaseline ? null : stamp;
            lock (_lock)
            {
                if (_timestamps.TryGetValue(ruleName, out var existing))
                {
                    return existing == DateTime.MinValue ? null : existing;
                }
                var toStore = treatAsBaseline ? DateTime.MinValue : stamp;
                _timestamps[ruleName] = toStore;
                _dirty = true;
                return toStore == DateTime.MinValue ? null : toStore;
            }
        }

        public DateTime? Get(string ruleName)
        {
            if (string.IsNullOrEmpty(ruleName)) return null;
            lock (_lock)
            {
                if (_timestamps.TryGetValue(ruleName, out var ts) && ts != DateTime.MinValue)
                    return ts;
                return null;
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