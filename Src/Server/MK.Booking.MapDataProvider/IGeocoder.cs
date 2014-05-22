using System;
using apcurium.MK.Booking.MapDataProvider.Resources;

namespace apcurium.MK.Booking.MapDataProvider
{
	public interface IGeocoder
	{
		GeoAddress[] GeocodeAddress(string address);
		GeoAddress[]  GeocodeLocation(double latitude, double longitude);

	}
}

