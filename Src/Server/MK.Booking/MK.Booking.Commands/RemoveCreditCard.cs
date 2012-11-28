using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class RemoveCreditCard : ICommand
    {
        public RemoveCreditCard()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public Guid CreditCardId { get; set; }
        public Guid AccountId { get; set; } 
    }
}