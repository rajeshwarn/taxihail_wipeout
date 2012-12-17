using System;
using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Booking.ReadModel
{
    [ComplexType]
    public class PaymentInformationDetails
    {
        public bool PayWithCreditCard { get; set; }

        public Guid? CreditCardId { get; set; }

        public decimal? TipAmount { get; set; }

        public decimal? TipPercent { get; set; }
    }
}
