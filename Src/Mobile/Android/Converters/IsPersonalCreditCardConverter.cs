using System;
using Cirrious.CrossCore.Converters;
using apcurium.MK.Common;

namespace apcurium.MK.Booking.Mobile.Client.Converters
{
    public class IsPersonalCreditCardConverter : MvxValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter != null && parameter.ToString() == "Inverted")
            {
                return value.ToString() != CreditCardLabelConstants.Personal.ToString();
            }
            return value.ToString() == CreditCardLabelConstants.Personal.ToString();
        }

        public override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter != null && parameter.ToString() == "Inverted")
            {
                return bool.Parse(value.ToString()) ? CreditCardLabelConstants.Business : CreditCardLabelConstants.Personal;
            }
            return bool.Parse(value.ToString()) ? CreditCardLabelConstants.Personal : CreditCardLabelConstants.Business;
        }
    }
}

