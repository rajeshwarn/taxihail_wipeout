using System;
using Cirrious.CrossCore.Converters;
using apcurium.MK.Common;

namespace apcurium.MK.Booking.Mobile.BindingConverter
{
	public class CreditCardLabelToInt : MvxValueConverter
	{
		public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return value.ToString() == CreditCardConstants.Personal ? 0 : 1;
		}

		public override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return int.Parse(value.ToString()) == 0 ? CreditCardConstants.Personal : CreditCardConstants.Business;
		}
	}
}

