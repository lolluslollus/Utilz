using System;
using System.Collections.Specialized;


namespace Utilz
{
    // LOLLO http://blog.stephencleary.com/2009/07/interpreting-notifycollectionchangedeve.html
    public sealed class SwitchableObservableDisposableCollection<T> : SwitchableObservableCollection<T>, IDisposable
    {
		public override event NotifyCollectionChangedEventHandler CollectionChanged;
		private void ClearListeners()
		{
			CollectionChanged = null;
		}
		public void Dispose()
		{
			ClearListeners();
		}

		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
			if (_isObserving) CollectionChanged?.Invoke(this, e); // base.OnCollectionChanged(e); // this was all, it's smarter now

			//if (_isObserving) // LOLLO TODO check if we really need this in the UniFiler
			//{
			//    try
			//    {
			//        if (CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess)
			//        {
			//            CollectionChanged?.Invoke(this, e);
			//        }
			//        else
			//        {
			//            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate
			//            {
			//                CollectionChanged?.Invoke(this, e);
			//            }).AsTask().ConfigureAwait(false);
			//        }
			//    }
			//    catch (Exception ex)
			//    {
			//        Logger.Add_TPL(ex.ToString(), Logger.PersistentDataLogFilename);
			//    }
			//}
		}
    }
}