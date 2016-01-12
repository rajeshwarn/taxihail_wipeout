using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class SettleOverduePayment : ICommand
    {
        public SettleOverduePayment()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        public Guid AccountId { get; set; }

        public Guid OrderId { get; set; }

        public Guid? CreditCardId { get; set; }
    }
}
