using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using Snake.ViewModels;

namespace Snake.Views
{
    /// <summary>
    /// Vue du jeu : affichage du serpent et du fruit, gestion de la pause.
    /// </summary>
    public partial class GameView : UserControl
    {
        private GameViewModel? _viewModel;
        // Couleurs modernes pour le serpent
        private static readonly SolidColorBrush SnakeBodyBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6BA644")!);
        private static readonly SolidColorBrush SnakeHeadBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4A7C2A")!);
        // Couleur moderne pour la nourriture
        private static readonly SolidColorBrush FoodBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E74C3C")!);
        private static readonly SolidColorBrush FoodAccentBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C0392B")!);

        public GameView()
        {
            InitializeComponent();
            Loaded += GameView_Loaded;
            DataContextChanged += GameView_DataContextChanged;
        }

        private void GameView_Loaded(object sender, RoutedEventArgs e)
        {
            SubscribeToViewModel();
            DrawGameArea();
            Focus();
        }

        private void GameView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UnsubscribeFromViewModel();
            SubscribeToViewModel();
        }

        private void SubscribeToViewModel()
        {
            _viewModel = DataContext as GameViewModel;
            if (_viewModel != null)
            {
                _viewModel.FrameUpdated += OnFrameUpdated;
            }
        }

        private void UnsubscribeFromViewModel()
        {
            if (_viewModel != null)
            {
                _viewModel.FrameUpdated -= OnFrameUpdated;
                _viewModel = null;
            }
        }

        private void OnFrameUpdated(object? sender, EventArgs e)
        {
            DrawFrame();
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
            if (_viewModel == null) return;

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
