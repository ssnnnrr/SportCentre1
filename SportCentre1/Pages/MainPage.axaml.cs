using Avalonia.Controls;
using Avalonia.VisualTree;
using Microsoft.EntityFrameworkCore;
using SportCentre1.Data;
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
                using (var db = new AppDbContext())
                {
                    bool hasNewRequests = await db.Requests.AnyAsync(r => r.Status == "Открыт");
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
        }

        public async Task CheckEquipmentMaintenanceAsync()
        {
            using (var db = new AppDbContext())
            {
                bool maintenanceIsDue = await db.Equipment
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

            // Сначала скроем все кнопки, а потом будем включать нужные
            SetAllButtonsVisibility(false);

            // Кнопки, видимые почти для всех
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
                    ManageChallengesButton.IsVisible = true; // <-- ПОКАЗАТЬ КНОПКУ
                    break;

                case "Менеджер":
                    ClientsButton.IsVisible = true;
                    PaymentsButton.IsVisible = true;
                    RequestsButton.IsVisible = true;
                    AnalyticsButton.IsVisible = true;
                    ManageChallengesButton.IsVisible = true; // <-- ПОКАЗАТЬ КНОПКУ
                    break;

                case "Тренер":
                    EquipmentButton.IsVisible = true;
                    break;

                case "Пользователь":
                    DashboardButton.IsVisible = true;
                    MyBookingsButton.IsVisible = true;
                    RequestsButton.IsVisible = true;
                    MembershipButton.IsVisible = true;   // <-- ПОКАЗАТЬ КНОПКУ
                    ChallengesButton.IsVisible = true;   // <-- ПОКАЗАТЬ КНОПКУ
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
            DashboardButton.IsVisible = isVisible;
            ProfileButton.IsVisible = isVisible;
            MyBookingsButton.IsVisible = isVisible;
            ClientsButton.IsVisible = isVisible;
            ScheduleButton.IsVisible = isVisible;
            EquipmentButton.IsVisible = isVisible;
            PaymentsButton.IsVisible = isVisible;
            ReviewsButton.IsVisible = isVisible;
            RequestsButton.IsVisible = isVisible;
            AnalyticsButton.IsVisible = isVisible;
            // Добавляем новые кнопки в список
            MembershipButton.IsVisible = isVisible;
            ChallengesButton.IsVisible = isVisible;
            ManageChallengesButton.IsVisible = isVisible;
        }

        private void OpenDefaultPage()
        {
            var userRole = MainWindow.CurrentUser?.Role?.Rolename;
            // Для клиента стартовой страницей теперь будет Дашборд
            if (userRole == "Пользователь")
            {
                MainContentControl.Content = new DashboardPage();
                return;
            }
            // Для остальных - Профиль
            if (ProfileButton.IsVisible)
            {
                MainContentControl.Content = new ProfilePage();
                return;
            }
        }

        // --- Обработчики нажатий ---

        private void DashboardButton_Click(object? s, Avalonia.Interactivity.RoutedEventArgs e) => MainContentControl.Content = new DashboardPage();
        private void ProfileButton_Click(object? s, Avalonia.Interactivity.RoutedEventArgs e) => MainContentControl.Content = new ProfilePage();
        private void MyBookingsButton_Click(object? s, Avalonia.Interactivity.RoutedEventArgs e) => MainContentControl.Content = new MyBookingsPage();
        private void ClientsButton_Click(object? s, Avalonia.Interactivity.RoutedEventArgs e) => MainContentControl.Content = new ClientPage();
        private void ScheduleButton_Click(object? s, Avalonia.Interactivity.RoutedEventArgs e) => MainContentControl.Content = new SchedulePage();
        private void EquipmentButton_Click(object? s, Avalonia.Interactivity.RoutedEventArgs e) => MainContentControl.Content = new EquipmentPage();
        private void PaymentsButton_Click(object? s, Avalonia.Interactivity.RoutedEventArgs e) => MainContentControl.Content = new PaymentsPage();
        private void ReviewsButton_Click(object? s, Avalonia.Interactivity.RoutedEventArgs e) => MainContentControl.Content = new ReviewsPage();
        private void AnalyticsButton_Click(object? s, Avalonia.Interactivity.RoutedEventArgs e) => MainContentControl.Content = new AnalyticsPage();

        // НОВЫЕ ОБРАБОТЧИКИ
        private void MembershipButton_Click(object? s, Avalonia.Interactivity.RoutedEventArgs e) => MainContentControl.Content = new MembershipManagementPage();
        private void ChallengesButton_Click(object? s, Avalonia.Interactivity.RoutedEventArgs e) => MainContentControl.Content = new ChallengesPage();
        private void ManageChallengesButton_Click(object? s, Avalonia.Interactivity.RoutedEventArgs e) => MainContentControl.Content = new AdminChallengesPage();

        private void RequestsButton_Click(object? s, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var userRole = MainWindow.CurrentUser?.Role?.Rolename;
            if (userRole == "Пользователь") { MainContentControl.Content = new MyRequestsPage(); }
            else { MainContentControl.Content = new ManagerRequestsPage(); }
        }

        private void LogoutButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (this.GetVisualRoot() is MainWindow mainWindow) { mainWindow.Logout(); }
        }
    }
}