using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;


namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [RestService("/account/resetpassword/{EmailAddress}", "POST")]
    public class ResetPassword : BaseDTO
    { 
        public string EmailAddress { get; set; } 
    }
}
