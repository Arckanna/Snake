using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
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
            // Gestionnaire d'exceptions non gérées
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            
            var services = new ServiceCollection();
            services.AddTransient<IGameEngine, GameEngine>();
            services.AddTransient<ITimerService, DispatcherTimerService>();
            services.AddSingleton<IScoreService, FileScoreService>();
            services.AddTransient<MainWindow>();

            _serviceProvider = services.BuildServiceProvider();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Debug.WriteLine($"Exception non gérée dans le Dispatcher: {e.Exception.Message}\n{e.Exception.StackTrace}");
            MessageBox.Show($"Erreur: {e.Exception.Message}\n\n{e.Exception.StackTrace}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true; // Empêcher le crash
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                Debug.WriteLine($"Exception non gérée: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Erreur critique: {ex.Message}\n\n{ex.StackTrace}", "Erreur Critique", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
