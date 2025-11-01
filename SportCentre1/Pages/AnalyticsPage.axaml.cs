using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using SportCentre1.Data;
using System;
using System.Linq;

namespace SportCentre1.Pages
{
    public class TrainerStats
    {
        public string TrainerName { get; set; } = "";
        public int WorkoutCount { get; set; }
    }

    public class ClientStats
    {
        public string ClientName { get; set; } = "";
        public decimal TotalSpent { get; set; }
    }

    public partial class AnalyticsPage : UserControl
    {
        public AnalyticsPage()
        {
            InitializeComponent();
            LoadAnalytics();
        }

        private async void LoadAnalytics()
        {
            using (var dbContext = new AppDbContext())
            {
                var totalRevenue = await dbContext.Payments.SumAsync(p => p.Amount);
                TotalRevenueTextBlock.Text = $"{totalRevenue:C}";

                var totalClients = await dbContext.Clients.CountAsync();
                TotalClientsTextBlock.Text = totalClients.ToString();

                var activeBookings = await dbContext.Bookings
                    .Include(b => b.Schedule)
                    .CountAsync(b => b.Schedule.Starttime >= DateTime.Now);
                ActiveBookingsTextBlock.Text = activeBookings.ToString();

                var topTrainers = await dbContext.Schedules
                    .Include(s => s.Trainer).Where(s => s.Trainer != null).GroupBy(s => s.Trainer)
                    .Select(g => new TrainerStats { TrainerName = g.Key.Firstname + " " + g.Key.Lastname, WorkoutCount = g.Count() })
                    .OrderByDescending(t => t.WorkoutCount).Take(5).ToListAsync();
                TopTrainersDataGrid.ItemsSource = topTrainers;

                var topClients = await dbContext.Payments
                    .Include(p => p.Client).Where(p => p.Client != null).GroupBy(p => p.Client)
                    .Select(g => new ClientStats { ClientName = g.Key.Firstname + " " + g.Key.Lastname, TotalSpent = g.Sum(p => p.Amount) })
                    .OrderByDescending(c => c.TotalSpent).Take(5).ToListAsync();
                TopClientsDataGrid.ItemsSource = topClients;
            }
        }
    }
}