using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Windows.Storage;

namespace Utilz
{
	public static class RegistryAccess
	{
		// If settings.Values[regKey] is not found when reading, it returns null but does not throw. 
		// If it is not found when writing, it creates the key

		public static string ReadAllReg() //LOLLO this is only for testing or diagnosing
		{
			var settings = ApplicationData.Current.LocalSettings;
			string output = string.Empty;
			foreach (var item in settings.Values)
			{
				// Debug.WriteLine(item.Key + " = " + item.Value.ToString());
				output += (item.Key + " = " + item.Value.ToString() + Environment.NewLine);
			}
			return output;
		}

		public static bool TrySetValue(string regKey, string value)
		{
			try
			{
				// Debug.WriteLine("writing value " + value + " into reg key " + regKey);
				var settings = ApplicationData.Current.LocalSettings;
				settings.Values[regKey] = value;
				return true;
			}
			catch (Exception ex)
			{
				Logger.Add_TPL(ex.ToString(), Logger.ForegroundLogFilename);
			}
			return false;
		}

		public static string GetValue(string regKey)
		{
			string valueStr = string.Empty;
			try
			{
				var settings = ApplicationData.Current.LocalSettings;
				object valueObj = settings.Values[regKey];
				if (valueObj != null)
				{
					valueStr = valueObj.ToString();
				}
				// Debug.WriteLine("reg key " + regKey + " has value " + valueStr);
			}
			catch (Exception ex)
			{
				Logger.Add_TPL(ex.ToString(), Logger.ForegroundLogFilename);
			}
			return valueStr;
		}

		public static async Task<T> GetObject<T>(string regKey)
		{
			string serialised = GetValue(regKey);
			if (string.IsNullOrWhiteSpace(serialised)) return default(T);

			try
			{
				using (MemoryStream memoryStream = new MemoryStream())
				{
					using (StreamWriter streamWriter = new StreamWriter(memoryStream))
					{
						await streamWriter.WriteAsync(serialised).ConfigureAwait(false);
						await streamWriter.FlushAsync().ConfigureAwait(false);
						memoryStream.Seek(0, SeekOrigin.Begin);
						DataContractSerializer serializer = new DataContractSerializer(typeof(T));
						var tileSource = (T)(serializer.ReadObject(memoryStream));
						return tileSource;
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Add_TPL(ex.ToString(), Logger.FileErrorLogFilename);
				return default(T);
			}
		}

		public static async Task<bool> TrySetObject<T>(string regKey, T instance)
		{
			try
			{
				if (instance == null)
				{
					TrySetValue(regKey, string.Empty);
				}
				else
				{
					using (MemoryStream memoryStream = new MemoryStream())
					{
						DataContractSerializer serializer = new DataContractSerializer(typeof(T));
						serializer.WriteObject(memoryStream, instance);
						await memoryStream.FlushAsync().ConfigureAwait(false);
						memoryStream.Seek(0, SeekOrigin.Begin);
						using (StreamReader streamReader = new StreamReader(memoryStream))
						{
							string serialised = await streamReader.ReadToEndAsync().ConfigureAwait(false);
							TrySetValue(regKey, serialised);
							return true;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Add_TPL(ex.ToString(), Logger.FileErrorLogFilename);
			}
			return false;
		}
	}
}