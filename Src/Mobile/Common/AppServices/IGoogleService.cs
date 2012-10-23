using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IGoogleService
	{
        Address GetPlaceDetail(string name, string reference);
		Address[] GetNearbyPlaces(double? latitude, double? longitude, int? radius = null);
		//Address[] GetNearbyPlaces(double? latitude, double? longitude);
	}
}

