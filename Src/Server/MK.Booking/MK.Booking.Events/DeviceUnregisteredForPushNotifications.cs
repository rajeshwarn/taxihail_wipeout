using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class DeviceUnregisteredForPushNotifications: VersionedEvent
    {
        public object DeviceToken { get; set; }
    }
}
