using System.Windows;

namespace Snake
{
    /// <summary>
    /// Ancien modèle lié à la vue (Rectangle WPF). Conservé pour MainWindow
    /// jusqu’à la migration vers GameViewModel. À supprimer ensuite.
    /// </summary>
    internal class SnakePartView
    {
        public UIElement? UiElement { get; set; }
        public Point Position { get; set; }
    }
}
