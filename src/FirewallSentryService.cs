// File: FirewallSentryService.cs
using System.IO;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MinimalFirewall
{
    public partial class FirewallSentryService : IDisposable
    {
        private readonly FirewallRuleService firewallService;
        private ManagementEventWatcher? _watcher;
        private Dictionary<string, string> _ruleBaseline = [];
        private bool _isStarted = false;
        private readonly string _baselinePath;
        public event Action? RuleSetChanged;

        public FirewallSentryService(FirewallRuleService firewallService)
        {
            this.firewallService = firewallService;
            _baselinePath = Path.Combine(AppContext.BaseDirectory, "sentry_baseline.json");
            LoadBaseline();
        }

        public void Start()
        {
            if (_isStarted)
            {
                return;
            }

            try
            {
                if (!_ruleBaseline.Any())
                {
                    CreateBaseline();
                }

                var scope = new ManagementScope(@"root\StandardCimv2");
                var query = new WqlEventQuery(
                    "SELECT * FROM __InstanceOperationEvent WITHIN 1 " +
                    "WHERE TargetInstance ISA 'MSFT_NetFirewallRule'");
                _watcher = new ManagementEventWatcher(scope, query);
                _watcher.EventArrived += OnFirewallRuleChangeEvent;
                _watcher.Start();
                _isStarted = true;
            }
            catch (ManagementException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SENTRY ERROR] Failed to start WMI watcher: {ex.Message}");
            }
        }

        public void Stop()
        {
            if (!_isStarted)
            {
                return;
            }

            try
            {
                _watcher?.Stop();
                _watcher?.Dispose();
                _watcher = null;
                _isStarted = false;
            }
            catch (ManagementException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SENTRY ERROR] Failed to stop WMI watcher: {ex.Message}");
            }
        }

        private void OnFirewallRuleChangeEvent(object sender, EventArrivedEventArgs e)
        {
            RuleSetChanged?.Invoke();
        }

        public void CreateBaseline()
        {
            _ruleBaseline.Clear();
            var allRules = firewallService.GetAllRules();
            foreach (var rule in allRules.Where(r => r != null && !string.IsNullOrEmpty(r.Name) && !IsMfwRule(r)))
            {
                _ruleBaseline[rule.Name] = GenerateRuleHash(rule);
            }
            SaveBaseline();
        }

        public void ClearBaseline()
        {
            _ruleBaseline.Clear();
            SaveBaseline();
        }

        private void LoadBaseline()
        {
            try
            {
                if (File.Exists(_baselinePath))
                {
                    string json = File.ReadAllText(_baselinePath);
                    _ruleBaseline = JsonSerializer.Deserialize(json, SentryJsonContext.Default.DictionaryStringString) ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Failed to load Sentry baseline: {ex.Message}");
                _ruleBaseline = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private void SaveBaseline()
        {
            try
            {
                string json = JsonSerializer.Serialize(_ruleBaseline, SentryJsonContext.Default.DictionaryStringString);
                File.WriteAllText(_baselinePath, json);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Failed to save Sentry baseline: {ex.Message}");
            }
        }

        public List<FirewallRuleChange> CheckForChanges(ForeignRuleTracker acknowledgedTracker)
        {
            var changes = new List<FirewallRuleChange>();
            var currentRules = firewallService.GetAllRules()
                .Where(r => r != null && !string.IsNullOrEmpty(r.Name))
                .GroupBy(r => r.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
            foreach (var rule in currentRules.Values)
            {
                if (IsMfwRule(rule) || acknowledgedTracker.IsAcknowledged(rule.Name))
                {
                    continue;
                }

                if (!_ruleBaseline.ContainsKey(rule.Name))
                {
                    changes.Add(new FirewallRuleChange { Type = ChangeType.New, Rule = FirewallDataService.CreateAdvancedRuleViewModel(rule) });
                }
                else
                {
                    var newHash = GenerateRuleHash(rule);
                    if (_ruleBaseline.TryGetValue(rule.Name, out var oldHash) && oldHash != newHash)
                    {
                        changes.Add(new FirewallRuleChange { Type = ChangeType.Modified, Rule = FirewallDataService.CreateAdvancedRuleViewModel(rule) });
                    }
                }
            }

            foreach (var baselineRuleName in _ruleBaseline.Keys)
            {
                if (!acknowledgedTracker.IsAcknowledged(baselineRuleName) && !currentRules.ContainsKey(baselineRuleName))
                {
                    changes.Add(new FirewallRuleChange { Type = ChangeType.Deleted, Rule = new AdvancedRuleViewModel { Name = baselineRuleName, Description = "This rule was deleted by an external process." } });
                }
            }

            return changes;
        }

        private static bool IsMfwRule(NetFwTypeLib.INetFwRule2 rule)
        {
            if (string.IsNullOrEmpty(rule.Grouping)) return false;
            return rule.Grouping.EndsWith(MFWConstants.MfwRuleSuffix) ||
                   rule.Grouping == "Minimal Firewall" ||
                   rule.Grouping == "Minimal Firewall (Wildcard)";
        }

        private static string GenerateRuleHash(NetFwTypeLib.INetFwRule2 rule)
        {
            var ruleProperties = new FirewallRuleHashModel
            {
                Name = rule.Name,
                Description = rule.Description,
                ApplicationName = rule.ApplicationName,
                ServiceName = rule.serviceName,
                Protocol = rule.Protocol,
                LocalPorts = rule.LocalPorts,
                RemotePorts = rule.RemotePorts,
                LocalAddresses = rule.LocalAddresses,
                RemoteAddresses = rule.RemoteAddresses,
                Direction = rule.Direction,
                Action = rule.Action,
                Enabled = rule.Enabled
            };
            byte[] jsonBytes = JsonSerializer.SerializeToUtf8Bytes(ruleProperties, SentryJsonContext.Default.FirewallRuleHashModel);
            byte[] hashBytes = SHA256.HashData(jsonBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }
    }

    [JsonSourceGenerationOptions(WriteIndented = false, PropertyNameCaseInsensitive = true)]
    [JsonSerializable(typeof(FirewallRuleHashModel))]
    [JsonSerializable(typeof(Dictionary<string, string>))]
    internal partial class SentryJsonContext : JsonSerializerContext
    {
    }
}