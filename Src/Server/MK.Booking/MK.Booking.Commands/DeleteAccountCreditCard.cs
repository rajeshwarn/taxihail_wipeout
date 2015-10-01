using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class DeleteAccountCreditCard : ICommand
    {
        public DeleteAccountCreditCard()
        {
            Id = Guid.NewGuid();
        }

        public Guid AccountId { get; set; }
        public Guid CreditCardId { get; set; }
        public Guid Id { get; private set; }
    }
}
