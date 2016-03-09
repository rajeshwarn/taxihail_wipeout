#region

using System;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/account/addresses/history/{AddressId}", "DELETE")]
    [Route("/account/addresses/history", "GET")]
    public class AddressHistoryRequest : BaseDto
    {
        public Guid AddressId { get; set; }
    }
}