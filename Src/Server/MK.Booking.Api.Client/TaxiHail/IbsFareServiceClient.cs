using System.Globalization;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;
using MK.Common.Android;
using apcurium.MK.Booking.Mobile;
using System;
using System.Threading.Tasks;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
	public class IbsFareServiceClient: BaseServiceClient, IIbsFareClient
    {
        public IbsFareServiceClient(string url, string sessionId, string userAgent)
            : base(url, sessionId, userAgent)
        {
            
        }


        public DirectionInfo GetDirectionInfoFromIbs(double pickupLatitude, double pickupLongitude, double dropoffLatitude, double dropoffLongitude)
        {
            var req = string.Format(CultureInfo.InvariantCulture, "/ibsfare?PickupLatitude={0}&PickupLongitude={1}&DropoffLatitude={2}&DropoffLongitude={3}", pickupLatitude, pickupLongitude, dropoffLatitude, dropoffLongitude);
            var result = Client.Get<DirectionInfo>(req);
            return result;
        }
    }
}
