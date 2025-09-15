// File: AddRuleSelectionForm.cs
using DarkModeForms;

namespace MinimalFirewall
{
    public partial class AddRuleSelectionForm : Form
    {
        private readonly FirewallActionsService _actionsService;
        private readonly WildcardRuleService _wildcardRuleService;
        private readonly DarkModeCS dm;

        public AddRuleSelectionForm(FirewallActionsService actionsService, WildcardRuleService wildcardRuleService)
        {
            InitializeComponent();
            dm = new DarkModeCS(this);
            _actionsService = actionsService;
            _wildcardRuleService = wildcardRuleService;
        }

        private void ProgramRuleButton_Click(object sender, EventArgs e)
        {
            using var openFileDialog = new OpenFileDialog
            {
                Filter = "Executables (*.exe)|*.exe|All files (*.*)|*.*",
                Multiselect = true,
                Title = "Select one or more programs"
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                using var programRuleDialog = new CreateProgramRuleForm(openFileDialog.FileNames, _actionsService);
                if (programRuleDialog.ShowDialog(this) == DialogResult.OK)
                {
                    this.DialogResult = DialogResult.OK;
                }
            }
        }

        private void WildcardRuleButton_Click(object sender, EventArgs e)
        {
            using var wildcardDialog = new WildcardCreatorForm(_wildcardRuleService);
            if (wildcardDialog.ShowDialog(this) == DialogResult.OK)
            {
                this.DialogResult = DialogResult.OK;
            }
        }
    }
}