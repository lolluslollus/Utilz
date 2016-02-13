using System;
using Windows.UI.Xaml;

namespace Utilz
{
	public sealed class DispatcherTimerPlus : DispatcherTimer, IDisposable
	{
		private volatile DispatcherTimer _dispatcherTimer = null;
		private Action _action = null;
		private readonly TimeSpan _interval = new TimeSpan(0, 0, 5);

		public DispatcherTimerPlus(Action action, int seconds)
		{
			_interval = new TimeSpan(0, 0, seconds);
			_action = action;
		}
		public void Dispose()
		{
			Stop();
			_action = null;
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
			_dispatcherTimer?.Stop();
			RemoveHandlers();
			_dispatcherTimer = null;
		}

		private bool _isHandlersActive = false;
		private void AddHandlers()
		{
			var timer = _dispatcherTimer;
			if (_isHandlersActive || timer == null) return;

			_isHandlersActive = true;
			timer.Tick += OnAnimationTimer_Tick;
		}
		private void RemoveHandlers()
		{
			var timer = _dispatcherTimer;
			if (timer != null)
			{
				timer.Tick -= OnAnimationTimer_Tick;
			}
			_isHandlersActive = false;
		}
		private void OnAnimationTimer_Tick(object sender, object e)
		{
			_action?.Invoke();
		}
	}
}