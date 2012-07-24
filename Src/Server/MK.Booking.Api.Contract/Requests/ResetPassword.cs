using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;


namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [RestService("/accounts/resetpassword/{EmailAddress}", "POST")]
    public class ResetPassword : BaseDTO
    { 
        public string EmailAddress { get; set; } 
    }
}
