using System;
using System.Threading.Tasks;
using apcurium.MK.Common;


#if !CLIENT
using apcurium.MK.Booking.Api.Client.Extensions;
#endif
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests.Payment.PayPal;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Resources;

namespace apcurium.MK.Booking.Api.Client.Payments.PayPal
{
    public class PayPalServiceClient : BaseServiceClient
    {
        public PayPalServiceClient(string url, string sessionId, IPackageInfo packageInfo, IConnectivityService connectivityService)
            : base(url, sessionId, packageInfo, connectivityService)
        {
        }

        public Task<BasePaymentResponse> LinkPayPalAccount(LinkPayPalAccountRequest request)
        {
            return Client.PostAsync(request);
        }

        public Task<BasePaymentResponse> UnlinkPayPalAccount(UnlinkPayPalAccountRequest request)
        {
            return Client.PostAsync(request);
        }
    }
}
