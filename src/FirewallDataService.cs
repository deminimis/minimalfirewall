// File: FirewallDataService.cs
using NetFwTypeLib;
using System.IO;
using System.Linq;
using MinimalFirewall.TypedObjects;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MinimalFirewall
{
    public class FirewallDataService
    {
        private readonly FirewallRuleService _firewallService;
        private readonly WildcardRuleService _wildcardRuleService;
        private readonly RuleCacheService _ruleCacheService;
        private readonly HashSet<string> _knownRulePaths = new(StringComparer.OrdinalIgnoreCase);
        public FirewallDataService(FirewallRuleService firewallService, WildcardRuleService wildcardRuleService, RuleCacheService ruleCacheService)
        {
            _firewallService = firewallService;
            _wildcardRuleService = wildcardRuleService;
            _ruleCacheService = ruleCacheService;
        }

        public void RemoveRulesFromCache(List<string> ruleNames)
        {
            var rules = LoadAdvancedRules();
            if (rules.Count == 0) return;

            var nameSet = new HashSet<string>(ruleNames, StringComparer.OrdinalIgnoreCase);
            var updatedRules = rules.Where(r => !nameSet.Contains(r.Name)).ToList();
            if (updatedRules.Count < rules.Count)
            {
                _ruleCacheService.UpdateCache(null, updatedRules);
            }
        }

        public void AddRulesToCache(List<AdvancedRuleViewModel> newRules)
        {
            var rules = LoadAdvancedRules();
            rules.AddRange(newRules);
            var sortedRules = rules.OrderBy(r => r.Name).ToList();
            _ruleCacheService.UpdateCache(null, sortedRules);
        }

        public async Task InitialLoadAsync()
        {
            var appRules = await Task.Run(() => _firewallService.GetApplicationRules()).ConfigureAwait(false);
            var allWildcards = _wildcardRuleService.GetRules();
            var uwpService = new UwpService();
            var uwpApps = uwpService.LoadUwpAppsFromCache();

            var advancedRules = ProcessAndGetAdvancedRules(appRules, uwpApps, allWildcards);
            _ruleCacheService.UpdateCache(null, advancedRules);
        }

        public bool DoesManagedRuleExist(string appPath, string serviceName, string direction)
        {
            if (!Enum.TryParse<Directions>(direction, true, out var dirEnum))
            {
                return false;
            }

            var allManagedRules = _ruleCacheService.GetAdvancedRules();
            if (allManagedRules.Any(r => r.ApplicationName.Equals(appPath, StringComparison.OrdinalIgnoreCase) && r.Direction.HasFlag(dirEnum)))
            {
                return true;
            }

            if (!string.IsNullOrEmpty(serviceName))
            {
                var serviceNames = serviceName.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries);
                foreach (var sName in serviceNames)
                {
                    if (allManagedRules.Any(r => r.ServiceName.Equals(sName, StringComparison.OrdinalIgnoreCase) && r.Direction.HasFlag(dirEnum)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public List<AdvancedRuleViewModel> LoadAdvancedRules()
        {
            return _ruleCacheService.GetAdvancedRules();
        }

        public List<AggregatedRuleViewModel> GetAggregatedAdvancedRules()
        {
            var advancedRules = LoadAdvancedRules();
            var aggregatedRules = new List<AggregatedRuleViewModel>();

            var groups = advancedRules.GroupBy(r =>
            {
                var name = r.Name ?? string.Empty;
                var baseName = name.Replace(" - TCP", "").Replace(" - UDP", "");
                return new { BaseName = baseName, r.ApplicationName, r.ServiceName, r.Direction, r.Status, r.Type };
            });

            foreach (var group in groups)
            {
                var firstRule = group.First();
                var protocols = group.Select(r => r.ProtocolName).Distinct().OrderBy(p => p).ToList();
                var protocolString = string.Join(", ", protocols);
                if (protocols.Contains("TCP") && protocols.Contains("UDP"))
                {
                    protocolString = "TCP, UDP";
                }

                var aggregatedRule = new AggregatedRuleViewModel
                {
                    Name = group.Key.BaseName,
                    IsEnabled = group.All(r => r.IsEnabled),
                    Status = firstRule.Status,
                    Direction = firstRule.Direction,
                    ProtocolName = protocolString,
                    LocalPorts = firstRule.LocalPorts,
                    RemotePorts = firstRule.RemotePorts,
                    LocalAddresses = firstRule.LocalAddresses,
                    RemoteAddresses = firstRule.RemoteAddresses,
                    ApplicationName = firstRule.ApplicationName,
                    ServiceName = firstRule.ServiceName,
                    Profiles = firstRule.Profiles,
                    Grouping = firstRule.Grouping,
                    Description = firstRule.Description,
                    Type = firstRule.Type,
                    WildcardDefinition = firstRule.WildcardDefinition,
                    UnderlyingRules = group.ToList()
                };
                aggregatedRules.Add(aggregatedRule);
            }

            return aggregatedRules.OrderBy(r => r.Name).ToList();
        }

        public void ClearLocalCaches()
        {
            _knownRulePaths.Clear();
        }

        public async Task RefreshAndCacheAsync(bool forceFullScan = false)
        {
            var appRules = await Task.Run(() => _firewallService.GetApplicationRules()).ConfigureAwait(false);
            var allWildcards = _wildcardRuleService.GetRules();

            var uwpService = new UwpService();
            List<UwpApp> uwpApps;
            if (forceFullScan)
            {
                uwpApps = await uwpService.ScanForUwpApps().ConfigureAwait(false);
            }
            else
            {
                uwpApps = uwpService.LoadUwpAppsFromCache();
                if (uwpApps.Count == 0)
                {
                    uwpApps = await uwpService.ScanForUwpApps().ConfigureAwait(false);
                }
            }

            var advancedRules = ProcessAndGetAdvancedRules(appRules, uwpApps, allWildcards);
            _ruleCacheService.UpdateCache(null, advancedRules);
        }

        private List<AdvancedRuleViewModel> ProcessAndGetAdvancedRules(List<INetFwRule2> appRules, List<UwpApp> uwpApps, List<WildcardRule> allWildcards)
        {
            var advancedRules = new List<AdvancedRuleViewModel>();
            foreach (var rule in appRules)
            {
                var vm = CreateAdvancedRuleViewModel(rule);
                vm.Type = DetermineRuleType(rule, uwpApps);
                advancedRules.Add(vm);
            }

            foreach (var wildcard in allWildcards)
            {
                advancedRules.Add(new AdvancedRuleViewModel
                {
                    Name = $"*{wildcard.ExeName} in {Path.GetFileName(wildcard.FolderPath)}",
                    Description = $"[WILDCARD DEFINITION] Path: {wildcard.FolderPath}",
                    Grouping = MFWConstants.WildcardRuleGroup,
                    Status = wildcard.Action.Contains("Allow") ? "Allow" : "Block",
                    Direction = Directions.Outgoing,
                    Protocol = ProtocolTypes.Any.Value,
                    ProtocolName = ProtocolTypes.Any.Name,
                    ApplicationName = wildcard.FolderPath,
                    LocalPorts = [],
                    RemotePorts = [],
                    LocalAddresses = [],
                    RemoteAddresses = [],
                    Profiles = "All",
                    ServiceName = "N/A",
                    Type = RuleType.Wildcard,
                    WildcardDefinition = wildcard
                });
            }

            return [.. advancedRules.OrderBy(r => r.Name)];
        }

        private static RuleType DetermineRuleType(INetFwRule2 rule, List<UwpApp> uwpApps)
        {
            if (!string.IsNullOrEmpty(rule.serviceName) && rule.serviceName != "*") return RuleType.Service;
            if (!string.IsNullOrEmpty(rule.ApplicationName))
            {
                if (uwpApps.Any(u => rule.Description?.Contains(u.PackageFamilyName) == true))
                {
                    return RuleType.UWP;
                }
                return RuleType.Program;
            }
            return RuleType.Advanced;
        }

        public static AdvancedRuleViewModel CreateAdvancedRuleViewModel(INetFwRule2 rule)
        {
            return new AdvancedRuleViewModel
            {
                Name = rule.Name ?? "Unnamed Rule",
                Description = rule.Description ?? "N/A",
                IsEnabled = rule.Enabled,
                Status = rule.Action == NET_FW_ACTION_.NET_FW_ACTION_ALLOW ? "Allow" : "Block",
                Direction = (Directions)rule.Direction,
                ApplicationName = PathResolver.NormalizePath(rule.ApplicationName ?? string.Empty),
                LocalPorts = ParsingUtility.ParseStringToList<PortRange>(rule.LocalPorts, PortRange.TryParse),
                RemotePorts = ParsingUtility.ParseStringToList<PortRange>(rule.RemotePorts, PortRange.TryParse),
                Protocol = (short)rule.Protocol,
                ProtocolName = GetProtocolName(rule.Protocol),
                ServiceName = string.IsNullOrEmpty(rule.serviceName) || rule.serviceName == "*" ? "Any" : rule.serviceName,
                LocalAddresses = ParsingUtility.ParseStringToList<IPAddressRange>(rule.LocalAddresses, IPAddressRange.TryParse),
                RemoteAddresses = ParsingUtility.ParseStringToList<IPAddressRange>(rule.RemoteAddresses, IPAddressRange.TryParse),
                Profiles = GetProfileString(rule.Profiles),
                Grouping = rule.Grouping ?? string.Empty
            };
        }

        private static string GetProtocolName(int protocolValue)
        {
            return protocolValue switch
            {
                6 => "TCP",
                17 => "UDP",
                1 => "ICMPv4",
                58 => "ICMPv6",
                2 => "IGMP",
                256 => "Any",
                _ => protocolValue.ToString(),
            };
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