using apcurium.MK.Booking.MapDataProvider.Resources;

namespace apcurium.MK.Booking.MapDataProvider
{
	public interface IPlaceDataProvider
	{
		GeoPlace[] GetNearbyPlaces(double? latitude, double? longitude, string languageCode, int radius, uint maximumNumberOfPlaces = 0, string pipedTypeList = null);

		GeoPlace[] SearchPlaces(double? latitude, double? longitude, string query, string languageCode, int radius);

		GeoPlace GetPlaceDetail (string id);
	}
}