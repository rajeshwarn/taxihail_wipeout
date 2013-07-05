using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class PayPalExpressCheckoutPaymentInitiated: VersionedEvent
    {
        public object OrderId { get; set; }
        public object Token { get; set; }
        public decimal Amount { get; set; }
    }
}