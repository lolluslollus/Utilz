using System;
using System.Threading.Tasks;
using Utilz.Data;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace Utilz.Controlz
{
	public abstract class BackOrientOpenObservControl : OpenableObservableControl, IOpenable
	{
		#region lifecycle
		public BackOrientOpenObservControl() : base()
		{
			_appView = ApplicationView.GetForCurrentView();
			//_orientationSensor = SimpleOrientationSensor.GetDefault();
			//if (_orientationSensor != null) { _lastOrientation = _orientationSensor.GetCurrentOrientation(); }
			UseLayoutRounding = true;
			Loaded += OnLoaded;
			Unloaded += OnUnloaded;
		}

		private async void OnLoaded(object sender, RoutedEventArgs e)
		{
			AddAppViewHandlers();
			AddBackHandlers();

			await OnLoadedMayOverrideAsync();

			OnVisibleBoundsChangedMayOverride(_appView, null);
		}
		protected virtual Task OnLoadedMayOverrideAsync() { return Task.CompletedTask; }
		private async void OnUnloaded(object sender, RoutedEventArgs e)
		{
			RemoveAppViewHandlers();
			RemoveBackHandlers();

			await OnUnloadedMayOverrideAsync();
		}
		protected virtual Task OnUnloadedMayOverrideAsync() { return Task.CompletedTask; }
		#endregion lifecycle


		#region appView
		private ApplicationView _appView = null;
		public ApplicationView AppView { get { return _appView; } }

		private volatile bool _isAppViewHandlersActive = false;
		private void AddAppViewHandlers()
		{
			var av = _appView;
			if (_isAppViewHandlersActive == false && av != null)
			{
				_isAppViewHandlersActive = true;
				av.VisibleBoundsChanged += OnVisibleBoundsChangedMayOverride;
			}
		}

		private void RemoveAppViewHandlers()
		{
			var av = _appView;
			if (av != null) av.VisibleBoundsChanged -= OnVisibleBoundsChangedMayOverride;
			_isAppViewHandlersActive = false;
		}

		protected virtual void OnVisibleBoundsChangedMayOverride(ApplicationView sender, object args) { }
		#endregion appView


		#region goBack
		private volatile bool _isBackHandlersActive = false;
		private void AddBackHandlers()
		{
			var bpr = BackPressedRaiser;
			if (_isBackHandlersActive == false && bpr != null)
			{
				_isBackHandlersActive = true;
				bpr.BackOrHardSoftKeyPressed += OnHardwareOrSoftwareButtons_BackPressed_MayOverride;
			}
		}

		private void RemoveBackHandlers()
		{
			RemoveBackHandlers(BackPressedRaiser);
		}

		private void RemoveBackHandlers(IBackPressedRaiser bpr)
		{
			if (bpr != null) bpr.BackOrHardSoftKeyPressed -= OnHardwareOrSoftwareButtons_BackPressed_MayOverride;
			_isBackHandlersActive = false;
		}
		
		protected virtual void OnHardwareOrSoftwareButtons_BackPressed_MayOverride(object sender, BackOrHardSoftKeyPressedEventArgs e) { }

		public IBackPressedRaiser BackPressedRaiser
		{
			get { return (IBackPressedRaiser)GetValue(BackPressedRaiserProperty); }
			set { SetValue(BackPressedRaiserProperty, value); }
		}
		public static readonly DependencyProperty BackPressedRaiserProperty =
			DependencyProperty.Register("BackPressedRaiser", typeof(IBackPressedRaiser), typeof(BackOrientOpenObservControl), new PropertyMetadata(null, OnBackPressedRaiserChanged));
		private static void OnBackPressedRaiserChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
		{
			var instance = obj as BackOrientOpenObservControl;
			instance.RemoveBackHandlers(args.OldValue as IBackPressedRaiser);
			instance.AddBackHandlers();
		}
		#endregion goBack


		// the following works but we don't need it
		//#region sensor
		//private SimpleOrientationSensor _orientationSensor;
		//private SimpleOrientation _lastOrientation;
		//protected virtual void OnSensor_OrientationChanged(SimpleOrientationSensor sender, SimpleOrientationSensorOrientationChangedEventArgs args)
		//{
		//    if (_lastOrientation == null || args.Orientation != _lastOrientation)
		//    {
		//        bool mustRaise = false; // LOLLO check this if you really want to use this code
		//        switch (args.Orientation)
		//        {
		//            case SimpleOrientation.Facedown:
		//                break;
		//            case SimpleOrientation.Faceup:
		//                break;
		//            case SimpleOrientation.NotRotated:
		//                break;
		//            case SimpleOrientation.Rotated180DegreesCounterclockwise:
		//                mustRaise = true; break;
		//            case SimpleOrientation.Rotated270DegreesCounterclockwise:
		//                mustRaise = true; break;
		//            case SimpleOrientation.Rotated90DegreesCounterclockwise:
		//                mustRaise = true; break;
		//            default:
		//                break;
		//        }
		//        _lastOrientation = args.Orientation;
		//        if (mustRaise) RaiseOrientationChanged(args);
		//    }
		//}
		///// <summary>
		///// Raised when the orientation changes, even if rotation for the app is disabled
		///// </summary>
		//public event TypedEventHandler<SimpleOrientationSensor, SimpleOrientationSensorOrientationChangedEventArgs> OrientationChanged;
		//private void RaiseOrientationChanged(SimpleOrientationSensorOrientationChangedEventArgs args)
		//{
		//    var listener = OrientationChanged;
		//    if (listener != null)
		//    {
		//        listener(_orientationSensor, args);
		//    }
		//}
		//#endregion sensor
	}

	/// <summary>
	/// Only define one IBackPressedRaiser each page.
	/// The controls within will take it as a dependency property and respond to its events.
	/// </summary>
	public interface IBackPressedRaiser
	{
		event EventHandler<BackOrHardSoftKeyPressedEventArgs> BackOrHardSoftKeyPressed;
	}
	public class BackOrHardSoftKeyPressedEventArgs : EventArgs
	{
		public bool Handled { get; set; }
	}
}