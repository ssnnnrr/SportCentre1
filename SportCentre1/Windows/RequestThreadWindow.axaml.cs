using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Microsoft.EntityFrameworkCore;
using SportCentre1.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SportCentre1.Windows
{
    public class MessageViewModel
    {
        public Message OriginalMessage { get; set; }
        public HorizontalAlignment HorizontalAlignment { get; set; }
        public Color BackgroundColor { get; set; }
        public User Senderuser => OriginalMessage.Senderuser;
        public string Messagetext => OriginalMessage.Messagetext;
        public DateTime Sentdate => OriginalMessage.Sentdate;
    }

    public partial class RequestThreadWindow : Window
    {
        private Request _currentRequest;

        public RequestThreadWindow() { InitializeComponent(); }

        public RequestThreadWindow(Request request)
        {
            InitializeComponent();
            _currentRequest = request;
            SubjectTitle.Text = _currentRequest.Subject;
            _ = LoadMessages();
        }

        private async Task LoadMessages()
        {
            try
            {
                using (var dbContext = new AppDbContext())
                {
                    var messages = await dbContext.Messages
                        .Include(m => m.Senderuser)
                        .Where(m => m.Requestid == _currentRequest.Requestid)
                        .OrderBy(m => m.Sentdate)
                        .ToListAsync();

                    var messageViewModels = messages.Select(m => new MessageViewModel
                    {
                        OriginalMessage = m,
                        HorizontalAlignment = m.Senderuserid == MainWindow.CurrentUser.Userid ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                        BackgroundColor = m.Senderuserid == MainWindow.CurrentUser.Userid ? Color.FromRgb(123, 104, 238) : Color.FromRgb(230, 224, 255)
                    }).ToList();

                    MessagesItemsControl.ItemsSource = messageViewModels;

                    await Task.Delay(100);
                    MessagesScrollViewer.ScrollToEnd();
                }
            }
            catch (Exception ex)
            {
                var dialog = new ConfirmationDialog($"Ошибка загрузки сообщений: {ex.Message}", true);
                await dialog.ShowDialog<bool>(this);
            }
        }

        private async void ReplyButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ReplyTextBox.Text)) return;

            try
            {
                using (var dbContext = new AppDbContext())
                {
                    var currentRequest = await dbContext.Requests
                        .FirstOrDefaultAsync(r => r.Requestid == _currentRequest.Requestid);

                    if (currentRequest == null)
                    {
                        var errorDialog = new ConfirmationDialog("Запрос не найден.", true);
                        await errorDialog.ShowDialog<bool>(this);
                        return;
                    }

                    var newMessage = new Message
                    {
                        Requestid = currentRequest.Requestid,
                        Senderuserid = MainWindow.CurrentUser.Userid,
                        Messagetext = ReplyTextBox.Text,
                        Sentdate = DateTime.Now
                    };
                    dbContext.Messages.Add(newMessage);

                    var userRole = MainWindow.CurrentUser.Role.Rolename;
                    if (userRole == "Менеджер" || userRole == "Администратор")
                    {
                        currentRequest.Status = "Отвечен";
                    }
                    else
                    {
                        currentRequest.Status = "Открыт";
                    }

                    await dbContext.SaveChangesAsync();

                    ReplyTextBox.Text = "";
                    await LoadMessages();
                }
            }
            catch (Exception ex)
            {
                var dialog = new ConfirmationDialog($"Ошибка отправки ответа: {ex.Message}", true);
                await dialog.ShowDialog<bool>(this);
            }
        }
    }
}