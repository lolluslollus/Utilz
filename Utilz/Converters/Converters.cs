using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using System.Diagnostics;

namespace Utilz.Converters
{
	public class BooleanToVisibleConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (!(value is bool)) return Visibility.Collapsed;
			bool boo = (bool)value;
			if (boo) return Visibility.Visible;
			else return Visibility.Collapsed;
		}
		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new Exception("this is a one-way bonding, it should never come here");
		}
	}

	public class BooleanToCollapsedConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (!(value is bool)) return Visibility.Visible;
			bool boo = (bool)value;
			if (boo) return Visibility.Collapsed;
			else return Visibility.Visible;
		}
		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new Exception("this is a one-way bonding, it should never come here");
		}
	}

	public class TrueToFalseConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (!(value is bool)) return true;
			bool boo = (bool)value;
			if (boo) return false;
			else return true;
		}
		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new Exception("this is a one-way bonding, it should never come here");
		}
	}

	public class IntIsNullToVisibleConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (!(value is int)) return Visibility.Visible;
			int val = (int)value;
			if (val > 0) return Visibility.Collapsed;
			else return Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new Exception("this is a one-way bonding, it should never come here");
		}
	}

	public class IntIsNullToFalseConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (!(value is int)) return false;
			int val = (int)value;
			if (val > 0) return true;
			else return false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new Exception("this is a one-way bonding, it should never come here");
		}
	}

	public class IntIsNullToCollapsedConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (!(value is int)) return Visibility.Collapsed;
			int val = (int)value;
			if (val > 0) return Visibility.Visible;
			else return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new Exception("this is a one-way bonding, it should never come here");
		}
	}

	public class SeriesCountGreaterThanZeroToBooleanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (!(value is int)) return false;
			int val = (int)value;
			if (val > 0) return true;
			else return false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new Exception("this is a one-way binding, it should never come here");
		}
	}

	public class StringNotEmptyToVisibleConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value == null) return Visibility.Collapsed;
			if (string.IsNullOrWhiteSpace(value.ToString())) return Visibility.Collapsed;
			else return Visibility.Visible;
		}
		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new Exception("this is a one-way bonding, it should never come here");
		}
	}

	public class StringNotEmptyToTrueConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value == null) return false;
			if (string.IsNullOrWhiteSpace(value.ToString())) return false;
			else return true;
		}
		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new Exception("this is a one-way bonding, it should never come here");
		}
	}

	public class StringFormatterConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			string format = parameter.ToString();
			string output = string.Empty;
			try
			{
				output = string.Format(CultureInfo.CurrentUICulture, format, new object[1] { value });
			}
			catch (FormatException)
			{
				output = value.ToString();
			}
			return output;
		}
		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new Exception("this is a one-way binding, it should never come here");
		}
	}

	public class FloatNotNullToVisibleConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value == null) return Visibility.Collapsed;
			if (string.IsNullOrWhiteSpace(value.ToString()) || value.ToString().Equals(default(double).ToString(CultureInfo.CurrentUICulture)) || value.ToString().Equals(default(int).ToString())) return Visibility.Collapsed;
			else return Visibility.Visible;
		}
		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new Exception("this is a one-way binding, it should never come here");
		}
	}

	public class DateNotNullToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value == null) return Visibility.Collapsed;

			DateTime dt = default(DateTime);
			if (!DateTime.TryParse(value.ToString(), CultureInfo.CurrentUICulture, DateTimeStyles.None, out dt))
			{
				DateTime.TryParse(value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);
			}

			if (dt == default(DateTime)) return Visibility.Collapsed;
			else return Visibility.Visible;

		}
		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new Exception("this is a one-way bonding, it should never come here");
		}
	}
}