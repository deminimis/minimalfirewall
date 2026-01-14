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
using System;

namespace MinimalFirewall
{
    public enum MfwRuleStatus { None, MfwAllow, MfwBlock }

    public class FirewallDataService(FirewallRuleService firewallRuleService, WildcardRuleService wildcardRuleService, UwpService uwpService)
    {
        private readonly FirewallRuleService _firewallRuleService = firewallRuleService;
        private readonly WildcardRuleService _wildcardRuleService = wildcardRuleService;
        private readonly UwpService _uwpService = uwpService;
        private readonly MemoryCache _localCache = new(new MemoryCacheOptions());
        private const string ServicesCacheKey = "ServicesList";
        private const string MfwRulesCacheKey = "MfwRulesList";
        private const string AggregatedRulesCacheKey = "AggregatedRulesList";
        private static readonly char[] _separators = [',', ' '];

        public void ClearAggregatedRulesCache()
        {
            _localCache.Remove(AggregatedRulesCacheKey);
        }

        public void InvalidateRuleCache()
        {
            _localCache.Remove(MfwRulesCacheKey);
            _localCache.Remove(AggregatedRulesCacheKey);
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

        public List<UwpApp> LoadUwpAppsFromCache()
        {
            return _uwpService.LoadUwpAppsFromCache();
        }

        private Task<List<AdvancedRuleViewModel>> FetchAllMfwRulesAsync(CancellationToken token)
        {
            return Task.Run(() =>
            {
                var allRules = _firewallRuleService.GetAllRules().ToList();
                try
                {
                    var mfwRules = allRules
                        .Where(rule =>
                            !string.IsNullOrEmpty(rule.Grouping) &&
                            (rule.Grouping == MFWConstants.MainRuleGroup || rule.Grouping == MFWConstants.WildcardRuleGroup || rule.Grouping.EndsWith(MFWConstants.MfwRuleSuffix))
                        )
                        .Select(CreateAdvancedRuleViewModel)
                        .ToList();

                    if (token.IsCancellationRequested)
                    {
                        return [];
                    }
                    return mfwRules;
                }
                finally
                {
                    foreach (var rule in allRules)
                    {
                        if (rule != null) Marshal.ReleaseComObject(rule);
                    }
                }
            }, token);
        }

        public async Task<List<AdvancedRuleViewModel>> GetMfwRulesAsync(CancellationToken token)
        {
            var result = await _localCache.GetOrCreateAsync(MfwRulesCacheKey, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(10);
                return await FetchAllMfwRulesAsync(token);
            });

            return result ?? [];
        }

        public async Task<List<AggregatedRuleViewModel>> GetAggregatedRulesAsync(CancellationToken token, IProgress<int>? progress = null)
        {
            if (_localCache.TryGetValue(AggregatedRulesCacheKey, out List<AggregatedRuleViewModel>? cachedRules) && cachedRules != null)
            {
                progress?.Report(100);
                return cachedRules;
            }

            var allMfwRules = await GetMfwRulesAsync(token);
            if (token.IsCancellationRequested) return [];

            var aggregatedRules = await Task.Run(() =>
            {
                int totalRules = allMfwRules.Count;
                if (totalRules == 0)
                {
                    progress?.Report(100);
                    return new List<AggregatedRuleViewModel>();
                }

                var groupedByGroupingAndProtocol = allMfwRules
                    .Where(r => r.IsEnabled)
                    .GroupBy(r => (r.Grouping, r.ApplicationName, r.ServiceName, r.Protocol))
                    .ToList();

                var aggRules = new List<AggregatedRuleViewModel>();
                int processedCount = 0;

                foreach (var group in groupedByGroupingAndProtocol)
                {
                    if (token.IsCancellationRequested) return [];
                    var groupList = group.ToList();
                    aggRules.Add(CreateAggregatedViewModelForRuleGroup(groupList));
                    processedCount += groupList.Count;
                    progress?.Report((processedCount * 100) / totalRules);
                }

                progress?.Report(100);
                return aggRules.OrderBy(r => r.Name).ToList();
            }, token);

            if (token.IsCancellationRequested) return [];

            var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5));
            _localCache.Set(AggregatedRulesCacheKey, aggregatedRules, cacheEntryOptions);

