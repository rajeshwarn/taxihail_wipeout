using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class CompletePayPalExpressCheckoutPayment : ICommand
    {
        public CompletePayPalExpressCheckoutPayment()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; private set; }

        public Guid PaymentId { get; set; }

        public string PayPalPayerId { get; set; }

        public string TransactionId { get; set; }
    }
}