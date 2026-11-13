using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;

namespace MinimalFirewall
{
    public class WildcardRuleService : IDisposable
    {
        private readonly string _configPath;
        private List<WildcardRule> _rules = [];

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        private Dictionary<WildcardRule, (Regex? FolderRegex, Regex? ExeRegex)> _regexCache = [];

        public WildcardRuleService()
        {
            _configPath = ConfigPathManager.GetConfigPath("wildcard_rules.json");
            LoadRules();
        }

        public List<WildcardRule> GetRules()
        {
            _lock.EnterReadLock();
            try
            {
                return new List<WildcardRule>(_rules);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void AddRule(WildcardRule rule)
        {
            _lock.EnterWriteLock();
            try
            {
                // Check for duplicates
                if (!_rules.Any(r => r.FolderPath.Equals(rule.FolderPath, StringComparison.OrdinalIgnoreCase) &&
                                     r.ExeName.Equals(rule.ExeName, StringComparison.OrdinalIgnoreCase)))
                {
                    _rules.Add(rule);
                    UpdateCacheForRule(rule); 
                    SaveRules();
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void UpdateRule(WildcardRule oldRule, WildcardRule newRule)
        {
            _lock.EnterWriteLock();
            try
            {
                var ruleToRemove = _rules.FirstOrDefault(r =>
                    r.FolderPath.Equals(oldRule.FolderPath, StringComparison.OrdinalIgnoreCase) &&
                    r.ExeName.Equals(oldRule.ExeName, StringComparison.OrdinalIgnoreCase) &&
                    r.Action.Equals(oldRule.Action, StringComparison.OrdinalIgnoreCase) &&
                    r.Protocol == oldRule.Protocol &&
                    r.LocalPorts.Equals(oldRule.LocalPorts, StringComparison.OrdinalIgnoreCase) &&
                    r.RemotePorts.Equals(oldRule.RemotePorts, StringComparison.OrdinalIgnoreCase) &&
                    r.RemoteAddresses.Equals(oldRule.RemoteAddresses, StringComparison.OrdinalIgnoreCase));

                if (ruleToRemove != null)
                {
                    _rules.Remove(ruleToRemove);
                    _regexCache.Remove(ruleToRemove);
                }

                if (!_rules.Any(r => r.FolderPath.Equals(newRule.FolderPath, StringComparison.OrdinalIgnoreCase) &&
                                     r.ExeName.Equals(newRule.ExeName, StringComparison.OrdinalIgnoreCase)))
                {
                    _rules.Add(newRule);
                    UpdateCacheForRule(newRule);
                }
                SaveRules();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void RemoveRule(WildcardRule rule)
        {
            _lock.EnterWriteLock();
            try
            {
                var ruleToRemove = _rules.FirstOrDefault(r =>
                    r.FolderPath.Equals(rule.FolderPath, StringComparison.OrdinalIgnoreCase) &&
                    r.ExeName.Equals(rule.ExeName, StringComparison.OrdinalIgnoreCase) &&
                    r.Action.Equals(rule.Action, StringComparison.OrdinalIgnoreCase) &&
                    r.Protocol == rule.Protocol &&
                    r.LocalPorts.Equals(rule.LocalPorts, StringComparison.OrdinalIgnoreCase) &&
                    r.RemotePorts.Equals(rule.RemotePorts, StringComparison.OrdinalIgnoreCase) &&
                    r.RemoteAddresses.Equals(rule.RemoteAddresses, StringComparison.OrdinalIgnoreCase));

                if (ruleToRemove != null)
                {
                    _rules.Remove(ruleToRemove);
                    _regexCache.Remove(ruleToRemove); // Remove from cache
                    SaveRules();
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void ClearRules()
        {
            _lock.EnterWriteLock();
            try
            {
                _rules.Clear();
                _regexCache.Clear();
                SaveRules();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private void LoadRules()
        {
            _lock.EnterWriteLock();
            try
            {
                if (File.Exists(_configPath))
                {
                    string json = File.ReadAllText(_configPath);
                    var loadedRules = JsonSerializer.Deserialize(json, WildcardRuleJsonContext.Default.ListWildcardRule);
                    _rules = loadedRules ?? [];
                }
                else
                {
                    _rules = [];
                }

                RebuildCache();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            {
                Debug.WriteLine("[ERROR] Failed to load wildcard rules: " + ex.Message);
                _rules = [];
                _regexCache.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
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


        // Converts wildcard strings (e.g. "path\to\*" or "*.exe") into compiled Regex objects.
        private void RebuildCache()
        {
            _regexCache.Clear();
            foreach (var rule in _rules)
            {
                UpdateCacheForRule(rule);
            }
        }

        private void UpdateCacheForRule(WildcardRule rule)
        {
            Regex? folderRegex = null;
            Regex? exeRegex = null;

            // Compile Folder Regex if it contains wildcards
            string expandedFolderPath = PathResolver.NormalizePath(rule.FolderPath);
            if (expandedFolderPath.Contains('*') || expandedFolderPath.Contains('?'))
            {
                try
                {
                    string pattern = "^" + Regex.Escape(expandedFolderPath)
                                           .Replace("\\*", ".*")
                                           .Replace("\\?", ".") + "$";


                    folderRegex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Wildcard Cache] Invalid folder regex for {rule.FolderPath}: {ex.Message}");
                }
            }

            try
            {
                string exePattern = string.IsNullOrWhiteSpace(rule.ExeName) ? "*" : rule.ExeName.Trim();
                string regexPattern = "^" + Regex.Escape(exePattern)
                                       .Replace("\\*", ".*")
                                       .Replace("\\?", ".") + "$";

                exeRegex = new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Wildcard Cache] Invalid exe regex for {rule.ExeName}: {ex.Message}");
            }

            _regexCache[rule] = (folderRegex, exeRegex);
        }

        // Version folders (app-1.0.9015, 120.0.6099.71) change on every update, so a wildcard on them stops
        // matching. Suggest the stable parent instead, but never a drive root or a shared system folder,
        // where a wildcard would cover unrelated programs.
        private static readonly Regex VersionFolder = new(@"\d+[._-]\d+", RegexOptions.CultureInvariant);
        private static readonly string[] BroadFolders =
        [
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            Environment.GetFolderPath(Environment.SpecialFolder.Windows),
        ];

        public static string? SuggestFolder(string exePath)
        {
            string? folder = Path.GetDirectoryName(exePath);
            for (int level = 0; level < 3 && folder != null && VersionFolder.IsMatch(Path.GetFileName(folder)); level++)
            {
                string? parent = Path.GetDirectoryName(folder);
                if (parent == null || Path.GetPathRoot(parent) == parent
                    || BroadFolders.Any(broad => broad.Length > 0 && parent.TrimEnd('\\').Equals(broad.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase)))
                {
                    break;
                }
                folder = parent;
            }
            return folder;
        }

        // A folder prefix only matches on a separator boundary: "C:\Tools" must not match "C:\ToolsEvil\x.exe".
        public static bool IsInFolder(string path, string folder)
        {
            string root = folder.TrimEnd('\\', '/');
            return root.Length > 0
                && path.Length > root.Length
                && path.StartsWith(root, StringComparison.OrdinalIgnoreCase)
                && (path[root.Length] == '\\' || path[root.Length] == '/');
        }

        public WildcardRule? Match(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            string normalizedPath = PathResolver.NormalizePath(path);
            string fileName = Path.GetFileName(normalizedPath);

            _lock.EnterReadLock();
            try
            {
                foreach (var rule in _rules)
                {
                    if (!_regexCache.TryGetValue(rule, out var regexes))
                    {
                        continue;
                    }

                    string expandedFolderPath = PathResolver.NormalizePath(rule.FolderPath);
                    bool isFolderMatch = false;

                    if (regexes.FolderRegex != null)
                    {
                        // Use the cached Regex for folder wildcards
                        string? directoryName = Path.GetDirectoryName(normalizedPath);
                        if (directoryName != null && regexes.FolderRegex.IsMatch(directoryName))
                        {
                            isFolderMatch = true;
                        }
                        // Fallback: Check if the regex was meant to match the full path prefix
                        else if (regexes.FolderRegex.IsMatch(normalizedPath))
                        {
                            isFolderMatch = true;
                        }
                    }
                    else
                    {
                        if (IsInFolder(normalizedPath, expandedFolderPath))
                        {
                            isFolderMatch = true;
                        }
                    }

                    if (isFolderMatch)
                    {
                        if (regexes.ExeRegex != null)
                        {
                            if (regexes.ExeRegex.IsMatch(fileName))
                            {
                                return rule;
                            }
                        }
                        else
                        {
                            if (rule.ExeName == "*" || rule.ExeName == "*.exe")
                            {
                                return rule;
                            }
                        }
                    }
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }

            return null;
        }

        public void Dispose()
        {
            _lock?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}