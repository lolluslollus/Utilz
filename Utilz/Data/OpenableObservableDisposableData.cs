using SQLite;
using System;
using System.Diagnostics;
using System.Runtime.Serialization;


namespace Utilz.Data
{
	[DataContract]
	public abstract class OpenableObservableDisposableData : OpenableObservableData, IDisposable, IOpenable
	{
		#region properties
		protected volatile bool _isDisposed = false;
		[IgnoreDataMember]
		[Ignore]
		public bool IsDisposed { get { return _isDisposed; } protected set { if (_isDisposed != value) { _isDisposed = value; } } }
		#endregion properties


		#region IDisposable
		public void Dispose()
		{
			Dispose(true);
		}
		protected virtual void Dispose(bool isDisposing)
		{
			_isDisposed = true;
			CloseAsync().Wait();
			ClearListeners();
		}
		#endregion IDisposable
	}
}