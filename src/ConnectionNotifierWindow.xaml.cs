using System.IO;
using System.Windows;

namespace MinimalFirewall
{
    public partial class ConnectionNotifierWindow : Window
    {
        public enum NotifierResult { Ignore, Allow, Block, AllowTemporary }
        public NotifierResult Result { get; private set; }
        public int Minutes { get; set; }
        public string AppPath { get; }
        public string AppName => Path.GetFileName(AppPath);
        public string Direction { get; }
        public string AllowButtonText => "Allow " + Direction;
        public string BlockButtonText => "Block " + Direction;

        public bool CreateWildcard { get; set; }

        public ConnectionNotifierWindow(string appPath, string direction, int defaultMinutes)
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            Result = NotifierResult.Ignore;
            AppPath = appPath;
            Direction = direction;
            Minutes = defaultMinutes;
            CreateWildcard = false;
            DataContext = this;
        }

        private void AllowButton_Click(object sender, RoutedEventArgs e)
        {
            Result = NotifierResult.Allow;
            DialogResult = true;
        }

        private void BlockButton_Click(object sender, RoutedEventArgs e)
        {
            Result = NotifierResult.Block;
            DialogResult = true;
        }

        private void AllowTempButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(MinutesTextBox.Text, out var minutes) || minutes <= 0)
            {
                MessageBox.Show("Please enter a valid, positive number of minutes.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Minutes = minutes;
            Result = NotifierResult.AllowTemporary;
            DialogResult = true;
        }
    }
}