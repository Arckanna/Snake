using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Snake.ViewModels;

namespace Snake
{
    /// <summary>
    /// Écran d'accueil : binding sur WelcomeViewModel, commande Démarrer.
    /// </summary>
    public partial class WelcomeWindow : Window
    {
        private readonly WelcomeViewModel _viewModel;
        private readonly IServiceProvider _serviceProvider;

        public WelcomeWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            _serviceProvider = serviceProvider;
            _viewModel = new WelcomeViewModel();
            _viewModel.StartGameRequested += OnStartGameRequested;
            DataContext = _viewModel;
        }

        private void OnStartGameRequested(object? sender, Snake.Models.Difficulty difficulty)
        {
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.SetDifficulty(difficulty);
            mainWindow.Show();
            Hide();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _viewModel.StartGameRequested -= OnStartGameRequested;
        }
    }
}
