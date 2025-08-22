// File: MainForm.cs
using DarkModeForms;
using NetFwTypeLib;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms.VisualStyles;
using MinimalFirewall.Groups;
using Firewall.Traffic.ViewModels;
using System.Collections.Specialized;
using MinimalFirewall.TypedObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Text;

namespace MinimalFirewall
{
    public partial class MainForm : Form
    {
        #region Fields
        private readonly INetFwPolicy2 _firewallPolicy;
        private readonly FirewallDataService _dataService;
        private readonly FirewallActionsService _actionsService;
        private readonly FirewallEventListenerService _eventListenerService;
        private readonly FirewallSentryService _firewallSentryService;
        private readonly FirewallRuleService _firewallRuleService;
        private readonly UserActivityLogger _activityLogger;
        private readonly WildcardRuleService _wildcardRuleService;
        private readonly ForeignRuleTracker _foreignRuleTracker;
        private readonly AppSettings _appSettings;
        private readonly StartupService _startupService;
        private readonly RuleCacheService _ruleCacheService;
        private readonly FirewallGroupManager _groupManager;
        private readonly IconService _iconService;
        private readonly PublisherWhitelistService _whitelistService;
        private readonly List<PendingConnectionViewModel> _pendingConnections = [];
        private readonly Queue<PendingConnectionViewModel> _popupQueue = new();
        private volatile bool _isPopupVisible = false;
        private readonly object _popupLock = new();
        private readonly DarkModeCS dm;
        private System.Threading.Timer? _autoRefreshTimer;
        private List<FirewallRuleChange> _masterSystemChanges = [];
        private bool _systemChangesLoaded = false;
        private Image? _lockedGreenIcon;
        private Image? _unlockedWhiteIcon;
        private Image? _refreshWhiteIcon;
        private Rectangle? _hotDashboardButtonBounds;
        private Rectangle? _hotSystemButtonBounds;
        private ListViewItem? _hotDashboardItem = null;
        private ListViewItem? _hotSystemItem = null;
        private ListViewItem? _hoveredItem = null;
        private readonly TrafficMonitorViewModel _trafficMonitorViewModel;

        private int _rulesSortColumn = -1;
        private int _dashboardSortColumn = -1;
        private int _systemChangesSortColumn = -1;
        private int _groupsSortColumn = -1;
        private int _liveConnectionsSortColumn = -1;
        private SortOrder _rulesSortOrder = SortOrder.None;
        private SortOrder _dashboardSortOrder = SortOrder.None;
        private SortOrder _systemChangesSortOrder = SortOrder.None;
        private SortOrder _groupsSortOrder = SortOrder.None;
        private SortOrder _liveConnectionsSortOrder = SortOrder.None;

        private ToolStripMenuItem? lockdownTrayMenuItem;
        private Icon? _defaultTrayIcon;
        private Icon? _unlockedTrayIcon;
        private int _unseenSystemChangesCount = 0;
        private Icon? _alertTrayIcon;
        #endregion

        #region Native Methods
        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetProcessWorkingSetSize(IntPtr process,
            IntPtr minimumWorkingSetSize, IntPtr maximumWorkingSetSize);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        extern static bool DestroyIcon(IntPtr handle);
        #endregion

        #region Constructor and Initialization
        public MainForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            dashboardListView.Resize += ListView_Resize;
            rulesListView.Resize += ListView_Resize;
            systemChangesListView.Resize += ListView_Resize;
            groupsListView.Resize += ListView_Resize;
            liveConnectionsListView.Resize += ListView_Resize;

            this.Opacity = 0;
            this.ShowInTaskbar = false;
            typeof(ListView).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(dashboardListView, true);
            typeof(ListView).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(systemChangesListView, true);
            typeof(ListView).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(rulesListView, true);
            typeof(ListView).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(groupsListView, true);
            typeof(ListView).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(liveConnectionsListView, true);

            this.Text = "Minimal Firewall";
            dm = new DarkModeCS(this);
            if (this.components != null)
            {
                dm.Components = this.components.Components;
            }

            try
            {
                Type? firewallPolicyType = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
                if (firewallPolicyType != null)
                {
                    _firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(firewallPolicyType)!;
                }
                else
                {
                    throw new InvalidOperationException("Firewall policy type could not be retrieved.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not initialize Windows Firewall policy management. The application cannot continue.\n\nError: " + ex.Message, "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
                return;
            }

            _appSettings = AppSettings.Load();
            _startupService = new StartupService();
            _ruleCacheService = new RuleCacheService();
            _groupManager = new FirewallGroupManager(_firewallPolicy);
            _iconService = new IconService { ImageList = this.appIconList };
            _whitelistService = new PublisherWhitelistService();
            versionLabel.Text = "Version " + Assembly.GetExecutingAssembly().GetName()?.Version?.ToString(3);
            _firewallRuleService = new FirewallRuleService(_firewallPolicy);
            _activityLogger = new UserActivityLogger { IsEnabled = _appSettings.IsLoggingEnabled };
            _wildcardRuleService = new WildcardRuleService();
            _foreignRuleTracker = new ForeignRuleTracker();
            _dataService = new FirewallDataService(_firewallRuleService, _wildcardRuleService, _ruleCacheService);
            _firewallSentryService = new FirewallSentryService(_firewallRuleService);
            _trafficMonitorViewModel = new TrafficMonitorViewModel();

            _eventListenerService = new FirewallEventListenerService(_dataService, _wildcardRuleService, IsLockedDown, msg => _activityLogger.LogDebug(msg), _appSettings, _whitelistService);
            _actionsService = new FirewallActionsService(_firewallRuleService, _activityLogger, _eventListenerService, _foreignRuleTracker, _firewallSentryService, _whitelistService, _firewallPolicy);
            _eventListenerService.ActionsService = _actionsService;

            _firewallSentryService.RuleSetChanged += OnRuleSetChanged;
            _eventListenerService.PendingConnectionDetected += OnPendingConnectionDetected;
            _trafficMonitorViewModel.ActiveConnections.CollectionChanged += ActiveConnections_CollectionChanged;

            rulesListView.SmallImageList = this.appIconList;
            dashboardListView.SmallImageList = this.appIconList;
            liveConnectionsListView.SmallImageList = this.appIconList;

            SetupTrayIcon();
        }

        private void ListView_Resize(object? sender, EventArgs e)
        {
            if (sender is not ListView listView || listView.Columns.Count == 0 || listView.ClientSize.Width == 0)
            {
                return;
            }

            ColumnHeader lastColumn = listView.Columns[^1];

            int totalWidthOfOtherColumns = 0;
            for (int i = 0; i < listView.Columns.Count - 1; i++)
            {
                totalWidthOfOtherColumns += listView.Columns[i].Width;
            }

            int newLastColumnWidth = listView.ClientSize.Width - totalWidthOfOtherColumns;
            const int minColumnWidth = 100;
            if (newLastColumnWidth < minColumnWidth)
            {
                newLastColumnWidth = minColumnWidth;
            }

            lastColumn.Width = newLastColumnWidth;
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            using var statusForm = new StatusForm("Initializing...");
            statusForm.StartPosition = FormStartPosition.CenterScreen;
            statusForm.Show();
            Application.DoEvents();

            DarkModeCS.ExcludeFromProcessing(rescanButton);
            rescanButton.BackColor = Color.Transparent;
            DarkModeCS.ExcludeFromProcessing(lockdownButton);
            lockdownButton.BackColor = Color.Transparent;

            SetupAppIcons();
            await _ruleCacheService.LoadCacheFromDiskAsync();
            if (IsLockedDown())
            {
                AdminTaskService.SetAuditPolicy(true);
            }

            await Task.Run(() =>
            {
                _eventListenerService.Start();
                _firewallSentryService.Start();
            });

            await CheckForInitialSystemChangesAsync();

            UpdateTrayStatus();
            _activityLogger.LogDebug("Application Started: " + versionLabel.Text);

            await ForceDataRefreshAsync(true, false);

            LoadSettingsToUI();
            SetupAutoRefreshTimer();
            await DisplayCurrentTabData();
            UpdateThemeAndColors();
            UpdateIconColumnVisibility();

            ApplyLastWindowState();

            this.Opacity = 1;
            this.ShowInTaskbar = true;
            this.Activate();
        }

        private Icon CreateRecoloredIcon(Icon originalIcon, Color color)
        {
            using var bmp = originalIcon.ToBitmap();
            using var recoloredImage = RecolorImage(bmp, color);
            IntPtr hIcon = ((Bitmap)recoloredImage).GetHicon();
            try
            {
                using var newIcon = Icon.FromHandle(hIcon);
                return (Icon)newIcon.Clone();
            }
            finally
            {
                DestroyIcon(hIcon);
            }
        }

        private void SetupAppIcons()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("MinimalFirewall.logo.ico"))
            {
                if (stream != null)
                {
                    var icon = new Icon(stream);
                    this.Icon = icon;
                    _defaultTrayIcon = icon;
                    _unlockedTrayIcon = CreateRecoloredIcon(icon, Color.Red);
                    _alertTrayIcon = CreateRecoloredIcon(icon, Color.Orange);

                    if (notifyIcon != null)
                    {
                        notifyIcon.Icon = IsLockedDown() ? _defaultTrayIcon : _unlockedTrayIcon;
                    }
                }
            }

            appImageList.ImageSize = new Size(32, 32);
            mainTabControl.ImageList = appImageList;

            Image? lockedIcon = appImageList.Images["locked.png"];
            if (lockedIcon != null)
            {
                _lockedGreenIcon = DarkModeCS.RecolorImage(lockedIcon, Color.FromArgb(0, 200, 83));
            }
            Image? unlockedIcon = appImageList.Images["unlocked.png"];
            if (unlockedIcon != null)
            {
                _unlockedWhiteIcon = DarkModeCS.RecolorImage(unlockedIcon, Color.White);
            }
            Image? refreshIcon = appImageList.Images["refresh.png"];
            if (refreshIcon != null)
            {
                _refreshWhiteIcon = DarkModeCS.RecolorImage(refreshIcon, Color.White);
            }

            lockdownButton!.Text = string.Empty;
            rescanButton.Text = string.Empty;
            lockdownButton.AutoSize = false;
            lockdownButton.Size = new Size(40, 40);
            rescanButton.AutoSize = false;
            rescanButton.Size = new Size(40, 40);
            using (var stream = assembly.GetManifestResourceStream("MinimalFirewall.logo.png"))
            {
                if (stream != null)
                {
                    logoPictureBox.Image = Image.FromStream(stream);
                }
            }
        }
        #endregion

