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
        private readonly ConcurrentDictionary<string, DateTime> _recentlyNotifiedApps = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, System.Threading.Timer> _snoozeTimers = [];
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
            catch (Exception ex)
            {
                _logAction($"[EventListener FATAL ERROR] Could not start event watcher: {ex.Message}");
            }
        }

        public void Stop()
        {
            if (_eventWatcher != null && _eventWatcher.Enabled)
            {
                _eventWatcher.Enabled = false;
                _logAction("[EventListener] Event watcher disabled.");
            }
        }

        private void OnEventRecordWritten(object? sender, EventRecordWrittenEventArgs e)
        {
            if (e.EventRecord == null)
            {
                return;
            }

            Task.Run(() =>
            {
                using (var eventRecord = e.EventRecord)
                {
                    OnFirewallBlockEvent(eventRecord);
                }
            });
        }

        private void OnFirewallBlockEvent(EventRecord eventRecord)
        {
            try
            {
                if (ActionsService == null) return;

                _logAction("[EventListener] Firing OnFirewallBlockEvent.");
                string xmlContent = eventRecord.ToXml();

                string rawAppPath = GetValueFromXml(xmlContent, "Application");
                string appPath = PathResolver.ConvertDevicePathToDrivePath(rawAppPath);

                if (appPath.Equals("System", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(appPath))
                {
                    _logAction($"[EventListener] Event for '{rawAppPath}' process ignored.");
                    return;
                }

                appPath = PathResolver.NormalizePath(appPath);

                _logAction($"[EventListener] Event Path: {appPath} (Raw: {rawAppPath})");
                if (!ShouldProcessEvent(appPath))
                {
                    _logAction("[EventListener] Event SKIPPED by ShouldProcessEvent filter.");
                    return;
                }

                string eventDirection = ParseDirection(GetValueFromXml(xmlContent, "Direction"));
                string serviceName = SystemDiscoveryService.GetServicesByPID(GetValueFromXml(xmlContent, "ProcessID"));

                if (_dataService.DoesManagedRuleExist(appPath, serviceName, eventDirection))
                {
                    _logAction("[EventListener] Event SKIPPED: A managed rule already exists.");
                    return;
                }

                // 1. Check custom publisher whitelist (always active).
                if (SignatureValidationService.GetPublisherInfo(appPath, out var publisherName) && publisherName != null && _whitelistService.IsTrusted(publisherName))
                {
                    _logAction($"[EventListener] Auto-allowing whitelisted publisher application: {appPath} from {publisherName}");
                    string allowAction = $"Allow ({eventDirection})";
                    ActionsService.ApplyApplicationRuleChange([appPath], allowAction);
                    return;
                }

                // 2. Check if auto-allowing apps trusted by Windows is enabled.
                if (_appSettings.AutoAllowSystemTrusted)
                {
                    if (SignatureValidationService.IsSignatureTrusted(appPath, out var trustedPublisherName) && trustedPublisherName != null)
                    {
                        _logAction($"[EventListener] Auto-allowing system-trusted application: {appPath} from {trustedPublisherName}");
                        string allowAction = $"Allow ({eventDirection})";
                        ActionsService.ApplyApplicationRuleChange([appPath], allowAction);
                        return;
                    }
                }

                if (_recentlyNotifiedApps.TryGetValue(appPath, out DateTime lastNotificationTime))
                {
                    if ((DateTime.UtcNow - lastNotificationTime).TotalSeconds < 60)
                    {
                        _logAction($"[EventListener] Event DEBOUNCED for {appPath}. Last notification was too recent.");
                        return;
                    }
                }

                _recentlyNotifiedApps[appPath] = DateTime.UtcNow;
                _logAction($"[EventListener] Event PASSED all filters. Firing PendingConnectionDetected for {appPath}.");
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
            }
        }

        public void SnoozeNotificationsForApp(string appPath, TimeSpan duration)
        {
            _snoozedApps[appPath] = DateTime.UtcNow.Add(duration);
        }

        public void ClearAllSnoozes()
        {
            _snoozedApps.Clear();
            foreach (var timer in _snoozeTimers.Values)
            {
                timer.Dispose();
            }
            _snoozeTimers.Clear();
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
                "%%14592" => "Inbound",
                "%%14593" => "Outbound",
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
            catch (Exception ex)
            {
                Debug.WriteLine($"[XML PARSE ERROR] Failed to parse event XML for element '{elementName}': {ex.Message}");
            }
            return string.Empty;
        }

        public void Dispose()
        {
            if (_eventWatcher != null)
            {
                _eventWatcher.Enabled = false;
                _eventWatcher.EventRecordWritten -= OnEventRecordWritten;
                _eventWatcher.Dispose();
            }
            foreach (var timer in _snoozeTimers.Values)
            {
                timer.Dispose();
            }
            GC.SuppressFinalize(this);
        }
    }
}

