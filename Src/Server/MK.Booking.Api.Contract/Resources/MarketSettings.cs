using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class MarketSettings
    {
        public MarketSettings()
        {
            HashedMarket = string.Empty;
        }

        public string HashedMarket { get; set; }

        public bool EnableDriverBonus { get; set; }
        
        public bool EnableFutureBooking { get; set; }

        public bool OverrideEnableAppFareEstimates { get; set; }
        public double MinimumRate { get; set; }
        public decimal FlatRate { get; set; }
        public double KilometricRate { get; set; }
        public double PerMinuteRate { get; set; }
        public double KilometerIncluded { get; set; }
        public double MarginOfError { get; set; }
        
        public bool IsLocalMarket 
        {
            get { return !HashedMarket.HasValue(); }
        }
    }
}
