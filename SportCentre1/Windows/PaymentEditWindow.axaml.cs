using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using SportCentre1.Data;
using System;
using System.Linq;

namespace SportCentre1.Windows
{
    public partial class PaymentEditWindow : Window
    {
        private Payment _currentPayment;
        private bool _isNew;

        public PaymentEditWindow()
        {
            InitializeComponent();
            _isNew = true;
            _currentPayment = new Payment();
            SetupControls();
            PaymentDatePicker.SelectedDate = DateTime.Today;
        }

        public PaymentEditWindow(Payment paymentToEdit)
        {
            InitializeComponent();
            _isNew = false;
            _currentPayment = paymentToEdit;
            SetupControls();
            LoadPaymentData();
        }

        private async void SetupControls()
        {
            using (var dbContext = new AppDbContext())
            {
                ClientComboBox.ItemsSource = await dbContext.Clients.OrderBy(c => c.Lastname).ToListAsync();
            }
        }

        private void LoadPaymentData()
        {
            ClientComboBox.SelectedItem = (ClientComboBox.ItemsSource as System.Collections.IEnumerable)?
                .OfType<Client>().FirstOrDefault(c => c.Clientid == _currentPayment.Clientid);

            AmountUpDown.Value = (decimal?)(double)_currentPayment.Amount;
            DescriptionTextBox.Text = _currentPayment.Description;

        }

        private async void SaveButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (ClientComboBox.SelectedItem == null)
            {
                var dialog = new ConfirmationDialog("Необходимо выбрать клиента.", true);
                await dialog.ShowDialog<bool>(this); return;
            }
            if (AmountUpDown.Value <= 0)
            {
                var dialog = new ConfirmationDialog("Сумма платежа должна быть больше нуля.", true);
                await dialog.ShowDialog<bool>(this); return;
            }

            _currentPayment.Clientid = (ClientComboBox.SelectedItem as Client)!.Clientid;
            _currentPayment.Amount = (decimal)(AmountUpDown.Value ?? 0);
            _currentPayment.Description = DescriptionTextBox.Text;
            if (PaymentDatePicker.SelectedDate.HasValue)
            {
                _currentPayment.Paymentdate = DateOnly.FromDateTime(PaymentDatePicker.SelectedDate.Value.DateTime);
            }

            using (var dbContext = new AppDbContext())
            {
                if (_isNew)
                {
                    dbContext.Payments.Add(_currentPayment);
                }
                else
                {
                    dbContext.Payments.Update(_currentPayment);
                }
                await dbContext.SaveChangesAsync();
            }
            Close(true);
        }
    }
}