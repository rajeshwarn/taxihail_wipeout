using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Security;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [AuthorizationRequired(ApplyTo.All, RoleName.Admin)]
    [RestService("/account/admindisable", "PUT")]
    public class DisableAccountByAdminRequest : BaseDTO
    {
        public string AccountEmail { get; set; }
    }
}