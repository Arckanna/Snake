using System.IO;
using System.Text.Json;

namespace Snake.Services
{
    /// <summary>
    /// Implémentation de IScoreService qui persiste les scores dans un fichier JSON dans UserAppData.
    /// </summary>
    public sealed class FileScoreService : IScoreService
    {
        private readonly string _scoreFilePath;

        public FileScoreService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "Snake");
            Directory.CreateDirectory(appFolder);
            _scoreFilePath = Path.Combine(appFolder, "bestscore.json");
        }

        public int GetBestScore()
        {
            try
            {
                if (!File.Exists(_scoreFilePath))
                    return 0;

                var json = File.ReadAllText(_scoreFilePath);
                var data = JsonSerializer.Deserialize<ScoreData>(json);
                return data?.BestScore ?? 0;
            }
            catch
            {
                // En cas d'erreur (fichier corrompu, etc.), retourner 0
                return 0;
            }
        }

        public void SaveScore(int score)
        {
            try
            {
                var currentBest = GetBestScore();
                if (score > currentBest)
                {
                    var data = new ScoreData { BestScore = score };
                    var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(_scoreFilePath, json);
                }
            }
            catch
            {
                // Ignorer les erreurs d'écriture (permissions, etc.)
            }
        }

        private class ScoreData
        {
            public int BestScore { get; set; }
        }
    }
}
