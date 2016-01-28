using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.MapDataProvider.Resources;

namespace apcurium.MK.Booking.MapDataProvider
{
	public interface IGeocoder
	{
        GeoAddress[] GeocodeAddress(string query, string currentLanguage, double? pickupLatitude, double? pickupLongitude, double searchRadiusInMeters);

        Task<GeoAddress[]> GeocodeAddressAsync(string query, string currentLanguage, double? pickupLatitude, double? pickupLongitude, double searchRadiusInMeters);
        
		GeoAddress[]  GeocodeLocation(double latitude, double longitude, string currentLanguage);

	    Task<GeoAddress[]> GeocodeLocationAsync(double latitude, double longitude, string currentLanguage);
	}
}

