using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Snake.Core;
using Snake.Services;

namespace Snake
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var services = new ServiceCollection();
            services.AddTransient<IGameEngine, GameEngine>();
            services.AddTransient<ITimerService, DispatcherTimerService>();
            services.AddTransient<MainWindow>();

            var provider = services.BuildServiceProvider();

            var welcome = new WelcomeWindow(provider);
            welcome.Show();
        }
    }
}
