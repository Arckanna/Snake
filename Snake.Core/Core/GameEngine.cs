using System.Diagnostics;
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

            // Vérification des collisions avec les murs
            // Note: Les positions sont en pixels, mais on doit vérifier qu'elles restent dans les limites
            // La dernière position valide est (areaWidth - squareSize) pour X et (areaHeight - squareSize) pour Y
            // Donc on vérifie si newHeadX >= areaWidth (pas >) car si areaWidth=700 et squareSize=20, la dernière case est à 680
            // Si le serpent est à 680 et va à droite, newHeadX = 700, ce qui est >= 700, donc collision ✓
            if (newHeadX < 0 || newHeadX >= _areaWidth || newHeadY < 0 || newHeadY >= _areaHeight)
            {
                Debug.WriteLine($"GameEngine.Move: COLLISION MUR détectée! newHeadX={newHeadX}, newHeadY={newHeadY}, areaWidth={_areaWidth}, areaHeight={_areaHeight}");
                Debug.WriteLine($"GameEngine.Move: Tête actuelle: ({head.X}, {head.Y}), direction: {_direction}, dX={dX}, dY={dY}");
                Debug.WriteLine($"GameEngine.Move: Dernière position valide X: {_areaWidth - _squareSize}, Y: {_areaHeight - _squareSize}");
                _state = GameState.GameOver;
                return;
            }

            // Vérification de collision avec la nourriture AVANT de vérifier les collisions avec le corps
            // Utiliser une comparaison exacte car les positions sont toujours des multiples de squareSize
            bool ateFood = false;
            if (_foodPosition is { } fp && fp.X == newHeadX && fp.Y == newHeadY)
            {
                _score++;
                ateFood = true;
            }

            // Vérification de collision avec le corps
            // IMPORTANT: On vérifie seulement les segments qui ne seront PAS déplacés
            // - Si on ne mange pas de pomme: on vérifie les segments 0 à Count-2 (la queue à l'index Count-1 sera déplacée)
            // - Si on mange une pomme: on vérifie les segments 0 à Count-1 (tous les segments actuels, car on ajoute un nouveau segment)
            int segmentsToCheck = ateFood ? _snake.Count : _snake.Count - 1;
            for (int i = 0; i < segmentsToCheck; i++)
            {
                // Comparaison exacte - les positions sont toujours des multiples entiers de squareSize
                if (_snake[i].X == newHeadX && _snake[i].Y == newHeadY)
                {
                    Debug.WriteLine($"GameEngine.Move: COLLISION CORPS détectée! Segment[{i}] à ({_snake[i].X}, {_snake[i].Y}), nouvelle tête à ({newHeadX}, {newHeadY}), segmentsToCheck={segmentsToCheck}, snake.Count={_snake.Count}, ateFood={ateFood}");
                    Debug.WriteLine($"GameEngine.Move: Positions du serpent:");
                    for (int j = 0; j < _snake.Count; j++)
                    {
                        Debug.WriteLine($"  Segment[{j}]: ({_snake[j].X}, {_snake[j].Y})");
                    }
                    _state = GameState.GameOver;
                    return;
                }
            }

            // Si on a mangé une pomme, ajouter un nouveau segment
            if (ateFood)
            {
                _snake.Add(new Snake.Models.SnakePart { X = newHeadX, Y = newHeadY });
                SpawnFood();
                return;
            }

            // Déplacer le serpent (décaler tous les segments sauf la tête)
            for (int i = 0; i < _snake.Count - 1; i++)
            {
                _snake[i].X = _snake[i + 1].X;
                _snake[i].Y = _snake[i + 1].Y;
            }
            // Mettre à jour la position de la tête
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

            // Calculer toutes les positions libres
            var freePositions = new List<(double X, double Y)>();
            for (int col = 0; col < maxCol; col++)
            {
                for (int row = 0; row < maxRow; row++)
                {
                    double x = col * _squareSize;
                    double y = row * _squareSize;
                    if (!IsPositionOnSnake(x, y))
                    {
                        freePositions.Add((x, y));
                    }
                }
            }

            // Si aucune position libre, ne pas placer de nourriture
            if (freePositions.Count == 0)
            {
                _foodPosition = null;
                return;
            }

            // Choisir une position aléatoire parmi les positions libres
            int index = Random.Shared.Next(0, freePositions.Count);
            _foodPosition = freePositions[index];
        }

        private bool IsPositionOnSnake(double x, double y)
        {
            // Comparaison exacte - les positions sont toujours des multiples entiers de squareSize
            foreach (var part in _snake)
            {
                if (part.X == x && part.Y == y)
                    return true;
            }
            return false;
        }
    }
}
