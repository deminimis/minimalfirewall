// File: WildcardCreatorForm.cs
using System.IO;
using DarkModeForms;
using MinimalFirewall.TypedObjects;
using System.ComponentModel;

namespace MinimalFirewall
{
    public partial class WildcardCreatorForm : Form
    {
        private readonly WildcardRuleService _wildcardRuleService;
        private readonly DarkModeCS dm;
        private readonly WildcardRule? _originalRule;

        public WildcardRule NewRule { get; private set; } = new();

        public WildcardCreatorForm(WildcardRuleService wildcardRuleService, AppSettings appSettings, WildcardRule? ruleToEdit = null)
        {
            InitializeComponent();
            dm = new DarkModeCS(this);
            dm.ColorMode = appSettings.Theme == "Dark" ? DarkModeCS.DisplayMode.DarkMode : DarkModeCS.DisplayMode.ClearMode;
            dm.ApplyTheme(appSettings.Theme == "Dark");
            _wildcardRuleService = wildcardRuleService;
            directionCombo.SelectedIndex = 0;
            protocolComboBox.Items.AddRange(new object[] { "Any", "TCP", "UDP" });
            protocolComboBox.SelectedItem = "Any";

            _originalRule = ruleToEdit;
            if (_originalRule != null)
            {
                this.Text = "Edit Wildcard Rule";
                PopulateForm(_originalRule);
            }
        }

        public WildcardCreatorForm(WildcardRuleService wildcardRuleService, string initialAppPath, AppSettings appSettings) : this(wildcardRuleService, appSettings)
        {
            string? dirPath = Path.GetDirectoryName(initialAppPath);
            if (!string.IsNullOrEmpty(dirPath) && Directory.Exists(dirPath))
            {
                folderPathTextBox.Text = dirPath;
                exeNameTextBox.Text = Path.GetFileName(initialAppPath);
            }
        }

        private void PopulateForm(WildcardRule rule)
        {
            folderPathTextBox.Text = rule.FolderPath;
            exeNameTextBox.Text = rule.ExeName;

            if (rule.Action.StartsWith("Allow"))
            {
                allowRadio.Checked = true;
            }
            else
            {
                blockRadio.Checked = true;
            }

            if (rule.Action.Contains("Inbound"))
            {
                directionCombo.SelectedItem = "Inbound";
            }
            else if (rule.Action.Contains("All"))
            {
                directionCombo.SelectedItem = "All";
            }
            else
            {
                directionCombo.SelectedItem = "Outbound";
            }

            switch (rule.Protocol)
            {
                case 6:
                    protocolComboBox.SelectedItem = "TCP";
                    break;
                case 17:
                    protocolComboBox.SelectedItem = "UDP";
                    break;
                default:
                    protocolComboBox.SelectedItem = "Any";
                    break;
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
            string folderPath = folderPathTextBox.Text;
            string expandedPath = Environment.ExpandEnvironmentVariables(folderPath);
            errorProvider1.SetError(folderPathTextBox, string.Empty);
            errorProvider1.SetError(localPortsTextBox, string.Empty);
            errorProvider1.SetError(remotePortsTextBox, string.Empty);
            errorProvider1.SetError(remoteAddressTextBox, string.Empty);

            if (string.IsNullOrWhiteSpace(folderPath))
            {
                errorProvider1.SetError(folderPathTextBox, "Folder path cannot be empty.");
                return;
            }

            if (!Directory.Exists(expandedPath))
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

            NewRule.FolderPath = PathResolver.NormalizePath(folderPath);
            NewRule.ExeName = exeNameTextBox.Text;
            string action = allowRadio.Checked ? "Allow" : "Block";
            string direction = directionCombo.Text;
            NewRule.Action = $"{action} ({direction})";

            NewRule.Protocol = protocolComboBox.SelectedItem switch
            {
                "TCP" => 6,
                "UDP" => 17,
                _ => 256,
            };

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

        private void localPortsTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (!ValidationUtility.ValidatePortString(textBox.Text, out string errorMessage))
                {
                    errorProvider1.SetError(textBox, errorMessage);
                    e.Cancel = true;
                }
                else
                {
                    errorProvider1.SetError(textBox, string.Empty);
                }
            }
        }

        private void remotePortsTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (!ValidationUtility.ValidatePortString(textBox.Text, out string errorMessage))
                {
                    errorProvider1.SetError(textBox, errorMessage);
                    e.Cancel = true;
                }
                else
                {
                    errorProvider1.SetError(textBox, string.Empty);
                }
            }
        }

        private void remoteAddressTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (!ValidationUtility.ValidateAddressString(textBox.Text, out string errorMessage))
                {
                    errorProvider1.SetError(textBox, errorMessage);
                    e.Cancel = true;
                }
                else
                {
                    errorProvider1.SetError(textBox, string.Empty);
                }
            }
        }
    }
}