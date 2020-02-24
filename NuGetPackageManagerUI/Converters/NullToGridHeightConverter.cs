using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NuGetPackageManagerUI.Converters
{
	public class NullToGridHeightConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (string.IsNullOrWhiteSpace(value?.ToString()))
				return new GridLength(0);
			else
				return new GridLength(1, GridUnitType.Auto);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
