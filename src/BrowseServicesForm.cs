using DarkModeForms;
using System.Data;

namespace MinimalFirewall
{
    public partial class BrowseServicesForm : Form
    {
        private readonly DarkModeCS dm;
        private readonly List<ServiceViewModel> _allServices;
        public ServiceViewModel? SelectedService { get; private set; }

        public BrowseServicesForm(List<ServiceViewModel> services)
        {
            InitializeComponent();
            dm = new DarkModeCS(this);
            _allServices = services;
            LoadServices();
        }

        private void LoadServices(string filter = "")
        {
            servicesListBox.BeginUpdate();
            servicesListBox.Items.Clear();

            var filteredServices = string.IsNullOrWhiteSpace(filter)
                ? _allServices
                : _allServices.Where(s =>
                    s.DisplayName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    s.ServiceName.Contains(filter, StringComparison.OrdinalIgnoreCase));

            foreach (var service in filteredServices)
            {
                servicesListBox.Items.Add($"{service.DisplayName} ({service.ServiceName})");
            }
            servicesListBox.EndUpdate();
        }

        private void searchTextBox_TextChanged(object sender, EventArgs e)
        {
            LoadServices(searchTextBox.Text);
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (servicesListBox.SelectedItem is string selectedItem)
            {
                SelectedService = _allServices.FirstOrDefault(s => selectedItem == $"{s.DisplayName} ({s.ServiceName})");
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void servicesListBox_DoubleClick(object sender, EventArgs e)
        {
            okButton_Click(sender, e);
        }
    }
}