using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class PayPalExpressCheckoutPaymentCompleted: VersionedEvent
    {
        public string PayPalPayerId { get; set; }
        public Guid OrderId { get; set; }
    }
}