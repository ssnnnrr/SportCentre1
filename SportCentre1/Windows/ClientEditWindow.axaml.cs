using Avalonia.Controls;
using SportCentre1.Data;
using System;
using System.Threading.Tasks;

namespace SportCentre1.Windows
{
    public partial class ClientEditWindow : Window
    {
        private Client _currentClient;
        private bool _isNew;

        public ClientEditWindow()
        {
            InitializeComponent();
            _isNew = true;
            _currentClient = new Client { Registrationdate = DateOnly.FromDateTime(DateTime.Now) };
        }

        public ClientEditWindow(Client clientToEdit)
        {
            InitializeComponent();
            _isNew = false;
            _currentClient = clientToEdit;
            LoadClientData();
        }

        private void LoadClientData()
        {
            FirstNameTextBox.Text = _currentClient.Firstname;
            LastNameTextBox.Text = _currentClient.Lastname;
            EmailTextBox.Text = _currentClient.Email;
            PhoneTextBox.Text = _currentClient.Phonenumber;
        }

        private async void SaveButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text) || string.IsNullOrWhiteSpace(LastNameTextBox.Text))
            {
                var dialog = new ConfirmationDialog("Имя и Фамилия обязательны для заполнения.", true);
                await dialog.ShowDialog<bool>(this);
                return;
            }

            _currentClient.Firstname = FirstNameTextBox.Text;
            _currentClient.Lastname = LastNameTextBox.Text;
            _currentClient.Email = EmailTextBox.Text;
            _currentClient.Phonenumber = PhoneTextBox.Text;

            using (var dbContext = new AppDbContext())
            {
                if (_isNew)
                {
                    dbContext.Clients.Add(_currentClient);
                }
                else
                {
                    dbContext.Clients.Update(_currentClient);
                }
                await dbContext.SaveChangesAsync();
            }
            Close(true);
        }
    }
}