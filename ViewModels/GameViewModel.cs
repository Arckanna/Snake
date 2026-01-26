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
        private Direction _pendingDirection = Direction.Right;

        public GameViewModel(IGameEngine engine, ITimerService timerService)
        {
            _engine = engine;
            _timerService = timerService;
        }

        /// <summary>Titre de la fenêtre (score, Game Over).</summary>
        public string Title => _engine.State == GameState.GameOver
            ? $"Snake - Score: {_engine.Score} - Game Over"
            : $"Snake - Score: {_engine.Score}";

        /// <summary>Score actuel.</summary>
        public int Score => _engine.Score;

        /// <summary>État de la partie.</summary>
        public GameState State => _engine.State;

        /// <summary>Segments du serpent (tête en dernier).</summary>
        public IReadOnlyList<Snake.Models.SnakePart> SnakeParts => _engine.SnakeParts;

        /// <summary>Position du fruit.</summary>
        public (double X, double Y)? FoodPosition => _engine.FoodPosition;

        /// <summary>Taille d'une case en pixels.</summary>
        public double SquareSize => _engine.SquareSize;

        /// <summary>Indique si la partie est terminée (pour afficher l'overlay).</summary>
        public bool IsGameOver => _engine.State == GameState.GameOver;

        /// <summary>Déclenché à chaque frame pour que la vue redessine.</summary>
        public event EventHandler? FrameUpdated;

        /// <summary>Déclenché lorsque l'utilisateur choisit de retourner à l'écran d'accueil.</summary>
        public event EventHandler? ReturnToWelcomeRequested;

        private int _tickIntervalMs = GameConfig.TickIntervalMs;

        /// <summary>Démarre une nouvelle partie (dimensions et paramètres depuis GameConfig).</summary>
        /// <param name="tickIntervalMs">Intervalle du timer en millisecondes. Si non spécifié, utilise la valeur par défaut de GameConfig.</param>
        public void Start(int? tickIntervalMs = null)
        {
            _tickIntervalMs = tickIntervalMs ?? GameConfig.TickIntervalMs;
            
            _engine.Initialize(
                GameConfig.AreaWidth,
                GameConfig.AreaHeight,
                GameConfig.SquareSize,
                GameConfig.InitialSnakeLength);
            _pendingDirection = Direction.Right;

            _timerService.Start(TimeSpan.FromMilliseconds(_tickIntervalMs), OnTickCallback);

            NotifyAll();
            FrameUpdated?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void Rejouer()
        {
            _engine.Initialize(
                GameConfig.AreaWidth,
                GameConfig.AreaHeight,
                GameConfig.SquareSize,
                GameConfig.InitialSnakeLength);
            _pendingDirection = Direction.Right;

            _timerService.Start(TimeSpan.FromMilliseconds(_tickIntervalMs), OnTickCallback);

            NotifyAll();
            FrameUpdated?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void RetourAccueil()
        {
            _timerService.Stop();
            ReturnToWelcomeRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Enregistre la direction demandée par l'utilisateur (demi-tours gérés par le moteur).</summary>
        public void SetDirection(Direction direction)
        {
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
                _timerService.Stop();
        }

        private void NotifyAll()
        {
            OnPropertyChanged(nameof(Score));
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(State));
            OnPropertyChanged(nameof(IsGameOver));
            OnPropertyChanged(nameof(SnakeParts));
            OnPropertyChanged(nameof(FoodPosition));
        }
    }
}
