using System.Globalization;
using Cirrious.MvvmCross.Converters.Visibility;

namespace apcurium.MK.Booking.Mobile.BindingConverter
{
    public class BoolVisibilityInverter : MvxBaseVisibilityConverter
    {
        public override MvxVisibility ConvertToMvxVisibility(object value, object parameter, CultureInfo culture)
        {
            var visibility = !(bool)value;
			return visibility ? MvxVisibility.Visible : MvxVisibility.Collapsed;
        }
    }
}