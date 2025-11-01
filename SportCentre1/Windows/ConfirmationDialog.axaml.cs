using Avalonia.Controls;
using Avalonia.Interactivity;

namespace SportCentre1.Windows
{
    public partial class ConfirmationDialog : Window
    {
        public ConfirmationDialog() { InitializeComponent(); }

        public ConfirmationDialog(string message, bool isAlert = false)
        {
            InitializeComponent();
            MessageTextBlock.Text = message;
            if (isAlert)
            {
                NoButton.IsVisible = false;
                YesButton.Content = "ÎÊ";
            }
        }

        private void YesButton_Click(object? sender, RoutedEventArgs e) => Close(true);
        private void NoButton_Click(object? sender, RoutedEventArgs e) => Close(false);
    }
}