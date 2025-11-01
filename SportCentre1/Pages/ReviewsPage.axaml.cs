using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using SportCentre1.Data;
using SportCentre1.Windows;
using System.Linq;
using System.Threading.Tasks;

namespace SportCentre1.Pages
{
    public partial class ReviewsPage : UserControl
    {
        public ReviewsPage()
        {
            InitializeComponent();
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                var userRole = MainWindow.CurrentUser?.Role?.Rolename;

                if (userRole == "Пользователь")
                {
                    AddButton.Content = "Оставить свой отзыв";
                }
                else if (userRole == "Менеджер" || userRole == "Тренер")
                {
                    BottomPanel.IsVisible = false;
                }
                else if (userRole == "Администратор")
                {
                    AddButton.IsVisible = false;
                    EditButton.IsVisible = false;
                }
            });
            await LoadReviewsAsync();
        }

        private async Task LoadReviewsAsync()
        {
            using (var dbContext = new AppDbContext())
            {
                var data = await dbContext.Reviews
                    .Include(r => r.Client).Include(r => r.Trainer)
                    .OrderByDescending(r => r.Reviewdate)
                    .ToListAsync();
                ReviewsListBox.ItemsSource = data;
            }
        }

        private async void ReviewsListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (ReviewsListBox.SelectedItem is not Review selectedReview)
            {
                EditButton.IsEnabled = false;
                DeleteButton.IsEnabled = false;
                return;
            }

            var userRole = MainWindow.CurrentUser?.Role?.Rolename;
            bool canEdit = false;
            bool canDelete = false;

            if (userRole == "Администратор")
            {
                canDelete = true;
                canEdit = false;
            }
            else if (userRole == "Пользователь")
            {
                using (var dbContext = new AppDbContext())
                {
                    var clientProfile = await dbContext.Clients.FirstOrDefaultAsync(c => c.Userid == MainWindow.CurrentUser.Userid);
                    if (clientProfile != null && selectedReview.Clientid == clientProfile.Clientid)
                    {
                        canEdit = true;
                        canDelete = true;
                    }
                }
            }

            EditButton.IsEnabled = canEdit;
            DeleteButton.IsEnabled = canDelete;
        }

        private async void AddButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var editWindow = new ReviewEditWindow();
            var result = await editWindow.ShowDialog<bool>(this.VisualRoot as Window);
            if (result == true) await LoadReviewsAsync();
        }

        private async void EditButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (ReviewsListBox.SelectedItem is Review selectedReview)
            {
                var editWindow = new ReviewEditWindow(selectedReview);
                var result = await editWindow.ShowDialog<bool>(this.VisualRoot as Window);
                if (result == true) await LoadReviewsAsync();
            }
        }

        private async void DeleteButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (ReviewsListBox.SelectedItem is Review selectedReview)
            {
                var dialog = new ConfirmationDialog("Вы уверены, что хотите удалить этот отзыв?");
                var result = await dialog.ShowDialog<bool>(this.VisualRoot as Window);

                if (result == true)
                {
                    using (var dbContext = new AppDbContext())
                    {
                        var reviewToDelete = await dbContext.Reviews.FindAsync(selectedReview.Reviewid);
                        if (reviewToDelete != null)
                        {
                            dbContext.Reviews.Remove(reviewToDelete);
                            await dbContext.SaveChangesAsync();
                        }
                    }
                    await LoadReviewsAsync();
                }
            }
        }
    }
}