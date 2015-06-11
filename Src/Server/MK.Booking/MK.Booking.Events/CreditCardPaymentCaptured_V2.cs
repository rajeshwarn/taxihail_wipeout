using System;
using apcurium.MK.Common.Enumeration;

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

        [Obsolete("Use FeeType enum instead")]
        public bool IsCancellationFee { get; set; }

        public decimal BookingFees { get; set; }

        public FeeTypes FeeType { get; set; }

        public void MigrateFees()
        {
            if (IsNoShowFee)
            {
                FeeType = FeeTypes.NoShow;
            }
            if (IsCancellationFee)
            {
                FeeType = FeeTypes.Cancellation;
            }
        }
    }
}