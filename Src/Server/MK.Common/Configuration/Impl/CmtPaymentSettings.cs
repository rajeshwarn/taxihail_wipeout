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
            SandboxMobileBaseUrl = "https://mobile-sandbox.cmtapi.com/";
            SandboxBaseUrl = "https://payment-sandbox.cmtapi.com/";
            MobileBaseUrl = "https://mobile.cmtapi.com/";
            BaseUrl = "https://payment.cmtapi.com/";
            IsSandbox = true;
            CurrencyCode = CurrencyCodes.Main.UnitedStatesDollar;
            Market = "PHL";
#endif
        }

        public bool IsManualRidelinqCheckInEnabled { get; set; }

        public bool IsSandbox { get; set; }

        public string BaseUrl { get; set; }

        public string SandboxBaseUrl { get; set; }

        public string MobileBaseUrl { get; set; }

        public string SandboxMobileBaseUrl { get; set; }

        public string ConsumerSecretKey { get; set; }

        public string ConsumerKey { get; set; }

        public string FleetToken { get; set; }

        public string CurrencyCode { get; set; }

        public string Market { get; set; }
    }
}