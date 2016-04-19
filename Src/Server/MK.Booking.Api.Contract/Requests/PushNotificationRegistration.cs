#region

using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Http;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/accounts/pushnotifications/{DeviceToken}", "POST,DELETE")]
    public class PushNotificationRegistration
    {
        public string DeviceToken { get; set; }
        public string OldDeviceToken { get; set; }
        public PushNotificationServicePlatform Platform { get; set; }
    }
}