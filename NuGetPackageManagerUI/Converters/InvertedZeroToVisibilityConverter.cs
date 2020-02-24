using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NuGetPackageManagerUI.Converters
{
	public class InvertedZeroToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
				return Visibility.Hidden;

			if (int.TryParse(value.ToString(), out int number))
			{
				return number == 0 ? Visibility.Visible : Visibility.Hidden;
			}

			return Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
