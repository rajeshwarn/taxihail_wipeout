#region

using System;
using Infrastructure.EventSourcing;

#endregion

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
                EventDate = EventDate,
                SourceId = SourceId,
                Version = Version,
            };
        }
    }
}