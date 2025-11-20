using Avalonia.Controls;
using SportCentre1.Data;
using SportCentre1.Windows;
using System;
using System.Collections.Generic;

namespace SportCentre1.Windows
{
    public partial class ChallengeEditWindow : Window
    {
        private Challenge _currentChallenge;
        private bool _isNew;

        public ChallengeEditWindow()
        {
            InitializeComponent();
            _isNew = true;
            _currentChallenge = new Challenge();
            SetupControls();
            StartDatePicker.SelectedDate = DateTime.Today;
            EndDatePicker.SelectedDate = DateTime.Today.AddDays(30);
        }

        public ChallengeEditWindow(Challenge challengeToEdit)
        {
            InitializeComponent();
            _isNew = false;
            _currentChallenge = challengeToEdit;
            SetupControls();
            LoadChallengeData();
        }

        private void SetupControls()
        {
            // В будущем можно будет добавить больше типов, например, 'Manual'
            TypeComboBox.ItemsSource = new List<string> { "Attendance" };
            TypeComboBox.SelectedIndex = 0;
        }

        private void LoadChallengeData()
        {
            // Используем имена свойств из вашего Challenge.cs
            TitleTextBox.Text = _currentChallenge.Title;
            DescriptionTextBox.Text = _currentChallenge.Description;
            RewardTextBox.Text = _currentChallenge.Reward;
            StartDatePicker.SelectedDate = _currentChallenge.Startdate;
            EndDatePicker.SelectedDate = _currentChallenge.Enddate;
            TypeComboBox.SelectedItem = _currentChallenge.Challengetype;
        }

        private async void SaveButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text) || TypeComboBox.SelectedItem == null)
            {
                var dialog = new ConfirmationDialog("Название и Тип являются обязательными полями.", true);
                await dialog.ShowDialog<bool>(this);
                return;
            }
            if (!StartDatePicker.SelectedDate.HasValue || !EndDatePicker.SelectedDate.HasValue || EndDatePicker.SelectedDate.Value < StartDatePicker.SelectedDate.Value)
            {
                var dialog = new ConfirmationDialog("Даты проведения челленджа указаны некорректно.", true);
                await dialog.ShowDialog<bool>(this);
                return;
            }

            _currentChallenge.Title = TitleTextBox.Text;
            _currentChallenge.Description = DescriptionTextBox.Text;
            _currentChallenge.Reward = RewardTextBox.Text;
            // Убеждаемся, что сохраняем DateTime, а не DateTimeOffset
            _currentChallenge.Startdate = StartDatePicker.SelectedDate.Value.DateTime;
            _currentChallenge.Enddate = EndDatePicker.SelectedDate.Value.DateTime;
            _currentChallenge.Challengetype = TypeComboBox.SelectedItem as string ?? "Attendance";

            try
            {
                using (var dbContext = new AppDbContext())
                {
                    if (_isNew)
                    {
                        dbContext.Challenges.Add(_currentChallenge);
                    }
                    else
                    {
                        // Обновляем существующую запись
                        dbContext.Challenges.Update(_currentChallenge);
                    }
                    await dbContext.SaveChangesAsync();
                }
                Close(true); // Закрываем окно с результатом "успех"
            }
            catch (Exception ex)
            {
                var dialog = new ConfirmationDialog($"Ошибка сохранения челленджа: {ex.Message}", true);
                await dialog.ShowDialog<bool>(this);
            }
        }
    }
}