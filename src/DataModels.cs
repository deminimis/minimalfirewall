using System.IO;
using System.ComponentModel;
using NetFwTypeLib;
using System.Text.Json.Serialization;
using MinimalFirewall.TypedObjects;

namespace MinimalFirewall;

public class ExportContainer
{
    public DateTime ExportDate { get; set; }
    public List<AdvancedRuleViewModel> AdvancedRules { get; set; } = [];
    public List<WildcardRule> WildcardRules { get; set; } = [];
}

public enum SearchMode { Name, Path }
public enum RuleType { Program, Service, UWP, Wildcard, Advanced }
public enum ChangeType { New, Modified, Deleted }

public class FirewallRuleChange
{
    public ChangeType Type { get; set; }

    // Ensure Rule is never null to prevent crashes in computed properties below
    private AdvancedRuleViewModel _rule = new();
    public AdvancedRuleViewModel Rule
    {
        get => _rule;
        set => _rule = value ?? new();
    }

    public AdvancedRuleViewModel? OldRule { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;

    // Computed properties for easy UI binding or logging
    public string Name => Rule.Name;
    public string Status => Rule.Status;
    public string ProtocolName => Rule.ProtocolName;
    public string LocalPorts => Rule.LocalPorts;
    public string RemotePorts => Rule.RemotePorts;
    public string LocalAddresses => Rule.LocalAddresses;
    public string RemoteAddresses => Rule.RemoteAddresses;
    public string ApplicationName => Rule.ApplicationName;
    public string ServiceName => Rule.ServiceName;
    public string Profiles => Rule.Profiles;
    public string Grouping => Rule.Grouping;
    public string Description => Rule.Description;
    public string Publisher { get; set; } = string.Empty;
}

public class UnifiedRuleViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public RuleType Type { get; set; }

