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
        public enum NavigationParameters { Launched, FileActivated }

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

        private static readonly object _isOnMeLocker = new object();
        private bool _isOnMe = false;
        protected bool IsOnMe { get { lock (_isOnMeLocker) { return _isOnMe; } } private set { lock (_isOnMeLocker) { _isOnMe = value; } } }

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
            var me = (obj as OpenableObservablePage);
            if (me == null) return;
            Task upd = me.UpdateIsEnabledAsync();
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
            var app = Application.Current as ISuspenderResumer;
            if (app != null)
            {
                app.ResumeStarted -= OnResuming;
                app.ResumeStarted += OnResuming;
                app.SuspendStarted -= OnSuspending;
                app.SuspendStarted += OnSuspending;
            }
            Task upd = UpdateIsEnabledAsync();
        }
        #endregion ctor


        #region event handlers        
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            try
            {
                if (IsOnMe) RegistryAccess.TrySetValue(LastNavigatedPageRegKey, GetType().Name);
                await CloseAsync(LifecycleEvents.Suspending).ConfigureAwait(false);
            }
            finally
            {
                deferral.Complete();
            }
        }

        private async void OnResuming(object sender, object e)
        {
            if (!IsOnMe) return;
            await OpenAsync(LifecycleEvents.Resuming).ConfigureAwait(false);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            IsOnMe = true;
            bool isAfterFileActivated = e.Parameter != null && (NavigationParameters)(e.Parameter) == NavigationParameters.FileActivated;
            Task open = isAfterFileActivated ? OpenAsync(LifecycleEvents.NavigatedToAfterFileActivated) : OpenAsync(LifecycleEvents.NavigatedToAfterLaunch);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            IsOnMe = false;
            base.OnNavigatingFrom(e);
            Task close = CloseAsync(LifecycleEvents.NavigatingFrom);
        }
        #endregion event handlers


        #region open close
        public async Task<bool> OpenAsync(object args = null)
        {
            if (_isOpen) return await RunFunctionIfOpenAsyncA(delegate { IsEnabledAllowed = true; }).ConfigureAwait(false);

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

                    await OpenMayOverrideAsync(args).ConfigureAwait(false);

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

            return false;
        }

        protected virtual Task OpenMayOverrideAsync(object args = null)
        {
            return Task.CompletedTask; // avoid warning
        }

        public async Task<bool> CloseAsync(object args = null)
        {
            if (!_isOpen) return await RunFunctionIfOpenAsyncA(delegate { IsEnabledAllowed = false; }).ConfigureAwait(false);

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

                    await CloseMayOverrideAsync(args).ConfigureAwait(false);
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

        protected virtual Task CloseMayOverrideAsync(object args = null)
        {
            return Task.CompletedTask;
        }
        #endregion open close


        #region while open
        //private async Task<bool> SetIsEnabledAsync(bool enable)
        //{
        //    if (!_isOpen || IsEnabled == enable) return false;

        //    try
        //    {
        //        await _isOpenSemaphore.WaitAsync(); //.ConfigureAwait(false);
        //        if (_isOpen && IsEnabled != enable)
        //        {
        //            IsEnabledAllowed = enable;
        //            return true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        if (SemaphoreSlimSafeRelease.IsAlive(_isOpenSemaphore))
        //            await Logger.AddAsync(GetType().Name + ex.ToString(), Logger.ForegroundLogFilename);
        //    }
        //    finally
        //    {
        //        SemaphoreSlimSafeRelease.TryRelease(_isOpenSemaphore);
        //    }
        //    return false;
        //}

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
                var naviManager = SystemNavigationManager.GetForCurrentView();
                if (naviManager != null) naviManager.BackRequested += OnTabletSoftwareButton_BackPressed;
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
                var naviManager = SystemNavigationManager.GetForCurrentView();
                if (naviManager != null) naviManager.BackRequested -= OnTabletSoftwareButton_BackPressed;

                _isBackHandlersRegistered = false;
            });
        }

        protected virtual void OnHardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            if (!e.Handled) e.Handled = GoBackMayOverride();
        }
        protected virtual void OnTabletSoftwareButton_BackPressed(object sender, BackRequestedEventArgs e)
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