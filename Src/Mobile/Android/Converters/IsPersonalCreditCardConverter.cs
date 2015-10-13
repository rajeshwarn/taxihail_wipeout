using System;
using Cirrious.CrossCore.Converters;
using apcurium.MK.Common;

namespace apcurium.MK.Booking.Mobile.Client.Converters
{
    public class IsPersonalCreditCardConverter : MvxValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.ToString() == CreditCardLabelConstants.Personal.ToString() ? true : false;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return bool.Parse(value.ToString()) == true ? CreditCardLabelConstants.Personal.ToString() : CreditCardLabelConstants.Business.ToString();
        }
    }
}

