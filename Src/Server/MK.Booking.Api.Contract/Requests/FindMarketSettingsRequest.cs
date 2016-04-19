using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/roaming/marketsettings", "GET")]
    public class FindMarketSettingsRequest
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }
}
