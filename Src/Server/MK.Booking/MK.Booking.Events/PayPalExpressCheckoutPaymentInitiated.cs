﻿#region

using System;
using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Events
{
    [Obsolete("This event is deprecated. PayPal express checkout is not longer supported.")]
    public class PayPalExpressCheckoutPaymentInitiated : VersionedEvent
    {
        public Guid OrderId { get; set; }
        public string Token { get; set; }
        public decimal Amount { get; set; }
        public decimal Meter { get; set; }
        public decimal Tip { get; set; }
        public decimal Tax { get; set; }
    }
}