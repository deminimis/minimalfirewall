using NetFwTypeLib;
using System.Data;
using System.IO;
using MinimalFirewall.TypedObjects;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;
using System.Threading;
using System.Text.Json;

namespace MinimalFirewall
{
    public partial class FirewallActionsService
    {
        private readonly FirewallRuleService firewallService;
        private readonly UserActivityLogger activityLogger;
        private readonly FirewallEventListenerService eventListenerService;
        private readonly ForeignRuleTracker foreignRuleTracker;
        private readonly FirewallSentryService sentryService;
        private readonly PublisherWhitelistService _whitelistService;
        private readonly TemporaryRuleManager _temporaryRuleManager;
        private readonly WildcardRuleService _wildcardRuleService;
        private readonly FirewallDataService _dataService;
        private readonly ConcurrentDictionary<string, System.Threading.Timer> _temporaryRuleTimers = new();
        private const string CryptoRuleName = "Minimal Firewall System - Certificate Checks";
        private const string DhcpRuleName = "Minimal Firewall System - DHCP Client";

        public BackgroundFirewallTaskService? BackgroundTaskService { get; set; }

        public FirewallActionsService(FirewallRuleService firewallService, UserActivityLogger activityLogger, FirewallEventListenerService eventListenerService, ForeignRuleTracker foreignRuleTracker, FirewallSentryService sentryService, PublisherWhitelistService whitelistService, WildcardRuleService wildcardRuleService, FirewallDataService dataService)
        {
            this.firewallService = firewallService;
            this.activityLogger = activityLogger;
            this.eventListenerService = eventListenerService;
            this.foreignRuleTracker = foreignRuleTracker;
            this.sentryService = sentryService;
            this._whitelistService = whitelistService;
            this._wildcardRuleService = wildcardRuleService;
            _temporaryRuleManager = new TemporaryRuleManager();
            _dataService = dataService;
        }

        public void CleanupTemporaryRulesOnStartup()
        {
            var expiredRules = _temporaryRuleManager.GetExpiredRules();
            if (expiredRules.Any())
            {
                var ruleNamesToRemove = expiredRules.Keys.ToList();
                try
                {
                    firewallService.DeleteRulesByName(ruleNamesToRemove);
                    foreach (var ruleName in ruleNamesToRemove)
                    {
                        _temporaryRuleManager.Remove(ruleName);
                    }
                    activityLogger.LogDebug($"Cleaned up {ruleNamesToRemove.Count} expired temporary rules on startup.");
                }
                catch (COMException ex)
                {
                    activityLogger.LogException("CleanupTemporaryRulesOnStartup", ex);
                }
            }
        }

        private static bool IsMfwRule(INetFwRule2 rule)
        {
            if (string.IsNullOrEmpty(rule.Grouping)) return false;
            return rule.Grouping.EndsWith(MFWConstants.MfwRuleSuffix) ||
                   rule.Grouping == MFWConstants.MainRuleGroup ||
                   rule.Grouping == MFWConstants.WildcardRuleGroup;
        }

        private void FindAndQueueDeleteForGeneralBlockRule(string appPath)
        {
            string normalizedAppPath = PathResolver.NormalizePath(appPath);
            var rulesToDelete = new List<string>();
            var allRules = firewallService.GetAllRules();
            try
            {
                foreach (var rule in allRules)
                {
                    if (rule != null &&
                        IsMfwRule(rule) &&
                        rule.Action == NET_FW_ACTION_.NET_FW_ACTION_BLOCK &&
                        string.Equals(PathResolver.NormalizePath(rule.ApplicationName), normalizedAppPath, StringComparison.OrdinalIgnoreCase) &&
                        rule.Protocol == 256 &&
                        rule.LocalPorts == "*" &&
                        rule.RemotePorts == "*")
                    {
                        rulesToDelete.Add(rule.Name);
                    }
                }
            }
            finally
            {
                foreach (var rule in allRules)
                {
                    if (rule != null) Marshal.ReleaseComObject(rule);
                }
            }

            if (rulesToDelete.Any())
            {
                activityLogger.LogDebug($"Auto-deleting general block rule(s) for {appPath} to apply new Allow rule: {string.Join(", ", rulesToDelete)}");
                try
                {
                    firewallService.DeleteRulesByName(rulesToDelete);
                    foreach (var name in rulesToDelete)
                        activityLogger.LogChange("Rule Auto-Deleted", name);
                }
                catch (COMException ex)
                {
                    activityLogger.LogException($"Auto-deleting rules for {appPath}", ex);
                }
            }
        }

