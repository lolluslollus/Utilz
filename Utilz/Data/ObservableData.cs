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
	public abstract class ObservableData : INotifyPropertyChanged
	{
		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;
        protected bool IsAnyoneListening()
        {
            return PropertyChanged != null;
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
		protected void RaisePropertyChangedUrgent_UI([CallerMemberName] string propertyName = "")
		{
			if (PropertyChanged == null) return;
			try
			{
				Task raise = CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate
				{
					RaisePropertyChanged(propertyName);
				}).AsTask();
			}
			catch (InvalidOperationException) // called from a background task: ignore
			{ }
			catch (Exception ex)
			{
				Logger.Add_TPL(ex.ToString(), Logger.PersistentDataLogFilename);
			}
		}
		#endregion INotifyPropertyChanged


		#region UIThread
		protected async Task RunInUiThreadIdleAsync(DispatchedHandler action)
		{
			try
			{
				if (CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess)
				{
					action();
				}
				else
				{
					await CoreApplication.MainView.CoreWindow.Dispatcher.RunIdleAsync(a => action()).AsTask().ConfigureAwait(false);
				}
			}
			catch (InvalidOperationException) // called from a background task: ignore
			{ }
			catch (Exception ex)
			{
				Logger.Add_TPL(ex.ToString(), Logger.PersistentDataLogFilename);
			}
		}
		protected async Task RunInUiThreadAsync(DispatchedHandler action)
		{
			try
			{
				if (CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess)
				{
					action();
				}
				else
				{
					await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Low, action).AsTask().ConfigureAwait(false);
				}
			}
			catch (InvalidOperationException) // called from a background task: ignore
			{ }
			catch (Exception ex)
			{
				Logger.Add_TPL(ex.ToString(), Logger.PersistentDataLogFilename);
			}
		}
		#endregion UIThread


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