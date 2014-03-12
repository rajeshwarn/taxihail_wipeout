#region

using System;
using apcurium.MK.Common.Entity;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [Route("/account/addresses", "POST")]
    [Route("/account/addresses/{Id}", "PUT, DELETE")]
    public class SaveAddress
    {
        public Guid Id { get; set; }
        public Address Address { get; set; }
    }
}