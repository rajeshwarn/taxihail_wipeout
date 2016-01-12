using apcurium.MK.Common.Entity;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IGoogleService
	{
        Task<Address> GetPlaceDetail(string name, string placeId);
        Address[] GetNearbyPlaces(double? latitude, double? longitude, string name = null, int? radius = null);		
	}
}

