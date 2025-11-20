using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using SportCentre1.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SportCentre1.Windows
{
    public partial class RescheduleWindow : Window
    {
        private readonly Booking _originalBooking;

        public RescheduleWindow(Booking originalBooking)
        {
            InitializeComponent();
            _originalBooking = originalBooking;
            _ = LoadAvailableSlotsAsync();
        }

        private async Task LoadAvailableSlotsAsync()
        {
            using (var dbContext = new AppDbContext())
            {
                var originalSchedule = await dbContext.Schedules
                    .Include(s => s.Workouttype)
                    .FirstOrDefaultAsync(s => s.Scheduleid == _originalBooking.Scheduleid);

                if (originalSchedule == null)
                {
                    await new ConfirmationDialog("Исходное занятие не найдено.", true).ShowDialog<bool>(this);
                    Close(false);
                    return;
                }

                TitleTextBlock.Text = $"Перенос занятия: {originalSchedule.Workouttype.Typename}";

                var availableSlots = await dbContext.Schedules
                    .Include(s => s.Trainer)
                    .Where(s => s.Workouttypeid == originalSchedule.Workouttypeid &&
                                s.Scheduleid != _originalBooking.Scheduleid &&
                                s.Starttime > DateTime.Now &&
                                s.Currentenrollment < s.Maxcapacity)
                    .OrderBy(s => s.Starttime)
                    .ToListAsync();

                AvailableSlotsListBox.ItemsSource = availableSlots;
            }
        }

        private void AvailableSlotsListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            ConfirmButton.IsEnabled = AvailableSlotsListBox.SelectedItem != null;
        }

        private async void ConfirmButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (AvailableSlotsListBox.SelectedItem is not Schedule newSchedule) return;

            using (var dbContext = new AppDbContext())
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // ======================================================================
                    // НОВЫЙ БЛОК: ПРОВЕРКА НА КОНФЛИКТ ЗАПИСЕЙ У КЛИЕНТА
                    // ======================================================================
                    bool clientHasConflict = await dbContext.Bookings
                        .Include(b => b.Schedule)
                        .AnyAsync(b =>
                            b.Clientid == _originalBooking.Clientid && // У этого же клиента
                            b.Bookingid != _originalBooking.Bookingid && // Исключаем текущую запись
                            b.Schedule.Starttime < newSchedule.Endtime && // Проверка пересечения
                            b.Schedule.Endtime > newSchedule.Starttime);

                    if (clientHasConflict)
                    {
                        await new ConfirmationDialog("Ошибка! У вас уже есть другая запись, пересекающаяся с выбранным временем.", true)
                            .ShowDialog<bool>(this);
                        await transaction.RollbackAsync();
                        return; // Прерываем операцию
                    }
                    // ======================================================================

                    var oldSchedule = await dbContext.Schedules.FindAsync(_originalBooking.Scheduleid);
                    if (oldSchedule != null && oldSchedule.Currentenrollment > 0)
                    {
                        oldSchedule.Currentenrollment--;
                    }

                    var targetSchedule = await dbContext.Schedules.FindAsync(newSchedule.Scheduleid);
                    if (targetSchedule == null || targetSchedule.Currentenrollment >= targetSchedule.Maxcapacity)
                    {
                        throw new Exception("В выбранной группе закончились места.");
                    }
                    targetSchedule.Currentenrollment++;

                    var bookingToUpdate = await dbContext.Bookings.FindAsync(_originalBooking.Bookingid);
                    if (bookingToUpdate != null)
                    {
                        bookingToUpdate.Scheduleid = newSchedule.Scheduleid;
                        bookingToUpdate.Bookingtime = DateTime.Now;
                    }
                    else
                    {
                        throw new Exception("Исходное бронирование не найдено.");
                    }

                    await dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    await new ConfirmationDialog("Запись успешно перенесена!", true).ShowDialog<bool>(this);
                    Close(true);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    await new ConfirmationDialog($"Ошибка при переносе: {ex.Message}", true).ShowDialog<bool>(this);
                    Close(false);
                }
            }
        }
    }
}