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

        public OrderReportAccount Account { get; set; }

        public OrderReportOrder Order { get; set; }

        public OrderReportOrderStatus OrderStatus { get; set; }

        public OrderReportPayment Payment { get; set; }

        public OrderReportPromotion Promotion { get; set; }

        public OrderReportVehicleInfos VehicleInfos { get; set; }

        public OrderReportClient Client { get; set; }

        public string Rating { get; set; }

        public OrderReportDetail()
        {
            Account = new OrderReportAccount();
            Order = new OrderReportOrder();
            OrderStatus = new OrderReportOrderStatus();
            Payment = new OrderReportPayment();
            Promotion = new OrderReportPromotion();
            VehicleInfos = new OrderReportVehicleInfos();
            Client = new OrderReportClient();
            Rating = "";
        }
    }

    public class OrderReportAccount
    {
        public Guid AccountId { get; set; }

        public string Name { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public int? IBSAccountId { get; set; }

        public Guid? DefaultCardToken { get; set; }
    }

    public class OrderReportOrder
    {
        public string CompanyName { get; set; }

        public int? IBSOrderId { get; set; }

        public string ChargeType { get; set; }

        public DateTime PickupDateTime { get; set; }

        public DateTime CreateDateTime { get; set; }

        public Address PickupAddress { get; set; }

        public Address DropOffAddress { get; set; }
    }

    public class OrderReportOrderStatus
    {
        public int Status { get; set; }

        public bool OrderIsCancelled { get; set; }

        public bool OrderIsCompleted { get; set; }
    }

    public class OrderReportPayment
    {
        public decimal? MeterAmount { get; set; }

        public decimal? TipAmount { get; set; }

        public decimal? TotalAmountCharged { get; set; }

        public PaymentProvider? Provider { get; set; }

        public PaymentType? Type { get; set; }

        public string TransactionId { get; set; }

        public string AuthorizationCode { get; set; }

        public string CardToken { get; set; }

        public string PalPayerId { get; set; }

        public string PayPalToken { get; set; }

        public double? MdtTip { get; set; }

        public double? MdtToll { get; set; }

        public double? MdtFare { get; set; }
    }

    public class OrderReportPromotion
    {
        public string Code { get; set; }

        public bool WasApplied { get; set; }

        public bool WasRedeemed { get; set; }

        public decimal? SavedAmount { get; set; }
    }

    public class OrderReportVehicleInfos
    {
        public string Number { get; set; }

        public string Type { get; set; }

        public string Make { get; set; }

        public string Model { get; set; }

        public string Color { get; set; }

        public string Registration { get; set; }

        public string DriverFirstName { get; set; }

        public string DriverLastName { get; set; }

        public bool WasConfirmed { get; set; }
    }

    public class OrderReportClient
    {
        public string OperatingSystem { get; set; }

        public string UserAgent { get; set; }

        public string Version { get; set; }
    }
}