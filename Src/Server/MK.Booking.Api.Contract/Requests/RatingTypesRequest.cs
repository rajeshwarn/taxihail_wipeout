#region

using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/ratingtypes", "GET")]
    [Route("/ratingtypes/{Language}", "GET")]
    public class RatingTypesRequest
    {
        public string Language { get; set; }
    }
}