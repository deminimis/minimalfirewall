using System.Diagnostics;
using System.Runtime.InteropServices;
using MinimalFirewall.TypedObjects;
using NetFwTypeLib;

namespace MinimalFirewall
{
    public class FirewallRuleService
    {
        private const int E_ACCESSDENIED = unchecked((int)0x80070005);
        private const int HRESULT_FROM_WIN32_ERROR_FILE_NOT_FOUND = unchecked((int)0x80070002);
        private const int HRESULT_FROM_WIN32_ERROR_ALREADY_EXISTS = unchecked((int)0x800700B7);
        private static readonly Lazy<Type?> _firewallPolicyType = new(() => Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));

        public FirewallRuleService()
        {
        }

        private static INetFwPolicy2 GetLocalPolicy()
        {
            if (_firewallPolicyType.Value == null)
            {
                throw new InvalidOperationException("Firewall policy type could not be retrieved.");
            }
            return (INetFwPolicy2)Activator.CreateInstance(_firewallPolicyType.Value)!;
        }

        public static List<T> GetAllRulesMapped<T>(Func<INetFwRule2, T> mapper)
        {
            var mappedList = new List<T>();
            INetFwPolicy2 firewallPolicy = GetLocalPolicy();
            if (firewallPolicy?.Rules == null)
            {
                return mappedList;
            }

            var comRules = firewallPolicy.Rules;
            try
            {
                foreach (INetFwRule2 rule in comRules)
                {
                    if (rule == null)
                    {
                        continue;
                    }

                    try
                    {
                        mappedList.Add(mapper(rule));
                    }
                    catch (Exception ex)
                    {
                        // Ignore rules that fail to map
                        Debug.WriteLine($"[WARN] GetAllRulesMapped: Failed to map firewall rule: {ex.Message}");
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(rule);
                    }
                }
            }
            finally
            {
                if (comRules != null)
                {
                    Marshal.ReleaseComObject(comRules);
                }

                if (firewallPolicy != null)
                {
                    Marshal.ReleaseComObject(firewallPolicy);
                }
            }
            return mappedList;
        }

        private sealed record ApplicationReplacementPlan
        {
            public string AppPath { get; init; } = string.Empty;
            public Directions Directions { get; init; }
            public string Action { get; init; } = string.Empty;
            public List<FirewallRuleSnapshot> ExistingRules { get; init; } = [];
        }

        private sealed record ApplicationDeletionPlan
        {
            public string AppPath { get; init; } = string.Empty;
            public List<FirewallRuleSnapshot> ExistingRules { get; init; } = [];
        }

        private sealed record FirewallRuleSnapshot
        {
            public string Name { get; init; } = string.Empty;
            public string Description { get; init; } = string.Empty;
            public string ApplicationName { get; init; } = string.Empty;
            public string ServiceName { get; init; } = string.Empty;
            public int Protocol { get; init; }
            public string LocalPorts { get; init; } = string.Empty;
            public string RemotePorts { get; init; } = string.Empty;
            public string IcmpTypesAndCodes { get; init; } = string.Empty;
            public string LocalAddresses { get; init; } = string.Empty;
            public string RemoteAddresses { get; init; } = string.Empty;
            public NET_FW_RULE_DIRECTION_ Direction { get; init; }
            public NET_FW_ACTION_ Action { get; init; }
            public bool Enabled { get; init; }
            public int Profiles { get; init; }
            public string Grouping { get; init; } = string.Empty;
            public string InterfaceTypes { get; init; } = string.Empty;
            public bool EdgeTraversal { get; init; }
        }

        public static List<INetFwRule2> GetAllRules()
        {
            INetFwPolicy2 firewallPolicy = GetLocalPolicy();
            if (firewallPolicy?.Rules == null)
            {
                return [];
            }

            var rulesList = new List<INetFwRule2>();
            var comRules = firewallPolicy.Rules;
            try
            {
                foreach (INetFwRule2 rule in comRules)
                {
                    rulesList.Add(rule);
                }
                return rulesList;
            }
            catch (COMException ex)
            {
                Debug.WriteLine($"[ERROR] GetAllRules: Failed to retrieve firewall rules. HResult: 0x{ex.HResult:X8}. Message: {ex.Message}");
                foreach (var rule in rulesList)
                {
                    Marshal.ReleaseComObject(rule);
                }
                return [];
            }
            finally
            {
                if (comRules != null)
                {
                    Marshal.ReleaseComObject(comRules);
                }

                if (firewallPolicy != null)
                {
                    Marshal.ReleaseComObject(firewallPolicy);
                }
            }
        }

