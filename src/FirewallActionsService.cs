using NetFwTypeLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;

namespace MinimalFirewall
{
    public class FirewallActionsService
    {
        private readonly FirewallRuleService _firewallService;
        private readonly FirewallDataService _dataService;
        private readonly UserActivityLogger _activityLogger;
        private readonly Dictionary<string, Timer> _activeTempRuleTimers = new Dictionary<string, Timer>();

        public FirewallActionsService(FirewallRuleService firewallService, FirewallDataService dataService, UserActivityLogger activityLogger)
        {
            _firewallService = firewallService;
            _dataService = dataService;
            _activityLogger = activityLogger;
        }

        public void ApplyApplicationRuleChange(List<string> appPaths, string action)
        {
            _firewallService.DeleteRulesByPath(appPaths);
            foreach (var appPath in appPaths)
            {
                string appName = Path.GetFileNameWithoutExtension(appPath);

                // Converted to local function per IDE0039
                void createRule(string name, NET_FW_RULE_DIRECTION_ dir, NET_FW_ACTION_ act) => CreateApplicationRule(name, appPath, dir, act);

                ApplyRuleAction(appName, action, createRule);
                _activityLogger.Log("Rule Changed", action + " for " + appPath);
                _dataService.AddOrUpdateAppRule(appPath);
            }
        }

        public void ApplyUwpRuleChange(List<UwpApp> uwpApps, string action)
        {
            var packageFamilyNames = uwpApps.Select(app => app.PackageFamilyName).ToList();
            _firewallService.DeleteUwpRules(packageFamilyNames);
            foreach (var app in uwpApps)
            {
                // Converted to local function per IDE0039
                void createRule(string name, NET_FW_RULE_DIRECTION_ dir, NET_FW_ACTION_ act) => CreateUwpRule(name, app.PackageFamilyName, dir, act);

                ApplyRuleAction(app.Name, action, createRule);
                _activityLogger.Log("UWP Rule Changed", action + " for " + app.Name);
            }
            _dataService.LoadInitialData();
        }

