#region

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Entity;
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
        public string ClientLanguage { get; set; }
        public RatingType[] RatingTypes { get; set; }
    }
}