    [JsonIgnore]
    public string RuleTarget => Type switch
    {
        RuleType.Program => Path,
        RuleType.Service => Name,
        RuleType.UWP => UwpPackageFamilyName ?? string.Empty,
        _ => string.Empty
    };

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? UwpPackageFamilyName { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public WildcardRule? WildcardDefinition { get; set; }
}

public class AggregatedRuleViewModel : AdvancedRuleViewModel
{
    public string InboundStatus { get; set; } = string.Empty;
    public string OutboundStatus { get; set; } = string.Empty;
    public List<AdvancedRuleViewModel> UnderlyingRules { get; set; } = [];
}

public class AdvancedRuleViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public Directions Direction { get; set; }
    public string LocalPorts { get; set; } = string.Empty;
    public string RemotePorts { get; set; } = string.Empty;
    public int Protocol { get; set; }
    public string ProtocolName { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string LocalAddresses { get; set; } = string.Empty;
    public string RemoteAddresses { get; set; } = string.Empty;
    public string Profiles { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Grouping { get; set; } = string.Empty;
    public RuleType Type { get; set; }
    public WildcardRule? WildcardDefinition { get; set; }
    public string InterfaceTypes { get; set; } = string.Empty;
    public string IcmpTypesAndCodes { get; set; } = string.Empty;

    // Checks if all firewall-relevant properties match (Case-Insensitive for strings)
    public bool HasSameSettings(AdvancedRuleViewModel? other)
    {
        if (other is null) return false;

        static bool Eq(string? a, string? b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

        return Eq(Name, other.Name) &&
               Eq(Description, other.Description) &&
               IsEnabled == other.IsEnabled &&
               Eq(Status, other.Status) &&
               Direction == other.Direction &&
               Protocol == other.Protocol &&
               Eq(ApplicationName, other.ApplicationName) &&
               Eq(ServiceName, other.ServiceName) &&
               Eq(LocalPorts, other.LocalPorts) &&
               Eq(RemotePorts, other.RemotePorts) &&
               Eq(LocalAddresses, other.LocalAddresses) &&
               Eq(RemoteAddresses, other.RemoteAddresses) &&
               Eq(Profiles, other.Profiles) &&
               Eq(Grouping, other.Grouping) &&
               Eq(InterfaceTypes, other.InterfaceTypes) &&
               Eq(IcmpTypesAndCodes, other.IcmpTypesAndCodes);
    }
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
    public string Protocol { get; set; } = string.Empty;
    public string RemotePort { get; set; } = string.Empty;
    public string RemoteAddress { get; set; } = string.Empty;
    public string FilterId { get; set; } = string.Empty;
    public string LayerId { get; set; } = string.Empty;
}

public class WildcardRule
{
    public string FolderPath { get; set; } = string.Empty;
    public string ExeName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public int Protocol { get; set; } = 256; // 256 is the magic number for "Any" protocol
    public string LocalPorts { get; set; } = "*";
    public string RemotePorts { get; set; } = "*";
    public string RemoteAddresses { get; set; } = "*";
}

[JsonSerializable(typeof(List<WildcardRule>))]
internal partial class WildcardRuleJsonContext : JsonSerializerContext { }

[JsonSerializable(typeof(ExportContainer))]
internal partial class ExportContainerJsonContext : JsonSerializerContext { }

public class UwpApp
{
    public string Name { get; set; } = string.Empty;
    public string PackageFamilyName { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
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
    EnableForeignRule,
    DeleteForeignRule,
    DisableForeignRule,
    AcceptAllForeignRules,
    CreateAdvancedRule,
    AddWildcardRule,
    SetGroupEnabledState,
    UpdateWildcardRule,
    RemoveWildcardRule,
    RemoveWildcardDefinitionOnly,
    DeleteAllMfwRules,
    ImportRules,
    QuarantineForeignRule
}

public class FirewallTask
{
    public FirewallTaskType TaskType { get; set; }
    public object Payload { get; set; }
    public string Description { get; set; }

    public FirewallTask(FirewallTaskType taskType, object payload, string description = "Processing...")
    {
        TaskType = taskType;
        Payload = payload;
        Description = description;
    }

    // Generic helper to avoid manual casting in Logic/UI layers
    public T? GetPayload<T>() where T : class
    {
        return Payload as T;
    }
}

// Payload DTOs converted to records for Immutability and built-in ToString() logging
public record ApplyApplicationRulePayload { public List<string> AppPaths { get; init; } = []; public string Action { get; init; } = ""; public string? WildcardSourcePath { get; init; } }
public record ApplyServiceRulePayload { public string ServiceName { get; init; } = ""; public string Action { get; init; } = ""; public string? AppPath { get; init; } }
public record ApplyUwpRulePayload { public List<UwpApp> UwpApps { get; init; } = []; public string Action { get; init; } = ""; }
public record DeleteRulesPayload { public List<string> RuleIdentifiers { get; init; } = []; }
public record DeleteWildcardRulePayload { public WildcardRule Wildcard { get; init; } = new(); }
public record ProcessPendingConnectionPayload { public PendingConnectionViewModel PendingConnection { get; init; } = new(); public string Decision { get; init; } = ""; public TimeSpan Duration { get; init; } = default; public bool TrustPublisher { get; init; } = false; }
public record ForeignRuleChangePayload { public FirewallRuleChange Change { get; init; } = new(); public bool Acknowledge { get; init; } = true; }
public record AllForeignRuleChangesPayload { public List<FirewallRuleChange> Changes { get; init; } = []; }
public record CreateAdvancedRulePayload { public AdvancedRuleViewModel ViewModel { get; init; } = new(); public string InterfaceTypes { get; init; } = ""; public string IcmpTypesAndCodes { get; init; } = ""; }
public record SetGroupEnabledStatePayload { public string GroupName { get; init; } = string.Empty; public bool IsEnabled { get; init; } }
public record UpdateWildcardRulePayload { public WildcardRule OldRule { get; init; } = new(); public WildcardRule NewRule { get; init; } = new(); }
public record ImportRulesPayload { public string JsonContent { get; init; } = string.Empty; public bool Replace { get; init; } }