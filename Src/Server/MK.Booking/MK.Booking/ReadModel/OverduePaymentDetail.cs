using System;
using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Booking.ReadModel
{
    public class OverduePaymentDetail
    {
        [Key]
        public Guid OrderId { get; set; }

        public int? IBSOrderId { get; set; }

        public Guid AccountId { get; set; }

        public decimal OverdueAmount { get; set; }

        public string TransactionId { get; set; }

        public DateTime? TransactionDate { get; set; }

        public bool IsPaid { get; set; }

        /// <summary>
        /// When true, it means that the OverdueAmount will contain the trip amount + the booking fees
        /// </summary>
        public bool ContainBookingFees { get; set; }

        /// <summary>
        /// When true, it means that the OverdueAmount only contains a fee
        /// </summary>
        public bool ContainStandaloneFees { get; set; }
    }
}
