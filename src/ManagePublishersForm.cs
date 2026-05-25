
using System.Data; // Required for LINQ extensions if not globally imported
using DarkModeForms;

namespace MinimalFirewall
{
    // Visual form to manage publishers
    public partial class ManagePublishersForm : Form
    {
        private readonly PublisherWhitelistService _whitelistService;

        public ManagePublishersForm(PublisherWhitelistService whitelistService, AppSettings appSettings)
        {
            InitializeComponent();

            bool isDark = appSettings.Theme == "Dark" || (appSettings.Theme == "Auto" && Theme.IsSystemDarkMode());
            Theme.Colors = Theme.GetSystemColors(isDark ? 0 : 1);
            Theme.ApplyTitleBarTheme(Handle, isDark ? Theme.DisplayMode.DarkMode : Theme.DisplayMode.ClearMode);
            BackColor = Theme.Colors.Background;
            ForeColor = Theme.Colors.TextInactive;


            _whitelistService = whitelistService;

            publishersListBox.SelectedIndexChanged += new System.EventHandler(PublishersListBox_SelectedIndexChanged);

            LoadPublishers();
        }

        // Refresh publishers from whitelist 
        private void LoadPublishers()
        {
            publishersListBox.BeginUpdate();
            try
            {
                publishersListBox.Items.Clear();
                var publishers = _whitelistService.GetTrustedPublishers();

                publishersListBox.Items.AddRange([.. publishers]);
            }
            finally
            {
                publishersListBox.EndUpdate();
            }

            UpdateUIState();
        }

        // Toggles button based on selection
        private void UpdateUIState()
        {
            removeButton.Enabled = publishersListBox.SelectedItems.Count > 0;
        }

        private void PublishersListBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateUIState();
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            // Support removing single or multiple items
            if (publishersListBox.SelectedItems.Count > 0)
            {
                int count = publishersListBox.SelectedItems.Count;
                string message = count == 1
                    ? $"Are you sure you want to remove '{publishersListBox.SelectedItem}' from the trusted list?"
                    : $"Are you sure you want to remove {count} publishers from the trusted list?";

                var result = MessageBox.Show(message, "Confirm Removal", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    // Cast selected items to a list to avoid modifying the collection while iterating
                    var selectedItems = publishersListBox.SelectedItems.Cast<string>().ToList();

                    foreach (var publisher in selectedItems)
                    {
                        _whitelistService.Remove(publisher);
                    }
                    LoadPublishers();
                }
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
