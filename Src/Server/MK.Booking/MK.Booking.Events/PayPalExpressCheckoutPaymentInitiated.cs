#region

using System;
using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class PayPalExpressCheckoutPaymentInitiated : VersionedEvent
    {
        public Guid OrderId { get; set; }
        public string Token { get; set; }
        public decimal Amount { get; set; }
        public decimal Meter { get; set; }
        public decimal Tip { get; set; }
    }
}