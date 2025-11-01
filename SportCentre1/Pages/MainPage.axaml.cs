using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using SportCentre1.Windows;
using System.Threading.Tasks;

namespace SportCentre1.Pages
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
            ApplyRolesAsync();
        }

        public async Task CheckNotificationsAsync()
        {
            var userRole = MainWindow.CurrentUser?.Role?.Rolename;
            if (userRole == "Администратор" || userRole == "Менеджер")
            {
                bool hasNewRequests = await MainWindow.dbContext.Requests.AnyAsync(r => r.Status == "Открыт");
                if (hasNewRequests)
                {
                    RequestsButton.Classes.Add("notification");
                }
                else
                {
                    RequestsButton.Classes.Remove("notification");
                }
            }
        }

        public async Task CheckEquipmentMaintenanceAsync()
        {
            bool maintenanceIsDue = await MainWindow.dbContext.Equipment
                .AnyAsync(e => e.Lastmaintenancedate.HasValue &&
                               e.Lastmaintenancedate.Value.ToDateTime(System.TimeOnly.MinValue) < System.DateTime.Now.AddDays(-180));

            if (maintenanceIsDue)
            {
                EquipmentButton.Classes.Add("notification_warning");
            }
            else
            {
                EquipmentButton.Classes.Remove("notification_warning");
            }
        }

        private async Task ApplyRolesAsync()
        {
            var user = MainWindow.CurrentUser;
            if (user == null || user.Role == null)
            {
                SetAllButtonsVisibility(false);
                return;
            }

            var userRole = user.Role.Rolename;

            SetAllButtonsVisibility(false);

            ProfileButton.IsVisible = true;
            ScheduleButton.IsVisible = true;
            ReviewsButton.IsVisible = true;

            switch (userRole)
            {
                case "Администратор":
                    ClientsButton.IsVisible = true;
                    EquipmentButton.IsVisible = true;
                    PaymentsButton.IsVisible = true;
                    RequestsButton.IsVisible = true;
                    AnalyticsButton.IsVisible = true;
                    break;
                case "Менеджер":
                    ClientsButton.IsVisible = true;
                    PaymentsButton.IsVisible = true;
                    RequestsButton.IsVisible = true;
                    AnalyticsButton.IsVisible = true;
                    break;
                case "Тренер":
                    EquipmentButton.IsVisible = true;
                    break;
                case "Пользователь":
                    MyBookingsButton.IsVisible = true;
                    RequestsButton.IsVisible = true;
                    break;
            }

            if (RequestsButton.IsVisible)
            {
                RequestsButton.Content = (userRole == "Пользователь") ? "Мои запросы 📬" : "Запросы клиентов 📬";
            }

            await CheckNotificationsAsync();
            await CheckEquipmentMaintenanceAsync();

            OpenDefaultPage();
        }

        private void SetAllButtonsVisibility(bool isVisible)
        {
            ProfileButton.IsVisible = isVisible;
            MyBookingsButton.IsVisible = isVisible;
            ClientsButton.IsVisible = isVisible;
            ScheduleButton.IsVisible = isVisible;
            EquipmentButton.IsVisible = isVisible;
            PaymentsButton.IsVisible = isVisible;
            ReviewsButton.IsVisible = isVisible;
            RequestsButton.IsVisible = isVisible;
            AnalyticsButton.IsVisible = isVisible;
        }

        private void OpenDefaultPage()
        {
            if (ProfileButton.IsVisible) { MainContentControl.Content = new ProfilePage(); return; }
        }

        private void ProfileButton_Click(object? s, Avalonia.Interactivity.RoutedEventArgs e) => MainContentControl.Content = new ProfilePage();
        private void MyBookingsButton_Click(object? s, Avalonia.Interactivity.RoutedEventArgs e) => MainContentControl.Content = new MyBookingsPage();
        private void ClientsButton_Click(object? s, Avalonia.Interactivity.RoutedEventArgs e) => MainContentControl.Content = new ClientPage();
        private void ScheduleButton_Click(object? s, Avalonia.Interactivity.RoutedEventArgs e) => MainContentControl.Content = new SchedulePage();
        private void EquipmentButton_Click(object? s, Avalonia.Interactivity.RoutedEventArgs e) => MainContentControl.Content = new EquipmentPage();
        private void PaymentsButton_Click(object? s, Avalonia.Interactivity.RoutedEventArgs e) => MainContentControl.Content = new PaymentsPage();
        private void ReviewsButton_Click(object? s, Avalonia.Interactivity.RoutedEventArgs e) => MainContentControl.Content = new ReviewsPage();
        private void AnalyticsButton_Click(object? s, Avalonia.Interactivity.RoutedEventArgs e) => MainContentControl.Content = new AnalyticsPage();

        private void RequestsButton_Click(object? s, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var userRole = MainWindow.CurrentUser?.Role?.Rolename;
            if (userRole == "Пользователь") { MainContentControl.Content = new MyRequestsPage(); }
            else { MainContentControl.Content = new ManagerRequestsPage(); }
        }

        private void LogoutButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (this.VisualRoot is MainWindow mainWindow) { mainWindow.Logout(); }
        }
    }
}