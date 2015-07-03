using System;
using Cirrious.CrossCore.Converters;
using System.Globalization;

namespace apcurium.MK.Booking.Mobile.BindingConverter
{
	public class DialCodeConverter:MvxValueConverter
	{
		public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null)
			{
				int val = 0;

				if (int.TryParse(value.ToString(), out val))
				{
					if (val > 0)
					{
						return "+" + val.ToString(culture);
					}
				}
			}

			return null;
		}
	}
}