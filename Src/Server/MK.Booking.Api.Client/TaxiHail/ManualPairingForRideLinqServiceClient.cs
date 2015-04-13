using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.Extensions;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Resources;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class ManualPairingForRideLinqServiceClient : BaseServiceClient
    {
        public ManualPairingForRideLinqServiceClient(string url, string sessionId, IPackageInfo packageInfo)
            : base(url, sessionId, packageInfo)
        {
            
        }

		public Task<ManualRideLinqResponse> Pair(ManualRideLinqPairingRequest manualRideLinqPairingRequest)
        {
            var req = string.Format("/account/ridelinq");
            return Client.PostAsync<ManualRideLinqResponse>(req, manualRideLinqPairingRequest);
        }

        public Task Unpair(Guid orderId)
        {
            var req = string.Format("/account/ridelinq/{0}", orderId);
            return Client.DeleteAsync<string>(req);
        }

        public Task<ManualRideLinqResponse> GetUpdatedTrip(Guid orderId)
        {
            var req = string.Format("/account/ridelinq/{0}", orderId);
            return Client.GetAsync<ManualRideLinqResponse>(req);
        }

    }
}