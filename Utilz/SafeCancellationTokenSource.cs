using System;
using System.Diagnostics;
using System.Threading;

namespace Utilz
{
	public sealed class SafeCancellationTokenSource : CancellationTokenSource
	{
		private volatile bool _isDisposed = false;
		public bool IsDisposed { get { return _isDisposed; } }

		protected override void Dispose(bool disposing)
		{
			try
			{
				_isDisposed = true;
				base.Dispose(disposing);
			}
			catch (Exception ex)
			{
				Logger.Add_TPL(ex.ToString(), Logger.ForegroundLogFilename);
			}
		}

		public void CancelSafe(bool throwOnFirstException)
		{
			try
			{
				if (!_isDisposed) Cancel(throwOnFirstException);
			}
			catch (OperationCanceledException) { } // maniman
			catch (ObjectDisposedException) { }
			catch (AggregateException) { }
			catch (Exception ex)
			{
				Logger.Add_TPL(ex.ToString(), Logger.ForegroundLogFilename);
			}
		}
	}
}