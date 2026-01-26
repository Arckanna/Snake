using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using Snake.Core;
using Snake.Models;
using Snake.Services;
using Snake.ViewModels;

namespace Snake
{
    /// <summary>
    /// Vue du jeu : binding sur GameViewModel, dessin du serpent et du fruit sur FrameUpdated.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly GameViewModel _viewModel;
        private Difficulty _difficulty = Difficulty.Normal;
        // Couleurs modernes pour le serpent
        private static readonly SolidColorBrush SnakeBodyBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6BA644")!);
        private static readonly SolidColorBrush SnakeHeadBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4A7C2A")!);
        private static readonly SolidColorBrush SnakeHeadAccentBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D5016")!);
        // Couleur moderne pour la nourriture
        private static readonly SolidColorBrush FoodBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E74C3C")!);
        private static readonly SolidColorBrush FoodAccentBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C0392B")!);

        public MainWindow(IGameEngine engine, ITimerService timerService, IScoreService scoreService)
        {
            InitializeComponent();
            DataContext = _viewModel = new GameViewModel(engine, timerService, scoreService);
        }

        /// <summary>Définit la difficulté du jeu avant le démarrage.</summary>
        public void SetDifficulty(Difficulty difficulty)
        {
            _difficulty = difficulty;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            DrawGameArea();
            _viewModel.FrameUpdated += OnFrameUpdated;
            _viewModel.ReturnToWelcomeRequested += OnReturnToWelcomeRequested;
            _viewModel.Start((int)_difficulty);
            Focus();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _viewModel.Stop();
            _viewModel.FrameUpdated -= OnFrameUpdated;
            _viewModel.ReturnToWelcomeRequested -= OnReturnToWelcomeRequested;
        }

        private void OnReturnToWelcomeRequested(object? sender, EventArgs e)
        {
            var app = (App)Application.Current;
            
            // Synchroniser la position avec la fenêtre de jeu avant de fermer
            if (app._welcomeWindow != null)
            {
                app._welcomeWindow.Left = Left;
                app._welcomeWindow.Top = Top;
            }
            
            app.ShowWelcomeWindow();
            Close();
        }

        private void OnFrameUpdated(object? sender, EventArgs e)
        {
            DrawFrame();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.P)
            {
                _viewModel.TogglePause();
                return;
            }

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
            // Fond avec dégradé subtil
            var rect = new Rectangle
            {
                Width = GameArea.ActualWidth,
                Height = GameArea.ActualHeight
            };
            var gradientBrush = new LinearGradientBrush
            {
                StartPoint = new System.Windows.Point(0, 0),
                EndPoint = new System.Windows.Point(1, 1)
            };
            gradientBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#F5F2EB")!, 0));
            gradientBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#E8E5DD")!, 1));
            rect.Fill = gradientBrush;
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
                
                // Créer un rectangle avec coins arrondis
                var r = new Rectangle
                {
                    Width = size,
                    Height = size,
                    Fill = isHead ? SnakeHeadBrush : SnakeBodyBrush,
                    RadiusX = size * 0.2, // Coins arrondis (20% de la taille)
                    RadiusY = size * 0.2
                };
                
                // Ajouter une ombre pour la profondeur
                r.Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    Direction = 270,
                    ShadowDepth = 2,
                    BlurRadius = 3,
                    Opacity = 0.3
                };
                
                // Pour la tête, ajouter un accent
                if (isHead)
                {
                    // Ajouter un petit cercle pour les yeux (optionnel)
                    var eyeSize = size * 0.15;
                    var eyeOffset = size * 0.25;
                    var leftEye = new Ellipse
                    {
                        Width = eyeSize,
                        Height = eyeSize,
                        Fill = Brushes.White,
                        Opacity = 0.8
                    };
                    var rightEye = new Ellipse
                    {
                        Width = eyeSize,
                        Height = eyeSize,
                        Fill = Brushes.White,
                        Opacity = 0.8
                    };
                    GameArea.Children.Add(leftEye);
                    GameArea.Children.Add(rightEye);
                    Canvas.SetLeft(leftEye, part.X + eyeOffset);
                    Canvas.SetTop(leftEye, part.Y + eyeOffset);
                    Canvas.SetLeft(rightEye, part.X + size - eyeOffset - eyeSize);
                    Canvas.SetTop(rightEye, part.Y + eyeOffset);
                }
                
                GameArea.Children.Add(r);
                Canvas.SetLeft(r, part.X);
                Canvas.SetTop(r, part.Y);
            }

            if (_viewModel.FoodPosition is { } fp)
            {
                // Nourriture avec coins arrondis et effet visuel
                var food = new Ellipse
                {
                    Width = size * 0.85, // Légèrement plus petit pour un effet visuel
                    Height = size * 0.85,
                    Fill = FoodBrush
                };
                
                // Ajouter une ombre plus prononcée pour la nourriture
                food.Effect = new DropShadowEffect
                {
                    Color = Colors.DarkRed,
                    Direction = 270,
                    ShadowDepth = 3,
                    BlurRadius = 5,
                    Opacity = 0.5
                };
                
                // Ajouter un accent intérieur (cercle plus petit)
                var foodAccent = new Ellipse
                {
                    Width = size * 0.4,
                    Height = size * 0.4,
                    Fill = FoodAccentBrush,
                    Opacity = 0.6
                };
                
                GameArea.Children.Add(food);
                GameArea.Children.Add(foodAccent);
                Canvas.SetLeft(food, fp.X + size * 0.075);
                Canvas.SetTop(food, fp.Y + size * 0.075);
                Canvas.SetLeft(foodAccent, fp.X + size * 0.3);
                Canvas.SetTop(foodAccent, fp.Y + size * 0.3);
            }
        }
    }
}
