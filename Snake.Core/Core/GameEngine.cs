using Snake.Models;

namespace Snake.Core
{
    /// <summary>
    /// Implémentation du moteur de jeu Snake : déplacement, collisions, nourriture.
    /// Aucune dépendance WPF — uniquement Snake.Models.
    /// </summary>
    public class GameEngine : IGameEngine
    {
        private readonly List<Snake.Models.SnakePart> _snake = new();
        private double _areaWidth;
        private double _areaHeight;
        private double _squareSize;
        private Direction _direction;
        private GameState _state;
        private int _score;
        private (double X, double Y)? _foodPosition;

        public GameState State => _state;
        public IReadOnlyList<SnakePart> SnakeParts => _snake;
        public (double X, double Y)? FoodPosition => _foodPosition;
        public int Score => _score;
        public double SquareSize => _squareSize;

        public void Initialize(double areaWidth, double areaHeight, double squareSize = 20, int initialSnakeLength = 10)
        {
            _areaWidth = areaWidth;
            _areaHeight = areaHeight;
            _squareSize = squareSize;
            _direction = Direction.Right;
            _state = GameState.Playing;
            _score = 0;
            _foodPosition = null;
            _snake.Clear();

            int maxRow = (int)(areaHeight / squareSize);
            double startY = (maxRow / 2) * squareSize;

            for (int i = 0; i < initialSnakeLength; i++)
            {
                _snake.Add(new Snake.Models.SnakePart
                {
                    X = i * squareSize,
                    Y = startY
                });
            }

            SpawnFood();
        }

        public void SetState(GameState state)
        {
            _state = state;
        }

        public void Move(Direction requestedDirection)
        {
            if (_state == GameState.GameOver || _state == GameState.Paused)
                return;

            bool forbidden = (_direction == Direction.Right && requestedDirection == Direction.Left)
                || (_direction == Direction.Left && requestedDirection == Direction.Right)
                || (_direction == Direction.Up && requestedDirection == Direction.Down)
                || (_direction == Direction.Down && requestedDirection == Direction.Up);
            if (!forbidden)
                _direction = requestedDirection;

            var head = _snake[^1];
            double dX = _direction switch
            {
                Direction.Right => _squareSize,
                Direction.Left => -_squareSize,
                _ => 0
            };
            double dY = _direction switch
            {
                Direction.Down => _squareSize,
                Direction.Up => -_squareSize,
                _ => 0
            };

            double newHeadX = head.X + dX;
            double newHeadY = head.Y + dY;

            if (newHeadX < 0 || newHeadX >= _areaWidth || newHeadY < 0 || newHeadY >= _areaHeight)
            {
                _state = GameState.GameOver;
                return;
            }

            for (int i = 0; i < _snake.Count - 1; i++)
            {
                if (_snake[i].X == newHeadX && _snake[i].Y == newHeadY)
                {
                    _state = GameState.GameOver;
                    return;
                }
            }

            if (_foodPosition is { } fp && fp.X == newHeadX && fp.Y == newHeadY)
            {
                _score++;
                _snake.Add(new Snake.Models.SnakePart { X = newHeadX, Y = newHeadY });
                SpawnFood();
                return;
            }

            for (int i = 0; i < _snake.Count - 1; i++)
            {
                _snake[i].X = _snake[i + 1].X;
                _snake[i].Y = _snake[i + 1].Y;
            }
            _snake[^1].X = newHeadX;
            _snake[^1].Y = newHeadY;
        }

        private void SpawnFood()
        {
            int maxCol = Math.Max(0, (int)(_areaWidth / _squareSize));
            int maxRow = Math.Max(0, (int)(_areaHeight / _squareSize));

            if (maxCol <= 0 || maxRow <= 0)
            {
                _foodPosition = null;
                return;
            }

            for (int attempt = 0; attempt < 100; attempt++)
            {
                int col = Random.Shared.Next(0, maxCol);
                int row = Random.Shared.Next(0, maxRow);
                double x = col * _squareSize;
                double y = row * _squareSize;

                if (IsPositionOnSnake(x, y))
                    continue;

                _foodPosition = (x, y);
                return;
            }

            _foodPosition = null;
        }

        private bool IsPositionOnSnake(double x, double y)
        {
            foreach (var part in _snake)
            {
                if (part.X == x && part.Y == y)
                    return true;
            }
            return false;
        }
    }
}
