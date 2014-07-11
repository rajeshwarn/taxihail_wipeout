#region
using apcurium.MK.Booking.Api.Client.Extensions;

using System.Globalization;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Infrastructure;

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class IbsFareServiceClient : BaseServiceClient, IIbsFareClient
    {
        public IbsFareServiceClient(string url, string sessionId, IPackageInfo packageInfo)
            : base(url, sessionId, packageInfo)
        {
        }


        public Task<DirectionInfo> GetDirectionInfoFromIbs(double pickupLatitude, double pickupLongitude,
            double dropoffLatitude, double dropoffLongitude)
        {
            var req = string.Format(CultureInfo.InvariantCulture,
                "/ibsfare?PickupLatitude={0}&PickupLongitude={1}&DropoffLatitude={2}&DropoffLongitude={3}",
                pickupLatitude, pickupLongitude, dropoffLatitude, dropoffLongitude);
            var result = Client.GetAsync<DirectionInfo>(req);
            return result;
        }
    }
}