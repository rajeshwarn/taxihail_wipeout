namespace apcurium.MK.Common.Configuration.Impl
{
    public class CmtPaymentSettings
    {
        public CmtPaymentSettings()
        {
#if DEBUG
            // TODO Change these values when we have them

            FleetToken = "270d2767ec30c1c2";
            ConsumerKey = "AH7j9KweF235hP";
            ConsumerSecretKey = "K09JucBn23dDrehZa";
            SandboxMobileBaseUrl = "https://mobile-sandbox.cmtapi.com/";
            SandboxBaseUrl = "https://payment-sandbox.cmtapi.com/";
            MobileBaseUrl = "https://mobile.cmtapi.com/";
            BaseUrl = "https://payment.cmtapi.com/";
            IsSandbox = true;
            CurrencyCode = CurrencyCodes.Main.UnitedStatesDollar;
#endif
        }

        public bool IsSandbox { get; set; }

        public string BaseUrl { get; set; }

        public string SandboxBaseUrl { get; set; }

        public string MobileBaseUrl { get; set; }

        public string SandboxMobileBaseUrl { get; set; }

        public string ConsumerSecretKey { get; set; }

        public string ConsumerKey { get; set; }

        public string FleetToken { get; set; }

        public string CurrencyCode { get; set; }
    }
}