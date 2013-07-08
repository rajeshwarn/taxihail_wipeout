using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class PayPalExpressCheckoutPaymentInitiated: VersionedEvent
    {
        public Guid OrderId { get; set; }
        public string Token { get; set; }
        public decimal Amount { get; set; }
    }
}