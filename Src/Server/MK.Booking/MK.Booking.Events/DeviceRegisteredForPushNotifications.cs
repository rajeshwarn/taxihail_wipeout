#region

using apcurium.MK.Common.Enumeration;
using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class DeviceRegisteredForPushNotifications : VersionedEvent
    {
        public string DeviceToken { get; set; }
        public PushNotificationServicePlatform Platform { get; set; }
    }
}