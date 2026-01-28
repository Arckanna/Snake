using System;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Snake.Core;
using Snake.Models;
using Snake.Services;

namespace Snake.ViewModels
{
    /// <summary>
    /// ViewModel du jeu Snake : orchestre IGameEngine, ITimerService, et expose les données pour la vue.
    /// </summary>
    public partial class GameViewModel : ObservableObject
    {
        private readonly IGameEngine _engine;
        private readonly ITimerService _timerService;
        private readonly IScoreService _scoreService;
        private Direction _pendingDirection = Direction.Right;
        private int _bestScore;
        private double _areaWidth = GameConfig.AreaWidth;
        private double _areaHeight = GameConfig.AreaHeight;
        private bool _dimensionsInitialized = false;

        public GameViewModel(IGameEngine engine, ITimerService timerService, IScoreService scoreService)
        {
            _engine = engine;
            _timerService = timerService;
            _scoreService = scoreService;
            _bestScore = _scoreService.GetBestScore();
        }

        /// <summary>Titre de la fenêtre (score, Game Over, Pause).</summary>
        public string Title => _engine.State switch
        {
            GameState.GameOver => $"Serpentium - Score: {_engine.Score} - Game Over",
            GameState.Paused => $"Serpentium - Score: {_engine.Score} - Pause",
            _ => $"Serpentium - Score: {_engine.Score}"
        };

        /// <summary>Score actuel.</summary>
        public int Score => _engine.Score;

        /// <summary>Meilleur score sauvegardé.</summary>
        public int BestScore => _bestScore;

        /// <summary>État de la partie.</summary>
        public GameState State => _engine.State;

        /// <summary>Segments du serpent (tête en dernier).</summary>
        public IReadOnlyList<Snake.Models.SnakePart> SnakeParts => _engine.SnakeParts;

        /// <summary>Position du fruit.</summary>
        public (double X, double Y)? FoodPosition => _engine.FoodPosition;

        /// <summary>Taille d'une case en pixels.</summary>
        public double SquareSize => _engine.SquareSize;

        /// <summary>Largeur de la zone de jeu (pixels).</summary>
        public double AreaWidth
        {
            get => _areaWidth;
            set => SetProperty(ref _areaWidth, value);
        }

        /// <summary>Hauteur de la zone de jeu (pixels).</summary>
        public double AreaHeight
        {
            get => _areaHeight;
            set => SetProperty(ref _areaHeight, value);
        }

        /// <summary>Initialise le moteur avec les dimensions réelles du canvas.</summary>
        public void InitializeWithDimensions(double width, double height)
        {
            if (width > 0 && height > 0)
            {
                AreaWidth = width;
                AreaHeight = height;
                _dimensionsInitialized = true;
            }
        }

        /// <summary>Indique si les dimensions réelles du canvas ont été initialisées.</summary>
        public bool AreDimensionsInitialized => _dimensionsInitialized;

        /// <summary>Indique si la partie est terminée (pour afficher l'overlay).</summary>
        public bool IsGameOver => _engine.State == GameState.GameOver;

        /// <summary>Indique si le jeu est en pause.</summary>
        public bool IsPaused => _engine.State == GameState.Paused;

        /// <summary>Déclenché à chaque frame pour que la vue redessine.</summary>
        public event EventHandler? FrameUpdated;

        /// <summary>Déclenché lorsque l'utilisateur choisit de retourner à l'écran d'accueil.</summary>
        public event EventHandler? ReturnToWelcomeRequested;

        /// <summary>Déclenché lorsque le jeu est terminé (Game Over).</summary>
        public event EventHandler? GameOverRequested;

        /// <summary>Déclenché lorsque le jeu redémarre (nouvelle partie).</summary>
        public event EventHandler? GameRestarted;

        private int _tickIntervalMs = GameConfig.TickIntervalMs;

        /// <summary>Démarre une nouvelle partie (dimensions et paramètres depuis GameConfig).</summary>
        /// <param name="tickIntervalMs">Intervalle du timer en millisecondes. Si non spécifié, utilise la valeur par défaut de GameConfig.</param>
        public void Start(int? tickIntervalMs = null)
        {
            try
            {
                // Vérifier que les dimensions réelles ont été initialisées
                if (!_dimensionsInitialized)
                    return;

                _tickIntervalMs = tickIntervalMs ?? GameConfig.TickIntervalMs;

                _engine.Initialize(
                    _areaWidth,
                    _areaHeight,
                    GameConfig.SquareSize,
                    GameConfig.InitialSnakeLength);
                _pendingDirection = Direction.Right;

                _timerService.Start(TimeSpan.FromMilliseconds(_tickIntervalMs), OnTickCallback);

                NotifyAll();
                FrameUpdated?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur dans GameViewModel.Start: {ex.Message}\n{ex.StackTrace}");
                throw; // Re-lancer pour que l'appelant puisse gérer
            }
        }

        [RelayCommand]
        private void Rejouer()
        {
            _engine.Initialize(
                _areaWidth,
                _areaHeight,
                GameConfig.SquareSize,
                GameConfig.InitialSnakeLength);
            _pendingDirection = Direction.Right;

            _timerService.Start(TimeSpan.FromMilliseconds(_tickIntervalMs), OnTickCallback);

            NotifyAll();
            FrameUpdated?.Invoke(this, EventArgs.Empty);
            GameRestarted?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void RetourAccueil()
        {
            _timerService.Stop();
            ReturnToWelcomeRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Met le jeu en pause ou le reprend.</summary>
        public void TogglePause()
        {
            if (_engine.State == GameState.Playing)
            {
                _engine.SetState(GameState.Paused);
                _timerService.Stop();
                OnPropertyChanged(nameof(IsPaused));
                OnPropertyChanged(nameof(Title));
            }
            else if (_engine.State == GameState.Paused)
            {
                _engine.SetState(GameState.Playing);
                _timerService.Start(TimeSpan.FromMilliseconds(_tickIntervalMs), OnTickCallback);
                OnPropertyChanged(nameof(IsPaused));
                OnPropertyChanged(nameof(Title));
            }
        }

        [RelayCommand]
        private void Reprendre()
        {
            if (_engine.State == GameState.Paused)
            {
                TogglePause();
            }
        }

        /// <summary>Enregistre la direction demandée par l'utilisateur (demi-tours gérés par le moteur).</summary>
        public void SetDirection(Direction direction)
        {
            if (_engine.State == GameState.Playing)
                _pendingDirection = direction;
        }

        /// <summary>Arrête le timer. À appeler à la fermeture de la vue.</summary>
        public void Stop()
        {
            _timerService.Stop();
        }

        private void OnTickCallback()
        {
            _engine.Move(_pendingDirection);

            NotifyAll();
            FrameUpdated?.Invoke(this, EventArgs.Empty);

            if (_engine.State == GameState.GameOver)
            {
                _timerService.Stop();
                _scoreService.SaveScore(_engine.Score);
                _bestScore = _scoreService.GetBestScore();
                OnPropertyChanged(nameof(BestScore));
                GameOverRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        private void NotifyAll()
        {
            OnPropertyChanged(nameof(Score));
            OnPropertyChanged(nameof(BestScore));
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(State));
            OnPropertyChanged(nameof(IsGameOver));
            OnPropertyChanged(nameof(IsPaused));
            OnPropertyChanged(nameof(SnakeParts));
            OnPropertyChanged(nameof(FoodPosition));
        }
    }
}
