﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace Utilz.Controlz
{
	public abstract class ObservablePage : Page, INotifyPropertyChanged
	{
		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;
		protected void ClearListeners() // we can use this inside a Dispose
		{
			PropertyChanged = null;
		}
		protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		protected void RaisePropertyChanged_UI([CallerMemberName] string propertyName = "")
		{
			try
			{
				Task raise = RunInUiThreadAsync(delegate { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); });
			}
			catch (Exception ex)
			{
				Logger.Add_TPL(ex.ToString(), Logger.PersistentDataLogFilename);
			}
		}
		#endregion INotifyPropertyChanged


		#region construct dispose
		public ObservablePage() { }
		#endregion construct dispose


		#region UIThread
		protected async Task RunInUiThreadAsync(DispatchedHandler action)
		{
			if (Dispatcher.HasThreadAccess)
			{
				action();
			}
			else
			{
				await Dispatcher.RunAsync(CoreDispatcherPriority.Low, action).AsTask().ConfigureAwait(false);
			}
		}
		#endregion UIThread
	}
}