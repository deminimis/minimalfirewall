// WildcardCreatorForm.cs
using System.IO;
using DarkModeForms;

namespace MinimalFirewall
{
    public partial class WildcardCreatorForm : Form
    {
        private readonly WildcardRuleService _wildcardRuleService;
        private string _folderPath = string.Empty;
        private readonly DarkModeCS dm;

        public WildcardCreatorForm(WildcardRuleService wildcardRuleService)
        {
            InitializeComponent();
            dm = new DarkModeCS(this);
            _wildcardRuleService = wildcardRuleService;
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
            if (string.IsNullOrWhiteSpace(_folderPath) || !Directory.Exists(_folderPath))
            {
                Messenger.MessageBox("Please select a valid folder path.", "Invalid Path", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string action = allowRadio.Checked ? "Allow" : "Block";
            string direction = allowRadio.Checked ? allowDirectionCombo.Text : blockDirectionCombo.Text;
            string finalAction = $"{action} ({direction})";
            var newRule = new WildcardRule
            {
                FolderPath = _folderPath,
                ExeName = exeNameTextBox.Text,
                Action = finalAction
            };
            _wildcardRuleService.AddRule(newRule);

            DialogResult = DialogResult.OK;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
