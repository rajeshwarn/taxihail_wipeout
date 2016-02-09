using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Common.Configuration.Impl
{
    public class CmtPaymentSettings
    {
        public CmtPaymentSettings()
        {
#if DEBUG
            FleetToken = "270d2767ec30c1c2";
            ConsumerKey = "vmAoqWEY3zIvUCM4";
            ConsumerSecretKey = "DUWzh0jAldPc7C5I";
            FleetTokenLuxury = "270d2767ec30c1c2";
            ConsumerKeyLuxury = "vmAoqWEY3zIvUCM4";
            ConsumerSecretKeyLuxury = "DUWzh0jAldPc7C5I";
            SandboxMobileBaseUrl = "https://mobile-sandbox.cmtapi.com/v1/";
            SandboxBaseUrl = "https://payment-sandbox.cmtapi.com/v2/";
            MobileBaseUrl = "https://mobile.cmtapi.com/v1/";
            BaseUrl = "https://payment.cmtapi.com/v2/";
            IsSandbox = true;
            CurrencyCode = CurrencyCodes.Main.UnitedStatesDollar;
            Market = "PHL";
            SubmitAsFleetAuthorization = true;
#endif
        }

        public bool IsManualRidelinqCheckInEnabled { get; set; }

        public bool IsSandbox { get; set; }

		[PropertyEncrypt]
        public string BaseUrl { get; set; }

		[PropertyEncrypt]
        public string SandboxBaseUrl { get; set; }

		[PropertyEncrypt]
        public string MobileBaseUrl { get; set; }

		[PropertyEncrypt]
        public string SandboxMobileBaseUrl { get; set; }

		[PropertyEncrypt]
        public string ConsumerSecretKey { get; set; }

		[PropertyEncrypt]
        public string ConsumerKey { get; set; }

		[PropertyEncrypt]
        public string FleetToken { get; set; }

        public string ConsumerSecretKeyLuxury { get; set; }

        public string ConsumerKeyLuxury { get; set; }

        public string FleetTokenLuxury { get; set; }

        public string CurrencyCode { get; set; }

		[PropertyEncrypt]
        public string Market { get; set; }

        public bool SubmitAsFleetAuthorization { get; set; }

		[PropertyEncrypt]
        public string MerchantToken { get; set; }

        public bool UsePairingCode { get; set; }

        public class Credentials
        {
            public string ConsumerSecretKey { get; set; }

            public string ConsumerKey { get; set; }

            public string FleetToken { get; set; }
        }

        public Credentials GetCredentials(ServiceType serviceType)
        {
            return serviceType == ServiceType.Luxury
                ? new Credentials
                {
                    ConsumerKey = ConsumerKeyLuxury,
                    ConsumerSecretKey = ConsumerSecretKeyLuxury,
                    FleetToken = FleetToken
                }
                : new Credentials
                {
                    ConsumerKey = ConsumerKey,
                    ConsumerSecretKey = ConsumerSecretKey,
                    FleetToken = FleetTokenLuxury
                };
        }
    }
}