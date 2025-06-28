using System.IO;
using System.Windows;

namespace MinimalFirewall
{
    public partial class WildcardCreatorWindow : Window
    {
        public string FolderPath { get; private set; }
        public string SelectedAction { get; private set; }

        public WildcardCreatorWindow()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            if (FolderPicker.TryPickFolder(out var folderPath))
            {
                FolderPathTextBox.Text = folderPath;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FolderPathTextBox.Text) || !Directory.Exists(FolderPathTextBox.Text))
            {
                MessageBox.Show("Please select a valid folder path.", "Invalid Path", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            FolderPath = FolderPathTextBox.Text;
            if (AllowOutboundRadio.IsChecked == true) SelectedAction = "Allow (Outbound)";
            else if (AllowInboundRadio.IsChecked == true) SelectedAction = "Allow (Inbound)";
            else if (BlockOutboundRadio.IsChecked == true) SelectedAction = "Block (Outbound)";
            else if (BlockInboundRadio.IsChecked == true) SelectedAction = "Block (Inbound)";

            DialogResult = true;
        }
    }
}