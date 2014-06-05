using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class OrderFareUpdated : VersionedEvent
    {
        public double Fare { get; set; }

        public double Toll { get; set; }

        public double Tip { get; set; }

        public double Tax { get; set; }
    }
}