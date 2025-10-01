// File: FirewallGroups.cs
using NetFwTypeLib;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
namespace MinimalFirewall.Groups
{
    public class FirewallGroup : INotifyPropertyChanged
    {
        private readonly INetFwPolicy2 _firewallPolicy;

        public string Name { get; }
        public int RuleCount { get; }

        public event PropertyChangedEventHandler? PropertyChanged;
        public FirewallGroup(string name, List<INetFwRule2> groupRules, INetFwPolicy2 firewallPolicy)
        {
            Name = name;
            _firewallPolicy = firewallPolicy;
            RuleCount = groupRules.Count;
            IsEnabled = groupRules.Count > 0 && groupRules.All(r => r.Enabled);
        }

        public bool IsEnabled { get; private set; }

        public void SetEnabledState(bool isEnabled)
        {
            if (IsEnabled != isEnabled)
            {
                IsEnabled = isEnabled;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEnabled)));
            }
        }
    }

    public class FirewallGroupManager
    {
        private readonly INetFwPolicy2 _policy;
        public FirewallGroupManager(INetFwPolicy2 policy) => _policy = policy;

        public List<FirewallGroup> GetAllGroups()
        {
            var groupsData = new Dictionary<string, List<INetFwRule2>>(System.StringComparer.OrdinalIgnoreCase);
            INetFwRules? comRules = null;
            try
            {
                comRules = _policy.Rules;
                foreach (INetFwRule2 rule in comRules)
                {
                    if (rule?.Grouping is { Length: > 0 } && rule.Grouping.EndsWith(MFWConstants.MfwRuleSuffix))
                    {
                        if (!groupsData.TryGetValue(rule.Grouping, out var ruleList))
                        {
                            ruleList = new List<INetFwRule2>();
                            groupsData[rule.Grouping] = ruleList;
                        }
                        ruleList.Add(rule);
                    }
                    else
                    {
                        if (rule != null) Marshal.ReleaseComObject(rule);
                    }
                }
            }
            finally
            {
                if (comRules != null) Marshal.ReleaseComObject(comRules);
            }

            var list = new List<FirewallGroup>(groupsData.Count);
            foreach (var group in groupsData)
            {
                list.Add(new FirewallGroup(group.Key, group.Value, _policy));
            }

            foreach (var ruleList in groupsData.Values)
            {
                foreach (var rule in ruleList)
                {
                    Marshal.ReleaseComObject(rule);
                }
            }

            return list.OrderBy(g => g.Name).ToList();
        }
    }
}