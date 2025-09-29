// File: C:/Users/anon/PROGRAMMING/C#/SimpleFirewall/VS Minimal Firewall/MinimalFirewall-NET8/MinimalFirewall-WindowsStore/DataModels.cs
// File: DataModels.cs
using System.IO;
using System.ComponentModel;
using NetFwTypeLib;
using System.Text.Json.Serialization;
using MinimalFirewall.TypedObjects;

namespace MinimalFirewall
{
    public enum SearchMode { Name, Path }
    public enum RuleType { Program, Service, UWP, Wildcard, Advanced }
    public enum ChangeType { New, Modified, Deleted }

    public class FirewallRuleChange
    {
        public ChangeType Type { get; set; }
        public AdvancedRuleViewModel Rule { get; set; } = new();
    }

    public class UnifiedRuleViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public RuleType Type { get; set; }

        [JsonIgnore]
        public string RuleTarget
        {
            get
            {
                return Type switch
                {
                    RuleType.Program => Path,
                    RuleType.Service => Name,
                    RuleType.UWP => UwpPackageFamilyName ?? string.Empty,
                    _ => string.Empty
                };
            }
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? UwpPackageFamilyName { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public WildcardRule? WildcardDefinition { get; set; }
    }

    public class AggregatedRuleViewModel : AdvancedRuleViewModel
    {
        public List<AdvancedRuleViewModel> UnderlyingRules { get; set; } = [];
    }

    public class AdvancedRuleViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public Directions Direction { get; set; }
        public ICollection<PortRange> LocalPorts { get; set; } = [];
        public ICollection<PortRange> RemotePorts { get; set; } = [];
        public short Protocol { get; set; }
        public string ProtocolName { get; set; } = string.Empty;
        public string ApplicationName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public ICollection<IPAddressRange> LocalAddresses { get; set; } = [];
        public ICollection<IPAddressRange> RemoteAddresses { get; set; } = [];
        public string Profiles { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Grouping { get; set; } = string.Empty;
        public RuleType Type { get; set; }
        public WildcardRule? WildcardDefinition { get; set; }
    }

    public class FirewallRuleHashModel
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ApplicationName { get; set; }
        public string? ServiceName { get; set; }
        public int Protocol { get; set; }
        public string? LocalPorts { get; set; }
        public string? RemotePorts { get; set; }
        public string? LocalAddresses { get; set; }
        public string? RemoteAddresses { get; set; }
        public NET_FW_RULE_DIRECTION_ Direction { get; set; }
        public NET_FW_ACTION_ Action { get; set; }
        public bool Enabled { get; set; }
    }

    public class ProgramViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string ExePath { get; set; } = string.Empty;
    }

    public class RuleFilterViewModel : INotifyPropertyChanged
    {
        private bool _isEnabled = true;
        public string Name { get; set; } = string.Empty;
        public RuleType Type { get; set; }
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEnabled)));
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public class PendingConnectionViewModel
    {
        public string AppPath { get; set; } = string.Empty;
        public string FileName => Path.GetFileName(AppPath);
        public string Direction { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
    }

    public class WildcardRule
    {
        public string FolderPath { get; set; } = string.Empty;
        public string ExeName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
    }

    [JsonSerializable(typeof(List<WildcardRule>))]
    internal partial class WildcardRuleJsonContext : JsonSerializerContext { }

    public class UwpApp
    {
        public string Name { get; set; } = string.Empty;
        public string PackageFamilyName { get; set; } = string.Empty;
        public string Status { get; set; } = "Undefined";
    }

    [JsonSerializable(typeof(List<UwpApp>))]
    internal partial class UwpAppJsonContext : JsonSerializerContext { }

    public class ServiceViewModel
    {
        public string ServiceName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string ExePath { get; set; } = string.Empty;
    }

    public class RuleCacheModel
    {
        public string? ProgramRules { get; set; }
        public string? AdvancedRules { get; set; }
    }

    [JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonSerializable(typeof(List<UnifiedRuleViewModel>))]
    [JsonSerializable(typeof(List<AdvancedRuleViewModel>))]
    [JsonSerializable(typeof(RuleCacheModel))]
    internal partial class CacheJsonContext : JsonSerializerContext { }

    public enum FirewallTaskType
    {
        ApplyApplicationRule,
        ApplyServiceRule,
        ApplyUwpRule,
        DeleteApplicationRules,
        DeleteUwpRules,
        DeleteAdvancedRules,
        DeleteGroup,
        DeleteWildcardRules,
        ProcessPendingConnection,
        AcceptForeignRule,
        DeleteForeignRule,
        AcceptAllForeignRules,
        CreateAdvancedRule,
        AddWildcardRule
    }

    public class FirewallTask
    {
        public FirewallTaskType TaskType { get; set; }
        public object Payload { get; set; }

        public FirewallTask(FirewallTaskType taskType, object payload)
        {
            TaskType = taskType;
            Payload = payload;
        }
    }

    public class ApplyApplicationRulePayload { public List<string> AppPaths { get; set; } = []; public string Action { get; set; } = ""; public string? WildcardSourcePath { get; set; } }
    public class ApplyServiceRulePayload { public string ServiceName { get; set; } = ""; public string Action { get; set; } = ""; }
    public class ApplyUwpRulePayload { public List<UwpApp> UwpApps { get; set; } = []; public string Action { get; set; } = ""; }
    public class DeleteRulesPayload { public List<string> RuleIdentifiers { get; set; } = []; }
    public class DeleteWildcardRulePayload { public WildcardRule Wildcard { get; set; } = new(); }
    public class ProcessPendingConnectionPayload { public PendingConnectionViewModel PendingConnection { get; set; } = new(); public string Decision { get; set; } = ""; public TimeSpan Duration { get; set; } = default; public bool TrustPublisher { get; set; } = false; }
    public class ForeignRuleChangePayload { public FirewallRuleChange Change { get; set; } = new(); }
    public class AllForeignRuleChangesPayload { public List<FirewallRuleChange> Changes { get; set; } = []; }
    public class CreateAdvancedRulePayload { public AdvancedRuleViewModel ViewModel { get; set; } = new(); public string InterfaceTypes { get; set; } = ""; public string IcmpTypesAndCodes { get; set; } = ""; }
}