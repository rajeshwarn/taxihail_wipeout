#region


using apcurium.MK.Booking.MapDataProvider.Resources;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.MapDataProvider.Google.Resources;

#endregion

namespace apcurium.MK.Booking.Maps
{
    public interface IGeocoding
    {
		Address[] Search(string addressName, string currentLanguage, GeoResult geoResult = null);

		Address[] Search(double latitude, double longitude, string currentLanguage, GeoResult geoResult = null,
            bool searchPopularAddresses = false);
    }
}