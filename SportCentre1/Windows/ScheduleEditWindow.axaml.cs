using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using SportCentre1.Data;
using SportCentre1.Windows;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SportCentre1.Windows
{
    public partial class ScheduleEditWindow : Window
    {
        private Schedule _currentSchedule;
        private bool _isNew;

        public ScheduleEditWindow()
        {
            InitializeComponent();
            _isNew = true;
            _currentSchedule = new Schedule();
            LoadComboBoxes();
            StartDatePicker.SelectedDate = DateTime.Today;
            EndDatePicker.SelectedDate = DateTime.Today;
        }

        public ScheduleEditWindow(Schedule scheduleToEdit)
        {
            InitializeComponent();
            _isNew = false;
            _currentSchedule = scheduleToEdit;
            LoadComboBoxes();
            LoadScheduleData();
        }

        private async void LoadComboBoxes()
        {
            using (var dbContext = new AppDbContext())
            {
                WorkoutTypeComboBox.ItemsSource = await dbContext.Workouttypes.ToListAsync();
                TrainerComboBox.ItemsSource = await dbContext.Trainers.ToListAsync();
            }
        }

        private void LoadScheduleData()
        {
            WorkoutTypeComboBox.SelectedItem = (WorkoutTypeComboBox.ItemsSource as System.Collections.IEnumerable)?
                .OfType<Workouttype>().FirstOrDefault(w => w.Workouttypeid == _currentSchedule.Workouttypeid);

            TrainerComboBox.SelectedItem = (TrainerComboBox.ItemsSource as System.Collections.IEnumerable)?
                .OfType<Trainer>().FirstOrDefault(t => t.Trainerid == _currentSchedule.Trainerid);

            StartDatePicker.SelectedDate = _currentSchedule.Starttime.Date;
            StartTimePicker.SelectedTime = _currentSchedule.Starttime.TimeOfDay;

            EndDatePicker.SelectedDate = _currentSchedule.Endtime.Date;
            EndTimePicker.SelectedTime = _currentSchedule.Endtime.TimeOfDay;

            MaxCapacityUpDown.Value = _currentSchedule.Maxcapacity;
        }

        private async void SaveButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // --- Шаг 1: Валидация введенных данных (без изменений) ---
            if (!StartDatePicker.SelectedDate.HasValue || !StartTimePicker.SelectedTime.HasValue ||
                !EndDatePicker.SelectedDate.HasValue || !EndTimePicker.SelectedTime.HasValue)
            {
                var dialog = new ConfirmationDialog("Необходимо указать полную дату и время.", true);
                await dialog.ShowDialog<bool>(this); return;
            }

            var startTime = StartDatePicker.SelectedDate.Value.Date + StartTimePicker.SelectedTime.Value;
            var endTime = EndDatePicker.SelectedDate.Value.Date + EndTimePicker.SelectedTime.Value;

            if (WorkoutTypeComboBox.SelectedItem == null || TrainerComboBox.SelectedItem == null)
            {
                var dialog = new ConfirmationDialog("Нужно выбрать тип тренировки и тренера.", true);
                await dialog.ShowDialog<bool>(this); return;
            }
            if (endTime <= startTime)
            {
                var dialog = new ConfirmationDialog("Время окончания должно быть позже времени начала.", true);
                await dialog.ShowDialog<bool>(this); return;
            }
            if (_isNew && startTime < DateTime.Now)
            {
                var dialog = new ConfirmationDialog("Нельзя создавать занятие в прошлом.", true);
                await dialog.ShowDialog<bool>(this); return;
            }
            if ((endTime - startTime).TotalHours > 3)
            {
                var dialog = new ConfirmationDialog("Тренировка не может длиться более 3 часов.", true);
                await dialog.ShowDialog<bool>(this); return;
            }

            // --- Шаг 2: Присваиваем данные объекту ---
            var selectedTrainer = (TrainerComboBox.SelectedItem as Trainer)!;
            _currentSchedule.Workouttypeid = (WorkoutTypeComboBox.SelectedItem as Workouttype)!.Workouttypeid;
            _currentSchedule.Trainerid = selectedTrainer.Trainerid;
            _currentSchedule.Starttime = startTime;
            _currentSchedule.Endtime = endTime;
            _currentSchedule.Maxcapacity = (int)(MaxCapacityUpDown.Value ?? 1);


            using (var dbContext = new AppDbContext())
            {
                // ======================================================================
                // НОВЫЙ БЛОК: ПРОВЕРКА НА ПЕРЕСЕЧЕНИЕ ЗАНЯТИЙ У ТРЕНЕРА
                // ======================================================================

                // Создаем базовый запрос для поиска пересечений
                IQueryable<Schedule> overlapQuery = dbContext.Schedules
                    .Where(s =>
                        s.Trainerid == selectedTrainer.Trainerid && // У того же тренера
                        s.Starttime < endTime &&                  // Которое начинается до окончания нашего нового занятия
                        s.Endtime > startTime);                    // И заканчивается после начала нашего нового занятия

                // Если мы редактируем существующее занятие,
                // его нужно исключить из проверки, иначе оно найдет пересечение само с собой.
                if (!_isNew)
                {
                    overlapQuery = overlapQuery.Where(s => s.Scheduleid != _currentSchedule.Scheduleid);
                }

                // Выполняем запрос
                bool isOverlapping = await overlapQuery.AnyAsync();

                if (isOverlapping)
                {
                    // Если найдено хотя бы одно пересечение, показываем ошибку и прекращаем сохранение
                    var dialog = new ConfirmationDialog(
                        $"Ошибка! Тренер {selectedTrainer.Firstname} {selectedTrainer.Lastname} уже занят в указанное время. Пожалуйста, выберите другое время или другого тренера.",
                        true);
                    await dialog.ShowDialog<bool>(this);
                    return; // ВАЖНО: выходим из метода, не сохраняя данные
                }

                // ======================================================================
                // КОНЕЦ НОВОГО БЛОКА
                // ======================================================================


                // --- Шаг 3: Сохраняем данные, если проверка пройдена ---
                if (_isNew)
                {
                    dbContext.Schedules.Add(_currentSchedule);
                }
                else
                {
                    dbContext.Schedules.Update(_currentSchedule);
                }
                await dbContext.SaveChangesAsync();
            }
            Close(true);
        }
    }
}