using apcurium.MK.Common.Configuration;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class NotificationSettingsAddedOrUpdated : VersionedEvent
    {
        public NotificationSettings NotificationSettings { get; set; }
    }
}