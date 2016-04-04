#region

using ServiceStack.ServiceHost;

#endregion



namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/accounts/resetpassword/{EmailAddress}", "POST")]
    public class ResetPassword : BaseDto
    {
        public string EmailAddress { get; set; }
    }
}