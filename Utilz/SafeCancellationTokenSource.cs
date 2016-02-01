using System;
using System.Diagnostics;
using System.Threading;

namespace Utilz
{
	public class SafeCancellationTokenSource : CancellationTokenSource
	{
		private readonly object _lock = new object();
		private volatile bool _isDisposed = false;
		public bool IsDisposed { get { return _isDisposed; } }

		protected override void Dispose(bool disposing)
		{
			try
			{
				lock (_lock)
				{
					_isDisposed = true;
					base.Dispose(disposing);
				}
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
				lock (_lock)
				{
					if (!_isDisposed) Cancel(throwOnFirstException);
				}
			}
			catch (OperationCanceledException) { } // maniman
			catch (ObjectDisposedException) { }
			catch (Exception ex)
			{
				Logger.Add_TPL(ex.ToString(), Logger.ForegroundLogFilename);
			}
		}

		private bool IsCancellationRequestedSafe
		{
			get
			{
				try
				{
					lock (_lock)
					{
						if (!_isDisposed) return Token.IsCancellationRequested;
						else return true;
					}
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

		//private CancellationToken TokenSafe
		//{
		//	get
		//	{
		//		try
		//		{
		//			lock (_lock)
		//			{
		//				if (!_isDisposed) return Token;
		//				else return new CancellationToken(true);
		//			}
		//		}
		//		catch (Exception ex)
		//		{
		//			Logger.Add_TPL(ex.ToString(), Logger.ForegroundLogFilename);
		//			return new CancellationToken(true);
		//		}
		//	}
		//}

		/// <summary>
		/// Returns true if the given token is null, disposed or has a cancelled token, false otherwise.
		/// </summary>
		/// <param name="cts"></param>
		/// <returns></returns>
		public static bool IsNullOrCancellationRequestedSafe(SafeCancellationTokenSource cts)
		{
			if (cts?.IsCancellationRequestedSafe == false) return false;
			else return true;
		}

		/// <summary>
		/// Returns true if the given token is null, disposed or has a cancelled token, false otherwise.
		/// </summary>
		/// <param name="cts"></param>
		/// <returns></returns>
		public static bool IsNotNullAndCancellationRequestedSafe(SafeCancellationTokenSource cts)
		{
			if (cts?.IsCancellationRequestedSafe == true) return true;
			else return false;
		}

		/// <summary>
		/// Always gets a CancellationToken, even if the given token is disposed.
		/// In this case, the token will be cancelled if cancelTokenIfCtsNull is true, otherwise not.
		/// </summary>
		/// <param name="cts"></param>
		/// <param name="cancelTokenIfCtsNull"></param>
		/// <returns></returns>
		public static CancellationToken GetCancellationTokenSafe(SafeCancellationTokenSource cts, bool cancelTokenIfCtsNull = true)
		{
			//try
			//{
			//	var lcts = cts;
			//	if (lcts?.IsDisposed == false) return lcts.TokenSafe;
			//	else return new CancellationToken(cancelTokenIfCtsNull);
			//}
			//catch (Exception ex)
			//{
			//	Logger.Add_TPL(ex.ToString(), Logger.ForegroundLogFilename);
			//	return new CancellationToken(true);
			//}

			try
			{
				var lcts = cts;
				/// Stopwatch sw0 = new Stopwatch(); sw0.Start();
				lock (cts._lock) // this lock takes 0 msec and up to 400 ticks on x86 in debug mode
				{
					//sw0.Stop();
					//Debug.WriteLine("msec to acquire lock = " + sw0.ElapsedMilliseconds);
					//Debug.WriteLine("ticks to acquire lock = " + sw0.ElapsedTicks);
					if (lcts == null) return new CancellationToken(cancelTokenIfCtsNull);
					if (!lcts._isDisposed) return lcts.Token;
					return new CancellationToken(cancelTokenIfCtsNull);
				}
			}
			catch (Exception ex)
			{
				Logger.Add_TPL(ex.ToString(), Logger.ForegroundLogFilename);
				return new CancellationToken(true);
			}
		}
	}
}