using System.Threading.Tasks;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Maps
{
    public interface IPlaces
    {
        Task<Address> GetPlaceDetail(string name, string placeId);
		Address[] SearchPlaces(string query, double? latitude, double? longitude, string currentLanguage);
        Task<Address[]> GetFilteredPlacesList();
    }
}