// File: BrowseServicesForm.cs
using DarkModeForms;
using System.Data;

namespace MinimalFirewall
{
    public partial class BrowseServicesForm : Form
    {
        private readonly DarkModeCS dm;
        private readonly List<ServiceViewModel> _allServices;
        public ServiceViewModel? SelectedService { get; private set; }

        public BrowseServicesForm(List<ServiceViewModel> services, AppSettings appSettings)
        {
            InitializeComponent();

            // prevents crashes if null list is passed
            _allServices = services ?? new List<ServiceViewModel>();

            dm = new DarkModeCS(this);
            dm.ColorMode = appSettings.Theme == "Dark" ? DarkModeCS.DisplayMode.DarkMode : DarkModeCS.DisplayMode.ClearMode;
            dm.ApplyTheme(appSettings.Theme == "Dark");

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
                servicesListBox.Items.Add(new ServiceListItem(service));
            }

            if (servicesListBox.Items.Count == 1)
            {
                servicesListBox.SelectedIndex = 0;
            }

            servicesListBox.EndUpdate();
        }

        private void searchTextBox_TextChanged(object sender, EventArgs e)
        {
            LoadServices(searchTextBox.Text);
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            //  Retrieve object directly. No string parsing needed.
            if (servicesListBox.SelectedItem is ServiceListItem selectedItem)
            {
                SelectedService = selectedItem.Service;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void servicesListBox_DoubleClick(object sender, EventArgs e)
        {
            if (servicesListBox.SelectedItem != null)
            {
                okButton_Click(sender, e);
            }
        }

        private class ServiceListItem
        {
            public ServiceViewModel Service { get; }

            public ServiceListItem(ServiceViewModel service)
            {
                Service = service;
            }

            public override string ToString()
            {
                return $"{Service.DisplayName} ({Service.ServiceName})";
            }
        }
    }
}