using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;

namespace Utilz
{
	public static class FileDirectoryExtensions
	{
		//	private static readonly ulong MaxBufferSize = 16 * 1024 * 1024;

		//// LOLLO TODO test the following, it is a smarter way to copy files (it rpevents errorr when they are too large)
		//	public static async Task<StorageFile> CopyAsync(this StorageFile self, StorageFolder desiredFolder, string desiredNewName, CreationCollisionOption option)
		//	{
		//		StorageFile desiredFile = await desiredFolder.CreateFileAsync(desiredNewName, option);
		//		StorageStreamTransaction desiredTransaction = await desiredFile.OpenTransactedWriteAsync();
		//		BasicProperties props = await self.GetBasicPropertiesAsync();
		//		IInputStream stream = await self.OpenSequentialReadAsync();

		//		ulong copiedSize = 0L;
		//		while (copiedSize < props.Size)
		//		{
		//			ulong bufferSize = (props.Size - copiedSize) >= MaxBufferSize ? MaxBufferSize : props.Size - copiedSize;
		//			IBuffer buffer = BytesToBuffer(new byte[bufferSize]);
		//			await stream.ReadAsync(buffer, (uint)bufferSize, InputStreamOptions.None);
		//			await desiredTransaction.Stream.GetOutputStreamAt(copiedSize).WriteAsync(buffer);
		//			buffer = null;
		//			copiedSize += (bufferSize);

		//			//Debug.WriteLine(DeviceStatus.ApplicationCurrentMemoryUsage);
		//		}

		//		await desiredTransaction.CommitAsync();

		//		return desiredFile;
		//	}

		//	private static IBuffer BytesToBuffer(byte[] bytes)
		//	{
		//		using (var dataWriter = new DataWriter())
		//		{
		//			dataWriter.WriteBytes(bytes);
		//			return dataWriter.DetachBuffer();
		//		}
		//	}
		

		public static Task CopyDirContentsAsync(this StorageFolder from, StorageFolder toDirectory, int maxDepth = 0)
		{
			// copy its contents
			return new FileDirectoryExts().CopyDirContents2Async(from, toDirectory, maxDepth);
		}

		public static async Task<ulong> GetFileSizeAsync(this StorageFile file)
		{
			if (file == null) return 0;

			BasicProperties fileProperties = await file.GetBasicPropertiesAsync().AsTask().ConfigureAwait(false);
			if (fileProperties != null) return fileProperties.Size;
			else return 0;
		}

		public static async Task DeleteDirContentsAsync(this StorageFolder dir)
		{
			try
			{
				if (dir == null) return;

				var contents = await dir.GetItemsAsync().AsTask().ConfigureAwait(false);
				var delTasks = new List<Task>();
				foreach (var item in contents)
				{
					delTasks.Add(item.DeleteAsync(StorageDeleteOption.PermanentDelete).AsTask());
				}
				await Task.WhenAll(delTasks).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				await Logger.AddAsync(ex.ToString(), Logger.FileErrorLogFilename).ConfigureAwait(false);
			}
		}

		private class FileDirectoryExts
		{
			internal FileDirectoryExts() { }
			private int _currentDepth = 0;
			// LOLLO TODO what if you copy a directory to an existing one? Shouldn't you delete the contents first? No! But then, shouldn't you issue a warning?
			internal async Task CopyDirContents2Async(StorageFolder from, StorageFolder to, int maxDepth = 0)
			{
				try
				{
					if (from == null || to == null) return;
					// read files
					var filesDepth0 = await from.GetFilesAsync().AsTask().ConfigureAwait(false);
					// copy files
					var copyTasks = new List<Task>();
					foreach (var file in filesDepth0)
					{
						copyTasks.Add(file.CopyAsync(to, file.Name, NameCollisionOption.ReplaceExisting).AsTask());
						await Logger.AddAsync("File copied: " + file.Name, Logger.FileErrorLogFilename, Logger.Severity.Info).ConfigureAwait(false);
					}
					await Task.WhenAll(copyTasks).ConfigureAwait(false);

					//var plr = Parallel.ForEach(filesDepth0, (file) =>
					//{
					//	// LOLLO NOTE avoid async calls within a Parallel.ForEach coz they are not awaited
					//	file.CopyAsync(to, file.Name, NameCollisionOption.ReplaceExisting).AsTask().Wait();
					//});

					//Debug.WriteLine("CopyDirContentsReplacingAsync: plr is completed = " + plr.IsCompleted);

					//foreach (var file in filesDepth0)
					//{
					//	await file.CopyAsync(to, file.Name, NameCollisionOption.ReplaceExisting).AsTask().ConfigureAwait(false);
					//}
					// check depth
					//var toFiles = await to.GetFilesAsync().AsTask().ConfigureAwait(false);
					//foreach(var file in toFiles)
					//{

					//}
				}
				catch (Exception ex)
				{
					await Logger.AddAsync(ex.ToString(), Logger.FileErrorLogFilename).ConfigureAwait(false);
				}

				_currentDepth += 1;
				if (_currentDepth > maxDepth) return;
				// read dirs
				var dirsDepth0 = await from.GetFoldersAsync().AsTask().ConfigureAwait(false);
				// copy dirs
				foreach (var dirFrom in dirsDepth0)
				{
					var dirTo = await to.CreateFolderAsync(dirFrom.Name, CreationCollisionOption.ReplaceExisting).AsTask().ConfigureAwait(false);
					await CopyDirContents2Async(dirFrom, dirTo).ConfigureAwait(false);
				}
			}
		}
	}
}