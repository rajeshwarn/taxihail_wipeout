using Infrastructure.Messaging;
using System;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.Commands
{
    public class ReactToPaymentFailure : ICommand
    {
        public ReactToPaymentFailure()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; private set; }

        public Guid AccountId { get; set; }

        public Guid? CreditCardId { get; set; }

        public Guid OrderId { get; set; }

        public int? IBSOrderId { get; set; }

        public decimal OverdueAmount { get; set; }

        public string TransactionId { get; set; }

        public DateTime? TransactionDate { get; set; }

        public FeeTypes FeeType { get; set; }
    }
}
