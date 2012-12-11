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
    [RestService("/admin/addresses", "GET")]
    [RestService("/admin/addresses", "POST")]
    [RestService("/admin/addresses/{Id}", "PUT, DELETE")]
    public class DefaultFavoriteAddress : BaseDTO
    {
        public Guid Id { get; set; }
        public Address Address { get; set; }
    }
}
