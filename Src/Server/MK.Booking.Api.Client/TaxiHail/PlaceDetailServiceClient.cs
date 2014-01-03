#region

using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Entity;

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class PlaceDetailServiceClient : BaseServiceClient
    {
        public PlaceDetailServiceClient(string url, string sessionId, string userAgent)
            : base(url, sessionId, userAgent)
        {
        }


        public Address GetPlaceDetail(string reference, string placeName)
        {
            var result = Client.Get(new PlaceDetailRequest
            {
                ReferenceId = reference,
                PlaceName = placeName
            });
            return result;
        }
    }
}