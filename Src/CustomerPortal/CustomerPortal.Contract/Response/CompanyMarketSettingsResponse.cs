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

        public bool EnableAppFareEstimates { get; set; }
        public double MinimumRate { get; set; }
        public decimal FlatRate { get; set; }
        public double KilometricRate { get; set; }
        public double PerMinuteRate { get; set; }
        public double KilometerIncluded { get; set; }
        public double MarginOfError { get; set; }
    }
}