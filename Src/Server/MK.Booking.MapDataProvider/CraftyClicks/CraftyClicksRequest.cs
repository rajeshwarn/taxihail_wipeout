using apcurium.MK.Booking.MapDataProvider.CraftyClicks.Resources;
namespace apcurium.MK.Booking.MapDataProvider.CraftyClicks
{
    [Route("/rapidaddress", "POST")]
    public class CraftyClicksRequest : IReturn<CraftyClicksAddress>
    {
        public CraftyClicksRequest()
        {
            Response = "data_formatted";
            Include_geocode = true;
            Sort = "asc";
        }
        public string Postcode { get; set; }
        public string Key { get; set; }
        public string Response { get; private set; }
        public bool Include_geocode { get; private set; }

        public string Sort { get; private set; }
    }
}