using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MinimalFirewall
{
    public class WildcardRuleService
    {
        private readonly string _configPath;
        private List<WildcardRule> _rules;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public WildcardRuleService()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _configPath = Path.Combine(baseDirectory, "wildcard_rules.json");
            LoadRules();
        }

        public List<WildcardRule> GetRules()
        {
            return _rules;
        }

        public void AddRule(WildcardRule rule)
        {
            if (!_rules.Any(r => r.Pattern.Equals(rule.Pattern, StringComparison.OrdinalIgnoreCase)))
            {
                _rules.Add(rule);
                SaveRules();
            }
        }

        public void RemoveRule(WildcardRule rule)
        {
            var ruleToRemove = _rules.FirstOrDefault(r => r.Pattern.Equals(rule.Pattern, StringComparison.OrdinalIgnoreCase));
            if (ruleToRemove != null)
            {
                _rules.Remove(ruleToRemove);
                SaveRules();
            }
        }

        private void LoadRules()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    string json = File.ReadAllText(_configPath);
                    _rules = JsonSerializer.Deserialize<List<WildcardRule>>(json, _jsonOptions) ?? new List<WildcardRule>();
                }
                else
                {
                    _rules = new List<WildcardRule>();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[ERROR] Failed to load wildcard rules: " + ex.Message);
                _rules = new List<WildcardRule>();
            }
        }

        private void SaveRules()
        {
            try
            {
                string json = JsonSerializer.Serialize(_rules, _jsonOptions);
                File.WriteAllText(_configPath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[ERROR] Failed to save wildcard rules: " + ex.Message);
            }
        }

        public WildcardRule Match(string path)
        {
            foreach (var rule in _rules)
            {
                string[] parts = rule.Pattern.Split(new[] { '*' }, 2);
                if (parts.Length == 2)
                {
                    string start = parts[0];
                    string end = parts[1];
                    if (path.StartsWith(start, StringComparison.OrdinalIgnoreCase) && path.EndsWith(end, StringComparison.OrdinalIgnoreCase))
                    {
                        return rule;
                    }
                }
            }
            return null;
        }
    }
}