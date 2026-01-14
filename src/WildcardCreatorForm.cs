using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using DarkModeForms;
using MinimalFirewall.TypedObjects;

namespace MinimalFirewall
{
    public partial class WildcardCreatorForm : Form
    {
        private readonly DarkModeCS dm;

        public WildcardRule NewRule { get; private set; } = new();

        public WildcardCreatorForm(WildcardRuleService wildcardRuleService, AppSettings appSettings, WildcardRule? ruleToEdit = null)
        {
            InitializeComponent();

            // Apply Theme
            dm = new DarkModeCS(this);
            dm.ColorMode = appSettings.Theme == "Dark" ? DarkModeCS.DisplayMode.DarkMode : DarkModeCS.DisplayMode.ClearMode;
            dm.ApplyTheme(appSettings.Theme == "Dark");


            directionCombo.SelectedIndex = 0; // Default to first item
            protocolComboBox.Items.Clear();
            protocolComboBox.Items.AddRange(new object[] { "Any", "TCP", "UDP" });
            protocolComboBox.SelectedItem = "Any";

            if (ruleToEdit != null)
            {
                this.Text = "Edit Wildcard Rule";
                NewRule = ruleToEdit;
                PopulateForm(ruleToEdit);
            }
        }

        public WildcardCreatorForm(WildcardRuleService wildcardRuleService, string initialAppPath, AppSettings appSettings)
            : this(wildcardRuleService, appSettings)
        {
            try
            {
                string? dirPath = Path.GetDirectoryName(initialAppPath);
                if (!string.IsNullOrEmpty(dirPath))
                {
                    folderPathTextBox.Text = dirPath;
                }
                exeNameTextBox.Text = Path.GetFileName(initialAppPath);
            }
            catch { /* Ignore path parsing errors on init */ }
        }

        private void PopulateForm(WildcardRule rule)
        {
            folderPathTextBox.Text = rule.FolderPath;
            exeNameTextBox.Text = rule.ExeName;

            if (rule.Action.StartsWith("Allow", StringComparison.OrdinalIgnoreCase))
            {
                allowRadio.Checked = true;
            }
            else
            {
                blockRadio.Checked = true;
            }

            if (rule.Action.Contains("Inbound", StringComparison.OrdinalIgnoreCase))
            {
                directionCombo.SelectedItem = "Inbound";
            }
            else if (rule.Action.Contains("All", StringComparison.OrdinalIgnoreCase))
            {
                directionCombo.SelectedItem = "All";
            }
            else
            {
                directionCombo.SelectedItem = "Outbound";
            }

            switch (rule.Protocol)
            {
                case 6: protocolComboBox.SelectedItem = "TCP"; break;
                case 17: protocolComboBox.SelectedItem = "UDP"; break;
                default: protocolComboBox.SelectedItem = "Any"; break;
            }

            localPortsTextBox.Text = rule.LocalPorts;
            remotePortsTextBox.Text = rule.RemotePorts;
            remoteAddressTextBox.Text = rule.RemoteAddresses;
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    folderPathTextBox.Text = dialog.SelectedPath;
                }
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            errorProvider1.SetError(folderPathTextBox, string.Empty);
            errorProvider1.SetError(localPortsTextBox, string.Empty);
            errorProvider1.SetError(remotePortsTextBox, string.Empty);
            errorProvider1.SetError(remoteAddressTextBox, string.Empty);

            string folderPath = folderPathTextBox.Text;
            string expandedPath = Environment.ExpandEnvironmentVariables(folderPath);

            // --- Validation Logic ---

            if (string.IsNullOrWhiteSpace(folderPath))
            {
                errorProvider1.SetError(folderPathTextBox, "Folder path cannot be empty.");
                return;
            }

            bool isWildcardPath = folderPath.Contains('*') || folderPath.Contains('?');
            if (!isWildcardPath && !Directory.Exists(expandedPath))
            {
                errorProvider1.SetError(folderPathTextBox, "The specified directory does not exist.");
                return;
            }

            if (!ValidationUtility.ValidatePortString(localPortsTextBox.Text, out string localPortError))
            {
                errorProvider1.SetError(localPortsTextBox, localPortError);
                return;
            }

            if (!ValidationUtility.ValidatePortString(remotePortsTextBox.Text, out string remotePortError))
            {
                errorProvider1.SetError(remotePortsTextBox, remotePortError);
                return;
            }

            if (!ValidationUtility.ValidateAddressString(remoteAddressTextBox.Text, out string remoteAddressError))
            {
                errorProvider1.SetError(remoteAddressTextBox, remoteAddressError);
                return;
            }

            // --- Object Creation ---

            NewRule.FolderPath = PathResolver.NormalizePath(folderPath);
            NewRule.ExeName = exeNameTextBox.Text;

            string action = allowRadio.Checked ? "Allow" : "Block";
            string direction = directionCombo.Text;
            NewRule.Action = $"{action} ({direction})";

            NewRule.Protocol = protocolComboBox.SelectedItem?.ToString() switch
            {
                "TCP" => 6,
                "UDP" => 17,
                _ => 256, 
            };

            // Use "*" if empty
            NewRule.LocalPorts = string.IsNullOrWhiteSpace(localPortsTextBox.Text) ? "*" : localPortsTextBox.Text;
            NewRule.RemotePorts = string.IsNullOrWhiteSpace(remotePortsTextBox.Text) ? "*" : remotePortsTextBox.Text;
            NewRule.RemoteAddresses = string.IsNullOrWhiteSpace(remoteAddressTextBox.Text) ? "*" : remoteAddressTextBox.Text;

            DialogResult = DialogResult.OK;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void advancedButton_Click(object sender, EventArgs e)
        {
            bool isVisible = advancedGroupBox.Visible;
            advancedGroupBox.Visible = !isVisible;
            this.Height += isVisible ? -advancedGroupBox.Height : advancedGroupBox.Height;
        }

        // --- Non-Blocking Validation Events ---

        private void localPortsTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (!ValidationUtility.ValidatePortString(textBox.Text, out string errorMessage))
                    errorProvider1.SetError(textBox, errorMessage);
                else
                    errorProvider1.SetError(textBox, string.Empty);
            }
        }

        private void remotePortsTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (!ValidationUtility.ValidatePortString(textBox.Text, out string errorMessage))
                    errorProvider1.SetError(textBox, errorMessage);
                else
                    errorProvider1.SetError(textBox, string.Empty);
            }
        }

        private void remoteAddressTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (!ValidationUtility.ValidateAddressString(textBox.Text, out string errorMessage))
                    errorProvider1.SetError(textBox, errorMessage);
                else
                    errorProvider1.SetError(textBox, string.Empty);
            }
        }
    }
}