using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using SportCentre1.Data;
using SportCentre1.Windows;
using System.Linq;
using System.Threading.Tasks;

namespace SportCentre1.Pages
{
    public partial class ManagerRequestsPage : UserControl
    {
        public ManagerRequestsPage()
        {
            InitializeComponent();
            LoadAllRequests();
        }

        private async void LoadAllRequests()
        {
            using (var dbContext = new AppDbContext())
            {
                RequestsListBox.ItemsSource = await dbContext.Requests
                    .Include(r => r.Client)
                    .OrderByDescending(r => r.Creationdate)
                    .ToListAsync();
            }
        }

        private async void RequestsListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (RequestsListBox.SelectedItem is Request selectedRequest)
            {
                var threadWindow = new RequestThreadWindow(selectedRequest);
                await threadWindow.ShowDialog<bool>(this.VisualRoot as Window);

                if (this.VisualRoot is MainWindow { Content: MainPage mainPage })
                {
                    await mainPage.CheckNotificationsAsync();
                }

                LoadAllRequests();
                RequestsListBox.SelectedItem = null;
            }
        }
    }
}