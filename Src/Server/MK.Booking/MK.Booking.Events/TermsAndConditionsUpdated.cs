using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class TermsAndConditionsUpdated : VersionedEvent
    {
        public string TermsAndConditions { get; set; }
    }
}