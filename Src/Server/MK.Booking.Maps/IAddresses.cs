#region

using apcurium.MK.Booking.Google.Resources;
using apcurium.MK.Common.Entity;

#endregion

namespace apcurium.MK.Booking.Maps
{
    public interface IAddresses
    {
        Address[] Search(string name, double? latitude, double? longitude, GeoResult geoResult = null);
    }
}