namespace Snake.Services
{
    /// <summary>
    /// Abstraction du timer pour les ticks de jeu. Permet de mocker en tests.
    /// </summary>
    public interface ITimerService
    {
        /// <summary>Démarre le timer : appelle <paramref name="onTick"/> à chaque intervalle.</summary>
        /// <param name="interval">Intervalle entre deux appels.</param>
        /// <param name="onTick">Callback invoqué à chaque tick.</param>
        void Start(TimeSpan interval, Action onTick);

        /// <summary>Arrête le timer et désabonne le callback.</summary>
        void Stop();
    }
}
