using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class LogCancellationFailurePayPalPayment : ICommand
    {
        public LogCancellationFailurePayPalPayment()
        {
            Id = Guid.NewGuid();
        }

        public Guid PaymentId { get; set; }

        public string Reason { get; set; }

        public Guid Id { get; private set; }
    }
}
