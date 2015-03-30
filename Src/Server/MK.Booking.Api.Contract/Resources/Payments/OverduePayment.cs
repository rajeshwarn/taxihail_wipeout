﻿using System;

namespace apcurium.MK.Booking.Api.Contract.Resources.Payments
{
    public class OverduePayment : BaseDto
    {
        public Guid OrderId { get; set; }

        public int? IBSOrderId { get; set; }

        public decimal OverdueAmount { get; set; }

        public string TransactionId { get; set; }

        public DateTime? TransactionDate { get; set; }
    }
}
