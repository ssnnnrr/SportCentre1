using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using SportCentre1.Data;
using SportCentre1.Windows;
using System;
using System.Linq;

namespace SportCentre1.Pages
{
    public partial class PaymentsPage : UserControl
    {
        public PaymentsPage()
        {
            InitializeComponent();
            LoadPayments();
        }

        private async void LoadPayments()
        {
            using (var dbContext = new AppDbContext())
            {
                PaymentsDataGrid.ItemsSource = await dbContext.Payments
                    .Include(p => p.Client)
                    .OrderByDescending(p => p.Paymentdate)
                    .AsNoTracking()
                    .ToListAsync();
            }
        }

        private void PaymentsDataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (PaymentsDataGrid.SelectedItem is Payment selectedPayment)
            {
                bool canRefund = selectedPayment.Amount > 0 && !IsRefunded(selectedPayment);
                RefundButton.IsEnabled = canRefund;
            }
            else
            {
                RefundButton.IsEnabled = false;
            }
        }

        private bool IsRefunded(Payment payment)
        {
            using (var dbContext = new AppDbContext())
            {
                return dbContext.Payments
                    .Any(p => (p.Bookingid.HasValue && p.Bookingid == payment.Bookingid && p.Amount == -payment.Amount) ||
                              p.Description.Contains($"Возврат средств за: {payment.Description}"));
            }
        }

        private async void AddButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var editWindow = new PaymentEditWindow();
            var result = await editWindow.ShowDialog<bool>(this.VisualRoot as Window);
            if (result == true) LoadPayments();
        }

        private async void RefundButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (PaymentsDataGrid.SelectedItem is not Payment paymentToRefund) return;

            if (IsRefunded(paymentToRefund))
            {
                var errorDialog = new ConfirmationDialog("Возврат по этому платежу уже был выполнен ранее.", true);
                await errorDialog.ShowDialog<bool>(this.VisualRoot as Window);
                RefundButton.IsEnabled = false;
                return;
            }

            var dialog = new ConfirmationDialog($"Оформить возврат на сумму {paymentToRefund.Amount:C}?");
            var result = await dialog.ShowDialog<bool>(this.VisualRoot as Window);
            if (result != true) return;

            using (var dbContext = new AppDbContext())
            {
                try
                {
                    var originalPayment = await dbContext.Payments.FindAsync(paymentToRefund.Paymentid);
                    if (originalPayment == null)
                    {
                        await new ConfirmationDialog("Платеж не найден.", true).ShowDialog<bool>(this.VisualRoot as Window);
                        return;
                    }

                    var refundPayment = new Payment
                    {
                        Clientid = originalPayment.Clientid,
                        Bookingid = originalPayment.Bookingid, 
                        Amount = -originalPayment.Amount,
                        Paymentdate = DateOnly.FromDateTime(DateTime.Now),
                        Description = $"Возврат средств за: {originalPayment.Description}"
                    };
                    dbContext.Payments.Add(refundPayment);

                    if (originalPayment.Bookingid != null)
                    {
                        var booking = await dbContext.Bookings.FindAsync(originalPayment.Bookingid);
                        if (booking != null)
                        {
                            booking.Ispaid = false; 
                        }
                    }

                    if (originalPayment.Membershiptypeid != null)
                    {
                        var membershipToExpire = await dbContext.ClientMemberships
                            .Where(cm => cm.Clientid == originalPayment.Clientid && cm.Membershiptypeid == originalPayment.Membershiptypeid)
                            .OrderByDescending(cm => cm.EndDate)
                            .FirstOrDefaultAsync();
                        if (membershipToExpire != null)
                        {
                            membershipToExpire.EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
                        }
                    }

                    await dbContext.SaveChangesAsync();
                    await new ConfirmationDialog("Возврат успешно оформлен! Клиент теперь может отменить свою запись.", true).ShowDialog<bool>(this.VisualRoot as Window);
                }
                catch (Exception ex)
                {
                    await new ConfirmationDialog($"Ошибка при оформлении возврата: {ex.Message}", true).ShowDialog<bool>(this.VisualRoot as Window);
                }
            }

            LoadPayments();
            PaymentsDataGrid.SelectedItem = null;
        }
    }
    }
