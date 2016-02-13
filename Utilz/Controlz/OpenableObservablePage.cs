using System;
using System.Threading;
using System.Threading.Tasks;
using Utilz.Data;
using Windows.ApplicationModel;
using Windows.Foundation.Metadata;
using Windows.Phone.UI.Input;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace Utilz.Controlz
{
	/// <summary>
	/// This is a smarter Page that can be opened and closed, asynchronously. 
	/// It will stay disabled as long as it is closed.
	/// Do not bind to IsEnabled, but to IsEnabledOverride instead.
	/// </summary>
	public abstract class OpenableObservablePage : ObservablePage, IOpenable
	{
		#region properties
		/// <summary>
		/// A registry key to store data when the page is suspended
		/// </summary>
		public string LastNavigatedPageRegKey
		{
			get { return (string)GetValue(LastNavigatedPageRegKeyProperty); }
			set { SetValue(LastNavigatedPageRegKeyProperty, value); }
		}
		public static readonly DependencyProperty LastNavigatedPageRegKeyProperty =
			DependencyProperty.Register("LastNavigatedPageRegKey", typeof(string), typeof(OpenableObservablePage), new PropertyMetadata(""));

		private bool _isOpenWhenSuspending = false;
		private bool _isOnMe = false;

		protected volatile SemaphoreSlimSafeRelease _isOpenSemaphore = null;

		protected volatile bool _isOpen = false;
		public bool IsOpen { get { return _isOpen; } protected set { if (_isOpen != value) { _isOpen = value; RaisePropertyChanged_UI(); } } }

		protected volatile bool _isEnabledAllowed = false;
		public bool IsEnabledAllowed
		{
			get { return _isEnabledAllowed; }
			protected set
			{
				if (_isEnabledAllowed == value) return;

				_isEnabledAllowed = value; RaisePropertyChanged_UI();
				Task upd = UpdateIsEnabledAsync();
			}
		}

		public bool IsEnabledOverride
		{
			get { return (bool)GetValue(IsEnabledOverrideProperty); }
			set { SetValue(IsEnabledOverrideProperty, value); }
		}
		public static readonly DependencyProperty IsEnabledOverrideProperty =
			DependencyProperty.Register("IsEnabledOverride", typeof(bool), typeof(OpenableObservablePage), new PropertyMetadata(true, OnIsEnabledOverrideChanged));
		private static void OnIsEnabledOverrideChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
		{
			Task upd = (obj as OpenableObservablePage)?.UpdateIsEnabledAsync();
		}
		private Task UpdateIsEnabledAsync()
		{
			return RunInUiThreadAsync(delegate
			{
				IsEnabled = IsEnabledAllowed && IsEnabledOverride;
			});
		}

		private readonly object _ctsLocker = new object();
		private SafeCancellationTokenSource _cts = null;
		//protected SafeCancellationTokenSource Cts
		//{
		//	get
		//	{
		//		lock (_ctsLocker) { return _cts; }
		//	}
		//}

		private CancellationToken _cancToken;
		protected CancellationToken CancToken
		{
			get
			{
				lock (_ctsLocker) { return _cancToken; }
			}
		}
		#endregion properties


		#region ctor
		protected OpenableObservablePage() : base()
		{
			Application.Current.Resuming += OnResuming;
			Application.Current.Suspending += OnSuspending;
			Task upd = UpdateIsEnabledAsync();
		}
		#endregion ctor


		#region event handlers
		private async void OnSuspending(object sender, SuspendingEventArgs e)
		{
			var deferral = e.SuspendingOperation.GetDeferral();

			_isOpenWhenSuspending = _isOnMe;
			if (_isOnMe) RegistryAccess.TrySetValue(LastNavigatedPageRegKey, GetType().Name);
			await CloseAsync().ConfigureAwait(false);

			deferral.Complete();
		}

		private async void OnResuming(object sender, object e)
		{
			if (_isOpenWhenSuspending) await OpenAsync().ConfigureAwait(false);
		}

		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			_isOnMe = true;
			await OpenAsync().ConfigureAwait(false);
		}

		protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
		{
			base.OnNavigatingFrom(e);
			_isOnMe = false;
			await CloseAsync().ConfigureAwait(false);
		}
		#endregion event handlers


		#region open close
		public async Task<bool> OpenAsync()
		{
			if (!_isOpen)
			{
				if (!SemaphoreSlimSafeRelease.IsAlive(_isOpenSemaphore)) _isOpenSemaphore = new SemaphoreSlimSafeRelease(1, 1);
				try
				{
					await _isOpenSemaphore.WaitAsync().ConfigureAwait(false);
					if (!_isOpen)
					{
						lock (_ctsLocker)
						{
							_cts?.Dispose();
							_cts = new SafeCancellationTokenSource();
							_cancToken = _cts.Token;
						}

						await OpenMayOverrideAsync().ConfigureAwait(false);

						IsOpen = true;
						IsEnabledAllowed = true;

						await RegisterBackEventHandlersAsync().ConfigureAwait(false);

						return true;
					}
				}
				catch (Exception ex)
				{
					if (SemaphoreSlimSafeRelease.IsAlive(_isOpenSemaphore))
						await Logger.AddAsync(GetType().Name + ex.ToString(), Logger.ForegroundLogFilename);
				}
				finally
				{
					SemaphoreSlimSafeRelease.TryRelease(_isOpenSemaphore);
				}
			}
			if (_isOpen) await SetIsEnabledAsync(true).ConfigureAwait(false);
			return false;
		}

		protected virtual Task OpenMayOverrideAsync()
		{
			return Task.CompletedTask; // avoid warning
		}

		public async Task<bool> CloseAsync()
		{
			if (!_isOpen) return false;

			lock (_ctsLocker)
			{
				_cts?.CancelSafe(true);
			}

			try
			{
				await _isOpenSemaphore.WaitAsync().ConfigureAwait(false);
				if (_isOpen)
				{
					lock (_ctsLocker)
					{
						_cts?.Dispose();
						_cts = null;
						_cancToken = new CancellationToken(true); // CancellationToken is not nullable and not disposable
					}

					await UnregisterBackEventHandlersAsync();

					IsEnabledAllowed = false;
					IsOpen = false;

					await CloseMayOverrideAsync().ConfigureAwait(false);
					return true;
				}
			}
			catch (Exception ex)
			{
				if (SemaphoreSlimSafeRelease.IsAlive(_isOpenSemaphore))
					await Logger.AddAsync(GetType().Name + ex.ToString(), Logger.ForegroundLogFilename);
			}
			finally
			{
				SemaphoreSlimSafeRelease.TryDispose(_isOpenSemaphore);
				_isOpenSemaphore = null;
			}
			return false;
		}

		protected virtual Task CloseMayOverrideAsync()
		{
			return Task.CompletedTask;
		}
		#endregion open close


		#region while open
		private async Task<bool> SetIsEnabledAsync(bool enable)
		{
			if (!_isOpen || IsEnabled == enable) return false;

			try
			{
				await _isOpenSemaphore.WaitAsync().ConfigureAwait(false);
				if (_isOpen && IsEnabled != enable)
				{
					IsEnabledAllowed = enable;
					return true;
				}
			}
			catch (Exception ex)
			{
				if (SemaphoreSlimSafeRelease.IsAlive(_isOpenSemaphore))
					await Logger.AddAsync(GetType().Name + ex.ToString(), Logger.ForegroundLogFilename);
			}
			finally
			{
				SemaphoreSlimSafeRelease.TryRelease(_isOpenSemaphore);
			}
			return false;
		}

		protected async Task<bool> RunFunctionIfOpenAsyncA(Action func)
		{
			if (!_isOpen) return false;

			try
			{
				await _isOpenSemaphore.WaitAsync(CancToken); //.ConfigureAwait(false);
				if (_isOpen)
				{
					func();
					return true;
				}
			}
			catch (OperationCanceledException) { }
			catch (Exception ex)
			{
				if (SemaphoreSlimSafeRelease.IsAlive(_isOpenSemaphore))
					await Logger.AddAsync(GetType().Name + ex.ToString(), Logger.ForegroundLogFilename);
			}
			finally
			{
				SemaphoreSlimSafeRelease.TryRelease(_isOpenSemaphore);
			}
			return false;
		}
		protected async Task<bool> RunFunctionIfOpenAsyncB(Func<bool> func)
		{
			if (!_isOpen) return false;

			try
			{
				await _isOpenSemaphore.WaitAsync(CancToken); //.ConfigureAwait(false);
				if (_isOpen) return func();
			}
			catch (OperationCanceledException) { }
			catch (Exception ex)
			{
				if (SemaphoreSlimSafeRelease.IsAlive(_isOpenSemaphore))
					await Logger.AddAsync(GetType().Name + ex.ToString(), Logger.ForegroundLogFilename);
			}
			finally
			{
				SemaphoreSlimSafeRelease.TryRelease(_isOpenSemaphore);
			}
			return false;
		}
		protected async Task<bool> RunFunctionIfOpenAsyncT(Func<Task> funcAsync)
		{
			if (!_isOpen) return false;

			try
			{
				await _isOpenSemaphore.WaitAsync(CancToken); //.ConfigureAwait(false);
				if (_isOpen)
				{
					await funcAsync().ConfigureAwait(false);
					return true;
				}
			}
			catch (OperationCanceledException) { }
			catch (Exception ex)
			{
				if (SemaphoreSlimSafeRelease.IsAlive(_isOpenSemaphore))
					await Logger.AddAsync(GetType().Name + ex.ToString(), Logger.ForegroundLogFilename);
			}
			finally
			{
				SemaphoreSlimSafeRelease.TryRelease(_isOpenSemaphore);
			}
			return false;
		}
		protected async Task<bool> RunFunctionIfOpenAsyncTB(Func<Task<bool>> funcAsync)
		{
			if (!_isOpen) return false;

			try
			{
				await _isOpenSemaphore.WaitAsync(CancToken); //.ConfigureAwait(false);
				if (_isOpen) return await funcAsync().ConfigureAwait(false);
			}
			catch (OperationCanceledException) { }
			catch (Exception ex)
			{
				if (SemaphoreSlimSafeRelease.IsAlive(_isOpenSemaphore))
					await Logger.AddAsync(GetType().Name + ex.ToString(), Logger.ForegroundLogFilename);
			}
			finally
			{
				SemaphoreSlimSafeRelease.TryRelease(_isOpenSemaphore);
			}
			return false;
		}
		#endregion while open


		#region back
		private volatile bool _isBackHandlersRegistered = false;
		private Task RegisterBackEventHandlersAsync()
		{
			return RunInUiThreadAsync(delegate
			{
				if (_isBackHandlersRegistered) return;

				_isBackHandlersRegistered = true;
				if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
				{
					HardwareButtons.BackPressed += OnHardwareButtons_BackPressed;
				}
				SystemNavigationManager.GetForCurrentView().BackRequested += OnTabletSoftwareButton_BackPressed;
			});
		}
		private Task UnregisterBackEventHandlersAsync()
		{
			return RunInUiThreadAsync(delegate
			{
				if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
				{
					HardwareButtons.BackPressed -= OnHardwareButtons_BackPressed;
				}
				SystemNavigationManager.GetForCurrentView().BackRequested -= OnTabletSoftwareButton_BackPressed;

				_isBackHandlersRegistered = false;
			});
		}

		private void OnHardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
		{
			if (!e.Handled) e.Handled = GoBackMayOverride();
		}
		private void OnTabletSoftwareButton_BackPressed(object sender, BackRequestedEventArgs e)
		{
			if (!e.Handled) e.Handled = GoBackMayOverride();
		}
		/// <summary>
		/// Deals with the back requested event and returns true if the event has been dealt with
		/// </summary>
		/// <returns></returns>
		protected virtual bool GoBackMayOverride()
		{
			return false;
		}
		#endregion back
	}
}