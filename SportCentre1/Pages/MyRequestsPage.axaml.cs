using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using SportCentre1.Data;
using SportCentre1.Windows;
using System.Linq;

namespace SportCentre1.Pages
{
    public partial class MyRequestsPage : UserControl
    {
        public MyRequestsPage()
        {
            InitializeComponent();
            LoadRequests();
        }

        private async void LoadRequests()
        {
            using (var dbContext = new AppDbContext())
            {
                var clientProfile = await dbContext.Clients
                    .FirstOrDefaultAsync(c => c.Userid == MainWindow.CurrentUser!.Userid);
                if (clientProfile == null) return;

                RequestsListBox.ItemsSource = await dbContext.Requests
                    .Where(r => r.Clientid == clientProfile.Clientid)
                    .OrderByDescending(r => r.Creationdate)
                    .AsNoTracking() 
                    .ToListAsync();
            }
        }

        private async void NewRequestButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var newRequestWindow = new RequestEditWindow();
            var result = await newRequestWindow.ShowDialog<bool>(this.VisualRoot as Window);
            if (result == true) LoadRequests();
        }

        private async void RequestsListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (RequestsListBox.SelectedItem is Request selectedRequest)
            {
                var threadWindow = new RequestThreadWindow(selectedRequest);
                await threadWindow.ShowDialog<bool>(this.VisualRoot as Window);
                LoadRequests(); 
                RequestsListBox.SelectedItem = null;
            }
        }
    }
}