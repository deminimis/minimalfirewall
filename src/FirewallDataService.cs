// File: FirewallDataService.cs
using NetFwTypeLib;
using System.IO;
using System.Linq;
using MinimalFirewall.TypedObjects;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
namespace MinimalFirewall
{
    public class FirewallDataService
    {
        private readonly FirewallRuleService _firewallService;
        private readonly WildcardRuleService _wildcardRuleService;
        private readonly UwpService _uwpService;
        private readonly MemoryCache _localCache;
        private const string ServicesCacheKey = "ServicesList";
        private const string MfwRulesCacheKey = "MfwRulesList";
        private const string AggregatedRulesCacheKey = "AggregatedRulesList";
        public FirewallDataService(FirewallRuleService firewallService, WildcardRuleService wildcardRuleService, UwpService uwpService)
        {
            _firewallService = firewallService;
            _wildcardRuleService = wildcardRuleService;
            _uwpService = uwpService;
            _localCache = new MemoryCache(new MemoryCacheOptions());
        }

        public void ClearAggregatedRulesCache()
        {
            _localCache.Remove(AggregatedRulesCacheKey);
        }

        public void InvalidateMfwRuleCache()
        {
            _localCache.Remove(MfwRulesCacheKey);
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

        public async Task<List<AggregatedRuleViewModel>> GetAggregatedRulesAsync(CancellationToken token, IProgress<int>? progress = null)
        {
            if (_localCache.TryGetValue(AggregatedRulesCacheKey, out List<AggregatedRuleViewModel>? cachedRules) && cachedRules != null)
            {
                progress?.Report(100);
                return cachedRules;
            }

            var aggregatedRules = await Task.Run(() =>
            {
                var allRules = _firewallService.GetAllRules();
                try
                {
                    var enabledRules = allRules.Where(r => r.Enabled && !string.IsNullOrEmpty(r.Grouping)).ToList();
                    int totalRules = enabledRules.Count;
                    if (totalRules == 0)
                    {
                        progress?.Report(100);
                        return new List<AggregatedRuleViewModel>();
                    }

                    var groupedByGroupingAndProtocol = enabledRules
                        .GroupBy(r => $"{r.Grouping}|{r.ApplicationName}|{r.serviceName}|{r.Protocol}")
                        .ToList();

                    var aggRules = new List<AggregatedRuleViewModel>();
                    int processedCount = 0;

                    foreach (var group in groupedByGroupingAndProtocol)
                    {
                        if (token.IsCancellationRequested) return new List<AggregatedRuleViewModel>();
                        var groupList = group.ToList();
                        aggRules.Add(CreateAggregatedViewModelForRuleGroup(groupList));
                        processedCount += groupList.Count;
                        progress?.Report((processedCount * 100) / totalRules);
                    }

                    progress?.Report(100);
                    return aggRules.OrderBy(r => r.Name).ToList();
                }
                finally
                {
                    foreach (var rule in allRules)
                    {
                        if (rule != null) Marshal.ReleaseComObject(rule);
                    }
                }
            }, token);

            if (token.IsCancellationRequested) return new List<AggregatedRuleViewModel>();

            var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5));
            _localCache.Set(AggregatedRulesCacheKey, aggregatedRules, cacheEntryOptions);

            return aggregatedRules;
        }

