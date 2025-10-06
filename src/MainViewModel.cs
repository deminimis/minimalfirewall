using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using MinimalFirewall.TypedObjects;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using Firewall.Traffic.ViewModels;
using System.Diagnostics;
using System.IO;
namespace MinimalFirewall
{
    public class MainViewModel : ObservableViewModel
    {
        private readonly FirewallRuleService _firewallRuleService;
        private readonly WildcardRuleService _wildcardRuleService;
        private readonly BackgroundFirewallTaskService _backgroundTaskService;
        private readonly FirewallDataService _dataService;
        private readonly FirewallSentryService _firewallSentryService;
        private readonly ForeignRuleTracker _foreignRuleTracker;
        private readonly FirewallEventListenerService _eventListenerService;
        private readonly AppSettings _appSettings;
        private readonly UserActivityLogger _activityLogger;
        private System.Threading.Timer? _sentryRefreshDebounceTimer;

        public TrafficMonitorViewModel TrafficMonitorViewModel { get; }
        public ObservableCollection<PendingConnectionViewModel> PendingConnections { get; } = new();
        public List<AggregatedRuleViewModel> AllAggregatedRules { get; private set; } = [];
        public List<AggregatedRuleViewModel> VirtualRulesData { get; private set; } = [];
        public List<FirewallRuleChange> SystemChanges { get; private set; } = [];
        public int UnseenSystemChangesCount => SystemChanges.Count;
        public event Action? RulesListUpdated;
        public event Action? SystemChangesUpdated;
        public event Action<PendingConnectionViewModel>? PopupRequired;
        public event Action<PendingConnectionViewModel>? DashboardActionProcessed;
        public MainViewModel(
            FirewallRuleService firewallRuleService,
            WildcardRuleService wildcardRuleService,
            BackgroundFirewallTaskService backgroundTaskService,
            FirewallDataService dataService,
            FirewallSentryService firewallSentryService,
            ForeignRuleTracker foreignRuleTracker,
            TrafficMonitorViewModel trafficMonitorViewModel,
            FirewallEventListenerService eventListenerService,
            AppSettings appSettings,
            UserActivityLogger activityLogger)
        {
            _firewallRuleService = firewallRuleService;
            _wildcardRuleService = wildcardRuleService;
            _backgroundTaskService = backgroundTaskService;
            _dataService = dataService;
            _firewallSentryService = firewallSentryService;
            _foreignRuleTracker = foreignRuleTracker;
            TrafficMonitorViewModel = trafficMonitorViewModel;
            _eventListenerService = eventListenerService;
            _appSettings = appSettings;
            _activityLogger = activityLogger;

            _sentryRefreshDebounceTimer = new System.Threading.Timer(DebouncedSentryRefresh, null, Timeout.Infinite, Timeout.Infinite);

            _firewallSentryService.RuleSetChanged += OnRuleSetChanged;
            _eventListenerService.PendingConnectionDetected += OnPendingConnectionDetected;
        }

        public bool IsLockedDown => _firewallRuleService.GetDefaultOutboundAction() == NetFwTypeLib.NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
        public void ClearRulesCache()
        {
            _dataService.ClearCaches();
        }

        public void ClearRulesData()
        {
            ClearRulesCache();
            AllAggregatedRules.Clear();
            VirtualRulesData.Clear();
            RulesListUpdated?.Invoke();
        }

        public async Task RefreshRulesDataAsync(CancellationToken token, IProgress<int>? progress = null)
        {
            AllAggregatedRules = await _dataService.GetAggregatedRulesAsync(token, progress);
        }

        public void ApplyRulesFilters(string searchText, HashSet<RuleType> enabledTypes, int sortColumn, SortOrder sortOrder, bool showSystemRules)
        {
            IEnumerable<AggregatedRuleViewModel> filteredRules = AllAggregatedRules;
            if (!showSystemRules)
            {
                filteredRules = filteredRules.Where(r => r.Grouping.EndsWith(" - MFW"));
            }

            if (enabledTypes.Count > 0 && enabledTypes.Count < 5)
            {
                filteredRules = filteredRules.Where(r => enabledTypes.Contains(r.Type));
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filteredRules = filteredRules.Where(r =>
                    r.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    r.Description.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    r.ApplicationName.Contains(searchText, StringComparison.OrdinalIgnoreCase));
            }

            if (sortOrder != SortOrder.None && sortColumn != -1)
            {
                Func<AggregatedRuleViewModel, object> keySelector = GetRuleKeySelector(sortColumn);
                if (sortOrder == SortOrder.Ascending)
                {
                    filteredRules = filteredRules.OrderBy(keySelector);
                }
                else
                {
                    filteredRules = filteredRules.OrderByDescending(keySelector);
                }
            }

            VirtualRulesData = filteredRules.ToList();
            RulesListUpdated?.Invoke();
        }

