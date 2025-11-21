using Firewall.Traffic.ViewModels;
using MinimalFirewall.TypedObjects;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System;
using DarkModeForms;

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
        private readonly FirewallActionsService _actionsService;
        private readonly FirewallSnapshotService _snapshotService;
        private readonly Dictionary<uint, (string Name, string Path, string ServiceName)> _processCache = [];

        private readonly System.Threading.Timer? _sentryRefreshDebounceTimer;
        public TrafficMonitorViewModel TrafficMonitorViewModel { get; }
        public ObservableCollection<PendingConnectionViewModel> PendingConnections { get; } = [];
        public List<AggregatedRuleViewModel> AllAggregatedRules { get; private set; } = [];
        public SortableBindingList<AggregatedRuleViewModel> VirtualRulesData { get; private set; } = new([]);
        public List<FirewallRuleChange> SystemChanges { get; private set; } = [];
        public int UnseenSystemChangesCount => SystemChanges.Count(c => c.Rule.IsEnabled);

        public event Action? RulesListUpdated;
        public event Action? SystemChangesUpdated;
        public event Action<PendingConnectionViewModel>? PopupRequired;
        public event Action<PendingConnectionViewModel>? DashboardActionProcessed;
        public event Action<string>? StatusTextChanged; 

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
            UserActivityLogger activityLogger,
            FirewallActionsService actionsService)
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
            _actionsService = actionsService;

            _snapshotService = new FirewallSnapshotService();
            _sentryRefreshDebounceTimer = new System.Threading.Timer(DebouncedSentryRefresh, null, Timeout.Infinite, Timeout.Infinite);

            _firewallSentryService.RuleSetChanged += OnRuleSetChanged;
            _eventListenerService.PendingConnectionDetected += OnPendingConnectionDetected;

            _backgroundTaskService.StatusChanged += (text) => StatusTextChanged?.Invoke(text);
        }

        public bool IsLockedDown => _firewallRuleService.GetDefaultOutboundAction() == NetFwTypeLib.NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
        public void ClearRulesCache()
        {
            _dataService.InvalidateRuleCache();
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

        public async Task RefreshLiveConnectionsAsync(CancellationToken token, IProgress<int>? progress = null)
        {
            var vms = await Task.Run(() =>
            {
                progress?.Report(0);

                var connections = Firewall.Traffic.TcpTrafficTracker.GetConnections().Distinct().ToList();

                if (token.IsCancellationRequested) return new List<TcpConnectionViewModel>();

                progress?.Report(20);

                var currentPids = connections.Select(c => c.ProcessId).Distinct().ToHashSet();

                foreach (var cachedPid in _processCache.Keys.ToList())
                {
                    if (!currentPids.Contains(cachedPid))
                    {
                        _processCache.Remove(cachedPid);
                    }
                }

                // Find new PIDs
                var pidsToResolve = currentPids.Where(pid => !_processCache.ContainsKey(pid)).ToList();
                int totalToResolve = pidsToResolve.Count > 0 ? pidsToResolve.Count : 1;
                int resolvedCount = 0;

                foreach (var pid in pidsToResolve)
                {
                    if (token.IsCancellationRequested) break;
                    (string Name, string Path, string ServiceName) info;
                    try
                    {
                        using var p = Process.GetProcessById((int)pid);
                        string name = p.ProcessName;
                        string path = string.Empty;
                        string serviceName = string.Empty;
                        try
                        {
                            if (p.MainModule != null) path = p.MainModule.FileName;
                        }
                        catch (Win32Exception) { path = "N/A (Access Denied)"; }
                        catch { }

                        if (name.Equals("svchost", StringComparison.OrdinalIgnoreCase))
                        {
                            serviceName = SystemDiscoveryService.GetServicesByPID(pid.ToString());
                        }
                        info = (name, path, serviceName);
                    }
                    catch (ArgumentException)
                    {
                        info = ("(Exited)", string.Empty, string.Empty);
                    }
                    catch
                    {
                        info = (pid == 4 ? "System" : "Unknown", string.Empty, string.Empty);
                    }

                    _processCache[pid] = info;
                    resolvedCount++;
                    int currentProgress = 20 + (resolvedCount * 60 / totalToResolve);
                    progress?.Report(currentProgress);
                }

                // Build ViewModels
                var viewModels = new List<TcpConnectionViewModel>(connections.Count);
                foreach (var conn in connections)
                {
                    if (!_processCache.TryGetValue(conn.ProcessId, out var info))
                    {
                        info = ($"PID: {conn.ProcessId}", string.Empty, string.Empty);
                    }

                    viewModels.Add(new TcpConnectionViewModel(conn, info, _backgroundTaskService));
                }

                progress?.Report(100);
                return viewModels;
            }, token);

            if (token.IsCancellationRequested) return;

            TrafficMonitorViewModel.ActiveConnections = new ObservableCollection<TcpConnectionViewModel>(vms);
        }

        public void ApplyRulesFilters(string searchText, HashSet<RuleType> enabledTypes, bool showSystemRules)
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

            VirtualRulesData = new SortableBindingList<AggregatedRuleViewModel>(filteredRules.ToList());
            RulesListUpdated?.Invoke();
        }

        private static Func<AggregatedRuleViewModel, object> GetRuleKeySelector(int columnIndex)
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
                    _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.ApplyApplicationRule, payload, "Applying wildcard match..."));
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
            _eventListenerService.SnoozeNotificationsForApp(pending.AppPath, TimeSpan.FromSeconds(5));

            var payload = new ProcessPendingConnectionPayload { PendingConnection = pending, Decision = decision, TrustPublisher = trustPublisher };
            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.ProcessPendingConnection, payload, $"Processing: {decision} {pending.FileName}"));
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
                ApplyRulesFilters(string.Empty, new HashSet<RuleType>(), false);
            }

            DashboardActionProcessed?.Invoke(pending);
        }

        public void ProcessTemporaryDashboardAction(PendingConnectionViewModel pending, string decision, TimeSpan duration)
        {
            var payload = new ProcessPendingConnectionPayload { PendingConnection = pending, Decision = decision, Duration = duration };
            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.ProcessPendingConnection, payload, $"Processing Temp: {pending.FileName}"));
            PendingConnections.Remove(pending);
            DashboardActionProcessed?.Invoke(pending);
        }

        public async Task ScanForSystemChangesAsync(CancellationToken token, IProgress<int>? progress = null)
        {
            var newChanges = await Task.Run(() => _firewallSentryService.CheckForChanges(_foreignRuleTracker, progress, token), token);
            if (token.IsCancellationRequested) return;

            var knownState = _snapshotService.LoadSnapshot();
            bool isFirstInitialization = knownState.Count == 0 && !_snapshotService.SnapshotExists();
            if (_appSettings.QuarantineMode)
            {
                foreach (var change in newChanges)
                {
                    bool isFresh = !knownState.Contains(change.Name);
                    if (change.Type == ChangeType.New && change.Rule.IsEnabled && isFresh && !isFirstInitialization)
                    {
                        var payload = new ForeignRuleChangePayload { Change = change };
                        _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.QuarantineForeignRule, payload, $"Quarantining: {change.Name}"));

                        change.Rule.IsEnabled = false;
                        change.Rule.Status = "Blocked (Pending Review)";
                    }
                    else if (change.Type == ChangeType.New && !change.Rule.IsEnabled)
                    {
                        change.Rule.Status = "Blocked (Pending Review)";
                    }
                }
            }

            var currentRuleNames = newChanges.Select(c => c.Name).ToList();
            _snapshotService.SaveSnapshot(currentRuleNames);

            SystemChanges.Clear();
            SystemChanges.AddRange(newChanges);
            SystemChangesUpdated?.Invoke();
        }

        public async Task RebuildBaselineAsync()
        {
            _foreignRuleTracker.Clear();
            _snapshotService.DeleteSnapshot();
            await ScanForSystemChangesAsync(CancellationToken.None);
        }

        public void AcceptForeignRule(FirewallRuleChange change)
        {
            if (change.Rule?.Name is not null)
            {
                var payload = new ForeignRuleChangePayload { Change = change };
                _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.AcceptForeignRule, payload, $"Archiving rule: {change.Name}"));
                SystemChanges.Remove(change);
                SystemChangesUpdated?.Invoke();
            }
        }

        public void EnableForeignRule(FirewallRuleChange change)
        {
            if (change.Rule?.Name is not null)
            {
                var payload = new ForeignRuleChangePayload { Change = change, Acknowledge = false };
                _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.EnableForeignRule, payload, $"Enabling rule: {change.Name}"));

                change.Rule.IsEnabled = true;
            }
        }

        public void DisableForeignRule(FirewallRuleChange change)
        {
            if (change.Rule?.Name is not null)
            {
                var payload = new ForeignRuleChangePayload { Change = change, Acknowledge = false };
                _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.DisableForeignRule, payload, $"Disabling rule: {change.Name}"));

                change.Rule.IsEnabled = false;
            }
        }

        public void DeleteForeignRule(FirewallRuleChange change)
        {
            if (change.Rule?.Name is not null)
            {
                var payload = new ForeignRuleChangePayload { Change = change };
                _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.DeleteForeignRule, payload, $"Deleting rule: {change.Name}"));
                SystemChanges.Remove(change);
                SystemChangesUpdated?.Invoke();
            }
        }

        public void AcceptAllForeignRules()
        {
            if (SystemChanges.Count == 0) return;
            var payload = new AllForeignRuleChangesPayload { Changes = new List<FirewallRuleChange>(SystemChanges) };
            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.AcceptAllForeignRules, payload, "Archiving all rules..."));
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
                    _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.ApplyApplicationRule, appPayload, $"Applying: {action} to {item.Name}"));
                    break;
                case RuleType.Service:
                    var servicePayload = new ApplyServiceRulePayload { ServiceName = firstRule.ServiceName, Action = action };
                    _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.ApplyServiceRule, servicePayload, $"Applying: {action} to Service {item.Name}"));
                    break;
                case RuleType.UWP:
                    if (firstRule.Description.Contains(MFWConstants.UwpDescriptionPrefix))
                    {
                        var pfn = firstRule.Description.Replace(MFWConstants.UwpDescriptionPrefix, "");
                        var uwpApp = new UwpApp { Name = item.Name, PackageFamilyName = pfn };
                        var uwpPayload = new ApplyUwpRulePayload { UwpApps = [uwpApp], Action = action };
                        _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.ApplyUwpRule, uwpPayload, $"Applying: {action} to UWP {item.Name}"));
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
                _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.DeleteWildcardRules, payload, "Deleting wildcard rule..."));
                _wildcardRuleService.RemoveRule(wildcardRule);
            }

            if (standardRuleNamesToDelete.Count > 0)
            {
                var payload = new DeleteRulesPayload { RuleIdentifiers = standardRuleNamesToDelete };
                _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.DeleteAdvancedRules, payload, $"Deleting {standardRuleNamesToDelete.Count} rules..."));
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
                UnderlyingRules = [advancedRule]
            };
        }


        public void CreateAdvancedRule(AdvancedRuleViewModel vm, string interfaceTypes, string icmpTypesAndCodes)
        {
            var payload = new CreateAdvancedRulePayload { ViewModel = vm, InterfaceTypes = interfaceTypes, IcmpTypesAndCodes = icmpTypesAndCodes };
            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.CreateAdvancedRule, payload, $"Creating rule: {vm.Name}"));

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
            ApplyRulesFilters(string.Empty, new HashSet<RuleType>(), false);
        }

        public void CreateProgramRule(string appPath, string action)
        {
            if (!string.IsNullOrEmpty(appPath))
            {
                _eventListenerService.SnoozeNotificationsForApp(appPath, TimeSpan.FromSeconds(5));
            }

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
            ApplyRulesFilters(string.Empty, new HashSet<RuleType>(), false);
            var payload = new ApplyApplicationRulePayload { AppPaths = [appPath], Action = action };
            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.ApplyApplicationRule, payload, $"Applying {action} to {Path.GetFileName(appPath)}"));
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
            _eventListenerService.SnoozeNotificationsForApp(pending.AppPath, TimeSpan.FromSeconds(5));

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
            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.CreateAdvancedRule, advPayload, $"Creating rule for {pending.FileName}"));
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
                UnderlyingRules = [vm]
            };
            AllAggregatedRules.Add(newAggregatedRule);
            ApplyRulesFilters(string.Empty, new HashSet<RuleType>(), false);

            DashboardActionProcessed?.Invoke(pending);
        }

        public void CreateWildcardRule(PendingConnectionViewModel pending, WildcardRule newRule)
        {
            _eventListenerService.SnoozeNotificationsForApp(pending.AppPath, TimeSpan.FromSeconds(5));

            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.AddWildcardRule, newRule));

            string decision = newRule.Action.StartsWith("Block", StringComparison.OrdinalIgnoreCase) ? "Block" : "Allow";
            var allowPayload = new ProcessPendingConnectionPayload
            {
                PendingConnection = pending,
                Decision = decision,
                Duration = default,
                TrustPublisher = false
            };
            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.ProcessPendingConnection, allowPayload));

            PendingConnections.Remove(pending);
        }

        public async Task CleanUpOrphanedRulesAsync()
        {
            using var statusForm = new StatusForm("Scanning for orphaned rules...", _appSettings);
            statusForm.Show(Form.ActiveForm);
            var progress = new Progress<int>(p => statusForm.UpdateProgress(p));
            var cts = new CancellationTokenSource();
            statusForm.FormClosing += (s, e) =>
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    cts.Cancel();
                }
            };
            try
            {
                var deletedRules = await _actionsService.CleanUpOrphanedRulesAsync(cts.Token, progress);
                if (!cts.IsCancellationRequested)
                {
                    statusForm.Close();
                    Messenger.MessageBox(
                        $"{deletedRules.Count} orphaned rule(s) found and deleted.",
                        "Cleanup Complete",
                        MessageBoxButtons.OK,
                        MsgIcon.None);
                    ClearRulesCache();
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _activityLogger.LogException("CleanUpOrphanedRulesAsync", ex);
                if (!statusForm.IsDisposed)
                {
                    statusForm.Close();
                }
                Messenger.MessageBox(
                    "An error occurred during the cleanup process. Please check the debug log for details.",
                    "Cleanup Error",
                    MessageBoxButtons.OK,
                    MsgIcon.Error);
            }
        }
    }
}