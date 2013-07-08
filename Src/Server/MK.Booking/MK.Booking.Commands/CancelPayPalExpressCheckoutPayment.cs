using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class CancelPayPalExpressCheckoutPayment : ICommand
    {
        public CancelPayPalExpressCheckoutPayment()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; private set; }
        public Guid PaymentId { get; set; }
    }
}