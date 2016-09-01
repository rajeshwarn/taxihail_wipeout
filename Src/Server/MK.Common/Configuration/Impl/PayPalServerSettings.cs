using apcurium.MK.Common.Cryptography;
using apcurium.MK.Common.Enumeration.PayPal;

namespace apcurium.MK.Common.Configuration.Impl
{
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