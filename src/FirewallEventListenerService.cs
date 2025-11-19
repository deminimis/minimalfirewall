using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Xml;
using System.Collections.Concurrent;
using MinimalFirewall.TypedObjects;
using System.Collections.Generic;

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

            try
            {
                var query = new EventLogQuery("Security", PathType.LogName, "*[System[EventID=5157]]");
                _eventWatcher = new EventLogWatcher(query);
                _eventWatcher.EventRecordWritten += OnEventRecordWritten;
                _eventWatcher.Enabled = true;
                _logAction("[EventListener] Event watcher started successfully.");
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
            if (e.EventRecord == null)
            {
                return;
            }

            try
            {
                string xmlContent = e.EventRecord.ToXml();
                Task.Run(async () => await OnFirewallBlockEvent(xmlContent));
            }
            catch (EventLogException)
            {

            }
        }

        private async Task OnFirewallBlockEvent(string xmlContent)
        {
            string rawAppPathForClear = GetValueFromXml(xmlContent, "Application");
            string appPathForClear = PathResolver.NormalizePath(PathResolver.ConvertDevicePathToDrivePath(rawAppPathForClear));
            string directionForClear = ParseDirection(GetValueFromXml(xmlContent, "Direction"));
            string remoteAddressForClear = GetValueFromXml(xmlContent, "RemoteAddress");
            string remotePortForClear = GetValueFromXml(xmlContent, "RemotePort");
            string protocolForClear = GetValueFromXml(xmlContent, "Protocol");

            try
            {
                string rawAppPath = GetValueFromXml(xmlContent, "Application");
                string protocol = GetValueFromXml(xmlContent, "Protocol");
                string remotePort = GetValueFromXml(xmlContent, "RemotePort");
                string remoteAddress = GetValueFromXml(xmlContent, "RemoteAddress");
                string eventDirection = ParseDirection(GetValueFromXml(xmlContent, "Direction"));
                string filterId = GetValueFromXml(xmlContent, "FilterId");
                string layerId = GetValueFromXml(xmlContent, "LayerId");

                string xmlServiceName = GetValueFromXml(xmlContent, "ServiceName");
                string serviceName = (xmlServiceName == "N/A" || string.IsNullOrEmpty(xmlServiceName)) ? string.Empty : xmlServiceName;

                string appPath = PathResolver.ConvertDevicePathToDrivePath(rawAppPath);
                if (string.IsNullOrEmpty(appPath) || appPath.Equals("System", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
                appPath = PathResolver.NormalizePath(appPath);

                string notificationKey = $"{appPath}|{eventDirection}|{remoteAddress}|{remotePort}|{protocol}";
                if (!_pendingNotifications.TryAdd(notificationKey, true))
                {
                    _logAction($"[EventListener] Notification already pending for '{notificationKey}'. Ignoring duplicate event.");
                    return;
                }

                if (!ShouldProcessEvent(appPath))
                {
                    ClearPendingNotification(appPath, eventDirection, remoteAddress, remotePort, protocol);
                    return;
                }

                if (Path.GetFileName(appPath).Equals("svchost.exe", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(serviceName))
                    {
                        string processId = GetValueFromXml(xmlContent, "ProcessID");
                        if (!string.IsNullOrEmpty(processId) && processId != "0")
                        {
                            serviceName = SystemDiscoveryService.GetServicesByPID(processId);
                            _logAction($"[EventListener] svchost.exe detected. XML ServiceName was empty. PID: {processId}, Resolved Service(s): '{serviceName}'");
                        }
                    }
                    else
                    {
                        _logAction($"[EventListener] svchost.exe detected. ServiceName from XML: '{serviceName}'");
                    }
                }

                if (!string.IsNullOrEmpty(serviceName) &&
                    (serviceName.Equals("Dhcp", StringComparison.OrdinalIgnoreCase) ||
                     serviceName.Equals("Dnscache", StringComparison.OrdinalIgnoreCase)))
                {
                    _logAction($"[EventListener] Ignoring event for '{serviceName}' service (managed by system).");
                    ClearPendingNotification(appPath, eventDirection, remoteAddress, remotePort, protocol);
                    return;
                }

                MfwRuleStatus existingRuleStatus = await _dataService.CheckMfwRuleStatusAsync(appPath, serviceName, eventDirection);
                _logAction($"[EventListener] CheckMfwRuleStatus result for '{appPath}' (Service: '{serviceName}', Direction: '{eventDirection}') is: {existingRuleStatus}");

                if (existingRuleStatus == MfwRuleStatus.MfwBlock)
                {
                    _logAction($"[EventListener] An MFW Block rule already exists. Ignoring event.");
                    ClearPendingNotification(appPath, eventDirection, remoteAddress, remotePort, protocol);
                    return;
                }
                else if (existingRuleStatus == MfwRuleStatus.MfwAllow)
                {
                    if (filterId == "0")
                    {
                        _logAction($"[EventListener] Race condition detected: An MFW Allow rule exists, but a block event (FilterId 0) was received. Invalidating cache and snoozing to allow network to stabilize.");
                        _dataService.InvalidateRuleCache();
                        SnoozeNotificationsForApp(appPath, TimeSpan.FromSeconds(10));
                        ClearPendingNotification(appPath, eventDirection, remoteAddress, remotePort, protocol);
                        return;
                    }

                    _logAction($"[EventListener] An MFW Allow rule exists for this connection. Ignoring block event (FilterId: {filterId}).");
                    ClearPendingNotification(appPath, eventDirection, remoteAddress, remotePort, protocol);
                    return;
                }

                var matchingRule = _wildcardRuleService.Match(appPath);
                if (matchingRule != null)
                {
                    _logAction($"[EventListener] Wildcard rule matched for '{appPath}'. Action: '{matchingRule.Action}'.");
                    if (matchingRule.Action.StartsWith("Allow", StringComparison.OrdinalIgnoreCase) && ActionsService != null)
                    {
                        ActionsService.ApplyWildcardMatch(appPath, serviceName, matchingRule);
                        _logAction($"[EventListener] Applying Allow action from matched wildcard rule.");
                    }
                    else
                    {
                        _logAction($"[EventListener] Matched wildcard rule action is '{matchingRule.Action}'. Ignoring block event.");
                    }
                    ClearPendingNotification(appPath, eventDirection, remoteAddress, remotePort, protocol);
                    return;
                }

                if (_appSettings.AutoAllowSystemTrusted)
                {
                    if (SignatureValidationService.IsSignatureTrusted(appPath, out var trustedPublisherName) && trustedPublisherName != null)
                    {
                        _logAction($"[EventListener] Auto-allowing trusted application '{appPath}' by publisher '{trustedPublisherName}'.");
                        string allowAction = $"Allow ({eventDirection})";

                        if (_backgroundTaskService != null && !string.IsNullOrEmpty(appPath))
                        {
                            var appPayload = new ApplyApplicationRulePayload { AppPaths = new List<string> { appPath }, Action = allowAction };
                            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.ApplyApplicationRule, appPayload));
                        }
                        else
                        {
                            _logAction($"[EventListener ERROR] Cannot auto-allow. BackgroundTaskService is null or appPath is invalid.");
                        }

                        ClearPendingNotification(appPath, eventDirection, remoteAddress, remotePort, protocol);
                        return;
                    }
                    else
                    {
                        _logAction($"[EventListener] App '{appPath}' not trusted or signature check failed. Not auto-allowing.");
                    }
                }

                var pendingVm = new PendingConnectionViewModel
                {
                    AppPath = appPath,
                    Direction = eventDirection,
                    ServiceName = serviceName,
                    Protocol = protocol,
                    RemotePort = remotePort,
                    RemoteAddress = remoteAddress,
                    FilterId = filterId,
                    LayerId = layerId
                };
                _logAction($"[EventListener] Queuing pending connection for user decision: '{appPath}' (Service: '{serviceName}', Direction: '{eventDirection}', FilterId: {filterId})");
                PendingConnectionDetected?.Invoke(pendingVm);

            }
            catch (Exception ex)
            {
                _logAction($"[FATAL ERROR IN EVENT HANDLER] {ex}");
                ClearPendingNotification(appPathForClear, directionForClear, remoteAddressForClear, remotePortForClear, protocolForClear);
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
            _logAction($"[EventListener] Snoozing notifications for '{appPath}' for {duration}.");
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
                _logAction($"[EventListener] Event for '{appPath}' is snoozed. Ignoring.");
                return false;
            }

            bool lockdown = _isLockdownEnabled();
            if (!lockdown)
            {
                _logAction($"[EventListener] ShouldProcessEvent=false (Lockdown not enabled)");
            }
            return lockdown;
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
                Debug.WriteLine($"[XML PARSE ERROR] Failed to parse event XML for element '{elementName}': {ex.Message}\nXML: {xml}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UNEXPECTED XML PARSE ERROR] for element '{elementName}': {ex.Message}\nXML: {xml}");
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