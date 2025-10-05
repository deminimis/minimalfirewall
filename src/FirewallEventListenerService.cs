// File: FirewallEventListenerService.cs
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Xml;
using System.Collections.Concurrent;

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

        public FirewallActionsService? ActionsService { get; set; }

        public event Action<PendingConnectionViewModel>? PendingConnectionDetected;
        public FirewallEventListenerService(FirewallDataService dataService, WildcardRuleService wildcardRuleService, Func<bool> isLockdownEnabled, Action<string> logAction, AppSettings appSettings, PublisherWhitelistService whitelistService)
        {
            _dataService = dataService;
            _wildcardRuleService = wildcardRuleService;
            _isLockdownEnabled = isLockdownEnabled;
            _logAction = logAction;
            _appSettings = appSettings;
            _whitelistService = whitelistService;
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
                Task.Run(() => OnFirewallBlockEvent(xmlContent));
            }
            catch (EventLogException)
            {
            }
        }

        private void OnFirewallBlockEvent(string xmlContent)
        {
            try
            {
                string rawAppPath = GetValueFromXml(xmlContent, "Application");
                string appPath = PathResolver.ConvertDevicePathToDrivePath(rawAppPath);
                if (string.IsNullOrEmpty(appPath) || appPath.Equals("System", StringComparison.OrdinalIgnoreCase)) return;
                appPath = PathResolver.NormalizePath(appPath);
                string eventDirection = ParseDirection(GetValueFromXml(xmlContent, "Direction"));

                string notificationKey = $"{appPath}|{eventDirection}";
                if (!_pendingNotifications.TryAdd(notificationKey, true)) return;

                if (!ShouldProcessEvent(appPath))
                {
                    ClearPendingNotification(appPath, eventDirection);
                    return;
                }

                string serviceName = string.Empty;
                if (Path.GetFileName(appPath).Equals("svchost.exe", StringComparison.OrdinalIgnoreCase))
                {
                    string processId = GetValueFromXml(xmlContent, "ProcessID");
                    if (!string.IsNullOrEmpty(processId) && processId != "0")
                    {
                        serviceName = SystemDiscoveryService.GetServicesByPID(processId);
                    }
                }

                if (_dataService.DoesAnyRuleExist(appPath, serviceName, eventDirection))
                {
                    ClearPendingNotification(appPath, eventDirection);
                    return;
                }

                var matchingRule = _wildcardRuleService.Match(appPath);
                if (matchingRule != null)
                {
                    if (matchingRule.Action.StartsWith("Allow", StringComparison.OrdinalIgnoreCase) && ActionsService != null)
                    {
                        ActionsService.ApplyApplicationRuleChange(new List<string> { appPath }, matchingRule.Action, matchingRule.FolderPath);
                    }
                    ClearPendingNotification(appPath, eventDirection);
                    return;
                }

                if (_appSettings.AutoAllowSystemTrusted)
                {
                    if (SignatureValidationService.IsSignatureTrusted(appPath, out var trustedPublisherName) && trustedPublisherName != null)
                    {
                        string allowAction = $"Allow ({eventDirection})";
                        ActionsService?.ApplyApplicationRuleChange(new List<string> { appPath }, allowAction);
                        ClearPendingNotification(appPath, eventDirection);
                        return;
                    }
                }

                var pendingVm = new PendingConnectionViewModel
                {
                    AppPath = appPath,
                    Direction = eventDirection,
                    ServiceName = serviceName
                };
                PendingConnectionDetected?.Invoke(pendingVm);
            }
            catch (Exception ex)
            {
                _logAction($"[FATAL ERROR IN EVENT HANDLER] {ex}");
                string appPathForClear = PathResolver.NormalizePath(PathResolver.ConvertDevicePathToDrivePath(GetValueFromXml(xmlContent, "Application")));
                string directionForClear = ParseDirection(GetValueFromXml(xmlContent, "Direction"));
                if (!string.IsNullOrEmpty(appPathForClear))
                {
                    ClearPendingNotification(appPathForClear, directionForClear);
                }
            }
        }

        public void ClearPendingNotification(string appPath, string direction)
        {
            if (string.IsNullOrEmpty(appPath) || string.IsNullOrEmpty(direction)) return;
            string key = $"{appPath}|{direction}";
            _pendingNotifications.TryRemove(key, out _);
        }

        public void SnoozeNotificationsForApp(string appPath, TimeSpan duration)
        {
            _snoozedApps[appPath] = DateTime.UtcNow.Add(duration);
        }

        public void ClearAllSnoozes()
        {
            _snoozedApps.Clear();
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
                using var xmlReader = XmlReader.Create(stringReader);
                while (xmlReader.ReadToFollowing("Data"))
                {
                    if (xmlReader.GetAttribute("Name") == elementName)
                    {
                        return xmlReader.ReadElementContentAsString();
                    }
                }
            }
            catch (XmlException ex)
            {
                Debug.WriteLine($"[XML PARSE ERROR] Failed to parse event XML for element '{elementName}': {ex.Message}");
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
