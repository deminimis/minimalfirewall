using DarkModeForms;
using Firewall.Traffic.ViewModels;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Drawing;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using MinimalFirewall.TypedObjects;
using System.Collections.Generic;
using System.Linq;


namespace MinimalFirewall
{
    public partial class SettingsControl : UserControl
    {
        private AppSettings _appSettings;
        private StartupService _startupService;
        private PublisherWhitelistService _whitelistService;
        private FirewallActionsService _actionsService;
        private UserActivityLogger _activityLogger;
        private MainViewModel _mainViewModel;
        private ImageList _appImageList;
        private DarkModeCS _dm;

        public event Action ThemeChanged;
        public event Action IconVisibilityChanged;
        public event Func<Task> DataRefreshRequested;
        public event Action AutoRefreshTimerChanged;
        public event Action? TrafficMonitorSettingChanged;

        public SettingsControl()
        {
            InitializeComponent();
        }

        public void Initialize(
            AppSettings appSettings,
            StartupService startupService,
            PublisherWhitelistService whitelistService,
            FirewallActionsService actionsService,
            UserActivityLogger activityLogger,
            MainViewModel mainViewModel,
            ImageList appImageList,
            string version,
            DarkModeCS dm)
        {
            _appSettings = appSettings;
            _startupService = startupService;
            _whitelistService = whitelistService;
            _actionsService = actionsService;
            _activityLogger = activityLogger;
            _mainViewModel = mainViewModel;
            _appImageList = appImageList;
            _dm = dm;

            versionLabel.Text = version;
            loggingSwitch.CheckedChanged += new System.EventHandler(this.loggingSwitch_CheckedChanged);
            coffeePictureBox.Image = _appImageList.Images["coffee.png"];
        }

        public void ApplyThemeFixes()
        {
            if (_dm == null) return;
            deleteAllRulesButton.FlatAppearance.BorderSize = 1;
            deleteAllRulesButton.FlatAppearance.BorderColor = _dm.OScolors.ControlDark;
            revertFirewallButton.FlatAppearance.BorderSize = 1;
            revertFirewallButton.FlatAppearance.BorderColor = _dm.OScolors.ControlDark;
            managePublishersButton.FlatAppearance.BorderSize = 1;
            managePublishersButton.FlatAppearance.BorderColor = _dm.OScolors.ControlDark;
            openFirewallButton.FlatAppearance.BorderSize = 1;
            openFirewallButton.FlatAppearance.BorderColor = _dm.OScolors.ControlDark;
            openAppDataButton.FlatAppearance.BorderSize = 1;
            openAppDataButton.FlatAppearance.BorderColor = _dm.OScolors.ControlDark;
            checkForUpdatesButton.FlatAppearance.BorderSize = 1;
            checkForUpdatesButton.FlatAppearance.BorderColor = _dm.OScolors.ControlDark;
            cleanUpOrphanedRulesButton.FlatAppearance.BorderSize = 1;
            cleanUpOrphanedRulesButton.FlatAppearance.BorderColor = _dm.OScolors.ControlDark;
            exportRulesButton.FlatAppearance.BorderSize = 1;
            exportRulesButton.FlatAppearance.BorderColor = _dm.OScolors.ControlDark;
            importMergeButton.FlatAppearance.BorderSize = 1;
            importMergeButton.FlatAppearance.BorderColor = _dm.OScolors.ControlDark;
            importReplaceButton.FlatAppearance.BorderSize = 1;
            importReplaceButton.FlatAppearance.BorderColor = _dm.OScolors.ControlDark;
            exportDiagnosticButton.FlatAppearance.BorderSize = 1;
            exportDiagnosticButton.FlatAppearance.BorderColor = _dm.OScolors.ControlDark;

            if (_dm.IsDarkMode)
            {
                deleteAllRulesButton.ForeColor = Color.White;
                revertFirewallButton.ForeColor = Color.White;
                managePublishersButton.ForeColor = Color.White;
                openFirewallButton.ForeColor = Color.White;
                openAppDataButton.ForeColor = Color.White;
                checkForUpdatesButton.ForeColor = Color.White;
                cleanUpOrphanedRulesButton.ForeColor = Color.White;
                exportRulesButton.ForeColor = Color.White;
                importMergeButton.ForeColor = Color.White;
                importReplaceButton.ForeColor = Color.White;
                exportDiagnosticButton.ForeColor = Color.White;
            }
            else
            {
                deleteAllRulesButton.ForeColor = SystemColors.ControlText;
                revertFirewallButton.ForeColor = SystemColors.ControlText;
                managePublishersButton.ForeColor = SystemColors.ControlText;
                openFirewallButton.ForeColor = SystemColors.ControlText;
                openAppDataButton.ForeColor = SystemColors.ControlText;
                checkForUpdatesButton.ForeColor = SystemColors.ControlText;
                cleanUpOrphanedRulesButton.ForeColor = SystemColors.ControlText;
                exportRulesButton.ForeColor = SystemColors.ControlText;
                importMergeButton.ForeColor = SystemColors.ControlText;
                importReplaceButton.ForeColor = SystemColors.ControlText;
                exportDiagnosticButton.ForeColor = SystemColors.ControlText;
            }
        }

