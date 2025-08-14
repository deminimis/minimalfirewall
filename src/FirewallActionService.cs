// FirewallActionsService.cs
using NetFwTypeLib;
using System.Data;
using System.IO;
using MinimalFirewall.TypedObjects;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace MinimalFirewall
{
    public partial class FirewallActionsService
    {
        private readonly FirewallRuleService firewallService;
        private readonly UserActivityLogger activityLogger;
        private readonly FirewallEventListenerService eventListenerService;
        private readonly ForeignRuleTracker foreignRuleTracker;
        private readonly FirewallSentryService sentryService;
        public FirewallActionsService(FirewallRuleService firewallService, UserActivityLogger activityLogger, FirewallEventListenerService eventListenerService, ForeignRuleTracker foreignRuleTracker, FirewallSentryService sentryService)
        {
            this.firewallService = firewallService;
            this.activityLogger = activityLogger;
            this.eventListenerService = eventListenerService;
            this.foreignRuleTracker = foreignRuleTracker;
            this.sentryService = sentryService;
        }

        public void ApplyApplicationRuleChange(List<string> appPaths, string action, string? wildcardSourcePath = null)
        {
            foreach (var appPath in appPaths)
            {
                if (string.IsNullOrEmpty(wildcardSourcePath))
                {
                    if (action.Contains("Inbound") || action.Contains("(All)"))
                    {
                        firewallService.DeleteRuleByPathAndDirection(appPath, NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN);
                    }
                    if (action.Contains("Outbound") || action.Contains("(All)"))
                    {
                        firewallService.DeleteRuleByPathAndDirection(appPath, NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT);
                    }
                }

                string appName = Path.GetFileNameWithoutExtension(appPath);
                void createRule(string name, Directions dir, Actions act)
                {
                    string description = string.IsNullOrEmpty(wildcardSourcePath) ? "" : $"{MFWConstants.WildcardDescriptionPrefix}{wildcardSourcePath}]";
                    CreateApplicationRule(name, appPath, dir, act, description);
                }
                ApplyRuleAction(appName, action, createRule);
                activityLogger.LogChange("Rule Changed", action + " for " + appPath);
            }
        }

        public void ApplyServiceRuleChange(string serviceName, string action)
        {
            if (string.IsNullOrEmpty(serviceName)) return;
            firewallService.DeleteRulesByServiceName(serviceName);

            void createRule(string name, Directions dir, Actions act)
            {
                var firewallRule = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule")!)!;
                firewallRule.WithName(name)
                            .ForService(serviceName)
                            .WithDirection(dir)
                            .WithAction(act)
                            .WithProtocol(ProtocolTypes.Any.Value)
                            .WithGrouping(MFWConstants.MainRuleGroup)
                            .IsEnabled();
                firewallService.CreateRule(firewallRule);
            }

            ApplyRuleAction(serviceName, action, createRule);
            activityLogger.LogChange("Service Rule Changed", action + " for " + serviceName);
        }

        public void ApplyUwpRuleChange(List<UwpApp> uwpApps, string action)
        {
            var packageFamilyNames = uwpApps.Select(app => app.PackageFamilyName).ToList();
            firewallService.DeleteUwpRules(packageFamilyNames);
            foreach (var app in uwpApps)
            {
                void createRule(string name, Directions dir, Actions act) => CreateUwpRule(name, app.PackageFamilyName, dir, act);
                ApplyRuleAction(app.Name, action, createRule);
                activityLogger.LogChange("UWP Rule Changed", action + " for " + app.Name);
            }
        }

        public void DeleteApplicationRules(List<string> appPaths)
        {
            if (appPaths.Count == 0) return;
            firewallService.DeleteRulesByPath(appPaths);
            foreach (var path in appPaths) activityLogger.LogChange("Rule Deleted", path);
        }

        public void DeleteRulesForWildcard(WildcardRule wildcard)
        {
            if (wildcard == null) return;
            string descriptionTag = $"{MFWConstants.WildcardDescriptionPrefix}{wildcard.FolderPath}]";
            firewallService.DeleteRulesByDescription(descriptionTag);
            activityLogger.LogChange("Wildcard Rules Deleted", $"Deleted rules for folder {wildcard.FolderPath}");
        }

        public void DeleteUwpRules(List<string> packageFamilyNames)
        {
            if (packageFamilyNames.Count == 0) return;
            firewallService.DeleteUwpRules(packageFamilyNames);
            foreach (var pfn in packageFamilyNames) activityLogger.LogChange("UWP Rule Deleted", pfn);
        }

        public void DeleteAdvancedRules(List<string> ruleNames)
        {
            if (ruleNames.Count == 0) return;
            firewallService.DeleteRulesByName(ruleNames);
            foreach (var name in ruleNames) activityLogger.LogChange("Advanced Rule Deleted", name);
        }

        public void ToggleLockdown()
        {
            var isCurrentlyLocked = firewallService.GetDefaultOutboundAction() == NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
            bool newLockdownState = !isCurrentlyLocked;

            AdminTaskService.SetAuditPolicy(newLockdownState);

            if (newLockdownState)
            {
                string? policyState = AdminTaskService.GetAuditPolicy();
                if (policyState == null || !policyState.Contains("Failure", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show(
                        "Failed to verify that Windows Security Auditing was enabled.\n\n" +
                        "The Lockdown dashboard will not be able to detect blocked connections.\n\n" +
                        "Potential Causes:\n" +
                        "1. A local or domain Group Policy is preventing this change.\n" +
                        "2. Other security software is blocking this action.\n\n" +
                        "The firewall's default policy will be set back to 'Allow' for safety.",
                        "Lockdown Mode Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    firewallService.SetDefaultOutboundAction(NET_FW_ACTION_.NET_FW_ACTION_ALLOW);
                    activityLogger.LogDebug("Lockdown Mode Failed: Could not enable audit policy.");
                    return;
                }
            }

            firewallService.SetDefaultOutboundAction(newLockdownState ? NET_FW_ACTION_.NET_FW_ACTION_BLOCK : NET_FW_ACTION_.NET_FW_ACTION_ALLOW);
            activityLogger.LogChange("Lockdown Mode", newLockdownState ? "Enabled" : "Disabled");
        }

        public void ProcessPendingConnection(PendingConnectionViewModel pending, string decision)
        {
            switch (decision)
            {
                case "Allow":
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
                case "Block":
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
                    eventListenerService.SnoozeNotificationsForApp(pending.AppPath, 2);
                    activityLogger.LogDebug($"Ignored Connection: {pending.Direction} for {pending.AppPath}");
                    break;
            }
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

            if (hasProgramOrService && vm.Protocol == ProtocolTypes.Any.Value)
            {
                var tcpVm = CopyViewModel(vm);
                tcpVm.Protocol = ProtocolTypes.TCP.Value;
                tcpVm.Name = vm.Name + " - TCP";
                CreateSingleAdvancedRule(tcpVm, interfaceTypes, icmpTypesAndCodes);

                var udpVm = CopyViewModel(vm);
                udpVm.Protocol = ProtocolTypes.UDP.Value;
                udpVm.Name = vm.Name + " - UDP";
                CreateSingleAdvancedRule(udpVm, interfaceTypes, icmpTypesAndCodes);
            }
            else
            {
                CreateSingleAdvancedRule(vm, interfaceTypes, icmpTypesAndCodes);
            }
        }

        private void CreateSingleAdvancedRule(AdvancedRuleViewModel vm, string interfaceTypes, string icmpTypesAndCodes)
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
                return false;
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

        private static INetFwRule2 CreateRuleObject(string name, string appPath, Directions direction, Actions action, string description = "")
        {
            var firewallRule = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule")!)!;
            firewallRule.WithName(name)
                        .ForApplication(appPath)
                        .WithDirection(direction)
                        .WithAction(action)
                        .WithProtocol(ProtocolTypes.Any.Value)
                        .WithDescription(description)
                        .IsEnabled();
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

        private void CreateApplicationRule(string name, string appPath, Directions direction, Actions action, string description)
        {
            var newRule = CreateRuleObject(name, appPath, direction, action, description);
            firewallService.CreateRule(newRule);
        }

        private void CreateUwpRule(string name, string packageFamilyName, Directions direction, Actions action)
        {
            var firewallRule = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule")!)!;
            firewallRule.WithName(name)
                        .WithDescription(MFWConstants.UwpDescriptionPrefix + packageFamilyName)
                        .WithDirection(direction)
                        .WithAction(action)
                        .WithProtocol(ProtocolTypes.Any.Value)
                        .WithGrouping(MFWConstants.MainRuleGroup)
                        .IsEnabled();
            firewallService.CreateRule(firewallRule);
        }

        public async Task DeleteGroupAsync(string groupName)
        {
            await Task.Run(() =>
            {
                firewallService.DeleteRulesByGroup(groupName);
            });
        }
    }
}