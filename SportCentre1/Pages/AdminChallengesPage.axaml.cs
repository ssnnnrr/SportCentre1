using Avalonia.Controls;
using Avalonia.VisualTree;
using Microsoft.EntityFrameworkCore;
using SportCentre1.Data;
using SportCentre1.Windows;
using System.Linq;
using System.Threading.Tasks;
namespace SportCentre1.Pages
{
    public partial class AdminChallengesPage : UserControl
    {
        public AdminChallengesPage()
        {
            InitializeComponent();
            LoadChallenges();
        }

        private async void LoadChallenges()
        {
            try
            {
                using (var dbContext = new AppDbContext())
                {
                    ChallengesDataGrid.ItemsSource = await dbContext.Challenges
                        .OrderByDescending(c => c.Startdate)
                        .ToListAsync();
                }
            }
            catch (System.Exception ex)
            {
                var errorDialog = new ConfirmationDialog($"Не удалось загрузить челленджи: {ex.Message}", true);
                await errorDialog.ShowDialog<bool>(this.GetVisualRoot() as Window);
            }
        }

        private void ChallengesDataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            bool isSelected = ChallengesDataGrid.SelectedItem != null;
            EditButton.IsEnabled = isSelected;
            DeleteButton.IsEnabled = isSelected;
        }

        private async void AddButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var editWindow = new ChallengeEditWindow();
            var result = await editWindow.ShowDialog<bool>(this.GetVisualRoot() as Window);
            if (result == true) LoadChallenges();
        }

        private async void EditButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (ChallengesDataGrid.SelectedItem is Challenge selectedChallenge)
            {
                var editWindow = new ChallengeEditWindow(selectedChallenge);
                var result = await editWindow.ShowDialog<bool>(this.GetVisualRoot() as Window);
                if (result == true) LoadChallenges();
            }
        }

        private async void DeleteButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (ChallengesDataGrid.SelectedItem is Challenge selectedChallenge)
            {
                var dialog = new ConfirmationDialog($"Удалить челлендж '{selectedChallenge.Title}'? Это действие нельзя будет отменить.");
                var result = await dialog.ShowDialog<bool>(this.GetVisualRoot() as Window);
                if (result != true) return;

                try
                {
                    using (var dbContext = new AppDbContext())
                    {
                        // Важно! EF Core не может удалить объект, который не отслеживается.
                        // Поэтому мы сначала "прикрепляем" его к контексту.
                        dbContext.Challenges.Attach(selectedChallenge);
                        dbContext.Challenges.Remove(selectedChallenge);
                        await dbContext.SaveChangesAsync();
                    }
                    LoadChallenges();
                }
                catch (System.Exception ex)
                {
                    var errorDialog = new ConfirmationDialog($"Ошибка удаления: {ex.Message}", true);
                    await errorDialog.ShowDialog<bool>(this.GetVisualRoot() as Window);
                }
            }
        }
    }
}