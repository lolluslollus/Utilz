using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utilz
{
	public class SafeCancellationTokenSource : CancellationTokenSource
	{
		private volatile bool _isDisposed = false;
		public bool IsDisposed { get { return _isDisposed; } }

		protected override void Dispose(bool disposing)
		{
			_isDisposed = true;
			try
			{
				base.Dispose(disposing);
			}
			catch (Exception ex)
			{
				Logger.Add_TPL(ex.ToString(), Logger.ForegroundLogFilename);
			}
		}

		public void CancelSafe(bool throwOnFirstException = false)
		{
			try
			{
				if (!_isDisposed) Cancel(throwOnFirstException);
			}
			catch (OperationCanceledException) { } // maniman
			catch (ObjectDisposedException) { }
			catch (Exception ex)
			{
				Logger.Add_TPL(ex.ToString(), Logger.ForegroundLogFilename);
			}
		}

		public bool IsCancellationRequestedSafe
		{
			get
			{
				try
				{
					if (!_isDisposed) return Token.IsCancellationRequested;
					else return true;
				}
				catch (OperationCanceledException) // maniman
				{
					return true;
				}
				catch (ObjectDisposedException)
				{
					return true;
				}
				catch (Exception ex)
				{
					Logger.Add_TPL(ex.ToString(), Logger.ForegroundLogFilename);
					return true;
				}
			}
		}
		/// <summary>
		/// Always gets a CancellationToken, even if the object is disposed.
		/// In this case, the token will say "operation cancelled".
		/// </summary>
		public CancellationToken TokenSafe
		{
			get
			{
				try
				{
					if (!_isDisposed) return Token;
					else return new CancellationToken(true);
				}
				catch (Exception ex)
				{
					Logger.Add_TPL(ex.ToString(), Logger.ForegroundLogFilename);
					return new CancellationToken(true);
				}
			}
		}
	}

	//public static class SafeCancellationTokenSourceExtensions
	//{
	//	public static bool IsAlive(this SafeCancellationTokenSource cts)
	//	{
	//		var lcts = cts;
	//		return (lcts != null && !lcts.IsDisposed);
	//	}
	//}
}