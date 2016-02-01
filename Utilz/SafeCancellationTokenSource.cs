using System;
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

		public void CancelSafe(bool throwOnFirstException = false)
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

		public bool IsCancellationRequestedSafe
		{
			get
			{
				lock (_lock)
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
		}
		/// <summary>
		/// Always gets a CancellationToken, even if the object is disposed.
		/// In this case, the token will say "operation cancelled".
		/// </summary>
		public CancellationToken TokenSafe
		{
			get
			{
				lock (_lock)
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

		public static bool IsNullOrCancellationRequestedSafe(SafeCancellationTokenSource cts)
		{
			var lcts = cts;
			if (lcts == null || lcts.IsCancellationRequestedSafe) return true;
			else return false;
		}

		public static CancellationToken GetCancellationTokenSafe(SafeCancellationTokenSource cts, bool cancelTokenIfCtsNull = true)
		{
			try
			{
				var lcts = cts;
				if (lcts?.IsDisposed == false) return lcts.TokenSafe;
				else return new CancellationToken(cancelTokenIfCtsNull);
			}
			catch (Exception ex)
			{
				Logger.Add_TPL(ex.ToString(), Logger.ForegroundLogFilename);
				return new CancellationToken(true);
			}
		}
	}
}