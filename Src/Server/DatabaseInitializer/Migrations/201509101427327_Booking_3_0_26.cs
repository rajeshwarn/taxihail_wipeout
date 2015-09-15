using System.Data.SqlClient;
using DatabaseInitializer;

namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_3_0_26 : DbMigration
    {
        public override void Up()
        {
            //dirty but there's no easy way to pass argument to a migration
            if (Program.IsUpdate)
            {
                return;
            }
            
            CreateTable(
               "Booking.OrderStatusDetail",
               c => new
               {
                   OrderId = c.Guid(nullable: false),
                   Status = c.Int(nullable: false),
                   DriverInfos_VehicleType = c.String(),
                   DriverInfos_VehicleMake = c.String(),
                   DriverInfos_VehicleModel = c.String(),
                   DriverInfos_VehicleColor = c.String(),
                   DriverInfos_VehicleRegistration = c.String(),
                   DriverInfos_FirstName = c.String(),
                   DriverInfos_LastName = c.String(),
                   DriverInfos_DriverId = c.String(),
                   DriverInfos_MobilePhone = c.String(),
                   DriverInfos_DriverPhotoUrl = c.String(),
                   IBSOrderId = c.Int(),
                   IBSStatusId = c.String(),
                   IBSStatusDescription = c.String(),
                   VehicleNumber = c.String(),
                   VehicleLatitude = c.Double(),
                   VehicleLongitude = c.Double(),
                   FareAvailable = c.Boolean(nullable: false),
                   IsChargeAccountPaymentWithCardOnFile = c.Boolean(nullable: false),
                   IsPrepaid = c.Boolean(nullable: false),
                   AccountId = c.Guid(nullable: false),
                   PickupDate = c.DateTime(nullable: false),
                   Eta = c.DateTime(),
                   Name = c.String(),
                   ReferenceNumber = c.String(),
                   TerminalId = c.String(),
                   PairingTimeOut = c.DateTime(),
                   UnpairingTimeOut = c.DateTime(),
                   PairingError = c.String(),
                   TaxiAssignedDate = c.DateTime(),
                   Market = c.String(),
                   CompanyKey = c.String(),
                   CompanyName = c.String(),
                   NextDispatchCompanyName = c.String(),
                   NextDispatchCompanyKey = c.String(),
                   IgnoreDispatchCompanySwitch = c.Boolean(nullable: false),
                   NetworkPairingTimeout = c.DateTime(),
                   IsManualRideLinq = c.Boolean(nullable: false),
                   RideLinqPairingCode = c.String(),
                   OriginalEta = c.Long(),
               })
               .PrimaryKey(t => t.OrderId);

            CreateTable(
                "Booking.OrderPairingDetail",
                c => new
                {
                    OrderId = c.Guid(nullable: false),
                    Medallion = c.String(),
                    DriverId = c.String(),
                    PairingToken = c.String(),
                    PairingCode = c.String(),
                    TokenOfCardToBeUsedForPayment = c.String(),
                    AutoTipAmount = c.Double(),
                    AutoTipPercentage = c.Int(),
                })
                .PrimaryKey(t => t.OrderId);

            CreateTable(
                "Booking.OrderManualRideLinqDetail",
                c => new
                {
                    OrderId = c.Guid(nullable: false),
                    AccountId = c.Guid(nullable: false),
                    PairingCode = c.String(),
                    PairingToken = c.String(),
                    PairingDate = c.DateTime(nullable: false),
                    StartTime = c.DateTime(),
                    EndTime = c.DateTime(),
                    IsCancelled = c.Boolean(nullable: false),
                    Distance = c.Double(),
                    Total = c.Double(),
                    Fare = c.Double(),
                    FareAtAlternateRate = c.Double(),
                    Toll = c.Double(),
                    Extra = c.Double(),
                    Tip = c.Double(),
                    Surcharge = c.Double(),
                    Tax = c.Double(),
                    RateAtTripStart = c.Double(),
                    RateAtTripEnd = c.Double(),
                    RateChangeTime = c.String(),
                    Medallion = c.String(),
                    TripId = c.Int(nullable: false),
                    DriverId = c.Int(nullable: false),
                    AccessFee = c.Double(),
                    LastFour = c.String(),
                })
                .PrimaryKey(t => t.OrderId);

            CreateTable(
                "Booking.OrderVehiclePositionDetail",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    OrderId = c.Guid(nullable: false),
                    DateOfPosition = c.DateTime(nullable: false),
                    Latitude = c.Double(nullable: false),
                    Longitude = c.Double(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "Booking.PromotionDetail",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    Name = c.String(),
                    Description = c.String(),
                    StartDate = c.DateTime(),
                    EndDate = c.DateTime(),
                    StartTime = c.DateTime(),
                    EndTime = c.DateTime(),
                    DaysOfWeek = c.String(),
                    AppliesToCurrentBooking = c.Boolean(nullable: false),
                    AppliesToFutureBooking = c.Boolean(nullable: false),
                    DiscountValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                    DiscountType = c.Int(nullable: false),
                    MaxUsagePerUser = c.Int(),
                    MaxUsage = c.Int(),
                    Code = c.String(),
                    PublishedStartDate = c.DateTime(),
                    PublishedEndDate = c.DateTime(),
                    Active = c.Boolean(nullable: false),
                    Deleted = c.Boolean(nullable: false),
                    TriggerSettings_Type = c.Int(nullable: false),
                    TriggerSettings_RideCount = c.Int(nullable: false),
                    TriggerSettings_AmountSpent = c.Int(nullable: false),
                    TriggerSettings_ApplyToExisting = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "Booking.PromotionUsageDetail",
                c => new
                {
                    OrderId = c.Guid(nullable: false),
                    PromoId = c.Guid(nullable: false),
                    AccountId = c.Guid(nullable: false),
                    DiscountValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                    DiscountType = c.Int(nullable: false),
                    AmountSaved = c.Decimal(nullable: false, precision: 18, scale: 2),
                    Code = c.String(),
                    UserEmail = c.String(),
                    DateRedeemed = c.DateTime(),
                })
                .PrimaryKey(t => t.OrderId);

            CreateTable(
                "Booking.PromotionProgressDetail",
                c => new
                {
                    AccountId = c.Guid(nullable: false),
                    PromoId = c.Guid(nullable: false),
                    RideCount = c.Int(),
                    AmountSpent = c.Double(),
                    LastTriggeredAmount = c.Double(),
                })
                .PrimaryKey(t => new { t.AccountId, t.PromoId });

            CreateTable(
                "Booking.AccountDetail",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    Name = c.String(),
                    Email = c.String(),
                    Password = c.Binary(),
                    IBSAccountId = c.Int(),
                    TwitterId = c.String(),
                    FacebookId = c.String(),
                    IsConfirmed = c.Boolean(nullable: false),
                    DisabledByAdmin = c.Boolean(nullable: false),
                    Language = c.String(),
                    ConfirmationToken = c.String(),
                    Roles = c.Int(nullable: false),
                    Settings_Name = c.String(),
                    Settings_Country_Code = c.String(),
                    Settings_Phone = c.String(),
                    Settings_ProviderId = c.Int(),
                    Settings_VehicleTypeId = c.Int(),
                    Settings_VehicleType = c.String(),
                    Settings_ChargeTypeId = c.Int(),
                    Settings_ChargeType = c.String(),
                    Settings_AccountNumber = c.String(),
                    Settings_CustomerNumber = c.String(),
                    Settings_NumberOfTaxi = c.Int(nullable: false),
                    Settings_LargeBags = c.Int(nullable: false),
                    Settings_PayBack = c.String(),
                    Settings_Passengers = c.Int(nullable: false),
                    DefaultCreditCard = c.Guid(),
                    DefaultTipPercent = c.Int(),
                    IsPayPalAccountLinked = c.Boolean(nullable: false),
                    CreationDate = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "Booking.AccountIbsDetail",
                c => new
                {
                    AccountId = c.Guid(nullable: false),
                    CompanyKey = c.String(nullable: false, maxLength: 128),
                    IBSAccountId = c.Int(nullable: false),
                })
                .PrimaryKey(t => new { t.AccountId, t.CompanyKey });

            CreateTable(
                "Booking.DeviceDetail",
                c => new
                {
                    AccountId = c.Guid(nullable: false),
                    DeviceToken = c.String(nullable: false, maxLength: 1024),
                    Platform = c.Int(nullable: false),
                })
                .PrimaryKey(t => new { t.AccountId, t.DeviceToken });

            CreateTable(
                "Booking.OrderStatusUpdateDetail",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    UpdaterUniqueId = c.String(),
                    LastUpdateDate = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "Booking.AddressDetails",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    AccountId = c.Guid(nullable: false),
                    IsHistoric = c.Boolean(nullable: false),
                    FriendlyName = c.String(),
                    Apartment = c.String(),
                    FullAddress = c.String(),
                    RingCode = c.String(),
                    BuildingName = c.String(),
                    Longitude = c.Double(nullable: false),
                    Latitude = c.Double(nullable: false),
                    PlaceReference = c.String(),
                    StreetNumber = c.String(),
                    Street = c.String(),
                    City = c.String(),
                    ZipCode = c.String(),
                    State = c.String(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "Booking.OrderDetail",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    AccountId = c.Guid(nullable: false),
                    PickupDate = c.DateTime(nullable: false),
                    DropOffDate = c.DateTime(),
                    CreatedDate = c.DateTime(nullable: false),
                    IBSOrderId = c.Int(),
                    PickupAddress_Id = c.Guid(nullable: false),
                    PickupAddress_PlaceId = c.String(),
                    PickupAddress_FriendlyName = c.String(),
                    PickupAddress_StreetNumber = c.String(),
                    PickupAddress_AddressLocationType = c.Int(nullable: false),
                    PickupAddress_Street = c.String(),
                    PickupAddress_City = c.String(),
                    PickupAddress_ZipCode = c.String(),
                    PickupAddress_State = c.String(),
                    PickupAddress_FullAddress = c.String(),
                    PickupAddress_Longitude = c.Double(nullable: false),
                    PickupAddress_Latitude = c.Double(nullable: false),
                    PickupAddress_Apartment = c.String(),
                    PickupAddress_RingCode = c.String(),
                    PickupAddress_BuildingName = c.String(),
                    PickupAddress_IsHistoric = c.Boolean(nullable: false),
                    PickupAddress_Favorite = c.Boolean(nullable: false),
                    PickupAddress_AddressType = c.String(),
                    DropOffAddress_Id = c.Guid(nullable: false),
                    DropOffAddress_PlaceId = c.String(),
                    DropOffAddress_FriendlyName = c.String(),
                    DropOffAddress_StreetNumber = c.String(),
                    DropOffAddress_AddressLocationType = c.Int(nullable: false),
                    DropOffAddress_Street = c.String(),
                    DropOffAddress_City = c.String(),
                    DropOffAddress_ZipCode = c.String(),
                    DropOffAddress_State = c.String(),
                    DropOffAddress_FullAddress = c.String(),
                    DropOffAddress_Longitude = c.Double(nullable: false),
                    DropOffAddress_Latitude = c.Double(nullable: false),
                    DropOffAddress_Apartment = c.String(),
                    DropOffAddress_RingCode = c.String(),
                    DropOffAddress_BuildingName = c.String(),
                    DropOffAddress_IsHistoric = c.Boolean(nullable: false),
                    DropOffAddress_Favorite = c.Boolean(nullable: false),
                    DropOffAddress_AddressType = c.String(),
                    Settings_Name = c.String(),
                    Settings_Country_Code = c.String(),
                    Settings_Phone = c.String(),
                    Settings_ProviderId = c.Int(),
                    Settings_VehicleTypeId = c.Int(),
                    Settings_VehicleType = c.String(),
                    Settings_ChargeTypeId = c.Int(),
                    Settings_ChargeType = c.String(),
                    Settings_AccountNumber = c.String(),
                    Settings_CustomerNumber = c.String(),
                    Settings_NumberOfTaxi = c.Int(nullable: false),
                    Settings_LargeBags = c.Int(nullable: false),
                    Settings_PayBack = c.String(),
                    Settings_Passengers = c.Int(nullable: false),
                    PaymentInformation_PayWithCreditCard = c.Boolean(nullable: false),
                    PaymentInformation_CreditCardId = c.Guid(),
                    PaymentInformation_TipAmount = c.Double(),
                    PaymentInformation_TipPercent = c.Double(),
                    Status = c.Int(nullable: false),
                    Fare = c.Double(),
                    Toll = c.Double(),
                    Tip = c.Double(),
                    Tax = c.Double(),
                    Surcharge = c.Double(),
                    BookingFees = c.Decimal(nullable: false, precision: 18, scale: 2),
                    IsRemovedFromHistory = c.Boolean(nullable: false),
                    TransactionId = c.Long(nullable: false),
                    IsRated = c.Boolean(nullable: false),
                    EstimatedFare = c.Double(),
                    UserNote = c.String(),
                    CompanyKey = c.String(),
                    CompanyName = c.String(),
                    Market = c.String(),
                    UserAgent = c.String(),
                    ClientLanguageCode = c.String(),
                    ClientVersion = c.String(),
                    IsManualRideLinq = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "Booking.TariffDetail",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    CompanyId = c.Guid(nullable: false),
                    Name = c.String(),
                    MinimumRate = c.Double(nullable: false),
                    FlatRate = c.Decimal(nullable: false, precision: 18, scale: 2),
                    KilometricRate = c.Double(nullable: false),
                    PerMinuteRate = c.Double(nullable: false),
                    MarginOfError = c.Double(nullable: false),
                    KilometerIncluded = c.Double(nullable: false),
                    DaysOfTheWeek = c.Int(nullable: false),
                    StartTime = c.DateTime(nullable: false),
                    EndTime = c.DateTime(nullable: false),
                    Type = c.Int(nullable: false),
                    VehicleTypeId = c.Int(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "Booking.RuleDetail",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    CompanyId = c.Guid(nullable: false),
                    Name = c.String(),
                    Message = c.String(),
                    Type = c.Int(nullable: false),
                    Category = c.Int(nullable: false),
                    ZoneList = c.String(),
                    ZoneRequired = c.Boolean(nullable: false),
                    AppliesToCurrentBooking = c.Boolean(nullable: false),
                    AppliesToFutureBooking = c.Boolean(nullable: false),
                    AppliesToPickup = c.Boolean(nullable: false),
                    AppliesToDropoff = c.Boolean(nullable: false),
                    DaysOfTheWeek = c.Int(nullable: false),
                    StartTime = c.DateTime(),
                    EndTime = c.DateTime(),
                    ActiveFrom = c.DateTime(),
                    ActiveTo = c.DateTime(),
                    Priority = c.Int(nullable: false),
                    IsActive = c.Boolean(nullable: false),
                    Market = c.String(),
                    DisableFutureBookingOnError = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "Booking.RatingTypeDetail",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    Language = c.String(nullable: false, maxLength: 128),
                    CompanyId = c.Guid(nullable: false),
                    Name = c.String(),
                    IsHidden = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => new { t.Id, t.Language });

            CreateTable(
                "Booking.DefaultAddressDetails",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    FriendlyName = c.String(),
                    Apartment = c.String(),
                    FullAddress = c.String(),
                    RingCode = c.String(),
                    BuildingName = c.String(),
                    Longitude = c.Double(nullable: false),
                    Latitude = c.Double(nullable: false),
                    PlaceReference = c.String(),
                    StreetNumber = c.String(),
                    Street = c.String(),
                    City = c.String(),
                    ZipCode = c.String(),
                    State = c.String(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "Booking.PopularAddressDetails",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    FriendlyName = c.String(),
                    Apartment = c.String(),
                    FullAddress = c.String(),
                    RingCode = c.String(),
                    BuildingName = c.String(),
                    Longitude = c.Double(nullable: false),
                    Latitude = c.Double(nullable: false),
                    PlaceReference = c.String(),
                    AddressLocationType = c.Int(nullable: false),
                    StreetNumber = c.String(),
                    Street = c.String(),
                    City = c.String(),
                    ZipCode = c.String(),
                    State = c.String(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "Booking.OrderRatingDetails",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    OrderId = c.Guid(nullable: false),
                    Note = c.String(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "Booking.RatingScoreDetails",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    OrderId = c.Guid(nullable: false),
                    RatingTypeId = c.Guid(nullable: false),
                    Name = c.String(),
                    Score = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "Booking.CreditCardDetails",
                c => new
                {
                    CreditCardId = c.Guid(nullable: false),
                    AccountId = c.Guid(nullable: false),
                    NameOnCard = c.String(),
                    Token = c.String(),
                    Last4Digits = c.String(),
                    CreditCardCompany = c.String(),
                    ExpirationMonth = c.String(),
                    ExpirationYear = c.String(),
                    IsDeactivated = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.CreditCardId);

            CreateTable(
                "Booking.OrderPaymentDetail",
                c => new
                {
                    PaymentId = c.Guid(nullable: false),
                    OrderId = c.Guid(nullable: false),
                    PreAuthorizedAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                    Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                    Meter = c.Decimal(nullable: false, precision: 18, scale: 2),
                    Tip = c.Decimal(nullable: false, precision: 18, scale: 2),
                    Tax = c.Decimal(nullable: false, precision: 18, scale: 2),
                    Toll = c.Decimal(nullable: false, precision: 18, scale: 2),
                    Surcharge = c.Decimal(nullable: false, precision: 18, scale: 2),
                    BookingFees = c.Decimal(nullable: false, precision: 18, scale: 2),
                    CardToken = c.String(),
                    PayPalToken = c.String(),
                    Type = c.Int(nullable: false),
                    Provider = c.Int(nullable: false),
                    PayPalPayerId = c.String(),
                    FirstPreAuthTransactionId = c.String(),
                    TransactionId = c.String(),
                    AuthorizationCode = c.String(),
                    IsCancelled = c.Boolean(nullable: false),
                    IsCompleted = c.Boolean(nullable: false),
                    IsRefunded = c.Boolean(nullable: false),
                    FeeType = c.Int(nullable: false),
                    Error = c.String(),
                    CompanyKey = c.String(),
                })
                .PrimaryKey(t => t.PaymentId);

            CreateTable(
                "Booking.CompanyDetail",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    TermsAndConditions = c.String(),
                    PrivacyPolicy = c.String(),
                    Version = c.String(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "Booking.OrderUserGpsDetail",
                c => new
                {
                    OrderId = c.Guid(nullable: false),
                    UserLatitude = c.Double(),
                    UserLongitude = c.Double(),
                })
                .PrimaryKey(t => t.OrderId);

            CreateTable(
                "Booking.AppStartUpLogDetail",
                c => new
                {
                    UserId = c.Guid(nullable: false),
                    DateOccured = c.DateTime(nullable: false),
                    ApplicationVersion = c.String(),
                    Platform = c.String(),
                    PlatformDetails = c.String(),
                    ServerVersion = c.String(),
                    Latitude = c.Double(nullable: false),
                    Longitude = c.Double(nullable: false),
                })
                .PrimaryKey(t => t.UserId);

            CreateTable(
                "Booking.PayPalAccountDetails",
                c => new
                {
                    AccountId = c.Guid(nullable: false),
                    EncryptedRefreshToken = c.String(),
                })
                .PrimaryKey(t => t.AccountId);

            CreateTable(
                "Booking.OrderReportDetail",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    Account_AccountId = c.Guid(nullable: false),
                    Account_Name = c.String(),
                    Account_Phone = c.String(),
                    Account_Email = c.String(),
                    Account_IBSAccountId = c.Int(),
                    Account_PayBack = c.String(),
                    Account_DefaultCardToken = c.Guid(),
                    Order_IBSOrderId = c.Int(),
                    Order_CompanyName = c.String(),
                    Order_CompanyKey = c.String(),
                    Order_Market = c.String(),
                    Order_ChargeType = c.String(),
                    Order_IsChargeAccountPaymentWithCardOnFile = c.Boolean(nullable: false),
                    Order_IsPrepaid = c.Boolean(nullable: false),
                    Order_PickupDateTime = c.DateTime(),
                    Order_CreateDateTime = c.DateTime(),
                    Order_PickupAddress_Id = c.Guid(nullable: false),
                    Order_PickupAddress_PlaceId = c.String(),
                    Order_PickupAddress_FriendlyName = c.String(),
                    Order_PickupAddress_StreetNumber = c.String(),
                    Order_PickupAddress_AddressLocationType = c.Int(nullable: false),
                    Order_PickupAddress_Street = c.String(),
                    Order_PickupAddress_City = c.String(),
                    Order_PickupAddress_ZipCode = c.String(),
                    Order_PickupAddress_State = c.String(),
                    Order_PickupAddress_FullAddress = c.String(),
                    Order_PickupAddress_Longitude = c.Double(nullable: false),
                    Order_PickupAddress_Latitude = c.Double(nullable: false),
                    Order_PickupAddress_Apartment = c.String(),
                    Order_PickupAddress_RingCode = c.String(),
                    Order_PickupAddress_BuildingName = c.String(),
                    Order_PickupAddress_IsHistoric = c.Boolean(nullable: false),
                    Order_PickupAddress_Favorite = c.Boolean(nullable: false),
                    Order_PickupAddress_AddressType = c.String(),
                    Order_DropOffAddress_Id = c.Guid(nullable: false),
                    Order_DropOffAddress_PlaceId = c.String(),
                    Order_DropOffAddress_FriendlyName = c.String(),
                    Order_DropOffAddress_StreetNumber = c.String(),
                    Order_DropOffAddress_AddressLocationType = c.Int(nullable: false),
                    Order_DropOffAddress_Street = c.String(),
                    Order_DropOffAddress_City = c.String(),
                    Order_DropOffAddress_ZipCode = c.String(),
                    Order_DropOffAddress_State = c.String(),
                    Order_DropOffAddress_FullAddress = c.String(),
                    Order_DropOffAddress_Longitude = c.Double(nullable: false),
                    Order_DropOffAddress_Latitude = c.Double(nullable: false),
                    Order_DropOffAddress_Apartment = c.String(),
                    Order_DropOffAddress_RingCode = c.String(),
                    Order_DropOffAddress_BuildingName = c.String(),
                    Order_DropOffAddress_IsHistoric = c.Boolean(nullable: false),
                    Order_DropOffAddress_Favorite = c.Boolean(nullable: false),
                    Order_DropOffAddress_AddressType = c.String(),
                    Order_WasSwitchedToAnotherCompany = c.Boolean(nullable: false),
                    Order_HasTimedOut = c.Boolean(nullable: false),
                    Order_OriginalEta = c.Long(),
                    OrderStatus_Status = c.Int(nullable: false),
                    OrderStatus_OrderIsCancelled = c.Boolean(nullable: false),
                    OrderStatus_OrderIsCompleted = c.Boolean(nullable: false),
                    Payment_PaymentId = c.Guid(),
                    Payment_PairingToken = c.String(),
                    Payment_MeterAmount = c.Decimal(precision: 18, scale: 2),
                    Payment_TipAmount = c.Decimal(precision: 18, scale: 2),
                    Payment_PreAuthorizedAmount = c.Decimal(precision: 18, scale: 2),
                    Payment_TotalAmountCharged = c.Decimal(precision: 18, scale: 2),
                    Payment_Provider = c.Int(),
                    Payment_Type = c.Int(),
                    Payment_FirstPreAuthTransactionId = c.String(),
                    Payment_TransactionId = c.String(),
                    Payment_AuthorizationCode = c.String(),
                    Payment_CardToken = c.String(),
                    Payment_PayPalPayerId = c.String(),
                    Payment_PayPalToken = c.String(),
                    Payment_MdtTip = c.Double(),
                    Payment_MdtToll = c.Double(),
                    Payment_MdtFare = c.Double(),
                    Payment_BookingFees = c.Decimal(precision: 18, scale: 2),
                    Payment_IsPaired = c.Boolean(nullable: false),
                    Payment_IsCompleted = c.Boolean(nullable: false),
                    Payment_IsCancelled = c.Boolean(nullable: false),
                    Payment_IsRefunded = c.Boolean(nullable: false),
                    Payment_WasChargedNoShowFee = c.Boolean(nullable: false),
                    Payment_WasChargedCancellationFee = c.Boolean(nullable: false),
                    Payment_WasChargedBookingFee = c.Boolean(nullable: false),
                    Payment_Error = c.String(),
                    Promotion_Code = c.String(),
                    Promotion_WasApplied = c.Boolean(nullable: false),
                    Promotion_WasRedeemed = c.Boolean(nullable: false),
                    Promotion_SavedAmount = c.Decimal(precision: 18, scale: 2),
                    VehicleInfos_Number = c.String(),
                    VehicleInfos_Type = c.String(),
                    VehicleInfos_Make = c.String(),
                    VehicleInfos_Model = c.String(),
                    VehicleInfos_Color = c.String(),
                    VehicleInfos_Registration = c.String(),
                    VehicleInfos_DriverFirstName = c.String(),
                    VehicleInfos_DriverLastName = c.String(),
                    Client_OperatingSystem = c.String(),
                    Client_UserAgent = c.String(),
                    Client_Version = c.String(),
                    Rating = c.String(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "Booking.OrderNotificationDetail",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    IsTaxiNearbyNotificationSent = c.Boolean(nullable: false),
                    IsUnpairingReminderNotificationSent = c.Boolean(nullable: false),
                    InfoAboutPaymentWasSentToDriver = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "Booking.OverduePaymentDetail",
                c => new
                {
                    OrderId = c.Guid(nullable: false),
                    IBSOrderId = c.Int(),
                    AccountId = c.Guid(nullable: false),
                    OverdueAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                    TransactionId = c.String(),
                    TransactionDate = c.DateTime(),
                    IsPaid = c.Boolean(nullable: false),
                    ContainBookingFees = c.Boolean(nullable: false),
                    ContainStandaloneFees = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.OrderId);

            CreateTable(
                "Booking.FeesDetail",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    Market = c.String(),
                    Booking = c.Decimal(nullable: false, precision: 18, scale: 2),
                    Cancellation = c.Decimal(nullable: false, precision: 18, scale: 2),
                    NoShow = c.Decimal(nullable: false, precision: 18, scale: 2),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "Booking.AccountChargeQuestion",
                c => new
                {
                    Id = c.Int(nullable: false),
                    AccountId = c.Guid(nullable: false),
                    Question = c.String(),
                    Answer = c.String(),
                    IsRequired = c.Boolean(nullable: false),
                    IsCaseSensitive = c.Boolean(nullable: false),
                    MaxLength = c.Int(),
                    ErrorMessage = c.String(),
                    SaveAnswer = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => new { t.Id, t.AccountId })
                .ForeignKey("Booking.AccountChargeDetail", t => t.AccountId, cascadeDelete: true)
                .Index(t => t.AccountId);

            CreateTable(
                "Booking.AccountChargeDetail",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    Name = c.String(),
                    Number = c.String(),
                    UseCardOnFileForPayment = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "Booking.VehicleTypeDetail",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    Name = c.String(),
                    LogoName = c.String(),
                    ReferenceDataVehicleId = c.Int(nullable: false),
                    CreatedDate = c.DateTime(nullable: false),
                    MaxNumberPassengers = c.Int(nullable: false),
                    ReferenceNetworkVehicleTypeId = c.Int(),
                    IsWheelchairAccessible = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "Booking.TemporaryOrderCreationInfoDetail",
                c => new
                {
                    OrderId = c.Guid(nullable: false),
                    SerializedOrderCreationInfo = c.String(),
                })
                .PrimaryKey(t => t.OrderId);

            CreateTable(
                "Booking.TemporaryOrderPaymentInfoDetail",
                c => new
                {
                    OrderId = c.Guid(nullable: false),
                    Cvv = c.String(),
                })
                .PrimaryKey(t => t.OrderId);

            CreateTable(
                "Booking.AccountChargeQuestionAnswer",
                c => new
                {
                    AccountId = c.Guid(nullable: false),
                    AccountChargeId = c.Guid(nullable: false),
                    AccountChargeQuestionId = c.Int(nullable: false),
                    LastAnswer = c.String(),
                })
                .PrimaryKey(t => new { t.AccountId, t.AccountChargeId, t.AccountChargeQuestionId });
            
        }
        
        public override void Down()
        {
            
        }
    }
}
