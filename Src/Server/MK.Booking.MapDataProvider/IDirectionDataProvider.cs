using System;
using apcurium.MK.Booking.MapDataProvider.Resources;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.MapDataProvider
{
	public interface IDirectionDataProvider
	{
        Task<GeoDirection> GetDirectionsAsync(double originLat, double originLng, double destLat, double destLng, DateTime? date);
	}
}

