// File: CreateProgramRuleForm.cs
using DarkModeForms;

namespace MinimalFirewall
{
    public partial class CreateProgramRuleForm : Form
    {
        private readonly string[] _filePaths;
        private readonly FirewallActionsService _actionsService;
        private readonly DarkModeCS dm;

        public CreateProgramRuleForm(string[] filePaths, FirewallActionsService actionsService)
        {
            InitializeComponent();
            dm = new DarkModeCS(this);
            _filePaths = filePaths;
            _actionsService = actionsService;

            // AutoEllipsis for long paths
            programListLabel.AutoEllipsis = true;
            programListLabel.Text = filePaths.Length == 1
                ? $"Program: {System.IO.Path.GetFileName(filePaths[0])}"
                : $"{filePaths.Length} programs selected.";

            // Set default selections to avoid empty input errors
            allowDirectionCombo.SelectedIndex = 0; 
            blockDirectionCombo.SelectedIndex = 0; 

            // Bind radio button events to toggle combo box availability
            allowRadio.CheckedChanged += (s, e) => UpdateUiState();
            blockRadio.CheckedChanged += (s, e) => UpdateUiState();

            UpdateUiState();
        }

        private void UpdateUiState()
        {
            allowDirectionCombo.Enabled = allowRadio.Checked;
            blockDirectionCombo.Enabled = blockRadio.Checked;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Determine action and direction based on user selection
                string action = allowRadio.Checked ? "Allow" : "Block";
                string direction = allowRadio.Checked ? allowDirectionCombo.Text : blockDirectionCombo.Text;
                string finalAction = $"{action} ({direction})";

                _actionsService.ApplyApplicationRuleChange(new List<string>(_filePaths), finalAction);
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying rule: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}