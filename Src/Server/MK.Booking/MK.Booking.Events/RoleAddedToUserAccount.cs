#region

using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class RoleAddedToUserAccount : VersionedEvent
    {
        public string RoleName { get; set; }
    }
}