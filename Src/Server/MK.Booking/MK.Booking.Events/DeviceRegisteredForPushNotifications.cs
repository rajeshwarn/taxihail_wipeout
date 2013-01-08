using Infrastructure.EventSourcing;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.Events
{
    public class DeviceRegisteredForPushNotifications: VersionedEvent
    {
        public string DeviceToken { get; set; }
        public PushNotificationServicePlatform Platform { get; set; }
    }
}
