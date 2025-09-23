// File: MainViewModel.cs
using System.Collections.ObjectModel;
using System.Linq;

namespace MinimalFirewall
{
    public class MainViewModel : ObservableViewModel
    {
        private readonly FirewallRuleService _firewallRuleService;
        private readonly WildcardRuleService _wildcardRuleService;
        private readonly BackgroundFirewallTaskService _backgroundTaskService;

        public ObservableCollection<PendingConnectionViewModel> PendingConnections { get; } = new();

        public MainViewModel(FirewallRuleService firewallRuleService, WildcardRuleService wildcardRuleService, BackgroundFirewallTaskService backgroundTaskService)
        {
            _firewallRuleService = firewallRuleService;
            _wildcardRuleService = wildcardRuleService;
            _backgroundTaskService = backgroundTaskService;
        }

        public bool IsLockedDown => _firewallRuleService.GetDefaultOutboundAction() == NetFwTypeLib.NET_FW_ACTION_.NET_FW_ACTION_BLOCK;

        public void AddPendingConnection(PendingConnectionViewModel pending)
        {
            var matchingRule = _wildcardRuleService.Match(pending.AppPath);
            if (matchingRule != null)
            {
                if (matchingRule.Action.StartsWith("Allow", StringComparison.OrdinalIgnoreCase))
                {
                    var payload = new ApplyApplicationRulePayload
                    {
                        AppPaths = [pending.AppPath],
                        Action = matchingRule.Action,
                        WildcardSourcePath = matchingRule.FolderPath
                    };
                    _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.ApplyApplicationRule, payload));
                    return;
                }
                if (matchingRule.Action.StartsWith("Block", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            bool alreadyPending = PendingConnections.Any(p => p.AppPath.Equals(pending.AppPath, StringComparison.OrdinalIgnoreCase));

            if (!alreadyPending)
            {
                PendingConnections.Add(pending);
            }
        }

        public void ProcessDashboardAction(PendingConnectionViewModel pending, string decision)
        {
            var payload = new ProcessPendingConnectionPayload { PendingConnection = pending, Decision = decision };
            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.ProcessPendingConnection, payload));
            PendingConnections.Remove(pending);
        }
    }
}