#region

using System;
using apcurium.MK.Common.Entity;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class CreateOrder : ICommand
    {
        public CreateOrder()
        {
            Id = Guid.NewGuid();
            Payment = new PaymentInformation();
        }

        public Guid OrderId { get; set; }

        public Guid AccountId { get; set; }

        public DateTime PickupDate { get; set; }

        public Address PickupAddress { get; set; }

        public Address DropOffAddress { get; set; }

        public BookingSettings Settings { get; set; }

        public PaymentInformation Payment { get; set; }

        public double? EstimatedFare { get; set; }

        public string ClientLanguageCode { get; set; }

        public double? UserLatitude { get; set; }

        public double? UserLongitude { get; set; }

        public string UserNote { get; set; }

        public string UserAgent { get; set; }

        public string ClientVersion { get; set; }

        public Guid Id { get; private set; }

        public bool IsChargeAccountPaymentWithCardOnFile { get; set; }

        public string CompanyKey { get; set; }

        public string CompanyName { get; set; }

        public string Market { get; set; }

        public bool IsPrepaid { get; set; }

        public decimal BookingFees { get; set; }

        public double? TipIncentive { get; set; }

        public class PaymentInformation
        {
            public bool PayWithCreditCard { get; set; }
            public Guid CreditCardId { get; set; }
            public double? TipAmount { get; set; }
            public double? TipPercent { get; set; }
        }
    }
}