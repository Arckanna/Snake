using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Snake.Core;
using Snake.Models;

namespace Snake.ViewModels
{
    /// <summary>
    /// ViewModel du jeu Snake : orchestre IGameEngine, timer, et expose les données pour la vue.
    /// </summary>
    public partial class GameViewModel : ObservableObject
    {
        private readonly IGameEngine _engine;
        private DispatcherTimer? _timer;
        private Direction _pendingDirection = Direction.Right;
        private double _areaWidth;
        private double _areaHeight;

        public GameViewModel(IGameEngine engine)
        {
            _engine = engine;
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

        /// <summary>
        /// Démarre une nouvelle partie.
        /// </summary>
        /// <param name="areaWidth">Largeur de la zone de jeu (pixels).</param>
        /// <param name="areaHeight">Hauteur de la zone de jeu (pixels).</param>
        public void Start(double areaWidth, double areaHeight)
        {
            _areaWidth = areaWidth;
            _areaHeight = areaHeight;
            _engine.Initialize(areaWidth, areaHeight);
            _pendingDirection = Direction.Right;

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _timer.Tick += OnTick;
            _timer.Start();

            NotifyAll();
            FrameUpdated?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void Rejouer()
        {
            _engine.Initialize(_areaWidth, _areaHeight);
            _pendingDirection = Direction.Right;

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _timer.Tick += OnTick;
            _timer.Start();

            NotifyAll();
            FrameUpdated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Enregistre la direction demandée par l'utilisateur (demi-tours gérés par le moteur).</summary>
        public void SetDirection(Direction direction)
        {
            _pendingDirection = direction;
        }

        private void OnTick(object? sender, EventArgs e)
        {
            _engine.Move(_pendingDirection);

            NotifyAll();
            FrameUpdated?.Invoke(this, EventArgs.Empty);

            if (_engine.State == GameState.GameOver)
            {
                _timer!.Stop();
                _timer.Tick -= OnTick;
            }
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
