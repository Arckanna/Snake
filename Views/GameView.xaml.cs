using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using Snake.ViewModels;
using Snake;

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
            IsVisibleChanged += GameView_IsVisibleChanged;
        }
        
        private void GameView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible && IsLoaded && (bool)e.NewValue)
            {
                Debug.WriteLine("GameView.IsVisibleChanged: Le contrôle est maintenant visible");
                // Réinitialiser le flag pour permettre un nouveau démarrage
                _startGameAttempted = false;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    TryStartGame();
                    if (_viewModel != null)
                    {
                        DrawFrame();
                    }
                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }
        }

        private void GameView_Loaded(object sender, RoutedEventArgs e)
        {
            SubscribeToViewModel();
            // Attendre que le layout soit complété avant de dessiner
            Dispatcher.BeginInvoke(new Action(() =>
            {
                DrawGameArea();
                
                // Démarrer le jeu si nécessaire (via ShellViewModel)
                TryStartGame();
                
                if (_viewModel != null)
                {
                    // Forcer un premier dessin si le jeu est déjà démarré
                    DrawFrame();
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
            Focus();
        }
        
        private bool _startGameAttempted = false;
        
        private void TryStartGame()
        {
            // Éviter les tentatives multiples
            if (_startGameAttempted)
                return;
                
            var shellViewModel = FindShellViewModel();
            if (shellViewModel != null)
            {
                Debug.WriteLine("GameView: ShellViewModel trouvé, appel de StartGameIfPending");
                _startGameAttempted = true;
                shellViewModel.StartGameIfPending();
            }
            else
            {
                Debug.WriteLine("GameView: ShellViewModel non trouvé");
                // Réessayer une seule fois après un court délai
                if (!_startGameAttempted)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (!_startGameAttempted)
                        {
                            var shell = FindShellViewModel();
                            if (shell != null)
                            {
                                Debug.WriteLine("GameView: ShellViewModel trouvé au deuxième essai");
                                _startGameAttempted = true;
                                shell.StartGameIfPending();
                            }
                        }
                    }), System.Windows.Threading.DispatcherPriority.Loaded, null);
                }
            }
        }

        private ViewModels.ShellViewModel? FindShellViewModel()
        {
            // Essayer d'abord depuis la fenêtre (plus fiable)
            var window = System.Windows.Window.GetWindow(this);
            if (window?.DataContext is ViewModels.ShellViewModel windowShellViewModel)
            {
                Debug.WriteLine("GameView: ShellViewModel trouvé via Window.DataContext");
                return windowShellViewModel;
            }
            
            // Essayer via le MainWindow directement avec la propriété publique
            if (window is MainWindow mainWindow)
            {
                var shellVm = mainWindow.ShellViewModel;
                if (shellVm != null)
                {
                    Debug.WriteLine("GameView: ShellViewModel trouvé via MainWindow.ShellViewModel");
                    return shellVm;
                }
            }
            
            // Essayer via Application.Current.MainWindow
            if (System.Windows.Application.Current?.MainWindow is MainWindow appMainWindow)
            {
                var shellVm = appMainWindow.ShellViewModel;
                if (shellVm != null)
                {
                    Debug.WriteLine("GameView: ShellViewModel trouvé via Application.Current.MainWindow");
                    return shellVm;
                }
            }
            
            // Remonter dans l'arbre visuel pour trouver le ShellViewModel
            var element = this.Parent as System.Windows.FrameworkElement;
            int depth = 0;
            while (element != null && depth < 10) // Limiter la profondeur pour éviter les boucles infinies
            {
                if (element.DataContext is ViewModels.ShellViewModel shellViewModel)
                {
                    Debug.WriteLine($"GameView: ShellViewModel trouvé via arbre visuel (profondeur {depth})");
                    return shellViewModel;
                }
                
                element = element.Parent as System.Windows.FrameworkElement;
                depth++;
            }
            
            Debug.WriteLine($"GameView: ShellViewModel non trouvé (profondeur max atteinte: {depth})");
            return null;
        }

        private void GameView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UnsubscribeFromViewModel();
            SubscribeToViewModel();
            
            // Si le contrôle est déjà chargé et qu'on a un nouveau DataContext, essayer de démarrer le jeu
            if (IsLoaded && _viewModel != null)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    TryStartGame();
                    DrawFrame();
                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }
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
                Debug.WriteLine($"GameView.OnFrameUpdated: IsLoaded={IsLoaded}, GameArea={GetGameArea() != null}");
                // Vérifier que le contrôle est chargé avant de dessiner
                if (IsLoaded && GetGameArea() != null)
                {
                    DrawFrame();
                }
                else
                {
                    Debug.WriteLine($"GameView.OnFrameUpdated: Conditions non remplies pour dessiner");
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
                if (_viewModel == null || gameArea == null)
                {
                    Debug.WriteLine($"GameView.DrawFrame: _viewModel={_viewModel != null}, gameArea={gameArea != null}");
                    return;
                }

                // Vérifier que le Canvas a une taille valide (utiliser Width/Height si ActualWidth/Height sont 0)
                double canvasWidth = gameArea.ActualWidth > 0 ? gameArea.ActualWidth : gameArea.Width;
                double canvasHeight = gameArea.ActualHeight > 0 ? gameArea.ActualHeight : gameArea.Height;
                
                if (canvasWidth <= 0 || canvasHeight <= 0)
                {
                    Debug.WriteLine($"GameView.DrawFrame: Canvas taille invalide (Actual: {gameArea.ActualWidth}x{gameArea.ActualHeight}, Size: {gameArea.Width}x{gameArea.Height})");
                    // Réessayer après un court délai
                    Dispatcher.BeginInvoke(new Action(() => DrawFrame()), System.Windows.Threading.DispatcherPriority.Loaded);
                    return;
                }

                // S'assurer que le fond est dessiné si nécessaire
                if (gameArea.Children.Count == 0)
                {
                    DrawGameArea();
                }

                // Garder le fond (index 0), supprimer le reste
                while (gameArea.Children.Count > 1)
                    gameArea.Children.RemoveAt(1);

                var size = _viewModel.SquareSize;
                if (size <= 0)
                {
                    Debug.WriteLine($"GameView.DrawFrame: SquareSize invalide ({size}), State={_viewModel.State}");
                    return;
                }

                var parts = _viewModel.SnakeParts;
                if (parts == null || parts.Count == 0)
                {
                    Debug.WriteLine($"GameView.DrawFrame: SnakeParts est null ou vide (Count={parts?.Count ?? 0}), State={_viewModel.State}");
                    return;
                }

                var count = parts.Count;
                Debug.WriteLine($"GameView.DrawFrame: Dessin de {count} segments, FoodPosition={_viewModel.FoodPosition}, State={_viewModel.State}");

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
