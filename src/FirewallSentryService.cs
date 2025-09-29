// File: C:/Users/anon/PROGRAMMING/C#/SimpleFirewall/VS Minimal Firewall/MinimalFirewall-NET8/MinimalFirewall-WindowsStore/FirewallSentryService.cs
// File: FirewallSentryService.cs
using System.Management;
using System.Runtime.InteropServices;

namespace MinimalFirewall
{
    public partial class FirewallSentryService : IDisposable
    {
        private readonly FirewallRuleService firewallService;
        private ManagementEventWatcher? _watcher;
        private bool _isStarted = false;
        public event Action? RuleSetChanged;

        public FirewallSentryService(FirewallRuleService firewallService)
        {
            this.firewallService = firewallService;
        }

        public void Start()
        {
            if (_isStarted)
            {
                return;
            }

            try
            {
                var scope = new ManagementScope(@"root\StandardCimv2");
                var query = new WqlEventQuery(
                    "SELECT * FROM __InstanceOperationEvent WITHIN 1 " +
                    "WHERE TargetInstance ISA 'MSFT_NetFirewallRule'");
                _watcher = new ManagementEventWatcher(scope, query);
                _watcher.EventArrived += OnFirewallRuleChangeEvent;
                _watcher.Start();
                _isStarted = true;
            }
            catch (ManagementException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SENTRY ERROR] Failed to start WMI watcher: {ex.Message}");
            }
        }

        public void Stop()
        {
            if (!_isStarted)
            {
                return;
            }

            try
            {
                _watcher?.Stop();
                _watcher?.Dispose();
                _watcher = null;
                _isStarted = false;
            }
            catch (ManagementException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SENTRY ERROR] Failed to stop WMI watcher: {ex.Message}");
            }
        }

        private void OnFirewallRuleChangeEvent(object sender, EventArrivedEventArgs e)
        {
            RuleSetChanged?.Invoke();
        }

        public List<FirewallRuleChange> CheckForChanges(ForeignRuleTracker acknowledgedTracker)
        {
            var changes = new List<FirewallRuleChange>();
            var allRules = firewallService.GetAllRules();
            try
            {
                foreach (var rule in allRules)
                {
                    if (rule == null || string.IsNullOrEmpty(rule.Name)) continue;

                    if (IsMfwRule(rule) || acknowledgedTracker.IsAcknowledged(rule.Name))
                    {
                        continue;
                    }

                    changes.Add(new FirewallRuleChange { Type = ChangeType.New, Rule = FirewallDataService.CreateAdvancedRuleViewModel(rule) });
                }
            }
            finally
            {
                foreach (var rule in allRules)
                {
                    if (rule != null) Marshal.ReleaseComObject(rule);
                }
            }
            return changes;
        }

        private static bool IsMfwRule(NetFwTypeLib.INetFwRule2 rule)
        {
            if (string.IsNullOrEmpty(rule.Grouping)) return false;
            return rule.Grouping.EndsWith(MFWConstants.MfwRuleSuffix) ||
                   rule.Grouping == "Minimal Firewall" ||
                   rule.Grouping == "Minimal Firewall (Wildcard)";
        }

        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }
    }
}