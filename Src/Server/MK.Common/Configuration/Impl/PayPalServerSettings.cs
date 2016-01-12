using System;
using apcurium.MK.Common.Cryptography;
using apcurium.MK.Common.Enumeration.PayPal;

namespace apcurium.MK.Common.Configuration.Impl
{
    [Obsolete("Kept for legacy support (order still in progress during update), use Braintree vZero instead")]
    public class PayPalServerSettings
    {
        public PayPalServerSettings()
        {
            SandboxCredentials = new PayPalServerCredentials();
            Credentials = new PayPalServerCredentials();
        }

		[PropertyEncrypt]
        public PayPalServerCredentials SandboxCredentials { get; set; }

		[PropertyEncrypt]
        public PayPalServerCredentials Credentials { get; set; }

        public LandingPageTypes LandingPageType { get; set; }
    }
}