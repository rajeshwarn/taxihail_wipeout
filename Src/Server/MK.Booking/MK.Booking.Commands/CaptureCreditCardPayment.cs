#region

using System;
using apcurium.MK.Common.Enumeration;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class CaptureCreditCardPayment : ICommand
    {
        public CaptureCreditCardPayment()
        {
            Id = Guid.NewGuid();
        }

        public Guid AccountId { get; set; }

        public Guid PaymentId { get; set; }

        public string AuthorizationCode { get; set; }

        public string TransactionId { get; set; }

        public PaymentProvider Provider { get; set; }
        
        public decimal TotalAmount { get; set; }

        public decimal MeterAmount { get; set; }

        public decimal TipAmount { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal TollAmount { get; set; }

        public decimal SurchargeAmount { get; set; }

        public FeeTypes FeeType { get; set; }

        public bool IsSettlingOverduePayment { get; set; }

        public Guid? PromotionUsed { get; set; }

        public decimal AmountSavedByPromotion { get; set; }

        public string NewCardToken { get; set; }

        public bool IsForPrepaidOrder { get; set; }

        public decimal BookingFees { get; set; }

        public Guid Id { get; private set; }
    }
}