using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace Utilz.Data
{
    [DataContract]
    public abstract class UIThreadAware
    {
        protected static bool? GetHasThreadAccess()
        {
            try
            {
                return CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess;
            }
            catch
            {
                return null;
            }
        }
        protected static async Task RunInUiThreadIdleAsync(DispatchedHandler action)
        {
            bool? hasThreadAccess = GetHasThreadAccess();
            if (hasThreadAccess == true)
            {
                action();
            }
            else if (hasThreadAccess == false)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunIdleAsync(a => action()).AsTask().ConfigureAwait(false);
            }
        }
        protected static async Task RunInUiThreadAsync(DispatchedHandler action)
        {
            bool? hasThreadAccess = GetHasThreadAccess();
            if (hasThreadAccess == true)
            {
                action();
            }
            else if (hasThreadAccess == false)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Low, action).AsTask().ConfigureAwait(false);
            }
        }
        protected static async Task RunInUiThreadAsyncT(Func<Task> action)
        {
            if (action == null) return;

            Task task = Task.CompletedTask;

            bool? hasThreadAccess = GetHasThreadAccess();
            if (hasThreadAccess == true)
            {
                task = action();
            }
            else if (hasThreadAccess == false)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                    CoreDispatcherPriority.Low,
                    () => task = action()
                ).AsTask().ConfigureAwait(false);
            }
            else return;

            await task.ConfigureAwait(false);
        }
        protected static async Task<TResult> RunInUiThreadAsyncTT<TResult>(Func<Task<TResult>> func)
        {
            if (func == null) return default(TResult);

            Task<TResult> task = null;

            bool? hasThreadAccess = GetHasThreadAccess();
            if (hasThreadAccess == true)
            {
                task = func();
            }
            else if (hasThreadAccess == false)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                    CoreDispatcherPriority.Low,
                    () => task = func()
                ).AsTask().ConfigureAwait(false);
            }
            else return default(TResult);

            return await task.ConfigureAwait(false);
        }
    }

    [DataContract]
    public abstract class ObservableData : UIThreadAware, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected bool IsAnyoneListening()
        {
            var pc = PropertyChanged;
            if (pc == null) return false;

            // LOLLO NOTE this test code may slow down the app unnecessarily, but it's useful for testing
            //try
            //{
            //    string invokers = string.Empty;
            //    var invocationList = pc.GetInvocationList();
            //    foreach (var item in invocationList)
            //    {
            //        invokers += item.Target.GetType().Name;
            //        invokers += Environment.NewLine;
            //    }
            //    string invocationListLength = invocationList != null ? invocationList.Length.ToString() : "CANNOT SAY";
            //    Logger.Add_TPL($"ObservableData.IsAnyoneListening found {invocationListLength} listeners: {invokers}", Logger.AppEventsLogFilename, Logger.Severity.Info, false);
            //}
            //catch { }

            return pc != null;
        }
        protected void ClearListeners()
        {
            PropertyChanged = null;
        }

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Runs in the UI thread if available, otherwise queues the operation in it.
        /// </summary>
        /// <param name="propertyName"></param>
        protected void RaisePropertyChanged_UI([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged == null) return;
            Task raise = RunInUiThreadAsync(delegate
            {
                RaisePropertyChanged(propertyName);
            });
        }
        //protected void RaisePropertyChangedUrgent_UI([CallerMemberName] string propertyName = "")
        //{
        //    if (PropertyChanged == null) return;
        //    try
        //    {
        //        Task raise = CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate
        //        {
        //            RaisePropertyChanged(propertyName);
        //        }).AsTask();
        //    }
        //    catch (InvalidOperationException) // called from a background task: ignore
        //    { }
        //    catch (Exception ex)
        //    {
        //        Logger.Add_TPL(ex.ToString(), Logger.PersistentDataLogFilename);
        //    }
        //}
        #endregion INotifyPropertyChanged


        #region get and set
        protected T GetPropertyLocking<T>(ref T fldValue, object locker)
        {
            lock (locker)
            {
                return fldValue;
            }
        }
        protected T GetProperty<T>(ref T fldValue)
        {
            return fldValue;
        }
        protected void SetPropertyLocking<T>(ref T fldValue, T newValue, object locker, bool raise = true, bool onlyIfDifferent = true, [CallerMemberName] string propertyName = "")
        {
            bool isValueChanged = false;
            lock (locker)
            {
                T oldValue = fldValue;
                if (!newValue.Equals(oldValue) || !onlyIfDifferent)
                {
                    fldValue = newValue;
                    isValueChanged = true;
                }
            }
            // separate to avoid deadlocks
            if (isValueChanged && raise)
            {
                RaisePropertyChanged_UI(propertyName);
            }
        }
        protected void SetProperty<T>(ref T fldValue, T newValue, bool raise = true, bool onlyIfDifferent = true, [CallerMemberName] string propertyName = "")
        {
            T oldValue = fldValue;
            if (newValue.Equals(oldValue) && onlyIfDifferent) return;

            fldValue = newValue;
            if (raise) RaisePropertyChanged_UI(propertyName);
        }
        #endregion get and set
    }
}