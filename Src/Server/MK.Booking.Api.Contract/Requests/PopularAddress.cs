#region

using System;
using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Entity;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [AuthorizationRequired(ApplyTo.All, RoleName.Admin)]
    [Route("/admin/popularaddresses", "POST")]
    [Route("/admin/popularaddresses/{Id}", "PUT, DELETE")]
    public class PopularAddress : BaseDto
    {
        public Guid Id { get; set; }
        public Address Address { get; set; }
    }
}