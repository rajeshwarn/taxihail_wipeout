using MK.Common.Android.Entity;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/rapidaddress/", "Get")]
    public class CraftyClicksRequest : IReturn<CraftyClicksAddress>
    {
        public CraftyClicksRequest()
        {
            Response = "data_formatted";
            Include_Geocode = true;
        }

        public string PostalCode { get; set; }
        public string Key { get; set; }
        public string Response { get; private set; }
        public bool Include_Geocode { get; private set; }
    }
}