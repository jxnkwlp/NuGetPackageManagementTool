using System;
using System.Globalization;
using System.Windows.Data;

namespace NuGetPackageManagerUI.Converters
{
	public class InvertedBooleanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
				return false;

			if (bool.TryParse(value.ToString(), out bool result))
			{
				return !result;
			}

			return false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}

}
