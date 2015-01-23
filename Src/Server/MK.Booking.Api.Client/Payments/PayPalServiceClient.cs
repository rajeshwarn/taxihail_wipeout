using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Api.Client.Payments.PayPal
{
    public class PayPalServiceClient : BaseServiceClient
    {
        public PayPalServiceClient(string url, string sessionId, IPackageInfo packageInfo)
            : base(url, sessionId, packageInfo)
        {
        }

        public async Task LinkAccount(string authCode)
        {
            // TODO
            throw new NotImplementedException("TODO");
        }

        public async Task UnLinkAccount()
        {
            // TODO
            throw new NotImplementedException("TODO");
        }
    }
}
