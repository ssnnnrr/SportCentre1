using Avalonia.Controls;
using SportCentre1.Pages;
using SportCentre1.Data;

namespace SportCentre1
{
    public partial class MainWindow : Window
    {
       
        public static AppDbContext dbContext { get; set; } = new AppDbContext();
        public static User? CurrentUser { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            this.Content = new WelcomePage();
        }

        public void ShowMainPage()
        {
            this.Content = new MainPage();
        }

        public void Logout()
        {
            CurrentUser = null;
            this.Content = new WelcomePage();
        }
    }
}