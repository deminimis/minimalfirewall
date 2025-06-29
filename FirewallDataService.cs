using NetFwTypeLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MinimalFirewall
{
    public class FirewallDataService
    {
        private readonly FirewallRuleService _firewallService;
        private readonly UwpService _uwpService;
        private readonly UserActivityLogger _activityLogger;

        private readonly List<FirewallRuleViewModel> _masterProgramRules = new List<FirewallRuleViewModel>();
        private readonly List<FirewallRuleViewModel> _masterServiceRules = new List<FirewallRuleViewModel>();
        private readonly List<AdvancedRuleViewModel> _masterAdvancedRules = new List<AdvancedRuleViewModel>();
        private readonly List<AdvancedRuleViewModel> _masterForeignRules = new List<AdvancedRuleViewModel>();
        private readonly List<FirewallRuleViewModel> _filteredProgramRules = new List<FirewallRuleViewModel>();
        private readonly List<FirewallRuleViewModel> _filteredServiceRules = new List<FirewallRuleViewModel>();
        private readonly List<ProgramViewModel> _allUndefinedPrograms = new List<ProgramViewModel>();
        private readonly List<AdvancedRuleViewModel> _filteredAdvancedRules = new List<AdvancedRuleViewModel>();
        private readonly List<UwpApp> _allUwpApps = new List<UwpApp>();

        private readonly object _uwpLock = new object();

        public IReadOnlyList<FirewallRuleViewModel> AllProgramRules => _filteredProgramRules;
        public IReadOnlyList<FirewallRuleViewModel> AllServiceRules => _filteredServiceRules;
        public IReadOnlyList<ProgramViewModel> AllUndefinedPrograms => _allUndefinedPrograms;
        public IReadOnlyList<AdvancedRuleViewModel> AllAdvancedRules => _filteredAdvancedRules;
        public IReadOnlyList<AdvancedRuleViewModel> AllForeignRules => _masterForeignRules;
        public IReadOnlyList<UwpApp> AllUwpApps => _allUwpApps;
        public bool ShowSystemRules { get; set; }

        public FirewallDataService(FirewallRuleService firewallService, UwpService uwpService, UserActivityLogger activityLogger)
        {
            _firewallService = firewallService;
            _uwpService = uwpService;
            _activityLogger = activityLogger;
            ShowSystemRules = true;
        }

        public void ScanForForeignRules(ForeignRuleTracker tracker)
        {
            _masterForeignRules.Clear();
            var allRules = _firewallService.GetAllRules();

            var foreignRules = allRules.Where(r => r != null && (string.IsNullOrEmpty(r.Grouping) || !r.Grouping.StartsWith(MFWConstants.MainRuleGroup, StringComparison.OrdinalIgnoreCase)));

            foreach (var rule in foreignRules)
            {
                if (!tracker.IsAcknowledged(rule.Name))
                {
                    _masterForeignRules.Add(CreateAdvancedRuleViewModel(rule));
                }
            }
        }

        public void LoadInitialData()
        {
            _masterProgramRules.Clear();
            _masterServiceRules.Clear();
            _masterAdvancedRules.Clear();

            var allServices = SystemDiscoveryService.GetServicesWithExePaths();
            var allRules = _firewallService.GetAllRules();

            var rulesByAppPath = allRules
                .Where(r => !string.IsNullOrEmpty(r.ApplicationName))
                .GroupBy(r => r.ApplicationName, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.ToList());

            var rulesByServiceName = allRules
                .Where(r => !string.IsNullOrEmpty(r.serviceName))
                .GroupBy(r => r.serviceName, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.ToList());

            var allServicePaths = new HashSet<string>(allServices.Select(s => s.ExePath), StringComparer.OrdinalIgnoreCase);

            foreach (var service in allServices)
            {
                var relevantRules = new List<INetFwRule2>();

                if (rulesByServiceName.TryGetValue(service.ServiceName, out var serviceRules))
                {
                    relevantRules.AddRange(serviceRules);
                }

                if (rulesByAppPath.TryGetValue(service.ExePath, out var appRules))
                {
                    relevantRules.AddRange(appRules);
                }

                var status = relevantRules.Any() ? GetRuleStatusForGroup(relevantRules.Distinct()) : "Undefined";
                var displayName = string.IsNullOrEmpty(service.DisplayName) ? service.ServiceName : service.DisplayName;

                _masterServiceRules.Add(new FirewallRuleViewModel
                {
                    Name = displayName,
                    ApplicationName = service.ExePath,
                    Status = status
                });
            }

            foreach (var appGroup in rulesByAppPath)
            {
                if (!allServicePaths.Contains(appGroup.Key))
                {
                    if (appGroup.Value.All(r => r.Grouping != MFWConstants.WildcardRuleGroup))
                    {
                        _masterProgramRules.Add(new FirewallRuleViewModel
                        {
                            Name = Path.GetFileNameWithoutExtension(appGroup.Key),
                            ApplicationName = appGroup.Key,
                            Status = GetRuleStatusForGroup(appGroup.Value)
                        });
                    }
                }
            }

            var nonAppRules = allRules.Where(r => string.IsNullOrEmpty(r.ApplicationName) && string.IsNullOrEmpty(r.serviceName));
            foreach (var rule in nonAppRules)
            {
                if (!string.IsNullOrEmpty(rule.Description) && rule.Description.StartsWith(MFWConstants.UwpDescriptionPrefix, StringComparison.Ordinal))
                    continue;

                _masterAdvancedRules.Add(CreateAdvancedRuleViewModel(rule));
            }

            UpdateUwpAppStatuses(allRules);
            ApplyFilters();
        }

        public void ApplyFilters()
        {
            string windowsFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            bool shouldFilter(string path) => !ShowSystemRules && !string.IsNullOrEmpty(path) && path.StartsWith(windowsFolderPath, StringComparison.OrdinalIgnoreCase);

            _filteredProgramRules.Clear();
            _filteredProgramRules.AddRange(_masterProgramRules.Where(r => !shouldFilter(r.ApplicationName)));

            _filteredServiceRules.Clear();
            _filteredServiceRules.AddRange(_masterServiceRules.Where(r => !shouldFilter(r.ApplicationName)));

            _filteredAdvancedRules.Clear();
            _filteredAdvancedRules.AddRange(_masterAdvancedRules.Where(r => ShowSystemRules || (r.Grouping != null && r.Grouping.StartsWith(MFWConstants.MainRuleGroup))));
        }

        public void AddOrUpdateAppRule(string _appPath)
        {
            LoadInitialData();
        }

        public void ClearUndefinedPrograms()
        {
            _allUndefinedPrograms.Clear();
        }

        public void RemoveRulesByPath(List<string> appPaths)
        {
            var pathSet = new HashSet<string>(appPaths, StringComparer.OrdinalIgnoreCase);
            _masterProgramRules.RemoveAll(r => pathSet.Contains(r.ApplicationName));
            _masterServiceRules.RemoveAll(r => pathSet.Contains(r.ApplicationName));
        }

        public void RemoveAdvancedRulesByName(List<string> ruleNames)
        {
            var nameSet = new HashSet<string>(ruleNames, StringComparer.OrdinalIgnoreCase);
            _masterAdvancedRules.RemoveAll(r => nameSet.Contains(r.Name));
            ApplyFilters();
        }

        public bool DoesServiceRuleExist(string serviceName, string direction)
        {
            var allServices = SystemDiscoveryService.GetServicesWithExePaths();
            var targetService = allServices.FirstOrDefault(s => s.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase));
            if (targetService == null) return false;

            var displayName = string.IsNullOrEmpty(targetService.DisplayName) ? targetService.ServiceName : targetService.DisplayName;
            var rule = _masterServiceRules.FirstOrDefault(r => r.Name.Equals(displayName, StringComparison.OrdinalIgnoreCase));
            return DoesRuleMatchDirection(rule, direction);
        }

        public bool DoesRuleExist(string appPath, string direction, string serviceName)
        {
            if (!string.IsNullOrEmpty(serviceName))
            {
                var services = serviceName.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var service in services)
                {
                    if (DoesServiceRuleExist(service, direction))
                    {
                        return true;
                    }
                }
            }

            var rule = _masterProgramRules.FirstOrDefault(r => r.ApplicationName.Equals(appPath, StringComparison.OrdinalIgnoreCase)) ??
                       _masterServiceRules.FirstOrDefault(r => r.ApplicationName.Equals(appPath, StringComparison.OrdinalIgnoreCase));

            return DoesRuleMatchDirection(rule, direction);
        }

        private bool DoesRuleMatchDirection(FirewallRuleViewModel rule, string direction)
        {
            if (rule == null || rule.Status == "Undefined")
            {
                return false;
            }

            if (direction == "Outbound")
            {
                return rule.Status.Contains("Out") ||
                       rule.Status.Contains("(All)");
            }

            if (direction == "Inbound")
            {
                return rule.Status.Contains("In") ||
                       rule.Status.Contains("(All)");
            }

            return false;
        }

        public void LoadUwpAppsFromCache()
        {
            lock (_uwpLock)
            {
                _allUwpApps.Clear();
                _allUwpApps.AddRange(_uwpService.LoadUwpAppsFromCache());
            }
        }

        public async Task<int> ScanForUwpAppsAsync()
        {
            var apps = await _uwpService.ScanForUwpApps();
            lock (_uwpLock)
            {
                _allUwpApps.Clear();
                _allUwpApps.AddRange(apps);
                UpdateUwpAppStatuses(_firewallService.GetAllRules());
            }
            return apps.Count;
        }

        private void UpdateUwpAppStatuses(List<INetFwRule2> allRules)
        {
            var uwpRuleGroups = allRules.Where(r => r != null && !string.IsNullOrEmpty(r.Description) && r.Description.StartsWith(MFWConstants.UwpDescriptionPrefix, StringComparison.Ordinal))
                                        .GroupBy(r => r.Description.Substring(MFWConstants.UwpDescriptionPrefix.Length), StringComparer.OrdinalIgnoreCase);
            lock (_uwpLock)
            {
                foreach (var app in _allUwpApps)
                {
                    var group = uwpRuleGroups.FirstOrDefault(g => string.Equals(g.Key, app.PackageFamilyName, StringComparison.OrdinalIgnoreCase));
                    app.Status = group != null ? GetRuleStatusForGroup(group) : "Undefined";
                }
            }
        }

        public async Task<int> ScanDirectoryForUndefined(string directoryPath)
        {
            var newPrograms = await Task.Run(() =>
            {
                var existingPaths = _masterProgramRules.Select(r => r.ApplicationName)
                                   .Concat(_masterServiceRules.Select(r => r.ApplicationName))
                    .Concat(_allUndefinedPrograms.Select(p => p.ExePath))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
                return SystemDiscoveryService.ScanDirectoryForExecutables(directoryPath, existingPaths);
            });
            foreach (var program in newPrograms)
            {
                _allUndefinedPrograms.Add(program);
            }
            _activityLogger.Log("Directory Scanned", directoryPath);
            return newPrograms.Count;
        }

        public static string GetRuleStatusForGroup(IEnumerable<INetFwRule2> group)
        {
            bool hasInAllow = group.Any(r => r.Enabled && r.Action == NET_FW_ACTION_.NET_FW_ACTION_ALLOW && r.Direction == NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN);
            bool hasOutAllow = group.Any(r => r.Enabled && r.Action == NET_FW_ACTION_.NET_FW_ACTION_ALLOW && r.Direction == NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT);
            bool hasInBlock = group.Any(r => r.Enabled && r.Action == NET_FW_ACTION_.NET_FW_ACTION_BLOCK && r.Direction == NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN);
            bool hasOutBlock = group.Any(r => r.Enabled && r.Action == NET_FW_ACTION_.NET_FW_ACTION_BLOCK && r.Direction == NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT);

            if (hasInBlock && hasOutBlock) return "Block (All)";
            if (hasInAllow && hasOutAllow && !hasInBlock && !hasOutBlock) return "Allow (All)";

            var outStatus = hasOutBlock ? "Block Out" : (hasOutAllow ? "Allow Out" : "");
            var inStatus = hasInBlock ? "Block In" : (hasInAllow ? "Allow In" : "");

            var parts = new[] { outStatus, inStatus }.Where(s => !string.IsNullOrEmpty(s));
            var finalStatus = string.Join(", ", parts);

            return string.IsNullOrEmpty(finalStatus) ? "Undefined" : finalStatus;
        }

        public static AdvancedRuleViewModel CreateAdvancedRuleViewModel(INetFwRule2 rule)
        {
            return new AdvancedRuleViewModel
            {
                Name = rule.Name ?? "Unnamed Rule",
                Description = rule.Description ?? "N/A",
                IsEnabled = rule.Enabled,
                Status = rule.Action == NET_FW_ACTION_.NET_FW_ACTION_ALLOW ? "Allow" : "Block",
                Direction = rule.Direction == NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN ? "Inbound" : "Outbound",
                Ports = string.IsNullOrEmpty(rule.LocalPorts) || rule.LocalPorts == "*" ? "Any" : rule.LocalPorts,
                Protocol = GetProtocolString(rule.Protocol),
                ServiceName = string.IsNullOrEmpty(rule.serviceName) || rule.serviceName == "*" ? "Any" : rule.serviceName,
                RemoteAddresses = string.IsNullOrEmpty(rule.RemoteAddresses) || rule.RemoteAddresses == "*" ? "Any" : rule.RemoteAddresses,
                Profiles = GetProfileString(rule.Profiles),
                Grouping = rule.Grouping
            };
        }

        private static string GetProtocolString(int protocol)
        {
            switch (protocol)
            {
                case 1: return "ICMPv4";
                case 6: return "TCP";
                case 17: return "UDP";
                case 58: return "ICMPv6";
                case 256: return "Any";
                default: return protocol.ToString();
            }
        }

        private static string GetProfileString(int profiles)
        {
            if (profiles == (int)NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_ALL) return "All";
            var profileNames = new List<string>();
            if ((profiles & (int)NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_DOMAIN) != 0) profileNames.Add("Domain");
            if ((profiles & (int)NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PRIVATE) != 0) profileNames.Add("Private");
            if ((profiles & (int)NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PUBLIC) != 0) profileNames.Add("Public");
            return string.Join(", ", profileNames);
        }

        public void ClearDataCaches()
        {
            _masterProgramRules.Clear();
            _masterServiceRules.Clear();
            _masterAdvancedRules.Clear();
            _allUndefinedPrograms.Clear();
            _allUwpApps.Clear();

            _filteredProgramRules.Clear();
            _filteredServiceRules.Clear();
            _filteredAdvancedRules.Clear();
        }
    }
}