#region

using System.Globalization;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.Extensions;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Infrastructure;

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class DirectionsServiceClient : BaseServiceClient
    {
        public DirectionsServiceClient(string url, string sessionId, IPackageInfo packageInfo)
            : base(url, sessionId, packageInfo)
        {
        }


        public Task<DirectionInfo> GetDirectionDistance(double originLatitude, double originLongitude,
            double destinationLatitude, double destinationLongitude)
        {
            var resource = string.Format(CultureInfo.InvariantCulture,
                "/directions?OriginLat={0}&OriginLng={1}&DestinationLat={2}&DestinationLng={3}", originLatitude,
                originLongitude, destinationLatitude, destinationLongitude);
            var result = Client.GetAsync<DirectionInfo>(resource);
            return result;
        }
    }
}