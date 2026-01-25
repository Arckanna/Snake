using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Snake
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        const string _gameTitle = "Snake";
        const int SnakeSquareSize = 20;

        private SolidColorBrush snakeBodyBrush = Brushes.Green;
        private SolidColorBrush snakeHeadBrush = Brushes.DarkGreen; 
        private SolidColorBrush foodBrush = Brushes.Red;

        private List<SnakePartView> snakeParts = new List<SnakePartView>();

        public enum SnakeDirection
        {
            Left,
            Right,
            Up,
            Down
        };
        private SnakeDirection snakeDirection = SnakeDirection.Right;
        private int snakeLength = 10;

        private DispatcherTimer? gameTimer;

        private int _score;
        private Point? _foodPosition;
        private Rectangle? _foodUiElement;
        private int _backgroundCount;

        private void InitializeSnake()
        {
            snakeParts.Clear();
            for (int i = 0; i < snakeLength; i++)
            {
                snakeParts.Add(new SnakePartView
                {
                    Position = new Point(i * SnakeSquareSize, 200)
                });
            }
        }

        private void DrawSnake()
        {
            for (int i = 0; i < snakeParts.Count; i++)
            {
                var snakePart = snakeParts[i];
                bool isHead = (i == snakeParts.Count - 1);

                if (snakePart.UiElement == null)
                {
                    snakePart.UiElement = new Rectangle
                    {
                        Width = SnakeSquareSize,
                        Height = SnakeSquareSize
                    };
                    GameArea.Children.Add(snakePart.UiElement);
                }

                ((Rectangle)snakePart.UiElement!).Fill = isHead ? snakeHeadBrush : snakeBodyBrush;
                Canvas.SetTop(snakePart.UiElement, snakePart.Position.Y);
                Canvas.SetLeft(snakePart.UiElement, snakePart.Position.X);
            }
        }

        private void Window_ContentRendered(object sender, System.EventArgs e)
        {
            DrawGameArea();
            InitializeSnake();
            SpawnFood();
            DrawSnake();
            UpdateTitle();

            gameTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            gameTimer.Tick += GameTick;
            gameTimer.Start();
        }

        private void GameTick(object? sender, EventArgs e) => MoveSnake();

        private void MoveSnake()
        {
            var head = snakeParts[^1];
            double dX = snakeDirection switch
            {
                SnakeDirection.Right => SnakeSquareSize,
                SnakeDirection.Left => -SnakeSquareSize,
                _ => 0
            };
            double dY = snakeDirection switch
            {
                SnakeDirection.Down => SnakeSquareSize,
                SnakeDirection.Up => -SnakeSquareSize,
                _ => 0
            };

            double newHeadX = head.Position.X + dX;
            double newHeadY = head.Position.Y + dY;

            if (newHeadX < 0 || newHeadX >= GameArea.ActualWidth
                || newHeadY < 0 || newHeadY >= GameArea.ActualHeight)
            {
                gameTimer!.Stop();
                Title = _gameTitle + " - Score: " + _score + " - Game Over";
                MessageBox.Show("Le serpent a heurté un mur !", "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            for (int i = 0; i < snakeParts.Count - 1; i++)
            {
                if (snakeParts[i].Position.X == newHeadX && snakeParts[i].Position.Y == newHeadY)
                {
                    gameTimer!.Stop();
                    Title = _gameTitle + " - Score: " + _score + " - Game Over";
                    MessageBox.Show("Le serpent a heurté son propre corps !", "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }

            if (_foodPosition is Point fp && fp.X == newHeadX && fp.Y == newHeadY)
            {
                GameArea.Children.Remove(_foodUiElement!);
                _foodUiElement = null;
                _score++;
                UpdateTitle();
                SpawnFood();
                snakeParts.Add(new SnakePartView { Position = new Point(newHeadX, newHeadY) });
                DrawSnake();
                return;
            }

            for (int i = 0; i < snakeParts.Count - 1; i++)
                snakeParts[i].Position = snakeParts[i + 1].Position;

            snakeParts[^1].Position = new Point(newHeadX, newHeadY);
            DrawSnake();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            SnakeDirection? newDir = e.Key switch
            {
                Key.Up or Key.Z => SnakeDirection.Up,
                Key.Down or Key.S => SnakeDirection.Down,
                Key.Left or Key.A or Key.Q => SnakeDirection.Left,
                Key.Right or Key.D => SnakeDirection.Right,
                _ => null
            };

            if (newDir is null) return;

            bool forbidden = (snakeDirection == SnakeDirection.Right && newDir == SnakeDirection.Left)
                || (snakeDirection == SnakeDirection.Left && newDir == SnakeDirection.Right)
                || (snakeDirection == SnakeDirection.Up && newDir == SnakeDirection.Down)
                || (snakeDirection == SnakeDirection.Down && newDir == SnakeDirection.Up);

            if (!forbidden)
                snakeDirection = newDir.Value;
        }
        private void DrawGameArea()
        {
            var creamBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5F2EB")!);
            var rect = new Rectangle
            {
                Width = GameArea.ActualWidth,
                Height = GameArea.ActualHeight,
                Fill = creamBrush
            };
            GameArea.Children.Add(rect);
            Canvas.SetLeft(rect, 0);
            Canvas.SetTop(rect, 0);
            _backgroundCount = 1;
        }

        private void SpawnFood()
        {
            if (_foodUiElement != null)
            {
                GameArea.Children.Remove(_foodUiElement);
                _foodUiElement = null;
            }

            int maxCol = (int)(GameArea.ActualWidth / SnakeSquareSize);
            int maxRow = (int)(GameArea.ActualHeight / SnakeSquareSize);

            for (int attempt = 0; attempt < 100; attempt++)
            {
                int col = Random.Shared.Next(0, maxCol);
                int row = Random.Shared.Next(0, maxRow);
                var pos = new Point(col * SnakeSquareSize, row * SnakeSquareSize);
                if (IsPositionOnSnake(pos)) continue;

                _foodPosition = pos;
                _foodUiElement = new Rectangle
                {
                    Width = SnakeSquareSize,
                    Height = SnakeSquareSize,
                    Fill = foodBrush
                };
                GameArea.Children.Insert(_backgroundCount, _foodUiElement);
                Canvas.SetLeft(_foodUiElement, pos.X);
                Canvas.SetTop(_foodUiElement, pos.Y);
                return;
            }
            _foodPosition = null;
        }

        private bool IsPositionOnSnake(Point p)
        {
            foreach (var part in snakeParts)
                if (part.Position.X == p.X && part.Position.Y == p.Y) return true;
            return false;
        }

        private void UpdateTitle() => Title = _gameTitle + " - Score: " + _score;
    }
}
