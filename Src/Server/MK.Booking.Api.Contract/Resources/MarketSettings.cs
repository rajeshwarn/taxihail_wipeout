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
    }
}
