// Copyright (c) 2025 Deminimis
// Licensed under the GNU AGPL v3.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MinimalFirewall
{
    public partial class EditRuleWindow : Window
    {
        public string SelectedAction { get; private set; } = string.Empty;

        public EditRuleWindow(List<string> appNames)
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;

            if (appNames.Count == 1)
            {
                AppNameTextBlock.Text = $"Editing rule for: {appNames.First()}";
            }
            else
            {
                AppNameTextBlock.Text = $"Editing rules for {appNames.Count} applications.";
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (AllowAllRadio.IsChecked == true) SelectedAction = "Allow (All)";
            else if (AllowOutboundRadio.IsChecked == true) SelectedAction = "Allow (Outbound)";
            else if (AllowInboundRadio.IsChecked == true) SelectedAction = "Allow (Inbound)";
            else if (BlockAllRadio.IsChecked == true) SelectedAction = "Block (All)";
            else if (BlockOutboundRadio.IsChecked == true) SelectedAction = "Block (Outbound)";
            else if (BlockInboundRadio.IsChecked == true) SelectedAction = "Block (Inbound)";

            this.DialogResult = true;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}