            return aggregatedRules;
        }

        private static AggregatedRuleViewModel CreateAggregatedViewModelForRuleGroup(List<AdvancedRuleViewModel> group)
        {
            var firstRule = group[0];
            var commonName = GetCommonName(group);
            if (string.IsNullOrEmpty(commonName) || commonName.StartsWith('@'))
            {
                commonName = firstRule.Grouping ?? string.Empty;
            }

            var aggRule = new AggregatedRuleViewModel
            {
                Name = commonName,
                ApplicationName = firstRule.ApplicationName ?? string.Empty,
                ServiceName = firstRule.ServiceName ?? string.Empty,
                Protocol = firstRule.Protocol,
                ProtocolName = GetProtocolName(firstRule.Protocol),
                Type = DetermineRuleType(firstRule),
                UnderlyingRules = [.. group],
                IsEnabled = group.TrueForAll(r => r.IsEnabled),
                Profiles = firstRule.Profiles,
                Grouping = firstRule.Grouping ?? "",
                Description = firstRule.Description ?? ""
            };

            bool hasInAllow = group.Exists(r => r.Status == "Allow" && r.Direction.HasFlag(Directions.Incoming));
            bool hasOutAllow = group.Exists(r => r.Status == "Allow" && r.Direction.HasFlag(Directions.Outgoing));
            bool hasInBlock = group.Exists(r => r.Status == "Block" && r.Direction.HasFlag(Directions.Incoming));
            bool hasOutBlock = group.Exists(r => r.Status == "Block" && r.Direction.HasFlag(Directions.Outgoing));

            aggRule.InboundStatus = hasInAllow ? "Allow" : (hasInBlock ? "Block" : "-");
            if (hasInAllow && hasInBlock) aggRule.InboundStatus = "Allow, Block";

            aggRule.OutboundStatus = hasOutAllow ? "Allow" : (hasOutBlock ? "Block" : "-");
            if (hasOutAllow && hasOutBlock) aggRule.OutboundStatus = "Allow, Block";

            var localPorts = group.Select(r => r.LocalPorts).Where(p => !string.IsNullOrEmpty(p) && p != "*").Distinct().ToList();
            aggRule.LocalPorts = localPorts.Count > 0 ? string.Join(", ", localPorts) : "*";

            var remotePorts = group.Select(r => r.RemotePorts).Where(p => !string.IsNullOrEmpty(p) && p != "*").Distinct().ToList();
            aggRule.RemotePorts = remotePorts.Count > 0 ? string.Join(", ", remotePorts) : "*";

            var localAddresses = group.Select(r => r.LocalAddresses).Where(p => !string.IsNullOrEmpty(p) && p != "*").Distinct().ToList();
            aggRule.LocalAddresses = localAddresses.Count > 0 ? string.Join(", ", localAddresses) : "*";

            var remoteAddresses = group.Select(r => r.RemoteAddresses).Where(p => !string.IsNullOrEmpty(p) && p != "*").Distinct().ToList();
            aggRule.RemoteAddresses = remoteAddresses.Count > 0 ? string.Join(", ", remoteAddresses) : "*";

            return aggRule;
        }

        private static string GetCommonName(List<AdvancedRuleViewModel> group)
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

            string commonPrefix = first[..commonPrefixLength].Trim();
            if (commonPrefix.EndsWith('-') || commonPrefix.EndsWith('('))
            {
                commonPrefix = commonPrefix[..^1].Trim();
            }

            return string.IsNullOrEmpty(commonPrefix) ? (group[0].Grouping ?? string.Empty) : commonPrefix;
        }


        private static RuleType DetermineRuleType(AdvancedRuleViewModel rule)
        {
            if ((rule.Description != null && rule.Description.StartsWith(MFWConstants.UwpDescriptionPrefix, StringComparison.Ordinal)) ||
                 (rule.ApplicationName != null && rule.ApplicationName.StartsWith('@')) ||
                (rule.Name != null && rule.Name.StartsWith('@')))
            {
                return RuleType.UWP;
            }

            if (!string.IsNullOrEmpty(rule.ServiceName) && rule.ServiceName != "*")
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

        public async Task<MfwRuleStatus> CheckMfwRuleStatusAsync(string appPath, string serviceName, string direction)
        {
            if (!Enum.TryParse<Directions>(direction, true, out var dirEnum))
            {
                return MfwRuleStatus.None;
            }

            string normalizedAppPath = string.IsNullOrEmpty(appPath) ? string.Empty : PathResolver.NormalizePath(appPath);
            var serviceNamesSet = string.IsNullOrEmpty(serviceName)
                 ? null
                : new HashSet<string>(serviceName.Split(_separators, StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase);

            var mfwRules = await GetMfwRulesAsync(CancellationToken.None);

            if (mfwRules == null)
            {
                Debug.WriteLine("[ERROR] CheckMfwRuleStatus: GetMfwRulesAsync() returned null unexpectedly.");
                return MfwRuleStatus.None;
            }

            bool foundAllow = false;
            bool foundBlock = false;

            bool eventHasService = serviceNamesSet != null && serviceNamesSet.Count > 0;
            bool eventHasApp = !string.IsNullOrEmpty(normalizedAppPath);

            foreach (var rule in mfwRules)
            {
                if (rule == null) continue;
                if (!rule.Direction.HasFlag(dirEnum)) continue;

                bool ruleHasService = !string.IsNullOrEmpty(rule.ServiceName) && rule.ServiceName != "*";
                bool ruleHasApp = !string.IsNullOrEmpty(rule.ApplicationName) && rule.ApplicationName != "*";

                bool match = false;

                if (eventHasService)
                {
                    if (ruleHasService && serviceNamesSet!.Contains(rule.ServiceName))
                    {
                        if (ruleHasApp)
                        {
                            if (eventHasApp && string.Equals(rule.ApplicationName, normalizedAppPath, StringComparison.OrdinalIgnoreCase))
                            {
                                match = true;
                            }
                        }
                        else
                        {
                            match = true;
                        }
                    }
                }
                else
                {
                    if (!ruleHasService && ruleHasApp && eventHasApp)
                    {
                        if (string.Equals(rule.ApplicationName, normalizedAppPath, StringComparison.OrdinalIgnoreCase))
                        {
                            match = true;
                        }
                    }
                }

                if (match)
                {
                    if (rule.Status == "Allow")
                    {
                        foundAllow = true;
                    }
                    else if (rule.Status == "Block")
                    {
                        foundBlock = true;
                    }

                    if (foundBlock) break;
                }
            }

            if (foundBlock) return MfwRuleStatus.MfwBlock;
            if (foundAllow) return MfwRuleStatus.MfwAllow;
            return MfwRuleStatus.None;
        }

        public void ClearCaches()
        {
            _localCache.Remove(ServicesCacheKey);
            InvalidateRuleCache();
        }


        public static AdvancedRuleViewModel CreateAdvancedRuleViewModel(INetFwRule2 rule)
        {
            // Read COM properties once to avoid repeated interop calls
            var rawAppName = rule.ApplicationName;
            var rawProtocol = rule.Protocol;

            string appName = string.IsNullOrEmpty(rawAppName) ? string.Empty : rawAppName;

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
                Protocol = (int)rawProtocol,
                ProtocolName = GetProtocolName(rawProtocol),
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
            List<string> profileNames = [];
            if ((profiles & (int)NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_DOMAIN) != 0) profileNames.Add("Domain");
            if ((profiles & (int)NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PRIVATE) != 0) profileNames.Add("Private");
            if ((profiles & (int)NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PUBLIC) != 0) profileNames.Add("Public");
            return string.Join(", ", profileNames);
        }
    }
}