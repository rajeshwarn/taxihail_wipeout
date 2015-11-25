using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Projections;
using Infrastructure.Messaging.Handling;
using MK.Common.Configuration;

namespace apcurium.MK.Booking.EventHandlers
{
    public class NotificationSettingsGenerator : IEventHandler<NotificationSettingsAddedOrUpdated>
    {
        private readonly IProjectionSet<NotificationSettings> _notificationSettingsProjectionSet;

        public NotificationSettingsGenerator(IProjectionSet<NotificationSettings> notificationSettingsProjectionSet)
        {
            _notificationSettingsProjectionSet = notificationSettingsProjectionSet;
        }

        public void Handle(NotificationSettingsAddedOrUpdated @event)
        {
            _notificationSettingsProjectionSet.AddOrReplace(@event.NotificationSettings);
        }
    }
}