using DarkModeForms;

namespace MinimalFirewall
{
    public partial class ExcludedFoldersForm : Form
    {
        private readonly AppSettings _appSettings;
        private readonly DarkModeCS dm;

        public ExcludedFoldersForm(AppSettings appSettings)
        {
            InitializeComponent();

            _appSettings = appSettings;

            dm = new DarkModeCS(this);
            dm.ColorMode = appSettings.Theme == "Dark" ? DarkModeCS.DisplayMode.DarkMode : DarkModeCS.DisplayMode.ClearMode;
            dm.ApplyTheme(appSettings.Theme == "Dark");

            LoadFolders();
        }

        private void LoadFolders()
        {
            foldersListBox.BeginUpdate();
            try
            {
                foldersListBox.Items.Clear();
                var folders = _appSettings.AutoAllowExclusions ?? new List<string>();
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

        private void foldersListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateUIState();
        }

        private void addButton_Click(object sender, EventArgs e)
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
                    folders = new List<string>();
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

        private void removeButton_Click(object sender, EventArgs e)
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

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
