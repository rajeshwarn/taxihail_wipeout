using System;

namespace apcurium.MK.Booking.Domain
{
    public class PaymentInformation
    {
        public Guid CreditCardId { get; set; }
        public decimal? TipAmount { get; set; }
        public decimal? TipPercent { get; set; }
    }
}
