﻿using System;
using System.Diagnostics;

namespace Utilz
{
	public static class KeepAlive
	{
		private static volatile Windows.System.Display.DisplayRequest _appDisplayRequest = null; //new Windows.System.Display.DisplayRequest();
		private const int MAX_COUNT = 10000; // 2147483647L;
		private static int _displayRequestRefCount = 0;
		private static readonly object _countLock = new object();

		/// <summary>
		/// Always call this from the UI thread
		/// </summary>
		/// <param name="isMustKeepAlive"></param>
		[STAThread]
		public static void UpdateKeepAlive(bool isMustKeepAlive)
		{
			try
			{
				lock (_countLock)
				{
					if (isMustKeepAlive) SetTrue();
					else SetFalse();
				}
			}
			catch (Exception ex)
			{
				Logger.Add_TPL(ex.ToString(), Logger.AppExceptionLogFilename);
			}
		}

		private static void SetFalse()
		{
			if (_displayRequestRefCount > 0)
			{
				if (_appDisplayRequest == null) _appDisplayRequest = new Windows.System.Display.DisplayRequest();
				_appDisplayRequest.RequestRelease();
				_displayRequestRefCount--;
			}
		}

		private static void SetTrue()
		{
			if (_displayRequestRefCount < MAX_COUNT)
			{
				if (_appDisplayRequest == null) _appDisplayRequest = new Windows.System.Display.DisplayRequest();
				_appDisplayRequest.RequestActive();
				_displayRequestRefCount++;
			}
		}

		/// <summary>
		/// Always call this from the UI thread
		/// </summary>
		[STAThread]
		public static void StopKeepAlive()
		{
			try
			{
				lock (_countLock)
				{
					while (_displayRequestRefCount > 0) SetFalse();
				}
			}
			catch (Exception ex)
			{
				Logger.Add_TPL(ex.ToString(), Logger.AppExceptionLogFilename);
			}
		}
		//public static void ReleaseKeepAlive()
		//{
		//    // release all display requests // do I need this?
		//}
	}
}