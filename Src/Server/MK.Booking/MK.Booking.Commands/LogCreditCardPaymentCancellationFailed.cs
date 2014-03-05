using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class LogCreditCardPaymentCancellationFailed : ICommand
    {
        public LogCreditCardPaymentCancellationFailed()
        {
            Id = Guid.NewGuid();
        }

        public Guid PaymentId { get; set; }

        public string Reason { get; set; }

        public Guid Id { get; private set; }
    }
}
