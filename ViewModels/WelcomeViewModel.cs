using System;
using System.Diagnostics;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Snake.Models;
using Snake.Services;

namespace Snake.ViewModels
{
    /// <summary>
    /// ViewModel de l'écran d'accueil : titre, sélection de difficulté et commande Démarrer.
    /// </summary>
    public partial class WelcomeViewModel : ObservableObject
    {
        private readonly IScoreService _scoreService;
        private Difficulty _selectedDifficulty = Difficulty.Normal;

        public WelcomeViewModel(IScoreService scoreService)
        {
            _scoreService = scoreService;
        }

        /// <summary>Difficulté sélectionnée par l'utilisateur.</summary>
        public Difficulty SelectedDifficulty
        {
            get => _selectedDifficulty;
            set => SetProperty(ref _selectedDifficulty, value);
        }

        /// <summary>Indique si la difficulté "Lent" est sélectionnée.</summary>
        public bool IsLent
        {
            get => _selectedDifficulty == Difficulty.Lent;
            set
            {
                if (value)
                {
                    SelectedDifficulty = Difficulty.Lent;
                    OnPropertyChanged(nameof(IsNormal));
                    OnPropertyChanged(nameof(IsRapide));
                }
            }
        }

        /// <summary>Indique si la difficulté "Normal" est sélectionnée.</summary>
        public bool IsNormal
        {
            get => _selectedDifficulty == Difficulty.Normal;
            set
            {
                if (value)
                {
                    SelectedDifficulty = Difficulty.Normal;
                    OnPropertyChanged(nameof(IsLent));
                    OnPropertyChanged(nameof(IsRapide));
                }
            }
        }

        /// <summary>Indique si la difficulté "Rapide" est sélectionnée.</summary>
        public bool IsRapide
        {
            get => _selectedDifficulty == Difficulty.Rapide;
            set
            {
                if (value)
                {
                    SelectedDifficulty = Difficulty.Rapide;
                    OnPropertyChanged(nameof(IsLent));
                    OnPropertyChanged(nameof(IsNormal));
                }
            }
        }

        /// <summary>Meilleur score sauvegardé.</summary>
        public int BestScore => _scoreService.GetBestScore();

        /// <summary>Rafraîchit l'affichage du meilleur score (à appeler lors du retour à l'accueil).</summary>
        public void RefreshBestScore()
        {
            OnPropertyChanged(nameof(BestScore));
        }

        /// <summary>Déclenché lorsque l'utilisateur choisit de lancer une partie, avec la difficulté sélectionnée.</summary>
        public event EventHandler<Difficulty>? StartGameRequested;

        [RelayCommand]
        private void Demarrer()
        {
            try
            {
                StartGameRequested?.Invoke(this, SelectedDifficulty);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur dans Demarrer: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Erreur lors du démarrage: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
