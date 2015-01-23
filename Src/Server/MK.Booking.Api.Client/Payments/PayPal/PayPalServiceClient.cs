using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests.Payment.PayPal;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Api.Client.Extensions;
using apcurium.MK.Common.Resources;

namespace apcurium.MK.Booking.Api.Client.Payments.PayPal
{
    public class PayPalServiceClient : BaseServiceClient
    {
        public PayPalServiceClient(string url, string sessionId, IPackageInfo packageInfo)
            : base(url, sessionId, packageInfo)
        {
        }

        public Task<BasePaymentResponse> LinkAccount(LinkPayPalAccountRequest request)
        {
            return Client.PostAsync(request);
        }

        public Task<BasePaymentResponse> UnLinkAccount(UnlinkPayPalAccountRequest request)
        {
            return Client.PostAsync(request);
        }
    }
}
