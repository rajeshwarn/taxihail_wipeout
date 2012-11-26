using System.Globalization;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class PlaceDetailServiceClient: BaseServiceClient
    {
        public PlaceDetailServiceClient(string url, string sessionId)
            : base(url, sessionId)
        {
        }

         
        public Address GetPlaceDetail(string reference)
        {
            var result = Client.Get<Address>(string.Format(CultureInfo.InvariantCulture, "/places/{0}", reference));
            return result;
        }
    }
}
