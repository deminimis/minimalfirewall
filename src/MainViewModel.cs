using DarkModeForms;
using Firewall.Traffic.ViewModels;
using Microsoft.VisualBasic.ApplicationServices;
using MinimalFirewall.TypedObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MinimalFirewall
{
    public class MainViewModel : ObservableViewModel, IDisposable
    {
        private readonly FirewallRuleService _firewallRuleService;
        private readonly WildcardRuleService _wildcardRuleService;
        private readonly BackgroundFirewallTaskService _backgroundTaskService;
        private readonly FirewallDataService _dataService;
        private readonly FirewallSentryService _firewallSentryService;
        private readonly FirewallEventListenerService _eventListenerService;
        private readonly AppSettings _appSettings;
        private readonly UserActivityLogger _activityLogger;
        private readonly FirewallActionsService _actionsService;
        private readonly FirewallSnapshotService _snapshotService;

        private readonly ConcurrentDictionary<uint, (string Name, string Path, string ServiceName)> _processCache = new();

        private readonly System.Threading.Timer? _sentryRefreshDebounceTimer;
        private readonly System.Threading.SynchronizationContext? _uiContext;

        public TrafficMonitorViewModel TrafficMonitorViewModel { get; }
        public ObservableCollection<PendingConnectionViewModel> PendingConnections { get; } = [];
        public List<AggregatedRuleViewModel> AllAggregatedRules { get; private set; } = [];
        public SortableBindingList<AggregatedRuleViewModel> VirtualRulesData { get; private set; } = new([]);
        public List<FirewallRuleChange> SystemChanges { get; private set; } = [];
        public int UnseenSystemChangesCount => SystemChanges.Count(c => c.Rule.IsEnabled);

        public event Action? RulesListUpdated;
        public event Action? SystemChangesUpdated;
        public event Action<PendingConnectionViewModel>? PopupRequired;
        public event Action<FirewallRuleChange>? RulePopupRequired;
        public event Action<PendingConnectionViewModel>? DashboardActionProcessed;
        public event Action<string>? StatusTextChanged;

        public MainViewModel(
            FirewallRuleService firewallRuleService,
            WildcardRuleService wildcardRuleService,
            BackgroundFirewallTaskService backgroundTaskService,
            FirewallDataService dataService,
            FirewallSentryService firewallSentryService,
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
            TrafficMonitorViewModel = trafficMonitorViewModel;
            _eventListenerService = eventListenerService;
            _appSettings = appSettings;
            _activityLogger = activityLogger;
            _actionsService = actionsService;

            _snapshotService = new FirewallSnapshotService();
            _sentryRefreshDebounceTimer = new System.Threading.Timer(DebouncedSentryRefresh, null, Timeout.Infinite, Timeout.Infinite);
            _uiContext = System.Threading.SynchronizationContext.Current;

            _firewallSentryService.RuleSetChanged += OnRuleSetChanged;
            _eventListenerService.PendingConnectionDetected += OnPendingConnectionDetected;

            _backgroundTaskService.StatusChanged += OnBackgroundStatusChanged;

            DebouncedSentryRefresh(null);
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

                if (token.IsCancellationRequested)
                {
                    return [];
                }

                progress?.Report(20);

                var currentPids = connections.Select(c => c.ProcessId).Distinct().ToHashSet();

                // Cleanup old PIDs from cache
                foreach (var cachedPid in _processCache.Keys.ToArray())
                {
                    if (!currentPids.Contains(cachedPid))
                    {
                        _processCache.TryRemove(cachedPid, out _);
                    }
                }

                // Resolve new PIDs
                var pidsToResolve = currentPids.Where(pid => !_processCache.ContainsKey(pid)).ToList();
                int totalToResolve = pidsToResolve.Count > 0 ? pidsToResolve.Count : 1;
                int resolvedCount = 0;

                foreach (var pid in pidsToResolve)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    var info = ResolveProcessInfo(pid);
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

            if (token.IsCancellationRequested)
            {
                return;
            }

            TrafficMonitorViewModel.ActiveConnections = new ObservableCollection<TcpConnectionViewModel>(vms);
        }

        private (string Name, string Path, string ServiceName) ResolveProcessInfo(uint pid)
        {
            try
            {
                if (pid == 0)
                {
                    return ("System Idle", string.Empty, string.Empty);
                }

                if (pid == 4)
                {
                    return ("System", string.Empty, string.Empty);
                }

                using var p = Process.GetProcessById((int)pid);
                string name = p.ProcessName;
                string path = string.Empty;
                string serviceName = string.Empty;

                try
                {
                    if (p.MainModule != null)
                    {
                        path = p.MainModule.FileName;
                    }
                }
                catch (Win32Exception) { path = "N/A (Access Denied)"; }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[WARN] ResolveProcessInfo failed to read main module for PID {pid}: {ex.Message}");
                }

                if (name.Equals("svchost", StringComparison.OrdinalIgnoreCase))
                {
                    serviceName = SystemDiscoveryService.GetServicesByPID(pid.ToString());
                }
                return (name, path, serviceName);
            }
            catch (ArgumentException)
            {
                return ("(Exited)", string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[WARN] ResolveProcessInfo failed for PID {pid}: {ex.Message}");
                return ("Unknown", string.Empty, string.Empty);
            }
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

        private void AddRuleAndRefresh(AggregatedRuleViewModel newRule)
        {
            newRule.DateAdded ??= DateTime.UtcNow;
            AllAggregatedRules.Add(newRule);
            ApplyRulesFilters(string.Empty, [], false);
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
                14 => rule => rule.DateAdded ?? DateTime.MinValue,
                15 => rule => rule.AutoAllowedPublisher,
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

            // Release the lock 
            _eventListenerService.ClearPendingNotification(pending.AppPath, pending.Direction);

            var payload = new ProcessPendingConnectionPayload { PendingConnection = pending, Decision = decision, TrustPublisher = trustPublisher };
            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.ProcessPendingConnection, payload, $"Processing: {decision} {pending.FileName}"));
            PendingConnections.Remove(pending);

            if (decision == "Allow" || decision == "Block")
            {
                string action = $"{decision} ({pending.Direction})";
                FirewallActionsService.ParseActionString(action, out Actions parsedAction, out Directions parsedDirection);

                // Force svchost rules without services to be granular
                if (pending.FileName.Equals("svchost.exe", StringComparison.OrdinalIgnoreCase))
                {
                    var granularRule = new AdvancedRuleViewModel
                    {
                        Name = $"MFW - svchost.exe ({pending.ServiceName}) - {pending.RemoteAddress}:{pending.RemotePort}",
                        Description = $"Granular {decision} rule for svchost.exe to prevent blanket OS breakage.",
                        IsEnabled = true,
                        Grouping = MFWConstants.MainRuleGroup,
                        Status = decision,
                        Direction = parsedDirection,
                        Protocol = int.TryParse(pending.Protocol, out int proto) ? proto : 256,
                        ApplicationName = pending.AppPath,
                        ServiceName = string.IsNullOrEmpty(pending.ServiceName) ? "*" : pending.ServiceName,
                        RemotePorts = string.IsNullOrEmpty(pending.RemotePort) ? "*" : pending.RemotePort,
                        RemoteAddresses = string.IsNullOrEmpty(pending.RemoteAddress) ? "*" : pending.RemoteAddress,
                        LocalPorts = "*",
                        LocalAddresses = "*",
                        Profiles = "All",
                        Type = RuleType.Advanced,
                        InterfaceTypes = "All"
                    };
                    CreateAdvancedRule(granularRule, "All", "");
                    return; // Exit early so we dont' make blanket rule
                }

                var newAggregatedRule = CreateStandardProgramRule(pending.FileName, pending.AppPath, parsedDirection, parsedAction);
                AddRuleAndRefresh(newAggregatedRule);
            }

            DashboardActionProcessed?.Invoke(pending);
        }

        public void ToggleLockdownMode()
        {
            _actionsService.ToggleLockdown();
            if (IsLockedDown)
            {
                _eventListenerService.EnableAuditing();
                _eventListenerService.Start();
            }
            else
            {
                _eventListenerService.Stop();

                // Clear pending notifications when unlocked
                if (_uiContext != null)
                {
                    _uiContext.Post(_ => PendingConnections.Clear(), null);
                }
                else
                {
                    PendingConnections.Clear();
                }
            }
        }

        public void ProcessTemporaryDashboardAction(PendingConnectionViewModel pending, string decision, TimeSpan duration)
        {
            _eventListenerService.SnoozeNotificationsForApp(pending.AppPath, duration);
            _eventListenerService.ClearPendingNotification(pending.AppPath, pending.Direction);

            var payload = new ProcessPendingConnectionPayload { PendingConnection = pending, Decision = decision, Duration = duration };
            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.ProcessPendingConnection, payload, $"Processing Temp: {pending.FileName}"));
            PendingConnections.Remove(pending);
            DashboardActionProcessed?.Invoke(pending);
        }

        public async Task ScanForSystemChangesAsync(CancellationToken token, IProgress<int>? progress = null)
        {
            var incrementalChanges = await Task.Run(() => _firewallSentryService.CheckForChanges(progress, token), token);
            if (token.IsCancellationRequested)
            {
                return;
            }

            if (incrementalChanges.Count == 0)
            {
                return; // Nothing to update
            }

            var knownState = _snapshotService.LoadSnapshot();
            bool isFirstInitialization = knownState.Count == 0 && !_snapshotService.SnapshotExists();

            foreach (var change in incrementalChanges)
            {
                bool isFresh = change.Name != null && !knownState.Contains(change.Name);

                // Helper to identify Microsoft/OS rules 
                bool isWindowsRule = (change.Grouping?.StartsWith("@") == true) ||
                                     (change.Name?.StartsWith("@") == true) ||
                                     (change.Name?.StartsWith("ms-resource:", StringComparison.OrdinalIgnoreCase) == true) ||
                                     string.Equals(change.ApplicationName, "System", StringComparison.OrdinalIgnoreCase) ||
                                     (change.Publisher?.Contains("Microsoft", StringComparison.OrdinalIgnoreCase) == true) ||
                                     (change.ApplicationName?.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.Windows), StringComparison.OrdinalIgnoreCase) == true);

                if (change.Type == ChangeType.New && isFresh && !isFirstInitialization)
                {
                    if (isWindowsRule)
                    {
                        bool isAllowRule = string.Equals(change.Rule.Status, "Allow", StringComparison.OrdinalIgnoreCase);

                        // log rules
                        if (change.Rule.IsEnabled)
                        {
                            change.Intervention = isAllowRule ? "OS Allowed Rule (Retained)" : "OS Block Rule (Retained)";
                        }
                        else
                        {
                            change.Intervention = "Disabled (OS)";
                        }
                    }
                    else
                    {
                        // It's a Third-Party Rule
                        if (change.Rule.IsEnabled)
                        {
                            var payload = new ForeignRuleChangePayload { Change = change, Acknowledge = false };
                            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.DisableForeignRule, payload, $"Blocking Third-Party Rule: {change.Name}"));
                            change.Rule.IsEnabled = false;
                            change.Intervention = "Blocked (Pending Review)";
                        }
                        else if (!change.Rule.IsEnabled)
                        {
                            change.Intervention = "Blocked (Pending Review)";
                        }

                        // Trigger popup
                        RulePopupRequired?.Invoke(change);
                    }
                }
                else if (change.Type == ChangeType.New)
                {
                    // Restore historical intervention context on app restart
                    if (isWindowsRule)
                    {
                        bool isAllowRule = string.Equals(change.Rule.Status, "Allow", StringComparison.OrdinalIgnoreCase);

                        // We no longer auto-disable any OS rules (Allow or Block).
                        // Just retain and log them to prevent breaking OS features/VPNs.
                        if (change.Rule.IsEnabled)
                        {
                            change.Intervention = isAllowRule ? "OS Allowed Rule (Retained)" : "OS Block Rule (Retained)";
                        }
                        else
                        {
                            change.Intervention = "Disabled (OS)";
                        }
                    }
                    else
                    {
                        change.Intervention = change.Rule.IsEnabled ? "User Allowed" : "User Blocked";
                    }
                }
            } // <--- THIS WAS THE MISSING BRACKET! It closes the FIRST foreach loop safely.

            // Update ui list
            foreach (var change in incrementalChanges)
            {
                if (change.Type == ChangeType.Deleted)
                {
                    SystemChanges.RemoveAll(c => c.Name == change.Name);
                }
                else
                {
                    // Preserve previous Intervention state if it's a modification
                    if (change.Type == ChangeType.Modified)
                    {
                        var existingRule = SystemChanges.FirstOrDefault(c => c.Name == change.Name);
                        if (existingRule != null && string.IsNullOrEmpty(change.Intervention))
                        {
                            change.Intervention = existingRule.Intervention;
                        }
                    }

                    // Add updated object
                    SystemChanges.RemoveAll(c => c.Name == change.Name);
                    SystemChanges.Add(change);
                }
            }

            var currentRuleNames = SystemChanges.Select(c => c.Name).ToList();
            _snapshotService.SaveSnapshot(currentRuleNames);

            SystemChangesUpdated?.Invoke();
        }

        public async Task RebuildBaselineAsync()
        {
            _snapshotService.DeleteSnapshot();
            _firewallSentryService.ResetBaseline();
            SystemChanges.Clear();
            await ScanForSystemChangesAsync(CancellationToken.None);
        }

        public void EnableForeignRule(FirewallRuleChange change)
        {
            if (change.Rule?.Name is not null)
            {
                var payload = new ForeignRuleChangePayload { Change = change, Acknowledge = false };
                _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.EnableForeignRule, payload, $"Enabling rule: {change.Name}"));

                change.Rule.IsEnabled = true;
                change.Intervention = "User Allowed (Popup)";
                SystemChangesUpdated?.Invoke();
            }
        }

        public void DisableForeignRule(FirewallRuleChange change)
        {
            if (change.Rule?.Name is not null)
            {
                var payload = new ForeignRuleChangePayload { Change = change, Acknowledge = false };
                _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.DisableForeignRule, payload, $"Disabling rule: {change.Name}"));

                change.Rule.IsEnabled = false;
                change.Intervention = "User Blocked (Popup)";
                SystemChangesUpdated?.Invoke();
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

        public void ApplyRuleChange(AggregatedRuleViewModel item, string action)
        {
            var firstRule = item.UnderlyingRules.FirstOrDefault();
            if (firstRule == null)
            {
                return;
            }

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
                    string pfn = string.Empty;

                    // Check if it's MFW-created UWP rule 
                    if (firstRule.Description != null && firstRule.Description.Contains(MFWConstants.UwpDescriptionPrefix))
                    {
                        pfn = firstRule.Description.Replace(MFWConstants.UwpDescriptionPrefix, "");
                    }
                    else
                    {
                        // Match from the cached UWP apps list by Name
                        var uwpApps = _dataService.LoadUwpAppsFromCache();
                        var matchingUwp = uwpApps.FirstOrDefault(u => string.Equals(u.Name, item.Name, StringComparison.OrdinalIgnoreCase));

                        if (matchingUwp != null)
                        {
                            pfn = matchingUwp.PackageFamilyName;
                        }
                        else
                        {
                            // Fallback, use the raw ApplicationName
                            pfn = firstRule.ApplicationName ?? string.Empty;
                        }
                    }

                    if (!string.IsNullOrEmpty(pfn))
                    {
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
                if (parsedDirection.HasFlag(Directions.Incoming))
                {
                    ruleToUpdate.InboundStatus = parsedAction.ToString();
                }

                if (parsedDirection.HasFlag(Directions.Outgoing))
                {
                    ruleToUpdate.OutboundStatus = parsedAction.ToString();
                }
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

        private AggregatedRuleViewModel CreateStandardProgramRule(string name, string appPath, Directions direction, Actions action)
        {
            // MFW prefix to prevent Windows from deleting the rule.
            string safeName = name.StartsWith("MFW - ", StringComparison.OrdinalIgnoreCase) ? name : $"MFW - {name}";

            return new AggregatedRuleViewModel
            {
                Name = safeName,
                ApplicationName = appPath,
                InboundStatus = direction.HasFlag(Directions.Incoming) ? action.ToString() : "N/A",
                OutboundStatus = direction.HasFlag(Directions.Outgoing) ? action.ToString() : "N/A",
                Type = RuleType.Program,
                IsEnabled = true,
                Grouping = MFWConstants.MainRuleGroup,
                Profiles = "All",
                ProtocolName = "Any",
                LocalPorts = "Any",
                RemotePorts = "Any",
                LocalAddresses = "Any",
                RemoteAddresses = "Any",
                InterfaceTypes = "All",
                Description = "N/A",
                ServiceName = "N/A"
            };
        }

        public void CreateAdvancedRule(AdvancedRuleViewModel vm, string interfaceTypes, string icmpTypesAndCodes)
        {
            var payload = new CreateAdvancedRulePayload { ViewModel = vm, InterfaceTypes = interfaceTypes, IcmpTypesAndCodes = icmpTypesAndCodes };
            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.CreateAdvancedRule, payload, $"Creating rule: {vm.Name}"));

            var newAggregatedRule = CreateAggregatedRuleFromAdvancedRule(vm);
            AddRuleAndRefresh(newAggregatedRule);
        }

        public void CreateProgramRule(string appPath, string action)
        {
            if (!string.IsNullOrEmpty(appPath))
            {
                _eventListenerService.SnoozeNotificationsForApp(appPath, TimeSpan.FromSeconds(5));
            }

            FirewallActionsService.ParseActionString(action, out Actions parsedAction, out Directions parsedDirection);

            var newAggregatedRule = CreateStandardProgramRule(Path.GetFileName(appPath), appPath, parsedDirection, parsedAction);
            AddRuleAndRefresh(newAggregatedRule);

            var payload = new ApplyApplicationRulePayload { AppPaths = [appPath], Action = action };
            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.ApplyApplicationRule, payload, $"Applying {action} to {Path.GetFileName(appPath)}"));
        }

        private void OnBackgroundStatusChanged(string text)
        {
            StatusTextChanged?.Invoke(text);
        }

        private void OnRuleSetChanged()
        {
            ClearRulesCache();
            if (!_appSettings.AlertOnForeignRules)
            {
                return;
            }

            try { _sentryRefreshDebounceTimer?.Change(1000, Timeout.Infinite); }
            catch (ObjectDisposedException) { } // timer disposed during shutdown
        }

        private async void DebouncedSentryRefresh(object? state)
        {
            try
            {
                _activityLogger.LogDebug("Sentry: Debounce timer elapsed. Checking for foreign rules.");
                await ScanForSystemChangesAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                _activityLogger.LogException("Error during sentry refresh", ex);
            }
        }

        private void OnPendingConnectionDetected(PendingConnectionViewModel pending)
        {
            Action processDetection = () =>
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
            };

            if (_uiContext != null)
            {
                _uiContext.Post(_ => processDetection(), null);
            }
            else
            {
                processDetection();
            }
        }

        public void ProcessSpecificAllow(PendingConnectionViewModel pending)
        {
            _eventListenerService.SnoozeNotificationsForApp(pending.AppPath, TimeSpan.FromSeconds(5));
            _eventListenerService.ClearPendingNotification(pending.AppPath, pending.Direction);
            PendingConnections.Remove(pending);

            var vm = new AdvancedRuleViewModel
            {
                // MFW prefix to protect against OS overwrites
                Name = $"MFW - {pending.FileName} - {pending.RemoteAddress}:{pending.RemotePort}",
                Description = "Granular rule created by Minimal Firewall popup.",
                IsEnabled = true,
                Grouping = MFWConstants.MainRuleGroup,
                Status = "Allow",
                Direction = pending.Direction.Equals("Incoming", StringComparison.OrdinalIgnoreCase)?
                    Directions.Incoming : Directions.Outgoing, 
                Protocol = int.TryParse(pending.Protocol, out int proto)?
                    proto: 256, 
                ApplicationName = pending.AppPath,
                RemotePorts = pending.RemotePort,
                RemoteAddresses = pending.RemoteAddress,
                LocalPorts = "*",
                LocalAddresses = "*",
                Profiles = "All",
                Type = RuleType.Advanced,
                InterfaceTypes = "All"
            };

         var advPayload = new CreateAdvancedRulePayload { ViewModel = vm, InterfaceTypes = "All", IcmpTypesAndCodes = "" }; 
         _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.CreateAdvancedRule, advPayload, $"Creating rule for {pending.FileName}"));

            var newAggregatedRule = CreateAggregatedRuleFromAdvancedRule(vm);
            AddRuleAndRefresh(newAggregatedRule);

            DashboardActionProcessed?.Invoke(pending);
        }

        public void CreateWildcardRule(PendingConnectionViewModel pending, WildcardRule newRule)
        {
            _eventListenerService.SnoozeNotificationsForApp(pending.AppPath, TimeSpan.FromSeconds(5));
            _eventListenerService.ClearPendingNotification(pending.AppPath, pending.Direction);

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

        public void Dispose()
        {
            // Unsubscribe before disposing the timer; residual in-flight
            // callbacks are caught in OnRuleSetChanged and EnqueueTask.
            // StatusChanged unsub avoids worker -> UI Invoke deadlock during shutdown.
            _firewallSentryService.RuleSetChanged -= OnRuleSetChanged;
            _eventListenerService.PendingConnectionDetected -= OnPendingConnectionDetected;
            _backgroundTaskService.StatusChanged -= OnBackgroundStatusChanged;
            _sentryRefreshDebounceTimer?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
