#region

using System;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class DeleteCreditCardsFromAccounts : ICommand
    {
        public DeleteCreditCardsFromAccounts()
        {
            Id = Guid.NewGuid();
        }

        public Guid[] AccountIds { get; set; }
        public Guid Id { get; private set; }
    }
}