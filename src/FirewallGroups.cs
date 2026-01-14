using NetFwTypeLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;

namespace MinimalFirewall.Groups
{
    public class FirewallGroup : INotifyPropertyChanged
    {
        public string Name { get; }
        public int RuleCount { get; }

        // Store the actual COM objects so we can modify them later
        private readonly List<INetFwRule2> _rules;
        private bool _isEnabled;

        public event PropertyChangedEventHandler? PropertyChanged;

        public FirewallGroup(string name, List<INetFwRule2> groupRules)
        {
            Name = name;
            // Transfer ownership of the list to this class
            _rules = groupRules;
            RuleCount = _rules.Count;

            _isEnabled = _rules.Count > 0 && _rules.All(r => r.Enabled);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    try
                    {
                        foreach (var rule in _rules)
                        {
                            rule.Enabled = value;
                        }

                        _isEnabled = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEnabled)));
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to toggle rules: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        public void SetEnabledState(bool isEnabled)
        {
            IsEnabled = isEnabled;
        }
    }

    public class FirewallGroupManager
    {
        public FirewallGroupManager() { }

        private INetFwPolicy2 GetFirewallPolicy()
        {
            try
            {
                Type? policyType = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
                if (policyType == null)
                {
                    throw new InvalidOperationException("Firewall policy type could not be retrieved.");
                }
                return (INetFwPolicy2)Activator.CreateInstance(policyType)!;
            }
            catch (COMException ex)
            {
                throw new InvalidOperationException("Failed to create firewall policy instance.", ex);
            }
        }

        public List<FirewallGroup> GetAllGroups()
        {
            var groupsData = new Dictionary<string, List<INetFwRule2>>(StringComparer.OrdinalIgnoreCase);
            INetFwPolicy2? policy = null;
            INetFwRules? comRules = null;

            try
            {
                policy = GetFirewallPolicy();
                comRules = policy.Rules;

                foreach (INetFwRule2 rule in comRules)
                {
                    string grouping = rule.Grouping;

                    if (!string.IsNullOrEmpty(grouping) && grouping.EndsWith(MFWConstants.MfwRuleSuffix))
                    {
                        if (!groupsData.TryGetValue(grouping, out var ruleList))
                        {
                            ruleList = new List<INetFwRule2>();
                            groupsData[grouping] = ruleList;
                        }
                        ruleList.Add(rule);
                    }
                    else
                    {
                        Marshal.ReleaseComObject(rule);
                    }
                }

                var list = new List<FirewallGroup>(groupsData.Count);
                foreach (var groupKey in groupsData.Keys.OrderBy(k => k))
                {
                    list.Add(new FirewallGroup(groupKey, groupsData[groupKey]));
                }

                return list;
            }
            finally
            {
                if (comRules != null) Marshal.ReleaseComObject(comRules);
                if (policy != null) Marshal.ReleaseComObject(policy);
            }
        }
    }
}