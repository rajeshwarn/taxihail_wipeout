#region

using System;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [Route("/accounts/{AccountId}/updatePassword", "POST")]
    public class UpdatePassword : BaseDto
    {
        public Guid AccountId { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}