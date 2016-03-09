#region

using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/admin/pushnotifications/{EmailAddress}", "POST")]
    public class PushNotificationAdministrationRequest
    {
        public string EmailAddress { get; set; }
        public string Message { get; set; }
    }
}