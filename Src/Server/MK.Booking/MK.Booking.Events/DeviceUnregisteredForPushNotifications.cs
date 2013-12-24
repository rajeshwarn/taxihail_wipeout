#region

using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class DeviceUnregisteredForPushNotifications : VersionedEvent
    {
        public object DeviceToken { get; set; }
    }
}