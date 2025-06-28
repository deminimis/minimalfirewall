using NetFwTypeLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
        public ObservableCollection<AdvancedRuleViewModel> ForeignRules { get; private set; }

        private readonly FirewallDataService _dataService;
        private readonly FirewallActionsService _actionsService;
        private readonly FirewallEventListenerService _eventListenerService;
        private readonly FirewallRuleService _firewallService;
        private readonly UserActivityLogger _activityLogger;
        private readonly WildcardRuleService _wildcardRuleService;
        private readonly StartupService _startupService;
        private readonly AppSettings _appSettings;
        private readonly ForeignRuleTracker _foreignRuleTracker;
        private int _lastTemporaryMinutes = 5;
        private bool _uwpScanPerformed = false;
        private bool _foreignRulesLoaded = false;

        public bool IsCacheCleared { get; private set; } = false;

        public string VersionInfo { get; }

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
        public ICommand AcknowledgeForeignRulesCommand { get; private set; }
        public ICommand CreateWildcardFromTabCommand { get; private set; }
        public ICommand CheckForUpdatesCommand { get; private set; }
        public ICommand ScanDirectoryCommand { get; private set; }
        public ICommand ClearUndefinedProgramsCommand { get; private set; }

        public MainViewModel()
        {
            _appSettings = AppSettings.Load();
            if (Application.Current is App app)
            {
                app.ApplyTheme(_appSettings.Theme);
            }

            VersionInfo = "Version " + Assembly.GetExecutingAssembly().GetName().Version.ToString(3);

            ProgramRules = new ObservableCollection<FirewallRuleViewModel>();
            Services = new ObservableCollection<FirewallRuleViewModel>();
            UwpApps = new ObservableCollection<UwpApp>();
            UndefinedPrograms = new ObservableCollection<ProgramViewModel>();
            AdvancedRules = new ObservableCollection<AdvancedRuleViewModel>();
            PendingConnections = new ObservableCollection<PendingConnectionViewModel>();
            WildcardRules = new ObservableCollection<WildcardRule>();
            ForeignRules = new ObservableCollection<AdvancedRuleViewModel>();

            _firewallService = new FirewallRuleService();
            _activityLogger = new UserActivityLogger { IsEnabled = _appSettings.IsLoggingEnabled };
            var uwpService = new UwpService();
            _wildcardRuleService = new WildcardRuleService();
            _startupService = new StartupService();
            _foreignRuleTracker = new ForeignRuleTracker();
            _dataService = new FirewallDataService(_firewallService, uwpService, _activityLogger) { ShowSystemRules = _appSettings.ShowSystemRules };
            _actionsService = new FirewallActionsService(_firewallService, _dataService, _activityLogger);
            _eventListenerService = new FirewallEventListenerService(_dataService, _wildcardRuleService, _actionsService, () => IsLockedDown);

            ToggleLockdownCommand = new RelayCommand(new Action(ToggleLockdown));
            AllowPendingCommand = new RelayCommand<PendingConnectionViewModel>(AllowPendingConnection);
            BlockPendingCommand = new RelayCommand<PendingConnectionViewModel>(BlockPendingConnection);
            AcknowledgeForeignRulesCommand = new RelayCommand(AcknowledgeForeignRules);
            IgnorePendingCommand = new RelayCommand<PendingConnectionViewModel>(IgnorePendingConnection);
            RemoveWildcardRuleCommand = new RelayCommand<WildcardRule>(RemoveWildcardRule);
            CreateWildcardFromTabCommand = new RelayCommand(() => HandleWildcardCreationRequest(null));
            CheckForUpdatesCommand = new RelayCommand(CheckForUpdates);
            ScanDirectoryCommand = new RelayCommand(() => _ = ScanDirectoryForUndefined());
            ClearUndefinedProgramsCommand = new RelayCommand(ClearUndefinedPrograms);

            _eventListenerService.PendingConnectionDetected += OnPendingConnectionDetected;
            _actionsService.ApplicationRuleSetExpired += OnApplicationRuleSetExpired;
        }

        private void ClearUndefinedPrograms()
        {
            _dataService.ClearUndefinedPrograms();
            UndefinedPrograms.Clear();
        }

        private void CheckForUpdates()
        {
            try
            {
                Process.Start("https://github.com/deminimis/minimalfirewall/releases");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open the link.\n\nError: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

            await Task.Run(() => SyncWildcardRules(false));

            Application.Current.Dispatcher.Invoke(() =>
            {
                UpdateCollectionsFromSource();
                WildcardRules = new ObservableCollection<WildcardRule>(_wildcardRuleService.GetRules());
                OnPropertyChanged("WildcardRules");
                FilterAllCollections();
            });

            CheckFirewallStatus();
            _eventListenerService.Start();
            _activityLogger.Log("Application Started", "Version " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);

            IsCacheCleared = false;
            Application.Current.Dispatcher.Invoke(new Action(statusWindow.Close));
        }

        public void SyncWildcardRules(bool shouldRefreshUI)
        {
            var allWildcards = _wildcardRuleService.GetRules();
            if (!allWildcards.Any()) return;

            var allSystemRules = _firewallService.GetAllRules();

            foreach (var wildcard in allWildcards)
            {
                var executablesInFolder = SystemDiscoveryService.GetExecutablesInFolder(wildcard.FolderPath);
                if (!executablesInFolder.Any()) continue;

                string descriptionTag = $"{MFWConstants.WildcardDescriptionPrefix}{wildcard.FolderPath}]";
                var existingRulePaths = allSystemRules
                    .Where(r => r.Description != null && r.Description == descriptionTag)
                    .Select(r => r.ApplicationName)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var missingExePaths = executablesInFolder.Where(exe => !existingRulePaths.Contains(exe)).ToList();

                if (missingExePaths.Any())
                {
                    _actionsService.ApplyApplicationRuleChange(missingExePaths, wildcard.Action, wildcard.FolderPath);
                }
            }
            if (shouldRefreshUI)
            {
                FastRefresh();
            }
        }

        private void UpdateCollectionsFromSource()
        {
            PopulateCollectionFromSource(ProgramRules, _dataService.AllProgramRules);
            PopulateCollectionFromSource(Services, _dataService.AllServiceRules);
            PopulateCollectionFromSource(UwpApps, _dataService.AllUwpApps);
            PopulateCollectionFromSource(UndefinedPrograms, _dataService.AllUndefinedPrograms);
            PopulateCollectionFromSource(AdvancedRules, _dataService.AllAdvancedRules);
            PopulateCollectionFromSource(ForeignRules, _dataService.AllForeignRules);
        }

        private void FastRefresh()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _dataService.ApplyFilters();
                UpdateCollectionsFromSource();
                FilterAllCollections();
            });
        }

        public async void SlowRefresh()
        {
            var statusWindow = new StatusWindow("Reloading Rules...");
            Application.Current.Dispatcher.Invoke(new Action(statusWindow.Show));
            await Task.Run(() => {
                _dataService.LoadInitialData();
                SyncWildcardRules(false);
            });

            Application.Current.Dispatcher.Invoke(() =>
            {
                UpdateCollectionsFromSource();
                FilterAllCollections();
                IsCacheCleared = false;
            });

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

        private void OnApplicationRuleSetExpired(string appPath)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _dataService.AddOrUpdateAppRule(appPath);
                FastRefresh();
            });
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

        public async Task LoadForeignRulesOnDemandAsync(bool forceRescan = false)
        {
            if (_foreignRulesLoaded && !forceRescan)
            {
                return;
            }

            var statusWindow = new StatusWindow("Scanning for new third-party rules...");
            Application.Current.Dispatcher.Invoke(() => statusWindow.Show());

            await Task.Run(() => _dataService.ScanForForeignRules(_foreignRuleTracker));

            UpdateCollectionsFromSource();
            FilterAllCollections();
            _foreignRulesLoaded = true;

            Application.Current.Dispatcher.Invoke(() => statusWindow.Close());
        }

        private void AcknowledgeForeignRules()
        {
            var ruleNamesToAcknowledge = ForeignRules.Select(r => r.Name).ToList();
            if (ruleNamesToAcknowledge.Count == 0)
            {
                MessageBox.Show("There are no new foreign rules to acknowledge.", "No New Rules", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to acknowledge {ruleNamesToAcknowledge.Count} new rule(s)?\n\nThey will be hidden from this list unless they are modified.", "Confirm Acknowledgment", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _foreignRuleTracker.AcknowledgeRules(ruleNamesToAcknowledge);
                ForeignRules.Clear();
                _activityLogger.Log("Foreign Rules", $"Acknowledged {ruleNamesToAcknowledge.Count} rule(s).");
            }
        }

        public async Task ScanDirectoryForUndefined()
        {
            if (FolderPicker.TryPickFolder(out var folderPath) && folderPath != null)
            {
                var statusWindow = new StatusWindow("Scanning Directory");
                Application.Current.Dispatcher.Invoke(new Action(statusWindow.Show));
                var count = await _dataService.ScanDirectoryForUndefined(folderPath);
                UpdateCollectionsFromSource();
                FilterAllCollections();
                Application.Current.Dispatcher.Invoke(new Action(() => statusWindow.Complete("Found " + count + " new executable(s).")));
            }
        }

        public void ClearCachesForTray()
        {
            _dataService.ClearDataCaches();

            ProgramRules.Clear();
            Services.Clear();
            UwpApps.Clear();
            UndefinedPrograms.Clear();
            AdvancedRules.Clear();
            ForeignRules.Clear();

            _uwpScanPerformed = false;
            _foreignRulesLoaded = false;
            IsCacheCleared = true;
        }

        public void CreateWildcardRule(string folderPath, string action)
        {
            var executables = SystemDiscoveryService.GetExecutablesInFolder(folderPath);
            if (!executables.Any())
            {
                MessageBox.Show("No executable files were found in the selected folder.", "No Files Found", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var newRule = new WildcardRule { FolderPath = folderPath, Action = action };
            _wildcardRuleService.AddRule(newRule);
            WildcardRules.Add(newRule);
            _activityLogger.Log("Wildcard Rule Created", $"{action} for folder {folderPath}");

            _actionsService.ApplyApplicationRuleChange(executables, action, folderPath);
            FastRefresh();
        }

        public void HandleWildcardCreationRequest(string initialPath, PendingConnectionViewModel pendingToRemove = null)
        {
            var dialog = new WildcardCreatorWindow();
            if (!string.IsNullOrEmpty(initialPath))
            {
                dialog.FolderPathTextBox.Text = initialPath;
            }

            if (dialog.ShowDialog() == true)
            {
                CreateWildcardRule(dialog.FolderPath, dialog.SelectedAction);
                if (pendingToRemove != null)
                {
                    PendingConnections.Remove(pendingToRemove);
                }
            }
        }

        public void ApplyApplicationRuleChange(List<string> appPaths, string action) { _actionsService.ApplyApplicationRuleChange(appPaths, action); FastRefresh(); }
        public void ApplyUwpRuleChange(List<UwpApp> uwpApps, string action) { _actionsService.ApplyUwpRuleChange(uwpApps, action); SlowRefresh(); }

        public void DeleteApplicationRules(List<string> appPaths)
        {
            _actionsService.DeleteApplicationRules(appPaths);
            _eventListenerService.ClearAllSnoozes();
            FastRefresh();
        }

        public void DeleteUwpRules(List<string> packageFamilyNames) { _actionsService.DeleteUwpRules(packageFamilyNames); SlowRefresh(); }
        public void DeleteAdvancedRules(List<string> ruleNames) { _actionsService.DeleteAdvancedRules(ruleNames); FastRefresh(); }
        public void CreatePowerShellRule(string command) { _actionsService.CreatePowerShellRule(command); SlowRefresh(); }
        private void ToggleLockdown() { _actionsService.ToggleLockdown(); CheckFirewallStatus(); }

        private void RemoveWildcardRule(WildcardRule rule)
        {
            if (rule == null) return;
            var result = MessageBox.Show($"This will delete the wildcard rule and all firewall rules created by it for the folder:\n\n{rule.FolderPath}\n\nAre you sure you want to continue?", "Confirm Wildcard Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No) return;

            _actionsService.DeleteRulesForWildcard(rule);
            _wildcardRuleService.RemoveRule(rule);
            WildcardRules.Remove(rule);
            _eventListenerService.ClearAllSnoozes();
            _activityLogger.Log("Wildcard Rule Removed", rule.FolderPath);
            FastRefresh();
        }

        private void AllowPendingConnection(PendingConnectionViewModel pending)
        {
            if (pending == null) return;
            ApplyApplicationRuleChange(new List<string> { pending.AppPath }, "Allow (" + pending.Direction + ")");
            PendingConnections.Remove(pending);
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
            FastRefresh();
        }

        private void ShowConnectionNotifier(PendingConnectionViewModel pendingVm)
        {
            var dialog = new ConnectionNotifierWindow(pendingVm.AppPath, pendingVm.Direction, _lastTemporaryMinutes);
            if (dialog.ShowDialog() != true)
            {
                IgnorePendingConnection(pendingVm);
                return;
            }

            _lastTemporaryMinutes = dialog.Minutes;

            switch (dialog.Result)
            {
                case ConnectionNotifierWindow.NotifierResult.Allow:
                    AllowPendingConnection(pendingVm);
                    break;
                case ConnectionNotifierWindow.NotifierResult.Block:
                    BlockPendingConnection(pendingVm);
                    break;
                case ConnectionNotifierWindow.NotifierResult.AllowTemporary:
                    AllowPendingConnectionTemporarily(pendingVm, dialog.Minutes);
                    break;
                case ConnectionNotifierWindow.NotifierResult.CreateWildcard:
                    HandleWildcardCreationRequest(Path.GetDirectoryName(pendingVm.AppPath), pendingVm);
                    break;
            }
        }

        private void CheckFirewallStatus() => IsLockedDown = _firewallService.GetDefaultOutboundAction() == NET_FW_ACTION_.NET_FW_ACTION_BLOCK;

        private void FilterAllCollections()
        {
            bool namePredicate(string text) { return text != null && text.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0; }
            bool pathPredicate(string text) { return text != null && text.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0; }

            PopulateCollectionFromSource(ProgramRules, _dataService.AllProgramRules, r => CurrentSearchMode == SearchMode.Name ? namePredicate(r.Name) : pathPredicate(r.ApplicationName));
            PopulateCollectionFromSource(Services, _dataService.AllServiceRules, r => CurrentSearchMode == SearchMode.Name ? namePredicate(r.Name) : pathPredicate(r.ApplicationName));
            PopulateCollectionFromSource(UwpApps, _dataService.AllUwpApps, u => CurrentSearchMode == SearchMode.Name ? namePredicate(u.Name) : pathPredicate(u.PackageFamilyName));
            PopulateCollectionFromSource(UndefinedPrograms, _dataService.AllUndefinedPrograms, p => CurrentSearchMode == SearchMode.Name ? namePredicate(p.Name) : pathPredicate(p.ExePath));
            PopulateCollectionFromSource(AdvancedRules, _dataService.AllAdvancedRules, r => namePredicate(r.Name) || namePredicate(r.Description));
            PopulateCollectionFromSource(ForeignRules, _dataService.AllForeignRules, r => namePredicate(r.Name) || namePredicate(r.Description));
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
                case "ForeignRulesListView": collection = ForeignRules; break;
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