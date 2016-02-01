﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Utilz
{
	// LOLLO http://blog.stephencleary.com/2009/07/interpreting-notifycollectionchangedeve.html
	public class SwitchableObservableCollection<T> : ObservableCollection<T>
	{
		protected bool _isObserving = true;
		public bool IsObserving { get { return _isObserving; } set { _isObserving = value; } }

		protected uint _capacity = UInt32.MaxValue;
		public uint Capacity { get { return _capacity; } }

		public SwitchableObservableCollection() : base() { }
		public SwitchableObservableCollection(IEnumerable<T> collection) : base(collection) { }
		public SwitchableObservableCollection(uint capacity) : base() { _capacity = capacity; }
		public SwitchableObservableCollection(bool isObserving) : base() { _isObserving = isObserving; }
		public SwitchableObservableCollection(bool isObserving, IEnumerable<T> collection) : base(collection) { _isObserving = isObserving; }
		public SwitchableObservableCollection(bool isObserving, uint capacity) : base() { _isObserving = isObserving; _capacity = capacity; }


		public void AddRange(IEnumerable<T> range)
		{
			// get out if no new items
			if (range == null || range.Count() < 1) return;

			// prepare data for firing the events
			int newStartingIndex = Count;
			var newItems = new List<T>();
			newItems.AddRange(range);

			// add the items, making sure no events are fired
			_isObserving = false;
			foreach (var item in range)
			{
				Add(item);
			}
			_isObserving = true;

			// fire the events
			OnPropertyChanged(new PropertyChangedEventArgs("Count"));
			OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
			// this is tricky: call Reset first to make sure the controls will respond properly and not only add one item
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Reset));
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Add, changedItems: newItems, startingIndex: newStartingIndex));
		}

		public void ReplaceRange(IEnumerable<T> range)
		{
			// get out if no new items
			if (range == null || range.Count() < 1)
			{
				Clear();
			}
			else
			{
				// prepare data for firing the events
				int newStartingIndex = Count;
				var newItems = new List<T>();
				newItems.AddRange(range);

				// add the items, making sure no events are fired
				_isObserving = false;
				ClearItems();
				foreach (var item in range)
				{
					Add(item);
				}
				_isObserving = true;

				// fire the events
				OnPropertyChanged(new PropertyChangedEventArgs("Count"));
				OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
				// this is tricky: call Reset first to make sure the controls will respond properly and not only add one item
				// when clearing a collection, only .Reset is thrown!
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Reset));
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Add, changedItems: newItems, startingIndex: newStartingIndex));
			}
		}

		//public override event NotifyCollectionChangedEventHandler CollectionChanged;
		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			if (_isObserving) base.OnCollectionChanged(e); // this was all, it's smarter now

			//if (_isObserving)
			//{
			//	try
			//	{
			//		if (CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess)
			//		{
			//			CollectionChanged?.Invoke(this, e);
			//		}
			//		else
			//		{
			//			await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate
			//			{
			//				CollectionChanged?.Invoke(this, e);
			//			}).AsTask().ConfigureAwait(false);
			//		}
			//	}
			//	catch (Exception ex)
			//	{
			//		Logger.Add_TPL(ex.ToString(), Logger.PersistentDataLogFilename);
			//	}
			//}
		}
		protected override void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if (_isObserving) base.OnPropertyChanged(e);
		}
		protected override void InsertItem(int index, T item)
		{
			if (Count < _capacity) base.InsertItem(index, item);
			else throw new IndexOutOfRangeException("SwitchableObservableCollection has reached max capacity = " + Capacity + " items");
		}
	}
}