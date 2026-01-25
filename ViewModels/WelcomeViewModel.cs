using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Snake.ViewModels
{
    /// <summary>
    /// ViewModel de l'écran d'accueil : titre et commande Démarrer.
    /// </summary>
    public partial class WelcomeViewModel : ObservableObject
    {
        /// <summary>Déclenché lorsque l'utilisateur choisit de lancer une partie.</summary>
        public event EventHandler? StartGameRequested;

        [RelayCommand]
        private void Demarrer()
        {
            StartGameRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
