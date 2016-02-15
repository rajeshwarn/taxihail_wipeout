using System.Threading.Tasks;
using apcurium.MK.Booking.MapDataProvider.Resources;

namespace apcurium.MK.Booking.MapDataProvider
{
	public interface IPlaceDataProvider
	{
        Task<GeoPlace[]> GetNearbyPlacesAsync(double? latitude, double? longitude, string languageCode, int radius, uint maximumNumberOfPlaces = 0, string pipedTypeList = null);

        Task<GeoPlace[]> SearchPlacesAsync(double? latitude, double? longitude, string query, string languageCode, int radius);

	    Task<GeoPlace> GetPlaceDetailAsync(string id);

        /// <summary>
        /// Do not use this call on mobile
        /// </summary>
        GeoPlace[] GetNearbyPlaces(double? latitude, double? longitude, string languageCode, int radius, uint maximumNumberOfPlaces = 0, string pipedTypeList = null);

        /// <summary>
        /// Do not use this call on mobile
        /// </summary>
        GeoPlace[] SearchPlaces(double? latitude, double? longitude, string query, string languageCode, int radius);

        /// <summary>
        /// Do not use this call on mobile
        /// </summary>
		GeoPlace GetPlaceDetail (string id);
	}
}