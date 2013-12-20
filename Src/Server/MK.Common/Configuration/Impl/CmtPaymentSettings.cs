namespace apcurium.MK.Common.Configuration.Impl
{
    public class CmtPaymentSettings
    {
        public CmtPaymentSettings()
        {
#if DEBUG
            MerchantToken = "E4AFE87B0E864228200FA947C4A5A5F98E02AA7A3CFE907B0AD33B56D61D2D13E0A75F51641AB031500BD3C5BDACC114";
            CustomerKey = "AH7j9KweF235hP";
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

        public string CustomerKey { get; set; }

        public string MerchantToken { get; set; }

        public string CurrencyCode { get; set; }
        
    }
}