        public void ApplyApplicationRuleChange(List<string> appPaths, string action, string? wildcardSourcePath = null)
        {
            var normalizedAppPaths = appPaths.Select(PathResolver.NormalizePath).Where(p => !string.IsNullOrEmpty(p)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (action.StartsWith("Allow", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var appPath in normalizedAppPaths)
                {
                    FindAndQueueDeleteForGeneralBlockRule(appPath);
                }
            }

            foreach (var appPath in normalizedAppPaths)
            {
                if (!File.Exists(appPath))
                {
                    activityLogger.LogDebug($"[Validation] Skipped creating rule for non-existent path: {appPath}");
                    continue;
                }

                var rulesToRemove = new List<string>();
                if (string.IsNullOrEmpty(wildcardSourcePath))
                {
                    if (action.Contains("Inbound") || action.Contains("(All)"))
                    {
                        rulesToRemove.AddRange(firewallService.GetRuleNamesByPathAndDirection(appPath, NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN));
                    }
                    if (action.Contains("Outbound") || action.Contains("(All)"))
                    {
                        rulesToRemove.AddRange(firewallService.GetRuleNamesByPathAndDirection(appPath, NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT));
                    }
                }

                string appName = Path.GetFileNameWithoutExtension(appPath);
                void createRule(string baseName, Directions dir, Actions act)
                {
                    string description = string.IsNullOrEmpty(wildcardSourcePath) ?
                        "" : $"{MFWConstants.WildcardDescriptionPrefix}{wildcardSourcePath}]";
                    CreateApplicationRule(baseName, appPath, dir, act, ProtocolTypes.Any.Value, description);
                }

                ApplyRuleAction(appName, action, createRule);
                if (rulesToRemove.Any())
                {
                    firewallService.DeleteRulesByName(rulesToRemove);
                }

                activityLogger.LogChange("Rule Changed", action + " for " + appPath);
            }
        }

        public void ApplyServiceRuleChange(string serviceName, string action, string? appPath = null)
        {
            if (string.IsNullOrEmpty(serviceName)) return;
            if (!ParseActionString(action, out Actions parsedAction, out Directions parsedDirection))
            {
                return;
            }

            var rulesToRemove = new List<string>();
            if (parsedDirection.HasFlag(Directions.Incoming))
            {
                rulesToRemove.AddRange(firewallService.DeleteConflictingServiceRules(serviceName, (NET_FW_ACTION_)parsedAction, NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN));
            }
            if (parsedDirection.HasFlag(Directions.Outgoing))
            {
                rulesToRemove.AddRange(firewallService.DeleteConflictingServiceRules(serviceName, (NET_FW_ACTION_)parsedAction, NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT));
            }

            var protocolsToCreate = new List<int> { 6, 17 };
            foreach (var protocol in protocolsToCreate)
            {
                string protocolSuffix = (protocol == 6) ?
                    " - TCP" : " - UDP";
                string actionStr = parsedAction == Actions.Allow ? "" : "Block ";
                if (parsedDirection.HasFlag(Directions.Incoming))
                {
                    string inName = $"{serviceName} - {actionStr}In{protocolSuffix}";
                    CreateServiceRule(inName, serviceName, Directions.Incoming, parsedAction, protocol, appPath);
                }
                if (parsedDirection.HasFlag(Directions.Outgoing))
                {
                    string outName = $"{serviceName} - {actionStr}Out{protocolSuffix}";
                    CreateServiceRule(outName, serviceName, Directions.Outgoing, parsedAction, protocol, appPath);
                }
            }

            if (rulesToRemove.Any())
            {
                firewallService.DeleteRulesByName(rulesToRemove);
            }

            activityLogger.LogChange("Service Rule Changed", action + " for " + serviceName);
        }


        public void ApplyUwpRuleChange(List<UwpApp> uwpApps, string action)
        {
            var validApps = new List<UwpApp>();
            var cachedUwpApps = _dataService.LoadUwpAppsFromCache();
            var cachedPfnSet = new HashSet<string>(cachedUwpApps.Select(a => a.PackageFamilyName), StringComparer.OrdinalIgnoreCase);
            foreach (var app in uwpApps)
            {
                if (cachedPfnSet.Contains(app.PackageFamilyName))
                {
                    validApps.Add(app);
                }
                else
                {
                    activityLogger.LogDebug($"[Validation] Skipped creating rule for non-existent UWP app: {app.Name} ({app.PackageFamilyName})");
                }
            }

            if (validApps.Count == 0) return;
            var packageFamilyNames = validApps.Select(app => app.PackageFamilyName).ToList();
            var rulesToRemove = firewallService.DeleteUwpRules(packageFamilyNames);
            foreach (var app in validApps)
            {
                void createRule(string name, Directions dir, Actions act) => CreateUwpRule(name, app.PackageFamilyName, dir, act, ProtocolTypes.Any.Value);
                ApplyRuleAction(app.Name, action, createRule);
                activityLogger.LogChange("UWP Rule Changed", action + " for " + app.Name);
            }

            if (rulesToRemove.Any())
            {
                firewallService.DeleteRulesByName(rulesToRemove);
            }
        }

        public void DeleteApplicationRules(List<string> appPaths)
        {
            if (appPaths.Count == 0) return;
            try
            {
                firewallService.DeleteRulesByPath(appPaths);
                foreach (var path in appPaths) activityLogger.LogChange("Rule Deleted", path);
            }
            catch (COMException ex)
            {
                activityLogger.LogException($"DeleteApplicationRules for {string.Join(",", appPaths)}", ex);
            }
        }

        public void DeleteRulesForWildcard(WildcardRule wildcard)
        {
            if (wildcard == null) return;
            try
            {
                string descriptionTag = $"{MFWConstants.WildcardDescriptionPrefix}{wildcard.FolderPath}]";
                firewallService.DeleteRulesByDescription(descriptionTag);
                activityLogger.LogChange("Wildcard Rules Deleted", $"Deleted rules for folder {wildcard.FolderPath}");
            }
            catch (COMException ex)
            {
                activityLogger.LogException($"DeleteRulesForWildcard for {wildcard.FolderPath}", ex);
            }
        }

        public void DeleteUwpRules(List<string> packageFamilyNames)
        {
            if (packageFamilyNames.Count == 0) return;
            try
            {
                firewallService.DeleteUwpRules(packageFamilyNames);
                foreach (var pfn in packageFamilyNames) activityLogger.LogChange("UWP Rule Deleted", pfn);
            }
            catch (COMException ex)
            {
                activityLogger.LogException($"DeleteUwpRules for {string.Join(",", packageFamilyNames)}", ex);
            }
        }

        public void DeleteAdvancedRules(List<string> ruleNames)
        {
            if (ruleNames.Count == 0) return;
            try
            {
                firewallService.DeleteRulesByName(ruleNames);
                foreach (var name in ruleNames) activityLogger.LogChange("Advanced Rule Deleted", name);
            }
            catch (COMException ex)
            {
                activityLogger.LogException($"DeleteAdvancedRules for {string.Join(",", ruleNames)}", ex);
            }
        }


        private void ManageCryptoServiceRule(bool enable)
        {
            INetFwRule2? rule = null;
            try
            {
                rule = firewallService.GetRuleByName(CryptoRuleName);
                if (enable)
                {
                    if (rule == null)
                    {
                        var newRule = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule")!)!;
                        newRule.WithName(CryptoRuleName)
                               .WithDescription("Allows Windows to check for certificate revocation online. Essential for the 'auto-allow trusted' feature in Lockdown Mode.")
                               .ForService("CryptSvc")
                               .WithDirection(Directions.Outgoing)
                               .WithAction(Actions.Allow)
                               .WithProtocol(ProtocolTypes.TCP.Value)
                               .WithRemotePorts("80,443")
                               .WithGrouping(MFWConstants.MainRuleGroup)
                               .IsEnabled();
                        firewallService.CreateRule(newRule);
                        activityLogger.LogDebug("Created system rule for certificate checks.");
                    }
                    else if (!rule.Enabled)
                    {
                        rule.Enabled = true;
                        activityLogger.LogDebug("Enabled system rule for certificate checks.");
                    }
                }
                else
                {
                    if (rule != null && rule.Enabled)
                    {
                        rule.Enabled = false;
                        activityLogger.LogDebug("Disabled system rule for certificate checks.");
                    }
                }
            }
            catch (COMException ex)
            {
                activityLogger.LogException($"ManageCryptoServiceRule (enable: {enable})", ex);
            }
            finally
            {
                if (rule != null) Marshal.ReleaseComObject(rule);
            }
        }

        private void ManageDhcpClientRule(bool enable)
        {
            INetFwRule2? rule = null;
            try
            {
                rule = firewallService.GetRuleByName(DhcpRuleName);
                if (enable)
                {
                    if (rule == null)
                    {
                        var newRule = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule")!)!;
                        newRule.WithName(DhcpRuleName)
                               .WithDescription("Allows the DHCP Client (Dhcp) service to get an IP address from your router. Essential for network connectivity in Lockdown Mode.")
                               .ForService("Dhcp")
                               .WithDirection(Directions.Outgoing)
                               .WithAction(Actions.Allow)
                               .WithProtocol(ProtocolTypes.UDP.Value)
                               .WithLocalPorts("68")
                               .WithRemotePorts("67")
                               .WithGrouping(MFWConstants.MainRuleGroup)
                               .IsEnabled();
                        firewallService.CreateRule(newRule);
                        activityLogger.LogDebug("Created system rule for DHCP Client.");
                    }
                    else if (!rule.Enabled)
                    {
                        rule.Enabled = true;
                        activityLogger.LogDebug("Enabled system rule for DHCP Client.");
                    }
                }
                else
                {
                    if (rule != null)
                    {
                        firewallService.DeleteRulesByName(new List<string> { DhcpRuleName });
                        activityLogger.LogDebug("Disabled/Deleted system rule for DHCP Client.");
                    }
                }
            }
            catch (COMException ex)
            {
                activityLogger.LogException($"ManageDhcpClientRule (enable: {enable})", ex);
            }
            finally
            {
                if (rule != null) Marshal.ReleaseComObject(rule);
            }
        }

        public void ToggleLockdown()
        {
            var isCurrentlyLocked = firewallService.GetDefaultOutboundAction() == NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
            bool newLockdownState = !isCurrentlyLocked;
            activityLogger.LogDebug($"Toggling Lockdown. Current state: {(isCurrentlyLocked ? "Locked" : "Unlocked")}. New state: {(newLockdownState ? "Locked" : "Unlocked")}.");
            try
            {
                AdminTaskService.SetAuditPolicy(newLockdownState);
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                activityLogger.LogException("SetAuditPolicy", ex);
            }

            ManageCryptoServiceRule(newLockdownState);
            ManageDhcpClientRule(newLockdownState);
            if (newLockdownState && !AdminTaskService.IsAuditPolicyEnabled())
            {
                MessageBox.Show(
                    "Failed to verify that Windows Security Auditing was enabled.\n\n" +
                     "The Lockdown dashboard will not be able to detect blocked connections.\n\n" +
                     "Potential Causes:\n" +
                    "1. A local or domain Group Policy is preventing this change.\n" +
                    "2. Other security software is blocking this action.\n\n" +
                    "The firewall's default policy will be set back to 'Allow' for safety.",
                     "Lockdown Mode Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                try
                {
                    firewallService.SetDefaultOutboundAction(NET_FW_ACTION_.NET_FW_ACTION_ALLOW);
                }
                catch (COMException ex)
                {
                    activityLogger.LogException("SetDefaultOutboundAction(Allow) after audit failure", ex);
                }
                activityLogger.LogDebug("Lockdown Mode Failed: Could not enable audit policy.");
                return;
            }

            try
            {
                firewallService.SetDefaultOutboundAction(
                    newLockdownState ? NET_FW_ACTION_.NET_FW_ACTION_BLOCK : NET_FW_ACTION_.NET_FW_ACTION_ALLOW);
            }
            catch (COMException ex)
            {
                activityLogger.LogException("SetDefaultOutboundAction", ex);
                MessageBox.Show("Failed to change default outbound policy.\nCheck debug_log.txt for details.",
                    "Lockdown Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (newLockdownState)
            {
                eventListenerService.Start();
            }
            else
            {
                eventListenerService.Stop();
            }

            activityLogger.LogChange("Lockdown Mode", newLockdownState ? "Enabled" : "Disabled");
            if (!newLockdownState)
            {
                ReenableMfwRules();
                activityLogger.LogDebug("All MFW rules re-enabled upon disabling Lockdown mode.");
            }
        }


        public void ProcessPendingConnection(PendingConnectionViewModel pending, string decision, TimeSpan duration = default, bool trustPublisher = false)
        {
            activityLogger.LogDebug($"Processing Pending Connection for '{pending.AppPath}'. Decision: {decision}, Duration: {duration}, Trust Publisher: {trustPublisher}");
            TimeSpan shortSnoozeDuration = TimeSpan.FromSeconds(10);
            TimeSpan longSnoozeDuration = TimeSpan.FromMinutes(2);
            if (trustPublisher && SignatureValidationService.GetPublisherInfo(pending.AppPath, out var publisherName) && publisherName != null)
            {
                _whitelistService.Add(publisherName);
                activityLogger.LogChange("Publisher Whitelisted", $"Publisher '{publisherName}' was added to the whitelist.");
            }

            eventListenerService.ClearPendingNotification(pending.AppPath, pending.Direction);
            switch (decision)
            {
                case "Allow":
                case "Block":
                    eventListenerService.SnoozeNotificationsForApp(pending.AppPath, shortSnoozeDuration);
                    string action = (decision == "Allow" ? "Allow" : "Block") + " (" + pending.Direction + ")";
                    if (!string.IsNullOrEmpty(pending.ServiceName))
                    {
                        var serviceNames = pending.ServiceName.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries);
                        foreach (var serviceName in serviceNames)
                        {
                            ApplyServiceRuleChange(serviceName, action, pending.AppPath);
                        }
                    }
                    else if (!string.IsNullOrEmpty(pending.AppPath))
                    {
                        ApplyApplicationRuleChange([pending.AppPath], action);
                    }
                    break;
                case "TemporaryAllow":
                    eventListenerService.SnoozeNotificationsForApp(pending.AppPath, shortSnoozeDuration);
                    CreateTemporaryAllowRule(pending.AppPath, pending.ServiceName, pending.Direction, duration);
                    break;

                case "Ignore":
                    eventListenerService.SnoozeNotificationsForApp(pending.AppPath, longSnoozeDuration);
                    activityLogger.LogDebug($"Ignored Connection: {pending.Direction} for {pending.AppPath}");
                    break;
            }
        }

        public void ReenableMfwRules()
        {
            var allRules = firewallService.GetAllRules();
            try
            {
                foreach (var rule in allRules)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(rule.Grouping) &&
                             (rule.Grouping.EndsWith(MFWConstants.MfwRuleSuffix) ||
                               rule.Grouping == "Minimal Firewall" ||
                               rule.Grouping == "Minimal Firewall (Wildcard)"))
                        {
                            if (!rule.Enabled)
                            {
                                rule.Enabled = true;
                            }
                        }
                    }
                    catch (COMException ex)
                    {
                        activityLogger.LogException($"Enable rule '{rule.Name}'", ex);
                    }
                }
            }
            finally
            {
                foreach (var rule in allRules)
                {
                    Marshal.ReleaseComObject(rule);
                }
            }
        }

