using NetFwTypeLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Xml.Linq;

namespace MinimalFirewall
{
    public class FirewallEventListenerService : IDisposable
    {
        private readonly FirewallDataService _dataService;
        private readonly Func<bool> _isLockdownEnabled;
        private EventLogWatcher _eventWatcher;

        private readonly WildcardRuleService _wildcardRuleService;
        private readonly FirewallActionsService _actionsService;
        private readonly HashSet<string> _snoozedApps = new HashSet<string>();
        private readonly Dictionary<string, Timer> _snoozeTimers = new Dictionary<string, Timer>();

        public event Action<PendingConnectionViewModel> PendingConnectionDetected;
        public FirewallEventListenerService(FirewallDataService dataService, WildcardRuleService wildcardRuleService, FirewallActionsService actionsService, Func<bool> isLockdownEnabled)
        {
            _dataService = dataService;
            _wildcardRuleService = wildcardRuleService;
            _actionsService = actionsService;
            _isLockdownEnabled = isLockdownEnabled;
        }

        public void Start()
        {
            try
            {
                var query = new EventLogQuery("Security", PathType.LogName, "*[System[EventID=5157]]");
                _eventWatcher = new EventLogWatcher(query);
                _eventWatcher.EventRecordWritten += OnFirewallBlockEvent;
                _eventWatcher.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not start event listener. Make sure auditing is enabled and the app is run as Admin.\n\n" + ex.Message, "Listener Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnFirewallBlockEvent(object sender, EventRecordWrittenEventArgs e)
        {
            try
            {
                if (e.EventRecord == null) return;
                string xmlContent = e.EventRecord.ToXml();

                string rawAppPath = GetValueFromXml(xmlContent, "Application");
                string appPath = PathResolver.ConvertDevicePathToDrivePath(rawAppPath);
                if (!ShouldProcessEvent(appPath)) return;

                var eventDirection = ParseDirection(GetValueFromXml(xmlContent, "Direction"));

                if (_dataService.DoesRuleExist(appPath, eventDirection))
                {
                    return;
                }

                var matchingRule = _wildcardRuleService.Match(appPath);
                if (matchingRule != null)
                {
                    if (matchingRule.Action == WildcardAction.AutoAllow)
                    {
                        _actionsService.ApplyApplicationRuleChange(new List<string> { appPath }, "Allow (" + eventDirection + ")");
                    }
                    return;
                }

                var pendingVm = new PendingConnectionViewModel
                {
                    AppPath = appPath,
                    Direction = eventDirection
                };
                PendingConnectionDetected?.Invoke(pendingVm);
            }
            catch (Exception ex) { Debug.WriteLine("[FATAL ERROR IN EVENT HANDLER] " + ex); }
        }

        public void SnoozeNotificationsForApp(string appPath, int minutes)
        {
            _snoozedApps.Add(appPath);
            if (_snoozeTimers.TryGetValue(appPath, out var oldTimer))
            {
                oldTimer.Dispose();
            }
            var timer = new Timer(_ => Application.Current.Dispatcher.Invoke(new Action(() => _snoozedApps.Remove(appPath))), null, TimeSpan.FromMinutes(minutes), Timeout.InfiniteTimeSpan);
            _snoozeTimers[appPath] = timer;
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
            switch (rawDirection)
            {
                case "%%14592": return "Inbound";
                case "%%14593": return "Outbound";
                default: return rawDirection;
            }
        }

        private static string GetValueFromXml(string xml, string elementName)
        {
            try
            {
                var xdoc = XDocument.Parse(xml);
                var ns = xdoc.Root.GetDefaultNamespace();
                var dataElement = xdoc.Descendants(ns + "Data").FirstOrDefault(d => d.Attribute("Name")?.Value == elementName);
                return dataElement?.Value ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public void Dispose()
        {
            _eventWatcher?.Dispose();
            foreach (var timer in _snoozeTimers.Values)
            {
                timer.Dispose();
            }
            GC.SuppressFinalize(this);
        }
    }
}