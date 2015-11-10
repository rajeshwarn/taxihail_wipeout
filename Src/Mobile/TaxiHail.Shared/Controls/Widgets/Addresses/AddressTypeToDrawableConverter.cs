using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets.Addresses
{
    internal class AddressTypeToDrawableConverter
    {
        public static int GetDrawable(AddressType addressType)
        {
            switch (addressType)
            {
                case AddressType.Favorites:
                    return Resource.Drawable.favorites;
                case AddressType.Places:
					return Resource.Drawable.places;
                case AddressType.History:
                    return Resource.Drawable.history;
                default:
					return Resource.Drawable.places;
            }
        }
    }
}