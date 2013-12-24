#region

using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Security;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
#if !CLIENT
    [AuthorizationRequired(ApplyTo.Post, RoleName.Admin)]
#endif
    [Route("/admin/pushnotifications/{EmailAddress}", "POST")]
    public class PushNotificationAdministrationRequest
    {
        public string EmailAddress { get; set; }
        public string Message { get; set; }
    }
}