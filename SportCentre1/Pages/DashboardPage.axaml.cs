using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;
using Microsoft.EntityFrameworkCore;
using SportCentre1.Data;
using SportCentre1.Windows;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportCentre1.Pages
{
    public partial class DashboardPage : UserControl
    {
        private Client? _currentClient;

        public DashboardPage()
        {
            InitializeComponent();
            DateDatePicker.SelectedDate = DateTime.Today;
            _ = LoadClientDataAsync();
        }

        private async Task LoadClientDataAsync()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    _currentClient = await db.Clients
                        .Include(c => c.Clientprogresses)
                        .FirstOrDefaultAsync(c => c.Userid == MainWindow.CurrentUser!.Userid);

                    if (_currentClient is null) { /* ... */ return; }

                    WelcomeTextBlock.Text = $"Добро пожаловать, {_currentClient.Firstname}! 👋";

                    var today = DateOnly.FromDateTime(DateTime.Now);
                    var activeMembership = await db.Clientmemberships.Include(cm => cm.Membershiptype)
                        .Where(cm => cm.Clientid == _currentClient.Clientid && cm.Startdate <= today && cm.Enddate >= today)
                        .FirstOrDefaultAsync();

                    if (activeMembership is not null) { /* ... */ }
                    else { MembershipStatusTextBlock.Text = "У вас нет активного абонемента."; }

                    // Заполняем поля из _currentClient
                    HeightUpDown.Value = _currentClient.Height;
                    TargetWeightUpDown.Value = _currentClient.Targetweight;
                    TargetBodyFatUpDown.Value = _currentClient.Targetbodyfatpercentage;

                    ProgressDataGrid.ItemsSource = _currentClient.Clientprogresses.OrderByDescending(cp => cp.Date).Take(10).ToList();
                    UpdateAnalytics();
                }
            }
            catch (Exception ex) { /* ... */ }
        }

        // КНОПКА СОХРАНЯЕТ ТОЛЬКО ЦЕЛИ
        private async void SaveGoalsButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_currentClient is null) return;
            try
            {
                using (var db = new AppDbContext())
                {
                    db.Clients.Attach(_currentClient);
                    _currentClient.Targetweight = TargetWeightUpDown.Value;
                    _currentClient.Targetbodyfatpercentage = TargetBodyFatUpDown.Value;
                    await db.SaveChangesAsync();
                }
                UpdateAnalytics();
                var successDialog = new ConfirmationDialog("Ваши цели успешно сохранены!", true);
                await successDialog.ShowDialog<bool>(this.GetVisualRoot() as Window);
            }
            catch (Exception ex) { await ShowErrorDialog($"Ошибка сохранения целей: {ex.Message}"); }
        }

        // КНОПКА СОХРАНЯЕТ ПРОГРЕСС И РОСТ + ПРОВЕРКА НА ДАТУ
        private async void AddProgressButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_currentClient is null) return;
            if (!DateDatePicker.SelectedDate.HasValue)
            {
                await ShowErrorDialog("Пожалуйста, выберите дату.");
                return;
            }
            var selectedDate = DateOnly.FromDateTime(DateDatePicker.SelectedDate.Value.DateTime);

            try
            {
                using (var db = new AppDbContext())
                {
                    // ПРОВЕРКА: существует ли уже запись за эту дату
                    bool recordExists = await db.Clientprogresses
                        .AnyAsync(p => p.Clientid == _currentClient.Clientid && p.Date == selectedDate);
                    if (recordExists)
                    {
                        await ShowErrorDialog("Вы уже добавили запись о прогрессе за эту дату. Вы можете добавить только одну запись в день.");
                        return;
                    }

                    // Обновляем рост клиента, если он был изменен
                    db.Clients.Attach(_currentClient);
                    _currentClient.Height = (int?)HeightUpDown.Value;

                    var newProgress = new Clientprogress
                    {
                        Clientid = _currentClient.Clientid,
                        Date = selectedDate,
                        Weight = WeightUpDown.Value,
                        Notes = NotesTextBox.Text
                    };
                    db.Clientprogresses.Add(newProgress);
                    await db.SaveChangesAsync();
                }
                await LoadClientDataAsync();
            }
            catch (Exception ex) { await ShowErrorDialog($"Ошибка сохранения прогресса: {ex.Message}"); }
        }


        private void UpdateAnalytics()
        {
            if (_currentClient?.Height is null || _currentClient.Height <= 0)
            {
                BmiValueTextBlock.Text = "-";
                BmiInterpretationTextBlock.Text = "Введите ваш рост для расчета ИМТ.";
                BmiInterpretationTextBlock.Foreground = Brushes.Gray;
                RecommendationTextBlock.Text = "Введите ваши данные и цели, чтобы получить персональные рекомендации.";
                return;
            }

            var lastProgress = _currentClient.Clientprogresses.OrderByDescending(p => p.Date).FirstOrDefault();
            if (lastProgress?.Weight is null || lastProgress.Weight <= 0)
            {
                BmiValueTextBlock.Text = "-";
                BmiInterpretationTextBlock.Text = "Добавьте запись о своем весе для расчета ИМТ.";
                BmiInterpretationTextBlock.Foreground = Brushes.Gray;
                RecommendationTextBlock.Text = "Добавьте данные о весе и укажите цели, чтобы получить персональные рекомендации.";
                return;
            }

            decimal heightInMeters = (decimal)_currentClient.Height / 100;
            decimal bmi = Math.Round((decimal)lastProgress.Weight / (heightInMeters * heightInMeters), 1);
            BmiValueTextBlock.Text = bmi.ToString();

            if (bmi < 18.5m) { BmiInterpretationTextBlock.Text = "Недостаточный вес"; BmiInterpretationTextBlock.Foreground = Brushes.Blue; }
            else if (bmi < 25) { BmiInterpretationTextBlock.Text = "Нормальный вес"; BmiInterpretationTextBlock.Foreground = Brushes.Green; }
            else if (bmi < 30) { BmiInterpretationTextBlock.Text = "Избыточный вес"; BmiInterpretationTextBlock.Foreground = Brushes.Orange; }
            else { BmiInterpretationTextBlock.Text = "Ожирение"; BmiInterpretationTextBlock.Foreground = Brushes.Red; }

            var recommendations = new StringBuilder();
            var currentWeight = (decimal)lastProgress.Weight;

            if (_currentClient.Targetweight.HasValue && _currentClient.Targetweight > 0)
            {
                decimal targetWeight = _currentClient.Targetweight.Value;
                if (currentWeight > targetWeight) { recommendations.AppendLine("🎯 Ваша цель - снижение веса."); recommendations.AppendLine("• Рекомендуемые тренировки: Сочетайте кардио (беговая дорожка, эллипс) и силовые тренировки. Обратите внимание на групповые занятия 'Cycling' и 'Zumba' в нашем расписании."); recommendations.AppendLine("• Питание: Увеличьте потребление белка и клетчатки (овощи), сократите быстрые углеводы и сахар."); }
                else if (currentWeight < targetWeight) { recommendations.AppendLine("🎯 Ваша цель - набор мышечной массы."); recommendations.AppendLine("• Рекомендуемые тренировки: Сконцентрируйтесь на силовых тренировках с прогрессирующей нагрузкой. Персональные занятия с тренером помогут составить эффективную программу."); recommendations.AppendLine("• Питание: Обеспечьте профицит калорий за счет сложных углеводов (крупы, макароны) и достаточного количества белка (1.5-2г на кг веса)."); }
                else { recommendations.AppendLine("🎯 Ваша цель - поддержание формы."); recommendations.AppendLine("• Рекомендуемые тренировки: Подойдут функциональные тренировки, йога, пилатес и игровые виды спорта для поддержания тонуса и гибкости."); recommendations.AppendLine("• Питание: Придерживайтесь сбалансированного рациона, чтобы обеспечить организм всеми необходимыми нутриентами."); }
            }
            else { recommendations.AppendLine("Укажите ваш целевой вес, чтобы мы могли подобрать для вас персональные рекомендации по тренировкам и питанию."); }

            RecommendationTextBlock.Text = recommendations.ToString();
        }

        private async Task ShowErrorDialog(string message)
        {
            var dialog = new ConfirmationDialog(message, true);
            await dialog.ShowDialog<bool>(this.GetVisualRoot() as Window);
        }
    }
}