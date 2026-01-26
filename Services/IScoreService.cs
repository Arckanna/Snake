namespace Snake.Services
{
    /// <summary>
    /// Service de persistance des scores. Permet de sauvegarder et récupérer le meilleur score.
    /// </summary>
    public interface IScoreService
    {
        /// <summary>Récupère le meilleur score sauvegardé.</summary>
        /// <returns>Le meilleur score, ou 0 si aucun score n'a été sauvegardé.</returns>
        int GetBestScore();

        /// <summary>Sauvegarde un score s'il est supérieur au meilleur score actuel.</summary>
        /// <param name="score">Le score à sauvegarder.</param>
        void SaveScore(int score);
    }
}
