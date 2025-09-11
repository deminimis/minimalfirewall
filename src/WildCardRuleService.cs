// File: WildcardRuleService.cs
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace MinimalFirewall
{
    public class WildcardRuleService
    {
        private readonly string _configPath;
        private List<WildcardRule> _rules = [];

        public WildcardRuleService()
        {
            string baseDirectory = AppContext.BaseDirectory;
            _configPath = Path.Combine(baseDirectory, "wildcard_rules.json");
            LoadRules();
        }

        public List<WildcardRule> GetRules()
        {
            return _rules;
        }

        public void AddRule(WildcardRule rule)
        {
            if (!_rules.Any(r => r.FolderPath.Equals(rule.FolderPath, StringComparison.OrdinalIgnoreCase) && r.ExeName.Equals(rule.ExeName, StringComparison.OrdinalIgnoreCase)))
            {
                _rules.Add(rule);
                SaveRules();
            }
        }

        public void RemoveRule(WildcardRule rule)
        {
            var ruleToRemove = _rules.FirstOrDefault(r => r.FolderPath.Equals(rule.FolderPath, StringComparison.OrdinalIgnoreCase));
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
                    _rules = JsonSerializer.Deserialize(json, WildcardRuleJsonContext.Default.ListWildcardRule) ?? [];
                }
                else
                {
                    _rules = [];
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            {
                Debug.WriteLine("[ERROR] Failed to load wildcard rules: " + ex.Message);
                _rules = [];
            }
        }

        private void SaveRules()
        {
            try
            {
                string json = JsonSerializer.Serialize(_rules, WildcardRuleJsonContext.Default.ListWildcardRule);
                File.WriteAllText(_configPath, json);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Debug.WriteLine("[ERROR] Failed to save wildcard rules: " + ex.Message);
            }
        }

        public WildcardRule? Match(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            string normalizedPath = PathResolver.NormalizePath(path);
            string fileName = Path.GetFileName(normalizedPath);

            foreach (var rule in _rules)
            {
                string expandedFolderPath = PathResolver.NormalizePath(rule.FolderPath);

                if (normalizedPath.StartsWith(expandedFolderPath, StringComparison.OrdinalIgnoreCase))
                {
                    string exePattern = string.IsNullOrWhiteSpace(rule.ExeName) ? "*" : rule.ExeName.Trim();

                    if (exePattern == "*" || exePattern == "*.exe")
                    {
                        return rule;
                    }

                    if (exePattern.StartsWith("*") && exePattern.EndsWith("*"))
                    {
                        if (fileName.Contains(exePattern.Trim('*'), StringComparison.OrdinalIgnoreCase)) return rule;
                    }
                    else if (exePattern.StartsWith("*"))
                    {
                        if (fileName.EndsWith(exePattern.TrimStart('*'), StringComparison.OrdinalIgnoreCase)) return rule;
                    }
                    else if (exePattern.EndsWith("*"))
                    {
                        if (fileName.StartsWith(exePattern.TrimEnd('*'), StringComparison.OrdinalIgnoreCase)) return rule;
                    }
                    else
                    {
                        if (fileName.Equals(exePattern, StringComparison.OrdinalIgnoreCase)) return rule;
                    }
                }
            }
            return null;
        }
    }
}
