using System;
using apcurium.MK.Common.Enumeration;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class MarkPrepaidOrderAsSuccessful : ICommand
    {
        public MarkPrepaidOrderAsSuccessful()
        {
            Id = Guid.NewGuid();
        }

        public Guid OrderId { get; set; }

        public Guid Id { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal MeterAmount { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal TipAmount { get; set; }

        public string TransactionId { get; set; }

        public PaymentProvider Provider { get; set; }

        public PaymentType Type { get; set; }
    }
}