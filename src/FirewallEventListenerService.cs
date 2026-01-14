using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Xml;
using System.Collections.Concurrent;
using MinimalFirewall.TypedObjects;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms; 
using System;

namespace MinimalFirewall
{
    public partial class FirewallEventListenerService : IDisposable
    {
        private const int BlockedConnectionEventId = 5157;
        private const string SecurityLogName = "Security";
        private const string WfpAuditSubcategoryGuid = "{0CCE9226-69AE-11D9-BED3-505054503030}";

        private const string DirectionInboundCode = "%%14592";
        private const string DirectionOutboundCode = "%%14593";
        private const string DirectionInbound = "Incoming";
        private const string DirectionOutbound = "Outgoing";
        private readonly FirewallDataService _dataService;
        private readonly WildcardRuleService _wildcardRuleService;
        private readonly Func<bool> _isLockdownEnabled;
        private readonly AppSettings _appSettings;
        private readonly PublisherWhitelistService _whitelistService;
        private readonly BackgroundFirewallTaskService _backgroundTaskService;
        private readonly Action<string> _logAction;

        private readonly ConcurrentDictionary<string, DateTime> _snoozedApps = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, bool> _pendingNotifications = new(StringComparer.OrdinalIgnoreCase);

        private EventLogWatcher? _eventWatcher;
        private readonly string _currentAssemblyName;

        public FirewallActionsService? ActionsService { get; set; }
        public event Action<PendingConnectionViewModel>? PendingConnectionDetected;
        public FirewallEventListenerService(
            FirewallDataService dataService,
            WildcardRuleService wildcardRuleService,
            Func<bool> isLockdownEnabled,
            Action<string> logAction,
            AppSettings appSettings,
            PublisherWhitelistService whitelistService,
            BackgroundFirewallTaskService backgroundTaskService)
        {
            _dataService = dataService;
            _wildcardRuleService = wildcardRuleService;
            _isLockdownEnabled = isLockdownEnabled;
            _logAction = logAction;
            _appSettings = appSettings;
            _whitelistService = whitelistService;
            _backgroundTaskService = backgroundTaskService;

            _currentAssemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name ?? "MinimalFirewall";
        }

