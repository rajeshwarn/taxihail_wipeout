#region

using apcurium.MK.Common.Http;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/admin/pushnotifications/{EmailAddress}", "POST")]
    public class PushNotificationAdministrationRequest
    {
        public string EmailAddress { get; set; }
        public string Message { get; set; }
    }
}