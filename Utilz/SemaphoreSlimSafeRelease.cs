using System;
using System.Threading;

namespace Utilz
{
	public sealed class SemaphoreSlimSafeRelease : SemaphoreSlim
	{
		//private const int MAX_COUNT_ABSURD = -1;
		private volatile bool _isDisposed = false;
		public bool IsDisposed { get { return _isDisposed; } }

		//private int _maxCount = MAX_COUNT_ABSURD;

		public SemaphoreSlimSafeRelease(int initialCount) : base(initialCount) { }
		public SemaphoreSlimSafeRelease(int initialCount, int maxCount) : base(initialCount, maxCount)
		{
			//_maxCount = maxCount;
		}

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed) return;

			_isDisposed = true;
			try
			{
				base.Dispose(disposing);
			}
			catch (ObjectDisposedException) { } // fires when I dispose sema and have not rector'd it while the current thread is inside it
			catch (SemaphoreFullException) { } // fires when I dispose sema and rector it while the current thread is inside it
			catch (Exception) { }
		}
		/// <summary>
		/// Returns true if a SemaphoreSlimSafeRelease is not null and not disposed.
		/// </summary>
		/// <param name="semaphore"></param>
		/// <returns></returns>
		public static bool IsAlive(SemaphoreSlimSafeRelease semaphore)
		{
			return semaphore != null && !semaphore._isDisposed;
		}
		public static bool TryRelease(SemaphoreSlimSafeRelease semaphore)
		{
			try
			{
				// the following commented out stuff wouldbe nice to avoid the exception but it is not atomic, so we leave it.
				if (IsAlive(semaphore) /*&& semaphore.CurrentCount < semaphore._maxCount*/)
				{
					semaphore.Release();
					return true;
				}
			}
			catch { }
			return false;
		}
		public static bool TryRelease(SemaphoreSlimSafeRelease semaphore, int releaseCount)
		{
			try
			{
				if (IsAlive(semaphore))
				{
					semaphore.Release(releaseCount);
					return true;
				}
			}
			catch { }
			return false;
		}

		/// <summary>
		/// Disposes of a semaphore.
		/// You can use this method to interrupt all operations, which are queuing at the semaphore, if you initialised it for max 1 thread.
		/// Wait at the semaphore before calling this method, to be sure you don't disrupt any running operations at an unexpected moment.
		/// Every method using this semaphore should make allowance for it being disposed.
		/// </summary>
		/// <example> 
		/// Use it as follows:
		/// <code>
		/// try
		/// {
		///     semaphore.Wait();
		///     do some work
		/// }
		/// catch (Exception ex)
		/// {
		///     if (SemaphoreSlimSafeRelease.IsAlive(semaphore))
		///         // you have a real error, otherwise it's simply the semaphore being disposed and reset to null
		/// }
		/// finally
		/// {
		///     SemaphoreSlimSafeRelease.TryRelease(semaphore);
		///     // do not just call semaphore.Release() because the semaphore may be disposed or null at this stage
		/// }
		/// </code>
		/// </example>
		/// <param name="semaphore"></param>
		/// <returns></returns>
		public static bool TryDispose(SemaphoreSlimSafeRelease semaphore)
		{
			try
			{
				if (IsAlive(semaphore))
				{
					semaphore.Dispose();
					return true;
				}
			}
			catch { }
			return false;
		}
	}

	public static class SemaphoreExtensions
	{
		public static bool TryRelease(this Semaphore semaphore)
		{
			try
			{
				semaphore?.Release();
				return true;
			}
			catch { }
			return false;
		}
		/// <summary>
		/// LOLLO Disposes of a semaphore.
		/// You can use this method to interrupt all operations, which are queuing at the semaphore, if you initialised it for max 1 thread.
		/// Wait at the semaphore before calling this method, to be sure you don't disrupt any running operations at an unexpected moment.
		/// Every method using this semaphore should make allowance for it being disposed, eg
		/// try
		/// {
		///     semaphore.Wait();
		///     do some work
		/// }
		/// catch (Exception ex)
		/// {
		///     if (SemaphoreExtensions.IsAlive(semaphore))
		///         // you have a real error, otherwise it's simply the semaphore being disposed and reset to null
		/// }
		/// finally
		/// {
		///     SemaphoreExtensions.TryRelease(semaphore);
		///     // do not just call semaphore.Release() because the semaphore may be disposed or null at this stage
		/// }
		/// </summary>
		/// <param name="semaphore"></param>
		/// <returns></returns>
		public static bool TryDispose(this Semaphore semaphore)
		{
			try
			{
				semaphore?.Dispose();
				return true;
			}
			catch { }
			return false;
		}
	}
}