        private void SetupRuleTimer(string ruleName, TimeSpan duration)
        {
            var timer = new System.Threading.Timer(_ =>
            {
                try
                {
                    firewallService.DeleteRulesByName([ruleName]);
                    _temporaryRuleManager.Remove(ruleName);
                    if (_temporaryRuleTimers.TryRemove(ruleName, out var t))
                    {
                        t.Dispose();
                    }
                    activityLogger.LogDebug($"Temporary rule {ruleName} expired and was removed.");
                }
                catch (COMException ex)
                {
                    activityLogger.LogException($"Deleting temporary rule {ruleName}", ex);
                }
            }, null, duration, Timeout.InfiniteTimeSpan);
            _temporaryRuleTimers[ruleName] = timer;
        }

        private void CreateTemporaryAllowRule(string appPath, string serviceName, string direction, TimeSpan duration)
        {
            if (!ParseActionString($"Allow ({direction})", out Actions parsedAction, out Directions parsedDirection)) return;
            string baseName = !string.IsNullOrEmpty(serviceName) ? serviceName.Split(',')[0].Trim() : Path.GetFileNameWithoutExtension(appPath);
            string guid = Guid.NewGuid().ToString();
            string description = "Temporarily allowed by Minimal Firewall.";
            DateTime expiry = DateTime.UtcNow.Add(duration);

            if (!string.IsNullOrEmpty(serviceName))
            {
                var protocolsToCreate = new List<int> { 6, 17 };
                foreach (var protocol in protocolsToCreate)
                {
                    string protocolSuffix = (protocol == 6) ?
                        " - TCP" : " - UDP";
                    string actionStr = parsedAction == Actions.Allow ? "" : "Block ";
                    if (parsedDirection.HasFlag(Directions.Incoming))
                    {
                        string ruleName = $"Temp Allow - {baseName} - In - {guid}{protocolSuffix}";
                        CreateServiceRule(ruleName, serviceName, Directions.Incoming, parsedAction, protocol, appPath);
                        _temporaryRuleManager.Add(ruleName, expiry);
                        SetupRuleTimer(ruleName, duration);
                    }
                    if (parsedDirection.HasFlag(Directions.Outgoing))
                    {
                        string ruleName = $"Temp Allow - {baseName} - Out - {guid}{protocolSuffix}";
                        CreateServiceRule(ruleName, serviceName, Directions.Outgoing, parsedAction, protocol, appPath);
                        _temporaryRuleManager.Add(ruleName, expiry);
                        SetupRuleTimer(ruleName, duration);
                    }
                }
                activityLogger.LogChange("Temporary Rule Created", $"Allowed {baseName} (service) for {duration.TotalMinutes} minutes.");
            }
            else
            {
                string ruleName = $"Temp Allow - {baseName} - {direction} - {guid}";
                CreateApplicationRule(ruleName, appPath, parsedDirection, parsedAction, ProtocolTypes.Any.Value, description);

                _temporaryRuleManager.Add(ruleName, expiry);
                SetupRuleTimer(ruleName, duration);
                activityLogger.LogChange("Temporary Rule Created", $"Allowed {baseName} ({appPath}) for {duration.TotalMinutes} minutes.");
            }
        }

