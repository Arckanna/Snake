using System.Windows;

namespace Snake
{
    public partial class WelcomeWindow : Window
    {
        public WelcomeWindow()
        {
            InitializeComponent();
        }

        private void BtnDemarrer_Click(object sender, RoutedEventArgs e)
        {
            var game = new MainWindow();
            game.Show();
            Close();
        }
    }
}
