#region

using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class AccountPasswordUpdated : VersionedEvent
    {
        public byte[] Password { get; set; }
    }
}