using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class PrivacyPolicyUpdated : VersionedEvent
    {
        public string Policy { get; set; }
    }
}