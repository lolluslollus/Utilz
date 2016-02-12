using SQLite;
using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;


namespace Utilz.Data
{
	[DataContract]
	public abstract class OpenableObservableData : ObservableData
	{
		#region properties
		protected volatile SemaphoreSlimSafeRelease _isOpenSemaphore = null;

		protected volatile bool _isOpen = false;
		[IgnoreDataMember]
		[Ignore]
		public bool IsOpen { get { return _isOpen; } protected set { if (_isOpen != value) { _isOpen = value; RaisePropertyChanged_UI(); } } }

		private readonly object _ctsLocker = new object();
		private SafeCancellationTokenSource _cts = null;
		//[IgnoreDataMember]
		//[Ignore]
		//protected SafeCancellationTokenSource Cts
		//{
		//	get
		//	{
		//		lock (_ctsLocker) { return _cts; }
		//	}
		//}

		private CancellationToken _cancToken;
		[IgnoreDataMember]
		[Ignore]
		protected CancellationToken CancToken
		{
			get
			{
				lock (_ctsLocker) { return _cancToken; }
			}
		}

		//protected Func<Task> _runAsSoonAsOpen = null;
		#endregion properties


		#region open close
		public async Task<bool> OpenAsync()
		{
			if (!_isOpen)
			{
				if (!SemaphoreSlimSafeRelease.IsAlive(_isOpenSemaphore)) _isOpenSemaphore = new SemaphoreSlimSafeRelease(1, 1);
				try
				{
					await _isOpenSemaphore.WaitAsync().ConfigureAwait(false);
					if (!_isOpen)
					{
						lock (_ctsLocker)
						{
							_cts?.Dispose();
							_cts = new SafeCancellationTokenSource();
							_cancToken = _cts.Token;
						}

						await OpenMayOverrideAsync().ConfigureAwait(false);

						IsOpen = true;

						//try
						//{
						//	var runAsSoonAsOpen = _runAsSoonAsOpen;
						//	if (runAsSoonAsOpen != null)
						//	{
						//		Task asSoonAsOpen = Task.Run(runAsSoonAsOpen, CancToken);
						//	}
						//}
						//catch { }
						//finally { _runAsSoonAsOpen = null; }

						return true;
					}
				}
				catch (Exception ex)
				{
					if (SemaphoreSlimSafeRelease.IsAlive(_isOpenSemaphore))
						await Logger.AddAsync(GetType().Name + ex.ToString(), Logger.ForegroundLogFilename);
				}
				finally
				{
					SemaphoreSlimSafeRelease.TryRelease(_isOpenSemaphore);
				}
			}
			return false;
		}

		protected virtual Task OpenMayOverrideAsync()
		{
			return Task.CompletedTask;
		}

		public async Task<bool> CloseAsync()
		{
			if (_isOpen)
			{
				lock (_ctsLocker)
				{
					_cts?.CancelSafe(true);
				}

				try
				{
					await _isOpenSemaphore.WaitAsync().ConfigureAwait(false);
					if (_isOpen)
					{
						lock (_ctsLocker)
						{
							_cts?.Dispose();
							_cts = null;
							_cancToken = new CancellationToken(true); // CancellationToken is not nullable and not disposable
						}

						IsOpen = false;
						await CloseMayOverrideAsync().ConfigureAwait(false);

						return true;
					}
				}
				catch (Exception ex)
				{
					if (SemaphoreSlimSafeRelease.IsAlive(_isOpenSemaphore))
						await Logger.AddAsync(GetType().Name + ex.ToString(), Logger.ForegroundLogFilename);
				}
				finally
				{
					SemaphoreSlimSafeRelease.TryDispose(_isOpenSemaphore);
					_isOpenSemaphore = null;
				}
			}
			return false;
		}

		protected virtual Task CloseMayOverrideAsync()
		{
			return Task.CompletedTask;
		}
		#endregion open close


		#region while open
		protected async Task<bool> RunFunctionIfOpenAsyncA(Action func)
		{
			if (_isOpen)
			{
				try
				{
					await _isOpenSemaphore.WaitAsync(CancToken); //.ConfigureAwait(false);
					if (_isOpen)
					{
						func();
						return true;
					}
				}
				catch (OperationCanceledException) { }
				catch (Exception ex)
				{
					if (SemaphoreSlimSafeRelease.IsAlive(_isOpenSemaphore))
						await Logger.AddAsync(GetType().Name + ex.ToString(), Logger.ForegroundLogFilename);
				}
				finally
				{
					SemaphoreSlimSafeRelease.TryRelease(_isOpenSemaphore);
				}
			}
			return false;
		}
		protected async Task<bool> RunFunctionIfOpenAsyncB(Func<bool> func)
		{
			if (_isOpen)
			{
				try
				{
					await _isOpenSemaphore.WaitAsync(CancToken); //.ConfigureAwait(false);
					if (_isOpen) return func();
				}
				catch (OperationCanceledException) { }
				catch (Exception ex)
				{
					if (SemaphoreSlimSafeRelease.IsAlive(_isOpenSemaphore))
						await Logger.AddAsync(GetType().Name + ex.ToString(), Logger.ForegroundLogFilename);
				}
				finally
				{
					SemaphoreSlimSafeRelease.TryRelease(_isOpenSemaphore);
				}
			}
			return false;
		}
		protected async Task<bool> RunFunctionIfOpenAsyncT(Func<Task> funcAsync)
		{
			if (_isOpen)
			{
				try
				{
					await _isOpenSemaphore.WaitAsync(CancToken); //.ConfigureAwait(false);
					if (_isOpen)
					{
						await funcAsync().ConfigureAwait(false);
						return true;
					}
				}
				catch (OperationCanceledException) { }
				catch (Exception ex)
				{
					if (SemaphoreSlimSafeRelease.IsAlive(_isOpenSemaphore))
						await Logger.AddAsync(GetType().Name + ex.ToString(), Logger.ForegroundLogFilename);
				}
				finally
				{
					SemaphoreSlimSafeRelease.TryRelease(_isOpenSemaphore);
				}
			}
			return false;
		}
		protected async Task<bool> RunFunctionIfOpenAsyncTB(Func<Task<bool>> funcAsync)
		{
			if (_isOpen)
			{
				try
				{
					await _isOpenSemaphore.WaitAsync(CancToken); //.ConfigureAwait(false);
					if (_isOpen) return await funcAsync().ConfigureAwait(false);
				}
				catch (OperationCanceledException) { }
				catch (Exception ex)
				{
					if (SemaphoreSlimSafeRelease.IsAlive(_isOpenSemaphore))
						await Logger.AddAsync(GetType().Name + ex.ToString(), Logger.ForegroundLogFilename);
				}
				finally
				{
					SemaphoreSlimSafeRelease.TryRelease(_isOpenSemaphore);
				}
			}
			return false;
		}

