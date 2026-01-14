using System.Management;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using MinimalFirewall.TypedObjects;
using System.Collections.Generic;
using System.Linq;

namespace MinimalFirewall
{
    public partial class FirewallSentryService : IDisposable
    {
        private readonly FirewallRuleService firewallService;
        private ManagementEventWatcher? _watcher;
        private bool _isStarted = false;

        private Dictionary<string, AdvancedRuleViewModel> _ruleCache = new(StringComparer.OrdinalIgnoreCase);

        public event Action? RuleSetChanged;

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
                var query = new WqlEventQuery(
                    "SELECT * FROM __InstanceOperationEvent WITHIN 3 " +
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
            RuleSetChanged?.Invoke();
        }

        public List<FirewallRuleChange> CheckForChanges(ForeignRuleTracker acknowledgedTracker, IProgress<int>? progress = null, CancellationToken token = default)
        {
            var changes = new List<FirewallRuleChange>();
            var allRules = firewallService.GetAllRules();

            // Snapshot of rules currently in the system
            var currentScan = new Dictionary<string, AdvancedRuleViewModel>(StringComparer.OrdinalIgnoreCase);

            try
            {
                int totalRules = allRules.Count;
                if (totalRules == 0)
                {
                    progress?.Report(100);
                    return changes;
                }

                int processedRules = 0;
                foreach (var rule in allRules)
                {
                    if (token.IsCancellationRequested) return new List<FirewallRuleChange>();

                    processedRules++;
                    progress?.Report((processedRules * 100) / totalRules);

                    if (rule == null) continue;

                    string ruleName = rule.Name;
                    string ruleGrouping = rule.Grouping;

                    if (string.IsNullOrEmpty(ruleName)) continue;

                    // Pass the cached string instead of the COM object
                    if (IsMfwRule(ruleGrouping)) continue;

                    var ruleVm = FirewallDataService.CreateAdvancedRuleViewModel(rule);

                    // Use the cached ruleName for dictionary lookups
                    currentScan[ruleName] = ruleVm;

                    bool isAcknowledged = acknowledgedTracker.IsAcknowledged(ruleName);
                    bool existsInCache = _ruleCache.TryGetValue(ruleName, out var cachedRule);

                    FirewallRuleChange? changeObj = null;

                    // DETECTION LOGIC
                    if (existsInCache)
                    {
                        if (!ruleVm.HasSameSettings(cachedRule))
                        {
                            changeObj = new FirewallRuleChange
                            {
                                Type = ChangeType.Modified,
                                Rule = ruleVm,
                                OldRule = cachedRule
                            };
                        }
                        else
                        {
                            if (!isAcknowledged)
                            {
                                changeObj = new FirewallRuleChange { Type = ChangeType.New, Rule = ruleVm };
                            }
                        }
                    }
                    else
                    {
                        if (!isAcknowledged)
                        {
                            changeObj = new FirewallRuleChange { Type = ChangeType.New, Rule = ruleVm };
                        }
                    }

                    if (changeObj != null)
                    {
                        EnrichWithPublisher(changeObj, rule.ApplicationName);
                        changes.Add(changeObj);
                    }
                }

                // Check for deletions
                foreach (var kvp in _ruleCache)
                {
                    if (!currentScan.ContainsKey(kvp.Key))
                    {
                        if (!acknowledgedTracker.IsAcknowledged(kvp.Key))
                        {
                            changes.Add(new FirewallRuleChange
                            {
                                Type = ChangeType.Deleted,
                                Rule = kvp.Value,
                                OldRule = kvp.Value
                            });
                        }
                    }
                }

                _ruleCache = currentScan;
            }
            finally
            {
                foreach (var rule in allRules)
                {
                    if (rule != null)
                    {
                        try { Marshal.ReleaseComObject(rule); }
                        catch { /* Ignore release errors to prevent loop crash */ }
                    }
                }
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