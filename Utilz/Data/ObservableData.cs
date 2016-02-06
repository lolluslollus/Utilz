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
		protected async void RaisePropertyChanged_UI([CallerMemberName] string propertyName = "")
		{
			await RunInUiThreadAsync(delegate
			{
				RaisePropertyChanged(propertyName);
			}).ConfigureAwait(false);
		}
		protected async void RaisePropertyChangedUrgent_UI([CallerMemberName] string propertyName = "")
		{
			try
			{
				await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate
				{
					RaisePropertyChanged(propertyName);
				}).AsTask().ConfigureAwait(false);
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
	}
}