using Infrastructure.EventSourcing;
using MK.Common.Configuration;

namespace apcurium.MK.Booking.Events
{
    public class NotificationSettingsAddedOrUpdated : VersionedEvent
    {
        public NotificationSettings NotificationSettings { get; set; }
    }
}