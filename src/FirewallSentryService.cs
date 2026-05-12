using System.Management;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using MinimalFirewall.TypedObjects;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using System;
using System.Threading;

namespace MinimalFirewall
{
    public partial class FirewallSentryService : IDisposable
    {
        private readonly FirewallRuleService firewallService;
        private ManagementEventWatcher? _watcher;
        private bool _isStarted = false;

        // Lightweight incremental queue
        private readonly ConcurrentQueue<(string Name, ChangeType Type)> _pendingChanges = new();
        private bool _isFirstScan = true;

        public event Action? RuleSetChanged;

        public void ResetBaseline()
        {
            _isFirstScan = true;
            _pendingChanges.Clear();
        }

        public FirewallSentryService(FirewallRuleService firewallService)
        {
            this.firewallService = firewallService;
        }

        public void Start()
        {
            if (_isStarted) return;
            try
            {
                var scope = new ManagementScope(@"root\StandardCimv2");
                // Listen for rule change
                var query = new WqlEventQuery(
                    "SELECT * FROM __InstanceOperationEvent WITHIN 3 " +
                    "WHERE TargetInstance ISA 'MSFT_NetFirewallRule' AND " +
                    "(__Class = '__InstanceCreationEvent' OR __Class = '__InstanceModificationEvent' OR __Class = '__InstanceDeletionEvent')");

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
            if (!_isStarted || _watcher == null) return;
            try
            {
                _watcher.EventArrived -= OnFirewallRuleChangeEvent;
                _watcher.Stop();
                _watcher.Dispose();
            }
            catch (ManagementException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SENTRY ERROR] Failed to stop WMI watcher: {ex.Message}");
            }
            finally
            {
                _watcher = null;
                _isStarted = false;
            }
        }

        private void OnFirewallRuleChangeEvent(object sender, EventArrivedEventArgs e)
        {
            try
            {
                // Release unmanaged COM handles immediately
                using var newEvent = e.NewEvent;
                var eventClass = newEvent.ClassPath.ClassName;

                using var targetInstance = (ManagementBaseObject)newEvent["TargetInstance"];

                string ruleName = targetInstance["InstanceID"]?.ToString() ?? string.Empty;
                string grouping = targetInstance["DisplayGroup"]?.ToString() ?? string.Empty;

                if (string.IsNullOrEmpty(ruleName) || IsMfwRule(grouping)) return;

                ChangeType type = eventClass switch
                {
                    "__InstanceCreationEvent" => ChangeType.New,
                    "__InstanceModificationEvent" => ChangeType.Modified,
                    "__InstanceDeletionEvent" => ChangeType.Deleted,
                    _ => ChangeType.Modified
                };

                _pendingChanges.Enqueue((ruleName, type));
                RuleSetChanged?.Invoke();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SENTRY ERROR] Failed to parse WMI event: {ex.Message}");
            }
        }

        public List<FirewallRuleChange> CheckForChanges(ForeignRuleTracker acknowledgedTracker, IProgress<int>? progress = null, CancellationToken token = default)
        {
            var changes = new List<FirewallRuleChange>();

            // Baseline load: fetch all unacknowledged rules
            // Afterwards, ONLY process the specific rules caught by WMI event queue 
            if (_isFirstScan)
            {
                changes = PerformFullBaselineScan(acknowledgedTracker, progress, token);
                _isFirstScan = false;
                return changes;
            }

            // Incremental event processing
            int processed = 0;
            int total = _pendingChanges.Count;

            while (_pendingChanges.TryDequeue(out var changeEvent))
            {
                if (token.IsCancellationRequested) break;

                processed++;
                progress?.Report(total > 0 ? (processed * 100) / total : 100);

                if (acknowledgedTracker.IsAcknowledged(changeEvent.Name)) continue;

                if (changeEvent.Type == ChangeType.Deleted)
                {
                    changes.Add(new FirewallRuleChange
                    {
                        Type = ChangeType.Deleted,
                        Rule = new AdvancedRuleViewModel { Name = changeEvent.Name }
                    });
                    continue;
                }

                // Look up single rule safely (COM throws exceptions if rule is missing/deleted quickly)
                NetFwTypeLib.INetFwRule2? comRule = null;
                try
                {
                    comRule = firewallService.GetRuleByName(changeEvent.Name);
                }
                catch (FileNotFoundException)
                {
                    // Race condition (WMI report change, COM says doesn't exist)
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[SENTRY ERROR] COM Lookup failed for {changeEvent.Name}: {ex.Message}");
                }

                if (comRule != null)
                {
                    try
                    {
                        var ruleVm = FirewallDataService.CreateAdvancedRuleViewModel(comRule);
                        var changeObj = new FirewallRuleChange
                        {
                            Type = changeEvent.Type,
                            Rule = ruleVm
                        };

                        EnrichWithPublisher(changeObj, ruleVm.ApplicationName);
                        changes.Add(changeObj);
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(comRule);
                    }
                }
                else if (changeEvent.Type != ChangeType.Deleted)
                {
                    System.Diagnostics.Debug.WriteLine($"[SENTRY WARN] Dropped {changeEvent.Type} event for '{changeEvent.Name}': COM rule could not be fetched.");
                }
            }

            progress?.Report(100);
            return changes;
        }

