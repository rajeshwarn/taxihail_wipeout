using System;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common.Configuration.Impl;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.EventHandlers
{
    public class NotificationSettingsGenerator : IEventHandler<NotificationSettingsAddedOrUpdated>
    {
        private readonly Func<ConfigurationDbContext> _contextFactory;

        public NotificationSettingsGenerator(Func<ConfigurationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Handle(NotificationSettingsAddedOrUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var notificationSettings = context.NotificationSettings.Find(@event.SourceId);
                if (notificationSettings != null)
                {
                    context.NotificationSettings.Remove(notificationSettings);
                }

                context.NotificationSettings.Add(@event.NotificationSettings);
                context.SaveChanges();
            }
        }
    }
}