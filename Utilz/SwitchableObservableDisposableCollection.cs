using System;
using System.Collections.Specialized;
using Windows.ApplicationModel.Core;

namespace Utilz
{
	// LOLLO http://blog.stephencleary.com/2009/07/interpreting-notifycollectionchangedeve.html
	public sealed class SwitchableObservableDisposableCollection<T> : SwitchableObservableCollection<T>, IDisposable
	{
		private volatile bool _isDisposed = false;
		public bool IsDisposed { get { return _isDisposed; } }
		private void ClearListeners()
		{
			CollectionChanged = null;
		}
		public void Dispose()
		{
			_isDisposed = true;
			ClearListeners();
		}

		public override event NotifyCollectionChangedEventHandler CollectionChanged;

		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
		{
			// bool hta = CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess; // remove when done testing
			if (_isObserving) CollectionChanged?.Invoke(this, args); // base.OnCollectionChanged(e); NO!
		}
	}
}