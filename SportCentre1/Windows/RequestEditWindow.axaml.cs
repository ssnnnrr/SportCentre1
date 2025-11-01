using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using SportCentre1.Data;
using System;

namespace SportCentre1.Windows
{
    public partial class RequestEditWindow : Window
    {
        public RequestEditWindow()
        {
            InitializeComponent();
        }

        private async void SendButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SubjectTextBox.Text) || string.IsNullOrWhiteSpace(MessageTextBox.Text))
            {
                var dialog = new ConfirmationDialog("Тема и текст сообщения не могут быть пустыми.", true);
                await dialog.ShowDialog<bool>(this);
                return;
            }

            var clientProfile = await MainWindow.dbContext.Clients.FirstOrDefaultAsync(c => c.Userid == MainWindow.CurrentUser!.Userid);
            if (clientProfile == null) return;

            var newRequest = new Request
            {
                Clientid = clientProfile.Clientid,
                Subject = SubjectTextBox.Text,
                Status = "Открыт",
                Creationdate = DateTime.Now
            };
            MainWindow.dbContext.Requests.Add(newRequest);
            await MainWindow.dbContext.SaveChangesAsync();

            var newMessage = new Message
            {
                Requestid = newRequest.Requestid,
                Senderuserid = MainWindow.CurrentUser.Userid,
                Messagetext = MessageTextBox.Text,
                Sentdate = DateTime.Now
            };
            MainWindow.dbContext.Messages.Add(newMessage);
            await MainWindow.dbContext.SaveChangesAsync();

            Close(true);
        }
    }
}