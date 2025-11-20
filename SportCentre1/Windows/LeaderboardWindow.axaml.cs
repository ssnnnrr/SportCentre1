using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using SportCentre1.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SportCentre1.Windows
{
    // Вспомогательный класс для отображения в таблице
    public class LeaderboardEntry
    {
        public int Rank { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public int Progress { get; set; }
    }

    public partial class LeaderboardWindow : Window
    {
        private readonly int _challengeId;

        // Конструктор для дизайнера
        public LeaderboardWindow()
        {
            InitializeComponent();
        }

        // Рабочий конструктор
        public LeaderboardWindow(int challengeId)
        {
            InitializeComponent();
            _challengeId = challengeId;
            _ = LoadLeaderboardAsync();
        }

        private async Task LoadLeaderboardAsync()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var challenge = await db.Challenges.FindAsync(_challengeId);
                    if (challenge == null)
                    {
                        ChallengeTitleTextBlock.Text = "Челлендж не найден";
                        return;
                    }

                    ChallengeTitleTextBlock.Text = challenge.Title;

                    var participants = await db.ClientChallenges
                        .Where(cc => cc.Challengeid == _challengeId)
                        .Include(cc => cc.Client)
                        .OrderByDescending(cc => cc.Progress)
                        .ToListAsync();

                    ParticipantsCountTextBlock.Text = $"Количество участников: {participants.Count}";

                    var leaderboardEntries = participants
                        .Select((p, index) => new LeaderboardEntry
                        {
                            Rank = index + 1,
                            ClientName = $"{p.Client.Firstname} {p.Client.Lastname}",
                            Progress = p.Progress
                        })
                        .ToList();

                    LeaderboardDataGrid.ItemsSource = leaderboardEntries;
                }
            }
            catch (Exception ex)
            {
                var dialog = new ConfirmationDialog($"Ошибка загрузки таблицы лидеров: {ex.Message}", true);
                await dialog.ShowDialog<bool>(this);
            }
        }
    }
}