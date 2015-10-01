using System;
using Cirrious.CrossCore.Converters;
using apcurium.MK.Common;

namespace apcurium.MK.Booking.Mobile.Client.Converters
{
    public class CreditCardLabelToBool : MvxValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.ToString() == CreditCardConstants.Personal ? true : false;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return bool.Parse(value.ToString()) == true ? CreditCardConstants.Personal : CreditCardConstants.Business;
        }
    }
}

