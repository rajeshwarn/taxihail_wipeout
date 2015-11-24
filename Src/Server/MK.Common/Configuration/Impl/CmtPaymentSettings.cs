using apcurium.MK.Common.Cryptography;
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

        public string CurrencyCode { get; set; }

		[PropertyEncrypt]
        public string Market { get; set; }

        public bool SubmitAsFleetAuthorization { get; set; }

		[PropertyEncrypt]
        public string MerchantToken { get; set; }

        public bool UsePairingCode { get; set; }
    }
}