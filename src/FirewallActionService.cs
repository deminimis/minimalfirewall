// File: FirewallActionsService.cs
using NetFwTypeLib;
using System.Data;
using System.IO;
using MinimalFirewall.TypedObjects;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Concurrent;

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
        private readonly FirewallDataService _dataService;
        private readonly ConcurrentDictionary<string, System.Threading.Timer> _temporaryRuleTimers = new();
        private const string CryptoRuleName = "Minimal Firewall System - Certificate Checks";
        public FirewallActionsService(FirewallRuleService firewallService, UserActivityLogger activityLogger, FirewallEventListenerService eventListenerService, ForeignRuleTracker foreignRuleTracker, FirewallSentryService sentryService, PublisherWhitelistService whitelistService, INetFwPolicy2 firewallPolicy, FirewallDataService dataService)
        {
            this.firewallService = firewallService;
            this.activityLogger = activityLogger;
            this.eventListenerService = eventListenerService;
            this.foreignRuleTracker = foreignRuleTracker;
            this.sentryService = sentryService;
            this._whitelistService = whitelistService;
            this._firewallPolicy = firewallPolicy;
            _temporaryRuleManager = new TemporaryRuleManager();
            _dataService = dataService;
        }

        public void CleanupTemporaryRulesOnStartup()
        {
            var expiredRules = _temporaryRuleManager.GetExpiredRules();
            if (expiredRules.Any())
            {
                var ruleNamesToRemove = expiredRules.Keys.ToList();
                firewallService.DeleteRulesByName(ruleNamesToRemove);
                foreach (var ruleName in ruleNamesToRemove)
                {
                    _temporaryRuleManager.Remove(ruleName);
                }
                activityLogger.LogDebug($"Cleaned up {ruleNamesToRemove.Count} expired temporary rules on startup.");
            }
        }

        public void ApplyApplicationRuleChange(List<string> appPaths, string action, string? wildcardSourcePath = null)
        {
            var normalizedAppPaths = appPaths.Select(PathResolver.NormalizePath).Where(p => !string.IsNullOrEmpty(p)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var rulesToAdd = new List<AdvancedRuleViewModel>();
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

                string appName = Path.GetFileNameWithoutExtension(appPath);
                void createTcpAndUdpRules(string baseName, Directions dir, Actions act)
                {
                    string description = string.IsNullOrEmpty(wildcardSourcePath) ? "" : $"{MFWConstants.WildcardDescriptionPrefix}{wildcardSourcePath}]";
                    rulesToAdd.Add(FirewallDataService.CreateAdvancedRuleViewModel(CreateApplicationRule(baseName + " - TCP", appPath, dir, act, ProtocolTypes.TCP.Value, description)));
                    rulesToAdd.Add(FirewallDataService.CreateAdvancedRuleViewModel(CreateApplicationRule(baseName + " - UDP", appPath, dir, act, ProtocolTypes.UDP.Value, description)));
                }

                ApplyRuleAction(appName, action, createTcpAndUdpRules);
                activityLogger.LogChange("Rule Changed", action + " for " + appPath);
            }

            if (rulesToRemove.Count > 0)
            {
                firewallService.DeleteRulesByName(rulesToRemove);
                _dataService.RemoveRulesFromCache(rulesToRemove);
            }
            if (rulesToAdd.Count > 0)
            {
                _dataService.AddRulesToCache(rulesToAdd);
            }
        }

        public void ApplyServiceRuleChange(string serviceName, string action)
        {
            if (string.IsNullOrEmpty(serviceName)) return;
            var existingRuleNames = firewallService.DeleteRulesByServiceName(serviceName);
            _dataService.RemoveRulesFromCache(existingRuleNames);
            var rulesToAdd = new List<AdvancedRuleViewModel>();
            void createRuleForProtocol(string name, Directions dir, Actions act, short protocol)
            {
                var firewallRule = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule")!)!;
                firewallRule.WithName(name)
                            .ForService(serviceName)
                            .WithDirection(dir)
                            .WithAction(act)
                            .WithProtocol(protocol)
                            .WithGrouping(MFWConstants.MainRuleGroup)
                            .IsEnabled();
                firewallRule.Profiles = (int)NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_ALL;
                firewallRule.InterfaceTypes = "All";
                firewallService.CreateRule(firewallRule);
                rulesToAdd.Add(FirewallDataService.CreateAdvancedRuleViewModel(firewallRule));
            }

            void createTcpAndUdpRules(string baseName, Directions dir, Actions act)
            {
                createRuleForProtocol(baseName + " - TCP", dir, act, ProtocolTypes.TCP.Value);
                createRuleForProtocol(baseName + " - UDP", dir, act, ProtocolTypes.UDP.Value);
            }

            ApplyRuleAction(serviceName, action, createTcpAndUdpRules);
            if (rulesToAdd.Count > 0)
            {
                _dataService.AddRulesToCache(rulesToAdd);
            }

            activityLogger.LogChange("Service Rule Changed", action + " for " + serviceName);
        }

        public void ApplyUwpRuleChange(List<UwpApp> uwpApps, string action)
        {
            var packageFamilyNames = uwpApps.Select(app => app.PackageFamilyName).ToList();
            var ruleNamesToDelete = firewallService.DeleteUwpRules(packageFamilyNames);
            _dataService.RemoveRulesFromCache(ruleNamesToDelete);

            var rulesToAdd = new List<AdvancedRuleViewModel>();
            foreach (var app in uwpApps)
            {
                void createRule(string name, Directions dir, Actions act) => rulesToAdd.Add(FirewallDataService.CreateAdvancedRuleViewModel(CreateUwpRule(name, app.PackageFamilyName, dir, act)));
                ApplyRuleAction(app.Name, action, createRule);
                activityLogger.LogChange("UWP Rule Changed", action + " for " + app.Name);
            }

            if (rulesToAdd.Count > 0)
            {
                _dataService.AddRulesToCache(rulesToAdd);
            }
        }

        public void DeleteApplicationRules(List<string> appPaths)
        {
            if (appPaths.Count == 0) return;
            var ruleNames = firewallService.DeleteRulesByPath(appPaths);
            _dataService.RemoveRulesFromCache(ruleNames);
            foreach (var path in appPaths) activityLogger.LogChange("Rule Deleted", path);
        }

        public void DeleteRulesForWildcard(WildcardRule wildcard)
        {
            if (wildcard == null) return;
            string descriptionTag = $"{MFWConstants.WildcardDescriptionPrefix}{wildcard.FolderPath}]";
            var ruleNames = firewallService.DeleteRulesByDescription(descriptionTag);
            _dataService.RemoveRulesFromCache(ruleNames);
            activityLogger.LogChange("Wildcard Rules Deleted", $"Deleted rules for folder {wildcard.FolderPath}");
        }

        public void DeleteUwpRules(List<string> packageFamilyNames)
        {
            if (packageFamilyNames.Count == 0) return;
            var ruleNames = firewallService.DeleteUwpRules(packageFamilyNames);
            _dataService.RemoveRulesFromCache(ruleNames);
            foreach (var pfn in packageFamilyNames) activityLogger.LogChange("UWP Rule Deleted", pfn);
        }

        public void DeleteAdvancedRules(List<string> ruleNames)
        {
            if (ruleNames.Count == 0) return;
            firewallService.DeleteRulesByName(ruleNames);
            _dataService.RemoveRulesFromCache(ruleNames);
            foreach (var name in ruleNames) activityLogger.LogChange("Advanced Rule Deleted", name);
        }

        private void ManageCryptoServiceRule(bool enable)
        {
            var rule = firewallService.GetAllRules().FirstOrDefault(r => r.Name == CryptoRuleName) as INetFwRule2;
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

        public void ToggleLockdown()
        {
            var isCurrentlyLocked = firewallService.GetDefaultOutboundAction() == NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
            bool newLockdownState = !isCurrentlyLocked;

            try
            {
                AdminTaskService.SetAuditPolicy(newLockdownState);
            }
            catch (Exception ex)
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
                    "The firewall's default policy will be set back " +
                    "to 'Allow' for safety.",
                    "Lockdown Mode Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                try
                {
                    firewallService.SetDefaultOutboundAction(NET_FW_ACTION_.NET_FW_ACTION_ALLOW);
                }
                catch (Exception ex)
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
            catch (Exception ex)
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

            switch (decision)
            {
                case "Allow":
                    eventListenerService.SnoozeNotificationsForApp(pending.AppPath, shortSnoozeDuration);
                    string allowAction = "Allow (" + pending.Direction + ")";
                    if (!string.IsNullOrEmpty(pending.ServiceName))
                    {
                        var services = pending.ServiceName.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries);
                        foreach (var service in services) ApplyServiceRuleChange(service, allowAction);
                    }
                    else
                    {
                        ApplyApplicationRuleChange([pending.AppPath], allowAction);
                    }
                    break;
                case "TemporaryAllow":
                    eventListenerService.SnoozeNotificationsForApp(pending.AppPath, shortSnoozeDuration);
                    CreateTemporaryAllowRule(pending.AppPath, pending.Direction, duration);
                    break;
                case "Block":
                    eventListenerService.SnoozeNotificationsForApp(pending.AppPath, shortSnoozeDuration);
                    string blockAction = "Block (" + pending.Direction + ")";
                    if (!string.IsNullOrEmpty(pending.ServiceName))
                    {
                        var services = pending.ServiceName.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries);
                        foreach (var service in services) ApplyServiceRuleChange(service, blockAction);
                    }
                    else
                    {
                        ApplyApplicationRuleChange([pending.AppPath], blockAction);
                    }
                    break;
                case "Ignore":
                    eventListenerService.SnoozeNotificationsForApp(pending.AppPath, longSnoozeDuration);
                    activityLogger.LogDebug($"Ignored Connection: {pending.Direction} for {pending.AppPath}");
                    break;
            }
        }

        private void ReenableMfwRules()
        {
            var mfwRules = firewallService.GetApplicationRules();
            foreach (var rule in mfwRules)
            {
                try
                {
                    if (!rule.Enabled) rule.Enabled = true;
                }
                catch (Exception ex)
                {
                    activityLogger.LogException($"Enable rule '{rule.Name}'", ex);
                }
            }
        }

        private void CreateTemporaryAllowRule(string appPath, string direction, TimeSpan duration)
        {
            string appName = Path.GetFileNameWithoutExtension(appPath);
            string ruleNameTcp = $"Temp Allow - {appName} - TCP - {Guid.NewGuid()}";
            string ruleNameUdp = $"Temp Allow - {appName} - UDP - {Guid.NewGuid()}";
            string action = $"Allow ({direction})";
            DateTime expiry = DateTime.UtcNow.Add(duration);
            string description = "Temporarily allowed by Minimal Firewall.";
            var rulesToAdd = new List<AdvancedRuleViewModel>();
            ApplyRuleAction(appName, action, (baseName, dir, act) =>
            {
                rulesToAdd.Add(FirewallDataService.CreateAdvancedRuleViewModel(CreateApplicationRule(ruleNameTcp, appPath, dir, act, ProtocolTypes.TCP.Value, description)));
                rulesToAdd.Add(FirewallDataService.CreateAdvancedRuleViewModel(CreateApplicationRule(ruleNameUdp, appPath, dir, act, ProtocolTypes.UDP.Value, description)));
            });
            if (rulesToAdd.Count > 0)
            {
                _dataService.AddRulesToCache(rulesToAdd);
            }

            _temporaryRuleManager.Add(ruleNameTcp, expiry);
            _temporaryRuleManager.Add(ruleNameUdp, expiry);
            activityLogger.LogChange("Temporary Rule Created", $"Allowed {appPath} for {duration.TotalMinutes} minutes.");
            var timerTcp = new System.Threading.Timer(_ =>
            {
                firewallService.DeleteRulesByName([ruleNameTcp]);
                _dataService.RemoveRulesFromCache([ruleNameTcp]);
                _temporaryRuleManager.Remove(ruleNameTcp);
                if (_temporaryRuleTimers.TryRemove(ruleNameTcp, out var timer))
                {
                    timer.Dispose();
                }
                activityLogger.LogDebug($"Temporary rule {ruleNameTcp} expired and was removed.");
            }, null, duration, Timeout.InfiniteTimeSpan);
            var timerUdp = new System.Threading.Timer(_ =>
            {
                firewallService.DeleteRulesByName([ruleNameUdp]);
                _dataService.RemoveRulesFromCache([ruleNameUdp]);
                _temporaryRuleManager.Remove(ruleNameUdp);
                if (_temporaryRuleTimers.TryRemove(ruleNameUdp, out var timer))
                {
                    timer.Dispose();
                }
                activityLogger.LogDebug($"Temporary rule {ruleNameUdp} expired and was removed.");
            }, null, duration, Timeout.InfiniteTimeSpan);
            _temporaryRuleTimers[ruleNameTcp] = timerTcp;
            _temporaryRuleTimers[ruleNameUdp] = timerUdp;
        }

        public void AcceptForeignRule(FirewallRuleChange change)
        {
            if (change.Rule?.Name is null) return;
            var rule = firewallService.GetAllRules().FirstOrDefault(r => r.Name == change.Rule.Name);
            if (rule != null)
            {
                try
                {
                    rule.Grouping = MFWConstants.MainRuleGroup;
                    activityLogger.LogChange("Foreign Rule Accepted", rule.Name);
                    sentryService.CreateBaseline();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ERROR] Failed to accept foreign rule '{rule.Name}': {ex.Message}");
                }
            }
        }

        public void AcknowledgeForeignRule(FirewallRuleChange change)
        {
            if (change.Rule?.Name is not null)
            {
                foreignRuleTracker.AcknowledgeRules([change.Rule.Name]);
                sentryService.CreateBaseline();
            }
        }

        public void DeleteForeignRule(FirewallRuleChange change)
        {
            if (change.Rule?.Name is not null)
            {
                DeleteAdvancedRules([change.Rule.Name]);
                sentryService.CreateBaseline();
            }
        }

        public void AcceptAllForeignRules(List<FirewallRuleChange> changes)
        {
            if (changes == null || changes.Count == 0) return;
            var allRules = firewallService.GetAllRules().ToDictionary(r => r.Name);
            foreach (var change in changes)
            {
                if (change.Rule?.Name != null && allRules.TryGetValue(change.Rule.Name, out var rule))
                {
                    try
                    {
                        rule.Grouping = MFWConstants.MainRuleGroup;
                        activityLogger.LogChange("Foreign Rule Accepted", rule.Name);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[ERROR] Failed to accept foreign rule '{rule.Name}': {ex.Message}");
                    }
                }
            }
            sentryService.CreateBaseline();
        }

        public void AcknowledgeAllForeignRules(List<FirewallRuleChange> changes)
        {
            if (changes == null || changes.Count == 0) return;
            var ruleNames = changes.Select(c => c.Rule?.Name).Where(n => n != null).Select(n => n!).ToList();
            if (ruleNames.Any())
            {
                foreignRuleTracker.AcknowledgeRules(ruleNames);
                sentryService.CreateBaseline();
            }
        }

        private AdvancedRuleViewModel CopyViewModel(AdvancedRuleViewModel source)
        {
            return new AdvancedRuleViewModel
            {
                Name = source.Name,
                Status = source.Status,
                IsEnabled = source.IsEnabled,
                Direction = source.Direction,
                LocalPorts = source.LocalPorts,
                RemotePorts = source.RemotePorts,
                Protocol = source.Protocol,
                ProtocolName = source.ProtocolName,
                ApplicationName = source.ApplicationName,
                ServiceName = source.ServiceName,
                LocalAddresses = source.LocalAddresses,
                RemoteAddresses = source.RemoteAddresses,
                Profiles = source.Profiles,
                Description = source.Description,
                Grouping = source.Grouping,
                Type = source.Type
            };
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

            var directionsToCreate = new List<Directions>();
            if (vm.Direction.HasFlag(Directions.Incoming)) directionsToCreate.Add(Directions.Incoming);
            if (vm.Direction.HasFlag(Directions.Outgoing)) directionsToCreate.Add(Directions.Outgoing);

            var rulesToAdd = new List<AdvancedRuleViewModel>();
            foreach (var direction in directionsToCreate)
            {
                var directedVm = CopyViewModel(vm);
                directedVm.Direction = direction;
                directedVm.Name = vm.Name + (directionsToCreate.Count > 1 ? $" - {direction}" : "");
                if (hasProgramOrService && directedVm.Protocol == ProtocolTypes.Any.Value)
                {
                    var tcpVm = CopyViewModel(directedVm);
                    tcpVm.Protocol = ProtocolTypes.TCP.Value;
                    tcpVm.Name = directedVm.Name + " - TCP";
                    rulesToAdd.Add(CreateSingleAdvancedRule(tcpVm, interfaceTypes, icmpTypesAndCodes));

                    var udpVm = CopyViewModel(directedVm);
                    udpVm.Protocol = ProtocolTypes.UDP.Value;
                    udpVm.Name = directedVm.Name + " - UDP";
                    rulesToAdd.Add(CreateSingleAdvancedRule(udpVm, interfaceTypes, icmpTypesAndCodes));
                }
                else
                {
                    rulesToAdd.Add(CreateSingleAdvancedRule(directedVm, interfaceTypes, icmpTypesAndCodes));
                }
            }

            if (rulesToAdd.Count > 0)
            {
                _dataService.AddRulesToCache(rulesToAdd);
            }
        }

        private AdvancedRuleViewModel CreateSingleAdvancedRule(AdvancedRuleViewModel vm, string interfaceTypes, string icmpTypesAndCodes)
        {
            var firewallRule = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule")!)!;
            firewallRule.Name = vm.Name;
            firewallRule.Description = vm.Description;
            firewallRule.Enabled = vm.IsEnabled;
            firewallRule.Grouping = vm.Grouping;
            firewallRule.Action = vm.Status == "Allow" ? NET_FW_ACTION_.NET_FW_ACTION_ALLOW : NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
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
                firewallRule.LocalPorts = vm.LocalPorts.Any() ? string.Join(",", vm.LocalPorts.Select(p => p.ToString())) : "*";
                firewallRule.RemotePorts = vm.RemotePorts.Any() ? string.Join(",", vm.RemotePorts.Select(p => p.ToString())) : "*";
            }

            firewallRule.LocalAddresses = vm.LocalAddresses.Any() ? string.Join(",", vm.LocalAddresses.Select(a => a.ToString())) : "*";
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
            return FirewallDataService.CreateAdvancedRuleViewModel(firewallRule);
        }

        private static bool ParseActionString(string action, out Actions parsedAction, out Directions parsedDirection)
        {
            parsedAction = Actions.Allow;
            parsedDirection = Directions.Outgoing;

            if (string.IsNullOrEmpty(action)) return false;

            parsedAction = action.StartsWith("Allow", StringComparison.OrdinalIgnoreCase) ? Actions.Allow : Actions.Block;
            if (action.Contains("(All)"))
            {
                parsedDirection = Directions.Incoming | Directions.Outgoing;
            }
            else if (action.Contains("Inbound"))
            {
                parsedDirection = Directions.Incoming;
            }
            else if (action.Contains("Outbound"))
            {
                parsedDirection = Directions.Outgoing;
            }
            else
            {
                if (action.Contains("In", StringComparison.OrdinalIgnoreCase))
                {
                    parsedDirection = Directions.Incoming;
                }
                else
                {
                    parsedDirection = Directions.Outgoing;
                }
            }

            return true;
        }

        private static void ApplyRuleAction(string appName, string action, Action<string, Directions, Actions> createRule)
        {
            if (!ParseActionString(action, out Actions parsedAction, out Directions parsedDirection))
            {
                return;
            }

            string actionStr = parsedAction == Actions.Allow ? "" : "Block ";
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

        private static INetFwRule2 CreateRuleObject(string name, string appPath, Directions direction, Actions action, short protocol, string description = "")
        {
            var firewallRule = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule")!)!;
            firewallRule.WithName(name)
                        .ForApplication(appPath)
                        .WithDirection(direction)
                        .WithAction(action)
                        .WithProtocol(protocol)
                        .WithDescription(description)
                        .IsEnabled();
            firewallRule.Profiles = (int)NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_ALL;
            firewallRule.InterfaceTypes = "All";
            if (!string.IsNullOrEmpty(description) && description.StartsWith(MFWConstants.WildcardDescriptionPrefix))
            {
                firewallRule.WithGrouping(MFWConstants.WildcardRuleGroup);
            }
            else
            {
                firewallRule.WithGrouping(MFWConstants.MainRuleGroup);
            }
            return firewallRule;
        }

        private INetFwRule2 CreateApplicationRule(string name, string appPath, Directions direction, Actions action, short protocol, string description)
        {
            var firewallRule = CreateRuleObject(name, appPath, direction, action, protocol, description);
            firewallService.CreateRule(firewallRule);
            return firewallRule;
        }

        private INetFwRule2 CreateUwpRule(string name, string packageFamilyName, Directions direction, Actions action)
        {
            var firewallRule = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule")!)!;
            firewallRule.WithName(name)
                        .WithDescription(MFWConstants.UwpDescriptionPrefix + packageFamilyName)
                        .WithDirection(direction)
                        .WithAction(action)
                        .WithProtocol(ProtocolTypes.Any.Value)
                        .WithGrouping(MFWConstants.MainRuleGroup)
                        .IsEnabled();
            firewallRule.Profiles = (int)NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_ALL;
            firewallRule.InterfaceTypes = "All";
            firewallService.CreateRule(firewallRule);
            return firewallRule;
        }

        public async Task DeleteGroupAsync(string groupName)
        {
            await Task.Run(() =>
            {
                var ruleNames = firewallService.DeleteRulesByGroup(groupName);
                _dataService.RemoveRulesFromCache(ruleNames);
            });
        }
    }
}