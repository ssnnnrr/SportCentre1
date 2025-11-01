using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using SportCentre1.Data;
using System;
using System.Linq;

namespace SportCentre1.Windows
{
    public partial class RegistrationWindow : Window
    {
        public RegistrationWindow() { InitializeComponent(); }

        private async void RegisterButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ErrorTextBlock.Text = "";
            var email = EmailTextBox.Text;
            var password = PasswordTextBox.Text;
            var phoneNumber = PhoneTextBox.Text;

            if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text) || string.IsNullOrWhiteSpace(LastNameTextBox.Text) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ErrorTextBlock.Text = "Имя, Фамилия, Email и Пароль являются обязательными."; return;
            }
            if (await MainWindow.dbContext.Users.AnyAsync(u => u.Email == email))
            {
                ErrorTextBlock.Text = "Пользователь с таким Email уже зарегистрирован."; return;
            }
            if (!string.IsNullOrWhiteSpace(phoneNumber) && await MainWindow.dbContext.Clients.AnyAsync(c => c.Phonenumber == phoneNumber))
            {
                ErrorTextBlock.Text = "Клиент с таким номером телефона уже существует."; return;
            }

            var newUser = new User { Email = email, Password = password, Roleid = 4 };
            MainWindow.dbContext.Users.Add(newUser);
            await MainWindow.dbContext.SaveChangesAsync();

            var newClient = new Client
            {
                Firstname = FirstNameTextBox.Text,
                Lastname = LastNameTextBox.Text,
                Email = email,
                Phonenumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber,
                Registrationdate = DateOnly.FromDateTime(DateTime.Now),
                Userid = newUser.Userid
            };
            MainWindow.dbContext.Clients.Add(newClient);
            await MainWindow.dbContext.SaveChangesAsync();

            var user = await MainWindow.dbContext.Users.Include(u => u.Role).FirstAsync(u => u.Userid == newUser.Userid);
            MainWindow.CurrentUser = user;
            if (this.Owner is MainWindow mainWindow)
            {
                mainWindow.ShowMainPage();
            }
            Close(true);
        }
    }
}