        public void AcceptForeignRule(FirewallRuleChange change)
        {
            if (change.Rule?.Name is not null)
            {
                firewallService.EnableRuleByName(change.Rule.Name);

                foreignRuleTracker.AcknowledgeRules([change.Rule.Name]);
                activityLogger.LogChange("Foreign Rule Accepted", change.Rule.Name);
                activityLogger.LogDebug($"Sentry: Accepted and Enabled foreign rule '{change.Rule.Name}'");
            }
        }

        public void DisableForeignRule(FirewallRuleChange change)
        {
            if (change.Rule?.Name is not null)
            {
                firewallService.DisableRuleByName(change.Rule.Name);
                foreignRuleTracker.AcknowledgeRules([change.Rule.Name]);
                activityLogger.LogChange("Foreign Rule Disabled", change.Rule.Name);
                activityLogger.LogDebug($"Sentry: Disabled and acknowledged foreign rule '{change.Rule.Name}'");
            }
        }

        // Quarantine Logic 
        public void QuarantineForeignRule(FirewallRuleChange change)
        {
            if (change.Rule?.Name is not null)
            {
                firewallService.DisableRuleByName(change.Rule.Name);
                activityLogger.LogChange("Foreign Rule Quarantined", change.Rule.Name);
                activityLogger.LogDebug($"Sentry: Quarantined (Disabled) foreign rule '{change.Rule.Name}' without acknowledgement.");
            }
        }

        public void DeleteForeignRule(FirewallRuleChange change)
        {
            if (change.Rule?.Name is not null)
            {
                activityLogger.LogDebug($"Sentry: Deleting foreign rule '{change.Rule.Name}'");
                DeleteAdvancedRules([change.Rule.Name]);
            }
        }

        public void SetGroupEnabledState(string groupName, bool isEnabled)
        {
            INetFwRules? comRules = null;
            var rulesInGroup = new List<INetFwRule2>();
            INetFwPolicy2? firewallPolicy = null;
            try
            {
                Type? policyType = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
                if (policyType == null) return;
                firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(policyType)!;
                if (firewallPolicy == null) return;

                comRules = firewallPolicy.Rules;
                foreach (INetFwRule2 r in comRules)
                {
                    if (r != null && string.Equals(r.Grouping, groupName, StringComparison.OrdinalIgnoreCase))
                    {
                        rulesInGroup.Add(r);
                    }
                    else
                    {
                        if (r != null) Marshal.ReleaseComObject(r);
                    }
                }

                foreach (var rule in rulesInGroup)
                {
                    try
                    {
                        if (rule.Enabled != isEnabled)
                        {
                            rule.Enabled = isEnabled;
                        }
                    }
                    catch (COMException ex)
                    {
                        activityLogger.LogException($"SetGroupEnabledState for rule '{rule.Name}'", ex);
                    }
                }
                activityLogger.LogChange("Group State Changed", $"Group '{groupName}' {(isEnabled ? "Enabled" : "Disabled")}");
            }
            catch (COMException ex)
            {
                activityLogger.LogException($"SetGroupEnabledState for group '{groupName}'", ex);
            }
            finally
            {
                foreach (var rule in rulesInGroup)
                {
                    if (rule != null) Marshal.ReleaseComObject(rule);
                }
                if (comRules != null) Marshal.ReleaseComObject(comRules);
                if (firewallPolicy != null) Marshal.ReleaseComObject(firewallPolicy);
            }
        }

