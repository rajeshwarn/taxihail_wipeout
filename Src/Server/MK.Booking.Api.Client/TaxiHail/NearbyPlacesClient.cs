#region

using System.Globalization;
using System.Threading.Tasks;
#if !CLIENT
using apcurium.MK.Booking.Api.Client.Extensions;
#endif
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class NearbyPlacesClient : BaseServiceClient
    {
        public NearbyPlacesClient(string url, string sessionId, IPackageInfo packageInfo)
            : base(url, sessionId, packageInfo)
        {
        }


        public Task<Address[]> GetNearbyPlaces(double? latitude, double? longitude, int? radius)
        {
            var result =
                Client.GetAsync<Address[]>(string.Format(CultureInfo.InvariantCulture, "/places?lat={0}&lng={1}&radius={2}",
                    latitude, longitude, radius));
            return result;
        }

        public Task<Address[]> GetNearbyPlaces(double? latitude, double? longitude)
        {
            var result =
                Client.GetAsync<Address[]>(string.Format(CultureInfo.InvariantCulture, "/places?lat={0}&lng={1}", latitude,
                    longitude));
            return result;
        }
    }
}