#region

using System;
using apcurium.MK.Common.Http;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/accounts/addresses/history/{AddressId}", "DELETE")]
    [RouteDescription("/accounts/addresses/history", "GET")]
    public class AddressHistoryRequest : BaseDto
    {
        public Guid AddressId { get; set; }
    }
}