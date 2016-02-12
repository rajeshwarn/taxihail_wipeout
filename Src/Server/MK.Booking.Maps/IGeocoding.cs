#region

using System.Threading.Tasks;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.MapDataProvider.Google.Resources;

#endregion

namespace apcurium.MK.Booking.Maps
{
    public interface IGeocoding
    {
        Address[] Search(string query, double? pickupLatitude, double? pickupLongitude, string currentLanguage, GeoResult geoResult = null);

        Task<Address[]> SearchAsync(string query, double? pickupLatitude, double? pickupLongitude, string currentLanguage, GeoResult geoResult = null);

		Address[] Search(double latitude, double longitude, string currentLanguage, GeoResult geoResult = null, bool searchPopularAddresses = false);

        Task<Address[]> SearchAsync(double latitude, double longitude, string currentLanguage, GeoResult geoResult = null, bool searchPopularAddresses = false);

        Task<Address> GetPlaceDetail(string placeId);
    }
}