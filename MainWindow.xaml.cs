using System;
using System.Windows;
using System.Windows.Input;
using Snake.Core;
using Snake.Models;
using Snake.Services;
using Snake.ViewModels;

namespace Snake
{
    /// <summary>
    /// Fenêtre principale : gère la navigation entre les écrans via MainViewModel.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainViewModel MainViewModel => _viewModel;

        public MainWindow(IGameEngine engine, ITimerService timerService, IScoreService scoreService)
        {
            InitializeComponent();

            // Créer le MainViewModel qui pilote l'affichage et la navigation
            DataContext = _viewModel = new MainViewModel(engine, timerService, scoreService);
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            Focus();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _viewModel.GameViewModel?.Stop();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Gérer les touches uniquement si on est en jeu
            if (_viewModel.CurrentState != AppState.Playing && _viewModel.CurrentState != AppState.GameOver)
                return;

            var gameViewModel = _viewModel.GameViewModel;
            if (gameViewModel == null)
                return;

            if (e.Key == Key.P)
            {
                gameViewModel.TogglePause();
                return;
            }

            var d = KeyToDirection(e.Key);
            if (d.HasValue)
                gameViewModel.SetDirection(d.Value);
        }

        private static Direction? KeyToDirection(Key key)
        {
            return key switch
            {
                Key.Up or Key.Z => Direction.Up,
                Key.Down or Key.S => Direction.Down,
                Key.Left or Key.A or Key.Q => Direction.Left,
                Key.Right or Key.D => Direction.Right,
                _ => null
            };
        }
    }
}
