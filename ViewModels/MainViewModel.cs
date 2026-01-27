using CommunityToolkit.Mvvm.ComponentModel;
using Snake.Core;
using Snake.Models;
using Snake.Services;

namespace Snake.ViewModels
{
    /// <summary>
    /// ViewModel principal qui gère l'état de l'application et la navigation entre les écrans.
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        private readonly IGameEngine _engine;
        private readonly ITimerService _timerService;
        private readonly IScoreService _scoreService;
        private AppState _currentState = AppState.Home;
        private GameViewModel? _gameViewModel;
        private WelcomeViewModel? _welcomeViewModel;
        private Difficulty? _pendingDifficulty;

        public MainViewModel(IGameEngine engine, ITimerService timerService, IScoreService scoreService)
        {
            _engine = engine;
            _timerService = timerService;
            _scoreService = scoreService;
        }

        /// <summary>État actuel de l'application (écran affiché).</summary>
        public AppState CurrentState
        {
            get => _currentState;
            private set
            {
                SetProperty(ref _currentState, value);
                OnPropertyChanged(nameof(Title));
            }
        }

        /// <summary>Titre de la fenêtre selon l'état actuel.</summary>
        public string Title => CurrentState switch
        {
            AppState.Playing => _gameViewModel?.Title ?? "Serpentium",
            AppState.GameOver => _gameViewModel?.Title ?? "Serpentium - Game Over",
            _ => "Serpentium"
        };

        /// <summary>ViewModel pour l'écran d'accueil.</summary>
        public WelcomeViewModel WelcomeViewModel
        {
            get
            {
                if (_welcomeViewModel == null)
                {
                    _welcomeViewModel = new WelcomeViewModel(_scoreService);
                    _welcomeViewModel.StartGameRequested += OnStartGameRequested;
                }
                return _welcomeViewModel;
            }
        }

        /// <summary>ViewModel pour le jeu.</summary>
        public GameViewModel? GameViewModel => _gameViewModel;

        /// <summary>Change l'état vers l'écran d'accueil.</summary>
        public void NavigateToHome()
        {
            if (_gameViewModel != null)
            {
                _gameViewModel.Stop();
                _gameViewModel.ReturnToWelcomeRequested -= OnReturnToWelcomeRequested;
                _gameViewModel.GameOverRequested -= OnGameOverRequested;
                _gameViewModel.GameRestarted -= OnGameRestarted;
            }
            CurrentState = AppState.Home;
            WelcomeViewModel.RefreshBestScore();
        }

        /// <summary>Change l'état vers l'écran de jeu.</summary>
        public void NavigateToGame(Difficulty difficulty)
        {
            try
            {
                if (_gameViewModel == null)
                {
                    _gameViewModel = new GameViewModel(_engine, _timerService, _scoreService);
                    _gameViewModel.ReturnToWelcomeRequested += OnReturnToWelcomeRequested;
                    _gameViewModel.GameOverRequested += OnGameOverRequested;
                    _gameViewModel.GameRestarted += OnGameRestarted;
                }
                // Stocker la difficulté pour démarrer le jeu une fois que GameView est prêt
                _pendingDifficulty = difficulty;
                CurrentState = AppState.Playing;
                // Le jeu sera démarré par GameView quand il sera chargé
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur dans NavigateToGame: {ex.Message}\n{ex.StackTrace}");
                System.Windows.MessageBox.Show($"Erreur lors de la navigation vers le jeu: {ex.Message}", "Erreur", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>Démarre le jeu si une difficulté est en attente. Appelé par GameView quand il est prêt.</summary>
        public void StartGameIfPending()
        {
            if (_pendingDifficulty.HasValue && _gameViewModel != null)
            {
                try
                {
                    _gameViewModel.Start((int)_pendingDifficulty.Value);
                    _pendingDifficulty = null;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erreur lors du démarrage du jeu: {ex.Message}\n{ex.StackTrace}");
                    System.Windows.MessageBox.Show($"Erreur lors du démarrage du jeu: {ex.Message}", "Erreur", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        /// <summary>Change l'état vers l'écran Game Over.</summary>
        public void NavigateToGameOver()
        {
            CurrentState = AppState.GameOver;
        }

        private void OnStartGameRequested(object? sender, Difficulty difficulty)
        {
            NavigateToGame(difficulty);
        }

        private void OnReturnToWelcomeRequested(object? sender, EventArgs e)
        {
            NavigateToHome();
        }

        private void OnGameOverRequested(object? sender, EventArgs e)
        {
            NavigateToGameOver();
        }

        private void OnGameRestarted(object? sender, EventArgs e)
        {
            CurrentState = AppState.Playing;
        }
    }
}
