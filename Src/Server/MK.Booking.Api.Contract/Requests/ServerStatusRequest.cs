
using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Security;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/status", "GET")]
    [Authenticate]
    [AuthorizationRequired(ApplyTo.Get, RoleName.Support, RoleName.Admin, RoleName.SuperAdmin)]
    public class ServerStatusRequest
    {
    }
}