		protected async Task<bool> RunFunctionIfOpenAsyncA_MT(Action func)
		{
			if (_isOpen)
			{
				try
				{
					await _isOpenSemaphore.WaitAsync(CancToken); //.ConfigureAwait(false);
					if (_isOpen && func != null)
					{
						await Task.Run(func, CancToken).ConfigureAwait(false);
						return true;
					}
				}
				catch (OperationCanceledException) { }
				catch (Exception ex)
				{
					if (SemaphoreSlimSafeRelease.IsAlive(_isOpenSemaphore))
						await Logger.AddAsync(GetType().Name + ex.ToString(), Logger.ForegroundLogFilename);
				}
				finally
				{
					SemaphoreSlimSafeRelease.TryRelease(_isOpenSemaphore);
				}
			}
			return false;
		}
		protected async Task<bool> RunFunctionIfOpenAsyncT_MT(Func<Task> funcAsync)
		{
			if (_isOpen)
			{
				try
				{
					await _isOpenSemaphore.WaitAsync(CancToken); //.ConfigureAwait(false);
					if (_isOpen && funcAsync != null)
					{
						await Task.Run(funcAsync, CancToken).ConfigureAwait(false);
						return true;
					}
				}
				catch (OperationCanceledException) { }
				catch (Exception ex)
				{
					if (SemaphoreSlimSafeRelease.IsAlive(_isOpenSemaphore))
						await Logger.AddAsync(GetType().Name + ex.ToString(), Logger.ForegroundLogFilename);
				}
				finally
				{
					SemaphoreSlimSafeRelease.TryRelease(_isOpenSemaphore);
				}
			}
			return false;
		}

		public enum BoolWhenOpen { Yes, No, ObjectClosed, Error };

		protected async Task<BoolWhenOpen> RunFunctionIfOpenThreeStateAsyncB(Func<bool> func)
		{
			BoolWhenOpen result = BoolWhenOpen.ObjectClosed;
			if (_isOpen)
			{
				try
				{
					await _isOpenSemaphore.WaitAsync(CancToken); //.ConfigureAwait(false);
					if (_isOpen)
					{
						result = func() ? BoolWhenOpen.Yes : BoolWhenOpen.No;
					}
				}
				catch (OperationCanceledException)
				{
					result = BoolWhenOpen.ObjectClosed;
				}
				catch (Exception ex)
				{
					if (SemaphoreSlimSafeRelease.IsAlive(_isOpenSemaphore))
					{
						result = BoolWhenOpen.Error;
						await Logger.AddAsync(GetType().Name + ex.ToString(), Logger.ForegroundLogFilename);
					}
				}
				finally
				{
					SemaphoreSlimSafeRelease.TryRelease(_isOpenSemaphore);
				}
			}
			return result;
		}

		protected async Task<BoolWhenOpen> RunFunctionIfOpenThreeStateAsyncT(Func<Task> funcAsync)
		{
			if (_isOpen)
			{
				try
				{
					await _isOpenSemaphore.WaitAsync(CancToken); //.ConfigureAwait(false);
					if (_isOpen)
					{
						await funcAsync().ConfigureAwait(false);
						return BoolWhenOpen.Yes;
					}
				}
				catch (OperationCanceledException)
				{
					return BoolWhenOpen.ObjectClosed;
				}
				catch (Exception ex)
				{
					if (SemaphoreSlimSafeRelease.IsAlive(_isOpenSemaphore))
					{
						await Logger.AddAsync(GetType().Name + ex.ToString(), Logger.ForegroundLogFilename);
						return BoolWhenOpen.Error;
					}
				}
				finally
				{
					SemaphoreSlimSafeRelease.TryRelease(_isOpenSemaphore);
				}
			}

			return BoolWhenOpen.ObjectClosed;
		}
		#endregion while open
	}

	public interface IOpenable
	{
		Task<bool> OpenAsync();
		Task<bool> CloseAsync();
		bool IsOpen { get; }
	}
}