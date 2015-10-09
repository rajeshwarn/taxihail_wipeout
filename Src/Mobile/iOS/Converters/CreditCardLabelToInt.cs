using System;
using Cirrious.CrossCore.Converters;
using apcurium.MK.Common;

namespace apcurium.MK.Booking.Mobile.Client.Converters
{
    public class CreditCardLabelToInt : MvxValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var labelEnum = value as CreditCardLabelConstants?;
            if (labelEnum.HasValue)
            {
                CreditCardLabelConstants label;
                if (Enum.TryParse<CreditCardLabelConstants>(labelEnum.Value.ToString(), out label))
                {
                    return (int)label; 
                }
            }
            return (int)CreditCardLabelConstants.Personal;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int val;
            if (int.TryParse(value.ToString(), out val))
            {
                return (CreditCardLabelConstants)val;
            }

            return CreditCardLabelConstants.Personal;
        }
    }
}

