using System;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [RestService("/account/addresses", "POST")]
    [RestService("/account/addresses/{Id}", "PUT, DELETE")]
    public class SaveAddress
    {
        public Guid Id { get; set; }
        public Address Address { get; set; }
    }
}
