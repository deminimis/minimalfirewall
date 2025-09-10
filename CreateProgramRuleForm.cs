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
            programListLabel.Text = filePaths.Length == 1
                ? $"Program: {System.IO.Path.GetFileName(filePaths[0])}"
                : $"{filePaths.Length} programs selected.";
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            string action = allowRadio.Checked ? "Allow" : "Block";
            string direction = allowRadio.Checked ? allowDirectionCombo.Text : blockDirectionCombo.Text;
            string finalAction = $"{action} ({direction})";

            _actionsService.ApplyApplicationRuleChange([.. _filePaths], finalAction);
            DialogResult = DialogResult.OK;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
