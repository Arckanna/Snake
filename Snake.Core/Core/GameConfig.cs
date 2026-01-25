namespace Snake.Core
{
    /// <summary>
    /// Constantes centralisées du jeu pour éviter les magic numbers.
    /// </summary>
    public static class GameConfig
    {
        /// <summary>Largeur de la zone de jeu (pixels).</summary>
        public const int AreaWidth = 700;

        /// <summary>Hauteur de la zone de jeu (pixels).</summary>
        public const int AreaHeight = 400;

        /// <summary>Intervalle entre deux ticks (ms).</summary>
        public const int TickIntervalMs = 100;

        /// <summary>Taille d'une case (carré) en pixels.</summary>
        public const int SquareSize = 20;

        /// <summary>Longueur initiale du serpent.</summary>
        public const int InitialSnakeLength = 10;
    }
}
