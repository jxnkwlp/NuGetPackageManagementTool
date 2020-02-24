using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Humanizer;

namespace NuGetPackageManagerUI.Converters
{
	public class NumberHumanizerConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
				return null;

			if ((value.GetType() == typeof(int) || value.GetType() == typeof(long)) && int.TryParse(value.ToString(), out int number1))
			{
				return number1.ToMetric(decimals: 2);
			}
			else if (value.GetType() == typeof(double) && double.TryParse(value.ToString(), out double number3))
			{
				return number3.ToMetric(decimals: 2);
			}

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
