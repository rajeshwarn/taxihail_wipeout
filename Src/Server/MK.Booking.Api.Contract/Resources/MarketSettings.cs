using apcurium.MK.Common.Entity;
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

        public bool OverrideEnableAppFareEstimates { get; set; }

        public bool EnableFutureBooking { get; set; }
        
        public Tariff MarketTariff { get; set; }
        
        public bool IsLocalMarket 
        {
            get { return !HashedMarket.HasValue(); }
        }
    }
}
