using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Security;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
#if !CLIENT
    [AuthorizationRequired(ApplyTo.Post, RoleName.Admin)]
#endif
    [Route("/admin/testemail/{EmailAddress}", "POST")]
    public class TestEmailAdministrationRequest
    {
        public string EmailAddress { get; set; }
        public string TemplateName { get; set; }
    }
}