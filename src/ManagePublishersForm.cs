// File: ManagePublishersForm.cs
using DarkModeForms;
using System.Data; // Required for LINQ extensions if not globally imported

namespace MinimalFirewall
{
    // Form for viewing and removing trusted software publishers
    public partial class ManagePublishersForm : Form
    {
        private readonly PublisherWhitelistService _whitelistService;
        private readonly DarkModeCS dm;

        public ManagePublishersForm(PublisherWhitelistService whitelistService, AppSettings appSettings)
        {
            InitializeComponent();

            // Initialize Dark Mode integration based on app settings
            dm = new DarkModeCS(this);
            dm.ColorMode = appSettings.Theme == "Dark" ? DarkModeCS.DisplayMode.DarkMode : DarkModeCS.DisplayMode.ClearMode;
            dm.ApplyTheme(appSettings.Theme == "Dark");

            _whitelistService = whitelistService;

            // Ensure we track selection changes to update button states
            this.publishersListBox.SelectedIndexChanged += new System.EventHandler(this.publishersListBox_SelectedIndexChanged);

            LoadPublishers();
        }

        // Refreshes the list of publishers from the whitelist service
        private void LoadPublishers()
        {
            // BeginUpdate/EndUpdate prevents visual flickering during bulk updates
            publishersListBox.BeginUpdate();
            try
            {
                publishersListBox.Items.Clear();
                var publishers = _whitelistService.GetTrustedPublishers();

                // AddRange is more efficient than iterating and adding one by one
                publishersListBox.Items.AddRange(publishers.ToArray());
            }
            finally
            {
                publishersListBox.EndUpdate();
            }

            UpdateUIState();
        }

        // Toggles button availability based on current selection
        private void UpdateUIState()
        {
            removeButton.Enabled = publishersListBox.SelectedItems.Count > 0;
        }

        private void publishersListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateUIState();
        }

        private void removeButton_Click(object sender, EventArgs e)
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

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}