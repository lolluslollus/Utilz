﻿using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
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
				string newValue = value ?? DEFAULT_ID; if (_id != newValue) { _id = newValue; }
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
				SetPropertyUpdatingDb(ref _parentId, value ?? DEFAULT_ID, false);
				// if (_parentId != newValue) { _parentId = newValue; RaisePropertyChanged_UI(); /*Task upd = UpdateDbAsync();*/ }
			}
		}
		// LOLLO the following are various experiments with SetProperty
		// 
		// see http://www.blackwasp.co.uk/Volatile.aspx for good volatile stuff.
		// and https://blogs.msdn.microsoft.com/ericlippert/2011/06/16/atomicity-volatility-and-immutability-are-different-part-three/
		// and http://www.albahari.com/threading/part2.aspx
		// and http://www.drdobbs.com/parallel/writing-lock-free-code-a-corrected-queue/210604448
		// 
		// my idea is: in general, use locks when writing and volatile when reading.
		// the trouble only applies to fields, which can be written to or read from by different threads at the same time.
		// MSDN is even more restrictive ( https://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k(volatile_CSharpKeyword);k(TargetFrameworkMoniker-.NETCore,Version%3Dv5.0);k(DevLang-csharp)&rd=true )
		// they say: The volatile keyword indicates that a field might be modified by multiple threads that are executing at the same time.
		// However, the simple blackwasp example above suggests that thread A can change a field and thread B will not see the new value unless it is volatile.
		// This is also some basic mt.
		// Anyway:
		// Atomicity, volatility and immutability are 3 very different things. One does not imply the other. Immutability even negates volatility.
		// Only atomic fields can be volatilised.
		// locks are atomic by construction.
		// volatile is apparently a very complex affair and must be understood thoroughly. Otherwise, better use locks, read/write locks etc.
		// but how can I put them into this generic SetProperty<T>() ? A simple answer could be the SetProperty<T> lower down.
		//
		protected void SetPropertyUpdatingDb<T>(ref T fldValue, T newValue, bool raise = true, bool onlyIfDifferent = true, [CallerMemberName] string propertyName = "")
		{
			T oldValue = fldValue;
			if (newValue.Equals(oldValue) && onlyIfDifferent) return;

			fldValue = newValue;
			if (raise) RaisePropertyChanged_UI(propertyName);

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

		protected void SetPropertyLockingUpdatingDb<T>(ref T fldValue, T newValue, object locker, bool raise = true, bool onlyIfDifferent = true, [CallerMemberName] string propertyName = "")
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
			if (!isValueChanged) return;

			if (raise) RaisePropertyChanged_UI(propertyName);

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

		// You can also try the following, or simply use return Volatile.Read in the property getters.
		// However, they seem slower than the volatile keyword on the private field by a factor of 3 to 5.
		//protected T GetProperty<T>(ref T fldValue) where T : class
		//{
		//	return Volatile.Read(ref fldValue);
		//}
		//protected bool GetProperty(ref bool fldValue)
		//{
		//	return Volatile.Read(ref fldValue);
		//}
		//protected int GetProperty(ref int fldValue)
		//{
		//	return Volatile.Read(ref fldValue);
		//}

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
		protected DbBoundObservableData() : base()
		{
			_id = Guid.NewGuid().ToString(); // LOLLO copying Id from a DBIndex assigned by the DB is tempting,
											 // but it fails because DbIndex is only set when the record is put into the DB,
											 // which may be too late. So we get a GUID in the constructor, hoping it doesn't get too slow.
		}
		#endregion construct and dispose

		protected abstract bool CheckMeMustOverride();
		public static bool Check(DbBoundObservableData item)
		{
			return item != null && item.CheckMeMustOverride();
		}

		public static bool Check(IEnumerable<DbBoundObservableData> items)
		{
			return items != null && items.All(item => item.CheckMeMustOverride());
		}

		protected abstract bool UpdateDbMustOverride();
	}
}