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

        // Flag pour savoir si le fond a déjà été dessiné
        private bool _backgroundDrawn = false;

        public GameView()
        {
            InitializeComponent();
            Loaded += GameView_Loaded;
            DataContextChanged += GameView_DataContextChanged;
            IsVisibleChanged += GameView_IsVisibleChanged;
            LayoutUpdated += GameView_LayoutUpdated;
        }

        private bool _layoutInitialized = false;

        private void GameView_LayoutUpdated(object? sender, EventArgs e)
        {
            if (_layoutInitialized)
                return;

            var gameArea = GetGameArea();
            if (gameArea == null)
            {
                Debug.WriteLine("GameView.LayoutUpdated: gameArea est null");
                return;
            }

            // Si _viewModel est null, essayer de le récupérer depuis DataContext
            if (_viewModel == null)
            {
                Debug.WriteLine($"GameView.LayoutUpdated: _viewModel est null, DataContext={DataContext?.GetType().Name ?? "null"}");
                SubscribeToViewModel();
                if (_viewModel == null)
                {
                    Debug.WriteLine("GameView.LayoutUpdated: _viewModel toujours null après SubscribeToViewModel");
                    return;
                }
            }

            // Essayer ActualWidth/ActualHeight d'abord, puis RenderSize
            double width = gameArea.ActualWidth > 0 ? gameArea.ActualWidth : gameArea.RenderSize.Width;
            double height = gameArea.ActualHeight > 0 ? gameArea.ActualHeight : gameArea.RenderSize.Height;

            Debug.WriteLine($"GameView.LayoutUpdated: ActualWidth={gameArea.ActualWidth}, ActualHeight={gameArea.ActualHeight}, RenderSize={gameArea.RenderSize.Width}x{gameArea.RenderSize.Height}, Width={width}, Height={height}");

            if (width > 0 && height > 0)
            {
                Debug.WriteLine($"GameView.LayoutUpdated: Canvas a maintenant une taille = {width}x{height}");
                if (!_viewModel.AreDimensionsInitialized)
                {
                    Debug.WriteLine($"GameView.LayoutUpdated: Initialisation des dimensions et démarrage du jeu");
                    _layoutInitialized = true;
                    _viewModel.InitializeWithDimensions(width, height);
                    DrawGameArea();
                    _startGameAttempted = false;
                    TryStartGame();

                    // Se désabonner après la première initialisation réussie pour éviter les appels répétés
                    LayoutUpdated -= GameView_LayoutUpdated;
                }
            }
        }

        private void GameArea_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Debug.WriteLine($"GameView.GameArea_SizeChanged: Ancienne taille = {e.PreviousSize.Width}x{e.PreviousSize.Height}, Nouvelle taille = {e.NewSize.Width}x{e.NewSize.Height}");
            var gameArea = sender as Canvas;
            if (gameArea == null)
            {
                Debug.WriteLine("GameView.GameArea_SizeChanged: gameArea est null");
                return;
            }

            // Si _viewModel est null, essayer de le récupérer depuis DataContext
            if (_viewModel == null)
            {
                Debug.WriteLine($"GameView.GameArea_SizeChanged: _viewModel est null, DataContext={DataContext?.GetType().Name ?? "null"}");
                SubscribeToViewModel();
                if (_viewModel == null)
                {
                    Debug.WriteLine("GameView.GameArea_SizeChanged: _viewModel toujours null après SubscribeToViewModel");
                    return;
                }
            }

            if (e.NewSize.Width > 0 && e.NewSize.Height > 0)
            {
                Debug.WriteLine($"GameView.GameArea_SizeChanged: Nouvelle taille valide = {e.NewSize.Width}x{e.NewSize.Height}");
                // Initialiser les dimensions si elles ne l'ont pas encore été
                if (!_viewModel.AreDimensionsInitialized)
                {
                    Debug.WriteLine("GameView.GameArea_SizeChanged: Initialisation des dimensions et démarrage du jeu");
                    _viewModel.InitializeWithDimensions(e.NewSize.Width, e.NewSize.Height);
                    DrawGameArea();

                    // Réinitialiser le flag pour permettre le démarrage
                    _startGameAttempted = false;
                    TryStartGame();
                }
                else
                {
                    Debug.WriteLine("GameView.GameArea_SizeChanged: Mise à jour des dimensions");
                    // Mettre à jour les dimensions si elles ont changé
                    _viewModel.InitializeWithDimensions(e.NewSize.Width, e.NewSize.Height);
                }
            }
            else
            {
                Debug.WriteLine($"GameView.GameArea_SizeChanged: Taille invalide (Width={e.NewSize.Width}, Height={e.NewSize.Height})");
            }
        }

        private void GameView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible && IsLoaded && (bool)e.NewValue)
            {
                Debug.WriteLine("GameView.IsVisibleChanged: Le contrôle est maintenant visible");
                // Réinitialiser le flag pour permettre un nouveau démarrage
                _startGameAttempted = false;
                _layoutInitialized = false;

                // Réabonner à LayoutUpdated si nécessaire
                LayoutUpdated += GameView_LayoutUpdated;
            }
        }

        private void GameView_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("GameView.GameView_Loaded: Le contrôle est chargé");
            SubscribeToViewModel();

            // S'abonner à SizeChanged du canvas pour détecter quand il a une taille réelle
            var gameArea = GetGameArea();
            if (gameArea != null)
            {
                Debug.WriteLine($"GameView.GameView_Loaded: Abonnement à SizeChanged du canvas");
                gameArea.SizeChanged += GameArea_SizeChanged;

                // Vérifier immédiatement la taille
                Debug.WriteLine($"GameView.GameView_Loaded: Taille immédiate du canvas = {gameArea.ActualWidth}x{gameArea.ActualHeight}");
            }
            else
            {
                Debug.WriteLine("GameView.GameView_Loaded: gameArea est null !");
            }

            // Forcer une mise à jour du layout pour que le canvas ait une taille réelle
            UpdateLayout();

            // Attendre que le layout soit complété avant de dessiner
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Debug.WriteLine("GameView.GameView_Loaded: Dispatcher.BeginInvoke exécuté");
                InitializeCanvasDimensionsAndStart();
            }), System.Windows.Threading.DispatcherPriority.Loaded);
            Focus();
        }

        private void InitializeCanvasDimensionsAndStart()
        {
            var gameArea = GetGameArea();
            if (gameArea == null)
            {
                Debug.WriteLine("GameView.InitializeCanvasDimensionsAndStart: gameArea est null");
                return;
            }

            if (_viewModel == null)
            {
                Debug.WriteLine("GameView.InitializeCanvasDimensionsAndStart: _viewModel est null");
                return;
            }

            Debug.WriteLine($"GameView.InitializeCanvasDimensionsAndStart: ActualWidth={gameArea.ActualWidth}, ActualHeight={gameArea.ActualHeight}, Width={gameArea.Width}, Height={gameArea.Height}");

            // Attendre que le canvas ait une taille réelle
            if (gameArea.ActualWidth > 0 && gameArea.ActualHeight > 0)
            {
                Debug.WriteLine($"GameView.InitializeCanvasDimensions: Canvas taille réelle = {gameArea.ActualWidth}x{gameArea.ActualHeight}");
                _viewModel.InitializeWithDimensions(gameArea.ActualWidth, gameArea.ActualHeight);
                DrawGameArea();

                // Réinitialiser le flag pour permettre le démarrage maintenant que les dimensions sont prêtes
                _startGameAttempted = false;

                // Démarrer le jeu si nécessaire (via MainViewModel) - maintenant que les dimensions sont initialisées
                TryStartGame();

                if (_viewModel != null)
                {
                    // Forcer un premier dessin si le jeu est déjà démarré
                    DrawFrame();
                }
            }
            else
            {
                // Réessayer après un court délai si les dimensions ne sont pas encore disponibles
                Debug.WriteLine($"GameView.InitializeCanvasDimensions: Canvas pas encore dimensionné (ActualWidth={gameArea.ActualWidth}, ActualHeight={gameArea.ActualHeight}), réessai...");
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    InitializeCanvasDimensionsAndStart();
                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }
        }

        private void InitializeCanvasDimensions()
        {
            var gameArea = GetGameArea();
            if (gameArea == null || _viewModel == null)
                return;

            // Attendre que le canvas ait une taille réelle
            if (gameArea.ActualWidth > 0 && gameArea.ActualHeight > 0)
            {
                Debug.WriteLine($"GameView.InitializeCanvasDimensions: Canvas taille réelle = {gameArea.ActualWidth}x{gameArea.ActualHeight}");
                _viewModel.InitializeWithDimensions(gameArea.ActualWidth, gameArea.ActualHeight);
            }
        }

        private bool _startGameAttempted = false;

        private void TryStartGame()
        {
            // Éviter les tentatives multiples
            if (_startGameAttempted)
                return;

            var mainViewModel = FindMainViewModel();
            if (mainViewModel != null)
            {
                Debug.WriteLine("GameView: MainViewModel trouvé, appel de StartGameIfPending");
                _startGameAttempted = true;
                mainViewModel.StartGameIfPending();
            }
            else
            {
                Debug.WriteLine("GameView: MainViewModel non trouvé");
                // Réessayer une seule fois après un court délai
                if (!_startGameAttempted)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (!_startGameAttempted)
                        {
                            var mainVm = FindMainViewModel();
                            if (mainVm != null)
                            {
                                Debug.WriteLine("GameView: MainViewModel trouvé au deuxième essai");
                                _startGameAttempted = true;
                                mainVm.StartGameIfPending();
                            }
                        }
                    }), System.Windows.Threading.DispatcherPriority.Loaded, null);
                }
            }
        }

        private ViewModels.MainViewModel? FindMainViewModel()
        {
            // Essayer d'abord depuis la fenêtre (plus fiable)
            var window = System.Windows.Window.GetWindow(this);
            if (window?.DataContext is ViewModels.MainViewModel windowMainViewModel)
            {
                Debug.WriteLine("GameView: MainViewModel trouvé via Window.DataContext");
                return windowMainViewModel;
            }

            // Essayer via le MainWindow directement avec la propriété publique
            if (window is MainWindow mainWindow)
            {
                var mainVm = mainWindow.MainViewModel;
                if (mainVm != null)
                {
                    Debug.WriteLine("GameView: MainViewModel trouvé via MainWindow.MainViewModel");
                    return mainVm;
                }
            }

            // Essayer via Application.Current.MainWindow
            if (System.Windows.Application.Current?.MainWindow is MainWindow appMainWindow)
            {
                var mainVm = appMainWindow.MainViewModel;
                if (mainVm != null)
                {
                    Debug.WriteLine("GameView: MainViewModel trouvé via Application.Current.MainWindow");
                    return mainVm;
                }
            }

            // Remonter dans l'arbre visuel pour trouver le MainViewModel
            var element = this.Parent as System.Windows.FrameworkElement;
            int depth = 0;
            while (element != null && depth < 10) // Limiter la profondeur pour éviter les boucles infinies
            {
                if (element.DataContext is ViewModels.MainViewModel mainViewModel)
                {
                    Debug.WriteLine($"GameView: MainViewModel trouvé via arbre visuel (profondeur {depth})");
                    return mainViewModel;
                }

                element = element.Parent as System.Windows.FrameworkElement;
                depth++;
            }

            Debug.WriteLine($"GameView: MainViewModel non trouvé (profondeur max atteinte: {depth})");
            return null;
        }

        private void GameView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Debug.WriteLine($"GameView.DataContextChanged: Ancien={e.OldValue?.GetType().Name ?? "null"}, Nouveau={e.NewValue?.GetType().Name ?? "null"}");
            UnsubscribeFromViewModel();
            SubscribeToViewModel();

            // Réinitialiser le flag du fond lors du changement de DataContext
            _backgroundDrawn = false;
            _layoutInitialized = false;

            // Si le contrôle est déjà chargé et qu'on a un nouveau DataContext, essayer de démarrer le jeu
            if (IsLoaded && _viewModel != null)
            {
                Debug.WriteLine("GameView.DataContextChanged: Contrôle chargé, initialisation des dimensions");
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    InitializeCanvasDimensionsAndStart();
                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }
        }

        private void SubscribeToViewModel()
        {
            _viewModel = DataContext as GameViewModel;
            Debug.WriteLine($"GameView.SubscribeToViewModel: DataContext={DataContext?.GetType().Name ?? "null"}, _viewModel={_viewModel != null}");
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

            // Ne dessiner le fond qu'une seule fois
            if (_backgroundDrawn && gameArea.Children.Count > 0)
            {
                // Mettre à jour la taille du fond existant si nécessaire
                if (gameArea.Children[0] is Rectangle existingRect)
                {
                    existingRect.Width = gameArea.ActualWidth;
                    existingRect.Height = gameArea.ActualHeight;
                }
                return;
            }

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
            gameArea.Children.Insert(0, rect); // Insérer au début pour toujours être à l'index 0
            Canvas.SetLeft(rect, 0);
            Canvas.SetTop(rect, 0);
            _backgroundDrawn = true;
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
                DrawGameArea();

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
                Debug.WriteLine($"GameView.DrawFrame: Dessin de {count} segments, FoodPosition={_viewModel.FoodPosition}, State={_viewModel.State}, Area={_viewModel.AreaWidth}x{_viewModel.AreaHeight}");

                for (int i = 0; i < count; i++)
                {
                    var part = parts[i];
                    bool isHead = (i == count - 1);

                    Debug.WriteLine($"GameView.DrawFrame: Segment {i} à position ({part.X}, {part.Y}), isHead={isHead}");

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
