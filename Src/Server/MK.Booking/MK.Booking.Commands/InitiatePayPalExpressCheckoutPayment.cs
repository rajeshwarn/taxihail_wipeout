using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class InitiatePayPalExpressCheckoutPayment : ICommand
    {
        public InitiatePayPalExpressCheckoutPayment()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; private set; }
        public Guid PaymentId { get; set; }
        public Guid OrderId { get; set; }
        public string Token { get; set; }
        public decimal Amount { get; set; }
        public decimal Tip { get; set; }
        public decimal Meter { get; set; }
    }
}