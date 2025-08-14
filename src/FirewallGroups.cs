// FirewallGroups.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using NetFwTypeLib;

namespace MinimalFirewall.Groups
{
    public class FirewallGroup : INotifyPropertyChanged
    {
        private readonly INetFwPolicy2 _firewallPolicy;
        public string Name { get; }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled == value) return;
                _isEnabled = value;
                try
                {
                    _firewallPolicy.EnableRuleGroup((int)NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_ALL, this.Name, value);
                    OnPropertyChanged();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating group '{this.Name}': {ex.Message}");
                }
            }
        }

        public FirewallGroup(string name, INetFwPolicy2 firewallPolicy)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            this.Name = name;
            this._firewallPolicy = firewallPolicy;
            this._isEnabled = _firewallPolicy.IsRuleGroupEnabled((int)NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_ALL, this.Name);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class FirewallGroupManager
    {
        private readonly INetFwPolicy2 _firewallPolicy;
        public FirewallGroupManager(INetFwPolicy2 firewallPolicy)
        {
            _firewallPolicy = firewallPolicy;
        }

        public List<FirewallGroup> GetAllGroups()
        {
            var groupNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (INetFwRule2 rule in _firewallPolicy.Rules)
            {
                if (rule != null && !string.IsNullOrEmpty(rule.Grouping) && rule.Grouping.EndsWith(MFWConstants.MfwRuleSuffix))
                {
                    groupNames.Add(rule.Grouping);
                }
            }

            return groupNames
                .OrderBy(name => name)
                .Select(groupName => new FirewallGroup(groupName, _firewallPolicy))
                .ToList();
        }

        public FirewallGroup CreateNewGroup(string name)
        {
            return new FirewallGroup(name, _firewallPolicy);
        }
    }
}