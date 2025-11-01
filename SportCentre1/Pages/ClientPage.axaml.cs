using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using SportCentre1.Data;
using SportCentre1.Windows;
using System.Linq;

namespace SportCentre1.Pages
{
    public partial class ClientPage : UserControl
    {
        public ClientPage()
        {
            InitializeComponent();
            LoadClients();
        }

        private async void LoadClients()
        {
            using (var dbContext = new AppDbContext())
            {
                ClientsDataGrid.ItemsSource = await dbContext.Clients.OrderBy(c => c.Lastname).ToListAsync();
            }
        }

        private void ClientsDataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            EditButton.IsEnabled = ClientsDataGrid.SelectedItem != null;
            DeleteButton.IsEnabled = ClientsDataGrid.SelectedItem != null;
        }

        private async void EditButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (ClientsDataGrid.SelectedItem is Client selectedClient)
            {
                var editWindow = new ClientEditWindow(selectedClient);
                var result = await editWindow.ShowDialog<bool>(this.VisualRoot as Window);
                if (result == true) LoadClients();
            }
        }

        private async void DeleteButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (ClientsDataGrid.SelectedItem is Client selectedClient)
            {
                var dialog = new ConfirmationDialog($"Удалить клиента {selectedClient.Firstname} {selectedClient.Lastname}?");
                var result = await dialog.ShowDialog<bool>(this.VisualRoot as Window);
                if (result != true) return;

                using (var dbContext = new AppDbContext())
                {
                    if (selectedClient.Userid != null)
                    {
                        var userToDelete = await dbContext.Users.FindAsync(selectedClient.Userid);
                        if (userToDelete != null)
                        {
                            dbContext.Users.Remove(userToDelete);
                        }
                    }

                    dbContext.Clients.Remove(selectedClient);
                    await dbContext.SaveChangesAsync();
                }
                LoadClients();
            }
        }
    }
}