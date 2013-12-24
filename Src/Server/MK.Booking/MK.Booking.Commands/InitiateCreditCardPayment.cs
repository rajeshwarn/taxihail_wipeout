#region

using System;
using apcurium.MK.Common.Enumeration;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class InitiateCreditCardPayment : ICommand
    {
        public InitiateCreditCardPayment()
        {
            Id = Guid.NewGuid();
        }

        public Guid PaymentId { get; set; }
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }

        public decimal Meter { get; set; }
        public decimal Tip { get; set; }

        public Guid OrderId { get; set; }

        public PaymentProvider Provider { get; set; }

        public string CardToken { get; set; }
        public Guid Id { get; private set; }
    }
}