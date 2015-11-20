#region

using System.Globalization;
using System.Threading.Tasks;
using apcurium.MK.Common.Extensions;
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
            double dropoffLatitude, double dropoffLongitude, 
			string pickupZipCode, string dropoffZipCode,
			string accountNumber, int? tripDurationInSeconds,
			int? vehicleType)
        {
            var req = string.Format(CultureInfo.InvariantCulture,
                "/ibsfare?PickupLatitude={0}&PickupLongitude={1}&DropoffLatitude={2}&DropoffLongitude={3}&" +
				"PickupZipCode={4}&DropoffZipCode={5}&" +
				"AccountNumber={6}&CustomerNumber={7}&TripDurationInSeconds={8}&vehicleType={9}",
                pickupLatitude, pickupLongitude, dropoffLatitude, dropoffLongitude, 
				pickupZipCode, dropoffZipCode,
				accountNumber.ToSafeString(), 0, tripDurationInSeconds.HasValue ? tripDurationInSeconds.ToString() : "", vehicleType);
            var result = Client.GetAsync<DirectionInfo>(req);
            return result;
        }
    }
}