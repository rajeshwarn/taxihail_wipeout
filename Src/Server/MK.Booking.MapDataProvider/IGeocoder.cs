using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.MapDataProvider.Resources;

namespace apcurium.MK.Booking.MapDataProvider
{
	public interface IGeocoder
	{
		GeoAddress[] GeocodeAddress(string address, string currentLanguage);

	    Task<GeoAddress[]> GeocodeAddressAsync(string address, string currentLanguage);
        
		GeoAddress[]  GeocodeLocation(double latitude, double longitude, string currentLanguage);
	}
}

