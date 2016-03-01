using System;
using System.Threading.Tasks;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Resources;
using apcurium.MK.Common;
using apcurium.MK.Common.Diagnostic;

#if CLIENT
using MK.Common.Exceptions;
#else
using apcurium.MK.Booking.Api.Client.Extensions;
using ServiceStack.ServiceClient.Web;
#endif

namespace apcurium.MK.Booking.Api.Client.Payments
{
    public class PairingServiceClient : BaseServiceClient, IPairingServiceClient
    {
        public PairingServiceClient(string url, string sessionId, IPackageInfo packageInfo, IConnectivityService connectivityService, ILogger logger)
            : base(url, sessionId, packageInfo, connectivityService, logger)
        {
        }

        public async Task<BasePaymentResponse> Unpair(Guid orderId)
        {
            try
            {
                var response = await Client.PostAsync(new UnpairingForPaymentRequest
                {
                    OrderId = orderId
                }, Logger);
                return response;
            }
            catch (WebServiceException)
            {
                return new PairingResponse { IsSuccessful = false };
            }
        }

        public async Task<bool> UpdateAutoTip(Guid orderId, int autoTipPercentage)
        {
            try
            {
                var req = string.Format("/account/orders/{0}/pairing/tip", orderId);

                await Client.PostAsync<string>(req, new UpdateAutoTipRequest
                {
                    OrderId = orderId,
                    AutoTipPercentage = autoTipPercentage
                }, logger: Logger);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}