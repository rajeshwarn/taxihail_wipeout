using System;
using apcurium.MK.Booking.MapDataProvider.Resources;

namespace apcurium.MK.Booking.MapDataProvider
{
	public interface IPlaceDataProvider
	{
		GeoPlace[] GetNearbyPlaces(double? latitude, double? longitude, string languageCode, bool sensor, int radius,
			string pipedTypeList = null);

		GeoPlace[] SearchPlaces(double? latitude, double? longitude, string name, string languageCode, bool sensor,
			int radius, string countryCode);

		GeoPlace GetPlaceDetail (string id);
	}
}

