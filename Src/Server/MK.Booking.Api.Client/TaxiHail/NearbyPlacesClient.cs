#region

using System.Globalization;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Entity;

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class NearbyPlacesClient : BaseServiceClient
    {
        public NearbyPlacesClient(string url, string sessionId, IPackageInfo packageInfo)
            : base(url, sessionId, packageInfo)
        {
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