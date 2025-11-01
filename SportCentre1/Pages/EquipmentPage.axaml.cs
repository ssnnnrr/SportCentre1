using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using SportCentre1.Data;
using SportCentre1.Models; 
using SportCentre1.Windows;
using System.Linq;
using System.Threading.Tasks;

namespace SportCentre1.Pages
{
    public partial class EquipmentPage : UserControl
    {
        public EquipmentPage()
        {
            InitializeComponent();
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                var userRole = MainWindow.CurrentUser?.Role?.Rolename;
                if (userRole == "Тренер" || userRole == "Пользователь")
                {
                    BottomPanel.IsVisible = false;
                }
            });

            await LoadEquipmentAsync();
        }

        private async Task LoadEquipmentAsync()
        {
            using (var dbContext = new AppDbContext())
            {
                var data = await dbContext.Equipment.OrderBy(e => e.Name).ToListAsync();
                // Оборачиваем каждый объект Equipment в EquipmentViewModel
                EquipmentDataGrid.ItemsSource = data.Select(eq => new EquipmentViewModel(eq)).ToList();
            }
        }

        private void EquipmentDataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            bool isSelected = EquipmentDataGrid.SelectedItem != null;
            EditButton.IsEnabled = isSelected;
            DeleteButton.IsEnabled = isSelected;
        }

        private async void AddButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var editWindow = new EquipmentEditWindow();
            var result = await editWindow.ShowDialog<bool>(this.VisualRoot as Window);
            if (result == true) await LoadEquipmentAsync();
        }

        private async void EditButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Получаем ViewModel из выбранной строки
            if (EquipmentDataGrid.SelectedItem is EquipmentViewModel selectedVM)
            {
                using (var db = new AppDbContext())
                {
                    // Находим оригинальный объект Equipment по ID для редактирования
                    var equipmentToEdit = await db.Equipment.FindAsync(selectedVM.Equipmentid);
                    if (equipmentToEdit != null)
                    {
                        var editWindow = new EquipmentEditWindow(equipmentToEdit);
                        var result = await editWindow.ShowDialog<bool>(this.VisualRoot as Window);
                        if (result == true) await LoadEquipmentAsync();
                    }
                }
            }
        }

        private async void DeleteButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (EquipmentDataGrid.SelectedItem is EquipmentViewModel selectedVM)
            {
                var dialog = new ConfirmationDialog($"Вы уверены, что хотите удалить: {selectedVM.Name}?");
                var result = await dialog.ShowDialog<bool>(this.VisualRoot as Window);
                if (result != true) return;

                using (var db = new AppDbContext())
                {
                    var equipmentToDelete = await db.Equipment.FindAsync(selectedVM.Equipmentid);
                    if (equipmentToDelete != null)
                    {
                        db.Equipment.Remove(equipmentToDelete);
                        await db.SaveChangesAsync();
                        await LoadEquipmentAsync();
                    }
                }
            }
        }
    }
}