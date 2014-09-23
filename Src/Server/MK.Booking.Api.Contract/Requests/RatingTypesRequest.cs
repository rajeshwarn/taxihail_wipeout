#region

using System;
using System.Runtime.CompilerServices;
using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Security;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
#if !CLIENT
    [AuthorizationRequired(ApplyTo.Post | ApplyTo.Put | ApplyTo.Delete, RoleName.Admin)]
#endif
    [Route("/ratingtypes", "GET,POST,PUT")]
    [Route("/ratingtypes/{Language}", "GET")]
    [Route("/ratingtypes/{Id}", "DELETE")]
    public class RatingTypesRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Language { get; set; }
    }
}