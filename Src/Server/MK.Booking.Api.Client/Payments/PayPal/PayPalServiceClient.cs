using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests.Payment.PayPal;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Api.Client.Extensions;

namespace apcurium.MK.Booking.Api.Client.Payments.PayPal
{
    public class PayPalServiceClient : BaseServiceClient
    {
        public PayPalServiceClient(string url, string sessionId, IPackageInfo packageInfo)
            : base(url, sessionId, packageInfo)
        {
        }

        public Task LinkAccount(LinkPayPalAccountRequest request)
        {
            var req = string.Format("/paypal/{0}/link", request.AccountId);
            return Client.PostAsync<string>(req, request);
        }

        public Task UnLinkAccount(UnlinkPayPalAccountRequest request)
        {
            var req = string.Format("/paypal/{0}/unlink", request.AccountId);
            return Client.PostAsync<string>(req, request);
        }
    }
}
