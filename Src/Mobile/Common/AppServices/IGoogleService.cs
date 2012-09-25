using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IGoogleService
	{
        Address GetPlaceDetail(string reference);
		Address[] GetNearbyPlaces(double? latitude, double? longitude, int? radius);
		Address[] GetNearbyPlaces(double? latitude, double? longitude);
	}
}

