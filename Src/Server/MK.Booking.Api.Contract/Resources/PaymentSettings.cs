using System;

namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class PaymentSettings
    {
        public bool PayWithCreditCard { get; set; }
        public Guid CreditCardId { get; set; }
        public decimal? TipAmount { get; set; }
        public decimal? TipPercent { get; set; }
    }
}

