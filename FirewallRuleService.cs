using NetFwTypeLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace MinimalFirewall
{
    public class FirewallRuleService
    {
        private readonly INetFwPolicy2 _firewallPolicy;
        public FirewallRuleService()
        {
            try
            {
                Type firewallPolicyType = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
                if (firewallPolicyType != null)
                {
                    _firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(firewallPolicyType);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not initialize firewall policy: \n\n" + ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public List<INetFwRule2> GetAllRules()
        {
            if (_firewallPolicy == null) return new List<INetFwRule2>();
            return new List<INetFwRule2>(_firewallPolicy.Rules.Cast<INetFwRule2>());
        }

        public void SetDefaultOutboundAction(NET_FW_ACTION_ action)
        {
            if (_firewallPolicy == null) return;
            var profiles = new[] { NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_DOMAIN, NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PRIVATE, NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PUBLIC };
            foreach (var profile in profiles)
            {
                _firewallPolicy.DefaultOutboundAction[profile] = action;
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
                // This block now only logs the error to a file for the end-user.
                try
                {
                    string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash_log.txt");
                    string logContent = $"--- Minimal Firewall Crash Log ---\r\n" +
                                        $"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\r\n" +
                                        $"Source: FirewallRuleService.GetDefaultOutboundAction\r\n" +
                                        $"--- Exception Details ---\r\n" +
                                        $"{ex}\r\n\r\n";
                    File.AppendAllText(logPath, logContent);
                }
                catch
                {
                    // If logging fails, do nothing to avoid a crash loop.
                }
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
                try { _firewallPolicy.Rules.Remove(ruleName); } catch { /* Ignore errors */ }
            }
        }

        public void DeleteUwpRules(List<string> packageFamilyNames)
        {
            if (_firewallPolicy == null || packageFamilyNames.Count == 0) return;
            var pfnSet = new HashSet<string>(packageFamilyNames, StringComparer.OrdinalIgnoreCase);
            var rulesToRemove = new List<string>();
            foreach (INetFwRule2 rule in _firewallPolicy.Rules)
            {
                if (rule != null && !string.IsNullOrEmpty(rule.Description) && rule.Description.StartsWith("UWP App; PFN="))
                {
                    string pfnInRule = rule.Description.Substring("UWP App; PFN=".Length);
                    if (pfnSet.Contains(pfnInRule))
                    {
                        rulesToRemove.Add(rule.Name);
                    }
                }
            }

            foreach (var ruleName in rulesToRemove)
            {
                try { _firewallPolicy.Rules.Remove(ruleName); } catch { /* Ignore errors */ }
            }
        }

        public void DeleteRulesByName(List<string> ruleNames)
        {
            if (_firewallPolicy == null || ruleNames.Count == 0) return;
            foreach (var name in ruleNames)
            {
                try { _firewallPolicy.Rules.Remove(name); } catch { /* Ignore errors */ }
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
                MessageBox.Show("Failed to create rule. The firewall API rejected the input.\n\nError: " + ex.Message, "Rule Creation Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}