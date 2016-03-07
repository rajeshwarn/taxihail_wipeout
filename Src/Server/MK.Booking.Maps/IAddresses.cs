#region

using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.MapDataProvider.Resources;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.MapDataProvider.Google.Resources;

#endregion

namespace apcurium.MK.Booking.Maps
{
    public interface IAddresses
    {
        [Obsolete("Use SearchAsync instead")]
		Address[] Search(string name, double? latitude, double? longitude, string currentLanguage, GeoResult geoResult = null);

        Task<Address[]> SearchAsync(string name, double? latitude, double? longitude, string currentLanguage, GeoResult geoResult = null);
    }
}