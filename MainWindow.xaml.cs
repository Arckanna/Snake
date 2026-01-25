using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Snake.Core;
using Snake.Models;
using Snake.ViewModels;

namespace Snake
{
    /// <summary>
    /// Vue du jeu : binding sur GameViewModel, dessin du serpent et du fruit sur FrameUpdated.
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int AreaWidth = 700;
        private const int AreaHeight = 400;

        private readonly GameViewModel _viewModel;
        private static readonly SolidColorBrush SnakeBodyBrush = Brushes.Green;
        private static readonly SolidColorBrush SnakeHeadBrush = Brushes.DarkGreen;
        private static readonly SolidColorBrush FoodBrush = Brushes.Red;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _viewModel = new GameViewModel(new GameEngine());
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            DrawGameArea();
            _viewModel.FrameUpdated += OnFrameUpdated;
            _viewModel.Start(AreaWidth, AreaHeight);
            Focus();
        }

        private void OnFrameUpdated(object? sender, EventArgs e)
        {
            DrawFrame();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            var d = KeyToDirection(e.Key);
            if (d.HasValue)
                _viewModel.SetDirection(d.Value);
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
        }

        private void DrawFrame()
        {
            // Garder le fond (index 0), supprimer le reste
            while (GameArea.Children.Count > 1)
                GameArea.Children.RemoveAt(1);

            var size = _viewModel.SquareSize;
            var parts = _viewModel.SnakeParts;
            var count = parts.Count;

            for (int i = 0; i < count; i++)
            {
                var part = parts[i];
                bool isHead = (i == count - 1);
                var r = new Rectangle
                {
                    Width = size,
                    Height = size,
                    Fill = isHead ? SnakeHeadBrush : SnakeBodyBrush
                };
                GameArea.Children.Add(r);
                Canvas.SetLeft(r, part.X);
                Canvas.SetTop(r, part.Y);
            }

            if (_viewModel.FoodPosition is { } fp)
            {
                var food = new Rectangle
                {
                    Width = size,
                    Height = size,
                    Fill = FoodBrush
                };
                GameArea.Children.Add(food);
                Canvas.SetLeft(food, fp.X);
                Canvas.SetTop(food, fp.Y);
            }
        }
    }
}
