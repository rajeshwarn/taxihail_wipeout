using System;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [AuthorizationRequired(ApplyTo.All, Permissions.Admin)]
    [RestService("/admin/popularaddresses", "POST")]
    [RestService("/admin/popularaddresses/{Id}", "PUT, DELETE")]
    public class PopularAddress : BaseDTO
    {
        public Guid Id { get; set; }
        public Address Address { get; set; }
    }
}