        #region Settings and Theme
        private void DarkModeSwitch_CheckedChanged(object sender, EventArgs e)
        {
            _appSettings.Theme = darkModeSwitch.Checked ? "Dark" : "Light";
            UpdateThemeAndColors();
        }

        private void UpdateThemeAndColors()
        {
            this.SuspendLayout();
            bool isDark = _appSettings.Theme == "Dark";
            dm.ColorMode = isDark ? DarkModeCS.DisplayMode.DarkMode : DarkModeCS.DisplayMode.ClearMode;
            dm.ApplyTheme(isDark);

            var linkColor = isDark ? Color.SkyBlue : SystemColors.HotTrack;
            helpLink.LinkColor = linkColor;
            reportProblemLink.LinkColor = linkColor;
            forumLink.LinkColor = linkColor;
            coffeeLinkLabel.LinkColor = linkColor;

            helpLink.VisitedLinkColor = linkColor;
            reportProblemLink.VisitedLinkColor = linkColor;
            forumLink.VisitedLinkColor = linkColor;
            coffeeLinkLabel.VisitedLinkColor = linkColor;

            Image? coffeeImage = appImageList.Images["coffee.png"];
            if (coffeeImage != null)
            {
                Color coffeeColor = isDark ? Color.LightGray : Color.Black;
                Image? oldImage = coffeePictureBox.Image;
                coffeePictureBox.Image = DarkModeCS.RecolorImage(coffeeImage, coffeeColor);
                oldImage?.Dispose();
            }

            rescanButton.Image = isDark ? _refreshWhiteIcon ?? appImageList.Images["refresh.png"] : appImageList.Images["refresh.png"];
            UpdateTrayStatus();
            this.ResumeLayout(true);
            this.Refresh();
            lockdownButton.FlatAppearance.BorderColor = this.BackColor;
            rescanButton.FlatAppearance.BorderColor = this.BackColor;
        }

        private void LoadSettingsToUI()
        {
            closeToTraySwitch.Checked = _appSettings.CloseToTray;
            startOnStartupSwitch.Checked = _appSettings.StartOnSystemStartup;
            darkModeSwitch.Checked = _appSettings.Theme == "Dark";
            popupsSwitch.Checked = _appSettings.IsPopupsEnabled;
            loggingSwitch.Checked = _appSettings.IsLoggingEnabled;
            autoRefreshTextBox.Text = _appSettings.AutoRefreshIntervalMinutes.ToString();
            trafficMonitorSwitch.Checked = _appSettings.IsTrafficMonitorEnabled;
            showAppIconsSwitch.Checked = _appSettings.ShowAppIcons;
            autoAllowSystemTrustedCheck.Checked = _appSettings.AutoAllowSystemTrusted;
            auditAlertsSwitch.Checked = _appSettings.AlertOnForeignRules;
            managePublishersButton.Enabled = true;
        }

        private void SaveSettingsFromUI()
        {
            _appSettings.CloseToTray = closeToTraySwitch.Checked;
            _appSettings.StartOnSystemStartup = startOnStartupSwitch.Checked;
            _appSettings.Theme = darkModeSwitch.Checked ? "Dark" : "Light";
            _appSettings.IsPopupsEnabled = popupsSwitch.Checked;
            _appSettings.IsLoggingEnabled = loggingSwitch.Checked;
            if (int.TryParse(autoRefreshTextBox.Text, out int val) && val >= 1)
            {
                _appSettings.AutoRefreshIntervalMinutes = val;
            }
            _appSettings.IsTrafficMonitorEnabled = trafficMonitorSwitch.Checked;
            _appSettings.ShowAppIcons = showAppIconsSwitch.Checked;
            _appSettings.AutoAllowSystemTrusted = autoAllowSystemTrustedCheck.Checked;
            _appSettings.AlertOnForeignRules = auditAlertsSwitch.Checked;

            _startupService.SetStartup(_appSettings.StartOnSystemStartup);
            _activityLogger.IsEnabled = _appSettings.IsLoggingEnabled;
            if (_appSettings.IsTrafficMonitorEnabled)
            {
                _trafficMonitorViewModel.StartMonitoring();
            }
            else
            {
                _trafficMonitorViewModel.StopMonitoring();
            }

            UpdateIconColumnVisibility();
            _appSettings.Save();
        }

        private void managePublishersButton_Click(object sender, EventArgs e)
        {
            using var form = new ManagePublishersForm(_whitelistService);
            form.ShowDialog(this);
        }

        private void PopupsSwitch_CheckedChanged(object sender, EventArgs e)
        {
            if (_appSettings != null)
            {
                _appSettings.IsPopupsEnabled = popupsSwitch.Checked;
            }
        }

        private void TrafficMonitorSwitch_CheckedChanged(object sender, EventArgs e)
        {
            _appSettings.IsTrafficMonitorEnabled = trafficMonitorSwitch.Checked;
            liveConnectionsListView.Visible = _appSettings.IsTrafficMonitorEnabled;
            liveConnectionsDisabledLabel.Visible = !_appSettings.IsTrafficMonitorEnabled;

            if (_appSettings.IsTrafficMonitorEnabled)
            {
                if (mainTabControl.SelectedTab == liveConnectionsTabPage)
                {
                    _trafficMonitorViewModel.StartMonitoring();
                }
            }
            else
            {
                _trafficMonitorViewModel.StopMonitoring();
            }
        }

        private void ShowAppIconsSwitch_CheckedChanged(object sender, EventArgs e)
        {
            _appSettings.ShowAppIcons = showAppIconsSwitch.Checked;
            UpdateIconColumnVisibility();
            DisplayCurrentTabData();
        }

        private void UpdateIconColumnVisibility()
        {
            advIconColumn.Width = _appSettings.ShowAppIcons ? 32 : 0;
            dashIconColumn.Width = _appSettings.ShowAppIcons ? 32 : 0;
            liveIconColumn.Width = _appSettings.ShowAppIcons ? 32 : 0;
        }
        #endregion

        #region Core Logic and Backend Event Handlers
        public bool IsLockedDown() => _firewallRuleService.GetDefaultOutboundAction() == NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
        private void UpdateTrayStatus()
        {
            bool locked = IsLockedDown();

            logoPictureBox.Visible = !locked;
            dashboardListView.Visible = locked;

            if (locked)
            {
                lockdownButton.Image = _lockedGreenIcon;
            }
            else
            {
                lockdownButton.Image = (_appSettings.Theme == "Dark") ? _unlockedWhiteIcon ?? appImageList.Images["unlocked.png"] : appImageList.Images["unlocked.png"];
            }

            if (notifyIcon != null)
            {
                if (locked)
                {
                    notifyIcon.Icon = _defaultTrayIcon;
                }
                else if (_appSettings.AlertOnForeignRules && _unseenSystemChangesCount > 0)
                {
                    notifyIcon.Icon = _alertTrayIcon;
                }
                else
                {
                    notifyIcon.Icon = _unlockedTrayIcon;
                }
            }
        }

