using System;

namespace apcurium.MK.Booking.Mobile.ViewModels.Payment
{
    public class PaymentInformation
    {
        public Guid ?CreditCardId { get; set; }
        public double? TipAmount { get; set; }
        public double? TipPercent { get; set; }
    }
}

