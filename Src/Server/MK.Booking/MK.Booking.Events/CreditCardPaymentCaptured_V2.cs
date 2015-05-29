using System;

namespace apcurium.MK.Booking.Events
{
    public class CreditCardPaymentCaptured_V2 : CreditCardPaymentCaptured
    {
        public Guid AccountId { get; set; }

        public decimal Tax { get; set; }

        public decimal Toll { get; set; }

        public decimal Surcharge { get; set; }

        public string NewCardToken { get; set; }

        public bool IsSettlingOverduePayment { get; set; }

        public bool IsForPrepaidOrder { get; set; }

        public bool IsCancellationFee { get; set; }

        public bool IsBookingFee { get; set; }

        public decimal BookingFees { get; set; }
    }
}