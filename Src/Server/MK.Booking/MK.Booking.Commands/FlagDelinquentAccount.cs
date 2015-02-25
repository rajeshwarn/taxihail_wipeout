using Infrastructure.Messaging;
using System;

namespace apcurium.MK.Booking.Commands
{
    public class FlagDelinquentAccount : ICommand
    {
        public FlagDelinquentAccount()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; private set; }

        public Guid AccountId { get; set; }

        public Guid OrderId { get; set; }

        public decimal OverdueAmount { get; set; }

        public string TransactionId { get; set; }

        public DateTime? TransactionDate { get; set; }
    }
}
