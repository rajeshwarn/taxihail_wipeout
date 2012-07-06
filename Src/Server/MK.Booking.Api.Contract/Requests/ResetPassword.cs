using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RestService("/account/resetpassword/{EmailAddress}", "POST")]
    public class ResetPassword
    {
        public string EmailAddress { get; set; } 
    }
}
