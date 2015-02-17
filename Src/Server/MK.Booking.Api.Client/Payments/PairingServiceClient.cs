using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.Extensions;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Resources;
using ServiceStack.ServiceClient.Web;

namespace apcurium.MK.Booking.Api.Client.Payments
{
    public class PairingServiceClient : BaseServiceClient, IPairingServiceClient
    {
        public PairingServiceClient(string url, string sessionId, IPackageInfo packageInfo)
            : base(url, sessionId, packageInfo)
        {
        }

        public async Task<PairingResponse> Pair(Guid orderId, string cardToken, int? autoTipPercentage)
        {
            try
            {
                var response = await Client.PostAsync(new PairingForPaymentRequest
                {
                    OrderId = orderId,
                    CardToken = cardToken,
                    AutoTipPercentage = autoTipPercentage

                });
                return response;
            }
            catch (WebServiceException)
            {
                return new PairingResponse { IsSuccessful = false };
            }
        }

        public async Task<BasePaymentResponse> Unpair(Guid orderId)
        {
            try
            {
                var response = await Client.PostAsync(new UnpairingForPaymentRequest
                {
                    OrderId = orderId
                });
                return response;
            }
            catch (WebServiceException)
            {
                return new PairingResponse { IsSuccessful = false };
            }
        }
    }
}