        public void LoadSettingsToUI()
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

        public void SaveSettingsFromUI()
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

            _activityLogger.IsEnabled = _appSettings.IsLoggingEnabled;

            if (!_appSettings.IsTrafficMonitorEnabled)
            {
                _mainViewModel.TrafficMonitorViewModel.StopMonitoring();
            }

            IconVisibilityChanged?.Invoke();
            _appSettings.Save();
        }

        public void ApplyTheme(bool isDark, DarkModeCS dm)
        {
            var linkColor = isDark ?
                Color.SkyBlue : SystemColors.HotTrack;
            helpLink.LinkColor = linkColor;
            reportProblemLink.LinkColor = linkColor;
            forumLink.LinkColor = linkColor;
            coffeeLinkLabel.LinkColor = linkColor;
            helpLink.VisitedLinkColor = linkColor;
            reportProblemLink.VisitedLinkColor = linkColor;
            forumLink.VisitedLinkColor = linkColor;
            coffeeLinkLabel.VisitedLinkColor = linkColor;

            Image? coffeeImage = _appImageList.Images["coffee.png"];
            if (coffeeImage != null)
            {
                Color coffeeColor = isDark ?
                    Color.LightGray : Color.Black;
                Image? oldImage = coffeePictureBox.Image;
                coffeePictureBox.Image = DarkModeCS.RecolorImage(coffeeImage, coffeeColor);
                oldImage?.Dispose();
            }
        }

        private void DarkModeSwitch_CheckedChanged(object sender, EventArgs e)
        {
            _appSettings.Theme = darkModeSwitch.Checked ?
                "Dark" : "Light";
            ThemeChanged?.Invoke();
        }

        private void startOnStartupSwitch_CheckedChanged(object sender, EventArgs e)
        {
            if (_appSettings != null && _startupService != null)
            {
                _appSettings.StartOnSystemStartup = startOnStartupSwitch.Checked;
                _startupService.SetStartup(_appSettings.StartOnSystemStartup);
            }
        }

        private void PopupsSwitch_CheckedChanged(object sender, EventArgs e)
        {
            if (_appSettings != null)
            {
                _appSettings.IsPopupsEnabled = popupsSwitch.Checked;
            }
        }

        private void loggingSwitch_CheckedChanged(object sender, EventArgs e)
        {
            if (_appSettings != null)
            {
                _appSettings.IsLoggingEnabled = loggingSwitch.Checked;
                _activityLogger.IsEnabled = _appSettings.IsLoggingEnabled;
            }
        }

        private void TrafficMonitorSwitch_CheckedChanged(object sender, EventArgs e)
        {
            _appSettings.IsTrafficMonitorEnabled = trafficMonitorSwitch.Checked;
            TrafficMonitorSettingChanged?.Invoke();
        }

        private void ShowAppIconsSwitch_CheckedChanged(object sender, EventArgs e)
        {
            _appSettings.ShowAppIcons = showAppIconsSwitch.Checked;
            IconVisibilityChanged?.Invoke();
        }

        private void managePublishersButton_Click(object sender, EventArgs e)
        {
            using var form = new ManagePublishersForm(_whitelistService, _appSettings);
            form.ShowDialog(this.FindForm());
        }

