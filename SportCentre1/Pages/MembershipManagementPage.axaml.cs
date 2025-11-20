using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Microsoft.EntityFrameworkCore;
using SportCentre1.Data;
using SportCentre1.Windows;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SportCentre1.Pages
{
    public partial class MembershipManagementPage : UserControl
    {
        private const decimal SINGLE_VISIT_PRICE = 700m;

        private Client? _currentClient;
        // ИСПРАВЛЕНО: Тип Clientmembership соответствует вашему сгенерированному классу
        private Clientmembership? _activeMembership;
        private decimal _refundAmount = 0;

        public MembershipManagementPage()
        {
            InitializeComponent();
            _ = LoadActiveMembershipAsync();
        }

        private async Task LoadActiveMembershipAsync()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    _currentClient = await db.Clients
                        .FirstOrDefaultAsync(c => c.Userid == MainWindow.CurrentUser!.Userid);

                    if (_currentClient is null)
                    {
                        ShowNoActiveMembershipMessage();
                        return;
                    }

                    var today = DateOnly.FromDateTime(DateTime.Now);

                    // ИСПРАВЛЕНО: Используем db.Clientmemberships (с маленькой буквы)
                    _activeMembership = await db.Clientmemberships
                        .Include(cm => cm.Membershiptype)
                        .Where(cm => cm.Clientid == _currentClient.Clientid && cm.Enddate >= today)
                        .FirstOrDefaultAsync();

                    if (_activeMembership is not null)
                    {
                        MembershipTypeTextBlock.Text = _activeMembership.Membershiptype.Typename;
                        // ИСПРАВЛЕНО: Используем свойство Enddate (с большой буквы) из вашего класса
                        EndDateTextBlock.Text = _activeMembership.Enddate.ToString("dd.MM.yyyy");
                        StatusTextBlock.Text = "Активен";

                        // ИСПРАВЛЕНО: Убран проблемный код с .Presenter. Валидация будет в кнопке.
                    }
                    else
                    {
                        ShowNoActiveMembershipMessage();
                    }
                }
            }
            catch (Exception ex)
            {
                await ShowErrorDialog($"Ошибка загрузки абонемента: {ex.Message}");
                ShowNoActiveMembershipMessage();
            }
        }

        private void ShowNoActiveMembershipMessage()
        {
            NoMembershipMessage.IsVisible = true;
            CurrentMembershipCard.IsVisible = false;
            FreezeCard.IsVisible = false;
            RefundCard.IsVisible = false;
        }

        private async void FreezeButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_activeMembership is null) return;
            if (!FreezeStartDatePicker.SelectedDate.HasValue || !FreezeEndDatePicker.SelectedDate.HasValue)
            {
                await ShowErrorDialog("Пожалуйста, выберите дату начала и окончания заморозки.");
                return;
            }

            var startDate = DateOnly.FromDateTime(FreezeStartDatePicker.SelectedDate.Value.DateTime);
            var endDate = DateOnly.FromDateTime(FreezeEndDatePicker.SelectedDate.Value.DateTime);

            // ИСПРАВЛЕНО: Вместо .Presenter теперь простая и надежная проверка даты здесь
            if (startDate < DateOnly.FromDateTime(DateTime.Today) || endDate < startDate)
            {
                await ShowErrorDialog("Даты выбраны некорректно. Начало заморозки не может быть в прошлом, а окончание - раньше начала.");
                return;
            }

            int freezeDays = (endDate.DayNumber - startDate.DayNumber) + 1;

            var dialog = new ConfirmationDialog($"Вы уверены, что хотите заморозить абонемент на {freezeDays} дней? " +
                                                $"Новая дата окончания: {_activeMembership.Enddate.AddDays(freezeDays):dd.MM.yyyy}.");
            var result = await dialog.ShowDialog<bool>(this.GetVisualRoot() as Window);

            if (result == true)
            {
                try
                {
                    using (var db = new AppDbContext())
                    {
                        var membershipToUpdate = await db.Clientmemberships.FindAsync(_activeMembership.Clientmembershipid);
                        if (membershipToUpdate != null)
                        {
                            membershipToUpdate.Enddate = membershipToUpdate.Enddate.AddDays(freezeDays);

                            var pauseRecord = new ClientMembershipPause
                            {
                                Clientmembershipid = membershipToUpdate.Clientmembershipid,
                                Startdate = startDate,
                                Enddate = endDate
                            };
                            db.ClientMembershipPauses.Add(pauseRecord);

                            await db.SaveChangesAsync();

                            await new ConfirmationDialog("Абонемент успешно заморожен!", true).ShowDialog<bool>(this.GetVisualRoot() as Window);
                            await LoadActiveMembershipAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    await ShowErrorDialog($"Ошибка при заморозке: {ex.Message}");
                }
            }
        }

        private async void CalculateRefundButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_activeMembership is null || _currentClient is null) return;
            ConfirmRefundButton.IsVisible = false;

            try
            {
                using (var db = new AppDbContext())
                {
                    decimal originalPrice = _activeMembership.Membershiptype.Price;

                    var membershipStartDate = _activeMembership.Startdate.ToDateTime(TimeOnly.MinValue);

                    int visitsCount = await db.Bookings
                        .CountAsync(b => b.Clientid == _currentClient.Clientid &&
                                         b.Bookingtime.Date >= membershipStartDate);

                    decimal usedAmount = visitsCount * SINGLE_VISIT_PRICE;
                    _refundAmount = originalPrice - usedAmount;

                    if (_refundAmount < 0) _refundAmount = 0;

                    RefundInfoTextBlock.Text = $"Стоимость абонемента: {originalPrice:C}\n" +
                                               $"Количество посещений: {visitsCount}\n" +
                                               $"Списано за посещения: {usedAmount:C}\n" +
                                               $"СУММА К ВОЗВРАТУ: {_refundAmount:C}";

                    if (_refundAmount > 0)
                    {
                        ConfirmRefundButton.IsVisible = true;
                    }
                }
            }
            catch (Exception ex)
            {
                await ShowErrorDialog($"Ошибка расчета: {ex.Message}");
            }
        }

        private async void ConfirmRefundButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_currentClient is null || _activeMembership is null) return;

            var dialog = new ConfirmationDialog($"Вы уверены, что хотите отправить запрос на возврат средств в размере {_refundAmount:C}? " +
                                                "Ваш абонемент будет деактивирован.");
            var result = await dialog.ShowDialog<bool>(this.GetVisualRoot() as Window);
            if (result != true) return;

            try
            {
                using (var db = new AppDbContext())
                {
                    var newRequest = new Request
                    {
                        Clientid = _currentClient.Clientid,
                        Subject = $"Запрос на возврат средств по абонементу",
                        Status = "Открыт",
                        Creationdate = DateTime.Now
                    };
                    db.Requests.Add(newRequest);
                    await db.SaveChangesAsync();

                    var newMessage = new Message
                    {
                        Requestid = newRequest.Requestid,
                        Senderuserid = MainWindow.CurrentUser!.Userid,
                        Messagetext = $"Прошу произвести возврат средств в размере {_refundAmount:C} и деактивировать мой абонемент (ID: {_activeMembership.Clientmembershipid}).",
                        Sentdate = DateTime.Now
                    };
                    db.Messages.Add(newMessage);

                    var membershipToExpire = await db.Clientmemberships.FindAsync(_activeMembership.Clientmembershipid);
                    if (membershipToExpire is not null)
                    {
                        membershipToExpire.Enddate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
                    }

                    await db.SaveChangesAsync();

                    await new ConfirmationDialog("Ваш запрос отправлен администратору. Абонемент деактивирован.", true)
                        .ShowDialog<bool>(this.GetVisualRoot() as Window);

                    await LoadActiveMembershipAsync();
                }
            }
            catch (Exception ex)
            {
                await ShowErrorDialog($"Ошибка отправки запроса: {ex.Message}");
            }
        }

        private async Task ShowErrorDialog(string message)
        {
            var dialog = new ConfirmationDialog(message, true);
            await dialog.ShowDialog<bool>(this.GetVisualRoot() as Window);
        }
    }
}