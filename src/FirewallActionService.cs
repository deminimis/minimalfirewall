// File: C:/Users/anon/PROGRAMMING/C#/SimpleFirewall/VS Minimal Firewall/MinimalFirewall-NET8/MinimalFirewall-WindowsStore/FirewallActionService.cs
// File: FirewallActionService.cs
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
        private readonly INetFwPolicy2 _firewallPolicy;
        private readonly TemporaryRuleManager _temporaryRuleManager;
        private readonly WildcardRuleService _wildcardRuleService;
        private readonly FirewallDataService _dataService;
        private readonly ConcurrentDictionary<string, System.Threading.Timer> _temporaryRuleTimers = new();
        private const string CryptoRuleName = "Minimal Firewall System - Certificate Checks";
        public FirewallActionsService(FirewallRuleService firewallService, UserActivityLogger activityLogger, FirewallEventListenerService eventListenerService, ForeignRuleTracker foreignRuleTracker, FirewallSentryService sentryService, PublisherWhitelistService whitelistService, INetFwPolicy2 firewallPolicy, WildcardRuleService wildcardRuleService, FirewallDataService dataService)
        {
            this.firewallService = firewallService;
            this.activityLogger = activityLogger;
            this.eventListenerService = eventListenerService;
            this.foreignRuleTracker = foreignRuleTracker;
            this.sentryService = sentryService;
            this._whitelistService = whitelistService;
            this._firewallPolicy = firewallPolicy;
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

        public void ApplyApplicationRuleChange(List<string> appPaths, string action, string? wildcardSourcePath = null)
        {
            var normalizedAppPaths = appPaths.Select(PathResolver.NormalizePath).Where(p => !string.IsNullOrEmpty(p)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var rulesToRemove = new List<string>();

            foreach (var appPath in normalizedAppPaths)
            {
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

                if (rulesToRemove.Any())
                {
                    firewallService.DeleteRulesByName(rulesToRemove);
                }

                string appName = Path.GetFileNameWithoutExtension(appPath);
                void createRule(string baseName, Directions dir, Actions act)
                {
                    string description = string.IsNullOrEmpty(wildcardSourcePath) ?
                        "" : $"{MFWConstants.WildcardDescriptionPrefix}{wildcardSourcePath}]";
                    CreateApplicationRule(baseName, appPath, dir, act, ProtocolTypes.Any.Value, description);
                }

                ApplyRuleAction(appName, action, createRule);
                activityLogger.LogChange("Rule Changed", action + " for " + appPath);
            }
        }

        public void ApplyServiceRuleChange(string serviceName, string action)
        {
            if (string.IsNullOrEmpty(serviceName)) return;
            try
            {
                firewallService.DeleteRulesByServiceName(serviceName);
            }
            catch (COMException ex)
            {
                activityLogger.LogException($"ApplyServiceRuleChange (Deleting old rules for {serviceName})", ex);
            }

            void createRule(string name, Directions dir, Actions act) =>
                CreateServiceRule(name, serviceName, dir, act, ProtocolTypes.Any.Value);
            ApplyRuleAction(serviceName, action, createRule);
            activityLogger.LogChange("Service Rule Changed", action + " for " + serviceName);
        }

        public void ApplyUwpRuleChange(List<UwpApp> uwpApps, string action)
        {
            var packageFamilyNames = uwpApps.Select(app => app.PackageFamilyName).ToList();
            try
            {
                firewallService.DeleteUwpRules(packageFamilyNames);
            }
            catch (COMException ex)
            {
                activityLogger.LogException($"ApplyUwpRuleChange (Deleting old rules)", ex);
            }

            foreach (var app in uwpApps)
            {
                void createRule(string name, Directions dir, Actions act) => CreateUwpRule(name, app.PackageFamilyName, dir, act, ProtocolTypes.Any.Value);
                ApplyRuleAction(app.Name, action, createRule);
                activityLogger.LogChange("UWP Rule Changed", action + " for " + app.Name);
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
            INetFwRule2?
            rule = null;
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

        public void ToggleLockdown()
        {
            var isCurrentlyLocked = firewallService.GetDefaultOutboundAction() == NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
            bool newLockdownState = !isCurrentlyLocked;

            try
            {
                AdminTaskService.SetAuditPolicy(newLockdownState);
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                activityLogger.LogException("SetAuditPolicy", ex);
            }

            ManageCryptoServiceRule(newLockdownState);
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

            activityLogger.LogChange("Lockdown Mode", newLockdownState ? "Enabled" : "Disabled");
            if (!newLockdownState)
            {
                ReenableMfwRules();
                activityLogger.LogDebug("All MFW rules re-enabled by per-rule toggle.");
            }
        }

        public void ProcessPendingConnection(PendingConnectionViewModel pending, string decision, TimeSpan duration = default, bool trustPublisher = false)
        {
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
                    string?
                    pathToRule = pending.AppPath;

                    if (!string.IsNullOrEmpty(pending.ServiceName))
                    {
                        var allServices = _dataService.GetCachedServicesWithExePaths();
                        var serviceInfo = allServices.FirstOrDefault(s => s.ServiceName.Equals(pending.ServiceName.Split(',')[0].Trim(), StringComparison.OrdinalIgnoreCase));
                        if (serviceInfo != null && !string.IsNullOrEmpty(serviceInfo.ExePath))
                        {
                            pathToRule = serviceInfo.ExePath;
                        }
                    }

                    if (!string.IsNullOrEmpty(pathToRule))
                    {
                        ApplyApplicationRuleChange([pathToRule], action);
                    }
                    break;
                case "TemporaryAllow":
                    eventListenerService.SnoozeNotificationsForApp(pending.AppPath, shortSnoozeDuration);
                    CreateTemporaryAllowRule(pending.AppPath, pending.Direction, duration);
                    break;

                case "Ignore":
                    eventListenerService.SnoozeNotificationsForApp(pending.AppPath, longSnoozeDuration);
                    activityLogger.LogDebug($"Ignored Connection: {pending.Direction} for {pending.AppPath}");
                    break;
            }
        }

        private void ReenableMfwRules()
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

        private void CreateTemporaryAllowRule(string appPath, string direction, TimeSpan duration)
        {
            if (!ParseActionString($"Allow ({direction})", out Actions parsedAction, out Directions parsedDirection)) return;
            string appName = Path.GetFileNameWithoutExtension(appPath);
            string guid = Guid.NewGuid().ToString();
            string description = "Temporarily allowed by Minimal Firewall.";
            string ruleName = $"Temp Allow - {appName} - {direction} - {guid}";

            CreateApplicationRule(ruleName, appPath, parsedDirection, parsedAction, ProtocolTypes.Any.Value, description);
            DateTime expiry = DateTime.UtcNow.Add(duration);
            _temporaryRuleManager.Add(ruleName, expiry);
            activityLogger.LogChange("Temporary Rule Created", $"Allowed {appPath} for {duration.TotalMinutes} minutes.");
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

        public void AcceptForeignRule(FirewallRuleChange change)
        {
            if (change.Rule?.Name is not null)
            {
                foreignRuleTracker.AcknowledgeRules([change.Rule.Name]);
                activityLogger.LogChange("Foreign Rule Accepted", change.Rule.Name);
            }
        }

        public void DeleteForeignRule(FirewallRuleChange change)
        {
            if (change.Rule?.Name is not null)
            {
                DeleteAdvancedRules([change.Rule.Name]);
            }
        }

        public void SetGroupEnabledState(string groupName, bool isEnabled)
        {
            INetFwRules? comRules = null;
            var rulesInGroup = new List<INetFwRule2>();
            try
            {
                comRules = _firewallPolicy.Rules;
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
            }
        }

        public void CreateAdvancedRule(AdvancedRuleViewModel vm, string interfaceTypes, string icmpTypesAndCodes)
        {
            if (!string.IsNullOrWhiteSpace(vm.ApplicationName))
            {
                vm.ApplicationName = PathResolver.NormalizePath(vm.ApplicationName);
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
                        Protocol = (short)protocol
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
            firewallRule.Name = vm.Name;
            firewallRule.Description = vm.Description;
            firewallRule.Enabled = vm.IsEnabled;
            firewallRule.Grouping = vm.Grouping;
            firewallRule.Action = vm.Status == "Allow" ?
                NET_FW_ACTION_.NET_FW_ACTION_ALLOW : NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
            firewallRule.Direction = (NET_FW_RULE_DIRECTION_)vm.Direction;
            firewallRule.Protocol = vm.Protocol;

            if (!string.IsNullOrWhiteSpace(vm.ApplicationName))
            {
                firewallRule.ApplicationName = vm.ApplicationName;
            }

            if (!string.IsNullOrWhiteSpace(vm.ServiceName))
            {
                firewallRule.serviceName = vm.ServiceName;
            }

            if (vm.Protocol != ProtocolTypes.TCP.Value && vm.Protocol != ProtocolTypes.UDP.Value)
            {
                firewallRule.LocalPorts = "*";
                firewallRule.RemotePorts = "*";
            }
            else
            {
                firewallRule.LocalPorts = vm.LocalPorts.Any() ?
                    string.Join(",", vm.LocalPorts.Select(p => p.ToString())) : "*";
                firewallRule.RemotePorts = vm.RemotePorts.Any() ? string.Join(",", vm.RemotePorts.Select(p => p.ToString())) : "*";
            }

            firewallRule.LocalAddresses = vm.LocalAddresses.Any() ?
                string.Join(",", vm.LocalAddresses.Select(a => a.ToString())) : "*";
            firewallRule.RemoteAddresses = vm.RemoteAddresses.Any() ? string.Join(",", vm.RemoteAddresses.Select(a => a.ToString())) : "*";

            NET_FW_PROFILE_TYPE2_ profiles = 0;
            if (vm.Profiles.Contains("Domain")) profiles |= NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_DOMAIN;
            if (vm.Profiles.Contains("Private")) profiles |= NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PRIVATE;
            if (vm.Profiles.Contains("Public")) profiles |= NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PUBLIC;
            if (profiles == 0) profiles = NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_ALL;
            firewallRule.Profiles = (int)profiles;

            firewallRule.InterfaceTypes = interfaceTypes;
            if (vm.Protocol == ProtocolTypes.ICMPv4.Value || vm.Protocol == ProtocolTypes.ICMPv6.Value)
            {
                if (!string.IsNullOrWhiteSpace(icmpTypesAndCodes))
                {
                    firewallRule.IcmpTypesAndCodes = icmpTypesAndCodes;
                }
            }

            firewallService.CreateRule(firewallRule);
            activityLogger.LogChange("Advanced Rule Created", vm.Name);
        }

        private static bool ParseActionString(string action, out Actions parsedAction, out Directions parsedDirection)
        {
            parsedAction = Actions.Allow;
            parsedDirection = Directions.Outgoing;
            if (string.IsNullOrEmpty(action)) return false;

            parsedAction = action.StartsWith("Allow", StringComparison.OrdinalIgnoreCase) ? Actions.Allow : Actions.Block;
            if (action.Contains("(All)"))
            {
                parsedDirection = Directions.Incoming |
                Directions.Outgoing;
            }
            else if (action.Contains("Inbound") || action.Contains("Incoming"))
            {
                parsedDirection = Directions.Incoming;
            }
            else
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
            var firewallRule = CreateRuleObject(name, appPath, direction, action, protocol, description);
            firewallService.CreateRule(firewallRule);
        }

        private void CreateServiceRule(string name, string serviceName, Directions direction, Actions action, int protocol)
        {
            var firewallRule = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule")!)!;
            firewallRule.Name = name;
            firewallRule.serviceName = serviceName;
            firewallRule.Direction = (NET_FW_RULE_DIRECTION_)direction;
            firewallRule.Action = (NET_FW_ACTION_)action;
            firewallRule.Protocol = protocol;
            firewallRule.Grouping = MFWConstants.MainRuleGroup;
            firewallRule.Enabled = true;
            firewallService.CreateRule(firewallRule);
        }

        private void CreateUwpRule(string name, string packageFamilyName, Directions direction, Actions action, int protocol)
        {
            var firewallRule = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule")!)!;
            firewallRule.Name = name;
            firewallRule.Description = MFWConstants.UwpDescriptionPrefix + packageFamilyName;
            firewallRule.Direction = (NET_FW_RULE_DIRECTION_)direction;
            firewallRule.Action = (NET_FW_ACTION_)action;
            firewallRule.Protocol = protocol;
            firewallRule.Grouping = MFWConstants.MainRuleGroup;
            firewallRule.Enabled = true;
            firewallService.CreateRule(firewallRule);
        }

        public async Task DeleteGroupAsync(string groupName)
        {
            await Task.Run(() =>
            {
                try
                {
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
    }
}