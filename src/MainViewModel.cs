// Copyright (c) 2025 Deminimis
// Licensed under the GNU AGPL v3.

using NetFwTypeLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MinimalFirewall
{
    public class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        public ObservableCollection<FirewallRuleViewModel> ProgramRules { get; private set; }
        public ObservableCollection<FirewallRuleViewModel> Services { get; private set; }
        public ObservableCollection<UwpApp> UwpApps { get; private set; }
        public ObservableCollection<ProgramViewModel> UndefinedPrograms { get; private set; }
        public ObservableCollection<AdvancedRuleViewModel> AdvancedRules { get; private set; }
        public ObservableCollection<PendingConnectionViewModel> PendingConnections { get; private set; }
        public ObservableCollection<WildcardRule> WildcardRules { get; private set; }

        private readonly FirewallDataService _dataService;
        private readonly FirewallActionsService _actionsService;
        private readonly FirewallEventListenerService _eventListenerService;
        private readonly FirewallRuleService _firewallService;
        private readonly UserActivityLogger _activityLogger;
        private readonly WildcardRuleService _wildcardRuleService;
        private readonly StartupService _startupService;
        private readonly AppSettings _appSettings;
        private int _lastTemporaryMinutes = 2;
        private bool _uwpScanPerformed = false;

        public event Action<PendingConnectionViewModel, WildcardAction> WildcardRuleRequested;
        public static IEnumerable<SearchMode> SearchModes => Enum.GetValues(typeof(SearchMode)).Cast<SearchMode>();
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged("SearchText"); FilterAllCollections(); }
        }

        private SearchMode _currentSearchMode = SearchMode.Name;
        public SearchMode CurrentSearchMode
        {
            get => _currentSearchMode;
            set { _currentSearchMode = value; OnPropertyChanged("CurrentSearchMode"); FilterAllCollections(); }
        }

        private bool _isLockedDown;
        public bool IsLockedDown
        {
            get => _isLockedDown;
            set { _isLockedDown = value; OnPropertyChanged("IsLockedDown"); OnPropertyChanged("FirewallStatus"); }
        }

        public string FirewallStatus => IsLockedDown ? "Status: Locked Down" : "Status: Unlocked";

        public bool ShowSystemRules
        {
            get => _appSettings.ShowSystemRules;
            set
            {
                if (_appSettings.ShowSystemRules == value) return;
                _appSettings.ShowSystemRules = value;
                _dataService.ShowSystemRules = value;
                OnPropertyChanged("ShowSystemRules");
                _appSettings.Save();
                FastRefresh();
            }
        }

        public bool IsPopupsEnabled
        {
            get => _appSettings.IsPopupsEnabled;
            set
            {
                if (_appSettings.IsPopupsEnabled == value) return;
                _appSettings.IsPopupsEnabled = value;
                OnPropertyChanged("IsPopupsEnabled");
                _appSettings.Save();
            }
        }

        public bool IsLoggingEnabled
        {
            get => _appSettings.IsLoggingEnabled;
            set
            {
                if (_appSettings.IsLoggingEnabled == value) return;
                _appSettings.IsLoggingEnabled = value;
                _activityLogger.IsEnabled = value;
                OnPropertyChanged("IsLoggingEnabled");
                _appSettings.Save();
                if (value) _activityLogger.Log("Event Logging", "Enabled");
            }
        }

        public bool IsDarkModeEnabled
        {
            get => _appSettings.Theme == "Dark";
            set
            {
                string newTheme = value ? "Dark" : "Light";
                if (_appSettings.Theme == newTheme) return;

                _appSettings.Theme = newTheme;
                OnPropertyChanged("IsDarkModeEnabled");
                _appSettings.Save();
                (Application.Current as App)?.ApplyTheme(newTheme);
            }
        }

        public bool IsStartupEnabled
        {
            get => _appSettings.StartOnSystemStartup;
            set
            {
                if (_appSettings.StartOnSystemStartup == value) return;
                _appSettings.StartOnSystemStartup = value;
                _startupService.SetStartup(value);
                OnPropertyChanged("IsStartupEnabled");
                _appSettings.Save();
            }
        }

        public bool IsCloseToTrayEnabled
        {
            get => _appSettings.CloseToTray;
            set
            {
                if (_appSettings.CloseToTray == value) return;
                _appSettings.CloseToTray = value;
                OnPropertyChanged("IsCloseToTrayEnabled");
                _appSettings.Save();
            }
        }

        public ICommand ToggleLockdownCommand { get; private set; }
        public ICommand AllowPendingCommand { get; private set; }
        public ICommand BlockPendingCommand { get; private set; }
        public ICommand IgnorePendingCommand { get; private set; }
        public ICommand RemoveWildcardRuleCommand { get; private set; }

        public MainViewModel()
        {
            _appSettings = AppSettings.Load();
            if (Application.Current is App app)
            {
                app.ApplyTheme(_appSettings.Theme);
            }

            ProgramRules = new ObservableCollection<FirewallRuleViewModel>();
            Services = new ObservableCollection<FirewallRuleViewModel>();
            UwpApps = new ObservableCollection<UwpApp>();
            UndefinedPrograms = new ObservableCollection<ProgramViewModel>();
            AdvancedRules = new ObservableCollection<AdvancedRuleViewModel>();
            PendingConnections = new ObservableCollection<PendingConnectionViewModel>();
            WildcardRules = new ObservableCollection<WildcardRule>();

            _firewallService = new FirewallRuleService();
            _activityLogger = new UserActivityLogger { IsEnabled = _appSettings.IsLoggingEnabled };
            var uwpService = new UwpService();
            _wildcardRuleService = new WildcardRuleService();
            _startupService = new StartupService();
            _dataService = new FirewallDataService(_firewallService, uwpService, _activityLogger) { ShowSystemRules = _appSettings.ShowSystemRules };
            _actionsService = new FirewallActionsService(_firewallService, _dataService, _activityLogger);
            _eventListenerService = new FirewallEventListenerService(_dataService, _wildcardRuleService, _actionsService, () => IsLockedDown);

            ToggleLockdownCommand = new RelayCommand(new Action(ToggleLockdown));
            AllowPendingCommand = new RelayCommand<PendingConnectionViewModel>(p => AllowPendingConnection(p, false));
            BlockPendingCommand = new RelayCommand<PendingConnectionViewModel>(BlockPendingConnection);
            IgnorePendingCommand = new RelayCommand<PendingConnectionViewModel>(IgnorePendingConnection);
            RemoveWildcardRuleCommand = new RelayCommand<WildcardRule>(RemoveWildcardRule);
            _eventListenerService.PendingConnectionDetected += OnPendingConnectionDetected;
        }

        public async Task InitializeAsync()
        {
            var statusWindow = new StatusWindow("Loading Firewall Rules...");
            Application.Current.Dispatcher.Invoke(new Action(statusWindow.Show));

            await Task.Run(() =>
            {
                _dataService.LoadUwpAppsFromCache();
                _dataService.LoadInitialData();
            });
            UpdateCollectionsFromSource();
            WildcardRules = new ObservableCollection<WildcardRule>(_wildcardRuleService.GetRules());
            OnPropertyChanged("WildcardRules");
            FilterAllCollections();

            CheckFirewallStatus();
            _eventListenerService.Start();
            _activityLogger.Log("Application Started", "Version " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);

            Application.Current.Dispatcher.Invoke(new Action(statusWindow.Close));
        }

        private void UpdateCollectionsFromSource()
        {
            PopulateCollectionFromSource(ProgramRules, _dataService.AllProgramRules);
            PopulateCollectionFromSource(Services, _dataService.AllServiceRules);
            PopulateCollectionFromSource(UwpApps, _dataService.AllUwpApps);
            PopulateCollectionFromSource(UndefinedPrograms, _dataService.AllUndefinedPrograms);
            PopulateCollectionFromSource(AdvancedRules, _dataService.AllAdvancedRules);
        }

        private void FastRefresh()
        {
            _dataService.ApplyFilters();
            UpdateCollectionsFromSource();
            FilterAllCollections();
        }

        private async void SlowRefresh()
        {
            var statusWindow = new StatusWindow("Reloading Rules...");
            Application.Current.Dispatcher.Invoke(new Action(statusWindow.Show));
            await Task.Run(new Action(() => _dataService.LoadInitialData()));
            UpdateCollectionsFromSource();
            FilterAllCollections();
            Application.Current.Dispatcher.Invoke(new Action(statusWindow.Close));
        }

        private void OnPendingConnectionDetected(PendingConnectionViewModel pending)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (PendingConnections.Any(p => p.AppPath.Equals(pending.AppPath, StringComparison.OrdinalIgnoreCase) && p.Direction == pending.Direction)) return;
                PendingConnections.Add(pending);
                _activityLogger.Log("Pending Connection", pending.Direction + " for " + pending.AppPath);
                if (IsPopupsEnabled) { ShowConnectionNotifier(pending); }
            }));
        }

        public async Task LoadUwpAppsOnDemandAsync()
        {
            if (_uwpScanPerformed) return;
            var statusWindow = new StatusWindow("Scanning for UWP Apps");
            Application.Current.Dispatcher.Invoke(new Action(statusWindow.Show));
            var count = await _dataService.ScanForUwpAppsAsync();
            UpdateCollectionsFromSource();
            FilterAllCollections();
            _uwpScanPerformed = true;
            Application.Current.Dispatcher.Invoke(new Action(() => statusWindow.Complete("Found " + count + " UWP apps.")));
        }

        public async Task ScanDirectoryForUndefined(string directoryPath)
        {
            var statusWindow = new StatusWindow("Scanning Directory");
            Application.Current.Dispatcher.Invoke(new Action(statusWindow.Show));
            var count = await _dataService.ScanDirectoryForUndefined(directoryPath);
            UpdateCollectionsFromSource();
            FilterAllCollections();
            Application.Current.Dispatcher.Invoke(new Action(() => statusWindow.Complete("Found " + count + " new executable(s).")));
        }

        public void ApplyApplicationRuleChange(List<string> appPaths, string action) { _actionsService.ApplyApplicationRuleChange(appPaths, action); FastRefresh(); }
        public void ApplyUwpRuleChange(List<UwpApp> uwpApps, string action) { _actionsService.ApplyUwpRuleChange(uwpApps, action); SlowRefresh(); }
        public void DeleteApplicationRules(List<string> appPaths) { _actionsService.DeleteApplicationRules(appPaths); FastRefresh(); }
        public void DeleteUwpRules(List<string> packageFamilyNames) { _actionsService.DeleteUwpRules(packageFamilyNames); SlowRefresh(); }
        public void DeleteAdvancedRules(List<string> ruleNames) { _actionsService.DeleteAdvancedRules(ruleNames); FastRefresh(); }
        public void CreatePowerShellRule(string command) { _actionsService.CreatePowerShellRule(command); SlowRefresh(); }
        private void ToggleLockdown() { _actionsService.ToggleLockdown(); CheckFirewallStatus(); }

        public void CompleteWildcardRuleCreation(PendingConnectionViewModel pending, string selectedFolder, WildcardAction action)
        {
            string pattern = Path.Combine(selectedFolder, "*", Path.GetFileName(pending.AppPath));
            var newRule = new WildcardRule { Pattern = pattern, Action = action };

            _wildcardRuleService.AddRule(newRule);
            WildcardRules.Add(newRule);
            _activityLogger.Log("Wildcard Rule Created", pattern);
            if (action == WildcardAction.AutoAllow)
            {
                AllowPendingConnection(pending, false);
            }
            else
            {
                PendingConnections.Remove(pending);
            }
        }

        private void RemoveWildcardRule(WildcardRule rule)
        {
            if (rule == null) return;
            _wildcardRuleService.RemoveRule(rule);
            WildcardRules.Remove(rule);
            _activityLogger.Log("Wildcard Rule Removed", rule.Pattern);
        }

        private void AllowPendingConnection(PendingConnectionViewModel pending, bool createWildcard)
        {
            if (pending == null) return;
            if (createWildcard)
            {
                WildcardRuleRequested?.Invoke(pending, WildcardAction.AutoAllow);
            }
            else
            {
                ApplyApplicationRuleChange(new List<string> { pending.AppPath }, "Allow (" + pending.Direction + ")");
                PendingConnections.Remove(pending);
            }
        }

        private void BlockPendingConnection(PendingConnectionViewModel pending)
        {
            if (pending == null) return;
            ApplyApplicationRuleChange(new List<string> { pending.AppPath }, "Block (" + pending.Direction + ")");
            PendingConnections.Remove(pending);
        }

        private void IgnorePendingConnection(PendingConnectionViewModel pending)
        {
            if (pending == null) return;
            _eventListenerService.SnoozeNotificationsForApp(pending.AppPath, 2);
            PendingConnections.Remove(pending);
            _activityLogger.Log("Ignored Connection", pending.Direction + " for " + pending.AppPath);
        }

        public void AllowPendingConnectionTemporarily(PendingConnectionViewModel pending, int minutes)
        {
            if (pending == null) return;
            _actionsService.AllowPendingConnectionTemporarily(pending, minutes);
            PendingConnections.Remove(pending);
            SlowRefresh();
        }

        private void ShowConnectionNotifier(PendingConnectionViewModel pendingVm)
        {
            var dialog = new ConnectionNotifierWindow(pendingVm.AppPath, pendingVm.Direction, _lastTemporaryMinutes);
            if (dialog.ShowDialog() == true)
            {
                _lastTemporaryMinutes = dialog.Minutes;
                switch (dialog.Result)
                {
                    case ConnectionNotifierWindow.NotifierResult.Allow:
                        AllowPendingConnection(pendingVm, dialog.CreateWildcard);
                        break;
                    case ConnectionNotifierWindow.NotifierResult.Block:
                        BlockPendingConnection(pendingVm);
                        break;
                    case ConnectionNotifierWindow.NotifierResult.AllowTemporary:
                        AllowPendingConnectionTemporarily(pendingVm, dialog.Minutes);
                        break;
                }
            }
            else
            {
                IgnorePendingConnection(pendingVm);
            }
        }

        private void CheckFirewallStatus() => IsLockedDown = _firewallService.GetDefaultOutboundAction() == NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
        #region Filtering and Sorting
        private void FilterAllCollections()
        {
            bool namePredicate(string text) { return text != null && text.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0; }
            bool pathPredicate(string text) { return text != null && text.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0; }

            PopulateCollectionFromSource(ProgramRules, _dataService.AllProgramRules, r => CurrentSearchMode == SearchMode.Name ? namePredicate(r.Name) : pathPredicate(r.ApplicationName));
            PopulateCollectionFromSource(Services, _dataService.AllServiceRules, r => CurrentSearchMode == SearchMode.Name ? namePredicate(r.Name) : pathPredicate(r.ApplicationName));
            PopulateCollectionFromSource(UwpApps, _dataService.AllUwpApps, u => CurrentSearchMode == SearchMode.Name ? namePredicate(u.Name) : pathPredicate(u.PackageFamilyName));
            PopulateCollectionFromSource(UndefinedPrograms, _dataService.AllUndefinedPrograms, p => CurrentSearchMode == SearchMode.Name ? namePredicate(p.Name) : pathPredicate(p.ExePath));
            PopulateCollectionFromSource(AdvancedRules, _dataService.AllAdvancedRules, r => namePredicate(r.Name) || namePredicate(r.Description));
        }

        private void PopulateCollectionFromSource<T>(ObservableCollection<T> target, IReadOnlyList<T> source, Func<T, bool> filter = null)
        {
            if (target == null || source == null) return;
            target.Clear();
            var results = (filter == null || string.IsNullOrEmpty(_searchText)) ? source : source.Where(filter);
            foreach (var item in results)
            {
                target.Add(item);
            }
        }

        public void SortCollection(string listViewName, string propertyName, ListSortDirection direction)
        {
            IList collection;
            switch (listViewName)
            {
                case "ProgramsListView": collection = ProgramRules; break;
                case "ServicesListView": collection = Services; break;
                case "AdvancedRulesListView": collection = AdvancedRules; break;
                case "UndefinedProgramsListView": collection = UndefinedPrograms; break;
                case "PendingConnectionsListView": collection = PendingConnections; break;
                case "UwpAppsListView": collection = UwpApps; break;
                case "WildcardRulesListView": collection = WildcardRules; break;
                default: return;
            }

            if (collection == null || collection.Count == 0) return;
            var propertyInfo = collection.GetType().GetGenericArguments()[0].GetProperty(propertyName);
            if (propertyInfo == null) return;

            var sortedList = new List<object>();
            if (direction == ListSortDirection.Ascending)
            {
                sortedList.AddRange(collection.Cast<object>().OrderBy(item => propertyInfo.GetValue(item, null)));
            }
            else
            {
                sortedList.AddRange(collection.Cast<object>().OrderByDescending(item => propertyInfo.GetValue(item, null)));
            }

            collection.Clear();
            foreach (var item in sortedList)
            {
                collection.Add(item);
            }
        }
        #endregion

        public void Dispose()
        {
            _eventListenerService?.Dispose();
            GC.SuppressFinalize(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}