// File: FirewallGroups.cs
using NetFwTypeLib;
using System.ComponentModel;
using System.Linq;
namespace MinimalFirewall.Groups
{
    public class FirewallGroup : INotifyPropertyChanged
    {
        private readonly INetFwPolicy2 _firewallPolicy;
        private bool _isEnabled;

        public string Name { get; }
        public int RuleCount { get; }

        public event PropertyChangedEventHandler? PropertyChanged;
        public FirewallGroup(string name, INetFwPolicy2 firewallPolicy)
        {
            Name = name;
            _firewallPolicy = firewallPolicy;
            var rules = _firewallPolicy.Rules.Cast<INetFwRule2>().Where(r => r != null && string.Equals(r.Grouping, Name, System.StringComparison.OrdinalIgnoreCase)).ToList();
            RuleCount = rules.Count;
            _isEnabled = rules.Count > 0 && rules.All(r => r.Enabled);
        }

        public bool IsEnabled
        {
            get
            {
                var rules = _firewallPolicy.Rules.Cast<INetFwRule2>().Where(r => r != null && string.Equals(r.Grouping, Name, System.StringComparison.OrdinalIgnoreCase)).ToList();
                if (rules.Count == 0) return false;
                return rules.All(r => r.Enabled);
            }
            set
            {
                foreach (INetFwRule2 r in _firewallPolicy.Rules)
                {
                    if (r != null && !string.IsNullOrEmpty(r.Grouping) && string.Equals(r.Grouping, Name, System.StringComparison.OrdinalIgnoreCase))
                    {
                        r.Enabled = value;
                    }
                }
                _isEnabled = value;
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
            var mfwRules = _policy.Rules.Cast<INetFwRule2>()
                .Where(r => r?.Grouping is { Length: > 0 } && r.Grouping.EndsWith(MFWConstants.MfwRuleSuffix));
            var groups = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);
            foreach (INetFwRule2 rule in mfwRules)
            {
                groups[rule.Grouping] = groups.TryGetValue(rule.Grouping, out var c) ?
                    c + 1 : 1;
            }

            var list = new List<FirewallGroup>(groups.Count);
            foreach (var kv in groups)
            {
                list.Add(new FirewallGroup(kv.Key, _policy));
            }
            return list.OrderBy(g => g.Name).ToList();
        }
    }
}