        public void Start()
        {
            if (_eventWatcher != null)
            {
                if (!_eventWatcher.Enabled)
                {
                    _eventWatcher.Enabled = true;
                    _logAction("[EventListener] Event watcher re-enabled.");
                }
                return;
            }


            try
            {
                var query = new EventLogQuery(SecurityLogName, PathType.LogName, $"*[System[(EventID={BlockedConnectionEventId})]]");
                _eventWatcher = new EventLogWatcher(query);
                _eventWatcher.EventRecordWritten += OnEventRecordWritten;
                _eventWatcher.Enabled = true;
                _logAction($"[EventListener] Event watcher started successfully (Listening for {BlockedConnectionEventId}).");
            }
            catch (EventLogException ex)
            {
                _logAction($"[EventListener ERROR] Permission denied reading Security log: {ex.Message}");
                MessageBox.Show("Could not start firewall event listener. Please run as Administrator.", "Permission Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Stop()
        {
            if (_eventWatcher != null)
            {
                _eventWatcher.Enabled = false;
                _eventWatcher.EventRecordWritten -= OnEventRecordWritten;
                _eventWatcher.Dispose();
                _eventWatcher = null;
                _logAction("[EventListener] Event watcher stopped and disposed.");
            }

            DisableWfpAuditing();
        }


        public void EnableAuditing()
        {
            EnsureWfpAuditingEnabled();
        }

        public void DisableAuditing()
        {
            DisableWfpAuditing();
        }


        private void OnEventRecordWritten(object? sender, EventRecordWrittenEventArgs e)
        {
            if (e.EventRecord == null) return;
            try
            {
                string xmlContent = e.EventRecord.ToXml();
                Task.Run(async () => await ProcessFirewallBlockEventAsync(xmlContent));
            }
            catch (EventLogException) { /* Ignore log read errors */ }
        }

        private async Task ProcessFirewallBlockEventAsync(string xmlContent)
        {
            string appPath = string.Empty;
            string direction = string.Empty;

            try
            {
                // explicit extraction to guarantee no fields are skipped
                string rawAppPath = GetValueFromXml(xmlContent, "Application");
                direction = ParseDirection(GetValueFromXml(xmlContent, "Direction"));
                string remoteAddress = GetValueFromXml(xmlContent, "RemoteAddress");
                string remotePort = GetValueFromXml(xmlContent, "RemotePort");
                string protocol = GetValueFromXml(xmlContent, "Protocol");
                string filterId = GetValueFromXml(xmlContent, "FilterId");
                string layerId = GetValueFromXml(xmlContent, "LayerId");
                string xmlServiceName = GetValueFromXml(xmlContent, "ServiceName");
                string pidStr = GetValueFromXml(xmlContent, "ProcessID");
                string serviceName = (xmlServiceName == "N/A" || string.IsNullOrEmpty(xmlServiceName)) ? string.Empty : xmlServiceName;

                if (IsNetworkNoise(remoteAddress)) return;
                if (!string.IsNullOrEmpty(rawAppPath) && rawAppPath.Contains(_currentAssemblyName, StringComparison.OrdinalIgnoreCase)) return;

                appPath = ResolveAppPath(rawAppPath);

                if (!IsValidAppPath(appPath)) return;

                string notificationKey = $"{appPath}|{direction}";
                if (!_pendingNotifications.TryAdd(notificationKey, true)) return;
                // Logic Check (Snoozed or Lockdown)
                if (!ShouldProcessEvent(appPath))
                {
                    ClearPendingNotification(appPath, direction);
                    return;
                }

                serviceName = ResolveServiceName(appPath, serviceName, pidStr);
                // Ignore Noisy Services
                if (IsNoisyService(serviceName))
                {
                    ClearPendingNotification(appPath, direction);
                    return;
                }

                MfwRuleStatus existingRuleStatus = await _dataService.CheckMfwRuleStatusAsync(appPath, serviceName, direction);
                if (existingRuleStatus == MfwRuleStatus.MfwBlock)
                {
                    ClearPendingNotification(appPath, direction);
                    return;
                }

                if (existingRuleStatus == MfwRuleStatus.MfwAllow)
                {
                    if (filterId == "0")
                    {

                        _dataService.InvalidateRuleCache();
                        SnoozeNotificationsForApp(appPath, TimeSpan.FromSeconds(10));
                    }
                    ClearPendingNotification(appPath, direction);
                    return;
                }

                if (CheckWildcardMatch(appPath, serviceName))
                {
                    ClearPendingNotification(appPath, direction);
                    return;
                }

                if (await CheckAutoAllowTrustedAsync(appPath, direction))
                {
                    ClearPendingNotification(appPath, direction);
                    return;
                }

                //  Notify UI
                var pendingVm = new PendingConnectionViewModel
                {
                    AppPath = appPath,
                    Direction =
                    direction,
                    ServiceName = serviceName,
                    Protocol = protocol,
                    RemotePort = remotePort,
                    RemoteAddress = remoteAddress,

                    FilterId = filterId,
                    LayerId = layerId
                };
                PendingConnectionDetected?.Invoke(pendingVm);
            }
            catch (Exception ex)
            {
                _logAction($"[FATAL ERROR IN EVENT HANDLER] {ex.Message}");
                if (!string.IsNullOrEmpty(appPath) && !string.IsNullOrEmpty(direction))
                {
                    ClearPendingNotification(appPath, direction);
                }
            }
        }


        private string ResolveAppPath(string rawPath)
        {
            string finalPath = rawPath;
            try
            {
                string converted = PathResolver.ConvertDevicePathToDrivePath(rawPath);
                if (!string.IsNullOrEmpty(converted)) finalPath = converted;
                finalPath = PathResolver.NormalizePath(finalPath);
            }
            catch { /* Keep original on failure */ }
            return finalPath;
        }

        private bool IsValidAppPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            if (path.Equals("System", StringComparison.OrdinalIgnoreCase)) return false;
            if (path.Equals("Unsolicited Traffic (No Process)", StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }

        private string ResolveServiceName(string appPath, string currentService, string pid)
        {
            if (!string.IsNullOrEmpty(currentService)) return currentService;
            if (Path.GetFileName(appPath).Equals("svchost.exe", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrEmpty(pid) &&
                pid != "0")
            {
                string resolved = SystemDiscoveryService.GetServicesByPID(pid);
                _logAction($"[EventListener] svchost.exe detected. PID: {pid}, Resolved: '{resolved}'");
                return resolved;
            }
            return string.Empty;
        }

        private bool IsNoisyService(string serviceName)
        {
            if (string.IsNullOrEmpty(serviceName)) return false;
            return serviceName.Equals("Dhcp", StringComparison.OrdinalIgnoreCase) ||
                   serviceName.Equals("Dnscache", StringComparison.OrdinalIgnoreCase) ||
                   serviceName.Equals("Ssdpsrv", StringComparison.OrdinalIgnoreCase);
        }

        private bool CheckWildcardMatch(string appPath, string serviceName)
        {
            var matchingRule = _wildcardRuleService.Match(appPath);
            if (matchingRule != null)
            {
                if (matchingRule.Action.StartsWith("Allow", StringComparison.OrdinalIgnoreCase) && ActionsService != null)
                {
                    ActionsService.ApplyWildcardMatch(appPath, serviceName, matchingRule);
                }
                return true;
            }
            return false;
        }

        private async Task<bool> CheckAutoAllowTrustedAsync(string appPath, string direction)
        {
            if (!_appSettings.AutoAllowSystemTrusted) return false;
            return await Task.Run(() =>
            {
                if (File.Exists(appPath) &&
                    SignatureValidationService.IsSignatureTrusted(appPath, out var trustedPublisherName) &&
                    !string.IsNullOrEmpty(trustedPublisherName))
                {

                    if (_backgroundTaskService != null && !string.IsNullOrEmpty(appPath))
                    {
                        string allowAction = $"Allow ({direction})";
                        var appPayload = new ApplyApplicationRulePayload
                        {
                            AppPaths = new List<string>
                        { appPath },
                            Action = allowAction
                        };
                        _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.ApplyApplicationRule, appPayload));
                    }
                    return true;
                }

                return false;
            });
        }

        private bool IsNetworkNoise(string remoteIp)
        {
            if (string.IsNullOrEmpty(remoteIp)) return false;
            if (remoteIp == "255.255.255.255") return true;
            if (remoteIp.StartsWith("224.") || remoteIp.StartsWith("239.")) return true;
            if (remoteIp.StartsWith("ff", StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        private void EnsureWfpAuditingEnabled()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName =
                    "auditpol.exe",
                    Arguments = $"/set /subcategory:\"{WfpAuditSubcategoryGuid}\" /success:disable /failure:enable",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden

                };
                using var p = Process.Start(psi);
                p?.WaitForExit(1000);
            }
            catch (Exception ex)
            {
                _logAction($"[EventListener] Failed to set audit policy: {ex.Message}");
            }
        }

        private void DisableWfpAuditing()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "auditpol.exe",
                    // Disable both success and failure to "clean up" the policy
                    Arguments = $"/set /subcategory:\"{WfpAuditSubcategoryGuid}\" /success:disable /failure:disable",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                using var p = Process.Start(psi);
                p?.WaitForExit(1000);
                _logAction($"[EventListener] Audit policy disabled (Cleaned up).");
            }
            catch (Exception ex)
            {
                _logAction($"[EventListener] Failed to clean up audit policy: {ex.Message}");
            }
        }

        public void ClearPendingNotification(string appPath, string direction, string remoteAddress, string remotePort, string protocol)
        {
            if (string.IsNullOrEmpty(appPath) || string.IsNullOrEmpty(direction)) return;
            string key = $"{appPath}|{direction}|{remoteAddress}|{remotePort}|{protocol}";
            _pendingNotifications.TryRemove(key, out _);
        }

        public void ClearPendingNotification(string appPath, string direction)
        {
            if (string.IsNullOrEmpty(appPath) || string.IsNullOrEmpty(direction)) return;
            string broadKey = $"{appPath}|{direction}";
            _pendingNotifications.TryRemove(broadKey, out _);

            string keyPrefix = broadKey + "|";
            foreach (var key in _pendingNotifications.Keys)
            {
                if (key.StartsWith(keyPrefix, StringComparison.Ordinal))
                {
                    _pendingNotifications.TryRemove(key, out _);
                }
            }
        }

        public void SnoozeNotificationsForApp(string appPath, TimeSpan duration)
        {
            _snoozedApps[appPath] = DateTime.UtcNow.Add(duration);
        }

        public void ClearAllSnoozes()
        {
            _snoozedApps.Clear();
            _logAction("[EventListener] Cleared all snoozes.");
        }

        private bool ShouldProcessEvent(string appPath)
        {
            if (string.IsNullOrEmpty(appPath)) return false;
            if (_snoozedApps.TryGetValue(appPath, out DateTime snoozeUntil))
            {
                if (DateTime.UtcNow < snoozeUntil) return false;
                _snoozedApps.TryRemove(appPath, out _);
            }

            return _isLockdownEnabled();
        }

        private static string ParseDirection(string rawDirection)
        {
            return rawDirection switch
            {
                DirectionInboundCode => DirectionInbound,
                DirectionOutboundCode => DirectionOutbound,
                _ =>
                rawDirection,
            };
        }

        private static string GetValueFromXml(string xml, string elementName)
        {
            try
            {
                using var stringReader = new StringReader(xml);
                using var xmlReader = XmlReader.Create(stringReader, new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment });
                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "Data")
                    {
                        if (xmlReader.GetAttribute("Name") == elementName)

                        {
                            if (xmlReader.IsEmptyElement)
                            {

                                return string.Empty;
                            }
                            xmlReader.Read();
                            if (xmlReader.NodeType == XmlNodeType.Text)
                            {
                                return xmlReader.Value;
                            }
                            return string.Empty;
                        }
                    }
                }
            }
            catch (XmlException ex)
            {
                Debug.WriteLine($"[XML PARSE ERROR] {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UNEXPECTED XML ERROR] {ex.Message}");
            }
            return string.Empty;
        }

        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }
    }
}