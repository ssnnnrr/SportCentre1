using Avalonia.Controls;

using SportCentre1.Windows;
namespace SportCentre1.Pages
{
    public partial class WelcomePage : UserControl
    {
        public WelcomePage() { InitializeComponent(); }

        private async void LoginButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            await loginWindow.ShowDialog<bool>(this.VisualRoot as Window);
        }

        private async void RegisterButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var registerWindow = new RegistrationWindow();
            await registerWindow.ShowDialog<bool>(this.VisualRoot as Window);
        }
    }
}