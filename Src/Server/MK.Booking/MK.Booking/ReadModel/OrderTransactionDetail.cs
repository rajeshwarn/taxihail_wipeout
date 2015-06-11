using System;
using System.ComponentModel.DataAnnotations;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.ReadModel
{
    public class OrderTransactionDetail
    {
        [Key]
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }

        /// <summary>
        /// Null value if "home" company
        /// </summary>
        public string CompanyKey { get; set; }

        public decimal TotalAmount { get; set; }

        public TransactionTypes TransactionType { get; set; }

        public PaymentProvider Provider { get; set; }
    }
}
