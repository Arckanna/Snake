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
    /// Fenêtre principale : gère la navigation entre les écrans via ShellViewModel.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ShellViewModel _viewModel;

        public MainWindow(IGameEngine engine, ITimerService timerService, IScoreService scoreService)
        {
            InitializeComponent();
            
            // Créer les ViewModels nécessaires
            var welcomeViewModel = new WelcomeViewModel(scoreService);
            var gameViewModel = new GameViewModel(engine, timerService, scoreService);
            
            // Créer le ShellViewModel qui pilote l'affichage
            DataContext = _viewModel = new ShellViewModel(welcomeViewModel, gameViewModel);
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            Focus();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _viewModel.Game.Stop();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Gérer les touches uniquement si on est en jeu
            if (_viewModel.CurrentScreen != ScreenKind.Game)
                return;

            if (e.Key == Key.P)
            {
                _viewModel.Game.TogglePause();
                return;
            }

            var d = KeyToDirection(e.Key);
            if (d.HasValue)
                _viewModel.Game.SetDirection(d.Value);
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
