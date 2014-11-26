using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class OrderSwitchedToNextDispatchCompany : VersionedEvent
    {
        public int IBSOrderId { get; set; }

        public string CompanyKey { get; set; }

        public string CompanyName { get; set; }

        public string Market { get; set; }
    }
}
