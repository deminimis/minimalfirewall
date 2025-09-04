// FirewallRuleService.cs
using NetFwTypeLib;
using System.Diagnostics;

namespace MinimalFirewall
{
    public class FirewallRuleService
    {
        private readonly INetFwPolicy2 _firewallPolicy;

        public FirewallRuleService(INetFwPolicy2 firewallPolicy)
        {
            _firewallPolicy = firewallPolicy;
        }

        public List<INetFwRule2> GetApplicationRules()
        {
            if (_firewallPolicy == null) return [];
            var appRules = new List<INetFwRule2>();
            foreach (INetFwRule2 rule in _firewallPolicy.Rules)
            {
                if (rule != null && !string.IsNullOrEmpty(rule.Grouping) &&
                   (rule.Grouping.EndsWith(MFWConstants.MfwRuleSuffix) ||
                    rule.Grouping == "Minimal Firewall" ||
                    rule.Grouping == "Minimal Firewall (Wildcard)"))
                {
                    appRules.Add(rule);
                }
            }
            return appRules;
        }

        public List<INetFwRule2> GetAllRules()
        {
            if (_firewallPolicy == null) return [];
            return new List<INetFwRule2>(_firewallPolicy.Rules.Cast<INetFwRule2>());
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
                catch (Exception ex)
                {
                    // optional: if you have a logger, call it here
                    System.Diagnostics.Debug.WriteLine($"Failed to set outbound action for {profile}: {ex.HResult:X8} {ex.Message}");
                }
            }
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
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting default outbound action: {ex.Message}");
            }

            return NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
        }

        public void DeleteRulesByPath(List<string> appPaths)
        {
            if (_firewallPolicy == null || appPaths.Count == 0) return;
            var pathSet = new HashSet<string>(appPaths, StringComparer.OrdinalIgnoreCase);
            var rulesToRemove = _firewallPolicy.Rules.Cast<INetFwRule>()
                .Where(r => r != null && !string.IsNullOrEmpty(r.ApplicationName) && pathSet.Contains(r.ApplicationName))
                .Select(r => r.Name)
                .ToList();
            foreach (var ruleName in rulesToRemove)
            {
                try { _firewallPolicy.Rules.Remove(ruleName); } catch { }
            }
        }

        public void DeleteRuleByPathAndDirection(string appPath, NET_FW_RULE_DIRECTION_ direction)
        {
            if (_firewallPolicy == null || string.IsNullOrEmpty(appPath)) return;
            var rulesToRemove = _firewallPolicy.Rules.Cast<INetFwRule>()
                .Where(r => r != null &&
                            string.Equals(r.ApplicationName, appPath, StringComparison.OrdinalIgnoreCase) &&
                            r.Direction == direction)
                 .Select(r => r.Name)
                .ToList();
            foreach (var ruleName in rulesToRemove)
            {
                try { _firewallPolicy.Rules.Remove(ruleName); } catch { }
            }
        }

        public void DeleteRulesByServiceName(string serviceName)
        {
            if (_firewallPolicy == null || string.IsNullOrEmpty(serviceName)) return;
            var rulesToRemove = _firewallPolicy.Rules.Cast<INetFwRule2>()
                .Where(r => r != null && string.Equals(r.serviceName, serviceName, StringComparison.OrdinalIgnoreCase))
                .Select(r => r.Name)
                .ToList();
            foreach (var ruleName in rulesToRemove)
            {
                try { _firewallPolicy.Rules.Remove(ruleName); } catch { }
            }
        }

        public void DeleteUwpRules(List<string> packageFamilyNames)
        {
            if (_firewallPolicy == null || packageFamilyNames.Count == 0) return;
            var pfnSet = new HashSet<string>(packageFamilyNames, StringComparer.OrdinalIgnoreCase);
            var rulesToRemove = new List<string>();
            foreach (INetFwRule2 rule in _firewallPolicy.Rules)
            {
                if (rule?.Description?.StartsWith(MFWConstants.UwpDescriptionPrefix, StringComparison.Ordinal) == true)
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
                try { _firewallPolicy.Rules.Remove(ruleName); } catch { }
            }
        }

        public void DeleteRulesByName(List<string> ruleNames)
        {
            if (_firewallPolicy == null || ruleNames.Count == 0) return;
            foreach (var name in ruleNames)
            {
                try { _firewallPolicy.Rules.Remove(name); } catch { }
            }
        }

        public void CreateRule(INetFwRule2 rule)
        {
            try
            {
                _firewallPolicy?.Rules.Add(rule);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create rule. The firewall API rejected the input.\n\nError: " + ex.Message, "Rule Creation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void DeleteRulesByDescription(string description)
        {
            if (_firewallPolicy == null || string.IsNullOrEmpty(description)) return;
            var rulesToRemove = _firewallPolicy.Rules.Cast<INetFwRule>()
                .Where(r => r != null && string.Equals(r.Description, description, StringComparison.OrdinalIgnoreCase))
                .Select(r => r.Name)
                .ToList();
            foreach (var ruleName in rulesToRemove)
            {
                try { _firewallPolicy.Rules.Remove(ruleName); } catch { }
            }
        }

        public void DeleteRulesByGroup(string groupName)
        {
            if (_firewallPolicy == null || string.IsNullOrEmpty(groupName)) return;
            var rulesToRemove = _firewallPolicy.Rules.Cast<INetFwRule>()
                .Where(r => r != null && string.Equals(r.Grouping, groupName, StringComparison.OrdinalIgnoreCase))
                .Select(r => r.Name)
                .ToList();
            foreach (var ruleName in rulesToRemove)
            {
                try { _firewallPolicy.Rules.Remove(ruleName); } catch { }
            }
        }
    }
}