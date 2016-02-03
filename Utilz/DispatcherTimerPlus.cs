using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Utilz
{
    public sealed class DispatcherTimerPlus : DispatcherTimer, IDisposable
    {
        private volatile DispatcherTimer _dispatcherTimer = null;
        private Action _action = null;
        private TimeSpan _interval = new TimeSpan(0, 0, 5);

        public DispatcherTimerPlus(Action action, int seconds)
        {
            _interval = new TimeSpan(0, 0, seconds);
            _action = action;
        }
        public void Dispose()
        {
            Stop();
        }

        public new void Start()
        {
            Stop();
            _dispatcherTimer = new DispatcherTimer() { Interval = _interval };
            AddHandlers();
            _dispatcherTimer.Start();
        }
        public new void Stop()
        {
            if (_dispatcherTimer != null) _dispatcherTimer.Stop();
            RemoveHandlers();
            _dispatcherTimer = null;
        }

        private bool _isHandlersActive = false;
        private void AddHandlers()
        {
            if (!_isHandlersActive && _dispatcherTimer != null)
            {
                _dispatcherTimer.Tick += OnAnimationTimer_Tick;
                _isHandlersActive = true;
            }
        }
        private void RemoveHandlers()
        {
            if (_dispatcherTimer != null)
            {
                _dispatcherTimer.Tick -= OnAnimationTimer_Tick;
                _isHandlersActive = false;
            }
        }
        private void OnAnimationTimer_Tick(object sender, object e)
        {
            _action?.Invoke();
        }
    }
}