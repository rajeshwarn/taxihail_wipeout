using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    [Obsolete("Replaced by RoleAddedToUserAccount", false)]
    public class AdminRightGranted : VersionedEvent, IUpgradableEvent
    {
        public IVersionedEvent Upgrade()
        {
            return new RoleAddedToUserAccount
            {
                RoleName = "Admin",
                EventDate = this.EventDate,
                SourceId = this.SourceId,
                Version = this.Version,
            };
        }
    }
}