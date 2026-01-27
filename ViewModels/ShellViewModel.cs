using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Snake.Models;

namespace Snake.ViewModels
{
    /// <summary>
    /// Type d'écran affiché dans l'application.
    /// </summary>
    public enum ScreenKind
    {
        Home,
        Game
    }

    /// <summary>
    /// ViewModel principal qui pilote quel écran est affiché.
    /// </summary>
    public partial class ShellViewModel : ObservableObject
    {
        [ObservableProperty]
        private ScreenKind currentScreen = ScreenKind.Home;

        public WelcomeViewModel Welcome { get; }
        public GameViewModel Game { get; }

        private Difficulty? _pendingDifficulty;

        public ShellViewModel(WelcomeViewModel welcome, GameViewModel game)
        {
            Welcome = welcome;
            Game = game;

            // Quand l'accueil demande "Start", on bascule sur le jeu
            Welcome.StartGameRequested += (sender, difficulty) =>
            {
                CurrentScreen = ScreenKind.Game;
                // Stocker la difficulté pour démarrer le jeu une fois que GameView est prêt
                _pendingDifficulty = difficulty;
            };

            // Quand le jeu demande de retourner à l'accueil
            Game.ReturnToWelcomeRequested += (sender, e) =>
            {
                CurrentScreen = ScreenKind.Home;
                Game.Stop();
                Welcome.RefreshBestScore();
            };

            // Quand le jeu est terminé, on reste sur l'écran de jeu (overlay GameOver)
            // Le GameViewModel gère déjà l'affichage du GameOver
        }

        /// <summary>Démarre le jeu si une difficulté est en attente. Appelé par GameView quand il est prêt.</summary>
        public void StartGameIfPending()
        {
            System.Diagnostics.Debug.WriteLine($"ShellViewModel.StartGameIfPending: _pendingDifficulty={_pendingDifficulty}");
            if (_pendingDifficulty.HasValue)
            {
                try
                {
                    int tickInterval = (int)_pendingDifficulty.Value;
                    System.Diagnostics.Debug.WriteLine($"ShellViewModel.StartGameIfPending: Démarrage du jeu avec intervalle {tickInterval}ms");
                    Game.Start(tickInterval);
                    _pendingDifficulty = null;
                    System.Diagnostics.Debug.WriteLine("ShellViewModel.StartGameIfPending: Jeu démarré avec succès");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erreur lors du démarrage du jeu: {ex.Message}\n{ex.StackTrace}");
                    System.Windows.MessageBox.Show($"Erreur lors du démarrage du jeu: {ex.Message}", "Erreur", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ShellViewModel.StartGameIfPending: Aucune difficulté en attente");
            }
        }
    }
}
