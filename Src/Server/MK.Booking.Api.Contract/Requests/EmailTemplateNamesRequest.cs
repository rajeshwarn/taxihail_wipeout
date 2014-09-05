using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Security;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
#if !CLIENT
    [AuthorizationRequired(ApplyTo.Get, RoleName.Admin)]
#endif
    [Route("/admin/testemail/templates", "GET")]
    public class EmailTemplateNamesRequest
    {
    }
}