        private List<FirewallRuleChange> PerformFullBaselineScan(ForeignRuleTracker acknowledgedTracker, IProgress<int>? progress, CancellationToken token)
        {
            var changes = new List<FirewallRuleChange>();
            Type? policyType = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
            if (policyType == null) return changes;

            NetFwTypeLib.INetFwPolicy2? firewallPolicy = (NetFwTypeLib.INetFwPolicy2?)Activator.CreateInstance(policyType);
            if (firewallPolicy?.Rules == null) return changes;

            var comRules = firewallPolicy.Rules;
            try
            {
                int totalRules = comRules.Count;
                if (totalRules == 0) { progress?.Report(100); return changes; }

                int processedRules = 0;
                foreach (NetFwTypeLib.INetFwRule2 rule in comRules)
                {
                    if (token.IsCancellationRequested) break;
                    processedRules++;
                    progress?.Report((processedRules * 100) / totalRules);

                    if (rule == null) continue;
                    try
                    {
                        string ruleName = rule.Name;
                        if (string.IsNullOrEmpty(ruleName) || IsMfwRule(rule.Grouping) || acknowledgedTracker.IsAcknowledged(ruleName)) continue;

                        var ruleVm = FirewallDataService.CreateAdvancedRuleViewModel(rule);
                        var changeObj = new FirewallRuleChange { Type = ChangeType.New, Rule = ruleVm };
                        EnrichWithPublisher(changeObj, ruleVm.ApplicationName);
                        changes.Add(changeObj);
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(rule);
                    }
                }
            }
            finally
            {
                if (comRules != null) Marshal.ReleaseComObject(comRules);
                if (firewallPolicy != null) Marshal.ReleaseComObject(firewallPolicy);
            }
            return changes;
        }

        private void EnrichWithPublisher(FirewallRuleChange changeObj, string? appPath)
        {
            if (string.IsNullOrWhiteSpace(appPath)) return;

            if (string.Equals(appPath, "System", StringComparison.OrdinalIgnoreCase))
            {
                changeObj.Publisher = "Microsoft Corporation (System)";
                return;
            }

            try
            {
                string expandedPath = Environment.ExpandEnvironmentVariables(appPath.Trim());
                if (File.Exists(expandedPath))
                {
                    if (SignatureValidationService.GetPublisherInfo(expandedPath, out string? publisher))
                    {
                        changeObj.Publisher = publisher ?? string.Empty;
                    }
                }
            }
            catch
            {
                /* Ignore invalid paths/perms to prevent scanning interruption */
            }
        }

        // Changed to accept string directly to avoid passing COM objects
        private static bool IsMfwRule(string grouping)
        {
            if (string.IsNullOrEmpty(grouping)) return false;
            return grouping.EndsWith(MFWConstants.MfwRuleSuffix) ||
                   grouping == MFWConstants.MainRuleGroup ||
                   grouping == MFWConstants.WildcardRuleGroup;
        }

        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }
    }
}
