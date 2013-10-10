using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class InitiateCreditCardPayment : ICommand
    {
        public InitiateCreditCardPayment()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; private set; }
        public Guid PaymentId { get; set; }
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }
        public Guid OrderId { get; set; }

        public string CardToken { get; set; }
    }
}