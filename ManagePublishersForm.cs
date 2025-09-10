// ManagePublishersForm.cs
using DarkModeForms;

namespace MinimalFirewall
{
    public partial class ManagePublishersForm : Form
    {
        private readonly PublisherWhitelistService _whitelistService;
        private readonly DarkModeCS dm;

        public ManagePublishersForm(PublisherWhitelistService whitelistService)
        {
            InitializeComponent();
            dm = new DarkModeCS(this);
            _whitelistService = whitelistService;
            LoadPublishers();
        }

        private void LoadPublishers()
        {
            publishersListBox.Items.Clear();
            var publishers = _whitelistService.GetTrustedPublishers();
            foreach (var publisher in publishers)
            {
                publishersListBox.Items.Add(publisher);
            }
        }

        private void removeButton_Click(object sender, EventArgs e)
        {
            if (publishersListBox.SelectedItem is string selectedPublisher)
            {
                var result = MessageBox.Show($"Are you sure you want to remove '{selectedPublisher}' from the trusted list?", "Confirm Removal", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    _whitelistService.Remove(selectedPublisher);
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