        private void OnRuleSetChanged()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(OnRuleSetChanged));
                return;
            }

            OnRuleSetChanged_LiveUpdate();
        }

        private async void OnRuleSetChanged_LiveUpdate()
        {
            if (!_appSettings.AlertOnForeignRules)
            {
                _systemChangesLoaded = false;
                return;
            }

            if (mainTabControl.SelectedTab == systemChangesTabPage && this.ContainsFocus)
            {
                _systemChangesLoaded = false;
                return;
            }

            _activityLogger.LogDebug("Sentry: Firewall rule change detected. Checking for foreign rules.");
            var changes = await Task.Run(() => _firewallSentryService.CheckForChanges(_foreignRuleTracker));
            if (changes.Count > 0)
            {
                var newChanges = changes.Where(c => !_masterSystemChanges.Any(mc => mc.Rule.Name == c.Rule.Name)).ToList();
                if (newChanges.Any())
                {
                    _unseenSystemChangesCount += newChanges.Count;
                    _masterSystemChanges.AddRange(newChanges);
                    _systemChangesLoaded = true;
                    UpdateUiWithChangesCount();
                }
            }
        }

        private async Task CheckForInitialSystemChangesAsync()
        {
            if (!_appSettings.AlertOnForeignRules) return;

            _activityLogger.LogDebug("Performing initial check for system changes on startup.");
            var changes = await Task.Run(() => _firewallSentryService.CheckForChanges(_foreignRuleTracker));
            if (changes.Count > 0)
            {
                _unseenSystemChangesCount = changes.Count;
                _masterSystemChanges = changes;
                _systemChangesLoaded = true;
                UpdateUiWithChangesCount();
            }
        }

        private void UpdateUiWithChangesCount()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(UpdateUiWithChangesCount));
                return;
            }

            if (_appSettings.AlertOnForeignRules && _unseenSystemChangesCount > 0)
            {
                systemChangesTabPage.Text = "Audit";
                dm.SetNotificationCount(systemChangesTabPage, _unseenSystemChangesCount);
            }
            else
            {
                systemChangesTabPage.Text = "Audit";
                dm.SetNotificationCount(systemChangesTabPage, 0);
            }
            UpdateTrayStatus();
        }


        private void OnPendingConnectionDetected(PendingConnectionViewModel pending)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(() => OnPendingConnectionDetected(pending));
                return;
            }

            var matchingRule = _wildcardRuleService.Match(pending.AppPath);
            if (matchingRule != null)
            {
                if (matchingRule.Action.StartsWith("Allow", StringComparison.OrdinalIgnoreCase))
                {
                    _actionsService.ApplyApplicationRuleChange([pending.AppPath], matchingRule.Action, matchingRule.FolderPath);
                    return;
                }
            }

            bool alreadyPending = _pendingConnections.Any(p => p.AppPath.Equals(pending.AppPath, StringComparison.OrdinalIgnoreCase)) ||
                                  _popupQueue.Any(p => p.AppPath.Equals(pending.AppPath, StringComparison.OrdinalIgnoreCase));

            if (alreadyPending)
            {
                _activityLogger.LogDebug($"Ignoring duplicate pending connection for {pending.AppPath}");
                return;
            }

            if (_appSettings.IsPopupsEnabled)
            {
                lock (_popupLock)
                {
                    _popupQueue.Enqueue(pending);
                }
                BeginInvoke(new Action(ProcessNextPopup));
            }
            else
            {
                _pendingConnections.Add(pending);
                ApplyDashboardSearchFilter();
            }
        }

        private void ActiveConnections_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (liveConnectionsListView is null || !this.Visible || IsDisposed) return;
            if (liveConnectionsListView.InvokeRequired)
            {
                liveConnectionsListView.Invoke(new Action(() => ActiveConnections_CollectionChanged(sender, e)));
                return;
            }

            liveConnectionsListView.BeginUpdate();
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                liveConnectionsListView.Items.Clear();
            }

            if (e.OldItems != null)
            {
                foreach (TcpConnectionViewModel oldVm in e.OldItems)
                {
                    var itemToRemove = liveConnectionsListView.Items.Cast<ListViewItem>()
                        .FirstOrDefault(item => item.Tag == oldVm);
                    if (itemToRemove != null)
                    {
                        liveConnectionsListView.Items.Remove(itemToRemove);
                    }
                }
            }

            if (e.NewItems != null)
            {
                foreach (TcpConnectionViewModel newVm in e.NewItems)
                {
                    int iconIndex = -1;
                    if (_appSettings.ShowAppIcons && !string.IsNullOrEmpty(newVm.ProcessPath))
                    {
                        iconIndex = _iconService.GetIconIndex(newVm.ProcessPath);
                    }
                    var newItem = new ListViewItem("", iconIndex)
                    {
                        Tag = newVm
                    };
                    newItem.SubItems.AddRange(new[] { newVm.ProcessName, newVm.RemoteAddress, newVm.RemotePort.ToString() });
                    liveConnectionsListView.Items.Add(newItem);
                }
            }

            liveConnectionsListView.EndUpdate();
        }

        private void ProcessNextPopup()
        {
            lock (_popupLock)
            {
                if (_isPopupVisible || _popupQueue.Count == 0)
                {
                    return;
                }

                _isPopupVisible = true;
                var pending = _popupQueue.Dequeue();

                var notifier = new NotifierForm(pending, _appSettings.Theme == "Dark");
                notifier.FormClosed += Notifier_FormClosed;
                notifier.Show(this);
            }
        }

        private void Notifier_FormClosed(object? sender, FormClosedEventArgs e)
        {
            if (sender is not NotifierForm notifier) return;
            notifier.FormClosed -= Notifier_FormClosed;

            var pending = notifier.PendingConnection;
            var result = notifier.Result;
            if (result == NotifierForm.NotifierResult.CreateWildcard)
            {
                _actionsService.ProcessPendingConnection(pending, "Ignore");
                this.BeginInvoke(new Action(async () =>
                {
                    using var wildcardDialog = new WildcardCreatorForm(_wildcardRuleService, pending.AppPath);
                    if (wildcardDialog.ShowDialog(this) == DialogResult.OK)
                    {
                        await ForceDataRefreshAsync(showStatus: false);
                    }
                }));
            }
            else
            {
                string decision = result.ToString();
                if (result == NotifierForm.NotifierResult.TemporaryAllow)
                {
                    _actionsService.ProcessPendingConnection(pending, "TemporaryAllow", notifier.TemporaryDuration);
                }
                else
                {
                    _actionsService.ProcessPendingConnection(pending, decision, trustPublisher: notifier.TrustPublisher);
                }

                if (decision != "Ignore")
                {
                    _ = ForceDataRefreshAsync(showStatus: false);
                }
            }

            lock (_popupLock)
            {
                _isPopupVisible = false;
            }

            BeginInvoke(new Action(ProcessNextPopup));
        }
        #endregion

        #region System Tray & Lifecycle
        private void SetupTrayIcon()
        {
            lockdownTrayMenuItem = new ToolStripMenuItem("Toggle Lockdown", null, ToggleLockdownTrayMenuItem_Click);

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add(lockdownTrayMenuItem);
            contextMenu.Items.Add(new ToolStripMenuItem("Show", null, ShowWindow));
            contextMenu.Items.Add(new ToolStripMenuItem("Exit", null, ExitApplication));
            contextMenu.Opening += TrayContextMenu_Opening;
            notifyIcon = new NotifyIcon(this.components)
            {
                Icon = this.Icon,
                Text = "Minimal Firewall",
                Visible = false,
                ContextMenuStrip = contextMenu
            };
            notifyIcon.DoubleClick += ShowWindow;
        }

        private void ToggleLockdownTrayMenuItem_Click(object? sender, EventArgs e)
        {
            _actionsService.ToggleLockdown();
            UpdateTrayStatus();
        }

        private void TrayContextMenu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (lockdownTrayMenuItem != null)
            {
                lockdownTrayMenuItem.Text = IsLockedDown() ? "Disable Lockdown" : "Enable Lockdown";
            }
        }

        private void SetupAutoRefreshTimer()
        {
            _autoRefreshTimer?.Dispose();
            if (_appSettings.AutoRefreshIntervalMinutes > 0)
            {
                var interval = TimeSpan.FromMinutes(_appSettings.AutoRefreshIntervalMinutes);
                _autoRefreshTimer = new System.Threading.Timer(_ =>
                {
                    if (this.IsDisposed || !this.IsHandleCreated)
                    {
                        return;
                    }

                    try
                    {
                        this.Invoke(new Action(async () =>
                        {
                            if (this.Visible && (mainTabControl.SelectedTab?.Name is "rulesTabPage"))
                            {
                                await ForceDataRefreshAsync();
                            }
                        }));
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                }, null, interval, interval);
                _activityLogger.LogDebug($"Auto-refresh timer set to {_appSettings.AutoRefreshIntervalMinutes} minutes.");
            }
        }

        private void ApplyLastWindowState()
        {
            if (_appSettings.WindowSize.Width > 0 && _appSettings.WindowSize.Height > 0)
            {
                this.Size = _appSettings.WindowSize;
            }

            bool isVisible = false;
            foreach (Screen screen in Screen.AllScreens)
            {
                if (screen.WorkingArea.Contains(_appSettings.WindowLocation))
                {
                    isVisible = true;
                    break;
                }
            }
            if (isVisible)
            {
                this.Location = _appSettings.WindowLocation;
            }
            else
            {
                this.StartPosition = FormStartPosition.CenterScreen;
            }

            FormWindowState savedState = (FormWindowState)_appSettings.WindowState;
            if (savedState == FormWindowState.Minimized)
            {
                savedState = FormWindowState.Normal;
            }

            this.WindowState = savedState;
        }

        private void ShowWindow(object? sender, EventArgs e)
        {
            this.Show();
            ApplyLastWindowState();
            if (notifyIcon != null)
            {
                notifyIcon.Visible = false;
            }
            this.Activate();
            if (_appSettings.IsTrafficMonitorEnabled)
            {
                _trafficMonitorViewModel.StartMonitoring();
            }
            _eventListenerService.Start();
            _firewallSentryService.Start();
            SetupAutoRefreshTimer();
            RefreshOnRestoreAsync(sender, e);
        }

        private async void RefreshOnRestoreAsync(object? sender, EventArgs e)
        {
            _firewallSentryService.CreateBaseline();
            await _ruleCacheService.LoadCacheFromDiskAsync();
            await ForceDataRefreshAsync();
        }

        private void ExitApplication(object? sender, EventArgs e)
        {
            Application.Exit();
        }

        private async void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                _appSettings.WindowLocation = this.RestoreBounds.Location;
                _appSettings.WindowSize = this.RestoreBounds.Size;
                _appSettings.WindowState = (int)FormWindowState.Maximized;
            }
            else if (this.WindowState == FormWindowState.Normal)
            {
                _appSettings.WindowLocation = this.Location;
                _appSettings.WindowSize = this.Size;
                _appSettings.WindowState = (int)FormWindowState.Normal;
            }
            else
            {
                _appSettings.WindowLocation = this.RestoreBounds.Location;
                _appSettings.WindowSize = this.RestoreBounds.Size;
                _appSettings.WindowState = (int)FormWindowState.Normal;
            }

            SaveSettingsFromUI();
            if (_appSettings.CloseToTray && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                if (notifyIcon != null)
                {
                    notifyIcon.Visible = true;
                }
                await PrepareForTrayAsync();
            }
            else
            {
                _firewallSentryService.RuleSetChanged -= OnRuleSetChanged;
                _eventListenerService.PendingConnectionDetected -= OnPendingConnectionDetected;
                _trafficMonitorViewModel.ActiveConnections.CollectionChanged -= ActiveConnections_CollectionChanged;
                Application.Exit();
            }
        }

        public async Task PrepareForTrayAsync()
        {
            _firewallSentryService.Stop();
            _trafficMonitorViewModel.StopMonitoring();
            _autoRefreshTimer?.Dispose();

            _pendingConnections.Clear();
            _masterSystemChanges.Clear();
            rulesListView.Items.Clear();
            dashboardListView.Items.Clear();
            systemChangesListView.Items.Clear();
            liveConnectionsListView.Items.Clear();
            _iconService.ClearCache();
            _dataService.ClearLocalCaches();

            SystemDiscoveryService.ClearCache();
            _firewallSentryService.ClearBaseline();
            await _ruleCacheService.PersistCacheToDiskAsync(clearMemoryCache: true);

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            GC.WaitForPendingFinalizers();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
            }
        }
        #endregion

        #region Tab Loading and Filtering
        private async Task DisplayCurrentTabData()
        {
            if (mainTabControl is null) return;
            var selectedTab = mainTabControl.SelectedTab;
            if (selectedTab == null) return;

            this.SuspendLayout();

            if (selectedTab != liveConnectionsTabPage)
            {
                _trafficMonitorViewModel.StopMonitoring();
            }

            switch (selectedTab.Name)
            {
                case "dashboardTabPage":
                    ApplyDashboardSearchFilter();
                    break;
                case "rulesTabPage":
                    await DisplayRulesAsync();
                    break;
                case "systemChangesTabPage":
                    ApplySystemChangesSearchFilter();
                    break;
                case "groupsTabPage":
                    await DisplayGroupsAsync();
                    break;
                case "liveConnectionsTabPage":
                    liveConnectionsListView.Visible = _appSettings.IsTrafficMonitorEnabled;
                    liveConnectionsDisabledLabel.Visible = !_appSettings.IsTrafficMonitorEnabled;
                    if (_appSettings.IsTrafficMonitorEnabled)
                    {
                        _trafficMonitorViewModel.StartMonitoring();
                    }
                    else
                    {
                        liveConnectionsListView.Items.Clear();
                    }
                    break;
            }
            this.ResumeLayout(true);
        }

        private async Task ForceDataRefreshAsync(bool forceFullScan = false, bool showStatus = true)
        {
            StatusForm? statusForm = null;
            if (showStatus && this.Visible)
            {
                statusForm = new StatusForm("Refreshing Data...");
                statusForm.Show(this);
                Application.DoEvents();
            }

            try
            {
                await _dataService.RefreshAndCacheAsync(forceFullScan);
                _systemChangesLoaded = false;
                if (this.Visible)
                {
                    await DisplayCurrentTabData();
                }
            }
            finally
            {
                statusForm?.Close();
                statusForm?.Dispose();
            }
        }

        private async Task DisplayRulesAsync()
        {
            var rules = _dataService.GetAggregatedAdvancedRules();
            ApplyRulesFilters(rules);
            await Task.CompletedTask;
        }

        private async Task DisplayGroupsAsync()
        {
            if (groupsListView is null) return;
            var groups = await Task.Run(() => _groupManager.GetAllGroups());
            groupsListView.BeginUpdate();
            groupsListView.Items.Clear();
            foreach (var group in groups)
            {
                var item = new ListViewItem(new[] { group.Name, "" })
                {
                    Tag = group,
                };
                groupsListView.Items.Add(item);
            }
            groupsListView.EndUpdate();
        }

        private void ApplyRulesFilters(List<AggregatedRuleViewModel> rules)
        {
            var enabledTypes = new HashSet<RuleType> { RuleType.Program };
            if (advFilterServiceCheck.Checked) enabledTypes.Add(RuleType.Service);
            if (advFilterUwpCheck.Checked) enabledTypes.Add(RuleType.UWP);
            if (advFilterWildcardCheck.Checked) enabledTypes.Add(RuleType.Wildcard);
            if (advFilterAdvancedCheck.Checked) enabledTypes.Add(RuleType.Advanced);

            rulesListView.BeginUpdate();
            rulesListView.Items.Clear();
            if (enabledTypes.Count > 0)
            {
                var filteredRules = rules.Where(r => enabledTypes.Contains(r.Type));
                if (!string.IsNullOrWhiteSpace(rulesSearchTextBox.Text))
                {
                    filteredRules = filteredRules.Where(r =>
                        r.Name.Contains(rulesSearchTextBox.Text, StringComparison.OrdinalIgnoreCase) ||
                        r.Description.Contains(rulesSearchTextBox.Text, StringComparison.OrdinalIgnoreCase) ||
                        r.ApplicationName.Contains(rulesSearchTextBox.Text, StringComparison.OrdinalIgnoreCase));
                }

                foreach (var rule in filteredRules)
                {
                    int iconIndex = -1;
                    if (_appSettings.ShowAppIcons && !string.IsNullOrEmpty(rule.ApplicationName))
                    {
                        iconIndex = _iconService.GetIconIndex(rule.ApplicationName);
                    }

                    var item = new ListViewItem("", iconIndex) { Tag = rule };
                    item.SubItems.AddRange(new[]
                    {
                        rule.Name,
                        rule.IsEnabled.ToString(),
                        rule.Status,
                        rule.Direction.ToString(),
                        rule.ProtocolName,
                        rule.LocalPorts.Count > 0 ? string.Join(",", rule.LocalPorts) : "Any",
                        rule.RemotePorts.Count > 0 ? string.Join(",", rule.RemotePorts) : "Any",
                        rule.LocalAddresses.Count > 0 ? string.Join(",", rule.LocalAddresses) : "Any",
                        rule.RemoteAddresses.Count > 0 ? string.Join(",", rule.RemoteAddresses) : "Any",
                        rule.ApplicationName,
                        rule.ServiceName,
                        rule.Profiles,
                        rule.Grouping,
                        rule.Description
                    });
                    rulesListView.Items.Add(item);
                }
            }
            rulesListView.EndUpdate();
        }

        private void ApplyDashboardSearchFilter()
        {
            if (dashboardListView is null) return;
            var itemsToShow = _pendingConnections;
            dashboardListView.BeginUpdate();
            dashboardListView.Items.Clear();

            foreach (var pending in itemsToShow)
            {
                int iconIndex = -1;
                if (_appSettings.ShowAppIcons && !string.IsNullOrEmpty(pending.AppPath))
                {
                    iconIndex = _iconService.GetIconIndex(pending.AppPath);
                }
                var item = new ListViewItem("", iconIndex) { Tag = pending };
                item.SubItems.AddRange(new[] { "", pending.FileName, pending.ServiceName, pending.Direction, pending.AppPath });
                dashboardListView.Items.Add(item);
            }
            dashboardListView.EndUpdate();
        }

        private async Task ScanForSystemChangesAsync(bool showStatusWindow = false)
        {
            StatusForm? statusWindow = null;
            if (showStatusWindow && this.Visible)
            {
                statusWindow = new StatusForm("Scanning for system changes...");
                statusWindow.Show(this);
                Application.DoEvents();
            }

            _masterSystemChanges = await Task.Run(() => _firewallSentryService.CheckForChanges(_foreignRuleTracker));
            _systemChangesLoaded = true;
            ApplySystemChangesSearchFilter();
            statusWindow?.Close();
        }

        private void ApplySystemChangesSearchFilter()
        {
            if (systemChangesListView is null) return;
            systemChangesListView.BeginUpdate();
            systemChangesListView.Items.Clear();
            string searchText = auditSearchTextBox.Text;

            var filteredChanges = string.IsNullOrWhiteSpace(searchText)
                ? _masterSystemChanges
                : _masterSystemChanges.Where(c => c.Rule?.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true ||
                                                   c.Rule?.Description.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true ||
                                                   c.Rule?.ApplicationName.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true);

            foreach (var change in filteredChanges)
            {
                var item = new ListViewItem(
                    new[]
                    {
                        "",
                        change.Type.ToString(),
                        change.Rule?.Name ?? "N/A",
                        change.Rule?.Status,
                        change.Rule?.Direction.ToString(),
                        change.Rule?.ProtocolName,
                        change.Rule?.ApplicationName,
                        change.Rule?.RemoteAddresses.Any() == true ? string.Join(",", change.Rule.RemoteAddresses) : "Any",
                        change.Rule?.Description
                    })
                {
                    Tag = change
                };
                systemChangesListView.Items.Add(item);
            }
            systemChangesListView.EndUpdate();
        }
        #endregion

        #region UI Event Handlers
        private async void MainTabControl_SelectedIndexChanged(object? sender, EventArgs e)
        {
            var selectedTab = mainTabControl.SelectedTab;
            if (selectedTab == null) return;

            if (selectedTab.Name == "systemChangesTabPage")
            {
                if (!_systemChangesLoaded)
                {
                    await ScanForSystemChangesAsync(true);
                }
            }
            else
            {
                await DisplayCurrentTabData();
            }

            if (selectedTab.Name == "settingsTabPage")
            {
                mainTabControl.Focus();
            }
        }

        private async void RescanButton_Click(object? sender, EventArgs e)
        {
            if (mainTabControl.SelectedTab != null)
            {
                _activityLogger.LogDebug($"Rescan triggered for tab: {mainTabControl.SelectedTab.Text}");
            }
            if (mainTabControl.SelectedTab == systemChangesTabPage)
            {
                await ScanForSystemChangesAsync(true);
            }
            else
            {
                await ForceDataRefreshAsync(true);
            }
        }

        private void ToggleLockdownButton_Click(object sender, EventArgs e)
        {
            bool wasLocked = IsLockedDown();
            _actionsService.ToggleLockdown();
            UpdateTrayStatus();

            bool isNowLocked = IsLockedDown();
            if (wasLocked && !isNowLocked)
            {
                DismissAllPopups();
            }
        }

        private void DismissAllPopups()
        {
            foreach (var form in Application.OpenForms.OfType<NotifierForm>().ToList())
            {
                form.Close();
            }

            lock (_popupLock)
            {
                _popupQueue.Clear();
                _isPopupVisible = false;
            }
        }

        private async void CreateRuleButton_Click(object sender, EventArgs e)
        {
            using var dialog = new AddRuleSelectionForm(_actionsService, _wildcardRuleService);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                await ForceDataRefreshAsync();
            }
        }

        private async void AdvancedRuleButton_Click(object sender, EventArgs e)
        {
            using var dialog = new CreateAdvancedRuleForm(_firewallPolicy, _actionsService);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                await ForceDataRefreshAsync();
            }
        }

        private async void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            await DisplayCurrentTabData();
        }

        private void OpenFirewallButton_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("wf.msc");
            }
            catch (Exception ex)
            {
                Messenger.MessageBox($"Could not open Windows Firewall console.\n\nError: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (sender is not LinkLabel { Tag: string url }) return;
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Messenger.MessageBox($"Could not open the link.\n\nError: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CoffeeLink_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo("https://www.buymeacoffee.com/deminimis") { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Messenger.MessageBox($"Could not open the link.\n\nError: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CoffeePictureBox_MouseEnter(object? sender, EventArgs e)
        {
        }

        private void CoffeePictureBox_MouseLeave(object? sender, EventArgs e)
        {
        }

        private void LockdownButton_MouseEnter(object? sender, EventArgs e)
        {
            lockdownButton.FlatAppearance.BorderColor = dm.OScolors.Accent;
        }

        private void LockdownButton_MouseLeave(object? sender, EventArgs e)
        {
            lockdownButton.FlatAppearance.BorderColor = this.BackColor;
        }

        private void RescanButton_MouseEnter(object? sender, EventArgs e)
        {
            rescanButton.FlatAppearance.BorderColor = dm.OScolors.Accent;
        }

        private void RescanButton_MouseLeave(object? sender, EventArgs e)
        {
            rescanButton.FlatAppearance.BorderColor = this.BackColor;
        }

        private void CheckForUpdatesButton_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo("https://github.com/deminimis/minimalfirewall/releases") { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Messenger.MessageBox($"Could not open the link.\n\nError: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void RebuildBaselineButton_Click(object sender, EventArgs e)
        {
            var result = Messenger.MessageBox("This will clear all acknowledged rules and rebuild the firewall baseline from its current state. Are you sure?", "Rebuild Baseline", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result != DialogResult.Yes) return;

            _foreignRuleTracker.Clear();
            _firewallSentryService.CreateBaseline();
            await ScanForSystemChangesAsync();
            _activityLogger.LogChange("Sentry", "Firewall baseline rebuilt.");
        }

        private async void AdvFilter_CheckedChanged(object sender, EventArgs e)
        {
            await DisplayRulesAsync();
        }

        private void ArrowPictureBox_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Color arrowColor = (_appSettings.Theme == "Dark") ? Color.White : Color.Black;
            using (var arrowPen = new Pen(arrowColor, 2.5f))
            {
                arrowPen.EndCap = LineCap.ArrowAnchor;
                Point startPoint = new(arrowPictureBox.Width - 1, 0);
                Point endPoint = new(5, arrowPictureBox.Height - 5);
                Point controlPoint1 = new(arrowPictureBox.Width - 5, arrowPictureBox.Height / 2);
                Point controlPoint2 = new(arrowPictureBox.Width / 2, arrowPictureBox.Height);
                e.Graphics.DrawBezier(arrowPen, startPoint, controlPoint1, controlPoint2, endPoint);
            }
        }

        #endregion

        #region DPI Scaling Helper
        private int Scale(int value, Graphics g) => (int)(value * (g.DpiX / 96f));
        #endregion

        #region ListView Custom Drawing
        private void RulesListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = false;
        }

        private void RulesListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            Color backColor;
            Color foreColor;

            if (e.Item.Selected)
            {
                backColor = e.Item.ListView.Focused ? SystemColors.Highlight : SystemColors.ControlDark;
                foreColor = e.Item.ListView.Focused ? SystemColors.HighlightText : SystemColors.ControlText;
            }
            else
            {
                string statusText = string.Empty;
                if (e.Item.Tag is AdvancedRuleViewModel advancedRule)
                {
                    statusText = advancedRule.Status;
                }

                bool isAllow = statusText.Contains("Allow", StringComparison.OrdinalIgnoreCase);
                bool isBlock = statusText.Contains("Block", StringComparison.OrdinalIgnoreCase);

                if (isAllow && isBlock)
                {
                    backColor = Color.FromArgb(255, 255, 204);
                    foreColor = Color.Black;
                }
                else if (isAllow)
                {
                    backColor = Color.FromArgb(204, 255, 204);
                    foreColor = Color.Black;
                }
                else if (isBlock)
                {
                    backColor = Color.FromArgb(255, 204, 204);
                    foreColor = Color.Black;
                }
                else
                {
                    backColor = e.Item.ListView.BackColor;
                    foreColor = e.Item.ListView.ForeColor;
                }
            }

            e.Graphics.FillRectangle(new SolidBrush(backColor), e.Bounds);
            if (_hoveredItem == e.Item && !e.Item.Selected)
            {
                using var overlayBrush = new SolidBrush(Color.FromArgb(25, Color.Black));
                e.Graphics.FillRectangle(overlayBrush, e.Bounds);
            }

            if (e.ColumnIndex == 0)
            {
                if (e.Item.ImageIndex != -1 && e.Item.ImageList != null)
                {
                    Image img = e.Item.ImageList.Images[e.Item.ImageIndex];
                    int imgX = e.Bounds.Left + (e.Bounds.Width - img.Width) / 2;
                    int imgY = e.Bounds.Top + (e.Bounds.Height - img.Height) / 2;
                    e.Graphics.DrawImage(img, imgX, imgY);
                }
                return;
            }

            TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.PathEllipsis;
            TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.SubItem.Font, e.Bounds, foreColor, flags);
        }

        private void ListView_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender is not ListView listView) return;
            ListViewItem? itemUnderMouse = listView.GetItemAt(e.X, e.Y);

            if (_hoveredItem != itemUnderMouse)
            {
                if (_hoveredItem != null)
                {
                    listView.Invalidate(_hoveredItem.Bounds);
                }
                _hoveredItem = itemUnderMouse;
                if (_hoveredItem != null)
                {
                    listView.Invalidate(_hoveredItem.Bounds);
                }
            }
        }

        private void ListView_MouseLeave(object sender, EventArgs e)
        {
            if (_hoveredItem != null)
            {
                if (sender is ListView listView)
                {
                    listView.Invalidate(_hoveredItem.Bounds);
                }
                _hoveredItem = null;
            }
        }

        private void ButtonListView_DrawItem(object? sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = false;
        }

        private void ButtonListView_DrawSubItem(object? sender, DrawListViewSubItemEventArgs e)
        {
            if (e.Item == null) return;
            e.DrawDefault = false;
            Color backColor;
            Color foreColor;

            if (e.Item.Selected)
            {
                backColor = e.Item.ListView.Focused ? SystemColors.Highlight : SystemColors.ControlDark;
                foreColor = e.Item.ListView.Focused ? SystemColors.HighlightText : SystemColors.ControlText;
            }
            else
            {
                backColor = e.Item.ListView.BackColor;
                foreColor = e.Item.ListView.ForeColor;

                if (e.Item.ListView == systemChangesListView && e.Item.Tag is FirewallRuleChange change && change.Rule != null)
                {
                    switch (change.Type)
                    {
                        case ChangeType.New:
                            backColor = Color.FromArgb(204, 255, 204);
                            foreColor = Color.Black;
                            break;
                        case ChangeType.Modified:
                            if (change.Rule.Status.Contains("Allow", StringComparison.OrdinalIgnoreCase))
                            {
                                backColor = Color.FromArgb(204, 255, 204);
                            }
                            else
                            {
                                backColor = Color.FromArgb(255, 204, 204);
                            }
                            foreColor = Color.Black;
                            break;
                        case ChangeType.Deleted:
                            backColor = Color.FromArgb(255, 204, 204);
                            foreColor = Color.Black;
                            break;
                    }
                }
            }

            e.Graphics.FillRectangle(new SolidBrush(backColor), e.Bounds);
            if (_hoveredItem == e.Item && !e.Item.Selected)
            {
                using var overlayBrush = new SolidBrush(Color.FromArgb(25, Color.Black));
                e.Graphics.FillRectangle(overlayBrush, e.Bounds);
            }

            int buttonColumnIndex = (e.Item.ListView == dashboardListView) ? 1 : 0;
            int iconColumnIndex = 0;

            if (e.ColumnIndex == iconColumnIndex && e.Item.ListView == dashboardListView)
            {
                if (e.Item.ImageIndex != -1 && e.Item.ImageList != null)
                {
                    Image img = e.Item.ImageList.Images[e.Item.ImageIndex];
                    int imgX = e.Bounds.Left + (e.Bounds.Width - img.Width) / 2;
                    int imgY = e.Bounds.Top + (e.Bounds.Height - img.Height) / 2;
                    e.Graphics.DrawImage(img, imgX, imgY);
                }
            }
            else if (e.ColumnIndex == buttonColumnIndex)
            {
                int buttonWidth = Scale(70, e.Graphics);
                int buttonHeight = Scale(22, e.Graphics);
                int buttonSpacing = Scale(5, e.Graphics);
                Point center = new(e.Bounds.Left + buttonSpacing, e.Bounds.Y + (e.Bounds.Height - buttonHeight) / 2);
                if (e.Item.ListView == dashboardListView)
                {
                    var allowButtonRect = new Rectangle(center, new Size(buttonWidth, buttonHeight));
                    var blockButtonRect = new Rectangle(allowButtonRect.Right + buttonSpacing, center.Y, buttonWidth, buttonHeight);
                    var ignoreButtonRect = new Rectangle(blockButtonRect.Right + buttonSpacing, center.Y, buttonWidth, buttonHeight);
                    DrawManualButton(e.Graphics, allowButtonRect, "Allow", _hotDashboardButtonBounds == allowButtonRect ? PushButtonState.Hot : PushButtonState.Normal);
                    DrawManualButton(e.Graphics, blockButtonRect, "Block", _hotDashboardButtonBounds == blockButtonRect ? PushButtonState.Hot : PushButtonState.Normal);
                    DrawManualButton(e.Graphics, ignoreButtonRect, "Ignore", _hotDashboardButtonBounds == ignoreButtonRect ? PushButtonState.Hot : PushButtonState.Normal);
                }
                else
                {
                    var acceptButtonRect = new Rectangle(center, new Size(buttonWidth, buttonHeight));
                    var deleteButtonRect = new Rectangle(acceptButtonRect.Right + buttonSpacing, center.Y, buttonWidth, buttonHeight);
                    var ignoreButtonRect = new Rectangle(deleteButtonRect.Right + buttonSpacing, center.Y, buttonWidth, buttonHeight);
                    DrawManualButton(e.Graphics, acceptButtonRect, "Accept", _hotSystemButtonBounds == acceptButtonRect ? PushButtonState.Hot : PushButtonState.Normal);
                    DrawManualButton(e.Graphics, deleteButtonRect, "Delete", _hotSystemButtonBounds == deleteButtonRect ? PushButtonState.Hot : PushButtonState.Normal);
                    DrawManualButton(e.Graphics, ignoreButtonRect, "Ignore", _hotSystemButtonBounds == ignoreButtonRect ? PushButtonState.Hot : PushButtonState.Normal);
                }
            }
            else
            {
                TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.PathEllipsis;
                TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.SubItem.Font, e.Bounds, foreColor, flags);
            }
        }

        private void DrawManualButton(Graphics g, Rectangle bounds, string text, PushButtonState state)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            bool isDark = _appSettings.Theme == "Dark";
            Color defaultBackColor = isDark ? dm.OScolors.SurfaceDark : ControlPaint.Dark(SystemColors.Window, 0.05f);
            Color textColor = isDark ? dm.OScolors.TextActive : SystemColors.ControlText;
            Color currentBackColor = defaultBackColor;

            if (state == PushButtonState.Hot)
            {
                currentBackColor = isDark ? ControlPaint.Light(defaultBackColor, 0.9f) : ControlPaint.Light(defaultBackColor, 0.2f);
            }
            else if (state == PushButtonState.Pressed)
            {
                currentBackColor = isDark ? ControlPaint.Light(defaultBackColor, 1.2f) : ControlPaint.Dark(defaultBackColor, 0.2f);
            }

            using (var backBrush = new SolidBrush(currentBackColor))
            {
                g.FillRectangle(backBrush, bounds);
            }

            TextRenderer.DrawText(g, text, Font, bounds, textColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private async void DashboardListView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (dashboardListView.FocusedItem != null && dashboardListView.FocusedItem.Bounds.Contains(e.Location))
                {
                    dashboardContextMenu.Show(Cursor.Position);
                }
                return;
            }

            if (e.Button != MouseButtons.Left) return;
            var hitTest = dashboardListView.HitTest(e.Location);
            if (hitTest.Item?.Tag is not PendingConnectionViewModel pending || hitTest.SubItem == null)
            {
                return;
            }

            if (hitTest.Item.SubItems.IndexOf(hitTest.SubItem) == 1)
            {
                int buttonWidth = Scale(70, dashboardListView.CreateGraphics());
                int buttonHeight = Scale(22, dashboardListView.CreateGraphics());
                int buttonSpacing = Scale(5, dashboardListView.CreateGraphics());
                Rectangle bounds = hitTest.SubItem.Bounds;
                Point center = new(bounds.Left + buttonSpacing, bounds.Y + (bounds.Height - buttonHeight) / 2);

                Rectangle allowButtonRect = new(center, new Size(buttonWidth, buttonHeight));
                Rectangle blockButtonRect = new(allowButtonRect.Right + buttonSpacing, center.Y, buttonWidth, buttonHeight);
                Rectangle ignoreButtonRect = new(blockButtonRect.Right + buttonSpacing, center.Y, buttonWidth, buttonHeight);

                string? decision = null;
                if (allowButtonRect.Contains(e.Location)) decision = "Allow";
                else if (blockButtonRect.Contains(e.Location)) decision = "Block";
                else if (ignoreButtonRect.Contains(e.Location)) decision = "Ignore";
                if (decision != null)
                {
                    _actionsService.ProcessPendingConnection(pending, decision);
                    _pendingConnections.Remove(pending);
                    ApplyDashboardSearchFilter();
                    if (decision != "Ignore")
                    {
                        await ForceDataRefreshAsync(showStatus: false);
                    }
                }
            }
        }

        private void DashboardListView_MouseMove(object sender, MouseEventArgs e)
        {
            ListView_MouseMove(sender, e);
            var hitTest = dashboardListView.HitTest(e.Location);
            Rectangle? newHotBounds = null;
            ListViewItem? currentItem = hitTest.Item;
            if (currentItem != null && hitTest.SubItem != null && currentItem.SubItems.IndexOf(hitTest.SubItem) == 1)
            {
                int buttonWidth = Scale(70, dashboardListView.CreateGraphics());
                int buttonHeight = Scale(22, dashboardListView.CreateGraphics());
                int buttonSpacing = Scale(5, dashboardListView.CreateGraphics());
                Rectangle bounds = hitTest.SubItem.Bounds;
                Point center = new(bounds.Left + buttonSpacing, bounds.Y + (bounds.Height - buttonHeight) / 2);

                Rectangle allowButtonRect = new(center, new Size(buttonWidth, buttonHeight));
                if (allowButtonRect.Contains(e.Location)) newHotBounds = allowButtonRect;

                Rectangle blockButtonRect = new(allowButtonRect.Right + buttonSpacing, center.Y, buttonWidth, buttonHeight);
                if (blockButtonRect.Contains(e.Location)) newHotBounds = blockButtonRect;
                Rectangle ignoreButtonRect = new(blockButtonRect.Right + buttonSpacing, center.Y, buttonWidth, buttonHeight);
                if (ignoreButtonRect.Contains(e.Location)) newHotBounds = ignoreButtonRect;
            }

            ListViewItem? newHotItem = newHotBounds.HasValue ? currentItem : null;
            if (_hotDashboardItem != newHotItem || _hotDashboardButtonBounds != newHotBounds)
            {
                ListViewItem? oldHotItem = _hotDashboardItem;
                _hotDashboardButtonBounds = newHotBounds;
                _hotDashboardItem = newHotItem;
                if (oldHotItem != null && oldHotItem.ListView != null)
                {
                    dashboardListView.Invalidate(oldHotItem.SubItems[1].Bounds);
                }
                if (_hotDashboardItem != null)
                {
                    dashboardListView.Invalidate(_hotDashboardItem.SubItems[1].Bounds);
                }
            }
        }

        private void SystemChangesListView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                auditContextMenu?.Show(Cursor.Position);
                return;
            }

            if (e.Button != MouseButtons.Left) return;
            var hitTest = systemChangesListView.HitTest(e.Location);
            if (hitTest.Item?.Tag is not FirewallRuleChange change || hitTest.SubItem == null)
            {
                return;
            }

            if (hitTest.Item.SubItems.IndexOf(hitTest.SubItem) == 0)
            {
                int buttonWidth = Scale(70, systemChangesListView.CreateGraphics());
                int buttonHeight = Scale(22, systemChangesListView.CreateGraphics());
                int buttonSpacing = Scale(5, systemChangesListView.CreateGraphics());
                Rectangle bounds = hitTest.SubItem.Bounds;
                Point center = new(bounds.Left + buttonSpacing, bounds.Y + (bounds.Height - buttonHeight) / 2);
                var acceptButtonRect = new Rectangle(center, new Size(buttonWidth, buttonHeight));
                var deleteButtonRect = new Rectangle(acceptButtonRect.Right + buttonSpacing, center.Y, buttonWidth, buttonHeight);
                var ignoreButtonRect = new Rectangle(deleteButtonRect.Right + buttonSpacing, center.Y, buttonWidth, buttonHeight);
                bool changeHandled = false;
                if (acceptButtonRect.Contains(e.Location))
                {
                    _actionsService.AcceptForeignRule(change);
                    systemChangesListView.Items.Remove(hitTest.Item);
                    _masterSystemChanges.Remove(change);
                    changeHandled = true;
                }
                else if (deleteButtonRect.Contains(e.Location))
                {
                    _actionsService.DeleteForeignRule(change);
                    systemChangesListView.Items.Remove(hitTest.Item);
                    _masterSystemChanges.Remove(change);
                    changeHandled = true;
                }
                else if (ignoreButtonRect.Contains(e.Location))
                {
                    _actionsService.AcknowledgeForeignRule(change);
                    systemChangesListView.Items.Remove(hitTest.Item);
                    _masterSystemChanges.Remove(change);
                    changeHandled = true;
                }

                if (changeHandled)
                {
                    _unseenSystemChangesCount = _masterSystemChanges.Count;
                    UpdateUiWithChangesCount();
                }
            }
        }

        private void SystemChangesListView_MouseMove(object sender, MouseEventArgs e)
        {
            ListView_MouseMove(sender, e);
            var hitTest = systemChangesListView.HitTest(e.Location);
            Rectangle? newHotBounds = null;
            ListViewItem? currentItem = hitTest.Item;
            if (currentItem != null && hitTest.SubItem != null && currentItem.SubItems.IndexOf(hitTest.SubItem) == 0)
            {
                int buttonWidth = Scale(70, systemChangesListView.CreateGraphics());
                int buttonHeight = Scale(22, systemChangesListView.CreateGraphics());
                int buttonSpacing = Scale(5, systemChangesListView.CreateGraphics());
                Rectangle bounds = hitTest.SubItem.Bounds;
                Point center = new(bounds.Left + buttonSpacing, bounds.Y + (bounds.Height - buttonHeight) / 2);
                var acceptButtonRect = new Rectangle(center, new Size(buttonWidth, buttonHeight));
                if (acceptButtonRect.Contains(e.Location)) newHotBounds = acceptButtonRect;
                var deleteButtonRect = new Rectangle(acceptButtonRect.Right + buttonSpacing, center.Y, buttonWidth, buttonHeight);
                if (deleteButtonRect.Contains(e.Location)) newHotBounds = deleteButtonRect;
                var ignoreButtonRect = new Rectangle(deleteButtonRect.Right + buttonSpacing, center.Y, buttonWidth, buttonHeight);
                if (ignoreButtonRect.Contains(e.Location)) newHotBounds = ignoreButtonRect;
            }

            ListViewItem? newHotItem = newHotBounds.HasValue ? currentItem : null;
            if (_hotSystemItem != newHotItem || _hotSystemButtonBounds != newHotBounds)
            {
                ListViewItem? oldHotItem = _hotSystemItem;
                _hotSystemButtonBounds = newHotBounds;
                _hotSystemItem = newHotItem;

                if (oldHotItem != null && oldHotItem.ListView != null)
                {
                    systemChangesListView.Invalidate(oldHotItem.SubItems[0].Bounds);
                }
                if (_hotSystemItem != null)
                {
                    systemChangesListView.Invalidate(_hotSystemItem.SubItems[0].Bounds);
                }
            }
        }

        private void AcceptAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_masterSystemChanges.Count == 0) return;
            var result = Messenger.MessageBox($"Are you sure you want to accept all {_masterSystemChanges.Count} detected changes?", "Confirm Accept All", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                _actionsService.AcceptAllForeignRules(_masterSystemChanges);
                _masterSystemChanges.Clear();
                _unseenSystemChangesCount = 0;
                UpdateUiWithChangesCount();
                ApplySystemChangesSearchFilter();
            }
        }

        private void IgnoreAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_masterSystemChanges.Count == 0) return;
            var result = Messenger.MessageBox($"Are you sure you want to ignore all {_masterSystemChanges.Count} detected changes? They will not be shown again.", "Confirm Ignore All", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                _actionsService.AcknowledgeAllForeignRules(_masterSystemChanges);
                _masterSystemChanges.Clear();
                _unseenSystemChangesCount = 0;
                UpdateUiWithChangesCount();
                ApplySystemChangesSearchFilter();
            }
        }
        #endregion

        #region Groups Tab Custom Drawing & Events
        private void GroupsListView_MouseClick(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            var hitInfo = groupsListView.HitTest(e.Location);
            if (hitInfo.Item == null || hitInfo.SubItem == null) return;
            if (hitInfo.Item.SubItems.IndexOf(hitInfo.SubItem) == 1)
            {
                if (hitInfo.Item.Tag is FirewallGroup group)
                {
                    group.IsEnabled = !group.IsEnabled;
                    groupsListView.Invalidate(hitInfo.Item.Bounds);
                }
            }
        }

        private async void DeleteGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (groupsListView.SelectedItems.Count > 0)
            {
                var result = MessageBox.Show("Are you sure you want to delete the selected group(s) and all associated rules?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    foreach (ListViewItem item in groupsListView.SelectedItems)
                    {
                        if (item.Tag is FirewallGroup group)
                        {
                            await _actionsService.DeleteGroupAsync(group.Name);
                        }
                    }
                    await DisplayGroupsAsync();
                }
            }
        }

        private void GroupsListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawBackground();
            if ((e.State & ListViewItemStates.Focused) != 0)
            {
                e.DrawFocusRectangle();
            }
        }

        private void GroupsListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            Color backColor;
            Color foreColor;

            if (e.Item.Selected)
            {
                backColor = SystemColors.Highlight;
                foreColor = SystemColors.HighlightText;
            }
            else
            {
                backColor = e.Item.ListView.BackColor;
                foreColor = e.Item.ListView.ForeColor;
            }

            using (var backBrush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(backBrush, e.Bounds);
            }

            if (_hoveredItem == e.Item && !e.Item.Selected)
            {
                using var overlayBrush = new SolidBrush(Color.FromArgb(20, Color.Black));
                e.Graphics.FillRectangle(overlayBrush, e.Bounds);
            }

            if (e.ColumnIndex == 0)
            {
                TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.SubItem.Font, e.Bounds, foreColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            }
            else if (e.ColumnIndex == 1)
            {
                if (e.Item.Tag is FirewallGroup group)
                {
                    DrawToggleSwitch(e.Graphics, e.Bounds, group.IsEnabled);
                }
            }
        }

        private void DrawToggleSwitch(Graphics g, Rectangle bounds, bool isChecked)
        {
            int switchWidth = Scale(50, g);
            int switchHeight = Scale(25, g);
            int thumbSize = Scale(21, g);
            int padding = Scale(10, g);
            Rectangle switchRect = new Rectangle(
                bounds.X + padding,
                bounds.Y + (bounds.Height - switchHeight) / 2,
                switchWidth,
                switchHeight);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color backColor = isChecked ? Color.FromArgb(0, 192, 0) : Color.FromArgb(200, 0, 0);
            using (var path = new GraphicsPath())
            {
                path.AddArc(switchRect.X, switchRect.Y, switchRect.Height, switchRect.Height, 90, 180);
                path.AddArc(switchRect.Right - switchRect.Height, switchRect.Y, switchRect.Height, switchRect.Height, 270, 180);
                path.CloseFigure();
                g.FillPath(new SolidBrush(backColor), path);
            }

            int thumbX = isChecked ? switchRect.Right - thumbSize - Scale(2, g) : switchRect.X + Scale(2, g);
            Rectangle thumbRect = new Rectangle(
                thumbX,
                switchRect.Y + (switchRect.Height - thumbSize) / 2,
                thumbSize,
                thumbSize);
            using (var thumbBrush = new SolidBrush(dm.OScolors.TextActive))
            {
                g.FillEllipse(thumbBrush, thumbRect);
            }
        }
        #endregion

        #region Rule Management
        private void RulesListView_MouseClick(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (rulesListView.FocusedItem != null && rulesListView.FocusedItem.Bounds.Contains(e.Location))
                {
                    rulesContextMenu.Show(Cursor.Position);
                }
            }
        }

        private async void ApplyRuleMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is not ToolStripMenuItem menuItem || menuItem.Tag?.ToString() is not string action || rulesListView.SelectedItems.Count == 0) return;
            var items = rulesListView.SelectedItems.Cast<ListViewItem>()
                .Select(i => i.Tag as AggregatedRuleViewModel)
                .Where(r => r != null)
                .Select(r => r!)
                .ToList();
            if (items.Count > 0)
            {
                foreach (var item in items)
                {
                    var firstRule = item.UnderlyingRules.FirstOrDefault();
                    if (firstRule == null) continue;

                    switch (firstRule.Type)
                    {
                        case RuleType.Program:
                            _actionsService.ApplyApplicationRuleChange([firstRule.ApplicationName], action);
                            break;
                        case RuleType.Service:
                            _actionsService.ApplyServiceRuleChange(firstRule.ServiceName, action);
                            break;
                        case RuleType.UWP:
                            if (firstRule.Description.Contains(MFWConstants.UwpDescriptionPrefix))
                            {
                                var pfn = firstRule.Description.Replace(MFWConstants.UwpDescriptionPrefix, "");
                                var uwpApp = new UwpApp { Name = item.Name, PackageFamilyName = pfn };
                                _actionsService.ApplyUwpRuleChange([uwpApp], action);
                            }
                            break;
                    }
                }
                await ForceDataRefreshAsync();
            }
        }

        private async void DeleteRuleMenuItem_Click(object sender, EventArgs e)
        {
            if (rulesListView.SelectedItems.Count == 0) return;
            var items = rulesListView.SelectedItems.Cast<ListViewItem>()
                .Select(i => i.Tag as AggregatedRuleViewModel)
                .Where(r => r != null)
                .Select(r => r!)
                .ToList();
            if (items.Count > 0)
            {
                var result = Messenger.MessageBox($"Are you sure you want to delete all underlying rules for the {items.Count} selected item(s)?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No) return;

                var ruleNamesToDelete = items.SelectMany(i => i.UnderlyingRules.Select(r => r.Name)).ToList();
                if (ruleNamesToDelete.Count > 0)
                {
                    _actionsService.DeleteAdvancedRules(ruleNamesToDelete);
                }
                await ForceDataRefreshAsync();
            }
        }

        #endregion

        #region Live Connections & Sorting
        private void LiveConnectionsListView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (liveConnectionsListView.FocusedItem != null && liveConnectionsListView.FocusedItem.Bounds.Contains(e.Location))
                {
                    liveConnectionsContextMenu.Show(Cursor.Position);
                }
            }
        }

        private void LiveConnectionsListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = false;
        }

        private void LiveConnectionsListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            Color backColor;
            Color foreColor;
            if (e.Item.Selected)
            {
                backColor = e.Item.ListView.Focused ? SystemColors.Highlight : SystemColors.ControlDark;
                foreColor = e.Item.ListView.Focused ? SystemColors.HighlightText : SystemColors.ControlText;
            }
            else
            {
                backColor = e.Item.ListView.BackColor;
                foreColor = e.Item.ListView.ForeColor;
            }

            e.Graphics.FillRectangle(new SolidBrush(backColor), e.Bounds);
            if (_hoveredItem == e.Item && !e.Item.Selected)
            {
                using var overlayBrush = new SolidBrush(Color.FromArgb(25, Color.Black));
                e.Graphics.FillRectangle(overlayBrush, e.Bounds);
            }

            if (e.ColumnIndex == 0)
            {
                if (e.Item.ImageIndex != -1 && e.Item.ImageList != null)
                {
                    Image img = e.Item.ImageList.Images[e.Item.ImageIndex];
                    int imgX = e.Bounds.Left + (e.Bounds.Width - img.Width) / 2;
                    int imgY = e.Bounds.Top + (e.Bounds.Height - img.Height) / 2;
                    e.Graphics.DrawImage(img, imgX, imgY);
                }
                return;
            }

            TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.PathEllipsis;
            TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.SubItem.Font, e.Bounds, foreColor, flags);
        }

        private void KillProcessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (liveConnectionsListView.SelectedItems.Count > 0 && liveConnectionsListView.SelectedItems[0].Tag is TcpConnectionViewModel vm)
            {
                vm.KillProcessCommand.Execute(null);
            }
        }

        private void BlockRemoteIPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (liveConnectionsListView.SelectedItems.Count > 0 && liveConnectionsListView.SelectedItems[0].Tag is TcpConnectionViewModel vm)
            {
                var rule = new AdvancedRuleViewModel
                {
                    Name = $"Block {vm.RemoteAddress}",
                    Description = "Blocked from Live Connections",
                    IsEnabled = true,
                    Grouping = MFWConstants.MainRuleGroup,
                    Status = "Block",
                    Direction = Directions.Outgoing,
                    Protocol = 6,
                    RemoteAddresses = [new(System.Net.IPAddress.Parse(vm.RemoteAddress))],
                    Profiles = "All",
                    Type = RuleType.Advanced
                };
                _actionsService.CreateAdvancedRule(rule, "All", "");
                _ = ForceDataRefreshAsync(showStatus: false);
            }
        }

        private void ListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (sender is not ListView listView) return;
            int sortColumn;
            SortOrder sortOrder;

            switch (listView.Name)
            {
                case "rulesListView":
                    sortColumn = _rulesSortColumn;
                    sortOrder = _rulesSortOrder;
                    break;
                case "dashboardListView":
                    sortColumn = _dashboardSortColumn;
                    sortOrder = _dashboardSortOrder;
                    break;
                case "systemChangesListView":
                    sortColumn = _systemChangesSortColumn;
                    sortOrder = _systemChangesSortOrder;
                    break;
                case "groupsListView":
                    sortColumn = _groupsSortColumn;
                    sortOrder = _groupsSortOrder;
                    break;
                case "liveConnectionsListView":
                    sortColumn = _liveConnectionsSortColumn;
                    sortOrder = _liveConnectionsSortOrder;
                    break;
                default:
                    return;
            }

            if (e.Column == sortColumn)
            {
                sortOrder = (sortOrder == SortOrder.Ascending) ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                sortColumn = e.Column;
                sortOrder = SortOrder.Ascending;
            }

            switch (listView.Name)
            {
                case "rulesListView":
                    _rulesSortColumn = sortColumn;
                    _rulesSortOrder = sortOrder;
                    break;
                case "dashboardListView":
                    _dashboardSortColumn = sortColumn;
                    _dashboardSortOrder = sortOrder;
                    break;
                case "systemChangesListView":
                    _systemChangesSortColumn = sortColumn;
                    _systemChangesSortOrder = sortOrder;
                    break;
                case "groupsListView":
                    _groupsSortColumn = sortColumn;
                    _groupsSortOrder = sortOrder;
                    break;
                case "liveConnectionsListView":
                    _liveConnectionsSortColumn = sortColumn;
                    _liveConnectionsSortOrder = sortOrder;
                    break;
            }

            int columnToSort = e.Column;
            if (listView == rulesListView && _appSettings.ShowAppIcons)
            {
                columnToSort = e.Column > 0 ? e.Column : 1;
            }
            if (listView == dashboardListView && _appSettings.ShowAppIcons)
            {
                columnToSort = e.Column > 1 ? e.Column : 2;
            }
            if (listView == liveConnectionsListView && _appSettings.ShowAppIcons)
            {
                columnToSort = e.Column > 0 ? e.Column : 1;
            }

            listView.ListViewItemSorter = new ListViewItemComparer(columnToSort, sortOrder);
            listView.Sort();
        }
        #endregion

        #region Context Menu Handlers
        private void ContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sender is not ContextMenuStrip contextMenu) return;

            ListView? listView = null;
            if (contextMenu == dashboardContextMenu) listView = dashboardListView;
            else if (contextMenu == rulesContextMenu) listView = rulesListView;
            else if (contextMenu == systemChangesListView.ContextMenuStrip) listView = systemChangesListView;
            else if (contextMenu == liveConnectionsContextMenu) listView = liveConnectionsListView;

            if (listView == null || listView.SelectedItems.Count == 0)
            {
                e.Cancel = true;
                return;
            }

            var selectedItem = listView.SelectedItems[0];
            string appPath = "";
            bool isProgram = false;

            switch (selectedItem.Tag)
            {
                case PendingConnectionViewModel pending:
                    appPath = pending.AppPath;
                    isProgram = !string.IsNullOrEmpty(appPath);
                    break;
                case TcpConnectionViewModel live:
                    appPath = live.ProcessPath;
                    isProgram = !string.IsNullOrEmpty(appPath);
                    break;
                case AggregatedRuleViewModel agg:
                    appPath = agg.ApplicationName;
                    isProgram = agg.Type == RuleType.Program && !string.IsNullOrEmpty(appPath);
                    break;
                case FirewallRuleChange change:
                    appPath = change.Rule.ApplicationName;
                    isProgram = !string.IsNullOrEmpty(appPath);
                    break;
            }

            foreach (ToolStripItem item in contextMenu.Items)
            {
                if (item.Name.Contains("openFileLocation"))
                {
                    item.Enabled = isProgram && File.Exists(appPath);
                }
                if (item.Name.Contains("createAdvancedRule"))
                {
                    item.Enabled = isProgram;
                }
                if (item.Name.Contains("allowAndTrustPublisher"))
                {
                    bool isSigned = SignatureValidationService.GetPublisherInfo(appPath, out _);
                    item.Visible = isSigned;
                }
            }
        }

        private async void createAdvancedRuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListViewItem? selectedItem = null;
            string appPath = "";
            string direction = "Outbound";

            if (dashboardListView.SelectedItems.Count > 0)
            {
                selectedItem = dashboardListView.SelectedItems[0];
                if (selectedItem.Tag is PendingConnectionViewModel pending)
                {
                    appPath = pending.AppPath;
                    direction = pending.Direction;
                }
            }
            else if (liveConnectionsListView.SelectedItems.Count > 0)
            {
                selectedItem = liveConnectionsListView.SelectedItems[0];
                if (selectedItem.Tag is TcpConnectionViewModel live)
                {
                    appPath = live.ProcessPath;
                }
            }

            if (!string.IsNullOrEmpty(appPath))
            {
                using var dialog = new CreateAdvancedRuleForm(_firewallPolicy, _actionsService, appPath, direction);
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    await ForceDataRefreshAsync();
                }
            }
        }

        private void copyRemoteAddressToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (liveConnectionsListView.SelectedItems.Count > 0 && liveConnectionsListView.SelectedItems[0].Tag is TcpConnectionViewModel vm)
            {
                Clipboard.SetText(vm.RemoteAddress);
            }
        }

        private void copyDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListView? activeListView = mainTabControl.SelectedTab?.Controls.OfType<ListView>().FirstOrDefault();
            if (activeListView?.SelectedItems.Count == 0) return;

            var selectedItem = activeListView.SelectedItems[0];
            var details = new StringBuilder();

            switch (selectedItem.Tag)
            {
                case PendingConnectionViewModel pending:
                    details.AppendLine($"Type: Pending Connection");
                    details.AppendLine($"Application: {pending.FileName}");
                    details.AppendLine($"Path: {pending.AppPath}");
                    details.AppendLine($"Service: {pending.ServiceName}");
                    details.AppendLine($"Direction: {pending.Direction}");
                    break;
                case AggregatedRuleViewModel agg:
                    details.AppendLine($"Type: {agg.Type} Rule");
                    details.AppendLine($"Name: {agg.Name}");
                    details.AppendLine($"Application: {agg.ApplicationName}");
                    details.AppendLine($"Service: {agg.ServiceName}");
                    details.AppendLine($"Action: {agg.Status}");
                    details.AppendLine($"Direction: {agg.Direction}");
                    details.AppendLine($"Protocol: {agg.ProtocolName}");
                    details.AppendLine($"Remote Addresses: {(agg.RemoteAddresses.Any() ? string.Join(", ", agg.RemoteAddresses) : "Any")}");
                    details.AppendLine($"Description: {agg.Description}");
                    break;
                case TcpConnectionViewModel live:
                    details.AppendLine($"Type: Live Connection");
                    details.AppendLine($"Process: {live.ProcessName}");
                    details.AppendLine($"Path: {live.ProcessPath}");
                    details.AppendLine($"Remote Address: {live.RemoteAddress}");
                    details.AppendLine($"Remote Port: {live.RemotePort}");
                    break;
                case FirewallRuleChange change:
                    details.AppendLine($"Type: Audited Change ({change.Type})");
                    if (change.Rule != null)
                    {
                        details.AppendLine($"Rule Name: {change.Rule.Name}");
                        details.AppendLine($"Application: {change.Rule.ApplicationName}");
                        details.AppendLine($"Action: {change.Rule.Status}");
                        details.AppendLine($"Direction: {change.Rule.Direction}");
                        details.AppendLine($"Protocol: {change.Rule.ProtocolName}");
                        details.AppendLine($"Remote Addresses: {(change.Rule.RemoteAddresses.Any() ? string.Join(", ", change.Rule.RemoteAddresses) : "Any")}");
                    }
                    break;
            }

            if (details.Length > 0)
            {
                Clipboard.SetText(details.ToString());
            }
        }

        private void OpenFileLocationMenuItem_Click(object sender, EventArgs e)
        {
            ListView? activeListView = mainTabControl.SelectedTab?.Controls.OfType<ListView>().FirstOrDefault();
            if (activeListView?.SelectedItems.Count == 0) return;

            var selectedItem = activeListView.SelectedItems[0];
            string? appPath = null;

            switch (selectedItem.Tag)
            {
                case PendingConnectionViewModel pending:
                    appPath = pending.AppPath;
                    break;
                case AggregatedRuleViewModel agg when agg.Type == RuleType.Program:
                    appPath = agg.ApplicationName;
                    break;
                case TcpConnectionViewModel live:
                    appPath = live.ProcessPath;
                    break;
                case FirewallRuleChange change:
                    appPath = change.Rule?.ApplicationName;
                    break;
            }

            if (string.IsNullOrEmpty(appPath) || !File.Exists(appPath))
            {
                Messenger.MessageBox("The file path for this item could not be found.", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                Process.Start("explorer.exe", $"/select, \"{appPath}\"");
            }
            catch (Exception ex)
            {
                Messenger.MessageBox($"Could not open file location.\n\nError: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Dashboard Context Menu Handlers
        private void TempAllowMenuItem_Click(object sender, EventArgs e)
        {
            if (dashboardListView.SelectedItems.Count > 0 &&
                dashboardListView.SelectedItems[0].Tag is PendingConnectionViewModel pending &&
                sender is ToolStripMenuItem menuItem &&
                int.TryParse(menuItem.Tag?.ToString(), out int minutes))
            {
                _actionsService.ProcessPendingConnection(pending, "TemporaryAllow", TimeSpan.FromMinutes(minutes));
                _pendingConnections.Remove(pending);
                ApplyDashboardSearchFilter();
                _ = ForceDataRefreshAsync(showStatus: false);
            }
        }

        private void PermanentAllowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dashboardListView.SelectedItems.Count > 0 &&
                dashboardListView.SelectedItems[0].Tag is PendingConnectionViewModel pending)
            {
                _actionsService.ProcessPendingConnection(pending, "Allow");
                _pendingConnections.Remove(pending);
                ApplyDashboardSearchFilter();
                _ = ForceDataRefreshAsync(showStatus: false);
            }
        }

        private void AllowAndTrustPublisherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dashboardListView.SelectedItems.Count > 0 &&
                dashboardListView.SelectedItems[0].Tag is PendingConnectionViewModel pending)
            {
                _actionsService.ProcessPendingConnection(pending, "Allow", trustPublisher: true);
                _pendingConnections.Remove(pending);
                ApplyDashboardSearchFilter();
                _ = ForceDataRefreshAsync(showStatus: false);
            }
        }

        private void PermanentBlockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dashboardListView.SelectedItems.Count > 0 &&
                dashboardListView.SelectedItems[0].Tag is PendingConnectionViewModel pending)
            {
                _actionsService.ProcessPendingConnection(pending, "Block");
                _pendingConnections.Remove(pending);
                ApplyDashboardSearchFilter();
                _ = ForceDataRefreshAsync(showStatus: false);
            }
        }

        private void IgnoreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dashboardListView.SelectedItems.Count > 0 &&
                dashboardListView.SelectedItems[0].Tag is PendingConnectionViewModel pending)
            {
                _actionsService.ProcessPendingConnection(pending, "Ignore");
                _pendingConnections.Remove(pending);
                ApplyDashboardSearchFilter();
            }
        }

        private async void createWildcardRuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dashboardListView.SelectedItems.Count > 0 &&
                dashboardListView.SelectedItems[0].Tag is PendingConnectionViewModel pending)
            {
                using var wildcardDialog = new WildcardCreatorForm(_wildcardRuleService, pending.AppPath);
                if (wildcardDialog.ShowDialog(this) == DialogResult.OK)
                {
                    _pendingConnections.Remove(pending);
                    ApplyDashboardSearchFilter();
                    await ForceDataRefreshAsync(showStatus: false);
                }
            }
        }
        #endregion

        private static Image RecolorImage(Image sourceImage, Color newColor)
        {
            var newBitmap = new Bitmap(sourceImage.Width, sourceImage.Height);
            using (var g = Graphics.FromImage(newBitmap))
            {
                float r = newColor.R / 255f;
                float g_ = newColor.G / 255f;
                float b = newColor.B / 255f;
                var colorMatrix = new ColorMatrix(
                [
                    [0, 0, 0, 0, 0],
                    [0, 0, 0, 0, 0],
                    [0, 0, 0, 0, 0],
                    [0, 0, 0, 1, 0],
                    [r, g_, b, 0, 1]
                ]);
                using (var attributes = new ImageAttributes())
                {
                    attributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                    g.DrawImage(sourceImage, new Rectangle(0, 0, sourceImage.Width, sourceImage.Height),
                                0, 0, sourceImage.Width, sourceImage.Height, GraphicsUnit.Pixel, attributes);
                }
            }
            return newBitmap;
        }
    }
}