        public void AcceptAllForeignRules(List<FirewallRuleChange> changes)
        {
            if (changes == null || changes.Count == 0) return;
            var ruleNames = changes.Select(c => c.Rule?.Name).Where(n => n != null).Select(n => n!).ToList();
            if (ruleNames.Any())
            {
                foreignRuleTracker.AcknowledgeRules(ruleNames);
                activityLogger.LogChange("All Foreign Rules Accepted", $"{ruleNames.Count} rules accepted.");
                activityLogger.LogDebug($"Sentry: Accepted all {ruleNames.Count} foreign rules.");
            }
        }

        public void CreateAdvancedRule(AdvancedRuleViewModel vm, string interfaceTypes, string icmpTypesAndCodes)
        {
            if (!string.IsNullOrWhiteSpace(vm.ApplicationName))
            {
                vm.ApplicationName = PathResolver.NormalizePath(vm.ApplicationName);
                if (!File.Exists(vm.ApplicationName))
                {
                    activityLogger.LogDebug($"[Validation] Aborted creating advanced rule due to non-existent path: {vm.ApplicationName}");
                    return;
                }
            }

            if (vm.Status == "Allow" && !string.IsNullOrWhiteSpace(vm.ApplicationName))
            {
                FindAndQueueDeleteForGeneralBlockRule(vm.ApplicationName);
            }

            bool hasProgramOrService = !string.IsNullOrWhiteSpace(vm.ApplicationName) || !string.IsNullOrWhiteSpace(vm.ServiceName);
            bool isProtocolTcpUdpOrAny = vm.Protocol == ProtocolTypes.TCP.Value ||
                                     vm.Protocol == ProtocolTypes.UDP.Value ||
                                     vm.Protocol == ProtocolTypes.Any.Value;
            if (hasProgramOrService && !isProtocolTcpUdpOrAny)
            {
                MessageBox.Show(
                     "When specifying a program or service, the protocol must be TCP, UDP, or Any.",
                     "Invalid Rule", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var directionsToCreate = new List<Directions>(2);
            if (vm.Direction.HasFlag(Directions.Incoming)) directionsToCreate.Add(Directions.Incoming);
            if (vm.Direction.HasFlag(Directions.Outgoing)) directionsToCreate.Add(Directions.Outgoing);

            var protocolsToCreate = new List<int>();
            if (hasProgramOrService && vm.Protocol == ProtocolTypes.Any.Value)
            {
                protocolsToCreate.Add(ProtocolTypes.TCP.Value);
                protocolsToCreate.Add(ProtocolTypes.UDP.Value);
            }
            else
            {
                protocolsToCreate.Add(vm.Protocol);
            }

            foreach (var direction in directionsToCreate)
            {
                foreach (var protocol in protocolsToCreate)
                {
                    var ruleVm = new AdvancedRuleViewModel
                    {
                        Name = vm.Name,
                        Status = vm.Status,
                        IsEnabled = vm.IsEnabled,
                        Description = vm.Description,
                        Grouping = vm.Grouping,
                        ApplicationName = vm.ApplicationName,
                        ServiceName = vm.ServiceName,
                        LocalPorts = vm.LocalPorts,
                        RemotePorts = vm.RemotePorts,
                        LocalAddresses = vm.LocalAddresses,
                        RemoteAddresses = vm.RemoteAddresses,
                        Profiles = vm.Profiles,
                        Type = vm.Type,
                        Direction = direction,
                        Protocol = protocol,
                        InterfaceTypes = vm.InterfaceTypes,
                        IcmpTypesAndCodes = vm.IcmpTypesAndCodes
                    };
                    string nameSuffix = "";
                    if (directionsToCreate.Count > 1)
                    {
                        nameSuffix += $" - {direction}";
                    }
                    if (protocolsToCreate.Count > 1)
                    {
                        nameSuffix += (protocol == ProtocolTypes.TCP.Value) ?
                        " - TCP" : " - UDP";
                    }
                    ruleVm.Name = vm.Name + nameSuffix;
                    CreateSingleAdvancedRule(ruleVm, interfaceTypes, icmpTypesAndCodes);
                }
            }
        }

        private void CreateSingleAdvancedRule(AdvancedRuleViewModel vm, string interfaceTypes, string icmpTypesAndCodes)
        {
            var firewallRule = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule")!)!;
            try
            {
                firewallRule.Name = vm.Name;
                firewallRule.Description = vm.Description;
                firewallRule.Enabled = vm.IsEnabled;
                firewallRule.Grouping = vm.Grouping;
                firewallRule.Action = vm.Status == "Allow" ?
                    NET_FW_ACTION_.NET_FW_ACTION_ALLOW : NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
                firewallRule.Direction = (NET_FW_RULE_DIRECTION_)vm.Direction;
                firewallRule.Protocol = vm.Protocol;

                if (!string.IsNullOrWhiteSpace(vm.ServiceName))
                {
                    firewallRule.serviceName = vm.ServiceName;
                }

                if (!string.IsNullOrWhiteSpace(vm.ApplicationName))
                {
                    firewallRule.ApplicationName = vm.ApplicationName;
                }
                else
                {
                    firewallRule.ApplicationName = null;
                }

                if (vm.Protocol == ProtocolTypes.TCP.Value || vm.Protocol == ProtocolTypes.UDP.Value || vm.Protocol == ProtocolTypes.Any.Value)
                {
                    firewallRule.LocalPorts = !string.IsNullOrEmpty(vm.LocalPorts) ?
                        vm.LocalPorts : "*";
                    firewallRule.RemotePorts = !string.IsNullOrEmpty(vm.RemotePorts) ? vm.RemotePorts : "*";
                }

                firewallRule.LocalAddresses = !string.IsNullOrEmpty(vm.LocalAddresses) ?
                    vm.LocalAddresses : "*";
                firewallRule.RemoteAddresses = !string.IsNullOrEmpty(vm.RemoteAddresses) ? vm.RemoteAddresses : "*";

                NET_FW_PROFILE_TYPE2_ profiles = 0;
                if (vm.Profiles.Contains("Domain")) profiles |= NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_DOMAIN;
                if (vm.Profiles.Contains("Private")) profiles |= NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PRIVATE;
                if (vm.Profiles.Contains("Public")) profiles |= NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PUBLIC;
                if (profiles == 0 || vm.Profiles == "All") profiles = NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_ALL;
                firewallRule.Profiles = (int)profiles;

                firewallRule.InterfaceTypes = interfaceTypes;
                if (vm.Protocol == ProtocolTypes.ICMPv4.Value || vm.Protocol == ProtocolTypes.ICMPv6.Value)
                {
                    firewallRule.IcmpTypesAndCodes = !string.IsNullOrWhiteSpace(icmpTypesAndCodes) ?
                        icmpTypesAndCodes : "*";
                }
                else
                {
                    firewallRule.IcmpTypesAndCodes = null;
                }

                firewallService.CreateRule(firewallRule);
                activityLogger.LogChange("Advanced Rule Created", vm.Name);
                activityLogger.LogDebug($"Created Advanced Rule: '{vm.Name}'");
            }
            finally
            {
                if (firewallRule != null) Marshal.ReleaseComObject(firewallRule);
            }
        }

        public static bool ParseActionString(string action, out Actions parsedAction, out Directions parsedDirection)
        {
            parsedAction = Actions.Allow;
            parsedDirection = 0;
            if (string.IsNullOrEmpty(action)) return false;

            parsedAction = action.StartsWith("Allow", StringComparison.OrdinalIgnoreCase) ? Actions.Allow : Actions.Block;
            if (action.Contains("(All)"))
            {
                parsedDirection = Directions.Incoming |
                    Directions.Outgoing;
            }
            else
            {
                if (action.Contains("Inbound") || action.Contains("Incoming"))
                {
                    parsedDirection |= Directions.Incoming;
                }
                if (action.Contains("Outbound") || action.Contains("Outgoing"))
                {
                    parsedDirection |= Directions.Outgoing;
                }
            }

            if (parsedDirection == 0)
            {
                parsedDirection = Directions.Outgoing;
            }

            return true;
        }

        private static void ApplyRuleAction(string appName, string action, Action<string, Directions, Actions> createRule)
        {
            if (!ParseActionString(action, out Actions parsedAction, out Directions parsedDirection))
            {
                return;
            }

            string actionStr = parsedAction == Actions.Allow ?
                "" : "Block ";
            string inName = $"{appName} - {actionStr}In";
            string outName = $"{appName} - {actionStr}Out";
            if (parsedDirection.HasFlag(Directions.Incoming))
            {
                createRule(inName, Directions.Incoming, parsedAction);
            }
            if (parsedDirection.HasFlag(Directions.Outgoing))
            {
                createRule(outName, Directions.Outgoing, parsedAction);
            }
        }

        private static INetFwRule2 CreateRuleObject(string name, string appPath, Directions direction, Actions action, int protocol, string description = "")
        {
            var firewallRule = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule")!)!;
            firewallRule.Name = name;
            firewallRule.ApplicationName = appPath;
            firewallRule.Direction = (NET_FW_RULE_DIRECTION_)direction;
            firewallRule.Action = (NET_FW_ACTION_)action;
            firewallRule.Enabled = true;
            firewallRule.Protocol = protocol;
            if (protocol == ProtocolTypes.TCP.Value || protocol == ProtocolTypes.UDP.Value || protocol == ProtocolTypes.Any.Value)
            {
                firewallRule.LocalPorts = "*";
                firewallRule.RemotePorts = "*";
            }

            if (!string.IsNullOrEmpty(description) && description.StartsWith(MFWConstants.WildcardDescriptionPrefix))
            {
                firewallRule.Grouping = MFWConstants.WildcardRuleGroup;
                firewallRule.Description = description;
            }
            else
            {
                firewallRule.Grouping = MFWConstants.MainRuleGroup;
            }
            return firewallRule;
        }

        private void CreateApplicationRule(string name, string appPath, Directions direction, Actions action, int protocol, string description)
        {
            activityLogger.LogDebug($"Creating Application Rule: '{name}' for '{appPath}'");
            var firewallRule = CreateRuleObject(name, appPath, direction, action, protocol, description);
            firewallService.CreateRule(firewallRule);
        }

        private void CreateServiceRule(string name, string serviceName, Directions direction, Actions action, int protocol, string? appPath = null)
        {
            activityLogger.LogDebug($"Creating Service Rule: '{name}' for service '{serviceName}' with AppPath: '{appPath ?? "null"}'");
            var firewallRule = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule")!)!;
            try
            {
                firewallRule.Name = name;
                firewallRule.serviceName = serviceName;
                firewallRule.ApplicationName = string.IsNullOrEmpty(appPath) ? null : appPath;
                firewallRule.Direction = (NET_FW_RULE_DIRECTION_)direction;
                firewallRule.Action = (NET_FW_ACTION_)action;
                firewallRule.Protocol = protocol;
                if (protocol == ProtocolTypes.TCP.Value || protocol == ProtocolTypes.UDP.Value || protocol == ProtocolTypes.Any.Value)
                {
                    firewallRule.LocalPorts = "*";
                    firewallRule.RemotePorts = "*";
                }

                firewallRule.Grouping = MFWConstants.MainRuleGroup;
                firewallRule.Enabled = true;
                firewallService.CreateRule(firewallRule);
            }
            finally
            {
                if (firewallRule != null) Marshal.ReleaseComObject(firewallRule);
            }
        }

        private void CreateUwpRule(string name, string packageFamilyName, Directions direction, Actions action, int protocol)
        {
            activityLogger.LogDebug($"Creating UWP Rule: '{name}' for PFN '{packageFamilyName}'");
            var firewallRule = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule")!)!;
            try
            {
                firewallRule.Name = name;
                firewallRule.Description = MFWConstants.UwpDescriptionPrefix + packageFamilyName;
                firewallRule.Direction = (NET_FW_RULE_DIRECTION_)direction;
                firewallRule.Action = (NET_FW_ACTION_)action;
                firewallRule.Protocol = protocol;
                if (protocol == ProtocolTypes.TCP.Value || protocol == ProtocolTypes.UDP.Value || protocol == ProtocolTypes.Any.Value)
                {
                    firewallRule.LocalPorts = "*";
                    firewallRule.RemotePorts = "*";
                }

                firewallRule.Grouping = MFWConstants.MainRuleGroup;
                firewallRule.Enabled = true;
                firewallService.CreateRule(firewallRule);
            }
            finally
            {
                if (firewallRule != null) Marshal.ReleaseComObject(firewallRule);
            }
        }

        public async Task DeleteGroupAsync(string groupName)
        {
            await Task.Run(() =>
            {
                try
                {
                    activityLogger.LogDebug($"Deleting all rules in group: {groupName}");
                    firewallService.DeleteRulesByGroup(groupName);
                }
                catch (COMException ex)
                {
                    activityLogger.LogException($"DeleteGroupAsync for {groupName}", ex);
                }
            });
        }

        public void DeleteAllMfwRules()
        {
            try
            {
                firewallService.DeleteAllMfwRules();
                _wildcardRuleService.ClearRules();
                activityLogger.LogChange("Bulk Delete", "All Minimal Firewall rules deleted by user.");
            }
            catch (COMException ex)
            {
                activityLogger.LogException("DeleteAllMfwRules", ex);
            }
        }

        public void UpdateWildcardRule(WildcardRule oldRule, WildcardRule newRule)
        {
            _wildcardRuleService.UpdateRule(oldRule, newRule);
            DeleteRulesForWildcard(oldRule);
            activityLogger.LogChange("Wildcard Rule Updated", newRule.FolderPath);
        }

        public void RemoveWildcardRule(WildcardRule rule)
        {
            _wildcardRuleService.RemoveRule(rule);
            DeleteRulesForWildcard(rule);
            activityLogger.LogChange("Wildcard Rule Removed", rule.FolderPath);
        }

        public void RemoveWildcardDefinitionOnly(WildcardRule rule)
        {
            _wildcardRuleService.RemoveRule(rule);
            activityLogger.LogChange("Wildcard Definition Removed", rule.FolderPath);
        }

        public void ApplyWildcardMatch(string appPath, string serviceName, WildcardRule rule)
        {
            if (!ParseActionString(rule.Action, out Actions parsedAction, out Directions parsedDirection))
            {
                activityLogger.LogDebug($"[ApplyWildcardMatch] Invalid action string in wildcard rule for {rule.FolderPath}: {rule.Action}");
                return;
            }

            void createRule(string baseName, Directions dir, Actions act, int protocol, string? serviceNameToUse)
            {
                if (!ValidationUtility.ValidatePortString(rule.LocalPorts, out string localPortError))
                {
                    activityLogger.LogDebug($"[ApplyWildcardMatch] Invalid LocalPorts '{rule.LocalPorts}' in wildcard for {rule.FolderPath}. Rule '{baseName}' not created. Error: {localPortError}");
                    return;
                }
                if (!ValidationUtility.ValidatePortString(rule.RemotePorts, out string remotePortError))
                {
                    activityLogger.LogDebug($"[ApplyWildcardMatch] Invalid RemotePorts '{rule.RemotePorts}' in wildcard for {rule.FolderPath}. Rule '{baseName}' not created. Error: {remotePortError}");
                    return;
                }
                if (!ValidationUtility.ValidateAddressString(rule.RemoteAddresses, out string remoteAddressError))
                {
                    activityLogger.LogDebug($"[ApplyWildcardMatch] Invalid RemoteAddresses '{rule.RemoteAddresses}' in wildcard for {rule.FolderPath}. Rule '{baseName}' not created. Error: {remoteAddressError}");
                    return;
                }

                var firewallRule = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule")!)!;
                string? valueWithError = null;
                bool ruleCreationSkipped = false;
                try
                {
                    firewallRule.Name = baseName;
                    if (!string.IsNullOrEmpty(serviceNameToUse))
                    {
                        firewallRule.serviceName = serviceNameToUse;
                        firewallRule.ApplicationName = appPath;
                    }
                    else
                    {
                        firewallRule.ApplicationName = appPath;
                        firewallRule.serviceName = null;
                    }

                    firewallRule.Direction = (NET_FW_RULE_DIRECTION_)dir;
                    firewallRule.Action = (NET_FW_ACTION_)act;
                    firewallRule.Enabled = true;
                    firewallRule.Grouping = MFWConstants.WildcardRuleGroup;
                    firewallRule.Description = $"{MFWConstants.WildcardDescriptionPrefix}{rule.FolderPath}]";
                    firewallRule.Protocol = protocol;
                    try
                    {
                        valueWithError = rule.LocalPorts;
                        if (protocol != 6 && protocol != 17) valueWithError = "*";
                        else if (string.IsNullOrEmpty(valueWithError)) valueWithError = "*";
                        firewallRule.LocalPorts = valueWithError;
                    }
                    catch (ArgumentException portEx)
                    {
                        activityLogger.LogException($"ApplyWildcardMatch-SetLocalPorts-{baseName}", portEx);
                        activityLogger.LogDebug($"[ApplyWildcardMatch] Falling back setting LocalPorts to '*' for rule '{baseName}' due to error: {portEx.Message}");
                        try
                        {
                            firewallRule.LocalPorts = "*";
                        }
                        catch (Exception fbEx)
                        {
                            activityLogger.LogException($"ApplyWildcardMatch-FallbackLocalPorts-{baseName}", fbEx);
                            ruleCreationSkipped = true;
                        }
                    }

                    if (ruleCreationSkipped) return;
                    try
                    {
                        valueWithError = rule.RemotePorts;
                        if (protocol != 6 && protocol != 17) valueWithError = "*";
                        else if (string.IsNullOrEmpty(valueWithError)) valueWithError = "*";
                        firewallRule.RemotePorts = valueWithError;
                    }
                    catch (ArgumentException portEx)
                    {
                        activityLogger.LogException($"ApplyWildcardMatch-SetRemotePorts-{baseName}", portEx);
                        activityLogger.LogDebug($"[ApplyWildcardMatch] Falling back setting RemotePorts to '*' for rule '{baseName}' due to error: {portEx.Message}");
                        try
                        {
                            firewallRule.RemotePorts = "*";
                        }
                        catch (Exception fbEx)
                        {
                            activityLogger.LogException($"ApplyWildcardMatch-FallbackRemotePorts-{baseName}", fbEx);
                            ruleCreationSkipped = true;
                        }
                    }

                    if (ruleCreationSkipped) return;
                    try
                    {
                        valueWithError = rule.RemoteAddresses;
                        firewallRule.RemoteAddresses = valueWithError;
                    }
                    catch (ArgumentException addrEx)
                    {
                        activityLogger.LogException($"ApplyWildcardMatch-SetRemoteAddr-{baseName}", addrEx);
                        activityLogger.LogDebug($"[ApplyWildcardMatch] Falling back setting RemoteAddresses to '*' for rule '{baseName}' due to error: {addrEx.Message}");
                        try
                        {
                            firewallRule.RemoteAddresses = "*";
                        }
                        catch (Exception fbEx)
                        {
                            activityLogger.LogException($"ApplyWildcardMatch-FallbackRemoteAddr-{baseName}", fbEx);
                            ruleCreationSkipped = true;
                        }
                    }

                    if (ruleCreationSkipped) return;
                    firewallService.CreateRule(firewallRule);
                    activityLogger.LogDebug($"[ApplyWildcardMatch] Successfully created rule '{baseName}' from wildcard match.");
                }
                catch (COMException comEx)
                {
                    activityLogger.LogException($"ApplyWildcardMatch-CreateRuleCOM-{baseName}", comEx);
                }
                catch (Exception ex)
                {
                    activityLogger.LogException($"ApplyWildcardMatch-CreateRuleGeneral-{baseName}", ex);
                }
                finally
                {
                    if (firewallRule != null) Marshal.ReleaseComObject(firewallRule);
                }
            }

            var serviceNames = serviceName.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries);
            bool isSvcHost = Path.GetFileName(appPath).Equals("svchost.exe", StringComparison.OrdinalIgnoreCase);
            string appNameBase = Path.GetFileNameWithoutExtension(appPath);

            List<string?> servicesToCreateRulesFor;
            if (serviceNames.Length > 0)
            {
                servicesToCreateRulesFor = new List<string?>(serviceNames);
            }
            else if (isSvcHost)
            {
                servicesToCreateRulesFor = ["*"];
            }
            else
            {
                servicesToCreateRulesFor = [null];
            }

            foreach (var sName in servicesToCreateRulesFor)
            {
                string ruleNameBase = string.IsNullOrEmpty(sName) ?
                    appNameBase : (sName == "*" ? appNameBase : sName);

                if (rule.Protocol == ProtocolTypes.Any.Value)
                {
                    string actionStr = parsedAction == Actions.Allow ?
                        "" : "Block ";
                    var protocolsToCreate = new List<int> { 6, 17 };
                    foreach (var protocol in protocolsToCreate)
                    {
                        string protocolSuffix = (protocol == 6) ?
                            " - TCP" : " - UDP";
                        if (parsedDirection.HasFlag(Directions.Incoming))
                        {
                            createRule($"{ruleNameBase} - {actionStr}In{protocolSuffix}", Directions.Incoming, parsedAction, protocol, sName);
                        }
                        if (parsedDirection.HasFlag(Directions.Outgoing))
                        {
                            createRule($"{ruleNameBase} - {actionStr}Out{protocolSuffix}", Directions.Outgoing, parsedAction, protocol, sName);
                        }
                    }
                }
                else
                {
                    ApplyRuleAction(ruleNameBase, rule.Action, (name, dir, act) => createRule(name, dir, act, rule.Protocol, sName));
                }
            }

            activityLogger.LogChange("Wildcard Rule Applied", rule.Action + " for " + appPath);
        }


        public async Task<List<string>> CleanUpOrphanedRulesAsync(CancellationToken token, IProgress<int>? progress = null)
        {
            var orphanedRuleNames = new List<string>();
            var mfwRules = new List<INetFwRule2>();
            var allRules = firewallService.GetAllRules();

            try
            {
                foreach (var rule in allRules)
                {
                    if (IsMfwRule(rule))
                    {
                        mfwRules.Add(rule);
                    }
                    else
                    {
                        if (rule != null) Marshal.ReleaseComObject(rule);
                    }
                }

                int total = mfwRules.Count;
                if (total == 0)
                {
                    progress?.Report(100);
                    return orphanedRuleNames;
                }

                int processed = 0;
                await Task.Run(() =>
                {
                    foreach (var rule in mfwRules)
                    {
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }

                        string appPath = rule.ApplicationName;

                        if (!string.IsNullOrEmpty(appPath) && appPath != "*" && !appPath.StartsWith("@"))
                        {
                            string expandedPath = Environment.ExpandEnvironmentVariables(appPath);
                            if (!File.Exists(expandedPath))
                            {
                                orphanedRuleNames.Add(rule.Name);
                                activityLogger.LogDebug($"Found orphaned rule '{rule.Name}' for path: {expandedPath}");
                            }
                        }

                        processed++;
                        progress?.Report((processed * 100) / total);
                    }
                }, token);
                if (token.IsCancellationRequested)
                {
                    return new List<string>();
                }

                if (orphanedRuleNames.Any())
                {
                    activityLogger.LogDebug($"Deleting {orphanedRuleNames.Count} orphaned rules.");
                    try
                    {
                        firewallService.DeleteRulesByName(orphanedRuleNames);
                        activityLogger.LogChange("Orphaned Rules Cleaned", $"{orphanedRuleNames.Count} rules deleted.");
                    }
                    catch (COMException ex)
                    {
                        activityLogger.LogException("CleanUpOrphanedRulesAsync (Deletion)", ex);
                    }
                }
            }
            finally
            {
                foreach (var rule in mfwRules)
                {
                    if (rule != null) Marshal.ReleaseComObject(rule);
                }
            }

            return orphanedRuleNames;
        }

