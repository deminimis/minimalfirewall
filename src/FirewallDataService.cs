// FirewallDataService.cs
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

        private List<AdvancedRuleViewModel> _appRulesCache = [];
        private List<AdvancedRuleViewModel> _serviceRulesCache = [];

        private delegate bool TryParseHandler<T>(string value, [NotNullWhen(true)] out T? result);

        public FirewallDataService(FirewallRuleService firewallService, WildcardRuleService wildcardRuleService, RuleCacheService ruleCacheService)
        {
            _firewallService = firewallService;
            _wildcardRuleService = wildcardRuleService;
            _ruleCacheService = ruleCacheService;
        }

        public bool DoesManagedRuleExist(string appPath, string serviceName, string direction)
        {
            if (!Enum.TryParse<Directions>(direction, true, out var dirEnum))
            {
                return false;
            }

            if (_appRulesCache.Any(r => r.ApplicationName.Equals(appPath, StringComparison.OrdinalIgnoreCase) &&
                                       r.Direction.HasFlag(dirEnum)))
            {
                return true;
            }

            if (!string.IsNullOrEmpty(serviceName))
            {
                var serviceNames = serviceName.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries);
                foreach (var sName in serviceNames)
                {
                    if (_serviceRulesCache.Any(r => r.ServiceName.Equals(sName, StringComparison.OrdinalIgnoreCase) &&
                                                  r.Direction.HasFlag(dirEnum)))
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
                    UnderlyingRules = group.ToList()
                };

                aggregatedRules.Add(aggregatedRule);
            }

            return aggregatedRules.OrderBy(r => r.Name).ToList();
        }

        public void ClearLocalCaches()
        {
            _knownRulePaths.Clear();
            _appRulesCache.Clear();
            _serviceRulesCache.Clear();
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
            _appRulesCache = advancedRules
                .Where(r => r.IsEnabled && !string.IsNullOrEmpty(r.ApplicationName) && r.ServiceName.Equals("Any", StringComparison.OrdinalIgnoreCase))
                .ToList();
            _serviceRulesCache = advancedRules
                .Where(r => r.IsEnabled && !string.IsNullOrEmpty(r.ServiceName) && !r.ServiceName.Equals("Any", StringComparison.OrdinalIgnoreCase))
                .ToList();
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
                    Description = $"[WILDCARD DEFINITION]",
                    Grouping = MFWConstants.WildcardRuleGroup,
                    Status = wildcard.Action,
                    Direction = Directions.Outgoing,
                    Protocol = ProtocolTypes.Any.Value,
                    ProtocolName = ProtocolTypes.Any.Name,
                    LocalPorts = [],
                    RemotePorts = [],
                    LocalAddresses = [],
                    RemoteAddresses = [],
                    Profiles = "All",
                    ServiceName = "N/A",
                    Type = RuleType.Wildcard
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

        private static ICollection<T> ParseStringToList<T>(string? input, TryParseHandler<T> tryParse)
        {
            if (string.IsNullOrEmpty(input) || input == "*")
            {
                return new List<T>();
            }
            var results = new List<T>();
            var parts = input.Split(',');
            foreach (var part in parts)
            {
                if (tryParse(part.Trim(), out T? result))
                {
                    if (result != null) results.Add(result);
                }
            }
            return results;
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
                ApplicationName = rule.ApplicationName ?? string.Empty,
                LocalPorts = ParseStringToList<PortRange>(rule.LocalPorts, PortRange.TryParse),
                RemotePorts = ParseStringToList<PortRange>(rule.RemotePorts, PortRange.TryParse),
                Protocol = (short)rule.Protocol,
                ProtocolName = GetProtocolName(rule.Protocol),
                ServiceName = string.IsNullOrEmpty(rule.serviceName) || rule.serviceName == "*" ? "Any" : rule.serviceName,
                LocalAddresses = ParseStringToList<IPAddressRange>(rule.LocalAddresses, IPAddressRange.TryParse),
                RemoteAddresses = ParseStringToList<IPAddressRange>(rule.RemoteAddresses, IPAddressRange.TryParse),
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