        private AggregatedRuleViewModel CreateAggregatedViewModelForRuleGroup(List<INetFwRule2> group)
        {
            var firstRule = group.First();

            var commonName = GetCommonName(group);
            if (string.IsNullOrEmpty(commonName) || commonName.StartsWith("@"))
            {
                commonName = firstRule.Grouping ?? string.Empty;
            }

            var aggRule = new AggregatedRuleViewModel
            {
                Name = commonName,
                ApplicationName = firstRule.ApplicationName ?? string.Empty,
                ServiceName = firstRule.serviceName ?? string.Empty,
                Protocol = firstRule.Protocol,
                ProtocolName = GetProtocolName(firstRule.Protocol),
                Type = DetermineRuleType(firstRule),
                UnderlyingRules = group.Select(CreateAdvancedRuleViewModel).ToList(),
                IsEnabled = group.All(r => r.Enabled),
                Profiles = GetProfileString(firstRule.Profiles),
                Grouping = firstRule.Grouping ?? "",
                Description = firstRule.Description ?? ""
            };

            bool hasInAllow = group.Any(r => r.Action == NET_FW_ACTION_.NET_FW_ACTION_ALLOW && r.Direction == NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN);
            bool hasOutAllow = group.Any(r => r.Action == NET_FW_ACTION_.NET_FW_ACTION_ALLOW && r.Direction == NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT);
            bool hasInBlock = group.Any(r => r.Action == NET_FW_ACTION_.NET_FW_ACTION_BLOCK && r.Direction == NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN);
            bool hasOutBlock = group.Any(r => r.Action == NET_FW_ACTION_.NET_FW_ACTION_BLOCK && r.Direction == NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT);

            aggRule.InboundStatus = hasInAllow ? "Allow" : (hasInBlock ? "Block" : "N/A");
            if (hasInAllow && hasInBlock) aggRule.InboundStatus = "Allow, Block";

            aggRule.OutboundStatus = hasOutAllow ? "Allow" : (hasOutBlock ? "Block" : "N/A");
            if (hasOutAllow && hasOutBlock) aggRule.OutboundStatus = "Allow, Block";

            var localPorts = group.Select(r => r.LocalPorts).Where(p => !string.IsNullOrEmpty(p) && p != "*").Distinct().ToList();
            aggRule.LocalPorts = localPorts.Any() ? string.Join(", ", localPorts) : "*";

            var remotePorts = group.Select(r => r.RemotePorts).Where(p => !string.IsNullOrEmpty(p) && p != "*").Distinct().ToList();
            aggRule.RemotePorts = remotePorts.Any() ? string.Join(", ", remotePorts) : "*";

            var localAddresses = group.Select(r => r.LocalAddresses).Where(p => !string.IsNullOrEmpty(p) && p != "*").Distinct().ToList();
            aggRule.LocalAddresses = localAddresses.Any() ? string.Join(", ", localAddresses) : "*";

            var remoteAddresses = group.Select(r => r.RemoteAddresses).Where(p => !string.IsNullOrEmpty(p) && p != "*").Distinct().ToList();
            aggRule.RemoteAddresses = remoteAddresses.Any() ? string.Join(", ", remoteAddresses) : "*";

            return aggRule;
        }

        private string GetCommonName(List<INetFwRule2> group)
        {
            if (group.Count == 0) return string.Empty;
            if (group.Count == 1) return group[0].Name ?? string.Empty;

            var names = group.Select(r => r.Name ?? string.Empty).ToList();
            string first = names[0];
            int commonPrefixLength = first.Length;

            foreach (string name in names.Skip(1))
            {
                commonPrefixLength = Math.Min(commonPrefixLength, name.Length);
                for (int i = 0; i < commonPrefixLength; i++)
                {
                    if (first[i] != name[i])
                    {
                        commonPrefixLength = i;
                        break;
                    }
                }
            }

            string commonPrefix = first.Substring(0, commonPrefixLength).Trim();
            if (commonPrefix.EndsWith("-") || commonPrefix.EndsWith("("))
            {
                commonPrefix = commonPrefix.Substring(0, commonPrefix.Length - 1).Trim();
            }

            return string.IsNullOrEmpty(commonPrefix) ? (group[0].Grouping ?? string.Empty) : commonPrefix;
        }


        private RuleType DetermineRuleType(INetFwRule2 rule)
        {
            if (!string.IsNullOrEmpty(rule.serviceName) && rule.serviceName != "*")
                return RuleType.Service;
            if (!string.IsNullOrEmpty(rule.ApplicationName) && rule.ApplicationName != "*")
            {
                bool hasSpecifics = (!string.IsNullOrEmpty(rule.LocalPorts) && rule.LocalPorts != "*") ||
                                    (!string.IsNullOrEmpty(rule.RemotePorts) && rule.RemotePorts != "*") ||
                                    (!string.IsNullOrEmpty(rule.LocalAddresses) && rule.LocalAddresses != "*") ||
                                    (!string.IsNullOrEmpty(rule.RemoteAddresses) && rule.RemoteAddresses != "*");
                return hasSpecifics ? RuleType.Advanced : RuleType.Program;
            }
            return RuleType.Advanced;
        }


