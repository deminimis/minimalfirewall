// File: WildcardCreatorForm.cs
using System.IO;
using DarkModeForms;

namespace MinimalFirewall
{
    public partial class WildcardCreatorForm : Form
    {
        private readonly WildcardRuleService _wildcardRuleService;
        private string _folderPath = string.Empty;
        private readonly DarkModeCS dm;

        public string FolderPath { get; private set; } = string.Empty;
        public string ExeName { get; private set; } = string.Empty;
        public string FinalAction { get; private set; } = string.Empty;
        public WildcardCreatorForm(WildcardRuleService wildcardRuleService)
        {
            InitializeComponent();
            dm = new DarkModeCS(this);
            _wildcardRuleService = wildcardRuleService;

            allowDirectionCombo.SelectedIndex = 0;
            blockDirectionCombo.SelectedIndex = 0;
        }

        public WildcardCreatorForm(WildcardRuleService wildcardRuleService, string initialAppPath) : this(wildcardRuleService)
        {
            string? dirPath = Path.GetDirectoryName(initialAppPath);
            if (!string.IsNullOrEmpty(dirPath) && Directory.Exists(dirPath))
            {
                _folderPath = dirPath;
                folderPathTextBox.Text = _folderPath;
                exeNameTextBox.Text = Path.GetFileName(initialAppPath);
            }
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _folderPath = dialog.SelectedPath;
                    folderPathTextBox.Text = _folderPath;
                }
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            _folderPath = folderPathTextBox.Text;
            string expandedPath = Environment.ExpandEnvironmentVariables(_folderPath);
            if (string.IsNullOrWhiteSpace(_folderPath) || !Directory.Exists(expandedPath))
            {
                Messenger.MessageBox("Please select or enter a valid folder path.", "Invalid Path", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.FolderPath = PathResolver.NormalizePath(_folderPath);
            this.ExeName = exeNameTextBox.Text;
            string action = allowRadio.Checked ? "Allow" : "Block";
            string direction = allowRadio.Checked ? allowDirectionCombo.Text : blockDirectionCombo.Text;
            this.FinalAction = $"{action} ({direction})";
            DialogResult = DialogResult.OK;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}