using System.Globalization;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class PlaceDetailServiceClient: BaseServiceClient
    {
        public PlaceDetailServiceClient(string url, string sessionId)
            : base(url, sessionId)
        {
        }

         
        public Address GetPlaceDetail(string reference, string placeName)
        {
            var result = Client.Get(new PlaceDetailRequest()
                {
                    ReferenceId = reference,
                    PlaceName = placeName
                });
            return result;
        }
    }
}
