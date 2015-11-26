using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Projections;
using apcurium.MK.Common.Configuration;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.EventHandlers
{
    public class UserTaxiHailNetworkSettingsGenerator : 
        IEventHandler<UserTaxiHailNetworkSettingsAddedOrUpdated>
    {
        private readonly IProjectionSet<UserTaxiHailNetworkSettings> _userTaxiHailNetworkSettingsProjectionSet;

        public UserTaxiHailNetworkSettingsGenerator(IProjectionSet<UserTaxiHailNetworkSettings> userTaxiHailNetworkSettingsProjectionSet)
        {
            _userTaxiHailNetworkSettingsProjectionSet = userTaxiHailNetworkSettingsProjectionSet;
        }

        public void Handle(UserTaxiHailNetworkSettingsAddedOrUpdated @event)
        {
            _userTaxiHailNetworkSettingsProjectionSet.AddOrReplace(new UserTaxiHailNetworkSettings
            {
                Id = @event.SourceId,
                IsEnabled = @event.IsEnabled,
                DisabledFleets = @event.DisabledFleets
            });
        }
    }
}
