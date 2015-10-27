using System;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common.Configuration.Impl;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.EventHandlers
{
    public class ServiceTypeSettingsGenerator : IEventHandler<ServiceTypeSettingsUpdated>
    {
        private readonly Func<ConfigurationDbContext> _contextFactory;

        public ServiceTypeSettingsGenerator(Func<ConfigurationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Handle(ServiceTypeSettingsUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var serviceTypeSettings = context.ServiceTypeSettings.Find(@event.ServiceTypeSettings.ServiceType);
                if (serviceTypeSettings != null)
                {
                    context.ServiceTypeSettings.Remove(serviceTypeSettings);
                }

                context.ServiceTypeSettings.Add(@event.ServiceTypeSettings);
                context.SaveChanges();
            }
        }
    }
}