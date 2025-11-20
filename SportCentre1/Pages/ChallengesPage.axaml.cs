using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Microsoft.EntityFrameworkCore;
using SportCentre1.Data;
using SportCentre1.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SportCentre1.Pages
{
    // Вспомогательная ViewModel для удобного отображения данных в UI
    public class ChallengeViewModel
    {
        public int ChallengeId { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public string Dates { get; set; } = "";
        public string? Reward { get; set; }
        public string UserProgress { get; set; } = "0";
        public bool IsJoined { get; set; }
        public bool CanJoin => !IsJoined;
    }

    public partial class ChallengesPage : UserControl
    {
        private Client? _currentClient;

        public ChallengesPage()
        {
            InitializeComponent();
            _ = LoadChallengesAsync();
        }

        private async Task LoadChallengesAsync()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    _currentClient = await db.Clients
                        .FirstOrDefaultAsync(c => c.Userid == MainWindow.CurrentUser!.Userid);
                    if (_currentClient is null) return;

                    var now = DateTime.Now;

                    var activeChallenges = await db.Challenges
                        .Where(ch => ch.Startdate <= now && ch.Enddate >= now)
                        .OrderBy(ch => ch.Enddate)
                        .ToListAsync();

                    var clientParticipations = await db.ClientChallenges
                        .Where(cc => cc.Clientid == _currentClient.Clientid &&
                                     activeChallenges.Select(ac => ac.Challengeid).Contains(cc.Challengeid))
                        .ToDictionaryAsync(cc => cc.Challengeid);

                    var viewModels = activeChallenges.Select(ch =>
                    {
                        var participation = clientParticipations.GetValueOrDefault(ch.Challengeid);
                        return new ChallengeViewModel
                        {
                            ChallengeId = ch.Challengeid,
                            Title = ch.Title,
                            Description = ch.Description,
                            Dates = $"{ch.Startdate:dd.MM} - {ch.Enddate:dd.MM.yyyy}",
                            Reward = ch.Reward,
                            IsJoined = participation != null,
                            UserProgress = participation?.Progress.ToString() ?? "Не участвуете"
                        };
                    }).ToList();

                    ChallengesListBox.ItemsSource = viewModels;
                }
            }
            catch (Exception ex)
            {
                var dialog = new ConfirmationDialog($"Ошибка загрузки челленджей: {ex.Message}", true);
                await dialog.ShowDialog<bool>(this.GetVisualRoot() as Window);
            }
        }

        private async void JoinButton_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is not Button { CommandParameter: int challengeId }) return;
            if (_currentClient is null) return;

            var dialog = new ConfirmationDialog("Вы уверены, что хотите присоединиться к этому челленджу?");
            var result = await dialog.ShowDialog<bool>(this.GetVisualRoot() as Window);
            if (result != true) return;

            try
            {
                using (var db = new AppDbContext())
                {
                    bool alreadyExists = await db.ClientChallenges
                        .AnyAsync(cc => cc.Clientid == _currentClient.Clientid && cc.Challengeid == challengeId);

                    if (alreadyExists)
                    {
                        await new ConfirmationDialog("Вы уже участвуете в этом челлендже.", true).ShowDialog<bool>(this.GetVisualRoot() as Window);
                        return;
                    }

                    var newParticipation = new ClientChallenge
                    {
                        Clientid = _currentClient.Clientid,
                        Challengeid = challengeId,
                    };

                    db.ClientChallenges.Add(newParticipation);
                    await db.SaveChangesAsync();

                    await LoadChallengesAsync();
                }
            }
            catch (Exception ex)
            {
                await new ConfirmationDialog($"Ошибка при присоединении: {ex.Message}", true).ShowDialog<bool>(this.GetVisualRoot() as Window);
            }
        }

        // ===============================================================
        // СТАРЫЙ МЕТОД ChallengesListBox_SelectionChanged УДАЛЕН
        // ===============================================================


        // ===============================================================
        // НОВЫЙ МЕТОД ДЛЯ ОБРАБОТКИ НАЖАТИЯ КНОПКИ
        // ===============================================================
        private async void LeaderboardButton_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button { CommandParameter: int challengeId })
            {
                var leaderboardWindow = new LeaderboardWindow(challengeId);
                await leaderboardWindow.ShowDialog(this.GetVisualRoot() as Window);
            }
        }
    }
}