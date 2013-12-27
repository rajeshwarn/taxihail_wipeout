using Cirrious.MvvmCross.Converters.Visibility;
using apcurium.MK.Booking.Mobile.BindingConverter;

namespace apcurium.MK.Booking.Mobile.Client.Converters
{
    public class AppConverters
    {
        public readonly CoordinateConverter CoordinateConverter = new CoordinateConverter();
		public readonly MvxVisibilityConverter Visibility = new MvxVisibilityConverter();
		public readonly HasValueToVisibilityConverter HasValueToVisibilityConverter = new HasValueToVisibilityConverter();
        public readonly MvxInvertedVisibilityConverter InvertedVisibility = new MvxInvertedVisibilityConverter();
        public readonly BoolInverter BoolInverter = new BoolInverter();
        public readonly OrderStatusToTextColorConverter OrderStatusToTextColorConverter = new OrderStatusToTextColorConverter();
		public readonly EmptyToResourceConverter EmptyToResource = new EmptyToResourceConverter();
		public readonly EnumToBoolConverter EnumToBool = new EnumToBoolConverter();
    }
}