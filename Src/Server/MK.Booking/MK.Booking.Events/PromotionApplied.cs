﻿using System;
using apcurium.MK.Common.Enumeration;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class PromotionApplied : VersionedEvent
    {
        public Guid OrderId { get; set; }

        public Guid AccountId { get; set; }

        public decimal DiscountValue { get; set; }

        public PromoDiscountType DiscountType { get; set; }

        public string Code { get; set; }
    }
}