        private List<AdvancedRuleViewModel> GetMfwRulesFromCache()
        {
            if (_localCache.TryGetValue(MfwRulesCacheKey, out List<AdvancedRuleViewModel>? cachedRules) && cachedRules != null)
            {
                return cachedRules;
            }

            var allRules = _firewallService.GetAllRules();
            try
            {
                var newCachedRules = allRules
                    .Where(rule =>
                        !string.IsNullOrEmpty(rule.Grouping) &&
                        (rule.Grouping == MFWConstants.MainRuleGroup || rule.Grouping == MFWConstants.WildcardRuleGroup || rule.Grouping.EndsWith(MFWConstants.MfwRuleSuffix))
                    )
                    .Select(CreateAdvancedRuleViewModel)
                    .ToList();
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10));
                _localCache.Set(MfwRulesCacheKey, newCachedRules, cacheEntryOptions);
                return newCachedRules;
            }
            finally
            {
                foreach (var rule in allRules)
                {
                    Marshal.ReleaseComObject(rule);
                }
            }
        }

        public bool DoesAnyRuleExist(string appPath, string serviceName, string direction)
        {
            if (!Enum.TryParse<Directions>(direction, true, out var dirEnum))
            {
                return false;
            }

            var mfwRules = GetMfwRulesFromCache();
            bool ruleExists = false;
            foreach (var rule in mfwRules)
            {
                if (rule.Direction.HasFlag(dirEnum))
                {
                    if (!string.IsNullOrEmpty(rule.ApplicationName) && rule.ApplicationName.Equals(appPath, StringComparison.OrdinalIgnoreCase))
                    {
                        ruleExists = true;
                        break;
                    }
                    if (!string.IsNullOrEmpty(serviceName) && !string.IsNullOrEmpty(rule.ServiceName))
                    {
                        var serviceNames = serviceName.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries);
                        foreach (var sName in serviceNames)
                        {
                            if (rule.ServiceName.Equals(sName, StringComparison.OrdinalIgnoreCase))
                            {
                                ruleExists = true;
                                break;
                            }
                        }
                        if (ruleExists) break;
                    }
                }
            }
            return ruleExists;
        }

        public void ClearCaches()
        {
            _localCache.Remove(ServicesCacheKey);
            _localCache.Remove(MfwRulesCacheKey);
            _localCache.Remove(AggregatedRulesCacheKey);
        }

        public static AdvancedRuleViewModel CreateAdvancedRuleViewModel(INetFwRule2 rule)
        {
            var appName = rule.ApplicationName ?? string.Empty;
            return new AdvancedRuleViewModel
            {
                Name = rule.Name ?? "Unnamed Rule",
                Description = rule.Description ?? "N/A",
                IsEnabled = rule.Enabled,
                Status = rule.Action == NET_FW_ACTION_.NET_FW_ACTION_ALLOW ? "Allow" : "Block",
                Direction = (Directions)rule.Direction,
                ApplicationName = appName == "*" ? "*" : PathResolver.NormalizePath(appName),
                LocalPorts = string.IsNullOrEmpty(rule.LocalPorts) ? "*" : rule.LocalPorts,
                RemotePorts = string.IsNullOrEmpty(rule.RemotePorts) ? "*" : rule.RemotePorts,
                Protocol = (int)rule.Protocol,
                ProtocolName = GetProtocolName(rule.Protocol),
                ServiceName = (string.IsNullOrEmpty(rule.serviceName) || rule.serviceName == "*") ? string.Empty : rule.serviceName,
                LocalAddresses = string.IsNullOrEmpty(rule.LocalAddresses) ? "*" : rule.LocalAddresses,
                RemoteAddresses = string.IsNullOrEmpty(rule.RemoteAddresses) ? "*" : rule.RemoteAddresses,
                Profiles = GetProfileString(rule.Profiles),
                Grouping = rule.Grouping ?? string.Empty,
                InterfaceTypes = rule.InterfaceTypes ?? "All",
                IcmpTypesAndCodes = rule.IcmpTypesAndCodes ?? ""
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