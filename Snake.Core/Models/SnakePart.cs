namespace Snake.Models
{
    /// <summary>
    /// Modèle d'un segment du serpent. Sans référence UI pour rester testable et découplé de la vue.
    /// </summary>
    public class SnakePart
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
}
