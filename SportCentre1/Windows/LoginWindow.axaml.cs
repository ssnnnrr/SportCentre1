using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;
using SportCentre1.Data;
using System.Linq;

namespace SportCentre1.Windows
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void LoginButton_Click(object? sender, RoutedEventArgs e)
        {
            var email = LoginTextBox.Text;
            var password = PasswordTextBox.Text;

            var user = await MainWindow.dbContext.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email && u.Password == password);

            if (user != null)
            {
                MainWindow.CurrentUser = user;

                if (this.Owner is MainWindow mainWindow)
                {
                    mainWindow.ShowMainPage();
                }
                Close(true);
            }
            else
            {
                ErrorTextBlock.Text = "Неверный Email или пароль";
            }
        }
    }
}