        private Func<AggregatedRuleViewModel, object> GetRuleKeySelector(int columnIndex)
        {
            return columnIndex switch
            {
                2 => rule => rule.InboundStatus,
                3 => rule => rule.OutboundStatus,
                4 => rule => rule.ProtocolName,
                5 => rule => rule.LocalPorts,
                6 => rule => rule.RemotePorts,
                7 => rule => rule.LocalAddresses,
                8 => rule => rule.RemoteAddresses,
                9 => rule => rule.ApplicationName,
                10 => rule => rule.ServiceName,
                11 => rule => rule.Profiles,
                12 => rule => rule.Grouping,
                13 => rule => rule.Description,
                _ => rule => rule.Name,
            };
        }

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

        public void ProcessDashboardAction(PendingConnectionViewModel pending, string decision, bool trustPublisher = false)
        {
            var payload = new ProcessPendingConnectionPayload { PendingConnection = pending, Decision = decision, TrustPublisher = trustPublisher };
            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.ProcessPendingConnection, payload));
            PendingConnections.Remove(pending);

            if (decision == "Allow" || decision == "Block")
            {
                string action = $"{decision} ({pending.Direction})";
                FirewallActionsService.ParseActionString(action, out Actions parsedAction, out Directions parsedDirection);

                var newAggregatedRule = new AggregatedRuleViewModel
                {
                    Name = pending.FileName,
                    ApplicationName = pending.AppPath,
                    InboundStatus = parsedDirection == Directions.Incoming ? parsedAction.ToString() : "N/A",
                    OutboundStatus = parsedDirection == Directions.Outgoing ? parsedAction.ToString() : "N/A",
                    Type = RuleType.Program,
                    IsEnabled = true,
                    Grouping = MFWConstants.MainRuleGroup,
                    Profiles = "All",
                    ProtocolName = "Any"
                };
                AllAggregatedRules.Add(newAggregatedRule);
                ApplyRulesFilters(string.Empty, new HashSet<RuleType>(), -1, SortOrder.None, false);
            }

