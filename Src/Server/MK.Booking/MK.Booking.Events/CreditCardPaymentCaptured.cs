﻿using System;
using apcurium.MK.Common.Enumeration;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    [Obsolete("Replaced by CreditCardPaymentCaptured_V2", false)]
    public class CreditCardPaymentCaptured : VersionedEvent
    {
        public string TransactionId { get; set; }
        public string AuthorizationCode { get; set; }
        public decimal Amount { get; set; }
        public decimal Meter { get; set; }
        public decimal Tip { get; set; }
        public PaymentProvider Provider { get; set; }
        public Guid OrderId { get; set; }
        public bool IsNoShowFee { get; set; }
        public Guid? PromotionUsed { get; set; }
        public decimal AmountSavedByPromotion { get; set; }
    }
}