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
        private bool _isEnabled;

        public string Name { get; }
        public int RuleCount { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public FirewallGroup(string name, INetFwPolicy2 firewallPolicy)
        {
            Name = name;
            _firewallPolicy = firewallPolicy;

            var rules = new List<INetFwRule2>();
            INetFwRules? comRules = null;
            try
            {
                comRules = _firewallPolicy.Rules;
                foreach (INetFwRule2 r in comRules)
                {
                    try
                    {
                        if (r != null && string.Equals(r.Grouping, Name, System.StringComparison.OrdinalIgnoreCase))
                        {
                            rules.Add(r);
                        }
                        else if (r != null)
                        {
                            Marshal.ReleaseComObject(r);
                        }
                    }
                    catch
                    {
                        if (r != null) Marshal.ReleaseComObject(r);
                        throw;
                    }
                }
                RuleCount = rules.Count;
                _isEnabled = rules.Count > 0 && rules.All(r => r.Enabled);
            }
            finally
            {
                foreach (var rule in rules)
                {
                    Marshal.ReleaseComObject(rule);
                }
                if (comRules != null) Marshal.ReleaseComObject(comRules);
            }
        }

        public bool IsEnabled
        {
            get
            {
                var rules = new List<INetFwRule2>();
                INetFwRules? comRules = null;
                try
                {
                    comRules = _firewallPolicy.Rules;
                    foreach (INetFwRule2 r in comRules)
                    {
                        try
                        {
                            if (r != null && string.Equals(r.Grouping, Name, System.StringComparison.OrdinalIgnoreCase))
                            {
                                rules.Add(r);
                            }
                            else if (r != null)
                            {
                                Marshal.ReleaseComObject(r);
                            }
                        }
                        catch
                        {
                            if (r != null) Marshal.ReleaseComObject(r);
                            throw;
                        }
                    }
                    if (rules.Count == 0) return false;
                    return rules.All(r => r.Enabled);
                }
                finally
                {
                    foreach (var rule in rules)
                    {
                        Marshal.ReleaseComObject(rule);
                    }
                    if (comRules != null) Marshal.ReleaseComObject(comRules);
                }
            }
            set
            {
                INetFwRules? comRules = null;
                try
                {
                    comRules = _firewallPolicy.Rules;
                    foreach (INetFwRule2 r in comRules)
                    {
                        try
                        {
                            if (r != null && !string.IsNullOrEmpty(r.Grouping) && string.Equals(r.Grouping, Name, System.StringComparison.OrdinalIgnoreCase))
                            {
                                r.Enabled = value;
                            }
                        }
                        finally
                        {
                            if (r != null) Marshal.ReleaseComObject(r);
                        }
                    }
                }
                finally
                {
                    if (comRules != null) Marshal.ReleaseComObject(comRules);
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
            var groups = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);
            INetFwRules? comRules = null;
            try
            {
                comRules = _policy.Rules;
                foreach (INetFwRule2 rule in comRules)
                {
                    try
                    {
                        if (rule?.Grouping is { Length: > 0 } && rule.Grouping.EndsWith(MFWConstants.MfwRuleSuffix))
                        {
                            groups[rule.Grouping] = groups.TryGetValue(rule.Grouping, out var c) ? c + 1 : 1;
                        }
                    }
                    finally
                    {
                        if (rule != null) Marshal.ReleaseComObject(rule);
                    }
                }
            }
            finally
            {
                if (comRules != null) Marshal.ReleaseComObject(comRules);
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