using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IGoogleService
	{
        Address GetPlaceDetail(string name, string reference);
        Address[] GetNearbyPlaces(double? latitude, double? longitude, string name = null, int? radius = null);		
	}
}

