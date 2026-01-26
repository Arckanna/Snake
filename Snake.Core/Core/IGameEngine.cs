using Snake.Models;

namespace Snake.Core
{
    /// <summary>
    /// Moteur de jeu pur : logique Snake sans dépendance WPF/UI.
    /// Testable et réutilisable.
    /// </summary>
    public interface IGameEngine
    {
        /// <summary>État courant de la partie.</summary>
        GameState State { get; }

        /// <summary>Segments du serpent (tête en dernier).</summary>
        IReadOnlyList<Snake.Models.SnakePart> SnakeParts { get; }

        /// <summary>Position du fruit, ou null si aucun.</summary>
        (double X, double Y)? FoodPosition { get; }

        /// <summary>Score actuel.</summary>
        int Score { get; }

        /// <summary>Taille d'une case (carré) en pixels.</summary>
        double SquareSize { get; }

        /// <summary>
        /// Initialise une nouvelle partie.
        /// </summary>
        void Initialize(double areaWidth, double areaHeight, double squareSize = 20, int initialSnakeLength = 10);

        /// <summary>
        /// Avance d'une frame. Gère les demi-tours (ignorés).
        /// </summary>
        void Move(Direction requestedDirection);

        /// <summary>
        /// Définit l'état du jeu (utilisé pour la pause).
        /// </summary>
        void SetState(GameState state);
    }
}