        private void OpenFirewallButton_Click(object sender, EventArgs e)
        {
            try
            {
                string wfPath = Path.Combine(Environment.SystemDirectory, "wf.msc");
                var startInfo = new ProcessStartInfo(wfPath)
                {
                    UseShellExecute = true
                };
                Process.Start(startInfo);
            }
            catch (Exception ex) when (ex is Win32Exception or FileNotFoundException)
            {
                Messenger.MessageBox($"Could not open Windows Firewall console.\n\nError: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void openAppDataButton_Click(object sender, EventArgs e)
        {
            try
            {
                string standardAppDataPath = ConfigPathManager.GetStandardAppDataDirectory();

                if (!Directory.Exists(standardAppDataPath))
                {
                    Directory.CreateDirectory(standardAppDataPath);
                }

                Process.Start("explorer.exe", standardAppDataPath);
            }
            catch (Exception ex)
            {
                Messenger.MessageBox($"Could not open %AppData% folder.\n\nError: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CheckForUpdatesButton_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo("https://github.com/deminimis/minimalfirewall/releases") { UseShellExecute = true });
            }
            catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
            {
                Messenger.MessageBox($"Could not open the link.\n\nError: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (sender is not LinkLabel { Tag: string url }) return;
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
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
            catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
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

        private async void deleteAllRulesButton_Click(object sender, EventArgs e)
        {
            var result = Messenger.MessageBox("This will permanently delete all firewall rules created by this application. This action cannot be undone. Are you sure you want to continue?",
                "Delete All Rules", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                _actionsService.DeleteAllMfwRules();
                await (DataRefreshRequested?.Invoke() ?? Task.CompletedTask);
                Messenger.MessageBox("All Minimal Firewall rules have been deleted.", "Operation Complete", MessageBoxButtons.OK, MessageBoxIcon.None);
            }
        }

        private async void revertFirewallButton_Click(object sender, EventArgs e)
        {
            var result = Messenger.MessageBox("WARNING: This will reset your ENTIRE Windows Firewall configuration to its default state. " +
                "All custom rules, including those not created by this application, will be deleted. This action is irreversible.\n\n" +
                "Are you absolutely sure you want to continue?",
                "Revert Windows Firewall Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                AdminTaskService.ResetFirewall();
                await (DataRefreshRequested?.Invoke() ?? Task.CompletedTask);
                Messenger.MessageBox("Windows Firewall has been reset to its default settings. It is recommended to restart the application.",
                    "Operation Complete", MessageBoxButtons.OK, MessageBoxIcon.None);
            }
        }

        private async void cleanUpOrphanedRulesButton_Click(object sender, EventArgs e)
        {
            var result = Messenger.MessageBox(
                "This will scan for rules whose associated application no longer exists on disk and delete them.\n\nAre you sure you want to continue?",
                "Clean Up Orphaned Rules",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                await _mainViewModel.CleanUpOrphanedRulesAsync();
            }
        }

        private async void exportRulesButton_Click(object sender, EventArgs e)
        {
            using var saveDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Export Minimal Firewall Rules",
                FileName = $"mfw_rules_backup_{DateTime.Now:yyyyMMdd}.json"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string jsonContent = await _actionsService.ExportAllMfwRulesAsync();
                    await File.WriteAllTextAsync(saveDialog.FileName, jsonContent);
                    Messenger.MessageBox("All rules have been successfully exported.", "Export Complete", MessageBoxButtons.OK, MsgIcon.Success);
                }
                catch (Exception ex)
                {
                    Messenger.MessageBox($"An error occurred during export: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void importMergeButton_Click(object sender, EventArgs e)
        {
            using var openDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Import and Add Rules"
            };

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string jsonContent = await File.ReadAllTextAsync(openDialog.FileName);
                    await _actionsService.ImportRulesAsync(jsonContent, replace: false);
                    await (DataRefreshRequested?.Invoke() ?? Task.CompletedTask);
                    Messenger.MessageBox("Rules have been successfully imported and added.", "Import Complete", MessageBoxButtons.OK, MsgIcon.Success);
                }
                catch (Exception ex)
                {
                    Messenger.MessageBox($"An error occurred during import: {ex.Message}", "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void importReplaceButton_Click(object sender, EventArgs e)
        {
            var confirmResult = Messenger.MessageBox(
                "WARNING: This will delete ALL current Minimal Firewall rules before importing the new ones. This action cannot be undone.\n\nAre you sure you want to continue?",
                "Confirm Replace", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirmResult == DialogResult.Yes)
            {
                using var openDialog = new OpenFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    Title = "Import and Replace Rules"
                };

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string jsonContent = await File.ReadAllTextAsync(openDialog.FileName);
                        await _actionsService.ImportRulesAsync(jsonContent, replace: true);
                        await (DataRefreshRequested?.Invoke() ?? Task.CompletedTask);
                        Messenger.MessageBox("All rules have been replaced successfully.", "Import Complete", MessageBoxButtons.OK, MsgIcon.Success);
                    }
                    catch (Exception ex)
                    {
                        Messenger.MessageBox($"An error occurred during import: {ex.Message}", "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private async Task<string> ReadLastNLinesAsync(string filePath, int n)
        {
            if (!File.Exists(filePath))
            {
                return $"{Path.GetFileName(filePath)} not found.";
            }

            try
            {
                var lines = await File.ReadAllLinesAsync(filePath);
                var lastNLines = lines.Skip(Math.Max(0, lines.Length - n));
                return string.Join(Environment.NewLine, lastNLines);
            }
            catch (Exception ex)
            {
                return $"Error reading {Path.GetFileName(filePath)}: {ex.Message}";
            }
        }
        private async void exportDiagnosticButton_Click(object sender, EventArgs e)
        {
            using var saveDialog = new SaveFileDialog
            {
                Filter = "Zip files (*.zip)|*.zip",
                Title = "Export Diagnostic Package",
                FileName = $"mfw_diagnostics_{DateTime.Now:yyyyMMdd_HHmmss}.zip"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                string zipPath = saveDialog.FileName;
                var statusForm = new StatusForm("Gathering diagnostic data...", _appSettings);
                statusForm.Show(this.FindForm());
                Application.DoEvents();

                try
                {

                    if (File.Exists(zipPath)) File.Delete(zipPath);
                    using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                    {

                        statusForm.UpdateStatus("Adding configuration files...");
                        var configFiles = new List<string>
                        {
                            "settings.json",
                            "wildcard_rules.json",
                            "trusted_publishers.json",
                            "foreign_rules_baseline.json",
                            "temporary_rules.json",
                            "uwp_apps.json"
                        };

                        foreach (var fileName in configFiles)
                        {
                            string filePath = ConfigPathManager.GetConfigPath(fileName);

                            if (File.Exists(filePath))
                            {
                                archive.CreateEntryFromFile(filePath, fileName);
                            }
                        }
                        statusForm.UpdateProgress(20);


                        statusForm.UpdateStatus("Adding debug log...");
                        string debugLogPath = ConfigPathManager.GetConfigPath("debug_log.txt");
                        string logContent = await ReadLastNLinesAsync(debugLogPath, 1500);
                        var logEntry = archive.CreateEntry("debug_log_last1500.txt");
                        using (var writer = new StreamWriter(logEntry.Open()))
                        {
                            await writer.WriteAsync(logContent);
                        }
                        statusForm.UpdateProgress(40);


                        statusForm.UpdateStatus("Exporting current rules...");
                        string rulesJson = await _actionsService.ExportAllMfwRulesAsync();
                        var rulesEntry = archive.CreateEntry("current_mfw_rules.json");
                        using (var writer = new StreamWriter(rulesEntry.Open()))
                        {
                            await writer.WriteAsync(rulesJson);
                        }
                        statusForm.UpdateProgress(60);


                        statusForm.UpdateStatus("Gathering system info...");
                        var sysInfo = new StringBuilder();
                        sysInfo.AppendLine($"OS Version: {Environment.OSVersion}");
                        sysInfo.AppendLine($".NET Runtime: {RuntimeInformation.FrameworkDescription}");
                        sysInfo.AppendLine($"App Version: {Assembly.GetExecutingAssembly().GetName()?.Version}");
                        sysInfo.AppendLine($"Timestamp: {DateTime.Now}");


                        var sysInfoEntry = archive.CreateEntry("system_info.txt");
                        using (var writer = new StreamWriter(sysInfoEntry.Open()))
                        {
                            await writer.WriteAsync(sysInfo.ToString());
                        }
                        statusForm.UpdateProgress(80);
                    }

                    statusForm.UpdateStatus("Finalizing package...");
                    statusForm.UpdateProgress(100);
                    statusForm.Close();
                    Messenger.MessageBox($"Diagnostic package exported successfully to:\n{zipPath}", "Export Complete", MessageBoxButtons.OK, MsgIcon.Success);
                }
                catch (Exception ex)
                {
                    statusForm?.Close();
                    _activityLogger.LogException("ExportDiagnosticPackage", ex);
                    Messenger.MessageBox($"An error occurred while exporting diagnostics:\n{ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}