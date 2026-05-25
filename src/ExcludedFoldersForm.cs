using DarkModeForms;

namespace MinimalFirewall
{
    public partial class ExcludedFoldersForm : Form
    {
        private readonly AppSettings _appSettings;

        public ExcludedFoldersForm(AppSettings appSettings)
        {
            InitializeComponent();

            _appSettings = appSettings;

            bool isDark = appSettings.Theme == "Dark" || (appSettings.Theme == "Auto" && Theme.IsSystemDarkMode());
            Theme.Colors = Theme.GetSystemColors(isDark ? 0 : 1);
            Theme.ApplyTitleBarTheme(this.Handle, isDark ? Theme.DisplayMode.DarkMode : Theme.DisplayMode.ClearMode);
            this.BackColor = Theme.Colors.Background;
            this.ForeColor = Theme.Colors.TextInactive;


            LoadFolders();
        }

        private void LoadFolders()
        {
            foldersListBox.BeginUpdate();
            try
            {
                foldersListBox.Items.Clear();
                var folders = _appSettings.AutoAllowExclusions ?? [];
                foreach (var folder in folders.OrderBy(f => f))
                {
                    foldersListBox.Items.Add(folder);
                }
            }
            finally
            {
                foldersListBox.EndUpdate();
            }

            UpdateUIState();
        }

        private void UpdateUIState()
        {
            removeButton.Enabled = foldersListBox.SelectedItems.Count > 0;
        }

        private void FoldersListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateUIState();
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Select a folder to exclude from auto-allow",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string selectedPath = dialog.SelectedPath;

                var folders = _appSettings.AutoAllowExclusions;
                if (folders == null)
                {
                    folders = [];
                    _appSettings.AutoAllowExclusions = folders;
                }

                if (!folders.Any(f => string.Equals(f, selectedPath, StringComparison.OrdinalIgnoreCase)))
                {
                    folders.Add(selectedPath);
                    _appSettings.QueueSave();
                    LoadFolders();
                }
            }
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            if (foldersListBox.SelectedItems.Count > 0)
            {
                int count = foldersListBox.SelectedItems.Count;
                string message = count == 1
                    ? $"Remove '{foldersListBox.SelectedItem}' from the excluded folders list?"
                    : $"Remove {count} folders from the excluded folders list?";

                var result = MessageBox.Show(message, "Confirm Removal", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    var selectedItems = foldersListBox.SelectedItems.Cast<string>().ToList();
                    var folders = _appSettings.AutoAllowExclusions;

                    foreach (var folder in selectedItems)
                    {
                        folders.Remove(folder);
                    }

                    _appSettings.QueueSave();
                    LoadFolders();
                }
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
