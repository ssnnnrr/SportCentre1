using Avalonia.Controls;
using SportCentre1.Data;
using System;
using System.Collections.Generic;

namespace SportCentre1.Windows
{
    public partial class EquipmentEditWindow : Window
    {
        private Equipment _currentEquipment;
        private bool _isNew;
        private readonly List<string> _statuses = new List<string> { "В рабочем состоянии", "В ремонте", "Списано" };

        public EquipmentEditWindow()
        {
            InitializeComponent();
            _isNew = true;
            _currentEquipment = new Equipment();
            SetupControls();
            MaintenanceDatePicker.SelectedDate = DateTime.Today;
        }

        public EquipmentEditWindow(Equipment equipmentToEdit)
        {
            InitializeComponent();
            _isNew = false;
            _currentEquipment = equipmentToEdit;
            SetupControls();
            LoadEquipmentData();
        }

        private void SetupControls()
        {
            StatusComboBox.ItemsSource = _statuses;
        }

        private void LoadEquipmentData()
        {
            NameTextBox.Text = _currentEquipment.Name;
            StatusComboBox.SelectedItem = _currentEquipment.Status;
            QuantityUpDown.Value = _currentEquipment.Quantity;
            if (_currentEquipment.Lastmaintenancedate.HasValue)
            {
                MaintenanceDatePicker.SelectedDate = new DateTimeOffset(_currentEquipment.Lastmaintenancedate.Value.ToDateTime(TimeOnly.MinValue));
            }
        }

        private async void SaveButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text) || StatusComboBox.SelectedItem == null)
            {
                var dialog = new ConfirmationDialog("Название и Статус являются обязательными полями.", true);
                await dialog.ShowDialog<bool>(this);
                return;
            }

            _currentEquipment.Name = NameTextBox.Text;
            _currentEquipment.Status = StatusComboBox.SelectedItem as string;
            _currentEquipment.Quantity = (int)(QuantityUpDown.Value ?? 1);
            _currentEquipment.Lastmaintenancedate = MaintenanceDatePicker.SelectedDate.HasValue ? DateOnly.FromDateTime(MaintenanceDatePicker.SelectedDate.Value.DateTime) : null;

            using (var dbContext = new AppDbContext())
            {
                if (_isNew)
                {
                    dbContext.Equipment.Add(_currentEquipment);
                }
                else
                {
                    dbContext.Equipment.Update(_currentEquipment);
                }
                await dbContext.SaveChangesAsync();
            }
            Close(true);
        }
    }
}