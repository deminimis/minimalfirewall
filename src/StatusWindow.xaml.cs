using System.Windows;

namespace MinimalFirewall
{
    public partial class StatusWindow : Window
    {
        public StatusWindow(string title)
        {
            InitializeComponent();
            this.Title = title;
            this.Owner = Application.Current.MainWindow;
        }

        public void Complete(string message)
        {
            this.StatusTextBlock.Text = message;
            this.ProgressBar.IsIndeterminate = false;
            this.ProgressBar.Value = 100;
            this.OkButton.Visibility = Visibility.Visible;
            this.ProgressBar.Visibility = Visibility.Collapsed;
            this.OkButton.Focus();
            this.Title = "Scan Complete";
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // A window shown with .Show() cannot have its DialogResult set.
            // We just need to close it.
            this.Close();
        }
    }
}
