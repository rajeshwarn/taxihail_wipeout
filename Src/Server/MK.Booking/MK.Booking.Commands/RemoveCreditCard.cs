#region

using System;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class RemoveCreditCard : ICommand
    {
        public RemoveCreditCard()
        {
            Id = Guid.NewGuid();
        }

        public Guid CreditCardId { get; set; }
        public Guid AccountId { get; set; }
        public Guid Id { get; set; }
    }
}