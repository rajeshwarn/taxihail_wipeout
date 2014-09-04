using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class DeleteAccountCreditCards : ICommand
    {
        public DeleteAccountCreditCards()
        {
            Id = Guid.NewGuid();
        }

        public Guid AccountId { get; set; }
        public Guid Id { get; private set; }
    }
}
