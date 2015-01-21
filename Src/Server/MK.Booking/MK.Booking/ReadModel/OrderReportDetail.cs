using System;
using System.ComponentModel.DataAnnotations;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.ReadModel
{
    public class OrderReportDetail
    {
        [Key]
        public Guid Id { get; set; }

        // Account

        public Guid AccountId { get; set; }

        public string Name { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public int? IBSAccountId { get; set; }

        // Order

        public Guid OrderId { get; set; }

        public int? IBSOrderId { get; set; }

        public string ChargeType { get; set; }

        public DateTime PickupDateTime { get; set; }

        public DateTime CreateDateTime { get; set; }

        public Address PickupAddress { get; set; }

        public Address DropOffAddress { get; set; }

        // Order Status
        
        public int OrderStatus { get; set; }

        public bool OrderIsCancelled { get; set; }

        public bool OrderIsCompleted { get; set; }

        // Payment

        public decimal? PaymentMeterAmount { get; set; }

        public decimal? PaymentTipAmount { get; set; }

        public decimal? PaymentTotalAmountCharged { get; set; }

        public PaymentProvider? PaymentProvider { get; set; }

        public PaymentType? PaymentType { get; set; }

        public string PaymentTransactionId { get; set; }

        public string PaymentAuthorizationCode { get; set; }

        public string PaymentCardToken { get; set; }

        public Guid? DefaultCardToken { get; set; }

        public string PayPalPayerId { get; set; }

        public string PayPalToken { get; set; }

        public double? MdtTip { get; set; }

        public double? MdtToll { get; set; }

        public double? MdtFare { get; set; }

        // Promotion

        public string PromotionCode { get; set; }

        public bool PromotionApplied { get; set; }

        public bool PromotionRedeemed { get; set; }

        public decimal? PromotionSavedAmount { get; set; }

        // VehicleInfos

        public string VehicleCompanyName { get; set; }

        public string VehicleNumber { get; set; }

        public string VehicleType { get; set; }

        public string VehicleMake { get; set; }

        public string VehicleModel { get; set; }

        public string VehicleColor { get; set; }

        public string VehicleRegistration { get; set; }

        public string VehicleDriverFirstName { get; set; }

        public string VehicleDriverLastName { get; set; }

        public bool VehicleWasConfirmed { get; set; }

        // Client

        public string ClientOperatingSystem { get; set; }

        public string ClientUserAgent { get; set; }

        public string ClientVersion { get; set; }

    }
}