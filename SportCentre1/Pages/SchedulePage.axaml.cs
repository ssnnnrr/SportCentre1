using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using SportCentre1.Data;
using SportCentre1.Models;
using SportCentre1.Windows;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;

namespace SportCentre1.Pages
{
    public partial class SchedulePage : UserControl
    {
        private Button? _bookButton;
        private Button? _buyMembershipButton;

        public SchedulePage()
        {
            InitializeComponent();
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            var userRole = MainWindow.CurrentUser?.Role?.Rolename;
            var data = await LoadScheduleDataAsync(userRole);

            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                ScheduleListBox.ItemsSource = data;

                if (userRole == "Пользователь")
                {
                    AddButton.IsVisible = false;
                    EditButton.IsVisible = false;
                    DeleteButton.IsVisible = false;

                    _buyMembershipButton = new Button { Content = "Купить абонемент" };
                    _buyMembershipButton.Classes.Add("accent"); 
                    _buyMembershipButton.Click += BuyMembershipButton_Click;
                    BottomPanel.Children.Add(_buyMembershipButton);

                    _bookButton = new Button { Content = "Записаться на занятие", IsEnabled = false };
                    _bookButton.Click += BookButton_Click;
                    BottomPanel.Children.Add(_bookButton);
                }
                else if (userRole == "Тренер")
                {
                    AddButton.IsVisible = false;
                    EditButton.IsVisible = false;
                    DeleteButton.IsVisible = false;
                }
            });
        }

        private async void BuyMembershipButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var purchaseWindow = new MembershipPurchaseWindow();
            await purchaseWindow.ShowDialog<bool>(this.VisualRoot as Window);
        }

        private async Task<IEnumerable> LoadScheduleDataAsync(string? userRole)
        {
            using (var dbContext = new AppDbContext())
            {
                var baseQuery = dbContext.Schedules.AsQueryable();

                if (userRole == "Тренер")
                {
                    var currentTrainer = await dbContext.Trainers
                        .FirstOrDefaultAsync(t => t.Userid == MainWindow.CurrentUser!.Userid);
                    if (currentTrainer != null)
                    {
                        baseQuery = baseQuery.Where(s => s.Trainerid == currentTrainer.Trainerid);
                    }
                }

                var scheduleData = await baseQuery
                    .Include(s => s.Trainer)
                    .Include(s => s.Workouttype)
                    .Where(s => s.Starttime >= DateTime.Now) 
                    .OrderBy(s => s.Starttime)
                    .ToListAsync();

                return scheduleData.Select(s => new ScheduleInfo
                {
                    ScheduleId = s.Scheduleid,
                    WorkoutName = s.Workouttype?.Typename ?? "N/A",
                    TrainerName = (s.Trainer != null) ? $"{s.Trainer.Firstname} {s.Trainer.Lastname}" : "N/A",
                    StartTime = s.Starttime,
                    EndTime = s.Endtime,
                    EnrollmentInfo = $"{s.Currentenrollment} / {s.Maxcapacity}"
                }).ToList();
            }
        }

        private void ScheduleListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            bool isSelected = ScheduleListBox.SelectedItem != null;
            EditButton.IsEnabled = isSelected;
            DeleteButton.IsEnabled = isSelected;
            if (_bookButton != null) _bookButton.IsEnabled = isSelected;
        }

        private async void BookButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (ScheduleListBox.SelectedItem is not ScheduleInfo selectedWorkout) return;

            using (var dbContext = new AppDbContext())
            {
                var clientProfile = await dbContext.Clients.FirstOrDefaultAsync(c => c.Userid == MainWindow.CurrentUser!.Userid);
                if (clientProfile == null) return;

                bool alreadyBooked = await dbContext.Bookings
                .AnyAsync(b => b.Scheduleid == selectedWorkout.ScheduleId && b.Clientid == clientProfile.Clientid);

                if (alreadyBooked)
                {
                    var dialog = new ConfirmationDialog("Вы уже записаны на это занятие.", true);
                    await dialog.ShowDialog<bool>(this.VisualRoot as Window);
                    return;
                }

                var scheduleItem = await dbContext.Schedules.FindAsync(selectedWorkout.ScheduleId);
                if (scheduleItem == null || scheduleItem.Currentenrollment >= scheduleItem.Maxcapacity)
                {
                    var dialog = new ConfirmationDialog("К сожалению, все места в группе заняты.", true);
                    await dialog.ShowDialog<bool>(this.VisualRoot as Window);
                    return;
                }

                var today = DateOnly.FromDateTime(DateTime.Now);
                bool hasActiveMembership = await dbContext.ClientMemberships
                    .AnyAsync(cm => cm.Clientid == clientProfile.Clientid && cm.StartDate <= today && cm.EndDate >= today);

                var newBooking = new Booking
                {
                    Clientid = clientProfile.Clientid,
                    Scheduleid = selectedWorkout.ScheduleId,
                    Ispaid = hasActiveMembership
                };

                scheduleItem.Currentenrollment++;
                dbContext.Bookings.Add(newBooking);
                await dbContext.SaveChangesAsync();

                ConfirmationDialog successDialog;
                if (hasActiveMembership)
                {
                    successDialog = new ConfirmationDialog("Вы успешно записаны! Занятие покрывается вашим действующим абонементом.", true);
                }
                else
                {
                    successDialog = new ConfirmationDialog("Вы успешно записаны! Не забудьте оплатить занятие в разделе 'Мои записи'.", true);
                }
                await successDialog.ShowDialog<bool>(this.VisualRoot as Window);

                ScheduleListBox.ItemsSource = await LoadScheduleDataAsync(MainWindow.CurrentUser?.Role?.Rolename);
            }
        }

        private async void AddButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var editWindow = new ScheduleEditWindow();
            var result = await editWindow.ShowDialog<bool>(this.VisualRoot as Window);
            if (result == true)
            {
                ScheduleListBox.ItemsSource = await LoadScheduleDataAsync(MainWindow.CurrentUser?.Role?.Rolename);
            }
        }

        private async void EditButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (ScheduleListBox.SelectedItem is ScheduleInfo selectedInfo)
            {
                using (var dbContext = new AppDbContext())
                {
                    var scheduleToEdit = await dbContext.Schedules.FindAsync(selectedInfo.ScheduleId);
                    if (scheduleToEdit == null) return;

                    var editWindow = new ScheduleEditWindow(scheduleToEdit);
                    var result = await editWindow.ShowDialog<bool>(this.VisualRoot as Window);
                    if (result == true)
                    {
                        ScheduleListBox.ItemsSource = await LoadScheduleDataAsync(MainWindow.CurrentUser?.Role?.Rolename);
                    }
                }
            }
        }

        private async void DeleteButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (ScheduleListBox.SelectedItem is ScheduleInfo selectedInfo)
            {
                var dialog = new ConfirmationDialog("Вы уверены, что хотите удалить эту запись из расписания?");
                var result = await dialog.ShowDialog<bool>(this.VisualRoot as Window);
                if (result != true) return;

                using (var dbContext = new AppDbContext())
                {
                    var scheduleToDelete = await dbContext.Schedules.FindAsync(selectedInfo.ScheduleId);
                    if (scheduleToDelete == null) return;

                    dbContext.Schedules.Remove(scheduleToDelete);
                    await dbContext.SaveChangesAsync();
                    ScheduleListBox.ItemsSource = await LoadScheduleDataAsync(MainWindow.CurrentUser?.Role?.Rolename);
                }
            }
        }
    }
}