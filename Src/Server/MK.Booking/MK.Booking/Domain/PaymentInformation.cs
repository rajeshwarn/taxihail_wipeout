#region

using System;

#endregion

namespace apcurium.MK.Booking.Domain
{
    public class PaymentInformation
    {
        public Guid CreditCardId { get; set; }
        public double? TipAmount { get; set; }
        public double? TipPercent { get; set; }
    }
}