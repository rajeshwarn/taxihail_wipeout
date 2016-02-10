using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Resources;
using apcurium.MK.Common;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class ManualPairingForRideLinqServiceClient : BaseServiceClient
    {
        public ManualPairingForRideLinqServiceClient(string url, string sessionId, IPackageInfo packageInfo, IConnectivityService connectivityService, ILogger logger)
            : base(url, sessionId, packageInfo, connectivityService, logger)
        {
        }

		public Task<ManualRideLinqResponse> Pair(ManualRideLinqPairingRequest manualRideLinqPairingRequest)
		{
            return Client.PostAsync<ManualRideLinqResponse>("/account/manualridelinq/pair", manualRideLinqPairingRequest, logger: Logger);
		}

        public Task Unpair(Guid orderId)
        {
            var req = string.Format("/account/manualridelinq/{0}/unpair", orderId);
            return Client.DeleteAsync<string>(req, logger: Logger);
        }

        public Task<ManualRideLinqResponse> UpdateAutoTip(Guid orderId, int autoTipPercentage)
        {
            var req = string.Format("/account/manualridelinq/{0}/pairing/tip", orderId);
            return Client.PutAsync<ManualRideLinqResponse>(req, new ManualRideLinqUpdateAutoTipRequest
            {
                OrderId = orderId,
                AutoTipPercentage = autoTipPercentage
            }, logger: Logger);
        }

        public Task<ManualRideLinqResponse> GetUpdatedTrip(Guid orderId)
        {
            var req = string.Format("/account/manualridelinq/{0}/status", orderId);
            return Client.GetAsync<ManualRideLinqResponse>(req, logger: Logger);
        }
    }
}