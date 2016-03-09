#region

using System;
using apcurium.MK.Common.Entity;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/ratingtypes", "GET,POST,PUT")]
    [Route("/ratingtypes/{ClientLanguage}", "GET")]
    [Route("/ratingtypes/{Id}", "DELETE")]
    public class RatingTypesRequest
    {
        public Guid Id { get; set; }
        public string ClientLanguage { get; set; }
        public RatingType[] RatingTypes { get; set; }
    }
}