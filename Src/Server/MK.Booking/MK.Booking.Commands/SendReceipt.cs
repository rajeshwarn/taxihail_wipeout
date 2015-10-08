#region

using System;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class SendReceipt : ICommand
    {
        public SendReceipt()
        {
            Id = Guid.NewGuid();
            PaymentInfo = null; // must be null if not used - see email template
        }

        public Guid OrderId { get; set; }
        public string EmailAddress { get; set; }
        public int IBSOrderId { get; set; }
        public DateTime PickupDate { get; set; }
        public DateTime? UtcDropOffDate { get; set; }
        public string VehicleNumber { get; set; }
        public DriverInfos DriverInfos { get; set; }
        public double Tip { get; set; }
        public double Fare { get; set; }
        public double Toll { get; set; }
        public double Tax { get; set; }
        public double Surcharge { get; set; }
        public double Extra { get; set; }
        public double BookingFees { get; set; }
        public double AmountSavedByPromotion { get; set; }
        public string PromoCode { get; set; }
        public PromoDiscountType PromoDiscountType { get; set; }
        public decimal PromoDiscountValue { get; set; }

        public string ClientLanguageCode { get; set; }

        public Address PickupAddress { get; set; }
        public Address DropOffAddress { get; set; }

        public CmtRideLinqReceiptFields CmtRideLinqFields { get; set; }

        public Payment PaymentInfo { get; set; } // OPTIONAL Null if not needed

        public double TotalFare
        {
            get { return Fare + Toll + Tip + Tax + Extra + Surcharge - AmountSavedByPromotion; }
        }

        public Guid Id { get; set; }

        public class CmtRideLinqReceiptFields
        {
            public DateTime? PickUpDateTime { get; set; }

            public DateTime? DropOffDateTime { get; set; }

            public double? Distance { get; set; }

            public int? TripId { get; set; }

            public string DriverId { get; set; }

            /// <summary>
            /// Aka: IMSRCH / Improvement Surcharge
            /// </summary>
            public double? AccessFee { get; set; }
 
            public string LastFour { get; set; }

            public int RateAtTripStart { get; set; }

            public int RateAtTripEnd { get; set; }

            public double? FareAtAlternateRate { get; set; }
            public double? LastLatitudeOfVehicle { get; set; }
            public double? LastLongitudeOfVehicle { get; set; }

            public TollDetail[] Tolls { get; set; }
            public double? TipIncentive { get; set; }
        }

        public class Payment
        {
            public Payment(decimal amount, string transactionId, string authorizationCode, string company)
            {
                Amount = amount;
                TransactionId = transactionId;
                AuthorizationCode = authorizationCode;
                Company = company;
            }

            public decimal Amount { get; set; }
            public string TransactionId { get; set; }
            public string AuthorizationCode { get; set; }
            public string Company { get; set; }
            public string Last4Digits { get; set; }
            public string NameOnCard { get; set; }
            public string ExpirationMonth { get; set; }
            public string ExpirationYear { get; set; }
        }
    }
}