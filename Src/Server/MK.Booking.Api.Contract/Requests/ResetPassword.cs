#region


#endregion

using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/accounts/resetpassword/{EmailAddress}", "POST")]
    public class ResetPassword : BaseDto
    {
        public string EmailAddress { get; set; }
    }
}