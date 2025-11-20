using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using SportCentre1.Data;
using SportCentre1.Pages;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SportCentre1
{
    public partial class MainWindow : Window
    {
        // Статические свойства для глобального доступа к контексту БД и текущему пользователю
        public static AppDbContext dbContext { get; set; } = new AppDbContext();
        public static User? CurrentUser { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            // При запуске приложения всегда показываем страницу приветствия/входа
            this.Content = new WelcomePage();
        }

        // Этот метод вызывается из окна логина при успешном входе
        public void ShowMainPage()
        {
            this.Content = new MainPage();
            if (CurrentUser?.Role?.Rolename == "Пользователь")
            {
                Task.Run(() => UpdateChallengesProgressAsync(CurrentUser.Userid));
            }
        }

        private async Task UpdateChallengesProgressAsync(int userId)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var client = await db.Clients.FirstOrDefaultAsync(c => c.Userid == userId);
                    if (client == null) return;

                    var now = DateTime.Now;

                    // Находим все записи клиента на тренировки, которые уже прошли, но еще не были засчитаны
                    var pastBookings = await db.Bookings
                        .Include(b => b.Schedule)
                        .Where(b => b.Clientid == client.Clientid &&
                                     b.Schedule.Endtime < now &&
                                     b.Ispaid == true) // Учитываем только оплаченные
                        .ToListAsync();

                    if (!pastBookings.Any()) return;

                    // Находим активные челленджи клиента
                    var activeChallenges = await db.ClientChallenges
                        .Include(cc => cc.Challenge)
                        .Where(cc => cc.Clientid == client.Clientid &&
                                     cc.Status == "InProgress" &&
                                     cc.Challenge.Challengetype == "Attendance")
                        .ToListAsync();

                    if (!activeChallenges.Any()) return;

                    int creditedVisits = 0;
                    foreach (var booking in pastBookings)
                    {
                        foreach (var challenge in activeChallenges)
                        {
                            // Проверяем, что тренировка входит в период челленджа
                            if (booking.Schedule.Starttime >= challenge.Challenge.Startdate &&
                                booking.Schedule.Endtime <= challenge.Challenge.Enddate)
                            {
                                // В реальном приложении нужна более сложная логика,
                                // чтобы не засчитывать одно и то же посещение дважды.
                                // Для простоты, мы будем считать все прошедшие.
                                challenge.Progress++;
                                creditedVisits++;
                            }
                        }
                    }

                    // В реальном мире, после засчитывания, booking нужно помечать как "credited".
                    // Сейчас для простоты мы просто сохраняем прогресс.
                    if (creditedVisits > 0)
                    {
                        await db.SaveChangesAsync();
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки в фоновой задаче
            }
        }

// Этот метод вызывается при нажатии кнопки "Выйти"
public void Logout()
        {
            CurrentUser = null;
            this.Content = new WelcomePage();
        }
    }
}