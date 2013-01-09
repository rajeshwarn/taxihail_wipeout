using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [Route("/account/pushnotifications/{DeviceToken}", "POST,DELETE")]
    public class PushNotificationRegistration
    {
        public string DeviceToken { get; set; }
        public PushNotificationServicePlatform Platform { get; set; }
    }
}
