using NetFwTypeLib;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
namespace MinimalFirewall.Groups
{
    public class FirewallGroup : INotifyPropertyChanged
    {
        public string Name { get; }
        public int RuleCount { get; }

        public event PropertyChangedEventHandler? PropertyChanged;
        public FirewallGroup(string name, List<INetFwRule2> groupRules)
        {
            Name = name;
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
            var groupsData = new Dictionary<string, List<INetFwRule2>>(System.StringComparer.OrdinalIgnoreCase);
            INetFwPolicy2? policy = null;
            INetFwRules? comRules = null;
            try
            {
                policy = GetFirewallPolicy();
                comRules = policy.Rules;
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
                if (policy != null) Marshal.ReleaseComObject(policy);
            }

            var list = new List<FirewallGroup>(groupsData.Count);
            foreach (var group in groupsData)
            {
                list.Add(new FirewallGroup(group.Key, group.Value));
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

