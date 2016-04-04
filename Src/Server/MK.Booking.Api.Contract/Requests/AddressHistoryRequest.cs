#region

using System;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/accounts/addresses/history/{AddressId}", "DELETE")]
    [Route("/accounts/addresses/history", "GET")]
    public class AddressHistoryRequest : BaseDto
    {
        public Guid AddressId { get; set; }
    }
}