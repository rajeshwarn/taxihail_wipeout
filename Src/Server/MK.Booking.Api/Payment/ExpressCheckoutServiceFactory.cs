using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MK.Booking.PayPal;

namespace apcurium.MK.Booking.Api.Payment
{
    public class ExpressCheckoutServiceFactory
    {
        public ExpressCheckoutServiceClient CreateService()
        {
            return new ExpressCheckoutServiceClient(new SandboxCredentials(), new RegionInfo("en-US"), "http://www.google.com", "http://www.google.com", useSandbox: true);
        }
        /// <summary>
        /// Temporary, for development purpose only
        /// </summary>
        private class SandboxCredentials: IPayPalCredentials
        {
            public string Username
            {
                get { return "vincent.costel-facilitator_api1.gmail.com"; }
            }
            public string Password
            {
                get { return "1372362468"; }
            }
            public string Signature
            {
                get { return "AFcWxV21C7fd0v3bYYYRCpSSRl31ADYXGX.gPsewqg6pNBBa9JL5zoCL"; }
            }
        }
    }
}