        private static List<string> GetRuleNamesAndRelease(Func<INetFwRule2, bool> predicate)
        {
            var matchedNames = new List<string>();
            INetFwPolicy2 firewallPolicy = GetLocalPolicy();
            if (firewallPolicy?.Rules == null)
            {
                return matchedNames;
            }

            var comRules = firewallPolicy.Rules;
            try
            {
                foreach (INetFwRule2 rule in comRules)
                {
                    try
                    {
                        if (rule != null && predicate(rule))
                        {
                            matchedNames.Add(rule.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[WARN] Error checking rule predicate: {ex.Message}");
                    }
                    finally
                    {
                        if (rule != null)
                        {
                            Marshal.ReleaseComObject(rule);
                        }
                    }
                }
            }
            finally
            {
                if (comRules != null)
                {
                    Marshal.ReleaseComObject(comRules);
                }

                if (firewallPolicy != null)
                {
                    Marshal.ReleaseComObject(firewallPolicy);
                }
            }
            return matchedNames;
        }

        public static INetFwRule2? GetRuleByName(string name)
        {
            INetFwPolicy2 firewallPolicy = GetLocalPolicy();
            if (firewallPolicy == null)
            {
                return null;
            }

            INetFwRules? rulesCollection = null;
            try
            {
                rulesCollection = firewallPolicy.Rules;
                if (rulesCollection.Item(name) is INetFwRule2 rule)
                {
                    return rule;
                }
                return null;
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (COMException ex)
            {
                Debug.WriteLine($"[ERROR] GetRuleByName ('{name}'): COM error. HResult: 0x{ex.HResult:X8}. Message: {ex.Message}");
                return null;
            }
            finally
            {
                if (rulesCollection != null)
                {
                    Marshal.ReleaseComObject(rulesCollection);
                }

                if (firewallPolicy != null)
                {
                    Marshal.ReleaseComObject(firewallPolicy);
                }
            }
        }

        public static bool TryEditExactRule(
    AdvancedRuleViewModel originalRule,
    AdvancedRuleViewModel updatedRule,
    out string errorMessage)
        {
            errorMessage = string.Empty;

            if (originalRule == null || updatedRule == null)
            {
                errorMessage = "The original or edited rule definition was missing.";
                return false;
            }

            string originalName = NormalizeRuleText(originalRule.Name);
            string updatedName = NormalizeRuleText(updatedRule.Name);

            if (string.IsNullOrWhiteSpace(originalName))
            {
                errorMessage = "The selected rule has no name and cannot be edited safely.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(updatedName))
            {
                errorMessage = "The edited rule must have a name.";
                return false;
            }

            if (originalRule.Direction is not
                (Directions.Incoming or Directions.Outgoing))
            {
                errorMessage =
                    "The selected row does not represent exactly one inbound or " +
                    "outbound firewall rule. Refresh the rule list and select one " +
                    "individual rule.";
                return false;
            }

            if (updatedRule.Direction is not
                (Directions.Incoming or Directions.Outgoing))
            {
                errorMessage =
                    "Editing one existing rule cannot change it to both directions. " +
                    "Choose Inbound or Outbound. To create both directions, create " +
                    "a new rule instead.";
                return false;
            }

            INetFwPolicy2? firewallPolicy = null;
            INetFwRules? rulesCollection = null;
            INetFwRule2? matchedRule = null;
            INetFwRule2? validationRule = null;

            try
            {
                firewallPolicy = GetLocalPolicy();
                rulesCollection = firewallPolicy.Rules;

                int originalNameCount = 0;
                int updatedNameCount = 0;

                foreach (INetFwRule2 rule in rulesCollection)
                {
                    bool keepRule = false;

                    try
                    {
                        string existingName = ReadOptionalString(() => rule.Name);

                        if (string.Equals(
                                existingName,
                                originalName,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            originalNameCount++;

                            if (originalNameCount == 1)
                            {
                                matchedRule = rule;
                                keepRule = true;
                            }
                        }

                        if (!string.Equals(
                                updatedName,
                                originalName,
                                StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(
                                existingName,
                                updatedName,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            updatedNameCount++;
                        }
                    }
                    finally
                    {
                        if (!keepRule && rule != null)
                        {
                            Marshal.ReleaseComObject(rule);
                        }
                    }
                }

                if (originalNameCount == 0 || matchedRule == null)
                {
                    errorMessage =
                        $"The rule '{originalName}' no longer exists. Refresh the " +
                        "rule list before trying again.";
                    return false;
                }

                if (originalNameCount > 1)
                {
                    errorMessage =
                        $"Windows Firewall contains {originalNameCount} rules named " +
                        $"'{originalName}'. Windows exposes name-only removal and " +
                        "lookup behavior, so Minimal Firewall cannot determine which " +
                        "duplicate should be edited. Rename or remove the duplicate " +
                        "rules in Windows Defender Firewall with Advanced Security, " +
                        "refresh, and try again.";
                    return false;
                }

                if (updatedNameCount > 0)
                {
                    errorMessage =
                        $"Another firewall rule is already named '{updatedName}'. " +
                        "Choose a unique rule name and try again.";
                    return false;
                }

                FirewallRuleSnapshot currentSnapshot =
                    CreateRuleSnapshot(matchedRule);

                if (!SnapshotMatchesViewModel(
                        currentSnapshot,
                        originalRule))
                {
                    errorMessage =
                        $"The rule '{originalName}' changed after it was loaded. " +
                        "No changes were applied. Refresh the rule list, review the " +
                        "current settings, and try again.";
                    return false;
                }

                Type? firewallRuleType =
                    Type.GetTypeFromProgID("HNetCfg.FWRule");

                if (firewallRuleType == null)
                {
                    errorMessage =
                        "The Windows Firewall rule COM type is unavailable.";
                    return false;
                }

                validationRule =
                    (INetFwRule2)Activator.CreateInstance(firewallRuleType)!;

                ApplyViewModelToComRule(validationRule, updatedRule);

                FirewallRuleSnapshot expectedSnapshot =
                    CreateRuleSnapshot(validationRule);

                ApplyViewModelToComRule(matchedRule, updatedRule);

                FirewallRuleSnapshot actualSnapshot =
                    CreateRuleSnapshot(matchedRule);

                if (!AreRulesExactlyEqual(
                        expectedSnapshot,
                        actualSnapshot))
                {
                    errorMessage =
                        "Windows Firewall accepted the edit but did not preserve all " +
                        "requested settings. Refresh the rule list and review the " +
                        "rule before making further changes.";
                    return false;
                }

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                errorMessage =
                    "Windows denied the rule edit. Run Minimal Firewall as " +
                    "administrator and try again.";
                return false;
            }
            catch (COMException ex)
            {
                errorMessage =
                    $"Windows Firewall rejected the rule edit " +
                    $"(0x{ex.HResult:X8}): {ex.Message}";
                return false;
            }
            catch (Exception ex)
            {
                errorMessage =
                    $"The rule could not be edited safely: {ex.Message}";
                return false;
            }
            finally
            {
                if (validationRule != null)
                {
                    Marshal.ReleaseComObject(validationRule);
                }

                if (matchedRule != null)
                {
                    Marshal.ReleaseComObject(matchedRule);
                }

                if (rulesCollection != null)
                {
                    Marshal.ReleaseComObject(rulesCollection);
                }

                if (firewallPolicy != null)
                {
                    Marshal.ReleaseComObject(firewallPolicy);
                }
            }
        }

        public static bool TryDeleteExactRule(
    AdvancedRuleViewModel selectedRule,
    out string errorMessage)
        {
            errorMessage = string.Empty;

            if (selectedRule == null)
            {
                errorMessage = "The selected rule definition was missing.";
                return false;
            }

            string selectedName = NormalizeRuleText(selectedRule.Name);
            if (string.IsNullOrWhiteSpace(selectedName))
            {
                errorMessage =
                    "The selected rule has no usable name and cannot be deleted safely.";
                return false;
            }

            if (selectedRule.Direction is not
                (Directions.Incoming or Directions.Outgoing))
            {
                errorMessage =
                    "The selected row does not identify exactly one inbound or " +
                    "outbound Windows Firewall rule. Refresh the rule list and " +
                    "select one individual rule.";
                return false;
            }

            INetFwPolicy2? firewallPolicy = null;
            INetFwRules? rulesCollection = null;
            INetFwRule2? matchedRule = null;

            try
            {
                firewallPolicy = GetLocalPolicy();
                rulesCollection = firewallPolicy.Rules;

                int matchingNameCount = 0;

                foreach (INetFwRule2 rule in rulesCollection)
                {
                    bool keepRule = false;

                    try
                    {
                        string existingName =
                            ReadOptionalString(() => rule.Name);

                        if (string.Equals(
                                existingName,
                                selectedName,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            matchingNameCount++;

                            if (matchingNameCount == 1)
                            {
                                matchedRule = rule;
                                keepRule = true;
                            }
                        }
                    }
                    finally
                    {
                        if (!keepRule && rule != null)
                        {
                            Marshal.ReleaseComObject(rule);
                        }
                    }
                }

                if (matchingNameCount == 0 || matchedRule == null)
                {
                    errorMessage =
                        $"The rule '{selectedName}' no longer exists. Refresh " +
                        "the rule list before trying again.";
                    return false;
                }

                if (matchingNameCount > 1)
                {
                    errorMessage =
                        $"Windows Firewall contains {matchingNameCount} rules " +
                        $"named '{selectedName}'. Windows exposes name-only " +
                        "removal through INetFwRules.Remove, so Minimal Firewall " +
                        "cannot safely determine which duplicate would be deleted. " +
                        "No rules were changed. Rename or remove the duplicates " +
                        "in Windows Defender Firewall with Advanced Security, " +
                        "refresh, and try again.";
                    return false;
                }

                FirewallRuleSnapshot currentSnapshot =
                    CreateRuleSnapshot(matchedRule);

                if (!SnapshotMatchesViewModel(
                        currentSnapshot,
                        selectedRule))
                {
                    errorMessage =
                        $"The rule '{selectedName}' changed after it was loaded. " +
                        "No rule was deleted. Refresh the rule list, review the " +
                        "current settings, and try again.";
                    return false;
                }

                rulesCollection.Remove(selectedName);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                errorMessage =
                    "Windows denied the rule deletion. Run Minimal Firewall as " +
                    "administrator and try again.";
                return false;
            }
            catch (FileNotFoundException)
            {
                errorMessage =
                    $"The rule '{selectedName}' no longer exists. Refresh the " +
                    "rule list before trying again.";
                return false;
            }
            catch (COMException ex)
            {
                errorMessage =
                    $"Windows Firewall rejected the rule deletion " +
                    $"(0x{ex.HResult:X8}): {ex.Message}";
                return false;
            }
            catch (Exception ex)
            {
                errorMessage =
                    $"The selected rule could not be deleted safely: {ex.Message}";
                return false;
            }
            finally
            {
                if (matchedRule != null)
                {
                    Marshal.ReleaseComObject(matchedRule);
                }

                if (rulesCollection != null)
                {
                    Marshal.ReleaseComObject(rulesCollection);
                }

                if (firewallPolicy != null)
                {
                    Marshal.ReleaseComObject(firewallPolicy);
                }
            }
        }

        private static bool SnapshotMatchesViewModel(
            FirewallRuleSnapshot snapshot,
            AdvancedRuleViewModel viewModel)
        {
            static bool EqualText(string? left, string? right) =>
                string.Equals(
                    NormalizeRuleText(left),
                    NormalizeRuleText(right),
                    StringComparison.OrdinalIgnoreCase);

            return
                EqualText(snapshot.Name, viewModel.Name) &&
                EqualText(snapshot.Description, viewModel.Description) &&
                string.Equals(
                    NormalizeRulePath(snapshot.ApplicationName),
                    NormalizeRulePath(viewModel.ApplicationName),
                    StringComparison.OrdinalIgnoreCase) &&
                EqualText(snapshot.ServiceName, viewModel.ServiceName) &&
                snapshot.Protocol == viewModel.Protocol &&
                EqualText(snapshot.LocalPorts, viewModel.LocalPorts) &&
                EqualText(snapshot.RemotePorts, viewModel.RemotePorts) &&
                EqualText(
                    snapshot.IcmpTypesAndCodes,
                    viewModel.IcmpTypesAndCodes) &&
                EqualText(snapshot.LocalAddresses, viewModel.LocalAddresses) &&
                EqualText(snapshot.RemoteAddresses, viewModel.RemoteAddresses) &&
                snapshot.Direction ==
                    (NET_FW_RULE_DIRECTION_)viewModel.Direction &&
                snapshot.Action ==
                    (viewModel.Status.Equals(
                        "Allow",
                        StringComparison.OrdinalIgnoreCase)
                        ? NET_FW_ACTION_.NET_FW_ACTION_ALLOW
                        : NET_FW_ACTION_.NET_FW_ACTION_BLOCK) &&
                snapshot.Enabled == viewModel.IsEnabled &&
                snapshot.Profiles ==
                    ConvertProfiles(viewModel.Profiles) &&
                EqualText(snapshot.Grouping, viewModel.Grouping) &&
                EqualText(
                    snapshot.InterfaceTypes,
                    string.IsNullOrWhiteSpace(viewModel.InterfaceTypes)
                        ? "All"
                        : viewModel.InterfaceTypes);
        }

        private static int ConvertProfiles(string? profilesText)
        {
            if (string.IsNullOrWhiteSpace(profilesText) ||
                profilesText.Equals(
                    "All",
                    StringComparison.OrdinalIgnoreCase))
            {
                return (int)NET_FW_PROFILE_TYPE2_
                    .NET_FW_PROFILE2_ALL;
            }

            NET_FW_PROFILE_TYPE2_ profiles = 0;

            if (profilesText.Contains(
                    "Domain",
                    StringComparison.OrdinalIgnoreCase))
            {
                profiles |= NET_FW_PROFILE_TYPE2_
                    .NET_FW_PROFILE2_DOMAIN;
            }

            if (profilesText.Contains(
                    "Private",
                    StringComparison.OrdinalIgnoreCase))
            {
                profiles |= NET_FW_PROFILE_TYPE2_
                    .NET_FW_PROFILE2_PRIVATE;
            }

            if (profilesText.Contains(
                    "Public",
                    StringComparison.OrdinalIgnoreCase))
            {
                profiles |= NET_FW_PROFILE_TYPE2_
                    .NET_FW_PROFILE2_PUBLIC;
            }

            return profiles == 0
                ? (int)NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_ALL
                : (int)profiles;
        }

        private static void ApplyViewModelToComRule(
            INetFwRule2 rule,
            AdvancedRuleViewModel viewModel)
        {
            rule.Name = NormalizeRuleText(viewModel.Name);
            rule.Description =
                NormalizeRuleText(viewModel.Description);
            rule.Enabled = viewModel.IsEnabled;
            rule.Grouping =
                NormalizeRuleText(viewModel.Grouping);
            rule.Action =
                viewModel.Status.Equals(
                    "Allow",
                    StringComparison.OrdinalIgnoreCase)
                    ? NET_FW_ACTION_.NET_FW_ACTION_ALLOW
                    : NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
            rule.Direction =
                (NET_FW_RULE_DIRECTION_)viewModel.Direction;
            rule.Protocol = viewModel.Protocol;

            rule.ApplicationName =
                string.IsNullOrWhiteSpace(viewModel.ApplicationName)
                    ? null
                    : PathResolver.NormalizePath(
                        viewModel.ApplicationName);

            rule.serviceName =
                string.IsNullOrWhiteSpace(viewModel.ServiceName) ||
                viewModel.ServiceName == "*"
                    ? null
                    : viewModel.ServiceName.Trim();

            if (viewModel.Protocol is 6 or 17)
            {
                rule.LocalPorts =
                    string.IsNullOrWhiteSpace(viewModel.LocalPorts)
                        ? "*"
                        : viewModel.LocalPorts.Trim();

                rule.RemotePorts =
                    string.IsNullOrWhiteSpace(viewModel.RemotePorts)
                        ? "*"
                        : viewModel.RemotePorts.Trim();
            }

            rule.LocalAddresses =
                string.IsNullOrWhiteSpace(viewModel.LocalAddresses)
                    ? "*"
                    : viewModel.LocalAddresses.Trim();

            rule.RemoteAddresses =
                string.IsNullOrWhiteSpace(viewModel.RemoteAddresses)
                    ? "*"
                    : viewModel.RemoteAddresses.Trim();

            if (viewModel.Protocol is 1 or 58)
            {
                rule.IcmpTypesAndCodes =
                    string.IsNullOrWhiteSpace(
                        viewModel.IcmpTypesAndCodes)
                        ? "*"
                        : viewModel.IcmpTypesAndCodes.Trim();
            }

            rule.Profiles = ConvertProfiles(viewModel.Profiles);

            rule.InterfaceTypes =
                string.IsNullOrWhiteSpace(viewModel.InterfaceTypes)
                    ? "All"
                    : viewModel.InterfaceTypes.Trim();
        }

        public static ApplicationRuleDeletionPreview
    PreviewApplicationRuleDeletion(string appPath)
        {
            ApplicationDeletionPlan plan =
                BuildApplicationDeletionPlan(appPath);

            return new ApplicationRuleDeletionPreview
            {
                AppPath = plan.AppPath,
                MatchingRuleCount = plan.ExistingRules.Count,
                InboundRuleCount = plan.ExistingRules.Count(rule =>
                    rule.Direction ==
                    NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN),
                OutboundRuleCount = plan.ExistingRules.Count(rule =>
                    rule.Direction ==
                    NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT),
                AllowRuleCount = plan.ExistingRules.Count(rule =>
                    rule.Action ==
                    NET_FW_ACTION_.NET_FW_ACTION_ALLOW),
                BlockRuleCount = plan.ExistingRules.Count(rule =>
                    rule.Action ==
                    NET_FW_ACTION_.NET_FW_ACTION_BLOCK),
                MinimalFirewallRuleCount = plan.ExistingRules.Count(
                    IsMinimalFirewallRule),
                OtherRuleCount = plan.ExistingRules.Count(rule =>
                    !IsMinimalFirewallRule(rule)),
                RuleNames = plan.ExistingRules
                    .Select(rule => DisplayValue(rule.Name))
                    .OrderBy(
                        name => name,
                        StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                RuleTypes = DistinctSummary(
                    plan.ExistingRules.Select(GetDeletionRuleType))
            };
        }

        public static void DeleteAllRulesForApplication(string appPath)
        {
            ApplicationDeletionPlan plan =
                BuildApplicationDeletionPlan(appPath);

            if (plan.ExistingRules.Count == 0)
            {
                return;
            }

            if (plan.ExistingRules.Any(rule =>
                string.IsNullOrWhiteSpace(rule.Name)))
            {
                throw new InvalidOperationException(
                    "At least one matching firewall rule has no usable name. " +
                    "No rules were deleted.");
            }

            List<FirewallRuleSnapshot> allRules =
                GetAllRulesMapped(CreateRuleSnapshot);

            var targetNames = plan.ExistingRules
                .Select(rule => rule.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            List<string> ambiguousNames = allRules
                .Where(rule => targetNames.Contains(rule.Name))
                .GroupBy(
                    rule => rule.Name,
                    StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() != 1)
                .Select(group => group.Key)
                .OrderBy(
                    name => name,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (ambiguousNames.Count > 0)
            {
                throw new InvalidOperationException(
                    "Application-wide deletion was stopped before deleting " +
                    "anything because Windows Firewall contains duplicate rule " +
                    "names: " +
                    string.Join(", ", ambiguousNames.Take(5)) +
                    (ambiguousNames.Count > 5
                        ? $" and {ambiguousNames.Count - 5} more."
                        : string.Empty) +
                    " INetFwRules.Remove deletes by name, so Minimal Firewall " +
                    "cannot safely guarantee which duplicate would be removed.");
            }

            List<string> namesToDelete = plan.ExistingRules
                .Select(rule => rule.Name)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            DeleteRulesByName(namesToDelete);

            ApplicationDeletionPlan remaining =
                BuildApplicationDeletionPlan(plan.AppPath);

            if (remaining.ExistingRules.Count > 0)
            {
                throw new InvalidOperationException(
                    $"{remaining.ExistingRules.Count} matching firewall rule(s) " +
                    "remained after application-wide deletion. Refresh the rule " +
                    "list and review the remaining rules.");
            }
        }

        private static ApplicationDeletionPlan
            BuildApplicationDeletionPlan(string appPath)
        {
            string normalizedPath = NormalizeRulePath(appPath);

            if (string.IsNullOrWhiteSpace(normalizedPath) ||
                normalizedPath == "*" ||
                normalizedPath.StartsWith('@'))
            {
                throw new InvalidOperationException(
                    "A fully qualified executable path is required.");
            }

            List<FirewallRuleSnapshot> matches =
                GetAllRulesMapped(CreateRuleSnapshot)
                    .Where(rule =>
                        string.Equals(
                            NormalizeRulePath(rule.ApplicationName),
                            normalizedPath,
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();

            return new ApplicationDeletionPlan
            {
                AppPath = normalizedPath,
                ExistingRules = matches
            };
        }

        private static string GetDeletionRuleType(
            FirewallRuleSnapshot rule)
        {
            string owner = IsMinimalFirewallRule(rule)
                ? "Minimal Firewall"
                : "Windows, manual, or third-party";

            string targetType =
                string.IsNullOrWhiteSpace(rule.ServiceName)
                    ? "application"
                    : "service-scoped application";

            string direction =
                rule.Direction ==
                NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN
                    ? "inbound"
                    : rule.Direction ==
                      NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT
                        ? "outbound"
                        : "other-direction";

            string action =
                rule.Action == NET_FW_ACTION_.NET_FW_ACTION_ALLOW
                    ? "Allow"
                    : "Block";

            return $"{owner}: {direction} {action} {targetType}";
        }

        public static ApplicationRuleReplacementPreview PreviewApplicationRuleReplacement(
    string appPath,
    Directions directions,
    string action)
        {
            ApplicationReplacementPlan plan =
                BuildApplicationReplacementPlan(appPath, directions, action);

            return new ApplicationRuleReplacementPreview
            {
                AppPath = plan.AppPath,
                Directions = plan.Directions,
                Action = plan.Action,
                MatchingRuleCount = plan.ExistingRules.Count,
                InboundRuleCount = plan.ExistingRules.Count(r =>
                    r.Direction ==
                    NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN),
                OutboundRuleCount = plan.ExistingRules.Count(r =>
                    r.Direction ==
                    NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT),
                IncludesThirdPartyRules =
                    plan.ExistingRules.Any(r => !IsMinimalFirewallRule(r)),
                Profiles = DistinctSummary(
                    plan.ExistingRules.Select(r =>
                        ProfilesToDisplayText(r.Profiles))),
                Protocols = DistinctSummary(
                    plan.ExistingRules.Select(r =>
                        ProtocolToDisplayText(r.Protocol))),
                LocalPorts = DistinctSummary(
                    plan.ExistingRules.Select(r =>
                        DisplayValue(r.LocalPorts))),
                RemotePorts = DistinctSummary(
                    plan.ExistingRules.Select(r =>
                        DisplayValue(r.RemotePorts))),
                LocalAddresses = DistinctSummary(
                    plan.ExistingRules.Select(r =>
                        DisplayValue(r.LocalAddresses))),
                RemoteAddresses = DistinctSummary(
                    plan.ExistingRules.Select(r =>
                        DisplayValue(r.RemoteAddresses))),
                Services = DistinctSummary(
                    plan.ExistingRules.Select(r =>
                        string.IsNullOrWhiteSpace(r.ServiceName)
                            ? "(application rule)"
                            : r.ServiceName))
            };
        }

        public static void ReplaceApplicationRules(
            string appPath,
            Directions directions,
            string action)
        {
            ApplicationReplacementPlan plan =
                BuildApplicationReplacementPlan(appPath, directions, action);

            ValidateApplicationReplacement(plan);

            List<FirewallRuleSnapshot> replacements =
                BuildReplacementSnapshots(plan);

            // Validate every replacement COM rule before deleting anything.
            foreach (FirewallRuleSnapshot replacement in replacements)
            {
                ValidateSnapshotCanBeCreated(replacement);
            }

            List<FirewallRuleSnapshot> allRules =
                GetAllRulesMapped(CreateRuleSnapshot);

                        HashSet<string> targetNames = plan.ExistingRules
                            .Select(rule => rule.Name)
                            .Where(name => !string.IsNullOrWhiteSpace(name))
                            .ToHashSet(StringComparer.OrdinalIgnoreCase);

                        List<string> duplicateNames = allRules
                            .Where(rule => targetNames.Contains(rule.Name))
                            .GroupBy(
                                rule => rule.Name,
                                StringComparer.OrdinalIgnoreCase)
                            .Where(group => group.Count() != 1)
                            .Select(group => group.Key)
                            .OrderBy(
                                name => name,
                                StringComparer.OrdinalIgnoreCase)
                            .ToList();

            if (duplicateNames.Count > 0)
            {
                throw new InvalidOperationException(
                    "Replacement was stopped before deleting anything because " +
                    "Windows Firewall contains duplicate matching rule names: " +
                    string.Join(", ", duplicateNames.Take(5)) +
                    (duplicateNames.Count > 5
                        ? $" and {duplicateNames.Count - 5} more."
                        : string.Empty) +
                    " Windows Firewall removes rules by name, so these rules " +
                    "cannot be snapshotted and restored safely.");
            }

            List<string> existingNames = plan.ExistingRules
                .Select(r => r.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var createdNames = new List<string>();

            try
            {
                DeleteRulesByName(existingNames);

                // Confirm that deletion really completed before adding replacements.
                ApplicationReplacementPlan afterDelete =
                    BuildApplicationReplacementPlan(
                        plan.AppPath,
                        plan.Directions,
                        plan.Action);

                if (afterDelete.ExistingRules.Count != 0)
                {
                    throw new InvalidOperationException(
                        $"{afterDelete.ExistingRules.Count} matching firewall " +
                        "rule(s) remained after deletion. Replacement was stopped.");
                }

                foreach (FirewallRuleSnapshot replacement in replacements)
                {
                    CreateRuleFromSnapshot(replacement);
                    createdNames.Add(replacement.Name);
                }

                VerifyReplacement(plan, replacements);
            }
            catch (Exception replacementError)
            {
                var rollbackErrors = new List<string>();

                try
                {
                    DeleteRulesByName(
                        createdNames
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToList());
                }
                catch (Exception ex)
                {
                    rollbackErrors.Add(
                        $"Could not remove replacement rules: {ex.Message}");
                }

                foreach (FirewallRuleSnapshot original in plan.ExistingRules)
                {
                    try
                    {
                        CreateRuleFromSnapshot(original);
                    }
                    catch (Exception ex)
                    {
                        rollbackErrors.Add(
                            $"Could not restore '{original.Name}': {ex.Message}");
                    }
                }

                if (rollbackErrors.Count == 0)
                {
                    throw new InvalidOperationException(
                        "The replacement failed. The original firewall rules " +
                        $"were restored. Details: {replacementError.Message}",
                        replacementError);
                }

                throw new InvalidOperationException(
                    "The replacement failed and rollback was incomplete. " +
                    $"Original error: {replacementError.Message}\r\n\r\n" +
                    string.Join("\r\n", rollbackErrors.Take(5)),
                    replacementError);
            }
        }

        private static ApplicationReplacementPlan BuildApplicationReplacementPlan(
            string appPath,
            Directions directions,
            string action)
        {
            string normalizedPath = NormalizeRulePath(appPath);

            if (string.IsNullOrWhiteSpace(normalizedPath) ||
                normalizedPath == "*" ||
                normalizedPath.StartsWith('@'))
            {
                throw new InvalidOperationException(
                    "A fully qualified executable path is required.");
            }

            if (directions == 0 ||
                (directions & ~(Directions.Incoming | Directions.Outgoing)) != 0)
            {
                throw new InvalidOperationException(
                    "Choose inbound, outbound, or both directions.");
            }

            if (!action.Equals("Allow", StringComparison.OrdinalIgnoreCase) &&
                !action.Equals("Block", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "The replacement action must be Allow or Block.");
            }

            List<FirewallRuleSnapshot> matches = GetAllRulesMapped(rule =>
                CreateRuleSnapshot(rule))
                .Where(rule =>
                    string.Equals(
                        NormalizeRulePath(rule.ApplicationName),
                        normalizedPath,
                        StringComparison.OrdinalIgnoreCase) &&
                    DirectionMatches(rule.Direction, directions))
                .ToList();

            return new ApplicationReplacementPlan
            {
                AppPath = normalizedPath,
                Directions = directions,
                Action = action,
                ExistingRules = matches
            };
        }

        private static void ValidateApplicationReplacement(
            ApplicationReplacementPlan plan)
        {
            if (!File.Exists(plan.AppPath) &&
                !plan.AppPath.Contains(
                    "WindowsApps",
                    StringComparison.OrdinalIgnoreCase) &&
                !plan.AppPath.Contains(
                    "SystemApps",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new FileNotFoundException(
                    "The executable no longer exists. No firewall rules were changed.",
                    plan.AppPath);
            }

            if (plan.ExistingRules.Any(r =>
                string.IsNullOrWhiteSpace(r.Name)))
            {
                throw new InvalidOperationException(
                    "At least one matching rule has no usable name. " +
                    "No firewall rules were changed.");
            }
        }

        private static List<FirewallRuleSnapshot> BuildReplacementSnapshots(
            ApplicationReplacementPlan plan)
        {
            var replacements = new List<FirewallRuleSnapshot>();
            string appName =
                Path.GetFileNameWithoutExtension(plan.AppPath);

            NET_FW_ACTION_ replacementAction =
                plan.Action.Equals(
                    "Allow",
                    StringComparison.OrdinalIgnoreCase)
                    ? NET_FW_ACTION_.NET_FW_ACTION_ALLOW
                    : NET_FW_ACTION_.NET_FW_ACTION_BLOCK;

            void Add(
                Directions direction,
                NET_FW_RULE_DIRECTION_ comDirection,
                string directionName)
            {
                if (!plan.Directions.HasFlag(direction))
                {
                    return;
                }

                replacements.Add(new FirewallRuleSnapshot
                {
                    Name = $"MFW - {appName} - {directionName}",
                    Description =
                        $"Replaced application-wide by Minimal Firewall: " +
                        $"{plan.Action} {directionName}.",
                    ApplicationName = plan.AppPath,
                    ServiceName = string.Empty,
                    Protocol = 256,
                    LocalPorts = string.Empty,
                    RemotePorts = string.Empty,
                    IcmpTypesAndCodes = string.Empty,
                    LocalAddresses = "*",
                    RemoteAddresses = "*",
                    Direction = comDirection,
                    Action = replacementAction,
                    Enabled = true,
                    Profiles = (int)NET_FW_PROFILE_TYPE2_
                        .NET_FW_PROFILE2_ALL,
                    Grouping = MFWConstants.MainRuleGroup,
                    InterfaceTypes = "All",
                    EdgeTraversal = false
                });
            }

            Add(
                Directions.Outgoing,
                NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT,
                "Outbound");

            Add(
                Directions.Incoming,
                NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN,
                "Inbound");

            return replacements;
        }

        private static void ValidateSnapshotCanBeCreated(
            FirewallRuleSnapshot snapshot)
        {
            Type? ruleType = Type.GetTypeFromProgID("HNetCfg.FWRule");
            if (ruleType == null)
            {
                throw new InvalidOperationException(
                    "The Windows Firewall rule COM type is unavailable.");
            }

            INetFwRule2? validationRule = null;

            try
            {
                validationRule =
                    (INetFwRule2)Activator.CreateInstance(ruleType)!;
                ApplySnapshotToComRule(validationRule, snapshot);

                FirewallRuleSnapshot actual =
                    CreateRuleSnapshot(validationRule);

                if (!AreRulesExactlyEqual(snapshot, actual))
                {
                    throw new InvalidOperationException(
                        $"Windows did not preserve all requested settings for " +
                        $"replacement rule '{snapshot.Name}'.");
                }
            }
            finally
            {
                if (validationRule != null)
                {
                    Marshal.ReleaseComObject(validationRule);
                }
            }
        }

        private static void CreateRuleFromSnapshot(
            FirewallRuleSnapshot snapshot)
        {
            Type? ruleType = Type.GetTypeFromProgID("HNetCfg.FWRule");
            if (ruleType == null)
            {
                throw new InvalidOperationException(
                    "The Windows Firewall rule COM type is unavailable.");
            }

            INetFwRule2? rule = null;
            bool passedToCreateRule = false;

            try
            {
                rule = (INetFwRule2)Activator.CreateInstance(ruleType)!;
                ApplySnapshotToComRule(rule, snapshot);
                passedToCreateRule = true;

                // CreateRule releases the COM rule in its finally block.
                CreateRule(rule);
                rule = null;
            }
            finally
            {
                if (!passedToCreateRule && rule != null)
                {
                    Marshal.ReleaseComObject(rule);
                }
            }
        }

        private static void ApplySnapshotToComRule(
            INetFwRule2 rule,
            FirewallRuleSnapshot snapshot)
        {
            rule.Name = snapshot.Name;
            rule.Description = snapshot.Description;
            rule.ApplicationName =
                string.IsNullOrWhiteSpace(snapshot.ApplicationName)
                    ? null
                    : snapshot.ApplicationName;
            rule.serviceName =
                string.IsNullOrWhiteSpace(snapshot.ServiceName)
                    ? null
                    : snapshot.ServiceName;
            rule.Direction = snapshot.Direction;
            rule.Action = snapshot.Action;
            rule.Enabled = snapshot.Enabled;
            rule.Profiles = snapshot.Profiles;
            rule.Grouping = snapshot.Grouping;
            rule.Protocol = snapshot.Protocol;
            rule.LocalAddresses =
                DisplayValue(snapshot.LocalAddresses);
            rule.RemoteAddresses =
                DisplayValue(snapshot.RemoteAddresses);
            rule.InterfaceTypes =
                string.IsNullOrWhiteSpace(snapshot.InterfaceTypes)
                    ? "All"
                    : snapshot.InterfaceTypes;
            rule.EdgeTraversal = snapshot.EdgeTraversal;

            if (snapshot.Protocol is 6 or 17)
            {
                rule.LocalPorts = DisplayValue(snapshot.LocalPorts);
                rule.RemotePorts = DisplayValue(snapshot.RemotePorts);
            }

            if (snapshot.Protocol is 1 or 58)
            {
                rule.IcmpTypesAndCodes =
                    DisplayValue(snapshot.IcmpTypesAndCodes);
            }
        }

        private static void VerifyReplacement(
            ApplicationReplacementPlan plan,
            IReadOnlyCollection<FirewallRuleSnapshot> expected)
        {
            ApplicationReplacementPlan actual =
                BuildApplicationReplacementPlan(
                    plan.AppPath,
                    plan.Directions,
                    plan.Action);

            if (actual.ExistingRules.Count != expected.Count)
            {
                throw new InvalidOperationException(
                    $"Replacement verification expected {expected.Count} rule(s), " +
                    $"but Windows Firewall returned {actual.ExistingRules.Count}.");
            }

            foreach (FirewallRuleSnapshot expectedRule in expected)
            {
                if (!actual.ExistingRules.Any(actualRule =>
                    AreRulesExactlyEqual(expectedRule, actualRule)))
                {
                    throw new InvalidOperationException(
                        $"Replacement rule '{expectedRule.Name}' could not be " +
                        "verified after creation.");
                }
            }
        }

        private static bool DirectionMatches(
            NET_FW_RULE_DIRECTION_ direction,
            Directions requested)
        {
            return
                (requested.HasFlag(Directions.Incoming) &&
                 direction == NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN) ||
                (requested.HasFlag(Directions.Outgoing) &&
                 direction == NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT);
        }

        private static IReadOnlyList<string> DistinctSummary(
            IEnumerable<string> values)
        {
            return values
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string DisplayValue(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? "*" : value.Trim();
        }

        private static string ProfilesToDisplayText(int profiles)
        {
            if (profiles == (int)NET_FW_PROFILE_TYPE2_
                    .NET_FW_PROFILE2_ALL ||
                profiles == 0)
            {
                return "All";
            }

            var names = new List<string>();

            if ((profiles & (int)NET_FW_PROFILE_TYPE2_
                    .NET_FW_PROFILE2_DOMAIN) != 0)
            {
                names.Add("Domain");
            }

            if ((profiles & (int)NET_FW_PROFILE_TYPE2_
                    .NET_FW_PROFILE2_PRIVATE) != 0)
            {
                names.Add("Private");
            }

            if ((profiles & (int)NET_FW_PROFILE_TYPE2_
                    .NET_FW_PROFILE2_PUBLIC) != 0)
            {
                names.Add("Public");
            }

            return names.Count == 0
                ? profiles.ToString()
                : string.Join(", ", names);
        }

        private static string ProtocolToDisplayText(int protocol)
        {
            return protocol switch
            {
                1 => "ICMPv4",
                6 => "TCP",
                17 => "UDP",
                58 => "ICMPv6",
                256 => "Any",
                _ => protocol.ToString()
            };
        }

        public static void SetDefaultOutboundAction(NET_FW_ACTION_ action)
        {
            INetFwPolicy2 firewallPolicy = GetLocalPolicy();
            if (firewallPolicy == null)
            {
                return;
            }

            foreach (NET_FW_PROFILE_TYPE2_ profile in new[]
            {
                NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_DOMAIN,
                NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PRIVATE,
                NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PUBLIC
            })
            {
                try
                {
                    firewallPolicy.set_DefaultOutboundAction(profile, action);
                }
                catch (COMException ex)
                {
                    Debug.WriteLine($"[ERROR] SetDefaultOutboundAction ({profile}): Failed. HResult: 0x{ex.HResult:X8}. Message: {ex.Message}");
                    if (ex.HResult == E_ACCESSDENIED)
                    {
                        Debug.WriteLine("[ERROR] SetDefaultOutboundAction: Access Denied. Ensure the application is running with administrator privileges.");
                    }
                }
            }
            if (firewallPolicy != null)
            {
                Marshal.ReleaseComObject(firewallPolicy);
            }
        }

        public static List<string> GetRuleNamesByPathAndDirection(string appPath, NET_FW_RULE_DIRECTION_ direction)
        {
            var rules = GetRulesByPathAndDirection(appPath, direction);
            var names = rules.Select(r => r.Name).ToList();
            foreach (var rule in rules)
            {
                Marshal.ReleaseComObject(rule);
            }
            return names;
        }

        public static List<INetFwRule2> GetRulesByPathAndDirection(string appPath, NET_FW_RULE_DIRECTION_ direction)
        {
            if (string.IsNullOrEmpty(appPath))
            {
                return [];
            }

            string normalizedAppPath = PathResolver.NormalizePath(appPath);
            var matchingRules = new List<INetFwRule2>();

            INetFwPolicy2 firewallPolicy = GetLocalPolicy();
            if (firewallPolicy?.Rules == null)
            {
                return matchingRules;
            }

            var comRules = firewallPolicy.Rules;
            try
            {
                foreach (INetFwRule2 rule in comRules)
                {
                    if (rule == null)
                    {
                        continue;
                    }

                    bool keep = false;
                    try
                    {
                        if (!string.IsNullOrEmpty(rule.ApplicationName) &&
                            string.Equals(PathResolver.NormalizePath(rule.ApplicationName), normalizedAppPath, StringComparison.OrdinalIgnoreCase) &&
                            rule.Direction == direction)
                        {
                            matchingRules.Add(rule);
                            keep = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[WARN] GetRulesByPathAndDirection: Failed to inspect firewall rule: {ex.Message}");
                    }
                    finally
                    {
                        if (!keep)
                        {
                            Marshal.ReleaseComObject(rule);
                        }
                    }
                }
            }
            finally
            {
                if (comRules != null)
                {
                    Marshal.ReleaseComObject(comRules);
                }

                if (firewallPolicy != null)
                {
                    Marshal.ReleaseComObject(firewallPolicy);
                }
            }
            return matchingRules;
        }

        public static NET_FW_ACTION_ GetDefaultOutboundAction()
        {
            INetFwPolicy2 firewallPolicy = GetLocalPolicy();
            if (firewallPolicy == null)
            {
                return NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
            }

            try
            {
                var currentProfileTypes = (NET_FW_PROFILE_TYPE2_)firewallPolicy.CurrentProfileTypes;
                if ((currentProfileTypes & NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PUBLIC) != 0)
                {
                    return firewallPolicy.DefaultOutboundAction[NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PUBLIC];
                }
                if ((currentProfileTypes & NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PRIVATE) != 0)
                {
                    return firewallPolicy.DefaultOutboundAction[NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PRIVATE];
                }
                if ((currentProfileTypes & NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_DOMAIN) != 0)
                {
                    return firewallPolicy.DefaultOutboundAction[NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_DOMAIN];
                }
                Debug.WriteLine("[WARN] GetDefaultOutboundAction: No specific profile type identified as active. Falling back to Public.");
                return firewallPolicy.DefaultOutboundAction[NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PUBLIC];
            }
            catch (COMException ex)
            {
                Debug.WriteLine($"[ERROR] GetDefaultOutboundAction: Failed. HResult: 0x{ex.HResult:X8}. Message: {ex.Message}");
                return NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
            }
            finally
            {
                if (firewallPolicy != null)
                {
                    Marshal.ReleaseComObject(firewallPolicy);
                }
            }
        }

        private static List<string> ExecuteDeleteAndReturnNames(Func<INetFwRule2, bool> predicate)
        {
            var rulesToRemove = GetRuleNamesAndRelease(predicate);
            DeleteRulesByName(rulesToRemove);
            return rulesToRemove;
        }

        public static List<string> DeleteRulesByPath(List<string> appPaths)
        {
            if (appPaths.Count == 0)
            {
                return [];
            }

            var pathSet = new HashSet<string>(appPaths.Select(PathResolver.NormalizePath), StringComparer.OrdinalIgnoreCase);

            return ExecuteDeleteAndReturnNames(rule =>
                !string.IsNullOrEmpty(rule.ApplicationName) &&
                pathSet.Contains(PathResolver.NormalizePath(rule.ApplicationName))
            );
        }

        public static List<string> DeleteRulesByServiceName(string serviceName)
        {
            if (string.IsNullOrEmpty(serviceName))
            {
                return [];
            }

            return ExecuteDeleteAndReturnNames(rule => string.Equals(rule.serviceName, serviceName, StringComparison.OrdinalIgnoreCase));
        }

        public static List<string> DeleteConflictingServiceRules(string serviceName, NET_FW_ACTION_ newAction, NET_FW_RULE_DIRECTION_ newDirection)
        {
            if (string.IsNullOrEmpty(serviceName))
            {
                return [];
            }

            NET_FW_ACTION_ conflictingAction = (newAction == NET_FW_ACTION_.NET_FW_ACTION_ALLOW)
                ? NET_FW_ACTION_.NET_FW_ACTION_BLOCK
                : NET_FW_ACTION_.NET_FW_ACTION_ALLOW;

            return ExecuteDeleteAndReturnNames(rule =>
                string.Equals(rule.serviceName, serviceName, StringComparison.OrdinalIgnoreCase) &&
                (rule.Direction == newDirection || rule.Direction == NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_MAX) &&
                rule.Action == conflictingAction
            );
        }

        public static List<string> DeleteUwpRules(List<string> packageFamilyNames)
        {
            if (packageFamilyNames.Count == 0)
            {
                return [];
            }

            var pfnSet = new HashSet<string>(packageFamilyNames, StringComparer.OrdinalIgnoreCase);

            return ExecuteDeleteAndReturnNames(rule =>
            {
                if (rule.Description?.StartsWith(MFWConstants.UwpDescriptionPrefix, StringComparison.Ordinal) == true)
                {
                    string pfnInRule = rule.Description[MFWConstants.UwpDescriptionPrefix.Length..];
                    return pfnSet.Contains(pfnInRule);
                }
                return false;
            });
        }

        public static void DeleteRulesByName(List<string> ruleNames)
        {
            if (ruleNames.Count == 0)
            {
                return;
            }

            INetFwPolicy2? firewallPolicy = null;
            INetFwRules? rulesCollection = null;
            var failures = new List<string>();

            try
            {
                firewallPolicy = GetLocalPolicy();
                rulesCollection = firewallPolicy.Rules;

                foreach (string name in ruleNames
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    try
                    {
                        rulesCollection.Remove(name);
                    }
                    catch (FileNotFoundException)
                    {
                        // Deletion is already satisfied.
                    }
                    catch (COMException ex)
                        when (ex.HResult ==
                              HRESULT_FROM_WIN32_ERROR_FILE_NOT_FOUND)
                    {
                        // Deletion is already satisfied.
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(
                            $"[ERROR] DeleteRulesByName ('{name}'): " +
                            $"{ex.GetType().Name}: {ex.Message}");

                        failures.Add($"'{name}': {ex.Message}");
                    }
                }
            }
            finally
            {
                if (rulesCollection != null)
                {
                    Marshal.ReleaseComObject(rulesCollection);
                }

                if (firewallPolicy != null)
                {
                    Marshal.ReleaseComObject(firewallPolicy);
                }
            }

            if (failures.Count > 0)
            {
                throw new InvalidOperationException(
                    "One or more firewall rules could not be deleted:\r\n" +
                    string.Join(
                        "\r\n",
                        failures.Take(5)) +
                    (failures.Count > 5
                        ? $"\r\n...and {failures.Count - 5} more."
                        : string.Empty));
            }
        }

        private static string NormalizeRuleText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim();
        }

        private static string NormalizeRulePath(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            string trimmed = value.Trim().Trim('"');

            if (trimmed == "*" || trimmed.StartsWith('@'))
            {
                return trimmed;
            }

            return PathResolver.NormalizePath(trimmed)
                .TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar);
        }

        private static string ReadOptionalString(
            Func<string?> reader,
            string fallback = "")
        {
            try
            {
                return NormalizeRuleText(reader());
            }
            catch (COMException)
            {
                return fallback;
            }
        }

        private static bool ReadOptionalBool(
            Func<bool> reader,
            bool fallback = false)
        {
            try
            {
                return reader();
            }
            catch (COMException)
            {
                return fallback;
            }
        }

        private static FirewallRuleSnapshot CreateRuleSnapshot(
            INetFwRule2 rule)
        {
            string applicationName;

            try
            {
                applicationName = NormalizeRulePath(
                    rule.ApplicationName);
            }
            catch (COMException)
            {
                applicationName = string.Empty;
            }

            return new FirewallRuleSnapshot
            {
                Name = ReadOptionalString(() => rule.Name),
                Description = ReadOptionalString(() => rule.Description),
                ApplicationName = applicationName,
                ServiceName = ReadOptionalString(() => rule.serviceName),
                Protocol = rule.Protocol,

                // Ports are unavailable for protocols that do not support ports.
                LocalPorts = ReadOptionalString(() => rule.LocalPorts),
                RemotePorts = ReadOptionalString(() => rule.RemotePorts),

                // ICMP settings are unavailable for non-ICMP protocols.
                IcmpTypesAndCodes =
                    ReadOptionalString(() => rule.IcmpTypesAndCodes),

                LocalAddresses =
                    ReadOptionalString(() => rule.LocalAddresses, "*"),

                RemoteAddresses =
                    ReadOptionalString(() => rule.RemoteAddresses, "*"),

                Direction = rule.Direction,
                Action = rule.Action,
                Enabled = rule.Enabled,
                Profiles = rule.Profiles,
                Grouping = ReadOptionalString(() => rule.Grouping),
                InterfaceTypes =
                    ReadOptionalString(() => rule.InterfaceTypes, "All"),

                EdgeTraversal =
                    ReadOptionalBool(() => rule.EdgeTraversal)
            };
        }

        private static bool AreRulesExactlyEqual(
            FirewallRuleSnapshot left,
            FirewallRuleSnapshot right)
        {
            static bool EqualText(string? first, string? second) =>
                string.Equals(
                    NormalizeRuleText(first),
                    NormalizeRuleText(second),
                    StringComparison.OrdinalIgnoreCase);

            return
                EqualText(left.Name, right.Name) &&
                EqualText(left.Description, right.Description) &&
                string.Equals(
                    NormalizeRulePath(left.ApplicationName),
                    NormalizeRulePath(right.ApplicationName),
                    StringComparison.OrdinalIgnoreCase) &&
                EqualText(left.ServiceName, right.ServiceName) &&
                left.Protocol == right.Protocol &&
                EqualText(left.LocalPorts, right.LocalPorts) &&
                EqualText(left.RemotePorts, right.RemotePorts) &&
                EqualText(
                    left.IcmpTypesAndCodes,
                    right.IcmpTypesAndCodes) &&
                EqualText(left.LocalAddresses, right.LocalAddresses) &&
                EqualText(left.RemoteAddresses, right.RemoteAddresses) &&
                left.Direction == right.Direction &&
                left.Action == right.Action &&
                left.Enabled == right.Enabled &&
                left.Profiles == right.Profiles &&
                EqualText(left.Grouping, right.Grouping) &&
                EqualText(left.InterfaceTypes, right.InterfaceTypes) &&
                left.EdgeTraversal == right.EdgeTraversal;
        }

        private static bool IsMinimalFirewallRule(
            FirewallRuleSnapshot rule)
        {
            return
                rule.Grouping.EndsWith(
                    MFWConstants.firewallRulesuffix,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    rule.Grouping,
                    MFWConstants.MainRuleGroup,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    rule.Grouping,
                    MFWConstants.WildcardRuleGroup,
                    StringComparison.OrdinalIgnoreCase);
        }

        public static bool RuleAlreadyExists(INetFwRule2 candidate)
        {
            FirewallRuleSnapshot candidateSnapshot =
                CreateRuleSnapshot(candidate);

            INetFwPolicy2? firewallPolicy = null;
            INetFwRules? rulesCollection = null;

            try
            {
                firewallPolicy = GetLocalPolicy();
                rulesCollection = firewallPolicy.Rules;

                foreach (INetFwRule2 existingRule in rulesCollection)
                {
                    try
                    {
                        FirewallRuleSnapshot existingSnapshot =
                            CreateRuleSnapshot(existingRule);

                        if (AreRulesExactlyEqual(
                                existingSnapshot,
                                candidateSnapshot))
                        {
                            return true;
                        }
                    }
                    catch (COMException ex)
                    {
                        Debug.WriteLine(
                            $"[WARN] RuleAlreadyExists: Could not inspect " +
                            $"a firewall rule. HResult: 0x{ex.HResult:X8}. " +
                            $"Message: {ex.Message}");
                    }
                    finally
                    {
                        if (existingRule != null)
                        {
                            Marshal.ReleaseComObject(existingRule);
                        }
                    }
                }

                return false;
            }
            finally
            {
                if (rulesCollection != null)
                {
                    Marshal.ReleaseComObject(rulesCollection);
                }

                if (firewallPolicy != null)
                {
                    Marshal.ReleaseComObject(firewallPolicy);
                }
            }
        }

        public static void CreateRule(INetFwRule2 rule)
        {
            INetFwPolicy2? firewallPolicy = null;
            INetFwRules? rulesCollection = null;

            try
            {
                if (RuleAlreadyExists(rule))
                {
                    Debug.WriteLine(
                        $"[INFO] CreateRule: Exact duplicate skipped: " +
                        $"'{rule.Name}'.");

                    return;
                }

                firewallPolicy = GetLocalPolicy();

                if (firewallPolicy.Rules == null)
                {
                    Debug.WriteLine(
                        "[ERROR] CreateRule: Firewall rules collection is null.");

                    return;
                }

                rulesCollection = firewallPolicy.Rules;

                try
                {
                    Debug.WriteLine(
                        $"[FirewallRuleService] Committing Rule: {rule.Name}");

                    Debug.WriteLine(
                        $"[FirewallRuleService] - App: " +
                        $"{rule.ApplicationName}");

                    Debug.WriteLine(
                        $"[FirewallRuleService] - Interfaces: " +
                        $"{rule.InterfaceTypes}");

                    Debug.WriteLine(
                        $"[FirewallRuleService] - Protocol: {rule.Protocol}");
                }
                catch (COMException)
                {
                    Debug.WriteLine(
                        "[FirewallRuleService] Could not read rule " +
                        "properties for logging.");
                }

                rulesCollection.Add(rule);
            }
            catch (COMException ex)
            {
                if (ex.HResult ==
                    HRESULT_FROM_WIN32_ERROR_ALREADY_EXISTS)
                {
                    Debug.WriteLine(
                        $"[WARN] CreateRule: Rule '{rule?.Name}' " +
                        "already exists. Skipping.");
                }
                else
                {
                    Debug.WriteLine(
                        $"[ERROR] CreateRule " +
                        $"('{rule?.Name ?? "null"}'): Failed. " +
                        $"HResult: 0x{ex.HResult:X8}. " +
                        $"Message: {ex.Message}");

                    throw;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[ERROR] CreateRule " +
                    $"('{rule?.Name ?? "null"}'): Unexpected error. " +
                    $"Type: {ex.GetType().Name}. " +
                    $"Message: {ex.Message}");

                throw;
            }
            finally
            {
                if (rule != null)
                {
                    Marshal.ReleaseComObject(rule);
                }

                if (rulesCollection != null)
                {
                    Marshal.ReleaseComObject(rulesCollection);
                }

                if (firewallPolicy != null)
                {
                    Marshal.ReleaseComObject(firewallPolicy);
                }
            }
        }

        public static int DeleteExactDuplicatefirewallRules()
        {
            INetFwPolicy2? firewallPolicy = null;
            INetFwRules? rulesCollection = null;

            try
            {
                firewallPolicy = GetLocalPolicy();
                rulesCollection = firewallPolicy.Rules;

                var snapshots =
                    new List<FirewallRuleSnapshot>();

                foreach (INetFwRule2 rule in rulesCollection)
                {
                    try
                    {
                        FirewallRuleSnapshot snapshot =
                            CreateRuleSnapshot(rule);

                        if (IsMinimalFirewallRule(snapshot))
                        {
                            snapshots.Add(snapshot);
                        }
                    }
                    catch (COMException ex)
                    {
                        Debug.WriteLine(
                            $"[WARN] Duplicate cleanup could not inspect " +
                            $"a rule. HResult: 0x{ex.HResult:X8}. " +
                            $"Message: {ex.Message}");
                    }
                    finally
                    {
                        if (rule != null)
                        {
                            Marshal.ReleaseComObject(rule);
                        }
                    }
                }

                var duplicateGroups =
                    new List<List<FirewallRuleSnapshot>>();

                foreach (FirewallRuleSnapshot snapshot in snapshots)
                {
                    List<FirewallRuleSnapshot>? matchingGroup =
                        duplicateGroups.FirstOrDefault(group =>
                            AreRulesExactlyEqual(group[0], snapshot));

                    if (matchingGroup == null)
                    {
                        duplicateGroups.Add([snapshot]);
                    }
                    else
                    {
                        matchingGroup.Add(snapshot);
                    }
                }

                int removedCount = 0;

                foreach (List<FirewallRuleSnapshot> group in
                         duplicateGroups.Where(g => g.Count > 1))
                {
                    // Keep the first copy and remove only the extras.
                    for (int index = 1; index < group.Count; index++)
                    {
                        string duplicateName = group[index].Name;

                        if (string.IsNullOrWhiteSpace(duplicateName))
                        {
                            continue;
                        }

                        try
                        {
                            rulesCollection.Remove(duplicateName);
                            removedCount++;

                            Debug.WriteLine(
                                $"[INFO] Removed exact duplicate rule: " +
                                $"'{duplicateName}'.");
                        }
                        catch (FileNotFoundException)
                        {
                            Debug.WriteLine(
                                $"[WARN] Duplicate rule '{duplicateName}' " +
                                "was already removed.");
                        }
                        catch (COMException ex)
                        {
                            Debug.WriteLine(
                                $"[ERROR] Failed to remove duplicate rule " +
                                $"'{duplicateName}'. " +
                                $"HResult: 0x{ex.HResult:X8}. " +
                                $"Message: {ex.Message}");
                        }
                    }
                }

                return removedCount;
            }
            finally
            {
                if (rulesCollection != null)
                {
                    Marshal.ReleaseComObject(rulesCollection);
                }

                if (firewallPolicy != null)
                {
                    Marshal.ReleaseComObject(firewallPolicy);
                }
            }
        }

        public static void UpdateRuleRemoteAddresses(string ruleName, string newRemoteAddresses)
        {
            if (string.IsNullOrEmpty(ruleName) || string.IsNullOrEmpty(newRemoteAddresses)) return;

            INetFwPolicy2 firewallPolicy = GetLocalPolicy();
            if (firewallPolicy?.Rules == null) return;

            INetFwRules? rulesCollection = null;
            try
            {
                rulesCollection = firewallPolicy.Rules;
                if (rulesCollection.Item(ruleName) is INetFwRule2 rule)
                {
                    rule.RemoteAddresses = newRemoteAddresses;
                    Marshal.ReleaseComObject(rule);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] UpdateRuleRemoteAddresses ('{ruleName}'): Failed. {ex.Message}");
            }
            finally
            {
                if (rulesCollection != null) Marshal.ReleaseComObject(rulesCollection);
                if (firewallPolicy != null) Marshal.ReleaseComObject(firewallPolicy);
            }
        }

        public static List<string> DeleteRulesByDescription(string description)
        {
            if (string.IsNullOrEmpty(description))
            {
                return [];
            }

            return ExecuteDeleteAndReturnNames(rule => string.Equals(rule.Description, description, StringComparison.OrdinalIgnoreCase));
        }

        public static List<string> DeleteRulesByGroup(string groupName)
        {
            if (string.IsNullOrEmpty(groupName))
            {
                return [];
            }

            return ExecuteDeleteAndReturnNames(rule => string.Equals(rule.Grouping, groupName, StringComparison.OrdinalIgnoreCase));
        }

        public static void DeleteallFirewallRules()
        {
            var rulesToRemove = GetRuleNamesAndRelease(rule =>
                !string.IsNullOrEmpty(rule.Grouping) &&
                (rule.Grouping.EndsWith(MFWConstants.firewallRulesuffix) ||
                 rule.Grouping == MFWConstants.MainRuleGroup ||
                 rule.Grouping == MFWConstants.WildcardRuleGroup)
            );

            Debug.WriteLine($"[INFO] DeleteallFirewallRules: Identified {rulesToRemove.Count} MFW rules for deletion.");

            if (rulesToRemove.Count > 0)
            {
                DeleteRulesByName(rulesToRemove);
                Debug.WriteLine($"[INFO] DeleteallFirewallRules: Requested deletion of {rulesToRemove.Count} MFW rules.");
            }
        }

        private static void SetRuleEnabledState(string ruleName, bool isEnabled, string callerName)
        {
            if (string.IsNullOrEmpty(ruleName))
            {
                return;
            }

            INetFwPolicy2 firewallPolicy = GetLocalPolicy();
            if (firewallPolicy == null)
            {
                return;
            }

            INetFwRules? rulesCollection = null;
            try
            {
                rulesCollection = firewallPolicy.Rules;
                if (rulesCollection == null)
                {
                    return;
                }

                if (rulesCollection.Item(ruleName) is INetFwRule2 rule)
                {
                    rule.Enabled = isEnabled;
                    Marshal.ReleaseComObject(rule);
                }
            }
            catch (FileNotFoundException)
            {
                Debug.WriteLine($"[WARN] {callerName}: Rule '{ruleName}' not found.");
            }
            catch (COMException ex)
            {
                Debug.WriteLine($"[ERROR] {callerName} ('{ruleName}'): Failed. HResult: 0x{ex.HResult:X8}. Message: {ex.Message}");
            }
            finally
            {
                if (rulesCollection != null)
                {
                    Marshal.ReleaseComObject(rulesCollection);
                }

                if (firewallPolicy != null)
                {
                    Marshal.ReleaseComObject(firewallPolicy);
                }
            }
        }

        public void DisableRuleByName(string ruleName) => SetRuleEnabledState(ruleName, false, nameof(DisableRuleByName));

        public void EnableRuleByName(string ruleName) => SetRuleEnabledState(ruleName, true, nameof(EnableRuleByName));
    }
}
