using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Utilz
{
	public static class Pickers
	{
		private const string PICKED_FOLDER_TOKEN = "PickedFolderToken";
		private const string PICKED_SAVE_FILE_TOKEN = "PickedSaveFileToken";
		private const string PICKED_OPEN_FILE_TOKEN = "PickedOpenFileToken";

		public static async Task<StorageFolder> PickDirectoryAsync(string[] extensions)
		{
			//bool unsnapped = ((ApplicationView.Value != ApplicationViewState.Snapped) || ApplicationView.TryUnsnap());
			//if (unsnapped)
			//{

			StorageFolder directory = null;
			try
			{
				Task<StorageFolder> dirTask = null;
				await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, delegate
				{
					var openPicker = new FolderPicker
					{
						ViewMode = PickerViewMode.List,
						SuggestedStartLocation = PickerLocationId.DocumentsLibrary
					};

					foreach (var ext in extensions)
					{
						openPicker.FileTypeFilter.Add(ext);
					}
					dirTask = openPicker.PickSingleFolderAsync().AsTask();
				});

				directory = await dirTask;
			}
			catch (Exception ex)
			{
				await Logger.AddAsync(ex.ToString(), Logger.FileErrorLogFilename);
			}
			finally
			{
				SetLastPickedFolder(directory);
				SetLastPickedFolderMRU(directory);
			}
			return directory;

			//}
			//return false;
		}

		public static async Task<StorageFile> PickOpenFileAsync(string[] extensions)
		{
			StorageFile file = null;
			try
			{
				Task<StorageFile> fileTask = null;
				await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, delegate
				{
					var openPicker = new FileOpenPicker
					{
						ViewMode = PickerViewMode.List,
						SuggestedStartLocation = PickerLocationId.DocumentsLibrary
					};

					foreach (var ext in extensions)
					{
						openPicker.FileTypeFilter.Add(ext);
					}
					fileTask = openPicker.PickSingleFileAsync().AsTask();
				});

				file = await fileTask;
			}
			catch (Exception ex)
			{
				await Logger.AddAsync(ex.ToString(), Logger.FileErrorLogFilename);
			}
			finally
			{
				SetLastPickedOpenFile(file);
				SetLastPickedOpenFileMRU(file);
			}
			return file;
		}

		public static async Task<StorageFile> PickSaveFileAsync(string[] extensions, string suggestedFileName = "")
		{
			StorageFile file = null;
			try
			{
				Task<StorageFile> fileTask = null;
				await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, delegate
				{
					var picker = new FileSavePicker { SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
					if (!string.IsNullOrWhiteSpace(suggestedFileName)) picker.SuggestedFileName = suggestedFileName;

					foreach (var ext in extensions)
					{
						var exts = new List<string> { ext };
						picker.FileTypeChoices.Add(ext + " file", exts);
					}

					fileTask = picker.PickSaveFileAsync().AsTask();
				});

				file = await fileTask;
			}
			catch (Exception ex)
			{
				await Logger.AddAsync(ex.ToString(), Logger.FileErrorLogFilename);
			}
			finally
			{
				SetLastPickedSaveFile(file);
				SetLastPickedSaveFileMRU(file);
			}
			return file;
		}

		public static async Task<StorageFolder> GetLastPickedFolderAsync()
		{
			StorageFolder result = null;
			try
			{
				result = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFolderAsync(PICKED_FOLDER_TOKEN).AsTask().ConfigureAwait(false);
			}
			catch
			{
				result = null;
			}
			return result;
		}

		public static async Task<StorageFile> GetLastPickedOpenFileAsync()
		{
			StorageFile result = null;
			try
			{
				result = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync(PICKED_OPEN_FILE_TOKEN).AsTask().ConfigureAwait(false);
				//if (result != null) Logger.Add_TPL("Pickers got file " + result.Name + " from FAL", Logger.AppEventsLogFilename, Logger.Severity.Info, false);
				//else Logger.Add_TPL("Pickers got no file from FAL", Logger.AppEventsLogFilename, Logger.Severity.Info, false);
			}
			catch
			{
				result = null;
			}
			return result;
		}

		public static async Task<StorageFile> GetLastPickedSaveFileAsync()
		{
			StorageFile result = null;
			try
			{
				result = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync(PICKED_SAVE_FILE_TOKEN).AsTask().ConfigureAwait(false);
			}
			catch
			{
				result = null;
			}
			return result;
		}
		// LOLLO TODO check if these setters really need to set the MRU. It can screw things, particularly when the file is internal to the app!
		// in particular, see if this makes trouble with the hiking mate.
		public static void SetLastPickedFolder(StorageFolder directory)
		{
			try
			{
				if (directory != null)
				{
					Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace(PICKED_FOLDER_TOKEN, directory);
				}
				else
				{
					Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Remove(PICKED_FOLDER_TOKEN);
				}
			}
			catch { }
		}

		public static void SetLastPickedOpenFile(StorageFile file)
		{
			try
			{
				if (file != null)
				{
					Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace(PICKED_OPEN_FILE_TOKEN, file);
				}
				else
				{
					Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Remove(PICKED_OPEN_FILE_TOKEN);
				}
			}
			catch { }
		}
		public static void SetLastPickedSaveFile(StorageFile file)
		{
			try
			{
				if (file != null)
				{
					Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace(PICKED_SAVE_FILE_TOKEN, file);
				}
				else
				{
					Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Remove(PICKED_SAVE_FILE_TOKEN);
				}
			}
			catch { }
		}
		public static void SetLastPickedFolderMRU(StorageFolder directory)
		{
			try
			{
				if (directory != null)
				{
					Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.AddOrReplace(PICKED_FOLDER_TOKEN, directory);
				}
				else
				{
					Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Remove(PICKED_FOLDER_TOKEN);
				}
			}
			catch { }
		}

		public static void SetLastPickedOpenFileMRU(StorageFile file)
		{
			try
			{
				if (file != null)
				{
					Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.AddOrReplace(PICKED_OPEN_FILE_TOKEN, file);
				}
				else
				{
					Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Remove(PICKED_OPEN_FILE_TOKEN);
				}
			}
			catch { }
		}
		public static void SetLastPickedSaveFileMRU(StorageFile file)
		{
			try
			{
				if (file != null)
				{
					Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.AddOrReplace(PICKED_SAVE_FILE_TOKEN, file);
				}
				else
				{
					Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Remove(PICKED_SAVE_FILE_TOKEN);
				}
			}
			catch { }
		}
	}
}