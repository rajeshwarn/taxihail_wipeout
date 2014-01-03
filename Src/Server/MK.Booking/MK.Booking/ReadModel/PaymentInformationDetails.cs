#region

using System;
using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace apcurium.MK.Booking.ReadModel
{
    [ComplexType]
    public class PaymentInformationDetails
    {
        public bool PayWithCreditCard { get; set; }

        public Guid? CreditCardId { get; set; }

        public double? TipAmount { get; set; }

        public double? TipPercent { get; set; }
    }
}