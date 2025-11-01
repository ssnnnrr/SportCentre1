using Avalonia.Controls;
using Avalonia.Media;
using Microsoft.EntityFrameworkCore;
using SportCentre1.Data;
using System.Linq;

namespace SportCentre1.Pages
{
    public partial class ProfilePage : UserControl
    {
        private User? _currentUser;

        public ProfilePage()
        {
            InitializeComponent();
            LoadUserData();
        }

        private async void LoadUserData()
        {
            _currentUser = MainWindow.CurrentUser;
            if (_currentUser == null) return;

            using (var dbContext = new AppDbContext())
            {
                var clientProfile = await dbContext.Clients.FirstOrDefaultAsync(c => c.Userid == _currentUser.Userid);
                var employeeProfile = await dbContext.Employees.FirstOrDefaultAsync(e => e.Userid == _currentUser.Userid);

                if (clientProfile != null)
                {
                    FirstNameTextBlock.Text = $"🤸‍♂️ {clientProfile.Firstname}";
                    LastNameTextBlock.Text = clientProfile.Lastname;
                    EmailTextBlock.Text = clientProfile.Email;
                }
                else if (employeeProfile != null)
                {
                    FirstNameTextBlock.Text = $"💼 {employeeProfile.Firstname}";
                    LastNameTextBlock.Text = employeeProfile.Lastname;
                    EmailTextBlock.Text = _currentUser.Email;
                }
            }
        }

        private async void ChangePasswordButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            StatusTextBlock.Text = "";
            if (_currentUser == null) return;

            var currentPassword = CurrentPasswordTextBox.Text;
            var newPassword = NewPasswordTextBox.Text;
            var confirmPassword = ConfirmPasswordTextBox.Text;

            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                SetStatusMessage("Все поля должны быть заполнены.", true); return;
            }
            if (_currentUser.Password != currentPassword)
            {
                SetStatusMessage("Текущий пароль введен неверно.", true); return;
            }
            if (newPassword != confirmPassword)
            {
                SetStatusMessage("Новый пароль и его подтверждение не совпадают.", true); return;
            }

            using (var dbContext = new AppDbContext())
            {
                var userToUpdate = await dbContext.Users.FindAsync(_currentUser.Userid);
                if (userToUpdate != null)
                {
                    userToUpdate.Password = newPassword;
                    await dbContext.SaveChangesAsync();

                    _currentUser.Password = newPassword;

                    SetStatusMessage("Пароль успешно изменен!", false);
                    CurrentPasswordTextBox.Text = "";
                    NewPasswordTextBox.Text = "";
                    ConfirmPasswordTextBox.Text = "";
                }
            }
        }

        private void SetStatusMessage(string message, bool isError)
        {
            StatusTextBlock.Text = message;
            StatusTextBlock.Foreground = isError ? Brushes.Red : Brushes.Green;
        }
    }
}