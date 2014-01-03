#region

using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class AccountPasswordReset : VersionedEvent
    {
        public byte[] Password { get; set; }
    }
}