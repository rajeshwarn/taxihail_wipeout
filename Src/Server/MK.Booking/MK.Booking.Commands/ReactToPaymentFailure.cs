using Infrastructure.Messaging;
using System;

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

        public Guid OrderId { get; set; }

        public int? IBSOrderId { get; set; }

        public decimal OverdueAmount { get; set; }

        public string TransactionId { get; set; }

        public DateTime? TransactionDate { get; set; }

        public bool IsBookingFee { get; set; }

        public bool IsCancellationFee { get; set; }

        public bool IsNoShowFee { get; set; }
    }
}
