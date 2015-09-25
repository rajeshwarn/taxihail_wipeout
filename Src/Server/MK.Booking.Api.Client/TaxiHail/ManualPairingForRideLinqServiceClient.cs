using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.Extensions;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Mobile.Infrastructure;
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
            var req = "/account/manualridelinq/pair";
            return Client.PostAsync<ManualRideLinqResponse>(req, manualRideLinqPairingRequest);
        }

        public Task Unpair(Guid orderId)
        {
            var req = string.Format("/account/manualridelinq/{0}/unpair", orderId);
            return Client.DeleteAsync<string>(req);
        }

        public Task<ManualRideLinqResponse> UpdateAutoTip(Guid orderId, int autoTipPercentage)
        {
            var req = string.Format("/account/manualridelinq/{0}/pairing/tip", orderId);
            return Client.PutAsync<ManualRideLinqResponse>(req, new ManualRideLinqUpdateAutoTipRequest
            {
                OrderId = orderId,
                AutoTipPercentage = autoTipPercentage
            });
        }

        public Task<ManualRideLinqResponse> GetUpdatedTrip(Guid orderId)
        {
            var req = string.Format("/account/manualridelinq/{0}/status", orderId);
            return Client.GetAsync<ManualRideLinqResponse>(req);
        }
    }
}