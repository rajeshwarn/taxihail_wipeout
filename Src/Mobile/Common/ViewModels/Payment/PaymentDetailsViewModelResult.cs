using System;

namespace apcurium.MK.Booking.Mobile.ViewModels.Payment
{
    public class PaymentDetailsViewModelResult
    {
        public Guid CreditCardId { get; set; }
        public decimal? TipAmount { get; set; }
        public decimal? TipPercent { get; set; }
    }
}

