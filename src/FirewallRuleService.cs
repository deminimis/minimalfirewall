// File: FirewallRuleService.cs
using NetFwTypeLib;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MinimalFirewall
{
    public class FirewallRuleService
    {
        private readonly INetFwPolicy2 _firewallPolicy;

        public FirewallRuleService(INetFwPolicy2 firewallPolicy)
        {
            _firewallPolicy = firewallPolicy;
        }

        public List<INetFwRule2> GetAllRules()
        {
            if (_firewallPolicy?.Rules == null) return [];

            try
            {
                return _firewallPolicy.Rules.Cast<INetFwRule2>().ToList();
            }
            catch (COMException ex)
            {
                Debug.WriteLine($"Failed to retrieve all firewall rules: {ex.Message}");
                return [];
            }
        }

        public INetFwRule2? GetRuleByName(string name)
        {
            if (_firewallPolicy == null) return null;
            try
            {
                return _firewallPolicy.Rules.Item(name) as INetFwRule2;
            }
            catch
            {
                return null;
            }
        }

        public void SetDefaultOutboundAction(NET_FW_ACTION_ action)
        {
            foreach (NET_FW_PROFILE_TYPE2_ profile in new[]
            {
                NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_DOMAIN,
                NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PRIVATE,
                NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PUBLIC
            })
            {
                try
                {
                    _firewallPolicy.set_DefaultOutboundAction(profile, action);
                }
                catch (COMException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to set outbound action for {profile}: {ex.HResult:X8} {ex.Message}");
                }
            }
        }

        public List<string> GetRuleNamesByPathAndDirection(string appPath, NET_FW_RULE_DIRECTION_ direction)
        {
            if (_firewallPolicy == null || string.IsNullOrEmpty(appPath)) return [];
            string normalizedAppPath = PathResolver.NormalizePath(appPath);
            var names = new List<string>();
            var allRules = GetAllRules();
            foreach (var rule in allRules)
            {
                if (rule != null &&
                    !string.IsNullOrEmpty(rule.ApplicationName) &&
                    string.Equals(PathResolver.NormalizePath(rule.ApplicationName), normalizedAppPath, StringComparison.OrdinalIgnoreCase) &&
                    rule.Direction == direction)
                {
                    names.Add(rule.Name);
                }
            }
            return names;
        }

        public NET_FW_ACTION_ GetDefaultOutboundAction()
        {
            if (_firewallPolicy == null) return NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
            try
            {
                var currentProfileTypes = (NET_FW_PROFILE_TYPE2_)_firewallPolicy.CurrentProfileTypes;
                if ((currentProfileTypes & NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PUBLIC) != 0)
                {
                    return _firewallPolicy.DefaultOutboundAction[NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PUBLIC];
                }
                if ((currentProfileTypes & NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PRIVATE) != 0)
                {
                    return _firewallPolicy.DefaultOutboundAction[NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PRIVATE];
                }
                if ((currentProfileTypes & NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_DOMAIN) != 0)
                {
                    return _firewallPolicy.DefaultOutboundAction[NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_DOMAIN];
                }
            }
            catch (COMException ex)
            {
                Debug.WriteLine($"Error getting default outbound action: {ex.Message}");
            }

            return NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
        }

        public List<string> DeleteRulesByPath(List<string> appPaths)
        {
            if (_firewallPolicy == null || appPaths.Count == 0) return [];
            var pathSet = new HashSet<string>(appPaths.Select(PathResolver.NormalizePath), StringComparer.OrdinalIgnoreCase);
            var rulesToRemove = new List<string>();
            var allRules = GetAllRules();
            foreach (var rule in allRules)
            {
                if (rule != null && !string.IsNullOrEmpty(rule.ApplicationName) && pathSet.Contains(PathResolver.NormalizePath(rule.ApplicationName)))
                {
                    rulesToRemove.Add(rule.Name);
                }
            }

            foreach (var ruleName in rulesToRemove)
            {
                try { _firewallPolicy.Rules.Remove(ruleName); } catch (Exception ex) when (ex is COMException or FileNotFoundException) { Debug.WriteLine($"[ERROR] Failed to remove rule '{ruleName}': {ex.Message}"); }
            }
            return rulesToRemove;
        }

        public List<string> DeleteRulesByServiceName(string serviceName)
        {
            if (_firewallPolicy == null || string.IsNullOrEmpty(serviceName)) return [];
            var rulesToRemove = new List<string>();
            var allRules = GetAllRules();
            foreach (var rule in allRules)
            {
                if (rule is INetFwRule2 rule2 && rule2 != null && string.Equals(rule2.serviceName, serviceName, StringComparison.OrdinalIgnoreCase))
                {
                    rulesToRemove.Add(rule2.Name);
                }
            }
            foreach (var ruleName in rulesToRemove)
            {
                try { _firewallPolicy.Rules.Remove(ruleName); } catch (Exception ex) when (ex is COMException or FileNotFoundException) { Debug.WriteLine($"[ERROR] Failed to remove rule '{ruleName}': {ex.Message}"); }
            }
            return rulesToRemove;
        }

        public List<string> DeleteUwpRules(List<string> packageFamilyNames)
        {
            if (_firewallPolicy == null || packageFamilyNames.Count == 0) return [];
            var pfnSet = new HashSet<string>(packageFamilyNames, StringComparer.OrdinalIgnoreCase);
            var rulesToRemove = new List<string>();
            var allRules = GetAllRules();
            foreach (var rule in allRules)
            {
                if (rule != null && rule.Description?.StartsWith(MFWConstants.UwpDescriptionPrefix, StringComparison.Ordinal) == true)
                {
                    string pfnInRule = rule.Description[MFWConstants.UwpDescriptionPrefix.Length..];
                    if (pfnSet.Contains(pfnInRule))
                    {
                        rulesToRemove.Add(rule.Name);
                    }
                }
            }
            foreach (var ruleName in rulesToRemove)
            {
                try { _firewallPolicy.Rules.Remove(ruleName); } catch (Exception ex) when (ex is COMException or FileNotFoundException) { Debug.WriteLine($"[ERROR] Failed to remove rule '{ruleName}': {ex.Message}"); }
            }
            return rulesToRemove;
        }

        public void DeleteRulesByName(List<string> ruleNames)
        {
            if (_firewallPolicy == null || ruleNames.Count == 0) return;
            foreach (var name in ruleNames)
            {
                try { _firewallPolicy.Rules.Remove(name); } catch (Exception ex) when (ex is COMException or FileNotFoundException) { Debug.WriteLine($"[ERROR] Failed to remove rule '{name}': {ex.Message}"); }
            }
        }

        public void CreateRule(INetFwRule2 rule)
        {
            try
            {
                _firewallPolicy?.Rules.Add(rule);
            }
            catch (COMException ex)
            {
                MessageBox.Show("Failed to create rule. The firewall API rejected the input.\n\nError: " + ex.Message, "Rule Creation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public List<string> DeleteRulesByDescription(string description)
        {
            if (_firewallPolicy == null || string.IsNullOrEmpty(description)) return [];
            var rulesToRemove = new List<string>();
            var allRules = GetAllRules();
            foreach (var rule in allRules)
            {
                if (rule != null && string.Equals(rule.Description, description, StringComparison.OrdinalIgnoreCase))
                {
                    rulesToRemove.Add(rule.Name);
                }
            }

            foreach (var ruleName in rulesToRemove)
            {
                try { _firewallPolicy.Rules.Remove(ruleName); } catch (Exception ex) when (ex is COMException or FileNotFoundException) { Debug.WriteLine($"[ERROR] Failed to remove rule '{ruleName}': {ex.Message}"); }
            }
            return rulesToRemove;
        }

        public List<string> DeleteRulesByGroup(string groupName)
        {
            if (_firewallPolicy == null || string.IsNullOrEmpty(groupName)) return [];
            var rulesToRemove = new List<string>();
            var allRules = GetAllRules();
            foreach (var rule in allRules)
            {
                if (rule != null && string.Equals(rule.Grouping, groupName, StringComparison.OrdinalIgnoreCase))
                {
                    rulesToRemove.Add(rule.Name);
                }
            }

            foreach (var ruleName in rulesToRemove)
            {
                try { _firewallPolicy.Rules.Remove(ruleName); } catch (Exception ex) when (ex is COMException or FileNotFoundException) { Debug.WriteLine($"[ERROR] Failed to remove rule '{ruleName}': {ex.Message}"); }
            }
            return rulesToRemove;
        }

        public void DeleteAllMfwRules()
        {
            if (_firewallPolicy == null) return;
            var rulesToRemove = new List<string>();
            var allRules = GetAllRules();
            foreach (var rule in allRules)
            {
                if (rule != null && !string.IsNullOrEmpty(rule.Grouping) && rule.Grouping.Contains("MFW"))
                {
                    rulesToRemove.Add(rule.Name);
                }
            }

            foreach (var ruleName in rulesToRemove)
            {
                try
                {
                    _firewallPolicy.Rules.Remove(ruleName);
                }
                catch (Exception ex) when (ex is COMException or FileNotFoundException)
                {
                    Debug.WriteLine($"[ERROR] Failed to remove rule '{ruleName}': {ex.Message}");
                }
            }
        }
    }
}
