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

        private List<SnakePart> snakeParts = new List<SnakePart>();

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

        private void InitializeSnake()
        {
            snakeParts.Clear();
            for (int i = 0; i < snakeLength; i++)
            {
                snakeParts.Add(new SnakePart
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
            DrawSnake();

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

            for (int i = 0; i < snakeParts.Count - 1; i++)
                snakeParts[i].Position = snakeParts[i + 1].Position;

            snakeParts[^1].Position = new Point(head.Position.X + dX, head.Position.Y + dY);
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
            bool doneDrawingBackground = false;
            int nextX = 0;
            int nextY = 0;
            int rowCounter = 0;
            bool nextIsOdd = false;

            while (doneDrawingBackground == false)
            {
                Rectangle rect = new Rectangle
                {
                    Width = SnakeSquareSize,
                    Height = SnakeSquareSize,
                    Fill = nextIsOdd ? Brushes.White : Brushes.Black
                };

                GameArea.Children.Add(rect);
                Canvas.SetTop(rect, nextY);
                Canvas.SetLeft(rect, nextX);

                nextIsOdd = !nextIsOdd;

                nextX += SnakeSquareSize;
                if (nextX >= GameArea.ActualWidth)
                {
                    nextX = 0;
                    nextY += SnakeSquareSize;
                    rowCounter++;
                    nextIsOdd = rowCounter % 2 == 0;
                }
                if (nextY >= GameArea.ActualHeight)
                {
                    doneDrawingBackground = true;
                }
            }
        }
    }
}
