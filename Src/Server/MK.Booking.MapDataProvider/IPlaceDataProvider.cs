using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.MapDataProvider.Resources;
using MK.Booking.MapDataProvider.Foursquare;

namespace apcurium.MK.Booking.MapDataProvider
{
	public interface IPlaceDataProvider
	{
	    Task<GeoPlace[]> GetNearbyPlacesAsync(double? latitude, double? longitude, string languageCode, bool sensor, int radius, uint maximumNumberOfPlaces = 0, string pipedTypeList = null);

	    Task<GeoPlace[]> SearchPlacesAsync(double? latitude, double? longitude, string name, string languageCode, bool sensor, int radius, string countryCode);

	    Task<GeoPlace> GetPlaceDetailAsync(string id);

        /// <summary>
        /// Do not use this call on mobile
        /// </summary>
        GeoPlace[] GetNearbyPlaces(double? latitude, double? longitude, string languageCode, bool sensor, int radius, uint maximumNumberOfPlaces = 0, string pipedTypeList = null);

        /// <summary>
        /// Do not use this call on mobile
        /// </summary>
		GeoPlace[] SearchPlaces(double? latitude, double? longitude, string name, string languageCode, bool sensor, int radius, string countryCode);

        /// <summary>
        /// Do not use this call on mobile
        /// </summary>
		GeoPlace GetPlaceDetail (string id);
	}
}