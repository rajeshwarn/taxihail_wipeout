using Infrastructure.EventSourcing;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Events
{
    public class PopularAddressAdded : VersionedEvent
    {
        public Address Address { get; set; }
    }
}
