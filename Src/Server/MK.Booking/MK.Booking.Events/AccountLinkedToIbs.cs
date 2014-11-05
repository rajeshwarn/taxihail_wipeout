using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class AccountLinkedToIbs : VersionedEvent
    {
        public string CompanyKey { get; set; }
        public int IbsAccountId { get; set; }
    }
}