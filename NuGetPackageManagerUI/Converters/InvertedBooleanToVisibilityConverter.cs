using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NuGetPackageManagerUI.Converters
{
	public class InvertedBooleanToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
			{
				return Visibility.Hidden;
			}
			if (bool.TryParse(value.ToString(), out bool result))
			{
				if (!result) return Visibility.Visible;
			}

			return Visibility.Hidden;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
