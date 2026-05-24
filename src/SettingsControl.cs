using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Firewall.Traffic.ViewModels;
using MinimalFirewall.TypedObjects;
using DarkModeForms;
using static DarkModeForms.OSThemeColors;

namespace MinimalFirewall
{
    public partial class SettingsControl : UserControl
    {
        private AppSettings _appSettings = null!;
        private StartupService _startupService = null!;
        private PublisherWhitelistService _whitelistService = null!;
        private FirewallActionsService _actionsService = null!;
        private UserActivityLogger _activityLogger = null!;
        private MainViewModel _mainViewModel = null!;
        private ImageList _appImageList = null!;


        public event Action? ThemeChanged;
        public event Action? IconVisibilityChanged;
        public event Func<Task>? DataRefreshRequested;
        public event Action? AutoRefreshTimerChanged;
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
            string version)
        {
            _appSettings = appSettings;
            _startupService = startupService;
            _whitelistService = whitelistService;
            _actionsService = actionsService;
            _activityLogger = activityLogger;
            _mainViewModel = mainViewModel;
            _appImageList = appImageList;

            versionLabel.Text = version;
            loggingSwitch.CheckedChanged += new System.EventHandler(LoggingSwitch_CheckedChanged);

            // Wire live-apply handlers for controls that the Designer does not already wire.
            closeToTraySwitch.CheckedChanged += new System.EventHandler(CloseToTraySwitch_CheckedChanged);
            autoAllowWhitelistedPublishersCheck.CheckedChanged += new System.EventHandler(AutoAllowWhitelistedPublishersCheck_CheckedChanged);
            autoAllowSystemSignedAppsCheck.CheckedChanged += new System.EventHandler(AutoAllowSystemSignedAppsCheck_CheckedChanged);
            auditAlertsSwitch.CheckedChanged += new System.EventHandler(AuditAlertsSwitch_CheckedChanged);
            autoRefreshTextBox.Leave += new System.EventHandler(AutoRefreshTextBox_Leave);

            if (_appImageList != null && _appImageList.Images.ContainsKey("coffee.png"))
            {
                coffeePictureBox.Image = _appImageList.Images["coffee.png"];
            }
        }

        public void ApplyThemeFixes()
        {
            void StyleButton(Button btn)
            {
                btn.FlatAppearance.BorderSize = 1;
                btn.FlatAppearance.BorderColor = Theme.Colors.ControlDark;
                btn.ForeColor = Theme.Colors.TextActive;
            }

            var buttonsToStyle = new[] {
                deleteAllRulesButton, revertFirewallButton, managePublishersButton,
                openFirewallButton, openAppDataButton, checkForUpdatesButton,
                cleanUpOrphanedRulesButton, exportRulesButton, importMergeButton,
                importReplaceButton, exportDiagnosticButton, viewTrustedCertsButton,
                excludedFoldersButton
            };

            foreach (var btn in buttonsToStyle)
            {
                StyleButton(btn);
            }

            // Apply theme to dividers
            dividerPanel1.BackColor = Theme.Colors.ControlDark;
            dividerPanel2.BackColor = Theme.Colors.ControlDark;
        }

        public void LoadSettingsToUI()
        {
            if (_appSettings == null)
            {
                return;
            }

            closeToTraySwitch.Checked = _appSettings.CloseToTray;
            startOnStartupSwitch.Checked = _appSettings.StartOnSystemStartup;
            autoThemeSwitch.Checked = _appSettings.Theme == "Auto";
            darkModeSwitch.Checked = _appSettings.Theme == "Dark" || (_appSettings.Theme == "Auto" && Theme.IsSystemDarkMode());
            darkModeSwitch.Enabled = !autoThemeSwitch.Checked;
            popupsSwitch.Checked = _appSettings.IsPopupsEnabled;
            loggingSwitch.Checked = _appSettings.IsLoggingEnabled;
            autoRefreshTextBox.Text = _appSettings.AutoRefreshIntervalMinutes.ToString();
            trafficMonitorSwitch.Checked = _appSettings.IsTrafficMonitorEnabled;
            showAppIconsSwitch.Checked = _appSettings.ShowAppIcons;
            autoAllowWhitelistedPublishersCheck.Checked = _appSettings.AutoAllowWhitelistedPublishers;
            autoAllowSystemSignedAppsCheck.Checked = _appSettings.AutoAllowSystemSignedApps;
            auditAlertsSwitch.Checked = _appSettings.AlertOnForeignRules;
            managePublishersButton.Enabled = true;
        }

