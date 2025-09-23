// File: FirewallDataService.cs
using NetFwTypeLib;
using System.IO;
using System.Linq;
using MinimalFirewall.TypedObjects;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using System.Runtime.InteropServices;

namespace MinimalFirewall
{
    public class FirewallDataService
    {
        private readonly FirewallRuleService _firewallService;
        private readonly WildcardRuleService _wildcardRuleService;
        private readonly MemoryCache _localCache;
        private const string ServicesCacheKey = "ServicesList";

        public FirewallDataService(FirewallRuleService firewallService, WildcardRuleService wildcardRuleService)
        {
            _firewallService = firewallService;
            _wildcardRuleService = wildcardRuleService;
            _localCache = new MemoryCache(new MemoryCacheOptions());
        }

        public List<ServiceViewModel> GetCachedServicesWithExePaths()
        {
            if (_localCache.TryGetValue(ServicesCacheKey, out List<ServiceViewModel>? services) && services != null)
            {
                return services;
            }

            services = SystemDiscoveryService.GetServicesWithExePaths();
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(10));
            _localCache.Set(ServicesCacheKey, services, cacheOptions);
            return services;
        }

        public async Task<List<AggregatedRuleViewModel>> GetAggregatedRulesAsync()
        {
            return await Task.Run(() =>
            {
                var allRules = _firewallService.GetAllRules();
                var allServices = GetCachedServicesWithExePaths();
                var allWildcards = _wildcardRuleService.GetRules();

                var rulesByAppPath = allRules
                    .Where(r => !string.IsNullOrEmpty(r.ApplicationName))
                    .GroupBy(r => PathResolver.NormalizePath(r.ApplicationName!), StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var rulesByServiceName = allRules
                    .Where(r => !string.IsNullOrEmpty(r.serviceName) && r.serviceName != "*")
                    .GroupBy(r => r.serviceName, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var aggregatedRules = new Dictionary<string, AggregatedRuleViewModel>();

                foreach (var service in allServices)
                {
                    var relevantRules = new List<INetFwRule2>();
                    if (rulesByServiceName.TryGetValue(service.ServiceName, out var serviceRules))
                    {
                        relevantRules.AddRange(serviceRules);
                    }

                    var normalizedServiceExePath = PathResolver.NormalizePath(service.ExePath);
                    if (rulesByAppPath.TryGetValue(normalizedServiceExePath, out var appRules))
                    {
                        relevantRules.AddRange(appRules);
                        rulesByAppPath.Remove(normalizedServiceExePath);
                    }

                    if (relevantRules.Count > 0)
                    {
                        var distinctRules = relevantRules.Distinct().ToList();
                        var aggRule = CreateAggregatedViewModelForRuleGroup(distinctRules, service.DisplayName, normalizedServiceExePath, service.ServiceName, RuleType.Service);
                        string key = $"{aggRule.Name}|{aggRule.ApplicationName}|{aggRule.ServiceName}";
                        aggregatedRules[key] = aggRule;
                    }
                }

                foreach (var appGroup in rulesByAppPath)
                {
                    var aggRule = CreateAggregatedViewModelForRuleGroup(appGroup.Value, Path.GetFileNameWithoutExtension(appGroup.Key), appGroup.Key, "", RuleType.Program);
                    string key = $"{aggRule.Name}|{aggRule.ApplicationName}|{aggRule.ServiceName}";
                    aggregatedRules[key] = aggRule;
                }

                var nonAppRules = allRules.Where(r => string.IsNullOrEmpty(r.ApplicationName) && (string.IsNullOrEmpty(r.serviceName) || r.serviceName == "*"));
                foreach (var rule in nonAppRules)
                {
                    if (rule.Description?.StartsWith(MFWConstants.UwpDescriptionPrefix, StringComparison.Ordinal) == true) continue;

                    var vm = CreateAdvancedRuleViewModel(rule);
                    vm.Type = RuleType.Advanced;

                    var aggVm = new AggregatedRuleViewModel
                    {
                        Name = vm.Name,
                        IsEnabled = vm.IsEnabled,
                        Status = vm.Status,
                        Direction = vm.Direction,
                        LocalPorts = vm.LocalPorts,
                        RemotePorts = vm.RemotePorts,
                        LocalAddresses = vm.LocalAddresses,
                        RemoteAddresses = vm.RemoteAddresses,
                        ApplicationName = vm.ApplicationName,
                        ServiceName = vm.ServiceName,
                        Profiles = vm.Profiles,
                        Grouping = vm.Grouping,
                        Description = vm.Description,
                        Type = vm.Type,
                        ProtocolName = vm.ProtocolName,
                        UnderlyingRules = { vm }
                    };
                    aggregatedRules[vm.Name] = aggVm;
                }

                AddWildcardViewModels(aggregatedRules, allWildcards);

                foreach (var rule in allRules)
                {
                    Marshal.ReleaseComObject(rule);
                }

                return aggregatedRules.Values.OrderBy(r => r.Name).ToList();
            });
        }

        private AggregatedRuleViewModel CreateAggregatedViewModelForRuleGroup(List<INetFwRule2> group, string displayName, string appPath, string serviceName, RuleType type)
        {
            var firstRule = group.First();
            var aggRule = new AggregatedRuleViewModel
            {
                Name = displayName,
                ApplicationName = appPath,
                ServiceName = serviceName,
                Type = type,
                UnderlyingRules = group.Select(CreateAdvancedRuleViewModel).ToList(),
                IsEnabled = group.All(r => r.Enabled),
                Profiles = GetProfileString(firstRule.Profiles),
                Grouping = firstRule.Grouping ?? "",
                Description = firstRule.Description ?? ""
            };

            bool hasInAllow = group.Any(r => r.Enabled && r.Action == NET_FW_ACTION_.NET_FW_ACTION_ALLOW && r.Direction == NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN);
            bool hasOutAllow = group.Any(r => r.Enabled && r.Action == NET_FW_ACTION_.NET_FW_ACTION_ALLOW && r.Direction == NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT);
            bool hasInBlock = group.Any(r => r.Enabled && r.Action == NET_FW_ACTION_.NET_FW_ACTION_BLOCK && r.Direction == NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN);
            bool hasOutBlock = group.Any(r => r.Enabled && r.Action == NET_FW_ACTION_.NET_FW_ACTION_BLOCK && r.Direction == NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT);

            var statusParts = new List<string>();
            var directionParts = new List<Directions>();

            if (hasOutAllow) { statusParts.Add("Allow Out"); directionParts.Add(Directions.Outgoing); }
            if (hasOutBlock) { statusParts.Add("Block Out"); directionParts.Add(Directions.Outgoing); }
            if (hasInAllow) { statusParts.Add("Allow In"); directionParts.Add(Directions.Incoming); }
            if (hasInBlock) { statusParts.Add("Block In"); directionParts.Add(Directions.Incoming); }

            if (statusParts.Count == 0)
            {
                aggRule.Status = "Undefined";
            }
            else
            {
                aggRule.Status = string.Join(", ", statusParts);
            }

            aggRule.Direction = directionParts.Distinct().Aggregate((Directions)0, (current, d) => current | d);

            var protocols = group.Select(r => GetProtocolName(r.Protocol)).Distinct().OrderBy(p => p).ToList();
            var protocolString = string.Join(", ", protocols);
            if (protocols.Contains("TCP") && protocols.Contains("UDP"))
            {
                protocolString = "TCP, UDP";
            }
            aggRule.ProtocolName = protocolString;

            return aggRule;
        }

        public bool DoesAnyRuleExist(string appPath, string serviceName, string direction)
        {
            if (!Enum.TryParse<Directions>(direction, true, out var dirEnum))
            {
                return false;
            }

            var allCurrentRules = _firewallService.GetAllRules();
            bool ruleExists = false;

            foreach (var rule in allCurrentRules)
            {
                if (rule == null || !rule.Enabled) continue;

                if (((Directions)rule.Direction).HasFlag(dirEnum))
                {
                    if (!string.IsNullOrEmpty(rule.ApplicationName) && rule.ApplicationName.Equals(appPath, StringComparison.OrdinalIgnoreCase))
                    {
                        ruleExists = true;
                        break;
                    }

                    if (!string.IsNullOrEmpty(serviceName) && !string.IsNullOrEmpty(rule.serviceName))
                    {
                        var serviceNames = serviceName.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries);
                        foreach (var sName in serviceNames)
                        {
                            if (rule.serviceName.Equals(sName, StringComparison.OrdinalIgnoreCase))
                            {
                                ruleExists = true;
                                break;
                            }
                        }
                        if (ruleExists) break;
                    }
                }
            }

            foreach (var rule in allCurrentRules)
            {
                Marshal.ReleaseComObject(rule);
            }

            return ruleExists;
        }

        private void AddWildcardViewModels(Dictionary<string, AggregatedRuleViewModel> rules, List<WildcardRule> wildcards)
        {
            foreach (var wildcard in wildcards)
            {
                var aggRule = new AggregatedRuleViewModel
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
                };
                string key = $"{aggRule.Name}|{aggRule.ApplicationName}|{aggRule.ServiceName}";
                rules[key] = aggRule;
            }
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
            var profileNames = new List<string>(3);
            if ((profiles & (int)NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_DOMAIN) != 0) profileNames.Add("Domain");
            if ((profiles & (int)NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PRIVATE) != 0) profileNames.Add("Private");
            if ((profiles & (int)NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PUBLIC) != 0) profileNames.Add("Public");
            return string.Join(", ", profileNames);
        }
    }
}