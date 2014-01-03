#region

using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class AccountUpdated : VersionedEvent
    {
        public string Name { get; set; }
    }
}