        public void SaveSettingsFromUI()
        {
            if (_appSettings == null)
            {
                return;
            }

            _appSettings.CloseToTray = closeToTraySwitch.Checked;
            _appSettings.StartOnSystemStartup = startOnStartupSwitch.Checked;
            // Preserve "Auto" theme — the live AutoThemeSwitch_CheckedChanged handler already keeps
            // _appSettings.Theme in sync; this line only runs on form close and must not clobber it.
			_appSettings.Theme = autoThemeSwitch.Checked ? "Auto" : (darkModeSwitch.Checked ? "Dark" : "Light");
            _appSettings.IsPopupsEnabled = popupsSwitch.Checked;
            _appSettings.IsLoggingEnabled = loggingSwitch.Checked;

            if (int.TryParse(autoRefreshTextBox.Text, out int val) && val >= 1)
            {
                _appSettings.AutoRefreshIntervalMinutes = val;
            }

            _appSettings.IsTrafficMonitorEnabled = trafficMonitorSwitch.Checked;
            _appSettings.ShowAppIcons = showAppIconsSwitch.Checked;
            _appSettings.AutoAllowWhitelistedPublishers = autoAllowWhitelistedPublishersCheck.Checked;
            _appSettings.AutoAllowSystemSignedApps = autoAllowSystemSignedAppsCheck.Checked;
            _appSettings.AlertOnForeignRules = auditAlertsSwitch.Checked;

            _activityLogger.IsEnabled = _appSettings.IsLoggingEnabled;

            if (!_appSettings.IsTrafficMonitorEnabled)
            {
                _mainViewModel?.TrafficMonitorViewModel?.StopMonitoring();
            }

            IconVisibilityChanged?.Invoke();
            AutoRefreshTimerChanged?.Invoke();
            _appSettings.Save();
        }

        public void ApplyTheme(bool isDark)
        {
            foreach (var link in new[] { helpLink, reportProblemLink, forumLink, coffeeLinkLabel })
            {
                link.LinkColor = Theme.Colors.LinkText;
                link.VisitedLinkColor = Theme.Colors.LinkText;
            }

            if (_appImageList != null && _appImageList.Images.ContainsKey("coffee.png"))
            {
                Image? coffeeImage = _appImageList.Images["coffee.png"]; 
                if (coffeeImage != null)
                {
                    Image? oldImage = coffeePictureBox.Image;
                    coffeePictureBox.Image = RecolorImage(coffeeImage, Theme.Colors.GraphicAccent);

                    // Only dispose if it's not the original resource from ImageList
                    if (oldImage != null && oldImage != coffeeImage)
                    {
                        oldImage.Dispose();
                    }
                }
            }
        }

