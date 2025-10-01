// File: MainViewModel.cs
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
            _dataService.ClearAggregatedRulesCache();
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

        public void ApplyRulesFilters(string searchText, HashSet<RuleType> enabledTypes, int sortColumn, SortOrder sortOrder)
        {
            IEnumerable<AggregatedRuleViewModel> filteredRules = AllAggregatedRules;
            if (enabledTypes.Count > 0 && enabledTypes.Count < 5)
            {
                filteredRules = AllAggregatedRules.Where(r => enabledTypes.Contains(r.Type));
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
                2 => rule => rule.Status,
                3 => rule => rule.ProtocolName,
                4 => rule => rule.LocalPorts,
                5 => rule => rule.RemotePorts,
                6 => rule => rule.LocalAddresses,
                7 => rule => rule.RemoteAddresses,
                8 => rule => rule.ApplicationName,
                9 => rule => rule.ServiceName,
                10 => rule => rule.Profiles,
                11 => rule => rule.Grouping,
                12 => rule => rule.Description,
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

        private void OnRuleSetChanged()
        {
            ClearRulesCache();
            if (!_appSettings.AlertOnForeignRules)
            {
                return;
            }

            _activityLogger.LogDebug("Sentry: Firewall rule change detected. Debouncing for 1 second.");
            _sentryRefreshDebounceTimer?.Change(1000, Timeout.Infinite);
        }

        private async void DebouncedSentryRefresh(object? state)
        {
            _activityLogger.LogDebug("Sentry: Debounce timer elapsed. Checking for foreign rules.");
            await ScanForSystemChangesAsync(CancellationToken.None);
        }

        private void OnPendingConnectionDetected(PendingConnectionViewModel pending)
        {
            if (string.IsNullOrEmpty(pending.ServiceName))
            {
                try
                {
                    var process = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(pending.AppPath)).FirstOrDefault();
                    if (process != null)
                    {
                        pending.ServiceName = SystemDiscoveryService.GetServicesByPID(process.Id.ToString());
                    }
                }
                catch
                {
                }
            }

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
    }
}

