using System.Threading.Tasks;
using apcurium.MK.Common;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests.Payment.PayPal;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Resources;

namespace apcurium.MK.Booking.Api.Client.Payments.PayPal
{
    public class PayPalServiceClient : BaseServiceClient
    {
        public PayPalServiceClient(string url, string sessionId, IPackageInfo packageInfo, IConnectivityService connectivityService, ILogger logger)
            : base(url, sessionId, packageInfo, connectivityService, logger)
        {
        }

        public Task<BasePaymentResponse> LinkPayPalAccount(LinkPayPalAccountRequest request)
        {
            return Client.PostAsync(request, Logger);
        }

        public Task<BasePaymentResponse> UnlinkPayPalAccount(UnlinkPayPalAccountRequest request)
        {
            return Client.PostAsync(request, Logger);
        }
    }
}
