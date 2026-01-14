using NetFwTypeLib;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MinimalFirewall
{
    public class FirewallRuleService
    {
        private const int E_ACCESSDENIED = unchecked((int)0x80070005);
        private const int HRESULT_FROM_WIN32_ERROR_FILE_NOT_FOUND = unchecked((int)0x80070002);
        private const int HRESULT_FROM_WIN32_ERROR_ALREADY_EXISTS = unchecked((int)0x800700B7);

        // Optimization: Cache the COM Type lookup to avoid repeated reflection calls
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

        /// <summary>
        /// Helper to iterate rules, extract names based on a condition, and immediately release COM objects.
        /// Reduces memory overhead and code duplication for Delete operations.
        /// </summary>
        private List<string> GetRuleNamesAndRelease(Func<INetFwRule2, bool> predicate)
        {
            var matchedNames = new List<string>();
            var allRules = GetAllRules();

            foreach (var rule in allRules)
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
                    // Always release the COM object immediately after inspection
                    if (rule != null) Marshal.ReleaseComObject(rule);
                }
            }
            return matchedNames;
        }

        public List<INetFwRule2> GetAllRules()
        {
            INetFwPolicy2 firewallPolicy = GetLocalPolicy();
            if (firewallPolicy?.Rules == null) return [];

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
                // If retrieval fails halfway, release what we collected
                foreach (var rule in rulesList)
                {
                    Marshal.ReleaseComObject(rule);
                }
                return [];
            }
            finally
            {
                if (comRules != null) Marshal.ReleaseComObject(comRules);
                if (firewallPolicy != null) Marshal.ReleaseComObject(firewallPolicy);
            }
        }

        public INetFwRule2? GetRuleByName(string name)
        {
            INetFwPolicy2 firewallPolicy = GetLocalPolicy();
            if (firewallPolicy == null) return null;
            try
            {
                if (firewallPolicy.Rules.Item(name) is INetFwRule2 rule)
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
                if (firewallPolicy != null) Marshal.ReleaseComObject(firewallPolicy);
            }
        }

        public void SetDefaultOutboundAction(NET_FW_ACTION_ action)
        {
            INetFwPolicy2 firewallPolicy = GetLocalPolicy();
            if (firewallPolicy == null) return;

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
            if (firewallPolicy != null) Marshal.ReleaseComObject(firewallPolicy);
        }

        public List<string> GetRuleNamesByPathAndDirection(string appPath, NET_FW_RULE_DIRECTION_ direction)
        {
            var rules = GetRulesByPathAndDirection(appPath, direction);
            var names = rules.Select(r => r.Name).ToList();
            foreach (var rule in rules)
            {
                Marshal.ReleaseComObject(rule);
            }
            return names;
        }

        public List<INetFwRule2> GetRulesByPathAndDirection(string appPath, NET_FW_RULE_DIRECTION_ direction)
        {
            if (string.IsNullOrEmpty(appPath)) return [];
            string normalizedAppPath = PathResolver.NormalizePath(appPath);
            var matchingRules = new List<INetFwRule2>();
            var allRules = GetAllRules();

            // Rules that we inspect but don't return need to be released
            var rulesToRelease = new List<INetFwRule2>();

            try
            {
                foreach (var rule in allRules)
                {
                    if (rule != null &&
                        !string.IsNullOrEmpty(rule.ApplicationName) &&
                         string.Equals(PathResolver.NormalizePath(rule.ApplicationName), normalizedAppPath, StringComparison.OrdinalIgnoreCase) &&
                        rule.Direction == direction)
                    {
                        matchingRules.Add(rule);
                    }
                    else if (rule != null)
                    {
                        rulesToRelease.Add(rule);
                    }
                }
            }
            finally
            {
                foreach (var rule in rulesToRelease)
                {
                    Marshal.ReleaseComObject(rule);
                }
            }
            return matchingRules;
        }

        public NET_FW_ACTION_ GetDefaultOutboundAction()
        {
            INetFwPolicy2 firewallPolicy = GetLocalPolicy();
            if (firewallPolicy == null) return NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
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
                if (firewallPolicy != null) Marshal.ReleaseComObject(firewallPolicy);
            }
        }

        public List<string> DeleteRulesByPath(List<string> appPaths)
        {
            if (appPaths.Count == 0) return [];

            var pathSet = new HashSet<string>(appPaths.Select(PathResolver.NormalizePath), StringComparer.OrdinalIgnoreCase);

            var rulesToRemove = GetRuleNamesAndRelease(rule =>
                !string.IsNullOrEmpty(rule.ApplicationName) &&
                pathSet.Contains(PathResolver.NormalizePath(rule.ApplicationName))
            );

            DeleteRulesByName(rulesToRemove);
            return rulesToRemove;
        }

        public List<string> DeleteRulesByServiceName(string serviceName)
        {
            if (string.IsNullOrEmpty(serviceName)) return [];

            var rulesToRemove = GetRuleNamesAndRelease(rule =>
                string.Equals(rule.serviceName, serviceName, StringComparison.OrdinalIgnoreCase)
            );

            DeleteRulesByName(rulesToRemove);
            return rulesToRemove;
        }

        public List<string> DeleteConflictingServiceRules(string serviceName, NET_FW_ACTION_ newAction, NET_FW_RULE_DIRECTION_ newDirection)
        {
            if (string.IsNullOrEmpty(serviceName)) return [];

            NET_FW_ACTION_ conflictingAction = (newAction == NET_FW_ACTION_.NET_FW_ACTION_ALLOW)
                ? NET_FW_ACTION_.NET_FW_ACTION_BLOCK
                : NET_FW_ACTION_.NET_FW_ACTION_ALLOW;

            var rulesToRemove = GetRuleNamesAndRelease(rule =>
                string.Equals(rule.serviceName, serviceName, StringComparison.OrdinalIgnoreCase) &&
                (rule.Direction == newDirection || rule.Direction == NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_MAX) &&
                rule.Action == conflictingAction
            );

            DeleteRulesByName(rulesToRemove);
            return rulesToRemove;
        }

        public List<string> DeleteUwpRules(List<string> packageFamilyNames)
        {
            if (packageFamilyNames.Count == 0) return [];

            var pfnSet = new HashSet<string>(packageFamilyNames, StringComparer.OrdinalIgnoreCase);

            var rulesToRemove = GetRuleNamesAndRelease(rule =>
            {
                if (rule.Description?.StartsWith(MFWConstants.UwpDescriptionPrefix, StringComparison.Ordinal) == true)
                {
                    string pfnInRule = rule.Description[MFWConstants.UwpDescriptionPrefix.Length..];
                    return pfnSet.Contains(pfnInRule);
                }
                return false;
            });

            DeleteRulesByName(rulesToRemove);
            return rulesToRemove;
        }

        public void DeleteRulesByName(List<string> ruleNames)
        {
            if (ruleNames.Count == 0) return;
            INetFwPolicy2 firewallPolicy = GetLocalPolicy();
            if (firewallPolicy?.Rules == null) return;

            var rulesCollection = firewallPolicy.Rules;
            try
            {
                foreach (var name in ruleNames)
                {
                    try
                    {
                        rulesCollection.Remove(name);
                    }
                    catch (FileNotFoundException)
                    {
                        Debug.WriteLine($"[WARN] DeleteRulesByName: Rule '{name}' not found for removal.");
                    }
                    catch (COMException ex)
                    {
                        Debug.WriteLine($"[ERROR] DeleteRulesByName ('{name}'): Failed. HResult: 0x{ex.HResult:X8}. Message: {ex.Message}");
                        if (ex.HResult == E_ACCESSDENIED)
                        {
                            Debug.WriteLine($"[ERROR] DeleteRulesByName ('{name}'): Access Denied.");
                        }
                        else if (ex.HResult == HRESULT_FROM_WIN32_ERROR_FILE_NOT_FOUND)
                        {
                            Debug.WriteLine($"[WARN] DeleteRulesByName: Rule '{name}' not found (reported via COMException HResult).");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[ERROR] DeleteRulesByName ('{name}'): Unexpected error during removal. Type: {ex.GetType().Name}. Message: {ex.Message}");
                    }
                }
            }
            finally
            {
                if (rulesCollection != null) Marshal.ReleaseComObject(rulesCollection);
                if (firewallPolicy != null) Marshal.ReleaseComObject(firewallPolicy);
            }
        }

        public void CreateRule(INetFwRule2 rule)
        {
            INetFwPolicy2 firewallPolicy = GetLocalPolicy();
            INetFwRules? rulesCollection = null;
            try
            {
                if (firewallPolicy?.Rules == null)
                {
                    Debug.WriteLine("[ERROR] CreateRule: Firewall policy or rules collection is null.");
                    return;
                }
                rulesCollection = firewallPolicy.Rules;

                try
                {
                    Debug.WriteLine($"[FirewallRuleService] Committing Rule: {rule.Name}");
                    Debug.WriteLine($"[FirewallRuleService] - App: {rule.ApplicationName}");
                    Debug.WriteLine($"[FirewallRuleService] - Interfaces: {rule.InterfaceTypes}");
                    Debug.WriteLine($"[FirewallRuleService] - Protocol: {rule.Protocol}");
                }
                catch
                {
                    Debug.WriteLine("[FirewallRuleService] Could not read rule properties for logging.");
                }

                rulesCollection.Add(rule);
            }
            catch (COMException ex)
            {
                if (ex.HResult == HRESULT_FROM_WIN32_ERROR_ALREADY_EXISTS)
                {
                    Debug.WriteLine($"[WARN] CreateRule: Rule '{rule?.Name}' already exists. Skipping.");
                }
                else
                {
                    Debug.WriteLine($"[ERROR] CreateRule ('{rule?.Name ?? "null"}'): Failed. HResult: 0x{ex.HResult:X8}. Message: {ex.Message}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] CreateRule ('{rule?.Name ?? "null"}'): Unexpected error. Type: {ex.GetType().Name}. Message: {ex.Message}");
                throw;
            }
            finally
            {
                if (rule != null) Marshal.ReleaseComObject(rule);
                if (rulesCollection != null) Marshal.ReleaseComObject(rulesCollection);
                if (firewallPolicy != null) Marshal.ReleaseComObject(firewallPolicy);
            }
        }

        public List<string> DeleteRulesByDescription(string description)
        {
            if (string.IsNullOrEmpty(description)) return [];

            var rulesToRemove = GetRuleNamesAndRelease(rule =>
                string.Equals(rule.Description, description, StringComparison.OrdinalIgnoreCase)
            );

            DeleteRulesByName(rulesToRemove);
            return rulesToRemove;
        }

        public List<string> DeleteRulesByGroup(string groupName)
        {
            if (string.IsNullOrEmpty(groupName)) return [];

            var rulesToRemove = GetRuleNamesAndRelease(rule =>
                string.Equals(rule.Grouping, groupName, StringComparison.OrdinalIgnoreCase)
            );

            DeleteRulesByName(rulesToRemove);
            return rulesToRemove;
        }

        public void DeleteAllMfwRules()
        {
            var rulesToRemove = GetRuleNamesAndRelease(rule =>
                !string.IsNullOrEmpty(rule.Grouping) &&
                (rule.Grouping.EndsWith(MFWConstants.MfwRuleSuffix) ||
                 rule.Grouping == MFWConstants.MainRuleGroup ||
                 rule.Grouping == MFWConstants.WildcardRuleGroup)
            );

            Debug.WriteLine($"[INFO] DeleteAllMfwRules: Identified {rulesToRemove.Count} MFW rules for deletion.");

            if (rulesToRemove.Count > 0)
            {
                DeleteRulesByName(rulesToRemove);
                Debug.WriteLine($"[INFO] DeleteAllMfwRules: Requested deletion of {rulesToRemove.Count} MFW rules.");
            }
        }

        public void DisableRuleByName(string ruleName)
        {
            if (string.IsNullOrEmpty(ruleName)) return;
            INetFwPolicy2 firewallPolicy = GetLocalPolicy();
            if (firewallPolicy?.Rules == null) return;

            try
            {
                if (firewallPolicy.Rules.Item(ruleName) is INetFwRule2 rule)
                {
                    rule.Enabled = false;
                    Marshal.ReleaseComObject(rule);
                }
            }
            catch (FileNotFoundException)
            {
                Debug.WriteLine($"[WARN] DisableRuleByName: Rule '{ruleName}' not found.");
            }
            catch (COMException ex)
            {
                Debug.WriteLine($"[ERROR] DisableRuleByName ('{ruleName}'): Failed. HResult: 0x{ex.HResult:X8}. Message: {ex.Message}");
            }
            finally
            {
                if (firewallPolicy != null) Marshal.ReleaseComObject(firewallPolicy);
            }
        }

        public void EnableRuleByName(string ruleName)
        {
            if (string.IsNullOrEmpty(ruleName)) return;
            INetFwPolicy2 firewallPolicy = GetLocalPolicy();
            if (firewallPolicy?.Rules == null) return;

            try
            {
                if (firewallPolicy.Rules.Item(ruleName) is INetFwRule2 rule)
                {
                    rule.Enabled = true;
                    Marshal.ReleaseComObject(rule);
                }
            }
            catch (FileNotFoundException)
            {
                Debug.WriteLine($"[WARN] EnableRuleByName: Rule '{ruleName}' not found.");
            }
            catch (COMException ex)
            {
                Debug.WriteLine($"[ERROR] EnableRuleByName ('{ruleName}'): Failed. HResult: 0x{ex.HResult:X8}. Message: {ex.Message}");
            }
            finally
            {
                if (firewallPolicy != null) Marshal.ReleaseComObject(firewallPolicy);
            }
        }
    }
}