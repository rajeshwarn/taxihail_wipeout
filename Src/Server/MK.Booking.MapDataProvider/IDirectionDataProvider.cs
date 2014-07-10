using System;
using apcurium.MK.Booking.MapDataProvider.Resources;


namespace apcurium.MK.Booking.MapDataProvider
{
	public interface IDirectionDataProvider
	{
		GeoDirection GetDirections(double originLatitude, double originLongitude, double destinationLatitude,
			double destinationLongitude);
	}
}

