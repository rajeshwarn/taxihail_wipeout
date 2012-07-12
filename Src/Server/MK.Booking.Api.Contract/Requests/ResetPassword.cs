using ServiceStack.ServiceHost;


namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RestService("/accounts/resetpassword/{EmailAddress}", "POST")]
    public class ResetPassword
    {
        public string EmailAddress { get; set; } 
    }
}
