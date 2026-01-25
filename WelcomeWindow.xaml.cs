using System.Windows;
using Snake.ViewModels;

namespace Snake
{
    /// <summary>
    /// Écran d'accueil : binding sur WelcomeViewModel, commande Démarrer.
    /// </summary>
    public partial class WelcomeWindow : Window
    {
        public WelcomeWindow()
        {
            InitializeComponent();

            var vm = new WelcomeViewModel();
            vm.StartGameRequested += (_, _) =>
            {
                new MainWindow().Show();
                Close();
            };
            DataContext = vm;
        }
    }
}
