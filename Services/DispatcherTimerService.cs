using System.Windows.Threading;

namespace Snake.Services
{
    /// <summary>
    /// Implémentation WPF de ITimerService basée sur DispatcherTimer.
    /// </summary>
    public sealed class DispatcherTimerService : ITimerService
    {
        private DispatcherTimer? _timer;
        private EventHandler? _handler;

        public void Start(TimeSpan interval, Action onTick)
        {
            Stop();

            _handler = (_, _) => onTick();
            _timer = new DispatcherTimer { Interval = interval };
            _timer.Tick += _handler;
            _timer.Start();
        }

        public void Stop()
        {
            if (_timer != null && _handler != null)
            {
                _timer.Stop();
                _timer.Tick -= _handler;
            }
            _timer = null;
            _handler = null;
        }
    }
}
