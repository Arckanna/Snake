using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Snake.Core;
using Snake.Services;

namespace Snake
{
    public partial class App : Application
    {
        private IServiceProvider? _serviceProvider;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var services = new ServiceCollection();
            services.AddTransient<IGameEngine, GameEngine>();
            services.AddTransient<ITimerService, DispatcherTimerService>();
            services.AddSingleton<IScoreService, FileScoreService>();
            services.AddTransient<MainWindow>();

            _serviceProvider = services.BuildServiceProvider();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}
