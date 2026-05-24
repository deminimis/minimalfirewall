using DarkModeForms;

namespace MinimalFirewall
{
    public partial class CreateProgramRuleForm : Form
    {
        private readonly string[] _filePaths;
        private readonly FirewallActionsService _actionsService;


        public CreateProgramRuleForm(string[] filePaths, FirewallActionsService actionsService)
        {
            InitializeComponent();

            bool isDark = Theme.IsSystemDarkMode();
            Theme.Colors = Theme.GetSystemColors(isDark ? 0 : 1);
            Theme.ApplyTitleBarTheme(this.Handle, isDark ? Theme.DisplayMode.DarkMode : Theme.DisplayMode.ClearMode);
            this.BackColor = Theme.Colors.Background;
            this.ForeColor = Theme.Colors.TextInactive;

            var styler = new ControlStyler(Theme.Colors, isDark);
            styler.ApplyStyle(this);

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

        private void OkButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Determine action and direction based on user selection
                string action = allowRadio.Checked ? "Allow" : "Block";
                string direction = allowRadio.Checked ? allowDirectionCombo.Text : blockDirectionCombo.Text;
                string finalAction = $"{action} ({direction})";

                _actionsService.ApplyApplicationRuleChange([.. _filePaths], finalAction);
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying rule: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
