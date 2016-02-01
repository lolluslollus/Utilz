using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace Utilz
{
    public class ProportionsTrigger : StateTriggerBase
    {
        private volatile ApplicationView _appView = null;
        //private SimpleOrientationSensor _orientationSensor;

        private FrameworkElement _targetElement;
        public FrameworkElement TargetElement
        {
            get
            {
                return _targetElement;
            }
            set
            {
                _targetElement = value;
                Task add = AddHandlersAsync();
            }
        }

        private volatile bool _isHandlersActive = false;
        private async Task AddHandlersAsync()
        {
            //if (_orientationSensor == null) _orientationSensor = SimpleOrientationSensor.GetDefault();
            if (_appView == null) _appView = ApplicationView.GetForCurrentView();
            if (!_isHandlersActive /*&& _orientationSensor != null */ && _appView != null)
            {
                //_orientationSensor.OrientationChanged += OnSensor_OrientationChanged;
                _appView.VisibleBoundsChanged += OnVisibleBoundsChanged;
				await UpdateTriggerAsync(_appView.VisibleBounds.Width < _appView.VisibleBounds.Height).ConfigureAwait(false);
				_isHandlersActive = true;
            }
        }

        private void RemoveHandlers()
        {
            //if (_orientationSensor != null) _orientationSensor.OrientationChanged -= OnSensor_OrientationChanged;
            if (_appView != null) _appView.VisibleBoundsChanged -= OnVisibleBoundsChanged;
            _isHandlersActive = false;
        }

        private async Task UpdateTriggerAsync(bool newValue)
        {
            if (_targetElement != null)
            {
                bool newValue_mt = newValue;
				await RunInUiThreadAsync(_targetElement.Dispatcher, delegate
                {
                    SetActive(newValue_mt);
                }).ConfigureAwait(false);
            }
            else
            {
                SetActive(false);
            }
        }

        private Rect? _lastVisibleBounds = null;
        private async void OnVisibleBoundsChanged(ApplicationView sender, object args)
        {
            if (_lastVisibleBounds == null || _appView.VisibleBounds.Height != _lastVisibleBounds?.Height || _appView.VisibleBounds.Width != _lastVisibleBounds?.Width)
            {
                await UpdateTriggerAsync(_appView.VisibleBounds.Width < _appView.VisibleBounds.Height).ConfigureAwait(false);
            }
            _lastVisibleBounds = _appView.VisibleBounds;
        }

		private async Task RunInUiThreadAsync(CoreDispatcher dispatcher, DispatchedHandler action)
		{
			if (dispatcher.HasThreadAccess)
			{
				action();
			}
			else
			{
				await dispatcher.RunAsync(CoreDispatcherPriority.Normal, action).AsTask().ConfigureAwait(false);
			}
		}
	}
}
