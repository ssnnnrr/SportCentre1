using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using SportCentre1.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SportCentre1.Windows
{
    public partial class ReviewEditWindow : Window
    {
        private Review _currentReview;
        private bool _isNew;

        public ReviewEditWindow()
        {
            InitializeComponent();
            _isNew = true;
            _currentReview = new Review();
            _ = SetupControlsAsync();
            ReviewDatePicker.SelectedDate = DateTime.Today;
        }

        public ReviewEditWindow(Review reviewToEdit)
        {
            InitializeComponent();
            _isNew = false;
            _currentReview = reviewToEdit;
            _ = SetupControlsAsync();
            LoadReviewData();
        }

        private async Task SetupControlsAsync()
        {
            try
            {
                var userRole = MainWindow.CurrentUser?.Role?.Rolename;

                if (userRole == "Администратор" || userRole == "Менеджер")
                {
                    ClientComboBox.IsVisible = true;
                    ClientPromptTextBlock.IsVisible = true;

                    using (var clientContext = new AppDbContext())
                    {
                        ClientComboBox.ItemsSource = await clientContext.Clients
                            .OrderBy(c => c.Lastname)
                            .AsNoTracking()
                            .ToListAsync();
                    }
                }
                else
                {
                    ClientComboBox.IsVisible = false;
                    ClientPromptTextBlock.IsVisible = false;
                }

                using (var trainerContext = new AppDbContext())
                {
                    TrainerComboBox.ItemsSource = await trainerContext.Trainers
                        .OrderBy(t => t.Lastname)
                        .AsNoTracking()
                        .ToListAsync();
                }
            }
            catch (Exception ex)
            {
                var dialog = new ConfirmationDialog($"Ошибка загрузки данных: {ex.Message}", true);
                await dialog.ShowDialog<bool>(this);
            }
        }

        private void LoadReviewData()
        {
            try
            {
                if (!_isNew)
                {
                    using (var loadContext = new AppDbContext())
                    {
                        var existingReview = loadContext.Reviews
                            .Include(r => r.Client)
                            .Include(r => r.Trainer)
                            .AsNoTracking()
                            .FirstOrDefault(r => r.Reviewid == _currentReview.Reviewid);

                        if (existingReview != null)
                        {
                            _currentReview = existingReview;
                        }
                    }
                }

                if (_currentReview.Client != null)
                {
                    var clients = ClientComboBox.ItemsSource as System.Collections.IList;
                    if (clients != null)
                    {
                        ClientComboBox.SelectedItem = clients.OfType<Client>()
                            .FirstOrDefault(c => c.Clientid == _currentReview.Clientid);
                    }
                }

                if (_currentReview.Trainer != null)
                {
                    var trainers = TrainerComboBox.ItemsSource as System.Collections.IList;
                    if (trainers != null)
                    {
                        TrainerComboBox.SelectedItem = trainers.OfType<Trainer>()
                            .FirstOrDefault(t => t.Trainerid == _currentReview.Trainerid);
                    }
                }

                RatingUpDown.Value = _currentReview.Rating;
                CommentTextBox.Text = _currentReview.Comment;

                if (_currentReview.Reviewdate.ToDateTime(TimeOnly.MinValue) != default)
                {
                    ReviewDatePicker.SelectedDate = _currentReview.Reviewdate.ToDateTime(TimeOnly.MinValue);
                }
            }
            catch (Exception ex)
            {
                var dialog = new ConfirmationDialog($"Ошибка загрузки данных отзыва: {ex.Message}", true);
                _ = dialog.ShowDialog<bool>(this);
            }
        }

        private async void SaveButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                using (var saveContext = new AppDbContext())
                {
                    var userRole = MainWindow.CurrentUser?.Role?.Rolename;
                    Review reviewToSave;

                    if (_isNew)
                    {
                        reviewToSave = new Review();
                    }
                    else
                    {
                        reviewToSave = await saveContext.Reviews
                            .FirstOrDefaultAsync(r => r.Reviewid == _currentReview.Reviewid);

                        if (reviewToSave == null)
                        {
                            var dialog = new ConfirmationDialog("Отзыв не найден в базе данных.", true);
                            await dialog.ShowDialog<bool>(this);
                            return;
                        }
                    }

                    if (userRole == "Пользователь")
                    {
                        var clientProfile = await saveContext.Clients
                            .FirstOrDefaultAsync(c => c.Userid == MainWindow.CurrentUser.Userid);
                        if (clientProfile == null)
                        {
                            var dialog = new ConfirmationDialog("Профиль клиента не найден.", true);
                            await dialog.ShowDialog<bool>(this);
                            return;
                        }
                        reviewToSave.Clientid = clientProfile.Clientid;
                    }
                    else if (ClientComboBox.SelectedItem is Client selectedClient)
                    {
                        reviewToSave.Clientid = selectedClient.Clientid;
                    }
                    else
                    {
                        var dialog = new ConfirmationDialog("Необходимо выбрать клиента.", true);
                        await dialog.ShowDialog<bool>(this);
                        return;
                    }

                    var selectedTrainer = TrainerComboBox.SelectedItem as Trainer;
                    reviewToSave.Trainerid = selectedTrainer?.Trainerid;

                    reviewToSave.Rating = (int)(RatingUpDown.Value ?? 5);
                    reviewToSave.Comment = CommentTextBox.Text;
                    reviewToSave.Reviewdate = DateOnly.FromDateTime(ReviewDatePicker.SelectedDate?.DateTime ?? DateTime.Now);

                    if (_isNew)
                    {
                        saveContext.Reviews.Add(reviewToSave);
                    }

                    await saveContext.SaveChangesAsync();
                    Close(true);
                }
            }
            catch (Exception ex)
            {
                var dialog = new ConfirmationDialog($"Ошибка сохранения: {ex.Message}", true);
                await dialog.ShowDialog<bool>(this);
            }
        }

        private void CancelButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close(false);
        }
    }
}