using apcurium.MK.Booking.Mobile.BindingConverter;
using Cirrious.MvvmCross.Converters.Visibility;

namespace apcurium.MK.Booking.Mobile.Client.Converters
{
    public class AppConverters
    {
        public readonly BoolInverter BoolInverter = new BoolInverter();
        public readonly CoordinateConverter CoordinateConverter = new CoordinateConverter();
        public readonly EmptyToResourceConverter EmptyToResource = new EmptyToResourceConverter();
        public readonly EnumToBoolConverter EnumToBool = new EnumToBoolConverter();

        public readonly HasValueToVisibilityConverter HasValueToVisibilityConverter =
            new HasValueToVisibilityConverter();

        public readonly MvxInvertedVisibilityConverter InvertedVisibility = new MvxInvertedVisibilityConverter();

        public readonly OrderStatusToTextColorConverter OrderStatusToTextColorConverter =
            new OrderStatusToTextColorConverter();

        public readonly MvxVisibilityConverter Visibility = new MvxVisibilityConverter();
    }
}