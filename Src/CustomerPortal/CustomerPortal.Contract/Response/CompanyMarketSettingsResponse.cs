using CustomerPortal.Contract.Resources;

namespace CustomerPortal.Contract.Response
{
    public class CompanyMarketSettingsResponse
    {
        public CompanyMarketSettingsResponse()
        {
            DispatcherSettings = new DispatcherSettings();
        }

        public string Market { get; set; }

        public bool EnableDriverBonus { get; set; }

        public string ReceiptFooter { get; set; }

        public DispatcherSettings DispatcherSettings { get; set; }
    }
}