        public async Task<string> ExportAllMfwRulesAsync()
        {
            var advancedRules = await _dataService.GetAggregatedRulesAsync(CancellationToken.None);
            var portableAdvancedRules = advancedRules.SelectMany(ar => ar.UnderlyingRules)
                .Select(r =>
                {
                    r.ApplicationName = PathResolver.ConvertToEnvironmentPath(r.ApplicationName);
                    return r;
                }).ToList();
            var wildcardRules = _wildcardRuleService.GetRules()
                .Select(r =>
                {
                    r.FolderPath = PathResolver.ConvertToEnvironmentPath(r.FolderPath);
                    return r;
                }).ToList();
            var container = new ExportContainer
            {
                ExportDate = DateTime.UtcNow,
                AdvancedRules = portableAdvancedRules,
                WildcardRules = wildcardRules
            };
            return JsonSerializer.Serialize(container, ExportContainerJsonContext.Default.ExportContainer);
        }

        public async Task ImportRulesAsync(string jsonContent, bool replace)
        {
            if (BackgroundTaskService == null)
            {
                activityLogger.LogDebug("[Import] BackgroundTaskService is not available.");
                return;
            }

            try
            {
                var container = JsonSerializer.Deserialize(jsonContent, ExportContainerJsonContext.Default.ExportContainer);
                if (container == null)
                {
                    activityLogger.LogDebug("[Import] Failed to deserialize JSON content.");
                    return;
                }

                if (replace)
                {
                    BackgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.DeleteAllMfwRules, new object()));
                    await Task.Delay(1000);
                }

                foreach (var ruleVm in container.AdvancedRules)
                {
                    ruleVm.ApplicationName = PathResolver.ConvertFromEnvironmentPath(ruleVm.ApplicationName);
                    var payload = new CreateAdvancedRulePayload { ViewModel = ruleVm, InterfaceTypes = ruleVm.InterfaceTypes, IcmpTypesAndCodes = ruleVm.IcmpTypesAndCodes };
                    BackgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.CreateAdvancedRule, payload));
                }

                foreach (var wildcardRule in container.WildcardRules)
                {
                    wildcardRule.FolderPath = PathResolver.ConvertFromEnvironmentPath(wildcardRule.FolderPath);
                    BackgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.AddWildcardRule, wildcardRule));
                }

                activityLogger.LogChange("Rules Imported", $"Imported {container.AdvancedRules.Count} advanced rules and {container.WildcardRules.Count} wildcard rules. Replace: {replace}");
            }
            catch (JsonException ex)
            {
                activityLogger.LogException("ImportRules", ex);
            }
        }
    }
}