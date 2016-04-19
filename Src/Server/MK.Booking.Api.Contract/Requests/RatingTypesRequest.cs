#region

using System;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Http;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/ratingtypes", "GET,POST,PUT")]
    [RouteDescription("/ratingtypes/{ClientLanguage}", "GET")]
    [RouteDescription("/ratingtypes/{Id}", "DELETE")]
    public class RatingTypesRequest
    {
        public Guid Id { get; set; }
        public string ClientLanguage { get; set; }
        public RatingType[] RatingTypes { get; set; }
    }
}