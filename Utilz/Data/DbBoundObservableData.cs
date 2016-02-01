using SQLite;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Utilz.Data
{
	[DataContract]
	public abstract class DbBoundObservableData : OpenableObservableDisposableData
	{
		public static readonly string DEFAULT_ID = string.Empty;

		#region properties
		// SQLite does not like a private set here
		protected string _id = DEFAULT_ID;
		[DataMember]
		[PrimaryKey]
		public string Id
		{
			get { return _id; }
			set
			{
				string newValue = value ?? DEFAULT_ID; if (_id != newValue) { _id = newValue; RaisePropertyChanged_UI(); /*Task upd = UpdateDbAsync();*/ }
			}
		}

		protected string _parentId = DEFAULT_ID;
		[DataMember]
		[Indexed(Unique = false)]
		public virtual string ParentId
		{
			get { return _parentId; }
			set
			{
				string newValue = value ?? DEFAULT_ID;
				SetProperty(ref _parentId, newValue);
				// if (_parentId != newValue) { _parentId = newValue; RaisePropertyChanged_UI(); /*Task upd = UpdateDbAsync();*/ }
			}
		}
		// LOLLO the following are various experiments with SetProperty
		// 
		// LOLLO TODO Atomicity bugs maybe? Also try inserting a big delay and see what happens. A semaphore may fix it.
		// see http://www.blackwasp.co.uk/Volatile.aspx for good volatile stuff.
		//
		//protected async void SetProperty1(object newValue, bool onlyIfDifferent = true, [CallerMemberName] string propertyName = "")
		//{
		//	string attributeName = '_' + propertyName[0].ToString().ToLower() + propertyName.Substring(1); // only works if naming conventions are respected
		//	var fieldInfo = GetType().GetField(attributeName);

		//	object oldValue = fieldInfo.GetValue(this);
		//	if (newValue != oldValue || !onlyIfDifferent)
		//	{
		//		fieldInfo.SetValue(this, newValue);

		//		await RunFunctionIfOpenAsyncA_MT(async delegate
		//		{
		//			if (UpdateDbMustOverride() == false)
		//			{
		//				fieldInfo.SetValue(this, oldValue);
		//				await Logger.AddAsync(GetType().ToString() + "." + propertyName + " could not be set", Logger.ForegroundLogFilename).ConfigureAwait(false);
		//			}
		//		});
		//		RaisePropertyChanged_UI(propertyName);
		//	}
		//}

		protected void SetProperty<T>(ref T fldValue, T newValue, bool onlyIfDifferent = true, [CallerMemberName] string propertyName = "")
		{
			T oldValue = fldValue;
			if (!newValue.Equals(oldValue) || !onlyIfDifferent)
			{
				fldValue = newValue;
				RaisePropertyChanged_UI(propertyName);

				Task db = RunFunctionIfOpenAsyncA_MT(async delegate
				{
					if (UpdateDbMustOverride() == false)
					{
						//	string attributeName = '_' + propertyName[0].ToString().ToLower() + propertyName.Substring(1); // only works if naming conventions are respected
						//	GetType().GetField(attributeName)?.SetValue(this, oldValue);
						//	RaisePropertyChanged_UI(propertyName);
						await Logger.AddAsync(GetType().ToString() + "." + propertyName + " could not be set", Logger.ForegroundLogFilename).ConfigureAwait(false);
					}
				});
			}
		}

		//protected Task SetProperty4(ref object fldValue, object newValue, bool onlyIfDifferent = true, [CallerMemberName] string propertyName = "")
		//{
		//	object oldValue = fldValue;
		//	if (newValue != oldValue || !onlyIfDifferent)
		//	{
		//		fldValue = newValue;

		//		if (_isOpen && _isEnabled)
		//		{
		//			try
		//			{
		//				_isOpenSemaphore.Wait(); //.ConfigureAwait(false);
		//				if (_isOpen && _isEnabled)
		//				{
		//					if (UpdateDbMustOverride() == false)
		//					{
		//						fldValue = oldValue;
		//					}
		//				}
		//			}
		//			catch (Exception ex)
		//			{
		//				if (SemaphoreSlimSafeRelease.IsAlive(_isOpenSemaphore))
		//					Logger.Add_TPL(ex.ToString(), Logger.ForegroundLogFilename);
		//			}
		//			finally
		//			{
		//				SemaphoreSlimSafeRelease.TryRelease(_isOpenSemaphore);
		//			}
		//		}
		//		RaisePropertyChanged_UI(propertyName);
		//	}
		//	return Task.CompletedTask;
		//}
		#endregion properties

		#region construct and dispose
		public DbBoundObservableData() : base()
		{
			_id = Guid.NewGuid().ToString(); // LOLLO copying Id from a DBIndex assigned by the DB is tempting,
											 // but it fails because DbIndex is only set when the record is put into the DB,
											 // which may be too late. So we get a GUID in the constructor, hoping it doesn't get too slow.
		}
		#endregion construct and dispose

		protected abstract bool CheckMeMustOverride();
		public static bool Check(DbBoundObservableData item)
		{
			if (item == null) return false;
			else return item.CheckMeMustOverride();
		}
		public static bool Check(IEnumerable<DbBoundObservableData> items)
		{
			if (items == null) return false;
			foreach (var item in items)
			{
				if (!item.CheckMeMustOverride()) return false;
			}
			return true;
		}

		protected abstract bool UpdateDbMustOverride();
	}
}