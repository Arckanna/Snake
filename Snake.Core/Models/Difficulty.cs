namespace Snake.Models
{
    /// <summary>
    /// Niveau de difficulté du jeu, défini par l'intervalle du timer en millisecondes.
    /// </summary>
    public enum Difficulty
    {
        /// <summary>Difficulté lente : 150 ms entre chaque tick.</summary>
        Lent = 150,

        /// <summary>Difficulté normale : 100 ms entre chaque tick.</summary>
        Normal = 100,

        /// <summary>Difficulté rapide : 70 ms entre chaque tick.</summary>
        Rapide = 70
    }
}
