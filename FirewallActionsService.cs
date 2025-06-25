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

        public event Action<string> ApplicationRuleSetExpired;

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

        // THIS METHOD IS REWRITTEN TO BE DIRECTIONAL
        public void AllowPendingConnectionTemporarily(PendingConnectionViewModel pending, int minutes)
        {
            string appName = Path.GetFileNameWithoutExtension(pending.AppPath);
            string guid = Guid.NewGuid().ToString("N").Substring(0, 8);

            // Determine direction from the pending connection
            NET_FW_RULE_DIRECTION_ directionEnum;
            string directionString;

            if (pending.Direction == "Outbound")
            {
                directionEnum = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT;
                directionString = "Out";
            }
            else
            {
                directionEnum = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
                directionString = "In";
            }

            // Create only one rule for the specific direction
            string tempRuleName = $"Temp Allow {directionString} - {appName} ({guid})";
            var tempRule = CreateRuleObject(tempRuleName, pending.AppPath, directionEnum, NET_FW_ACTION_.NET_FW_ACTION_ALLOW);
            _firewallService.CreateRule(tempRule);

            // Treat this as a normal program rule update so it appears in the main list
            _dataService.AddOrUpdateAppRule(pending.AppPath);
            _activityLogger.Log("Temporary Rule Created", $"{minutes} min for {pending.AppPath} ({directionString})");

            // Set a timer to delete the specific rule
            var timer = new Timer(_ => DeleteTemporaryRule(tempRuleName, pending.AppPath), null, TimeSpan.FromMinutes(minutes), Timeout.InfiniteTimeSpan);

            _activeTempRuleTimers[tempRuleName] = timer;
        }

        private void DeleteTemporaryRule(string ruleName, string appPath)
        {
            _firewallService.DeleteRulesByName(new List<string> { ruleName });

            if (_activeTempRuleTimers.ContainsKey(ruleName))
            {
                _activeTempRuleTimers[ruleName].Dispose();
                _activeTempRuleTimers.Remove(ruleName);
            }

            _activityLogger.Log("Temporary Rule Expired", ruleName);
            ApplicationRuleSetExpired?.Invoke(appPath);
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