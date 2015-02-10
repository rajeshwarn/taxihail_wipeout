using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class PayPalAccountLinked : VersionedEvent
    {
        public string EncryptedRefreshToken { get; set; }
    }
}
