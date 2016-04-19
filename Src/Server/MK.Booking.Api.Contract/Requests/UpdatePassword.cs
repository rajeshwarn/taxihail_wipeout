#region

using System;
using apcurium.MK.Common.Http;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/accounts/{AccountId}/updatePassword", "POST")]
    public class UpdatePassword : BaseDto
    {
        public Guid AccountId { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}