        private Image RecolorImage(Image image, Color color)
        {
            var bmp = new Bitmap(image.Width, image.Height);
            using var g = Graphics.FromImage(bmp);
            float r = color.R / 255f, cg = color.G / 255f, b = color.B / 255f;
            var matrix = new System.Drawing.Imaging.ColorMatrix([
                [0, 0, 0, 0, 0],
                [0, 0, 0, 0, 0],
                [0, 0, 0, 0, 0],
                [0, 0, 0, 1, 0],
                [r, cg, b, 0, 1]
            ]);
            using var attr = new System.Drawing.Imaging.ImageAttributes();
            attr.SetColorMatrix(matrix);
            g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attr);
            return bmp;
        }

        private void AutoThemeSwitch_CheckedChanged(object? sender, EventArgs e)
        {
            darkModeSwitch.Enabled = !autoThemeSwitch.Checked;
            if (_appSettings != null)
            {
                _appSettings.Theme = autoThemeSwitch.Checked ? "Auto" : (darkModeSwitch.Checked ? "Dark" : "Light");
                ThemeChanged?.Invoke();
            }
        }

        private void DarkModeSwitch_CheckedChanged(object? sender, EventArgs e)
        {
            if (_appSettings != null && !autoThemeSwitch.Checked)
            {
                _appSettings.Theme = darkModeSwitch.Checked ? "Dark" : "Light";
                ThemeChanged?.Invoke();
            }
        }

        private void StartOnStartupSwitch_CheckedChanged(object? sender, EventArgs e)
        {
            if (_appSettings != null && _startupService != null)
            {
                _appSettings.StartOnSystemStartup = startOnStartupSwitch.Checked;
                _startupService.SetStartup(_appSettings.StartOnSystemStartup);
            }
        }

        private void PopupsSwitch_CheckedChanged(object? sender, EventArgs e)
        {
            _appSettings?.IsPopupsEnabled = popupsSwitch.Checked;
        }

        private void LoggingSwitch_CheckedChanged(object? sender, EventArgs e)
        {
            if (_appSettings != null)
            {
                _appSettings.IsLoggingEnabled = loggingSwitch.Checked;
                _activityLogger.IsEnabled = _appSettings.IsLoggingEnabled;
            }
        }

        private void TrafficMonitorSwitch_CheckedChanged(object? sender, EventArgs e)
        {
            if (_appSettings != null)
            {
                _appSettings.IsTrafficMonitorEnabled = trafficMonitorSwitch.Checked;
                TrafficMonitorSettingChanged?.Invoke();
            }
        }

        private void ShowAppIconsSwitch_CheckedChanged(object? sender, EventArgs e)
        {
            if (_appSettings != null)
            {
                _appSettings.ShowAppIcons = showAppIconsSwitch.Checked;
                IconVisibilityChanged?.Invoke();
            }
        }

        private void CloseToTraySwitch_CheckedChanged(object? sender, EventArgs e)
        {
            _appSettings?.CloseToTray = closeToTraySwitch.Checked;
        }

        private void AutoAllowWhitelistedPublishersCheck_CheckedChanged(object? sender, EventArgs e)
        {
            _appSettings?.AutoAllowWhitelistedPublishers = autoAllowWhitelistedPublishersCheck.Checked;
        }

        private void AutoAllowSystemSignedAppsCheck_CheckedChanged(object? sender, EventArgs e)
        {
            _appSettings?.AutoAllowSystemSignedApps = autoAllowSystemSignedAppsCheck.Checked;
        }


        private void AuditAlertsSwitch_CheckedChanged(object? sender, EventArgs e)
        {
            _appSettings?.AlertOnForeignRules = auditAlertsSwitch.Checked;
        }

        private void AutoRefreshTextBox_Leave(object? sender, EventArgs e)
        {
            if (_appSettings == null)
            {
                return;
            }

            if (int.TryParse(autoRefreshTextBox.Text, out int val) && val >= 1)
            {
                if (val != _appSettings.AutoRefreshIntervalMinutes)
                {
                    _appSettings.AutoRefreshIntervalMinutes = val;
                    AutoRefreshTimerChanged?.Invoke();
                }
            }
            else
            {
                // Invalid input — revert the UI to the stored value.
                autoRefreshTextBox.Text = _appSettings.AutoRefreshIntervalMinutes.ToString();
            }
        }

        private void ManagePublishersButton_Click(object? sender, EventArgs e)
        {
            using var form = new ManagePublishersForm(_whitelistService, _appSettings);
            form.ShowDialog(FindForm());
        }

        private void ViewTrustedCertsButton_Click(object? sender, EventArgs e)
        {
            using var form = new TrustedCertificatesForm(_appSettings);
            form.ShowDialog(FindForm());
        }

        private void ExcludedFoldersButton_Click(object? sender, EventArgs e)
        {
            using var form = new ExcludedFoldersForm(_appSettings);
            form.ShowDialog(FindForm());
        }

        private void OpenFirewallButton_Click(object? sender, EventArgs e)
        {
            string wfPath = Path.Combine(Environment.SystemDirectory, "wf.msc");
            OpenLink(wfPath, "Windows Firewall console");
        }

        private void OpenAppDataButton_Click(object? sender, EventArgs e)
        {
            string standardAppDataPath = ConfigPathManager.GetStandardAppDataDirectory();
            if (!Directory.Exists(standardAppDataPath))
            {
                Directory.CreateDirectory(standardAppDataPath);
            }

            OpenLink(standardAppDataPath, "%AppData% folder");
        }

        private void CheckForUpdatesButton_Click(object? sender, EventArgs e)
        {
            OpenLink("https://github.com/deminimis/minimalfirewall/releases");
        }

        private void LinkLabel_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
        {
            if (sender is not LinkLabel { Tag: string url })
            {
                return;
            }

            OpenLink(url);
        }

        private void CoffeeLink_Click(object? sender, EventArgs e)
        {
            OpenLink("https://www.buymeacoffee.com/deminimis");
        }

        private static void OpenLink(string target, string errorContext = "the link")
        {
            try
            {
                Process.Start(new ProcessStartInfo(target) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Messenger.MessageBox($"Could not open {errorContext}.\n\nError: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CoffeePictureBox_MouseEnter(object? sender, EventArgs e) { }
        private void CoffeePictureBox_MouseLeave(object? sender, EventArgs e) { }

        private async void DeleteAllRulesButton_Click(object? sender, EventArgs e)
        {
            var result = Messenger.MessageBox("This will permanently delete all firewall rules created by this application. This action cannot be undone. Are you sure you want to continue?",
                "Delete All Rules", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    _actionsService.DeleteAllMfwRules();
                    await (DataRefreshRequested?.Invoke() ?? Task.CompletedTask);
                    Messenger.MessageBox("All Minimal Firewall rules have been deleted.", "Operation Complete", MessageBoxButtons.OK, MessageBoxIcon.None);
                }
                catch (Exception ex)
                {
                    _activityLogger?.LogException("DeleteAllRules", ex);
                    Messenger.MessageBox($"Failed to delete rules: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void RevertFirewallButton_Click(object? sender, EventArgs e)
        {
            var result = Messenger.MessageBox("WARNING: This will reset your ENTIRE Windows Firewall configuration to its default state. " +
                "All custom rules, including those not created by this application, will be deleted. This action is irreversible.\n\n" +
                "Are you absolutely sure you want to continue?",
                "Revert Windows Firewall Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    AdminTaskService.ResetFirewall();
                    await (DataRefreshRequested?.Invoke() ?? Task.CompletedTask);
                    Messenger.MessageBox("Windows Firewall has been reset to its default settings. It is recommended to restart the application.",
                        "Operation Complete", MessageBoxButtons.OK, MessageBoxIcon.None);
                }
                catch (Exception ex)
                {
                    _activityLogger?.LogException("RevertFirewall", ex);
                    Messenger.MessageBox($"Failed to reset firewall: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void CleanUpOrphanedRulesButton_Click(object? sender, EventArgs e)
        {
            var result = Messenger.MessageBox(
                "This will scan for rules whose associated application no longer exists on disk and delete them.\n\nAre you sure you want to continue?",
                "Clean Up Orphaned Rules",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    await _mainViewModel.CleanUpOrphanedRulesAsync();
                }
                catch (Exception ex)
                {
                    _activityLogger?.LogException("CleanOrphanedRules", ex);
                    Messenger.MessageBox($"Failed to clean rules: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void ExportRulesButton_Click(object? sender, EventArgs e)
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

        private async Task ProcessRuleImportAsync(bool replace)
        {
            using var openDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = replace ? "Import and Replace Rules" : "Import and Add Rules"
            };

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string jsonContent = await File.ReadAllTextAsync(openDialog.FileName);
                    await _actionsService.ImportRulesAsync(jsonContent, replace);
                    await (DataRefreshRequested?.Invoke() ?? Task.CompletedTask);

                    string successMsg = replace ? "All rules have been replaced successfully." : "Rules have been successfully imported and added.";
                    Messenger.MessageBox(successMsg, "Import Complete", MessageBoxButtons.OK, MsgIcon.Success);
                }
                catch (Exception ex)
                {
                    Messenger.MessageBox($"An error occurred during import: {ex.Message}", "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void ImportMergeButton_Click(object? sender, EventArgs e)
        {
            await ProcessRuleImportAsync(replace: false);
        }

        private async void ImportReplaceButton_Click(object? sender, EventArgs e)
        {
            var confirmResult = Messenger.MessageBox(
                "WARNING: This will delete ALL current Minimal Firewall rules before importing the new ones. This action cannot be undone.\n\nAre you sure you want to continue?",
                "Confirm Replace", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirmResult == DialogResult.Yes)
            {
                await ProcessRuleImportAsync(replace: true);
            }
        }

        private static async Task<string> ReadLastNLinesAsync(string filePath, int n)
        {
            if (!File.Exists(filePath))
            {
                return $"{Path.GetFileName(filePath)} not found.";
            }

            try
            {
                var lineQueue = new Queue<string>(n);

                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(fileStream))
                {
                    string? line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        if (lineQueue.Count >= n)
                        {
                            lineQueue.Dequeue();
                        }
                        lineQueue.Enqueue(line);
                    }
                }
                return string.Join(Environment.NewLine, lineQueue);
            }
            catch (Exception ex)
            {
                return $"Error reading {Path.GetFileName(filePath)}: {ex.Message}";
            }
        }

        private async void ExportDiagnosticButton_Click(object? sender, EventArgs e)
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
                statusForm.Show(FindForm());
                Application.DoEvents(); 

                try
                {
                    if (File.Exists(zipPath))
                    {
                        File.Delete(zipPath);
                    }

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
