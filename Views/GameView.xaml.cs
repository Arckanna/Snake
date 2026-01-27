using System;
using System.Diagnostics;
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

        // Propriété helper pour accéder au Canvas GameArea (évite les conflits avec le champ généré)
        private Canvas? GetGameArea() => FindName("GameArea") as Canvas;

        public GameView()
        {
            InitializeComponent();
            Loaded += GameView_Loaded;
            DataContextChanged += GameView_DataContextChanged;
        }

        private void GameView_Loaded(object sender, RoutedEventArgs e)
        {
            SubscribeToViewModel();
            // Attendre que le layout soit complété avant de dessiner
            Dispatcher.BeginInvoke(new Action(() =>
            {
                DrawGameArea();
                
                // Démarrer le jeu si nécessaire (via MainViewModel)
                var mainViewModel = FindMainViewModel();
                if (mainViewModel != null)
                {
                    mainViewModel.StartGameIfPending();
                }
                
                if (_viewModel != null)
                {
                    // Forcer un premier dessin si le jeu est déjà démarré
                    DrawFrame();
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
            Focus();
        }

        private ViewModels.MainViewModel? FindMainViewModel()
        {
            // Remonter dans l'arbre visuel pour trouver le MainViewModel
            var element = this.Parent as System.Windows.FrameworkElement;
            while (element != null)
            {
                if (element.DataContext is ViewModels.MainViewModel mainViewModel)
                    return mainViewModel;
                
                element = element.Parent as System.Windows.FrameworkElement;
            }
            
            // Si on ne trouve pas dans l'arbre visuel, essayer depuis la fenêtre
            var window = System.Windows.Window.GetWindow(this);
            if (window?.DataContext is ViewModels.MainViewModel windowMainViewModel)
                return windowMainViewModel;
            
            return null;
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
            try
            {
                // Vérifier que le contrôle est chargé avant de dessiner
                if (IsLoaded && GetGameArea() != null)
                {
                    DrawFrame();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur dans OnFrameUpdated: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void DrawGameArea()
        {
            var gameArea = GetGameArea();
            // Vérifier que le Canvas est initialisé et a une taille
            if (gameArea == null || gameArea.ActualWidth <= 0 || gameArea.ActualHeight <= 0)
                return;

            // Fond avec dégradé subtil
            var rect = new Rectangle
            {
                Width = gameArea.ActualWidth,
                Height = gameArea.ActualHeight
            };
            var gradientBrush = new LinearGradientBrush
            {
                StartPoint = new System.Windows.Point(0, 0),
                EndPoint = new System.Windows.Point(1, 1)
            };
            gradientBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#F5F2EB")!, 0));
            gradientBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#E8E5DD")!, 1));
            rect.Fill = gradientBrush;
            gameArea.Children.Add(rect);
            Canvas.SetLeft(rect, 0);
            Canvas.SetTop(rect, 0);
        }

        private void DrawFrame()
        {
            try
            {
                var gameArea = GetGameArea();
                if (_viewModel == null || gameArea == null) return;

                // Vérifier que le Canvas a une taille valide
                if (gameArea.ActualWidth <= 0 || gameArea.ActualHeight <= 0) return;

                // S'assurer que le fond est dessiné si nécessaire
                if (gameArea.Children.Count == 0)
                {
                    DrawGameArea();
                }

                // Garder le fond (index 0), supprimer le reste
                while (gameArea.Children.Count > 1)
                    gameArea.Children.RemoveAt(1);

                var size = _viewModel.SquareSize;
                if (size <= 0) return;

                var parts = _viewModel.SnakeParts;
                if (parts == null) return;

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
                    gameArea.Children.Add(leftEye);
                    gameArea.Children.Add(rightEye);
                    Canvas.SetLeft(leftEye, part.X + eyeOffset);
                    Canvas.SetTop(leftEye, part.Y + eyeOffset);
                    Canvas.SetLeft(rightEye, part.X + size - eyeOffset - eyeSize);
                    Canvas.SetTop(rightEye, part.Y + eyeOffset);
                }
                
                gameArea.Children.Add(r);
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
                
                gameArea.Children.Add(food);
                gameArea.Children.Add(foodAccent);
                Canvas.SetLeft(food, fp.X + size * 0.075);
                Canvas.SetTop(food, fp.Y + size * 0.075);
                Canvas.SetLeft(foodAccent, fp.X + size * 0.3);
                Canvas.SetTop(foodAccent, fp.Y + size * 0.3);
            }
            }
            catch (Exception ex)
            {
                // Log l'erreur mais ne pas faire crasher l'application
                Debug.WriteLine($"Erreur dans DrawFrame: {ex.Message}");
            }
        }
    }
}
