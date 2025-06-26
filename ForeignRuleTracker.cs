using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MinimalFirewall
{
    public class ForeignRuleTracker
    {
        private readonly string _baselinePath;
        private HashSet<string> _acknowledgedRuleNames;

        public ForeignRuleTracker()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
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
                    _acknowledgedRuleNames = JsonSerializer.Deserialize<HashSet<string>>(json) ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                }
                else
                {
                    _acknowledgedRuleNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                }
            }
            catch
            {
                _acknowledgedRuleNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private void Save()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(_acknowledgedRuleNames, options);
                File.WriteAllText(_baselinePath, json);
            }
            catch (Exception)
            {
                // A failure to save the baseline should not crash the app
            }
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
}