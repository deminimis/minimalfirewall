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
        public IReadOnlyList<UwpApp> AllUwpApps => _allUwpApps;
        public bool ShowSystemRules { get; set; }

        public FirewallDataService(FirewallRuleService firewallService, UwpService uwpService, UserActivityLogger activityLogger)
        {
            _firewallService = firewallService;
            _uwpService = uwpService;
            _activityLogger = activityLogger;
            ShowSystemRules = true;
        }

        public void LoadInitialData()
        {
            _masterProgramRules.Clear();
            _masterServiceRules.Clear();
            _masterAdvancedRules.Clear();
            _allUndefinedPrograms.Clear();

            var serviceExePaths = SystemDiscoveryService.GetServicesWithExePaths();
            var allRules = _firewallService.GetAllRules();
            var rulesByApp = allRules.Where(r => r != null && !string.IsNullOrEmpty(r.ApplicationName))
                                     .GroupBy(r => r.ApplicationName, StringComparer.OrdinalIgnoreCase)
                                     .ToDictionary(g => g.Key, g => g.AsEnumerable(), StringComparer.OrdinalIgnoreCase);

            var programRuleGroups = rulesByApp.Where(kvp => !serviceExePaths.ContainsKey(kvp.Key));
            foreach (var group in programRuleGroups)
            {
                _masterProgramRules.Add(new FirewallRuleViewModel
                {
                    Name = Path.GetFileNameWithoutExtension(group.Key),
                    ApplicationName = group.Key,
                    Status = GetRuleStatusForGroup(group.Value),
                    Icon = IconCacheService.GetIcon(group.Key)
                });
            }

            // *** THIS IS THE CORRECTED SECTION ***
            // Reverted the deconstruction to a standard foreach loop to fix compile errors.
            foreach (var service in serviceExePaths)
            {
                var appPath = service.Key;
                var serviceInfo = service.Value;
                var status = "Undefined";

                if (rulesByApp.TryGetValue(appPath, out var ruleGroup))
                {
                    status = GetRuleStatusForGroup(ruleGroup);
                }

                _masterServiceRules.Add(new FirewallRuleViewModel
                {
                    Name = string.IsNullOrEmpty(serviceInfo.DisplayName) ? serviceInfo.ServiceName : serviceInfo.DisplayName,
                    ApplicationName = appPath,
                    Status = status,
                    Icon = IconCacheService.GetIcon(appPath)
                });
            }

            foreach (var rule in allRules.Where(r => r != null && string.IsNullOrEmpty(r.ApplicationName) && (r.Description == null || !r.Description.StartsWith("UWP App;", StringComparison.Ordinal))))
            {
                _masterAdvancedRules.Add(CreateAdvancedRuleViewModel(rule));
            }

            UpdateUwpAppStatuses(allRules);
            ApplyFilters();
        }

        public void ApplyFilters()
        {
            string windowsFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);

            bool shouldFilter(string path) => !ShowSystemRules && path.StartsWith(windowsFolderPath, StringComparison.OrdinalIgnoreCase);

            _filteredProgramRules.Clear();
            _filteredProgramRules.AddRange(_masterProgramRules.Where(r => !shouldFilter(r.ApplicationName)));

            _filteredServiceRules.Clear();
            _filteredServiceRules.AddRange(_masterServiceRules.Where(r => !shouldFilter(r.ApplicationName)));

            _filteredAdvancedRules.Clear();
            _filteredAdvancedRules.AddRange(_masterAdvancedRules.Where(r => ShowSystemRules || !(r.Name.Contains("Windows Defender") || r.Name.StartsWith("Microsoft-"))));
        }

        public void AddOrUpdateAppRule(string appPath)
        {
            _masterProgramRules.RemoveAll(r => r.ApplicationName.Equals(appPath, StringComparison.OrdinalIgnoreCase));
            _masterServiceRules.RemoveAll(r => r.ApplicationName.Equals(appPath, StringComparison.OrdinalIgnoreCase));
            _allUndefinedPrograms.RemoveAll(p => p.ExePath.Equals(appPath, StringComparison.OrdinalIgnoreCase));

            var group = _firewallService.GetAllRules().Where(r => r != null && appPath.Equals(r.ApplicationName, StringComparison.OrdinalIgnoreCase));
            if (group.Any())
            {
                var newRuleVm = new FirewallRuleViewModel
                {
                    Name = Path.GetFileNameWithoutExtension(appPath),
                    ApplicationName = appPath,
                    Status = GetRuleStatusForGroup(group),
                    Icon = IconCacheService.GetIcon(appPath)
                };
                if (SystemDiscoveryService.GetServicesWithExePaths().ContainsKey(appPath))
                {
                    _masterServiceRules.Add(newRuleVm);
                }
                else
                {
                    _masterProgramRules.Add(newRuleVm);
                }
            }
        }

        public void RemoveRulesByPath(List<string> appPaths)
        {
            var pathSet = new HashSet<string>(appPaths, StringComparer.OrdinalIgnoreCase);
            _masterProgramRules.RemoveAll(r => pathSet.Contains(r.ApplicationName));
            _masterServiceRules.RemoveAll(r => pathSet.Contains(r.ApplicationName));
        }

        public void RemoveAdvancedRulesByName(List<string> ruleNames)
        {
            var nameSet = new HashSet<string>(ruleNames);
            _masterAdvancedRules.RemoveAll(r => nameSet.Contains(r.Name));
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
            var uwpRuleGroups = allRules.Where(r => r != null && !string.IsNullOrEmpty(r.Description) && r.Description.StartsWith("UWP App; PFN=", StringComparison.Ordinal))
                                        .GroupBy(r => r.Description.Substring("UWP App; PFN=".Length), StringComparer.OrdinalIgnoreCase);
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
                program.Icon = IconCacheService.GetIcon(program.ExePath);
                _allUndefinedPrograms.Add(program);
            }
            _activityLogger.Log("Directory Scanned", directoryPath);
            return newPrograms.Count;
        }

        public static string GetRuleStatusForGroup(IEnumerable<INetFwRule2> group)
        {
            bool hasInAllow = group.Any(r => r.Action == NET_FW_ACTION_.NET_FW_ACTION_ALLOW && r.Direction == NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN);
            bool hasOutAllow = group.Any(r => r.Action == NET_FW_ACTION_.NET_FW_ACTION_ALLOW && r.Direction == NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT);
            bool hasInBlock = group.Any(r => r.Action == NET_FW_ACTION_.NET_FW_ACTION_BLOCK && r.Direction == NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN);
            bool hasOutBlock = group.Any(r => r.Action == NET_FW_ACTION_.NET_FW_ACTION_BLOCK && r.Direction == NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT);

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
            var name = rule.Name ?? "Unnamed Rule";
            var description = rule.Description;
            var localPorts = rule.LocalPorts;
            var serviceName = rule.serviceName;
            var remoteAddresses = rule.RemoteAddresses;
            return new AdvancedRuleViewModel
            {
                Name = name,
                Description = (string.IsNullOrEmpty(description) || string.Equals(description, name, StringComparison.Ordinal)) ? "N/A" : description,
                IsEnabled = rule.Enabled,
                Status = rule.Action == NET_FW_ACTION_.NET_FW_ACTION_ALLOW ? "Allow" : "Block",
                Direction = rule.Direction == NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN ? "Inbound" : "Outbound",
                Ports = (string.IsNullOrEmpty(localPorts) || localPorts == "*") ? "Any" : localPorts,
                Protocol = GetProtocolString(rule.Protocol),
                ServiceName = (string.IsNullOrEmpty(serviceName) || serviceName == "*") ? "Any" : serviceName,
                RemoteAddresses = (string.IsNullOrEmpty(remoteAddresses) || remoteAddresses == "*") ? "Any" : remoteAddresses,
                Profiles = GetProfileString(rule.Profiles)
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
    }
}