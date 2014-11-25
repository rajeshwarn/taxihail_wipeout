using System;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using Infrastructure.Messaging.Handling;
using ServiceStack.Text;

namespace apcurium.MK.Booking.EventHandlers
{
    public class UserTaxiHailNetworkSettingsGenerator : IEventHandler<UserTaxiHailNetworkSettingsAddedOrUpdated>
    {
        private readonly Func<ConfigurationDbContext> _contextFactory;

        public UserTaxiHailNetworkSettingsGenerator(Func<ConfigurationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Handle(UserTaxiHailNetworkSettingsAddedOrUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var userTaxiHailNetworkSettings = context.UserTaxiHailNetworkSettings.Find(@event.SourceId);
                if (userTaxiHailNetworkSettings != null)
                {
                    context.UserTaxiHailNetworkSettings.Remove(userTaxiHailNetworkSettings);
                }

                context.UserTaxiHailNetworkSettings.Add(new UserTaxiHailNetworkSettings
                {
                    Id = @event.SourceId,
                    IsEnabled = @event.IsEnabled,
                    SerializedDisabledFleets = @event.DisabledFleets.ToJson()
                });

                context.SaveChanges();
            }
        }
    }
}
