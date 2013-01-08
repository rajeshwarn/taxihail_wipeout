using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class DeviceRegisteredForPushNotifications: VersionedEvent
    {
        public object DeviceToken { get; set; }
        public object Platform { get; set; }
    }
}
