using System.IO;
using System.Windows;

namespace MinimalFirewall
{
    public partial class ConnectionNotifierWindow : Window
    {
        public enum NotifierResult { Ignore, Allow, Block, AllowTemporary, CreateWildcard }
        public NotifierResult Result { get; private set; }
        public int Minutes { get; set; } = 5;

        public PendingConnectionViewModel PendingConnection { get; }

        public string AppPath => PendingConnection.AppPath;
        public string Direction => PendingConnection.Direction;
        public string AppNameWithServices
        {
            get
            {
                if (!string.IsNullOrEmpty(PendingConnection.ServiceName))
                {
                    return $"{PendingConnection.FileName}; {PendingConnection.ServiceName}";
                }
                return PendingConnection.FileName;
            }
        }
        public string AllowButtonText => "Allow " + Direction;
        public string BlockButtonText => "Block " + Direction;

        public ConnectionNotifierWindow(PendingConnectionViewModel pendingVm, int defaultMinutes)
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            Result = NotifierResult.Ignore;
            PendingConnection = pendingVm;
            Minutes = defaultMinutes;
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

        private void CreateWildcardButton_Click(object sender, RoutedEventArgs e)
        {
            Result = NotifierResult.CreateWildcard;
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