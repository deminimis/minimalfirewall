// FirewallEventListenerService.cs
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Xml;

namespace MinimalFirewall
{
    public partial class FirewallEventListenerService : IDisposable
    {
        private readonly FirewallDataService _dataService;
        private readonly WildcardRuleService _wildcardRuleService;
        private readonly Func<bool> _isLockdownEnabled;
        private readonly HashSet<string> _snoozedApps = [];
        private readonly Dictionary<string, System.Threading.Timer> _snoozeTimers = [];
        private EventLogWatcher? _eventWatcher;
        private readonly Action<string> _logAction;

        public event Action<PendingConnectionViewModel>? PendingConnectionDetected;
        public FirewallEventListenerService(FirewallDataService dataService, WildcardRuleService wildcardRuleService, Func<bool> isLockdownEnabled, Action<string> logAction)
        {
            _dataService = dataService;
            _wildcardRuleService = wildcardRuleService;
            _isLockdownEnabled = isLockdownEnabled;
            _logAction = logAction;
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
                _logAction("[EventListener] Firing OnFirewallBlockEvent.");
                string xmlContent = eventRecord.ToXml();

                string rawAppPath = GetValueFromXml(xmlContent, "Application");
                string appPath = PathResolver.ConvertDevicePathToDrivePath(rawAppPath);
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

                _logAction("[EventListener] Event PASSED all filters. Firing PendingConnectionDetected.");
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

        public void SnoozeNotificationsForApp(string appPath, int minutes)
        {
            _snoozedApps.Add(appPath);
            if (_snoozeTimers.TryGetValue(appPath, out var oldTimer))
            {
                oldTimer.Dispose();
            }
            var timer = new System.Threading.Timer(_ => _snoozedApps.Remove(appPath), null, TimeSpan.FromMinutes(minutes), Timeout.InfiniteTimeSpan);
            _snoozeTimers[appPath] = timer;
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
            return _isLockdownEnabled() && !_snoozedApps.Contains(appPath);
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
