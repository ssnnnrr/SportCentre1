using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;
using SportCentre1.Data;
using SportCentre1.Windows;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SportCentre1.Pages
{
    public partial class MyBookingsPage : UserControl
    {
        public MyBookingsPage()
        {
            InitializeComponent();
            _ = LoadMyBookingsAsync();
        }

        private async Task LoadMyBookingsAsync()
        {
            using (var dbContext = new AppDbContext())
            {
                var clientProfile = await dbContext.Clients
                    .FirstOrDefaultAsync(c => c.Userid == MainWindow.CurrentUser!.Userid);
                if (clientProfile == null) return;

                var now = DateTime.Now;

                // --- ИСПРАВЛЕННАЯ ЛОГИКА ЗАГРУЗКИ ---
                // Запрос отбирает все предстоящие записи, независимо от статуса оплаты.
                var futureBookings = await dbContext.Bookings
                    .Where(b => b.Clientid == clientProfile.Clientid && b.Schedule != null && b.Schedule.Starttime >= now)
                    .Include(b => b.Schedule).ThenInclude(s => s.Workouttype)
                    .Include(b => b.Schedule).ThenInclude(s => s.Trainer)
                    .OrderBy(b => b.Schedule.Starttime)
                    .AsNoTracking()
                    .ToListAsync();
                BookingsListBox.ItemsSource = futureBookings;

                // Загрузка прошедших записей
                var pastBookings = await dbContext.Bookings
                    .Where(b => b.Clientid == clientProfile.Clientid && b.Schedule != null && b.Schedule.Starttime < now)
                    .Include(b => b.Schedule).ThenInclude(s => s.Workouttype)
                    .Include(b => b.Schedule).ThenInclude(s => s.Trainer)
                    .OrderByDescending(b => b.Schedule.Starttime)
                    .Take(20)
                    .AsNoTracking()
                    .ToListAsync();
                PastBookingsListBox.ItemsSource = pastBookings;
            }
        }

        private async void PayButton_Click(object? sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not Booking bookingToPay) return;

            using (var dbContext = new AppDbContext())
            {
                var existingBooking = await dbContext.Bookings
                    .Include(b => b.Schedule)
                    .FirstOrDefaultAsync(b => b.Bookingid == bookingToPay.Bookingid);

                if (existingBooking?.Schedule == null)
                {
                    await new ConfirmationDialog("Невозможно произвести оплату: связанное занятие было удалено.", true).ShowDialog<bool>(this.VisualRoot as Window);
                    return;
                }

                var workoutType = await dbContext.Workouttypes
                    .FirstOrDefaultAsync(w => w.Workouttypeid == existingBooking.Schedule.Workouttypeid);
                var price = workoutType?.Price ?? 500m;

                var dialog = new ConfirmationDialog($"Подтвердите оплату на сумму {price:C} за занятие '{workoutType?.Typename}'.");
                var result = await dialog.ShowDialog<bool>(this.VisualRoot as Window);
                if (result != true) return;

                var newPayment = new Payment
                {
                    Clientid = existingBooking.Clientid,
                    Bookingid = existingBooking.Bookingid,
                    Amount = price,
                    Paymentdate = DateOnly.FromDateTime(DateTime.Now),
                    Description = $"Оплата за тренировку: {workoutType?.Typename} от {existingBooking.Schedule.Starttime:dd.MM.yyyy}"
                };

                existingBooking.Ispaid = true;
                dbContext.Payments.Add(newPayment);
                await dbContext.SaveChangesAsync();
            }

            await new ConfirmationDialog("Занятие успешно оплачено!", true).ShowDialog<bool>(this.VisualRoot as Window);
            await LoadMyBookingsAsync();
        }

        private async void RescheduleBooking_Click(object? sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not Booking bookingToReschedule) return;

            var rescheduleWindow = new RescheduleWindow(bookingToReschedule);
            var result = await rescheduleWindow.ShowDialog<bool>(this.VisualRoot as Window);

            if (result == true)
            {
                await LoadMyBookingsAsync();
            }
        }

        private async void CancelBooking_Click(object? sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not Booking bookingToCancel) return;

            if (bookingToCancel.Ispaid)
            {
                var dialog = new ConfirmationDialog("Это занятие уже оплачено. Для возврата средств обратитесь к администрации через раздел 'Мои запросы'.", true);
                await dialog.ShowDialog<bool>(this.VisualRoot as Window);
                return;
            }

            var confirmationDialog = new ConfirmationDialog("Вы уверены, что хотите отменить эту запись?");
            var result = await confirmationDialog.ShowDialog<bool>(this.VisualRoot as Window);
            if (result != true) return;

            using (var dbContext = new AppDbContext())
            {
                try
                {
                    var existingBooking = await dbContext.Bookings
                        .FirstOrDefaultAsync(b => b.Bookingid == bookingToCancel.Bookingid);

                    if (existingBooking == null) return;

                    var scheduleItem = await dbContext.Schedules
                        .FirstOrDefaultAsync(s => s.Scheduleid == existingBooking.Scheduleid);

                    if (scheduleItem != null && scheduleItem.Currentenrollment > 0)
                    {
                        scheduleItem.Currentenrollment--;
                    }

                    dbContext.Bookings.Remove(existingBooking);
                    await dbContext.SaveChangesAsync();

                    var successDialog = new ConfirmationDialog("Запись успешно отменена!", true);
                    await successDialog.ShowDialog<bool>(this.VisualRoot as Window);
                }
                catch (Exception ex)
                {
                    var errorDialog = new ConfirmationDialog($"Ошибка при отмене записи: {ex.Message}", true);
                    await errorDialog.ShowDialog<bool>(this.VisualRoot as Window);
                }
            }

            await LoadMyBookingsAsync();
        }
    }
}