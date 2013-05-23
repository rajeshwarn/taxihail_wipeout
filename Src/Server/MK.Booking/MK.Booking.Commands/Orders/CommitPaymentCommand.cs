using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands.Orders
{
    public class CommitPaymentCommand : ICommand
    {
        public CommitPaymentCommand()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; private set; }

        public Guid OrderId { get; set; }

        public long TransactionId { get; set; }

    }
}