        public void DeleteApplicationRules(List<string> appPaths)
        {
            if (appPaths.Count == 0) return;
            var result = MessageBox.Show("Are you sure you want to delete all rules for " + appPaths.Count + " application(s)?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (result == MessageBoxResult.No) return;

            _firewallService.DeleteRulesByPath(appPaths);
            _dataService.RemoveRulesByPath(appPaths);
            foreach (var path in appPaths) _activityLogger.Log("Rule Deleted", path);
        }

        public void DeleteUwpRules(List<string> packageFamilyNames)
        {
            if (packageFamilyNames.Count == 0) return;
            var result = MessageBox.Show("Are you sure you want to delete all rules for " + packageFamilyNames.Count + " UWP app(s)?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (result == MessageBoxResult.No) return;
            _firewallService.DeleteUwpRules(packageFamilyNames);
            foreach (var pfn in packageFamilyNames) _activityLogger.Log("UWP Rule Deleted", pfn);
            _dataService.LoadInitialData();
        }

        public void DeleteAdvancedRules(List<string> ruleNames)
        {
            if (ruleNames.Count == 0) return;
            var result = MessageBox.Show("Are you sure you want to delete " + ruleNames.Count + " rule(s)?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (result == MessageBoxResult.No) return;

            _firewallService.DeleteRulesByName(ruleNames);
            _dataService.RemoveAdvancedRulesByName(ruleNames);
            foreach (var name in ruleNames) _activityLogger.Log("Advanced Rule Deleted", name);
        }

        public void CreatePowerShellRule(string command)
        {
            if (string.IsNullOrWhiteSpace(command)) return;
            AdminTaskService.ExecutePowerShellRuleCommand(command);
            _activityLogger.Log("Advanced Rule Created", command);
            _dataService.LoadInitialData();
        }

        public void ToggleLockdown()
        {
            var isCurrentlyLocked = _firewallService.GetDefaultOutboundAction() == NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
            bool newLockdownState = !isCurrentlyLocked;
            AdminTaskService.SetAuditPolicy(newLockdownState);
            _firewallService.SetDefaultOutboundAction(newLockdownState ? NET_FW_ACTION_.NET_FW_ACTION_BLOCK : NET_FW_ACTION_.NET_FW_ACTION_ALLOW);
            _activityLogger.Log("Lockdown Mode", newLockdownState ? "Enabled" : "Disabled");
        }

        public void AllowPendingConnectionTemporarily(PendingConnectionViewModel pending, int minutes)
        {
            string appName = Path.GetFileNameWithoutExtension(pending.AppPath);
            string tempRuleName = "Temp - " + appName + " (" + Guid.NewGuid().ToString("N") + ")";
            var dir = pending.Direction == "Outbound" ? NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT : NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
            var firewallRule = CreateRuleObject(tempRuleName, pending.AppPath, dir, NET_FW_ACTION_.NET_FW_ACTION_ALLOW);
            _firewallService.CreateRule(firewallRule);
            var timer = new Timer(_ => DeleteTemporaryRule(tempRuleName), null, TimeSpan.FromMinutes(minutes), Timeout.InfiniteTimeSpan);
            _activeTempRuleTimers[tempRuleName] = timer;
            _activityLogger.Log("Temporary Rule Created", minutes + " min for " + pending.AppPath);
            _dataService.LoadInitialData();
        }

        private void DeleteTemporaryRule(string ruleName)
        {
            _firewallService.DeleteRulesByName(new List<string> { ruleName });
            if (_activeTempRuleTimers.TryGetValue(ruleName, out var timer))
            {
                timer.Dispose();
                _activeTempRuleTimers.Remove(ruleName);
            }
            _activityLogger.Log("Temporary Rule Expired", ruleName);
            Application.Current.Dispatcher.Invoke(new Action(() => _dataService.LoadInitialData()));
        }

        private static void ApplyRuleAction(string appName, string action, Action<string, NET_FW_RULE_DIRECTION_, NET_FW_ACTION_> createRule)
        {
            switch (action)
            {
                case "Allow (All)": createRule(appName + " - In (MFW)", NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN, NET_FW_ACTION_.NET_FW_ACTION_ALLOW); createRule(appName + " - Out (MFW)", NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT, NET_FW_ACTION_.NET_FW_ACTION_ALLOW); break;
                case "Allow (Outbound)": createRule(appName + " - Out (MFW)", NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT, NET_FW_ACTION_.NET_FW_ACTION_ALLOW); break;
                case "Allow (Inbound)": createRule(appName + " - In (MFW)", NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN, NET_FW_ACTION_.NET_FW_ACTION_ALLOW); break;
                case "Block (All)": createRule(appName + " - Block In (MFW)", NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN, NET_FW_ACTION_.NET_FW_ACTION_BLOCK); createRule(appName + " - Block Out (MFW)", NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT, NET_FW_ACTION_.NET_FW_ACTION_BLOCK); break;
                case "Block (Outbound)": createRule(appName + " - Block Out (MFW)", NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT, NET_FW_ACTION_.NET_FW_ACTION_BLOCK); break;
                case "Block (Inbound)": createRule(appName + " - Block In (MFW)", NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN, NET_FW_ACTION_.NET_FW_ACTION_BLOCK); break;
            }
        }

        private static INetFwRule2 CreateRuleObject(string name, string appPath, NET_FW_RULE_DIRECTION_ direction, NET_FW_ACTION_ action)
        {
            var firewallRule = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
            firewallRule.Name = name;
            firewallRule.ApplicationName = appPath;
            firewallRule.Direction = direction;
            firewallRule.Action = action;
            firewallRule.Enabled = true;
            firewallRule.Protocol = 256;
            firewallRule.Grouping = "Minimal Firewall";
            return firewallRule;
        }

        private void CreateApplicationRule(string name, string appPath, NET_FW_RULE_DIRECTION_ direction, NET_FW_ACTION_ action)
        {
            var rule = CreateRuleObject(name, appPath, direction, action);
            _firewallService.CreateRule(rule);
        }

        private void CreateUwpRule(string name, string packageFamilyName, NET_FW_RULE_DIRECTION_ direction, NET_FW_ACTION_ action)
        {
            var firewallRule = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
            firewallRule.Name = name;
            firewallRule.Description = "UWP App; PFN=" + packageFamilyName;
            firewallRule.Direction = direction;
            firewallRule.Action = action;
            firewallRule.Enabled = true;
            firewallRule.Protocol = 256;
            firewallRule.Grouping = "Minimal Firewall";
            _firewallService.CreateRule(firewallRule);
        }
    }
}