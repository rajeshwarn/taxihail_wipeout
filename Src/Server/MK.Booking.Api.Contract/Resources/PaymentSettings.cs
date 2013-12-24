#region

using System;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class PaymentSettings
    {
        public bool PayWithCreditCard { get; set; }
        public Guid? CreditCardId { get; set; }
        public double? TipAmount { get; set; }
        public double? TipPercent { get; set; }
    }
}