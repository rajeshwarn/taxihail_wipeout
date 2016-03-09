#region

using System;
using apcurium.MK.Common.Entity;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/admin/popularaddresses", "POST")]
    [Route("/admin/popularaddresses/{Id}", "PUT, DELETE")]
    public class PopularAddress : BaseDto
    {
        public Guid Id { get; set; }
        public Address Address { get; set; }
    }
}