            DashboardActionProcessed?.Invoke(pending);
        }

        public void ProcessTemporaryDashboardAction(PendingConnectionViewModel pending, string decision, TimeSpan duration)
        {
            var payload = new ProcessPendingConnectionPayload { PendingConnection = pending, Decision = decision, Duration = duration };
            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.ProcessPendingConnection, payload));
            PendingConnections.Remove(pending);
            DashboardActionProcessed?.Invoke(pending);
        }

        public async Task ScanForSystemChangesAsync(CancellationToken token, IProgress<int>? progress = null)
        {
            var newChanges = await Task.Run(() => _firewallSentryService.CheckForChanges(_foreignRuleTracker, progress, token), token);
            if (token.IsCancellationRequested) return;

            SystemChanges.Clear();
            SystemChanges.AddRange(newChanges);
            SystemChangesUpdated?.Invoke();
        }

        public async Task RebuildBaselineAsync()
        {
            _foreignRuleTracker.Clear();
            await ScanForSystemChangesAsync(CancellationToken.None);
        }

        public void AcceptForeignRule(FirewallRuleChange change)
        {
            if (change.Rule?.Name is not null)
            {
                var payload = new ForeignRuleChangePayload { Change = change };
                _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.AcceptForeignRule, payload));
                SystemChanges.Remove(change);
                SystemChangesUpdated?.Invoke();
            }
        }

        public void DeleteForeignRule(FirewallRuleChange change)
        {
            if (change.Rule?.Name is not null)
            {
                var payload = new ForeignRuleChangePayload { Change = change };
                _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.DeleteForeignRule, payload));
                SystemChanges.Remove(change);
                SystemChangesUpdated?.Invoke();
            }
        }

        public void AcceptAllForeignRules()
        {
            if (SystemChanges.Count == 0) return;
            var payload = new AllForeignRuleChangesPayload { Changes = new List<FirewallRuleChange>(SystemChanges) };
            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.AcceptAllForeignRules, payload));
            SystemChanges.Clear();
            SystemChangesUpdated?.Invoke();
        }

        public void ApplyRuleChange(AggregatedRuleViewModel item, string action)
        {
            var firstRule = item.UnderlyingRules.FirstOrDefault();
            if (firstRule == null) return;

            switch (firstRule.Type)
            {
                case RuleType.Program:
                    var appPayload = new ApplyApplicationRulePayload { AppPaths = [firstRule.ApplicationName], Action = action };
                    _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.ApplyApplicationRule, appPayload));
                    break;
                case RuleType.Service:
                    var servicePayload = new ApplyServiceRulePayload { ServiceName = firstRule.ServiceName, Action = action };
                    _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.ApplyServiceRule, servicePayload));
                    break;
                case RuleType.UWP:
                    if (firstRule.Description.Contains(MFWConstants.UwpDescriptionPrefix))
                    {
                        var pfn = firstRule.Description.Replace(MFWConstants.UwpDescriptionPrefix, "");
                        var uwpApp = new UwpApp { Name = item.Name, PackageFamilyName = pfn };
                        var uwpPayload = new ApplyUwpRulePayload { UwpApps = [uwpApp], Action = action };
                        _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.ApplyUwpRule, uwpPayload));
                    }
                    break;
            }

            FirewallActionsService.ParseActionString(action, out Actions parsedAction, out Directions parsedDirection);
            var ruleToUpdate = AllAggregatedRules.FirstOrDefault(r => r == item);
            if (ruleToUpdate != null)
            {
                if (parsedDirection.HasFlag(Directions.Incoming)) ruleToUpdate.InboundStatus = parsedAction.ToString();
                if (parsedDirection.HasFlag(Directions.Outgoing)) ruleToUpdate.OutboundStatus = parsedAction.ToString();
            }
            RulesListUpdated?.Invoke();
        }

        public void DeleteRules(List<AggregatedRuleViewModel> rulesToDelete)
        {
            var wildcardRulesToDelete = rulesToDelete
                .Where(i => i.Type == RuleType.Wildcard && i.WildcardDefinition != null)
                .Select(i => i.WildcardDefinition!)
                .ToList();
            var standardRuleNamesToDelete = rulesToDelete
                .Where(i => i.Type != RuleType.Wildcard)
                .SelectMany(i => i.UnderlyingRules.Select(r => r.Name))
                .ToList();
            foreach (var wildcardRule in wildcardRulesToDelete)
            {
                var payload = new DeleteWildcardRulePayload { Wildcard = wildcardRule };
                _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.DeleteWildcardRules, payload));
                _wildcardRuleService.RemoveRule(wildcardRule);
            }

            if (standardRuleNamesToDelete.Any())
            {
                var payload = new DeleteRulesPayload { RuleIdentifiers = standardRuleNamesToDelete };
                _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.DeleteAdvancedRules, payload));
            }

            AllAggregatedRules.RemoveAll(rulesToDelete.Contains);
        }

        public AggregatedRuleViewModel CreateAggregatedRuleFromAdvancedRule(AdvancedRuleViewModel advancedRule)
        {
            return new AggregatedRuleViewModel
            {
                Name = advancedRule.Name,
                ApplicationName = advancedRule.ApplicationName,
                ServiceName = advancedRule.ServiceName,
                Description = advancedRule.Description,
                Grouping = advancedRule.Grouping,
                IsEnabled = advancedRule.IsEnabled,
                InboundStatus = advancedRule.Direction.HasFlag(Directions.Incoming) ? advancedRule.Status : "N/A",
                OutboundStatus = advancedRule.Direction.HasFlag(Directions.Outgoing) ? advancedRule.Status : "N/A",
                ProtocolName = advancedRule.ProtocolName,
                LocalPorts = advancedRule.LocalPorts,
                RemotePorts = advancedRule.RemotePorts,
                LocalAddresses = advancedRule.LocalAddresses,
                RemoteAddresses = advancedRule.RemoteAddresses,
                Profiles = advancedRule.Profiles,
                Type = advancedRule.Type,
                UnderlyingRules = new List<AdvancedRuleViewModel> { advancedRule }
            };
        }


        public void CreateAdvancedRule(AdvancedRuleViewModel vm, string interfaceTypes, string icmpTypesAndCodes)
        {
            var payload = new CreateAdvancedRulePayload { ViewModel = vm, InterfaceTypes = interfaceTypes, IcmpTypesAndCodes = icmpTypesAndCodes };
            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.CreateAdvancedRule, payload));

            var newAggregatedRule = new AggregatedRuleViewModel
            {
                Name = vm.Name,
                ApplicationName = vm.ApplicationName,
                ServiceName = vm.ServiceName,
                Description = vm.Description,
                Grouping = vm.Grouping,
                IsEnabled = vm.IsEnabled,
                InboundStatus = vm.Direction.HasFlag(Directions.Incoming) ? vm.Status : "N/A",
                OutboundStatus = vm.Direction.HasFlag(Directions.Outgoing) ? vm.Status : "N/A",
                ProtocolName = vm.ProtocolName,
                LocalPorts = vm.LocalPorts,
                RemotePorts = vm.RemotePorts,
                LocalAddresses = vm.LocalAddresses,
                RemoteAddresses = vm.RemoteAddresses,
                Profiles = vm.Profiles,
                Type = vm.Type,
                UnderlyingRules = [vm]
            };
            AllAggregatedRules.Add(newAggregatedRule);
            ApplyRulesFilters(string.Empty, new HashSet<RuleType>(), -1, SortOrder.None, false);
        }

        public void CreateProgramRule(string appPath, string action)
        {
            FirewallActionsService.ParseActionString(action, out Actions parsedAction, out Directions parsedDirection);
            var newAggregatedRule = new AggregatedRuleViewModel
            {
                Name = Path.GetFileName(appPath),
                ApplicationName = appPath,
                InboundStatus = parsedDirection.HasFlag(Directions.Incoming) ? parsedAction.ToString() : "N/A",
                OutboundStatus = parsedDirection.HasFlag(Directions.Outgoing) ? parsedAction.ToString() : "N/A",
                Type = RuleType.Program,
                IsEnabled = true,
                Grouping = MFWConstants.MainRuleGroup,
                Profiles = "All",
                ProtocolName = "Any",
                LocalPorts = "Any",
                RemotePorts = "Any",
                LocalAddresses = "Any",
                RemoteAddresses = "Any",
                Description = "N/A",
                ServiceName = "N/A"
            };
            AllAggregatedRules.Add(newAggregatedRule);
            ApplyRulesFilters(string.Empty, new HashSet<RuleType>(), -1, SortOrder.None, false);
            var payload = new ApplyApplicationRulePayload { AppPaths = { appPath }, Action = action };
            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.ApplyApplicationRule, payload));
        }

        private void OnRuleSetChanged()
        {
            ClearRulesCache();
            if (!_appSettings.AlertOnForeignRules)
            {
                return;
            }

            _sentryRefreshDebounceTimer?.Change(1000, Timeout.Infinite);
        }

        private async void DebouncedSentryRefresh(object? state)
        {
            _activityLogger.LogDebug("Sentry: Debounce timer elapsed. Checking for foreign rules.");
            await ScanForSystemChangesAsync(CancellationToken.None);
        }

        private void OnPendingConnectionDetected(PendingConnectionViewModel pending)
        {
            bool alreadyPending = PendingConnections.Any(p => p.AppPath.Equals(pending.AppPath, StringComparison.OrdinalIgnoreCase) && p.Direction.Equals(pending.Direction, StringComparison.OrdinalIgnoreCase));
            if (alreadyPending)
            {
                _activityLogger.LogDebug($"Ignoring duplicate pending connection for {pending.AppPath} (already in dashboard list)");
                return;
            }

            AddPendingConnection(pending);
            if (_appSettings.IsPopupsEnabled)
            {
                PopupRequired?.Invoke(pending);
            }
        }

        public void ProcessSpecificAllow(PendingConnectionViewModel pending)
        {
            var vm = new AdvancedRuleViewModel
            {
                Name = $"Allow {pending.FileName} - {pending.RemoteAddress}:{pending.RemotePort}",
                Description = "Granular rule created by Minimal Firewall popup.",
                IsEnabled = true,
                Grouping = MFWConstants.MainRuleGroup,
                Status = "Allow",
                Direction = pending.Direction.Equals("Incoming", StringComparison.OrdinalIgnoreCase) ? Directions.Incoming : Directions.Outgoing,
                Protocol = int.TryParse(pending.Protocol, out int proto) ? proto : 256,
                ApplicationName = pending.AppPath,
                RemotePorts = pending.RemotePort,
                RemoteAddresses = pending.RemoteAddress,
                LocalPorts = "*",
                LocalAddresses = "*",
                Profiles = "All",
                Type = RuleType.Advanced
            };
            var advPayload = new CreateAdvancedRulePayload { ViewModel = vm, InterfaceTypes = "All", IcmpTypesAndCodes = "" };
            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.CreateAdvancedRule, advPayload));
            var newAggregatedRule = new AggregatedRuleViewModel
            {
                Name = vm.Name,
                ApplicationName = vm.ApplicationName,
                Description = vm.Description,
                Grouping = vm.Grouping,
                IsEnabled = vm.IsEnabled,
                InboundStatus = vm.Direction.HasFlag(Directions.Incoming) ? vm.Status : "N/A",
                OutboundStatus = vm.Direction.HasFlag(Directions.Outgoing) ? vm.Status : "N/A",
                ProtocolName = vm.ProtocolName,
                LocalPorts = vm.LocalPorts,
                RemotePorts = vm.RemotePorts,
                LocalAddresses = vm.LocalAddresses,
                RemoteAddresses = vm.RemoteAddresses,
                Profiles = vm.Profiles,
                Type = vm.Type,
                UnderlyingRules = new List<AdvancedRuleViewModel> { vm }
            };
            AllAggregatedRules.Add(newAggregatedRule);
            ApplyRulesFilters(string.Empty, new HashSet<RuleType>(), -1, SortOrder.None, false);

            DashboardActionProcessed?.Invoke(pending);
        }
    }
}