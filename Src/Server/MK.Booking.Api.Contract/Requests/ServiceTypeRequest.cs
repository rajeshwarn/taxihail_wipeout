using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Enumeration;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
#if !CLIENT
    [AuthorizationRequired(ApplyTo.Post | ApplyTo.Put | ApplyTo.Delete, RoleName.Admin)]
#endif
    [Route("/admin/servicetypes", "GET")]
    [Route("/admin/servicetypes/{ServiceType}", "GET,PUT")]
    public class ServiceTypeRequest
    {
        public ServiceType? ServiceType { get; set; }

        public string IBSWebServicesUrl { get; set; }

        public int ProviderId { get; set; }
    }
}