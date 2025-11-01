using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using SportCentre1.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SportCentre1.Windows
{
    public partial class MembershipPurchaseWindow : Window
    {
        public MembershipPurchaseWindow()
        {
            InitializeComponent();
            _ = LoadMembershipsAsync();
        }

        private async Task LoadMembershipsAsync()
        {
            using (var dbContext = new AppDbContext())
            {
                MembershipsListBox.ItemsSource = await dbContext.Membershiptypes
                    .OrderBy(m => m.Price)
                    .ToListAsync();
            }
        }

        private void MembershipsListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            ConfirmButton.IsEnabled = MembershipsListBox.SelectedItem != null;
        }

        private async void ConfirmButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (MembershipsListBox.SelectedItem is not Membershiptype selectedMembership) return;

            var dialog = new ConfirmationDialog($"Вы уверены, что хотите приобрести абонемент '{selectedMembership.Typename}' за {selectedMembership.Price:C}?");
            var result = await dialog.ShowDialog<bool>(this);
            if (result != true) return;

            using (var dbContext = new AppDbContext())
            {
                var clientProfile = await dbContext.Clients.FirstOrDefaultAsync(c => c.Userid == MainWindow.CurrentUser!.Userid);
                if (clientProfile == null)
                {
                    await new ConfirmationDialog("Ошибка: профиль клиента не найден.", true).ShowDialog<bool>(this);
                    return;
                }

                // --- НОВАЯ ЛОГИКА ПРОВЕРКИ ---
                // Проверяем, есть ли у клиента уже активный абонемент.
                bool hasActiveMembership = await dbContext.ClientMemberships
                    .AnyAsync(cm => cm.Clientid == clientProfile.Clientid && cm.EndDate >= DateOnly.FromDateTime(DateTime.Now));

                if (hasActiveMembership)
                {
                    // Если абонемент есть, выводим сообщение и прерываем операцию.
                    var errorDialog = new ConfirmationDialog(
                        "У вас уже есть активный абонемент. Вы сможете приобрести новый после окончания срока действия текущего.",
                        true);
                    await errorDialog.ShowDialog<bool>(this);
                    return; 
                }

                
                var newPayment = new Payment
                {
                    Clientid = clientProfile.Clientid,
                    Amount = selectedMembership.Price,
                    Paymentdate = DateOnly.FromDateTime(DateTime.Now),
                    Description = $"Покупка абонемента: {selectedMembership.Typename}",
                    Membershiptypeid = selectedMembership.Membershiptypeid
                };
                dbContext.Payments.Add(newPayment);

                var newClientMembership = new ClientMembership
                {
                    Clientid = clientProfile.Clientid,
                    Membershiptypeid = selectedMembership.Membershiptypeid,
                    StartDate = DateOnly.FromDateTime(DateTime.Now),
                    EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(selectedMembership.Durationdays))
                };
                dbContext.ClientMemberships.Add(newClientMembership);

                await dbContext.SaveChangesAsync();
            }

            var successDialog = new ConfirmationDialog(
                "Абонемент успешно приобретен! \n\nТеперь вернитесь на страницу 'Расписание', чтобы выбрать и забронировать тренировку.",
                true);
            await successDialog.ShowDialog<bool>(this);

            Close(true);
        }
    }
}