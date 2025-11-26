using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Xml;
using System.Collections.Concurrent;
using MinimalFirewall.TypedObjects;
using System.Collections.Generic;
using System.Net;

namespace MinimalFirewall
{
    public partial class FirewallEventListenerService : IDisposable
    {
        private readonly FirewallDataService _dataService;
        private readonly WildcardRuleService _wildcardRuleService;
        private readonly Func<bool> _isLockdownEnabled;
        private readonly AppSettings _appSettings;
        private readonly PublisherWhitelistService _whitelistService;
        private readonly ConcurrentDictionary<string, DateTime> _snoozedApps = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, bool> _pendingNotifications = new(StringComparer.OrdinalIgnoreCase);
        private EventLogWatcher? _eventWatcher;
        private readonly Action<string> _logAction;
        private readonly BackgroundFirewallTaskService _backgroundTaskService;

        public FirewallActionsService? ActionsService { get; set; }

        public event Action<PendingConnectionViewModel>? PendingConnectionDetected;

        public FirewallEventListenerService(FirewallDataService dataService, WildcardRuleService wildcardRuleService, Func<bool> isLockdownEnabled, Action<string> logAction, AppSettings appSettings, PublisherWhitelistService whitelistService, BackgroundFirewallTaskService backgroundTaskService)
        {
            _dataService = dataService;
            _wildcardRuleService = wildcardRuleService;
            _isLockdownEnabled = isLockdownEnabled;
            _logAction = logAction;
            _appSettings = appSettings;
            _whitelistService = whitelistService;
            _backgroundTaskService = backgroundTaskService;
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

            // Enforce auditing
            EnsureWfpAuditingEnabled();

            try
            {
                // Listens for both 5157 (Connection Blocked) and 5152 (Packet Dropped)
                var query = new EventLogQuery("Security", PathType.LogName, "*[System[(EventID=5157 or EventID=5152)]]");

                _eventWatcher = new EventLogWatcher(query);
                _eventWatcher.EventRecordWritten += OnEventRecordWritten;
                _eventWatcher.Enabled = true;
                _logAction("[EventListener] Event watcher started successfully (Listening for 5157 & 5152).");
            }
            catch (EventLogException ex)
            {
                _logAction($"[EventListener ERROR] You may not have permission to read the Security event log: {ex.Message}");
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
        }

        private void OnEventRecordWritten(object? sender, EventRecordWrittenEventArgs e)
        {
            if (e.EventRecord == null) return;

            try
            {
                string xmlContent = e.EventRecord.ToXml();
                Task.Run(async () => await OnFirewallBlockEvent(xmlContent));
            }
            catch (EventLogException) { }
        }

        private async Task OnFirewallBlockEvent(string xmlContent)
        {
            string rawAppPath = string.Empty;
            string appPathForClear = string.Empty;
            string directionForClear = string.Empty;
            string remoteAddressForClear = string.Empty;
            string remotePortForClear = string.Empty;
            string protocolForClear = string.Empty;

            try
            {
                // XML parsing
                rawAppPath = GetValueFromXml(xmlContent, "Application");
                directionForClear = ParseDirection(GetValueFromXml(xmlContent, "Direction"));
                remoteAddressForClear = GetValueFromXml(xmlContent, "RemoteAddress");
                remotePortForClear = GetValueFromXml(xmlContent, "RemotePort");
                protocolForClear = GetValueFromXml(xmlContent, "Protocol");
                string filterId = GetValueFromXml(xmlContent, "FilterId");
                string layerId = GetValueFromXml(xmlContent, "LayerId");
                string xmlServiceName = GetValueFromXml(xmlContent, "ServiceName");
                string pidStr = GetValueFromXml(xmlContent, "ProcessID");

                string serviceName = (xmlServiceName == "N/A" || string.IsNullOrEmpty(xmlServiceName)) ? string.Empty : xmlServiceName;

                if (IsNetworkNoise(remoteAddressForClear))
                {
                    return;
                }

                if (!string.IsNullOrEmpty(rawAppPath) &&
                    rawAppPath.Contains(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name ?? "MinimalFirewall", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                // Find name for 5152 events
                string appPath = rawAppPath;
                bool resolved = false;

                if (string.IsNullOrEmpty(appPath) || appPath == "-" || appPath.EndsWith(@"\-"))
                {
                    int.TryParse(pidStr, out int pid);
                    int.TryParse(protocolForClear, out int protocolNum);

                    if (protocolNum == 1 || protocolNum == 58)
                    {
                        appPath = "System (ICMP Traffic)";
                        resolved = true;
                    }
                    else if (pid == 0)
                    {
                        appPath = "Unsolicited Traffic (No Process)";
                        resolved = true;
                    }
                    else if (pid == 4)
                    {
                        appPath = "System (Kernel)";
                        resolved = true;
                    }
                    else if (pid > 4)
                    {
                        try
                        {
                            using var p = Process.GetProcessById(pid);
                            if (p.MainModule != null)
                            {
                                appPath = p.MainModule.FileName;
                                resolved = true;
                            }
                        }
                        catch
                        {
                            // Process may have closed
                        }
                    }

                    if (!resolved)
                    {
                        appPath = "System (Packet Drop)";
                    }
                }
                else
                {
                    // Normal path processing for standard events
                    try
                    {
                        string converted = PathResolver.ConvertDevicePathToDrivePath(rawAppPath);
                        if (!string.IsNullOrEmpty(converted)) appPath = converted;
                        appPath = PathResolver.NormalizePath(appPath);
                    }
                    catch { /* Keep original if normalization fails */ }
                }


                // Ignore System, Unsolicited Traffic, or Empty paths
                if (string.IsNullOrEmpty(appPath) ||
                    appPath.Equals("System", StringComparison.OrdinalIgnoreCase) ||
                    appPath.Equals("Unsolicited Traffic (No Process)", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                appPathForClear = appPath;

                // check pending alerts
                string notificationKey = $"{appPath}|{directionForClear}";
                if (!_pendingNotifications.TryAdd(notificationKey, true))
                {
                    return;
                }

                if (!ShouldProcessEvent(appPath))
                {
                    ClearPendingNotification(appPath, directionForClear);
                    return;
                }

                // Svchost resolution
                if (Path.GetFileName(appPath).Equals("svchost.exe", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(serviceName))
                    {
                        if (!string.IsNullOrEmpty(pidStr) && pidStr != "0")
                        {
                            serviceName = SystemDiscoveryService.GetServicesByPID(pidStr);
                            _logAction($"[EventListener] svchost.exe detected. PID: {pidStr}, Resolved: '{serviceName}'");
                        }
                    }
                }

                // Ignore noisy services
                if (!string.IsNullOrEmpty(serviceName) &&
                    (serviceName.Equals("Dhcp", StringComparison.OrdinalIgnoreCase) ||
                     serviceName.Equals("Dnscache", StringComparison.OrdinalIgnoreCase) ||
                     serviceName.Equals("Ssdpsrv", StringComparison.OrdinalIgnoreCase)))
                {
                    ClearPendingNotification(appPath, directionForClear);
                    return;
                }

                // check existing rules
                MfwRuleStatus existingRuleStatus = await _dataService.CheckMfwRuleStatusAsync(appPath, serviceName, directionForClear);
                if (existingRuleStatus == MfwRuleStatus.MfwBlock)
                {
                    ClearPendingNotification(appPath, directionForClear);
                    return;
                }
                else if (existingRuleStatus == MfwRuleStatus.MfwAllow)
                {
                    if (filterId == "0")
                    {
                        _dataService.InvalidateRuleCache();
                        SnoozeNotificationsForApp(appPath, TimeSpan.FromSeconds(10));
                        ClearPendingNotification(appPath, directionForClear);
                        return;
                    }
                    ClearPendingNotification(appPath, directionForClear);
                    return;
                }

                // wildcard check
                var matchingRule = _wildcardRuleService.Match(appPath);
                if (matchingRule != null)
                {
                    if (matchingRule.Action.StartsWith("Allow", StringComparison.OrdinalIgnoreCase) && ActionsService != null)
                    {
                        ActionsService.ApplyWildcardMatch(appPath, serviceName, matchingRule);
                    }
                    ClearPendingNotification(appPath, directionForClear);
                    return;
                }

                // auto-allow trusted
                if (_appSettings.AutoAllowSystemTrusted)
                {
                    if (File.Exists(appPath) && SignatureValidationService.IsSignatureTrusted(appPath, out var trustedPublisherName) && trustedPublisherName != null)
                    {
                        if (_backgroundTaskService != null && !string.IsNullOrEmpty(appPath))
                        {
                            string allowAction = $"Allow ({directionForClear})";
                            var appPayload = new ApplyApplicationRulePayload { AppPaths = new List<string> { appPath }, Action = allowAction };
                            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.ApplyApplicationRule, appPayload));
                        }
                        ClearPendingNotification(appPath, directionForClear);
                        return;
                    }
                }

                // View model for dashboard
                var pendingVm = new PendingConnectionViewModel
                {
                    AppPath = appPath,
                    Direction = directionForClear,
                    ServiceName = serviceName,
                    Protocol = protocolForClear,
                    RemotePort = remotePortForClear,
                    RemoteAddress = remoteAddressForClear,
                    FilterId = filterId,
                    LayerId = layerId
                };
                PendingConnectionDetected?.Invoke(pendingVm);
            }
            catch (Exception ex)
            {
                _logAction($"[FATAL ERROR IN EVENT HANDLER] {ex.Message}");
                if (!string.IsNullOrEmpty(appPathForClear) && !string.IsNullOrEmpty(directionForClear))
                {
                    ClearPendingNotification(appPathForClear, directionForClear);
                }
            }
        }

        // Noise filter method
        private bool IsNetworkNoise(string remoteIp)
        {
            if (string.IsNullOrEmpty(remoteIp)) return false;

            // Broadcast 
            if (remoteIp == "255.255.255.255") return true;

            // IPv4 Multicast (224.0.0.0 to 239.255.255.255)
            if (remoteIp.StartsWith("224.") || remoteIp.StartsWith("239.")) return true;

            // IPv6 Multicast (ff00::/8)
            if (remoteIp.StartsWith("ff", StringComparison.OrdinalIgnoreCase)) return true;

            return false;
        }

        // Audit Policy enforcement
        private void EnsureWfpAuditingEnabled()
        {
            try
            {
                // Enable 5157
                Process.Start(new ProcessStartInfo
                {
                    FileName = "auditpol.exe",
                    Arguments = "/set /subcategory:\"{0CCE9226-69AE-11D9-BED3-505054503030}\" /success:disable /failure:enable", // GUID for Filtering Platform Connection
                    CreateNoWindow = true,
                    UseShellExecute = false
                });

                // Enable 5152
                Process.Start(new ProcessStartInfo
                {
                    FileName = "auditpol.exe",
                    Arguments = "/set /subcategory:\"{0CCE9225-69AE-11D9-BED3-505054503030}\" /success:disable /failure:enable", // GUID for Filtering Platform Packet Drop
                    CreateNoWindow = true,
                    UseShellExecute = false
                });
            }
            catch (Exception ex)
            {
                _logAction($"[EventListener] Failed to set audit policy: {ex.Message}");
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

            string keyPrefix = $"{appPath}|{direction}|";
            var matchingKeys = _pendingNotifications.Keys.Where(k => k.StartsWith(keyPrefix)).ToList();
            foreach (var k in matchingKeys)
            {
                _pendingNotifications.TryRemove(k, out _);
            }
        }

        public void SnoozeNotificationsForApp(string appPath, TimeSpan duration)
        {
            _snoozedApps[appPath] = DateTime.UtcNow.Add(duration);
        }

        public void ClearAllSnoozes()
        {
            _snoozedApps.Clear();
            _logAction($"[EventListener] Cleared all snoozes.");
        }

        private bool ShouldProcessEvent(string appPath)
        {
            if (string.IsNullOrEmpty(appPath) || appPath.Equals("System", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (_snoozedApps.TryGetValue(appPath, out DateTime snoozeUntil) && DateTime.UtcNow < snoozeUntil)
            {
                return false;
            }

            return _isLockdownEnabled();
        }

        private static string ParseDirection(string rawDirection)
        {
            return rawDirection switch
            {
                "%%14592" => "Incoming",
                "%%14593" => "Outgoing",
                _ => rawDirection,
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