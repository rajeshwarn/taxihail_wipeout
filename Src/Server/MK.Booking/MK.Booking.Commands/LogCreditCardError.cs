using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class LogCreditCardError : ICommand
    {
        public LogCreditCardError()
        {
            Id = Guid.NewGuid();
        }

        public Guid PaymentId { get; set; }

        public string Reason { get; set; }

        public Guid Id { get; private set; }
    }
}
