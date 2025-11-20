using Avalonia.Controls;
using Avalonia.VisualTree;
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
            // EditButton.IsEnabled больше не нужен, т.к. кнопка скрыта
            DeleteButton.IsEnabled = ClientsDataGrid.SelectedItem != null;
        }

        // ќбработчик EditButton_Click полностью удален, т.к. кнопка больше не используетс€

        private async void DeleteButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (ClientsDataGrid.SelectedItem is Client selectedClient)
            {
                var dialog = new ConfirmationDialog($"”далить клиента {selectedClient.Firstname} {selectedClient.Lastname}?");
                var result = await dialog.ShowDialog<bool>(this.GetVisualRoot() as Window);
                if (result != true) return;

                using (var dbContext = new AppDbContext())
                {
                    // ¬ажно: нужно удалить и св€занные сущности, которые не удал€ютс€ каскадно, например User
                    if (selectedClient.Userid != null)
                    {
                        var userToDelete = await dbContext.Users.FindAsync(selectedClient.Userid);
                        if (userToDelete != null)
                        {
                            dbContext.Users.Remove(userToDelete);
                        }
                    }

                    // —ам клиент удалитс€ каскадно со многими другими запис€ми
                    dbContext.Clients.Remove(selectedClient);
                    await dbContext.SaveChangesAsync();
                }
                LoadClients();
            }
        }
    }
}