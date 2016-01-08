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
            Rating = string.Empty;
        }
    }

    public class OrderReportAccount
    {
        public Guid AccountId { get; set; }

        public string Name { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public int? IBSAccountId { get; set; }

        public string PayBack { get; set; }

        public Guid? DefaultCardToken { get; set; }
    }

    public class OrderReportOrder
    {
        private DateTime? _pickupDateTime;
        private DateTime? _createDateTime;

        public OrderReportOrder()
        {
            PickupAddress = new Address();
            DropOffAddress = new Address();
        }

        public int? IBSOrderId { get; set; }

        public string CompanyName { get; set; }

        public string CompanyKey { get; set; }

        public string Market { get; set; }

        public string ChargeType { get; set; }

        public bool IsChargeAccountPaymentWithCardOnFile { get; set; }

        public bool IsPrepaid { get; set; }

        public DateTime? PickupDateTime
        {
            get
            {
                if (_pickupDateTime == null || (_pickupDateTime != null && _pickupDateTime >= apcurium.MK.Booking.Database.BookingDbContext.MinimumDatabaseDateTime))
                {
                    return _pickupDateTime;
                }
                else
                {
                    return apcurium.MK.Booking.Database.BookingDbContext.MinimumDatabaseDateTime;
                }
            }

            set
            {
                if (value == null || (value != null && value >= apcurium.MK.Booking.Database.BookingDbContext.MinimumDatabaseDateTime))
                {
                    _pickupDateTime = value;
                }
                else
                {
                    _pickupDateTime = apcurium.MK.Booking.Database.BookingDbContext.MinimumDatabaseDateTime;
                }
            }
        }

        public DateTime? CreateDateTime
        {
            get
            {
                if (_createDateTime == null || (_createDateTime != null && _createDateTime >= apcurium.MK.Booking.Database.BookingDbContext.MinimumDatabaseDateTime))
                {
                    return _createDateTime;
                }
                else
                {
                    return apcurium.MK.Booking.Database.BookingDbContext.MinimumDatabaseDateTime;
                }
            }

            set
            {
                if (value == null || (value != null && value >= apcurium.MK.Booking.Database.BookingDbContext.MinimumDatabaseDateTime))
                {
                    _createDateTime = value;
                }
                else
                {
                    _createDateTime = apcurium.MK.Booking.Database.BookingDbContext.MinimumDatabaseDateTime;
                }
            }
        }

        public Address PickupAddress { get; set; }

        public Address DropOffAddress { get; set; }

        public bool WasSwitchedToAnotherCompany { get; set; }

        public bool HasTimedOut { get; set; }

        public long? OriginalEta { get; set; }

        public string Error { get; set; }
    }

    public class OrderReportOrderStatus
    {
        public OrderStatus Status { get; set; }

        public bool OrderIsCancelled { get; set; }

        public bool OrderIsCompleted { get; set; }
    }

    public class OrderReportPayment
    {
        public Guid? PaymentId { get; set; }

        public string DriverId { get; set; }

        public string Medallion { get; set; }

        public string PairingToken { get; set; }

        public decimal? MeterAmount { get; set; }

        public decimal? TipAmount { get; set; }

        public decimal? PreAuthorizedAmount { get; set; }

        public decimal? TotalAmountCharged { get; set; }

        public PaymentProvider? Provider { get; set; }

        public PaymentType? Type { get; set; }

        public string FirstPreAuthTransactionId { get; set; }

        public string TransactionId { get; set; }

        public string AuthorizationCode { get; set; }

        public string CardToken { get; set; }

        public string Last4Digits { get; set; }

        public string PayPalPayerId { get; set; }

        public string PayPalToken { get; set; }

        public double? MdtTip { get; set; }

        public double? MdtToll { get; set; }

        public double? MdtFare { get; set; }

        public decimal? BookingFees { get; set; }

        public bool IsPaired { get; set; }

        public bool WasUnpaired { get; set; }

        public bool IsCompleted { get; set; }

        public bool IsCancelled { get; set; }

        public bool IsRefunded { get; set; }

        public bool WasChargedNoShowFee { get; set; }

        public bool WasChargedCancellationFee { get; set; }

        public bool WasChargedBookingFee { get; set; }

        public string Error { get; set; }
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

        public string DriverId { get; set; }

        public string DriverFirstName { get; set; }

        public string DriverLastName { get; set; }
    }

    public class OrderReportClient
    {
        public string OperatingSystem { get; set; }

        public string UserAgent { get; set; }

        public string Version { get; set; }
    }
}