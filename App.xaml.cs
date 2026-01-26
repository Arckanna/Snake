using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Snake.Core;
using Snake.Services;

namespace Snake
{
    public partial class App : Application
    {
        private WelcomeWindow? _welcomeWindow;
        private IServiceProvider? _serviceProvider;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var services = new ServiceCollection();
            services.AddTransient<IGameEngine, GameEngine>();
            services.AddTransient<ITimerService, DispatcherTimerService>();
            services.AddTransient<MainWindow>();

            _serviceProvider = services.BuildServiceProvider();

            ShowWelcomeWindow();
        }

        /// <summary>Affiche la fenêtre d'accueil (crée une nouvelle instance si nécessaire).</summary>
        public void ShowWelcomeWindow()
        {
            if (_welcomeWindow == null)
            {
                _welcomeWindow = new WelcomeWindow(_serviceProvider!);
                _welcomeWindow.Closed += (s, e) => _welcomeWindow = null;
            }
            _welcomeWindow.Show();
            _welcomeWindow.Activate();
        }
    }
}
